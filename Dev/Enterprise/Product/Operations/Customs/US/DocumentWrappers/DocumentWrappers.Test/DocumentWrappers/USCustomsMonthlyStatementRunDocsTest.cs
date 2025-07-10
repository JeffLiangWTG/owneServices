using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	sealed class USCustomsMonthlyStatementRunDocsTest : BaseRunDocumentsTest
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
				Enterprise.Customs.US.Business.CusStatementHeader monthlyHeader = Factory.New<Enterprise.Customs.US.Business.CusStatementHeader>();
				monthlyHeader.B2_StatementNumber = "1112P2222";
				monthlyHeader.B2_IsMonthlyStatement = true;

				Enterprise.Customs.US.Business.CusStatementHeader childHeader1 = monthlyHeader.DailyStatements.AddNew();
				childHeader1.B2_StatementNumber = "117782222";
				childHeader1.B2_IsMonthlyStatement = false;

				Enterprise.Customs.US.Business.CusStatementLine statementLine = childHeader1.StatementLines.AddNew();
				statementLine.B3_CustomsFeesTotal = 154.00m;
				statementLine.B3_BrokerReference = "2013557";
				statementLine.B3_EntryNum = "4563210";
				statementLine.B3_Status = Enterprise.Customs.US.Business.StatementLineStatusList.Codes.Active;
				Enterprise.Customs.US.Business.CusStatementLineCharge lineCharge1 = statementLine.Charges.AddNew();
				lineCharge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.SoftwoodLumber;
				lineCharge1.B4_ChargeAmount = 4.00m;

				Enterprise.Customs.US.Business.CusStatementLineCharge lineCharge2 = statementLine.Charges.AddNew();
				lineCharge2.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.CountervailingDuty;
				lineCharge2.B4_ChargeAmount = 140.00m;

				Enterprise.Customs.US.Business.CusStatementLineCharge lineCharge4 = statementLine.Charges.AddNew();
				lineCharge4.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Duty;
				lineCharge4.B4_ChargeAmount = 10.00m;

				Enterprise.Customs.US.Business.CusStatementLine statementLine2 = childHeader1.StatementLines.AddNew();
				statementLine2.B3_CustomsFeesTotal = 100.00m;
				statementLine2.B3_EntryNum = "7763210";
				statementLine2.B3_Status = Enterprise.Customs.US.Business.StatementLineStatusList.Codes.Deleted;

				Enterprise.Customs.US.Business.CusStatementLineCharge lineCharge3 = statementLine2.Charges.AddNew();
				lineCharge3.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Watermelon;
				lineCharge3.B4_ChargeAmount = 100.00m;

				Enterprise.Customs.US.Business.CusStatementHeader childHeader2 = monthlyHeader.DailyStatements.AddNew();
				childHeader2.B2_StatementNumber = "117782211";
				childHeader2.B2_IsMonthlyStatement = false;

				Enterprise.Customs.US.Business.CusStatementLine statementLine3 = childHeader2.StatementLines.AddNew();
				statementLine3.B3_CustomsFeesTotal = 100.00m;
				statementLine3.B3_BrokerReference = "2013556";
				statementLine3.B3_EntryNum = "7700210";
				statementLine3.B3_Status = Enterprise.Customs.US.Business.StatementLineStatusList.Codes.Deleted;

				return monthlyHeader;
			}
		}

		[ExpectNoExceptions]
		public void TestPeriodicMonthlyDeletedReport()
		{
			Assert(((Enterprise.Customs.US.Business.CusStatementHeader)GetBusinessObject).IsMonthlyStatement);
			RunDocumentWithAllSections = ZBool.False;
			RunDocument("Deleted Report");
		}

		[ExpectNoExceptions]
		public void TestPeriodicMonthlyDetailReport()
		{
			Assert(((Enterprise.Customs.US.Business.CusStatementHeader)GetBusinessObject).IsMonthlyStatement);
			RunDocumentWithAllSections = ZBool.False;
			RunDocument("Detail Report");
		}

		[ExpectNoExceptions]
		public void TestPeriodicMonthlyPreliminaryReport()
		{
			Assert(((Enterprise.Customs.US.Business.CusStatementHeader)GetBusinessObject).IsMonthlyStatement);
			RunDocumentWithAllSections = ZBool.False;
			RunDocument("Preliminary Report");
		}

		[ExpectNoExceptions]
		public void TestPeriodicMonthlyTotalReport()
		{
			Assert(((Enterprise.Customs.US.Business.CusStatementHeader)GetBusinessObject).IsMonthlyStatement);
			RunDocumentWithAllSections = ZBool.False;
			RunDocument("Total Report");
		}

		new void RunDocument(string menuItemName)
		{
			ZQuery menuItemFilter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, menuItemName);
			menuItemFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.Equal, BusinessContext);
			menuItemFilter.AddToFilter(StmMenuItemSchema.SU_MenuType, Core.Constants.StmMenuItemTypes.Documents);

			DocumentCommand menuItem = Factory.LoadTop1<DocumentCommand>(menuItemFilter);
			AssertNotNull("The menuItemFilter you specified returned no StmMenuItem.\r\n\r\n" + menuItemFilter.ToCSharpCode(), menuItem);

			DeliveryInstructions instructions = new DeliveryInstructions();
			instructions.Destination = DeliveryInstructionDestination.None;

			using (DocumentPack documentPack = RunDocumentWithAllSections ? new DocumentPackAlwaysIncludesSections(menuItem, (IDocumentSupportable)GetBusinessObject, null) : new DocumentPack(menuItem, (IDocumentSupportable)GetBusinessObject, null, null))
			{
				using (PrintTask printTask = new PrintTask())
				{
					printTask.Add(documentPack);
					printTask.Run(instructions);
				}
			}
		}
	}
}
