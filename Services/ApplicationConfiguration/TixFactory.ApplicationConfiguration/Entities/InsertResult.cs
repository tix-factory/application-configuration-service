using System.Runtime.Serialization;

namespace TixFactory.ApplicationConfiguration.Entities
{
	[DataContract]
	internal class InsertResult<T>
	{
		[DataMember(Name = "ID")]
		public T Id { get; set; }
	}
}
