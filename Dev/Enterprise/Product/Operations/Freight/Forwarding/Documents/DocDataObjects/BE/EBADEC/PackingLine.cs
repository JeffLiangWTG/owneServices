using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	class PackingLine : DocDataObject
	{
		public PackingLine(object identifier)
			: base(identifier)
		{
		}

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

		#region VIN

		public ZString VIN
		{
			get => vin;
			set
			{
				if (SetNonPersistentPropertyValue(VINInfo, ref vin, value))
				{
					Validate(VINInfo);
				}
			}
		}

		ZString vin;

		public ZPropertyInfo VINInfo => GetZPropertyInfo(nameof(VIN));

		#endregion

		#region MovementReferenceNumber

		public IReadOnlyCollection<MovementReferenceNumber> MovementReferenceNumbers
		{
			get => movementReferenceNumbers;
			set => movementReferenceNumbers = SetChildCollection(movementReferenceNumbers, value);
		}

		IReadOnlyCollection<MovementReferenceNumber> movementReferenceNumbers;

		#endregion
	}
}
