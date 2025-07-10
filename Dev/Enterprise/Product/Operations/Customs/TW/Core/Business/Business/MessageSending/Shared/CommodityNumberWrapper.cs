using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class CommodityNumberWrapper : ICommodityNumber
	{
		readonly ZString id;
		readonly ZString identifierTypeCode;

		public CommodityNumberWrapper(ZString id, ZString identifierTypeCode)
		{
			this.id = id;
			this.identifierTypeCode = identifierTypeCode;
		}

		ZString ICommodityNumber.ID => id;

		ZString ICommodityNumber.IdentifierTypeCode => identifierTypeCode;
	}
}
