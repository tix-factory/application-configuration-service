using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TixFactory.ApplicationConfiguration.Entities
{
	internal interface ISettingEntityFactory
	{
		Task<Setting> CreateSetting(string settingsGroupName, string name, string value, CancellationToken cancellationToken);

		Task<Setting> GetSettingBySettingsGroupNameAndSettingName(string settingsGroupName, string name, CancellationToken cancellationToken);

		Task<IReadOnlyCollection<Setting>> GetSettingsByGroupName(string settingsGroupName, CancellationToken cancellationToken);

		Task UpdateSetting(Setting setting, CancellationToken cancellationToken);

		Task DeleteSetting(Setting setting, CancellationToken cancellationToken);
	}
}
