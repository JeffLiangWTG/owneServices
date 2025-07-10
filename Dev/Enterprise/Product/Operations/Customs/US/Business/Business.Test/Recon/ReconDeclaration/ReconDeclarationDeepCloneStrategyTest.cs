using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconDeclarationDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestCopyFromExistedReconciliation()
		{
			using (USCustomsDataRegistry.Instance.EntryDeclarant.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				var branch = company.Branches.AddNew();
				branch.GB_Code = "U#@";
				branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				var srcDec = Factory.New<JobDeclaration>();
				srcDec.JE_GB = branch.PK;
				srcDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				srcDec.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Recon;
				srcDec.JE_EntryAuthorisationDate = ZDateTime.BrettsBirthday.AddDays(1);
				srcDec.US_Comment = "COMMENTS";
				srcDec.US_PreliminaryStatementPrintDate = ZDateTime.BrettsBirthday.AddDays(3);
				srcDec.US_EntryFilerCode = "XJ5";
				srcDec.US_ClientBranchDesignation = "DP";
				var broker = Factory.New<GlbStaff>();
				broker.GS_Code = "AGT";
				srcDec.JE_GS_NKCusAgent = broker.GS_Code;
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "KD$#";
				org.OH_FullName = "TEST ORG";
				var reconDec = new ReconDeclaration(srcDec);
				srcDec.ReconDeclaration = reconDec;
				reconDec.US_PreliminaryStatementPrintDate = ZDateTime.BrettsBirthday.AddDays(3);
				reconDec.US_Comment = "Recon COMMENTS";
				reconDec.US_EstimatedEntryDate = ZDateTime.BrettsBirthday.AddDays(4);
				reconDec.US_PreliminaryStatementPrintDate = ZDateTime.BrettsBirthday.AddDays(6);
				reconDec.US_EntryFilerCode = "XJ6";
				reconDec.US_ClientBranchDesignation = "D1";
				reconDec.US_SchDEntry = "2934";
				reconDec.US_PaymentType = "T";
				reconDec.US_IssueCode = "AP1";
				reconDec.JE_OH_NotifyParty = org.PK;
				Factory.Save();
				var newReconDec = (JobDeclaration)reconDec.TemplateReconDeclarationCopyCore(Customs.Business.CloneType.TemplateCopy);
				AssertDetails(newReconDec, branch.PK, org.PK);
			}
		}

		void AssertDetails(JobDeclaration reconDec, ZGuid branchPK, ZGuid notifyPartyPK)
		{
			CombineAssertions(() =>
			{
				AssertEquals("JE_MessageType", Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Recon, reconDec.JE_MessageType);
				AssertEquals("JE_EntryAuthorisationDate", ZDateTime.Empty, reconDec.JE_EntryAuthorisationDate);
				AssertEquals("US_EntryFilerCode", "XJ6", reconDec.US_EntryFilerCode);
				AssertEquals("US_SchDEntry", "2934", reconDec.US_SchDEntry);
				AssertEquals("US_PreliminaryStatementPrintDate", ZDateTime.Empty, reconDec.US_PreliminaryStatementPrintDate);
				AssertEquals("TotalDutyDifference", ZDecimal.Zero, reconDec.ReconDeclaration.TotalDutyDifference);
				AssertEquals("US_Comment", ZString.Empty, reconDec.US_Comment);
				AssertEquals("ReconEntryNumber", string.Empty, reconDec.ReconDeclaration.ReconEntryNumber);
				AssertEquals("US_PaymentType", "T", reconDec.US_PaymentType);
				AssertEquals("US_IssueCode", "AP1", reconDec.US_IssueCode);
				AssertEquals("JE_GS_NKCusAgent", string.Empty, reconDec.JE_GS_NKCusAgent);
				AssertEquals("JE_OH_NotifyParty", notifyPartyPK, reconDec.JE_OH_NotifyParty);
				AssertEquals("US_ClientBranchDesignation", string.Empty, reconDec.US_ClientBranchDesignation);
				AssertEquals("JE_GB", branchPK, reconDec.JE_GB);
			});
		}
	}
}
