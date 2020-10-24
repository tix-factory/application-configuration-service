using System;
using System.Runtime.Serialization;

namespace TixFactory.ApplicationConfiguration.Entities
{
	[DataContract(Name = "settings")]
	internal class Setting
	{
		[DataMember(Name = "ID")]
		public long Id { get; set; }

		[DataMember(Name = "SettingsGroupID")]
		public long SettingsGroupId { get; set; }

		[DataMember(Name = "Name")]
		public string Name { get; set; }

		[DataMember(Name = "Value")]
		public string Value { get; set; }

		[DataMember(Name = "Updated")]
		public DateTime Updated { get; set; }

		[DataMember(Name = "Created")]
		public DateTime Created { get; set; }
	}
}
