using System;
using TixFactory.ApplicationConfiguration.Entities;
using TixFactory.Operations;

namespace TixFactory.ApplicationConfiguration
{
	internal class SetApplicationSettingOperation : IOperation<SetApplicationSettingRequest, SetApplicationSettingResult>
	{
		private readonly ISettingEntityFactory _SettingEntityFactory;

		public SetApplicationSettingOperation(ISettingEntityFactory settingEntityFactory)
		{
			_SettingEntityFactory = settingEntityFactory ?? throw new ArgumentNullException(nameof(settingEntityFactory));
		}

		public (SetApplicationSettingResult output, OperationError error) Execute(SetApplicationSettingRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.ApplicationName) || request.ApplicationName.Length > EntityValidation.MaxSettingsGroupNameLength)
			{
				return (default, new OperationError(ApplicationConfigurationError.InvalidSettingsGroupName));
			}

			if (string.IsNullOrWhiteSpace(request.SettingName) || request.SettingName.Length > EntityValidation.MaxSettingNameLength)
			{
				return (default, new OperationError(ApplicationConfigurationError.InvalidSettingName));
			}

			if (string.IsNullOrWhiteSpace(request.SettingValue) || request.SettingValue.Length > EntityValidation.MaxSettingValueLength)
			{
				return (default, new OperationError(ApplicationConfigurationError.InvalidSettingValue));
			}

			var setting = _SettingEntityFactory.GetSettingBySettingsGroupNameAndSettingName(request.ApplicationName, request.SettingName);
			if (setting == null)
			{
				_SettingEntityFactory.CreateSetting(request.ApplicationName, request.SettingName, request.SettingValue);
				return (SetApplicationSettingResult.Changed, null);
			}

			if (setting.Value == request.SettingValue)
			{
				return (SetApplicationSettingResult.Unchanged, null);
			}

			var restoreValue = setting.Value;
			setting.Value = request.SettingValue;

			try
			{
				_SettingEntityFactory.UpdateSetting(setting);
			}
			catch
			{
				setting.Value = restoreValue;
				throw;
			}

			return (SetApplicationSettingResult.Changed, null);
		}
	}
}
