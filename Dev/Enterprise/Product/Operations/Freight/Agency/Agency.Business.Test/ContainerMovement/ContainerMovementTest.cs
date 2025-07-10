using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ContainerMovement))]
	internal class ContainerMovementTest : EnterpriseBusinessObjectTestCase
	{
		public void TestJobNumber()
		{
			var movement1 = Factory.New<ContainerMovement>();
			var movement2 = Factory.New<ContainerMovement>();
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "aaaaa";
			movement2.E9_R6 = stock.PK;
			AssertEquals(ZString.Empty, movement1.JobNumber);
			AssertEquals("aaaaa", movement2.JobNumber);
		}

		public void TestIWorkflowProviderImpl()
		{
			var movement = Factory.New<ContainerMovement>();
			var workflowProvider = movement as IWorkflowProvider;
			AssertNotNull(workflowProvider);
			AssertNull(workflowProvider.GetWorkflowInformationProvider());
			AssertNotNull(workflowProvider.WorkflowItems);
			AssertEquals(0, workflowProvider.WorkflowItems.Count);
			AssertEquals(typeof(ColumnValueRanker), workflowProvider.GetTemplateSelectionCriteria().GetType());
			AssertEquals(WorkflowDescriptors.ContainerMovementWorkflowDescriptorCode, workflowProvider.WorkflowType);
		}

		public void TestDetentionCompanyCode()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_Code = "BAZ";
			Detention.NC_GC = company.PK;
			Movement.E9_NC = ZGuid.Empty;
			AssertEquals("", Movement.E9_DetentionCompanyCode);
			Movement.E9_NC = Detention.PK;
			AssertEquals("BAZ", Movement.E9_DetentionCompanyCode);
		}

		public void TestDefaultAddressTypes()
		{
			Movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			AssertEquals(AddressType.DLV, Movement.E9_OA_Depot_ZAddress.DefaultAddressType);
			Movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			AssertEquals(AddressType.PIC, Movement.E9_OA_Depot_ZAddress.DefaultAddressType);
			Movement.E9_MovementType = ContainerMovementTypes.Codes.ReShipRequested;
			AssertEquals(AddressType.NoDefault, Movement.E9_OA_Depot_ZAddress.DefaultAddressType);
		}

		public void TestValidateDepotOrgPK()
		{
			const string errorMissing = "Please enter a value.";
			const string errorInvalid = "Enter a valid selection.";
			Movement.E9_OA_Depot_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Invalid);
			AssertHasError(Movement.E9_OA_Depot_ZAddress.OrgPKInfo, errorInvalid);
			Movement.E9_OA_Depot_ZAddress.SetOrgWithoutSettingDefaultAddress(Depot.PK);
			AssertNoNotifications(Movement.E9_OA_Depot_ZAddress.OrgPKInfo);
			Movement.E9_OA_Depot_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
			AssertHasError(Movement.E9_OA_Depot_ZAddress.OrgPKInfo, errorMissing);
		}

		public void TestDetentionPort()
		{
			OrgHeader depot1 = Factory.New<OrgHeader>();
			depot1.OH_RL_NKClosestPort = "AUBNE";
			OrgHeader depot2 = Factory.New<OrgHeader>();
			depot2.OH_RL_NKClosestPort = "AUSYD";
			Movement.E9_OA_Depot = depot1.MainAddress.PK;
			AssertEquals("AUBNE", Movement.DepotPort);
			Movement.E9_OA_Depot = depot2.MainAddress.PK;
			AssertEquals("AUSYD", Movement.DepotPort);
			Movement.E9_OA_Depot = ZGuid.Empty;
			AssertEquals("", Movement.DepotPort);
		}

		[UseDummyDetentionStrategy]
		public void TestReCalculateDetentionDays()
		{
			DummyDetentionStrategy.Instance.GetDefaultDetentionDaysOverride = delegate(ContainerMovement movement)
			{
				return movement.E9_DetentionDays + 1;
			};
			ZDateTime now = ZDateTime.Now.ToSmallDateTime();
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			ContainerDetention detention = Factory.New<ContainerDetention>();
			Movement.E9_DetentionDays = 0;
			AssertEquals("nothing happend yet", (short)0, Movement.E9_DetentionDays);
			Movement.E9_MovementType = "XXX";
			AssertEquals("movement type set", (short)1, Movement.E9_DetentionDays);
			Movement.E9_MovementType = "XXX";
			AssertEquals("movement type set to existing value", (short)1, Movement.E9_DetentionDays);
			Movement.E9_MovementDate = now.AddDays(-1);
			AssertEquals("movement date set", (short)2, Movement.E9_DetentionDays);
			Movement.E9_MovementDate = now.AddDays(-1);
			AssertEquals("movement date set to existing value", (short)2, Movement.E9_DetentionDays);
			Movement.E9_JV = Voyage.PK;
			AssertEquals("voyage set", (short)3, Movement.E9_DetentionDays);
			Movement.E9_JV = Voyage.PK;
			AssertEquals("voyage set to existing value", (short)3, Movement.E9_DetentionDays);
			Movement.E9_OH_Principal = principal.PK;
			AssertEquals("principal set", (short)4, Movement.E9_DetentionDays);
			Movement.E9_OH_Principal = principal.PK;
			AssertEquals("principal set to existing value", (short)4, Movement.E9_DetentionDays);
			Movement.E9_OH_ResponsibleParty = client.PK;
			AssertEquals("client set", (short)5, Movement.E9_DetentionDays);
			Movement.E9_OH_ResponsibleParty = client.PK;
			AssertEquals("client set to existing value", (short)5, Movement.E9_DetentionDays);
			Movement.E9_NC = detention.PK;
			AssertEquals("detention job set", (short)5, Movement.E9_DetentionDays);
			Movement.E9_MovementType = "YYY";
			AssertEquals("do not update detention days once attached to a detention job.", (short)5, Movement.E9_DetentionDays);
		}

		public void TestFieldsReadonly()
		{
			AssertEquals(false, Movement.DetentionInvoiced);
			AssertEquals(false, Movement.DetentionPosted);
			AssertEquals(false, Movement.E9_MovementDateInfo.ReadOnly);
			AssertEquals(false, Movement.E9_MovementTypeInfo.ReadOnly);
			AssertEquals(false, Movement.E9_DetentionDaysInfo.ReadOnly);
			AssertEquals(false, Movement.E9_OH_PrincipalInfo.ReadOnly);
			AssertEquals(false, Movement.E9_OH_ResponsiblePartyInfo.ReadOnly);
			AssertEquals(true, Movement.E9_NCInfo.ReadOnly);
			Movement.E9_NC = Detention.PK;
			JobHeader header = new JobHeader.Loader(Detention).TryLoadOrCreate();
			AssertEquals(true, Movement.DetentionInvoiced);
			AssertEquals(false, Movement.DetentionPosted);
			AssertEquals(true, Movement.E9_MovementDateInfo.ReadOnly);
			AssertEquals(true, Movement.E9_MovementTypeInfo.ReadOnly);
			AssertEquals(false, Movement.E9_DetentionDaysInfo.ReadOnly);
			AssertEquals(true, Movement.E9_OH_PrincipalInfo.ReadOnly);
			AssertEquals(true, Movement.E9_OH_ResponsiblePartyInfo.ReadOnly);
			AssertEquals(true, Movement.E9_NCInfo.ReadOnly);
			NewCharge(header);
			AssertEquals(true, Movement.DetentionInvoiced);
			AssertEquals(true, Movement.DetentionPosted);
			AssertEquals(true, Movement.E9_MovementDateInfo.ReadOnly);
			AssertEquals(true, Movement.E9_MovementTypeInfo.ReadOnly);
			AssertEquals(true, Movement.E9_DetentionDaysInfo.ReadOnly);
			AssertEquals(true, Movement.E9_OH_PrincipalInfo.ReadOnly);
			AssertEquals(true, Movement.E9_OH_ResponsiblePartyInfo.ReadOnly);
			AssertEquals(true, Movement.E9_NCInfo.ReadOnly);
		}

		public void TestCanDelete()
		{
			ICanDelete canDelete = Movement;
			Movement.E9_NC = ZGuid.Empty;
			AssertEquals("can delete as not attached.", true, canDelete.CanDelete);
			AssertEquals("no reason as not attached.", null, canDelete.ReasonForNotAbleToDelete);
			Movement.E9_NC = Factory.New<ContainerDetention>().PK;
			AssertEquals("cannot delete as not attached.", false, canDelete.CanDelete);
			AssertEquals("correct reason for not being able to delete", "This movement is attached to a detention job. You must detach it from the detention job before you can delete it.", canDelete.ReasonForNotAbleToDelete);
		}

		public void TestIsAutoLogged()
		{
			Movement.Factory.Save();
			Assert("Should have event logs", Movement.Logs.GetAllLogs().Count > 0);
		}

		public void TestMovementType()
		{
			AssertType(typeof(DetentionNullStrategy), Movement.DetentionStrategy);
			Movement.E9_MovementType = "WGI";
			AssertType(typeof(DetentionExportStrategy), Movement.DetentionStrategy);
			Movement.E9_MovementType = "XXX";
			AssertType(typeof(DetentionNullStrategy), Movement.DetentionStrategy);
		}

		public void TestHumanReadableName()
		{
			Movement.E9_MovementType = "ABC";
			Movement.E9_MovementDate = new ZDateTime(2011, 10, 01);
			AssertEquals("Container Movement (FAKE4100013, ABC, 01-Oct-11)", Movement.HumanReadableName);
		}

		public void TestAttachOrDetachMovementIsLogged()
		{
			Detention.NC_OH_Client = Client.PK;
			Detention.NC_OH_Principal = Principal.PK;
			var container = Factory.New<BillOfLading>().RealContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerNum = "CONT1234457";
			var stock = Factory.LoadTop1<RefContainerStock>(new ZQuery(RefContainerStockSchema.R6_ContainerNum, container.JC_ContainerNum));
			var movement = container.Movements.AddNew();
			movement.E9_R6 = stock.PK;
			movement.E9_NC = Detention.PK;
			Assert("Attach event is logged", ContainsLog("ATC", Detention.Logs.GetAllLogs()));
			Factory.Save();
			movement.E9_NC = ZGuid.Empty;
			Assert("Detach event is logged", ContainsLog("DTC", Detention.Logs.GetAllLogs()));
		}

		public void TestRemoveUnsavedATCEventWhenDetach()
		{
			var containerDetention = Factory.New<ContainerDetention>();
			containerDetention.NC_OH_Client = Client.PK;
			containerDetention.NC_OH_Principal = Principal.PK;
			var container = Factory.New<BillOfLading>().RealContainers.AddNew();
			var movement = container.Movements.AddNew();
			movement.E9_NC = containerDetention.PK;
			Assert("Attach event is logged", ContainsLog("ATC", containerDetention.Logs.GetAllLogs()));
			movement.E9_NC = ZGuid.Empty;
			Assert("Attach event is removed", !ContainsLog("ATC", containerDetention.Logs.GetAllLogs()));
			Assert("Detach event is NOT logged", !ContainsLog("DTC", containerDetention.Logs.GetAllLogs()));
		}

		public void TestGetCreateTimeLocal_UTCTimeIsValid_ReturnLocalTime()
		{
			var testDate = new ZDateTime(DateTime.Now.ToUniversalTime());
			this.Movement.E9_SystemCreateTimeUtc = testDate;
			AssertEquals("Local time should be correct", Env.Time.GetLocalTimeFromUtc(testDate.ToDateTime()), movement.CreatedTimeLocal);
		}

		public void TestGetCreateTimeLocal_UTCTimeIsInvalid_ReturnInvalidZDateTime()
		{
			this.Movement.E9_SystemCreateTimeUtc = ZDateTime.Invalid;
			AssertEquals("Local time should be invalid", ZDateTime.Invalid, movement.CreatedTimeLocal);
		}

		public void TestGetCreateTimeLocal_UTCTimeIsEmpty_ReturnEmptyZDateTime()
		{
			this.Movement.E9_SystemCreateTimeUtc = ZDateTime.Empty;
			AssertEquals("Local time should be empty", ZDateTime.Empty, movement.CreatedTimeLocal);
		}

		public void TestMovementDate()
		{
			Movement.E9_MovementDate = new ZDateTime(2013, 1, 1, 12, 10, 10, 58);
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var movement2 = factory2.Load<ContainerMovement>(Movement.PK);
			AssertEquals("E9_MovementDate column does no store seconds", new ZDateTime(2013, 1, 1, 12, 10, 0, 0), movement2.E9_MovementDate);
			movement2.E9_MovementDate = new ZDateTime(2013, 1, 2, 11, 5, 10, 44);
			AssertEquals("E9_MovementDate does no accept seconds", new ZDateTime(2013, 1, 2, 11, 5, 0, 0), movement2.E9_MovementDate);
			movement2.E9_MovementDate = ZDateTime.Invalid;
			AssertEquals("Invalid E9_MovementDate", ZDateTime.Invalid, movement2.E9_MovementDate);
			movement2.E9_MovementDate = ZDateTime.Empty;
			AssertEquals("Empty E9_MovementDate", ZDateTime.Empty, movement2.E9_MovementDate);
		}

		public void TestMovementDateRefreshBinding()
		{
			bool listChangedFired = false;
			ListChangedEventHandler handler = (s, e) =>
			{
				if (e.ListChangedType == ListChangedType.ItemChanged)
				{
					listChangedFired = true;
				}
			};
			((IBindingList)Movement).ListChanged += handler;
			try
			{
				Movement.E9_MovementDate = new ZDateTime(2013, 1, 1, 10, 15, 0, 0);
				AssertEquals("refresh binding fired (set new)", true, listChangedFired);
				AssertEquals(new ZDateTime(2013, 1, 1, 10, 15, 0, 0), Movement.E9_MovementDate);
				listChangedFired = false;
				Movement.E9_MovementDate = new ZDateTime(2013, 1, 2, 10, 15, 0, 0);
				AssertEquals("refresh binding fired (day changed)", true, listChangedFired);
				AssertEquals(new ZDateTime(2013, 1, 2, 10, 15, 0, 0), Movement.E9_MovementDate);
				listChangedFired = false;
				Movement.E9_MovementDate = new ZDateTime(2013, 1, 2, 10, 15, 10, 0);
				AssertEquals("refresh binding fired though date hasn't changed (changed seconds only)", true, listChangedFired);
				AssertEquals(new ZDateTime(2013, 1, 2, 10, 15, 0, 0), Movement.E9_MovementDate);
				listChangedFired = false;
				Movement.E9_MovementDate = new ZDateTime(2013, 1, 2, 10, 15, 0, 0);
				AssertEquals("refresh binding not fired as date hasn't changed", false, listChangedFired);
				AssertEquals(new ZDateTime(2013, 1, 2, 10, 15, 0, 0), Movement.E9_MovementDate);
			}
			finally
			{
				((IBindingList)Movement).ListChanged -= handler;
			}
		}

		public void TestShipmentContainerEvents()
		{
			var origin = Voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			var destination = Voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			Voyage.GenerateSailings();
			var sailing = Voyage.Sailings[0];
			var billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading.JS_TransportMode = Core.Constants.TransportModes.Sea;
			billOfLading.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			billOfLading.JS_JX = sailing.PK;
			var container = billOfLading.RealContainers.AddNew();
			container.JC_ContainerNum = Stock.R6_ContainerNum;
			var depotOrg = Factory.NewWithValidTestData<OrgHeader>();
			depotOrg.MainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			Factory.Save();
			Movement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			Movement.E9_MovementDate = ZDateTime.Now.AddDays(-10);
			Movement.E9_OA_Depot = depotOrg.MainAddress.PK;
			Movement.E9_JV = Voyage.PK;
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var filter = new ZQuery();
			filter.AddToFilter(StmALogSchema.SL_Parent, container.PK);
			filter.AddToFilter(StmALogSchema.SL_EventTime, Movement.E9_MovementDate);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, AgencyShipmentContainerMovementEventSynchroniser.MovementTypeCodeEventMap[Movement.E9_MovementType]);
			var log = factory2.LoadTop1<StmALog>(filter);
			AssertNotNull(log);
			Movement.E9_MovementDate = ZDateTime.Now.AddDays(-20);
			Factory.Save();
			log = factory2.LoadTop1<StmALog>(filter);
			Assert(string.Format("log corresponding to {0} ({1}) is cancelled because the movement date has changed", Movement.E9_MovementType, log.SL_SE_NKEvent), log.IsCancelled);
			filter = new ZQuery();
			filter.AddToFilter(StmALogSchema.SL_Parent, container.PK);
			filter.AddToFilter(StmALogSchema.SL_EventTime, Movement.E9_MovementDate);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, AgencyShipmentContainerMovementEventSynchroniser.MovementTypeCodeEventMap[Movement.E9_MovementType]);
			log = factory2.LoadTop1<StmALog>(filter);
			AssertNotNull(string.Format("matching {0} movement log found", Movement.E9_MovementType), log);
			Movement.E9_MovementType = ContainerMovementTypes.Codes.Load;
			Factory.Save();
			log = factory2.LoadTop1<StmALog>(filter);
			Assert(string.Format("log corresponding to previous movement type {0} is cancelled", ContainerMovementTypes.Codes.WharfGateIn), log.IsCancelled);
			filter = new ZQuery();
			filter.AddToFilter(StmALogSchema.SL_Parent, container.PK);
			filter.AddToFilter(StmALogSchema.SL_EventTime, Movement.E9_MovementDate);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, AgencyShipmentContainerMovementEventSynchroniser.MovementTypeCodeEventMap[Movement.E9_MovementType]);
			log = factory2.LoadTop1<StmALog>(filter);
			AssertNotNull(string.Format("matching {0} movement log found", Movement.E9_MovementType), log);
			Movement.Delete();
			Factory.Save();
			log = factory2.LoadTop1<StmALog>(filter);
			Assert("log corresponding to the deleted movement has been cancelled", log.IsCancelled);
		}

		public void TestMovementMessagesWhileDeleting()
		{
			var depotOrg = Factory.NewWithValidTestData<OrgHeader>();
			depotOrg.MainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			Movement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			Movement.E9_MovementDate = ZDateTime.Now.AddDays(-10);
			Movement.E9_OA_Depot = depotOrg.MainAddress.PK;
			Movement.E9_JV = Voyage.PK;
			Factory.Save();
			var logFilter = new ZQuery();
			logFilter.AddToFilter(StmALogSchema.SL_Parent, Movement.PK);
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			var message1 = Factory.NewWithValidTestData<EDIMessage>();
			message1.EM_LinkedObject = Movement;
			message1.EM_EI = interchange.PK;
			var message2 = Factory.NewWithValidTestData<EDIMessage>();
			message2.EM_LinkedObject = Movement;
			Factory.Save();
			Movement.Delete();
			AssertEquals(false, message1.IsDeleted);
			AssertEquals(true, message2.IsDeleted);
			AssertEquals(1, movement.Messages.Count);
			AssertEquals("Error 'EDIMessage should not be deleted' should not have been reported.", 0, CargoWise.Common.ErrorReporter.TotalErrorCount);
		}

		public void TestCodePropertyAttribute()
		{
			var attribute = typeof(ContainerMovement).GetCustomAttributes(typeof(CodePropertyAttribute), false).Cast<CodePropertyAttribute>().Single();
			Assert("E9_MovementType used in CodePropertyAttribute", attribute.PropertyName == "E9_MovementType");
		}

		bool ContainsLog(string logCode, StmALogDependentCollection logs)
		{
			return logs.Cast<StmALog>().Any(log => log.SL_SE_NKEvent == logCode);
		}

		#region E9_LeaseNumber
		public void TestValidateLeaseNumbers()
		{
			#region Prepare Test Data
			const string noContractNumberWaring = @"The Lease Contract is valid up to the next Off-Hire movement.
Add Lease Contract No to the movements of leased container for better tracking and reporting.";
			const string mismatchedContractNumberWaring = @"The contract number entered does not match the contract number of the On-Hire movement.";
			var today = DateTime.Today;
			var onHire1 = Stock.Movements.AddNew();
			onHire1.E9_MovementType = ContainerMovementTypes.Codes.OnHire;
			onHire1.E9_MovementDate = today.AddDays(-50);
			onHire1.E9_LeaseNumber = "CONTRACT1";
			var offHire1 = Stock.Movements.AddNew();
			offHire1.E9_MovementType = ContainerMovementTypes.Codes.OffHire;
			offHire1.E9_MovementDate = today.AddDays(-40);
			var onHire2 = Stock.Movements.AddNew();
			onHire2.E9_MovementType = ContainerMovementTypes.Codes.OnHire;
			onHire2.E9_MovementDate = today.AddDays(-30);
			onHire2.E9_LeaseNumber = "CONTRACT2";
			var offHire2 = Stock.Movements.AddNew();
			offHire2.E9_MovementType = ContainerMovementTypes.Codes.OffHire;
			AssertMovementResult("offHire2 is in last hire and has same contract number with last OnHire", offHire2, today.AddDays(-10), "CONTRACT2");
			Movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			AssertMovementResult("Movement is in last hire and has same contract number with last OnHire.", Movement, today.AddDays(-25), "CONTRACT2");
			var movement2 = Stock.Movements.AddNew();
			movement2.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			AssertMovementResult("movement2 is in last hire and has same contract number with last OnHire.", movement2, today.AddDays(-20), "CONTRACT2");
			var movement3 = Stock.Movements.AddNew();
			movement3.E9_MovementType = ContainerMovementTypes.Codes.RePositionOutOfYard;
			AssertMovementResult("movement3 is out of last hire.", movement3, today.AddDays(-5));
			#endregion
			onHire2.E9_LeaseNumber = string.Empty;
			AssertNoNotifications("onHire2 has no contract number.", Movement.E9_LeaseNumberInfo);
			AssertNoNotifications("onHire2 has no contract number.", movement2.E9_LeaseNumberInfo);
			AssertNoNotifications("onHire2 has no contract number.", offHire2.E9_LeaseNumberInfo);
			AssertNoNotifications("movement3 is out of last hire.", movement3.E9_LeaseNumberInfo);
			onHire2.E9_LeaseNumber = "CAR";
			AssertHasWarning("Movement has different contract number.", Movement.E9_LeaseNumberInfo, mismatchedContractNumberWaring);
			AssertHasWarning("movement2 has different contract number.", movement2.E9_LeaseNumberInfo, mismatchedContractNumberWaring);
			AssertHasWarning("offHire2 has different contract number.", offHire2.E9_LeaseNumberInfo, mismatchedContractNumberWaring);
			AssertNoNotifications("movement3 is out of last hire.", movement3.E9_LeaseNumberInfo);
			onHire2.E9_MovementDate = today.AddDays(-23);
			AssertNoNotifications("Movement is out of last hire.", Movement.E9_LeaseNumberInfo);
			AssertHasWarning("movement2 has different contract number.", movement2.E9_LeaseNumberInfo, mismatchedContractNumberWaring);
			AssertHasWarning("offHire2 has different contract number.", offHire2.E9_LeaseNumberInfo, mismatchedContractNumberWaring);
			AssertNoNotifications("movement3 is out of last hire.", movement3.E9_LeaseNumberInfo);
			movement2.E9_MovementType = ContainerMovementTypes.Codes.OnHire;
			AssertNoNotifications("Movement is out of last hire.", Movement.E9_LeaseNumberInfo);
			AssertNoNotifications("movement2 becomes OnHire type.", movement2.E9_LeaseNumberInfo);
			AssertNoNotifications("offHire2 has same contract number.", offHire2.E9_LeaseNumberInfo);
			AssertNoNotifications("movement3 is out of last hire.", movement3.E9_LeaseNumberInfo);
			offHire2.E9_MovementType = ContainerMovementTypes.Codes.ReturnToWharf;
			AssertNoNotifications("Movement is out of last hire.", Movement.E9_LeaseNumberInfo);
			AssertNoNotifications("movement2 becomes OnHire type.", movement2.E9_LeaseNumberInfo);
			AssertNoNotifications("offHire2 has same contract number.", offHire2.E9_LeaseNumberInfo);
			AssertHasWarning("movement3 has no contract number.", movement3.E9_LeaseNumberInfo, noContractNumberWaring);
			movement3.E9_LeaseNumber = "CONTRACT2";
			AssertNoNotifications("Movement is out of last hire.", Movement.E9_LeaseNumberInfo);
			AssertNoNotifications("movement2 becomes OnHire type.", movement2.E9_LeaseNumberInfo);
			AssertNoNotifications("offHire2 has same contract number.", offHire2.E9_LeaseNumberInfo);
			AssertNoNotifications("movement3 has same contract number.", movement3.E9_LeaseNumberInfo);
		}

		void AssertMovementResult(string message, ContainerMovement containterMovement, ZDateTime movementDate, string leaseNumber = null, string expectWarning = null)
		{
			containterMovement.E9_MovementDate = movementDate;
			containterMovement.E9_LeaseNumber = leaseNumber ?? string.Empty;
			if (string.IsNullOrEmpty(expectWarning))
			{
				AssertNoNotifications(message, containterMovement.E9_LeaseNumberInfo);
			}
			else
			{
				AssertHasWarning(message, containterMovement.E9_LeaseNumberInfo, expectWarning);
			}
		}

		#endregion
		#region Business Object TestCase
		public override void TestSaveAndDeleteBusinessObject()
		{
			AssertExceptionThrown<ZSaveException>("ContainerMovement with invalid E9_R6 cannot be saved", () => base.TestSaveAndDeleteBusinessObject());
		}

		#endregion
		#region Implementation
		RefContainerStock Stock
		{
			get
			{
				if (stock == null)
				{
					stock = Factory.New<RefContainerStock>();
					stock.R6_ContainerNum = "FAKE4100013";
					stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				}

				return stock;
			}
		}

		RefContainerStock stock;
		ContainerMovement Movement
		{
			get
			{
				return movement ?? (movement = Stock.Movements.AddNew());
			}
		}

		ContainerMovement movement;
		JobVoyage Voyage
		{
			get
			{
				return voyage ?? (voyage = Factory.New<JobVoyage>());
			}
		}

		JobVoyage voyage;
		ContainerDetention Detention
		{
			get
			{
				if (detention == null)
				{
					detention = Factory.New<ContainerDetention>();
					detention.NC_OH_Client = Client.PK;
					detention.NC_OH_Principal = Principal.PK;
				}

				return detention;
			}
		}

		ContainerDetention detention;
		OrgHeader Principal
		{
			get
			{
				return principal ?? (principal = Factory.NewWithValidTestData<OrgHeader>());
			}
		}

		OrgHeader principal;
		OrgHeader Client
		{
			get
			{
				return client ?? (client = Factory.NewWithValidTestData<OrgHeader>());
			}
		}

		OrgHeader client;
		OrgHeader Depot
		{
			get
			{
				return depot ?? (depot = Factory.NewWithValidTestData<OrgHeader>());
			}
		}

		OrgHeader depot;
		JobCharge NewCharge(JobHeader job)
		{
			AccTransactionLines lines = Factory.NewWithValidTestData<AccTransactionLines>();
			lines.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AL_ARLine = lines.PK;
			return charge;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<AgencyShipmentContainer>().Movements.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.New<AgencyShipmentContainer>().Movements.AddNew();
		}
		#endregion
	}
}
