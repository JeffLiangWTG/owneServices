using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class PackingWarpper : IPackaging
	{
		public PackingWarpper(ZDateTime packingDateTime) : this(packingDateTime, ZString.Empty, ZDecimal.Zero)
		{
		}

		public PackingWarpper(ZDateTime packingDateTime, ZString typeCode, ZDecimal quantityQuantity)
		{
			this.packingDateTime = packingDateTime;
			this.typeCode = typeCode;
			this.quantityQuantity = quantityQuantity;
		}

		readonly ZDateTime packingDateTime;

		readonly ZString typeCode;

		readonly ZDecimal quantityQuantity;

		ZDecimal IPackaging.QuantityQuantity => quantityQuantity;

		ZString IPackaging.TypeCode => typeCode;

		ZString IPackaging.MarksNumbers => null;

		ZString IPackaging.PackagingMaterialDescription => null;

		ZString IPackaging.Combination => null;

		ZDate IPackaging.PackingDateTime => packingDateTime.IsValid ? packingDateTime.Date : ZDate.Empty;
	}
}
