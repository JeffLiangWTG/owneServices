using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccGLHeaderFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchStrategy_FetchForLoad()
		{
			var glHeaderPks = CreateTestData();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var headers = factory.Load<AccGLHeader>(new ZQuery(AccGLHeaderSchema.PK, glHeaderPks));
			AssertEquals("No fetch hint should be in place for Bank Account when loading GLHeader", 0, factory.ActiveFetchHintsForTable(AccBankAccountSchema.Constants.TableName));
			foreach (var glAccount in headers)
			{
				AssertNotNullOrEmpty("Precondition: touch all descriptions", glAccount.AG_Description);
			}
			var dbHitsOnGlHeader = factory.GetTableHitCount(AccGLHeaderSchema.Constants.TableName);
			var dbHitsOnBankAccount = factory.GetTableHitCount(AccBankAccountSchema.Constants.TableName);
			AssertEquals("Precondition: fetching all GL headers happens in one DB hit", 1, dbHitsOnGlHeader);
			AssertEquals("Bank accounts should not be loaded", 0, dbHitsOnBankAccount);
		}

		#region Implementation

		List<ZGuid> CreateTestData()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_Description = "I AM LINKED TO A BANK ACCOUNT";
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AG = glHeader.PK;
			bankAccount.AB_GC = GlbCompany.CurrentCompany.PK;
			bankAccount.AB_Desc = "LINKED BANK ACCOUNT";

			var glHeaderPks = new List<ZGuid>(10);
			glHeaderPks.Add(glHeader.PK);
			for (int i = 0; i < 9; i++)
			{
				var header = Factory.NewWithValidTestData<AccGLHeader>();
				header.AG_Description = "SOMETHING";
				glHeaderPks.Add(header.PK);
			}
			Factory.Save();

			return glHeaderPks;
		}

		#endregion
	}
}
