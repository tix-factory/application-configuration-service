using System.Runtime.Serialization;

namespace TixFactory.ApplicationConfiguration
{
	[DataContract]
	public class DeleteApplicationSettingRequest
	{
		[DataMember(Name = "applicationName")]
		public string ApplicationName { get; set; }

		[DataMember(Name = "settingName")]
		public string SettingName { get; set; }
	}
}
