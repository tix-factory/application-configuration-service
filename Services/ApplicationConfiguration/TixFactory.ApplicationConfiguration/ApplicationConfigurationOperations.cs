using System;
using TixFactory.ApplicationContext;
using TixFactory.Logging;

namespace TixFactory.ApplicationConfiguration
{
	public class ApplicationConfigurationOperations : IApplicationConfigurationOperations
	{
		public ApplicationConfigurationOperations(ILogger logger, IApplicationContext applicationContext)
		{
			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			if (applicationContext == null)
			{
				throw new ArgumentNullException(nameof(applicationContext));
			}
		}
	}
}
