using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TixFactory.ApplicationConfiguration.Entities;
using TixFactory.Collections;
using TixFactory.Http;
using TixFactory.Http.Client;
using TixFactory.Operations;

namespace TixFactory.ApplicationConfiguration
{
	internal class SetApplicationSettingValueOperation : IAsyncOperation<SetApplicationSettingValueRequest, SetApplicationSettingResult>
	{
		private const string _ApiKeyHeaderName = "Tix-Factory-Api-Key";
		private readonly IHttpClient _HttpClient;
		private readonly ISettingEntityFactory _SettingEntityFactory;
		private readonly Uri _WhoAmIEndpoint;
		private readonly ExpirableDictionary<Guid, string> _ApplicationNamesByapplicationKey;
		private readonly JsonSerializerOptions _JsonSerializerOptions;

		public SetApplicationSettingValueOperation(IHttpClient httpClient, Uri serviceUrl, ISettingEntityFactory settingEntityFactory)
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

		public async Task<(SetApplicationSettingResult output, OperationError error)> Execute(SetApplicationSettingValueRequest request, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(request.SettingName) || request.SettingName.Length > EntityValidation.MaxSettingNameLength)
			{
				return (default, new OperationError(ApplicationConfigurationError.InvalidSettingName));
			}

			if (string.IsNullOrWhiteSpace(request.SettingValue) || request.SettingValue.Length > EntityValidation.MaxSettingValueLength)
			{
				return (default, new OperationError(ApplicationConfigurationError.InvalidSettingValue));
			}

			var applicationName = await GetApplicationNameAsync(request.ApiKey, cancellationToken).ConfigureAwait(false);
			if (string.IsNullOrWhiteSpace(applicationName) || applicationName.Length > EntityValidation.MaxSettingsGroupNameLength)
			{
				return (default, new OperationError(ApplicationConfigurationError.InvalidSettingsGroupName));
			}

			var setting = await _SettingEntityFactory.GetSettingBySettingsGroupNameAndSettingName(applicationName, request.SettingName, cancellationToken).ConfigureAwait(false);
			if (setting == null)
			{
				await _SettingEntityFactory.CreateSetting(applicationName, request.SettingName, request.SettingValue, cancellationToken).ConfigureAwait(false);
				return (SetApplicationSettingResult.Changed, null);
			}

			if (setting.Value == request.SettingValue)
			{
				return (SetApplicationSettingResult.Unchanged, null);
			}

			var restoreValue = setting.Value;
			setting.Value = request.SettingValue;

			try
			{
				await _SettingEntityFactory.UpdateSetting(setting, cancellationToken).ConfigureAwait(false);
			}
			catch
			{
				setting.Value = restoreValue;
				throw;
			}

			return (SetApplicationSettingResult.Changed, null);
		}

		private async Task<string> GetApplicationNameAsync(Guid applicationKey, CancellationToken cancellationToken)
		{
			if (_ApplicationNamesByapplicationKey.TryGetValue(applicationKey, out var applicationName))
			{
				return applicationName;
			}

			var httpRequest = new HttpRequest(HttpMethod.Post, _WhoAmIEndpoint);
			httpRequest.Headers.Add(_ApiKeyHeaderName, applicationKey.ToString());

			var httpResponse = await _HttpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
			var responseBody = httpResponse.GetStringBody();
			if (!httpResponse.IsSuccessful)
			{
				throw new HttpException(httpRequest, httpResponse);
			}

			var whoAmResponse = JsonSerializer.Deserialize<ServiceResponse<WhoAmIResponse>>(responseBody, _JsonSerializerOptions);
			applicationName = _ApplicationNamesByapplicationKey[applicationKey] = whoAmResponse.Data.ApplicationName;

			return applicationName;
		}
	}
}
