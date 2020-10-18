using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TixFactory.ApplicationAuthorization;
using TixFactory.Configuration;
using TixFactory.Http.Client;
using TixFactory.Logging;
using TixFactory.Logging.Client;

namespace TixFactory.ApplicationConfiguration.Service
{
	public class Startup : TixFactory.Http.Service.Startup
	{
		public const string ApiKeyHeaderName = "Tix-Factory-Api-Key";
		private const string _ApplicationApiKeyEnvironmentVariableName = "ApplicationApiKey";
		private readonly IApplicationConfigurationOperations _ApplicationConfigurationOperations;
		private readonly IApiKeyParser _ApiKeyParser;
		private readonly IApplicationAuthorizationsAccessor _ApplicationAuthorizationsAccessor;

		public Startup()
			: base(CreateLogger())
		{
			var applicationKey = GetApplicationKey();
			var applicationAuthorizationUrl = new Uri("http://applicationauthorization.services.tixfactory.systems");

			_ApplicationConfigurationOperations = new ApplicationConfigurationOperations(Logger, applicationAuthorizationUrl);
			_ApiKeyParser = new ApiKeyHeaderParser(ApiKeyHeaderName);
			_ApplicationAuthorizationsAccessor = new ApplicationAuthorizationsAccessor(Logger, applicationAuthorizationUrl, applicationKey);
		}

		public override void ConfigureServices(IServiceCollection services)
		{
			services.AddTransient(s => _ApplicationConfigurationOperations);

			base.ConfigureServices(services);
		}

		protected override void ConfigureMvc(MvcOptions options)
		{
			options.Filters.Add(new ValidateApiKeyAttribute(_ApiKeyParser, _ApplicationAuthorizationsAccessor));

			base.ConfigureMvc(options);
		}

		private static ILogger CreateLogger()
		{
			var httpClient = new HttpClient();
			var consoleLogger = new ConsoleLogger();
			return new NetworkLogger(httpClient, consoleLogger, "TFACS1.TixFactory.ApplicationConfiguration.Service", "monitoring.tixfactory.systems");
		}

		private ISetting<Guid> GetApplicationKey()
		{
			var rawApiKey = Environment.GetEnvironmentVariable(_ApplicationApiKeyEnvironmentVariableName);
			if (!Guid.TryParse(rawApiKey, out var apiKey))
			{
				Logger.Warn($"\"{_ApplicationApiKeyEnvironmentVariableName}\" (environment variable) could not be parsed to Guid. Application will likely fail all authorizations.\n\tValue: \"{rawApiKey}\"");
			}

			return new Setting<Guid>(apiKey);
		}
	}
}
