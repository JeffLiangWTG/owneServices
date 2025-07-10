using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.eTail.DataTransfer
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ShipmentDirectionAttribute : Attribute
	{
		public ShipmentDirectionAttribute(Directions[] directions)
		{
			Directions = directions;
		}

		public readonly Directions[] Directions;
	}
}
