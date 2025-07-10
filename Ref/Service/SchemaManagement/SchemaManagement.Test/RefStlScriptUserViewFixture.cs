using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	[TestFixture]
	[TransactionedTestCase]
	class RefStlScriptUserViewFixture
	{
		[Test]
		public void View()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
	
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var refStlScriptUserView = new RefStlScriptUserView()
				{
					STL_PK = Guid.NewGuid(),
					STL_FeatureCode = "AAA",
					STL_RoleName = "Role Name",
					STL_ModuleName = "Module Name",
					STL_FunctionName = "Functional Name",
					STL_FeatureName = "Feature Name",
					STL_DataGranularity = "DAY",
					STL_CompanyCode = "AAA",
					STL_BranchCode = "",
					STL_AdditionalRefs = "",
					STL_BillingReference1 = "",
					STL_BillingReference2 = "",
					STL_BillingReference3 = "",
					STL_BillingReference4 = "",
					STL_CreatingUserCode = "",
					STL_MinCW1Version = "",
					STL_MaxCW1Version = "",
					STL_PreparationScript = "SELECT 1",
					STL_TransactionDateUtc = "2021-09-22 00:00:00.0000000",
					STL_GuidReference = "XXX",
					STL_TransactionCount = "1",
					STL_FromClause = "AAA",
					STL_WhereClause = "",
					STL_WithOptionRecompile = false,
					STL_UsedInBilling = true,
					STL_ActiveOn = "ALL",
					STL_DateType = "DTE",
					STL_IsPublished = true
				};
				context.RefStlScriptUserViews.Add(refStlScriptUserView);
				context.SaveChanges();
				Assert.That(context.DataSetChangeHistories.Where(x => x.DCH_ParentPK == refStlScriptUserView.STL_PK).ToArray(), Has.Length.EqualTo(1));
			}
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var refStlScriptUserView = context.RefStlScriptUserViews.FirstOrDefault();
				Assert.AreEqual("AAA", refStlScriptUserView.STL_FeatureCode);

				var versionControl = context.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == refStlScriptUserView.STL_PK);
				Assert.AreEqual(false, versionControl.RVC_Deleted);

				refStlScriptUserView.STL_RoleName = "New Role Name";
				refStlScriptUserView.STL_IsPublished = false;
				context.SaveChanges();
			}

			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var refStlScriptUserView = context.RefStlScriptUserViews.FirstOrDefault();
				Assert.AreEqual("New Role Name", refStlScriptUserView.STL_RoleName);
			
				var versionControl = context.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == refStlScriptUserView.STL_PK);
				Assert.AreEqual(true, versionControl.RVC_Deleted);
			}

			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var refStlScriptUserView = context.RefStlScriptUserViews.FirstOrDefault();
				context.RefStlScriptUserViews.Remove(refStlScriptUserView);
				Assert.Throws<DbUpdateException>(() => context.SaveChanges(), "Should not enable Delete in top-level tables");
			}
		}
	}
}
