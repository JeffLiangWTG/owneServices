using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class DocumentShipmentTest : BaseFreightTest
	{
		#region Freight Labels

		public void TestIncludeConsignee()
		{
			DocumentShipment.IncludeConsignee = true;
			AssertEquals(true, DocumentShipment.IncludeConsignee);

			DocumentShipment.IncludeConsignee = false;
			AssertEquals(false, DocumentShipment.IncludeConsignee);
		}

		public void TestIncludeConsignor()
		{
			DocumentShipment.IncludeConsignor = true;
			AssertEquals(true, DocumentShipment.IncludeConsignor);

			DocumentShipment.IncludeConsignor = false;
			AssertEquals(false, DocumentShipment.IncludeConsignor);
		}

		public void TestIncludeNone()
		{
			DocumentShipment.IncludeNone = true;
			AssertEquals(true, DocumentShipment.IncludeNone);

			DocumentShipment.IncludeNone = false;
			AssertEquals(false, DocumentShipment.IncludeNone);
		}

		public void TestIncludeSendingAgent()
		{
			DocumentShipment.IncludeSendingAgent = true;
			AssertEquals(true, DocumentShipment.IncludeSendingAgent);

			DocumentShipment.IncludeSendingAgent = false;
			AssertEquals(false, DocumentShipment.IncludeSendingAgent);
		}

		public void TestNumberOfLabelsToPrint()
		{
			const string ErrorLess = "It is not possible to print Zero or less labels";
			const string ErrorMore = "You cannot print more labels than Outer Packs";

			DocumentShipment.NumberOfLabelsToPrint = 0;
			AssertEquals(0, DocumentShipment.NumberOfLabelsToPrint);
			Assert("Has errors", DocumentShipment.NumberOfLabelsToPrintInfo.HasErrors());
			Assert("Has '0 or less' error", DocumentShipment.NumberOfLabelsToPrintInfo.HasError(ErrorLess));

			DocumentShipment.NumberOfLabelsToPrint = 3;
			AssertEquals(3, DocumentShipment.NumberOfLabelsToPrint);
			AssertEquals("Has no errors", false, DocumentShipment.NumberOfLabelsToPrintInfo.HasErrors());

			DocumentShipment.NumberOfLabelsToPrint = 7;
			AssertEquals(7, DocumentShipment.NumberOfLabelsToPrint);
			AssertEquals("Has no errors", false, DocumentShipment.NumberOfLabelsToPrintInfo.HasErrors());

			DocumentShipment.NumberOfLabelsToPrint = 8;
			AssertEquals(8, DocumentShipment.NumberOfLabelsToPrint);
			Assert("Has errors", DocumentShipment.NumberOfLabelsToPrintInfo.HasErrors());
			Assert("Has 'more' error", DocumentShipment.NumberOfLabelsToPrintInfo.HasError(ErrorMore));
		}

		public void TestTotalNumberOfLabels()
		{
			AssertEquals("of 7", DocumentShipment.TotalNumberOfLabelsDescription);
		}

		public void TestLabelRangeFrom()
		{
			DocumentShipment.LabelRangeTo = 3;
			DocumentShipment.LabelRangeFrom = 0;
			AssertEquals("Start range less than 1 should be invalid", true, DocumentShipment.LabelRangeFromInfo.HasErrors());

			DocumentShipment.LabelRangeFrom = 8;
			AssertEquals("Start range more than the total number of labels should be invalid", true, DocumentShipment.LabelRangeFromInfo.HasErrors());

			DocumentShipment.LabelRangeTo = 3;
			DocumentShipment.LabelRangeFrom = 4;
			AssertEquals("Start range that is more than the end range should be invalid", true, DocumentShipment.LabelRangeToInfo.HasErrors());

			DocumentShipment.LabelRangeFrom = 3;
			AssertEquals("Start range should be valid", false, DocumentShipment.LabelRangeFromInfo.HasErrors());
		}

		public void TestLabelRangeTo()
		{
			DocumentShipment.LabelRangeFrom = 1;
			DocumentShipment.LabelRangeTo = 0;
			AssertEquals("End range less than 1 should be invalid", true, DocumentShipment.LabelRangeToInfo.HasErrors());

			DocumentShipment.LabelRangeTo = 8;
			AssertEquals("End range more than the total number of labels should be invalid", true, DocumentShipment.LabelRangeToInfo.HasErrors());

			DocumentShipment.LabelRangeTo = 3;
			DocumentShipment.LabelRangeFrom = 4;
			AssertEquals("End range that is less than the start range should be invalid", true, DocumentShipment.LabelRangeToInfo.HasErrors());

			DocumentShipment.LabelRangeTo = 4;
			AssertEquals("End range should be valid", false, DocumentShipment.LabelRangeToInfo.HasErrors());
		}

		public void TestSetDefaultsFromDataContext()
		{
			DocumentShipment = new DocumentShipment(Shipment, Constants.DataContext.FreightLabels);
			DocumentShipment.SetDefaultsFromDataContext();
			AssertEquals("Defaults: IncludeConsignee should be ", ZBool.True, DocumentShipment.IncludeConsignee);
			AssertEquals("Defaults: NumberOfLabelsToPrint should be ", 7, DocumentShipment.NumberOfLabelsToPrint);

			DocumentShipment = new DocumentShipment(Shipment, Constants.DataContext.GenericFreightJobByPackages);
			DocumentShipment.SetDefaultsFromDataContext();
			AssertEquals("Defaults: IncludeConsignee should be ", ZBool.True, DocumentShipment.IncludeConsignee);
			AssertEquals("Defaults: NumberOfLabelsToPrint should be ", 7, DocumentShipment.NumberOfLabelsToPrint);

			DocumentShipment = new DocumentShipment(Shipment, Constants.DataContext.GenericFreightJobByPackages1Doc);
			DocumentShipment.SetDefaultsFromDataContext();
			AssertEquals("Defaults: IncludeConsignee should be ", ZBool.True, DocumentShipment.IncludeConsignee);
			AssertEquals("Defaults: NumberOfLabelsToPrint should be ", 7, DocumentShipment.NumberOfLabelsToPrint);

			DocumentShipment = new DocumentShipment(Shipment, Constants.DataContext.GenericFreightJobBySelectedPackages);
			DocumentShipment.SetDefaultsFromDataContext();
			AssertEquals("Defaults: LabelRangeTo should be ", 7, DocumentShipment.LabelRangeTo);

			DocumentShipment = new DocumentShipment(Shipment, Constants.DataContext.GenericFreightJobBySelectedPkgs1Doc);
			DocumentShipment.SetDefaultsFromDataContext();
			AssertEquals("Defaults: LabelRangeTo should be ", 7, DocumentShipment.LabelRangeTo);
		}

		#endregion

		#region New Details - Letter of Indemnity

		public void TestOldMarksAndNumbers()
		{
			DocumentShipment.OldMarksAndNumbers = "OldMarksAndNumbers";
			AssertEquals("OldMarksAndNumbers", DocumentShipment.OldMarksAndNumbers);
		}

		public void TestOldGoodsDescription()
		{
			DocumentShipment.OldGoodsDescription = "OldGoodsDescription";
			AssertEquals("OldGoodsDescription", DocumentShipment.OldGoodsDescription);
		}

		public void TestOldWeight()
		{
			DocumentShipment.OldWeightUnit = Constants.Weight.Kilograms;
			DocumentShipment.OldWeight = ZDecimal.ParseSafe("14.026", 0);
			AssertEquals(ZDecimal.ParseSafe("14.026", 0), DocumentShipment.OldWeight);
		}

		public void TestOldWeightUnit()
		{
			DocumentShipment.OldWeightUnit = "KG";
			AssertEquals("KG", DocumentShipment.OldWeightUnit);
		}

		public void TestOldVolume()
		{
			DocumentShipment.OldVolumeUnit = Constants.Volume.CubicMetres;
			DocumentShipment.OldVolume = ZDecimal.ParseSafe("14.026", 0);
			AssertEquals(ZDecimal.ParseSafe("14.026", 0), DocumentShipment.OldVolume);
		}

		public void TestOldVolumeUnit()
		{
			DocumentShipment.OldVolumeUnit = "M3";
			AssertEquals("M3", DocumentShipment.OldVolumeUnit);
		}

		public void TestNewMarksAndNumbers()
		{
			DocumentShipment.NewMarksAndNumbers = "NewMarksAndNumbers";
			AssertEquals("NewMarksAndNumbers", DocumentShipment.NewMarksAndNumbers);
		}

		public void TestNewGoodsDescription()
		{
			DocumentShipment.NewGoodsDescription = "NewGoodsDescription";
			AssertEquals("NewGoodsDescription", DocumentShipment.NewGoodsDescription);
		}

		public void TestNewWeight()
		{
			DocumentShipment.NewWeightUnit = Constants.Weight.Kilograms;
			DocumentShipment.NewWeight = ZDecimal.ParseSafe("14.026", 0);
			AssertEquals(ZDecimal.ParseSafe("14.026", 0), DocumentShipment.NewWeight);
			AssertEquals(false, DocumentShipment.NewWeightInfo.HasErrors());

			DocumentShipment.NewWeight = ZDecimal.ParseSafe("1234567890.0261", 0);
			AssertEquals(true, DocumentShipment.NewWeightInfo.HasErrors());
		}

		public void TestNewWeightUnit()
		{
			DocumentShipment.NewWeightUnit = "KG";
			AssertEquals("KG", DocumentShipment.NewWeightUnit);
			AssertEquals(false, DocumentShipment.NewWeightUnitInfo.HasErrors());

			DocumentShipment.NewWeightUnit = "JJ";
			AssertEquals(true, DocumentShipment.NewWeightUnitInfo.HasErrors());
		}

		public void TestNewVolume()
		{
			DocumentShipment.NewVolumeUnit = Constants.Volume.CubicMetres;
			DocumentShipment.NewVolume = ZDecimal.ParseSafe("14.026", 0);
			AssertEquals(ZDecimal.ParseSafe("14.026", 0), DocumentShipment.NewVolume);
			AssertEquals(false, DocumentShipment.NewVolumeInfo.HasErrors());

			DocumentShipment.NewVolume = ZDecimal.ParseSafe("1234567890.0261", 0);
			AssertEquals(true, DocumentShipment.NewVolumeInfo.HasErrors());
		}

		public void TestNewVolumeUnit()
		{
			DocumentShipment.NewVolumeUnit = "M3";
			AssertEquals("M3", DocumentShipment.NewVolumeUnit);

			DocumentShipment.NewVolumeUnit = "JJ";
			AssertEquals(true, DocumentShipment.NewVolumeUnitInfo.HasErrors());
		}

		public void TestChangeMarksAndNumbers()
		{
			DocumentShipment.ChangeMarksAndNumbers = true;
			AssertEquals(true, DocumentShipment.ChangeMarksAndNumbers);

			DocumentShipment.ChangeMarksAndNumbers = false;
			AssertEquals(false, DocumentShipment.ChangeMarksAndNumbers);
		}

		public void TestChangeGoodsDescription()
		{
			DocumentShipment.ChangeGoodsDescription = true;
			AssertEquals(true, DocumentShipment.ChangeGoodsDescription);

			DocumentShipment.ChangeGoodsDescription = false;
			AssertEquals(false, DocumentShipment.ChangeGoodsDescription);
		}

		public void TestChangeWeight()
		{
			DocumentShipment.ChangeWeight = true;
			AssertEquals(true, DocumentShipment.ChangeWeight);

			DocumentShipment.ChangeWeight = false;
			AssertEquals(false, DocumentShipment.ChangeWeight);
		}

		public void TestChangeWeightDisableClearsErrors()
		{
			DocumentShipment.OldWeight = ZDecimal.ParseSafe("15.987", 0);
			DocumentShipment.OldWeightUnit = "KG";

			DocumentShipment.ChangeWeight = true;
			DocumentShipment.NewWeight = ZDecimal.ParseSafe("8977899087.9879", 0);
			DocumentShipment.NewWeightUnit = "JJ";
			AssertEquals("New weight now has errors", true, DocumentShipment.NewWeightInfo.HasErrors());
			AssertEquals("New weight unit now has errors", true, DocumentShipment.NewWeightUnitInfo.HasErrors());

			DocumentShipment.ChangeWeight = false;
			AssertEquals("New weight errors cleared", false, DocumentShipment.NewWeightInfo.HasErrors());
			AssertEquals("New weight unit errors cleared", false, DocumentShipment.NewWeightUnitInfo.HasErrors());
			AssertEquals("New weight should be old weight", DocumentShipment.OldWeight, DocumentShipment.NewWeight);
			AssertEquals("New weight unit should be old weight unit", DocumentShipment.OldWeightUnit, DocumentShipment.NewWeightUnit);
		}

		public void TestChangeVolume()
		{
			DocumentShipment.ChangeVolume = true;
			AssertEquals(true, DocumentShipment.ChangeVolume);

			DocumentShipment.ChangeVolume = false;
			AssertEquals(false, DocumentShipment.ChangeVolume);
		}

		public void TestChangeVolumeDisableClearsErrors()
		{
			DocumentShipment.OldVolume = ZDecimal.ParseSafe("15.987", 0);
			DocumentShipment.OldVolumeUnit = "M3";

			DocumentShipment.ChangeVolume = true;
			DocumentShipment.NewVolume = ZDecimal.ParseSafe("8799878990.9879", 0);
			DocumentShipment.NewVolumeUnit = "JJ";
			AssertEquals("New Volume now has errors", true, DocumentShipment.NewVolumeInfo.HasErrors());
			AssertEquals("New Volume unit now has errors", true, DocumentShipment.NewVolumeUnitInfo.HasErrors());

			DocumentShipment.ChangeVolume = false;
			AssertEquals("New Volume errors cleared", false, DocumentShipment.NewVolumeInfo.HasErrors());
			AssertEquals("New Volume unit errors cleared", false, DocumentShipment.NewVolumeUnitInfo.HasErrors());
			AssertEquals("New Volume should be old Volume", DocumentShipment.OldVolume, DocumentShipment.NewVolume);
			AssertEquals("New Volume unit should be old Volume unit", DocumentShipment.OldVolumeUnit, DocumentShipment.NewVolumeUnit);
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestSettingLongGoodsDescriptionDoesNotThrowException()
		{
			var shipment = Factory.New<CommonShipment>();
			var docShipment = new DocumentShipment(shipment, Constants.DataContext.LetterOfIndemnity);

			shipment.DetailedGoodsDescriptionNoteText = new ZString('x', docShipment.OldGoodsDescriptionInfo.MaxLength + 1);
			docShipment.SetDefaultsFromDataContext();
		}

		#endregion

		#region Charge Sheet

		public void TestDebtor()
		{
			AssertNull(DocumentShipment.Debtor);

			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());
			DebtorToSelectFromForPrinting debtor = new DebtorToSelectFromForPrinting(orgHeader);
			DocumentShipment.Debtor = debtor;
			AssertEquals(debtor.PK, DocumentShipment.Debtor.PK);
		}

		public void TestDebtorsToPrint()
		{
			AssertEquals("Should contain no Debtors", 0, DocumentShipment.DebtorsToPrint.Count);

			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			JobHeader header = Factory.NewJobForTesting<JobHeader>();
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			header.JH_OA_LocalChargesAddr = Factory.LoadTop1<OrgHeader>(new ZQuery()).MainAddress.PK;

			var code = Factory.NewWithValidTestData<AccChargeCode>();

			JobCharge lineCharge1 = CreateLineCharge(header, header.LocalChargesPK, 10.000M, code.PK);

			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, header.LocalChargesPK));
			JobCharge lineCharge2 = CreateLineCharge(header, orgHeader.PK, 20.000M, code.PK);
			lineCharge2.JR_OH_SellAccount = orgHeader.PK;

			DocumentShipment docShipment = new DocumentShipment(shipment, Constants.DataContext.ChargeSheet);
			docShipment.SetDefaultsFromDataContext();

			AssertEquals("Should be of type DebtorToSelectFromForPrintingCollection", typeof(DebtorToSelectFromForPrintingCollection), docShipment.DebtorsToPrint.GetType());
			AssertEquals("Should contain 2 Debtors", 2, docShipment.DebtorsToPrint.Count);
		}

		public void TestSetDefaultsFromDataContextForChargeSheet()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			DocumentShipment documentShipment = new DocumentShipment(shipment, Constants.DataContext.ChargeSheet);
			documentShipment.SetDefaultsFromDataContext();
			AssertEquals("Should be empty", 0, documentShipment.DebtorsToPrint.Count);

			JobHeader header = Factory.NewJobForTesting<JobHeader>();
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			header.JH_OA_LocalChargesAddr = Factory.LoadTop1<OrgHeader>(new ZQuery()).MainAddress.PK;

			var code = Factory.NewWithValidTestData<AccChargeCode>();

			JobCharge lineCharge1 = CreateLineCharge(header, header.LocalChargesPK, 10.000M, code.PK);
			JobCharge lineCharge2 = CreateLineCharge(header, header.LocalChargesPK, 30.000M, code.PK);

			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, header.LocalChargesPK));
			JobCharge lineCharge3 = CreateLineCharge(header, orgHeader.PK, 20.000M, code.PK);
			lineCharge3.JR_OH_SellAccount = orgHeader.PK;

			documentShipment = new DocumentShipment(shipment, Constants.DataContext.ChargeSheet);
			documentShipment.SetDefaultsFromDataContext();
			AssertNotNull(documentShipment.DebtorsToPrint);
			AssertEquals("Should contain 2 Debtors", 2, documentShipment.DebtorsToPrint.Count);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			CurrentCompanyCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			CurrentBranchPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("AU");
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			Consol = Factory.New<CommonConsol>();
			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.JK_MasterBillNum = "08112345678";

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";

			Shipment = Consol.Shipments.AddNew();
			PackLine line1 = Shipment.OuterPackLines.AddNew();
			PackLine line2 = Shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 4;
			line2.JL_PackageCount = 3;
			Shipment.JS_OuterPacks = 7;
			DocumentShipment = new DocumentShipment(Shipment, Constants.DataContext.FreightLabels);
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = CurrentCompanyCountryCode;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = CurrentBranchPort;
			base.TearDown();
		}

		JobCharge CreateLineCharge(JobHeader jobHeaderBisObj, ZGuid localChargesPK, ZDecimal amount, ZGuid chargeCodePK)
		{
			JobCharge lineCharge = Factory.New<JobCharge>();
			lineCharge.JR_JH = jobHeaderBisObj.PK;
			lineCharge.JR_GE = jobHeaderBisObj.JH_GE;
			lineCharge.JR_GB = jobHeaderBisObj.JH_GB;
			lineCharge.JR_AC = chargeCodePK;
			lineCharge.JR_LocalSellAmt = amount;
			lineCharge.JR_OH_SellAccount = localChargesPK;
			return lineCharge;
		}

		CommonConsol Consol;
		CommonShipment Shipment;
		DocumentShipment DocumentShipment;
		ZString CurrentCompanyCountryCode;
		ZString CurrentBranchPort;

		#endregion
	}
}
