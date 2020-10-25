﻿using System;
using System.Collections.Generic;
using TixFactory.ApplicationConfiguration.Entities;
using TixFactory.Configuration;
using TixFactory.Http.Client;
using TixFactory.Logging;
using TixFactory.Operations;

namespace TixFactory.ApplicationConfiguration
{
	public class ApplicationConfigurationOperations : IApplicationConfigurationOperations
	{
		public IOperation<Guid, IReadOnlyDictionary<string, string>> GetApplicationSettingsOperation { get; }
		public IOperation<SetApplicationSettingRequest, SetApplicationSettingResult> SetApplicationSettingOperation { get; }
		public IOperation<DeleteApplicationSettingRequest, DeleteApplicationSettingResult> DeleteApplicationSettingOperation { get; }

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
			var databaseConnection = new DatabaseConnection(connectionString);
			var settingsGroupEntityFactory = new SettingsGroupEntityFactory(databaseConnection);
			var settingEntityFactory = new SettingEntityFactory(databaseConnection, settingsGroupEntityFactory);

			GetApplicationSettingsOperation = new GetApplicationSettingsOperation(httpClient, applicationAuthorizationServiceUrl, settingEntityFactory);
			SetApplicationSettingOperation = new SetApplicationSettingOperation(settingEntityFactory);
			DeleteApplicationSettingOperation = new DeleteApplicationSettingOperation(settingEntityFactory);
		}
	}
}
