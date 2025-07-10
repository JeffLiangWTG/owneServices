using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	sealed class USCustomsDailyStatementRunDocsTest : BaseRunDocumentsTest
	{
		ZString storedCountry;

		protected override void SetUp()
		{
			storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.UnitedStates);
			base.SetUp();
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.SetCountry(storedCountry);
			base.TearDown();
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CustomsStatementHdr; }
		}

		public override BusinessObject GetBusinessObject
		{
			get
			{
				Enterprise.Customs.US.Business.CusStatementHeader header = Factory.New<Enterprise.Customs.US.Business.CusStatementHeader>();

				Enterprise.Customs.US.Business.CusStatementLine statementLine = header.StatementLines.AddNew();
				statementLine.B3_CustomsFeesTotal = 154.00m;
				statementLine.B3_EntryNum = "4563210";
				statementLine.B3_BrokerReference = "2013557";
				Enterprise.Customs.US.Business.CusStatementLineCharge lineCharge = statementLine.Charges.AddNew();
				lineCharge.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.SoftwoodLumber;
				lineCharge.B4_ChargeAmount = 4.00m;

				Enterprise.Customs.US.Business.CusStatementLineCharge lineCharge2 = statementLine.Charges.AddNew();
				lineCharge2.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.CountervailingDuty;
				lineCharge2.B4_ChargeAmount = 150.00m;

				Enterprise.Customs.US.Business.CusStatementLine statementLine1 = header.StatementLines.AddNew();
				statementLine1.B3_CustomsFeesTotal = 150.00m;
				statementLine1.B3_EntryNum = "4500210";
				Enterprise.Customs.US.Business.CusStatementLineCharge lineCharge3 = statementLine1.Charges.AddNew();
				lineCharge3.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Mango;
				lineCharge3.B4_ChargeAmount = 10.00m;

				Enterprise.Customs.US.Business.CusStatementLineCharge lineCharge4 = statementLine1.Charges.AddNew();
				lineCharge4.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.CountervailingDuty;
				lineCharge4.B4_ChargeAmount = 140.00m;

				return header;
			}
		}

		[ExpectNoExceptions]
		public void TestDailyDeletedReport()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Deleted Report");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestDailyDetailReport()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Detail Report");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestDailyPreliminaryReport()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Preliminary Report");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestDailyTotalReport()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Total Report");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}
	}
}
