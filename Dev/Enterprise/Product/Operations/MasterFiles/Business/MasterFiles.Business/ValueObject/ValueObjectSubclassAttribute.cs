using System;

namespace Enterprise.DataTransfer.Xml
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ValueObjectSubclassAttribute : Attribute
	{
		public ValueObjectSubclassAttribute(string typeName)
		{
			this.TypeName = typeName;
		}

		public readonly string TypeName;
	}
}
