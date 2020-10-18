using System;
using System.Collections.Generic;
using TixFactory.ApplicationContext;
using TixFactory.Http.Client;
using TixFactory.Logging;
using TixFactory.Operations;

namespace TixFactory.ApplicationConfiguration
{
	public class ApplicationConfigurationOperations : IApplicationConfigurationOperations
	{
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

			GetApplicationSettingsOperation = new GetApplicationSettingsOperation(httpClient, applicationAuthorizationServiceUrl);
		}
	}
}
