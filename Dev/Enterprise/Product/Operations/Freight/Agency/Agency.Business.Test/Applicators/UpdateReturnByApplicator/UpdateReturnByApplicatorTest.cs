using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(UpdateReturnByApplicator))]
	internal class UpdateReturnByApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestPerformance()
		{
			const int ShipmentCount = 20;
			ZGuid[] pks = CreateShipmentsForPerformanceTest(ShipmentCount);
			BusinessObjectFactory factoryForRun = new BusinessObjectFactory();
			BusinessObject[] targets = factoryForRun.Load<BillOfLading>(new ZQuery(JobShipmentSchema.PK, pks));
			AssertEquals("precondition:", ShipmentCount, targets.Length);
			SimulateRun(targets, true);
			// DO NOT INCREASE THIS WITHOUT FIRST DISCUSSING WITH THE AGENCY TEAM!!!
			AssertMaxDbHits(254, factoryForRun);
			//StmALog: 61
			//ProcessTasks: 33
			//JobDocAddress: 20
			//JobPackLines: 20
			//UNDGDataItem: 20
			//ProcessTaskTemplate: 19
			//OrgContainerDetention: 18
			//JobContainer: 16
			//JobHeader: 16
			//JobContainerDetention: 15
			//OrgHeader: 3
			//OrgAddress: 2
			//StmEvent: 2
			//JobConsolTransport: 1
			//JobContainerMove: 1
			//JobSailing: 1
			//JobShipment: 1
			//JobVoyage: 1
			//JobVoyDestination: 1
			//JobVoyOrigin: 1
			//RefContainer: 1
			//RefContainerStock: 1
			//
			// Hits: 254
		}

		public void TestNonFclShipment()
		{
			ZGuid shipmentPK;
			{
				BusinessObjectFactory setupFactory = new BusinessObjectFactory();
				OrgHeader principal = setupFactory.NewWithValidTestData<OrgHeader>();
				principal.OH_Code = "principal";
				OrgHeader consignee = setupFactory.NewWithValidTestData<OrgHeader>();
				consignee.OH_Code = "consignee";
				var vessel = RefVessel.LookupVesselByName("NORDCLOUD", Factory).First();
				JobVoyage voyage = setupFactory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "001";
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
				BillOfLading bill = setupFactory.New<BillOfLading>();
				bill.JS_UniqueConsignRef = "V00000100";
				bill.JS_JX = voyage.Sailings[0].PK;
				bill.JS_OH_DeliveryAgent = principal.PK;
				bill.JS_PackingMode = Constants.ContainerModes.BreakBulk;
				bill.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
				setupFactory.Save();
				shipmentPK = bill.PK;
			}

			const string expected = "WARNING: [HL V00000100] is not a FCL shipment, skipping." + "";
			BillOfLading target = Factory.Load<BillOfLading>(shipmentPK);
			ApplyApplicator(new BusinessObject[] { target }, expected);
		}

		public void TestNoUpdateEmpty()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				ZGuid containerPK;
				{
					BusinessObjectFactory setupFactory = new BusinessObjectFactory();
					OrgHeader principal = setupFactory.NewWithValidTestData<OrgHeader>();
					principal.OH_Code = "principal";
					OrgHeader client = setupFactory.NewWithValidTestData<OrgHeader>();
					client.OH_Code = "client";
					var vessel = RefVessel.LookupVesselByName("NORDCLOUD", Factory).First();
					JobVoyage voyage = setupFactory.New<JobVoyage>();
					voyage.JV_RV_NKVessel = vessel.RV_FK;
					voyage.JV_VoyageFlight = "001";
					voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
					voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
					BillOfLading bill = setupFactory.New<BillOfLading>();
					bill.JS_UniqueConsignRef = "V00000100";
					bill.JS_JX = voyage.Sailings[0].PK;
					bill.JS_OH_DeliveryAgent = principal.PK;
					bill.ConsigneeDocumentaryAddress.OrganisationPK = client.PK;
					BillOfLadingContainer container = bill.RealContainers.AddNew();
					container.JC_ContainerNum = "TEST4100013";
					container.JC_RC = setupFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
					container.JC_EmptyReturnedBy = ZDateTime.Empty;
					ContainerMovement movement = container.Stock.Movements.AddNew();
					movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
					movement.E9_MovementDate = new ZDateTime(2010, 10, 12);
					movement.E9_JV = voyage.PK;
					setupFactory.Save();
					containerPK = container.PK;
				}

				const string expected = "INFO: Processing containers on [HL V00000100]:\r\n" + "WARNING: Was not able to determine the return required by date for [HL TEST4100013]. Leaving the value empty.\r\n" + "";
				BillOfLadingContainer target = Factory.Load<BillOfLadingContainer>(containerPK);
				ApplyApplicator(new BusinessObject[] { target }, expected);
				AssertEquals("JC_EmptyReturnedBy", ZDateTime.Empty, target.JC_EmptyReturnedBy);
			}
		}

		public void TestNoUpdateEmptyContainerNumberAndContainerType()
		{
			ZGuid containerPK;
			{
				BusinessObjectFactory setupFactory = new BusinessObjectFactory();
				var vessel = RefVessel.LookupVesselByName("NORDCLOUD", Factory).First();
				var voyage = setupFactory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "001";
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
				var bill = setupFactory.New<BillOfLading>();
				bill.JS_UniqueConsignRef = "V10000100";
				bill.JS_JX = voyage.Sailings[0].PK;
				var container = bill.RealContainers.AddNew();
				container.JC_ContainerNum = "TEST4100013";
				container.JC_RC = setupFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				container.JC_EmptyReturnedBy = ZDateTime.Empty;
				var movement = container.Stock.Movements.AddNew();
				movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
				movement.E9_MovementDate = new ZDateTime(2010, 10, 12);
				movement.E9_JV = voyage.PK;
				setupFactory.Save();
				containerPK = container.PK;
			}

			var target = Factory.Load<BillOfLadingContainer>(containerPK);
			target.JC_ContainerNum = string.Empty;
			target.JC_RC = Guid.Empty;
			Factory.Save();
			var expected = "INFO: Processing containers on [HL V10000100]:\r\n" + "ERROR: Containers exist without a Container Number & Container Type. Correct the errors on Bill of Lading V10000100\r\n" + "";
			CombineAssertions(delegate
			{
				ApplyApplicator(new BusinessObject[] { target }, expected);
				AssertEquals("There should be no container movement because the operational action should have failed", ZDateTime.Empty, target.JC_EmptyReturnedBy);
			});
		}

		public void TestNoUpdatePopulated()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				ZGuid containerPK;
				{
					BusinessObjectFactory setupFactory = new BusinessObjectFactory();
					OrgHeader principal = setupFactory.NewWithValidTestData<OrgHeader>();
					principal.OH_Code = "principal";
					OrgHeader client = setupFactory.NewWithValidTestData<OrgHeader>();
					client.OH_Code = "client";
					var vessel = RefVessel.LookupVesselByName("NORDCLOUD", Factory).First();
					JobVoyage voyage = setupFactory.New<JobVoyage>();
					voyage.JV_RV_NKVessel = vessel.RV_FK;
					voyage.JV_VoyageFlight = "001";
					voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
					voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
					BillOfLading bill = setupFactory.New<BillOfLading>();
					bill.JS_UniqueConsignRef = "V00000100";
					bill.JS_JX = voyage.Sailings[0].PK;
					bill.JS_OH_DeliveryAgent = principal.PK;
					bill.ConsigneeDocumentaryAddress.OrganisationPK = client.PK;
					BillOfLadingContainer container = bill.RealContainers.AddNew();
					container.JC_ContainerNum = "TEST4100013";
					container.JC_RC = setupFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
					container.JC_EmptyReturnedBy = new ZDateTime(2010, 10, 11);
					ContainerMovement movement = container.Stock.Movements.AddNew();
					movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
					movement.E9_MovementDate = new ZDateTime(2010, 10, 12);
					movement.E9_JV = voyage.PK;
					setupFactory.Save();
					containerPK = container.PK;
				}

				const string expected = "INFO: Processing containers on [HL V00000100]:\r\n" + "WARNING: Was not able to determine the return required by date for [HL TEST4100013]. Leaving the value as '11-Oct-10'.\r\n" + "";
				BillOfLadingContainer target = Factory.Load<BillOfLadingContainer>(containerPK);
				ApplyApplicator(new BusinessObject[] { target }, expected);
				AssertEquals("JC_EmptyReturnedBy", new ZDateTime(2010, 10, 11), target.JC_EmptyReturnedBy);
			}
		}

		public void TestUpdate()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				ZGuid containerPK;
				ZGuid destinationPK;
				{
					BusinessObjectFactory setupFactory = new BusinessObjectFactory();
					OrgHeader principal = setupFactory.NewWithValidTestData<OrgHeader>();
					principal.OH_Code = "principal";
					OrgHeader client = setupFactory.NewWithValidTestData<OrgHeader>();
					client.OH_Code = "client";
					var vessel = RefVessel.LookupVesselByName("NORDCLOUD", Factory).First();
					JobVoyage voyage = setupFactory.New<JobVoyage>();
					voyage.JV_RV_NKVessel = vessel.RV_FK;
					voyage.JV_VoyageFlight = "001";
					voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
					VoyageDestination destination = voyage.Destinations.AddNew();
					destination.JB_RL_NKPortOfDischarge = "AUBNE";
					BillOfLading bill = setupFactory.New<BillOfLading>();
					bill.JS_UniqueConsignRef = "V00000100";
					bill.JS_JX = voyage.Sailings[0].PK;
					bill.JS_OH_DeliveryAgent = principal.PK;
					bill.ConsigneeDocumentaryAddress.OrganisationPK = client.PK;
					BillOfLadingContainer container = bill.RealContainers.AddNew();
					container.JC_ContainerNum = "TEST4100013";
					container.JC_RC = setupFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
					container.JC_EmptyReturnedBy = new ZDateTime(2010, 10, 12);
					ContainerMovement movement = container.Stock.Movements.AddNew();
					movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
					movement.E9_MovementDate = new ZDateTime(2010, 10, 12);
					movement.E9_JV = voyage.PK;
					setupFactory.Save();
					containerPK = container.PK;
					destinationPK = destination.PK;
				}

				{
					// This *MUST* be done in a sepporate factory for the test to be effective.
					BusinessObjectFactory setupFactory2 = new BusinessObjectFactory();
					VoyageDestination destination = setupFactory2.Load<VoyageDestination>(destinationPK);
					destination.JB_AvailabilityDate = new ZDateTime(2010, 10, 1);
					setupFactory2.Save();
				}

				const string expected = "INFO: Processing containers on [HL V00000100]:\r\n" + "INFO: The return required by date for [HL TEST4100013] was updated from '12-Oct-10' to '10-Oct-10'.\r\n" + "INFO: Updating the detention days for the [HL Yard Gate In] movement from 0 to 2.\r\n";
				BillOfLadingContainer target = Factory.Load<BillOfLadingContainer>(containerPK);
				ApplyApplicator(new BusinessObject[] { target }, expected);
				AssertEquals("JC_EmptyReturnedBy", new ZDateTime(2010, 10, 10), target.JC_EmptyReturnedBy);
			}
		}

		public void TestUpdateReturnByDateWithDetentionDatesUpdater()
		{
			using (AgencyRegistry.Instance.UpdateEmptyReturnByWhenAvailabilityDatesChange.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				var originalReturnByDate = new ZDateTime(2010, 10, 12);
				var newReturnByDate = new ZDateTime(2010, 10, 1);
				ZGuid containerPK;
				JobSailing sailing;
				#region Set up in new Factory
				var setUpFactory = new BusinessObjectFactory();
				var vessel = RefVessel.LookupVesselByName("NORDCLOUD", Factory).First();
				var voyage = setUpFactory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "001";
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
				var voyageDestination = voyage.Destinations.AddNew();
				voyageDestination.JB_RL_NKPortOfDischarge = "AUBNE";
				voyage.GenerateSailings();
				var bill = setUpFactory.New<BillOfLading>();
				bill.JS_UniqueConsignRef = "V00000100";
				bill.JS_JX = voyage.Sailings[0].PK;
				bill.JS_OH_DeliveryAgent = setUpFactory.NewWithValidTestData<OrgHeader>().PK;
				bill.ConsigneeDocumentaryAddress.OrganisationPK = setUpFactory.NewWithValidTestData<OrgHeader>().PK;
				var container = bill.RealContainers.AddNew();
				container.JC_ContainerNum = "SUGA0000000";
				container.JC_RC = setUpFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				container.JC_EmptyReturnedBy = originalReturnByDate;
				var movement = container.Stock.Movements.AddNew();
				movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
				movement.E9_MovementDate = originalReturnByDate;
				movement.E9_JV = voyage.PK;
				setUpFactory.Save();
				containerPK = container.PK;
				sailing = voyage.Sailings[0];
				#endregion
				var applicationFactory = new BusinessObjectFactory();
				var reloadedSailing = applicationFactory.Load<JobSailing>(sailing.PK);
				AssertNotNull("Pre-condition: a sailing should have been created", reloadedSailing);
				AssertNotNull("Pre-condition: sailing should have a destination", reloadedSailing.Destination);
				reloadedSailing.Destination.JB_AvailabilityDate = newReturnByDate;
				new DetentionDatesUpdater().UpdateContainerDetentionDateFromSailing(reloadedSailing);
				applicationFactory.Save();
				var reloadedContainer = Factory.Load<BillOfLadingContainer>(containerPK);
				AssertNotNull(reloadedContainer);
				AssertEquals("Date should have been updated", newReturnByDate.AddDays(9), reloadedContainer.JC_EmptyReturnedBy);
				var expectedReference = @"INFO: Processing containers on [HL V00000100]:
INFO: The return required by date for [HL SUGA0000000] was updated from '12-Oct-10' to '10-Oct-10'.
INFO: Updating the detention days for the [HL Yard Gate In] movement from 0 to 2.";
				var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.EditedARecord.Code);
				logQuery.AddToFilter(StmALogSchema.SL_Reference, expectedReference);
				var lastEditLog = reloadedContainer.Logs.Find(logQuery);
				AssertNotNull("A log was created with the expected text", lastEditLog);
			}
		}

		public void TestSetup()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				ZGuid containerPK;
				ZGuid destinationPK;
				{
					BusinessObjectFactory setupFactory = new BusinessObjectFactory();
					OrgHeader principal = setupFactory.NewWithValidTestData<OrgHeader>();
					principal.OH_Code = "principal";
					OrgHeader client = setupFactory.NewWithValidTestData<OrgHeader>();
					client.OH_Code = "client";
					var vessel = RefVessel.LookupVesselByName("NORDCLOUD", Factory).First();
					JobVoyage voyage = setupFactory.New<JobVoyage>();
					voyage.JV_RV_NKVessel = vessel.RV_FK;
					voyage.JV_VoyageFlight = "001";
					voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
					VoyageDestination destination = voyage.Destinations.AddNew();
					destination.JB_RL_NKPortOfDischarge = "AUBNE";
					BillOfLading bill = setupFactory.New<BillOfLading>();
					bill.JS_UniqueConsignRef = "V00000100";
					bill.JS_JX = voyage.Sailings[0].PK;
					bill.JS_OH_DeliveryAgent = principal.PK;
					bill.ConsigneeDocumentaryAddress.OrganisationPK = client.PK;
					BillOfLadingContainer container = bill.RealContainers.AddNew();
					container.JC_ContainerNum = "TEST4100013";
					container.JC_RC = setupFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
					container.JC_EmptyReturnedBy = ZDateTime.Empty;
					ContainerMovement movement = container.Stock.Movements.AddNew();
					movement.E9_MovementType = ContainerMovementTypes.Codes.ReShipRequested;
					movement.E9_MovementDate = new ZDateTime(2010, 10, 12);
					movement.E9_JV = voyage.PK;
					ContainerDetention detention = setupFactory.New<ContainerDetention>();
					detention.NC_JobNumber = "DI00001000";
					detention.NC_DetentionType = DetentionInvoiceType.Codes.Import;
					detention.NC_OH_Principal = principal.PK;
					detention.NC_OH_Client = client.PK;
					detention.Movements.Add(movement);
					JobHeader job = new JobHeader.Loader(detention).TryLoadOrCreate();
					job.JH_GE = GlbDepartment.CurrentDepartment.PK;
					AddCharge(job, "FRT", TransactionLineTypes.Revenue);
					setupFactory.Save();
					containerPK = container.PK;
					destinationPK = destination.PK;
				}

				{
					// This *MUST* be done in a sepporate factory for the test to be effective.
					BusinessObjectFactory setupFactory2 = new BusinessObjectFactory();
					VoyageDestination destination = setupFactory2.Load<VoyageDestination>(destinationPK);
					destination.JB_AvailabilityDate = new ZDateTime(2010, 10, 1);
					setupFactory2.Save();
				}

				const string expected = "INFO: Processing containers on [HL V00000100]:\r\n" + "INFO: The return required by date for [HL TEST4100013] was updated from empty to '10-Oct-10'.\r\n" + "WARNING: Cannot update the detention days for the [HL Re-Ship Requested] movement from 0 to 2 as it already has a posted invoice on [HL DI00001000].\r\n" + "";
				BillOfLadingContainer target = Factory.Load<BillOfLadingContainer>(containerPK);
				ApplyApplicator(new BusinessObject[] { target }, expected);
				AssertEquals("JC_EmptyReturnedBy", new ZDateTime(2010, 10, 10), target.JC_EmptyReturnedBy);
			}
		}

		public void TestUpdateUnchanged()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				ZGuid containerPK;
				{
					BusinessObjectFactory setupFactory = new BusinessObjectFactory();
					OrgHeader principal = setupFactory.NewWithValidTestData<OrgHeader>();
					principal.OH_Code = "principal";
					OrgHeader client = setupFactory.NewWithValidTestData<OrgHeader>();
					client.OH_Code = "client";
					var vessel = RefVessel.LookupVesselByName("NORDCLOUD", Factory).First();
					JobVoyage voyage = setupFactory.New<JobVoyage>();
					voyage.JV_RV_NKVessel = vessel.RV_FK;
					voyage.JV_VoyageFlight = "001";
					voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
					VoyageDestination destination = voyage.Destinations.AddNew();
					destination.JB_RL_NKPortOfDischarge = "AUBNE";
					destination.JB_AvailabilityDate = new ZDateTime(2010, 10, 1);
					BillOfLading bill = setupFactory.New<BillOfLading>();
					bill.JS_UniqueConsignRef = "V00000100";
					bill.JS_JX = voyage.Sailings[0].PK;
					bill.JS_OH_DeliveryAgent = principal.PK;
					bill.ConsigneeDocumentaryAddress.OrganisationPK = client.PK;
					BillOfLadingContainer container = bill.RealContainers.AddNew();
					container.JC_ContainerNum = "TEST4100013";
					container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
					container.JC_EmptyReturnedBy = new ZDateTime(2010, 10, 10);
					ContainerMovement movement = container.Stock.Movements.AddNew();
					movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
					movement.E9_MovementDate = new ZDateTime(2010, 10, 12);
					movement.E9_JV = voyage.PK;
					ContainerDetention detention = setupFactory.New<ContainerDetention>();
					detention.NC_JobNumber = "DI00001000";
					detention.NC_OH_Principal = principal.PK;
					detention.NC_OH_Client = client.PK;
					detention.Movements.Add(movement);
					JobHeader job = new JobHeader.Loader(detention).TryLoadOrCreate();
					job.JH_GE = GlbDepartment.CurrentDepartment.PK;
					AddCharge(job, "FRT", TransactionLineTypes.WIP);
					setupFactory.Save();
					containerPK = container.PK;
				}

				const string expected = "INFO: Processing containers on [HL V00000100]:\r\n" + "INFO: The return required by date for [HL TEST4100013] is already correct as '10-Oct-10'.\r\n" + "WARNING: Updating the detention days for the [HL Yard Gate In] movement from 0 to 2 however it is currently attached to the detention invoice [HL DI00001000].\r\n" + "";
				BillOfLadingContainer target = Factory.Load<BillOfLadingContainer>(containerPK);
				ApplyApplicator(new BusinessObject[] { target }, expected);
				AssertEquals("JC_EmptyReturnedBy", new ZDateTime(2010, 10, 10), target.JC_EmptyReturnedBy);
			}
		}

		[ExpectNoExceptions]
		public void TestUpdateContainer_ContainerNumIsNull_ShouldNotThrowException()
		{
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_UniqueConsignRef = "V00000100";
			BillOfLadingContainer container = billOfLading.RealContainers.AddNew();
			container.JC_ContainerNum = ZString.Empty;
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_EmptyReturnedBy = new ZDateTime(2013, 03, 07);
			Factory.Save();
			const string expected = "INFO: Processing containers on [HL V00000100]:\r\n" + "WARNING: Was not able to determine the return required by date for [HL 20GP (1)]. Leaving the value as '07-Mar-13'.\r\n" + "";
			var target = Factory.Load<BillOfLadingContainer>(container.PK);
			ApplyApplicator(new BusinessObject[] { target }, expected);
		}

		#region Implementation
		static void AddCharge(JobHeader header, string chargeCode, string lineType)
		{
			ZQuery chargeCodeFilter = new ZQuery();
			chargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
			chargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_GC, header.JH_GC);
			AccChargeCode accChargeCode = header.Factory.LoadTop1<AccChargeCode>(chargeCodeFilter);
			JobCharge charge = (JobCharge)((IBusinessObjectCollection)header["Charges"]).AddNew();
			charge.JR_AC = accChargeCode.PK;
			if (!string.IsNullOrEmpty(lineType))
			{
				AccTransactionHeader invoice = header.Factory.NewWithValidTestData<AccTransactionHeader>();
				invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				invoice.AH_TransactionType = TransactionTypes.Invoice;
				AccTransactionLines line = header.Factory.New<AccTransactionLines>();
				line.AL_LineType = lineType;
				line.AL_AH = invoice.PK;
				line.AL_AC = accChargeCode.PK;
				line.AL_GE = header.JH_GE;
				line.AL_GB = header.JH_GB;
				line.AL_JH = header.PK;
				charge.JR_AL_ARLine = line.PK;
			}
		}

		static ZGuid[] CreateShipmentsForPerformanceTest(int shipmentCount)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZDateTime now = ZDateTime.Now;
			ZGuid[] shipmentPKs = new ZGuid[shipmentCount];
			var vessel1 = RefVessel.LookupVesselByName("BANOWATI", factory).First();
			var vessel2 = RefVessel.LookupVesselByName("MAJAPAHIT", factory).First();
			var vessel3 = RefVessel.LookupVesselByName("NORDCLOUD", factory).First();
			var vessel4 = RefVessel.LookupVesselByName("CONDOR", factory).First();
			JobVoyage voyage1 = factory.New<JobVoyage>();
			voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			voyage1.JV_VoyageFlight = "1";
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			VoyageDestination destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUBNE";
			destination1.JB_AvailabilityDate = now.AddDays(-20);
			JobVoyage voyage2 = factory.New<JobVoyage>();
			voyage2.JV_RV_NKVessel = vessel2.RV_FK;
			voyage2.JV_VoyageFlight = "2";
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "SGSIN";
			VoyageDestination destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "AUBNE";
			destination2.JB_AvailabilityDate = now.AddDays(-15);
			JobVoyage voyage3 = factory.New<JobVoyage>();
			voyage3.JV_RV_NKVessel = vessel3.RV_FK;
			voyage3.JV_VoyageFlight = "3";
			voyage3.Origins.AddNew().JA_RL_NKPortOfLoading = "MYBAG";
			VoyageDestination destination3 = voyage3.Destinations.AddNew();
			destination3.JB_RL_NKPortOfDischarge = "AUSYD";
			destination3.JB_AvailabilityDate = now.AddDays(-10);
			JobVoyage voyage4 = factory.New<JobVoyage>();
			voyage4.JV_RV_NKVessel = vessel4.RV_FK;
			voyage4.JV_VoyageFlight = "4";
			voyage4.Origins.AddNew().JA_RL_NKPortOfLoading = "HKHKG";
			VoyageDestination destination4 = voyage4.Destinations.AddNew();
			destination4.JB_RL_NKPortOfDischarge = "AUSYD";
			destination4.JB_AvailabilityDate = ZDateTime.Empty;
			OrgHeader client1 = factory.NewWithValidTestData<OrgHeader>();
			client1.OH_Code = "client1";
			OrgHeader client2 = factory.NewWithValidTestData<OrgHeader>();
			client2.OH_Code = "client2";
			OrgHeader client3 = factory.NewWithValidTestData<OrgHeader>();
			client2.OH_Code = "client3";
			OrgHeader principal1 = factory.NewWithValidTestData<OrgHeader>();
			principal1.OH_Code = "principal1";
			OrgHeader principal2 = factory.NewWithValidTestData<OrgHeader>();
			principal2.OH_Code = "principal2";
			OrgContainerDetention det1 = principal1.CarrierContainerPenalties.AddNew();
			det1.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			det1.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			det1.PD_FreeDays = 5;
			OrgContainerDetention det2 = client2.CarrierContainerPenalties.AddNew();
			det1.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			det1.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			det2.PD_FreeDays = 7;
			JobSailing[] sailings = { voyage1.Sailings[0], voyage2.Sailings[0], voyage3.Sailings[0], voyage4.Sailings[0] };
			OrgHeader[] clients = { client1, client2, client3 };
			OrgHeader[] principals = { principal1, principal2 };
			string[] containerNumbers = { "TEST4100013", "TEST4100029", "TEST4100034", "TEST4100040", "TEST4100055" };
			RefContainer[] containerTypes = factory.Load<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, new ZString[] { "20GP", "40GP", "20RE", "40RE" }));
			for (int i = 0; i < shipmentPKs.Length; i++)
			{
				BillOfLading shipment = factory.New<BillOfLading>();
				shipment.JS_OH_DeliveryAgent = principals[i % principals.Length].PK;
				shipment.ConsignorDocumentaryAddress.OrganisationPK = clients[i % clients.Length].PK;
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = clients[(i + 1) % clients.Length].PK;
				shipment.JS_JX = sailings[i % sailings.Length].PK;
				shipment.JS_RL_NKOrigin = shipment.JS_NKLoadPort;
				shipment.JS_RL_NKDestination = shipment.JS_NKDischargePort;
				BillOfLadingContainer container1 = shipment.RealContainers.AddNew();
				container1.JC_ContainerNum = containerNumbers[i % containerNumbers.Length];
				container1.JC_RC = containerTypes[i % containerTypes.Length].PK;
				container1.JC_EmptyReturnedBy = now;
				BillOfLadingContainer container2 = shipment.RealContainers.AddNew();
				container2.JC_ContainerNum = containerNumbers[(i + 1) % containerNumbers.Length];
				container2.JC_RC = containerTypes[(i + 1) % containerTypes.Length].PK;
				ContainerMovement movement1 = container1.Stock.Movements.AddNew();
				movement1.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
				movement1.E9_MovementDate = now;
				movement1.E9_JV = shipment.Sailing.Voyage.PK;
				movement1.E9_DetentionDays = 90;
				ContainerMovement movement2 = container2.Stock.Movements.AddNew();
				movement2.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
				movement2.E9_MovementDate = now;
				movement2.E9_JV = shipment.Sailing.Voyage.PK;
				movement2.E9_DetentionDays = 90;
				ContainerDetention detention = factory.New<ContainerDetention>();
				detention.NC_OH_Principal = shipment.JS_OH_DeliveryAgent;
				detention.NC_OH_Client = shipment.ConsigneeDocumentaryAddress.OrganisationPK;
				detention.Movements.Add(movement2);
				shipmentPKs[i] = shipment.PK;
			}

			factory.Save();
			return shipmentPKs;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UpdateReturnByApplicator();
		}
		#endregion
	}
}
