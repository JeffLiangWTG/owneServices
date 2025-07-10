using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAdHocServiceJob))]
	public class WhsAdHocServiceJobTest : EnterpriseBusinessObjectTestCase
	{
		#region INumberFountainConsumer

		public void TestINumberFountainConsumer()
		{
			var serviceJob = Factory.New<WhsAdHocServiceJob>();
			AssertEquals("WhsAdHocServiceJob uses correct Fountain.", Env.NumberFountains.WorkItemNo, ((INumberFountainConsumer)serviceJob).Fountain);
			serviceJob.WSJ_JobNumber = "WI01";
			AssertEquals("ID refers to correct Field.", "WI01", ((INumberFountainConsumer)serviceJob).ID);
			((INumberFountainConsumer)serviceJob).ID = "WI02";
			AssertEquals("ID refers to correct Field.", "WI02", serviceJob.WSJ_JobNumber);
		}

		public void TestINumberFountainConsumer_SavingSetsJobID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			AssertEquals("Precondition: Job Number is empty.", "", serviceJob.WSJ_JobNumber);
			Factory.Save();
			AssertNotEquals("Job Number should not be empty.", "", serviceJob.WSJ_JobNumber);
			AssertEquals("Job Number was set correctly.", "WI00000001", serviceJob.WSJ_JobNumber);
		}

		#endregion

		#region TestWSJ_WW_Whs_FiltersWarehouses

		#region TestWSJ_WW_Whs_FiltersInactiveWarehouses

		public void TestWSJ_WW_Whs_FiltersInactiveWarehouses()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var warehouse1 = Helper.CreateWarehouse("W1");
			warehouse1.WW_IsActive = false;
			warehouse1.WW_GB_RelatedCompanyBranch = branch.PK;

			var warehouse2 = Helper.CreateWarehouse("W2");
			warehouse2.WW_IsActive = true;
			warehouse2.WW_GB_RelatedCompanyBranch = branch.PK;

			var warehouse3 = Helper.CreateWarehouse("W3");
			warehouse3.WW_IsActive = false;
			warehouse3.WW_GB_RelatedCompanyBranch = branch.PK;

			var job = Helper.CreateWhsAdHocServiceJob(warehouse2, Helper.CreateClient(), ZDateTime.Today);
			Factory.Save();

			var newFactory = NewFactory();
			var job_InNewFactory = newFactory.Load<WhsAdHocServiceJob>(job.PK);

			AssertEquals("Should have loaded the active warehouse.", warehouse2.PK, job_InNewFactory.WSJ_WW_Whs);
		}

		#endregion

		#region TestWSJ_WW_Whs_FiltersVirtualWarehouses

		public void TestWJS_WW_Whs_FiltersVirtualWarehouses()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var warehouse1 = Helper.CreateWarehouse("W1");
			warehouse1.WW_IsVirtualWarehouse = true;
			warehouse1.WW_GB_RelatedCompanyBranch = branch.PK;

			var warehouse2 = Helper.CreateWarehouse("W2");
			warehouse2.WW_IsVirtualWarehouse = false;
			warehouse2.WW_GB_RelatedCompanyBranch = branch.PK;

			var warehouse3 = Helper.CreateWarehouse("W3");
			warehouse3.WW_IsVirtualWarehouse = true;
			warehouse3.WW_GB_RelatedCompanyBranch = branch.PK;

			var job = Helper.CreateWhsAdHocServiceJob(warehouse2, Helper.CreateClient(), ZDateTime.Today);
			Factory.Save();

			var newFactory = NewFactory();
			var job_InNewFactory = newFactory.Load<WhsAdHocServiceJob>(job.PK);

			AssertEquals("Should have loaded the non-virtual warehouse.", warehouse2.PK, job_InNewFactory.WSJ_WW_Whs);
		}

		#endregion

		#endregion

		#region Properties

		#region TestWSJ_WW_Whs

		public void TestWSJ_WW_Whs()
		{
			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			AssertEquals("WSJ_WW_Whs should be empty.", true, adHocServiceJob.WSJ_WW_Whs.IsEmpty);

			var refreshBindingCalled = 0;
			adHocServiceJob.WSJ_WW_WhsInfo.ValueChanged += (s, e) => refreshBindingCalled++;

			var warehouse = Helper.CreateWarehouse("Whs");
			var client = Helper.CreateClient();
			AssertEquals(true, warehouse.WW_GB_RelatedCompanyBranch.IsValid);

			adHocServiceJob.WSJ_WW_Whs = warehouse.PK;
			adHocServiceJob.WSJ_OH_Client = client.PK;
			Factory.Save();
			AssertEquals("WSJ_WW_Whs should be equal to warehouse.PK.", warehouse.PK, adHocServiceJob.WSJ_WW_Whs);
			AssertEquals("Warehouse should be warehouse.", warehouse, adHocServiceJob.Warehouse);
			AssertEquals("RefreshBinding called on WarehousePKInfo.", 1, refreshBindingCalled);
		}

		public void TestWSJ_WW_Whs_TracksWarehouseNotBranch()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var branch = Helper.CreateGlbBranch("BR1");
			var address = OrgAddress.New(Factory);
			address.Address1 = "TEST";
			address.OA_OH = data.Org1.PK;
			var whs1 = Helper.CreateWarehouse("WH1", address, branch);
			var whs2 = Helper.CreateWarehouse("WH2", address, branch);
			whs1.WW_IsActive = true;
			whs2.WW_IsActive = false;
			Factory.Save();

			var adHocServiceJob = Helper.CreateWhsAdHocServiceJob(whs1, data.Org1, ZDateTime.Today);
			AssertEquals("adHocServiceJob should be on WH1", whs1.PK, adHocServiceJob.WSJ_WW_Whs);

			whs1.WW_IsActive = false;
			whs2.WW_IsActive = true;
			Factory.Save();

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var adHocServiceJobReload = otherFactory.Load<WhsAdHocServiceJob>(adHocServiceJob.PK);
			AssertEquals("adHocServiceJob should still be on WH1", whs1.WW_WarehouseCode, adHocServiceJobReload.Warehouse.WW_WarehouseCode);
		}

		#endregion

		#region TestClientPK

		public void TestWSJ_OH_Client()
		{
			var client = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("Warehouse");
			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			AssertEquals("WSJ_OH_Client should be empty.", true, adHocServiceJob.WSJ_OH_Client.IsEmpty);

			adHocServiceJob.WSJ_OH_Client = client.PK;
			adHocServiceJob.WSJ_WW_Whs = whs.PK;
			AssertEquals("WSJ_OH_Client should be equal to client.PK.", client.PK, adHocServiceJob.WSJ_OH_Client);
			AssertEquals("Client should be client.", client, adHocServiceJob.Client);
			AssertNotNull("JH_OA_LocalChargesAddress should not be null.", adHocServiceJob.JobHeader.JH_OA_LocalChargesAddr);

			Factory.Save();
			AssertEquals("WSJ_OH_Client should be equal to client.PK.", client.PK, adHocServiceJob.WSJ_OH_Client);
			AssertEquals("Client should be client.", client, adHocServiceJob.Client);
		}

		public void TestWSJ_OH_Client_ValidateWSJ_CustomerReference()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			adhocServiceJob.WSJ_CustomerReference = "ABC";

			Factory.Save();

			var adhocServiceJob2 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			AssertNoErrors("Precondition", adhocServiceJob2.WSJ_CustomerReferenceInfo);

			adhocServiceJob2.WSJ_CustomerReference = "ABC";
			AssertHasError(adhocServiceJob2.WSJ_CustomerReferenceInfo, "Customer Reference must be unique per Client.");

			var client2 = Helper.CreateClient();
			adhocServiceJob2.WSJ_OH_Client = client2.PK;
			adhocServiceJob2.WSJ_CustomerReference = "ABC";
			AssertNoErrors(adhocServiceJob2.WSJ_CustomerReferenceInfo);

			adhocServiceJob2.Delete();
		}

		#endregion

		#region TestWSJ_CustomerReference

		public void TestWSJ_CustomerReference()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			adHocServiceJob.WSJ_OH_Client = client.PK;
			adHocServiceJob.WSJ_WW_Whs = whs.PK;
			AssertEquals("WSJ_CustomerReference should be empty.", ZString.Empty, adHocServiceJob.WSJ_CustomerReference);

			var refreshBindingCalled = 0;
			adHocServiceJob.WSJ_CustomerReferenceInfo.ValueChanged += (s, e) => refreshBindingCalled++;

			adHocServiceJob.WSJ_CustomerReference = "Ref1";
			AssertEquals("WSJ_CustomerReference should be Ref1.", "Ref1", adHocServiceJob.WSJ_CustomerReference);
			AssertEquals("RefreshBinding called on WSJ_CustomerReferenceInfo.", 1, refreshBindingCalled);

			Factory.Save();
			AssertEquals("WSJ_CustomerReference should still be Ref1.", "Ref1", adHocServiceJob.WSJ_CustomerReference);
		}

		[ExpectException(typeof(MaxLengthExceededException))]
		public void TestCustomerReferenceNumberSetter_ValueIsMoreThan50Chars_ThrowsMaxLengthExceededExpection()
		{
			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			adHocServiceJob.WSJ_CustomerReference = "Valid Length Reference";
			AssertNoErrors("No error expected when Customer Reference No. of valid length is set", adHocServiceJob.WSJ_CustomerReferenceInfo);

			try
			{
				adHocServiceJob.WSJ_CustomerReference = "Invalid Length Reference 1234567890 1234567890 1234567890";
			}
			catch (MaxLengthExceededException)
			{
				ErrorReporter.Clear();
				throw;
			}
		}

		#endregion

		#region TestBillingDate

		public void TestBillingDate()
		{
			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			AssertEquals("Billing Date should be defaulted to Today.", ZDateTime.Today, adHocServiceJob.BillingDate);

			var refreshBindingCalled = 0;
			adHocServiceJob.BillingDateInfo.ValueChanged += (s, e) => refreshBindingCalled++;

			var twoDaysLater = ZDateTime.Today.AddDays(2);
			adHocServiceJob.BillingDate = twoDaysLater;
			AssertEquals("Billing Date", twoDaysLater, adHocServiceJob.BillingDate);
			AssertEquals("RefreshBinding called on BillingDateInfo.", 1, refreshBindingCalled);

			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			adHocServiceJob.WSJ_OH_Client = client.PK;
			adHocServiceJob.WSJ_WW_Whs = whs.PK;

			Factory.Save();
			AssertEquals("Billing Date", twoDaysLater, adHocServiceJob.BillingDate);
		}

		#endregion

		#region TestIsFinalising

		public void TestIsFinalising()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adHocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			Factory.Save();

			var isFinalising = adHocServiceJob.IsFinalising;
			AssertEquals(false, isFinalising);

			ZPropertyValueChangedEventHandler handler = (property, oldValue) => { isFinalising = adHocServiceJob.IsFinalising; };
			adHocServiceJob.PropertyValueChanged += handler;

			AssertEquals("Should Finalise Ad Hoc Service Job.", true, adHocServiceJob.FinaliseAdHocServiceJob(Notify));
			AssertEquals("Should Finalise Ad Hoc Service Job.", true, adHocServiceJob.IsFinalised);
			AssertEquals("Flag changes to true when Finalising.", true, isFinalising);
			AssertEquals("Flag reset to false after Finalising.", false, adHocServiceJob.IsFinalising);
			adHocServiceJob.PropertyValueChanged -= handler;
		}

		#endregion

		#region TestIsFinalised

		public void TestIsFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			AssertEquals("IsFinalised should be false", false, adhocServiceJob.IsFinalised);

			Factory.Save();
			AssertEquals("IsFinalised should still be false", false, adhocServiceJob.IsFinalised);

			adhocServiceJob.WSJ_IsFinalised = true;
			AssertEquals("IsFinalised should be true", true, adhocServiceJob.IsFinalised);
		}

		#endregion

		#region TestStatus

		public void TestStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			AssertEquals("Status should be 'New Entry'", "New Entry", adhocServiceJob.Status);

			Factory.Save();
			AssertEquals("Status should be 'Entered'", "Entered", adhocServiceJob.Status);

			adhocServiceJob.WSJ_IsFinalised = true;
			AssertEquals("Status should be 'Finalized'", "Finalized", adhocServiceJob.Status);
		}

		#endregion

		#endregion

		#region TestJobHeader

		public void TestJobHeader()
		{
			var adhocServiceJob = Factory.New<WhsAdHocServiceJob>();
			AssertNotNull("JobHeader should not be null.", adhocServiceJob.JobHeader);

			adhocServiceJob.Delete();
		}

		public void TestJobHeader_IsCreatedWithMutex()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var adhocServiceJob = Factory.New<WhsAdHocServiceJob>();
			adhocServiceJob.WSJ_OH_Client = client.PK;
			adhocServiceJob.WSJ_WW_Whs = whs.PK;
			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				adhocServiceJob.WSJ_CustomerReference = "testing string";
				Factory.Save();
			});
		}

		public void TestJobHeader_Delete()
		{
			var adhocServiceJob = Factory.New<WhsAdHocServiceJob>();

			AssertNoExceptionThrown(() =>
			{
				adhocServiceJob.Delete();
			});
		}

		public void TestJobHeader_CreateOnRunPreSaveValidation()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			adHocServiceJob.WSJ_OH_Client = client.PK;
			adHocServiceJob.WSJ_WW_Whs = whs.PK;

			adHocServiceJob.RunPreSaveValidation();
			Factory.Save();
			// Need to factory load because calling JobHeader property of bizo will create the job header if it doesn't already exist
			var jobHeaders = Factory.Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, adHocServiceJob.PK));
			AssertEquals("JobHeader should be created during RunPreSaveValidation of ad hoc service job", 1, jobHeaders.Length);
		}

		#endregion

		#region TestJobHeader_LocalChargesAddr

		public void TestJobHeader_LocalChargesAddr()
		{
			var client = Helper.CreateClient("C1");
			var otherAddress = client.Addresses.AddNew();
			otherAddress.Address1 = "TEST";
			var whs = Helper.CreateWarehouse("Warehouse");
			var adHocServiceJob = Helper.CreateWhsAdHocServiceJob(whs, client, ZDateTime.Today);
			AssertEquals("Precondition.", client, adHocServiceJob.Client);
			AssertNotEquals("Precondition - default address", otherAddress.PK, adHocServiceJob.JobHeader.JH_OA_LocalChargesAddr);

			adHocServiceJob.JobHeader.JH_OA_LocalChargesAddr = otherAddress.PK;
			AssertEquals("Should not back to default.", otherAddress.PK, adHocServiceJob.JobHeader.JH_OA_LocalChargesAddr);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertEquals("Should not back to default.", otherAddress.PK, newFactory.Load<WhsAdHocServiceJob>(adHocServiceJob.PK).JobHeader.JH_OA_LocalChargesAddr);
		}

		#endregion

		#region TestWSJ_OH_Client_LocalChargesAddr

		public void TestWSJ_OH_Client_LocalChargesAddr()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var otherAddress = client1.Addresses.AddNew();
			otherAddress.Address1 = "TEST";
			var whs = Helper.CreateWarehouse("Warehouse");
			var adHocServiceJob = Helper.CreateWhsAdHocServiceJob(whs, client1, ZDateTime.Today);
			AssertEquals("Precondition.", client1, adHocServiceJob.Client);
			AssertNotEquals("Precondition - default address", otherAddress.PK, adHocServiceJob.JobHeader.JH_OA_LocalChargesAddr);

			adHocServiceJob.JobHeader.JH_OA_LocalChargesAddr = otherAddress.PK;
			AssertEquals("Should not back to default.", otherAddress.PK, adHocServiceJob.JobHeader.JH_OA_LocalChargesAddr);

			adHocServiceJob.WSJ_OH_Client = client1.PK;
			AssertEquals("Same client should not change the JobHeader address.", otherAddress.PK, adHocServiceJob.JobHeader.JH_OA_LocalChargesAddr);

			adHocServiceJob.WSJ_OH_Client = client2.PK;
			AssertEquals("JobHeader address should not default if already set.", otherAddress.PK, adHocServiceJob.JobHeader.JH_OA_LocalChargesAddr);

			adHocServiceJob.JobHeader.JH_OA_LocalChargesAddr = ZGuid.Empty;
			adHocServiceJob.WSJ_OH_Client = client2.PK;
			AssertEquals("JobHeader address should default if address is empty.", client2.MainAddress.PK, adHocServiceJob.JobHeader.JH_OA_LocalChargesAddr);

			adHocServiceJob.Delete();
		}

		#endregion

		#region TestLookups

		public void TestLookups()
		{
			var adhocServiceJob = Factory.New<WhsAdHocServiceJob>();
			AssertNotNull(adhocServiceJob.Lookups);
			AssertEquals(typeof(WhsAdHocServiceJobLookups), adhocServiceJob.Lookups.GetType());
		}

		#endregion

		#region TestValidation

		public void TestValidation()
		{
			var adhocServiceJob = Factory.New<WhsAdHocServiceJob>();
			AssertNotNull(adhocServiceJob.Validation);
			AssertEquals(typeof(WhsAdHocServiceJobValidation), adhocServiceJob.Validation.GetType());
		}

		#endregion

		#region TestReadOnly

		public void TestAdHocServiceJob_StandardReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adHocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			AssertEquals("AdHocServiceJob is not finalised, ReadOnly should be false.", false, adHocServiceJob.ReadOnly);

			Factory.Save();
			adHocServiceJob.FinaliseAdHocServiceJob(Notify);
			AssertEquals("Should Finalise Ad Hoc Service Job.", true, adHocServiceJob.IsFinalised);

			AssertEquals("AdHocServiceJob is finalised, it should be ReadOnly.", true, adHocServiceJob.ReadOnly);
		}

		public void TestStandardReadOnly_WSJ_OH_ClientInfo()
		{
			TestStandardReadOnly(a => a.WSJ_OH_ClientInfo);
		}

		public void TestStandardReadOnly_WSJ_WW_WhsInfo()
		{
			TestStandardReadOnly(a => a.WSJ_WW_WhsInfo);
		}
		public void TestStandardReadOnly_BillingDateInfo()
		{
			TestStandardReadOnly(a => a.BillingDateInfo);
		}

		public void TestStandardReadOnly_WSJ_CustomerReferenceInfo()
		{
			TestStandardReadOnly(a => a.WSJ_CustomerReferenceInfo);
		}

		void TestStandardReadOnly(Func<WhsAdHocServiceJob, ZPropertyInfo> getInfo)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adHocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);

			AssertEquals("AdHocServiceJob is not finalised, property ReadOnly should be false.", false, getInfo(adHocServiceJob).ReadOnly);

			Factory.Save();
			adHocServiceJob.FinaliseAdHocServiceJob(Notify);
			AssertEquals("Should Finalise Ad Hoc Service Job.", true, adHocServiceJob.IsFinalised);

			AssertEquals("AdHocServiceJob is finalised, property should be ReadOnly.", true, getInfo(adHocServiceJob).ReadOnly);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			AssertEquals("Warehouse Ad Hoc Service Job", adhocServiceJob.HumanReadableName);
			Factory.Save();
			AssertEquals("Warehouse Ad Hoc Service Job WI00000001", adhocServiceJob.HumanReadableName);
		}

		#endregion

		#region TestSetDefaultValues

		public void TestSetDefaultValues()
		{
			var adHocServiceJob = Factory.NewWithValidTestData<WhsAdHocServiceJob>();
			AssertEquals(ZDateTime.Today, adHocServiceJob.BillingDate);
		}

		#endregion

		#region TestJobNumber

		public void TestJobNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			AssertEquals("Precondition: WSJ_JobNumber should be empty.", ZString.Empty, adhocServiceJob.WSJ_JobNumber);

			Factory.Save();
			AssertEquals("Should have set Job ID.", "WI00000001", adhocServiceJob.WSJ_JobNumber);

			adhocServiceJob.BillingDate = ZDateTime.Today.AddDays(1);
			Factory.Save();
			AssertEquals("Should *not* change Job ID.", "WI00000001", adhocServiceJob.WSJ_JobNumber);
			AssertEquals("WI00000001", ((IJobNumber)adhocServiceJob).JobNumber);
		}

		public void TestJobNumber_CreatingMultipleJobs()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var adhocServiceJob1 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			var adhocServiceJob2 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			AssertEquals("Precondition: WSJ_JobNumber should be empty.", ZString.Empty, adhocServiceJob1.WSJ_JobNumber);
			AssertEquals("Precondition: WSJ_JobNumber should be empty.", ZString.Empty, adhocServiceJob2.WSJ_JobNumber);

			Factory.Save();
			AssertEquals("Should have set Job ID.", "WI00000001", adhocServiceJob1.WSJ_JobNumber);
			AssertEquals("Should have set Job ID.", "WI00000002", adhocServiceJob2.WSJ_JobNumber);
		}

		#endregion

		#region TestFinaliseAdHocServiceJob

		#region TestFinaliseAdHocServiceJob

		public void TestFinaliseAdHocServiceJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adHocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);

			AssertEquals("Should not Finalise Ad Hoc Service Job if there are changes.", false, adHocServiceJob.FinaliseAdHocServiceJob(Notify));
			AssertEquals("Should not Finalise Ad Hoc Service Job if there are changes.", false, adHocServiceJob.IsFinalised);
			AssertEquals("Should not Finalise Ad Hoc Service Job if there are changes.", "Save all changes before Finalizing this Ad Hoc Service Job.\r\n", Notify.AsString);

			Factory.Save();

			Notify.Clear();
			Notify.DefaultResponse = false;
			AssertEquals("Should not Finalise Ad Hoc Service Job if user said no.", false, adHocServiceJob.FinaliseAdHocServiceJob(Notify));
			AssertEquals("Should not Finalise Ad Hoc Service Job if user said no.", false, adHocServiceJob.IsFinalised);

			var queryUserArgs = (DefaultableQueryUserEventArgs)Notify.LastQueryUserEventArgs;
			AssertEquals("User query Caption should be correct.", "Finalize Ad Hoc Service Job", queryUserArgs.Caption);
			AssertEquals("User query Message should be correct.",
@"On finalization, this Ad Hoc Service Job will become read-only so that it cannot be modified.
This finalization process cannot be undone once saved.

Do you wish to finalize this Ad Hoc Service Job?", queryUserArgs.Message);
			AssertEquals("User query Buttons should be correct.", ZMessageBoxButtons.YesNo, queryUserArgs.Context.Buttons);
			AssertContainsExactElementsInAnyOrder("User query Results Not To Save should be correct.", new[] { ZDialogResult.No }, queryUserArgs.Context.DialogResultsToNotSave);

			Notify.Clear();
			Notify.DefaultResponse = true;
			AssertEquals("Should Finalise Ad Hoc Service Job.", true, adHocServiceJob.FinaliseAdHocServiceJob(Notify));
			AssertEquals("Should Finalise Ad Hoc Service Job.", true, adHocServiceJob.IsFinalised);
			AssertEquals("Ad Hoc Service Job should be Read Only if finalised.", true, adHocServiceJob.ReadOnly);
			AssertEquals("Services ReadOnly", true, adHocServiceJob.Services.ReadOnly);
			AssertNull("No Error Message should have been made.", Notify.LastEvent);
			AssertEquals("Should have added a Finalised Event.", 1, adHocServiceJob.Logs.Find(
				l => l.SL_SE_NKEvent == Events.ItemDocumentJobFinalisedCode && l.SL_Reference == "Ad Hoc Service Job").Count());

			Factory.Save();
			Notify.Clear();
			AssertEquals("Ad Hoc Service Job should be Read Only if finalised.", true, adHocServiceJob.ReadOnly);
			AssertEquals("Services ReadOnly", true, adHocServiceJob.Services.ReadOnly);
			AssertEquals("Should not re-finalise the Ad Hoc Service Job if it is already finalised.", false, adHocServiceJob.FinaliseAdHocServiceJob(Notify));
			AssertEquals("Should not re-finalise the Ad Hoc Service Job if it is already finalised.", "Ad Hoc Service Job already Finalized.\r\n", Notify.AsString);
		}

		#endregion

		#region TestFinaliseAdHocServiceJob_GivesPromptWithDefaultableQueryEventArgs

		public void TestFinaliseAdHocServiceJob_GivesPromptWithDefaultableQueryEventArgs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adHocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			Factory.Save();

			AssertEquals("Precondition: adhoc service job has no errors", false, adHocServiceJob.HasErrors);
			AssertEquals("Precondition: adhoc service job is unfinalized", false, adHocServiceJob.IsFinalised);

			Notify.PreQueryUser += (sender, e) =>
			{
				if (e is DefaultableQueryUserEventArgs args)
				{
					AssertEquals("Precondition: default response correct", false, args.Response);
					args.Response = true;
				}
			};
			adHocServiceJob.FinaliseAdHocServiceJob(Notify);

			var lastQueryEventArgs = (DefaultableQueryUserEventArgs)Notify.LastQueryUserEventArgs;
			AssertEquals("Postcondition: correct eventArgs type", true, lastQueryEventArgs is DefaultableQueryUserEventArgs);
			AssertEquals("Postcondition: correct notification message",
@"On finalization, this Ad Hoc Service Job will become read-only so that it cannot be modified.
This finalization process cannot be undone once saved.

Do you wish to finalize this Ad Hoc Service Job?", lastQueryEventArgs.Message);

			AssertEquals("Postcondition: correct response", true, lastQueryEventArgs.Response);
			AssertEquals("Postcondition: correct notification caption", "Finalize Ad Hoc Service Job", lastQueryEventArgs.Caption);
			AssertEquals("Postcondition: adhoc service job is finalized", true, adHocServiceJob.IsFinalised);
			AssertEquals("Postcondition: adhoc service job has no errors", false, adHocServiceJob.HasErrors);
		}

		#endregion

		#region TestFinaliseAdHocServiceJob_WhenThereAreIncompleteServices

		public void TestFinaliseAdHocServiceJob_WhenThereAreIncompleteServices()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adHocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);

			var service1 = Factory.New<WhsJobService>();
			service1.ES_ParentID = adHocServiceJob.PK;
			service1.ES_ParentTableCode = adHocServiceJob.TablePrefix;

			var service2 = Factory.New<WhsJobService>();
			service2.ES_ParentID = adHocServiceJob.PK;
			service2.ES_ParentTableCode = adHocServiceJob.TablePrefix;

			AssertEquals("Collection was not loaded and/or the relationship filter is incorrect.", 2, adHocServiceJob.Services.Count);

			Factory.Save();
			Notify.Clear();
			Notify.DefaultResponse = true;
			AssertEquals("Should not Finalise Ad Hoc Service Job if there are incomplete services.", false, adHocServiceJob.FinaliseAdHocServiceJob(Notify));
			AssertEquals("Should not Finalise Ad Hoc Service Job if there are incomplete services.", "Cannot Finalize Ad Hoc Service Job until all Services have been Completed.\r\n", Notify.AsString);

			Notify.Clear();
			service1.ES_Completed = ZDateTime.Today;
			Factory.Save();
			AssertEquals("Should not Finalise Ad Hoc Service Job if there are incomplete services.", false, adHocServiceJob.FinaliseAdHocServiceJob(Notify));
			AssertEquals("Should not Finalise Ad Hoc Service Job if there are incomplete services.", "Cannot Finalize Ad Hoc Service Job until all Services have been Completed.\r\n", Notify.AsString);

			Notify.Clear();
			Notify.DefaultResponse = true;
			service2.ES_Completed = ZDateTime.Today;
			Factory.Save();
			AssertEquals("Should Finalise Ad Hoc Service Job.", true, adHocServiceJob.FinaliseAdHocServiceJob(Notify));
			AssertEquals("Should Finalise Ad Hoc Service Job.", true, adHocServiceJob.IsFinalised);
			AssertEquals("Ad Hoc Service Job should be Read Only if finalised.", true, adHocServiceJob.ReadOnly);
			AssertEquals("Services ReadOnly", true, adHocServiceJob.Services.ReadOnly);
			AssertNull("No Error Message should have been made.", Notify.LastEvent);
		}

		#endregion

		#region TestFinaliseAdHocServiceJobWithNoNotifications

		public void TestFinaliseAdHocServiceJobWithNoNotifications()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adHocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			var errorMessage = string.Empty;

			adHocServiceJob.FinaliseAdHocServiceJobWithNoNotifications((error) => errorMessage = error);
			AssertEquals("Should Finalise Ad Hoc Service Job.", string.Empty, errorMessage);
			AssertEquals("Should Finalise Ad Hoc Service Job.", true, adHocServiceJob.IsFinalised);
			AssertEquals("Ad Hoc Service Job should be Read Only if finalised.", true, adHocServiceJob.ReadOnly);
			AssertEquals("Services ReadOnly", true, adHocServiceJob.Services.ReadOnly);
			AssertEquals("Should have added a Finalised Event.", 1, adHocServiceJob.Logs.Find(
				l => l.SL_SE_NKEvent == Events.ItemDocumentJobFinalisedCode && l.SL_Reference == "Ad Hoc Service Job").Count());

			Factory.Save();

			AssertEquals("Ad Hoc Service Job should be Read Only if finalised.", true, adHocServiceJob.ReadOnly);
			AssertEquals("Services ReadOnly", true, adHocServiceJob.Services.ReadOnly);
		}

		public void TestFinaliseAdHocServiceJobWithNoNotifications_JobAlreadyFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adHocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			var errorMessage = string.Empty;

			adHocServiceJob.FinaliseAdHocServiceJobWithNoNotifications((error) => errorMessage = error);
			AssertEquals("Should Finalise Ad Hoc Service Job.", string.Empty, errorMessage);
			AssertEquals("Should Finalise Ad Hoc Service Job.", true, adHocServiceJob.IsFinalised);
			AssertEquals("Ad Hoc Service Job should be Read Only if finalised.", true, adHocServiceJob.ReadOnly);
			AssertEquals("Services ReadOnly", true, adHocServiceJob.Services.ReadOnly);
			AssertEquals("Should have added a Finalised Event.", 1, adHocServiceJob.Logs.Find(
				l => l.SL_SE_NKEvent == Events.ItemDocumentJobFinalisedCode && l.SL_Reference == "Ad Hoc Service Job").Count());

			Factory.Save();

			AssertEquals("Ad Hoc Service Job should be Read Only if finalised.", true, adHocServiceJob.ReadOnly);
			AssertEquals("Services ReadOnly", true, adHocServiceJob.Services.ReadOnly);

			adHocServiceJob.FinaliseAdHocServiceJobWithNoNotifications((error) => errorMessage = error);
			AssertEquals("Should not re-finalise the Ad Hoc Service Job if it is already finalised.", "Ad Hoc Service Job already Finalized.", errorMessage);
		}

		public void TestFinaliseAdHocServiceJobWithNoNotifications_WhenThereAreIncompleteServices()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adHocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);

			var service1 = adHocServiceJob.Services.AddNew();
			var service2 = adHocServiceJob.Services.AddNew();

			AssertEquals("Collection was not loaded and/or the relationship filter is incorrect.", 2, adHocServiceJob.Services.Count);

			var errorMessage = string.Empty;

			adHocServiceJob.FinaliseAdHocServiceJobWithNoNotifications((error) => errorMessage = error);
			AssertEquals("Should not Finalise Ad Hoc Service Job if there are incomplete services.", "Cannot Finalize Ad Hoc Service Job until all Services have been Completed.", errorMessage);

			service1.ES_CompletedDateTimeOffset = ZDateTimeOffset.Now;
			errorMessage = string.Empty;
			adHocServiceJob.FinaliseAdHocServiceJobWithNoNotifications((error) => errorMessage = error);
			AssertEquals("Should not Finalise Ad Hoc Service Job if there are incomplete services.", "Cannot Finalize Ad Hoc Service Job until all Services have been Completed.", errorMessage);

			service2.ES_CompletedDateTimeOffset = ZDateTimeOffset.Now;
			errorMessage = string.Empty;
			adHocServiceJob.FinaliseAdHocServiceJobWithNoNotifications((error) => errorMessage = error);
			AssertEquals("Should Finalise Ad Hoc Service Job.", string.Empty, errorMessage);
			AssertEquals("Should Finalise Ad Hoc Service Job.", true, adHocServiceJob.IsFinalised);
			AssertEquals("Ad Hoc Service Job should be Read Only if finalised.", true, adHocServiceJob.ReadOnly);
			AssertEquals("Services ReadOnly", true, adHocServiceJob.Services.ReadOnly);
			AssertEquals("Should have added a Finalised Event.", 1, adHocServiceJob.Logs.Find(
				l => l.SL_SE_NKEvent == Events.ItemDocumentJobFinalisedCode && l.SL_Reference == "Ad Hoc Service Job").Count());
			Factory.Save();

			AssertEquals("Ad Hoc Service Job should be Read Only if finalised.", true, adHocServiceJob.ReadOnly);
			AssertEquals("Services ReadOnly", true, adHocServiceJob.Services.ReadOnly);
		}

		#endregion

		#endregion

		#region TestDelete

		public void TestDelete()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adHocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);

			Factory.Save();

			AssertNotNull("Pre-condition: Job Header Should NOT be null.", adHocServiceJob.JobHeader);
			var jobHeader = adHocServiceJob.JobHeader;

			AssertExceptionThrown<InvalidOperationException>(() => adHocServiceJob.Delete());
		}

		#endregion

		#region TestNoteTypes

		public void TestNoteTypes()
		{
			var noteTypes = Factory.New<WhsAdHocServiceJob>().NoteTypes.Cast<PredefinedNoteType>();
			CombineAssertions(() =>
			{
				AssertEquals("Expecting AutoRatingAuditLog note types", true, noteTypes.Contains(PredefinedNoteTypes.Instance.AutoRatingAuditLog));
				AssertEquals("Expecting ClientVisibleJobNotes note types", true, noteTypes.Contains(PredefinedNoteTypes.Instance.ClientVisibleJobNotes));
				AssertEquals("Expecting DangerousGoodsAdditionalHandlingInformation note types", true, noteTypes.Contains(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation));
				AssertEquals("Expecting HandlingInstructions note types", true, noteTypes.Contains(PredefinedNoteTypes.Instance.HandlingInstructions));
				AssertEquals("Expecting InternalWorkNotes note types", true, noteTypes.Contains(PredefinedNoteTypes.Instance.InternalWorkNotes));
			});
		}

		#endregion

		#region TestNoteTypes

		public void TestNoteContexts()
		{
			var whsAdHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			Assert("Should always be 'Warehouse' module", (whsAdHocServiceJob.GetNoteContextsForRelatedNotes().Module & StmNoteContextModule.W) != 0);
		}

		#endregion

		#region TestBusinessObjectsWithRelatedNotes

		public void TestBusinessObjectsWithRelatedNotes_ReturnsWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			adHocServiceJob.WSJ_WW_Whs = data.Whs1.PK;

			var businessObjectsWithRelatedNotes = adHocServiceJob.BusinessObjectsWithRelatedNotes;

			AssertEquals("Should only return one result.", 1, businessObjectsWithRelatedNotes.Length);
			AssertCollectionContains("Should return warehouse.", data.Whs1, businessObjectsWithRelatedNotes);
		}

		public void TestBusinessObjectsWithRelatedNotes_ReturnsOrganization()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			adHocServiceJob.WSJ_OH_Client = data.Org1.PK;

			var businessObjectsWithRelatedNotes = adHocServiceJob.BusinessObjectsWithRelatedNotes;

			AssertEquals("Should only return one result.", 1, businessObjectsWithRelatedNotes.Length);
			AssertCollectionContains("Should return organization.", data.Org1, businessObjectsWithRelatedNotes);
		}

		#endregion

		#region TestWSJ_JobNumberColumnName

		public void TestWSJ_JobNumberColumnName()
		{
			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();

			AssertEquals("Name should be 'Ad Hoc Job Number'", "Ad Hoc Job Number", adHocServiceJob.WSJ_JobNumberInfo.Description);
		}

		#endregion

		#region TestWSJ_JobNumberReadOnly

		public void TestWSJ_JobNumberReadOnly()
		{
			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();

			AssertEquals("WSJ_JobNumber should be read only.", true, adHocServiceJob.WSJ_JobNumberInfo.ReadOnly);
		}

		#endregion

		#region ICanDelete Members

		#region TestICanDelete_CanDelete

		public void TestICanDelete_CanDelete()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adHocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);

			var iCanDelete = (ICanDelete)adHocServiceJob;
			AssertEquals(false, adHocServiceJob.IsFinalised);
			Assert("Should be able to delete unfinalised Ad Hoc Service Job", iCanDelete.CanDelete);

			Factory.Save();

			AssertEquals(false, adHocServiceJob.IsFinalised);
			Assert("Should be able to delete unfinalised Ad Hoc Service Job", iCanDelete.CanDelete);

			adHocServiceJob.WSJ_IsFinalised = true;
			AssertEquals(true, adHocServiceJob.IsFinalised);
			Assert("Should not be able to delete finalised Ad Hoc Service Job", !iCanDelete.CanDelete);
		}

		#endregion

		#region TestICanDelete_ReasonForNotAbleToDelete

		public void TestICanDelete_ReasonForNotAbleToDelete()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adHocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			Factory.Save();

			var iCanDelete = (ICanDelete)adHocServiceJob;
			AssertNull(iCanDelete.ReasonForNotAbleToDelete);

			adHocServiceJob.WSJ_IsFinalised = true;
			AssertEquals("You cannot delete finalized ad hoc service jobs.", iCanDelete.ReasonForNotAbleToDelete);
		}

		#endregion

		#endregion

		#region Implementation

		protected override void SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(ZPropertyInfo info)
		{
			base.SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(info);
			info.BizObj.IgnoreValidationSuspended = info.Name == WhsAdHocServiceJob.Schema.IsFinalised;
			if (info.BizObj.IgnoreValidationSuspended)
			{
				RunValidationInvoker validation = null;
				validation = () => { info.RefreshBinding(); info.AdditionalValidation -= validation; };
				info.AdditionalValidation += validation;
			}
		}

		protected override Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
		{
			get
			{
				var result = base.CachedValueForSettingValueCallsRefreshBindingTestCore;
				result.Add(WhsAdHocServiceJob.Schema.IsFinalised, ZBool.False);
				return result;
			}
		}

		protected TestNotificationBuffer Notify
		{
			get
			{
				if (notify == null)
				{
					notify = new TestNotificationBuffer { DefaultResponse = true };
				}
				return notify;
			}
		}
		TestNotificationBuffer notify;

		#region IJobInvoicingPlugin

		public bool ExpectedAllowInvoiceDeletion { get { return false; } }
		public Type ExpectedJobInvoicingSupporterType { get { return typeof(WhsAdHocServiceJobInvoicingSupporter); } }

		#endregion

		#endregion

		#region IHaveServices

		public void TestIHaveServices()
		{
			var adhocServiceJob = Factory.New<WhsAdHocServiceJob>();
			var iHaveServices = (IHaveServices)adhocServiceJob;

			AssertEquals("", iHaveServices.ContainerMode);
			AssertEquals("", iHaveServices.TransportMode);
			AssertEquals(0, iHaveServices.DependentServiceParents.Length);
			AssertEquals(adhocServiceJob, iHaveServices.ServiceParent);
			AssertEquals(adhocServiceJob.TablePrefix, iHaveServices.TableCode);
		}

		public void TestServices()
		{
			var adhocServiceJob = Factory.New<WhsAdHocServiceJob>();
			var service = Factory.New<WhsJobService>();
			service.ES_ParentID = adhocServiceJob.PK;
			service.ES_ParentTableCode = adhocServiceJob.TablePrefix;

			AssertCollectionContains("Collection was not loaded and/or the relationship filter is incorrect.", service, adhocServiceJob.Services);
			AssertEquals(typeof(WhsJobService), adhocServiceJob.Services.TypeOfElements);
			AssertEquals(true, adhocServiceJob.IsRegisteredEditableChildObject(adhocServiceJob.Services));
		}

		public void TestServiceBranch()
		{
			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			var iHaveServices = (IHaveServices)adHocServiceJob;
			AssertNull("No service branch", iHaveServices.ServiceBranch);

			var warehouse = Helper.CreateWarehouse("Whs");
			AssertEquals(true, warehouse.WW_GB_RelatedCompanyBranch.IsValid);

			adHocServiceJob.WSJ_WW_Whs = warehouse.PK;
			AssertEquals("Service branch is warehouse branch", warehouse.WW_GB_RelatedCompanyBranch, iHaveServices.ServiceBranch.PK);
		}

		#endregion

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert(true);
		}

		#region Helper

		WhsTestHelperFunctions Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new WhsTestHelperFunctions(Factory);
				}
				return helper;
			}
		}
		WhsTestHelperFunctions helper;

		#endregion
	}
}
