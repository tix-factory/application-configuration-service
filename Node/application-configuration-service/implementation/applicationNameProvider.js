import { HttpRequest, HttpRequestError, httpMethods } from "@tix-factory/http";
const CacheExpiry = 60 * 1000;

export default class {
	constructor(httpClient) {
		let applicationAuthorizationServiceHost = process.env.ApplicationAuthorizationServiceHost;
		if (!schemeRegex.test(applicationAuthorizationServiceHost)) {
			applicationAuthorizationServiceHost = `https://${applicationAuthorizationServiceHost}`; 
		}

		this.httpClient = httpClient;
		this.applicationNameCache = {};
		this.applicationAuthorizationServiceHost = applicationAuthorizationServiceHost;
	}

	async getApplicationName(apiKey) {
		if (typeof(apiKey) !== "string" || !apiKey) {
			return Promise.resolve(null);
		}

		let applicationName = this.applicationNameCache[apiKey];
		if (applicationName !== undefined) {
			return Promise.resolve(applicationName);
		}

		const httpRequest = new HttpRequest(httpMethods.post, new URL(`${this.applicationAuthorizationServiceHost}/v1/WhoAmI`));
		httpRequest.addOrUpdateHeader("Tix-Factory-Api-Key", apiKey);
		
		const httpResponse = await this.httpClient.send(httpRequest);
		if (httpResponse.statusCode !== 200) {
			return Promise.reject(new HttpRequestError(httpRequest, httpResponse));
		}

		const responseBody = JSON.parse(httpResponse.body.toString());
		this.applicationNameCache[apiKey] = applicationName = responseBody.data?.applicationName || null;
		if (!applicationName) {
			setTimeout(() => {
				delete this.applicationNameCache[apiKey];
			}, CacheExpiry);
		}

		return Promise.resolve(applicationName);
	}
};
