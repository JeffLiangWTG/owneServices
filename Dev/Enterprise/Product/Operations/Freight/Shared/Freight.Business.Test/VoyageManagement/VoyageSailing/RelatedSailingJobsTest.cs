using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class RelatedSailingJobsTest : TestCaseWithFactory
	{
		#region Consol / Shipment

		public void TestLoadRelatedImportConsolJobs_ForwardingConsol()
		{
			AssertLoadRelatedImportConsolJobs(JobsForTesting.ImportConsol);
		}

		public void TestLoadRelatedImportConsolJobs_CommonConsol()
		{
			AssertLoadRelatedImportConsolJobs(JobsForTesting.ImportCommonConsol);
		}

		void AssertLoadRelatedImportConsolJobs(CommonConsol importConsol)
		{
			importConsol.JK_UniqueConsignRef = "C00005001";
			JobsForTesting.ExportConsol.JK_UniqueConsignRef = "C00005002";
			importConsol.Transports[0].JW_JX = ImportSailing.PK;
			JobsForTesting.ExportConsol.Transports[0].JW_JX = ImportSailing.PK;
			Factory.Save();

			ImportSailing.Voyage.ParentConsol = importConsol;
			ImportSailing.Destination.JB_E_ARV = new ZDateTime(2000, 1, 11);
			JobSailingRelatedJob[] jobs = RelatedSailingJobs.LoadRelatedConsolJobs(ChangeLogger.LogVoyageDateChanges(Voyage));
			AssertEquals("Loads as ForwardingConsol so shipments are loaded as ForwardingShipments", ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>(), jobs[0].Job.GetType());
			AssertEquals("1 consol found", 1, jobs.Length);
			AssertEquals("Import consols should be loaded from an ETA change", jobs[0].Job.PK, importConsol.PK);
		}

		public void TestLoadRelatedExportConsolJobs()
		{
			JobsForTesting.ImportConsol.JK_UniqueConsignRef = "C00005001";
			JobsForTesting.ExportConsol.JK_UniqueConsignRef = "C00005002";
			JobsForTesting.ImportConsol.Transports[0].JW_JX = ExportSailing.PK;
			JobsForTesting.ExportConsol.Transports[0].JW_JX = ExportSailing.PK;
			Factory.Save();

			ExportSailing.Origin.JA_E_DEP = new ZDateTime(2000, 1, 11);
			JobSailingRelatedJob[] jobs = RelatedSailingJobs.LoadRelatedConsolJobs(ChangeLogger.LogVoyageDateChanges(Voyage));
			AssertRelatedJobsLoaded("Export consols should be loaded from an ETD change", jobs, JobsForTesting.ExportConsol);
		}

		public void TestLoadRelatedConsolAndShipmentJobs()
		{
			CommonConsol consol1 = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol1.JK_IsCFS = true;
			consol1.JK_UniqueConsignRef = "C00005001";
			consol1.Transports.AddNew();
			consol1.Transports[0].JW_IsLinked = true;
			consol1.Transports[0].JW_JX = ImportSailing.PK;
			consol1.JK_RL_NKLoadPort = "MYPKG";
			consol1.JK_RL_NKDischargePort = "AUSYD";

			CommonShipment shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_IsCFSRegistered = true;
			Factory.Save();
			CommonShipment shipment2 = consol1.Shipments.AddNew();
			shipment2.JS_IsCFSRegistered = true;
			Factory.Save();

			CommonConsol consol2 = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol2.JK_IsCFS = true;
			consol2.JK_UniqueConsignRef = "C00005002";
			consol2.Transports.AddNew();
			consol2.Transports[0].JW_IsLinked = true;
			consol2.Transports[0].JW_JX = ImportSailing.PK;
			consol2.JK_RL_NKLoadPort = "MYPKG";
			consol2.JK_RL_NKDischargePort = "AUSYD";

			CommonShipment shipment3 = consol2.Shipments.AddNew();
			shipment3.JS_IsCFSRegistered = true;
			Factory.Save();
			CommonShipment shipment4 = consol2.Shipments.AddNew();
			shipment4.JS_IsCFSRegistered = true;
			Factory.Save();

			CommonConsol consol3 = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol3.JK_IsCFS = true;
			consol3.JK_UniqueConsignRef = "C00005003";
			consol3.Transports.AddNew();
			consol3.Transports[0].JW_IsLinked = true;
			consol3.Transports[0].JW_JX = ImportSailing.PK;
			consol3.JK_RL_NKLoadPort = "MYPKG";
			consol3.JK_RL_NKDischargePort = "AUSYD";

			CommonShipment shipment5 = Factory.NewWithValidTestData<CommonShipment>();
			shipment5.JS_IsCFSRegistered = true;
			shipment5.Transports.AddNew();
			shipment5.Transports[0].JW_IsLinked = true;
			shipment5.Transports[0].JW_JX = ImportSailing.PK;
			shipment5.JS_RL_NKOrigin = "MYPKG";
			shipment5.JS_RL_NKDestination = "AUSYD";

			CommonShipment shipment6 = Factory.NewWithValidTestData<CommonShipment>();
			shipment6.JS_IsCFSRegistered = true;
			shipment6.Transports.AddNew();
			shipment6.Transports[0].JW_IsLinked = true;
			shipment6.Transports[0].JW_JX = ExportSailing.PK;
			shipment6.JS_RL_NKOrigin = "AUSYD";
			shipment6.JS_RL_NKDestination = "USLAX";

			Factory.Save();

			ImportSailing.Destination.JB_E_ARV = new ZDateTime(2000, 1, 11);
			JobSailingRelatedJob[] jobs = RelatedSailingJobs.LoadRelatedConsolAndShipmentJobs(ChangeLogger.LogVoyageDateChanges(Voyage));

			AssertEquals("8 jobs expected (3 consols, 5 shipments)", 8, jobs.Length);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4, consol3, shipment5", consol1.PK, jobs[0].Job.PK);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4, consol3, shipment5", shipment1.PK, jobs[1].Job.PK);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4, consol3, shipment5", shipment2.PK, jobs[2].Job.PK);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4, consol3, shipment5", consol2.PK, jobs[3].Job.PK);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4, consol3, shipment5", shipment3.PK, jobs[4].Job.PK);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4, consol3, shipment5", shipment4.PK, jobs[5].Job.PK);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4, consol3, shipment5", consol3.PK, jobs[6].Job.PK);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4, consol3, shipment5", shipment5.PK, jobs[7].Job.PK);

			ExportSailing.Origin.JA_E_DEP = new ZDateTime(2000, 1, 12);

			jobs = RelatedSailingJobs.LoadRelatedConsolAndShipmentJobs(ChangeLogger.LogVoyageDateChanges(Voyage));

			AssertEquals("9 jobs expected (3 consols, 6 shipments)", 9, jobs.Length);
			AssertEquals(consol1.PK, jobs[0].Job.PK);
			AssertEquals(shipment1.PK, jobs[1].Job.PK);
			AssertEquals(shipment2.PK, jobs[2].Job.PK);
			AssertEquals(consol2.PK, jobs[3].Job.PK);
			AssertEquals(shipment3.PK, jobs[4].Job.PK);
			AssertEquals(shipment4.PK, jobs[5].Job.PK);
			AssertEquals(consol3.PK, jobs[6].Job.PK);
			AssertEquals(shipment5.PK, jobs[7].Job.PK);
			AssertEquals(shipment6.PK, jobs[8].Job.PK);
		}

		public void TestLoadRelatedConsolAndShipmentJobs_Domestic()
		{
			CommonConsol consol1 = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol1.JK_IsCFS = true;
			consol1.JK_UniqueConsignRef = "C00005001";
			consol1.Transports.AddNew();
			consol1.Transports[0].JW_JX = DomesticSailing.PK;
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_RL_NKDischargePort = "AUSYD";

			CommonShipment shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_IsCFSRegistered = true;
			Factory.Save();
			CommonShipment shipment2 = consol1.Shipments.AddNew();
			shipment2.JS_IsCFSRegistered = true;
			Factory.Save();

			CommonConsol consol2 = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol2.JK_IsCFS = true;
			consol2.JK_UniqueConsignRef = "C00005002";
			consol2.Transports.AddNew();
			consol2.Transports[0].JW_JX = ImportSailing.PK;
			consol2.JK_RL_NKLoadPort = "MYPKG";
			consol2.JK_RL_NKDischargePort = "AUSYD";

			CommonShipment shipment3 = consol2.Shipments.AddNew();
			shipment3.JS_IsCFSRegistered = true;
			Factory.Save();
			CommonShipment shipment4 = consol2.Shipments.AddNew();
			shipment4.JS_IsCFSRegistered = true;
			Factory.Save();

			CommonConsol consol3 = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol3.JK_IsCFS = true;
			consol3.JK_UniqueConsignRef = "C00005003";
			consol3.Transports.AddNew();
			consol3.Transports[0].JW_JX = ExportSailing.PK;
			consol3.JK_RL_NKLoadPort = "AUSYD";
			consol3.JK_RL_NKDischargePort = "USALX";

			CommonShipment shipment5 = Factory.NewWithValidTestData<CommonShipment>();
			shipment5.JS_IsCFSRegistered = true;
			shipment5.Transports.AddNew();
			shipment5.Transports[0].JW_JX = ExportSailing.PK;
			shipment5.JS_RL_NKOrigin = "AUSYD";
			shipment5.JS_RL_NKDestination = "USALX";

			Factory.Save();

			ImportSailing.Destination.JB_E_ARV = new ZDateTime(2000, 1, 11);
			JobSailingRelatedJob[] jobs = RelatedSailingJobs.LoadRelatedConsolAndShipmentJobs(ChangeLogger.LogVoyageDateChanges(Voyage));
			AssertEquals("6 jobs expected (2 consols, 4 shipments)", 6, jobs.Length);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4", consol1.PK, jobs[0].Job.PK);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4", shipment1.PK, jobs[1].Job.PK);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4", shipment2.PK, jobs[2].Job.PK);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4", consol2.PK, jobs[3].Job.PK);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4", shipment3.PK, jobs[4].Job.PK);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4", shipment4.PK, jobs[5].Job.PK);
		}

		public void TestLoadRelatedConsolAndShipmentJobs_WithBranchHomePort()
		{
			CommonConsol consol1 = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol1.JK_UniqueConsignRef = "C00005001";
			consol1.Transports.AddNew();
			consol1.Transports[0].JW_JX = ExportSailing.PK;
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "USLAX";

			CommonShipment shipment1 = consol1.Shipments.AddNew();
			Factory.Save();
			CommonShipment shipment2 = consol1.Shipments.AddNew();
			Factory.Save();

			ExportSailing.Destination.JB_E_ARV = new ZDateTime(2000, 1, 11);
			JobSailingRelatedJob[] jobs = RelatedSailingJobs.LoadRelatedConsolAndShipmentJobs(ChangeLogger.LogVoyageDateChanges(Voyage));
			AssertEquals("0 jobs expected (0 consols, 0 shipments)", 0, jobs.Length);

			GlbCompany uScompany = Factory.New<GlbCompany>();
			GlbBranch uSBranch = uScompany.Branches.AddNew();
			uSBranch.GB_RL_NKHomePort = "USLAS";
			jobs = RelatedSailingJobs.LoadRelatedConsolAndShipmentJobs(ChangeLogger.LogVoyageDateChanges(Voyage));
			AssertEquals("3 jobs expected (1 consols, 2 shipments)", 3, jobs.Length);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4, consol3", consol1.PK, jobs[0].Job.PK);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4, consol3", shipment1.PK, jobs[1].Job.PK);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4, consol3", shipment2.PK, jobs[2].Job.PK);
		}

		public void TestLoadRelatedConsolAndShipmentJobs_WithBranchAdditionalPort()
		{
			CommonConsol consol1 = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol1.JK_UniqueConsignRef = "C00005001";
			consol1.Transports.AddNew();
			consol1.Transports[0].JW_JX = ExportSailing.PK;
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "USLAX";

			CommonShipment shipment1 = consol1.Shipments.AddNew();
			Factory.Save();
			CommonShipment shipment2 = consol1.Shipments.AddNew();
			Factory.Save();

			ExportSailing.Destination.JB_E_ARV = new ZDateTime(2000, 1, 11);
			JobSailingRelatedJob[] jobs = RelatedSailingJobs.LoadRelatedConsolAndShipmentJobs(ChangeLogger.LogVoyageDateChanges(Voyage));
			AssertEquals("0 jobs expected (0 consols, 0 shipments)", 0, jobs.Length);

			GlbCompany uScompany = Factory.New<GlbCompany>();
			GlbBranch uSBranch = uScompany.Branches.AddNew();
			uSBranch.GB_RL_NKHomePort = "NZAKL";
			jobs = RelatedSailingJobs.LoadRelatedConsolAndShipmentJobs(ChangeLogger.LogVoyageDateChanges(Voyage));
			AssertEquals("0 jobs expected (0 consols, 0 shipments)", 0, jobs.Length);

			GlbBranchExtraPorts port = uSBranch.ExtraPorts.AddNew();
			port.GY_RL_NKAdditionalBranchRelatedPort = "USLAS";
			jobs = RelatedSailingJobs.LoadRelatedConsolAndShipmentJobs(ChangeLogger.LogVoyageDateChanges(Voyage));
			AssertEquals("3 jobs expected (1 consols, 2 shipments)", 3, jobs.Length);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4, consol3", consol1.PK, jobs[0].Job.PK);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4, consol3", shipment1.PK, jobs[1].Job.PK);
			AssertEquals("Expected (in order), consol1 shipment1 shipment2, consol2 shipment3 shipment4, consol3", shipment2.PK, jobs[2].Job.PK);
		}

		#endregion

		#region Declaration

		public void TestLoadRelatedDeclarationJobs()
		{
			FreightConfigurationRegistry.Instance.AutoDeliverDelayAlertDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DelayAlertDeliveryRuleCollection());
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(JobDeclarationSchema.JE_RL_NKOrigin, JobDeclarationSchema.JE_RL_NKFinalDestination);
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(JobDeclarationSchema.JE_RL_NKPortOfLoading, JobDeclarationSchema.JE_RL_NKPortOfArrival);
		}

		public void TestLoadRelatedDeclarationJobs_ForAir()
		{
			FreightConfigurationRegistry.Instance.AutoDeliverDelayAlertDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DelayAlertDeliveryRuleCollection());

			BusinessObject decoyDeclaration = CreateImportDeclaration(Core.Constants.TransportModes.Air);
			decoyDeclaration[JobDeclarationSchema.JE_VoyageFlightNo] = "QF323";
			decoyDeclaration[JobDeclarationSchema.JE_ExportDate] = new ZDateTime(1999, 12, 31, 0, 0, 0);
			decoyDeclaration[JobDeclarationSchema.JE_DateOfArrival] = new ZDateTime(2000, 1, 1, 0, 0, 0);
			BusinessObject declaration = CreateImportDeclaration(Core.Constants.TransportModes.Air);
			declaration[JobDeclarationSchema.JE_VoyageFlightNo] = "SQ23";
			declaration[JobDeclarationSchema.JE_ExportDate] = new ZDateTime(2000, 1, 2, 0, 0, 0);
			declaration[JobDeclarationSchema.JE_DateOfArrival] = new ZDateTime(2000, 1, 3, 0, 0, 0);

			ITransportParent decoyTransportParent = (ITransportParent)decoyDeclaration;
			Transport decoyTransport = decoyTransportParent.Transports[0];
			ITransportParent rightTransportParent = (ITransportParent)declaration;
			Transport rightTransport = rightTransportParent.Transports[0];

			Factory.Save();

			AssertNotEquals("Precondition: decoyTransport should have a linked Voyage", ZGuid.Empty, decoyTransport.JW_JX);
			AssertNotEquals("Precondition: rightTransport should have a linked Voyage", ZGuid.Empty, rightTransport.JW_JX);
			AssertNotEquals("Precondition: Transports should be linked, but to different voyages.", decoyTransport.JW_JX, rightTransport.JW_JX);

			JobSailing sailing = rightTransport.Sailing;
			sailing.Origin.JA_E_DEP = new ZDateTime(2000, 2, 2, 0, 0, 0);
			sailing.Destination.JB_E_ARV = new ZDateTime(2000, 3, 3, 0, 0, 0);

			JobSailingRelatedJob[] jobs = RelatedSailingJobs.LoadRelatedDeclarationJobs(ChangeLogger.LogVoyageDateChanges(sailing.Voyage));
			AssertRelatedJobsLoaded("Only the declaration job matching the flight/load/discharge/departure date loaded", jobs, declaration);
		}

		public void TestLoadRelatedDeclarationJobs_ForRoad()
		{
			FreightConfigurationRegistry.Instance.AutoDeliverDelayAlertDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DelayAlertDeliveryRuleCollection());

			BusinessObject decoyDeclaration = CreateImportDeclaration(Core.Constants.TransportModes.Road);
			decoyDeclaration[JobDeclarationSchema.JE_ExportDate] = new ZDateTime(2000, 1, 1, 0, 0, 0);
			decoyDeclaration[JobDeclarationSchema.JE_DateOfArrival] = new ZDateTime(2000, 1, 2, 0, 0, 0);

			BusinessObject declaration = CreateImportDeclaration(Core.Constants.TransportModes.Road);
			declaration[JobDeclarationSchema.JE_ExportDate] = new ZDateTime(2000, 1, 3, 0, 0, 0);
			declaration[JobDeclarationSchema.JE_DateOfArrival] = new ZDateTime(2000, 1, 4, 0, 0, 0);

			ITransportParent decoyTransportParent = (ITransportParent)decoyDeclaration;
			Transport decoyTransport = decoyTransportParent.Transports[0];

			ITransportParent rightTransportParent = (ITransportParent)declaration;
			Transport rightTransport = rightTransportParent.Transports[0];

			Factory.Save();

			AssertNotEquals("Precondition: decoyTransport should have a linked Voyage", ZGuid.Empty, decoyTransport.JW_JX);
			AssertNotEquals("Precondition: rightTransport should have a linked Voyage", ZGuid.Empty, rightTransport.JW_JX);
			AssertNotEquals("Precondition: Transports should be linked, but to different voyages.", decoyTransport.JW_JX, rightTransport.JW_JX);

			JobSailing sailing = rightTransport.Sailing;
			sailing.Origin.JA_E_DEP = new ZDateTime(2000, 2, 2, 23, 0, 0);
			sailing.Destination.JB_E_ARV = new ZDateTime(2000, 3, 3, 0, 0, 0);

			JobSailingRelatedJob[] jobs = RelatedSailingJobs.LoadRelatedDeclarationJobs(ChangeLogger.LogVoyageDateChanges(sailing.Voyage));
			AssertRelatedJobsLoaded("Only the declaration job matching the flight/load/discharge/departure date loaded", jobs, declaration);
		}

		BusinessObject CreateImportDeclaration(string transportMode)
		{
			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
			declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_TransportMode] = transportMode;
			declaration[JobDeclarationSchema.JE_VoyageFlightNo] = Voyage.JV_VoyageFlight;
			declaration[JobDeclarationSchema.JE_MessageType] = "IMP";
			declaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = ImportSailing.Origin.JA_RL_NKPortOfLoading;
			declaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = ImportSailing.Destination.JB_RL_NKPortOfDischarge;
			return declaration;
		}

		void TestSendEmailIfRequired_IncludeDeclarationJobsAffected(SchemaStringColumn originPort, SchemaStringColumn destinationPort)
		{
			const bool export = false;
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(originPort, destinationPort, JobVoyOriginSchema.JA_E_DEP, export);
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(originPort, destinationPort, JobVoyOriginSchema.JA_A_DEP, export);
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(originPort, destinationPort, JobVoyOriginSchema.JA_CutOff, export);
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(originPort, destinationPort, JobVoyOriginSchema.JA_ReceivalCommences, export);

			const bool import = true;
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(originPort, destinationPort, JobVoyDestinationSchema.JB_E_ARV, import);
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(originPort, destinationPort, JobVoyDestinationSchema.JB_A_ARV, import);
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(originPort, destinationPort, JobVoyDestinationSchema.JB_AvailabilityDate, import);
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(originPort, destinationPort, JobVoyDestinationSchema.JB_StorageDate, import);

			// expect no exception
			ImportSailing.Origin.JA_A_DEP = ZDateTime.Empty;
			ImportSailing.Origin.JA_E_DEP = ZDateTime.Empty;
			ImportSailing.Destination.JB_A_ARV = ZDateTime.Empty;
			ImportSailing.Destination.JB_E_ARV = ZDateTime.Empty;
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(originPort, destinationPort, JobVoyDestinationSchema.JB_E_ARV, import);
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(originPort, destinationPort, JobVoyDestinationSchema.JB_A_ARV, import);
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(originPort, destinationPort, JobVoyDestinationSchema.JB_AvailabilityDate, import);
			TestSendEmailIfRequired_IncludeDeclarationJobsAffected(originPort, destinationPort, JobVoyDestinationSchema.JB_StorageDate, import);
		}

		void TestSendEmailIfRequired_IncludeDeclarationJobsAffected(SchemaStringColumn originPort, SchemaStringColumn destinationPort, SchemaDateTimeColumn originOrDestinationDate, bool isImport)
		{
			JobSailing sailing = isImport ? ImportSailing : ExportSailing;
			sailing.Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;

			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_DeclarationReference] = "B00005002";
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_DeclarationReference] = "B00005002";
			BusinessObject declaration = isImport ? JobsForTesting.ImportDeclaration : JobsForTesting.ExportDeclaration;
			declaration[JobDeclarationSchema.JE_DeclarationReference] = "B00005001";

			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_TransportMode] = Core.Constants.TransportModes.Sea;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_TransportMode] = Core.Constants.TransportModes.Sea;

			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_VesselName] = Voyage.JV_RV_NKVessel;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_VesselName] = Voyage.JV_RV_NKVessel;
			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_VoyageFlightNo] = Voyage.JV_VoyageFlight;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_VoyageFlightNo] = Voyage.JV_VoyageFlight;
			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_OH_ShippingLine] = Voyage.JV_OH_Line;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_OH_ShippingLine] = Voyage.JV_OH_Line;

			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_RL_NKOrigin] = ZString.Empty;
			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = ZString.Empty;
			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = ZString.Empty;
			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = ZString.Empty;
			JobsForTesting.ImportDeclaration[JobDeclarationSchema.JE_RL_NKPortOfFirstArrival] = ZString.Empty;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_RL_NKOrigin] = ZString.Empty;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = ZString.Empty;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = ZString.Empty;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = ZString.Empty;
			JobsForTesting.ExportDeclaration[JobDeclarationSchema.JE_RL_NKPortOfFirstArrival] = ZString.Empty;

			JobsForTesting.ImportDeclaration[originPort] = sailing.Origin.JA_RL_NKPortOfLoading;
			JobsForTesting.ExportDeclaration[originPort] = sailing.Origin.JA_RL_NKPortOfLoading;
			JobsForTesting.ImportDeclaration[destinationPort] = sailing.Destination.JB_RL_NKPortOfDischarge;
			JobsForTesting.ExportDeclaration[destinationPort] = sailing.Destination.JB_RL_NKPortOfDischarge;

			ITransportParent importTransportParent = (ITransportParent)JobsForTesting.ImportDeclaration;
			importTransportParent.Transports[0].JW_IsLinked = true;
			ITransportParent exportTransportParent = (ITransportParent)JobsForTesting.ExportDeclaration;
			exportTransportParent.Transports[0].JW_IsLinked = true;

			Factory.Save();

			SetDateOnSailing(sailing, originOrDestinationDate);
			JobSailingRelatedJob[] jobs = RelatedSailingJobs.LoadRelatedDeclarationJobs(ChangeLogger.LogVoyageDateChanges(Voyage));
			string importOrExport = isImport ? "Import" : "Export";
			AssertRelatedJobsLoaded(importOrExport + " declarations should be loaded due to a change to " + originOrDestinationDate.Name, jobs, declaration);

			importTransportParent.Transports[0].JW_IsLinked = false;
			exportTransportParent.Transports[0].JW_IsLinked = false;

			sailing.Origin.Delete();
			sailing.Destination.Delete();
			sailing.Delete();
			Voyage.Delete();

			Factory.Save();
		}

		void SetDateOnSailing(JobSailing sailing, SchemaDateTimeColumn dateProperty)
		{
			if (dateProperty.TableName == JobVoyOriginSchema.Constants.TableName)
			{
				sailing.Origin[dateProperty] = new ZDateTime(2000, 1, 11);
			}
			else if (dateProperty.TableName == JobVoyDestinationSchema.Constants.TableName)
			{
				sailing.Destination[dateProperty] = new ZDateTime(2000, 1, 11);
			}
			else if (dateProperty.TableName == JobSailingSchema.Constants.TableName)
			{
				sailing[dateProperty] = new ZDateTime(2000, 1, 11);
			}
		}

		#endregion

		#region CFS Load List

		public void TestLoadRelatedImportLoadListJobs()
		{
			JobsForTesting.ImportLoadList.JK_UniqueConsignRef = "L00005001";
			JobsForTesting.ExportLoadList.JK_UniqueConsignRef = "L00005002";
			JobsForTesting.ImportLoadList.Transports[0].JW_JX = ImportSailing.PK;
			JobsForTesting.ExportLoadList.Transports[0].JW_JX = ImportSailing.PK;
			Factory.Save();

			ImportSailing.Destination.JB_E_ARV = new ZDateTime(2000, 1, 11);
			JobSailingRelatedJob[] jobs = RelatedSailingJobs.LoadRelatedLoadListJobs(ChangeLogger.LogVoyageDateChanges(Voyage));
			AssertRelatedJobsLoaded("Import load lists should be loaded from a change to ETA", jobs, JobsForTesting.ImportLoadList);
		}

		public void TestLoadRelatedExportLoadListJobs()
		{
			JobsForTesting.ImportLoadList.JK_UniqueConsignRef = "L00005001";
			JobsForTesting.ExportLoadList.JK_UniqueConsignRef = "L00005002";
			JobsForTesting.ImportLoadList.Transports[0].JW_JX = ExportSailing.PK;
			JobsForTesting.ExportLoadList.Transports[0].JW_JX = ExportSailing.PK;
			Factory.Save();

			ExportSailing.Origin.JA_E_DEP = new ZDateTime(2000, 1, 11);
			JobSailingRelatedJob[] jobs = RelatedSailingJobs.LoadRelatedLoadListJobs(ChangeLogger.LogVoyageDateChanges(Voyage));
			AssertRelatedJobsLoaded("Export load lists should be loaded from a change to ETD", jobs, JobsForTesting.ExportLoadList);
		}

		#endregion

		#region Export Booking

		public void TestLoadRelatedExportBookingJobs()
		{
			JobsForTesting.BookingShipment.SailingJX = ExportSailing.PK;
			JobsForTesting.BookingShipment.UniqueConsignRef = "S00005001";
			Factory.Save();

			ExportSailing.Destination.JB_E_ARV = new ZDateTime(2000, 1, 11);
			JobSailingRelatedJob[] jobs = RelatedSailingJobs.LoadRelatedExportBookingJobs(ChangeLogger.LogVoyageDateChanges(Voyage));
			AssertRelatedJobsLoaded("No export booking job should be affected by a change to ETA", jobs, Array.Empty<BusinessObject>());

			ExportSailing.Origin.JA_E_DEP = new ZDateTime(2000, 1, 11);
			jobs = RelatedSailingJobs.LoadRelatedExportBookingJobs(ChangeLogger.LogVoyageDateChanges(Voyage));
			AssertRelatedJobsLoaded("Export booking jobs should be affected by a change to ETD", jobs, new BusinessObject[] { (BusinessObject)((BusinessObject)JobsForTesting.BookingShipment)["Booking"] });
		}

		#endregion

		#region Agency Booking

		public void TestLoadRelatedAgencyBookingJobs()
		{
			JobsForTesting.ExportAgencyBooking.JS_JX = ExportSailing.PK;
			JobsForTesting.ExportAgencyBooking.JS_UniqueConsignRef = "S00005001";
			Factory.Save();

			ExportSailing.Destination.JB_E_ARV = new ZDateTime(2000, 1, 11);
			JobSailingRelatedJob[] jobs = RelatedSailingJobs.LoadRelatedAgencyBookingJobs(ChangeLogger.LogVoyageDateChanges(Voyage));
			AssertRelatedJobsLoaded("No agency booking job should be affected by a change to ETA", jobs, Array.Empty<BusinessObject>());

			ExportSailing.Origin.JA_E_DEP = new ZDateTime(2000, 1, 11);
			jobs = RelatedSailingJobs.LoadRelatedAgencyBookingJobs(ChangeLogger.LogVoyageDateChanges(Voyage));
			AssertRelatedJobsLoaded("Agency booking jobs should be affected by a change to ETD", jobs, JobsForTesting.ExportAgencyBooking);
		}

		#endregion

		#region Agency Documentation

		public void TestLoadRelatedImportAgencyDocumentationJobs()
		{
			JobsForTesting.ImportAgencyDocumentation.JS_UniqueConsignRef = "V00005001";
			JobsForTesting.ImportAgencyDocumentation.JS_JX = ImportSailing.PK;
			JobsForTesting.ImportAgencyDocumentation.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;

			JobsForTesting.ImportAgencyDocumentation2.JS_UniqueConsignRef = "V00005002";
			JobsForTesting.ImportAgencyDocumentation2.JS_JX = ImportSailing.PK;
			JobsForTesting.ImportAgencyDocumentation2.JS_ShipmentStatus = ShipmentStatusList.Codes.WebFwdInstruction;

			JobsForTesting.ExportAgencyDocumentation.JS_UniqueConsignRef = "V00005003";
			JobsForTesting.ExportAgencyDocumentation.JS_JX = ImportSailing.PK;
			JobsForTesting.ExportAgencyDocumentation.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;

			JobsForTesting.ExportAgencyDocumentation2.JS_UniqueConsignRef = "V00005004";
			JobsForTesting.ExportAgencyDocumentation2.JS_JX = ImportSailing.PK;
			JobsForTesting.ExportAgencyDocumentation2.JS_ShipmentStatus = ShipmentStatusList.Codes.WebFwdInstruction;

			Factory.Save();

			ImportSailing.Destination.JB_E_ARV = new ZDateTime(2000, 1, 11);
			JobSailingRelatedJob[] jobs = RelatedSailingJobs.LoadRelatedAgencyDocumentationJobs(ChangeLogger.LogVoyageDateChanges(Voyage));
			AssertRelatedJobsLoaded("Import agency documentation jobs should be affected by a change to ETA", jobs,
				JobsForTesting.ImportAgencyDocumentation,
				JobsForTesting.ImportAgencyDocumentation2);
		}

		public void TestLoadRelatedExportAgencyDocumentationJobs()
		{
			JobsForTesting.ImportAgencyDocumentation.JS_UniqueConsignRef = "V00005001";
			JobsForTesting.ImportAgencyDocumentation.JS_JX = ExportSailing.PK;
			JobsForTesting.ImportAgencyDocumentation.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;

			JobsForTesting.ImportAgencyDocumentation2.JS_UniqueConsignRef = "V00005002";
			JobsForTesting.ImportAgencyDocumentation2.JS_JX = ExportSailing.PK;
			JobsForTesting.ImportAgencyDocumentation2.JS_ShipmentStatus = ShipmentStatusList.Codes.WebFwdInstruction;

			JobsForTesting.ExportAgencyDocumentation.JS_UniqueConsignRef = "V00005003";
			JobsForTesting.ExportAgencyDocumentation.JS_JX = ExportSailing.PK;
			JobsForTesting.ExportAgencyDocumentation.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;

			JobsForTesting.ExportAgencyDocumentation2.JS_UniqueConsignRef = "V00005004";
			JobsForTesting.ExportAgencyDocumentation2.JS_JX = ExportSailing.PK;
			JobsForTesting.ExportAgencyDocumentation2.JS_ShipmentStatus = ShipmentStatusList.Codes.WebFwdInstruction;

			Factory.Save();

			ExportSailing.Origin.JA_E_DEP = new ZDateTime(2000, 1, 11);
			JobSailingRelatedJob[] jobs = RelatedSailingJobs.LoadRelatedAgencyDocumentationJobs(ChangeLogger.LogVoyageDateChanges(Voyage));
			AssertRelatedJobsLoaded("Export agency documentation jobs should be affected by a change to ETD", jobs,
				JobsForTesting.ExportAgencyDocumentation,
				JobsForTesting.ExportAgencyDocumentation2);
		}

		#endregion

		#region Orders

		public void TestLoadRelatedImportOrderJobs()
		{
			JobsForTesting.Order[JobOrderHeaderSchema.JD_OrderNumber] = "P00005001";
			JobsForTesting.OrderLineDeliverContainer[JobOrderLineDeliverContainerSchema.J5_RV_NKArrivalVessel] = Voyage.JV_RV_NKVessel;
			JobsForTesting.OrderLineDeliverContainer[JobOrderLineDeliverContainerSchema.J5_Voyage] = Voyage.JV_VoyageFlight;
			JobsForTesting.OrderLineDelivery[JobOrderLineDeliverySchema.J4_RL_NKDestinationPort] = ImportSailing.Destination.JB_RL_NKPortOfDischarge;
			Factory.Save();

			ImportSailing.Destination.JB_E_ARV = new ZDateTime(2000, 1, 11);
			JobSailingRelatedJob[] jobs = RelatedSailingJobs.LoadRelatedOrderJobs(ChangeLogger.LogVoyageDateChanges(Voyage));
			AssertRelatedJobsLoaded("Import order jobs should be affected by a change to ETA", jobs, JobsForTesting.Order);
		}

		public void TestLoadRelatedExportOrderJobs()
		{
			JobsForTesting.Order[JobOrderHeaderSchema.JD_OrderNumber] = "P00005002";
			JobsForTesting.OrderLineDeliverContainer[JobOrderLineDeliverContainerSchema.J5_RV_NKArrivalVessel] = Voyage.JV_RV_NKVessel;
			JobsForTesting.OrderLineDeliverContainer[JobOrderLineDeliverContainerSchema.J5_Voyage] = Voyage.JV_VoyageFlight;
			JobsForTesting.OrderLineDeliverContainer[JobOrderLineDeliverContainerSchema.J5_RL_NKLoadPort] = ExportSailing.Origin.JA_RL_NKPortOfLoading;
			Factory.Save();

			ExportSailing.Origin.JA_E_DEP = new ZDateTime(2000, 1, 11);
			JobSailingRelatedJob[] jobs = RelatedSailingJobs.LoadRelatedOrderJobs(ChangeLogger.LogVoyageDateChanges(Voyage));
			AssertRelatedJobsLoaded("Export order jobs should be affected by a change to ETD", jobs, JobsForTesting.Order);
		}

		#endregion

		#region Voyage / Sailing Objects

		OrgHeader ShippingLine
		{
			get { return shippingLine ?? (shippingLine = new VoyageTestHelper(Factory).CreateCarrier("MAERSK")); }
		}
		OrgHeader shippingLine;

		JobVoyage Voyage
		{
			get
			{
				if (voyage == null || voyage.IsDeleted)
				{
					voyage = NewJobVoyage(Core.Constants.TransportModes.Sea, "Vessel", "Voyage", ShippingLine.PK);
				}
				return voyage;
			}
		}
		JobVoyage voyage;

		JobSailing ImportSailing
		{
			get
			{
				if (importSailing == null || importSailing.IsDeleted)
				{
					importSailing = NewJobSailing(Voyage, "MYPKG", "AUSYD", new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 2), new ZDateTime(2000, 1, 3), new ZDateTime(2000, 1, 4));
				}
				return importSailing;
			}
		}
		JobSailing importSailing;

		JobSailing ExportSailing
		{
			get
			{
				if (exportSailing == null || exportSailing.IsDeleted)
				{
					exportSailing = NewJobSailing(Voyage, "AUSYD", "USLAX", new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 2), new ZDateTime(2000, 1, 3), new ZDateTime(2000, 1, 4));
				}
				return exportSailing;
			}
		}
		JobSailing exportSailing;

		JobSailing DomesticSailing
		{
			get
			{
				if (domesticSailing == null || domesticSailing.IsDeleted)
				{
					domesticSailing = NewJobSailing(Voyage, "AUMEL", "AUSYD", new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 2), new ZDateTime(2000, 1, 3), new ZDateTime(2000, 1, 4));
				}
				return domesticSailing;
			}
		}
		JobSailing domesticSailing;

		#endregion

		#region Implementation

		void AssertRelatedJobsLoaded(string message, JobSailingRelatedJob[] actualRelatedJobs, params BusinessObject[] expectedRelatedJobs)
		{
			AssertEquals(message, expectedRelatedJobs.Length, actualRelatedJobs.Length);
			foreach (JobSailingRelatedJob actualRelatedJob in actualRelatedJobs)
			{
				AssertCollectionContains(message, actualRelatedJob.Job, expectedRelatedJobs);
			}
		}

		RelatedSailingJobs RelatedSailingJobs
		{
			get
			{
				if (relatedSailingJobs == null)
				{
					relatedSailingJobs = new RelatedSailingJobs();
				}
				return relatedSailingJobs;
			}
		}
		RelatedSailingJobs relatedSailingJobs;

		JobsForTesting JobsForTesting
		{
			get
			{
				if (jobsForTesting == null)
				{
					jobsForTesting = new JobsForTesting(Factory);
				}
				return jobsForTesting;
			}
		}
		JobsForTesting jobsForTesting;

		JobScheduleChangeLogger ChangeLogger
		{
			get { return new JobScheduleChangeLogger(); }
		}

		JobVoyage NewJobVoyage(ZString transportMode, ZString vesselCode, ZString voyage, ZGuid carrierPK)
		{
			RefVessel vessel = RefVessel.LookupVesselByCode(vesselCode, Factory);
			if (vessel == null)
			{
				vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = vesselCode;
			}

			JobVoyage result = Factory.New<JobVoyage>();
			result.JV_AirSeaRoad = transportMode;
			result.JV_RV_NKVessel = vessel.RV_FK;
			result.JV_VoyageFlight = voyage;
			result.JV_OH_Line = carrierPK;

			return result;
		}

		JobSailing NewJobSailing(JobVoyage voyage, ZString loadPort, ZString dischargePort, ZDateTime eTD, ZDateTime aTD, ZDateTime eTA, ZDateTime aTA)
		{
			VoyageOrigin origin = NewVoyageOrigin(voyage, loadPort, eTD, aTD);
			VoyageDestination destination = NewVoyageDestination(voyage, dischargePort, eTA, aTA);
			return voyage.Sailings[0];
		}

		VoyageOrigin NewVoyageOrigin(JobVoyage voyage, ZString loadPort, ZDateTime eTD, ZDateTime aTD)
		{
			VoyageOrigin result;
			if ((result = voyage.Origins.GetOriginFromLoading(loadPort)) == null)
			{
				result = voyage.Origins.AddNew();
				result.JA_RL_NKPortOfLoading = loadPort;
				result.JA_E_DEP = eTD;
				result.JA_A_DEP = aTD;
			}
			return result;
		}

		VoyageDestination NewVoyageDestination(JobVoyage voyage, ZString dischargePort, ZDateTime eTA, ZDateTime aTA)
		{
			VoyageDestination result;
			if ((result = voyage.Destinations.GetDestinationFromDischarge(dischargePort)) == null)
			{
				result = voyage.Destinations.AddNew();
				result.JB_RL_NKPortOfDischarge = dischargePort;
				result.JB_E_ARV = eTA;
				result.JB_A_ARV = aTA;
			}
			return result;
		}
		#endregion
	}
}
