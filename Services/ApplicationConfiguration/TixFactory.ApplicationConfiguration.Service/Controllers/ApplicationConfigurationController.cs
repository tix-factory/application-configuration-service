using System;
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
		public IActionResult GetApplicationSettings([FromHeader(Name = Startup.ApiKeyHeaderName)] Guid apiKey)
		{
			return _OperationExecuter.Execute(_ApplicationConfigurationOperations.GetApplicationSettingsOperation, apiKey);
		}

		[HttpPost]
		public IActionResult SetApplicationSetting([FromBody] SetApplicationSettingRequest request)
		{
			return _OperationExecuter.Execute(_ApplicationConfigurationOperations.SetApplicationSettingOperation, request);
		}

		[HttpPost]
		public IActionResult DeleteApplicationSetting([FromBody] DeleteApplicationSettingRequest request)
		{
			return _OperationExecuter.Execute(_ApplicationConfigurationOperations.DeleteApplicationSettingOperation, request);
		}
	}
}
