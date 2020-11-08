using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TixFactory.Collections;
using TixFactory.Configuration;
using TixFactory.Data.MySql;

namespace TixFactory.ApplicationConfiguration.Entities
{
	internal class SettingsGroupEntityFactory : ISettingsGroupEntityFactory
	{
		private const string _InsertSettingsGroupStoredProcedureName = "InsertSettingsGroup";
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

		public async Task<SettingsGroup> GetOrCreateSettingsGroup(string name, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
			}

			if (name.Length > EntityValidation.MaxSettingsGroupNameLength)
			{
				throw new ArgumentException($"{nameof(name)} cannot be longer than {EntityValidation.MaxSettingsGroupNameLength}", nameof(name));
			}

			var settingsGroup = await GetSettingsGroupByName(name, cancellationToken).ConfigureAwait(false);
			if (settingsGroup != null)
			{
				return settingsGroup;
			}

			var settingsGroupId = await _DatabaseConnection.ExecuteInsertStoredProcedureAsync<long>(_InsertSettingsGroupStoredProcedureName, new[]
			{
				new MySqlParameter("@_Name", name)
			}, cancellationToken).ConfigureAwait(false);

			_SettingsGroupsByName.Remove(name);

			return await GetSettingsGroupByName(name, cancellationToken).ConfigureAwait(false);
		}

		public async Task<SettingsGroup> GetSettingsGroupByName(string name, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(name) || name.Length > EntityValidation.MaxSettingsGroupNameLength)
			{
				return null;
			}

			if (_SettingsGroupsByName.TryGetValue(name, out var settingsGroup))
			{
				return settingsGroup;
			}

			var settingsGroups = await _DatabaseConnection.ExecuteReadStoredProcedureAsync<SettingsGroup>(_GetSettingsGroupByNameStoredProcedureName, new[]
			{
				new MySqlParameter("@_Name", name)
			}, cancellationToken).ConfigureAwait(false);

			settingsGroup = _SettingsGroupsByName[name] = settingsGroups.FirstOrDefault();
			return settingsGroup;
		}

		public async Task DeleteSettingsGroup(SettingsGroup settingsGroup, CancellationToken cancellationToken)
		{
			await _DatabaseConnection.ExecuteWriteStoredProcedureAsync(_DeleteSettingsGroupStoredProcedureName, new[]
			{
				new MySqlParameter(@"_ID", settingsGroup.Id)
			}, cancellationToken).ConfigureAwait(false);

			_SettingsGroupsByName.Remove(settingsGroup.Name);
		}
	}
}
