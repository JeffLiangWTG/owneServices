using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class CusInBondMoveLineItemLookups : Customs.Business.CusInBondMoveLineItemLookups
	{
		public CusInBondMoveLineItemLookups(CusInBondMoveLineItem parent)
			: base(parent)
		{
		}

		protected new CusInBondMoveLineItem Parent => (CusInBondMoveLineItem)base.Parent;

		public CodeDescriptionPairList PackageTypeList => RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory);
	}
}
