using System.Threading;
using System.Threading.Tasks;

namespace TixFactory.ApplicationConfiguration.Entities
{
	internal interface ISettingsGroupEntityFactory
	{
		Task<SettingsGroup> GetOrCreateSettingsGroup(string name, CancellationToken cancellationToken);

		Task<SettingsGroup> GetSettingsGroupByName(string name, CancellationToken cancellationToken);

		Task DeleteSettingsGroup(SettingsGroup settingsGroup, CancellationToken cancellationToken);
	}
}
