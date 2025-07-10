using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	class ETradeComplementaryDecMessageProviderTest : TestCaseWithFactory
	{
		public void TestIETradeComplementaryDec()
		{
			var header = CreateHeader();
			IETradeComplementaryDec provider = new ETradeComplementaryDecMessageProvider(header);

			CombineAssertions(() =>
			{
				AssertExceptionThrown<System.ArgumentNullException>("Null Arg Exception expected", () => new ETradeComplementaryDecMessageProvider(null));

				AssertEquals("IMessageSender.Parent", header, provider.Parent);
				AssertEquals("IMessageSender.Messages", header.Messages, provider.Messages);
				AssertEquals("IMessageSender.JobReference", "ETR0000001", provider.JobReference);

				AssertEquals("DeclarationOwnerRepresentativeNameAndTitle", "Test Company Name", provider.DeclarationOwnerRepresentativeNameAndTitle);
				AssertEquals("DeclarationOwnerRepresentativeTaxNo", "BUS1234567", provider.DeclarationOwnerRepresentativeTaxNo);
				AssertEquals("RegistrationNo", "testRegNoforTest", provider.RegistrationNo);
				AssertEquals("Bills - Count", 2, provider.Bills.Count());
			});
		}

		public void TestConsigneeTaxIDNo()
		{
			var billMock = new Mock<AsycudaBill>(Factory, ((INeedRow)Factory.New<AsycudaBill>()).Row);
			billMock.Setup(m => m.SupplementaryDeclarationRegNoIdNo).Returns("20201224105");
			var provider = new BillComplementaryProvider(billMock.Object) as IBillComplementary;
			AssertEquals("ConsigneeTaxIDNo should return Bill.SupplementaryDeclarationRegNoIdNo", "20201224105", provider.ConsigneeTaxIDNo);
		}

		public void TestIBillComplementary()
		{
			var bill = CreateBill();
			IBillComplementary provider = new BillComplementaryProvider(bill);

			CombineAssertions(() =>
			{
				AssertExceptionThrown<System.ArgumentNullException>("Null Arg Exception expected", () => new BillComplementaryProvider(null));

				AssertEquals("BillNo", "B001", provider.BillNo);
				AssertEquals("DeliveryDate", new ZDateTime(2023, 3, 17), provider.DeliveryDate);
			});
		}

		AsycudaManifestHeader CreateHeader()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ETR0000001";
			header.RegistrationNumber = "testRegNoforTest";

			header.Bills.Add(CreateBill());
			header.Bills.Add(CreateBill());

			return header;
		}

		AsycudaBill CreateBill()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.ABL_BillNumber = "B001";
			bill.ABL_ConsigneeRegNo = "ConTax01";
			bill.SupplementaryDeclarationDeliveryDate = new ZDateTime(2023, 3, 17);

			return bill;
		}

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.GC_Name = "Test Company Name";
			GlbCompany.CurrentCompany.GC_BusinessRegNo = "BUS1234567";
		}
	}
}
