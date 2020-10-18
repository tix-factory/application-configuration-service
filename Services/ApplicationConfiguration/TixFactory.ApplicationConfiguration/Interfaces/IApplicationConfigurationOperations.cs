using System;
using System.Collections.Generic;
using TixFactory.Operations;

namespace TixFactory.ApplicationConfiguration
{
	public interface IApplicationConfigurationOperations
	{
		IOperation<Guid, IReadOnlyDictionary<string, string>> GetApplicationSettingsOperation { get; }
	}
}
