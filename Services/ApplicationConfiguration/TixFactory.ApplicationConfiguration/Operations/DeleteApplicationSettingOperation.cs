using System;
using TixFactory.ApplicationConfiguration.Entities;
using TixFactory.Operations;

namespace TixFactory.ApplicationConfiguration
{
	internal class DeleteApplicationSettingOperation : IOperation<DeleteApplicationSettingRequest, DeleteApplicationSettingResult>
	{
		private readonly ISettingEntityFactory _SettingEntityFactory;

		public DeleteApplicationSettingOperation(ISettingEntityFactory settingEntityFactory)
		{
			_SettingEntityFactory = settingEntityFactory ?? throw new ArgumentNullException(nameof(settingEntityFactory));
		}

		public (DeleteApplicationSettingResult output, OperationError error) Execute(DeleteApplicationSettingRequest request)
		{
			var setting = _SettingEntityFactory.GetSettingBySettingsGroupNameAndSettingName(request.ApplicationName, request.SettingName);
			if (setting == null)
			{
				return (DeleteApplicationSettingResult.AlreadyDeleted, null);
			}

			_SettingEntityFactory.DeleteSetting(setting.Id);

			return (DeleteApplicationSettingResult.Deleted, null);
		}
	}
}
