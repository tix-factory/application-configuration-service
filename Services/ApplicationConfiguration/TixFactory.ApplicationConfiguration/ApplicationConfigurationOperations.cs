using System;
using System.Collections.Generic;
using TixFactory.ApplicationConfiguration.Entities;
using TixFactory.Configuration;
using TixFactory.Data.MySql;
using TixFactory.Http.Client;
using TixFactory.Logging;
using TixFactory.Operations;

namespace TixFactory.ApplicationConfiguration
{
	public class ApplicationConfigurationOperations : IApplicationConfigurationOperations
	{
		public IAsyncOperation<Guid, IReadOnlyDictionary<string, string>> GetApplicationSettingsOperation { get; }

		public IAsyncOperation<SetApplicationSettingRequest, SetApplicationSettingResult> SetApplicationSettingOperation { get; }

		public IAsyncOperation<SetApplicationSettingValueRequest, SetApplicationSettingResult> SetApplicationSettingValueOperation { get; }

		public IAsyncOperation<DeleteApplicationSettingRequest, DeleteApplicationSettingResult> DeleteApplicationSettingOperation { get; }

		public ApplicationConfigurationOperations(ILogger logger, Uri applicationAuthorizationServiceUrl)
		{
			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			if (applicationAuthorizationServiceUrl == null)
			{
				throw new ArgumentNullException(nameof(applicationAuthorizationServiceUrl));
			}

			var httpClient = new HttpClient();
			var connectionString = new Setting<string>(Environment.GetEnvironmentVariable("CONFIGURATION_DATABASE_CONNECTION_STRING"));
			var databaseConnection = new DatabaseConnection(connectionString, logger);
			var settingsGroupEntityFactory = new SettingsGroupEntityFactory(databaseConnection);
			var settingEntityFactory = new SettingEntityFactory(databaseConnection, settingsGroupEntityFactory);

			GetApplicationSettingsOperation = new GetApplicationSettingsOperation(httpClient, applicationAuthorizationServiceUrl, settingEntityFactory);
			SetApplicationSettingOperation = new SetApplicationSettingOperation(settingEntityFactory);
			SetApplicationSettingValueOperation = new SetApplicationSettingValueOperation(httpClient, applicationAuthorizationServiceUrl, settingEntityFactory);
			DeleteApplicationSettingOperation = new DeleteApplicationSettingOperation(settingEntityFactory);
		}
	}
}
