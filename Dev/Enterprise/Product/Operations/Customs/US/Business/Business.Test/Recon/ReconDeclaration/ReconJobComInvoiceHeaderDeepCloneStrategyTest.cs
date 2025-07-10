using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconJobComInvoiceHeaderDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestCopyReconEntryHeader()
		{
			var srcDec = Factory.New<JobDeclaration>();
			srcDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			srcDec.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Recon;
			srcDec.JE_EntryAuthorisationDate = ZDateTime.BrettsBirthday;
			srcDec.US_Comment = "COMMENTS";
			srcDec.US_PreliminaryStatementPrintDate = ZDateTime.BrettsBirthday;
			srcDec.US_EntryFilerCode = "XJ5";
			srcDec.US_ClientBranchDesignation = "DP";
			CusEntryHeader entryRCI = Factory.New<CusEntryHeader>();
			entryRCI.CH_JE = srcDec.PK;
			entryRCI.CH_BGMReference = "XJ5123456";
			entryRCI.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry;
			entryRCI.Charges.AddNew("DDD", 45.5);
			entryRCI.Charges.AddNew("PPP", 46.6);
			CusEntryHeader entryREC = Factory.New<CusEntryHeader>();
			entryREC.CH_JE = srcDec.PK;
			entryREC.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconEntry;
			var broker = Factory.New<MasterFiles.Business.GlbStaff>();
			broker.GS_Code = "AGT";
			srcDec.JE_GS_NKCusAgent = broker.GS_Code;
			srcDec.ReconDeclaration = srcDec.ReconDeclaration ?? new ReconDeclaration(srcDec);
			var reconDec = srcDec.ReconDeclaration;
			reconDec.US_PreliminaryStatementPrintDate = ZDateTime.BrettsBirthday;
			reconDec.US_Comment = "Recon COMMENTS";
			reconDec.US_EstimatedEntryDate = ZDateTime.BrettsBirthday;
			JobComInvoiceHeader invoice = reconDec.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "XJ5123456";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_CH_ReconEntry = entryRCI.PK;
			Factory.Save();
			var newReconDec = (JobDeclaration)srcDec.ReconDeclaration.TemplateReconDeclarationCopyCore(Customs.Business.CloneType.TemplateCopy);
			AssertEquals(newReconDec.JE_MessageType, Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Recon);
			AssertEntries(newReconDec.ReconDeclaration.OriginalEntries, srcDec.ReconDeclaration.OriginalEntries);
		}

		void AssertEntries(ReconOriginalEntryHeaderCollection reconEntries, ReconOriginalEntryHeaderCollection srcEntries)
		{
			foreach (ReconOriginalEntryHeader oneEntry in srcEntries)
			{
				var reconEntry = reconEntries.FindEntryBy("XJ5123456");
				AssertEntry(reconEntry, oneEntry);
			}
		}

		void AssertEntry(ReconOriginalEntryHeader newEntry, ReconOriginalEntryHeader srcEntry)
		{
			CombineAssertions(() =>
			{
				AssertEquals(newEntry.CH_OrigEntryReference, srcEntry.CH_OrigEntryReference);
				AssertEquals(newEntry.US_R_NoLineDetails, srcEntry.US_R_NoLineDetails);
				AssertEquals(newEntry.US_R_OwnerRef, srcEntry.US_R_OwnerRef);
				AssertEquals(newEntry.US_NAFTAClaimStat, srcEntry.US_NAFTAClaimStat);
				AssertEquals(newEntry.US_PriorDisclosure, srcEntry.US_PriorDisclosure);
				AssertEquals(newEntry.US_ProtestStat, srcEntry.US_ProtestStat);
				AssertEquals(newEntry.US_ProtestID, srcEntry.US_ProtestID);
				AssertEquals(newEntry.US_PendingActionIDType, srcEntry.US_PendingActionIDType);
				AssertEquals(newEntry.US_PendingActionID, srcEntry.US_PendingActionID);
				AssertEquals(newEntry.US_R_CalcOrigDuty, srcEntry.US_R_CalcOrigDuty);
				AssertEquals(newEntry.US_ImportDate, srcEntry.US_ImportDate);
				AssertEquals(newEntry.US_R_ReleaseDate, srcEntry.US_R_ReleaseDate);
				AssertEquals(newEntry.US_R_DateForMPFCalc, srcEntry.US_R_DateForMPFCalc);
				AssertEquals(newEntry.US_R_DutyRateDate, srcEntry.US_R_DutyRateDate);
				AssertEquals(newEntry.US_R_IsHMFApplicable, srcEntry.US_R_IsHMFApplicable);
				AssertEquals(newEntry.US_SchDEntry, srcEntry.US_SchDEntry);
				AssertEquals(newEntry.US_PriorDisclosure, srcEntry.US_PriorDisclosure);
				AssertEquals(newEntry.US_NAFTAClaimStat, srcEntry.US_NAFTAClaimStat);
				AssertEquals(newEntry.US_ProtestID, srcEntry.US_ProtestID);
				AssertEquals(newEntry.US_PendingActionID, srcEntry.US_PendingActionID);
				AssertEquals(newEntry.US_PendingActionIDType, srcEntry.US_PendingActionIDType);
				foreach (CusEntryHeaderCharges charge in srcEntry.ReconCharges)
				{
					AssertEquals(newEntry.ReconCharges.GetAmount(charge.C1_ChargeType), srcEntry.OriginalCharges.GetAmount(charge.C1_ChargeType));
				}
			});
		}
	}
}
