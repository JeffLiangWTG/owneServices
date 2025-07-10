using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class CusInBondBillLookups : Customs.Business.CusInBondBillLookups
	{
		public CusInBondBillLookups(CusInBondBill parent)
			: base(parent)
		{
		}

		public new CusInBondBill Parent => (CusInBondBill)base.Parent;

		public ICodeDescriptionPairList WeightUnits => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

		public ICodeDescriptionPairList ManifestUnits => RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory);
	}
}
