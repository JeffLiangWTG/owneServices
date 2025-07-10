using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CargoManifestQueryBizObj))]
	sealed class CargoManifestQueryBizObjTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUseDifferentFactoryToHeader()
		{
			var header = new CargoManifestQueryHeader(Factory);
			var bizObj = new CargoManifestQueryBizObj(header);
			Assert(header.Factory.GetHashCode() != bizObj.Factory.GetHashCode());
		}

		public void TestValidateInBondNumber()
		{
			var header = new CargoManifestQueryHeader(Factory);
			var bizObj = new CargoManifestQueryBizObj(header);
			header.ActionCode = CargoManifestStatusQueryActionList.Codes.InBond;

			bizObj.InBondNumber = "";
			AssertHasMessageErrorContaining(bizObj.InBondNumberInfo, MandatoryValidation.YouHaveNotEntered);

			bizObj.InBondNumber = "1";
			AssertNoMessageErrorContaining(bizObj.InBondNumberInfo, MandatoryValidation.YouHaveNotEntered);

			header.ActionCode = CargoManifestStatusQueryActionList.Codes.AIR;
			bizObj.InBondNumber = "1";
			AssertNoNotifications(bizObj.InBondNumberInfo);
		}

		public void TestValidateMasterBillNumber()
		{
			var header = new CargoManifestQueryHeader(Factory);
			var bizObj = new CargoManifestQueryBizObj(header);
			header.ActionCode = CargoManifestStatusQueryActionList.Codes.AIR;

			bizObj.MasterBillNumber = "";
			AssertHasMessageErrorContaining(bizObj.MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);

			bizObj.MasterBillNumber = "1";
			AssertNoMessageErrorContaining(bizObj.MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);

			header.ActionCode = CargoManifestStatusQueryActionList.Codes.InBond;
			bizObj.MasterBillNumber = "1";
			AssertNoNotifications(bizObj.MasterBillNumberInfo);
		}

		public void TestValidateIssuerCode()
		{
			var header = new CargoManifestQueryHeader(Factory);
			var bizObj = new CargoManifestQueryBizObj(header);
			header.ActionCode = CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill;

			bizObj.Issuer = "";
			AssertHasMessageErrorContaining(bizObj.IssuerInfo, MandatoryValidation.YouHaveNotEntered);

			bizObj.Issuer = "1";
			AssertNoMessageErrorContaining(bizObj.IssuerInfo, MandatoryValidation.YouHaveNotEntered);

			header.ActionCode = CargoManifestStatusQueryActionList.Codes.AIR;
			bizObj.Issuer = "1";
			AssertNoNotifications(bizObj.IssuerInfo);
		}

		public void TestRunPreSaveValidation()
		{
			var header = new CargoManifestQueryHeader(Factory);
			var bizObj = new CargoManifestQueryBizObj(header);
			bizObj.RunPreSaveValidation();

			header.ActionCode = CargoManifestStatusQueryActionList.Codes.AIR;
			bizObj.RunPreSaveValidation();
			AssertNoMessageErrors(bizObj.IssuerInfo);
			AssertHasMessageErrors(bizObj.MasterBillNumberInfo);
			AssertNoNotifications(bizObj.HouseBillNumberInfo);
			AssertNoMessageErrors(bizObj.InBondNumberInfo);

			header.ActionCode = CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill;
			bizObj.RunPreSaveValidation();
			AssertHasMessageErrors(bizObj.IssuerInfo);
			AssertHasMessageErrors(bizObj.MasterBillNumberInfo);
			AssertNoNotifications(bizObj.HouseBillNumberInfo);
			AssertNoMessageErrors(bizObj.InBondNumberInfo);

			header.ActionCode = CargoManifestStatusQueryActionList.Codes.InBond;
			bizObj.RunPreSaveValidation();
			AssertNoMessageErrors(bizObj.IssuerInfo);
			AssertNoMessageErrors(bizObj.MasterBillNumberInfo);
			AssertNoNotifications(bizObj.HouseBillNumberInfo);
			AssertHasMessageErrors(bizObj.InBondNumberInfo);
		}

		public void TestICargoManifestStatusQueryData()
		{
			var header = new CargoManifestQueryHeader(Factory);
			var bizObj = new CargoManifestQueryBizObj(header);
			ICargoManifestQuerySendingObject queryData = bizObj;

			bizObj.MasterBillNumber = "M1";
			AssertEquals("M1", queryData.MasterBillNumber);

			bizObj.Issuer = "ABCD";
			AssertEquals("ABCD", queryData.BillIssuerCode);

			bizObj.InBondNumber = "1";
			AssertEquals("1", queryData.EntryOrInBondNumber);

			bizObj.HouseBillNumber = "2";
			AssertEquals("2", queryData.HouseBillNumber);
			AssertEquals(bizObj.Factory, queryData.Factory);
		}

		public void TestBillNumberMaxLength()
		{
			var header = new CargoManifestQueryHeader(Factory);
			var cargoManifestQueryBizObj = new CargoManifestQueryBizObj(header);

			header.ActionCode = CargoManifestStatusQueryActionList.Codes.AIR;
			AssertEquals("11 for air waybill", 11, cargoManifestQueryBizObj.MasterBillNumber_MaxLength);

			header.ActionCode = CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill;
			AssertEquals("otherwise 12", 12, cargoManifestQueryBizObj.MasterBillNumber_MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = new CargoManifestQueryHeader(Factory);
			return new CargoManifestQueryBizObj(header);
		}
	}
}
