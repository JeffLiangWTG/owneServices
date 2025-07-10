

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class DutiableUQList : CodeDescriptionPairList
	{
		public DutiableUQList()
			: base()
		{
			AddPair(UnitOfQuantityCodeList.Codes.DAL, UnitOfQuantityCodeList.Descriptions.DAL);
			AddPair(UnitOfQuantityCodeList.Codes.KGM, UnitOfQuantityCodeList.Descriptions.KGM);
			AddPair(UnitOfQuantityCodeList.Codes.LTR, UnitOfQuantityCodeList.Descriptions.LTR);
			AddPair(UnitOfQuantityCodeList.Codes.NMB, UnitOfQuantityCodeList.Descriptions.NMB);
			AddPair(UnitOfQuantityCodeList.Codes.STK, UnitOfQuantityCodeList.Descriptions.STK);
		}
	}
}
