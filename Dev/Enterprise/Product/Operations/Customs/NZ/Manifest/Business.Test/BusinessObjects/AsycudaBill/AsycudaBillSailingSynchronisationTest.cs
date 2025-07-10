using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class AsycudaBillSailingSynchronisationTest : TestCaseWithFactory
	{
		public void TestSynchronise_CustomsEntryNumber()
		{
			header.Bills.RemoveAndDeleteAll();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "AAA";
			bill.ABL_OA_Shipper = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			bill.ABL_OA_Consignee = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			bill.ABL_OA_NotifyParty = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			bill.ABL_PrepaidCollect = "PPD";
			bill.ABL_FreightValue = 998m;
			bill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.NewZealand;
			bill.ABL_MarksAndNumbers = "TEST MARK ON ASYCUDA BILL";
			bill.ABL_GoodsDescription = "TEST GOODS DESC ON ASYCUDA BILL";
			bill.ABL_GrossWeight = 1m;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			bill.ABL_Volume = 2m;
			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicMetres;
			bill.ABL_ManifestQty = 13;
			bill.ABL_ManifestUQ = "PKG";
			bill.CustomsEntryNumber = "TRY6789";
			header.AMA_ManifestType = NZManifestTypes.Codes.ICR;
			var synchronisationTarget = (ISailingSynchronisationTarget<BillOfLading>)bill;
			synchronisationTarget.Synchronise();
			AssertEquals("CustomsEntryNumber remains unchanged for ICR", "TRY6789", bill.CustomsEntryNumber);
			AssertSynchroniseForSourceCustomsNumberEntryTypes_OCR(bill);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "AALSMEERGRACHT";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "123SD";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2018, 1, 1);
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_E_ARV = new ZDateTime(2018, 9, 1);
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			sailingBill = Factory.New<BillOfLading>();
			sailingBill.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			sailingBill.JS_NKLoadPort = "AUSYD";
			sailingBill.JS_JX = sailing.PK;
			sailingBill.JS_HouseBill = "AAA";
			sailingBill.JS_RL_NKOrigin = "AUSYD";
			sailingBill.JS_RL_NKDestination = "SGSIN";
			sailingBill.JS_INCO = "CLT";
			sailingBill.JS_GoodsValue = 2018m;
			sailingBill.JS_RX_NKGoodsValueCurr = Core.Constants.CurrencyCodes.Singapore;
			sailingBill.JS_MarksAndNumbers = "TEST MARKS ON SailingBill";
			sailingBill.JS_GoodsDescription = "TEST GOODS DESC ON SailingBill";
			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.NewZealand;
			header.MasterBill.ABL_BillNumber = "MASTER1";
			header.ChangeSailing(sailing.PK);
		}

		AsycudaManifestHeader header;
		BillOfLading sailingBill;

		void AssertSynchroniseForSourceCustomsNumberEntryTypes_OCR(AsycudaBill bill)
		{
			header.AMA_ManifestType = NZManifestTypes.Codes.OCR;
			var entryTypes = new Common.NZ.CusEntryNumberTypeList();
			foreach (string type in entryTypes.GetAllCodes())
			{
				sailingBill.CustomsEntryNumberType = type;
				sailingBill.CustomsEntryNumber = "TRY1234";
				bill.CustomsEntryNumber = "TRY6789";
				var synchronisationTarget = (ISailingSynchronisationTarget<BillOfLading>)bill;
				synchronisationTarget.Synchronise();
				if (type == Common.NZ.CusEntryNumberTypeList.Codes.FormalEntry || type == Common.NZ.CusEntryNumberTypeList.Codes.OutwardReportNumber)
				{
					AssertEquals("CustomsEntryNumber is populated from Bill Of Lading", "TRY1234", bill.CustomsEntryNumber);
				}
				else
				{
					AssertEquals("CustomsEntryNumber remains unchanged", "TRY6789", bill.CustomsEntryNumber);
				}
			}
		}
	}
}
