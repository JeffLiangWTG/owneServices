using System;
using System.Reflection;

namespace Enterprise.Freight.Forwarding.AWB.TNT
{
	[AttributeUsage(AttributeTargets.All)]
	sealed class FieldDefinitionAttribute : Attribute
	{
		public FieldDefinitionAttribute(int order, int length, int decimals)
			: this(order, length)
		{
			this.Decimals = decimals;
		}

		public FieldDefinitionAttribute(int order, int length)
		{
			this.Order = order;
			this.Length = length;
		}

		internal readonly int Order;
		internal readonly int Length;
		internal readonly int Decimals;
		internal PropertyInfo DataPropertyInfo;
	}
}
