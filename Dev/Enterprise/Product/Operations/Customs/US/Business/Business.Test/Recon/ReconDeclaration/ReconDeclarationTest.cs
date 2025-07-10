using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconDeclaration))]
	sealed class ReconDeclarationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLiquidationDetails()
		{
			var reconDeclaration = GetNewBusinessObject() as ReconDeclaration;

			var liquidation1 = Factory.New<CusLiquidation>();
			liquidation1.B8_SystemCreateDate = new ZDateTime(2024, 9, 21);
			liquidation1.B8_LiquidationDate = new ZDateTime(2024, 9, 20);
			reconDeclaration.Liquidations.Add(liquidation1);

			var liquidation2 = Factory.New<CusLiquidation>();
			liquidation2.B8_SystemCreateDate = new ZDateTime(2024, 9, 22);
			liquidation2.B8_LiquidationDate = new ZDateTime(2024, 9, 19);
			reconDeclaration.Liquidations.Add(liquidation2);

			var liquidation3 = Factory.New<CusLiquidation>();
			liquidation3.B8_SystemCreateDate = new ZDateTime(2024, 9, 23);
			liquidation3.B8_LiquidationDate = new ZDateTime(2024, 9, 18);
			reconDeclaration.Liquidations.Add(liquidation3);

			var liquidation4 = Factory.New<CusLiquidation>();
			liquidation4.B8_SystemCreateDate = new ZDateTime(2024, 9, 24);
			liquidation4.B8_LiquidationDate = new ZDateTime(2024, 9, 17);
			reconDeclaration.Liquidations.Add(liquidation4);

			AssertEquals("Liquidation date should comes from most recent create date", new ZDateTime(2024, 9, 17), reconDeclaration.LiquidationDate);
		}

		public void TestUS_NAFTAReconIndicatorOfEntryShouldBeSetToTrueWhenUS_IssueCodeOfIsBeingCopiedReconIsNF()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.USFTARECONIND, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
				reconDec.US_IssueCode = ReconIssueCodeList.Codes.FTA;
				var entry = reconDec.OriginalEntries.AddNew();
				reconDec.ReconWrappedJobDeclaration.CustomsEntryHeaders.Load();
				Assert(!entry.US_NAFTAReconIndicator);
				var result = ((JobDeclaration)reconDec.TemplateReconDeclarationCopyCore(CloneType.TemplateCopy)).ReconDeclaration;
				AssertNotNull(result);
				Assert(result.OriginalEntries[0].US_NAFTAReconIndicator);
			}
		}

		public void TestEnsureThatAddInfoDataIsSaved()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var recon = ReconDeclaration.Get(declaration);
			recon.US_R_ImporterIDLodged = "TEST";
			recon.US_R_TeamNoLodged = "XYZ";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			recon = ReconDeclaration.Get(declaration);
			AssertEquals("US_R_ImporterIDLodged", "TEST", recon.US_R_ImporterIDLodged);
			AssertEquals("US_R_TeamNoLodged", "XYZ", recon.US_R_TeamNoLodged);
		}

		public void TestIsWaitingForRespones()
		{
			var declaration = Factory.New<JobDeclaration>();
			var recon = ReconDeclaration.Get(declaration);
			var reconEntry = recon.ReconEntry.GetEntry();
			Factory.Save();
			AssertEquals(false, recon.IsWaitingForRespones);

			reconEntry.CH_Status = ReconMessageStatusList.Codes.AwaitingReconOriginal;
			Factory.Save();
			AssertEquals(true, recon.IsWaitingForRespones);

			reconEntry.CH_Status = string.Empty;
			Factory.Save();
			AssertEquals(false, recon.IsWaitingForRespones);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();
			AssertEquals(false, recon.IsWaitingForRespones);

			reconEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			Factory.Save();
			AssertEquals(false, recon.IsWaitingForRespones);

			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			Factory.Save();
			AssertEquals(false, recon.IsWaitingForRespones);

			entry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.CertificationSentAckPending;
			Factory.Save();
			AssertEquals(true, recon.IsWaitingForRespones);
		}

		public void TestHumanReadableShortcutNameInRecon()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_FullName = "IMPORTERFULLNAME";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var reconDec = new ReconDeclaration(declaration);
			declaration.JE_DeclarationReference = "S005202392";
			declaration.JE_OH_Importer = organisation.PK;
			reconDec.ImporterAddress.E2_OA_Address = organisation.PK;

			AssertEquals("Recon Job# ImportFullName", string.Format("Recon. - {0} - {1}", reconDec.JE_DeclarationReference, reconDec.Importer.OH_FullName), reconDec.HumanReadableShortcutName);
		}

		public void TestIAllocateEntryNumberSupporter()
		{
			var dec = Factory.New<JobDeclaration>();
			var reconDec = new ReconDeclaration(dec);
			var supporter = (IAllocateNumberSupporter)reconDec;
			supporter.DoAllocate("342089");
			AssertEquals("342089", reconDec.ReconEntryNumber);
			AssertEquals("342089", supporter.GetExistingNumber());
			AssertEquals("Entry Number", supporter.NumberType);
		}

		public void TestHumanReadableName()
		{
			var declaration = Factory.New<JobDeclaration>();
			var reconDec = new ReconDeclaration(declaration);
			declaration.JE_DeclarationReference = "S005202392";
			AssertEquals("Recon Declaration HumanReadableName", "Recon. Declaration S005202392", reconDec.HumanReadableName);
		}

		public void TestAllocationUseMutex()
		{
			var declaration = Factory.New<JobDeclaration>();
			var reconDec = new ReconDeclaration(declaration);
			declaration.US_EntryFilerCode = "XJ5";
			Factory.Save();

			var reconEntryNumberRefreshCount = 0;
			reconDec.ReconEntryNumberInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				reconEntryNumberRefreshCount++;
			};

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var declarationInDiffFactory = newFactory.Load<JobDeclaration>(declaration.PK);
			var reconDecInDiffFactory = new ReconDeclaration(declarationInDiffFactory);
			IAllocateNumberSupporter supporterInDiffFactory = reconDecInDiffFactory;
			AssertEquals(true, supporterInDiffFactory.LockNumberAllocationMutex);
			supporterInDiffFactory.DoAllocate("12345678");
			AssertEquals("Allocated", "12345678", reconDecInDiffFactory.ReconEntryNumber);
			reconDecInDiffFactory.ReconEntry.CH_Status = ReconMessageStatusList.Codes.AwaitingReconOriginal;

			IAllocateNumberSupporter supporter = reconDec;
			AssertEquals(false, supporter.LockNumberAllocationMutex);
			supporter.UnlockNumberAllocationMutex();
			AssertEquals(false, supporter.LockNumberAllocationMutex);
			AssertEquals(true, supporterInDiffFactory.LockNumberAllocationMutex);
			AssertEquals("", supporter.GetReasonToStopProceeding());

			newFactory.Save();
			AssertEquals(0, reconEntryNumberRefreshCount);
			AssertEquals(JobDeclaration.Constants.DisallowEntryNumberAllocation.EntryNumberAlreadyAllocated("12345678"), supporter.GetReasonToStopProceeding());
			AssertEquals("12345678", reconDec.ReconEntryNumber);
			AssertEquals(1, reconEntryNumberRefreshCount);
			supporterInDiffFactory.UnlockNumberAllocationMutex();
		}

		public void TestIControllerIDProviderMembers()
		{
			var dec = Factory.New<JobDeclaration>();
			IControllerIDProvider provider = new ReconDeclaration(dec);
			AssertEquals("ControllerID", ControllerIDs.Customs.US.Recon, provider.ControllerID);
			AssertEquals("BusinessObjectPK", dec.PK.ToGuid(), provider.BusinessObjectPK);
		}

		public void TestReconDeclarationHasSamePKAsJobDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			var reconDeclaration = new ReconDeclaration(dec);
			AssertEquals("ReconDecalaration should have the same PK as the wrapped JobDeclaration as required by ModuleResultsPKCollection", dec.PK, reconDeclaration.PK);
		}

		public void TestIsPaymentTypeValid()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var list = reconDeclaration.Lookups.US_PaymentTypeList;
			reconDeclaration.US_PaymentType = ZString.Empty;
			AssertEquals(false, reconDeclaration.IsPaymentTypeValid);
			foreach (ICodeDescription codePair in list)
			{
				reconDeclaration.US_PaymentType = codePair.Code;
				AssertEquals(true, reconDeclaration.IsPaymentTypeValid);
			}
		}

		public void TestServiceDirection()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var reconDeclaration = new ReconDeclaration(declaration);
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			Assert(reconDeclaration.InvoicingSupporter is IServiceDirection);
			AssertEquals(JobMessageTypeList.Codes.FTZ, ((IServiceDirection)reconDeclaration.InvoicingSupporter).ServiceDirection);
		}

		public void TestBrokerToPayCalculation()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			AssertEquals("Broker To Pay is empty", ZString.Empty, reconDeclaration.BrokerToPayIndicator);

			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals("Broker To Pay should be empty, becuase payment type is 'IndividualBasis'", ZString.Empty, reconDeclaration.BrokerToPayIndicator);

			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertEquals("Broker To Pay should be YES, becuase payment type is 'Broker'", YesNoDefaultList.Codes.Yes, reconDeclaration.BrokerToPayIndicator);

			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			AssertEquals("Broker To Pay should be NO, becuase payment type is 'Importer'", YesNoDefaultList.Codes.No, reconDeclaration.BrokerToPayIndicator);
		}

		public void TestPaymentTypeDescription()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_PaymentType = ZString.Empty;
			AssertEquals("Payment Type Description", ZString.Empty, reconDeclaration.PaymentTypeDescription);

			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals("Payment Type Description", PaymentTypeList.Descriptions.IndividualBasis, reconDeclaration.PaymentTypeDescription);

			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			AssertEquals("Payment Type Description", PaymentTypeList.Descriptions.BatchedByDailyPrintDateAndImporter, reconDeclaration.PaymentTypeDescription);
		}

		public void TestReconYears()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry1 = reconDeclaration.OriginalEntries.AddNew();
			originalEntry1.US_PaymentDate = new ZDateTime(2009, 9, 11);
			var originalEntry2 = reconDeclaration.OriginalEntries.AddNew();
			originalEntry2.US_PaymentDate = new ZDateTime(2009, 9, 12);

			AssertEquals("2009", reconDeclaration.ReconYears);
			originalEntry2.US_PaymentDate = new ZDateTime(2010, 9, 11);

			AssertEquals("2009 - 2010", reconDeclaration.ReconYears);

			var originalEntry3 = reconDeclaration.OriginalEntries.AddNew();
			originalEntry3.US_PaymentDate = new ZDateTime(2008, 9, 12);
			AssertEquals("2008 - 2010", reconDeclaration.ReconYears);

			originalEntry1.US_PaymentDate = new ZDateTime(2011, 9, 11);
			AssertEquals("2008 - 2011", reconDeclaration.ReconYears);
		}

		public void TestEarliestEntryDate()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry1 = reconDeclaration.OriginalEntries.AddNew();
			originalEntry1.US_R_ReleaseDate = new ZDateTime(2008, 9, 11);
			originalEntry1.US_PaymentDate = new ZDateTime(2008, 9, 12);

			var originalEntry2 = reconDeclaration.OriginalEntries.AddNew();
			originalEntry2.US_R_ReleaseDate = new ZDateTime(2009, 9, 11);
			originalEntry2.US_PaymentDate = new ZDateTime(2009, 9, 12);

			AssertEquals(reconDeclaration.EarliestEntryDate, new ZDateTime(2008, 9, 12));
		}

		public void TestIControllerIDProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var reconDeclaration = (IControllerIDProvider)new ReconDeclaration(declaration);

			AssertEquals(declaration.PK, reconDeclaration.BusinessObjectPK);
			AssertEquals(ControllerIDs.Customs.US.Recon, reconDeclaration.ControllerID);
		}

		[TestDate(2008, 3, 25)]
		public void TestDutiesCalculatedOnlyForRequiredEntries()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2203.00.00 60";
			invoiceLine.US_UC_NKCountryOfOrigin = "NZ";
			invoiceLine.US_UC_NKCountryOfExport = "NZ";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 15000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Excise Tax", 2300.84m, declaration.CustomsEntryHeaders[0].TotalEstimatedTax);
			Factory.Save();

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_NoLineDetails = true;
			originalEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.OtherExcise, 500m);

			var originalEntry2 = reconDeclaration.OriginalEntries.AddNew();
			originalEntry2.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;
			originalEntry2.US_R_NoLineDetails = false;
			originalEntry2.US_R_CalcOrigDuty = true;
			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });
			originalEntry2.US_R_DutyRateDate = ZDateTime.Today;

			reconDeclaration.CalculateDutyFeesForChangedEntries();

			AssertEquals("Charges should stay there", 500m, originalEntry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise));
			AssertEquals("Charge amount should be zero", 0m, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise));
			AssertEquals("Charges should be re-calculated", 2300.84m, originalEntry2.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise));
			AssertEquals("Charges should be re-calculated", 2300.84m, originalEntry2.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise));

			originalEntry2.Invoice.JobComInvoiceLines[0].JI_CustomsQuantity = 20000m;
			reconDeclaration.CalculateDutyFeesForChangedEntries();

			AssertEquals("Charges should stay there", 500m, originalEntry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise));
			AssertEquals("Charge amount should be zero", 0m, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise));
			AssertEquals("Charges should be re-calculated", 2300.84m, originalEntry2.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise));
			AssertEquals("Charges should be re-calculated", 3067.78m, originalEntry2.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise));

			var reconDeclaration2 = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration2.US_R_IsNoChangeAgg = true;
			var originalEntry_reconDeclaration2 = reconDeclaration2.OriginalEntries.AddNew();
			originalEntry_reconDeclaration2.CH_OrigEntryReference = "XJ5" + declaration.ImportEntryNumber;

			reconDeclaration2.ImportLines(new JobDeclaration[] { declaration });
			Factory.Save();
			AssertEquals("No Charges calculated", 0m, originalEntry.OriginalCharges.GetGrandTotalFee());
			AssertEquals("No Charge amount", 0m, originalEntry.ReconCharges.GetGrandTotalFee());
		}

		public void TestUS_IssueCodeDescription()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.Value9802Recon;
			AssertEquals(ReconIssueCodeList.Descriptions.Value9802Recon, reconDeclaration.US_IssueCodeDescription);
			reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			AssertEquals(ReconIssueCodeList.Descriptions.FTA, reconDeclaration.US_IssueCodeDescription);
		}

		public void TestTotalOriginalDuty()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_NoLineDetails = true;
			originalEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Duty, 500m);

			AssertEquals(500m, reconDeclaration.TotalOriginalDuty);
		}

		public void TestTotalReconDuty()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_NoLineDetails = true;
			originalEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Duty, 400m);

			AssertEquals(400m, reconDeclaration.TotalReconDuty);
		}

		public void TestTotalDutyDifference()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_NoLineDetails = true;
			originalEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Duty, 400m);
			originalEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Duty, 500m);

			AssertEquals(-100m, reconDeclaration.TotalDutyDifference);
		}

		public void TestTotalOriginalFee()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_NoLineDetails = true;
			originalEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.FreshLimes, 300m);

			AssertEquals(300m, reconDeclaration.TotalOriginalFee);
		}

		public void TestTotalReconFee()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_NoLineDetails = true;
			originalEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.FreshLimes, 350m);

			AssertEquals(350m, reconDeclaration.TotalReconFee);
		}

		public void TestTotalFeeDifference()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_NoLineDetails = true;
			originalEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.FreshLimes, 390m);
			originalEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.FreshLimes, 350m);

			AssertEquals(-40m, reconDeclaration.TotalFeeDifference);
		}

		public void TestTotalOriginalTax()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_NoLineDetails = true;
			originalEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, 200m);

			AssertEquals(200m, reconDeclaration.TotalOriginalTax);
		}

		public void TestTotalReconTax()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_NoLineDetails = true;
			originalEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, 250m);

			AssertEquals(250m, reconDeclaration.TotalReconTax);
		}

		public void TestTotalTaxDifference()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.US_R_NoLineDetails = true;
			originalEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, 250m);
			originalEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, 260m);

			AssertEquals(10m, reconDeclaration.TotalTaxDifference);
		}

		public void TestInterestPaymentAmount()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_IsAggregate = true;
			reconDeclaration.US_R_AggregateInterest = 301m;

			AssertEquals(301m, reconDeclaration.InterestPaymentAmount);
		}

		public void TestIClientBranchDesignationDefaultMembers()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			AssertEquals(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), ((IClientBranchDesignationDefault)reconDeclaration).Branch);
		}

		public void TestUS_IsAggregate()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.RefundedFees.AddNewIfNotExists("499");

			reconDeclaration.US_IsAggregate = true;
			AssertEquals(0, originalEntry.RefundedFees.Count);

			reconDeclaration.AggregateRefundedFees.AddNewIfNotExists("499");
			reconDeclaration.US_IsAggregate = false;
			AssertEquals(0, reconDeclaration.AggregateRefundedFees.Count);
		}

		[TestDate(2011, 03, 15)]
		public void TestIPrelimStatementDetailsDefaultMembers()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			reconDeclaration.US_EstimatedEntryDate = new ZDateTime(2011, 03, 19); // Saturday

			var iPrelimStatementDetailsDefaultDeclaration = (IPrelimStatementDetailsDefault)reconDeclaration;

			AssertEquals(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), iPrelimStatementDetailsDefaultDeclaration.Branch);
			AssertEquals("US_EstimatedEntryDate falls to Saturday, 1 day should be added", 1, iPrelimStatementDetailsDefaultDeclaration.DaysToAddToStatementDate);

			reconDeclaration.US_EstimatedEntryDate = new ZDateTime(2011, 03, 17);
			AssertEquals("0 days should be added, because Estimated Entry Date is in the future", 0, iPrelimStatementDetailsDefaultDeclaration.DaysToAddToStatementDate);
		}

		public void TestICustomsJobInfoMembers()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			ICustomsJobInfo info = reconDeclaration;
			AssertEquals(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), info.Branch);

			var creditor = Factory.New<OrgHeader>();
			creditor.FillWithValidTestData();
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, creditor.PK.ToGuid());
			AssertEquals(creditor.PK, info.CreditorPK);
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reconDeclaration.PK.ToGuid());
			AssertEquals(ZGuid.Empty, info.CreditorPK);

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration1.US_EnableENS = true;
			declaration1.US_EntryFilerCode = "SV9";
			var invoice = declaration1.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "22030060";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1234567890";
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "8512300030";
			declaration1.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration1.ImportEntryNumber = "70027576";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration2.US_EnableENS = true;
			declaration2.US_EntryFilerCode = "SV9";
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine4 = invoice2.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "6104220040";
			var invoiceLine5 = invoice2.InvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "0306230020";
			declaration2.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration2.ImportEntryNumber = "70034713";
			Factory.Save();

			var reconDeclaration2 = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry1 = reconDeclaration2.OriginalEntries.AddNew();
			originalEntry1.CH_OrigEntryReference = "SV9" + declaration1.ImportEntryNumber;

			var originalEntry2 = reconDeclaration2.OriginalEntries.AddNew();
			originalEntry2.CH_OrigEntryReference = "SV9" + declaration2.ImportEntryNumber;

			reconDeclaration2.ImportLines(new JobDeclaration[] { declaration1, declaration2 });

			AssertEquals(2, reconDeclaration2.Invoices.Count);
			AssertEquals(3, reconDeclaration2.Invoices[0].JobComInvoiceLines.Count);

			ICustomsJobInfo info2 = reconDeclaration2;

			var anotherLine = reconDeclaration2.ReconWrappedJobDeclaration.InvoiceLines.AddNew();
			anotherLine.US_R_OrigCottonFeeExempt = "Y";

			AssertEquals(2, info2.Entries.Count);
			AssertEquals(0, info2.Entries[0].InvoiceLines);
			AssertEquals(3, info2.Entries[0].EntryLines);
			AssertEquals(0, info2.Entries[1].InvoiceLines);
			AssertEquals(3, info2.Entries[1].EntryLines);
		}

		public void TestGetReasonNotToAllowChangeOnCostDetails()
		{
			AccountingIntegrationOptions options = new AccountingIntegrationOptions();
			options.EnableAccountingIntegration = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, options);

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			IJobInvoicingPlugIn jobInvoicingParent = reconDeclaration;

			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeCode1.PK.ToGuid());

			Factory.Save();

			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertNull(jobInvoicingParent.InvoicingSupporter.GetReasonNotToAllowChangeOnCostDetails(chargeCode1.PK));
			AssertNull(jobInvoicingParent.InvoicingSupporter.GetReasonNotToAllowChangeOnCostDetails(chargeCode2.PK));

			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertEquals(JobDeclarationInvoicingSupporter.ShouldNotChangeCostOrPostWhileOnStatement, jobInvoicingParent.InvoicingSupporter.GetReasonNotToAllowChangeOnCostDetails(chargeCode1.PK));
			AssertNull(jobInvoicingParent.InvoicingSupporter.GetReasonNotToAllowChangeOnCostDetails(chargeCode2.PK));

			options.EnableAccountingIntegration = false;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, options);
			AssertNull(jobInvoicingParent.InvoicingSupporter.GetReasonNotToAllowChangeOnCostDetails(chargeCode1.PK));
		}

		public void TestGetReasonNotToAllowChangeWhenFinalStatementIsThere()
		{
			AccountingIntegrationOptions options = new AccountingIntegrationOptions();
			options.EnableAccountingIntegration = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, options);
			//TODO INC CustomsRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Core.Constants.Groups.PostMastersGroupPK);

			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeCode1.PK.ToGuid());

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_EntryFilerCode = "XJ5";
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;

			ReconEntryHeader entry = reconDeclaration.ReconEntry;
			entry.GetEntry().EntryNumber = "~12345";
			Factory.Save();

			AssertNotEquals(ZString.Empty, entry.EntryNumber);

			IJobInvoicingPlugIn jobInvoicingParent = reconDeclaration;
			AssertEquals(JobDeclarationInvoicingSupporter.ShouldNotChangeCostOrPostWhileOnStatement, jobInvoicingParent.InvoicingSupporter.GetReasonNotToAllowChangeOnCostDetails(chargeCode1.PK));

			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			statement.B2_Status = StatementHeaderStatusList.Codes.Final;
			CusStatementLine statementline = statement.StatementLines.AddNew();
			statementline.B3_EntryFilerCode = "XJ5";
			statementline.B3_EntryNum = entry.EntryNumber;
			statementline.B3_Status = StatementLineStatusList.Codes.Active;
			Factory.Save();
			AssertNull(jobInvoicingParent.InvoicingSupporter.GetReasonNotToAllowChangeOnCostDetails(chargeCode1.PK));
		}

		public void TestINeedDataSetOnReconDeclarationToShowDataSetProperlyOnDeveloperForms()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDeclaration = new ReconDeclaration(declaration);

			INeedDataSet iDec = reconDeclaration;
			AssertEquals(((INeedDataSet)declaration).Data, iDec.Data);
		}

		[TestDate(2011, 4, 11)]
		public void TestReconPaymentDate()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertEquals("ReconPayment Date", ZDate.Today.AddDays(1), reconDeclaration.ReconPaymentDate);

			reconDeclaration.US_EstimatedEntryDate = ZDateTime.BrettsBirthday.AddDays(1);
			AssertEquals("ReconPayment date", ZDateTime.BrettsBirthday.AddDays(2), reconDeclaration.ReconPaymentDate);

			reconDeclaration.US_PreliminaryStatementPrintDate = ZDateTime.BrettsBirthday;
			AssertEquals("ReconPayment date", ZDate.BrettsBirthday, reconDeclaration.ReconPaymentDate);

			reconDeclaration.US_PreliminaryStatementPrintDate = ZDateTime.Invalid;
			AssertEquals("ReconPayment date", ZDateTime.BrettsBirthday.AddDays(1), reconDeclaration.ReconPaymentDate);
		}

		public void TestSetDefaults()
		{
			var systemAccount = GlbStaff.CurrentUser.GS_IsSystemAccount;
			GlbStaff.CurrentUser.GS_IsSystemAccount = false;

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			AssertEquals(ReconciliationImportEntrySourceList.Codes.FiftyStates, reconDeclaration.US_ImportEntrySource);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, reconDeclaration.JE_GS_NKCusAgent);
			GlbStaff.CurrentUser.GS_IsSystemAccount = systemAccount;
		}

		public void TestImportEntrySourceDescription()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			AssertEquals("FiftyStates by default", ReconciliationImportEntrySourceList.Descriptions.FiftyStates, reconDeclaration.ImportEntrySourceDescription);

			reconDeclaration.US_ImportEntrySource = ReconciliationImportEntrySourceList.Codes.VirginIslands;
			AssertEquals("US_ImportEntrySource description", ReconciliationImportEntrySourceList.Descriptions.VirginIslands, reconDeclaration.ImportEntrySourceDescription);

			reconDeclaration.US_ImportEntrySource = ZString.Empty;
			AssertEquals("US_ImportEntrySource description should be empty", ZString.Empty, reconDeclaration.ImportEntrySourceDescription);
		}

		public void TestPreparerName()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "Test Preparer";
			staff.GS_Code = "TSP";

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.JE_GS_NKCusAgent = "TSP";
			AssertEquals("Preparer Name", "Test Preparer", reconDeclaration.PreparerName);
		}

		public void TestAuditProperties()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "Test Preparer";
			staff.GS_Code = "TSP";

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.ReconWrappedJobDeclaration.JE_SystemCreateUser = "TSP";
			reconDeclaration.ReconWrappedJobDeclaration.JE_SystemLastEditUser = "TSP";
			Factory.Save();
			AssertEquals("CreatedDate", ZDateTime.UtcNow.Date, reconDeclaration.JE_SystemCreateTimeUtc.Date);
			AssertEquals("LastEditTime", ZDateTime.UtcNow.Date, reconDeclaration.JE_SystemLastEditTimeUtc.Date);
			AssertEquals("CreatedUser", "TSP", reconDeclaration.JE_SystemCreateUser);
			AssertNotEquals("LastEditUser should not be empty", ZString.Empty, reconDeclaration.JE_SystemLastEditUser);
			AssertEquals("CreatedUserName", "Test Preparer", reconDeclaration.CreatedUserName);
			AssertNotEquals("LastEditUserName should not be empty", ZString.Empty, reconDeclaration.LastEditUserName);
		}

		public void TestDefaultClientBranchDesignationWhenPaymentTypeIsEntered()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "AB");

			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertEquals("ClientBranchDesignation should have been defaulted", "AB", reconDeclaration.US_ClientBranchDesignation);

			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals("ClientBranchDesignation should have been defaulted", "", reconDeclaration.US_ClientBranchDesignation);
		}

		[TestDate(2008, 12, 1)]
		public void TestDefaultFieldsFromImporterOfRecord()
		{
			var importerOfRecord = Factory.NewWithValidTestData<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(importerOfRecord);
			wrapper.ZO_ReconPaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			wrapper.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;
			Factory.Save();

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var declaration = reconDeclaration.ReconWrappedJobDeclaration;

			reconDeclaration.IOROrgPK = importerOfRecord.PK;
			AssertEquals("Payment Type is defaulted", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, reconDeclaration.US_PaymentType);
			AssertEquals("PSD Date will be calculated, because Payment Type was set",
				new ZDateTime(2008, 12, 02), reconDeclaration.US_PreliminaryStatementPrintDate);
			AssertEquals(ReconIssueCodeList.Codes.ValueRecon, reconDeclaration.US_IssueCode);
		}

		[TestDate(2009, 1, 5)]
		public void TestDefaultPreliminaryStatementPrintDate()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertEquals("Preliminary Statement PrintDate should be defaulted from Estimated Entry Date",
						new ZDateTime(2009, 01, 06), reconDeclaration.US_PreliminaryStatementPrintDate);

			reconDeclaration.US_EstimatedEntryDate = new ZDateTime(2009, 1, 10);
			AssertEquals("Preliminary Statement PrintDate is defaulted from Estimated Entry Date", new ZDateTime(2009, 1, 12), reconDeclaration.US_PreliminaryStatementPrintDate);

			reconDeclaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporterWithSuffixes;
			AssertEquals("Preliminary Statement PrintDate is defaulted from Estimated Entry Date", new ZDateTime(2009, 1, 12), reconDeclaration.US_PreliminaryStatementPrintDate);

			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals("Preliminary Statement PrintDate should be empty for Individual basis", ZDateTime.Empty, reconDeclaration.US_PreliminaryStatementPrintDate);

			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertEquals("Preliminary Statement PrintDate is defaulted from Estimated Entry Date", new ZDateTime(2009, 1, 12), reconDeclaration.US_PreliminaryStatementPrintDate);

			reconDeclaration.US_PaymentType = ZString.Empty;
			AssertEquals("Preliminary Statement PrintDate should be empty, because Payment Type is empty", ZDateTime.Empty, reconDeclaration.US_PreliminaryStatementPrintDate);

			var importerOfRecord = Factory.New<OrgHeader>();
			importerOfRecord.FillWithValidTestData();
			reconDeclaration.IOROrgPK = importerOfRecord.PK;
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertEquals("Preliminary Statement PrintDate is defaulted from Estimated Entry Date", new ZDateTime(2009, 1, 12), reconDeclaration.US_PreliminaryStatementPrintDate);
		}

		public void TestCalculateInterest()
		{
			DeclarationTestHelper.SetReconInterestInRegistry(new ZDate(1999, 1, 1), new ZDate(1999, 03, 31), 7m);//7%
			DeclarationTestHelper.SetReconInterestInRegistry(new ZDate(1999, 4, 1), new ZDate(1999, 09, 30), 8m);//8%

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_IsAggregate = false;
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDate(1999, 9, 15);

			ReconOriginalEntryHeader entry1 = reconDeclaration.OriginalEntries.AddNew();
			entry1.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4000m);
			entry1.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4300m);
			entry1.US_PaymentDate = new ZDate(1999, 1, 5);

			ReconOriginalEntryHeader entry2 = reconDeclaration.OriginalEntries.AddNew();
			entry2.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4000m);
			entry2.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4670m);
			entry2.US_PaymentDate = new ZDate(1999, 4, 12);

			ReconOriginalEntryHeader entry3 = reconDeclaration.OriginalEntries.AddNew();
			entry3.OriginalCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4000m);
			entry3.ReconCharges.SetAmount(Core.Constants.USCustoms.FeeCodes.Duty, 4600m);
			entry3.US_PaymentDate = new ZDate(1999, 5, 28);

			new ReconInterestCalculator(new ReconInterestDataProviderReconDec(reconDeclaration)).Execute();

			AssertEquals("entry1 Interest is calculated", 16.42m, entry1.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
			AssertEquals("entry2 Interest is calculated", 23.45m, entry2.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
			AssertEquals("entry3 Interest is calculated", 14.78m, entry3.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest));
		}

		public void TestTotalFeesAndCharges()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDec = new ReconDeclaration(declaration);

			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.Avocado, "TotalOriginalAvocado", "TotalReconAvocado", "TotalAvocadoDifference", 1m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.Beef, "TotalOriginalBeef", "TotalReconBeef", "TotalBeefDifference", 2m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.Blueberry, "TotalOriginalBlueberry", "TotalReconBlueberry", "TotalBlueberryDifference", 3m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.Cotton, "TotalOriginalCotton", "TotalReconCotton", "TotalCottonDifference", 4m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.DistilledSpirits, "TotalOriginalSpirits", "TotalReconSpirits", "TotalSpiritsDifference", 5m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.DutiableMail, "TotalOriginalDutiableMail", "TotalReconDutiableMail", "TotalDutiableMailDifference", 6m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.FreshLimes, "TotalOriginalLimes", "TotalReconLimes", "TotalLimesDifference", 7m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.Honey, "TotalOriginalHoney", "TotalReconHoney", "TotalHoneyDifference", 8m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.Mango, "TotalOriginalMango", "TotalReconMango", "TotalMangoDifference", 9m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.MerchandiseInformal, "TotalOriginalInformal", "TotalReconInformal", "TotalInformalDifference", 10m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge, "TotalOriginalSurcharge", "TotalReconSurcharge", "TotalSurchargeDifference", 11m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.Mushroom, "TotalOriginalMushroom", "TotalReconMushroom", "TotalMushroomDifference", 12m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.OtherExcise, "TotalOriginalOtherExcise", "TotalReconOtherExcise", "TotalOtherExciseDifference", 13m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.Raspberry, "TotalOriginalRaspberry", "TotalReconRaspberry", "TotalRaspberryDifference", 14m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.Pork, "TotalOriginalPork", "TotalReconPork", "TotalPorkDifference", 15m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.Potato, "TotalOriginalPotato", "TotalReconPotato", "TotalPotatoDifference", 16m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.SoftwoodLumber, "TotalOriginalSoftwoodLumber", "TotalReconSoftwoodLumber", "TotalSoftwoodLumberDifference", 17m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.Sugar, "TotalOriginalSugar", "TotalReconSugar", "TotalSugarDifference", 18m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.Tobacco, "TotalOriginalTobacco", "TotalReconTobacco", "TotalTobaccoDifference", 19m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.Watermelon, "TotalOriginalWatermelon", "TotalReconWatermelon", "TotalWatermelonDifference", 20m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.Wines, "TotalOriginalWines", "TotalReconWines", "TotalWinesDifference", 21m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.Sorghum, "TotalOriginalSorghum", "TotalReconSorghum", "TotalSorghumDifference", 22m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.OtherAgencies, "TotalOriginalOtherAgencies", "TotalReconOtherAgencies", "TotalOtherAgenciesDifference", 23m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, "TotalOriginalMPF", "TotalReconMPF", "TotalMPFDifference", 24m);
			AssertTotalChargeFeeAndDifference(reconDec, Core.Constants.USCustoms.FeeCodes.HMF, "TotalOriginalHMF", "TotalReconHMF", "TotalHMFDifference", 25m);
		}

		public void TestAggregatedEntries()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDec = new ReconDeclaration(declaration);

			reconDec.OriginalEntries.AddNew();
			reconDec.OriginalEntries.AddNew();

			AssertEquals("AggregatedEntries", 1, reconDec.AggregatedEntries.Count);

			reconDec.OriginalEntries.AddNew();
			reconDec.RefreshAggregatedEntries();
			AssertEquals("AggregatedEntries", 2, reconDec.AggregatedEntries.Count);
		}

		public void TestIDutyDataLineHeaderProvider()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDec = new ReconDeclaration(declaration);

			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.Invoice.InvoiceLines.AddNew();
			originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.Invoice.InvoiceLines.AddNew();

			IDutyDataLineHeaderProvider provider = reconDec;
			AssertEquals(4, new List<IDutyDataLineHeader>(provider.EntriesToCalculateDutyFeeTax).Count);
		}

		public void TestSettingValueToSelectedOriginalEntryDoesNotCauseHasChanges()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());

			ReconOriginalEntryHeader reconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice1 = reconOriginalEntry.Invoice;
			ReconOriginalEntryHeader reconOriginalEntry2 = reconDeclaration.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice2 = reconOriginalEntry2.Invoice;

			AssertEquals(2, reconDeclaration.FilteredInvoices.Count);

			Factory.Save();

			AssertEquals("No HasChanges", false, reconDeclaration.HasChanges);

			reconDeclaration.SelectedOriginalEntry = reconOriginalEntry.CH_PK;
			AssertEquals("This should not cause HasChanges", false, reconDeclaration.HasChanges);
		}

		public void TestHasAddInfoChangesSinceLastSaving()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			AssertEquals(false, reconDeclaration.HasAddInfoChangesSinceLastSaving(USAddInfoSchema.US_PreliminaryStatementPrintDate));

			Factory.Save();
			AssertEquals(false, reconDeclaration.HasAddInfoChangesSinceLastSaving(USAddInfoSchema.US_PreliminaryStatementPrintDate));

			reconDeclaration.US_PreliminaryStatementPrintDate = ZDateTime.Today;
			AssertEquals(true, reconDeclaration.HasAddInfoChangesSinceLastSaving(USAddInfoSchema.US_PreliminaryStatementPrintDate));

			Factory.Save();
			AssertEquals(false, reconDeclaration.HasAddInfoChangesSinceLastSaving(USAddInfoSchema.US_PreliminaryStatementPrintDate));
		}

		public void TestDefaultReconTeamFromPort()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDec = new ReconDeclaration(declaration);
			reconDec.US_SchDEntry = ReconPortsList.Codes._0712;
			AssertEquals("Team No should be defaulted from Filing Port", ReconTeamsList.Codes._1R1, reconDec.US_TeamNo);
			AssertEquals("Team No Description", ReconTeamsList.Descriptions._1R1, reconDec.TeamNoDescription);

			reconDec.US_SchDEntry = ReconPortsList.Codes._2402;
			AssertEquals("Team No should be defaulted from Filing Port", ReconTeamsList.Codes._6R3, reconDec.US_TeamNo);
			AssertEquals("Team No Description", ReconTeamsList.Descriptions._6R3, reconDec.TeamNoDescription);
		}

		public void TestCanSendOriginal()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDec = new ReconDeclaration(declaration);
			AssertEquals(true, reconDec.CanSendOriginal);

			reconDec.MessageStatus = ReconMessageStatusList.Codes.ClearReconOriginal;
			AssertEquals(false, reconDec.CanSendOriginal);
		}

		public void TestCanSendWithdrawal()
		{
			var declaration = Factory.New<JobDeclaration>();
			var reconDec = new ReconDeclaration(declaration);
			AssertEquals(false, reconDec.CanSendWithdrawal);

			var reconEntry = reconDec.ReconEntry.GetEntry();
			AssertEquals(false, reconEntry.HasBeenLodgedAtCustoms);

			reconEntry.CH_Status = ReconMessageStatusList.Codes.AwaitingReconOriginal;
			AssertEquals(false, reconEntry.HasBeenLodgedAtCustoms);
			AssertEquals(false, reconDec.CanSendWithdrawal);

			reconEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconOriginal;
			AssertEquals(true, reconEntry.HasBeenLodgedAtCustoms);
			AssertEquals(true, reconDec.CanSendWithdrawal);
			AssertEquals(false, reconDec.CanSendOriginal);

			reconEntry.CH_Status = ReconMessageStatusList.Codes.AwaitingReconReplace;
			AssertEquals(true, reconEntry.HasBeenLodgedAtCustoms);
			AssertEquals(true, reconDec.CanSendWithdrawal);
			AssertEquals(false, reconDec.CanSendOriginal);

			reconEntry.CH_Status = ReconMessageStatusList.Codes.ErrorReconReplace;
			AssertEquals(true, reconEntry.HasBeenLodgedAtCustoms);
			AssertEquals(true, reconDec.CanSendWithdrawal);
			AssertEquals(false, reconDec.CanSendOriginal);

			reconEntry.CH_Status = ReconMessageStatusList.Codes.ErrorReconDelete;
			AssertEquals(true, reconEntry.HasBeenLodgedAtCustoms);
			AssertEquals(true, reconDec.CanSendWithdrawal);
			AssertEquals(false, reconDec.CanSendOriginal);

			reconEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconDelete;
			AssertEquals(false, reconEntry.HasBeenLodgedAtCustoms);
			AssertEquals(false, reconDec.CanSendWithdrawal);
			AssertEquals(true, reconDec.CanSendOriginal);
			AssertEquals(true, reconEntry.HasBeenWithdrawn);
		}

		public void TestUS_EntryFilerCode()
		{
			EntryFiler filer = new EntryFiler();
			filer.EntryFilerCode = "XJ6";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDec = new ReconDeclaration(declaration);
			AssertEquals("Entry Filer Code", "XJ6", reconDec.US_EntryFilerCode);
		}

		public void TestReconEntry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDec = new ReconDeclaration(declaration);
			AssertNotNull("ReconEntry is created", reconDec.ReconEntry);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration decLoaded = factory2.Load<JobDeclaration>(declaration.PK);
			reconDec = new ReconDeclaration(decLoaded);
			AssertNotNull("ReconEntry is created", reconDec.ReconEntry);
		}

		public void TestMergeDoesNotHappenOnReconDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "SG";
			invoice.US_UC_NKCountryOfOrigin = "SG";
			invoice.Charges.AddNew("OFT", 50m, "USD");

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 3000m;
			invoiceLine.US_SPI = "";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.CustomsEntryHeaders[0].EntryNumber = "~7854378";

			AssertEquals("PreCondition", 45m, invoiceLine.CusEntryLine.DutyAmount);
			Factory.Save();

			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			ReconDeclaration reconDec = new ReconDeclaration(declaration2);
			ReconOriginalEntryHeader reconEntry = reconDec.OriginalEntries.AddNew();
			reconEntry.CH_OrigEntryReference = "~7854378";

			Factory.Save();
			AssertEquals("reconEntry should still be active", false, reconEntry.IsDeleted);
		}

		public void TestDoMerge()
		{
			ReconDeclaration reconDec = new DeclarationTestHelper().GetDutiableReconDeclaration(Factory);
			reconDec.OriginalEntries[0].US_R_DutyRateDate = ZDateTime.Today;
			ReconOriginalEntryHeader reconEntry = reconDec.OriginalEntries[0];

			reconDec.CalculateDutyFeesForChangedEntries();
			AssertEquals("Duty is calculated", 45m, reconEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Duty));

			reconEntry.Invoice.JobComInvoiceLines[0].JI_LinePrice = 0m;
			reconDec.CalculateDutyFeesForChangedEntries();
			AssertEquals("Duty is cleared", 0m, reconEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Duty));

			reconEntry.Invoice.JobComInvoiceLines[0].JI_LinePrice = 1000m;
			reconDec.CalculateDutyFeesForChangedEntries();
			AssertEquals("Duty is re-calculated", 15m, reconEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Duty));

			reconEntry.Invoice.JobComInvoiceLines[0].JI_LinePrice = 2000m;
			reconDec.CalculateDutyFeesForChangedEntries();
			AssertEquals("Duty is re-calculated", 30m, reconEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Duty));
		}

		public void TestDeclarationIsRegisteredAsEditableChild()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			Factory.Save();
			AssertEquals("no changes yet", false, reconDec.HasChanges);

			reconDec.Invoices.AddNew();
			reconDec.InvoiceLines.AddNew().JI_LinePrice = 2400m;
			AssertEquals("changes made in invoice lines affect ReconDec", true, reconDec.HasChanges);
		}

		public void TestMessageTypeIsSet()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Declaration);
			AssertEquals(true, Declaration.IsRecon);
			AssertEquals(JobMessageTypeList.Codes.Recon, Declaration.JE_MessageType);
		}

		public void TestCollections()
		{
			var entry = Declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry;

			var invoice = Declaration.Invoices.AddNew();
			var line1 = Declaration.InvoiceLines.AddNew();
			var line2 = Declaration.InvoiceLines.AddNew();
			var reconDec = new ReconDeclaration(Declaration);
			var message = reconDec.ReconEntry.Messages.AddNew(typeof(EDIMessage));
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			AssertEquals(true, reconDec.OriginalEntries.Contains(entry));
			AssertEquals(true, reconDec.Invoices.Contains(invoice));

			AssertEquals(true, reconDec.InvoiceLines.Contains(line1));
			AssertEquals(true, reconDec.InvoiceLines.Contains(line2));

			AssertEquals(true, reconDec.Messages.Contains(message));
		}

		public void TestMinMaxImportDateFromOriginalEntries()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_IsAggregate = false;
			reconDeclaration.US_PreliminaryStatementPrintDate = new ZDate(1999, 9, 15);

			AssertEquals("EarliestImportDate", ZDate.Empty, reconDeclaration.EarliestImportDate);
			AssertEquals("LatestImportDate", ZDate.Empty, reconDeclaration.LatestImportDate);

			ZDateTime minZDateTime = new ZDateTime(2005, 1, 5);
			ZDateTime maxZDateTime = minZDateTime.AddDays(10);

			ReconOriginalEntryHeader entry1 = reconDeclaration.OriginalEntries.AddNew();
			entry1.US_ImportDate = minZDateTime;

			AssertEquals("EarliestImportDate", minZDateTime.Date, reconDeclaration.EarliestImportDate);
			AssertEquals("LatestImportDate", minZDateTime.Date, reconDeclaration.LatestImportDate);

			ReconOriginalEntryHeader entry2 = reconDeclaration.OriginalEntries.AddNew();
			entry2.US_ImportDate = minZDateTime.AddDays(2);

			ReconOriginalEntryHeader entry3 = reconDeclaration.OriginalEntries.AddNew();
			entry3.US_ImportDate = maxZDateTime;

			ReconOriginalEntryHeader entry4 = reconDeclaration.OriginalEntries.AddNew();
			entry4.US_PaymentDate = new ZDate(1999, 1, 5);

			AssertEquals("EarliestImportDate", minZDateTime.Date, reconDeclaration.EarliestImportDate);
			AssertEquals("LatestImportDate", maxZDateTime.Date, reconDeclaration.LatestImportDate);
		}

		public void TestLogsAndNotes()
		{
			StmALog log = Declaration.Logs.AddNew();
			StmNote note = Declaration.Notes.AddNew();

			ReconDeclaration reconDec = new ReconDeclaration(Declaration);
			AssertEquals(true, reconDec.GetLogs().GetAllLogs().Contains(log));
			AssertEquals(true, reconDec.GetNotes().GetAllNotes().Contains(note));
			Factory.Save();

			AssertNotNull("AddedLog should be created", reconDec.GetLogs().AddedLog);
		}

		public void TestSaveAndDelete()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Declaration);
			AssertEquals(false, Declaration.IsInDatabase);
			AssertEquals(false, reconDec.IsInDatabase);

			Factory.Save();
			AssertEquals(true, Declaration.IsInDatabase);
			AssertEquals(true, reconDec.IsInDatabase);

			JobDeclaration decLoaded = new BusinessObjectFactory().Load<JobDeclaration>(Declaration.PK);
			reconDec = new ReconDeclaration(decLoaded);
			AssertEquals(true, decLoaded.IsInDatabase);
			AssertEquals(true, reconDec.IsInDatabase);

			reconDec.Delete();
			AssertEquals(true, decLoaded.IsDeleted);
			AssertEquals(true, reconDec.IsDeleted);
		}

		public void TestOnApportionmentProgressChangedHooked()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Declaration);

			JobComInvoiceHeader invoice = reconDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 10000m;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			invoice.Charges.AddNew("OFT", 10m, "USD");

			IInvoicesProvider iinvoicesProvider = reconDec;

			bool apportionmentProgressNotified = false;

			iinvoicesProvider.OnApportionmentProgressChanged += new BaseJobDeclaration.ApportionmentProgressEventHandler(delegate
			{ apportionmentProgressNotified = true; });

			reconDec.ResumeApportionment();

			Assert("Apportionment Progress notified", apportionmentProgressNotified);
		}

		public void TestRLF()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var reconDec = new ReconDeclaration(Declaration);
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDec.US_PaymentType = "2";
			reconDec.US_PreliminaryStatementPrintDate = ZDateTime.Today;
			reconDec.US_EntryFilerCode = "XJ6";
			reconDec.US_SchDEntry = "5556";
			var reconEntry = reconDec.ReconEntry;
			reconEntry.GetEntry().EntryNumber = "12345666";
			AssertEquals("8888", reconDec.PreparerDistrictPort);
			AssertEquals("5556", reconDec.US_SchDEntry);

			IStatementDeleteTransaction statDeleteTrans = reconDec;
			AssertEquals(false, statDeleteTrans.ShouldPopulatePreparerSite);

			var rcvMsg = (MQEDIMessage)reconEntry.Messages.AddNew(typeof(MQEDIMessage));
			rcvMsg.EM_ApplicationCode = ApplicationCodeList.Codes.USCustomsImport;
			rcvMsg.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ReconciliationEntrySummaryResponse;
			rcvMsg.EM_MessageSubType = EM_MessageSubTypeList.Codes.ReconOriginal;
			rcvMsg.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			rcvMsg.EM_Status = EDIMessage.Status.Received;
			rcvMsg.EM_MessageText =
