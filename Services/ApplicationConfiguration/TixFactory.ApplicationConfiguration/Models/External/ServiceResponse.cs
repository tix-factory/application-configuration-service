using System.Runtime.Serialization;

namespace TixFactory.ApplicationConfiguration
{
	[DataContract]
	internal class ServiceResponse<T>
	{
		[DataMember(Name = "data")]
		public T Data { get; set; }
	}
}
