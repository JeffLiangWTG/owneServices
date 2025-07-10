using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusInBondBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			AssertEquals(typeof(CusInBondBill), Lookups.Parent.GetType());
		}

		public void TestWeightUnits()
		{
			var weightUnits = Lookups.WeightUnits;
			var list = Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);
			AssertSame(list, weightUnits);
			Assert("Weight Units", weightUnits.ContainsCode(Core.Constants.Weight.Kilograms));
		}

		public void TestManifestUnits()
		{
			var manifestUnits = Lookups.ManifestUnits;
			var list = RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory);
			AssertSame(list, manifestUnits);
			Assert("Manifest Units", manifestUnits.ContainsCode(Core.Constants.PkgUnit.Package));
		}

		CusInBondBillLookups Lookups => CusInBondBill.Lookups;
		CusInBondHeader CusInBondHeader => fCusInBondHeader ?? (fCusInBondHeader = Factory.New<CusInBondHeader>());
		CusInBondHeader fCusInBondHeader;
		CusInBondBill CusInBondBill => fCusInBondBill ?? (fCusInBondBill = CusInBondHeader.Bills.AddNew());
		CusInBondBill fCusInBondBill;
	}
}
