using System;
using System.Runtime.Serialization;

namespace TixFactory.ApplicationConfiguration
{
	[DataContract]
	public class SetApplicationSettingValueRequest
	{
		public Guid ApiKey { get; set; }

		[DataMember(Name = "settingName")]
		public string SettingName { get; set; }

		[DataMember(Name = "settingValue")]
		public string SettingValue { get; set; }
	}
}
