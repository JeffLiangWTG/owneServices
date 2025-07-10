using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class IStatementForProviderExtensionsTest : TestCaseWithFactory
	{
		public void TestGetRelatedBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~US";
			company.GC_RN_NKCountryCode = "US";

			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "US1";

			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "US2";

			Factory.Save();

			using (Enterprise.Environment.DisposableEnvironment.ForBranch(branch1.PK.ToGuid()))
			{
				USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, "D1");
				USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, "D2");

				var dailyStatement = Factory.New<CusStatementHeader>();
				dailyStatement.B2_StatementNumber = "12345";
				dailyStatement.B2_IsMonthlyStatement = false;
				dailyStatement.B2_GC = company.PK;
				dailyStatement.B2_BranchDesignation = "D2";
				AssertEquals(branch2, dailyStatement.GetRelatedBranch());

				dailyStatement.B2_BranchDesignation = "D1";
				AssertEquals(branch1, dailyStatement.GetRelatedBranch());

				var monthlyStatement = Factory.New<CusStatementHeader>();
				monthlyStatement.B2_StatementNumber = "1234P";
				monthlyStatement.B2_IsMonthlyStatement = true;
				monthlyStatement.B2_GC = company.PK;
				dailyStatement.B2_B2_PeriodicStatement = monthlyStatement.PK;
				AssertEquals(branch1, monthlyStatement.GetRelatedBranch());

				monthlyStatement.B2_BranchDesignation = "D2";
				AssertEquals(branch2, monthlyStatement.GetRelatedBranch());

				monthlyStatement.B2_BranchDesignation = "";
				dailyStatement.B2_BranchDesignation = "";

				var statementLine = dailyStatement.StatementLines.AddNew();
				statementLine.B3_EntryFilerCode = "CJ5";
				statementLine.B3_EntryNum = "12345678";

				Factory.Save();

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.US_EnableENS = true;
				declaration.US_EntryFilerCode = "CJ5";
				declaration.ImportEntryNumber = "12345678";
				declaration.JE_GB = branch2.PK;

				declaration.Invoices.AddNew();
				declaration.InvoiceLines.AddNew();
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				Factory.Save();

				AssertEquals(declaration, statementLine.Declaration);
				AssertEquals(branch2, monthlyStatement.GetRelatedBranch());
				AssertEquals(branch2, dailyStatement.GetRelatedBranch());
			}
		}
	}
}
