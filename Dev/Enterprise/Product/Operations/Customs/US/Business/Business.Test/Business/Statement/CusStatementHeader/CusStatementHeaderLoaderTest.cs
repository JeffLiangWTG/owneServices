using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusStatementHeader.Loader))]
	sealed class CusStatementHeaderLoaderTest : LoaderTestCase
	{
		public void TestCreateNewCusStatementHeaderFilter()
		{
			AssertNotNull(loader.CreateNewCusStatementHeaderFilter("XJ5", "8804P04001", GlbCompany.CurrentCompany.PK));
		}

		public void TestLoadCusStatementHeader()
		{
			CusStatementHeader header = Factory.New<CusStatementHeader>();
			header.B2_EntryFilerCode = "XJ5";
			header.B2_ProcessPort = "8888";
			header.B2_StatementNumber = "8804P04001";
			Factory.Save();

			AssertEquals(header.B2_StatementNumber, loader.LoadWithStatementNumber("XJ5", "8804P04001", GlbCompany.CurrentCompany.PK).B2_StatementNumber);
		}

		public void TestHasDeletedStatement()
		{
			var header = Factory.New<CusStatementHeader>();

			var line = header.StatementLines.AddNew();
			line.B3_EntryNum = "12345678";
			line.B3_EntryFilerCode = "XJ5";
			Factory.Save();
			Assert("Does not have deleted statements", !loader.HasDeletedStatement("XJ5", "12345678", GlbCompany.CurrentCompany.PK));

			line.B3_Status = StatementLineStatusList.Codes.Deleted;
			Factory.Save();
			Assert("Has deleted statements", loader.HasDeletedStatement("XJ5", "12345678", GlbCompany.CurrentCompany.PK));
		}

		public void TestLoadWithEntryFilerCodeAndEntryNumber()
		{
			CusStatementHeader statement1 = CreateStatementWithLine("XJ5", "1");

			CusStatementHeader statement2_2 = CreateStatementWithLine("XJ5", "2");
			statement2_2.StatementLines[0].B3_Status = StatementLineStatusList.Codes.Deleted;

			CusStatementHeader statement2 = CreateStatementWithLine("XJ5", "2");
			CusStatementHeader statement3 = CreateStatementWithLine("XJ5", "3");
			Factory.Save();

			AssertEquals(statement1, new CusStatementHeader.Loader(Factory).Load("XJ5", "1", GlbCompany.CurrentCompany.PK));
			AssertEquals(statement2, new CusStatementHeader.Loader(Factory).Load("XJ5", "2", GlbCompany.CurrentCompany.PK));
		}

		public void TestLoadDoesNotCauseMultipleDBHits()
		{
			CusStatementHeader statement = CreateStatementWithLine("XJ5", "1");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertLoadAndAssertDBHit(statement.PK, newFactory, 1); // once
			AssertLoadAndAssertDBHit(statement.PK, newFactory, 1); // twice
			AssertLoadAndAssertDBHit(statement.PK, newFactory, 1); // thrice
		}

		void AssertLoadAndAssertDBHit(ZGuid statementPK, BusinessObjectFactory factory, int hitCount)
		{
			AssertEquals(statementPK, new CusStatementHeader.Loader(factory).Load("XJ5", "1", GlbCompany.CurrentCompany.PK).PK);
			var tableSelects = factory.TableSelects;
			AssertEquals(2, tableSelects.Length);
			var cusStatementLineTable = tableSelects[0];
			var cusStatementHeaderTable = tableSelects[1];
			if (cusStatementLineTable.TableName != CusStatementLineSchema.Constants.TableName)
			{
				cusStatementLineTable = tableSelects[1];
				cusStatementHeaderTable = tableSelects[0];
			}
			AssertEquals(CusStatementLineSchema.Constants.TableName, cusStatementLineTable.TableName);
			AssertEquals(hitCount, cusStatementLineTable.Value);
			AssertEquals(CusStatementHeaderSchema.Constants.TableName, cusStatementHeaderTable.TableName);
			AssertEquals(1, cusStatementHeaderTable.Value);
		}

		CusStatementHeader CreateStatementWithLine(ZString entryFilerCode, ZString entryNumber)
		{
			CusStatementHeader header = Factory.New<CusStatementHeader>();

			CusStatementLine statementLine = header.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = entryFilerCode;
			statementLine.B3_EntryNum = entryNumber;
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;

			return header;
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
