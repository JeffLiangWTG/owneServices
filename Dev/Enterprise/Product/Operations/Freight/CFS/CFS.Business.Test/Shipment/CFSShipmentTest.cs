using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSShipmentTest : BaseShipmentTest
	{
		public void TestDefaultJS_ShipmentStatusAsCNF_SetIsForwardRegistered()
		{
			var cFSClient1 = Factory.NewWithValidTestData<OrgHeader>();
			cFSClient1.OH_Code = "test1";
			cFSClient1.OH_FullName = "aaa";

			var cFSClient2 = Factory.NewWithValidTestData<OrgHeader>();
			cFSClient2.OH_Code = "test2";
			cFSClient2.OH_FullName = "bbb";

			var activeCompany = Factory.NewWithValidTestData<GlbCompany>();
			activeCompany.GC_OH_OrgProxy = cFSClient2.PK;
			activeCompany.GC_IsActive = true;
			Factory.Save();

			var shipment = Factory.New<CFSShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_OH_HandledOnBehalfOfForwarder = cFSClient1.PK;
			Assert("Expecting IsForwardRegistered is false", !shipment.JS_IsForwardRegistered);
			AssertEquals("Not Default for shipments", "", shipment.JS_ShipmentStatus);

			var shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USCHI";
			shipment2.JS_OH_HandledOnBehalfOfForwarder = cFSClient2.PK;
			Assert("Expecting IsForwardRegistered is true", shipment2.JS_IsForwardRegistered);
			AssertEquals("Default for shipments", ShipmentStatusList.Codes.Confirmed, shipment2.JS_ShipmentStatus);
		}

		public void TestSetDomainContext()
		{
			var factory = new BusinessObjectFactory();

			AssertEquals("domain context has not been set", FreightDomainContext.Unspecified, factory.GetFreightDomainContext());

			_ = factory.NewWithValidTestData<CFSShipment>();

			AssertEquals("domain context has been set", FreightDomainContext.CFS, factory.GetFreightDomainContext());
		}

		#region IOverrideStorageMainDocManagerCode

		public void TestGetOverridenCodeIfNecessary_CFS()
		{
			var cfsShipment = Factory.New<CFSShipment>();
			var codeManager = cfsShipment as IOverrideStorageMainDocManagerCode;
			AssertNotNull("cfs shipment is expected to implement IOverrideStorageMainDocManagerCode", codeManager);

			var result = codeManager.GetOverridenCodeIfNecessary(Core.Constants.DocManagerCodes.Shipment);
			AssertEquals("Result should be CSR when the doc manager code is SHP and is in the CFS context", Core.Constants.DocManagerCodes.CFSShipmentReceival, result);

			result = codeManager.GetOverridenCodeIfNecessary(Core.Constants.DocManagerCodes.Consol);
			AssertEquals("Result should be orginal value when the doc manager code is not SHP", Core.Constants.DocManagerCodes.Consol, result);

			result = codeManager.GetOverridenCodeIfNecessary(null);
			AssertEquals("Result should be orginal value when the doc manager code is not SHP", ZString.Empty, result);
		}

		#endregion

		#region GetNewValidation

		public virtual void TestGetNewValidation()
		{
			CFSShipment shipment = Factory.NewWithValidTestData<CFSShipment>();
			AssertEquals("Type of Validation", typeof(CFSShipmentValidation), shipment.Validation.GetType());
		}

		#endregion

		#region Sailing

		public void TestSailing()
		{
			ZString storedCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;
			try
			{
				CFSShipment newShipment = Factory.New<CFSShipment>();
				newShipment.JS_RL_NKDestination = "AUSYD";
				newShipment.JS_RL_NKOrigin = "USLAX";
				JobSailing originalSailing = CreateSailing("USLAX", "AUSYD");
				JobSailing arrivalSailing = CreateSailing("USLAX", "AUBNE");
				JobSailing departureSailing = CreateSailing("AUPER", "SGSIN");

				newShipment.JS_JX = originalSailing.PK;
				AssertEquals("CommonShipment with no Consols Sailing", originalSailing.PK, newShipment.Sailing.PK);

				CommonConsol departureConsol = newShipment.Consols.AddNew();

				departureConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				Transport departureTransport = departureConsol.Transports[0];
				departureTransport.JW_JX = departureSailing.PK;
				AssertEquals("CommonShipment should calculate sailing from departure sailing when no arrival consol exists", departureSailing.PK, newShipment.Sailing.PK);

				CommonConsol arrivalConsol = newShipment.Consols.AddNew();
				arrivalConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;

				Transport arrivalTransport = arrivalConsol.Transports[0];
				arrivalTransport.JW_JX = arrivalSailing.PK;
				AssertEquals("CommonShipment should calculate sailing from arrival sailing if both arrival and departure sailings exist", arrivalSailing.PK, newShipment.Sailing.PK);

				JobSailing foreignSailing = CreateSailing("SGSIN", "USLAX");

				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
				newShipment.Consols.RemoveAll();
				CommonConsol foreignConsol = newShipment.Consols.AddNew();
				foreignConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				Transport foreignTransport = foreignConsol.Transports[0];
				foreignTransport.JW_JX = foreignSailing.PK;
				AssertEquals("CommonShipment should calculate sailing from available consol if neither arrival or departure sailings exist", foreignSailing.PK, newShipment.Sailing.PK);

				newShipment.Consols.Add(departureConsol);
				newShipment.Consols.Add(arrivalConsol);
				AssertEquals("CommonShipment should calculate sailing from arrival sailing if arrival, departure and foreign sailings exist", arrivalSailing.PK, newShipment.Sailing.PK);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = storedCountryCode;
			}
		}

		JobSailing CreateSailing(ZString loadingPort, ZString dischargePort)
		{
			JobSailing result = Factory.New<JobSailing>();
			JobVoyage voyage = Factory.New<JobVoyage>();
			RefVessel vessel = Factory.New<RefVessel>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = loadingPort;
			origin.JA_JV = voyage.PK;
			VoyageDestination destination = Factory.New<VoyageDestination>();
			destination.JB_JV = voyage.PK;
			destination.JB_RL_NKPortOfDischarge = dischargePort;
			result.JX_JA = origin.PK;
			result.JX_JB = destination.PK;
			return result;
		}

		#endregion

		public override void TestDoNotLoadJobDocsAndCartage()
		{
			Assert("Job Docs And Cartage referenced when JS_RL_NKOrigin is set in SetDefaultValues when creating the CFS Shipment", true);
		}

		public void TestOrgProxyAddShipmentViaGridIsDualCFSForwarding()
		{
			var cFSClient = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var consol = Factory.NewWithValidTestData<CFSLoadListConsol>();

			consol.JK_OH_Forwarder = cFSClient;
			consol.JK_RL_NKLoadPort = "AUSYD";

			Factory.Save();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			Factory.Save();

			AssertEquals(true, shipment.JS_UniqueConsignRef.StartsWith(NumberFountains.JobShipmentFountainPrefix));
		}

		public void TestOrgProxyAddShipmentViaNewIsDualCFSForwarding()
		{
			var cFSClient = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var consol = Factory.NewWithValidTestData<CFSLoadListConsol>();

			consol.JK_OH_Forwarder = cFSClient;
			consol.JK_RL_NKLoadPort = "AUSYD";

			Factory.Save();

			var shipment = Factory.New<CFSShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.Consols.Add(consol);

			Factory.Save();

			AssertEquals(true, shipment.JS_UniqueConsignRef.StartsWith(NumberFountains.JobShipmentFountainPrefix));
		}

		public void TestNoBranchOrgProxyDoesNotThrowException()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			var shipment = Factory.New<CFSShipment>();

			ZGuid x;
			AssertNoExceptionThrown(() => x = ((IHaveInternalCartage)shipment).CartagePickupDepotAddress);
		}

		public void TestCFSShipmentShouldNotLoadDeclarations()
		{
			var shipment = Factory.New<CFSShipment>();

			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

			AssertEquals("Should not load declarations for CFS Shipments", 0, shipment.Declarations.Length);
		}

		public void TestCFSShipmentDoesNotDefaultDocTracking()
		{
			RefCountry australia = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);

			RefCountryRequiredDocument requiredDoc1 = australia.RequiredDocuments.AddNew();
			requiredDoc1.RD_DocType = Constants.RefDocTypes.BookingConfirmation;
			requiredDoc1.RD_RN_NKOrigin = Constants.CountryCodes.Australia;
			requiredDoc1.RD_RN_NKDestination = Constants.CountryCodes.NewZealand;
			requiredDoc1.RD_DocUsage = JobRequiredDocument.DocUsage.Both;
			requiredDoc1.RD_TransportMode = Constants.TransportModes.Sea;
			requiredDoc1.RD_OnShipment = true;

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignee.OH_RL_NKClosestPort = "NZAKL";
			OrgSupplierBuyerLink buyerSupplierLink = consignor.BuyerLinks.AddNew(consignee);
			buyerSupplierLink.OL_RN_NKImporterCountry = Constants.CountryCodes.NewZealand;

			JobRequiredDocument requiredDoc2 = buyerSupplierLink.RequiredDocuments.AddNew();
			requiredDoc2.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDoc2.EQ_DocType = Constants.RefDocTypes.DeliveryOrder;
			requiredDoc2.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			requiredDoc2.EQ_DocPeriod = Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			requiredDoc2.EQ_DateReceived = ZDateTimeOffset.Today.AddDays(-2);
			requiredDoc2.EQ_ValidToDate = ZDateTime.Today.AddDays(2);
			requiredDoc2.EQ_DocNumber = "0001";

			Factory.Save();

			Func<bool, CommonShipment> createShipment = (isCFS) =>
			{
				CommonShipment shipment = isCFS ? Factory.New<CFSShipment>() : Factory.New<CommonShipment>();
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "NZAKL";
				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				return shipment;
			};

			CommonShipment commonShipment = createShipment(false);
			CFSShipment cfsShipment = (CFSShipment)createShipment(true);

			Factory.Save();

			AssertEquals("CommonShipment document tracking should be defaulted from Country and Org", 2, commonShipment.DocsAndCartage.RequiredDocuments.Count);
			AssertEquals("CFSShipment currently does not support document tracking to be defaulted from Country or Org (who knows, we might decide to implement it later)", 0, cfsShipment.DocsAndCartage.RequiredDocuments.Count);
		}

		public void TestGetChildCollection()
		{
			CommonShipment masterShipment = GetShipment();
			CommonShipment coLoad1 = GetShipment();
			CommonShipment coLoad2 = GetShipment();

			coLoad1.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;
			coLoad2.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;
			masterShipment.CoLoadShipments.Add(coLoad1);
			masterShipment.CoLoadShipments.Add(coLoad2);

			var shipmentMenu = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Arrival Notice by Coload House Bills"));
			IDocumentSupportable[] docs = masterShipment.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 2, docs.Length);
		}

		public void TestGetCanOverrideCheckpoint()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			AssertEquals("Prequisite", true, shipment.IsImport());

			SecurityCheckpoint checkpoint = ((IDocAddresses)shipment).GetCanOverrideCheckpoint(shipment.ConsigneeDocumentaryAddress);
			AssertEquals(Env.Security.CFSShipmentImportCFSImpAirConsigneeD, checkpoint);

			checkpoint = ((IDocAddresses)shipment).GetCanOverrideCheckpoint(shipment.ConsignorDocumentaryAddress);
			AssertEquals(Env.Security.CFSShipmentImportCFSImpAirConsignorD, checkpoint);

			checkpoint = ((IDocAddresses)shipment).GetCanOverrideCheckpoint(shipment.ConsignorPickupAddress);
			AssertEquals(Env.Security.None, checkpoint);
		}

		public void TestGetCanOverrideAddressCheckpointWhenImport()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			IDocAddresses iShipment = shipment;

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals(Env.Security.CFSShipmentImportCFSImpAirConsigneeD, iShipment.GetCanOverrideCheckpoint(shipment.ConsigneeDocumentaryAddress));
			AssertEquals(Env.Security.CFSShipmentImportCFSImpAirConsignorD, iShipment.GetCanOverrideCheckpoint(shipment.ConsignorDocumentaryAddress));

			shipment.JS_TransportMode = Constants.TransportModes.AirSea;
			AssertEquals(Env.Security.CFSShipmentImportCFSImpAirConsigneeD, iShipment.GetCanOverrideCheckpoint(shipment.ConsigneeDocumentaryAddress));
			AssertEquals(Env.Security.CFSShipmentImportCFSImpAirConsignorD, iShipment.GetCanOverrideCheckpoint(shipment.ConsignorDocumentaryAddress));

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(Env.Security.CFSShipmentImportCFSImpSeaConsigneeD, iShipment.GetCanOverrideCheckpoint(shipment.ConsigneeDocumentaryAddress));
			AssertEquals(Env.Security.CFSShipmentImportCFSImpSeaConsignorD, iShipment.GetCanOverrideCheckpoint(shipment.ConsignorDocumentaryAddress));

			shipment.JS_TransportMode = Constants.TransportModes.SeaAir;
			AssertEquals(Env.Security.CFSShipmentImportCFSImpSeaConsigneeD, iShipment.GetCanOverrideCheckpoint(shipment.ConsigneeDocumentaryAddress));
			AssertEquals(Env.Security.CFSShipmentImportCFSImpSeaConsignorD, iShipment.GetCanOverrideCheckpoint(shipment.ConsignorDocumentaryAddress));

			shipment.JS_TransportMode = Constants.TransportModes.Rail;
			AssertEquals(Env.Security.CFSShipmentImportCFSImpRailConsigneeD, iShipment.GetCanOverrideCheckpoint(shipment.ConsigneeDocumentaryAddress));
			AssertEquals(Env.Security.CFSShipmentImportCFSImpRailConsignorD, iShipment.GetCanOverrideCheckpoint(shipment.ConsignorDocumentaryAddress));

			shipment.JS_TransportMode = Constants.TransportModes.Road;
			AssertEquals(Env.Security.CFSShipmentImportCFSImpRoadConsigneeD, iShipment.GetCanOverrideCheckpoint(shipment.ConsigneeDocumentaryAddress));
			AssertEquals(Env.Security.CFSShipmentImportCFSImpRoadConsignorD, iShipment.GetCanOverrideCheckpoint(shipment.ConsignorDocumentaryAddress));

			shipment.JS_TransportMode = Constants.TransportModes.Other;
			AssertEquals(Env.Security.CFSShipmentImportCFSImpOtConsignee, iShipment.GetCanOverrideCheckpoint(shipment.ConsigneeDocumentaryAddress));
			AssertEquals(Env.Security.CFSShipmentImportCFSImpOtConsignor, iShipment.GetCanOverrideCheckpoint(shipment.ConsignorDocumentaryAddress));

			shipment.JS_TransportMode = Constants.TransportModes.Unknown;
			AssertEquals(Env.Security.CFSShipmentImportCFSImpOtConsignee, iShipment.GetCanOverrideCheckpoint(shipment.ConsigneeDocumentaryAddress));
			AssertEquals(Env.Security.CFSShipmentImportCFSImpOtConsignor, iShipment.GetCanOverrideCheckpoint(shipment.ConsignorDocumentaryAddress));
		}

		public void TestJS_ActualChargeable()
		{
			ChargeableWeightRoundingCollection collection = FreightDataRegistry.Instance.FreightChargeableWeightRoundings.Value;

			CFSShipment airShipment = Factory.New<CFSShipment>();
			airShipment.JS_TransportMode = Constants.TransportModes.Air;
			airShipment.JS_ActualChargeable = 12.43M;
			AssertEquals("Default beahviour", 12.43M, airShipment.JS_ActualChargeable);

			collection[0].RoundingMode = nameof(ChargeableWeightRoundingType.Down);
			collection[0].RoundingScale = ChargeableWeightRoundingScales.Scale10;
			FreightDataRegistry.Instance.FreightChargeableWeightRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			airShipment.JS_ActualChargeable = 12.42M;
			AssertEquals(12M, airShipment.JS_ActualChargeable);

			CFSShipment seaShipment = Factory.New<CFSShipment>();
			seaShipment.JS_TransportMode = Constants.TransportModes.Sea;
			seaShipment.JS_ActualChargeable = 12.43M;
			AssertEquals(12.43M, seaShipment.JS_ActualChargeable);
		}

		public void TestActualUpdatesChargeable()
		{
			ChargeableWeightRoundingCollection collection = FreightDataRegistry.Instance.FreightChargeableWeightRoundings.Value;

			CFSShipment airShipment = Factory.New<CFSShipment>();
			airShipment.JS_TransportMode = Constants.TransportModes.Air;
			airShipment.JS_ActualWeight = 12.43M;

			AssertEquals("Default beahviour", 12.43M, airShipment.JS_ActualChargeable);
			AssertEquals(12.43M, airShipment.JS_DocumentedChargeable);
			AssertEquals(12.43M, airShipment.JS_ManifestedChargeable);

			collection[0].RoundingMode = nameof(ChargeableWeightRoundingType.Down);
			collection[0].RoundingScale = ChargeableWeightRoundingScales.Scale10;
			FreightDataRegistry.Instance.FreightChargeableWeightRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			airShipment.JS_ActualWeight = 12.42M;

			AssertEquals(12M, airShipment.JS_ActualChargeable);
			AssertEquals(12M, airShipment.JS_DocumentedChargeable);
			AssertEquals(12M, airShipment.JS_ManifestedChargeable);

			CFSShipment seaShipment = Factory.New<CFSShipment>();
			seaShipment.JS_TransportMode = Constants.TransportModes.Sea;
			seaShipment.JS_ActualWeight = 1M;
			seaShipment.JS_ActualVolume = 13.567;

			AssertEquals(13.567M, seaShipment.JS_ActualChargeable);
			AssertEquals(13.567M, seaShipment.JS_DocumentedChargeable);
			AssertEquals(13.567M, seaShipment.JS_ManifestedChargeable);
		}

		public void TestPackingMode()
		{
			GlbDepartment.CurrentDepartment.GE_Air = true;

			CFSShipment shipment = Factory.New<CFSShipment>();
			AssertEquals("LSE", shipment.JS_PackingMode);

			GlbDepartment.CurrentDepartment.GE_Air = false;

			CFSShipment shipment1 = Factory.New<CFSShipment>();
			AssertEquals("LCL", shipment1.JS_PackingMode);
		}

		public void TestAutoUpdatePackingModWhenShipmentTypeIsChanged()
		{
			var shipment = Factory.New<CFSShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals("Should auto change to BCN when the shipment type is change to BCN", "BCN", shipment.JS_PackingMode);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals("Should auto change to LCL when the shipment type is not change to BCN", "LCL", shipment.JS_PackingMode);
		}

		public void TestGetCanOverrideAddressCheckpointWhenExport()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			IDocAddresses iShipment = shipment;

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals(Env.Security.CFSShipmentExportCFSExpAirConsigneeD, iShipment.GetCanOverrideCheckpoint(shipment.ConsigneeDocumentaryAddress));
			AssertEquals(Env.Security.CFSShipmentExportCFSExpAirConsignorD, iShipment.GetCanOverrideCheckpoint(shipment.ConsignorDocumentaryAddress));

			shipment.JS_TransportMode = Constants.TransportModes.AirSea;
			AssertEquals(Env.Security.CFSShipmentExportCFSExpAirConsigneeD, iShipment.GetCanOverrideCheckpoint(shipment.ConsigneeDocumentaryAddress));
			AssertEquals(Env.Security.CFSShipmentExportCFSExpAirConsignorD, iShipment.GetCanOverrideCheckpoint(shipment.ConsignorDocumentaryAddress));

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(Env.Security.CFSShipmentExportCFSExpSeaConsigneeD, iShipment.GetCanOverrideCheckpoint(shipment.ConsigneeDocumentaryAddress));
			AssertEquals(Env.Security.CFSShipmentExportCFSExpSeaConsignorD, iShipment.GetCanOverrideCheckpoint(shipment.ConsignorDocumentaryAddress));

			shipment.JS_TransportMode = Constants.TransportModes.SeaAir;
			AssertEquals(Env.Security.CFSShipmentExportCFSExpSeaConsigneeD, iShipment.GetCanOverrideCheckpoint(shipment.ConsigneeDocumentaryAddress));
			AssertEquals(Env.Security.CFSShipmentExportCFSExpSeaConsignorD, iShipment.GetCanOverrideCheckpoint(shipment.ConsignorDocumentaryAddress));

			shipment.JS_TransportMode = Constants.TransportModes.Rail;
			AssertEquals(Env.Security.CFSShipmentExportCFSExpRailConsigneeD, iShipment.GetCanOverrideCheckpoint(shipment.ConsigneeDocumentaryAddress));
			AssertEquals(Env.Security.CFSShipmentExportCFSExpRailConsignorD, iShipment.GetCanOverrideCheckpoint(shipment.ConsignorDocumentaryAddress));

			shipment.JS_TransportMode = Constants.TransportModes.Road;
			AssertEquals(Env.Security.CFSShipmentExportCFSExpRoadConsigneeD, iShipment.GetCanOverrideCheckpoint(shipment.ConsigneeDocumentaryAddress));
			AssertEquals(Env.Security.CFSShipmentExportCFSExpRoadConsignorD, iShipment.GetCanOverrideCheckpoint(shipment.ConsignorDocumentaryAddress));

			shipment.JS_TransportMode = Constants.TransportModes.Other;
			AssertEquals(Env.Security.CFSShipmentExportCFSExpOtherConsigneeD, iShipment.GetCanOverrideCheckpoint(shipment.ConsigneeDocumentaryAddress));
			AssertEquals(Env.Security.CFSShipmentExportCFSExpOtherConsignorD, iShipment.GetCanOverrideCheckpoint(shipment.ConsignorDocumentaryAddress));

			shipment.JS_TransportMode = Constants.TransportModes.Unknown;
			AssertEquals(Env.Security.CFSShipmentExportCFSExpOtherConsigneeD, iShipment.GetCanOverrideCheckpoint(shipment.ConsigneeDocumentaryAddress));
			AssertEquals(Env.Security.CFSShipmentExportCFSExpOtherConsignorD, iShipment.GetCanOverrideCheckpoint(shipment.ConsignorDocumentaryAddress));
		}

		public void TestGetChildCollectionForImportCartageAdvice()
		{
			DocumentZQuery menuFilter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice");
			menuFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_DocumentDirection, SQLComparisonOperator.Equal, "ARV");
			var shipmentMenu = Factory.LoadTop1<StmMenuItem>(menuFilter);

			var shipment = GetShipment();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			IDocumentSupportable[] docs = shipment.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 0, docs.Length);

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_RL_NKDischargePort = OverseasPort;
			consol1.JK_RL_NKLoadPort = HomePort;
			docs = shipment.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 0, docs.Length);

			var pack1 = shipment.OuterPackLines.AddNew();
			var cont1 = consol1.Containers.AddNew();
			cont1.JC_ContainerMode = Constants.ContainerModes.FCL;
			cont1.PackLines.Add(pack1);

			docs = shipment.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 1, docs.Length);
			Assert("Container 1 is Container supporter", docs[0] is CommonContainer);

			CommonConsol consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_RL_NKDischargePort = OverseasPort2;
			consol2.JK_RL_NKLoadPort = OverseasPort;
			docs = shipment.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 0, docs.Length);

			var pack2 = shipment.OuterPackLines.AddNew();
			var cont2 = consol2.Containers.AddNew();
			cont2.JC_ContainerMode = Constants.ContainerModes.FCL;
			cont2.PackLines.Add(pack2);
			var pack3 = shipment.OuterPackLines.AddNew();
			var cont3 = consol2.Containers.AddNew();
			cont3.JC_ContainerMode = Constants.ContainerModes.FCL;
			cont3.PackLines.Add(pack3);
			docs = shipment.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 2, docs.Length);
			Assert("Container 1 is Container supporter", docs[0] is CommonContainer);
			Assert("Container 2 is Container supporter", docs[1] is CommonContainer);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			docs = shipment.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 1, docs.Length);
			AssertSame("Is Shipment", shipment, docs[0]);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			consol1.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			docs = shipment.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 2, docs.Length);
			Assert("Container 1 is Container supporter", docs[0] is CommonContainer);
			Assert("Container 2 is Container supporter", docs[1] is CommonContainer);

			var queryProvider = new Mock<ICommonShipmentDocumentSupporterQueryProvider>();
			Factory.SetValue(() => queryProvider.Object);

			queryProvider.Setup(m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), It.IsAny<bool>())).Returns((ContainersToPrintOptions)null);
			docs = shipment.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Count 0, continue is false", 0, docs.Length);

			Factory.RemoveValue<ICommonShipmentDocumentSupporterQueryProvider>();

			docs = shipment.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Prints all containers if no event is listening", 2, docs.Length);

			queryProvider.Verify(m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), It.IsAny<bool>()), Times.Once);
		}

		public void TestGetChildCollectionForExportCartageAdvice()
		{
			DocumentZQuery menuFilter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice");
			menuFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_DocumentDirection, SQLComparisonOperator.Equal, "DEP");
			var shipmentMenu = Factory.LoadTop1<StmMenuItem>(menuFilter);

			var shipment = GetShipment();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			IDocumentSupportable[] docs = shipment.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 0, docs.Length);

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_RL_NKDischargePort = HomePort;
			consol1.JK_RL_NKLoadPort = OverseasPort2;
			docs = shipment.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 0, docs.Length);

			var pack1 = shipment.OuterPackLines.AddNew();
			var cont1 = consol1.Containers.AddNew();
			cont1.JC_ContainerMode = Constants.ContainerModes.FCL;
			cont1.PackLines.Add(pack1);

			docs = shipment.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 1, docs.Length);
			Assert("Container 1 is Container supporter", docs[0] is CommonContainer);

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_RL_NKDischargePort = OverseasPort2;
			consol2.JK_RL_NKLoadPort = OverseasPort;
			docs = shipment.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 0, docs.Length);

			var pack2 = shipment.OuterPackLines.AddNew();
			var cont2 = consol2.Containers.AddNew();
			cont2.JC_ContainerMode = Constants.ContainerModes.FCL;
			cont2.PackLines.Add(pack2);
			var pack3 = shipment.OuterPackLines.AddNew();
			var cont3 = consol2.Containers.AddNew();
			cont3.JC_ContainerMode = Constants.ContainerModes.FCL;
			cont3.PackLines.Add(pack3);
			docs = shipment.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 2, docs.Length);
			Assert("Container 1 is Container supporter", docs[0] is CommonContainer);
			Assert("Container 2 is Container supporter", docs[1] is CommonContainer);

			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			docs = shipment.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 1, docs.Length);
			AssertSame("is Shipment", shipment, docs[0]);

			shipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			consol1.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			docs = shipment.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 1, docs.Length);
			AssertSame("is Shipment", shipment, docs[0]);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;

			var queryProvider = new Mock<ICommonShipmentDocumentSupporterQueryProvider>();
			Factory.SetValue(() => queryProvider.Object);

			queryProvider
				.Setup(m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), It.IsAny<bool>()))
				.Returns((ContainersToPrintOptions)null);

			docs = shipment.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Count 0, continue is false", 0, docs.Length);

			Factory.RemoveValue<ICommonShipmentDocumentSupporterQueryProvider>();

			docs = shipment.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Prints all containers if no event is listening", 2, docs.Length);

			queryProvider.Verify(
				m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), It.IsAny<bool>()), Times.Once);
		}

		public void TestJS_OA_CartageCOAddr()
		{
			CFSShipment shipment = (CFSShipment)GetExportShipment();
			shipment.CartageCoPK = LocalLocalTransportCo.PK;

			AssertEquals("Expecting DocsAndCartgage PickupCartage to be set.", LocalLocalTransportCo.PK, shipment.DocsAndCartage.PickupCartageCoPK);
			AssertEquals("Expecting DocsAndCartgage DeliveryCartage to be empty.", ZGuid.Empty, shipment.DocsAndCartage.DeliveryCartageCoPK);

			shipment.DocsAndCartage.DeliveryCartageCoPK = ZGuid.Empty;
			shipment.DocsAndCartage.PickupCartageCoPK = ZGuid.Empty;

			shipment.JS_RL_NKOrigin = OverseasPort2;
			shipment.JS_RL_NKDestination = HomePort;
			shipment.CartageCoPK = LocalLocalTransportCo.PK;

			AssertEquals("Expecting DocsAndCartgage DeliveryCartage to be set.", LocalLocalTransportCo.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);
			AssertEquals("Expecting DocsAndCartgage PickupCartage to be empty.", ZGuid.Empty, shipment.DocsAndCartage.PickupCartageCoPK);

			shipment.DocsAndCartage.DeliveryCartageCoPK = ZGuid.Empty;
			shipment.DocsAndCartage.PickupCartageCoPK = ZGuid.Empty;

			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = AlternateHomePort;
			shipment.CartageCoPK = LocalLocalTransportCo.PK;

			AssertEquals("Expecting DocsAndCartgage PickupCartage to be set.", LocalLocalTransportCo.PK, shipment.DocsAndCartage.PickupCartageCoPK);
			AssertEquals("Expecting DocsAndCartgage DeliveryCartage to be empty.", ZGuid.Empty, shipment.DocsAndCartage.DeliveryCartageCoPK);

			shipment.DocsAndCartage.DeliveryCartageCoPK = ZGuid.Empty;
			shipment.DocsAndCartage.PickupCartageCoPK = ZGuid.Empty;

			shipment.JS_RL_NKOrigin = AlternateHomePort;
			shipment.JS_RL_NKDestination = HomePort;
			shipment.CartageCoPK = LocalLocalTransportCo.PK;

			AssertEquals("Expecting DocsAndCartgage DeliveryCartage to be set.", LocalLocalTransportCo.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);
			AssertEquals("Expecting DocsAndCartgage PickupCartage to be empty.", ZGuid.Empty, shipment.DocsAndCartage.PickupCartageCoPK);
		}

		public void TestJS_CartageTypeOverride()
		{
			CFSShipment shipment = (CFSShipment)GetExportShipment();
			AssertEquals("JS_CartageTypeOverride should be Loose-Export", Constants.CartageJobType.LCLExport, ((IHaveInternalCartage)shipment).CartageTypeOverride);

			shipment.JS_RL_NKOrigin = OverseasPort2;
			shipment.JS_RL_NKDestination = HomePort;
			AssertEquals("JS_CartageTypeOverride should be Loose-Import", Constants.CartageJobType.LCLImport, ((IHaveInternalCartage)shipment).CartageTypeOverride);

			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = AlternateHomePort;
			AssertEquals("JS_CartageTypeOverride should be Loose-Pickup", Constants.CartageJobType.LCLExport, ((IHaveInternalCartage)shipment).CartageTypeOverride);

			shipment.JS_RL_NKOrigin = AlternateHomePort;
			shipment.JS_RL_NKDestination = HomePort;
			AssertEquals("JS_CartageTypeOverride should be Loose-Delivery", Constants.CartageJobType.LCLImport, ((IHaveInternalCartage)shipment).CartageTypeOverride);
		}

		public void TestHandledOnBehalfOfForwarderIsInSyncWithSQLFunctionForImport()
		{
			HandledOnBehalfOfForwarderIsInSyncWithSQLFunctionForGivenCountrys("NZAKL", "SGSIN", "AUDRW");
		}

		public void TestHandledOnBehalfOfForwarderIsInSyncWithSQLFunctionForExport()
		{
			HandledOnBehalfOfForwarderIsInSyncWithSQLFunctionForGivenCountrys("AUDRW", "SGSIN", "NZAKL");
		}

		public void TestHandledOnBehalfOfForwarderIsInSyncWithSQLFunctionForTransShipment()
		{
			HandledOnBehalfOfForwarderIsInSyncWithSQLFunctionForGivenCountrys("NZAKL", "AUDRW", "SGSIN");
		}

		public void TestHandledOnBehalfOfForwarderIsInSyncWithSQLFunctionForDomestic()
		{
			HandledOnBehalfOfForwarderIsInSyncWithSQLFunctionForGivenCountrys("AUCNS", "AUGOV", "AUDRW");
		}

		public void TestRequireOrderNumbersOnDocs()
		{
			var shipment = (CFSShipment)GetShipment();
			var testConsignee = CreateTestLocalConsignee();
			testConsignee.MiscServ.OM_IMImporterRequiresOrderNumbersOnDocs = true;
			shipment.ConsigneePK = testConsignee.PK;
			AssertEquals("Depot shipments should not be stopped by forwarding restrictions", false, ((IShipmentWithDocsAndCartage)shipment).RequiresOrderNumbersOnDocs());
		}

		public void TestBlankJobDocsAndCartageIsCreatedForNewShipment()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			AssertNotNull("A new JobDocsAndCartage should have been created", shipment.DocsAndCartage);
		}

		public void TestJobDocsAndCartageIsReloaded()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			AssertNotNull("Touch JobDocsAndCartage so it's created", shipment.DocsAndCartage);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CFSShipment loadedShipment = factory2.Load<CFSShipment>(shipment.PK);
			AssertEquals("The correct JobDocsAndCartage should have been reloaded", shipment.DocsAndCartage.PK, loadedShipment.DocsAndCartage.PK);
		}

		public void TestHasImportConsol()
		{
			CFSShipment shipment = (CFSShipment)GetShipment();
			AssertEquals("Has Import Consol", false, shipment.HasImportConsol);

			CFSLoadListConsol consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_JX = ImportSailing1.PK;
			consol.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			consol.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;

			AssertEquals("Has Import Consol", true, shipment.HasImportConsol);

			CFSLoadListConsol consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;

			Transport transport2 = consol2.Transports[0];
			transport2.JW_JX = ExportSailing1.PK;
			consol.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			consol.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;

			AssertEquals("Has Import Consol", true, shipment.HasImportConsol);

			shipment.Consols.Remove(consol);
			AssertEquals("Has Import Consol", false, shipment.HasImportConsol);
		}

		#region TestTranshipment

		public void TestTranshipment()
		{
			var consol = Factory.NewWithValidTestData<CFSLoadListConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKDiscPort = "AUSYD";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = "NZAKL";

			Factory.Save();

			AssertEquals("It is a Transhipment", true, shipment.IsTranshipment());

			shipment.JS_RL_NKDestination = "AUMEL";
			Factory.Save();

			AssertEquals("It is not a Transhipment", false, shipment.IsTranshipment());
		}

		#endregion

		#region TestOnForwarding

		public void TestOnForwarding()
		{
			var consol = Factory.NewWithValidTestData<CFSLoadListConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKDiscPort = "AUSYD";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = "AUMEL";

			Factory.Save();

			AssertEquals("It is On Forwarding", true, shipment.IsOnForwarding());

			shipment.JS_RL_NKDestination = "NZAKL";
			Factory.Save();
			AssertEquals("It is not On Forwarding", false, shipment.IsOnForwarding());
		}

		#endregion

		#region DocumentEventSource_DocumentPrintRequested

		public void TestDocumentEventSourceDocumentPrintRequested()
		{
			CFSShipmentForTest shipment = Factory.New<CFSShipmentForTest>();

			DocumentZQuery filter = new DocumentZQuery(shipment.DocumentSupporter.BusinessContext, "Export Label");
			filter.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, ZBool.True);
			var cFSExportLabelMenu = Factory.LoadTop1<StmMenuItem>(filter);
			DocumentCancelEventArgs @event = new DocumentCancelEventArgs(cFSExportLabelMenu);
			shipment.SetDocumentPrintRequested(this, @event);

			AssertEquals("Default number of copies to print", ZShort.Parse("1"), @event.MenuItem.NumberOfCopies);

			shipment.JS_OuterPacks = 10;
			shipment.SetDocumentPrintRequested(this, @event);
			AssertEquals("Default number of copies to print", ZShort.Parse("10"), @event.MenuItem.NumberOfCopies);

			shipment.OuterPackLines.RemoveAndDeleteAll();
			CFSPackLine line1 = shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 2;
			CFSPackLine line2 = shipment.OuterPackLines.AddNew();
			line2.JL_PackageCount = 3;
			shipment.SetDocumentPrintRequested(this, @event);
			AssertEquals("Default number of copies to print", ZShort.Parse("5"), @event.MenuItem.NumberOfCopies);
		}

		#endregion

		#region TestAttachLoadList

		public void TestAttachLoadList()
		{
			CFSShipment shipment = (CFSShipment)GetShipment();
			shipment.JS_JX = new ConstantsAndReusables(Factory).CreateNewSailing().PK;
			shipment.Consols.RemoveAll();
			AssertEquals("Consols.Count", 0, shipment.Consols.Count);

			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_TransportMode = Constants.TransportModes.Sea;
			loadList.Transports[0].JW_JX = new ConstantsAndReusables(Factory).CreateNewSailing(true).PK;

			shipment.Consols.Add(loadList);
			AssertEquals("Consols.Count", 1, shipment.Consols.Count);
			AssertEquals("Shipment's sailing", loadList.Schedule.PK, shipment.Sailing.PK);

			CFSLoadListConsol loadList2 = Factory.New<CFSLoadListConsol>();
			loadList2.JK_TransportMode = Constants.TransportModes.Sea;
			loadList2.Transports[0].JW_JX = new ConstantsAndReusables(Factory).CreateNewSailing(true).PK;
			shipment.Consols.Add(loadList2);
			AssertEquals("Consols.Count", 1, shipment.Consols.Count);
		}

		#endregion

		#region TestDetachLoadList

		public void TestDetachLoadList()
		{
			var shipment = (CFSShipment)GetShipment();
			AssertEquals("Precondition: Not expecting consol on shipment", 0, shipment.Consols.Count);

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.AutomaticallyUpdatePackLineContainers = true;
			loadList.JK_TransportMode = Constants.TransportModes.Sea;
			loadList.Transports[0].JW_JX = new ConstantsAndReusables(Factory).CreateNewSailing(true).PK;
			shipment.Consols.Add(loadList);
			AssertEquals("Consols.Count", 1, shipment.Consols.Count);
			AssertEquals("Shipment's sailing", loadList.Schedule.PK, shipment.Sailing.PK);

			var line = shipment.OuterPackLines.AddNew();
			line.JL_PackageCount = 1;
			var container = loadList.Containers.AddNew();
			AssertEquals("OuterPackLines.Count", 1, shipment.OuterPackLines.Count);
			AssertEquals("PackLines.Count", 1, container.PackLines.Count);

			container.PackLines.Add(line);
			AssertEquals("OuterPackLines.Count", 1, shipment.OuterPackLines.Count);
			AssertEquals("PackLines.Count", 1, container.PackLines.Count);

			Factory.Save();

			shipment.Consols.Remove(loadList);
			AssertEquals("Consols.Count", 0, shipment.Consols.Count);
			AssertEquals("OuterPackLines.Count", 1, shipment.OuterPackLines.Count);
			AssertEquals("PackLines.Count", 0, container.PackLines.Count);
		}

		#endregion

		#region JobInvoicing Tests

		public void TestSendingAgentOverride()
		{
			CFSShipment shipment = (CFSShipment)GetShipment();
			shipment.JS_OH_HandledOnBehalfOfForwarder = LocalForwarder.PK;
			shipment.ConsigneePK = OverseasConsignor.PK;
			shipment.JS_RL_NKDestination = OverseasPort;
			shipment.ConsignorPK = LocalConsignor.PK;
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_OuterPacks = 10;
			AssertEquals("Export Sending Agent should be Forwarder", LocalForwarder.PK, ((IJobInvoicingPlugIn)shipment).InvoicingSupporter.SendingAgent.PK);
		}

		public void TestAuditSecurity()
		{
			AssertEquals("AuditSecurity", Env.Security.CFSShipmentAuditBilling, ((IJobInvoicingPlugIn)Factory.New<CFSShipment>()).InvoicingSupporter.AuditSecurity);
		}

		#endregion

		public void TestLogs_QuickBookingLogsDoNotGetLoaded()
		{
			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory);
			var logParent = (IStmALogParent)quotedBooking;
			logParent.Logs.AddNew(Events.Authorised);

			logParent = (IStmALogParent)quotedBooking.ForwardingShipment;
			logParent.Logs.AddNew(Events.MiscellaneousEvent);

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var quotedBookingInAnotherFactory = factory.Load<IQuotedBooking>(quotedBooking.ForwardingShipment.PK);
			var forwardingShipment = (CommonShipment)quotedBookingInAnotherFactory.ForwardingShipment;
			forwardingShipment.JS_IsForwardRegistered = true;

			factory.Save();

			factory = new BusinessObjectFactory();

			var cfsShipment = (IStmALogParent)factory.Load<CFSShipment>(quotedBooking.ForwardingShipment.PK);

			AssertContainsExactElementsInAnyOrder("booking logs should not be loaded",
				new[] { "MIS" },
				cfsShipment.Logs.GetAllLogs().Cast<StmALog>()
					.Where(log => log.SL_SE_NKEvent != Events.AddedARecordToTheSystemCode && log.SL_SE_NKEvent != Events.EditedARecordCode)
					.Select(log => log.SL_SE_NKEvent.ToString()));
		}

		public void TestColoadShipmentListIsFilteredOnIsCFSRegistered()
		{
			var cfsShipment1 = Factory.New<CFSShipment>();
			cfsShipment1.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var cfsShipment2 = Factory.New<CFSShipment>();
			cfsShipment2.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var nonCFSShipment = Factory.New<CFSShipment>();
			nonCFSShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			nonCFSShipment.JS_IsCFSRegistered = false;

			var shipment = Factory.New<CFSShipment>();
			AssertContainsExactElementsInAnyOrder(new[] { cfsShipment1, cfsShipment2 }, shipment.Lookups.CoLoadMaster_List);
		}

		#region Canada Specific Tests

		public void TestCanadaHouseCCN()
		{
			var shipment = Factory.New<CFSShipment>();

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);
			Factory.Save();

			AssertEquals(ZString.Empty, shipment.CanadaHouseCCN);

			CusEntryNumber num1 = shipment.Numbers.AddNew();
			num1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			num1.CE_EntryNum = "1";

			Factory.Save();

			AssertEquals("CanadaHouseCCN", "1", shipment.CanadaHouseCCN);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);

			AssertEquals("CanadaHouseCCN should be empty", ZString.Empty, shipment.CanadaHouseCCN);
		}

		public void TestCanadaLoadListCCNPCN()
		{
			var shipment = Factory.New<CFSShipment>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				AssertEquals("CanadaLoadListCCN should be empty", ZString.Empty, shipment.CanadaLoadListCCN);
				AssertEquals("CanadaLoadListPCN should be empty", ZString.Empty, shipment.CanadaLoadListPCN);

				var loadList1 = shipment.Consols.AddNew();
				var loadList2 = shipment.Consols.AddNew();

				AssertEquals("CanadaLoadListCCN should be empty", ZString.Empty, shipment.CanadaLoadListCCN);
				AssertEquals("CanadaLoadListPCN should be empty", ZString.Empty, shipment.CanadaLoadListPCN);

				var num1 = loadList1.Numbers.AddNew();
				num1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
				num1.CE_EntryNum = "111";

				var num2 = loadList2.Numbers.AddNew();
				num2.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
				num2.CE_EntryNum = "222";

				AssertEquals("CanadaLoadListCCN", "111", shipment.CanadaLoadListCCN);
				AssertEquals("CanadaLoadListCCN", "222", shipment.CanadaLoadListPCN);

				var num3 = loadList1.Numbers.AddNew();
				num3.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
				num3.CE_EntryNum = "333";

				var num4 = loadList2.Numbers.AddNew();
				num4.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
				num4.CE_EntryNum = "444";

				AssertEquals("CanadaLoadListCCN", "111, 444", shipment.CanadaLoadListCCN);
				AssertEquals("CanadaLoadListCCN", "222, 333", shipment.CanadaLoadListPCN);

				var loadList3 = shipment.Consols.AddNew();

				var num5 = loadList3.Numbers.AddNew();
				num5.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
				num5.CE_EntryNum = "11 1";

				var num6 = loadList3.Numbers.AddNew();
				num6.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
				num6.CE_EntryNum = "22 2";

				AssertEquals("CanadaLoadListCCN", "111, 444", shipment.CanadaLoadListCCN);
				AssertEquals("CanadaLoadListCCN", "222, 333", shipment.CanadaLoadListPCN);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				AssertEquals("CanadaLoadListCCN should be empty", ZString.Empty, shipment.CanadaLoadListCCN);
				AssertEquals("CanadaLoadListPCN should be empty", ZString.Empty, shipment.CanadaLoadListPCN);

				var loadList1 = shipment.Consols.AddNew();
				var loadList2 = shipment.Consols.AddNew();

				var num1 = loadList1.Numbers.AddNew();
				num1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
				num1.CE_EntryNum = "1";

				var num2 = loadList2.Numbers.AddNew();
				num2.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
				num2.CE_EntryNum = "2";

				AssertEquals("CanadaLoadListCCN should be empty", ZString.Empty, shipment.CanadaLoadListCCN);
				AssertEquals("CanadaLoadListPCN should be empty", ZString.Empty, shipment.CanadaLoadListPCN);
			}
		}

		#region CustomsEntryNumber
		public void TestCustomsEntryNumber()
		{
			var shipment = Factory.New<CFSShipment>();

			var statusProvider = new CFSShipmentRNSStatusProviderTest.CFSShipmentRNSStatusProviderDummyObject(shipment);
			CFSShipmentRNSStatusProvider.DummyForTest = statusProvider;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				shipment.CustomsEntryNumber = "";
				statusProvider.TransactionNumber_Exposed = "";
				AssertEquals("CustomsEntryNumber", "", shipment.CustomsEntryNumber);
				Assert(!shipment.CustomsEntryNumberInfo.ReadOnly);

				shipment.CustomsEntryNumber = "1111";
				AssertEquals("CustomsEntryNumber", "1111", shipment.CustomsEntryNumber);
				Assert(!shipment.CustomsEntryNumberInfo.ReadOnly);

				statusProvider.TransactionNumber_Exposed = "shipment";
				AssertEquals("CustomsEntryNumber", "1111", shipment.CustomsEntryNumber);
				Assert(!shipment.CustomsEntryNumberInfo.ReadOnly);

				shipment.CustomsEntryNumber = "";
				AssertEquals("CustomsEntryNumber", "", shipment.CustomsEntryNumber);
				Assert(!shipment.CustomsEntryNumberInfo.ReadOnly);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				shipment.CustomsEntryNumber = "";
				statusProvider.TransactionNumber_Exposed = "";
				AssertEquals("CustomsEntryNumber", "", shipment.CustomsEntryNumber);
				Assert(!shipment.CustomsEntryNumberInfo.ReadOnly);

				shipment.CustomsEntryNumber = "1111";
				AssertEquals("CustomsEntryNumber", "1111", shipment.CustomsEntryNumber);
				Assert(!shipment.CustomsEntryNumberInfo.ReadOnly);

				statusProvider.TransactionNumber_Exposed = "2222";
				AssertEquals("CustomsEntryNumber", "1111", shipment.CustomsEntryNumber);
				Assert(!shipment.CustomsEntryNumberInfo.ReadOnly);

				shipment.CustomsEntryNumber = "";
				AssertEquals("CustomsEntryNumber", "2222", shipment.CustomsEntryNumber);
				Assert(shipment.CustomsEntryNumberInfo.ReadOnly);

				shipment.CustomsEntryNumber = "3333";
				AssertEquals("CustomsEntryNumber", "2222", shipment.CustomsEntryNumber);
			}
		}
		#endregion

		#region RNS Status
		public void TestRNSStatus()
		{
			var shipment = Factory.New<CFSShipment>();

			var statusProvider = new CFSShipmentRNSStatusProviderTest.CFSShipmentRNSStatusProviderDummyObject(shipment);
			CFSShipmentRNSStatusProvider.DummyForTest = statusProvider;
			statusProvider.ReleaseStatus_Exposed = "Goods Released";
			statusProvider.ReleaseDate_Exposed = new DateTime(2014, 12, 4);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				AssertEquals("RNSReleaseStatus", ZString.Empty, shipment.RNSReleaseStatus);
				AssertEquals("RNSReleaseDate", ZDateTime.Empty, shipment.RNSReleaseDate);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				AssertEquals("RNSReleaseStatus", "Goods Released", shipment.RNSReleaseStatus);
				AssertEquals("RNSReleaseDate", new DateTime(2014, 12, 4), shipment.RNSReleaseDate);
			}
		}
		#endregion

		#endregion

		public void TestCFSShipmentTypeShouldIncludeAssemblyMaster()
		{
			var cfsShipment = Factory.New<CFSShipment>();
			Assert(cfsShipment.Lookups.JS_ShipmentType_List.ContainsCode(Constants.ShipmentTypes.AssemblyMaster));
		}

		#region Fetch Strategy

		public void TestFetchForView_JH_ProfitLossReasonCode()
			=> AssertFetchForView("Job+JH_ProfitLossReasonCode", new Dictionary<string, int> { { JobHeader.Schema.TableName, 1 } });

		public void TestFetchForView_JH_TotalProfitRevenueMargin()
			=> AssertFetchForView("Job+JH_TotalProfitRevenueMargin", new Dictionary<string, int> { { JobHeader.Schema.TableName, 1 } });

		void AssertFetchForView(string propertyName, Dictionary<string, int> expectedDbHits)
		{
			for (var i = 0; i <= 10; i++)
			{
				CreateCFSShipment();
			}
			Factory.Save();

			var newFactory = NewFactory();
			var shipments = newFactory.Load<CFSShipment>(new ZQuery());
			newFactory.ResetDatabaseLoadCount();

			foreach (var shipment in shipments)
			{
				shipment.FetchStrategy.FetchForView(new[]
				{
					new TableColumn(string.Empty, propertyName)
				});
			}

			foreach (var shipment in shipments)
			{
				_ = shipment[propertyName];
			}

			AssertDbHits(expectedDbHits, newFactory);
		}

		CFSShipment CreateCFSShipment() => Factory.NewWithValidTestData<CFSShipment>();

		#endregion

		#region Implementation

		protected override CommonShipment GetShipment()
		{
			return GetShipment(Factory);
		}

		CommonShipment GetShipment(BusinessObjectFactory factory)
		{
			return factory.New<CFSShipment>();
		}

		ZGuid GetFunctionResult(CommonShipment shipment)
		{
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);

			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@IsImport", shipment.IsImport() ? 'Y' : 'N', new SchemaBoolColumn(Schema.GenericTableSchema, "IsImport", 0, false, false, false)); // IsImport is not in the db!
			@params.Add("@IsExport", shipment.IsExport() ? 'Y' : 'N', new SchemaBoolColumn(Schema.GenericTableSchema, "IsExport", 0, false, false, false)); // IsExport is not in the db!
			@params.Add("@PK", shipment.PK, JobShipmentSchema.PK);

			collection.Load("SELECT Value As Result FROM dbo.Report_GetSecondChanceClientFromShipment(@IsImport, @IsExport, @PK) ", @params);

			AssertEquals(1, collection.Count);

			object result = collection[0]["Result"];
			ZGuid resultGuid = ZGuid.Empty;

			resultGuid = (ZGuid)result;

			return resultGuid;
		}

		void HandledOnBehalfOfForwarderIsInSyncWithSQLFunctionForGivenCountrys(string first, string mid, string last)
		{
			CFSShipment shipment = (CFSShipment)GetShipment();

			Factory.Save();
			AssertEquals("EmptyShipment", shipment.JS_OH_HandledOnBehalfOfForwarder, GetFunctionResult(shipment));

			var firstPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, first);
			var midPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, mid);
			var lastPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, last);

			shipment.JS_RL_NKOrigin = firstPort.RL_Code;
			shipment.JS_RL_NKDestination = lastPort.RL_Code;

			JobVoyage voyage = Factory.New<JobVoyage>();

			VoyageOrigin firstOrigin = Factory.New<VoyageOrigin>();
			firstOrigin.JA_RL_NKPortOfLoading = firstPort.RL_Code;
			firstOrigin.JA_JV = voyage.PK;

			VoyageDestination firstDestination = Factory.New<VoyageDestination>();
			firstDestination.JB_RL_NKPortOfDischarge = midPort.RL_Code;
			firstDestination.JB_JV = voyage.PK;

			VoyageOrigin secondOrigin = Factory.New<VoyageOrigin>();
			secondOrigin.JA_RL_NKPortOfLoading = midPort.RL_Code;
			secondOrigin.JA_JV = voyage.PK;

			VoyageDestination secondDestination = Factory.New<VoyageDestination>();
			secondDestination.JB_RL_NKPortOfDischarge = lastPort.RL_Code;
			secondDestination.JB_JV = voyage.PK;

			JobSailing firstSailing = voyage.Sailings.AddNew();
			firstSailing.JX_JA = firstOrigin.PK;
			firstSailing.JX_JB = firstDestination.PK;

			JobSailing secondSailing = voyage.Sailings.AddNew();
			secondSailing.JX_JA = secondOrigin.PK;
			secondSailing.JX_JB = secondDestination.PK;

			var firstReceivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var firstSendingForwarder = Factory.NewWithValidTestData<OrgHeader>();

			CFSLoadListConsol firstConsol = shipment.Consols.AddNew();
			firstConsol.JK_TransportMode = Constants.TransportModes.Sea;
			firstConsol.Transports[0].JW_JX = firstSailing.PK;
			firstConsol.JK_OA_ReceivingForwarderAddress = firstReceivingForwarder.MainAddress.PK;
			firstConsol.JK_OA_SendingForwarderAddress = firstSendingForwarder.MainAddress.PK;

			var secondReceivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var secondSendingForwarder = Factory.NewWithValidTestData<OrgHeader>();

			CFSLoadListConsol secondConsol = shipment.Consols.AddNew();
			secondConsol.JK_TransportMode = Constants.TransportModes.Sea;
			secondConsol.Transports[0].JW_JX = secondSailing.PK;
			secondConsol.JK_OA_ReceivingForwarderAddress = secondReceivingForwarder.MainAddress.PK;
			secondConsol.JK_OA_SendingForwarderAddress = secondSendingForwarder.MainAddress.PK;

			Factory.Save();
			AssertEquals("With Consols", shipment.JS_OH_HandledOnBehalfOfForwarder, GetFunctionResult(shipment));

			var masterConsignor = Factory.NewWithValidTestData<OrgHeader>();
			var masterConsignee = Factory.NewWithValidTestData<OrgHeader>();

			CFSShipment masterShipment = (CFSShipment)GetShipment();

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = masterConsignee.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = masterConsignor.MainAddress.PK;

			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			Factory.Save();
			AssertEquals("With CoLoad Master", shipment.JS_OH_HandledOnBehalfOfForwarder, GetFunctionResult(shipment));
		}

		#endregion
	}
}
