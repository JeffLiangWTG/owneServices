using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public class EFSerializable : ISerializable
	{
		public EFSerializable()
		{
		}

		protected EFSerializable(SerializationInfo info, StreamingContext context)
		{
			foreach (var property in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (!property.IsDefined(typeof(IgnoreDataMemberAttribute), false))
					property.SetValue(this, info.GetValue(property.Name,  property.PropertyType));
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			foreach (var property in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (!property.IsDefined(typeof(IgnoreDataMemberAttribute), false))
					info.AddValue(property.Name, property.GetValue(this), property.PropertyType);
			}
		}
	}
}
