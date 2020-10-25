using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using TixFactory.Collections;
using TixFactory.Data.MySql;

namespace TixFactory.ApplicationConfiguration.Entities
{
	internal class SettingEntityFactory : ISettingEntityFactory
	{
		private const string _InsertSettingStoredProcedureName = "InsertSetting";
		private const string _UpdateSettingStoredProcedureName = "UpdateSetting";
		private const string _GetSettingsBySettingsGroupIdStoredProcedureName = "GetSettingsBySettingsGroupId";
		private const string _DeleteSettingStoredProcedureName = "DeleteSetting";
		private const int _MaxSettingsPerSettingsGroup = 1000;
		private readonly TimeSpan _SettingsCacheExpiry = TimeSpan.FromMinutes(1);
		private readonly IDatabaseConnection _DatabaseConnection;
		private readonly ISettingsGroupEntityFactory _SettingsGroupEntityFactory;
		private readonly ExpirableDictionary<long, IReadOnlyCollection<Setting>> _SettingsBySettingsGroupId;

		public SettingEntityFactory(IDatabaseConnection databaseConnection, ISettingsGroupEntityFactory settingsGroupEntityFactory)
		{
			_DatabaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
			_SettingsGroupEntityFactory = settingsGroupEntityFactory ?? throw new ArgumentNullException(nameof(settingsGroupEntityFactory));
			_SettingsBySettingsGroupId = new ExpirableDictionary<long, IReadOnlyCollection<Setting>>(_SettingsCacheExpiry, ExpirationPolicy.RenewOnRead);
		}

		public Setting CreateSetting(string settingsGroupName, string name, string value)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
			}

			if (name.Length > EntityValidation.MaxSettingNameLength)
			{
				throw new ArgumentException($"{nameof(name)} cannot be longer than {EntityValidation.MaxSettingNameLength}", nameof(name));
			}

			var settingsGroup = _SettingsGroupEntityFactory.GetOrCreateSettingsGroup(settingsGroupName);

			var setting = GetSettingBySettingsGroupNameAndSettingName(settingsGroupName, name);
			if (setting != null)
			{
				throw new DuplicateNameException($"A setting with the same name in the same settings group already exists.\n\tSettings group: {settingsGroup.Name}\n\tSetting name: {name}");
			}

			var settings = GetSettingsByGroupName(settingsGroupName);
			if (settings.Count >= _MaxSettingsPerSettingsGroup)
			{
				throw new ApplicationException($"There cannot be more than {_MaxSettingsPerSettingsGroup} per settings group.\n\tSettings group: {settingsGroup.Name}");
			}

			var settingId = _DatabaseConnection.ExecuteInsertStoredProcedure<long>(_InsertSettingStoredProcedureName, new[]
			{
				new MySqlParameter("@_SettingsGroupID", settingsGroup.Id),
				new MySqlParameter("@_Name", name),
				new MySqlParameter("@_Value", value)
			});

			_SettingsBySettingsGroupId.Remove(settingsGroup.Id);

			settings = GetSettingsByGroupName(settingsGroupName);
			return settings.First(s => s.Id == settingId);
		}

		public Setting GetSettingBySettingsGroupNameAndSettingName(string settingsGroupName, string name)
		{
			var settings = GetSettingsByGroupName(settingsGroupName);
			return settings.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
		}

		public IReadOnlyCollection<Setting> GetSettingsByGroupName(string settingsGroupName)
		{
			var settingsGroup = _SettingsGroupEntityFactory.GetSettingsGroupByName(settingsGroupName);
			if (settingsGroup == null)
			{
				return Array.Empty<Setting>();
			}

			if (_SettingsBySettingsGroupId.TryGetValue(settingsGroup.Id, out var settings))
			{
				return settings;
			}

			settings = _SettingsBySettingsGroupId[settingsGroup.Id] = _DatabaseConnection.ExecuteReadStoredProcedure<Setting>(_GetSettingsBySettingsGroupIdStoredProcedureName, new[]
			{
				new MySqlParameter("@_SettingsGroupID", settingsGroup.Id),
				new MySqlParameter("@_Count", _MaxSettingsPerSettingsGroup)
			});

			return settings;
		}

		public void UpdateSetting(Setting setting)
		{
			if (string.IsNullOrWhiteSpace(setting.Name))
			{
				throw new ArgumentException($"{nameof(setting)}.{nameof(setting.Name)} cannot be null or whitespace.", nameof(setting));
			}

			if (string.IsNullOrWhiteSpace(setting.Value))
			{
				throw new ArgumentException($"{nameof(setting)}.{nameof(setting.Value)} cannot be null or whitespace.", nameof(setting));
			}

			if (setting.Name.Length > EntityValidation.MaxSettingNameLength)
			{
				throw new ArgumentException($"{nameof(setting)}.{nameof(setting.Name)} cannot be longer than {EntityValidation.MaxSettingNameLength}", nameof(setting));
			}

			if (setting.Value.Length > EntityValidation.MaxSettingValueLength)
			{
				throw new ArgumentException($"{nameof(setting)}.{nameof(setting.Value)} cannot be longer than {EntityValidation.MaxSettingValueLength}", nameof(setting));
			}

			_DatabaseConnection.ExecuteWriteStoredProcedure(_UpdateSettingStoredProcedureName, new[]
			{
				new MySqlParameter(@"_ID", setting.Id),
				new MySqlParameter("@_SettingsGroupID", setting.SettingsGroupId),
				new MySqlParameter("@_Name", setting.Name),
				new MySqlParameter("@_Value", setting.Value),
			});
		}

		public void DeleteSetting(long id)
		{
			_DatabaseConnection.ExecuteWriteStoredProcedure(_DeleteSettingStoredProcedureName, new[]
			{
				new MySqlParameter(@"_ID", id)
			});

			foreach (var settings in _SettingsBySettingsGroupId)
			{
				foreach (var setting in settings.Value)
				{
					if (setting.Id == id)
					{
						_SettingsBySettingsGroupId.Remove(settings.Key);
						return;
					}
				}
			}
		}
	}
}
