using System;
using System.Collections.Concurrent;
using System.Linq;
using MySql.Data.MySqlClient;
using TixFactory.Collections;
using TixFactory.Configuration;

namespace TixFactory.ApplicationConfiguration.Entities
{
	internal class SettingsGroupEntityFactory : ISettingsGroupEntityFactory
	{
		private const string _InsertSettingsGroupStoredProcedureName = "InsertSettingsGroup";
		private const string _UpdateSettingsGroupStoredProcedureName = "UpdateSettingsGroup";
		private const string _GetSettingsGroupByNameStoredProcedureName = "GetSettingsGroupByName";
		private const string _DeleteSettingsGroupStoredProcedureName = "DeleteGroupsGroup";
		private readonly TimeSpan _SettingsGroupCacheExpiry = TimeSpan.FromMinutes(1);
		private readonly IDatabaseConnection _DatabaseConnection;
		private readonly ExpirableDictionary<string, SettingsGroup> _SettingsGroupsByName;

		public SettingsGroupEntityFactory(IDatabaseConnection databaseConnection)
		{
			_DatabaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
			_SettingsGroupsByName = new ExpirableDictionary<string, SettingsGroup>(
				dictionary: new ConcurrentDictionary<string, SettingsGroup>(StringComparer.OrdinalIgnoreCase),
				valueExpiration: new Setting<TimeSpan>(_SettingsGroupCacheExpiry),
				expirationPolicy: new Setting<ExpirationPolicy>(ExpirationPolicy.RenewOnRead));
		}

		public SettingsGroup GetOrCreateSettingsGroup(string name)
		{
			var settingsGroup = GetSettingsGroupByName(name);
			if (settingsGroup != null)
			{
				return settingsGroup;
			}

			var settingsGroupId = _DatabaseConnection.ExecuteInsertStoredProcedure<long>(_InsertSettingsGroupStoredProcedureName, new[]
			{
				new MySqlParameter("@_Name", name)
			});

			_SettingsGroupsByName.Remove(name);

			return GetSettingsGroupByName(name);
		}

		public SettingsGroup GetSettingsGroupByName(string name)
		{
			if (_SettingsGroupsByName.TryGetValue(name, out var settingsGroup))
			{
				return settingsGroup;
			}

			var settingsGroups = _DatabaseConnection.ExecuteReadStoredProcedure<SettingsGroup>(_GetSettingsGroupByNameStoredProcedureName, new[]
			{
				new MySqlParameter("@_Name", name)
			});

			settingsGroup = _SettingsGroupsByName[name] = settingsGroups.FirstOrDefault();
			return settingsGroup;
		}

		public void UpdateSettingsGroup(SettingsGroup settingsGroup)
		{
			_DatabaseConnection.ExecuteWriteStoredProcedure(_UpdateSettingsGroupStoredProcedureName, new[]
			{
				new MySqlParameter(@"_ID", settingsGroup.Id),
				new MySqlParameter("@_Name", settingsGroup.Name)
			});
		}

		public void DeleteSettingsGroup(long id)
		{
			_DatabaseConnection.ExecuteWriteStoredProcedure(_DeleteSettingsGroupStoredProcedureName, new[]
			{
				new MySqlParameter(@"_ID", id)
			});

			foreach (var settingsGroup in _SettingsGroupsByName.Values)
			{
				if (settingsGroup.Id == id)
				{
					_SettingsGroupsByName.Remove(settingsGroup.Name);
					return;
				}
			}
		}
	}
}
