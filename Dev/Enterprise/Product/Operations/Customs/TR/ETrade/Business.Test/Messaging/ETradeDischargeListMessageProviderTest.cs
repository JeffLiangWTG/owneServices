using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	public class ETradeDischargeListMessageProviderTest : TestCaseWithFactory
	{
		public void TestIDischargeList()
		{
			var header = CreateHeader();
			IDischargeList provider = new ETradeDischargeListMessageProvider(header);

			CombineAssertions(() =>
			{
				AssertExceptionThrown<System.ArgumentNullException>("Null Arg Exception expected", () => new ETradeDischargeListMessageProvider(null));

				AssertEquals("IMessageSender.Parent", header, provider.Parent);
				AssertEquals("IMessageSender.Messages", header.Messages, provider.Messages);
				AssertEquals("IMessageSender.JobReference", "ETR0000001", provider.JobReference);

				AssertEquals("DeclarationOwnerRepresentativeNameAndTitle", "Test Company Name", provider.DeclarationOwnerRepresentativeNameAndTitle);
				AssertEquals("DeclarationOwnerRepresentativeTaxNo", "BUS1234567", provider.DeclarationOwnerRepresentativeTaxNo);
				AssertEquals("CustomsOffice", "12345", provider.CustomsOffice);
				AssertEquals("GoodsLocationName", "LocInfo", provider.GoodsLocationName);
				AssertEquals("GoodsLocationCode", "LocCode", provider.GoodsLocationCode);
				AssertEquals("RegistrationNo", "testRegNoforTest", provider.RegistrationNo);
				AssertEquals("Pre-condition: Ensure that the header contains at least one bill with the separated field set to true.", 1, header.Bills.Where(x => x.Separated).Count());
				AssertEquals("There must be two bills with the Separation field false.", 2, provider.Bills.Count());
			});
		}

		public void TestIBillBL()
		{
			var header = CreateHeader();
			var bill = header.Bills.Cast<AsycudaBill>().First(x => x.ABL_SequenceNumber == 1);
			IBillBL provider = new BillBLProvider(bill);

			CombineAssertions(() =>
			{
				AssertExceptionThrown<System.ArgumentNullException>("Null Arg Exception expected", () => new BillBLProvider(null));

				AssertEquals("BillNo", "B001", provider.BillNo);
				AssertEquals("LineNo", "1", provider.LineNo);
				AssertEquals("ShipperName", "Shippy", provider.ShipperName);
				AssertEquals("ConsigneeNameAndTitle", "Mr Consignee", provider.ConsigneeNameAndTitle);
				AssertEquals("ConsigneeTaxNo", "ConReg1234", provider.ConsigneeTaxNo);
				AssertEquals("IsContainer", true, provider.IsContainer);
				AssertEquals("SequenceNo", "1", provider.SequenceNo);
				AssertEquals("PackType", "BAG", provider.PackType);
				AssertEquals("PackQuantity", 23, provider.PackQuantity);
				AssertEquals("MarksAndNumbers", "MandNs", provider.MarksAndNumbers);
				AssertEquals("Unit", "KGM", provider.Unit);
				AssertEquals("GrossWeight", 123.45m, provider.GrossWeight);
				AssertEquals("NetWeight", 2.3m, provider.NetWeight);
				AssertEquals("Packs - Count", 2, provider.Packs.Count());
			});
		}

		public void TestIPackBL()
		{
			var pack = CreatePack();
			IPackBL provider = new PackBLProvider(pack);

			CombineAssertions(() =>
			{
				AssertExceptionThrown<System.ArgumentNullException>("Null Arg Exception expected", () => new PackBLProvider(null));

				AssertEquals("LineNo", "1", provider.LineNo);
				AssertEquals("GoodDescription", "Good Boy", provider.GoodDescription);
				AssertEquals("Tariff", "12345678", provider.Tariff);
				AssertEquals("Unit", "KGM", provider.Unit);
				AssertEquals("GrossWeight", 123.45m, provider.GrossWeight);
				AssertEquals("NetWeight", 56.56m, provider.NetWeight);
			});
		}

		AsycudaManifestHeader CreateHeader()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ETR0000001";
			header.RegistrationNumber = "testRegNoforTest";
			header.AMA_CustomsOffice = "TR12345";
			header.MasterBill.ABL_LocationInformation = "LocInfo";
			header.MasterBill.ABL_GoodsLocation = "LocCode";
			header.AMA_ContainerMode = "CNT";

			CreateBill(header, 1);
			CreateBill(header, 2);
			CreateBill(header, 3, true);

			return header;
		}

		void CreateBill(AsycudaManifestHeader header, int seq, bool separated = false)
		{
			var bill = header.Bills.AddNew();

			bill.ABL_BillNumber = $"B00{seq}";
			bill.ABL_ConsigneeRegNo = "ConTax01";
			bill.ABL_ShipperName = "Shippy";
			bill.ABL_ConsigneeName = "Mr Consignee";
			bill.ABL_ConsigneeRegNo = "ConReg1234";
			bill.ABL_SequenceNumber = (ZShort)seq;
			bill.ABL_ManifestUQ = "BAG";
			bill.ABL_ManifestQty = 23;
			bill.ABL_MarksAndNumbers = "MandNs";
			bill.ABL_GrossWeight = 123.45m;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			bill.ABL_NetWeight = 2300m;
			bill.ABL_NetWeightUQ = Core.Constants.Weight.Grams;
			bill.Separated = separated;

			bill.Packs.Add(CreatePack());
			bill.Packs.Add(CreatePack());
		}

		AsycudaPack CreatePack()
		{
			var pack = Factory.New<AsycudaPack>();

			pack.APA_LineNo = 1;
			pack.PackedItem.API_GoodsDescription = "Good Boy";
			pack.PackedItem.API_Tariff = "12345678";
			pack.PackedItem.API_CustomsQty2 = 123.45m;
			pack.PackedItem.API_CustomsQty3 = 56.56m;

			return pack;
		}

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.GC_Name = "Test Company Name";
			GlbCompany.CurrentCompany.GC_BusinessRegNo = "BUS1234567";
		}
	}
}
