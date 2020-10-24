﻿using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using TixFactory.ApplicationConfiguration.Entities;
using TixFactory.Configuration;
using TixFactory.Http.Client;
using TixFactory.Logging;
using TixFactory.Operations;

namespace TixFactory.ApplicationConfiguration
{
	public class ApplicationConfigurationOperations : IApplicationConfigurationOperations
	{
		private readonly ILazyWithRetry<MySqlConnection> _MySqlConnection;

		public IOperation<Guid, IReadOnlyDictionary<string, string>> GetApplicationSettingsOperation { get; }

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
			var mySqlConnection = _MySqlConnection = new LazyWithRetry<MySqlConnection>(BuildConnection);
			var databaseConnection = new DatabaseConnection(mySqlConnection);
			var settingsGroupEntityFactory = new SettingsGroupEntityFactory(databaseConnection);

			GetApplicationSettingsOperation = new GetApplicationSettingsOperation(httpClient, applicationAuthorizationServiceUrl);
		}

		private MySqlConnection BuildConnection()
		{
			var connection = new MySqlConnection(Environment.GetEnvironmentVariable("CONFIGURATION_DATABASE_CONNECTION_STRING"));
			connection.StateChange += ConnectionStateChange;
			connection.Open();

			return connection;
		}

		private void ConnectionStateChange(object sender, StateChangeEventArgs e)
		{
			switch (e.CurrentState)
			{
				case ConnectionState.Broken:
				case ConnectionState.Closed:
					_MySqlConnection.Refresh();
					return;
			}
		}
	}
}
