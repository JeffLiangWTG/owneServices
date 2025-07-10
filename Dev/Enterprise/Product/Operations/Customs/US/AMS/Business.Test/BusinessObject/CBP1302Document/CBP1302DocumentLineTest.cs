using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CBP1302DocumentLine))]
	public class CBP1302DocumentLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBillNumberInCBP1302DocumentLine()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "WENDY THE DESTROYER".PadRight(OrgHeader.Schema.OH_FullNameMaxLength, '1');
			org.OH_RL_NKClosestPort = "USLAX";
			org.MainAddress.OA_Address1 = "ADDRESS 1".PadRight(OrgAddress.Schema.OA_Address1MaxLength, '2');
			org.MainAddress.OA_Address2 = "ADDRESS 2".PadRight(OrgAddress.Schema.OA_Address2MaxLength, '3');
			org.MainAddress.OA_City = "LOS ANGELES".PadRight(OrgAddress.Schema.OA_CityMaxLength, '4');
			org.MainAddress.OA_PostCode = "45663".PadRight(OrgAddress.Schema.OA_PostCodeMaxLength, '5');
			org.MainAddress.OA_State = "CA".PadRight(OrgAddress.Schema.OA_StateMaxLength, '6');
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "KDTE", Core.Constants.CountryCodes.UnitedStates);

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0011", "US11 Port of Discharge", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0012", "US12 Port of Discharge", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0013", "US13 Port of Discharge", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0014", "US14 Port of Discharge", startDate, endDate);
			newFactory.Save();

			var header = Factory.New<CusInBondHeader>();
			header.BH_PortUnladingDCode = "0014";

			var bill1 = header.Bills.AddNew();
			bill1.B0_IssuerCode = "ISR1";
			bill1.ForeignShipper.OrganisationPK = org.PK;
			bill1.B0_MasterBillNumber = "MASTERBILLNUMBER1";
			bill1.B0_InBondPortOfDestDCode = "0011";

			var bondBill1 = new CBP1302DocumentLine(bill1);
			bondBill1.FirstContainerInBill = true;
			var expected1 = string.Format("{0}{1}\r\n{2} {3}", bill1.B0_IssuerCode, bill1.B0_MasterBillNumber, "0011", "US11 Port of Discharge");
			AssertEquals("IssuerMasterBillNumber1", expected1, bondBill1.BillNumber);

			var bill2 = header.Bills.AddNew();
			bill2.B0_IssuerCode = "ISR2";
			bill2.ForeignShipper.OrganisationPK = org.PK;
			bill2.B0_MasterBillNumber = "";
			bill2.B0_InBondPortOfDestDCode = "0012";
			var container2 = bill2.MovementDetail.Containers.AddNew();
			container2.BC_ContainerNum = "CONT2";
			container2.BC_Seal2 = "SEAL2";

			var bill3 = header.Bills.AddNew();
			bill3.B0_IssuerCode = "ISR3";
			bill3.B0_MasterBillNumber = "MASTERBILLNUMBER3";
			bill3.B0_InBondPortOfDestDCode = "0013";
			var container3 = bill3.MovementDetail.Containers.AddNew();
			container3.BC_ContainerNum = "CONT3";
			container3.BC_Seal2 = "SEAL3";

			var bondBill2 = new CBP1302DocumentLine(bill2);
			bondBill2.FirstContainerInBill = true;
			var expected2 = string.Format("{0} {1}", "0012", "US12 Port of Discharge");
			AssertEquals("IssuerMasterBillNumber with MultiDischargePorts2", expected2, bondBill2.BillNumber);

			var bondBill3 = new CBP1302DocumentLine(bill3);
			bondBill3.FirstContainerInBill = true;
			var expected3 = string.Format("{0}{1}\r\n{2} {3}", bill3.B0_IssuerCode, bill3.B0_MasterBillNumber, "0013", "US13 Port of Discharge");
			AssertEquals("IssuerMasterBillNumber with MultiDischargePorts3", expected3, bondBill3.BillNumber);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CBP1302DocumentLine(Factory.New<CusInBondBill>());
		}
	}
}
