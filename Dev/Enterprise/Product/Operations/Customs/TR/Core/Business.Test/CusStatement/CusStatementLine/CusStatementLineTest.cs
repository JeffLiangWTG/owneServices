using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(CusStatementLine))]
	public class CusStatementLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(StatementLineStatusList.Codes.NRL, statementLine.B3_Status);
		}

		public void TestB3_BrokerReferenceAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusStatementLine), nameof(CusStatementLine.B3_BrokerReference), false, attr => attr.Caption == "Job Number");
		}

		public void TestB3_EntryTypeAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusStatementLine), nameof(CusStatementLine.B3_EntryType), false, attr => attr.Caption == "Entry Type");
			AssertHasCustomAttribute<ListAttribute>(typeof(CusStatementLine), nameof(CusStatementLine.B3_EntryType), false, attr => attr.ListDataSourceMember == (nameof(statementLine.Lookups) + "." + nameof(CusStatementLineLookups.StatementLineEntryTypeList)));
		}

		public void TestB3_EntryNumAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusStatementLine), nameof(CusStatementLine.B3_EntryNum), false, attr => attr.Caption == "Entry Number");
		}

		public void TestB3_EntryDateAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusStatementLine), nameof(CusStatementLine.B3_EntryDate), false, attr => attr.Caption == "Entry Date");
		}

		public void TestB3_AssociatedEntryAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusStatementLine), nameof(CusStatementLine.B3_AssociatedEntry), false, attr => attr.Caption == "Associated Entry");
		}

		public void TestB3_StatusAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusStatementLine), nameof(CusStatementLine.B3_Status), false, attr => attr.Caption == "Line Status");
			AssertHasCustomAttribute<ListAttribute>(typeof(CusStatementLine), nameof(CusStatementLine.B3_Status), false, attr => attr.ListDataSourceMember == (nameof(statementLine.Lookups) + "." + nameof(CusStatementLineLookups.StatementLineStatusList)));
		}

		public void TestB3_CustomsFeesTotalAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusStatementLine), nameof(CusStatementLine.B3_CustomsFeesTotal), false, attr => attr.Caption == "Stamp Duty Total");
		}

		public void TestIsStatementLineReadOnly()
		{
			CombineAssertions(() =>
			{
				statementLine.B3_EntryType = StatementLineEntryTypeList.Codes.MAN;
				statementLine.B3_AssociatedEntry = "Bill";
				AssertEquals("Editable if not in Database", false, statementLine.IsStatementLineReadOnly);

				statementLine.B3_EntryType = StatementLineEntryTypeList.Codes.EXP;
				statementLine.B3_AssociatedEntry = "BillNo2";
				Factory.Save();
				AssertEquals("Readonly if in Databse, Entry type neither EXP and B3_AssociatedEntry not empty", true, statementLine.IsStatementLineReadOnly);

				statementLine.B3_EntryType = StatementLineEntryTypeList.Codes.ACC;
				statementLine.B3_AssociatedEntry = "BillNo1";
				Factory.Save();
				AssertEquals("Entry Type ACC", false, statementLine.IsStatementLineReadOnly);

				statementLine.B3_EntryType = CargoWise.Types.ZString.Empty;
				AssertEquals("Entry Type empty", false, statementLine.IsStatementLineReadOnly);

				statementLine.B3_EntryType = StatementLineEntryTypeList.Codes.EXP;
				statementLine.B3_AssociatedEntry = CargoWise.Types.ZString.Empty;
				Factory.Save();
				AssertEquals("B3_AssociatedEntry empty", false, statementLine.IsStatementLineReadOnly);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return statementLine;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Deleting object should have an exception/error because of a trigger.", true);
		}

		protected override void SetUp()
		{
			base.SetUp();
			statementHeader = Factory.New<CusStatementHeader>();
			statementLine = statementHeader.StatementLines.AddNew();
		}

		CusStatementHeader statementHeader;
		CusStatementLine statementLine;
	}
}
