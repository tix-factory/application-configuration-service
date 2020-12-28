export default class {
	constructor(settingEntityFactory, applicationNameProvider) {
		this.settingEntityFactory = settingEntityFactory;
		this.applicationNameProvider = applicationNameProvider;
	}
 
    get name() {
        return "SetApplicationSettingValue";
    }
	
	get requestParameters() {
		return ["settingName", "settingValue"];
	}
 
    async execute(requestBody, request) {
		const applicationName = await this.applicationNameProvider.getApplicationName(request.apiKey);
		const updated = await this.settingEntityFactory.setSettingValue(applicationName, requestBody.settingName, requestBody.settingValue);
		return Promise.resolve(updated ? "Changed" : "Unchanged");
    }
};