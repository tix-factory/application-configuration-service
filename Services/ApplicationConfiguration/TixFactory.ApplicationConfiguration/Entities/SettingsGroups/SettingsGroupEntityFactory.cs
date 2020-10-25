using System;
using System.Collections.Concurrent;
using System.Linq;
using MySql.Data.MySqlClient;
using TixFactory.Collections;
using TixFactory.Configuration;
using TixFactory.Data.MySql;

namespace TixFactory.ApplicationConfiguration.Entities
{
	internal class SettingsGroupEntityFactory : ISettingsGroupEntityFactory
	{
		private const string _InsertSettingsGroupStoredProcedureName = "InsertSettingsGroup";
		private const string _UpdateSettingsGroupStoredProcedureName = "UpdateSettingsGroup";
		private const string _GetSettingsGroupByNameStoredProcedureName = "GetSettingsGroupByName";
		private const string _DeleteSettingsGroupStoredProcedureName = "DeleteSettingsGroup";
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
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
			}

			if (name.Length > EntityValidation.MaxSettingsGroupNameLength)
			{
				throw new ArgumentException($"{nameof(name)} cannot be longer than {EntityValidation.MaxSettingsGroupNameLength}", nameof(name));
			}

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
			if (string.IsNullOrWhiteSpace(name) || name.Length > EntityValidation.MaxSettingsGroupNameLength)
			{
				return null;
			}

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
			if (string.IsNullOrWhiteSpace(settingsGroup.Name))
			{
				throw new ArgumentException($"{nameof(settingsGroup)}.{nameof(settingsGroup.Name)} cannot be null or whitespace.", nameof(settingsGroup));
			}

			if (settingsGroup.Name.Length > EntityValidation.MaxSettingsGroupNameLength)
			{
				throw new ArgumentException($"{nameof(settingsGroup)}.{nameof(settingsGroup.Name)} cannot be longer than {EntityValidation.MaxSettingsGroupNameLength}", nameof(settingsGroup));
			}

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
