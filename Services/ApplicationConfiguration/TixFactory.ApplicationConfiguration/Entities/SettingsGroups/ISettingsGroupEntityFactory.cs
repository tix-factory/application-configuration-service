namespace TixFactory.ApplicationConfiguration.Entities
{
	internal interface ISettingsGroupEntityFactory
	{
		SettingsGroup GetOrCreateSettingsGroup(string name);

		SettingsGroup GetSettingsGroupByName(string name);

		void UpdateSettingsGroup(SettingsGroup settingsGroup);

		void DeleteSettingsGroup(long id);
	}
}
