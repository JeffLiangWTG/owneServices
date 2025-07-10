using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Transactions.Business
{
	[Immutable]
	public class AttributeParts : IPartAttributes
	{
		#region Constructors + Factory Methods

		public static AttributeParts New(WhsReleaseLine releaseLine)
		{
			Argument.NotNull(releaseLine, nameof(releaseLine));
			return new AttributeParts(
				releaseLine.PartAttribute1,
				releaseLine.PartAttribute2,
				releaseLine.PartAttribute3,
				releaseLine.SerialNumber,
				releaseLine.ExpiryDate,
				releaseLine.PackingDate);
		}

		public static AttributeParts New(WhsDocketLine docketLine)
		{
			Argument.NotNull(docketLine, nameof(docketLine));
			return new AttributeParts(
				docketLine.WE_PartAttrib1,
				docketLine.WE_PartAttrib2,
				docketLine.WE_PartAttrib3,
				docketLine.WE_SerialNumber,
				docketLine.WE_ExpiryDate,
				docketLine.WE_PackingDate);
		}

		AttributeParts(ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber, ZDate expiryDate, ZDate packingDate)
		{
			PartAttribute1 = partAttrib1;
			PartAttribute2 = partAttrib2;
			PartAttribute3 = partAttrib3;
			SerialNumber = serialNumber;
			ExpiryDate = expiryDate;
			PackingDate = packingDate;
		}

		#endregion

		public readonly ZString PartAttribute1;
		public readonly ZString PartAttribute2;
		public readonly ZString PartAttribute3;
		public readonly ZString SerialNumber;
		public readonly ZDate ExpiryDate;
		public readonly ZDate PackingDate;

		public string Key => WhsReleaseLineCollection.GetKey(this);

		public ZString GetPartAttribute(PartAttributeNumber partAttrib)
		{
			switch (partAttrib)
			{
				case PartAttributeNumber.One:
					return PartAttribute1;
				case PartAttributeNumber.Two:
					return PartAttribute2;
				case PartAttributeNumber.Three:
					return PartAttribute3;
				case PartAttributeNumber.SerialNumber:
					return SerialNumber;

				default:
					return ZString.Empty;
			}
		}

		public static readonly AttributeParts Empty = new AttributeParts(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDate.Empty, ZDate.Empty);

		// interfaces

		#region IPartAttributes Members

		ZDate IPartAttributes.ExpiryDate => ExpiryDate;
		ZDate IPartAttributes.PackingDate => PackingDate;
		ZString IPartAttributes.PartAttrib1 => PartAttribute1;
		ZString IPartAttributes.PartAttrib2 => PartAttribute2;
		ZString IPartAttributes.PartAttrib3 => PartAttribute3;
		ZString IPartAttributes.SerialNumber => SerialNumber;

		#endregion
	}
}