@"B001101SV9RX                                  8888XJ5  1   <<MSGNO PLACEHOLDER>>E0 RECONS 000001 REF ID: SV9  32212431 B00227358                                Y  1101SV9RX00001";

			AssertEquals(true, statDeleteTrans.ShouldPopulatePreparerSite);
		}

		public void TestIStatementLineDeclarationMembers()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			DeclarationTestHelper.SetPreparerOfficeCode("12");

			var reconDec = new ReconDeclaration(Declaration);
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDec.US_PaymentType = "2";
			reconDec.US_PreliminaryStatementPrintDate = ZDateTime.Today;
			reconDec.US_EntryFilerCode = "XJ6";
			reconDec.US_SchDEntry = "5556";
			reconDec.ReconEntry.GetEntry().EntryNumber = "12345666";
			AssertEquals("8888", reconDec.PreparerDistrictPort);
			AssertEquals("5556", reconDec.US_SchDEntry);

			var statLine = (IStatementLineDeclaration)reconDec;
			AssertEquals("8888", statLine.US_PreparerDistrictPort);
			AssertEquals("12", statLine.US_PreparerOfficeCode);
			AssertEquals(true, statLine.IsACE);
		}

		public void TestIStatementDeleteTransactionMembers()
		{
			var branch1 = GlbBranch.CurrentBranch;
			USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, "15");
			Declaration.JE_GB = branch1.PK;

			var reconDec = new ReconDeclaration(Declaration);
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			reconDec.US_PaymentType = "2";
			reconDec.US_PreliminaryStatementPrintDate = ZDateTime.Today;
			reconDec.US_EntryFilerCode = "XJ6";
			reconDec.US_SchDEntry = "5556";
			var reconEntry = reconDec.ReconEntry;
			reconEntry.GetEntry().EntryNumber = "12345";

			IStatementDeleteTransaction statDeleteTrans = reconDec;

			AssertEquals(ZString.Empty, statDeleteTrans.PeriodicStatementMonth);
			AssertEquals("15", statDeleteTrans.ClientBranchDesignation);
			AssertEquals(ZDateTime.Today, statDeleteTrans.PreliminaryStatementPrintDate);
			AssertEquals(ZDateTime.Empty, statDeleteTrans.ReleaseDate);
			AssertEquals("2", statDeleteTrans.PaymentType);
			AssertEquals("XJ6", statDeleteTrans.EntryFilerCode);
			AssertEquals("5556", statDeleteTrans.PortOfEntry);
			AssertEquals("5556", statDeleteTrans.ProcessingPort);
			AssertEquals("12345", statDeleteTrans.EntryNumber);
			AssertEquals(false, statDeleteTrans.IsACE);
			AssertEquals(true, statDeleteTrans.ShouldGenerateACEStatementMessage);

			USCustomsDataRegistry.Instance.ARecordOfficeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "~1");
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "8888");

			AssertEquals("8888", statDeleteTrans.PreparerPort);
			AssertEquals("~1", statDeleteTrans.PreparerOfficeCode);
			AssertEquals(false, statDeleteTrans.ShouldPopulatePreparerSite);

			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals(true, statDeleteTrans.IsACE);
			AssertEquals(true, statDeleteTrans.ShouldGenerateACEStatementMessage);

			var sntMsg = (MQEDIMessage)reconEntry.Messages.AddNew(typeof(MQEDIMessage));
			sntMsg.EM_ApplicationCode = ApplicationCodeList.Codes.USCustomsImport;
			sntMsg.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ReconciliationEntrySummary;
			sntMsg.EM_MessageSubType = EM_MessageSubTypeList.Codes.ReconOriginal;
			sntMsg.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			sntMsg.EM_Status = EDIMessage.Status.Sent;
			sntMsg.EM_MessageText =
