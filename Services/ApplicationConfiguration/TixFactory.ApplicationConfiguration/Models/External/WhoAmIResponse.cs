using System.Runtime.Serialization;

namespace TixFactory.ApplicationConfiguration
{
	[DataContract]
	internal class WhoAmIResponse
	{
		[DataMember(Name = "applicationName")]
		public string ApplicationName { get; set; }
	}
}
