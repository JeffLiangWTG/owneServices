using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CargoManifestQueryHeader))]
	sealed class CargoManifestQueryHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestVisibility()
		{
			var header = new CargoManifestQueryHeader(Factory);
			header.ActionCode = CargoManifestStatusQueryActionList.Codes.AIR;
			Assert(!header.IsIssuerVisible);
			Assert(header.IsMasterBillNumberVisible);
			Assert(header.IsHouseBillNumberVisible);
			Assert(!header.IsInBondNumberVisible);

			header.ActionCode = CargoManifestStatusQueryActionList.Codes.InBond;
			Assert(!header.IsIssuerVisible);
			Assert(!header.IsMasterBillNumberVisible);
			Assert(!header.IsHouseBillNumberVisible);
			Assert(header.IsInBondNumberVisible);

			header.ActionCode = CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill;
			Assert(header.IsIssuerVisible);
			Assert(header.IsMasterBillNumberVisible);
			Assert(!header.IsHouseBillNumberVisible);
			Assert(!header.IsInBondNumberVisible);
		}

		public void TestActionCodeList()
		{
			var header = new CargoManifestQueryHeader(Factory);
			var list = header.ActionCodeList;
			AssertEquals("Element number of action code list.", 3, list.Count);
			Assert("ORT", list.ContainsCode(CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill));
			Assert("AIR", list.ContainsCode(CargoManifestStatusQueryActionList.Codes.AIR));
			Assert("INB", list.ContainsCode(CargoManifestStatusQueryActionList.Codes.InBond));
			AssertSame(list, new CargoManifestQueryHeader(Factory).ActionCodeList);
		}

		public void TestValidateActionCode()
		{
			var header = new CargoManifestQueryHeader(Factory);
			header.ActionCode = ZString.Empty;
			AssertHasErrorContaining(header.ActionCodeInfo, "Action Code");

			header.ActionCode = CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill;
			AssertNoMessageErrors(header.ActionCodeInfo);

			header.ActionCode = "~";
			AssertHasMessageErrorContaining(header.ActionCodeInfo, ListValidation.InvalidCodeMessageError);

			header.ActionCode = CargoManifestStatusQueryActionList.Codes.AIR;
			AssertNoMessageErrors(header.ActionCodeInfo);

			header.ActionCode = CargoManifestStatusQueryActionList.Codes.HAWB;
			AssertHasMessageErrorContaining(header.ActionCodeInfo, ListValidation.InvalidCodeMessageError);

			header.ActionCode = CargoManifestStatusQueryActionList.Codes.InBond;
			AssertNoMessageErrors(header.ActionCodeInfo);
		}

		protected override BusinessObject GetNewBusinessObject() => new CargoManifestQueryHeader(Factory);
	}
}
