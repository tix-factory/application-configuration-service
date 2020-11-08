using System;
using System.Collections.Generic;
using TixFactory.Operations;

namespace TixFactory.ApplicationConfiguration
{
	public interface IApplicationConfigurationOperations
	{
		IAsyncOperation<Guid, IReadOnlyDictionary<string, string>> GetApplicationSettingsOperation { get; }

		IAsyncOperation<SetApplicationSettingRequest, SetApplicationSettingResult> SetApplicationSettingOperation { get; }

		IAsyncOperation<DeleteApplicationSettingRequest, DeleteApplicationSettingResult> DeleteApplicationSettingOperation { get; }
	}
}
