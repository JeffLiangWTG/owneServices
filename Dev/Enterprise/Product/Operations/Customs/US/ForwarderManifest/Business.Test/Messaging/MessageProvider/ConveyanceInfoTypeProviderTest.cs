using System.Linq;
using CargoWise.Customs.US.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public class ConveyanceInfoTypeProviderTest : TestCaseWithFactory
	{
		public void TestCarrierCode()
		{
			var org = GlbCompany.CurrentCompany.OrgProxy;
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("CCC", "123", "US");
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("CCP", "456", "US");

			manifestHeader.AMA_CarrierCode = "MAX";
			AssertEquals("MAX", provider.CarrierCode.Value);

			manifestHeader.AMA_TransportMode = "SEA";
			AssertEquals("123", provider.CarrierCode.Value);
		}

		public void TestBOLInfoList()
		{
			var manifestHeader1 = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var bill1 = manifestHeader1.Bills.AddNew();
			bill1.ABL_BolType = "BOL";
			var provider1 = new ConveyanceInfoTypeProvider(bill1, "A");
			AssertEquals(1, provider1.BOLInfoList.Count);
			AssertEquals(true, provider1.BOLInfoList.All(x => x.BOLClassificationCode.Value == BillOfLadingClassificationTypeList.Codes.MasterBillOfLading));

			bill1.ABL_BolType = "STD";
			AssertEquals(1, provider1.BOLInfoList.Count);
			AssertEquals(true, provider1.BOLInfoList.All(x => x.BOLClassificationCode.Value == BillOfLadingClassificationTypeList.Codes.HouseBillOfLading));

			manifestHeader1.AMA_ApplicationCode = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			bill1.ABL_BolType = "BOL";
			AssertEquals(1, provider1.BOLInfoList.Count);
			AssertEquals(true, provider1.BOLInfoList.All(x => x.BOLClassificationCode.Value == BillOfLadingClassificationTypeList.Codes.MasterBillOfLading));
		}

		public void TestConveyanceId()
		{
			manifestHeader.AMA_LloydsNumber = "123";

			AssertEquals("123", provider.ConveyanceId.Value);
		}

		public void TestConveyanceName()
		{
			manifestHeader.AMA_VesselName = "Titanic";

			AssertEquals("Titanic", provider.ConveyanceName.Value);
		}

		public void TestFlightTripVoyageNumber()
		{
			manifestHeader.AMA_Voyage = "42";

			AssertEquals("42", provider.FlightTripVoyageNumber.Value);
		}

		public void TestConveyanceCountryCode()
		{
			manifestHeader.AMA_RN_NKConveyanceNationality = "US";

			AssertEquals("US", provider.ConveyanceCountryCode.Value);
		}

		public void TestModeOfTransportationCode()
		{
			manifestHeader.AMA_TransportMode = "AIR";
			manifestHeader.AMA_ContainerMode = "CNT";

			AssertEquals("41", provider.ModeOfTransportationCode.Value);

			manifestHeader.AMA_ContainerMode = "BLK";

			AssertEquals("40", provider.ModeOfTransportationCode.Value);

			manifestHeader.AMA_TransportMode = "SEA";
			manifestHeader.AMA_ContainerMode = "CNT";

			AssertEquals("11", provider.ModeOfTransportationCode.Value);

			manifestHeader.AMA_ContainerMode = "BLK";

			AssertEquals("10", provider.ModeOfTransportationCode.Value);
		}

		public void TestScheduledDepartureDate()
		{
			masterBill.ABL_E_DEP = new CargoWise.Types.ZDateTime(2022, 12, 6);

			AssertEquals("20221206", provider.ScheduledDepartureDate.Value);
		}

		public void TestDeparturePortCode()
		{
			masterBill.ABL_CustomsLoadPort = "1101";

			AssertEquals("1101", provider.DeparturePortCode.Value);
		}

		protected override void SetUp()
		{
			base.SetUp();

			manifestHeader = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			bill = manifestHeader.Bills.AddNew();
			if (manifestHeader.MasterBill != null)
			{
				masterBill = manifestHeader.MasterBill;
			}
			else
			{
				masterBill = manifestHeader.Bills.AddNew();
				masterBill.ABL_BolType = "BOL";
			}
			provider = new ConveyanceInfoTypeProvider(bill, "A");
		}
		IConveyanceInfoType provider;
		USExportAsycudaManifestHeader manifestHeader;
		USExportAsycudaBill bill;
		USExportAsycudaBill masterBill;
	}
}
