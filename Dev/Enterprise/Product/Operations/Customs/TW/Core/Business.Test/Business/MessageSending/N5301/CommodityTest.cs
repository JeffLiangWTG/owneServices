using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.N5301;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CommodityTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAdditionalDocuments()
		{
			NUnit.Framework.Assert.That(commodity.AdditionalDocuments, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IAdditionalDocument>)));
		}

		[ExpectNoExceptions]
		public void TestCommercialCategorizationID()
		{
			NUnit.Framework.Assert.That(commodity.CommercialCategorizationID.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestDescription()
		{
			NUnit.Framework.Assert.That(commodity.Description.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestGoodsGroupNameCode()
		{
			NUnit.Framework.Assert.That(commodity.GoodsGroupNameCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			NUnit.Framework.Assert.That(commodity.Name.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestBarCode()
		{
			NUnit.Framework.Assert.That(commodity.BarCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestChineseDescription()
		{
			NUnit.Framework.Assert.That(commodity.ChineseDescription.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestEnglishDescription()
		{
			NUnit.Framework.Assert.That(commodity.EnglishDescription.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestCITESImportPermitID()
		{
			NUnit.Framework.Assert.That(commodity.CITESImportPermitID.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestFTATariffCode()
		{
			NUnit.Framework.Assert.That(commodity.FTATariffCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestSHTCImportPermitID()
		{
			NUnit.Framework.Assert.That(commodity.SHTCImportPermitID.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestTariffCodeExtensionCode()
		{
			NUnit.Framework.Assert.That(commodity.TariffCodeExtensionCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestClassifications()
		{
			NUnit.Framework.Assert.That(commodity.Classifications, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IClassification>)));
		}

		[ExpectNoExceptions]
		public void TestCommodityRelatedPackaging()
		{
			NUnit.Framework.Assert.That(commodity.CommodityRelatedPackaging, NUnit.Framework.Is.EqualTo(default(ICommodityRelatedPackaging)));
		}

		[ExpectNoExceptions]
		public void TestConstituent()
		{
			NUnit.Framework.Assert.That(commodity.Constituent, NUnit.Framework.Is.EqualTo(default(IConstituent)));
		}

		[ExpectNoExceptions]
		public void TestDutyTaxFee()
		{
			NUnit.Framework.Assert.That(commodity.DutyTaxFee, NUnit.Framework.Is.EqualTo(default(ICommodityDutyTaxFee)));
		}

		[ExpectNoExceptions]
		public void TestGovernmentProcedure()
		{
			NUnit.Framework.Assert.That(commodity.GovernmentProcedure, NUnit.Framework.Is.EqualTo(default(IGovernmentProcedure)));
		}

		[ExpectNoExceptions]
		public void TestHandlingInstructionsCodes()
		{
			NUnit.Framework.Assert.That(commodity.HandlingInstructionsCodes, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<CargoWise.Types.ZString>)));
		}

		[ExpectNoExceptions]
		public void TestInvoiceLine()
		{
			NUnit.Framework.Assert.That(commodity.InvoiceLine, NUnit.Framework.Is.EqualTo(default(IInvoiceLine)));
		}

		[ExpectNoExceptions]
		public void TestPreviousDocument()
		{
			NUnit.Framework.Assert.That(commodity.PreviousDocument, NUnit.Framework.Is.EqualTo(default(IPreviousDocument)));
		}

		[ExpectNoExceptions]
		public void TestCommodityNumbers()
		{
			NUnit.Framework.Assert.That(commodity.CommodityNumbers, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<ICommodityNumber>)));
		}

		[ExpectNoExceptions]
		public void TestDutyOtherTaxFees()
		{
			NUnit.Framework.Assert.That(commodity.DutyOtherTaxFees, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IDutyOtherTaxFee>)));
		}

		[ExpectNoExceptions]
		public void TestDutyTaxFeeAmount()
		{
			NUnit.Framework.Assert.That(commodity.DutyTaxFeeAmount, NUnit.Framework.Is.EqualTo(default(IDutyTaxFeeAmount)));
		}

		[ExpectNoExceptions]
		public void TestDutyTaxFeeQuantity()
		{
			NUnit.Framework.Assert.That(commodity.DutyTaxFeeQuantity, NUnit.Framework.Is.EqualTo(default(IDutyTaxFeeQuantity)));
		}

		[ExpectNoExceptions]
		public void TestFood()
		{
			NUnit.Framework.Assert.That(commodity.Food, NUnit.Framework.Is.EqualTo(default(IFood)));
		}

		[ExpectNoExceptions]
		public void TestQuarantine()
		{
			NUnit.Framework.Assert.That(commodity.Quarantine, NUnit.Framework.Is.EqualTo(default(IQuarantine)));
		}

		[ExpectNoExceptions]
		public void TestVehicle()
		{
			NUnit.Framework.Assert.That(commodity.Vehicle, NUnit.Framework.Is.EqualTo(default(IVehicle)));
		}

		[ExpectNoExceptions]
		public void TestWine()
		{
			NUnit.Framework.Assert.That(commodity.Wine, NUnit.Framework.Is.EqualTo(default(IWine)));
		}

		[ExpectNoExceptions]
		public void TestCargoDescription()
		{
			NUnit.Framework.Assert.That(commodity.CargoDescription, NUnit.Framework.Is.EqualTo("XX123").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBondedNoteCode()
		{
			NUnit.Framework.Assert.That(commodity.BondedNoteCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestVehicleIDs()
		{
			NUnit.Framework.Assert.That(commodity.VehicleIDs, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<CargoWise.Types.ZString>)));
		}

		[ExpectNoExceptions]
		public void TestClassification()
		{
			NUnit.Framework.Assert.That(commodity.Classification, NUnit.Framework.Is.EqualTo(default(IClassification)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			var arrivalBill = header.ArrivalBill;
			var movementBill = header.MovementBill;
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.InBondMoveDetail;
			var moveLine = moveDetail.InBondMoveLineItem;
			moveLine.BI_Description = "XX123";
			commodity = new Commodity(moveLine);
		}

		ICommodity commodity;
	}
}
