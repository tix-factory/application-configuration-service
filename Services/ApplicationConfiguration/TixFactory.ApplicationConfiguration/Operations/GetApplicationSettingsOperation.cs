using System;
using System.Collections.Generic;
using System.Text.Json;
using TixFactory.ApplicationConfiguration.Entities;
using TixFactory.Collections;
using TixFactory.Http;
using TixFactory.Http.Client;
using TixFactory.Operations;

namespace TixFactory.ApplicationConfiguration
{
	internal class GetApplicationSettingsOperation : IOperation<Guid, IReadOnlyDictionary<string, string>>
	{
		private const string _ApiKeyHeaderName = "Tix-Factory-Api-Key";
		private readonly IHttpClient _HttpClient;
		private readonly ISettingEntityFactory _SettingEntityFactory;
		private readonly Uri _WhoAmIEndpoint;
		private readonly ExpirableDictionary<Guid, string> _ApplicationNamesByapplicationKey;
		private readonly JsonSerializerOptions _JsonSerializerOptions;

		public GetApplicationSettingsOperation(IHttpClient httpClient, Uri serviceUrl, ISettingEntityFactory settingEntityFactory)
		{
			_HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_SettingEntityFactory = settingEntityFactory ?? throw new ArgumentNullException(nameof(settingEntityFactory));
			_WhoAmIEndpoint = new Uri($"{serviceUrl.GetLeftPart(UriPartial.Authority)}/v1/WhoAmI");
			_ApplicationNamesByapplicationKey = new ExpirableDictionary<Guid, string>(TimeSpan.FromHours(1), ExpirationPolicy.RenewOnRead);
			_JsonSerializerOptions = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};
		}

		public (IReadOnlyDictionary<string, string> output, OperationError error) Execute(Guid applicationKey)
		{
			var applicationSettings = new Dictionary<string, string>();
			var applicationName = GetApplicationName(applicationKey);

			if (!string.IsNullOrWhiteSpace(applicationName))
			{
				var settings = _SettingEntityFactory.GetSettingsByGroupName(applicationName);
				foreach (var setting in settings)
				{
					applicationSettings[setting.Name] = setting.Value;
				}
			}

			return (applicationSettings, null);
		}

		private string GetApplicationName(Guid applicationKey)
		{
			if (_ApplicationNamesByapplicationKey.TryGetValue(applicationKey, out var applicationName))
			{
				return applicationName;
			}

			var httpRequest = new HttpRequest(HttpMethod.Post, _WhoAmIEndpoint);
			httpRequest.Headers.Add(_ApiKeyHeaderName, applicationKey.ToString());

			var httpResponse = _HttpClient.Send(httpRequest);
			var responseBody = httpResponse.GetStringBody();
			if (!httpResponse.IsSuccessful)
			{
				throw new HttpException("Failed to load application name from application key."
										+ $"\n\tUrl: {httpResponse.Url}"
										+ $"\n\tStatus Code: {httpResponse.StatusCode} ({httpResponse.StatusText})"
										+ $"\n\tBody: {responseBody}",
					innerException: new Exception("fake exception to statisfy compiler"));
			}

			var whoAmResponse = JsonSerializer.Deserialize<ServiceResponse<WhoAmIResponse>>(responseBody, _JsonSerializerOptions);
			applicationName = _ApplicationNamesByapplicationKey[applicationKey] = whoAmResponse.Data.ApplicationName;

			return applicationName;
		}
	}
}
