using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TixFactory.Http.Service;

namespace TixFactory.ApplicationConfiguration.Service.Controllers
{
	[Route("v1/[action]")]
	public class ApplicationConfigurationController : Controller
	{
		private readonly IApplicationConfigurationOperations _ApplicationConfigurationOperations;
		private readonly IOperationExecuter _OperationExecuter;

		public ApplicationConfigurationController(IApplicationConfigurationOperations applicationConfigurationOperations, IOperationExecuter operationExecuter)
		{
			_ApplicationConfigurationOperations = applicationConfigurationOperations ?? throw new ArgumentNullException(nameof(applicationConfigurationOperations));
			_OperationExecuter = operationExecuter ?? throw new ArgumentNullException(nameof(operationExecuter));
		}

		[HttpPost]
		public Task<IActionResult> GetApplicationSettings([FromHeader(Name = Startup.ApiKeyHeaderName)] Guid apiKey, CancellationToken cancellationToken)
		{
			return _OperationExecuter.ExecuteAsync(_ApplicationConfigurationOperations.GetApplicationSettingsOperation, apiKey, cancellationToken);
		}

		[HttpPost]
		public Task<IActionResult> SetApplicationSetting([FromBody] SetApplicationSettingRequest request, CancellationToken cancellationToken)
		{
			return _OperationExecuter.ExecuteAsync(_ApplicationConfigurationOperations.SetApplicationSettingOperation, request, cancellationToken);
		}

		[HttpPost]
		public Task<IActionResult> SetApplicationSettingValue([FromHeader(Name = Startup.ApiKeyHeaderName)] Guid apiKey, [FromBody] SetApplicationSettingValueRequest request, CancellationToken cancellationToken)
		{
			request.ApiKey = apiKey;
			return _OperationExecuter.ExecuteAsync(_ApplicationConfigurationOperations.SetApplicationSettingValueOperation, request, cancellationToken);
		}

		[HttpPost]
		public Task<IActionResult> DeleteApplicationSetting([FromBody] DeleteApplicationSettingRequest request, CancellationToken cancellationToken)
		{
			return _OperationExecuter.ExecuteAsync(_ApplicationConfigurationOperations.DeleteApplicationSettingOperation, request, cancellationToken);
		}
	}
}
