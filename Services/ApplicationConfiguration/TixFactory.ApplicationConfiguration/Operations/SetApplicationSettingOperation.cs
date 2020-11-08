using System;
using System.Threading;
using System.Threading.Tasks;
using TixFactory.ApplicationConfiguration.Entities;
using TixFactory.Operations;

namespace TixFactory.ApplicationConfiguration
{
	internal class SetApplicationSettingOperation : IAsyncOperation<SetApplicationSettingRequest, SetApplicationSettingResult>
	{
		private readonly ISettingEntityFactory _SettingEntityFactory;

		public SetApplicationSettingOperation(ISettingEntityFactory settingEntityFactory)
		{
			_SettingEntityFactory = settingEntityFactory ?? throw new ArgumentNullException(nameof(settingEntityFactory));
		}

		public async Task<(SetApplicationSettingResult output, OperationError error)> Execute(SetApplicationSettingRequest request, CancellationToken cancellationToken)
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

			var setting = await _SettingEntityFactory.GetSettingBySettingsGroupNameAndSettingName(request.ApplicationName, request.SettingName, cancellationToken).ConfigureAwait(false);
			if (setting == null)
			{
				await _SettingEntityFactory.CreateSetting(request.ApplicationName, request.SettingName, request.SettingValue, cancellationToken).ConfigureAwait(false);
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
				await _SettingEntityFactory.UpdateSetting(setting, cancellationToken).ConfigureAwait(false);
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
