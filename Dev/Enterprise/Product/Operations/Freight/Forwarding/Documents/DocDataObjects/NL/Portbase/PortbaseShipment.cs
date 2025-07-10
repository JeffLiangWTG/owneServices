using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL
{
	sealed class PortbaseShipment : DocDataObject
	{
		#region Number

		public ZString Number
		{
			get => number;
			set
			{
				if (SetNonPersistentPropertyValue(NumberInfo, ref number, value))
				{
					Validate(NumberInfo);
				}
			}
		}
		ZString number;

		public ZPropertyInfo NumberInfo => GetZPropertyInfo(nameof(Number));

		#endregion

		#region Quantity

		public ZInt Quantity
		{
			get => quantity;
			set
			{
				if (SetNonPersistentPropertyValue(QuantityInfo, ref quantity, value))
				{
					Validate(QuantityInfo);
				}
			}
		}

		ZInt quantity;

		public ZPropertyInfo QuantityInfo => GetZPropertyInfo(nameof(Quantity));

		#endregion

		#region PackageType

		public ICodeDescription PackageType
		{
			get => packageType;
			set => packageType = SetChild(packageType, value);
		}

		ICodeDescription packageType;

		#endregion

		#region Weight

		public Measurement Weight
		{
			get => weight;
			set => weight = SetChild(weight, value);
		}

		Measurement weight;

		#endregion
	}
}