@"B  1101SV9RE                                  8888XJ5  1   <<MSGNO PLACEHOLDER>>
10ASV9  32212431 1101B00227358   X 58-123456789            891 NA1US            
11CHRISTINA RUSZCZAK  12159051100    CHRISTINA.RUSZCZAK@WISETECHGLOBAL.COM      
20SV9  73057174                                                                 
20SV9  73057125                                                                 
901                                                                             
Y  1101SV9RE";

			var rcvMsg = (MQEDIMessage)reconEntry.Messages.AddNew(typeof(MQEDIMessage));
			rcvMsg.EM_ApplicationCode = ApplicationCodeList.Codes.USCustomsImport;
			rcvMsg.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ReconciliationEntrySummaryResponse;
			rcvMsg.EM_MessageSubType = EM_MessageSubTypeList.Codes.ReconOriginal;
			rcvMsg.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			rcvMsg.EM_Status = EDIMessage.Status.Received;
			rcvMsg.EM_MessageText =
@"B001101SV9RX                                  8888XJ5  1   <<MSGNO PLACEHOLDER>>E0 RECONS 000001 REF ID: SV9  32212431 B00227358                                Y  1101SV9RX00001";

			AssertEquals("8888", statDeleteTrans.PreparerPort);
			AssertEquals("~1", statDeleteTrans.PreparerOfficeCode);
			AssertEquals(true, statDeleteTrans.ShouldPopulatePreparerSite);
		}

		public void TestIBondDetailsDefaultMembers()
		{
			var importerOfRecord = Factory.New<OrgHeader>();
			var reconDec = new ReconDeclaration(Declaration);
			reconDec.IOROrgPK = importerOfRecord.PK;
			IBondDetailsDefault bondDefault = reconDec;
			AssertEquals(importerOfRecord.PK, bondDefault.IORWrapper.organisation.PK);
			AssertEquals(ActivityCodeList.Codes._1, bondDefault.ActivityCode);
		}

		public void TestIORDefault()
		{
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.OH_Code = "Z1Z2Z3Z4";
			var wrapper1 = OrgHeaderWrapper.New(importer1);
			wrapper1.ZO_ReconPaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			wrapper1.ZO_NAFTAReconIndicator = ZBool.True;
			wrapper1.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;
			wrapper1.ZO_ReconBrokerToPay = YesNoDefaultList.Codes.Yes;

			var bondData1 = new CusBondDetailCollection(importer1);
			var oneBondData1 = bondData1.AddNew();
			oneBondData1.PW_ActivityCode = ActivityCodeList.Codes._1;
			oneBondData1.PW_BondAmount = 50000m;
			oneBondData1.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-3);
			oneBondData1.PW_BondNumber = "123456";
			oneBondData1.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			oneBondData1.PW_SuretyCode = "891";
			oneBondData1.PW_BondFiledPort = "3901";

			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_Code = "Z4Z3Z2Z1";
			var wrapper2 = OrgHeaderWrapper.New(importer2);
			wrapper2.ZO_ReconPaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			wrapper2.ZO_OtherReconIndicator = ZString.Empty;
			wrapper2.ZO_NAFTAReconIndicator = ZBool.True;
			wrapper2.ZO_ReconFilingPort = "5869";
			wrapper2.ZO_ImportSource = ReconciliationImportEntrySourceList.Codes.VirginIslands;
			var bondData2 = new CusBondDetailCollection(importer2);
			var oneBondData2 = bondData2.AddNew();
			oneBondData2.PW_ActivityCode = ActivityCodeList.Codes._1a1;
			oneBondData2.PW_BondAmount = 50000m;
			oneBondData2.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-3);
			oneBondData2.PW_BondNumber = "123456";
			oneBondData2.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			oneBondData2.PW_SuretyCode = "792";
			oneBondData2.PW_BondFiledPort = "5869";
			Factory.Save();

			var reconDec = new ReconDeclaration(Declaration);
			reconDec.US_ImportEntrySource = ReconciliationImportEntrySourceList.Codes.FiftyStates;
			reconDec.US_SchDEntry = "8888";
			reconDec.US_SuretyCode = "798";
			reconDec.JE_OH_Importer = importer1.PK;
			AssertEquals(importer1.PK, reconDec.IOROrgPK);
			AssertEquals(PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, reconDec.US_PaymentType);
			AssertEquals("Broker To Pay should be set from IOR", YesNoDefaultList.Codes.Yes, reconDec.BrokerToPayIndicator);
			AssertEquals(ReconIssueCodeList.Codes.ValueRecon, reconDec.US_IssueCode);
			AssertEquals("8888", reconDec.US_SchDEntry);
			AssertEquals(ReconciliationImportEntrySourceList.Codes.FiftyStates, reconDec.US_ImportEntrySource);
			AssertEquals("891", reconDec.US_SuretyCode);

			reconDec.JE_OH_Importer = importer2.PK;
			AssertEquals(importer1.PK, reconDec.IOROrgPK);
			AssertEquals(PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, reconDec.US_PaymentType);
			AssertEquals(YesNoDefaultList.Codes.Yes, reconDec.BrokerToPayIndicator);
			AssertEquals(ReconIssueCodeList.Codes.ValueRecon, reconDec.US_IssueCode);
			AssertEquals("8888", reconDec.US_SchDEntry);
			AssertEquals(ReconciliationImportEntrySourceList.Codes.FiftyStates, reconDec.US_ImportEntrySource);
			AssertEquals("891", reconDec.US_SuretyCode);

			reconDec.IOROrgPK = importer2.PK;
			AssertEquals(PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter, reconDec.US_PaymentType);
			AssertEquals(YesNoDefaultList.Codes.No, reconDec.BrokerToPayIndicator);
			AssertEquals(ReconIssueCodeList.Codes.FTA, reconDec.US_IssueCode);
			AssertEquals("5869", reconDec.US_SchDEntry);
			AssertEquals(ReconciliationImportEntrySourceList.Codes.VirginIslands, reconDec.US_ImportEntrySource);
			AssertEquals("792", reconDec.US_SuretyCode);
		}

		public void TestIORDocumentProperties()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "Z1Z2Z3Z4";
			importer.OH_FullName = "TEST Importer";
			importer.MainAddress.OA_Address1 = "Main Address 1";
			importer.MainAddress.OA_Address2 = "Main Address 2";
			importer.MainAddress.OA_City = "CITYMAIN";
			importer.MainAddress.OA_State = "TG";
			importer.MainAddress.OA_PostCode = "9000";
			importer.MainAddress.OA_Phone = "002456789789";
			importer.MainAddress.OA_Fax = "029005006";
			importer.MainAddress.OA_Email = "test@test.com";

			var reconDec = new ReconDeclaration(Declaration);
			reconDec.JE_OH_Importer = importer.PK;
			AssertEquals(importer.PK, reconDec.IOROrgPK);

			var assertionMessage = "Importer document details should be from Main Address - ";
			AssertEquals(assertionMessage + "Phone", "002456789789", reconDec.ImporterOfRecordPhone);
			AssertEquals(assertionMessage + "Fax", "029005006", reconDec.ImporterOfRecordFax);
			AssertEquals(assertionMessage + "Email", "test@test.com", reconDec.ImporterOfRecordEmail);

			var impoterCustomsAddress = importer.Addresses.AddNew();
			impoterCustomsAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.CustomsAddressOfRecord);
			impoterCustomsAddress.OA_Address1 = "Customs Address 1";
			impoterCustomsAddress.OA_State = "TG";
			impoterCustomsAddress.OA_Phone = "3005006";
			impoterCustomsAddress.OA_Fax = "02040506";
			impoterCustomsAddress.OA_Email = "fsdfsfd@erwerw.com";

			assertionMessage = "Importer document details should be from Customs Address - ";
			AssertEquals(assertionMessage + "Phone", "3005006", reconDec.ImporterOfRecordPhone);
			AssertEquals(assertionMessage + "Fax", "02040506", reconDec.ImporterOfRecordFax);
			AssertEquals(assertionMessage + "Email", "fsdfsfd@erwerw.com", reconDec.ImporterOfRecordEmail);
		}

		public void TestDocManagerInfo()
		{
			var reconciliationDeclaration = (ReconDeclaration)GetNewBusinessObject();
			AssertEquals("IDocManagerSupport.DocManagerInfo", Declaration.DocManagerInfo, ((IDocManagerSupport)reconciliationDeclaration).DocManagerInfo);
		}

		public void TestIEDocsProviderMembers()
		{
			var reconciliationDeclaration = (ReconDeclaration)GetNewBusinessObject();
			AssertEquals("EDocsProviderSupporter type", typeof(JobInvoicingEDocsProviderSupporter), ((IEDocsProvider)reconciliationDeclaration).GetEDocsProviderSupporter().GetType());
		}

		public void TestIDocsAndCartageParentMembers()
		{
			var iDocsAndCartageParent = (IDocsAndCartageParent)GetNewBusinessObject();
			AssertEquals("RequiredDocumentsProvider", ((IDocsAndCartageParent)Declaration).RequiredDocumentsProvider, iDocsAndCartageParent.RequiredDocumentsProvider);
			AssertEquals("DocsAndCartageType", ((IDocsAndCartageParent)Declaration).DocsAndCartageType, iDocsAndCartageParent.DocsAndCartageType);
			AssertEquals("DocsAndCartageParentType", ((IDocsAndCartageParent)Declaration).DocsAndCartageParentType, iDocsAndCartageParent.DocsAndCartageParentType);
		}

		public void TestIEDocsPluginHostDeciderMembers()
		{
			var iEDocsPluginHostDecider = (IEDocsPluginHostDecider)GetNewBusinessObject();
			AssertEquals("HostBusinessEntity for Recon Declaration should be Wrapped Declaration", Declaration, iEDocsPluginHostDecider.HostBusinessEntity);
		}

		public void TestCustomsStatusWithDescription()
		{
			var reconDeclaration = (ReconDeclaration)GetNewBusinessObject();
			Declaration.JE_EntryStatus = ReconMessageStatusList.Codes.ClearReconOriginal;

			AssertEquals("Recon declaration Customs Status", ReconMessageStatusList.Codes.ClearReconOriginal, reconDeclaration.CustomsStatus);
			AssertEquals("Recon declaration Customs Status Description", ReconMessageStatusList.Descriptions.ClearReconOriginal, reconDeclaration.CustomsStatusDescription);

			Declaration.JE_EntryStatus = ReconMessageStatusList.Codes.ReconReplaceAcceptedWarnings;

			AssertEquals("Recon declaration Customs Status", ReconMessageStatusList.Codes.ReconReplaceAcceptedWarnings, reconDeclaration.CustomsStatus);
			AssertEquals("Recon declaration Customs Status Description", ReconMessageStatusList.Descriptions.ReconReplaceAcceptedWarnings, reconDeclaration.CustomsStatusDescription);
		}

		public void TestImporterAddressAndImporterDocumentDetails()
		{
			var organization = Factory.New<OrgHeader>();
			organization.FillWithValidTestData();
			organization.MainAddress.OA_Email = "wws@der.com";
			organization.MainAddress.OA_Phone = "0412456";
			organization.MainAddress.OA_Fax = "029005006";

			var contact = organization.Contacts.AddNew();
			contact.OC_ContactName = "Test Contact For Importer";
			contact.OC_Email = "Test@co.com";
			contact.OC_Phone = "02123456";
			contact.Documents.AddNew().OD_DocumentGroup = ContactType.Consignee.ToString();

			var reconDeclaration = (ReconDeclaration)GetNewBusinessObject();
			reconDeclaration.JE_OH_Importer = organization.PK;

			AssertEquals("Importer on Wrapped Declaration", organization.PK, reconDeclaration.ReconWrappedJobDeclaration.JE_OH_Importer);
			AssertNotNull(reconDeclaration.ImporterAddress);
			AssertEquals("Defauls Contact Type", ContactType.Consignee, reconDeclaration.ImporterAddress.DefaultContactType);
			AssertEquals("Importer Name", organization.OH_FullName, reconDeclaration.ImporterName);
			AssertEquals("Importer Contact Name", "Test Contact For Importer", reconDeclaration.ImporterContactName);
			AssertEquals("Importer Contact Phone", "02123456", reconDeclaration.ImporterContactPhone);
			AssertEquals("Importer Contact Email", "Test@co.com", reconDeclaration.ImporterContactEmail);

			AssertEquals("Importer Phone", "0412456", reconDeclaration.ImporterPhone);
			AssertEquals("Importer Fax", "029005006", reconDeclaration.ImporterFax);
			AssertEquals("Importer Email", "wws@der.com", reconDeclaration.ImporterEmail);

			var newAddress = organization.Addresses.AddNew();
			newAddress.OA_Email = "sdsdxfs@sdf.com";
			newAddress.OA_Phone = "123456789";
			newAddress.OA_Fax = "65432197";

			reconDeclaration.ImporterAddress.E2_OA_Address = newAddress.PK;
			AssertEquals("Importer Phone", "123456789", reconDeclaration.ImporterPhone);
			AssertEquals("Importer Fax", "65432197", reconDeclaration.ImporterFax);
			AssertEquals("Importer Email", "sdsdxfs@sdf.com", reconDeclaration.ImporterEmail);

			var organization2 = Factory.New<OrgHeader>();
			organization2.FillWithValidTestData();
			organization2.OH_FullName = "Test Org2";
			var contact2 = organization2.Contacts.AddNew();
			contact2.OC_ContactName = "Test Contact 2";
			contact2.OC_Email = "mail@mail.com";
			contact2.OC_Phone = "0278945612";
			contact2.Documents.AddNew().OD_DocumentGroup = ContactType.Consignee.ToString();

			reconDeclaration.JE_OH_Importer = organization2.PK;
			AssertEquals("Importer on Wrapped Declaration", organization2.PK, reconDeclaration.ReconWrappedJobDeclaration.JE_OH_Importer);
			AssertNotNull(reconDeclaration.ImporterAddress);
			AssertEquals("Defauls Contact Type", ContactType.Consignee, reconDeclaration.ImporterAddress.DefaultContactType);
			AssertEquals("Importer Name", "Test Org2", reconDeclaration.ImporterName);
			AssertEquals("Importer Contact Name", "Test Contact 2", reconDeclaration.ImporterContactName);
			AssertEquals("Importer Contact Phone", "0278945612", reconDeclaration.ImporterContactPhone);
			AssertEquals("Importer Contact Email", "mail@mail.com", reconDeclaration.ImporterContactEmail);
		}

		public void TestReconDocumentSupressSSNFromOtherDeclaratoinTypes()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "Exporter";
			importer.MainAddress.OA_Address1 = "Exporter Address";
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "999-99-9999", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();
			AssertEquals("ImporterOfRecordNumber, SSN only then registration number print empty.", string.Empty, reconDec.ImporterOfRecordNumber4Print);
		}

		public void TestReconEntryNumberFormatted()
		{
			var reconDeclaration = (ReconDeclaration)GetNewBusinessObject();
			AssertEquals("Empty Recon Entry Number formatted", ZString.Empty, reconDeclaration.ReconEntryNumberFormatted);
			Declaration.US_EntryFilerCode = "XJ5";
			AssertEquals("Only Entry Filer Code in Recon Entry Number formatted", "XJ5", reconDeclaration.ReconEntryNumberFormatted);

			reconDeclaration.ReconWrappedJobDeclaration.ActiveEntryHeaders[0].EntryNumber = "2002";
			AssertEquals("Recon Entry Number formatted", "XJ5-0000200-2", reconDeclaration.ReconEntryNumberFormatted);

			reconDeclaration.ReconWrappedJobDeclaration.ActiveEntryHeaders[0].EntryNumber = "00000345";
			AssertEquals("Recon Entry Number formatted", "XJ5-0000034-5", reconDeclaration.ReconEntryNumberFormatted);
		}

		public void TestServiceLevel_CS00172151()
		{
			var serviceLevelCode = "STD";
			var serviceLevel = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, serviceLevelCode);
			if (serviceLevel == null)
			{
				serviceLevel = Factory.NewWithValidTestData<RefServiceLevel>();
				serviceLevel.RS_Code = serviceLevelCode;
			}
			serviceLevel.RS_IsActive = false;
			Factory.Save();

			var reconDeclaration = new ReconDeclaration(Declaration);
			var declaration = reconDeclaration.ReconWrappedJobDeclaration;
			AssertEquals("Service Level should be defaulted", "STD", declaration.JE_RS_NKServiceLevel);
		}

		public void TestLoadingResourceStringsAndJobDeclaration()
		{
			Assertion.AssertNotNull(DataBoundResourceStrings.GetStringForTable(typeof(ReconDeclaration)));
			Assertion.AssertEquals("Customs Declaration", DataBoundResourceStrings.GetStringForTable(typeof(JobDeclaration)));

			Assertion.AssertNull(DataBoundResourceStrings.GetDataForProperty(typeof(ReconDeclaration), "NoSuchProperty"));
			Assertion.AssertEquals("Agents Reference", DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), "JE_AgentsReference").Caption);
		}

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent reconDeclaration = (ReconDeclaration)GetNewBusinessObject();
			Assert(reconDeclaration.AllowInvoiceDeletion);
		}

		public void TestIReconOriginalChargeParent()
		{
			var reconDec = new ReconDeclaration(Declaration);
			var chargeParent = reconDec as IReconOriginalChargeParent;

			var list = chargeParent.FeeAndChargeList;
			Assert(!list.ContainsCode(Core.Constants.USCustoms.FeeCodes.OtherExcise));
			Assert(!list.ContainsCode(Core.Constants.USCustoms.FeeCodes.Wines));
			Assert(!list.ContainsCode(Core.Constants.USCustoms.FeeCodes.Tobacco));
			Assert(!list.ContainsCode(Core.Constants.USCustoms.FeeCodes.DistilledSpirits));

			AssertEquals(Declaration, chargeParent.ParentAsBusinessObject);
			AssertNull(chargeParent.Tariff);
			AssertEquals(false, chargeParent.DefaultValueForOverridenForNewChild);
			Assert("MonthlyFiling", !chargeParent.MonthlyFiling);

			reconDec.US_R_IsNoChangeAgg = true;
			Assert(reconDec.US_R_IsNoChangeAgg);
			reconDec.AggregateRefundedFees.AddNew();
			AssertEquals("Aggregate Fees", 1, reconDec.AggregateRefundedFees.Count);
		}

		public void TestUS_R_IsNoChangeAgg()
		{
			var reconDec = new ReconDeclaration(Declaration);
			reconDec.US_IsAggregate = true;
			Assert("Do not default this flag to true.", !reconDec.US_R_IsNoChangeAgg);

			reconDec.US_R_IsNoChangeAgg = true;
			reconDec.US_IsAggregate = false;
			Assert("reset this flag to false when recon is no longer an aggregate", !reconDec.US_R_IsNoChangeAgg);
		}

		public void TestIDeclarationMembers()
		{
			var reconDec = new ReconDeclaration(Declaration);
			var iOR = Factory.NewWithValidTestData<OrgHeader>();
			reconDec.IOROrgPK = iOR.PK;

			reconDec.US_ClientBranchDesignation = "14";
			reconDec.US_PaymentDate = ZDateTime.Today.AddDays(2);
			reconDec.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(4);
			reconDec.US_EntryFilerCode = "SV9";
			reconDec.ReconEntry.GetEntry().EntryNumber = "50012348";
			reconDec.US_SchDEntry = "3910";
			reconDec.ReconWrappedJobDeclaration.JE_DeclarationReference = "B00160013";

			var iDeclaration = reconDec as IStatementLineDeclaration;

			AssertEquals("US_PSDAccepted is not relevant for recon", ZDateTime.Empty, iDeclaration.US_PSDAccepted);
			AssertEquals("US_ClientBranchDesignation", "14", iDeclaration.US_ClientBranchDesignation);
			AssertEquals("US_PaymentDate", ZDateTime.Today.AddDays(2), iDeclaration.US_PaymentDate);
			AssertEquals("US_PaperlessEntry", ZString.Empty, iDeclaration.US_PaperlessEntry);
			AssertEquals("US_PeriodicStatementMM is not relevant for recon", ZString.Empty, iDeclaration.US_PeriodicStatementMM);
			AssertEquals("US_PreliminaryStatementPrintDate", ZDateTime.Today.AddDays(4), iDeclaration.US_PreliminaryStatementPrintDate);
			AssertEquals("FormattedEntryNumber", "SV9-5001234-8", iDeclaration.FormattedEntryNumber);
			AssertEquals("ProcessingDistrictPort", "3910", iDeclaration.ProcessingDistrictPort);
			AssertEquals("JobNumber", "B00160013", iDeclaration.JE_DeclarationReference);
			AssertEquals("HasBeenWithdrawn", false, iDeclaration.HasEntryBeenWithdrawn);
			AssertEquals("US_PreparerDistrictPort", ZString.Empty, iDeclaration.US_PreparerDistrictPort);
			AssertEquals("US_PreparerOfficeCode", ZString.Empty, iDeclaration.US_PreparerOfficeCode);
			AssertEquals("IsACE", true, iDeclaration.IsACE);
			AssertEquals("BrokerReferenceNumber", "B00160013", iDeclaration.BrokerReferenceNumber);
			AssertEquals("EntryNumber", "50012348", iDeclaration.EntryNumber);
			AssertEquals("EntryFilerCode", "SV9", iDeclaration.EntryFilerCode);
			AssertEquals("IOR", iOR, iDeclaration.IOR);
			AssertEquals(CRLReleaseStatusList.Codes.NRT, iDeclaration.ReleaseStatus);
			AssertEquals(CRLReleaseStatusList.Descriptions.NRT, iDeclaration.ReleaseStatusDescription);
		}

		public void TestIRatingSupporterWithAdapterMembers()
		{
			var reconDec = new ReconDeclaration(Declaration);
			var supporter = reconDec as IRatingSupporterWithAdapter;
			AssertNotNull(supporter);
			AssertType<ReconDeclarationRatingAdapter>(supporter.RatingAdapter);
		}

		public void TestCodePropertyAttribute()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			Declaration.PopulateJE_DeclarationReferenceIfNeeded();
			var reconDec = new ReconDeclaration(Declaration);
			var bizObjCode = CodePropertyAttribute.CodeFromBusinessObject(reconDec);
			AssertEquals(" reconDec.JE_DeclarationReference should equal CodePropertyAttribute from BusinessObject", reconDec.JE_DeclarationReference, bizObjCode);
		}

		[TestDate(2008, 3, 25)]
		public void TestReconDeclarationShouldNotLoadOriginalDeclaration()
		{
			var bizObjs = new List<BusinessObject>();
			for (int i = 0; i < 10; i++)
			{
				var originalDec = Factory.New<JobDeclaration>();
				originalDec.JE_MessageType = JobMessageTypeList.Codes.Import;
				originalDec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				originalDec.US_EnableENS = true;
				originalDec.US_EntryFilerCode = "XJ5";
				originalDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

				var invoice = originalDec.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "INV1";
				invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				invoice.JZ_InvoiceAmount = 10000m;
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "2203.00.00 60";
				invoiceLine.US_UC_NKCountryOfOrigin = "NZ";
				invoiceLine.US_UC_NKCountryOfExport = "NZ";
				invoiceLine.JI_LinePrice = 10000m;
				invoiceLine.JI_CustomsQuantity = 15000m;

				originalDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				originalDec.ImportEntryNumber = "1234567" + i.ToString();
				bizObjs.Add(originalDec);
				bizObjs.Add(invoice);
				bizObjs.Add(invoiceLine);
				bizObjs.Add(originalDec.ActiveEntryHeaders.EntrySummaryEntry);
			}
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var reconDec = new ReconDeclaration(declaration);
			for (int i = 0; i < 10; i++)
			{
				var originalEntry = reconDec.OriginalEntries.AddNew();
				originalEntry.CH_OrigEntryReference = "XJ51234567" + i.ToString();
				AssertNotEquals("originalEntry.CH_CH_OriginalEntry", ZGuid.Empty, originalEntry.CH_CH_OriginalEntry);
			}
			new ReconImportEntryRetriever(reconDec).ImportLines();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var reconDecInDiffFactory = new ReconDeclaration(newFactory.Load<JobDeclaration>(declaration.PK));
			reconDecInDiffFactory.RunPreSaveValidationWithFetchHints();

			var dataSet = ((INeedDataSet)newFactory).Data;
			var tables = dataSet.Tables;
			AssertEquals("Declaration Count", 11, tables[JobDeclaration.Schema.TableName].Rows.Count);
			AssertEquals("Entry Count", 21, tables[CusEntryHeader.Schema.TableName].Rows.Count);
			AssertEquals("Invoice Count", 11, tables[JobComInvoiceHeader.Schema.TableName].Rows.Count);
			AssertEquals("Invoice Line Count", 10, tables[JobComInvoiceLine.Schema.TableName].Rows.Count);
			AssertEquals("reconDecInDiffFactory.OriginalEntries.Count", 10, reconDecInDiffFactory.OriginalEntries.Count);
			foreach (var bizObj in bizObjs)
			{
				AssertEquals(string.Format("BizObj ({0}) should not be loaded", bizObj.TableName), 0, newFactory.GetBizOsForPK(bizObj.PK.ToGuid()).Length);
			}

			foreach (ZPropertyInfo info in reconDecInDiffFactory.ZPropertyInfoHash)
			{
				var data = info.Value;// touch the property
			}
			foreach (ReconOriginalEntryHeader entry in reconDecInDiffFactory.OriginalEntries)
			{
				foreach (ZPropertyInfo info in entry.ZPropertyInfoHash)
				{
					var data = info.Value;// touch the property
				}
			}
			AssertEquals("Declaration Count", 11, tables[JobDeclaration.Schema.TableName].Rows.Count);
			AssertEquals("Entry Count", 21, tables[CusEntryHeader.Schema.TableName].Rows.Count);
			AssertEquals("Invoice Count", 11, tables[JobComInvoiceHeader.Schema.TableName].Rows.Count);
			AssertEquals("Invoice Line Count", 10, tables[JobComInvoiceLine.Schema.TableName].Rows.Count);
			foreach (var bizObj in bizObjs)
			{
				AssertEquals(string.Format("BizObj ({0}) should not be loaded", bizObj.TableName), 0, newFactory.GetBizOsForPK(bizObj.PK.ToGuid()).Length);
			}
			newFactory.Save();
			AssertEquals("Declaration Count", 11, tables[JobDeclaration.Schema.TableName].Rows.Count);
			AssertEquals("Entry Count", 21, tables[CusEntryHeader.Schema.TableName].Rows.Count);
			AssertEquals("Invoice Count", 11, tables[JobComInvoiceHeader.Schema.TableName].Rows.Count);
			AssertEquals("Invoice Line Count", 10, tables[JobComInvoiceLine.Schema.TableName].Rows.Count);
			foreach (var bizObj in bizObjs)
			{
				AssertEquals(string.Format("BizObj ({0}) should not be loaded", bizObj.TableName), 0, newFactory.GetBizOsForPK(bizObj.PK.ToGuid()).Length);
			}
		}

		public void TestCanCancel()
		{
			var reconDec = new ReconDeclaration(Declaration);
			var entry = reconDec.ReconEntry.GetEntry();
			var iCancellable = reconDec as ICancellable;

			AssertEquals("Can be cancelled", ZString.Empty, iCancellable.CanCancel());

			entry.CH_Status = ReconMessageStatusList.Codes.AwaitingReconOriginal;
			AssertEquals("Can not be cancelled, because awaiting response", ReconDeclaration.ReasonUnableToCancel, iCancellable.CanCancel());

			entry.CH_Status = ReconMessageStatusList.Codes.ClearReconOriginal;
			AssertEquals("Can not be cancelled, because has been lodged", ReconDeclaration.ReasonUnableToCancel, iCancellable.CanCancel());

			entry.CH_Status = ReconMessageStatusList.Codes.AwaitingReconReplace;
			AssertEquals("Can not be cancelled, because awaiting response", ReconDeclaration.ReasonUnableToCancel, iCancellable.CanCancel());

			entry.CH_Status = ReconMessageStatusList.Codes.ClearReconDelete;
			AssertEquals("Can be cancelled, because has been withdrawn", ZString.Empty, iCancellable.CanCancel());
		}

		[TestDate(2014, 04, 04)]
		public void TestRefreshTariff()
		{
			var reconDecl = new ReconDeclaration(Declaration);
			var invoiceHeader = reconDecl.Invoices.AddNew();

			var reconEntryHeader = reconDecl.OriginalEntries.AddNew();
			reconEntryHeader.CH_OrigEntryReference = "A1234";
			reconEntryHeader.US_R_DutyRateDate = ZDateTime.Today;
			invoiceHeader.US_CH_ReconEntry = reconEntryHeader.CH_PK;

			var entryLine = invoiceHeader.JobComInvoiceLines.AddNew();
			entryLine.JI_Calc_Invoice = reconEntryHeader.CH_OrigEntryReference;

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9802009123";
			tariff.UE_DateFrom = new ZDateTime(1990, 01, 01);
			tariff.UE_DateTo = new ZDateTime(2099, 12, 31);

			entryLine.JI_Tariff = tariff.UE_Tariff;
			entryLine.US_R_OrigTariff = tariff.UE_Tariff;
			entryLine.US_SupTariff = tariff.UE_Tariff;
			entryLine.US_R_OrigSupTariff = tariff.UE_Tariff;

			AssertEquals(ZString.Empty, entryLine.JI_CustomsUnitQty);
			AssertEquals(ZString.Empty, entryLine.US_R_OrigFirstUQ);
			AssertEquals(ZString.Empty, entryLine.US_SupUQ1);
			AssertEquals(ZString.Empty, entryLine.US_R_OrigSupUQ1);

			tariff.UE_Unit1 = "KG";
			reconDecl.RefreshTariff();

			AssertEquals("KG", entryLine.JI_CustomsUnitQty);
			AssertEquals("KG", entryLine.US_R_OrigFirstUQ);
			AssertEquals("KG", entryLine.US_SupUQ1);
			AssertEquals("KG", entryLine.US_R_OrigSupUQ1);

			tariff.UE_Unit1 = ZString.Empty;
			reconDecl.RefreshTariff();

			AssertEquals(ZString.Empty, entryLine.JI_CustomsUnitQty);
			AssertEquals(ZString.Empty, entryLine.US_R_OrigFirstUQ);
			AssertEquals(ZString.Empty, entryLine.US_SupUQ1);
			AssertEquals(ZString.Empty, entryLine.US_R_OrigSupUQ1);
		}

		public void TestProcessTaskLoadType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var reconDec = new ReconDeclaration(declaration);
			AssertEquals(false, declaration.IsInDatabase);
			AssertEquals(false, reconDec.IsInDatabase);

			var workitem = reconDec.WorkflowItems.AddNew();
			Factory.Save();

			var decLoaded = Factory.Load<JobDeclaration>(declaration.PK);
			var reLoadWorkItem = decLoaded.WorkflowItems.FindByPK(workitem.PK);
			AssertNotNull(reLoadWorkItem);
			AssertType<BaseJobDeclarationProcessTask<JobDeclaration>>(reLoadWorkItem);
		}

		public void TestApplicationCodeChange()
		{
			var reconDec = new ReconDeclaration(Declaration);
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			reconDec.AggregateRefundedFees.AddNew();
			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.RefundedFees.AddNew();
			AssertEquals("PreCondition", 1, reconDec.AggregateRefundedFees.Count);
			AssertEquals("PreCondition", 1, originalEntry.RefundedFees.Count);

			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals("refunded fee is cleared", 0, reconDec.AggregateRefundedFees.Count);
			AssertEquals("refunded fee is cleared", 0, originalEntry.RefundedFees.Count);

			var invoiceLine = originalEntry.Invoice.InvoiceLines.AddNew();
			invoiceLine.ReconRefundedFees.AddNew();
			AssertEquals("PreCondition", 1, invoiceLine.ReconRefundedFees.Count);

			originalEntry.US_PriorDisclosure = true;
			originalEntry.US_NAFTAClaimStat = true;
			originalEntry.US_ProtestStat = true;
			originalEntry.US_ProtestID = "AAA";
			originalEntry.US_PendingActionID = "BBB";
			originalEntry.US_PendingActionIDType = "C";

			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Assert(!originalEntry.US_PriorDisclosure);
			Assert(!originalEntry.US_NAFTAClaimStat);
			Assert(!originalEntry.US_ProtestStat);
			AssertEquals(ZString.Empty, originalEntry.US_ProtestID);
			AssertEquals(ZString.Empty, originalEntry.US_PendingActionID);
			AssertEquals(ZString.Empty, originalEntry.US_PendingActionIDType);
		}

		public void TestEntryModeChange()
		{
			var reconDec = new ReconDeclaration(Declaration);
			reconDec.ReconWrappedJobDeclaration.US_EntryMode = ZString.Empty;
			reconDec.ReconWrappedJobDeclaration.US_EntryMode = EntryModeList.Codes.RLF;
			AssertEquals(EntryModeList.Codes.RLF, reconDec.ReconWrappedJobDeclaration.US_EntryMode);
		}

		public void TestIDISHostImplementation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var reconDec = new ReconDeclaration(declaration);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var disHost = declaration as IUSDISHost;
			Assert("Show DIS button", disHost.ShowDISFeatures);
			Assert("Don't need to do merger for recon", !disHost.NeedToDoPreFormAction());
			Assert("Don't need to do merger for recon", !disHost.DoPreFormAction());
			AssertEquals(1, disHost.ErrorMessages.Count());

			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EntryFiler() { EntryFilerCode = "SV9" });
			AssertEquals(0, disHost.ErrorMessages.Count());
		}

		public void TestWorkflowTemplateCustomFieldForReconDeclaration()
		{
			var reconTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			reconTemplate.P0_ProcessType = "REC";

			var reconColumn = reconTemplate.GenCustomColumnDefinitions.AddNew();
			reconColumn.XC_Name = "Recon Field 1";
			reconColumn.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();

			var job = Factory.New<JobDeclaration>();
			var recon = new ReconDeclaration(job);
			var customBusinessObject = ((ICustomFieldProvider)recon).GetCustomBusinessObject() as IDynamicBusinessObject;
			AssertContainsExactElementsInAnyOrder(new[] { "__RECON FIELD 1__prop__ZString", "__RECON FIELD 1__prop__ZStringInfo" }, customBusinessObject.PropertyNames);
		}

		public void TestNoExceptionThrownWhenImporterChanged()
		{
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.OH_Code = "TEST1";

			var bondData1 = new CusBondDetailCollection(importer1);
			var oneBondData1 = bondData1.AddNew();
			oneBondData1.PW_ActivityCode = ActivityCodeList.Codes._1;
			oneBondData1.PW_BondAmount = 50000m;
			oneBondData1.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-3);
			oneBondData1.PW_BondNumber = "123456";
			oneBondData1.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			oneBondData1.PW_SuretyCode = "891";
			oneBondData1.PW_BondFiledPort = "3901";
			Factory.Save();

			var reconDec = new ReconDeclaration(Declaration);
			reconDec.JE_OH_Importer = importer1.PK;

			var exceptionError = "AddInfo properties [US_BondType, US_BondProducerAccNo] not in ReconDeclaration should not be set when the Declaration is a Recon.";
			AssertNoExceptionThrown($"There will be no {exceptionError} error message thrown", () => { Factory.Save(); });
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoExceptionThrownWhenSchDArrivalChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			var reconDec = new ReconDeclaration(declaration);
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Factory.Save();
			UpdateDeclarationAdditionalInfo(declaration.PK.ToGuid(), "IsHMFApplicable=Y");

			var newFactory = new BusinessObjectFactory();
			var declarationReload = newFactory.Load<JobDeclaration>(declaration.PK);

			var filePath = BaseSourcePath + @"Enterprise\Product\Operations\Customs\US\Business\Business.Test\Recon\ReconDeclaration\TestFiles\UniversalCopyTemplate_HMFApplicableCopy.xml";
			CopyTemplateTree copyTemplateTree;
			using (var reader = new StreamReader(filePath))
			{
				copyTemplateTree = (CopyTemplateTree)new XmlSerializer(typeof(CopyTemplateTree)).Deserialize(reader);
			}
			var reconDeclaration = new BusinessObjectCopyManagerForTest().Copy(declarationReload, copyTemplateTree).Object;
			var exceptionError = "AddInfo properties [US_IsHMFApplicable] not in ReconDeclaration should not be set when the Declaration is a Recon.";
			AssertNoExceptionThrown($"There will be no {exceptionError} error message thrown", () => { newFactory.Save(); });
		}

		void UpdateDeclarationAdditionalInfo(Guid declarationPK, string additionalInfo)
		{
			var updateSQL = @"
UPDATE dbo.JobDeclaration
SET
	JE_AddInfo = @additionalInfo,
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = @declarationPK";

			using (var command = Db.Connection.Command(updateSQL))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@additionalInfo", SqlDbType.Text, additionalInfo);
				command.ExecuteNonQuery();
			}
		}

		public void TestNoExceptionThrownWhenTransportModeOrContainerModeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			var reconDec = new ReconDeclaration(declaration);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = ContainerModeList.Codes.Containerized;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var reconDeclaration = declaration.Clone();
			var exceptionError = "AddInfo properties [US_Box29IncludeContainers] not in ReconDeclaration should not be set when the Declaration is a Recon.";
			AssertNoExceptionThrown($"There will be no {exceptionError} error message thrown", () => { Factory.Save(); });
		}

		public void TestNoExceptionThrownWhenEstimatedEntryDateChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			var reconDec = new ReconDeclaration(declaration);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EstimatedEntryDate = new ZDateTime(2024, 05, 25);
			var reconDeclaration = declaration.Clone();
			var exceptionError = "AddInfo properties [US_PaymentDueDate] not in ReconDeclaration should not be set when the Declaration is a Recon.";
			AssertNoExceptionThrown($"There will be no {exceptionError} error message thrown", () => { Factory.Save(); });
		}

		[ExpectNoExceptions]
		public void TestMatchesFilter()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.JE_GB = GlbBranch.CurrentBranch.PK;

			var query = new ZQuery(JobDeclarationSchema.JE_GB, GlbBranch.CurrentBranch.PK);
			AssertEquals(true, reconDeclaration.MatchesFilter(query));
		}

		protected override BusinessObject GetNewBusinessObject() => new ReconDeclaration(Declaration);

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());

		void AssertTotalChargeFeeAndDifference(ReconDeclaration reconDec, string codeType, string originalAmountFieldName, string reconAmountFieldName, string differenceAmountFieldName, decimal amount)
		{
			ReconOriginalEntryHeader entry1 = reconDec.OriginalEntries.AddNew();
			ReconOriginalEntryHeader entry2 = reconDec.OriginalEntries.AddNew();

			entry1.OriginalCharges.AddNew(codeType, amount);
			entry2.OriginalCharges.AddNew(codeType, amount * 2);
			entry1.ReconCharges.AddNew(codeType, amount * 4);
			entry2.ReconCharges.AddNew(codeType, amount * 8);

			AssertEquals("total original amount " + codeType, amount + amount * 2, reconDec[originalAmountFieldName]);
			AssertEquals("total recon amount " + codeType, amount * 4 + amount * 8, reconDec[reconAmountFieldName]);
			AssertEquals("total difference amount " + codeType, (amount * 4 + amount * 8) - (amount + amount * 2), reconDec[differenceAmountFieldName]);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
