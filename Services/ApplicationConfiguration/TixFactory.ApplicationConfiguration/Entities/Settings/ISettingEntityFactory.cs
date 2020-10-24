using System.Collections.Generic;

namespace TixFactory.ApplicationConfiguration.Entities
{
	internal interface ISettingEntityFactory
	{
		Setting CreateSetting(string settingsGroupName, string name, string value);

		Setting GetSettingBySettingsGroupNameAndSettingName(string settingsGroupName, string name);

		IReadOnlyCollection<Setting> GetSettingsByGroupName(string settingsGroupName);

		void UpdateSetting(Setting setting);

		void DeleteSetting(long id);
	}
}
