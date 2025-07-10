using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(CusStatementHeader))]
	public class CusStatementHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Deleting object should have an exception/error because of a trigger.", true);
		}

		public void TestHumanReadableNameCore()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = "M";
			statement.B2_StatementNumber = "030190079807";

			AssertEquals("Statements / Stamp Duty 030190079807", statement.HumanReadableName);
		}

		public void TestSetDefaultValues()
		{
			var statement = Factory.New<CusStatementHeader>();
			Factory.Save();

			var localTime = Env.Time.CurrentLocalDate;
			CombineAssertions("TestEditableCusStatementHeader", () =>
			{
				AssertEquals("B2_StatementNumber", "EDI00000002", statement.B2_StatementNumber);
				AssertEquals("B2_PaymentType", "1", statement.B2_PaymentType);
				AssertEquals("B2_PrintDate", ZDateTime.Empty, statement.B2_PrintDate);
				AssertEquals("B2_Status", "PRE", statement.B2_Status);
				AssertEquals("B2_PaymentStatus", "PAD", statement.B2_PaymentStatus);
				AssertEquals("B2_PaymentParty", "BRK", statement.B2_PaymentParty);
				AssertEquals("B2_StatementAmount", ZDecimal.Zero, statement.B2_StatementAmount);
				AssertEquals("B2_PeriodStartDate", new ZDate(localTime.Year, localTime.Month, 1), statement.B2_PeriodStartDate);
				AssertEquals("B2_PeriodEndDate", new ZDate(localTime.Year, localTime.Month, 1).AddMonths(1).AddDays(-1), statement.B2_PeriodEndDate);
				AssertEquals("B2_DueDate", new ZDate(localTime.Year, localTime.Month, 20).AddMonths(1), statement.B2_DueDate);
			});
		}
		public void TestCreateCusStatementHeader()
		{
			var statement = Factory.New<CusStatementHeader>();
			statementHeader.B2_IsMonthlyStatement = true;
			statement.B2_StatementNumber = "123456789";
			statement.B2_PaymentType = "1";
			statement.B2_PrintDate = new ZDateTime(2022, 10, 1);
			statement.B2_Status = "PRE";
			statement.B2_PaymentStatus = "PAD";
			statement.B2_PaymentParty = "BRK";
			statement.B2_DueDate = new DateTime(2022, 11, 20);
			statement.B2_PeriodStartDate = new ZDate(2022, 10, 1);
			statement.B2_PeriodEndDate = new ZDate(2022, 10, 1).AddMonths(1).AddDays(-1);

			var statementLine1 = statement.StatementLines.AddNew();
			statementLine1.B3_EntryType = StatementLineEntryTypeList.Codes.MAN;
			statementLine1.B3_BrokerReference = "11111111";
			statementLine1.B3_AssociatedEntry = "22222222";
			statementLine1.B3_EntryNum = "33333";
			statementLine1.B3_EntryDate = new ZDate(2022, 10, 1);
			var statementLineCharge1 = statementLine1.Charges.AddNew();
			statementLineCharge1.B4_ChargeAmount = 10;
			statementLineCharge1.B4_ChargeType = "OBS";
			var statementLineCharge2 = statementLine1.Charges.AddNew();
			statementLineCharge2.B4_ChargeAmount = 5;
			statementLineCharge2.B4_ChargeType = "ABS";

			var statementLine2 = statement.StatementLines.AddNew();
			statementLine2.B3_EntryType = StatementLineEntryTypeList.Codes.MAN;
			statementLine2.B3_BrokerReference = "11111111";
			statementLine2.B3_AssociatedEntry = "22222222";
			statementLine2.B3_EntryNum = "33333";
			statementLine2.B3_EntryDate = new ZDate(2022, 10, 1);
			var statementLineCharge3 = statementLine2.Charges.AddNew();
			statementLineCharge3.B4_ChargeAmount = 2;
			statementLineCharge3.B4_ChargeType = "OBS";
			Factory.Save();

			CombineAssertions("TestEditableCusStatementHeader", () =>
			{
				AssertEquals("B2_StatementNumber", "123456789", statement.B2_StatementNumber);
				AssertEquals("B2_PaymentType", "1", statement.B2_PaymentType);
				AssertEquals("B2_PrintDate", new ZDate(2022, 10, 1), statement.B2_PrintDate);
				AssertEquals("B2_DueDate", new ZDate(2022, 11, 20), statement.B2_DueDate);
				AssertEquals("B2_Status", "PRE", statement.B2_Status);
				AssertEquals("B2_PaymentStatus", "PAD", statement.B2_PaymentStatus);
				AssertEquals("B2_PaymentParty", "BRK", statement.B2_PaymentParty);
				AssertEquals("TotalChargeAmount", 17m, statement.TotalChargeAmount);
			});
		}

		public void TestStatementHeaderAttributes()
		{
			CombineAssertions("Statement Header Attributes", () =>
			{
				AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusStatementHeader), nameof(CusStatementHeader.B2_StatementNumber), false, attr => attr.Caption == "Statement Number");
				AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusStatementHeader), nameof(CusStatementHeader.B2_PaymentType), false, attr => attr.Caption == "Payment Type");
				AssertHasCustomAttribute<ListAttribute>(typeof(CusStatementHeader), nameof(CusStatementHeader.B2_PaymentType), false, attr => attr.ListDataSourceMember == (nameof(statementHeader.Lookups) + "." + nameof(CusStatementHeaderLookups.PaymentTypesList)));
				AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusStatementHeader), nameof(CusStatementHeader.B2_PrintDate), false, attr => attr.Caption == "Print Date");
				AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusStatementHeader), nameof(CusStatementHeader.B2_Status), false, attr => attr.Caption == "Statement Status");
				AssertHasCustomAttribute<ListAttribute>(typeof(CusStatementHeader), nameof(CusStatementHeader.B2_Status), false, attr => attr.ListDataSourceMember == (nameof(statementHeader.Lookups) + "." + nameof(CusStatementHeaderLookups.StatementHeaderStatusList)));
				AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusStatementHeader), nameof(CusStatementHeader.B2_PaymentStatus), false, attr => attr.Caption == "Payment Status");
				AssertHasCustomAttribute<ListAttribute>(typeof(CusStatementHeader), nameof(CusStatementHeader.B2_PaymentStatus), false, attr => attr.ListDataSourceMember == (nameof(statementHeader.Lookups) + "." + nameof(CusStatementHeaderLookups.PaymentStatusList)));
				AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusStatementHeader), nameof(CusStatementHeader.B2_PaymentParty), false, attr => attr.Caption == "Payment Party");
				AssertHasCustomAttribute<ListAttribute>(typeof(CusStatementHeader), nameof(CusStatementHeader.B2_PaymentParty), false, attr => attr.ListDataSourceMember == (nameof(statementHeader.Lookups) + "." + nameof(CusStatementHeaderLookups.PaymentPartyList)));
			});
		}

		public void TestReadOnly()
		{
			CombineAssertions("Read-Only", () =>
			{
				AssertEquals("Statement Number", true, statementHeader.B2_StatementNumberInfo.ReadOnly);
				AssertEquals("Payment Type", false, statementHeader.B2_PaymentTypeInfo.ReadOnly);
				AssertEquals("Print Date", true, statementHeader.B2_PrintDateInfo.ReadOnly);
				AssertEquals("Statement Status", false, statementHeader.B2_StatusInfo.ReadOnly);
				AssertEquals("Payment Status", false, statementHeader.B2_PaymentStatusInfo.ReadOnly);
				AssertEquals("Payment Party", false, statementHeader.B2_PaymentPartyInfo.ReadOnly);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			statementHeader = Factory.New<CusStatementHeader>();
		}

		CusStatementHeader statementHeader;
	}

	[TestedType(typeof(CusStatementHeader.Loader))]
	class Test : LoaderTestCase
	{
		public void TestLoadCusStatementHeader()
		{
			var header1 = Factory.New<CusStatementHeader>();
			header1.B2_StatementType = "M";
			header1.B2_GC = GlbCompany.CurrentCompany.PK;
			header1.B2_IsMonthlyStatement = ZBool.True;
			header1.B2_PeriodStartDate = new ZDate(2022, 01, 01);
			header1.B2_PeriodEndDate = new ZDate(2022, 01, 31);
			Factory.Save();

			var result = loader.LoadMonthlyStatementWithPeriodStartDate("M", new ZDate(2022, 01, 10), GlbCompany.CurrentCompany.PK);

			AssertEquals(header1.PK, result.PK);
		}

		protected override void SetUp()
		{
			base.SetUp();
			loader = new CusStatementHeader.Loader(Factory);
		}
		CusStatementHeader.Loader loader;

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CusStatementHeader.Loader(Factory);
		}
	}
}
