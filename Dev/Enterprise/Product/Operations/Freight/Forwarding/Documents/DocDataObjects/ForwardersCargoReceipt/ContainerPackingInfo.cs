using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	internal class ContainerPackingInfo : DocDataObject
	{
		#region ContainerNumber

		public ZString ContainerNumber
		{
			get => containerNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ContainerNumberInfo, ref containerNumber, value))
				{
					Validate(ContainerNumberInfo);
				}
			}
		}

		ZString containerNumber;

		public ZPropertyInfo ContainerNumberInfo => GetZPropertyInfo(nameof(ContainerNumber));

		#endregion

		#region ContainerPackingInfos

		public IReadOnlyCollection<PackedOrderLine> PackedOrderLines
		{
			get => packedOrderLines;
			set => packedOrderLines = SetChildCollection(packedOrderLines, value);
		}

		IReadOnlyCollection<PackedOrderLine> packedOrderLines;

		#endregion
	}
}
