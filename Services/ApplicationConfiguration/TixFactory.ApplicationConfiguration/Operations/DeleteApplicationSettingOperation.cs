using System;
using System.Threading;
using System.Threading.Tasks;
using TixFactory.ApplicationConfiguration.Entities;
using TixFactory.Operations;

namespace TixFactory.ApplicationConfiguration
{
	internal class DeleteApplicationSettingOperation : IAsyncOperation<DeleteApplicationSettingRequest, DeleteApplicationSettingResult>
	{
		private readonly ISettingEntityFactory _SettingEntityFactory;

		public DeleteApplicationSettingOperation(ISettingEntityFactory settingEntityFactory)
		{
			_SettingEntityFactory = settingEntityFactory ?? throw new ArgumentNullException(nameof(settingEntityFactory));
		}

		public async Task<(DeleteApplicationSettingResult output, OperationError error)> Execute(DeleteApplicationSettingRequest request, CancellationToken cancellationToken)
		{
			var setting = await _SettingEntityFactory.GetSettingBySettingsGroupNameAndSettingName(request.ApplicationName, request.SettingName, cancellationToken).ConfigureAwait(false);
			if (setting == null)
			{
				return (DeleteApplicationSettingResult.AlreadyDeleted, null);
			}

			await _SettingEntityFactory.DeleteSetting(setting, cancellationToken).ConfigureAwait(false);

			return (DeleteApplicationSettingResult.Deleted, null);
		}
	}
}
