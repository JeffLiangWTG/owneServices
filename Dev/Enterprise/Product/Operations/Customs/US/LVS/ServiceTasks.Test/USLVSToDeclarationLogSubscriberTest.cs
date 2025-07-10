using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Customs.US.LVS.ServiceTasks.Testing
{
	[TestedType(typeof(USLVSToDeclarationLogSubscriber))]
	public class USLVSToDeclarationLogSubscriberTest : LogSubscriberTest<USLVSToDeclarationLogSubscriber>
	{
		public void TestUSLVSToDeclarationLogSubscriber_WhenTCILogIsAdded_CreateStandAloneDeclaration()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_JobNumber = "SEC00000001";
			var consignment = clearance.CusUSLVConsignments.AddNew();
			clearance.Logs.AddNew(AutoEvents.TransferToCustomsImportsDec, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.ReferenceNumber, consignment.PK.ToString()));
			Factory.Save();

			RunLogWalkerCycleForTest();

			consignment.Reload();
			AssertNotEquals("Declaration should be created for consignment", ZString.Empty, consignment.CE_EntryLineReference);
			var expectedLog = string.Format(
				"[USLVSToDeclarationLogSubscriber] {0} TCI Event processed for consignment. Stand Alone Declaration {1} has been created.",
				clearance.ULH_JobNumber, consignment.CE_EntryLineReference);
			AssertCollectionContains(expectedLog, NotifiedEventList);
		}

		public void TestUSLVSToDeclarationLogSubscriber_WhenTCILogIsCancelled_NoStandAloneDeclarationCreated()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_JobNumber = "SEC00000001";
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var log = clearance.Logs.AddNew(AutoEvents.TransferToCustomsImportsDec, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.ReferenceNumber, consignment.PK.ToString()));
			log.Cancel();
			Factory.Save();

			RunLogWalkerCycleForTest();
			consignment.Reload();
			AssertEquals("No declaration should be created for consignment", ZString.Empty, consignment.CE_EntryLineReference);
		}

		public void TestStandAloneDeclarationCreatedInUSBranch()
		{
			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.GC_RN_NKCountryCode = "US";
			var usBranch = usCompany.Branches.AddNew();
			usBranch.FillWithValidTestData();
			usBranch.GB_RL_NKHomePort = "USCHI";

			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_JobNumber = "SEC00000001";
			clearance.ULH_GB = usBranch.PK;
			var consignment = clearance.CusUSLVConsignments.AddNew();
			clearance.Logs.AddNew(AutoEvents.TransferToCustomsImportsDec, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.ReferenceNumber, consignment.PK.ToString()));
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				RunLogWalkerCycleForTest();
			}

			consignment.Reload();
			AssertNotEquals(ZString.Empty, consignment.CE_EntryLineReference);

			var declaration = Factory.LoadTop1<Integration.Customs.IBaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, consignment.CE_EntryLineReference));
			AssertEquals(usBranch.PK, declaration.JE_GB);
		}

		public void TestUSLVSToDeclarationLogSubscriber_WhenTCILogIsAddedCombined_CreateStandAloneDeclaration()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_JobNumber = "SEC00000001";
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_ConvertAction = ULBConvertActionList.Codes.Combined;
			clearance.Logs.AddNew(AutoEvents.TransferToCustomsImportsDec, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Reason, LVSConstants.ConvertToDeclarationReason.CombineConsignments));
			Factory.Save();

			RunLogWalkerCycleForTest();

			consignment.Reload();
			AssertNotEquals("Declaration should be created for consignment", ZString.Empty, consignment.CE_EntryLineReference);
			var expectedLog = string.Format(
				"[USLVSToDeclarationLogSubscriber] {0} TCI Event processed for combined consignments. Stand Alone Declaration {1} has been created.",
				clearance.ULH_JobNumber, consignment.CE_EntryLineReference);
			AssertCollectionContains(expectedLog, NotifiedEventList);
		}

		public void TestUSLVSToDeclarationLogSubsriber_WhenTCILogIsAddedCombined_CancelsLogAfterProcessing()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_JobNumber = "SEC00000001";
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_ConvertAction = ULBConvertActionList.Codes.Combined;
			clearance.Logs.AddNew(AutoEvents.TransferToCustomsImportsDec, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Reason, LVSConstants.ConvertToDeclarationReason.CombineConsignments));
			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Reference, "|RES=Consignments combined");

			AssertNull("precondition", Factory.LoadTop1<StmALog>(query));

			RunLogWalkerCycleForTest();

			var reloadedLog = new BusinessObjectFactory().LoadTop1<StmALog>(query);
			AssertNotNull("log reference updated after processing", reloadedLog);
		}
	}
}
