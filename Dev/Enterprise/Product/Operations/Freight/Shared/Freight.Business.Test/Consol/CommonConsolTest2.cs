using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Module = Enterprise.Registry.Business.Module;

namespace Enterprise.Freight.Business.Testing
{
	public class CommonConsolTest2 : BaseFreightTest
	{
		#region GetNewValidation

		public virtual void TestGetNewValidation()
		{
			AssertEquals("Type of Validation", typeof(CommonConsolValidation), Consol.Validation.GetType());
		}

		#endregion

		#region Related Business Objects

		public void TestShipmentContainerPackLines()
		{
			CommonConsol consol = GetNewConsol();
			consol.Containers.AddNew();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines[0].SetContainer(consol.Containers[0].PK);
			AssertEquals("Consol Containers Pack Lines and Consols Shipments Pack Lines", consol.Containers[0].PackLines[0], shipment.OuterPackLines[0]);
		}

		public void TestTransportMatchesConsol()
		{
			CommonConsol consol = GetNewConsol();
			Transport transport = consol.Transports.AddNew();
			AssertEquals("Consol does not match Consol's Transport Consol", consol, transport.Parent);
		}

		public void TestShipmentsChildEditableRegistration()
		{
			AssertEquals(ChildEditableServiceStates.Consol, ChildEditableService.GetState(Factory));
			var consol = FreightTestHelper.GetConsol<CommonConsol>("CON", Factory);
			var shipments = consol.Shipments;
			AssertEquals(true, consol.IsRegisteredEditableChildObject(shipments));

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			consol = FreightTestHelper.GetConsol<CommonConsol>("CON", Factory);
			shipments = consol.Shipments;
			AssertEquals(false, consol.IsRegisteredEditableChildObject(shipments));
		}

		#endregion

		#region TestSeaRailAdjustShipmentDatesByWholeDays

		public void TestSeaRailAdjustShipmentDatesIgnoringTimePart()
		{
			ZDateTime today = ZDateTime.Today;

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;

			Transport transport = consol.Transports[0];
			transport.JW_IsLinked = true;
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "1234";
			transport.JW_ETD = today.AddHours(24.5m);
			transport.JW_ETA = today.AddHours(47.5);

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_E_DEP = today.AddHours(24.25);
			shipment.JS_E_ARV = today.AddHours(47.75);

			Factory.Save();

			transport.JW_ETD = today.AddHours(24);
			transport.JW_ETA = today.AddHours(48);
			AssertEquals("ETD", today.AddHours(24).Date, shipment.JS_E_DEP.Date);
			AssertEquals("ETA", today.AddHours(48).Date, shipment.JS_E_ARV.Date);
		}

		#endregion

		#region TestAirRoadAdjustShipmentDatesByPartDays

		public void TestAirRoadAdjustShipmentDatesByPartDays()
		{
			ZDateTime today = ZDateTime.Today;

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;

			Transport transport = consol.Transports[0];
			transport.JW_IsLinked = true;
			transport.JW_VoyageFlight = "QF1234";
			transport.JW_ETD = today.AddHours(24.5m);
			transport.JW_ETA = today.AddHours(47.5);

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_E_DEP = today.AddHours(24.25);
			shipment.JS_E_ARV = today.AddHours(47.75);

			Factory.Save();

			transport.JW_ETD = today.AddHours(24);
			transport.JW_ETA = today.AddHours(48);
			AssertEquals("ETD", today.AddHours(23.75), shipment.JS_E_DEP);
			AssertEquals("ETA", today.AddHours(48.25), shipment.JS_E_ARV);
		}

		#endregion

		#region TestShowSubHouseBillShipments

		public void TestShowSubHouseBillShipments()
		{
			FreightDataRegistry.Instance.ShowSubHouseBills.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CommonConsol consol = Factory.New<CommonConsol>(); // should not access the registry until accessed

			FreightDataRegistry.Instance.ShowSubHouseBills.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("should default from the registry (A)", true, consol.ShowSubHouseBillShipments);
			AssertEquals("should default from the registry (B)", true, consol.GridShipments.ShouldShowChildShipments);

			FreightDataRegistry.Instance.ShowSubHouseBills.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("should cache value after first access (A)", true, consol.ShowSubHouseBillShipments);
			AssertEquals("should cache value after first access (B)", true, consol.GridShipments.ShouldShowChildShipments);

			consol.ShowSubHouseBillShipments = false;
			AssertEquals("value was manualy changed (A)", false, consol.ShowSubHouseBillShipments);
			AssertEquals("value was manualy changed (B)", false, consol.GridShipments.ShouldShowChildShipments);
		}

		#endregion

		#region Domestic Freight

		public void TestDomesticValidation()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "INBOM";
			consol.IsDomesticFreight = true;
			AssertHasErrors(consol.IsDomesticFreightInfo);

			consol.JK_RL_NKDischargePort = "";
			consol.IsDomesticFreight = true;
			AssertNoErrors(consol.IsDomesticFreightInfo);

			consol.JK_RL_NKDischargePort = "AUMEL";
			AssertEquals("IsDomesticFreight == 'Y'", true, consol.IsDomesticFreight);
			AssertNoErrors(consol.IsDomesticFreightInfo);

			consol.JK_RL_NKLoadPort = "USLAX";
			consol.IsDomesticFreight = true;
			AssertHasErrors(consol.IsDomesticFreightInfo);
		}

		#endregion

		#region Notes

		public void TestNoteTypes()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			Assert("PredefinedNoteTypes.Instance.DeliveryInstructionsNote should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.DeliveryInstructionsNote, consol.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.PickupInstructionsNote should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.PickupInstructionsNote, consol.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.HandlingInstructions should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.HandlingInstructions, consol.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.SpecialInstructions should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.SpecialInstructions, consol.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.InternalWorkNotes should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.InternalWorkNotes, consol.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.ForwardingInstructionNotes should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.ForwardingInstructionNotes, consol.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation, consol.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.ExportReceivalAdviceRemarks should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.ExportReceivalAdviceRemarks, consol.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.LoadListInstructions should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.LoadListInstructions, consol.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes, consol.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.AutoRatingAuditLog should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.AutoRatingAuditLog, consol.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.FaxEmailTransmissionLog should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.FaxEmailTransmissionLog, consol.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.UnmatchedOrgDetails should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.UnmatchedOrgDetails, consol.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.AdditionalSecurityInformation should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.AdditionalSecurityInformation, consol.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes should be in BO.NoteTypes", PredefinedNoteTypeExists(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes, consol.NoteTypes));
		}

		#endregion

		#region TestChangingContainerOnAPacklineRecalculatesGrossWeight

		public void TestChangingContainerOnAPacklineRecalculatesGrossWeight()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.Groupage;

			CommonContainer container = consol.Containers.AddNew();
			var refC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container.JC_RC = refC.PK;
			container.JC_ContainerMode = Constants.ContainerModes.LCL;

			Factory.Save();

			AssertEquals("Gross weight should be tare weight", 2280m, container.JC_GrossWeight);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_ActualVolume = 500m;
			shipment.JS_OuterPacks = 50;

			factory2.Save();

			shipment = Factory.Load<CommonShipment>(shipment.PK);
			consol.Shipments.Add(shipment);

			Factory.Save();

			shipment = factory2.Load<CommonShipment>(shipment.PK);
			consol = factory2.Load<CommonConsol>(consol.PK);
			container = factory2.Load<CommonContainer>(container.PK);
			PackLine packline = shipment.OuterPackLines[0];
			packline.CurrentConsol = consol;
			AssertEquals("CommonShipment packline has been packed into container on consol", container.PK, packline.JL_JC);
			AssertEquals("Container GrossWeight should include packline weight", 3280m, container.JC_GrossWeight);
			AssertEquals("Container GrossWeight should have been saved", false, container.HasChanges);

			packline.JL_JC = ZGuid.Empty;
			factory2.Save();

			AssertEquals("CommonShipment packline has been packed into container on consol", ZGuid.Empty, packline.JL_JC);
			AssertEquals("Container GrossWeight, packline removed", 2280m, container.JC_GrossWeight);
			AssertEquals("Container GrossWeight should have been saved", false, container.HasChanges);

			packline.JL_JC = container.PK;
			factory2.Save();

			AssertEquals("CommonShipment packline has been packed into container on consol", container.PK, packline.JL_JC);
			AssertEquals("Container GrossWeight, packline re-attached", 3280m, container.JC_GrossWeight);
			AssertEquals("Container GrossWeight should have been saved", false, container.HasChanges);
		}

		#endregion

		#region TestCharterNonCharterTransports

		public void TestCharterNonCharterTransports()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;

			Transport transport = consol.Transports[0];
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "Voyage";

			AssertNotNull(transport.Sailing);
			AssertEquals("voyage should not be chartered", false, transport.Sailing.Voyage.JV_IsChartered);

			transport.JW_IsCharter = true;
			AssertNotNull(transport.Sailing);
			AssertEquals("voyage should be chartered", true, transport.Sailing.Voyage.JV_IsChartered);
		}

		#endregion

		#region TestSettingTheConsolsLoadPortUpdatestTheTransportsLoadPortIfOnlyOne

		public void TestSettingTheConsolsLoadPortUpdatestTheTransportsLoadPortIfOnlyOne()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			AssertEquals("precondition:", 1, consol.Transports.Count);
			AssertEquals("precondition:", "", consol.Transports[0].JW_RL_NKLoadPort);

			Transport transport = consol.Transports[0];

			consol.JK_RL_NKLoadPort = HomePort;
			AssertEquals("Should have set the transports load port since there is only one", HomePort, transport.JW_RL_NKLoadPort);

			consol.JK_RL_NKLoadPort = AlternateHomePort;
			AssertEquals("Should have updated the transports load port since there is only one", AlternateHomePort, transport.JW_RL_NKLoadPort);

			Transport transport2 = consol.Transports.AddNew();
			consol.JK_RL_NKLoadPort = OverseasPort;
			AssertEquals("Should not have updated the transports since there are now 2 transports", AlternateHomePort, transport.JW_RL_NKLoadPort);
			AssertEquals("Should not have updated the transports since there are now 2 transports", "", transport2.JW_RL_NKLoadPort);

			transport.Delete();

			consol.JK_RL_NKLoadPort = OverseasPort2;
			AssertEquals("Should have updated the remaining transport as we are back down to one", OverseasPort2, transport2.JW_RL_NKLoadPort);
		}

		#endregion

		#region TestSettingTheConsolsDischargePortUpdatestTheTransportsDischargePortIfOnlyOne

		public void TestSettingTheConsolsDischargePortUpdatestTheTransportsDischargePortIfOnlyOne()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			AssertEquals("precondition:", 1, consol.Transports.Count);
			AssertEquals("precondition:", "", consol.Transports[0].JW_RL_NKDiscPort);

			Transport transport = consol.Transports[0];

			consol.JK_RL_NKDischargePort = HomePort;
			AssertEquals("Should have set the transports Discharge port since there is only one", HomePort, transport.JW_RL_NKDiscPort);

			consol.JK_RL_NKDischargePort = AlternateHomePort;
			AssertEquals("Should have updated the transports Discharge port since there is only one", AlternateHomePort, transport.JW_RL_NKDiscPort);

			Transport transport2 = consol.Transports.AddNew();
			consol.JK_RL_NKDischargePort = OverseasPort;
			AssertEquals("Should not have updated the transports since there are now 2 transports", AlternateHomePort, transport.JW_RL_NKDiscPort);
			AssertEquals("Should not have updated the transports since there are now 2 transports", "", transport2.JW_RL_NKDiscPort);

			transport.Delete();

			consol.JK_RL_NKDischargePort = OverseasPort2;
			AssertEquals("Should have updated the remaining transport as we are back down to one", OverseasPort2, transport2.JW_RL_NKDiscPort);
		}

		#endregion

		#region TestSettingTheConsolsTransportModeUpdatesTheTransportsTransportModeIfOnlyOne

		public void TestSettingTheConsolsTransportModeUpdatesTheTransportsTransportModeIfOnlyOne()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			AssertEquals("precondition:", 1, consol.Transports.Count);

			Transport transport = consol.Transports[0];

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(Constants.TransportModes.Sea, transport.JW_TransportMode);

			consol.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals(Constants.TransportModes.Road, transport.JW_TransportMode);

			Transport transport2 = consol.Transports.AddNew();
			AssertEquals(Constants.TransportModes.Road, transport2.JW_TransportMode);

			consol.JK_TransportMode = Constants.TransportModes.Rail;
			AssertEquals(Constants.TransportModes.Road, transport.JW_TransportMode);
			AssertEquals(Constants.TransportModes.Road, transport2.JW_TransportMode);

			consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals(Constants.TransportModes.Road, transport.JW_TransportMode);
			AssertEquals(Constants.TransportModes.Road, transport2.JW_TransportMode);

			transport.Delete();

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(Constants.TransportModes.Sea, transport2.JW_TransportMode);
		}

		#endregion

		#region JK_OA_PackDepotAddress and JK_OA_UnpackDepotAddress

		public void TestJK_OA_PackDepotAddress_and_UnpackDepotAddress()
		{
			var branch1 = Factory.Load<GlbBranch>(GlbCompany.CurrentCompany.Branches[0].PK);

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AAA";
			OrgAddress address1a = branch1.OrgProxy.Addresses.MainAddress;
			address1a.AddressCapability.SetCapabilityEnabled(nameof(AddressType.OFC));
			OrgAddress address1b = branch1.OrgProxy.Addresses.AddNew();
			address1b.AddressCapability.SetCapabilityEnabled(nameof(AddressType.PIC));
			org1.Addresses[0].OA_Address1 = "grrr";
			org1.Addresses[0].AddressCapability.SetCapabilityEnabled(nameof(AddressType.PIC));
			address1a.OA_Address1 = "aaaaa";
			address1b.OA_Address1 = "bbbbb";
			branch1.GB_OH_OrgProxy = branch1.GB_OH_OrgProxy.IsEmpty ? org1.PK : branch1.GB_OH_OrgProxy;
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "BBB";
			OrgAddress address2a = org2.MainAddress;
			OrgAddress address2b = org2.Addresses.AddNew();
			address2b.AddressCapability.SetCapabilityEnabled(nameof(AddressType.PIC));
			org2.Addresses[0].OA_Address1 = "A";
			address2a.OA_Address1 = "moo";
			address2b.OA_Address1 = "cow";

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = AlternateHomePort;
			Factory.Save();

			Assert("Precondition: expecting consol is domestic.", consol.IsDomestic());

			consol.JK_OA_PackDepotAddress = ZGuid.Empty;
			consol.JK_OA_UnpackDepotAddress = ZGuid.Empty;
			Assert("Not expecting Consol to be CFS Registered.", !consol.JK_IsCFS);

			consol.JK_OA_PackDepotAddress = address1a.PK;
			consol.JK_OA_UnpackDepotAddress = address2b.PK;
			AssertEquals("Preconditions: Is not CFS Yet", false, consol.JK_IsCFS);
			Factory.Save();
			Assert("Expecting Consol to be CFS Registered.", consol.JK_IsCFS);

			ZBool beforeJK_IsCFS = consol.JK_IsCFS;
			consol.JK_OA_PackDepotAddress = address2a.PK;
			Assert("Not expecting Consol CFS Registered to have changed.", beforeJK_IsCFS == consol.JK_IsCFS);

			consol.JK_RL_NKDischargePort = OverseasPort;
			Assert("Precondition: expecting consol is export.", consol.IsExport());

			consol.JK_OA_PackDepotAddress = ZGuid.Empty;
			consol.JK_OA_UnpackDepotAddress = ZGuid.Empty;
			Assert("Not expecting Consol CFS Registered to have changed.", beforeJK_IsCFS == consol.JK_IsCFS);
			Factory.Save();
			Assert("Not expecting Consol CFS Registered to have changed after saving Factory.", beforeJK_IsCFS == consol.JK_IsCFS);

			consol.JK_OA_PackDepotAddress = address1a.PK;
			Assert("Not expecting Consol CFS Registered to have changed.", beforeJK_IsCFS == consol.JK_IsCFS);
			Factory.Save();
			Assert("Not expecting Consol CFS Registered to have changed after saving Factory.", beforeJK_IsCFS == consol.JK_IsCFS);

			consol.JK_OA_PackDepotAddress = address2a.PK;
			consol.JK_OA_UnpackDepotAddress = address1b.PK;
			Assert("Not expecting Consol CFS Registered to have changed.", beforeJK_IsCFS == consol.JK_IsCFS);
			Factory.Save();
			Assert("Not expecting Consol CFS Registered to have changed after saving Factory.", beforeJK_IsCFS == consol.JK_IsCFS);

			consol.JK_RL_NKLoadPort = OverseasPort2;
			Assert("Precondition: expecting consol is cross trade.", consol.IsCrossTrade());

			consol.JK_OA_PackDepotAddress = ZGuid.Empty;
			consol.JK_OA_UnpackDepotAddress = ZGuid.Empty;
			Assert("Not expecting Consol CFS Registered to have changed.", beforeJK_IsCFS == consol.JK_IsCFS);
			Factory.Save();
			Assert("Not expecting Consol CFS Registered to have changed after saving Factory.", beforeJK_IsCFS == consol.JK_IsCFS);

			consol.JK_OA_PackDepotAddress = address1a.PK;
			Assert("Not expecting Consol CFS Registered to have changed.", beforeJK_IsCFS == consol.JK_IsCFS);
			Factory.Save();
			Assert("Not expecting Consol CFS Registered to have changed after saving Factory.", beforeJK_IsCFS == consol.JK_IsCFS);

			consol.JK_OA_UnpackDepotAddress = address1b.PK;
			Assert("Not expecting Consol CFS Registered to have changed.", beforeJK_IsCFS == consol.JK_IsCFS);
			Factory.Save();
			Assert("Not expecting Consol CFS Registered to have changed after saving Factory.", beforeJK_IsCFS == consol.JK_IsCFS);

			consol.JK_RL_NKDischargePort = AlternateHomePort;
			Assert("Precondition: expecting consol is import.", consol.IsImport());

			consol.JK_OA_PackDepotAddress = ZGuid.Empty;
			consol.JK_OA_UnpackDepotAddress = ZGuid.Empty;
			Assert("Not expecting Consol CFS Registered to have changed.", beforeJK_IsCFS == consol.JK_IsCFS);
			Factory.Save();
			Assert("Not expecting Consol CFS Registered to have changed after saving Factory.", beforeJK_IsCFS == consol.JK_IsCFS);

			consol.JK_OA_PackDepotAddress = address1a.PK;
			Assert("Not expecting Consol CFS Registered to have changed.", beforeJK_IsCFS == consol.JK_IsCFS);
			Factory.Save();
			Assert("Not expecting Consol CFS Registered to have changed after saving Factory.", beforeJK_IsCFS == consol.JK_IsCFS);

			consol.JK_OA_UnpackDepotAddress = address1b.PK;
			Assert("Not expecting Consol CFS Registered to have changed.", beforeJK_IsCFS == consol.JK_IsCFS);
			Factory.Save();
			Assert("Not expecting Consol CFS Registered to have changed after saving Factory.", beforeJK_IsCFS == consol.JK_IsCFS);
		}

		public void TestJK_IsCFSWhenOverseasBranchCreatesConsol()
		{
			using (RowFactory.SetCachedTables())
			{
				GlbBranch currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
				currentBranch.GB_RL_NKHomePort = "AUSYD";

				TestCaseHelper.ClearTable(TagRuleSchema.Constants.TableName);

				foreach (GlbBranch branch in Factory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK)))
				{
					branch.Delete();
				}

				GlbBranch perBranch = currentBranch.Company.Branches.AddNew();
				perBranch.GB_Code = "XX1";
				perBranch.GB_RL_NKHomePort = "AUPER";

				GlbBranch chiBranch = currentBranch.Company.Branches.AddNew();
				chiBranch.GB_Code = "XX2";
				chiBranch.GB_RL_NKHomePort = "USCHI";

				GlbBranch sinBranch = Factory.NewWithValidTestData<GlbBranch>();
				sinBranch.GB_Code = "XX3";
				sinBranch.GB_RL_NKHomePort = "SGSIN";

				GlbBranch hkgBranch = Factory.NewWithValidTestData<GlbBranch>();
				hkgBranch.GB_Code = "XX4";
				hkgBranch.GB_RL_NKHomePort = "HKHGK";

				GlbBranch torBranch = Factory.NewWithValidTestData<GlbBranch>();
				torBranch.GB_Code = "XX5";
				torBranch.GB_RL_NKHomePort = "CATOR";
				torBranch.GB_IsActive = ZBool.False;

				Factory.Save();

				AssertEquals("prerequisite - current location", "AUSYD", currentBranch.GB_RL_NKHomePort);
				AssertContainsExactElementsInAnyOrder("prerequisite - current company branches",
					new ZString[] { "AUSYD", "AUPER", "USCHI" }, currentBranch.Company.Branches.Select((branch) => branch.GB_RL_NKHomePort));

				var address = currentBranch.OrgProxy.MainAddress;

				AssertEquals("local no address", false, GetConsolIsCFS("AUPER", "AUSYD", null, null));
				AssertEquals("local", true, GetConsolIsCFS("AUPER", "AUSYD", null, address));
				AssertEquals("local", true, GetConsolIsCFS("AUPER", "AUSYD", address, null));
				AssertEquals("local import", true, GetConsolIsCFS("CATOR", "AUPER", null, address));
				AssertEquals("local export", true, GetConsolIsCFS("AUMEL", "SGSIN", address, null));

				AssertEquals("disabled branch import", false, GetConsolIsCFS("CATOR", "AUPER", address, null));
				AssertEquals("disabled branch export", false, GetConsolIsCFS("AUPER", "CATOR", null, address));

				AssertEquals("overseas branch import", true, GetConsolIsCFS("BRRIO", "USCHI", null, address));
				AssertEquals("overseas branch import", true, GetConsolIsCFS("BRRIO", "USSFO", null, address));
				AssertEquals("overseas branch export", true, GetConsolIsCFS("USCHI", "PLGDA", address, null));
				AssertEquals("overseas branch export", true, GetConsolIsCFS("USLAS", "PLGDA", address, null));

				AssertEquals("mankinis import to other company SGSIN branch", true, GetConsolIsCFS("KZALA", "SGSIN", null, address));
				AssertEquals("mankinis import to other company SGSIN branch no org proxy dest address", false, GetConsolIsCFS("KZALA", "SGSIN", address, null));
				AssertEquals("mankinis import/export to location without any branch", false, GetConsolIsCFS("KZALA", "PLWRO", address, null));

				using (chiBranch.SetAsTemporaryContext())
				{
					address = chiBranch.OrgProxy.MainAddress;
					AssertEquals("local no address", false, GetConsolIsCFS("USSFO", "USMIA", null, null));
					AssertEquals("local", true, GetConsolIsCFS("USSFO", "USMIA", null, address));
					AssertEquals("local", true, GetConsolIsCFS("USSFO", "USMIA", address, null));

					AssertEquals("import to other branch", true, GetConsolIsCFS("SGSIN", "AUSYD", null, address));
					AssertEquals("export from other branch", true, GetConsolIsCFS("AUSYD", "HKHKG", address, null));

					AssertEquals("disabled branch import", false, GetConsolIsCFS("CATOR", "USCHI", address, null));
					AssertEquals("disabled branch export", false, GetConsolIsCFS("USCHI", "CATOR", null, address));

					AssertEquals("mankinis import to other company SGSIN branch", true, GetConsolIsCFS("KZALA", "SGSIN", null, address));
					AssertEquals("mankinis import to other company SGSIN branch no org proxy dest address", false, GetConsolIsCFS("KZALA", "SGSIN", address, null));
					AssertEquals("mankinis import/export to location without any branch", false, GetConsolIsCFS("KZALA", "CATOR", address, null));
				}
			}
		}

		bool GetConsolIsCFS(ZString loadPort, ZString dischargePort, OrgAddress packAddress, OrgAddress unpackAddress)
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = loadPort;
			consol.JK_RL_NKDischargePort = dischargePort;

			if (packAddress != null)
			{
				consol.JK_OA_PackDepotAddress = packAddress.PK;
			}

			if (unpackAddress != null)
			{
				consol.JK_OA_UnpackDepotAddress = unpackAddress.PK;
			}

			Factory.Save();

			return consol.JK_IsCFS;
		}

		#endregion

		#region JK_OA_DepartureCTOAddress

		public void TestJK_OA_DepartureCTOAddress()
		{
			CommonConsol consol = GetNewConsol();
			consol.JK_OA_DepartureCTOAddress_ZAddress.DefaultAddressType = AddressType.PIC;
			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress address1 = org.Addresses.AddNew();
			address1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);

			consol.JK_RL_NKDischargePort = HomePort;
			consol.JK_OA_DepartureCTOAddress_ZAddress.OrgPK = org.PK;
			AssertEquals("JK_OA_DepartureCTOAddress should be defaulted to PAD", address1.PK, consol.JK_OA_DepartureCTOAddress);

			OrgAddress address2 = org.Addresses.AddNew();
			address2.AddressCapability.SetCapabilityEnabled(nameof(AddressType.PIC));

			address1.AddressCapability.SetIsNotMainAddress(OrgConstants.AddressType.PickupAndDelivery);
			consol.JK_OA_DepartureCTOAddress_ZAddress.OrgPK = ZGuid.Empty;
			consol.JK_OA_DepartureCTOAddress_ZAddress.OrgPK = org.PK;
			AssertEquals("JK_OA_DepartureCTOAddress should be defaulted to PIC", address2.PK, consol.JK_OA_DepartureCTOAddress);
		}

		#endregion

		#region TestUnsavedSailingShouldBeDeletedWhenSailingChanged

		public void TestUnsavedSailingShouldBeDeletedWhenSailingChanged()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];
			transport.JW_JX = ExportSailing.PK;
			transport.JW_JX = ExportSailing1.PK;
			AssertEquals("Old Sailing Should be deleted", true, ExportSailing.IsDeleted);
		}

		#endregion

		#region TestChangingToAnotherSavedSailingWithSameOrigin

		public void TestResetOriginAndDestinationDetailsWhenChangingSailing_ChangeLoad()
		{
			GenericResetOriginAndDestinationDetailsWhenChangingSailingTest(true, false);
		}

		public void TestResetOriginAndDestinationDetailsWhenChangingSailing_ChangeDischarge()
		{
			GenericResetOriginAndDestinationDetailsWhenChangingSailingTest(false, true);
		}

		public void TestResetOriginAndDestinationDetailsWhenChangingSailing_ChangeLoadAndDischarge()
		{
			GenericResetOriginAndDestinationDetailsWhenChangingSailingTest(true, true);
		}

		public void TestResetOriginAndDestinationDetailsWhenChangingSailing_SameSailing()
		{
			GenericResetOriginAndDestinationDetailsWhenChangingSailingTest(false, false);
		}

		public void GenericResetOriginAndDestinationDetailsWhenChangingSailingTest(bool changeOrigin, bool changeDestination)
		{
			ZDateTime newDate = ZDateTime.Today;
			ZDateTime date1 = newDate.AddDays(-10);
			ZDateTime date2 = newDate.AddDays(-5);
			ZDateTime date3 = newDate.AddDays(5);
			ZDateTime date4 = newDate.AddDays(10);

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = "DODGY ARSE";

			VoyageOrigin origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = OverseasPort;
			origin1.JA_E_DEP = date1;

			VoyageOrigin origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = OverseasPort2;
			origin2.JA_E_DEP = date2;

			VoyageDestination destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = HomePort;
			destination1.JB_E_ARV = date3;

			VoyageDestination destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = AlternateHomePort;
			destination2.JB_E_ARV = date4;

			voyage.GenerateSailings();

			JobSailing sailing1 = voyage.Sailings.GetSailingFromLoadAndDischarge(OverseasPort, HomePort);

			ZString loadPort = (changeOrigin ? OverseasPort2 : OverseasPort);
			ZString dischargePort = (changeDestination ? AlternateHomePort : HomePort);

			JobSailing sailing2 = voyage.Sailings.GetSailingFromLoadAndDischarge(loadPort, dischargePort);

			Factory.Save();

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			Transport transport = consol.Transports[0];
			transport.JW_JX = sailing1.PK;
			transport.JW_ETD = newDate;
			transport.JW_ETA = newDate;
			transport.JW_JX = sailing2.PK;

			if (changeOrigin)
			{
				AssertEquals("The Origin has changed, should be reset", date1, origin1.JA_E_DEP);
				AssertEquals("The Old Origin should not have changes", false, origin1.HasChanges);
			}
			else
			{
				AssertEquals("The Origin has not changed, leave alone", newDate, origin1.JA_E_DEP);
				AssertEquals("The Origin should still have changes", true, origin1.HasChanges);
			}

			if (changeDestination)
			{
				AssertEquals("The Destination has changed, should be reset", date3, destination1.JB_E_ARV);
				AssertEquals("The Old Destination should not have changes", false, destination1.HasChanges);
			}
			else
			{
				AssertEquals("The Destination has not changed, leave alone", newDate, destination1.JB_E_ARV);
				AssertEquals("The Destination should still have changes", true, destination1.HasChanges);
			}
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			AssertEquals("HumanReadableName without unique consign ref/masterbill", "Consol", consol.HumanReadableName);
			consol.JK_UniqueConsignRef = "uniqueconsignref";
			consol.JK_MasterBillNum = "master";
			AssertEquals("HumanReadableName with numbers", "Consol uniqueconsignref" + " " + "(Master Bill='master')", consol.HumanReadableName);
		}

		#endregion

		#region TestHumanReadableShortCutName

		public void TestHumanReadableShortcutName()
		{
			var consol = Factory.New<CommonConsol>();
			AssertEquals("Default shortcut", string.Empty, consol.HumanReadableShortcutName);
			consol.JK_UniqueConsignRef = "reference";
			consol.JK_RL_NKLoadPort = "USORD";
			consol.JK_RL_NKDischargePort = "AUSYD";
			AssertEquals("Shortcut with details", "reference - USORD - AUSYD", consol.HumanReadableShortcutName);
		}

		#endregion

		#region TestOrgFindboxListsDefaultsForNewChild

		public void TestOrganisationsFindBoxListDefaultFromUnmatchOrgNotes()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.JK_RL_NKLoadPort = HomePort;
			Consol.JK_RL_NKDischargePort = OverseasPort;

			var helper = new UnmatchOrgRecordTestHelper(Consol, CarrierRec, CreditorRec);

			helper.PopulateOrgDefaultsFromUnmatchedNote(Consol.GetType(), "SendingForwarderList", Consol.SendingForwarderList);
			AssertEquals("Sending Forwarder should have conditional defaults from notes", true, Consol.SendingForwarderList.DefaultsForNewChild.Count > 0);

			helper.PopulateOrgDefaultsFromUnmatchedNote(Consol.GetType(), "ReceivingForwarderList", Consol.ReceivingForwarderList);
			AssertEquals("Receiving Forwarder should have conditional defaults from notes", true, Consol.ReceivingForwarderList.DefaultsForNewChild.Count > 0);

			helper.PopulateOrgDefaultsFromUnmatchedNote(Consol.GetType(), "ShippingProviderList", Consol.ShippingProviderList);
			AssertEquals("Carrier should have conditional defaults from notes", true, Consol.ShippingProviderList.DefaultsForNewChild.Count > 0);
			helper.AssertOrgFieldDefaults(Consol.ShippingProviderList.DefaultsForNewChild, CarrierRec);

			helper.PopulateOrgDefaultsFromUnmatchedNote(Consol.GetType(), "CreditorList", Consol.CreditorList);
			AssertEquals("Creditor should have conditional defaults from notes", true, Consol.CreditorList.DefaultsForNewChild.Count > 0);
			helper.AssertOrgFieldDefaults(Consol.CreditorList.DefaultsForNewChild, CreditorRec);
		}

		#endregion

		#region  TestOrgFindboxListOrgnisationTypeForUnmatchOrgNotes

		UnmatchOrgRecord SendingForwarderRec
		{
			get
			{
				if (sendingForwarderRec == null)
				{
					sendingForwarderRec = UnmatchOrgRecordTestHelper.CreateUnmatchOrgRecord(OrganisationTypes.Forwarder,
						OrganisationsSubTypeList.Descriptions.SendingForwarder,
						"forwarder addr 1",
						"forwarder addr 2",
						"forwarderName",
						"4444",
						"VIC",
						"melbourne",
						"forwarder",
						"ownerCode",
						"");
				}
				return sendingForwarderRec;
			}
		}
		UnmatchOrgRecord sendingForwarderRec;

		UnmatchOrgRecord ReceivingForwarderRec
		{
			get
			{
				if (receivingForwarderRec == null)
				{
					receivingForwarderRec = UnmatchOrgRecordTestHelper.CreateUnmatchOrgRecord(OrganisationTypes.Forwarder,
						OrganisationsSubTypeList.Descriptions.ReceivingForwarder,
						"forwarder addr 1",
						"forwarder addr 2",
						"forwarderName",
						"4444",
						"VIC",
						"melbourne",
						"forwarder",
						"ownerCode",
						"");
				}
				return receivingForwarderRec;
			}
		}
		UnmatchOrgRecord receivingForwarderRec;
		public void TestSendingForwarderListOrganisationsTypeAndSubTypeFromUnmatchOrgNotes()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.JK_RL_NKLoadPort = HomePort;
			Consol.JK_RL_NKDischargePort = OverseasPort;

			var helper = new UnmatchOrgRecordTestHelper(Consol, SendingForwarderRec);
			var businessObjectCollection = (IBusinessObjectCollection)Consol.SendingForwarderList;
			businessObjectCollection.Parent = Consol;
			businessObjectCollection.ListPropertyDescriptor = new PropertyDescriptorForTest("Dummy", Array.Empty<Attribute>());
			var sendingForwarderFindBoxListProvider = new OrganisationsFindBoxListProvider(Consol.SendingForwarderList);
			sendingForwarderFindBoxListProvider.GetBusinessObjectFromCodeWithoutFilter("UNMATCHED");
			helper.AssertOrgFieldDefaults(Consol.SendingForwarderList.DefaultsForNewChild, SendingForwarderRec);
			AssertEquals("Forwarder", OrganisationTypes.Forwarder, Consol.SendingForwarderList.OrganisationType);
			AssertEquals("SendingForwarder", OrganisationsSubTypeList.Descriptions.SendingForwarder, Consol.SendingForwarderList.OrganisationSubType);
		}

		public void TestReceivingForwarderListOrganisationsTypeAndSubTypeFromUnmatchOrgNotes()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.JK_RL_NKLoadPort = HomePort;
			Consol.JK_RL_NKDischargePort = OverseasPort;

			var helper = new UnmatchOrgRecordTestHelper(Consol, ReceivingForwarderRec);
			var businessObjectCollection = (IBusinessObjectCollection)Consol.ReceivingForwarderList;
			businessObjectCollection.Parent = Consol;
			businessObjectCollection.ListPropertyDescriptor = new PropertyDescriptorForTest("Dummy", Array.Empty<Attribute>());
			var receivingForwarderFindBoxListProvider = new OrganisationsFindBoxListProvider(Consol.ReceivingForwarderList);
			receivingForwarderFindBoxListProvider.GetBusinessObjectFromCodeWithoutFilter("UNMATCHED");
			helper.AssertOrgFieldDefaults(Consol.ReceivingForwarderList.DefaultsForNewChild, ReceivingForwarderRec);
			AssertEquals("Forwarder", OrganisationTypes.Forwarder, Consol.ReceivingForwarderList.OrganisationType);
			AssertEquals("ReceivingForwarder", OrganisationsSubTypeList.Descriptions.ReceivingForwarder, Consol.ReceivingForwarderList.OrganisationSubType);
		}

		#endregion

		public void TestIManifestProvider_CustomsManifestVisibilityChanged_NoInitialEventChange()
		{
			visibilityChanged = false;
			IManifestProvider manifestProvider = GetNewConsol();
			manifestProvider.CustomsManifestVisibilityChanged += new EventHandler(OnManifestProvider_CustomsManifestVisibilityChanged);
			AssertEquals("Visibility shouldn't have changed initially", false, visibilityChanged);
		}

		public void TestIManifestProvider_CustomsManifestVisibilityChanged_JK_RL_NKLoadPort()
		{
			var consol = GetNewConsol();
			AssertCustomsManifestVisibilityChanged(consol, consol.JK_RL_NKLoadPortInfo, "XXXXX", "YYYYY");
		}

		public void TestIManifestProvider_CustomsManifestVisibilityChanged_JK_RL_NKDischargePort()
		{
			var consol = GetNewConsol();
			AssertCustomsManifestVisibilityChanged(consol, consol.JK_RL_NKDischargePortInfo, "SSSSS", "TTTTT");
		}

		public void TestIManifestProvider_CustomsManifestVisibilityChanged_JK_TransportMode()
		{
			var consol = GetNewConsol();
			AssertCustomsManifestVisibilityChanged(consol, consol.JK_TransportModeInfo, "AIR", "SEA");
		}

		public void TestIManifestProvider_CustomsManifestVisibilityChanged_JK_MasterBillNum()
		{
			var consol = GetNewConsol();
			AssertCustomsManifestVisibilityChanged(consol, consol.JK_MasterBillNumInfo, "98765432100", "98765432100");
		}

		public void TestIManifestProvider_CustomsManifestVisibilityChanged_JW_RL_NKDiscPort()
		{
			var consol = GetNewConsol();
			var tranport = consol.Transports.AddNew();
			AssertCustomsManifestVisibilityChanged(consol, tranport.JW_RL_NKDiscPortInfo, "AAAAA", "BBBBB");
		}

		public void TestIManifestProvider_CustomsManifestVisibilityChanged_JW_RL_NKLoadPort()
		{
			var consol = GetNewConsol();
			var tranport = consol.Transports.AddNew();
			AssertCustomsManifestVisibilityChanged(consol, tranport.JW_RL_NKLoadPortInfo, "CCCCC", "DDDDD");
		}

		public void TestIManifestProvider_CustomsManifestVisibilityChanged_JW_TransportMode()
		{
			var consol = GetNewConsol();
			var tranport = consol.Transports.AddNew();
			AssertCustomsManifestVisibilityChanged(consol, tranport.JW_TransportModeInfo, "ROA", "RAI");
		}

		public void TestVoyageLogsAddedToConsolLogs()
		{
			CommonConsol consol = GetNewConsol();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_JX = ImportSailing1.PK;
			Factory.Save();

			StmALog logFromVoyage = null;
			foreach (StmALog log in new StmALogCollectionView(consol))
			{
				if (log.SL_Table == AutoJobVoyage.Schema.TableName)
				{
					logFromVoyage = log;
					break;
				}
			}
			AssertNotNull("Failed to retrieve a Job Voyage Record from the Consol's Logs", logFromVoyage);
		}

		public void TestGetNewJK_UniqueConsignRef()
		{
			var consol = Factory.New<CommonConsol>();
			var consolUniqueConsignRef = consol.GetNewJK_UniqueConsignRef(Factory);
			AssertEquals("Common consol generate unique consign ref", "C00001000", consolUniqueConsignRef);
		}

		#region Test Shipments for Totalling Collection

		public void TestShipmentsForTotallingCollection()
		{
			decimal totalShipmentQuantity = 0;
			decimal totalShipmentWeight = 0;
			decimal totalShipmentVolume = 0;
			decimal totalShipmentLoadingMeters = 0;

			var consol = FreightTestHelper.GetConsol<CommonConsol>("CON", Factory);
			consol.JK_TotalShipmentQuantityInfo.ValueChanged += delegate
			{ totalShipmentQuantity = consol.JK_TotalShipmentQuantity; };
			consol.JK_TotalShipmentWeightInfo.ValueChanged += delegate
			{ totalShipmentWeight = consol.JK_TotalShipmentWeight; };
			consol.JK_TotalShipmentVolumeInfo.ValueChanged += delegate
			{ totalShipmentVolume = consol.JK_TotalShipmentVolume; };
			consol.JK_TotalShipmentLoadingMetersInfo.ValueChanged += delegate
			{ totalShipmentLoadingMeters = consol.JK_TotalShipmentLoadingMeters; };

			CommonShipment sTD = FreightTestHelper.GetShipment<CommonShipment>("STD", Constants.ShipmentTypes.StandardHouse, Factory);
			sTD.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			sTD.JS_OuterPacks = 100;
			sTD.JS_ActualWeight = 200;
			sTD.JS_ActualVolume = 300;
			sTD.JS_LoadingMeters = 4m;

			sTD.Consols.Add(consol);

			FreightTestHelper.AssertShipmentCollection("Shipments", consol.Shipments, sTD);
			FreightTestHelper.AssertShipmentCollection("ShipmentsForTotalling", consol.ShipmentsForTotalling, sTD);
			AssertEquals("Total Shipment Quantity", (ZDecimal)100, totalShipmentQuantity);
			AssertEquals("Total Shipment Weight", (ZDecimal)200, totalShipmentWeight);
			AssertEquals("Total Shipment Volume", (ZDecimal)300, totalShipmentVolume);
			AssertEquals("Total Shipment Loading Meters", 4m, totalShipmentLoadingMeters);

			var bCN_MAS = FreightTestHelper.GetShipment<CommonShipment>("BCN_MAS", Constants.ShipmentTypes.BuyersConsolLead, Factory);
			bCN_MAS.JS_OuterPacks = 11;
			bCN_MAS.JS_ActualWeight = 22;
			bCN_MAS.JS_ActualVolume = 33;

			var bCN_SUB = FreightTestHelper.GetShipment("BCN_SUB", bCN_MAS, Constants.ShipmentTypes.StandardHouse, Factory);
			bCN_SUB.JS_OuterPacks = 10;
			bCN_SUB.JS_ActualWeight = 20;
			bCN_SUB.JS_ActualVolume = 30;

			bCN_MAS.Consols.Add(consol);

			// Both the BCN Lead and Sub shipments should be included for totalling
			FreightTestHelper.AssertShipmentCollection("Shipments", consol.Shipments, sTD, bCN_MAS, bCN_SUB);
			FreightTestHelper.AssertShipmentCollection("ShipmentsForTotalling", consol.ShipmentsForTotalling, sTD, bCN_MAS, bCN_SUB);
			AssertEquals("Total Shipment Quantity", (ZDecimal)121, totalShipmentQuantity);
			AssertEquals("Total Shipment Weight", (ZDecimal)242, totalShipmentWeight);
			AssertEquals("Total Shipment Volume", (ZDecimal)363, totalShipmentVolume);

			var aSM_MAS = FreightTestHelper.GetShipment<CommonShipment>("ASM_MAS", Constants.ShipmentTypes.AssemblyMaster, Factory);
			aSM_MAS.JS_OuterPacks = 20;
			aSM_MAS.JS_ActualWeight = 40;
			aSM_MAS.JS_ActualVolume = 80;

			var aSM_SUB = FreightTestHelper.GetShipment("ASM_SUB", aSM_MAS, Constants.ShipmentTypes.StandardHouse, Factory);
			aSM_SUB.JS_OuterPacks = 13;
			aSM_SUB.JS_ActualWeight = 17;
			aSM_SUB.JS_ActualVolume = 19;

			aSM_MAS.Consols.Add(consol);

			// ASM sub-shipment should not be included in ShipmentsForTotalling
			FreightTestHelper.AssertShipmentCollection("Shipments", consol.Shipments, sTD, bCN_MAS, bCN_SUB, aSM_MAS, aSM_SUB);
			FreightTestHelper.AssertShipmentCollection("ShipmentsForTotalling", consol.ShipmentsForTotalling, sTD, bCN_MAS, bCN_SUB, aSM_MAS);
			AssertEquals("Total Shipment Quantity", (ZDecimal)141, totalShipmentQuantity);
			AssertEquals("Total Shipment Weight", (ZDecimal)282, totalShipmentWeight);
			AssertEquals("Total Shipment Volume", (ZDecimal)443, totalShipmentVolume);

			consol.Shipments.Remove(bCN_MAS);

			// BCN lead and sub shipment should be removed
			FreightTestHelper.AssertShipmentCollection("Shipments", consol.Shipments, sTD, bCN_SUB, aSM_MAS, aSM_SUB);
			FreightTestHelper.AssertShipmentCollection("ShipmentsForTotalling", consol.ShipmentsForTotalling, sTD, bCN_SUB, aSM_MAS);
			AssertEquals("Total Shipment Quantity", (ZDecimal)130, totalShipmentQuantity);
			AssertEquals("Total Shipment Weight", (ZDecimal)260, totalShipmentWeight);
			AssertEquals("Total Shipment Volume", (ZDecimal)410, totalShipmentVolume);

			aSM_SUB.JS_JS_ColoadMasterShipment = ZGuid.Empty;

			// Shipment should now be included
			FreightTestHelper.AssertShipmentCollection("Shipments", consol.Shipments, sTD, bCN_SUB, aSM_MAS, aSM_SUB);
			FreightTestHelper.AssertShipmentCollection("ShipmentsForTotalling", consol.ShipmentsForTotalling, sTD, bCN_SUB, aSM_MAS, aSM_SUB);
			AssertEquals("Total Shipment Quantity", (ZDecimal)143, totalShipmentQuantity);
			AssertEquals("Total Shipment Weight", (ZDecimal)277, totalShipmentWeight);
			AssertEquals("Total Shipment Volume", (ZDecimal)429, totalShipmentVolume);

			var cLD_MAS = FreightTestHelper.GetShipment<CommonShipment>("CLD_MAS", Constants.ShipmentTypes.CoLoadMaster, Factory);
			cLD_MAS.JS_OuterPacks = 1000;
			cLD_MAS.JS_ActualWeight = 2000;
			cLD_MAS.JS_ActualVolume = 3000;

			var cLD_SUB = FreightTestHelper.GetShipment("CLD_SUB", cLD_MAS, Constants.ShipmentTypes.StandardHouse, Factory);
			cLD_SUB.JS_OuterPacks = 10000;
			cLD_SUB.JS_ActualWeight = 20000;
			cLD_SUB.JS_ActualVolume = 30000;

			cLD_MAS.Consols.AddNew();
			cLD_SUB.Consols.Add(consol);

			FreightTestHelper.AssertShipmentCollection("Shipments", consol.Shipments, sTD, bCN_SUB, aSM_MAS, aSM_SUB, cLD_SUB);
			FreightTestHelper.AssertShipmentCollection("ShipmentsForTotalling", consol.ShipmentsForTotalling, sTD, bCN_SUB, aSM_MAS, aSM_SUB, cLD_SUB);
			AssertEquals("Total Shipment Quantity", (ZDecimal)10143, totalShipmentQuantity);
			AssertEquals("Total Shipment Weight", (ZDecimal)20277, totalShipmentWeight);
			AssertEquals("Total Shipment Volume", (ZDecimal)30429, totalShipmentVolume);
		}

		#endregion

		#region Test Prepaid and Collect Shipments

		public void TestPrepaidAndCollectShipmentCollections()
		{
			CommonConsolForTest consol = Factory.New<CommonConsolForTest>();
			AssertEquals("Precondition - No prepaid shipments when initialised", 0, consol.PrepaidShipmentForTesting.Count);
			AssertEquals("Precondition - No collect shipments when initialised", 0, consol.CollectShipmentsForTesting.Count);

			AssertAddsToPrepaidNotToCollect(Constants.IncoTerms.CostAndFreight, consol);
			AssertAddsToPrepaidNotToCollect(Constants.IncoTerms.CostInsuranceAndFreight, consol);
			AssertAddsToPrepaidNotToCollect(Constants.IncoTerms.CarriagePaidTo, consol);
			AssertAddsToPrepaidNotToCollect(Constants.IncoTerms.CarriageAndInsurancePaidTo, consol);
			AssertAddsToPrepaidNotToCollect(Constants.IncoTerms.DeliveredAtFrontier, consol);
			AssertAddsToPrepaidNotToCollect(Constants.IncoTerms.DeliveredExShip, consol);
			AssertAddsToPrepaidNotToCollect(Constants.IncoTerms.DeliveredExQuay, consol);
			AssertAddsToPrepaidNotToCollect(Constants.IncoTerms.DeliveredDutyUnpaid, consol);
			AssertAddsToPrepaidNotToCollect(Constants.IncoTerms.DeliveredDutyPaid, consol);

			AssertAddsToCollectNotToPrepaid(Constants.IncoTerms.ExWorks, consol);
			AssertAddsToCollectNotToPrepaid(Constants.IncoTerms.FreeCarrier, consol);
			AssertAddsToCollectNotToPrepaid(Constants.IncoTerms.FreeAlongsideShip, consol);
			AssertAddsToCollectNotToPrepaid(Constants.IncoTerms.FreeOnBoard, consol);

			CommonShipment collectShipment = GetAnyCollectShipment(consol.Shipments);
			AssertCollectionContains("CommonShipment currently on collect collection", collectShipment, consol.CollectShipmentsForTesting);
			collectShipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			Assert("CommonShipment now prepaid", collectShipment.IsPrepaid);
			AssertCollectionContains("CommonShipment now on Prepaid colleciton", collectShipment, consol.PrepaidShipmentForTesting);
			AssertCollectionNotContains("CommonShipment no longer on collect collection", collectShipment, consol.CollectShipmentsForTesting);

			CommonShipment prepaidShipment = GetAnyPrepaidShipment(consol.Shipments);
			AssertCollectionContains("CommonShipment currently on prepaid collection", prepaidShipment, consol.PrepaidShipmentForTesting);
			prepaidShipment.JS_INCO = Constants.IncoTerms.ExWorks;
			Assert("CommonShipment now prepaid", prepaidShipment.IsCollect);
			AssertCollectionContains("CommonShipment now on collect colleciton", prepaidShipment, consol.CollectShipmentsForTesting);
			AssertCollectionNotContains("CommonShipment no longer on prepaid collection", prepaidShipment, consol.PrepaidShipmentForTesting);

			collectShipment = null;
			collectShipment = GetAnyCollectShipment(consol.Shipments);
			AssertNotNull("Should be a collect CommonShipment there", collectShipment);
			AssertCollectionContains("It's on the collect collection", collectShipment, consol.CollectShipmentsForTesting);
			consol.Shipments.Remove(collectShipment);
			AssertCollectionNotContains("It's no longer on the collect collection", collectShipment, consol.CollectShipmentsForTesting);

			prepaidShipment = null;
			prepaidShipment = GetAnyPrepaidShipment(consol.Shipments);
			AssertNotNull("Should be a prepaid CommonShipment there", prepaidShipment);
			AssertCollectionContains("It's on the prepaid collection", prepaidShipment, consol.PrepaidShipmentForTesting);
			consol.Shipments.Remove(prepaidShipment);
			AssertCollectionNotContains("It's no longer on the prepaid collection", prepaidShipment, consol.PrepaidShipmentForTesting);
		}

		public void TestPrepaidAndCollectShipmentsWithBcnMaster()
		{
			AssertNoExceptionsWhenLoadingPrepaidOrCollectShipmentsWithBCNMaster(INCOTermType.Collect, ConsolModeTestType.BCN);
			AssertNoExceptionsWhenLoadingPrepaidOrCollectShipmentsWithBCNMaster(INCOTermType.Collect, ConsolModeTestType.Other);
			AssertNoExceptionsWhenLoadingPrepaidOrCollectShipmentsWithBCNMaster(INCOTermType.Prepaid, ConsolModeTestType.BCN);
			AssertNoExceptionsWhenLoadingPrepaidOrCollectShipmentsWithBCNMaster(INCOTermType.Prepaid, ConsolModeTestType.Other);
		}

		void AssertNoExceptionsWhenLoadingPrepaidOrCollectShipmentsWithBCNMaster(INCOTermType iNCOType, ConsolModeTestType consolMode)
		{
			string consolModeToUse = (consolMode == ConsolModeTestType.BCN) ? Constants.ContainerModes.BuyersConsol : Constants.ContainerModes.Other;
			string iNCOTermToUse = (iNCOType == INCOTermType.Collect) ? AnyCollectINCOTerm : AnyPrepaidINCOTerm;

			CommonConsolForTest consol = Factory.New<CommonConsolForTest>();
			consol.JK_ConsolMode = consolModeToUse;

			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_INCO = iNCOTermToUse;

			CommonShipment buyersConsolMaster = CommonShipment.New(Factory);
			buyersConsolMaster.JS_INCO = iNCOTermToUse;

			consol.Shipments.Add(buyersConsolMaster);
			consol.Shipments.Add(shipment);
			buyersConsolMaster.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			string message =
				"The collection should have at least one CommonShipment on it otherwise the INCOTerm " +
				"constant is wrong and there should be no exception when loading";

			if (iNCOType == INCOTermType.Collect)
			{
				AssertEquals(message, true, consol.CollectShipmentsForTesting.Count > 0);
			}
			else
			{
				AssertEquals(message, true, consol.PrepaidShipmentForTesting.Count > 0);
			}
		}

		enum INCOTermType { Prepaid, Collect }
		enum ConsolModeTestType { Other, BCN }
		const string AnyPrepaidINCOTerm = Constants.IncoTerms.CostAndFreight;
		const string AnyCollectINCOTerm = Constants.IncoTerms.ExWorks;

		CommonShipment GetAnyCollectShipment(ConsolShipmentCollection collection)
		{
			return collection.Cast<CommonShipment>().FirstOrDefault(shipment => shipment.IsCollect);
		}

		CommonShipment GetAnyPrepaidShipment(ConsolShipmentCollection collection)
		{
			return collection.Cast<CommonShipment>().FirstOrDefault(shipment => shipment.IsPrepaid);
		}

		void AssertAddsToPrepaidNotToCollect(string term, CommonConsolForTest consol)
		{
			AssertAddsToCorrectCollection(term, true, consol);
		}

		void AssertAddsToCollectNotToPrepaid(string term, CommonConsolForTest consol)
		{
			AssertAddsToCorrectCollection(term, false, consol);
		}

		void AssertAddsToCorrectCollection(string term, bool isPrepaid, CommonConsolForTest consol)
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_INCO = term;
			AssertEquals("Precondition: is prepaid", isPrepaid, shipment.IsPrepaid);
			AssertEquals("Precondition: is collect", !isPrepaid, shipment.IsCollect);
			consol.Shipments.Add(shipment);
			AssertCollectionContains("Prepaid shipments", shipment, consol.PrepaidShipmentForTesting, isPrepaid);
			AssertCollectionContains("Collect shipments", shipment, consol.CollectShipmentsForTesting, !isPrepaid);
		}

		#endregion

		#region FreightCost / ExchangeRates

		public void TestFreightCostCurrency()
		{
			CommonConsol consol = GetNewConsol();

			BusinessObject otherConsolCost = consol.CreateUnrelatedConsolCost("AUD", 8.9536m);

			consol.JK_TransportMode = "SEA";
			AssertEquals(Utilities.CurrencyUSD, consol.FreightCostsCurrency.PK.ToGuid());

			otherConsolCost[JobConsolCostSchema.E6_RX_NKCurrency] = "USD";

			consol.JK_TransportMode = "AIR";
			AssertEquals(GlbCompany.CurrentCompany.LocalCurrency.PK, consol.FreightCostsCurrency.PK);

			BusinessObject consolCost = consol.CreateConsolCost("USD", 8.9536m);
			AssertEquals("Freight Cost Ex Rate", 8.9536m, consol.FreightCostsExchangeRate);
		}

		public void TestFreightCostsAmount()
		{
			CommonConsol consol = GetNewConsol();

			BusinessObject otherConsolCost = consol.CreateUnrelatedConsolCost("USD", 8.9536m);
			AssertEquals(0m, consol.FreightCostsAmount);

			BusinessObject consolCost = consol.CreateConsolCost("USD", 8.9536m);
			BusinessObject postedConsolCost = consol.CreateConsolCost("USD", 7.6548m, true);

			AssertEquals("Freight Cost Ex Rate", 8.9536m, consol.FreightCostsExchangeRate);

			consolCost.Delete();
			AssertEquals("Freight Cost Ex Rate", 7.6548m, consol.FreightCostsExchangeRate);

			postedConsolCost[JobConsolCostSchema.E6_AH_APInvoice] = ZGuid.Empty;
			postedConsolCost.Delete();

			AssertEquals(0m, consol.FreightCostsAmount);
		}

		public void TestFreightCostsAmount_WithDifferentCurrencies()
		{
			CommonConsol consol = GetNewConsol();

			BusinessObject consolCost1 = consol.CreateConsolCost("USD", 8.9536m);
			BusinessObject consolCost2 = consol.CreateConsolCost("AUD", 8.9536m);

			AssertEquals(0m, consol.FreightCostsAmount);
		}

		public void TestFreightCostExRateForExport()
		{
			CommonConsol consol = GetNewConsol();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			AssertEquals("Should default rate to zero when no matching foreign currency rate can be found", 0m, consol.FreightCostsExchangeRate);

			consol.Transports[0].JW_Vessel = "QF";
			consol.Transports[0].JW_VoyageFlight = "1234";
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "SGSIN";
			consol.Transports[0].JW_ETD = ZDateTime.Now;
			consol.Transports[0].JW_ETA = ZDateTime.Now.AddDays(1);
			consol.Transports[0].JW_IsLinked = ZBool.True;

			AssertNotNull(consol.Transports[0].Sailing);
			AssertNotNull(consol.Transports[0].Sailing.Voyage);

			Transport secondTransport = consol.Transports.AddNew();
			secondTransport.JW_Vessel = "ABC";
			secondTransport.JW_VoyageFlight = "7890";
			secondTransport.JW_RL_NKLoadPort = "SGSIN";
			secondTransport.JW_RL_NKDiscPort = "USLAX";
			secondTransport.JW_ETD = ZDateTime.Now.AddDays(1);
			secondTransport.JW_ETA = ZDateTime.Now.AddDays(2);
			secondTransport.JW_IsLinked = ZBool.True;

			AssertNotNull(consol.Transports[1].Sailing);
			AssertNotNull(consol.Transports[1].Sailing.Voyage);

			VoyageExRate rate = consol.Transports[0].Sailing.Voyage.ExRates.AddNew();
			rate.E8_GC = GlbCompany.CurrentCompany.PK;
			rate.E8_RX_NKExCurrency = "USD";
			rate.E8_VoyageExchangeRate = 0.8237m;

			VoyageExRate secondRate = secondTransport.Sailing.Voyage.ExRates.AddNew();
			secondRate.E8_GC = GlbCompany.CurrentCompany.PK;
			secondRate.E8_RX_NKExCurrency = "USD";
			secondRate.E8_VoyageExchangeRate = 0.6831m;

			AssertEquals(0.8237m, consol.FreightCostsExchangeRate);

			BusinessObject foreignConsolCost = consol.CreateConsolCost("USD", 8.9536m);

			AssertEquals("Freight Cost Ex Rate", 8.9536m, consol.FreightCostsExchangeRate);

			foreignConsolCost.Delete();
			BusinessObject localConsolCost = consol.CreateConsolCost(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals("Freight Cost Ex Rate", 1m, consol.FreightCostsExchangeRate);
		}

		public void TestFreightCostExRateForExport_WithSpecifiedPortUnloco()
		{
			var consol = GetNewConsol();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";

			consol.Transports[0].JW_Vessel = "QF212";
			consol.Transports[0].JW_VoyageFlight = "1234";

			consol.Transports[0].Sailing.Voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUMEL";

			var rate = consol.Transports[0].Sailing.Voyage.ExRates.AddNew();
			rate.E8_RX_NKExCurrency = "USD";
			rate.E8_VoyageExchangeRate = 0.75m;

			var rate1 = consol.Transports[0].Sailing.Voyage.ExRates.AddNew();
			rate1.E8_RL_NKPort = "AUMEL";
			rate1.E8_RX_NKExCurrency = "USD";
			rate1.E8_VoyageExchangeRate = 0.76m;

			var rate2 = consol.Transports[0].Sailing.Voyage.ExRates.AddNew();
			rate2.E8_RL_NKPort = "SGSIN";
			rate2.E8_RX_NKExCurrency = "USD";
			rate2.E8_VoyageExchangeRate = 0.82m;

			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			AssertEquals("Freight Cost Ex Rate should be from load port when prepaid.", 0.75m, consol.FreightCostsExchangeRate);

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			AssertEquals("Freight Cost Ex Rate should be from discharge port when collect", 0.82m, consol.FreightCostsExchangeRate);
		}

		public void TestFreightCostExRateForImport()
		{
			CommonConsol consol = GetNewConsol();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			AssertEquals("Should default rate to zero when no matching foreign currency rate can be found", 0m, consol.FreightCostsExchangeRate);

			consol.Transports[0].JW_Vessel = "QF";
			consol.Transports[0].JW_VoyageFlight = "1234";
			consol.Transports[0].JW_RL_NKLoadPort = "USLAX";
			consol.Transports[0].JW_RL_NKDiscPort = "SGSIN";
			consol.Transports[0].JW_ETD = ZDateTime.Now;
			consol.Transports[0].JW_ETA = ZDateTime.Now.AddDays(1);
			consol.Transports[0].JW_IsLinked = ZBool.True;

			AssertNotNull(consol.Transports[0].Sailing);
			AssertNotNull(consol.Transports[0].Sailing.Voyage);

			Transport secondTransport = consol.Transports.AddNew();
			secondTransport.JW_Vessel = "ABC";
			secondTransport.JW_VoyageFlight = "7890";
			secondTransport.JW_RL_NKLoadPort = "SGSIN";
			secondTransport.JW_RL_NKDiscPort = "AUSYD";
			secondTransport.JW_ETD = ZDateTime.Now.AddDays(1);
			secondTransport.JW_ETA = ZDateTime.Now.AddDays(2);
			secondTransport.JW_IsLinked = ZBool.True;

			AssertNotNull(consol.Transports[1].Sailing);
			AssertNotNull(consol.Transports[1].Sailing.Voyage);

			VoyageExRate rate = consol.Transports[0].Sailing.Voyage.ExRates.AddNew();
			rate.E8_GC = GlbCompany.CurrentCompany.PK;
			rate.E8_RX_NKExCurrency = "USD";
			rate.E8_VoyageExchangeRate = 0.8237m;

			VoyageExRate secondRate = secondTransport.Sailing.Voyage.ExRates.AddNew();
			secondRate.E8_GC = GlbCompany.CurrentCompany.PK;
			secondRate.E8_RX_NKExCurrency = "USD";
			secondRate.E8_VoyageExchangeRate = 0.6831m;

			AssertEquals(0.6831m, consol.FreightCostsExchangeRate);

			BusinessObject foreignConsolCost = consol.CreateConsolCost("USD", 8.9536m);

			AssertEquals("Freight Cost Ex Rate", 8.9536m, consol.FreightCostsExchangeRate);

			foreignConsolCost.Delete();
			BusinessObject localConsolCost = consol.CreateConsolCost(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			AssertEquals("Freight Cost Ex Rate", 1m, consol.FreightCostsExchangeRate);
		}

		public void TestFreightCostExRateForImport_WithSpecifiedPortUnloco()
		{
			var consol = GetNewConsol();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "ITVCE";
			consol.JK_RL_NKDischargePort = "AUSYD";

			consol.Transports[0].JW_Vessel = "QF";
			consol.Transports[0].JW_VoyageFlight = "1234";

			var rate = consol.Transports[0].Sailing.Voyage.ExRates.AddNew();
			rate.E8_RX_NKExCurrency = "USD";
			rate.E8_VoyageExchangeRate = 0.8237m;

			AssertEquals("Precondition: Freight Cost Ex Rate should be non-port-specific", 0.8237m, consol.FreightCostsExchangeRate);

			var rate1 = consol.Transports[0].Sailing.Voyage.ExRates.AddNew();
			rate1.E8_RL_NKPort = "ITVCE";
			rate1.E8_RX_NKExCurrency = "USD";
			rate1.E8_VoyageExchangeRate = 1.23m;

			var rate2 = consol.Transports[0].Sailing.Voyage.ExRates.AddNew();
			rate2.E8_RL_NKPort = "AUSYD";
			rate2.E8_RX_NKExCurrency = "USD";
			rate2.E8_VoyageExchangeRate = 0.75m;

			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			AssertEquals("Freight Cost Ex Rate should be from load port when prepaid.", 1.23m, consol.FreightCostsExchangeRate);

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			AssertEquals("Freight Cost Ex Rate should be from discharge port when collect", 0.75m, consol.FreightCostsExchangeRate);
		}

		public void TestHasConsolCosts()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			CommonConsol consol = GetNewConsol();
			AssertEquals(false, consol.HasConsolCosts(currentCompany));

			BusinessObject otherConsolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			otherConsolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				otherConsolCost[JobConsolCostSchema.E6_ParentID] = ZGuid.NewZGuid();
				otherConsolCost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				otherConsolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			otherConsolCost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;

			AssertEquals(false, consol.HasConsolCosts(currentCompany));

			BusinessObject consolCostInOtherCompany = (BusinessObject)Factory.New<IJobConsolCost>();
			consolCostInOtherCompany.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCostInOtherCompany[JobConsolCostSchema.E6_ParentID] = consol.PK;
				consolCostInOtherCompany[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				consolCostInOtherCompany.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}

			consolCostInOtherCompany[JobConsolCostSchema.E6_GC] = ZGuid.NewZGuid();
			AssertEquals(false, consol.HasConsolCosts(currentCompany));

			BusinessObject consolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}

			consolCost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
			AssertEquals(true, consol.HasConsolCosts(currentCompany));
		}

		public void TestHasGatewaySellApportionments()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			CommonConsol consol = GetNewConsol();
			AssertEquals(false, consol.HasConsolCosts(currentCompany));

			BusinessObject consolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = ZGuid.NewZGuid();
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			AssertEquals(false, consol.HasGatewaySellApportionments(currentCompany));

			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			AssertEquals(false, consol.HasGatewaySellApportionments(currentCompany));

			consolCost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
			AssertEquals(false, consol.HasGatewaySellApportionments(currentCompany));

			consolCost[JobConsolCostSchema.E6_GatewaySellChargeID] = ZGuid.NewZGuid();
			AssertEquals(true, consol.HasGatewaySellApportionments(currentCompany));
		}

		public void TestGetExchangeRateFromFreightCostOrSchedule()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;

			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";

			VoyageExRate usdVoyageRate = voyage.ExRates.AddNew();
			usdVoyageRate.E8_RX_NKExCurrency = "USD";
			usdVoyageRate.E8_VoyageExchangeRate = 1.024m;

			VoyageExRate nzdVoyageRate = voyage.ExRates.AddNew();
			nzdVoyageRate.E8_RX_NKExCurrency = "NZD";
			nzdVoyageRate.E8_VoyageExchangeRate = 2.048m;

			CommonConsol consol = GetNewConsol();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_JX = voyage.Sailings[0].PK;

			BusinessObject consolCost = consol.CreateConsolCost("USD", 1.1111m);

			AssertEquals(0m, consol.GetExchangeRateFromFreightCostsOrSchedule(null));
			AssertEquals(1m, consol.GetExchangeRateFromFreightCostsOrSchedule(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));
			AssertEquals(0m, consol.GetExchangeRateFromFreightCostsOrSchedule("SGD"));
			AssertEquals(0m, consol.GetExchangeRateFromFreightCostsOrSchedule("EUR"));

			AssertEquals("Rate from consol cost", 1.1111m, consol.GetExchangeRateFromFreightCostsOrSchedule("USD"));
			AssertEquals("Rate from voyage", 2.048m, consol.GetExchangeRateFromFreightCostsOrSchedule("NZD"));

			consolCost.Delete();

			AssertEquals("Rate from voyage", 1.024m, consol.GetExchangeRateFromFreightCostsOrSchedule("USD"));
			AssertEquals("Rate from voyage", 2.048m, consol.GetExchangeRateFromFreightCostsOrSchedule("NZD"));
		}

		#endregion

		#region TestSetJK_OA_ShippingLineAddressForAirline

		public void TestSetJK_OA_ShippingLineAddressForAirline()
		{
			ZString currentCountry = GlbBranch.CurrentBranch.Country.Code;

			RefAirline airline1 = Factory.New<RefAirline>();
			airline1.RM_AirlineName1 = "International Air";
			airline1.RM_TwoCharacterCode = "ZZ";
			airline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "123";

			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_FullName = "Carrier AUS";
			OrgMiscServ misc1 = carrier1.MiscServ;
			misc1.OM_RM_Airline = airline1.PK;
			misc1.OM_RN_NKEXDefaultCntryOfOrigin = Constants.CountryCodes.Australia;

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_FullName = "Carrier NZ";
			OrgMiscServ misc2 = carrier2.MiscServ;
			misc2.OM_RM_Airline = airline1.PK;
			misc2.OM_RN_NKEXDefaultCntryOfOrigin = Constants.CountryCodes.NewZealand;

			CommonConsolForTest consol = Factory.New<CommonConsolForTest>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			GlbBranch.CurrentBranch.SetCountry(Constants.CountryCodes.NewZealand);

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "ZZ444";

			try
			{
				AssertEquals("Carrier should be Carrier2", carrier2.MainAddress.PK, consol.JK_OA_ShippingLineAddress);

				GlbBranch.CurrentBranch.SetCountry(Constants.CountryCodes.Australia);
				transport.JW_VoyageFlight = "";
				AssertEquals("Carrier should still be Carrier2", carrier2.MainAddress.PK, consol.JK_OA_ShippingLineAddress);

				transport.JW_VoyageFlight = "ZZ555";
				AssertEquals("Carrier should be Carrier1", carrier1.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
			}
			finally
			{
				GlbBranch.CurrentBranch.SetCountry(currentCountry);
			}
		}

		public void TestSetupShippingLineBaseOnAirline()
		{
			ZString currentCountry = GlbBranch.CurrentBranch.Country.Code;

			RefAirline airline1 = Factory.New<RefAirline>();
			airline1.RM_AirlineName1 = "Test Airline 1";
			airline1.RM_TwoCharacterCode = "ZZ";
			airline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "111";

			RefAirline airline2 = Factory.New<RefAirline>();
			airline2.RM_AirlineName1 = "Test Airline 2";
			airline2.RM_TwoCharacterCode = "XX";
			airline2.RM_EagleAddedAirlinePrefixOrAccountingCode = "222";

			OrgHeader carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_FullName = "Carrier AUS";
			OrgMiscServ miscServ1 = carrier1.MiscServ;
			miscServ1.OM_RM_Airline = airline1.PK;
			miscServ1.OM_RN_NKEXDefaultCntryOfOrigin = Constants.CountryCodes.Australia;

			OrgHeader carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_FullName = "Carrier NZ 1";
			OrgMiscServ miscServ2 = carrier2.MiscServ;
			miscServ2.OM_RM_Airline = airline1.PK;
			miscServ2.OM_RN_NKEXDefaultCntryOfOrigin = Constants.CountryCodes.NewZealand;

			OrgHeader carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			carrier3.OH_FullName = "Carrier NZ 2";
			OrgMiscServ miscServ3 = carrier3.MiscServ;
			miscServ3.OM_RM_Airline = airline2.PK;
			miscServ3.OM_RN_NKEXDefaultCntryOfOrigin = Constants.CountryCodes.NewZealand;

			OrgHeader carrier4 = Factory.NewWithValidTestData<OrgHeader>();
			carrier4.OH_FullName = "Carrier NZ 3";
			OrgMiscServ miscServ4 = carrier4.MiscServ;
			miscServ4.OM_RM_Airline = ZGuid.Empty;
			miscServ4.OM_RN_NKEXDefaultCntryOfOrigin = Constants.CountryCodes.NewZealand;

			CommonConsolForTest consol = Factory.New<CommonConsolForTest>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_TransportType = Constants.TransportPlanningType.Flight2;

			GlbBranch.CurrentBranch.SetCountry(Constants.CountryCodes.NewZealand);

			try
			{
				consol.SetShippingLineBaseOnAirlineForTest(airline1);
				AssertEquals("Carrier should be Carrier2", carrier2.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
				AssertEquals("Carrier should be Carrier2", carrier2.PK, consol.Transports.FirstTransportWithTransportMode(Constants.TransportModes.Air).CarrierPK);
				AssertEquals("Carrier should not be defaulted for Flight2 Transport", ZGuid.Empty, transport2.CarrierPK);

				consol.SetShippingLineBaseOnAirlineForTest(airline2);
				AssertEquals("Carrier should be Carrier1", carrier3.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
				AssertEquals("Carrier should be Carrier1", carrier3.PK, consol.Transports.FirstTransportWithTransportMode(Constants.TransportModes.Air).CarrierPK);
				AssertEquals("Carrier should not be defaulted for Flight2 Transport", ZGuid.Empty, transport2.CarrierPK);

				consol.JK_OA_ShippingLineAddress = carrier4.MainAddress.PK;
				consol.Transports.FirstTransportWithTransportMode(Constants.TransportModes.Air).CarrierPK = carrier4.PK;

				consol.SetShippingLineBaseOnAirlineForTest(airline1);
				AssertEquals("Carrier should not be changed", carrier4.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
				AssertEquals("Carrier should not be changed", carrier4.PK, consol.Transports.FirstTransportWithTransportMode(Constants.TransportModes.Air).CarrierPK);
				AssertEquals("Carrier should not be defaulted for Flight2 Transport", ZGuid.Empty, transport2.CarrierPK);
			}
			finally
			{
				GlbBranch.CurrentBranch.SetCountry(currentCountry);
			}
		}

		#endregion

		#region Notify Departure/Arrival Fields Changed

		public void TestJW_ETD_WhenChanged_ContainerEventDataVendorNotified_ForEachContainer()
		{
			using (MockContainerEventDataVendor.Instance)
			{
				Container1.JC_ContainerNum = "Container1";
				Container2.JC_ContainerNum = "Container2";
				Consol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Now;
				AssertEquals("Both containers should be notified of the ETD change", 2, MockContainerEventDataVendor.Instance.NotifyETDChangedCalledForContainers.Count);
				AssertEquals("ContainerEventDataVendor must be notified of the ETD change for each container", Container1.PK, MockContainerEventDataVendor.Instance.NotifyETDChangedCalledForContainers[0].PK);
				AssertEquals("ContainerEventDataVendor must be notified of the ETD change for each container", Container2.PK, MockContainerEventDataVendor.Instance.NotifyETDChangedCalledForContainers[1].PK);
			}
		}

		public void TestJW_ETA_WhenChanged_ContainerEventDataVendorNotified_ForEachContainer()
		{
			using (MockContainerEventDataVendor.Instance)
			{
				Container1.JC_ContainerNum = "Container1";
				Container2.JC_ContainerNum = "Container2";
				Consol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Now;
				AssertEquals("Both containers should be notified of the ETA change", 2, MockContainerEventDataVendor.Instance.NotifyETAChangedCalledForContainers.Count);
				AssertEquals("ContainerEventDataVendor must be notified of the ETA change for each container", Container1.PK, MockContainerEventDataVendor.Instance.NotifyETAChangedCalledForContainers[0].PK);
				AssertEquals("ContainerEventDataVendor must be notified of the ETA change for each container", Container2.PK, MockContainerEventDataVendor.Instance.NotifyETAChangedCalledForContainers[1].PK);
			}
		}

		public void TestJW_Vessel_WhenChanged_ContainerEventDataVendorNotified_ForEachContainer()
		{
			using (MockContainerEventDataVendor.Instance)
			{
				Container1.JC_ContainerNum = "Container1";
				Container2.JC_ContainerNum = "Container2";
				Consol.JK_TransportMode = Constants.TransportModes.Sea;
				Consol.Transports.MostInterestingTransport.JW_Vessel = "DIRECT CONDOR";
				AssertEquals("Both containers should be notified of the ETA change", 2, MockContainerEventDataVendor.Instance.NotifyVesselChangedCalledForContainers.Count);
				AssertEquals("ContainerEventDataVendor must be notified of the ETA change for each container", Container1.PK, MockContainerEventDataVendor.Instance.NotifyVesselChangedCalledForContainers[0].PK);
				AssertEquals("ContainerEventDataVendor must be notified of the ETA change for each container", Container2.PK, MockContainerEventDataVendor.Instance.NotifyVesselChangedCalledForContainers[1].PK);
			}
		}

		public void TestJW_VoyageFlight_WhenChanged_ContainerEventDataVendorNotified_ForEachContainer()
		{
			using (MockContainerEventDataVendor.Instance)
			{
				Container1.JC_ContainerNum = "Container1";
				Container2.JC_ContainerNum = "Container2";
				Consol.Transports.MostInterestingTransport.JW_VoyageFlight = "123";
				AssertEquals("Both containers should be notified of the ETA change", 2, MockContainerEventDataVendor.Instance.NotifyVoyageChangedCalledForContainers.Count);
				AssertEquals("ContainerEventDataVendor must be notified of the ETA change for each container", Container1.PK, MockContainerEventDataVendor.Instance.NotifyVoyageChangedCalledForContainers[0].PK);
				AssertEquals("ContainerEventDataVendor must be notified of the ETA change for each container", Container2.PK, MockContainerEventDataVendor.Instance.NotifyVoyageChangedCalledForContainers[1].PK);
			}
		}

		public void TestJW_DiscPort_WhenChanged_ContainerEventDataVendorNotified_ForEachContainer()
		{
			using (MockContainerEventDataVendor.Instance)
			{
				Container1.JC_ContainerNum = "Container1";
				Container2.JC_ContainerNum = "Container2";
				Consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "DEFRA";
				AssertEquals("Both containers should be notified of the ETA change", 2, MockContainerEventDataVendor.Instance.NotifyDischargePortChangedCalledForContainers.Count);
				AssertEquals("ContainerEventDataVendor must be notified of the ETA change for each container", Container1.PK, MockContainerEventDataVendor.Instance.NotifyDischargePortChangedCalledForContainers[0].PK);
				AssertEquals("ContainerEventDataVendor must be notified of the ETA change for each container", Container2.PK, MockContainerEventDataVendor.Instance.NotifyDischargePortChangedCalledForContainers[1].PK);
			}
		}

		public void TestShippingLineIsNotResetWhenTransportModeIsNotSea()
		{
			ZString[] modes = { Constants.TransportModes.Road, Constants.TransportModes.Rail, Constants.TransportModes.Air };
			foreach (ZString mode in modes)
			{
				CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
				consol.JK_TransportMode = mode;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "AUMEL";
				consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
				consol.Transports[0].JW_ETD = ZDateTime.Now;
				Factory.Save();

				consol.Transports[0].JW_Vessel = "111";
				AssertEquals(ShippingCompany1.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
			}
		}

		public void TestShippingLineIsNotResetWhenTransportLineIsEmpty()
		{
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			consol.Transports[0].JW_ETD = ZDateTime.Now;
			consol.Transports[0].JW_IsLinked = true;
			consol.Transports[0].JW_JX = ExportSailing1.PK;
			Factory.Save();

			consol.Transports[0].JW_Vessel = "111";
			AssertEquals(ShippingCompany1.MainAddress.PK, consol.JK_OA_ShippingLineAddress);

			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "333";

			consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;

			consol.Transports[0].JW_Vessel = vessel.RV_FK;
			AssertEquals(ShippingCompany1.MainAddress.PK, consol.JK_OA_ShippingLineAddress);

			consol.Transports[0].JW_Vessel = "111";
			vessel.RV_OH = ShippingCompany2.PK;
			consol.Transports[0].JW_Vessel = vessel.RV_FK;
			AssertEquals("Changing vessel should not reset shipping line", ShippingCompany1.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
		}

		CommonConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Factory.New<CommonConsol>();
					consol.JK_RL_NKLoadPort = AUSYDLoco;
					consol.JK_RL_NKDischargePort = USLAXLoco;
				}
				return consol;
			}
		}
		CommonConsol consol;

		CommonContainer Container1
		{
			get { return container1 ?? (container1 = Consol.Containers.AddNew()); }
		}
		CommonContainer container1;

		CommonContainer Container2
		{
			get { return container2 ?? (container2 = Consol.Containers.AddNew()); }
		}
		CommonContainer container2;

		#endregion

		#region Test New()

		public void TestNew()
		{
			CommonConsol consol = CommonConsol.New(Factory);

			AssertNotNull("CommonConsol.New(Factory) returned null.", consol);
			Assert("CommonConsol.New(Factory) did not return a typeof(CommonConsol).", typeof(CommonConsol).IsAssignableFrom(consol.GetType()));
		}

		#endregion

		#region IEDocsProvider Test

		public void TestGetEDocsProviderSupporter()
		{
			IEDocsProvider consol = Factory.New<CommonConsol>();
			AssertEquals("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter), consol.GetEDocsProviderSupporter().GetType());
		}

		#endregion

		#region IRelatedJobNumber Members

		public void TestIRelatedJobNumberMembers()
		{
			CommonConsol consol = GetNewConsol();
			consol.JK_UniqueConsignRef = "C00009999";
			string[] jobNumbers = ((IRelatedJobNumber)consol).JobNumber;
			AssertEquals("Number of elements in JobNumber", 1, jobNumbers.Length);
			AssertEquals("JobNumber must be equal Consol.JK_UniqueConsignRef", "C00009999", jobNumbers[0]);

			consol.Shipments.AddNew().JS_UniqueConsignRef = "S00009999";
			consol.Shipments.AddNew().JS_UniqueConsignRef = "S00008888";
			jobNumbers = ((IRelatedJobNumber)consol).JobNumber;
			AssertEquals("Number of elements in JobNumber", 3, jobNumbers.Length);
			AssertEquals("JobNumber must be equal correspondent Shipment.JS_UniqueConsignRef", "S00009999", jobNumbers[1]);
			AssertEquals("JobNumber must be equal correspondent Shipment.JS_UniqueConsignRef", "S00008888", jobNumbers[2]);
		}

		#endregion

		#region Copy / Clone

		public void TestCopy()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "Value";
			consol.JK_MasterBillNum = "300900";

			CommonConsol copiedConsol1 = (CommonConsol)((ITemplateCopyable)consol).TemplateCopy();

			AssertEquals("Unique Reference ID Cleared", ZString.Empty, copiedConsol1.JK_UniqueConsignRef);
			AssertEquals("Voyage / Vessel Info Cleared", ZString.Empty, copiedConsol1.JK_JX_JV_NKVessel);
			AssertEquals("Master bill prefix NOT cleared, but number cleared", "300", copiedConsol1.JK_MasterBillNum);

			AssertEquals("Transport Mode Same", consol.JK_TransportMode, copiedConsol1.JK_TransportMode);
			AssertEquals("Values Same", ZDateTime.Empty, copiedConsol1.JK_DateLastForeignPort);

			CommonConsol copiedConsol = null;

			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			consol.Transports.AddNew();
			consol.Transports.AddNew();

			consol.Transports[0].JW_Vessel = "Vessel1";
			consol.Transports[1].JW_Vessel = "Vessel2";

			consol.Transports[0].JW_VoyageFlight = "Voage1";
			consol.Transports[1].JW_VoyageFlight = "Voage2";

			foreach (Transport transport in consol.Transports)
			{
				foreach (ZPropertyInfo propertyInfo in transport.ZPropertyInfoHash)
				{
					if (propertyInfo.PropertyType == typeof(ZDateTime) && propertyInfo.HasSetter)
					{
						propertyInfo.Value = ZDateTime.Today;
					}
				}
			}

			copiedConsol = (CommonConsol)((ITemplateCopyable)consol).TemplateCopy();

			AssertEquals("Vessel must be Empty", ZString.Empty, copiedConsol.Transports[0].JW_Vessel);
			AssertEquals("Vessel must be Empty", ZString.Empty, copiedConsol.Transports[1].JW_Vessel);

			AssertEquals("VoyageFlight must be Empty", ZString.Empty, copiedConsol.Transports[0].JW_VoyageFlight);
			AssertEquals("VoyageFlight must be Empty", ZString.Empty, copiedConsol.Transports[1].JW_VoyageFlight);

			AssertEquals("2 shipments", 2, copiedConsol.Shipments.Count);
			AssertEquals("3 transports - 2 copied and 1 original", 3, copiedConsol.Transports.Count);

			foreach (Transport transport in copiedConsol.Transports)
			{
				foreach (ZPropertyInfo propertyInfo in transport.ZPropertyInfoHash)
				{
					if (propertyInfo.PropertyType == typeof(ZDateTime) && propertyInfo.HasSetter)
					{
						AssertEquals("Dates cleared", ZDateTime.Empty, propertyInfo.Value);
					}
				}
			}

			AssertEquals(0, copiedConsol.NotificationsIncludingChildren.Count());
		}

		public void TestOwnTemplateCopy()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "Value";
			consol.JK_MasterBillNum = "300900";

			CommonConsol copiedConsol1 = consol.TemplateCopy(true, true);

			AssertEquals("Unique Reference ID Cleared", ZString.Empty, copiedConsol1.JK_UniqueConsignRef);
			AssertEquals("Voyage / Vessel Info Cleared", ZString.Empty, copiedConsol1.JK_JX_JV_NKVessel);
			AssertEquals("Master bill prefix NOT cleared, but number cleared", "300", copiedConsol1.JK_MasterBillNum);

			AssertEquals("Transport Mode Same", consol.JK_TransportMode, copiedConsol1.JK_TransportMode);
			AssertEquals("Values Same", ZDateTime.Empty, copiedConsol1.JK_DateLastForeignPort);

			CommonConsol copiedConsol = null;

			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			consol.Transports.AddNew();
			consol.Transports.AddNew();

			consol.Transports[0].JW_Vessel = "Vessel1";
			consol.Transports[1].JW_Vessel = "Vessel2";

			consol.Transports[0].JW_VoyageFlight = "Voage1";
			consol.Transports[1].JW_VoyageFlight = "Voage2";

			foreach (Transport transport in consol.Transports)
			{
				foreach (ZPropertyInfo propertyInfo in transport.ZPropertyInfoHash)
				{
					if (propertyInfo.PropertyType == typeof(ZDateTime) && propertyInfo.HasSetter)
					{
						propertyInfo.Value = ZDateTime.Today;
					}
				}
			}

			copiedConsol = consol.TemplateCopy(true, true);

			AssertEquals("Vessel must be Empty", ZString.Empty, copiedConsol.Transports[0].JW_Vessel);
			AssertEquals("Vessel must be Empty", ZString.Empty, copiedConsol.Transports[1].JW_Vessel);

			AssertEquals("VoyageFlight must be Empty", ZString.Empty, copiedConsol.Transports[0].JW_VoyageFlight);
			AssertEquals("VoyageFlight must be Empty", ZString.Empty, copiedConsol.Transports[1].JW_VoyageFlight);

			AssertEquals("2 shipments", 2, copiedConsol.Shipments.Count);
			AssertEquals("3 transports - 2 copied and 1 original", 3, copiedConsol.Transports.Count);

			foreach (Transport transport in copiedConsol.Transports)
			{
				foreach (ZPropertyInfo propertyInfo in transport.ZPropertyInfoHash)
				{
					if (propertyInfo.PropertyType == typeof(ZDateTime) && propertyInfo.HasSetter)
					{
						AssertEquals("Dates cleared", ZDateTime.Empty, propertyInfo.Value);
					}
				}
			}

			AssertEquals(0, copiedConsol.NotificationsIncludingChildren.Count());

			copiedConsol = consol.TemplateCopy(false, true);

			AssertEquals("No shipments", 0, copiedConsol.Shipments.Count);
			AssertEquals("3 transports - 2 copied and 1 original", 3, copiedConsol.Transports.Count);
			AssertEquals(0, copiedConsol.NotificationsIncludingChildren.Count());

			copiedConsol = consol.TemplateCopy(true, false);

			AssertEquals("2 shipments", 2, copiedConsol.Shipments.Count);
			AssertEquals("1 transports - original", 1, copiedConsol.Transports.Count);
			AssertEquals(0, copiedConsol.NotificationsIncludingChildren.Count());

			foreach (ZPropertyInfo propertyInfo in copiedConsol.Transports[0].ZPropertyInfoHash)
			{
				if (propertyInfo.PropertyType == typeof(ZDateTime) && propertyInfo.HasSetter)
				{
					AssertEquals("Dates cleared", ZDateTime.Empty, propertyInfo.Value);
				}
			}
		}

		public void TestTemplateCopyWithMasterAndChildShipments()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			CommonShipment master1 = consol.Shipments.AddNew();
			master1.JS_RL_NKOrigin = "AUSYD";
			CommonShipment kid11 = consol.Shipments.AddNew();
			kid11.JS_RL_NKOrigin = "AUBNE";
			CommonShipment kid12 = consol.Shipments.AddNew();
			kid12.JS_RL_NKOrigin = "AUMEL";
			CommonShipment freeShipment1 = consol.Shipments.AddNew();
			freeShipment1.JS_RL_NKOrigin = "AUPER";

			CommonShipment master2 = consol.Shipments.AddNew();
			master2.JS_RL_NKOrigin = "AUADL";
			CommonShipment kid21 = consol.Shipments.AddNew();
			kid21.JS_RL_NKOrigin = "AUOOL";
			CommonShipment kid22 = consol.Shipments.AddNew();
			kid22.JS_RL_NKOrigin = "AUNTL";
			CommonShipment freeShipment2 = consol.Shipments.AddNew();
			freeShipment2.JS_RL_NKOrigin = "AUCBR";

			CommonShipment kid221 = consol.Shipments.AddNew();
			kid221.JS_RL_NKOrigin = "AUWOL";

			CommonShipment kid2211 = consol.Shipments.AddNew();
			kid2211.JS_RL_NKOrigin = "AUMCY";

			CommonShipment kid2212 = consol.Shipments.AddNew();
			kid2212.JS_RL_NKOrigin = "AUHBA";

			kid11.JS_JS_ColoadMasterShipment = master1.PK;
			kid12.JS_JS_ColoadMasterShipment = master1.PK;
			kid21.JS_JS_ColoadMasterShipment = master2.PK;
			kid22.JS_JS_ColoadMasterShipment = master2.PK;

			kid221.JS_JS_ColoadMasterShipment = kid22.PK;
			kid2211.JS_JS_ColoadMasterShipment = kid221.PK;
			kid2212.JS_JS_ColoadMasterShipment = kid221.PK;

			CommonConsol clonedConsol = consol.TemplateCopy(true, true);

			AssertEquals(11, clonedConsol.Shipments.Count);
			ZGuid parent1 = ZGuid.Empty;
			ZGuid parent2 = ZGuid.Empty;

			ZGuid kid22clonePK = ZGuid.Empty;
			ZGuid kid221clonePK = ZGuid.Empty;

			ZGuid kid11cloneParent = ZGuid.Empty;
			ZGuid kid12cloneParent = ZGuid.Empty;
			ZGuid kid21cloneParent = ZGuid.Empty;
			ZGuid kid22cloneParent = ZGuid.Empty;

			ZGuid kid221cloneParent = ZGuid.Empty;
			ZGuid kid2211cloneParent = ZGuid.Empty;
			ZGuid kid2212cloneParent = ZGuid.Empty;

			foreach (CommonShipment clone in clonedConsol.Shipments)
			{
				switch (clone.JS_RL_NKOrigin)
				{
					case "AUSYD":
						Assert(clone.JS_JS_ColoadMasterShipment.IsEmpty);
						parent1 = clone.PK;
						break;

					case "AUBNE":
						kid11cloneParent = clone.JS_JS_ColoadMasterShipment;
						break;

					case "AUMEL":
						kid12cloneParent = clone.JS_JS_ColoadMasterShipment;
						break;

					case "AUPER":
						Assert(clone.JS_JS_ColoadMasterShipment.IsEmpty);
						break;

					case "AUADL":
						Assert(clone.JS_JS_ColoadMasterShipment.IsEmpty);
						parent2 = clone.PK;
						break;

					case "AUOOL":
						kid21cloneParent = clone.JS_JS_ColoadMasterShipment;
						break;

					case "AUNTL":
						kid22cloneParent = clone.JS_JS_ColoadMasterShipment;
						kid22clonePK = clone.PK;
						break;

					case "AUCBR":
						Assert(clone.JS_JS_ColoadMasterShipment.IsEmpty);
						break;

					case "AUWOL":
						kid221cloneParent = clone.JS_JS_ColoadMasterShipment;
						kid221clonePK = clone.PK;
						break;

					case "AUMCY":
						kid2211cloneParent = clone.JS_JS_ColoadMasterShipment;
						break;

					case "AUHBA":
						kid2212cloneParent = clone.JS_JS_ColoadMasterShipment;
						break;

					default:
						Fail("Unexpected clone");
						break;
				}
			}

			AssertEquals(kid11cloneParent, parent1);
			AssertEquals(kid21cloneParent, parent2);
			AssertEquals(kid11cloneParent, kid12cloneParent);
			AssertEquals(kid21cloneParent, kid22cloneParent);

			AssertEquals(kid221cloneParent, kid22clonePK);
			AssertEquals(kid2211cloneParent, kid221clonePK);
			AssertEquals(kid2212cloneParent, kid221clonePK);

			Assert(!parent1.IsEmpty);
			Assert(!parent2.IsEmpty);
			Assert(!kid22clonePK.IsEmpty);
			Assert(!kid221clonePK.IsEmpty);
		}

		public void TestPDNNotCopiedWhileCloning()
		{
			var consol = Factory.New<CommonConsol>();
			var pdn = consol.Numbers.AddNew();
			pdn.CE_EntryType = "PDN";
			pdn.CE_EntryNum = "123";

			consol.JK_RL_NKDischargePort = "ILTLV";

			var clonedConsol = consol.TemplateCopy(true, true);

			AssertNull("PDN should not be copied", clonedConsol.Numbers.Cast<CusEntryNumber>().FirstOrDefault(n => n.CE_EntryType == "PDN"));
		}

		#endregion

		#region IAdditionalReferenceNumberSupporter Supporter

		CusEntryNumber GetCusEntryNumber(CommonConsol consol, ZString numberType, RefCountry country, string category)
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, numberType);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, country.Code);
			query.AddToFilter(CusEntryNumSchema.CE_ParentID, consol.PK);
			query.AddToFilter(CusEntryNumSchema.CE_Category, category);
			query.AddToFilter(CusEntryNumSchema.CE_ParentTable, consol.TableName);
			return Factory.LoadTop1<CusEntryNumber>(query);
		}

		void CreateCusEntryNumber(CommonConsol consol, ZString numberType, RefCountry country, ZString value, string category)
		{
			CusEntryNumber number = Factory.New<CusEntryNumber>();
			number.CE_Category = category;
			number.CE_EntryNum = value;
			number.CE_EntryType = numberType;
			number.CE_ParentID = consol.PK;
			number.CE_ParentTable = consol.TableName;
			number.CE_RN_NKCountryCode = country.Code;
		}

		public void TestAdditionalReferenceNumberSupporter()
		{
			CommonConsol consol = GetNewConsol();
			RefCountry australia = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Australia);
			RefCountry iceland = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Iceland);
			CreateCusEntryNumber(consol, "COC", iceland, "123", CusEntryNumber.Categories.AdditionalReferenceNumber);
			CreateCusEntryNumber(consol, "AAA", australia, "jjj", CusEntryNumber.Categories.AdditionalReferenceNumber);
			(consol as IAdditionalReferenceNumberSupporter).CreateOrUpdate("COC", australia.RN_Code, "123", new NotificationBuffer());
			CreateCusEntryNumber(consol, "COC", iceland, "123", CusEntryNumber.Categories.AdditionalReferenceNumber);
			CusEntryNumber number = GetCusEntryNumber(consol, "COC", australia, CusEntryNumber.Categories.AdditionalReferenceNumber);
			AssertNotNull("COC reference number", number);
			AssertEquals("123", number.CE_EntryNum);
			number = GetCusEntryNumber(consol, "AAA", australia, CusEntryNumber.Categories.AdditionalReferenceNumber);
			AssertNotNull("Reference number", number);
			(consol as IAdditionalReferenceNumberSupporter).CreateOrUpdate("COC", australia.RN_Code, "aaa", new NotificationBuffer());
			number = GetCusEntryNumber(consol, "COC", australia, CusEntryNumber.Categories.AdditionalReferenceNumber);
			AssertNotNull("COC reference number", number);
			AssertEquals("aaa", number.CE_EntryNum);
			NotificationBuffer notify = new NotificationBuffer();
			(consol as IAdditionalReferenceNumberSupporter).CreateOrUpdate("ZZ1", australia.RN_Code, "aaa", notify);
			number = GetCusEntryNumber(consol, "ZZ1", australia, CusEntryNumber.Categories.AdditionalReferenceNumber);
			AssertNull("ZZ1 reference number", number);
			AssertContains("No support for additional reference number type 'ZZ1'", notify.AsString);
			AssertEquals("Default value should be true", true, (consol as IAdditionalReferenceNumberSupporter).IncludeSpecialCustomsInstructionsItems);

			CreateCusEntryNumber(consol, "CON", australia, "jjj", CusEntryNumber.Categories.AdditionalReferenceNumber);
			(consol as IAdditionalReferenceNumberSupporter).CreateOrUpdate("CON", australia.RN_Code, "jjj", new NotificationBuffer());
			number = GetCusEntryNumber(consol, "CON", australia, CusEntryNumber.Categories.AdditionalReferenceNumber);
			AssertNotNull("Carrier Contract Number", number);
			AssertEquals("jjj", number.CE_EntryNum);
			(consol as IAdditionalReferenceNumberSupporter).CreateOrUpdate("CON", australia.RN_Code, "ccc", new NotificationBuffer());
			number = GetCusEntryNumber(consol, "CON", australia, CusEntryNumber.Categories.AdditionalReferenceNumber);
			AssertEquals("Number is updated", "ccc", number.CE_EntryNum);

			notify = new NotificationBuffer();
			(consol as IAdditionalReferenceNumberSupporter).CreateOrUpdate("CLC", australia.RN_Code, "CLC1", notify);
			number = GetCusEntryNumber(consol, "CLC", australia, CusEntryNumber.Categories.AdditionalReferenceNumber);
			AssertNotNull("Client Contract Number", number);
			AssertEquals("CLC1", number.CE_EntryNum);
			(consol as IAdditionalReferenceNumberSupporter).CreateOrUpdate("CLC", australia.RN_Code, "CLC2", new NotificationBuffer());
			number = GetCusEntryNumber(consol, "CLC", australia, CusEntryNumber.Categories.AdditionalReferenceNumber);
			AssertEquals("Number is updated", "CLC2", number.CE_EntryNum);

			AssertEquals("Consol should have 6 Additional Reference Numbers", 6, (consol as IAdditionalReferenceNumberSupporter).AdditionalReferenceNumbers.Count);
			Assert("Additional Reference Numbers is 'Numbers'", ReferenceEquals((consol as IAdditionalReferenceNumberSupporter).AdditionalReferenceNumbers, consol.Numbers));
		}

		#endregion

		#region IAdditionalReferenceNumberTypeProvider

		public void TestIAdditionalReferenceNumberTypeProvider()
		{
			var consol = Factory.New<CommonConsol>();
			var numberTypeProvider = consol as IAdditionalReferenceNumberTypeProvider;
			
			foreach (var countryCode in new[]
				{
					Constants.CountryCodes.Australia,
					Constants.CountryCodes.China,
					Constants.CountryCodes.HongKong,
					Constants.CountryCodes.Taiwan,
					Constants.CountryCodes.Iceland,
					Constants.CountryCodes.UnitedArabEmirates,
					Constants.CountryCodes.UnitedStates,
					Constants.CountryCodes.Germany,
					Constants.CountryCodes.Israel
				})
			{
				var port = consol.JK_RL_NKDischargePort = SetPort(countryCode);
				var actualList = numberTypeProvider.GetAdditionalReferenceNumberTypeList(CusEntryNumber.Categories.AdditionalReferenceNumber, countryCode);
				
				foreach (ICodeDescription pair in CusEntryNumLookups.GetAdditionalReferenceNumberTypes(new AdditionalReferenceNumberTypesParameters(countryCode) { Parent = consol, DischargeCountryCode = port.Left(2) }))
				{
					Assert($"Should contain {countryCode}/{pair.Code}", actualList.ContainsCode(pair.Code));
				}

				var nonCustomsAdditionalReferences = new ConsolNonCustomsAdditionalReferenceCodesCodeList();
				foreach (ICodeDescription pair in nonCustomsAdditionalReferences)
				{
					Assert($"Should contain {countryCode}/{pair.Code}", actualList.ContainsCode(pair.Code));
				}
			}
		}
		
		ZString SetPort(ZString countryCode)
		{
			var result = ZString.Empty;
			switch (countryCode)
			{
				case Constants.CountryCodes.Israel:
					result = "ILTLV";
					break;
			}
			return result;
		}
		
		#endregion

		#region Test FindBox Lists

		public void TestOrgCtoList()
		{
			BindToLists lists = BindToLists.GetCachedLists(new BusinessObjectFactory());
			lists.AirCTO_List.Load();
			lists.SeaCTO_List.Load();
			lists.OrgCTO_List.Load();

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.OrgDepartureCtoList.RemoveAll();
			Consol.OrgDepartureCtoList.Load();
			AssertEquals("Air CTO List", lists.AirCTO_List.Count, Consol.OrgDepartureCtoList.Count);
			Consol.OrgArrivalCtoList.RemoveAll();
			Consol.OrgArrivalCtoList.Load();
			AssertEquals("Air CTO List", lists.AirCTO_List.Count, Consol.OrgArrivalCtoList.Count);

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.OrgDepartureCtoList.RemoveAll();
			Consol.OrgDepartureCtoList.Load();
			AssertEquals("Sea CTO List", lists.SeaCTO_List.Count, Consol.OrgDepartureCtoList.Count);
			Consol.OrgArrivalCtoList.RemoveAll();
			Consol.OrgArrivalCtoList.Load();
			AssertEquals("Sea CTO List", lists.SeaCTO_List.Count, Consol.OrgArrivalCtoList.Count);

			Consol.JK_TransportMode = Constants.TransportModes.Road;
			Consol.OrgDepartureCtoList.RemoveAll();
			Consol.OrgDepartureCtoList.Load();
			AssertEquals("All CTO List", lists.OrgCTO_List.Count, Consol.OrgDepartureCtoList.Count);
			Consol.OrgArrivalCtoList.RemoveAll();
			Consol.OrgArrivalCtoList.Load();
			AssertEquals("All CTO List", lists.OrgCTO_List.Count, Consol.OrgArrivalCtoList.Count);
		}

		public void TestOrgListsDefaultPortFilter()
		{
			Consol.JK_RL_NKLoadPort = "";
			Consol.JK_RL_NKDischargePort = "";
			Assert("No default port filter expected", !Consol.OrgDepartureCtoList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			Assert("No default port filter expected", !Consol.OrgArrivalCtoList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			Assert("No default port filter expected", !Consol.OrgPackDepotList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			Assert("No default port filter expected", !Consol.OrgUnpackDepotList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			Assert("No default port filter expected", !Consol.OrgDepartureContainerYardList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			Assert("No default port filter expected", !Consol.OrgArrivalContainerYardList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));

			Consol.JK_RL_NKLoadPort = USLAXLoco;
			Assert("Default port filter expected", Consol.OrgDepartureCtoList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			Assert("No default port filter expected", !Consol.OrgArrivalCtoList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			Assert("Default port filter expected", Consol.OrgPackDepotList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			Assert("No default port filter expected", !Consol.OrgUnpackDepotList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			Assert("Default port filter expected", Consol.OrgDepartureContainerYardList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			Assert("No default port filter expected", !Consol.OrgArrivalContainerYardList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));

			Consol.JK_RL_NKDischargePort = AUSYDLoco;
			Assert("Default port filter expected", Consol.OrgDepartureCtoList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			Assert("Default port filter expected", Consol.OrgArrivalCtoList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			Assert("Default port filter expected", Consol.OrgPackDepotList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			Assert("Default port filter expected", Consol.OrgUnpackDepotList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			Assert("Default port filter expected", Consol.OrgDepartureContainerYardList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			Assert("Default port filter expected", Consol.OrgArrivalContainerYardList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
		}

		public void TestReceivingForwarderListType()
		{
			Consol.JK_AgentType = Constants.AgentType.Direct;
			Assert("Receiving forwarder list should be forwarder list", Consol.ReceivingForwarderList is ForwarderCollection);

			Consol.JK_AgentType = Constants.AgentType.Agent;
			Assert("Receiving forwarder list should be forwarder list", Consol.ReceivingForwarderList is ForwarderCollection);

			Consol.JK_AgentType = Constants.AgentType.Charter;
			Assert("Receiving forwarder list should be forwarder list", Consol.ReceivingForwarderList is ForwarderCollection);

			Consol.JK_AgentType = Constants.AgentType.CoLoad;
			Assert("Receiving forwarder list should be forwarder list", Consol.ReceivingForwarderList is ForwarderCollection);

			Consol.JK_AgentType = Constants.AgentType.Other;
			Assert("Receiving forwarder list should be forwarder list", Consol.ReceivingForwarderList is ForwarderCollection);
		}

		public void TestSendingForwarderListType()
		{
			Consol.JK_AgentType = Constants.AgentType.Direct;
			Assert("Sending forwarder list should be forwarder list", Consol.SendingForwarderList is ForwarderCollection);

			Consol.JK_AgentType = Constants.AgentType.Agent;
			Assert("Sending forwarder list should be forwarder list", Consol.SendingForwarderList is ForwarderCollection);

			Consol.JK_AgentType = Constants.AgentType.Charter;
			Assert("Sending forwarder list should be forwarder list", Consol.SendingForwarderList is ForwarderCollection);

			Consol.JK_AgentType = Constants.AgentType.CoLoad;
			Assert("Sending forwarder list should be forwarder list", Consol.SendingForwarderList is ForwarderCollection);

			Consol.JK_AgentType = Constants.AgentType.Other;
			Assert("Sending forwarder list should be forwarder list", Consol.SendingForwarderList is ForwarderCollection);
		}

		public void TestShippingProviderListType()
		{
			Consol.JK_TransportMode = "";
			AssertEquals("Shipping provider list with no transport mode", typeof(ShippingProviderCollection), Consol.ShippingProviderList.GetType());

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Shipping provider list with no transport mode", typeof(AirShippingProviderCollection), Consol.ShippingProviderList.GetType());

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Shipping provider list with no transport mode", typeof(SeaShippingProviderCollection), Consol.ShippingProviderList.GetType());

			Consol.JK_TransportMode = Constants.TransportModes.Rail;
			AssertEquals("Shipping provider list with no transport mode", typeof(RailShippingProviderCollection), Consol.ShippingProviderList.GetType());

			Consol.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Shipping provider list with no transport mode", typeof(LineHaulShippingProviderCollection), Consol.ShippingProviderList.GetType());

			Consol.JK_TransportMode = Constants.TransportModes.Mail;
			AssertEquals("Shipping provider list with no transport mode", typeof(AirShippingProviderCollection), Consol.ShippingProviderList.GetType());

			Consol.JK_TransportMode = Constants.TransportModes.Other;
			AssertEquals("Shipping provider list with no transport mode", typeof(ShippingProviderCollection), Consol.ShippingProviderList.GetType());
		}

		#endregion

		#region Test Defaults

		#region Sending/Receiving Forwarder Addresses

		public void TestSetDefaultSendingForwarderAddress()
		{
			var forwarder = GetForwarder();
			var direction = AgentDirectionExport;
			var country = HomePort.Left(2);

			forwarder.Factory.Save();

			TestDefaultForwarderAddress(Constants.TransportModes.Air, direction, forwarder, country);
			TestDefaultForwarderAddress(Constants.TransportModes.Rail, direction, forwarder, country);
			TestDefaultForwarderAddress(Constants.TransportModes.Road, direction, forwarder, country);
			TestDefaultForwarderAddress(Constants.TransportModes.Sea, direction, forwarder, country);
		}

		public void TestSetDefaultReceivingForwarderAddress()
		{
			var forwarder = GetForwarder();
			var direction = AgentDirectionImport;
			var country = HomePort.Left(2);

			forwarder.Factory.Save();

			TestDefaultForwarderAddress(Constants.TransportModes.Air, direction, forwarder, country);
			TestDefaultForwarderAddress(Constants.TransportModes.Rail, direction, forwarder, country);
			TestDefaultForwarderAddress(Constants.TransportModes.Road, direction, forwarder, country);
			TestDefaultForwarderAddress(Constants.TransportModes.Sea, direction, forwarder, country);
		}

		void TestDefaultForwarderAddress(string transportMode, string direction, OrgHeader forwarder, ZString country)
		{
			var proxy = GetForwarder();
			proxy.Factory.Save();

			Consol.JK_TransportMode = transportMode;

			if (direction == AgentDirectionExport)
			{
				Consol.JK_RL_NKLoadPort = ZString.Empty;
			}
			else
			{
				Consol.JK_RL_NKDischargePort = ZString.Empty;
			}

			AssertForwarderAddress(direction, forwarder, ZGuid.Empty, forwarder.MainAddress.PK);
			AssertProxyForwarderAddress(direction, proxy, proxy.MainAddress.PK);

			if (direction == AgentDirectionExport)
			{
				Consol.JK_RL_NKLoadPort = HomePort;
			}
			else
			{
				Consol.JK_RL_NKDischargePort = HomePort;
			}

			AssertForwarderAddress(direction, forwarder, ZGuid.Empty, forwarder.MainAddress.PK);
			AssertProxyForwarderAddress(direction, proxy, proxy.MainAddress.PK);

			var handlerForwarder = GetForwarder();

			AddForwarderAgentAddress(handlerForwarder, country, transportMode, AgentDirectionBoth, AgentStatusHandles);
			AssertProxyForwarderAddress("HAN should no longer have any effect", direction, handlerForwarder, handlerForwarder.MainAddress.PK);

			AddForwarderAgentAddress(handlerForwarder, country, transportMode, direction, AgentStatusHandles);
			AssertProxyForwarderAddress("HAN should no longer have any effect", direction, handlerForwarder, handlerForwarder.MainAddress.PK);

			AddForwarderAgentAddress(handlerForwarder, HomePort, transportMode, AgentDirectionBoth, AgentStatusHandles);
			AssertProxyForwarderAddress("HAN should no longer have any effect", direction, handlerForwarder, handlerForwarder.MainAddress.PK);

			AddForwarderAgentAddress(handlerForwarder, HomePort, transportMode, direction, AgentStatusHandles);
			AssertProxyForwarderAddress("HAN should no longer have any effect", direction, handlerForwarder, handlerForwarder.MainAddress.PK);

			var aPP_C_BTH = GetForwarderAgentAddress(AgentStatusAppointed, country, AgentDirectionBoth, transportMode);
			AssertForwarderAddress(direction, forwarder, aPP_C_BTH.PK, forwarder.MainAddress.PK);
			AssertProxyForwarderAddress(direction, proxy, aPP_C_BTH.PK);

			var aPP_C_DIR = GetForwarderAgentAddress(AgentStatusAppointed, country, direction, transportMode);
			AssertForwarderAddress(direction, forwarder, aPP_C_DIR.PK, forwarder.MainAddress.PK);

			var aPP_P_BTH = GetForwarderAgentAddress(AgentStatusAppointed, HomePort, AgentDirectionBoth, transportMode);
			AssertForwarderAddress(direction, forwarder, aPP_P_BTH.PK, forwarder.MainAddress.PK);

			var aPP_P_DIR = GetForwarderAgentAddress(AgentStatusAppointed, HomePort, direction, transportMode);
			AssertForwarderAddress(direction, forwarder, aPP_P_DIR.PK, forwarder.MainAddress.PK);

			var pUB_C_BTH = GetForwarderAgentAddress(AgentStatusPublished, country, AgentDirectionBoth, transportMode);
			AssertForwarderAddress(direction, forwarder, pUB_C_BTH.PK, forwarder.MainAddress.PK);

			var pUB_C_DIR = GetForwarderAgentAddress(AgentStatusPublished, country, direction, transportMode);
			AssertForwarderAddress(direction, forwarder, pUB_C_DIR.PK, forwarder.MainAddress.PK);

			GetForwarderAgentAddress(AgentStatusGatewayAgent, country, AgentDirectionBoth, transportMode);
			AssertForwarderAddress("GTA should no longer have any effect", direction, forwarder, pUB_C_DIR.PK, forwarder.MainAddress.PK);

			GetForwarderAgentAddress(AgentStatusGatewayAgent, country, direction, transportMode);
			AssertForwarderAddress("GTA should no longer have any effect", direction, forwarder, pUB_C_DIR.PK, forwarder.MainAddress.PK);

			GetForwarderAgentAddress(AgentStatusGatewayAgentWithTariff, country, AgentDirectionBoth, transportMode);
			AssertForwarderAddress("GTT should no longer have any effect", direction, forwarder, pUB_C_DIR.PK, forwarder.MainAddress.PK);

			GetForwarderAgentAddress(AgentStatusGatewayAgentWithTariff, country, direction, transportMode);
			AssertForwarderAddress("GTT should no longer have any effect", direction, forwarder, pUB_C_DIR.PK, forwarder.MainAddress.PK);

			var pUB_P_BTH = GetForwarderAgentAddress(AgentStatusPublished, HomePort, AgentDirectionBoth, transportMode);
			AssertForwarderAddress(direction, forwarder, pUB_P_BTH.PK, forwarder.MainAddress.PK);

			var pUB_P_DIR = GetForwarderAgentAddress(AgentStatusPublished, HomePort, direction, transportMode);
			AssertForwarderAddress(direction, forwarder, pUB_P_DIR.PK, forwarder.MainAddress.PK);

			var proxyAddressToFallback = AddForwarderAgentAddress(forwarder, country, transportMode, AgentDirectionBoth, AgentStatusAppointed);
			AssertForwarderAddress(direction, forwarder, pUB_P_DIR.PK, proxyAddressToFallback.PK);

			forwarder.AppointedAgentPorts.RemoveAndDeleteAll();
			forwarder.AppointedGatewayAgentPorts.RemoveAndDeleteAll();
			forwarder.Addresses.RemoveAndDelete(proxyAddressToFallback);
			forwarder.Factory.Save();
			AssertForwarderAddress(direction, forwarder, pUB_P_DIR.PK, forwarder.MainAddress.PK);
		}

		void AssertForwarderAddress(string direction, OrgHeader forwarder, ZGuid expectedAddress, ZGuid expectedFallbackAddress)
		{
			AssertForwarderAddress(string.Empty, direction, forwarder, expectedAddress, expectedFallbackAddress);
		}

		void AssertForwarderAddress(string message, string direction, OrgHeader forwarder, ZGuid expectedAddress, ZGuid expectedFallbackAddress)
		{
			if (direction == AgentDirectionExport)
			{
				AssertSendingForwarderAddress(message, forwarder, expectedAddress, expectedFallbackAddress);
			}
			else
			{
				AssertReceivingForwarderAddress(message, forwarder, expectedAddress, expectedFallbackAddress);
			}
		}

		void AssertSendingForwarderAddress(string message, OrgHeader forwarder, ZGuid expectedAddress, ZGuid expectedFallbackAddress)
		{
			Consol.SetDefaultSendingForwarderAddress();
			AssertEquals(message + " JK_OA_SendingForwarderAddress", expectedAddress, consol.JK_OA_SendingForwarderAddress);

			Consol.SetDefaultSendingForwarderAddress(forwarder);
			AssertEquals(message + " JK_OA_SendingForwarderAddress: fallback from dbo.OrgHeader", expectedFallbackAddress, consol.JK_OA_SendingForwarderAddress);

			Consol.SetDefaultSendingForwarderAddress(forwarder.PK);
			AssertEquals(message + " JK_OA_SendingForwarderAddress: fallback from OrgHeader.PK", expectedFallbackAddress, consol.JK_OA_SendingForwarderAddress);
		}

		void AssertReceivingForwarderAddress(string message, OrgHeader forwarder, ZGuid expectedAddress, ZGuid expectedFallbackAddress)
		{
			Consol.SetDefaultReceivingForwarderAddress();
			AssertEquals(message + " JK_OA_ReceivingForwarderAddress", expectedAddress, consol.JK_OA_ReceivingForwarderAddress);

			Consol.SetDefaultReceivingForwarderAddress(forwarder);
			AssertEquals(message + " JK_OA_ReceivingForwarderAddress: fallback from dbo.OrgHeader", expectedFallbackAddress, consol.JK_OA_ReceivingForwarderAddress);

			Consol.SetDefaultReceivingForwarderAddress(forwarder.PK);
			AssertEquals(message + " JK_OA_ReceivingForwarderAddress: fallback from OrgHeader.PK", expectedFallbackAddress, consol.JK_OA_ReceivingForwarderAddress);
		}

		void AssertProxyForwarderAddress(string direction, OrgHeader proxy, ZGuid expectedProxyAddress)
		{
			AssertProxyForwarderAddress(null, direction, proxy, expectedProxyAddress);
		}

		void AssertProxyForwarderAddress(string message, string direction, OrgHeader proxy, ZGuid expectedProxyAddress)
		{
			if (direction == AgentDirectionExport)
			{
				AssertSendingProxyForwarderAddress(message, proxy, expectedProxyAddress);
			}
			else
			{
				AssertReceivingProxyForwarderAddress(message, proxy, expectedProxyAddress);
			}
		}

		void AssertSendingProxyForwarderAddress(string message, OrgHeader proxy, ZGuid expectedProxyAddress)
		{
			Consol.SetDefaultSendingForwarderAddressWithFallbackToProxy(proxy);
			AssertEquals(message ?? "JK_OA_SendingForwarderAddress", expectedProxyAddress, consol.JK_OA_SendingForwarderAddress);
		}

		void AssertReceivingProxyForwarderAddress(string message, OrgHeader proxy, ZGuid expectedProxyAddress)
		{
			Consol.SetDefaultReceivingForwarderAddressWithFallbackToProxy(proxy);
			AssertEquals(message ?? "JK_OA_ReceivingForwarderAddress", expectedProxyAddress, consol.JK_OA_ReceivingForwarderAddress);
		}

		#endregion

		#region JK_OA_PackDepotAddress_ZAddressOrg

		public void TestDefaultJK_OA_PackDepotAddress_ZAddressOrg()
		{
			OrgHeader orgALL = Factory.New<OrgHeader>();
			OrgHeader orgAIR = Factory.New<OrgHeader>();
			OrgHeader orgSEA = Factory.New<OrgHeader>();
			OrgHeader orgFCL = Factory.New<OrgHeader>();
			OrgHeader orgLCL = Factory.New<OrgHeader>();
			OrgHeader orgMEL = Factory.New<OrgHeader>();
			OrgHeader orgAU = Factory.New<OrgHeader>();

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			Consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK);

			OrgHeader mainOrg = Factory.New<OrgHeader>();
			mainOrg.SetRelatedParty(orgAIR, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Air, ZString.Empty);
			Consol.JK_OA_SendingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(ZGuid.Empty, Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK);

			mainOrg.SetRelatedParty(orgALL, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.All, ZString.Empty);
			Consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_SendingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgALL.PK, Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK);

			mainOrg.SetRelatedParty(orgSEA, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, ZString.Empty);
			Consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_SendingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgSEA.PK, Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK);

			mainOrg.SetRelatedParty(orgFCL, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			mainOrg.SetRelatedParty(orgLCL, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			Consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_SendingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgSEA.PK, Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK);

			Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			Consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_SendingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgFCL.PK, Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK);

			Consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			Consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_SendingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgLCL.PK, Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK);

			mainOrg.SetRelatedParty(orgMEL, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, "AUMEL");
			mainOrg.SetRelatedParty(orgAU, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, "AU");

			Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			Consol.JK_RL_NKLoadPort = "AUMEL";
			Consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_SendingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgMEL.PK, Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK);

			Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_SendingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgAU.PK, Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK);

			Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			Consol.JK_RL_NKLoadPort = "USCHI";
			Consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_SendingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgFCL.PK, Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK);
		}

		public void TestSetSendingForwarderAddress_AddressIsEmpty_DoNotUpdateDepotAddress()
		{
			var forwarder = Factory.New<OrgHeader>();
			var cfs = Factory.New<OrgHeader>();

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.JK_OA_SendingForwarderAddress = forwarder.MainAddress.PK;
			Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK = cfs.PK;
			Consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;

			AssertEquals(cfs.PK, Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK);
		}

		public void TestSetSendingForwarderAddress_ForwarderDoesntHaveRelatedCFS_DoNotUpdateDepotAddress()
		{
			var forwarder = Factory.New<OrgHeader>();
			var cfs = Factory.New<OrgHeader>();

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK = cfs.PK;
			Consol.JK_OA_SendingForwarderAddress = forwarder.MainAddress.PK;

			AssertEquals(cfs.PK, Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK);
		}

		public void TestSetSendingForwarderAddress_AddressIsEmpty_DoNotUpdateLocalTransportAddress()
		{
			var forwarder = Factory.New<OrgHeader>();
			var cfs = Factory.New<OrgHeader>();

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.JK_OA_SendingForwarderAddress = forwarder.MainAddress.PK;
			Consol.JK_OA_DeparturePackCFSTransportAddress = cfs.MainAddress.PK;
			Consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;

			AssertEquals(cfs.MainAddress.PK, Consol.JK_OA_DeparturePackCFSTransportAddress);
		}

		public void TestSetSendingForwarderAddress_ForwarderDoesntHaveRelatedCFS_DoNotUpdateLocalTransportAddress()
		{
			var forwarder = Factory.New<OrgHeader>();
			var cfs = Factory.New<OrgHeader>();

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.JK_OA_DeparturePackCFSTransportAddress = cfs.MainAddress.PK;
			Consol.JK_OA_SendingForwarderAddress = forwarder.MainAddress.PK;

			AssertEquals(cfs.MainAddress.PK, Consol.JK_OA_DeparturePackCFSTransportAddress);
		}

		public void TestSingleCopyConsol_DoNotSetDefaultDepartureCFS()
		{
			SetUpExportSeaConsolForExportAirDepartment();

			AssertEquals("Pre-condition: Source SEA consol doesn't have CFS", ZGuid.Empty, Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK);

			var copiedConsol = (CommonConsol)((ITemplateCopyable)Consol).TemplateCopy();
			AssertEquals(Consol.JK_TransportMode, copiedConsol.JK_TransportMode);
			AssertEquals(Consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK, copiedConsol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK);
			AssertEquals(Consol.JK_RL_NKLoadPort, copiedConsol.JK_RL_NKLoadPort);
			AssertEquals(Consol.JK_RL_NKDischargePort, copiedConsol.JK_RL_NKDischargePort);
			AssertEquals("Copied consol doesn't have CFS", ZGuid.Empty, copiedConsol.JK_OA_PackDepotAddress_ZAddress.OrgPK);
		}

		public void TestUniversaleCopyConsol_DoNotSetDefaultDepartureCFS()
		{
			SetUpExportSeaConsolForExportAirDepartment();

			AssertEquals("Pre-condition: Source SEA consol doesn't have CFS", ZGuid.Empty, Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK);

			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobConsolSchema.Constants.JK_TransportMode, CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobConsolSchema.Constants.JK_RL_NKLoadPort, CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobConsolSchema.Constants.JK_RL_NKDischargePort, CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobConsolSchema.Constants.JK_OA_SendingForwarderAddress, CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobConsolSchema.Constants.JK_OA_PackDepotAddress, CopyMethod = CopyMethod.Copy });

			var copyTree = new CopyTemplateTree { InnerNode = entityNode };
			var copyManager = new BusinessObjectCopyManager();

			var copiedConsol = copyManager.Copy(Consol, copyTree).Object as CommonConsol;
			AssertEquals(Consol.JK_TransportMode, copiedConsol.JK_TransportMode);
			AssertEquals(Consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK, copiedConsol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK);
			AssertEquals(Consol.JK_RL_NKLoadPort, copiedConsol.JK_RL_NKLoadPort);
			AssertEquals(Consol.JK_RL_NKDischargePort, copiedConsol.JK_RL_NKDischargePort);
			AssertEquals("Copied consol doesn't have CFS", ZGuid.Empty, copiedConsol.JK_OA_PackDepotAddress_ZAddress.OrgPK);
		}

		[ExpectNoExceptions]
		public void TestUniversaleCopyConsol_DoNotCopyConsolID()
		{
			SetUpExportSeaConsolForExportAirDepartment();

			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobConsolSchema.Constants.JK_TransportMode, CopyMethod = CopyMethod.Copy });
			var copyTree = new CopyTemplateTree { InnerNode = entityNode };
			var copyManager = new BusinessObjectCopyManager();

			var copiedConsol = copyManager.Copy(Consol, copyTree).Object as CommonConsol;
			AssertEquals(Consol.JK_TransportMode, copiedConsol.JK_TransportMode);
			AssertEquals(ZString.Empty, copiedConsol.JK_UniqueConsignRef);

			Factory.Save();
			AssertNotEquals(ZString.Empty, copiedConsol.JK_UniqueConsignRef);
		}

		void SetUpExportSeaConsolForExportAirDepartment()
		{
			GlbDepartment.CurrentDepartment.GE_Air = true;
			GlbDepartment.CurrentDepartment.GE_Export = true;
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = forwarder.PK;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUMEL";
			var airCfs = Factory.NewWithValidTestData<OrgHeader>();
			forwarder.SetRelatedParty(airCfs, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Air, ZString.Empty);

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "NZAKL";
			Consol.JK_OA_SendingForwarderAddress = forwarder.MainAddress.PK;

			Factory.Save();
		}

		#endregion

		#region JK_OA_UnpackDepotAddress_ZAddressOrg

		public void TestDefaultJK_OA_UnpackDepotAddress_ZAddressOrg()
		{
			OrgHeader orgALL = Factory.New<OrgHeader>();
			OrgHeader orgAIR = Factory.New<OrgHeader>();
			OrgHeader orgSEA = Factory.New<OrgHeader>();
			OrgHeader orgFCL = Factory.New<OrgHeader>();
			OrgHeader orgLCL = Factory.New<OrgHeader>();
			OrgHeader orgMEL = Factory.New<OrgHeader>();
			OrgHeader orgAU = Factory.New<OrgHeader>();

			orgALL.OH_FullName = "orgALL";
			orgAIR.OH_FullName = "orgAIR";
			orgSEA.OH_FullName = "orgSEA";
			orgFCL.OH_FullName = "orgFCL";
			orgLCL.OH_FullName = "orgLCL";
			orgMEL.OH_FullName = "orgMEL";
			orgALL.OH_FullName = "orgALL";
			orgAU.OH_FullName = "orgAU";

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			Consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, Consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK);

			OrgHeader mainOrg = Factory.New<OrgHeader>();
			mainOrg.SetRelatedParty(orgAIR, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Air, ZString.Empty);
			Consol.JK_OA_ReceivingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(ZGuid.Empty, Consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK);

			mainOrg.SetRelatedParty(orgALL, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.All, ZString.Empty);
			Consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_ReceivingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgALL.PK, Consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK);

			mainOrg.SetRelatedParty(orgSEA, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, ZString.Empty);
			Consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_ReceivingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgSEA.PK, Consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK);

			mainOrg.SetRelatedParty(orgFCL, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			mainOrg.SetRelatedParty(orgLCL, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);

			Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			Consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_ReceivingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgFCL.PK, Consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK);

			Consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			Consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_ReceivingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgLCL.PK, Consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK);

			mainOrg.SetRelatedParty(orgMEL, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, "AUMEL");
			mainOrg.SetRelatedParty(orgAU, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, "AU");

			Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			Consol.JK_RL_NKDischargePort = "AUMEL";
			Consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_ReceivingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgMEL.PK, Consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK);

			Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			Consol.JK_RL_NKDischargePort = "AUSYD";
			Consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_ReceivingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgAU.PK, Consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK);

			Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			Consol.JK_RL_NKDischargePort = "USCHI";
			Consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_ReceivingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgFCL.PK, Consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK);
		}

		public void TestSetReceivingForwarderAddress_AddressIsEmpty_DoNotUpdateDepotAddress()
		{
			var forwarder = Factory.New<OrgHeader>();
			var cfs = Factory.New<OrgHeader>();

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.JK_OA_ReceivingForwarderAddress = forwarder.MainAddress.PK;
			Consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK = cfs.PK;
			Consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;

			AssertEquals(cfs.PK, Consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK);
		}

		public void TestSetReceivingForwarderAddress_ForwarderDoesntHaveRelatedCFS_DoNotUpdateDepotAddress()
		{
			var forwarder = Factory.New<OrgHeader>();
			var cfs = Factory.New<OrgHeader>();

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK = cfs.PK;
			Consol.JK_OA_ReceivingForwarderAddress = forwarder.MainAddress.PK;

			AssertEquals(cfs.PK, Consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK);
		}

		public void TestSetReceivingForwarderAddress_AddressIsEmpty_DoNotUpdateLocalTransportAddress()
		{
			var forwarder = Factory.New<OrgHeader>();
			var cfs = Factory.New<OrgHeader>();

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.JK_OA_ReceivingForwarderAddress = forwarder.MainAddress.PK;
			Consol.JK_OA_ArrivalUnpackCFSTransportAddress = cfs.MainAddress.PK;
			Consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;

			AssertEquals(cfs.MainAddress.PK, Consol.JK_OA_ArrivalUnpackCFSTransportAddress);
		}

		public void TestSetReceivingForwarderAddress_ForwarderDoesntHaveRelatedCFS_DoNotUpdateLocalTransportAddress()
		{
			var forwarder = Factory.New<OrgHeader>();
			var cfs = Factory.New<OrgHeader>();

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.JK_OA_ArrivalUnpackCFSTransportAddress = cfs.MainAddress.PK;
			Consol.JK_OA_ReceivingForwarderAddress = forwarder.MainAddress.PK;

			AssertEquals(cfs.MainAddress.PK, Consol.JK_OA_ArrivalUnpackCFSTransportAddress);
		}

		public void TestSingleCopyConsol_DoNotSetDefaultArrivalCFS()
		{
			SetUpImportSeaConsolForImportAirDepartment();

			AssertEquals("Pre-condition: Source SEA consol doesn't have CFS", ZGuid.Empty, Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK);

			var copiedConsol = (CommonConsol)((ITemplateCopyable)Consol).TemplateCopy();
			AssertEquals(Consol.JK_TransportMode, copiedConsol.JK_TransportMode);
			AssertEquals(Consol.JK_OA_ReceivingForwarderAddress_ZAddress.OrgPK, copiedConsol.JK_OA_ReceivingForwarderAddress_ZAddress.OrgPK);
			AssertEquals(Consol.JK_RL_NKLoadPort, copiedConsol.JK_RL_NKLoadPort);
			AssertEquals(Consol.JK_RL_NKDischargePort, copiedConsol.JK_RL_NKDischargePort);
			AssertEquals("Copied consol doesn't have CFS", ZGuid.Empty, copiedConsol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK);
		}

		public void TestUniversaleCopyConsol_DoNotSetDefaultArrivalCFS()
		{
			SetUpImportSeaConsolForImportAirDepartment();

			AssertEquals("Pre-condition: Source SEA consol doesn't have CFS", ZGuid.Empty, Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK);

			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobConsolSchema.Constants.JK_TransportMode, CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobConsolSchema.Constants.JK_RL_NKLoadPort, CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobConsolSchema.Constants.JK_RL_NKDischargePort, CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobConsolSchema.Constants.JK_OA_ReceivingForwarderAddress, CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobConsolSchema.Constants.JK_OA_UnpackDepotAddress, CopyMethod = CopyMethod.Copy });

			var copyTree = new CopyTemplateTree { InnerNode = entityNode };
			var copyManager = new BusinessObjectCopyManager();

			var copiedConsol = copyManager.Copy(Consol, copyTree).Object as CommonConsol;
			AssertEquals(Consol.JK_TransportMode, copiedConsol.JK_TransportMode);
			AssertEquals(Consol.JK_OA_ReceivingForwarderAddress_ZAddress.OrgPK, copiedConsol.JK_OA_ReceivingForwarderAddress_ZAddress.OrgPK);
			AssertEquals(Consol.JK_RL_NKLoadPort, copiedConsol.JK_RL_NKLoadPort);
			AssertEquals(Consol.JK_RL_NKDischargePort, copiedConsol.JK_RL_NKDischargePort);
			AssertEquals("Copied consol doesn't have CFS", ZGuid.Empty, copiedConsol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK);
		}

		void SetUpImportSeaConsolForImportAirDepartment()
		{
			GlbDepartment.CurrentDepartment.GE_Air = true;
			GlbDepartment.CurrentDepartment.GE_Import = true;
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = forwarder.PK;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUMEL";
			var airCfs = Factory.NewWithValidTestData<OrgHeader>();
			forwarder.SetRelatedParty(airCfs, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Air, ZString.Empty);

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_RL_NKLoadPort = "NZAKL";
			Consol.JK_RL_NKDischargePort = "AUSYD";
			Consol.JK_OA_ReceivingForwarderAddress = forwarder.MainAddress.PK;

			Factory.Save();
		}

		#endregion

		#region JK_OA_ArrivalCTOAddress

		public void TestJK_OA_ArrivalCTOAddress()
		{
			Consol.JK_OA_ArrivalCTOAddress_ZAddress.DefaultAddressType = AddressType.DLV;
			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress address1 = org.Addresses.AddNew();
			address1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);

			Consol.JK_RL_NKLoadPort = "";
			Consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Consol.JK_OA_ArrivalCTOAddress_ZAddress.OrgPK = org.PK;
			AssertEquals("JK_OA_ArrivalCTOAddress should be defaulted to PAD", address1.PK, Consol.JK_OA_ArrivalCTOAddress);

			OrgAddress address2 = org.Addresses.AddNew();
			address2.AddressCapability.SetCapabilityEnabled(nameof(AddressType.DLV));

			address1.AddressCapability.SetCapabilityDisabled(OrgConstants.AddressType.PickupAndDelivery);
			Consol.JK_OA_ArrivalCTOAddress_ZAddress.OrgPK = ZGuid.Empty;
			Consol.JK_OA_ArrivalCTOAddress_ZAddress.OrgPK = org.PK;
			AssertEquals("JK_OA_ArrivalCTOAddress should be defaulted to DLV", address2.PK, Consol.JK_OA_ArrivalCTOAddress);
		}

		#endregion

		#region JK_OA_DeparturePackCFSTransportAddress

		public void TestDefaultJK_OA_DeparturePackCFSTransportAddress()
		{
			OrgHeader orgALL = Factory.New<OrgHeader>();
			OrgHeader orgAIR = Factory.New<OrgHeader>();
			OrgHeader orgSEA = Factory.New<OrgHeader>();
			OrgHeader orgFCL = Factory.New<OrgHeader>();
			OrgHeader orgLCL = Factory.New<OrgHeader>();

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			Consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, Consol.JK_OA_DeparturePackCFSTransportAddress);

			OrgHeader mainOrg = Factory.New<OrgHeader>();
			mainOrg.SetRelatedParty(orgAIR, RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Air, ZString.Empty);
			Consol.JK_OA_SendingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(ZGuid.Empty, Consol.JK_OA_DeparturePackCFSTransportAddress);

			mainOrg.SetRelatedParty(orgALL, RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.All, ZString.Empty);
			Consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_SendingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgALL.MainAddress.PK, Consol.JK_OA_DeparturePackCFSTransportAddress);

			mainOrg.SetRelatedParty(orgSEA, RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, ZString.Empty);
			Consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_SendingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgSEA.MainAddress.PK, Consol.JK_OA_DeparturePackCFSTransportAddress);

			mainOrg.SetRelatedParty(orgFCL, RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			mainOrg.SetRelatedParty(orgLCL, RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			Consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_SendingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgSEA.MainAddress.PK, Consol.JK_OA_DeparturePackCFSTransportAddress);

			Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			Consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_SendingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgFCL.MainAddress.PK, Consol.JK_OA_DeparturePackCFSTransportAddress);

			Consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			Consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_SendingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgLCL.MainAddress.PK, Consol.JK_OA_DeparturePackCFSTransportAddress);
		}

		#endregion

		#region JK_OA_ArrivalUnpackCFSTransportAddress

		public void TestDefaultJK_OA_ArrivalUnpackCFSTransportAddress()
		{
			OrgHeader orgALL = Factory.New<OrgHeader>();
			OrgHeader orgAIR = Factory.New<OrgHeader>();
			OrgHeader orgSEA = Factory.New<OrgHeader>();
			OrgHeader orgFCL = Factory.New<OrgHeader>();
			OrgHeader orgLCL = Factory.New<OrgHeader>();

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			Consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, Consol.JK_OA_ArrivalUnpackCFSTransportAddress);

			OrgHeader mainOrg = Factory.New<OrgHeader>();
			mainOrg.SetRelatedParty(orgAIR, RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Air, ZString.Empty);
			Consol.JK_OA_ReceivingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(ZGuid.Empty, Consol.JK_OA_ArrivalUnpackCFSTransportAddress);

			mainOrg.SetRelatedParty(orgALL, RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.All, ZString.Empty);
			Consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_ReceivingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgALL.MainAddress.PK, Consol.JK_OA_ArrivalUnpackCFSTransportAddress);

			mainOrg.SetRelatedParty(orgSEA, RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, ZString.Empty);
			Consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_ReceivingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgSEA.MainAddress.PK, Consol.JK_OA_ArrivalUnpackCFSTransportAddress);

			mainOrg.SetRelatedParty(orgFCL, RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			mainOrg.SetRelatedParty(orgLCL, RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			Consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_ReceivingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgSEA.MainAddress.PK, Consol.JK_OA_ArrivalUnpackCFSTransportAddress);

			Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			Consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_ReceivingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgFCL.MainAddress.PK, Consol.JK_OA_ArrivalUnpackCFSTransportAddress);

			Consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			Consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			Consol.JK_OA_ReceivingForwarderAddress = mainOrg.MainAddress.PK;
			AssertEquals(orgLCL.MainAddress.PK, Consol.JK_OA_ArrivalUnpackCFSTransportAddress);
		}

		#endregion

		#region Load/Discharge Ports

		public void TestSetDefaultLoadPort_EmptyInNullObjectFactory()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			GlbDepartment.CurrentDepartment.GE_Export = true;

			var consol = Factory.New<CommonConsol>();
			AssertEquals("AUSYD", consol.JK_RL_NKLoadPort);

			consol = new BusinessObjectFactory().GetNull<CommonConsol>();
			AssertEquals("", consol.JK_RL_NKLoadPort);
		}

		public void TestSetDefaultDischargePort_EmptyInNullObjectFactory()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			GlbDepartment.CurrentDepartment.GE_Import = true;

			var consol = Factory.New<CommonConsol>();
			AssertEquals("AUSYD", consol.JK_RL_NKDischargePort);

			consol = new BusinessObjectFactory().GetNull<CommonConsol>();
			AssertEquals("", consol.JK_RL_NKDischargePort);
		}

		#endregion

		#endregion

		#region Test Property Overrides

		#region JK_RL_NKLastForeignPort

		public void TestJK_RL_NKLastForeignPort()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Assert("JK_RL_NKLastForeignPort should not be Readonly", !Consol.JK_RL_NKLastForeignPortInfo.ReadOnly);
			Assert("JK_DateLastForeignPort should not be Readonly", !Consol.JK_DateLastForeignPortInfo.ReadOnly);
			Consol.JK_RL_NKLastForeignPort = AUSYDLoco;
			AssertEquals("JK_RL_NKLastForeignPort should be 'AUSYD'", AUSYDLoco, Consol.JK_RL_NKLastForeignPort);

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals("JK_RL_NKLastForeignPort should be empty", "", Consol.JK_RL_NKLastForeignPort);
			Assert("JK_RL_NKLastForeignPort - No error expected", !Consol.JK_RL_NKLastForeignPortInfo.HasErrors());
			Assert("JK_RL_NKLastForeignPort should be Readonly", Consol.JK_RL_NKLastForeignPortInfo.ReadOnly);
			Assert("JK_DateLastForeignPort should be Readonly", Consol.JK_DateLastForeignPortInfo.ReadOnly);
		}

		#endregion

		#region JK_IsCFS

		public void TestJK_IsCFS()
		{
			CommonConsol consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_OA_PackDepotAddress = ZGuid.Empty;
			consol1.JK_OA_UnpackDepotAddress = ZGuid.Empty;

			Transport transport1 = consol1.Transports[0];
			transport1.JW_JX = new SailingsForTestClasses(Factory).SydLaxSailing.PK;

			CommonShipment shipment = consol1.Shipments.AddNew();
			CommonContainer container = consol1.Containers.AddNew();

			Assert("Not expecting Consol to be CFS Registered.", !consol1.JK_IsCFS);
			Assert("Not expecting CommonShipment to be CFS Registered.", !shipment.JS_IsCFSRegistered);
			Assert("Not expecting Container to be CFS Registered.", !container.JC_IsCFSRegistered);

			consol1.JK_IsCFS = true;

			Assert("Expecting Consol to be CFS Registered.", consol1.JK_IsCFS);
			Assert("Expecting CommonShipment to be CFS Registered.", shipment.JS_IsCFSRegistered);
			Assert("Expecting Container to be CFS Registered.", container.JC_IsCFSRegistered);
		}

		public void TestNoExceptionsWhenJK_IsCFSIsChanged()
		{
			CommonConsol consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_IsCFS = true;
			consol1.JK_IsForwarding = true;
			CommonShipment shipment = consol1.Shipments.AddNew();

			Assert("Expecting Consol to be CFS Registered.", consol1.JK_IsCFS);
			Assert("Expecting CommonShipment to be CFS Registered.", shipment.JS_IsCFSRegistered);

			Factory.Save();
			CommonConsol consol2 = Factory.New<CommonConsol>();
			consol2.Shipments.Add(shipment);
			consol2.JK_IsCFS = false;
			consol2.JK_IsForwarding = true;
			Assert("Not expecting Consol to be CFS Registered.", !consol2.JK_IsCFS);

			AssertNoExceptionThrown("Expect not throw exception when factory is saved", Factory.Save);
			Assert("Expecting CommonShipment to be CFS Registered.", shipment.JS_IsCFSRegistered);
			Assert("Expecting Consol to be CFS Registered.", consol1.JK_IsCFS);
		}

		public void TestIsCFSSetter_SetTranshipToOtherCFS_Import()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var importShipment = consol.Shipments.AddNew();
			importShipment.JS_RL_NKOrigin = "NZAKL";
			importShipment.JS_RL_NKDestination = "AUSYD";
			importShipment.JS_ShipmentType = "CLD";

			var importShipment1 = consol.Shipments.AddNew();
			importShipment1.JS_RL_NKOrigin = "NZAKL";
			importShipment1.JS_RL_NKDestination = "AUSYD";
			importShipment1.JS_ShipmentType = "STD";

			var exportShipment = consol.Shipments.AddNew();
			exportShipment.JS_RL_NKOrigin = "AUSYD";
			exportShipment.JS_RL_NKDestination = "NZAKL";
			exportShipment.JS_ShipmentType = "STD";

			consol.JK_IsCFS = true;

			Factory.Save();

			AssertEquals("Expecting CommonShipment to be CFS Registered.", true, importShipment.JS_IsCFSRegistered);
			AssertEquals("Expecting GatePass required to be false for CFS Shipment.", false, importShipment.JS_TranshipToOtherCFS);

			AssertEquals("Expecting CommonShipment to be CFS Registered.", true, importShipment1.JS_IsCFSRegistered);
			AssertEquals("Expecting GatePass required to be true for CFS Shipmentr.", true, importShipment1.JS_TranshipToOtherCFS);

			AssertEquals("Expecting CommonShipment to be CFS Registered.", true, exportShipment.JS_IsCFSRegistered);
			AssertEquals("Expecting GatePass required to be false for Export CFS Shipment.", false, exportShipment.JS_TranshipToOtherCFS);

			var consol1 = Factory.NewWithValidTestData<CommonConsol>();
			var shipment = consol1.Shipments.AddNew();

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			Factory.Save();

			AssertEquals("Expecting CommonShipment not to be CFS Registered.", false, shipment.JS_IsCFSRegistered);
			AssertEquals("Expecting GatePass required to be false for CFS Shipment.", false, shipment.JS_TranshipToOtherCFS);
		}

		public void TestConsolShouldNotResetJK_IsCFS()
		{
			var originalValue = ErrorReporter.SuppressReportingOfErrors;

			try
			{
				ErrorReporter.SuppressReportingOfErrors = false;

				var commonConsol = Factory.NewWithValidTestData<CommonConsol>();
				commonConsol.JK_IsCFS = true;

				Factory.Save();

				commonConsol.JK_IsCFS = false;

				Assert("Once a consol has been marked as CFS registered and saved it should not be unmarked", commonConsol.JK_IsCFS);

				ErrorReporter.Clear();
			}
			finally
			{
				ErrorReporter.SuppressReportingOfErrors = originalValue;
			}
		}

		#endregion

		#region TestJK_AgentType

		public void TestJK_AgentType()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "HKHKG";

			CommonShipment s = consol.Shipments.AddNew();
			s.JS_RL_NKOrigin = "AUSYD";
			s.JS_RL_NKDestination = "HKHKG";

			consol.JK_AgentType = Constants.AgentType.Direct;

			AssertEquals("Consol load should not change.", "AUBNE", consol.JK_JX_JA_RL_NKPortOfLoading);
		}

		public void TestWhenAgentTypeChangedConsolModeValidated()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = Constants.AgentType.Other;

			Assert("Consol Mode validated when Agent Type changed", consol.JK_ConsolModeInfo.HasErrors());
		}

		#endregion

		#endregion

		#region Calculated Properties

		#region TestHasETDPassed

		public void TestHasETDPassed()
		{
			Transport transport = Consol.Transports[0];
			transport.JW_ETD = ZDateTime.Empty;
			AssertEquals("no date", ZBool.False, Consol.HasETDPassed);

			transport.JW_ETD = ZDateTime.Today.AddDays(2);
			AssertEquals("HasETDPassed with tomorrows date", ZBool.False, Consol.HasETDPassed);

			transport.JW_ETD = ZDateTime.Today.AddDays(-1);
			AssertEquals("HasETDPassed with yesterdays date", ZBool.True, Consol.HasETDPassed);
		}

		#endregion

		#region JK_TotalShipmentWeight

		public void TestJK_TotalShipmentWeight()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			AssertTotalWeights(consol, 0m, 0m, 0m);

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 100m;
			shipment1.JS_DocumentedWeight = 101m;
			shipment1.JS_ManifestedWeight = 110m;

			AssertTotalWeights(consol, 100m, 101m, 110m);

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualWeight = 200m;
			shipment2.JS_DocumentedWeight = 202m;
			shipment2.JS_ManifestedWeight = 220m;

			AssertTotalWeights(consol, 300m, 303m, 330m);

			CommonShipment shipment3 = consol.Shipments.AddNew();
			shipment3.JS_ActualWeight = 3m;
			shipment3.JS_DocumentedWeight = 3.003m;
			shipment3.JS_ManifestedWeight = 3.03m;
			shipment3.JS_UnitOfWeight = Constants.Weight.Tonnes;

			AssertTotalWeights(consol, 3300m, 3306m, 3360m);

			consol.Shipments.Remove(shipment3);
			AssertTotalWeights(consol, 300m, 303m, 330m);

			consol.Shipments.RemoveAll();
			AssertTotalWeights(consol, 0m, 0m, 0m);
		}

		void AssertTotalWeights(CommonConsol consol, decimal actual, decimal documented, decimal manifested)
		{
			CombineAssertions(delegate
			{
				AssertEquals(actual, consol.JK_TotalShipmentWeight);
				AssertEquals(documented, consol.JK_TotalDocumentedWeight);
				AssertEquals(manifested, consol.JK_TotalManifestedWeight);
			});
		}

		public void TestJK_TotalShipmentWeightInfo()
		{
			AssertEquals("Correct name", "JK_TotalShipmentWeight", Consol.JK_TotalShipmentWeightInfo.Name);
		}

		public void TestJK_TotalShipmentWeightUnit()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			AssertEquals("Precondition", 0, consol.Shipments.Count);
			AssertEquals("Kilograms by default", Constants.Weight.Kilograms, consol.JK_TotalShipmentWeightUnit);

			consol.Shipments.AddNew().JS_UnitOfWeight = Constants.Weight.Pounds;
			consol.Shipments.AddNew().JS_UnitOfWeight = Constants.Weight.Ounces;
			AssertEquals("Pounds only when all shipments has imperial weight", Constants.Weight.Pounds, consol.JK_TotalShipmentWeightUnit);

			consol.Shipments[0].JS_UnitOfWeight = Constants.Weight.Grams;
			AssertEquals("Kilograms in all other cases", Constants.Weight.Kilograms, consol.JK_TotalShipmentWeightUnit);
		}

		#endregion

		#region Test Chargeable Weight

		public void TestJS_ActualChargeable_UsesRegistrySettings()
		{
			FreightDataRegistry.Instance.DomesticChargeableFactorAir.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(6250m, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms),
				new ConversionFactor(166m, Constants.Volume.CubicInches, Constants.Weight.Pounds)));

			FreightDataRegistry.Instance.DomesticChargeableFactorRoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(6060m, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms),
				new ConversionFactor(170m, Constants.Volume.CubicInches, Constants.Weight.Pounds)));

			FreightDataRegistry.Instance.DomesticChargeableFactorCourier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(5882m, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms),
				new ConversionFactor(175m, Constants.Volume.CubicInches, Constants.Weight.Pounds)));

			FreightDataRegistry.Instance.DomesticChargeableFactorSea.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(1000m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres),
				new ConversionFactor(100m, Constants.Volume.CubicInches, Constants.Weight.Pounds)));

			FreightDataRegistry.Instance.DomesticChargeableFactorRail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(1010m, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms),
				new ConversionFactor(110m, Constants.Volume.CubicInches, Constants.Weight.Pounds)));

			CommonShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = AlternateHomePort;
			AssertEquals("CommonShipment should be domestic", true, shipment.IsDomesticFreight);
			Consol.JK_RL_NKLoadPort = HomePort;
			Consol.JK_RL_NKDischargePort = AlternateHomePort;
			AssertEquals("Consol should be domestic", true, Consol.IsDomesticFreight);

			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_ActualWeight = 800;
			shipment.JS_ActualVolume = 5;

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Chargeable (Air)", 800M, Consol.JK_ConsolChargeable, 2);
			AssertEquals("Chargeable Unit (Air)", "KG", Consol.JK_ConsolChargeableUnit);

			Consol.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Chargeable (Road)", 825.08M, Consol.JK_ConsolChargeable, 2);
			AssertEquals("Chargeable Unit (Road)", "KG", Consol.JK_ConsolChargeableUnit);

			Consol.JK_TransportMode = Constants.TransportModes.Courier;
			AssertEquals("Chargeable (Courier)", 850.05M, Consol.JK_ConsolChargeable, 2);
			AssertEquals("Chargeable Unit (Courier)", "KG", Consol.JK_ConsolChargeableUnit);

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Chargeable (Sea)", 5M, Consol.JK_ConsolChargeable, 2);
			AssertEquals("Chargeable Unit (Sea)", "M3", Consol.JK_ConsolChargeableUnit);

			Consol.JK_TransportMode = Constants.TransportModes.Rail;
			AssertEquals("Chargeable (Rail)", 5M, Consol.JK_ConsolChargeable, 2);
			AssertEquals("Chargeable Unit (Rail)", "M3", Consol.JK_ConsolChargeableUnit);

			shipment.JS_ActualWeight = Constants.Weight.Convert(shipment.JS_ActualWeight, shipment.JS_UnitOfWeight, Constants.Weight.Pounds);
			shipment.JS_ActualVolume = Constants.Volume.Convert(shipment.JS_ActualVolume, shipment.JS_UnitOfVolume, Constants.Volume.CubicFeet);

			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicFeet;

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Chargeable (Air)", 1838.061M, Utilities.Round(Consol.JK_ConsolChargeable, 3));
			AssertEquals("Chargeable Unit (Air)", "LB", Consol.JK_ConsolChargeableUnit);

			Consol.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Chargeable (Road)", 1794.813M, Utilities.Round(Consol.JK_ConsolChargeable, 3));
			AssertEquals("Chargeable Unit (Road)", "LB", Consol.JK_ConsolChargeableUnit);

			Consol.JK_TransportMode = Constants.TransportModes.Courier;
			AssertEquals("Chargeable (Courier)", 1763.698M, Utilities.Round(Consol.JK_ConsolChargeable, 3));
			AssertEquals("Chargeable Unit (Courier)", "LB", Consol.JK_ConsolChargeableUnit);

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Chargeable (Sea)", 176.573M, Utilities.Round(Consol.JK_ConsolChargeable, 3));
			AssertEquals("Chargeable Unit (Sea)", "CF", Consol.JK_ConsolChargeableUnit);

			Consol.JK_TransportMode = Constants.TransportModes.Rail;
			AssertEquals("Chargeable (Rail)", 176.573M, Utilities.Round(Consol.JK_ConsolChargeable, 3));
			AssertEquals("Chargeable Unit (Rail)", "CF", Consol.JK_ConsolChargeableUnit);
		}

		#endregion

		#region JK_TotalShipmentChargeable

		public void TestJK_TotalShipmentChargeableForAir()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			AssertEquals("Precondition: consol's chargeable unit is weight", Constants.Weight.Kilograms, consol.JK_ConsolChargeableUnit);
			AssertEquals("Total chargeable", 0m, consol.JK_TotalShipmentChargeable);

			consol.Shipments.AddNew().JS_ActualChargeable = 100m;
			AssertEquals("Total chargeable", 100m, consol.JK_TotalShipmentChargeable);

			consol.Shipments.AddNew().JS_ActualChargeable = 200m;
			AssertEquals("Total chargeable", 300m, consol.JK_TotalShipmentChargeable);

			CommonShipment imperialShipment = consol.Shipments.AddNew();
			imperialShipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			imperialShipment.JS_UnitOfVolume = Constants.Volume.CubicFeet;
			imperialShipment.JS_ActualChargeable = 1000m;
			AssertEquals("Precondition: shipment's chargeable unit should be pounds", Constants.Weight.Pounds, imperialShipment.JS_ChargeableUnit);

			AssertEquals("Total chargeable, weight converted to consol weight unit", 753.592m, consol.JK_TotalShipmentChargeable);

			consol.Shipments.Remove(consol.Shipments.First());
			AssertEquals("Total chargeable", 653.592m, consol.JK_TotalShipmentChargeable);

			consol.Shipments.RemoveAll();
			AssertEquals("Total chargeable", 0m, consol.JK_TotalShipmentChargeable);
		}

		public void TestJK_TotalShipmentChargeableForDomesticAir_SeaAirShipments()
		{
			using (FreightDataRegistry.Instance.DomesticChargeableFactorAir.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(5000m, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms),
				new ConversionFactor(500m, Constants.Volume.CubicInches, Constants.Weight.Pounds))))
			{
				var consol = Factory.New<CommonConsol>();
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_TransportMode = Constants.TransportModes.Air;
				Assert("Precondition: consol is domestic", consol.IsDomestic());
				AssertEquals("Precondition: consol's chargeable unit is weight", Constants.Weight.Kilograms, consol.JK_ConsolChargeableUnit);
				AssertEquals("Total chargeable", 0m, consol.JK_TotalShipmentChargeable);

				consol.Shipments.AddNew().JS_ActualChargeable = 100m;
				AssertEquals("Total chargeable", 100m, consol.JK_TotalShipmentChargeable);

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_TransportMode = Constants.TransportModes.SeaAir;
				shipment1.JS_UnitOfWeight = Constants.Weight.Kilograms;
				shipment1.JS_UnitOfVolume = Constants.Volume.CubicMetres;
				shipment1.JS_ActualChargeable = 1m;
				AssertEquals("Precondition: shipment's chargeable unit should be cubicmetres", Constants.Volume.CubicMetres, shipment1.JS_ChargeableUnit);
				AssertEquals("Total chargeable, cubicmetres converted to kilograms", 300m, consol.JK_TotalShipmentChargeable);

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_TransportMode = Constants.TransportModes.SeaAir;
				shipment2.JS_UnitOfWeight = Constants.Weight.Pounds;
				shipment2.JS_UnitOfVolume = Constants.Volume.CubicFeet;
				shipment2.JS_ActualChargeable = 100m;
				AssertEquals("Precondition: shipment's chargeable unit should be cubicfeet", Constants.Volume.CubicFeet, shipment2.JS_ChargeableUnit);
				AssertEquals("Total chargeable, cubicfeet converted to kilograms", 866.337m, consol.JK_TotalShipmentChargeable);

				consol.Shipments.Remove(consol.Shipments.First());
				AssertEquals("Total chargeable", 766.337m, consol.JK_TotalShipmentChargeable);

				consol.Shipments.RemoveAll();
				AssertEquals("Total chargeable", 0m, consol.JK_TotalShipmentChargeable);
			}
		}

		public void TestJK_TotalShipmentChargeableForInternationalAir_SeaAirShipments()
		{
			using (FreightDataRegistry.Instance.InternationalChargeableFactorAir.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(
				new ConversionFactor(5000m, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms),
				new ConversionFactor(500m, Constants.Volume.CubicInches, Constants.Weight.Pounds))))
			{
				var consol = Factory.New<CommonConsol>();
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "NZAKL";
				consol.JK_TransportMode = Constants.TransportModes.Air;
				Assert("Precondition: consol is international", !consol.IsDomestic());
				AssertEquals("Precondition: consol's chargeable unit is weight", Constants.Weight.Kilograms, consol.JK_ConsolChargeableUnit);
				AssertEquals("Total chargeable", 0m, consol.JK_TotalShipmentChargeable);

				consol.Shipments.AddNew().JS_ActualChargeable = 100m;
				AssertEquals("Total chargeable", 100m, consol.JK_TotalShipmentChargeable);

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_TransportMode = Constants.TransportModes.SeaAir;
				shipment1.JS_UnitOfWeight = Constants.Weight.Kilograms;
				shipment1.JS_UnitOfVolume = Constants.Volume.CubicMetres;
				shipment1.JS_ActualChargeable = 1m;
				AssertEquals("Precondition: shipment's chargeable unit should be cubicmetres", Constants.Volume.CubicMetres, shipment1.JS_ChargeableUnit);
				AssertEquals("Total chargeable, cubicmetres converted to kilograms", 300m, consol.JK_TotalShipmentChargeable);

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_TransportMode = Constants.TransportModes.SeaAir;
				shipment2.JS_UnitOfWeight = Constants.Weight.Pounds;
				shipment2.JS_UnitOfVolume = Constants.Volume.CubicFeet;
				shipment2.JS_ActualChargeable = 100m;
				AssertEquals("Precondition: shipment's chargeable unit should be cubicfeet", Constants.Volume.CubicFeet, shipment2.JS_ChargeableUnit);
				AssertEquals("Total chargeable, cubicfeet converted to kilograms", 866.337m, consol.JK_TotalShipmentChargeable);

				consol.Shipments.Remove(consol.Shipments.First());
				AssertEquals("Total chargeable", 766.337m, consol.JK_TotalShipmentChargeable);

				consol.Shipments.RemoveAll();
				AssertEquals("Total chargeable", 0m, consol.JK_TotalShipmentChargeable);
			}
		}

		public void TestJK_TotalShipmentChargeableForSea()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			AssertEquals("Precondition: consol's chargeable unit is volume", Constants.Volume.CubicMetres, consol.JK_ConsolChargeableUnit);
			AssertEquals("Total chargeable", 0m, consol.JK_TotalShipmentChargeable);

			consol.Shipments.AddNew().JS_ActualChargeable = 10m;
			AssertEquals("Total chargeable", 10m, consol.JK_TotalShipmentChargeable);

			consol.Shipments.AddNew().JS_ActualChargeable = 20m;
			AssertEquals("Total chargeable", 30m, consol.JK_TotalShipmentChargeable);

			CommonShipment imperialShipment = consol.Shipments.AddNew();
			imperialShipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			imperialShipment.JS_UnitOfVolume = Constants.Volume.CubicFeet;
			imperialShipment.JS_ActualChargeable = 100m;
			AssertEquals("Precondition: shipment's chargeable unit should be cubic feet", Constants.Volume.CubicFeet, imperialShipment.JS_ChargeableUnit);

			AssertEquals("Total chargeable, weight converted to consol weight unit", 32.832m, consol.JK_TotalShipmentChargeable);

			consol.Shipments.Remove(consol.Shipments.First());
			AssertEquals("Total chargeable", 22.832m, consol.JK_TotalShipmentChargeable);

			consol.Shipments.RemoveAll();
			AssertEquals("Total chargeable", 0m, consol.JK_TotalShipmentChargeable);
		}

		public void TestJK_TotalShipmentChargeableForDifferentUnits()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.JK_RL_NKLoadPort = "USLAX";
			Consol.JK_RL_NKDischargePort = "AUSYD";

			var shipmentDomestic = Factory.New<CommonShipment>();
			shipmentDomestic.JS_TransportMode = Constants.TransportModes.Air;
			shipmentDomestic.JS_RL_NKOrigin = "USLAX";
			shipmentDomestic.JS_RL_NKDestination = "USMEM";
			shipmentDomestic.JS_UnitOfWeight = "LB";
			shipmentDomestic.JS_UnitOfVolume = "CF";
			shipmentDomestic.JS_ActualWeight = 2500m;
			shipmentDomestic.JS_ActualVolume = 50m;
			AssertEquals("Precondition - Shipment Chargeable", 2500m, shipmentDomestic.JS_ActualChargeable);
			AssertEquals("Precondition - Shipment Chargeable", "LB", shipmentDomestic.JS_ChargeableUnit);

			var shipmentInternational = Factory.New<CommonShipment>();
			shipmentInternational.JS_TransportMode = Constants.TransportModes.Air;
			shipmentInternational.JS_RL_NKOrigin = "USLAX";
			shipmentInternational.JS_RL_NKDestination = "AUSYD";
			shipmentInternational.JS_UnitOfWeight = "LB";
			shipmentInternational.JS_UnitOfVolume = "CF";
			shipmentInternational.JS_ActualWeight = 1000m;
			shipmentInternational.JS_ActualVolume = 20m;
			AssertEquals("Precondition - Shipment Chargeable", 1000m, shipmentInternational.JS_ActualChargeable);
			AssertEquals("Precondition - Shipment Chargeable", "LB", shipmentInternational.JS_ChargeableUnit);

			Consol.Shipments.Add(shipmentDomestic);
			Consol.Shipments.Add(shipmentInternational);

			AssertEquals("Consol Shipment Total Chargeable", 3500m, Consol.JK_TotalShipmentChargeable);
			AssertEquals("Consol Shipment Total Chargeable", "LB", Consol.JK_ConsolChargeableUnit);

			var shipmentMetric = Factory.New<CommonShipment>();
			shipmentMetric.JS_TransportMode = Constants.TransportModes.Air;
			shipmentMetric.JS_RL_NKOrigin = "USLAX";
			shipmentMetric.JS_RL_NKDestination = "AUSYD";
			shipmentMetric.JS_UnitOfWeight = "KG";
			shipmentMetric.JS_UnitOfVolume = "CF";
			shipmentMetric.JS_ActualWeight = 1000m;
			shipmentMetric.JS_ActualVolume = 2m;
			AssertEquals("Precondition - Shipment Chargeable", 1000m, shipmentMetric.JS_ActualChargeable);
			AssertEquals("Precondition - Shipment Chargeable", "KG", shipmentMetric.JS_ChargeableUnit);

			Consol.Shipments.Add(shipmentMetric);

			AssertEquals("Mixed shipment units; weight", "KG", Consol.JK_TotalShipmentWeightUnit);
			AssertEquals("Mixed shipment units; volume", "CF", Consol.JK_TotalShipmentVolumeUnit);
			AssertEquals("Consol Shipment Total Chargeable", 2587.573m, Utilities.Round(Consol.JK_TotalShipmentChargeable, 3));
			AssertEquals("Consol Shipment Total Chargeable", "KG", Consol.JK_ConsolChargeableUnit);

			shipmentMetric.JS_UnitOfWeight = "LB";
			shipmentMetric.JS_ActualWeight = 2205m;

			AssertEquals("Consol Shipment Total Chargeable", 5705m, Consol.JK_TotalShipmentChargeable);
			AssertEquals("Consol Shipment Total Chargeable", "LB", Consol.JK_ConsolChargeableUnit);
		}

		public void TestTotalShipmentChargeables()
		{
			CommonShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_ActualVolume = 10m;
			shipment.JS_ManifestedVolume = 9m;
			shipment.JS_DocumentedVolume = 8m;

			shipment = Consol.Shipments.AddNew();
			shipment.JS_ActualVolume = 20m;
			shipment.JS_ManifestedVolume = 18m;
			shipment.JS_DocumentedVolume = 16m;

			AssertEquals(30m, Consol.JK_TotalShipmentChargeable);
			AssertEquals(27m, Consol.JK_TotalManifestedChargeable);
			AssertEquals(24m, Consol.JK_TotalDocumentedChargeable);
		}

		#endregion

		#region JK_Calc_FreeSpace and JK_Calc_ActualVolumeWeight

		public void TestJK_Calc_FreeSpace_JK_Calc_ActualVolumeWeight_ChargeableByWeight()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			AssertEquals("Precondition: consol's chargeable unit is weight", Constants.Weight.Kilograms, consol.JK_ConsolChargeableUnit);
			AssertEquals("Total chargeable", 0m, consol.JK_TotalShipmentChargeable);

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 300;
			shipment1.JS_ActualVolume = 0.308;
			shipment1.JS_ActualChargeable = 300;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualWeight = 5.8;
			shipment2.JS_ActualVolume = 0.028;
			shipment2.JS_ActualChargeable = 6;

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_ActualWeight = 66;
			shipment3.JS_ActualVolume = 0.384;
			shipment3.JS_ActualChargeable = 66;

			var shipment4 = consol.Shipments.AddNew();
			shipment4.JS_ActualWeight = 1072;
			shipment4.JS_ActualVolume = 6.912;
			shipment4.JS_ActualChargeable = 1152;

			var shipment5 = consol.Shipments.AddNew();
			shipment5.JS_ActualWeight = 93.9;
			shipment5.JS_ActualVolume = 0.682;
			shipment5.JS_ActualChargeable = 114;

			var shipment6 = consol.Shipments.AddNew();
			shipment6.JS_ActualWeight = 292.7;
			shipment6.JS_ActualVolume = 2.539;
			shipment6.JS_ActualChargeable = 423.5;

			var shipment7 = consol.Shipments.AddNew();
			shipment7.JS_ActualWeight = 39.6;
			shipment7.JS_ActualVolume = 0.372;
			shipment7.JS_ActualChargeable = 62;

			AssertEquals("Free Space should be updated", 252.667m, consol.JK_Calc_FreeSpace);
			AssertEquals("Weight Volume should be correct", 1870.833333m, consol.JK_Calc_ActualVolumeWeight);
			AssertEquals("Unit should be correct", consol.JK_CorrectedConsolWeightUnit, consol.JK_Calc_ActualVolumeWeightUnit);

			AssertEquals(51.333333m, shipment1.JS_Calc_ActualVolumeWeight);
			AssertEquals(4.666667m, shipment2.JS_Calc_ActualVolumeWeight);
			AssertEquals(64m, shipment3.JS_Calc_ActualVolumeWeight);
			AssertEquals(1152m, shipment4.JS_Calc_ActualVolumeWeight);
			AssertEquals(113.666667m, shipment5.JS_Calc_ActualVolumeWeight);
			AssertEquals(423.166667m, shipment6.JS_Calc_ActualVolumeWeight);
			AssertEquals(62m, shipment7.JS_Calc_ActualVolumeWeight);
		}

		public void TestJK_Calc_FreeSpace_JK_Calc_ActualVolumeWeight_ChargeableByVolume()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			AssertEquals("Precondition: consol's chargeable unit is volume", Constants.Volume.CubicMetres, consol.JK_ConsolChargeableUnit);
			AssertEquals("Total chargeable", 0m, consol.JK_TotalShipmentChargeable);

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 300;
			shipment1.JS_ActualVolume = 0.308;
			shipment1.JS_ActualChargeable = 300;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualWeight = 5.8;
			shipment2.JS_ActualVolume = 0.028;
			shipment2.JS_ActualChargeable = 6;

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_ActualWeight = 66;
			shipment3.JS_ActualVolume = 0.384;
			shipment3.JS_ActualChargeable = 66;

			var shipment4 = consol.Shipments.AddNew();
			shipment4.JS_ActualWeight = 1072;
			shipment4.JS_ActualVolume = 6.912;
			shipment4.JS_ActualChargeable = 1152;

			var shipment5 = consol.Shipments.AddNew();
			shipment5.JS_ActualWeight = 93.9;
			shipment5.JS_ActualVolume = 0.682;
			shipment5.JS_ActualChargeable = 114;

			var shipment6 = consol.Shipments.AddNew();
			shipment6.JS_ActualWeight = 292.7;
			shipment6.JS_ActualVolume = 2.539;
			shipment6.JS_ActualChargeable = 423.5;

			var shipment7 = consol.Shipments.AddNew();
			shipment7.JS_ActualWeight = 39.6;
			shipment7.JS_ActualVolume = 0.372;
			shipment7.JS_ActualChargeable = 62;

			AssertEquals("Free Space should be updated", 2112.275m, consol.JK_Calc_FreeSpace);
			AssertEquals("Weight Volume should be correct", 1.87m, consol.JK_Calc_ActualVolumeWeight);
			AssertEquals("Unit should be correct", consol.JK_CorrectedConsolVolumeUnit, consol.JK_Calc_ActualVolumeWeightUnit);
		}

		public void TestJK_Calc_ActualVolumeWeight_NoExceptionThrown()
		{
			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_OverrideConsolChargeable = true;
			consol1.JK_CorrectedConsolWeightUnit = "KG";
			consol1.JK_CorrectedConsolVolumeUnit = "";
			AssertEquals(0m, consol1.JK_Calc_ActualVolumeWeight);
			consol1.JK_OverrideConsolChargeable = false;
			consol1.JK_CorrectedConsolWeightUnit = "KG";
			consol1.JK_CorrectedConsolVolumeUnit = "";
			AssertEquals(0m, consol1.JK_Calc_ActualVolumeWeight);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_OverrideConsolChargeable = true;
			consol2.JK_CorrectedConsolWeightUnit = "";
			consol2.JK_CorrectedConsolVolumeUnit = "M3";
			AssertEquals(0m, consol2.JK_Calc_ActualVolumeWeight);
			consol2.JK_OverrideConsolChargeable = false;
			consol2.JK_CorrectedConsolWeightUnit = "";
			consol2.JK_CorrectedConsolVolumeUnit = "M3";
			AssertEquals(0m, consol2.JK_Calc_ActualVolumeWeight);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_OverrideConsolChargeable = true;
			consol3.JK_CorrectedConsolWeightUnit = "";
			consol3.JK_CorrectedConsolVolumeUnit = "";
			AssertEquals(0m, consol3.JK_Calc_ActualVolumeWeight);
			consol3.JK_OverrideConsolChargeable = false;
			consol3.JK_CorrectedConsolWeightUnit = "";
			consol3.JK_CorrectedConsolVolumeUnit = "";
			AssertEquals(0m, consol3.JK_Calc_ActualVolumeWeight);

			var consol4 = Factory.New<CommonConsol>();
			consol4.JK_TransportMode = Constants.TransportModes.Sea;
			consol4.JK_OverrideConsolChargeable = true;
			consol4.JK_CorrectedConsolWeight = 10;
			consol4.JK_CorrectedConsolVolume = 10;
			consol4.JK_CorrectedConsolWeightUnit = "KG";
			consol4.JK_CorrectedConsolVolumeUnit = "ZX";
			AssertEquals(0m, consol4.JK_Calc_ActualVolumeWeight);
			consol4.JK_OverrideConsolChargeable = false;
			consol4.JK_CorrectedConsolWeight = 10;
			consol4.JK_CorrectedConsolVolume = 10;
			consol4.JK_CorrectedConsolWeightUnit = "KG";
			consol4.JK_CorrectedConsolVolumeUnit = "ZX";
			AssertEquals(0m, consol4.JK_Calc_ActualVolumeWeight);

			var consol5 = Factory.New<CommonConsol>();
			consol5.JK_TransportMode = Constants.TransportModes.Sea;
			consol5.JK_OverrideConsolChargeable = true;
			consol5.JK_CorrectedConsolWeight = 10;
			consol5.JK_CorrectedConsolVolume = 10;
			consol5.JK_CorrectedConsolWeightUnit = "ZX";
			consol5.JK_CorrectedConsolVolumeUnit = "M3";
			AssertEquals(0m, consol5.JK_Calc_ActualVolumeWeight);
			consol5.JK_OverrideConsolChargeable = false;
			consol5.JK_CorrectedConsolWeight = 10;
			consol5.JK_CorrectedConsolVolume = 10;
			consol5.JK_CorrectedConsolWeightUnit = "ZX";
			consol5.JK_CorrectedConsolVolumeUnit = "M3";
			AssertEquals(0m, consol5.JK_Calc_ActualVolumeWeight);
		}

		#endregion

		#region JK_OverrideConsolChargeable

		public void TestJK_OverrideConsolChargeable_AffectsReadOnly()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			AssertEquals("Default value", false, consol.JK_OverrideConsolChargeable);
			AssertEquals("Property is readonly", true, consol.JK_ConsolChargeableInfo.ReadOnly);
			AssertEquals("Property is readonly", true, consol.JK_CorrectedConsolVolumeInfo.ReadOnly);
			AssertEquals("Property is readonly", true, consol.JK_CorrectedConsolWeightInfo.ReadOnly);
			AssertEquals("Property is readonly", true, consol.JK_CorrectedConsolVolumeUnitInfo.ReadOnly);
			AssertEquals("Property is readonly", true, consol.JK_CorrectedConsolWeightUnitInfo.ReadOnly);

			consol.JK_OverrideConsolChargeable = true;
			AssertEquals("Property is not readonly", false, consol.JK_ConsolChargeableInfo.ReadOnly);
			AssertEquals("Property is not readonly", false, consol.JK_CorrectedConsolVolumeInfo.ReadOnly);
			AssertEquals("Property is not readonly", false, consol.JK_CorrectedConsolWeightInfo.ReadOnly);
			AssertEquals("Property is not readonly", false, consol.JK_CorrectedConsolVolumeUnitInfo.ReadOnly);
			AssertEquals("Property is not readonly", false, consol.JK_CorrectedConsolWeightUnitInfo.ReadOnly);
		}

		public void TestJK_OverrideConsolChargeable_UpdatesCorrectedMeasuresWithDefaultsOnChanging()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			CommonShipment shipment1 = consol.TopLevelShipments.AddNew();
			shipment1.JS_ActualVolume = 1m;
			shipment1.JS_ActualWeight = 100m;
			CommonShipment shipment2 = consol.TopLevelShipments.AddNew();
			shipment2.JS_ActualVolume = 2m;
			shipment2.JS_ActualWeight = 200m;

			AssertCorrectedMeasures("Precondition", consol, 3m, 300m, 3m);

			consol.JK_OverrideConsolChargeable = true;
			AssertCorrectedMeasures("Default values", consol, 3m, 300m, 3m);

			consol.JK_CorrectedConsolVolume = 16m;
			consol.JK_CorrectedConsolWeight = 32m;
			consol.JK_ConsolChargeable = 64m;
			AssertCorrectedMeasures("Corrected values", consol, 64m, 32m, 16m);

			consol.JK_OverrideConsolChargeable = false;
			AssertCorrectedMeasures("Values back to defaults", consol, 3m, 300m, 3m);
		}

		public void TestSetOverrideConsolChargeable()
		{
			var consol1 = Factory.New<CommonConsolForBaseTest>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;

			var shipment = consol1.Shipments.AddNew();
			shipment.JS_ActualVolume = 0m;
			shipment.JS_ActualWeight = 100m;

			consol1.JK_OverrideConsolChargeable = true;
			consol1.JK_CorrectedConsolVolume = 16m;
			consol1.JK_CorrectedConsolWeight = 32m;
			Factory.Save();
			AssertEquals(16m, consol1.JK_CorrectedConsolVolume);
			AssertEquals(32m, consol1.JK_CorrectedConsolWeight);

			consol1.JK_OverrideConsolChargeable = false;
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var consol2 = factory.Load<CommonConsolForBaseTest>(consol1.PK);
			AssertEquals(0m, consol2.BaseJK_CorrectedConsolVolume);
			AssertEquals(0m, consol2.BaseJK_CorrectedConsolWeight);
		}

		#endregion

		#region JK_Calc_TotalShipmentChargeableUnit

		public void TestJK_Calc_TotalShipmentChargeableUnit()
		{
			var shipment = Consol.Shipments.AddNew();

			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Total Shipment Chargeable Unit (Air)", "KG", Consol.JK_Calc_TotalShipmentChargeableUnit);

			Consol.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Total Shipment Chargeable Unit (Road)", "KG", Consol.JK_Calc_TotalShipmentChargeableUnit);

			Consol.JK_TransportMode = Constants.TransportModes.Courier;
			AssertEquals("Total Shipment Chargeable Unit (Courier)", "KG", Consol.JK_Calc_TotalShipmentChargeableUnit);

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Total Shipment Chargeable Unit (Sea)", "M3", Consol.JK_Calc_TotalShipmentChargeableUnit);

			Consol.JK_TransportMode = Constants.TransportModes.Rail;
			AssertEquals("Total Shipment Chargeable Unit (Rail)", "M3", Consol.JK_Calc_TotalShipmentChargeableUnit);

			shipment.JS_ActualWeight = Constants.Weight.Convert(shipment.JS_ActualWeight, shipment.JS_UnitOfWeight, Constants.Weight.Pounds);
			shipment.JS_ActualVolume = Constants.Volume.Convert(shipment.JS_ActualVolume, shipment.JS_UnitOfVolume, Constants.Volume.CubicFeet);

			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicFeet;

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Total Shipment Chargeable Unit (Air)", "LB", Consol.JK_Calc_TotalShipmentChargeableUnit);

			Consol.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Total Shipment Chargeable Unit (Road)", "LB", Consol.JK_Calc_TotalShipmentChargeableUnit);

			Consol.JK_TransportMode = Constants.TransportModes.Courier;
			AssertEquals("Total Shipment Chargeable Unit (Courier)", "LB", Consol.JK_Calc_TotalShipmentChargeableUnit);

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Total Shipment Chargeable Unit (Sea)", "CF", Consol.JK_Calc_TotalShipmentChargeableUnit);

			Consol.JK_TransportMode = Constants.TransportModes.Rail;
			AssertEquals("Total Shipment Chargeable Unit (Rail)", "CF", Consol.JK_Calc_TotalShipmentChargeableUnit);
		}

		#endregion

		#region JK_ConsolChargeable

		public void TestJK_ConsolChargeableForAir()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Air;

			CommonShipment shipment1 = CreateShipment(Constants.TransportModes.Air, 200m, 1m);  //Equivalent weight of volume is 166.667
			CommonShipment shipment2 = CreateShipment(Constants.TransportModes.Air, 500m, 2m);  //Equivalent weight of volume is 333.334
			CommonShipment shipment3 = CreateShipment(Constants.TransportModes.Air, 300m, 3m);  //Equivalent weight of volume is 500.00

			Consol.Shipments.Add(shipment1);
			AssertEquals("Total CommonShipment chargeable 200", 200m, Consol.JK_ConsolChargeable);

			Consol.Shipments.Add(shipment2);
			AssertEquals("Total CommonShipment chargeable 700", 700m, Consol.JK_ConsolChargeable);

			Consol.Shipments.Add(shipment3);
			AssertEquals("Despite CommonShipment chargeable using weight converted to volume consol will still use just weight, total is 1000", 1000m, Consol.JK_ConsolChargeable);

			CommonShipment newShipmentChargeableisWeight = AddShipment(Consol, 200m, 1m);
			AssertEquals("Addnew using weight should have worked, total is 1200", 1200m, Consol.JK_ConsolChargeable);

			CommonShipment newShipmentChargeableisVolume = AddShipment(Consol, 300m, 2m);
			AssertEquals("Addnew should work suing total weight not converted volume, total is 1500", 1500m, Consol.JK_ConsolChargeable);

			Consol.Shipments.Remove(shipment2);
			AssertEquals("Removing CommonShipment where chargeable is actual works, total is 1166.667", 1166.667m, Consol.JK_ConsolChargeable);

			Consol.Shipments.Remove(shipment3);
			AssertEquals("Removing CommonShipment where chargeable is volume works, total is 700", 700m, Consol.JK_ConsolChargeable);

			Consol.Shipments.Remove(shipment1);
			Consol.Shipments.Remove(newShipmentChargeableisWeight);
			Consol.Shipments.Remove(newShipmentChargeableisVolume);
			AssertEquals("Removing everything works total is 0", 0m, Consol.JK_ConsolChargeable);

			shipment1 = CreateShipment(Constants.TransportModes.Air, 200m, 1m); //Equivalent weight of volume is 166.667
			shipment2 = CreateShipment(Constants.TransportModes.Air, 500m, 1m); //Equivalent weight of volume is 333.334

			Consol.Shipments.Add(shipment1);
			AssertEquals("Total CommonShipment chargeable 200", 200m, Consol.JK_ConsolChargeable);

			Consol.JK_OverrideConsolChargeable = true;
			AssertEquals("Total CommonShipment chargeable 200", 200m, Consol.JK_ConsolChargeable);

			Consol.JK_ConsolChargeable = 100m;
			AssertEquals("Total CommonShipment chargeable 100", 100m, Consol.JK_ConsolChargeable);

			Consol.Shipments.Add(shipment2);
			AssertEquals("Total CommonShipment chargeable should be 100m", 100m, Consol.JK_ConsolChargeable);

			Consol.JK_OverrideConsolChargeable = false;
			AssertEquals("Total CommonShipment chargeable 700", 700m, Consol.JK_ConsolChargeable);

			Consol.Shipments.Remove(shipment2);
			Consol.Shipments.Remove(shipment1);
			AssertEquals("Removing everything works total is 0", 0m, Consol.JK_ConsolChargeable);
		}

		public void TestJK_ConsolChargeableForSea()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Sea;

			CommonShipment shipmentChargeableisVolume1 = CreateShipment(Constants.TransportModes.Sea, 200m, 1m); //Equivalent Volume of weight  is 0.2
			CommonShipment shipmentChargeableisVolume2 = CreateShipment(Constants.TransportModes.Sea, 500m, 2m);    //Equivalent Volume of weight is 0.5
			CommonShipment shipmentChargeableisWeight = CreateShipment(Constants.TransportModes.Sea, 5000m, 3m);    //Equivalent Volume of weight is 5

			Consol.TopLevelShipments.Add(shipmentChargeableisVolume1);
			AssertEquals("Total CommonShipment chargeable 1", 1m, Consol.JK_ConsolChargeable);

			Consol.TopLevelShipments.Add(shipmentChargeableisVolume2);
			AssertEquals("Total CommonShipment chargeable 3", 3m, Consol.JK_ConsolChargeable);

			Consol.TopLevelShipments.Add(shipmentChargeableisWeight);
			AssertEquals("Despite CommonShipment chargeable using weight consol total will use volume , total is 6", 6m, Consol.JK_ConsolChargeable);

			CommonShipment newShipmentChargeableisVolume = AddShipment(Consol, 200m, 1m);
			AssertEquals("Addnew using weight should have worked, total is 7", 7m, Consol.JK_ConsolChargeable);

			CommonShipment newShipmentChargeableisWeight = AddShipment(Consol, 4000m, 3m);
			AssertEquals("Addnew using converted volume should have worked, total is 10", 10m, Consol.JK_ConsolChargeable);

			Consol.TopLevelShipments.Remove(shipmentChargeableisVolume2);
			AssertEquals("Removing CommonShipment reduces total weight to 9400 and total volume to 8, chargeable is 9.4", 9.4m, Consol.JK_ConsolChargeable);

			Consol.TopLevelShipments.Remove(shipmentChargeableisWeight);
			AssertEquals("Removing CommonShipment where chargeable is volume works, total is 5", 5m, Consol.JK_ConsolChargeable);

			Consol.TopLevelShipments.Remove(shipmentChargeableisVolume1);
			Consol.TopLevelShipments.Remove(newShipmentChargeableisVolume);
			Consol.TopLevelShipments.Remove(newShipmentChargeableisWeight);
			AssertEquals("Removing everything works total is 0", 0m, Consol.JK_ConsolChargeable);

			shipmentChargeableisVolume1 = CreateShipment(Constants.TransportModes.Sea, 200m, 1m); //Equivalent Volume of weight  is 0.2
			shipmentChargeableisVolume2 = CreateShipment(Constants.TransportModes.Sea, 500m, 2m);   //Equivalent Volume of weight is 0.5

			Consol.TopLevelShipments.Add(shipmentChargeableisVolume1);
			AssertEquals("Total CommonShipment chargeable 1", 1m, Consol.JK_ConsolChargeable);

			Consol.JK_OverrideConsolChargeable = true;
			AssertEquals("Total CommonShipment chargeable 1", 1m, Consol.JK_ConsolChargeable);

			Consol.JK_ConsolChargeable = 2m;
			AssertEquals("Total CommonShipment chargeable 2", 2m, Consol.JK_ConsolChargeable);

			Consol.TopLevelShipments.Add(shipmentChargeableisVolume2);
			AssertEquals("Total CommonShipment chargeable should be 2", 2m, Consol.JK_ConsolChargeable);

			Consol.JK_OverrideConsolChargeable = false;
			AssertEquals("Total CommonShipment chargeable 3", 3m, Consol.JK_ConsolChargeable);

			Consol.TopLevelShipments.Remove(shipmentChargeableisVolume2);
			Consol.TopLevelShipments.Remove(shipmentChargeableisVolume1);
			AssertEquals("Removing everything works total is 0", 0m, Consol.JK_ConsolChargeable);
		}

		public void TestJK_ConsolChargeable_UpdatesCorrectedMeasures()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			CommonShipment shipment1 = consol.TopLevelShipments.AddNew();
			shipment1.JS_ActualVolume = 1m;
			shipment1.JS_ActualWeight = 100m;
			CommonShipment shipment2 = consol.TopLevelShipments.AddNew();
			shipment2.JS_ActualVolume = 2m;
			shipment2.JS_ActualWeight = 200m;

			// Consol chargeable is using volume units

			consol.JK_OverrideConsolChargeable = false;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Precondition", Constants.Volume.CubicMetres, consol.JK_ConsolChargeableUnit);
			AssertCorrectedMeasures("Precondition", consol, 3m, 300m, 3m);

			consol.JK_OverrideConsolChargeable = true;
			consol.JK_ConsolChargeable = 4m;
			AssertCorrectedMeasures("Chargeable changed, but both weight and volume were non-empty => no updates", consol, 4m, 300m, 3m);

			consol.JK_CorrectedConsolWeight = 0m;
			consol.JK_ConsolChargeable = 5m;
			AssertCorrectedMeasures("Chargeable changed => weight was updated from empty, volume not updated", consol, 5m, 5000m, 3m);

			consol.JK_CorrectedConsolVolume = 0m;
			consol.JK_CorrectedConsolWeight = 300m;
			consol.JK_ConsolChargeable = 6m;
			AssertCorrectedMeasures("Chargeable changed => weight not updated as it wasn't empty, volume not updated", consol, 6m, 300m, 0m);

			// Consol chargeable is using weight units

			consol.JK_OverrideConsolChargeable = false;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Precondition", Constants.Weight.Kilograms, consol.JK_ConsolChargeableUnit);
			AssertCorrectedMeasures("Precondition", consol, 500m, 300m, 3m);

			consol.JK_OverrideConsolChargeable = true;
			consol.JK_ConsolChargeable = 600m;
			AssertCorrectedMeasures("Chargeable changed, but both weight and volume were non-empty => no updates", consol, 600m, 300m, 3m);

			consol.JK_CorrectedConsolWeight = 0m;
			consol.JK_ConsolChargeable = 700m;
			AssertCorrectedMeasures("Chargeable changed => volume not updated as it wasn't empty, weight not updated", consol, 700m, 0m, 3m);

			consol.JK_CorrectedConsolVolume = 0m;
			consol.JK_CorrectedConsolWeight = 300m;
			consol.JK_ConsolChargeable = 800m;
			AssertCorrectedMeasures("Chargeable changed => volume was updated from empty, weight not updated", consol, 800m, 300m, 4.8m);
		}

		void AssertCorrectedMeasures(ZString message, CommonConsol consol, ZDecimal consolChargeable, ZDecimal correctedWeight, ZDecimal correctedVolume)
		{
			string measuresFormat = "Chargeable:{0} Weight:{1} Volume:{2}";
			string expectedValues = "Expected values were: " + string.Format(measuresFormat, consolChargeable, correctedWeight, correctedVolume);
			string actualValues = "But actual values were: " + string.Format(measuresFormat, consol.JK_ConsolChargeable, consol.JK_CorrectedConsolWeight, consol.JK_CorrectedConsolVolume);
			message = string.Join(System.Environment.NewLine, new string[] { message, expectedValues, actualValues });

			bool allMeasuresAsExpected = consol.JK_ConsolChargeable == consolChargeable
				&& consol.JK_CorrectedConsolWeight == correctedWeight
				&& consol.JK_CorrectedConsolVolume == correctedVolume;

			AssertEquals(message, true, allMeasuresAsExpected);
		}

		public void TestJK_ConsolChargeable_DefaultNumberOfDecimalsRegistry()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_Volume = collection.AddNew();
			defaultNumberOfDecimals_Volume.UnitOfMeasure = Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_Volume.TransportMode = Constants.TransportModes.Sea;
			defaultNumberOfDecimals_Volume.NumberOfDecimals = 1;
			defaultNumberOfDecimals_Volume.RoundingMode = RoundingModes.Up;
			var defaultNumberOfDecimals_Weight = collection.AddNew();
			defaultNumberOfDecimals_Weight.UnitOfMeasure = Constants.Weight.Kilograms;
			defaultNumberOfDecimals_Weight.TransportMode = Constants.TransportModes.Sea;
			defaultNumberOfDecimals_Weight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_Weight.RoundingMode = RoundingModes.Down;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_OverrideConsolChargeable = false;

			var shipment1 = CreateShipment(Constants.TransportModes.Sea, 200.112m, 3.528m); // 3.6m

			Consol.Shipments.Add(shipment1);
			AssertEquals("M3", Consol.JK_ConsolChargeableUnit);
			AssertEquals(3.6m, Consol.JK_ConsolChargeable);

			Consol.JK_OverrideConsolChargeable = true;
			AssertEquals(3.6m, Consol.JK_ConsolChargeable);

			Consol.JK_ConsolChargeable = 100.111m;
			AssertEquals(100.2m, Consol.JK_ConsolChargeable);
		}

		public void TestJK_ConsolChargeable_FreightChargeableWeightRoundingsRegistry()
		{
			var collection = new ChargeableWeightRoundingCollection();
			var freightChargeableWeightRounding = collection.AddNew();
			freightChargeableWeightRounding.RoundingScale = ChargeableWeightRoundingScales.DefaultScale;
			freightChargeableWeightRounding.RoundingMode = nameof(ChargeableWeightRoundingType.Up);

			FreightDataRegistry.Instance.FreightChargeableWeightRoundings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.JK_OverrideConsolChargeable = false;

			var shipment1 = CreateShipment(Constants.TransportModes.Air, 200.112m, 3.528m); // 588m

			Consol.Shipments.Add(shipment1);
			AssertEquals("KG", Consol.JK_ConsolChargeableUnit);
			AssertEquals(588m, Consol.JK_ConsolChargeable);

			Consol.JK_OverrideConsolChargeable = true;
			AssertEquals(588m, Consol.JK_ConsolChargeable);

			Consol.JK_ConsolChargeable = 100.111m;
			AssertEquals(100.5m, Consol.JK_ConsolChargeable);
		}

		#endregion

		#region JK_CorrectedConsolVolume

		public void TestJK_CorrectedConsolVolume()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			CommonShipment shipment1 = consol.TopLevelShipments.AddNew();
			shipment1.JS_ActualVolume = 8m;
			CommonShipment shipment2 = consol.TopLevelShipments.AddNew();
			shipment2.JS_ActualVolume = 16m;

			AssertEquals(false, consol.JK_OverrideConsolChargeable);
			AssertEquals(24m, consol.JK_TotalShipmentVolume);
			AssertEquals("Default value equals JK_TotalShipmentVolume", 24m, consol.JK_CorrectedConsolVolume);

			consol.JK_OverrideConsolChargeable = true;
			AssertEquals("Default value equals JK_TotalShipmentVolume", 24m, consol.JK_CorrectedConsolVolume);

			consol.JK_CorrectedConsolVolume = 32m;
			AssertEquals(32m, consol.JK_CorrectedConsolVolume);

			consol.JK_OverrideConsolChargeable = false;
			AssertEquals("Default value equals JK_TotalShipmentVolume", 24m, consol.JK_CorrectedConsolVolume);
		}

		public void TestJK_CorrectedConsolVolume_UpdatesConsolChargeable()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			CommonShipment shipment1 = consol.TopLevelShipments.AddNew();
			shipment1.JS_ActualVolume = 1m;
			shipment1.JS_ActualWeight = 100m;
			CommonShipment shipment2 = consol.TopLevelShipments.AddNew();
			shipment2.JS_ActualVolume = 2m;
			shipment2.JS_ActualWeight = 200m;

			consol.JK_OverrideConsolChargeable = false;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Precondition", Constants.Volume.CubicMetres, consol.JK_ConsolChargeableUnit);
			AssertCorrectedMeasures("Precondition", consol, 3m, 300m, 3m);

			consol.JK_OverrideConsolChargeable = true;
			consol.JK_CorrectedConsolVolume = 4m;
			AssertCorrectedMeasures("Volume changed => chargeable updated, weight not updated", consol, 4m, 300m, 4m);

			consol.JK_CorrectedConsolVolume = 1.6m;
			AssertCorrectedMeasures("Volume changed => chargeable updated, weight not updated", consol, 1.6m, 300m, 1.6m);

			consol.JK_CorrectedConsolVolume = 0.2;
			AssertCorrectedMeasures("Volume changed => chargeable updated = MAX(chargeable weight, chargeable volume), weight not updated", consol, 0.3m, 300m, 0.2m);
		}

		public void TestJK_CorrectedConsolVolumeUnit()
		{
			var consol = Factory.New<CommonConsol>();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualVolume = 100m;
			shipment1.JS_UnitOfVolume = "M3";

			AssertEquals(false, consol.JK_OverrideConsolChargeable);
			AssertEquals("Default value equals JK_TotalShipmentVOlume", 100m, consol.JK_CorrectedConsolVolume);
			AssertEquals("Default value equals JK_TotalShipmentVolumeUnit", "M3", consol.JK_CorrectedConsolVolumeUnit);

			consol.JK_OverrideConsolChargeable = true;
			consol.JK_CorrectedConsolVolume = 200m;
			consol.JK_CorrectedConsolVolumeUnit = "F3";
			AssertEquals(200m, consol.JK_CorrectedConsolVolume);
			AssertEquals("F3", consol.JK_CorrectedConsolVolumeUnit);

			consol.JK_OverrideConsolChargeable = false;
			AssertEquals("Default value equals JK_TotalShipmenVolume", 100m, consol.JK_CorrectedConsolVolume);
			AssertEquals("Default value equals JK_TotalShipmentVolumeUnit", "M3", consol.JK_CorrectedConsolVolumeUnit);
		}

		public void TestJK_CorrectedConsolVolumeUnit_UpdatesConsolChargeable()
		{
			var consol = Factory.New<CommonConsol>();

			var shipment1 = consol.TopLevelShipments.AddNew();
			shipment1.JS_ActualVolume = 100m;
			shipment1.JS_ActualWeight = 200m;
			shipment1.JS_UnitOfVolume = Constants.Volume.Litre;

			var shipment2 = consol.TopLevelShipments.AddNew();
			shipment2.JS_ActualVolume = 200m;
			shipment2.JS_ActualWeight = 200m;
			shipment2.JS_UnitOfVolume = Constants.Volume.Litre;

			consol.JK_OverrideConsolChargeable = false;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Precondition", Constants.Volume.CubicMetres, consol.JK_ConsolChargeableUnit);
			AssertCorrectedMeasures("Precondition", consol, 0.400m, 400m, 0.300m);

			consol.JK_OverrideConsolChargeable = true;
			consol.JK_CorrectedConsolVolumeUnit = Constants.Volume.MegaLitre;
			AssertCorrectedMeasures("Volume Unit changed => chargeable updated, weight and volume not updated", consol, 300m, 400m, 0.300m);
		}

		#endregion

		#region JK_CorrectedConsolWeight

		public void TestJK_CorrectedConsolWeight()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			CommonShipment shipment1 = consol.TopLevelShipments.AddNew();
			shipment1.JS_ActualWeight = 32m;
			CommonShipment shipment2 = consol.TopLevelShipments.AddNew();
			shipment2.JS_ActualWeight = 64m;

			AssertEquals(false, consol.JK_OverrideConsolChargeable);
			AssertEquals(96m, consol.JK_TotalShipmentWeight);
			AssertEquals("Default value equals JK_TotalShipmentWeight", 96m, consol.JK_CorrectedConsolWeight);

			consol.JK_OverrideConsolChargeable = true;
			AssertEquals("Default value equals JK_TotalShipmentWeight", 96m, consol.JK_CorrectedConsolWeight);

			consol.JK_CorrectedConsolWeight = 128m;
			AssertEquals(128m, consol.JK_CorrectedConsolWeight);

			consol.JK_OverrideConsolChargeable = false;
			AssertEquals("Default value equals JK_TotalShipmentWeight", 96m, consol.JK_CorrectedConsolWeight);
		}

		public void TestJK_CorrectedConsolWeight_UpdatesConsolChargeable()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			CommonShipment shipment1 = consol.TopLevelShipments.AddNew();
			shipment1.JS_ActualVolume = 1m;
			shipment1.JS_ActualWeight = 100m;
			CommonShipment shipment2 = consol.TopLevelShipments.AddNew();
			shipment2.JS_ActualVolume = 2m;
			shipment2.JS_ActualWeight = 200m;

			consol.JK_OverrideConsolChargeable = false;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Precondition", Constants.Volume.CubicMetres, consol.JK_ConsolChargeableUnit);
			AssertCorrectedMeasures("Precondition", consol, 3m, 300m, 3m);

			consol.JK_OverrideConsolChargeable = true;
			consol.JK_CorrectedConsolWeight = 4000m;
			AssertCorrectedMeasures("Weight changed => chargeable updated, volume not updated", consol, 4m, 4000m, 3m);

			consol.JK_CorrectedConsolWeight = 3200m;
			AssertCorrectedMeasures("Weight changed => chargeable updated, volume not updated", consol, 3.2m, 3200m, 3m);

			consol.JK_CorrectedConsolWeight = 2800m;
			AssertCorrectedMeasures("Weight changed => chargeable updated = MAX(chargeable weight, chargeable volume), volume not updated", consol, 3m, 2800m, 3m);
		}

		public void TestJK_CorrectedConsolWeightUnit()
		{
			var consol = Factory.New<CommonConsol>();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 500000m;
			shipment1.JS_UnitOfWeight = "KG";

			AssertEquals(false, consol.JK_OverrideConsolChargeable);
			AssertEquals("Default value equals JK_TotalShipmentWeight", 500000m, consol.JK_CorrectedConsolWeight);
			AssertEquals("Default value equals JK_TotalShipmentWeightUnit", "KG", consol.JK_CorrectedConsolWeightUnit);

			consol.JK_OverrideConsolChargeable = true;
			consol.JK_CorrectedConsolWeight = 100000m;
			consol.JK_CorrectedConsolWeightUnit = "T";
			AssertEquals("The value equals JK_TotalShipmentWeight", 100000m, consol.JK_CorrectedConsolWeight);
			AssertEquals("The value equals JK_TotalShipmentWeightUnit", "T", consol.JK_CorrectedConsolWeightUnit);

			consol.JK_OverrideConsolChargeable = false;
			AssertEquals("Default value equals JK_TotalShipmentWeight", 500000m, consol.JK_CorrectedConsolWeight);
			AssertEquals("Default value equals JK_TotalShipmentWeightUnit", "KG", consol.JK_CorrectedConsolWeightUnit);
		}

		public void TestJK_CorrectedConsolWeightUnit_UpdatesConsolChargeable()
		{
			var consol = Factory.New<CommonConsol>();

			var shipment1 = consol.TopLevelShipments.AddNew();
			shipment1.JS_ActualVolume = 1m;
			shipment1.JS_ActualWeight = 200m;
			shipment1.JS_UnitOfWeight = Constants.Weight.Grams;

			var shipment2 = consol.TopLevelShipments.AddNew();
			shipment2.JS_ActualVolume = 2m;
			shipment2.JS_ActualWeight = 200m;
			shipment2.JS_UnitOfWeight = Constants.Weight.Grams;

			consol.JK_OverrideConsolChargeable = false;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Precondition", Constants.Weight.Kilograms, consol.JK_ConsolChargeableUnit);
			AssertCorrectedMeasures("Precondition", consol, 500m, 0.400m, 3m);

			consol.JK_OverrideConsolChargeable = true;
			consol.JK_CorrectedConsolWeightUnit = Constants.Weight.Kilotonnes;
			AssertCorrectedMeasures("Weight Unit changed => chargeable updated, weight and volume not updated", consol, 400000, 0.400m, 3m);
		}

		#endregion

		#region JK_CorrectedConsol* Saving

		public void TestJK_CorrectedConsol_Saving()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			CommonShipment shipment1 = consol.TopLevelShipments.AddNew();
			shipment1.JS_ActualVolume = 1m;
			shipment1.JS_ActualWeight = 100m;
			CommonShipment shipment2 = consol.TopLevelShipments.AddNew();
			shipment2.JS_ActualVolume = 2m;
			shipment2.JS_ActualWeight = 200m;

			AssertEquals("Precondition", false, consol.JK_OverrideConsolChargeable);
			AssertCorrectedMeasures("Precondition", consol, 3m, 300m, 3m);

			Factory.Save();
			AssertCorrectedMeasuresInDatabase("Zero when JK_OverrideConsolChargeable is FALSE", consol.PK, 0m, 0m, 0m);

			consol.JK_OverrideConsolChargeable = true;
			consol.JK_CorrectedConsolVolume = 16m;
			consol.JK_CorrectedConsolWeight = 32m;
			consol.JK_ConsolChargeable = 64m;
			AssertCorrectedMeasures("Corrected values", consol, 64m, 32m, 16m);

			Factory.Save();
			AssertCorrectedMeasuresInDatabase("Persistant value when JK_OverrideConsolChargeable is TRUE", consol.PK, 64m, 32m, 16m);

			consol.JK_OverrideConsolChargeable = false;
			Factory.Save();
			AssertCorrectedMeasuresInDatabase("Zero when JK_OverrideConsolChargeable is FALSE", consol.PK, 0m, 0m, 0m);
		}

		void AssertCorrectedMeasuresInDatabase(string message, ZGuid consolPK, ZDecimal consolChargeable, ZDecimal correctedWeight, ZDecimal correctedVolume)
		{
			string query = string.Format(@"
				SELECT JK_ConsolChargeable, JK_CorrectedConsolVolume, JK_CorrectedConsolWeight
				FROM dbo.JobConsol
				WHERE JK_PK = '{0}'", consolPK.ToString());

			using (IDataReader reader = Db.Connection.Command(query).ExecuteReader()) // Need to check DB value as it can be overriden in business layer by calculations
			{
				int datarowsRead = 0;
				while (reader.Read())
				{
					AssertEquals(message + "; Chargeable", consolChargeable, (decimal)reader[AutoJobConsol.Schema.JK_ConsolChargeable]);
					AssertEquals(message + "; Corrected weight", correctedWeight, (decimal)reader[AutoJobConsol.Schema.JK_CorrectedConsolWeight]);
					AssertEquals(message + "; Corrected volume", correctedVolume, (decimal)reader[AutoJobConsol.Schema.JK_CorrectedConsolVolume]);
					datarowsRead++;
				}
				AssertEquals("One data row expected", 1, datarowsRead);
			}
		}

		#endregion

		#region JK_TotalPrepaidShipmentChargeableAmount

		public void TestJK_TotalPrepaidShipmentChargeableAmountForAir()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Air;

			CommonShipment collectShipments = CreateShipment(Constants.TransportModes.Air, 500m, 2m, 1.538m);
			CommonShipment prepaidShipmentChargeableisWeight1 = CreateShipment(Constants.TransportModes.Air, 200m, 1m, 1.538m, false);
			CommonShipment prepaidShipmentChargeableisVolume = CreateShipment(Constants.TransportModes.Air, 200m, 3m, 1.538m, false);
			AssertEquals("Total prepaid chargeable amount defaults to 0", Consol.JK_TotalPrepaidShipmentChargeableAmount, 0m);

			Consol.TopLevelShipments.Add(prepaidShipmentChargeableisWeight1);
			ZDecimal expectedTotal = prepaidShipmentChargeableisWeight1.ChargeableAmount;
			AssertEquals("Total is CommonShipment chargeable amount", Consol.JK_TotalPrepaidShipmentChargeableAmount, expectedTotal);

			Consol.TopLevelShipments.Add(collectShipments);
			AssertEquals("A collect CommonShipment shouldn't affect prepaid CommonShipment chargeable amount", Consol.JK_TotalPrepaidShipmentChargeableAmount, expectedTotal);

			Consol.TopLevelShipments.Add(prepaidShipmentChargeableisVolume);
			expectedTotal += prepaidShipmentChargeableisVolume.ChargeableAmount;
			AssertEquals("CommonShipment chargeable will be converted volume, sum will include this", Consol.JK_TotalPrepaidShipmentChargeableAmount, expectedTotal);

			CommonShipment newCollectShipment = AddShipment(Consol, 200m, 1m, 1.538m);
			AssertEquals("Collect CommonShipment created from consol shouldn't affect total", Consol.JK_TotalPrepaidShipmentChargeableAmount, expectedTotal);

			CommonShipment newPrepaidShipment = AddShipment(Consol, 200m, 1m, 1.538m, false);
			expectedTotal += newPrepaidShipment.ChargeableAmount;
			AssertEquals("Prepaid created from consol should increase prepaid CommonShipment chargeable amount", Consol.JK_TotalPrepaidShipmentChargeableAmount, expectedTotal);

			Consol.TopLevelShipments.Remove(newCollectShipment);
			AssertEquals("Removing a collect CommonShipment has no affect on total", Consol.JK_TotalPrepaidShipmentChargeableAmount, expectedTotal);

			expectedTotal -= newPrepaidShipment.ChargeableAmount;
			Consol.TopLevelShipments.Remove(newPrepaidShipment);
			AssertEquals("Removing a prepaid CommonShipment reduced total", Consol.JK_TotalPrepaidShipmentChargeableAmount, expectedTotal);

			Consol.TopLevelShipments.Remove(prepaidShipmentChargeableisWeight1);
			Consol.TopLevelShipments.Remove(prepaidShipmentChargeableisVolume);
			AssertEquals("Removing all prepaids sets total back to 0", Consol.JK_TotalPrepaidShipmentChargeableAmount, 0m);
		}

		public void TestJK_TotalPrepaidShipmentChargeableAmountNonAir()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Sea;

			CommonShipment collectShipments = CreateShipment(Constants.TransportModes.Sea, 500m, 2m, 1.538m);
			CommonShipment prepaidShipmentChargeableisWeight1 = CreateShipment(Constants.TransportModes.Sea, 200m, 1m, 1.538m, false);
			CommonShipment prepaidShipmentChargeableisVolume = CreateShipment(Constants.TransportModes.Sea, 200m, 3m, 1.538m, false);
			AssertEquals("Total prepaid chargeable amount defaults to 0", Consol.JK_TotalPrepaidShipmentChargeableAmount, 0m);

			Consol.TopLevelShipments.Add(prepaidShipmentChargeableisWeight1);
			ZDecimal expectedTotal = prepaidShipmentChargeableisWeight1.ChargeableAmount;
			AssertEquals("Total is CommonShipment chargeable amount", Consol.JK_TotalPrepaidShipmentChargeableAmount, expectedTotal);

			Consol.TopLevelShipments.Add(collectShipments);
			AssertEquals("A collect CommonShipment shouldn't affect prepaid CommonShipment chargeable amount", Consol.JK_TotalPrepaidShipmentChargeableAmount, expectedTotal);

			Consol.TopLevelShipments.Add(prepaidShipmentChargeableisVolume);
			expectedTotal += prepaidShipmentChargeableisVolume.ChargeableAmount;
			AssertEquals("CommonShipment chargeable will be converted volume, sum will include this", Consol.JK_TotalPrepaidShipmentChargeableAmount, expectedTotal);

			CommonShipment newCollectShipment = AddShipment(Consol, 200m, 1m, 1.538m);
			AssertEquals("Collect CommonShipment created from consol shouldn't affect total", Consol.JK_TotalPrepaidShipmentChargeableAmount, expectedTotal);

			CommonShipment newPrepaidShipment = AddShipment(Consol, 200m, 1m, 1.538m, false);
			expectedTotal += newPrepaidShipment.ChargeableAmount;
			AssertEquals("Prepaid created from consol should increase prepaid CommonShipment chargeable amount", Consol.JK_TotalPrepaidShipmentChargeableAmount, expectedTotal);

			Consol.TopLevelShipments.Remove(newCollectShipment);
			AssertEquals("Removing a collect CommonShipment has no affect on total", Consol.JK_TotalPrepaidShipmentChargeableAmount, expectedTotal);

			expectedTotal -= newPrepaidShipment.ChargeableAmount;
			Consol.TopLevelShipments.Remove(newPrepaidShipment);
			AssertEquals("Removing a prepaid CommonShipment reduced total", Consol.JK_TotalPrepaidShipmentChargeableAmount, expectedTotal);

			Consol.TopLevelShipments.Remove(prepaidShipmentChargeableisWeight1);
			Consol.TopLevelShipments.Remove(prepaidShipmentChargeableisVolume);
			AssertEquals("Removing all prepaids sets total back to 0", Consol.JK_TotalPrepaidShipmentChargeableAmount, 0m);
		}

		public void TestJK_TotalPrepaidShipmentChargeableAmountMultiCurrency()
		{
			CommonConsolForTest consol = Factory.New<CommonConsolForTest>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			RefCurrency defaultCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			ZString defaultCurrencyCode = defaultCurrency.RX_Code;

			CommonShipment prepaidShipment1 = CreateShipment(Constants.TransportModes.Air, 200m, 1m, 2m, false);
			prepaidShipment1.JS_RX_NKFrtRateCurrency = defaultCurrencyCode;
			CommonShipment prepaidShipment2 = CreateShipment(Constants.TransportModes.Air, 300m, 1m, 2m, false);
			prepaidShipment2.JS_RX_NKFrtRateCurrency = defaultCurrencyCode;
			consol.TopLevelShipments.Add(prepaidShipment1);
			consol.TopLevelShipments.Add(prepaidShipment2);
			AssertEquals("Prepaid shipments not multicurrency", false, consol.PrepaidIsMultiCurrency);
			AssertEquals("Currency will be global companies currency ", defaultCurrencyCode, consol.JK_TotalPrepaidShipmentChargeableAmountCurrencyCode);
			AssertEquals("Not multi currency, each CommonShipment is usin default currency, no conversion occurs", 1000m, consol.JK_TotalPrepaidShipmentChargeableAmount);

			prepaidShipment2.JS_RX_NKFrtRateCurrency = uSD.RX_Code;
			AssertEquals("Prepaid shipments are multicurrency", true, consol.PrepaidIsMultiCurrency);
			AssertEquals("Consol currency will be default companies currency ", defaultCurrencyCode, consol.JK_TotalPrepaidShipmentChargeableAmountCurrencyCode);
			AssertEquals("Consol is multi currency so a conversion occurs",
				uSD.ConvertUsingSellRate(ZDateTime.Now, prepaidShipment2.ChargeableAmount, defaultCurrency) +
				defaultCurrency.ConvertUsingSellRate(ZDateTime.Now, prepaidShipment1.ChargeableAmount, defaultCurrency),
				consol.JK_TotalPrepaidShipmentChargeableAmount);

			prepaidShipment1.JS_RX_NKFrtRateCurrency = uSD.RX_Code;
			AssertEquals("Prepaid shipments are not multicurrency", false, consol.PrepaidIsMultiCurrency);
			AssertEquals("Consol currency will be default companies currency ", defaultCurrencyCode, consol.JK_TotalPrepaidShipmentChargeableAmountCurrencyCode);
			AssertEquals("Consol currency is different to CommonShipment currency ", true, defaultCurrencyCode != prepaidShipment1.JS_RX_NKFrtRateCurrency);
			AssertEquals("Consol shipments are all same currency but not the default so a conversion takes place.",
				uSD.ConvertUsingSellRate(ZDateTime.Now, prepaidShipment2.ChargeableAmount, defaultCurrency) +
				uSD.ConvertUsingSellRate(ZDateTime.Now, prepaidShipment1.ChargeableAmount, defaultCurrency),
				consol.JK_TotalPrepaidShipmentChargeableAmount);
		}
		#endregion

		#region Transport sets AWB issue date

		public void TestTransportSetsAWBIssueDate()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			ZDateTime cutOffDate = ZDateTime.Now;
			AssertEquals(ZDateTime.Empty, consol.JK_MasterBillIssueDate);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.Transports[0].JW_TerminalCutOff = cutOffDate;
			AssertEquals(ZDateTime.Empty, consol.JK_MasterBillIssueDate);

			consol.JK_ConsolMode = Constants.ContainerModes.Combination;
			consol.Transports[0].JW_DepotCutOff = cutOffDate;
			AssertEquals(ZDateTime.Empty, consol.JK_MasterBillIssueDate);

			consol.Transports[0].JW_DepotCutOff = ZDateTime.Empty;
			consol.Transports[0].JW_TerminalCutOff = ZDateTime.Empty;

			FreightConfigurationRegistry.Instance.AWBIssueDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, FreightConfigurationRegistry.Instance.AWBIssueDateIsCutOffDate);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.Transports[0].JW_TerminalCutOff = cutOffDate;
			AssertEquals(cutOffDate, consol.JK_MasterBillIssueDate);

			consol.JK_MasterBillIssueDate = ZDateTime.Empty;
			consol.JK_ConsolMode = Constants.ContainerModes.Combination;
			consol.Transports[0].JW_DepotCutOff = cutOffDate;
			AssertEquals(cutOffDate, consol.JK_MasterBillIssueDate);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.Transports[0].JW_TerminalCutOff = cutOffDate.AddDays(3);
			AssertEquals(cutOffDate, consol.JK_MasterBillIssueDate);

			consol.JK_ConsolMode = Constants.ContainerModes.Combination;
			consol.Transports[0].JW_DepotCutOff = cutOffDate.AddDays(3);
			AssertEquals(cutOffDate, consol.JK_MasterBillIssueDate);
		}

		#endregion

		#region JK_TotalCollectShipmentChargeableAmount

		public void TestJK_TotalCollectShipmentChargeableAmountForAir()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Air;

			CommonShipment prepaidShipment = CreateShipment(Constants.TransportModes.Air, 500m, 2m, 1.538m, false);
			CommonShipment collectShipmentChargeableisWeight1 = CreateShipment(Constants.TransportModes.Air, 200m, 1m, 1.538m);
			CommonShipment collectShipmentChargeableisVolume = CreateShipment(Constants.TransportModes.Air, 200m, 3m, 1.538m);
			AssertEquals("Total Collect chargeable amount defaults to 0", Consol.JK_TotalCollectShipmentChargeableAmount, 0m);

			Consol.TopLevelShipments.Add(collectShipmentChargeableisWeight1);
			ZDecimal expectedTotal = collectShipmentChargeableisWeight1.ChargeableAmount;
			AssertEquals("Total is CommonShipment chargeable amount", Consol.JK_TotalCollectShipmentChargeableAmount, expectedTotal);

			Consol.TopLevelShipments.Add(prepaidShipment);
			AssertEquals("A Prepaid CommonShipment shouldn't affect Collect CommonShipment chargeable amount", Consol.JK_TotalCollectShipmentChargeableAmount, expectedTotal);

			Consol.TopLevelShipments.Add(collectShipmentChargeableisVolume);
			expectedTotal += collectShipmentChargeableisVolume.ChargeableAmount;
			AssertEquals("CommonShipment chargeable will be converted volume, sum will include this", Consol.JK_TotalCollectShipmentChargeableAmount, expectedTotal);

			CommonShipment newPrepaidShipment = AddShipment(Consol, 200m, 1m, 1.538m, false);
			AssertEquals("Prepaid CommonShipment created from consol shouldn't affect total", Consol.JK_TotalCollectShipmentChargeableAmount, expectedTotal);

			CommonShipment newCollectShipment = AddShipment(Consol, 200m, 1m, 1.538m);
			expectedTotal += newCollectShipment.ChargeableAmount;
			AssertEquals("Collect created from consol should increase Collect CommonShipment chargeable amount", Consol.JK_TotalCollectShipmentChargeableAmount, expectedTotal);

			Consol.TopLevelShipments.Remove(newPrepaidShipment);
			AssertEquals("Removing a Prepaid CommonShipment has no affect on total", Consol.JK_TotalCollectShipmentChargeableAmount, expectedTotal);

			expectedTotal -= newCollectShipment.ChargeableAmount;
			Consol.TopLevelShipments.Remove(newCollectShipment);
			AssertEquals("Removing a Collect CommonShipment reduced total", Consol.JK_TotalCollectShipmentChargeableAmount, expectedTotal);

			Consol.TopLevelShipments.Remove(collectShipmentChargeableisWeight1);
			Consol.TopLevelShipments.Remove(collectShipmentChargeableisVolume);
			AssertEquals("Removing all Collects sets total back to 0", Consol.JK_TotalCollectShipmentChargeableAmount, 0m);
		}

		public void TestJK_TotalCollectShipmentChargeableAmountNonAir()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Sea;

			CommonShipment prepaidShipment = CreateShipment(Constants.TransportModes.Sea, 500m, 2m, 1.538m, false);
			CommonShipment collectShipmentChargeableisWeight1 = CreateShipment(Constants.TransportModes.Sea, 200m, 1m, 1.538m);
			CommonShipment collectShipmentChargeableisVolume = CreateShipment(Constants.TransportModes.Sea, 200m, 3m, 1.538m);
			AssertEquals("Total Collect chargeable amount defaults to 0", Consol.JK_TotalCollectShipmentChargeableAmount, 0m);

			Consol.TopLevelShipments.Add(collectShipmentChargeableisWeight1);
			ZDecimal expectedTotal = collectShipmentChargeableisWeight1.ChargeableAmount;
			AssertEquals("Total is CommonShipment chargeable amount", Consol.JK_TotalCollectShipmentChargeableAmount, expectedTotal);

			Consol.TopLevelShipments.Add(prepaidShipment);
			AssertEquals("A Prepaid CommonShipment shouldn't affect Collect CommonShipment chargeable amount", Consol.JK_TotalCollectShipmentChargeableAmount, expectedTotal);

			Consol.TopLevelShipments.Add(collectShipmentChargeableisVolume);
			expectedTotal += collectShipmentChargeableisVolume.ChargeableAmount;
			AssertEquals("CommonShipment chargeable will be converted volume, sum will include this", Consol.JK_TotalCollectShipmentChargeableAmount, expectedTotal);

			CommonShipment newPrepaidShipment = AddShipment(Consol, 200m, 1m, 1.538m, false);
			AssertEquals("Prepaid CommonShipment created from consol shouldn't affect total", Consol.JK_TotalCollectShipmentChargeableAmount, expectedTotal);

			CommonShipment newCollectShipment = AddShipment(Consol, 200m, 1m, 1.538m);
			expectedTotal += newCollectShipment.ChargeableAmount;
			AssertEquals("Collect created from consol should increase Collect CommonShipment chargeable amount", Consol.JK_TotalCollectShipmentChargeableAmount, expectedTotal);

			Consol.TopLevelShipments.Remove(newPrepaidShipment);
			AssertEquals("Removing a Prepaid CommonShipment has no affect on total", Consol.JK_TotalCollectShipmentChargeableAmount, expectedTotal);

			expectedTotal -= newCollectShipment.ChargeableAmount;
			Consol.TopLevelShipments.Remove(newCollectShipment);
			AssertEquals("Removing a Collect CommonShipment reduced total", Consol.JK_TotalCollectShipmentChargeableAmount, expectedTotal);

			Consol.TopLevelShipments.Remove(collectShipmentChargeableisWeight1);
			Consol.TopLevelShipments.Remove(collectShipmentChargeableisVolume);
			AssertEquals("Removing all Collects sets total back to 0", Consol.JK_TotalCollectShipmentChargeableAmount, 0m);
		}

		public void TestJK_TotalCollectShipmentChargeableAmountMultiCurrency()
		{
			CommonConsolForTest consol = Factory.New<CommonConsolForTest>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			RefCurrency defaultCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			ZString defaultCurrencyCode = defaultCurrency.RX_Code;

			CommonShipment collectShipment1 = CreateShipment(Constants.TransportModes.Air, 200m, 1m, 2m);
			CommonShipment collectShipment2 = CreateShipment(Constants.TransportModes.Air, 200m, 1m, 2m);
			collectShipment1.JS_RX_NKFrtRateCurrency = defaultCurrencyCode;
			collectShipment2.JS_RX_NKFrtRateCurrency = defaultCurrencyCode;
			consol.TopLevelShipments.Add(collectShipment1);
			consol.TopLevelShipments.Add(collectShipment2);
			AssertEquals("Collect shipments not multicurrency", false, consol.CollectIsMultiCurrency);
			AssertEquals("Currency will be default currency same as first shipment's currency", defaultCurrencyCode, consol.JK_TotalCollectShipmentChargeableAmountCurrencyCode);
			AssertEquals("Not multi currency and shipment's currency same as default so no conversion occurs", 800m, consol.JK_TotalCollectShipmentChargeableAmount);

			collectShipment2.JS_RX_NKFrtRateCurrency = uSD.RX_Code;
			AssertEquals("Currency will be default company's currency ", defaultCurrencyCode, consol.JK_TotalCollectShipmentChargeableAmountCurrencyCode);
			AssertEquals("shipments are multicurrency", true, consol.CollectIsMultiCurrency);
			AssertEquals("It's multi currency so conversion occurs",
				uSD.ConvertUsingSellRate(ZDateTime.Now, collectShipment2.ChargeableAmount, defaultCurrency) +
				defaultCurrency.ConvertUsingSellRate(ZDateTime.Now, collectShipment1.ChargeableAmount, defaultCurrency),
				consol.JK_TotalCollectShipmentChargeableAmount);

			collectShipment1.JS_RX_NKFrtRateCurrency = uSD.RX_Code;
			AssertEquals("Collect shipments are not multicurrency", false, consol.CollectIsMultiCurrency);
			AssertEquals("Consol currency will be default company's currency ", defaultCurrencyCode, consol.JK_TotalCollectShipmentChargeableAmountCurrencyCode);
			AssertEquals("Consol currency is different to shipment's currency ", true, defaultCurrencyCode != collectShipment1.JS_RX_NKFrtRateCurrency);
			AssertEquals("Consol shipments are all same currency but not the default so a conversion takes place.",
				uSD.ConvertUsingSellRate(ZDateTime.Now, collectShipment2.ChargeableAmount, defaultCurrency) +
				uSD.ConvertUsingSellRate(ZDateTime.Now, collectShipment1.ChargeableAmount, defaultCurrency),
				consol.JK_TotalCollectShipmentChargeableAmount);
		}

		#endregion

		#region TestTotalShipmentChargeableAmountDecimals

		public void TestTotalShipmentChargeableAmountDecimals()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			AssertEquals(GlbCompany.CurrentCompany.LocalCurrency.Decimals, consol.JK_TotalPrepaidShipmentChargeableAmountDecimals);
			AssertEquals(GlbCompany.CurrentCompany.LocalCurrency.Decimals, consol.JK_TotalCollectShipmentChargeableAmountDecimals);
		}

		#endregion

		#region JK_TotalShipmentVolume

		public void TestJK_TotalShipmentVolume()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			AssertTotalVolumes(consol, 0m, 0m, 0m);

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualVolume = 1m;
			shipment1.JS_DocumentedVolume = 1.01m;
			shipment1.JS_ManifestedVolume = 1.10m;

			AssertTotalVolumes(consol, 1.00m, 1.01m, 1.10m);

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualVolume = 2m;
			shipment2.JS_DocumentedVolume = 2.02m;
			shipment2.JS_ManifestedVolume = 2.20m;

			AssertTotalVolumes(consol, 3.00m, 3.03m, 3.30m);

			CommonShipment shipment3 = consol.Shipments.AddNew();
			shipment3.JS_ActualVolume = 400m;
			shipment3.JS_DocumentedVolume = 404m;
			shipment3.JS_ManifestedVolume = 440m;
			shipment3.JS_UnitOfVolume = Constants.Volume.CubicDecimetres;

			AssertTotalVolumes(consol, 3.4m, 3.434m, 3.74m);

			consol.Shipments.Remove(shipment3);
			AssertTotalVolumes(consol, 3.00m, 3.03m, 3.30m);

			consol.Shipments.RemoveAll();
			AssertTotalVolumes(consol, 0m, 0m, 0m);
		}

		void AssertTotalVolumes(CommonConsol consol, decimal actual, decimal documented, decimal manifested)
		{
			CombineAssertions(delegate
			{
				AssertEquals(actual, consol.JK_TotalShipmentVolume);
				AssertEquals(documented, consol.JK_TotalDocumentedVolume);
				AssertEquals(manifested, consol.JK_TotalManifestedVolume);
			});
		}

		public void TestJK_TotalShipmentVolumeInfo()
		{
			AssertEquals("Correct name", "JK_TotalShipmentVolume", Consol.JK_TotalShipmentVolumeInfo.Name);
		}

		public void TestJK_TotalShipmentVolumeUnit()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			AssertEquals("Precondition", 0, consol.Shipments.Count);
			AssertEquals("CubicMetres by default", Constants.Volume.CubicMetres, consol.JK_TotalShipmentVolumeUnit);

			consol.Shipments.AddNew().JS_UnitOfVolume = Constants.Volume.CubicFeet;
			consol.Shipments.AddNew().JS_UnitOfVolume = Constants.Volume.CubicYards;
			AssertEquals("CubicFeet only when all shipments has imperial volume", Constants.Volume.CubicFeet, consol.JK_TotalShipmentVolumeUnit);

			consol.Shipments[0].JS_UnitOfVolume = Constants.Volume.CubicDecimetres;
			AssertEquals("CubicMetres in all other cases", Constants.Volume.CubicMetres, consol.JK_TotalShipmentVolumeUnit);
		}

		#endregion

		#region JK_TotalShipmentLoadingMeters

		public void TestJK_TotalShipmentLoadingMeters()
		{
			CommonShipment shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_LoadingMeters = 100m;
			shipment1.JS_DocumentedLoadingMeters = 101m;
			shipment1.JS_ManifestedLoadingMeters = 110m;

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_LoadingMeters = 200m;
			shipment2.JS_DocumentedLoadingMeters = 202m;
			shipment2.JS_ManifestedLoadingMeters = 220m;

			CommonConsol consol = Factory.New<CommonConsol>();
			AssertTotalLoadingMeters(consol, 0m, 0m, 0m);

			consol.Shipments.Add(shipment1);
			AssertTotalLoadingMeters(consol, 100m, 101m, 110m);

			consol.Shipments.Add(shipment2);
			AssertTotalLoadingMeters(consol, 300m, 303m, 330m);

			consol.Shipments.Remove(shipment1);
			AssertTotalLoadingMeters(consol, 200m, 202m, 220m);

			consol.Shipments.RemoveAll();
			AssertTotalLoadingMeters(consol, 0m, 0m, 0m);
		}

		void AssertTotalLoadingMeters(CommonConsol consol, decimal actual, decimal documented, decimal manifested)
		{
			CombineAssertions(delegate
			{
				AssertEquals(actual, consol.JK_TotalShipmentLoadingMeters);
				AssertEquals(documented, consol.JK_TotalDocumentedLoadingMeters);
				AssertEquals(manifested, consol.JK_TotalManifestedLoadingMeters);
			});
		}

		public void TestJK_TotalShipmentLoadingMetersInfo()
		{
			AssertEquals("Correct name", "JK_TotalShipmentLoadingMeters", Consol.JK_TotalShipmentLoadingMetersInfo.Name);
		}

		public void TestLoadingMetersAreUsedForChargeableCalculation()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Road;

			FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.RoadLoadingMetersWeightPerLDM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000m);

			AssertEquals("Precondition", true, consol.IsRoadLoadingMetersEnabled);

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 500m;
			shipment1.JS_DocumentedWeight = 505m;
			shipment1.JS_ManifestedWeight = 550m;

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualWeight = 700m;
			shipment2.JS_DocumentedWeight = 707m;
			shipment2.JS_ManifestedWeight = 770m;

			AssertEquals(1200m, consol.JK_ConsolChargeable);
			AssertEquals(1212m, consol.JK_TotalDocumentedChargeable);
			AssertEquals(1320m, consol.JK_TotalManifestedChargeable);

			shipment1.JS_LoadingMeters = 1m;
			shipment1.JS_DocumentedLoadingMeters = 1.1m;
			shipment1.JS_ManifestedLoadingMeters = 1.2m;

			shipment2.JS_LoadingMeters = 2m;
			shipment2.JS_DocumentedLoadingMeters = 2.1m;
			shipment2.JS_ManifestedLoadingMeters = 2.2m;

			AssertEquals(3000m, consol.JK_ConsolChargeable);
			AssertEquals(3200m, consol.JK_TotalDocumentedChargeable);
			AssertEquals(3400m, consol.JK_TotalManifestedChargeable);
		}

		public void TestIsRoadLoadingMetersEnabled()
		{
			string[] allTransportModes = typeof(Constants.TransportModes).GetFields().Select(fieldInfo => (string)fieldInfo.GetValue(null)).ToArray();
			AssertEquals("Precondition", true, allTransportModes.Length > 0);

			CommonConsol consol = Factory.New<CommonConsol>();
			foreach (string transportMode in allTransportModes)
			{
				consol.JK_TransportMode = transportMode;

				FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertEquals(false, consol.IsRoadLoadingMetersEnabled);

				FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AssertEquals(consol.JK_TransportMode == Constants.TransportModes.Road, consol.IsRoadLoadingMetersEnabled);
			}
		}

		#endregion

		#region JK_TotalShipmentQuantity

		public void TestJK_TotalShipmentQuantity()
		{
			//AssertNotNull("Returned null", Consol.JK_TotalShipmentQuantity);
			CommonShipment shipment1 = CommonShipment.New(Factory);
			CommonShipment shipment2 = CommonShipment.New(Factory);
			shipment1.JS_OuterPacks = 2;
			shipment2.JS_OuterPacks = 3;

			Consol.TopLevelShipments.Add(shipment1);
			AssertEquals("Quantity is 2", Consol.JK_TotalShipmentQuantity, 2m);

			Consol.TopLevelShipments.Add(shipment2);
			AssertEquals("Quantity is 5", Consol.JK_TotalShipmentQuantity, 5m);

			CommonShipment newShipment = Consol.TopLevelShipments.AddNew();
			newShipment.JS_OuterPacks = 4;
			AssertEquals("Creating CommonShipment from consol updates total, quantity is 9", Consol.JK_TotalShipmentQuantity, 9m);

			Consol.TopLevelShipments.Remove(shipment1);
			AssertEquals("Removing a CommonShipment works, , quantity is 7", Consol.JK_TotalShipmentQuantity, 7m);

			Consol.TopLevelShipments.Remove(shipment2);
			Consol.TopLevelShipments.Remove(newShipment);
			AssertEquals("Removing everything works  quantity is 0", Consol.JK_TotalShipmentQuantity, 0m);
		}

		public void TestJK_TotalShipmentQuantityInfo()
		{
			AssertEquals("Correct name", "JK_TotalShipmentQuantity", Consol.JK_TotalShipmentQuantityInfo.Name);
		}

		#endregion

		#region JK_ShipmentTotalPackageCountPackType

		public void TestJK_ShipmentTotalPackageCountPackType()
		{
			CommonShipment shipment1 = CommonShipment.New(Factory);
			CommonShipment shipment2 = CommonShipment.New(Factory);
			shipment1.JS_F3_NKTotalCountPackType = "ART";
			shipment2.JS_F3_NKTotalCountPackType = "ART";

			AssertEquals(FreightPacksDataRegistry.Instance.InnerPackUnit.Value, Consol.JK_ShipmentTotalPackageCountPackType);

			Consol.TopLevelShipments.Add(shipment1);
			Consol.TopLevelShipments.Add(shipment2);

			AssertEquals("ART", Consol.JK_ShipmentTotalPackageCountPackType);
			shipment1.JS_F3_NKTotalCountPackType = "RLS";

			AssertEquals(FreightPacksDataRegistry.Instance.InnerPackUnit.Value, Consol.JK_ShipmentTotalPackageCountPackType);
		}

		#endregion

		#region JK_ShipmentTotalQuantityPackType

		public void TestJK_ShipmentTotalQuantityPackType()
		{
			CommonShipment shipment1 = CommonShipment.New(Factory);
			CommonShipment shipment2 = CommonShipment.New(Factory);
			shipment1.JS_F3_NKPackType = "ART";
			shipment2.JS_F3_NKPackType = "ART";

			AssertEquals(FreightPacksDataRegistry.Instance.OuterPackUnit.Value, Consol.JK_ShipmentTotalQuantityPackType);

			Consol.TopLevelShipments.Add(shipment1);
			Consol.TopLevelShipments.Add(shipment2);

			AssertEquals("ART", Consol.JK_ShipmentTotalQuantityPackType);
			shipment1.JS_F3_NKPackType = "RLS";

			AssertEquals(FreightPacksDataRegistry.Instance.OuterPackUnit.Value, Consol.JK_ShipmentTotalQuantityPackType);
		}

		#endregion

		#region JK_TotalShipmentPackageCount

		public void TestJK_TotalShipmentPackageCount()
		{
			CommonShipment shipment1 = CommonShipment.New(Factory);
			CommonShipment shipment2 = CommonShipment.New(Factory);
			shipment1.JS_TotalPackageCount = 2;
			shipment2.JS_TotalPackageCount = 3;

			Consol.TopLevelShipments.Add(shipment1);
			AssertEquals("Quantity is 2", Consol.JK_TotalShipmentPackageCount, 2m);

			Consol.TopLevelShipments.Add(shipment2);
			AssertEquals("Quantity is 5", Consol.JK_TotalShipmentPackageCount, 5m);

			CommonShipment newShipment = Consol.TopLevelShipments.AddNew();
			newShipment.JS_TotalPackageCount = 4;
			AssertEquals("Creating CommonShipment from consol updates total, quantity is 9", Consol.JK_TotalShipmentPackageCount, 9m);

			Consol.TopLevelShipments.Remove(shipment1);
			AssertEquals("Removing a CommonShipment works, , quantity is 7", Consol.JK_TotalShipmentPackageCount, 7m);

			Consol.TopLevelShipments.Remove(shipment2);
			Consol.TopLevelShipments.Remove(newShipment);
			AssertEquals("Removing everything works  quantity is 0", Consol.JK_TotalShipmentPackageCount, 0m);
		}

		public void TestJK_TotalShipmentPackageCountInfo()
		{
			AssertEquals("Correct name", "JK_TotalShipmentPackageCount", Consol.JK_TotalShipmentPackageCountInfo.Name);
		}

		#endregion

		#region JK_CRN

		[ExpectNoExceptions]
		public void TestJK_CRN()
		{
			AssertEquals(0, Consol.CusEntryNums.Count);
			AssertEquals("By default there should be no CRN", ZString.Empty, Consol.JK_CRN);

			const string testCRNNumber1 = "1S019201820";
			Consol.JK_CRN = testCRNNumber1;
			AssertEquals(1, Consol.CusEntryNums.Count);
			var crn1 = Consol.CusEntryNums[0];
			crn1.CE_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			AssertEquals(testCRNNumber1, crn1.CE_EntryNum);
			AssertEquals("CRN should be new testCRNNumber1", testCRNNumber1, Consol.JK_CRN);

			var crn2 = Consol.CusEntryNums.AddNew();
			crn2.CE_ParentID = Consol.PK;
			crn2.CE_ParentTable = Consol.TableName;
			crn2.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			crn2.CE_EntryType = CusEntryNumberTypes.Standard.ClearancePermitNumber;
			crn2.CE_EntryIsSystemGenerated = true;
			crn2.CE_SystemLastEditTimeUtc = crn1.CE_SystemLastEditTimeUtc.AddMinutes(1);

			const string testCRNNumber2 = "1S019201821";
			Consol.JK_CRN = testCRNNumber2;
			AssertEquals(2, Consol.CusEntryNums.Count);
			AssertEquals(testCRNNumber2, crn2.CE_EntryNum);
			AssertEquals("CRN should be new testCRNNumber2", testCRNNumber2, Consol.JK_CRN);

			Consol.CusEntryNums.RemoveAndDeleteAll();
			AssertEquals("CRN should be blank again", ZString.Empty, Consol.JK_CRN);
		}

		#endregion

		#region TestJK_CRNReadonly

		public void TestJK_CRNReadonly()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "AU");

			CommonConsol commonConsol = Factory.NewWithValidTestData<CommonConsol>();
			Factory.Save();

			AssertEquals(true, commonConsol.JK_CRNInfo.ReadOnly);

			GlbCompany.CurrentCompany.SetCountry("IS");
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "IS");

			commonConsol = Factory.NewWithValidTestData<CommonConsol>();
			AssertEquals(commonConsol.JK_CRNInfo.ReadOnly, false);
		}

		#endregion

		#region JK_JX_DepartOrArriveReference and JK_JX_DepartOrArriveBerth

		public void TestJK_JX_DepartOrArriveReference_ForImport()
		{
			CommonConsol consol = SetupConsolWithVoyage("XXXXX", GlbBranch.CurrentBranch.GB_RL_NKHomePort, "depart", "arrive", "1", "2");
			AssertEquals("Arrival for imports", "arrive", consol.JK_JX_DepartOrArriveReference);

			consol.Transports.RemoveAndDeleteAll();
			AssertEquals("Arrival for imports", ZString.Empty, consol.JK_JX_DepartOrArriveReference);
		}

		public void TestJK_JX_DepartOrArriveReference_ForExport()
		{
			CommonConsol consol = SetupConsolWithVoyage(GlbBranch.CurrentBranch.GB_RL_NKHomePort, "XXXXX", "depart", "arrive", "1", "2");
			AssertEquals("Departure for exports", "depart", consol.JK_JX_DepartOrArriveReference);

			consol.Transports.RemoveAndDeleteAll();
			AssertEquals("Arrival for imports", ZString.Empty, consol.JK_JX_DepartOrArriveReference);
		}

		public void TestJK_JX_DepartOrArriveBerth_ForImport()
		{
			CommonConsol consol = SetupConsolWithVoyage("XXXXX", GlbBranch.CurrentBranch.GB_RL_NKHomePort, "depart", "arrive", "1", "2");
			AssertEquals("Berth for imports", "2", consol.JK_JX_DepartOrArriveBerth);

			consol.Transports.RemoveAndDeleteAll();
			AssertEquals("Arrival for imports", ZString.Empty, consol.JK_JX_DepartOrArriveReference);
		}

		public void TestJK_JX_DepartOrArriveBerth_ForExport()
		{
			CommonConsol consol = SetupConsolWithVoyage(GlbBranch.CurrentBranch.GB_RL_NKHomePort, "XXXXX", "depart", "arrive", "1", "2");
			AssertEquals("Berth for exports", "1", consol.JK_JX_DepartOrArriveBerth);

			consol.Transports.RemoveAndDeleteAll();
			AssertEquals("Arrival for imports", ZString.Empty, consol.JK_JX_DepartOrArriveReference);
		}

		CommonConsol SetupConsolWithVoyage(string origin, string destination, string departRef, string arriveRef, string departBerth, string arriveBerth)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = "sanoe";

			VoyageOrigin voyageOrigin = voyage.Origins.AddNew();
			voyageOrigin.JA_RL_NKPortOfLoading = origin;
			voyageOrigin.JA_DepartReference = departRef;
			voyageOrigin.JA_Berth = departBerth;

			VoyageDestination voyageDestination = voyage.Destinations.AddNew();
			voyageDestination.JB_RL_NKPortOfDischarge = destination;
			voyageDestination.JB_ArrivalReference = arriveRef;
			voyageDestination.JB_Berth = arriveBerth;

			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_JX = sailing.PK;

			return consol;
		}

		#endregion

		#region Weight / Volume / Chargeable / Loading Meters for documents

		public void TestGetTotalShipmentWeightForDoc()
		{
			CommonShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 10M;
			shipment.JS_ManifestedWeight = 9M;
			shipment.JS_DocumentedWeight = 8M;

			shipment = Consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 20M;
			shipment.JS_ManifestedWeight = 18M;
			shipment.JS_DocumentedWeight = 16M;

			AssertEquals("Weight is not overriden", false, Consol.JK_OverrideConsolChargeable);
			AssertGetTotalForDoc(Consol.GetTotalShipmentWeightForDoc, 30m, 27m, 24m);
			AssertEquals(30m, Consol.GetTotalShipmentWeightWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Actual).Item1);
			AssertEquals(27m, Consol.GetTotalShipmentWeightWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Carrier).Item1);
			AssertEquals(24m, Consol.GetTotalShipmentWeightWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Client).Item1);

			Consol.JK_OverrideConsolChargeable = true;
			AssertEquals("Weight can be overriden", true, Consol.JK_OverrideConsolChargeable);

			Consol.JK_CorrectedConsolWeight = 32m;
			AssertGetTotalForDoc(Consol.GetTotalShipmentWeightForDoc, 32m, 27m, 24m);
			AssertEquals(32m, Consol.GetTotalShipmentWeightWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Actual).Item1);
			AssertEquals(27m, Consol.GetTotalShipmentWeightWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Carrier).Item1);
			AssertEquals(24m, Consol.GetTotalShipmentWeightWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Client).Item1);
		}

		public void TestGetTotalShipmentWeightWithScaleForDoc()
		{
			AssertEquals(JobShipmentSchema.JS_ActualWeight.Scale, Consol.GetTotalShipmentWeightWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Actual).Item2);

			AssertEquals(JobShipmentSchema.JS_ManifestedWeight.Scale, Consol.GetTotalShipmentWeightWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Carrier).Item2);

			AssertEquals(JobShipmentSchema.JS_DocumentedWeight.Scale, Consol.GetTotalShipmentWeightWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Client).Item2);
		}

		public void TestGetTotalShipmentVolumeForDoc()
		{
			CommonShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_ActualVolume = 10M;
			shipment.JS_ManifestedVolume = 9M;
			shipment.JS_DocumentedVolume = 8M;

			shipment = Consol.Shipments.AddNew();
			shipment.JS_ActualVolume = 20M;
			shipment.JS_ManifestedVolume = 18M;
			shipment.JS_DocumentedVolume = 16M;

			AssertEquals("Volume is not overriden", false, Consol.JK_OverrideConsolChargeable);
			AssertGetTotalForDoc(Consol.GetTotalShipmentVolumeForDoc, 30m, 27m, 24m);
			AssertEquals(30m, Consol.GetTotalShipmentVolumeWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Actual).Item1);
			AssertEquals(27m, Consol.GetTotalShipmentVolumeWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Carrier).Item1);
			AssertEquals(24m, Consol.GetTotalShipmentVolumeWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Client).Item1);

			Consol.JK_OverrideConsolChargeable = true;
			AssertEquals("Volume can be overriden", true, Consol.JK_OverrideConsolChargeable);

			Consol.JK_CorrectedConsolVolume = 32m;
			AssertGetTotalForDoc(Consol.GetTotalShipmentVolumeForDoc, 32m, 27m, 24m);
			AssertEquals(32m, Consol.GetTotalShipmentVolumeWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Actual).Item1);
			AssertEquals(27m, Consol.GetTotalShipmentVolumeWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Carrier).Item1);
			AssertEquals(24m, Consol.GetTotalShipmentVolumeWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Client).Item1);
		}

		public void TestGetTotalShipmentVolumeWithScaleForDoc()
		{
			AssertEquals(JobShipmentSchema.JS_ActualVolume.Scale, Consol.GetTotalShipmentVolumeWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Actual).Item2);

			AssertEquals(JobShipmentSchema.JS_ManifestedVolume.Scale, Consol.GetTotalShipmentVolumeWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Carrier).Item2);

			AssertEquals(JobShipmentSchema.JS_DocumentedVolume.Scale, Consol.GetTotalShipmentVolumeWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Client).Item2);
		}

		public void TestGetTotalShipmentLoadingMetersForDoc()
		{
			CommonShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_LoadingMeters = 10m;
			shipment.JS_ManifestedLoadingMeters = 9m;
			shipment.JS_DocumentedLoadingMeters = 8m;

			shipment = Consol.Shipments.AddNew();
			shipment.JS_LoadingMeters = 20m;
			shipment.JS_ManifestedLoadingMeters = 18m;
			shipment.JS_DocumentedLoadingMeters = 16m;

			AssertGetTotalForDoc(Consol.GetTotalShipmentLoadingMetersForDoc, 30m, 27m, 24m);
			AssertEquals(30m, Consol.GetTotalShipmentLoadingMetersWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Actual).Item1);
			AssertEquals(27m, Consol.GetTotalShipmentLoadingMetersWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Carrier).Item1);
			AssertEquals(24m, Consol.GetTotalShipmentLoadingMetersWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Client).Item1);
		}

		public void TestGetTotalShipmentLoadingMetersWithScaleForDoc()
		{
			AssertEquals(JobShipmentSchema.JS_LoadingMeters.Scale, Consol.GetTotalShipmentLoadingMetersWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Actual).Item2);

			AssertEquals(JobShipmentSchema.JS_ManifestedLoadingMeters.Scale, Consol.GetTotalShipmentLoadingMetersWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Carrier).Item2);

			AssertEquals(JobShipmentSchema.JS_DocumentedLoadingMeters.Scale, Consol.GetTotalShipmentLoadingMetersWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Client).Item2);
		}

		public void TestGetTotalShipmentChargeableForDoc()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Air;

			CommonShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 10M;
			shipment.JS_ManifestedWeight = 9M;
			shipment.JS_DocumentedWeight = 8M;

			shipment = Consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 20M;
			shipment.JS_ManifestedWeight = 18M;
			shipment.JS_DocumentedWeight = 16M;

			AssertEquals("Chargeable is not overriden", false, Consol.JK_OverrideConsolChargeable);
			AssertGetTotalForDoc(Consol.GetTotalShipmentChargeableForDoc, 30m, 27m, 24m);
			AssertEquals(30m, Consol.GetTotalShipmentChargeableWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Actual).Item1);
			AssertEquals(27m, Consol.GetTotalShipmentChargeableWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Carrier).Item1);
			AssertEquals(24m, Consol.GetTotalShipmentChargeableWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Client).Item1);

			Consol.JK_OverrideConsolChargeable = true;
			AssertEquals("Chargeable is overriden", true, Consol.JK_OverrideConsolChargeable);

			Consol.JK_ConsolChargeable = 32m;
			AssertGetTotalForDoc(Consol.GetTotalShipmentChargeableForDoc, 32m, 27m, 24m);
			AssertEquals(32m, Consol.GetTotalShipmentChargeableWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Actual).Item1);
			AssertEquals(27m, Consol.GetTotalShipmentChargeableWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Carrier).Item1);
			AssertEquals(24m, Consol.GetTotalShipmentChargeableWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Client).Item1);
		}

		public void TestGetTotalShipmentChargeableWithScaleForDoc()
		{
			AssertEquals(JobShipmentSchema.JS_ActualChargeable.Scale, Consol.GetTotalShipmentChargeableWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Actual).Item2);

			AssertEquals(JobShipmentSchema.JS_ManifestedChargeable.Scale, Consol.GetTotalShipmentChargeableWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Carrier).Item2);

			AssertEquals(JobShipmentSchema.JS_DocumentedChargeable.Scale, Consol.GetTotalShipmentChargeableWithScaleForDoc(WeightAndVolumeDisplayTypes.Codes.Client).Item2);
		}

		void AssertGetTotalForDoc(Func<string, ZDecimal> getTotalForDoc, ZDecimal actual, ZDecimal carrier, ZDecimal client)
		{
			AssertEquals("Actual value", actual, getTotalForDoc(WeightAndVolumeDisplayTypes.Codes.Actual));
			AssertEquals("Manifested value", carrier, getTotalForDoc(WeightAndVolumeDisplayTypes.Codes.Carrier));
			AssertEquals("Documented value", client, getTotalForDoc(WeightAndVolumeDisplayTypes.Codes.Client));
		}

		#endregion

		public void TestAllowsShipmentsWithApprovedForCargoOnlyInspectionType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var consol = Factory.New<CommonConsol>();
				AssertEquals("Precondition", true, consol.AllowsShipmentsWithApprovedForCargoOnlyInspectionType);

				var transport = consol.Transports[0];
				transport.JW_TransportMode = "AIR";
				transport.JW_RL_NKLoadPort = "USCHI";
				transport.JW_RL_NKDiscPort = "SGSIN";
				AssertEquals("Unlinked transports are assumed to be cargo only", true, consol.AllowsShipmentsWithApprovedForCargoOnlyInspectionType);

				JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_IsCargoOnly = false;

				VoyageOrigin origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "USNYC";
				VoyageDestination dest = voyage.Destinations.AddNew();
				dest.JB_RL_NKPortOfDischarge = "SGSIN";

				JobSailing sailing = voyage.Sailings[0];
				transport.JW_JX = sailing.PK;
				AssertEquals("Linked non-cargo only export", false, consol.AllowsShipmentsWithApprovedForCargoOnlyInspectionType);

				transport.JW_IsCargoOnly = true;
				AssertEquals("Cargo only flight", true, consol.AllowsShipmentsWithApprovedForCargoOnlyInspectionType);

				transport.JW_IsCargoOnly = false;
				transport.JW_RL_NKLoadPort = "SGSIN";
				transport.JW_RL_NKDiscPort = "USCHI";
				AssertEquals("Doesn't apply to imports", true, consol.AllowsShipmentsWithApprovedForCargoOnlyInspectionType);
			}
		}

		public void TestAllowsShipmentsWithApprovedForCargoOnlyInspectionTypeForTwoRoutingLegs()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var consol = Factory.New<CommonConsol>();
				AssertEquals("Precondition", true, consol.AllowsShipmentsWithApprovedForCargoOnlyInspectionType);

				JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_IsCargoOnly = true;

				VoyageOrigin origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "USORD";
				VoyageDestination dest = voyage.Destinations.AddNew();
				dest.JB_RL_NKPortOfDischarge = "SGSIN";
				JobSailing sailing = voyage.Sailings[0];

				var transport1 = consol.Transports[0];
				transport1.JW_TransportMode = "AIR";
				transport1.JW_JX = sailing.PK;
				AssertEquals("One leg Linked cargo only ", true, consol.AllowsShipmentsWithApprovedForCargoOnlyInspectionType);

				Transport transport2 = consol.Transports.AddNew();
				transport2.JW_TransportMode = "AIR";
				transport2.JW_RL_NKLoadPort = "SGSIN";
				transport2.JW_RL_NKDiscPort = "AUSYD";
				transport2.JW_IsLinked = ZBool.False;
				AssertEquals("Two legs Cargo only flight", true, consol.AllowsShipmentsWithApprovedForCargoOnlyInspectionType);
			}
		}

		#endregion

		#region Test PackLine Allocation Methods

		public void TestCheckCanAllocateShipmentsToContainers()
		{
			RefContainer containerRef1 = Factory.New<RefContainer>();
			containerRef1.RC_Code = "20GM";

			RefContainer containerRef2 = Factory.New<RefContainer>();
			containerRef2.RC_Code = "40GM";

			CommonConsol consol = Factory.New<CommonConsol>();

			Assert("Not expecting to be able to add bookings to consol", !consol.CheckCanAllocateShipmentsToContainers());

			consol.Containers.AddNew();

			Assert("Not expecting to be able to add bookings to consol", !consol.CheckCanAllocateShipmentsToContainers());

			consol.Containers[0].JC_RC = containerRef1.PK;

			Assert("Not expecting to be able to add bookings to consol", !consol.CheckCanAllocateShipmentsToContainers());

			containerRef1.RC_CubicCapacity = 16m;

			Assert("Expecting to be able to add bookings to consol", consol.CheckCanAllocateShipmentsToContainers());

			consol.Containers.AddNew();
			consol.Containers[1].JC_RC = containerRef2.PK;

			Assert("Not expecting to be able to add bookings to consol", !consol.CheckCanAllocateShipmentsToContainers());
		}

		#endregion

		#region Test Set Is Forwarding Flag

		public void TestJK_IsForwardFlagSet()
		{
			var consol = Factory.New<CommonConsol>();
			AssertEquals("JK_IsForwarding should be set to true", true, consol.JK_IsForwarding);
		}

		#endregion

		#region Related Objects

		#region Containers

		public void TestResetContainerTrainWagonNumberForNonRail()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container = consol.Containers.AddNew();

			var allTransportModes = typeof(Constants.TransportModes).GetFields().Select(fieldInfo => (string)fieldInfo.GetValue(null));
			foreach (string transportMode in allTransportModes)
			{
				container.JC_TrainWagonNumber = "4815162342";
				consol.JK_TransportMode = transportMode;
				AssertEquals("Should be empty for non-rail", transportMode == Constants.TransportModes.Rail ? "4815162342" : "", container.JC_TrainWagonNumber);
			}
		}

		#endregion

		public void TestGetGlobalManifestHeaders()
		{
			var consol = Factory.New<CommonConsol>();
			var asycudaManifestHeader1 = Factory.New<Enterprise.Integration.Customs.ASYCUDA.IAsycudaManifestHeader>();
			((BusinessObject)asycudaManifestHeader1).FillWithValidTestData();
			asycudaManifestHeader1.AMA_ParentId = consol.PK;
			asycudaManifestHeader1.AMA_ParentTableCode = consol.TablePrefix;
			var asycudaManifestHeader2 = Factory.New<Enterprise.Integration.Customs.ASYCUDA.IAsycudaManifestHeader>();
			((BusinessObject)asycudaManifestHeader2).FillWithValidTestData();
			asycudaManifestHeader2.AMA_ParentId = consol.PK;
			asycudaManifestHeader2.AMA_ParentTableCode = consol.TablePrefix;
			AssertContainsExactElementsInAnyOrder(new Enterprise.Integration.Customs.ASYCUDA.IAsycudaManifestHeader[] { asycudaManifestHeader1, asycudaManifestHeader2 }, consol.GetGlobalManifestHeaders());
		}

		#endregion

		#region Notes

		public void TestBusinessObjectsWithRelatedNotes()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			AssertEquals("declaration.BusinessObjectsWithRelatedNotes.Count", 0, consol.BusinessObjectsWithRelatedNotes.Length);
			consol.JK_OA_SendingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			AssertEquals("declaration.BusinessObjectsWithRelatedNotes.Count", 1, consol.BusinessObjectsWithRelatedNotes.Length);
			consol.JK_OA_ReceivingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			AssertEquals("declaration.BusinessObjectsWithRelatedNotes.Count", 2, consol.BusinessObjectsWithRelatedNotes.Length);
			consol.JK_OA_CreditorAddress = Factory.New<OrgHeader>().MainAddress.PK;
			AssertEquals("declaration.BusinessObjectsWithRelatedNotes.Count", 3, consol.BusinessObjectsWithRelatedNotes.Length);
			consol.JK_OA_ShippingLineAddress = Factory.New<OrgHeader>().MainAddress.PK;
			AssertEquals("declaration.BusinessObjectsWithRelatedNotes.Count", 4, consol.BusinessObjectsWithRelatedNotes.Length);
			consol.Containers.AddNew();
			AssertEquals("declaration.BusinessObjectsWithRelatedNotes.Count", 5, consol.BusinessObjectsWithRelatedNotes.Length);
		}

		public void TestNoteContextsForRelatedNotes()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = "";
			Assert("Always have Forwarding module", (GetNoteContextsForRelatedNotesFromConsol(consol).Module & StmNoteContextModule.F) != 0);
			Assert("Always have 'CommonShipment and Declaration' module option", (GetNoteContextsForRelatedNotesFromConsol(consol).Module & StmNoteContextModule.E) != 0);
			Assert("Not attached to declaration yet", (GetNoteContextsForRelatedNotesFromConsol(consol).Module & StmNoteContextModule.D) == 0);
			Assert("Not be air yet", (GetNoteContextsForRelatedNotesFromConsol(consol).FreightMode & StmNoteContextFreightMode.I) == 0);

			consol.JK_TransportMode = Constants.TransportModes.Air;
			Assert("Should be air", (GetNoteContextsForRelatedNotesFromConsol(consol).FreightMode & StmNoteContextFreightMode.I) != 0);

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUMEL";

			Assert("Not Import or Export", (GetNoteContextsForRelatedNotesFromConsol(consol).Direction & StmNoteContextDirection.I) == 0);
			Assert("Not Import or Export", (GetNoteContextsForRelatedNotesFromConsol(consol).Direction & StmNoteContextDirection.E) == 0);

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			Assert("Export Only", (GetNoteContextsForRelatedNotesFromConsol(consol).Direction & StmNoteContextDirection.I) == 0);
			Assert("Export Only", (GetNoteContextsForRelatedNotesFromConsol(consol).Direction & StmNoteContextDirection.E) != 0);

			consol.JK_RL_NKLoadPort = "UAIEV";
			consol.JK_RL_NKDischargePort = "AUSYD";

			Assert("Import Only", (GetNoteContextsForRelatedNotesFromConsol(consol).Direction & StmNoteContextDirection.E) == 0);
			Assert("Import Only", (GetNoteContextsForRelatedNotesFromConsol(consol).Direction & StmNoteContextDirection.I) != 0);

			CommonShipment shipment = consol.Shipments.AddNew();
			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
			Assert("Should be attached to declaration", (GetNoteContextsForRelatedNotesFromConsol(consol).Module & StmNoteContextModule.D) != 0);

			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDestination = "NZAKL";

			Assert("Import and Export", (GetNoteContextsForRelatedNotesFromConsol(consol).Direction & StmNoteContextDirection.I) != 0);
			Assert("Import and Export", (GetNoteContextsForRelatedNotesFromConsol(consol).Direction & StmNoteContextDirection.E) != 0);
		}

		public void TestNoteContextsForRelatedNotes_ForSeaFreightMode()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			Assert("Should be sea", (GetNoteContextsForRelatedNotesFromConsol(consol).FreightMode & StmNoteContextFreightMode.S) != 0);

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			Assert("Export Only", (GetNoteContextsForRelatedNotesFromConsol(consol).Direction & StmNoteContextDirection.I) == 0);
			Assert("Export Only", (GetNoteContextsForRelatedNotesFromConsol(consol).Direction & StmNoteContextDirection.E) != 0);

			consol.JK_ConsolMode = "FCL";
			Assert("Freight mode should include FCL", (GetNoteContextsForRelatedNotesFromConsol(consol).FreightMode & StmNoteContextFreightMode.F) != 0);

			consol.JK_ConsolMode = "LCL";
			Assert("Feight mode should include LCL", (GetNoteContextsForRelatedNotesFromConsol(consol).FreightMode & StmNoteContextFreightMode.L) != 0);
		}

		StmNoteContexts GetNoteContextsForRelatedNotesFromConsol(CommonConsol consol)
		{
			PropertyInfo info = typeof(CommonConsol).GetProperty("NoteContextsForRelatedNotes", BindingFlags.NonPublic | BindingFlags.Instance);

			return (StmNoteContexts)info.GetValue(consol, null);
		}

		public void TestRelatedNotesDoesNotIncludeShipmentNotesOrShipmentRelatedNotes()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			StmNote orgNote = org.Notes.AddNew();

			ShipmentWithRelatedNotes shipment = Factory.New<ShipmentWithRelatedNotes>();
			StmNote shipmentNote = shipment.Notes.AddNew();

			shipment.ConsigneePK = org.PK;

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.Shipments.Add(shipment);

			Assert("Precondition - Shipment.BusinessObjectsWithRelatedNotes should contain our Org.", new ArrayList(shipment.BusinessObjectsWithRelatedNotes).Contains(org));
			AssertNull("Consol should not contain CommonShipment Note as a related note.", consol.Notes.VisibleNotes.FindByPK(shipmentNote.PK));
			AssertNull("Consol should not contain Shipment's Related Notes as the Consol's related notes.", consol.Notes.VisibleNotes.FindByPK(orgNote.PK));
		}

		class ShipmentWithRelatedNotes : CommonShipment
		{
			public ShipmentWithRelatedNotes(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override BusinessObject[] BusinessObjectsWithRelatedNotes
			{
				get { return new BusinessObject[] { Consignor, Consignee }; }
			}

			protected override ConsolCollection GetNewConsolCollection()
			{
				return new ConsolCollection(this);
			}
		}

		#endregion

		#region ICDArchive

		public void TestCDJobNumber()
		{
			Consol.JK_UniqueConsignRef = "ABC123";
			AssertEquals("Consol's unique consign ref should become the job number", "ABC123", Consol.CDArchiveInfo.JobNumber);

			Consol.JK_UniqueConsignRef = "DEF456";
			AssertEquals("Consol's unique consign ref should become the job number", "DEF456", Consol.CDArchiveInfo.JobNumber);
		}

		public void TestCDHouseBill()
		{
			AssertEquals("Housebill property should be empty on Consol", ZString.Empty, Consol.CDArchiveInfo.HouseBill);
		}

		public void TestCDMasterBill()
		{
			Consol.JK_MasterBillNum = "11223344";
			AssertEquals("Master bill number", "11223344", Consol.CDArchiveInfo.MasterBill);
		}

		public void TestCDContainerNumbers()
		{
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_ContainerNum = "12345";

			AssertEquals("Container numbers", "12345", Consol.CDArchiveInfo.ContainerNumbers);

			CommonContainer anotherContainer = Consol.Containers.AddNew();
			anotherContainer.JC_ContainerNum = "67890";
			AssertEquals("Container numbers", "12345, 67890", Consol.CDArchiveInfo.ContainerNumbers);
		}

		public void TestCDETA()
		{
			ZDateTime firstOfJan = new ZDateTime(2005, 01, 01);
			Consol.Transports[0].JW_ETA = firstOfJan;
			AssertEquals("ETA date", firstOfJan, Consol.CDArchiveInfo.ETA);
		}

		public void TestCDETD()
		{
			ZDateTime firstOfJan = new ZDateTime(2005, 01, 01);
			Consol.Transports[0].JW_ETD = firstOfJan;
			AssertEquals("ETD date", firstOfJan, Consol.CDArchiveInfo.ETD);
		}

		public void TestCDVessel()
		{
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "ABC Vessel";
			Consol.Transports[0].JW_Vessel = vessel.RV_FK;
			AssertEquals("Vessel", "ABC Vessel", Consol.CDArchiveInfo.Vessel);
		}

		public void TestCDVoyageFlight()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "Flight22";
			Consol.Transports[0].JW_VoyageFlight = "Flight22";
			AssertEquals("Voyage number", "Flight22", Consol.CDArchiveInfo.VoyageFlight);
		}

		public void TestCDEntryNumber()
		{
			var shipment1 = Factory.New<CommonShipment>();

			shipment1.JS_RL_NKOrigin = "USLAX";
			shipment1.JS_RL_NKDestination = "AUSYD";

			var shipment2 = Factory.New<CommonShipment>();

			shipment2.JS_RL_NKOrigin = "USLAX";
			shipment2.JS_RL_NKDestination = "AUSYD";

			var declaration1 = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration1.JE_JS = shipment1.PK;

			var declaration2 = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration2.JE_JS = shipment2.PK;

			Consol.Shipments.Add(shipment1);
			Consol.Shipments.Add(shipment2);

			var declarationNumberProperty1 = declaration1.GetType().GetProperty("DeclarationNumber");
			declarationNumberProperty1.SetValue(declaration1, new ZString("en1"));

			var declarationNumberProperty2 = declaration2.GetType().GetProperty("DeclarationNumber");
			declarationNumberProperty2.SetValue(declaration2, new ZString("en2"));

			AssertEquals("EntryNumber", "en1, en2", Consol.CDArchiveInfo.EntryNumber);
		}

		public void TestCDOrigin()
		{
			Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AssertEquals("Origin", "AUSYD", Consol.CDArchiveInfo.Origin);
		}

		public void TestCDDestination()
		{
			Consol.Transports[0].JW_RL_NKDiscPort = "USLAX";
			AssertEquals("Destination", "USLAX", Consol.CDArchiveInfo.Destination);
		}

		public void TestCDConsigneeCode()
		{
			var consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			Consol.JK_OA_ReceivingForwarderAddress = consignee.MainAddress.PK;
			AssertEquals("Consignee Code", consignee.OH_Code, Consol.CDArchiveInfo.ConsigneeCode);
		}

		public void TestCDConsignorCode()
		{
			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			Consol.JK_OA_SendingForwarderAddress = consignor.MainAddress.PK;
			AssertEquals("Consignor Code", consignor.OH_Code, Consol.CDArchiveInfo.ConsignorCode);
		}

		#endregion

		#region ICancellable

		public void TestCanCancelConsolWithShipments()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			Assert("Expected no related shipments", consol.Shipments.Count == 0);
			Assert("Expected not to cancel console without shipments", string.IsNullOrEmpty(consol.CanCancel()));

			CommonShipment shipment = consol.Shipments.AddNew();

			Assert("Expected 1 related shipment", consol.Shipments.Count == 1);
			Assert("Expected cancel console with shipments", !string.IsNullOrEmpty(shipment.CanCancel()));

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			shipment.Consols.Remove(consol);

			Assert("Expected no related shipments", shipment.Consols.Count == 0);
			Assert("Expected not to cancel console without shipments", string.IsNullOrEmpty(shipment.CanCancel()));
		}

		public void TestCanCancel_GatewayConsolWithCharges()
		{
			var gatewayConsol = Factory.NewWithValidTestData<CommonConsol>();
			gatewayConsol.JK_ReceivingForwarderHandlingType = "GTA";

			Assert("Can cancel gateway consol without charge", string.IsNullOrEmpty(gatewayConsol.CanCancel()));

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = gatewayConsol.PK;
			job.JH_ParentTableCode = "JK";
			job.JH_JobNum = "C00001234";

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_OSCostAmt = 5m;
			charge.JR_OSCostExRate = 1m;
			charge.JR_OSSellAmt = 0m;
			Factory.Save();

			var expectedMessage = $@"{gatewayConsol.HumanReadableName.ToString()} cannot be deactivated.
Job Invoicing Charge(s) have been saved against this Invoicing Job Header (C00001234) in the company EDI.";

			AssertEquals("Can Cancel should return message as gateway consol has a charge attached", expectedMessage, gatewayConsol.CanCancel());
		}

		#endregion

		#region Related PackLines

		public void TestRefreshRelatedPackLines()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_RL_NKLoadPort = "NZAKL";
			consol.Transports[0].JW_RL_NKDiscPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			CommonContainer newContainer = consol.Containers.AddNew();
			newContainer.JC_ContainerNum = "GFCU0292036";

			OrgHeader forwarderClient = Factory.New<OrgHeader>();
			forwarderClient.OH_FullName = "Test Forwarder";
			forwarderClient.MainAddress.OA_Address1 = "Forwarders Address";
			forwarderClient.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			consol.JK_OA_ReceivingForwarderAddress = forwarderClient.MainAddress.PK;
			newContainer.JC_OH_CFSClient = forwarderClient.PK;

			AssertEquals("Shipments.Count == 0", 0, consol.Shipments.Count);
			AssertEquals("RelatedPackLines.Count == 0", 0, consol.RelatedPackLines.Count);

			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertEquals("OuterPackLines.Count == 0", 0, shipment.OuterPackLines.Count);

			PackLine line = shipment.OuterPackLines.AddNew();
			AssertEquals("OuterPackLines.Count == 1", 1, shipment.OuterPackLines.Count);
			AssertEquals("Shipments.Count == 0", 0, consol.Shipments.Count);
			AssertEquals("RelatedPackLines.Count == 0", 0, consol.RelatedPackLines.Count);

			consol.Shipments.Add(shipment);
			AssertEquals("Shipments.Count == 1", 1, consol.Shipments.Count);
			AssertEquals("RelatedPackLines.Count == 1", 1, consol.RelatedPackLines.Count);

			consol.Shipments.Remove(shipment);
			AssertEquals("Shipments.Count == 0", 0, consol.Shipments.Count);
			AssertEquals("RelatedPackLines.Count == 0", 0, consol.RelatedPackLines.Count);
		}

		public void TestRefreshRelatedPackLinesFromSubShipmentPacklines()
		{
			var consol = Factory.New<CommonConsol>();
			consol.Containers.AddNew();
			consol.Containers[0].JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			consol.Containers[0].JC_ContainerNum = "ABCD1212112";

			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			CommonShipment subShipment = masterShipment.CoLoadShipments.AddNew();
			var subPackLine1 = subShipment.OuterPackLines.AddNew();
			var subPackLine2 = subShipment.OuterPackLines.AddNew();

			consol.Shipments.Add(masterShipment);
			AssertEquals("PackLines from subshipments should be packed on consol", 2, consol.RelatedPackLines.Count);

			var subPackLine3 = subShipment.OuterPackLines.AddNew();
			var subPackLine4 = subShipment.OuterPackLines.AddNew();
			AssertEquals(4, consol.RelatedPackLines.Count);

			subShipment.OuterPackLines.RemoveAndDelete(subPackLine4);
			AssertEquals(3, consol.RelatedPackLines.Count);

			consol.Shipments.RemoveAndDelete(masterShipment);
			AssertEquals(3, consol.RelatedPackLines.Count);
		}

		public void TestRefreshRelatedPackLinesFromSubShipmentsOfMasterShipments()
		{
			var consol = Factory.New<CommonConsol>();
			consol.Containers.AddNew();
			consol.Containers[0].JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			consol.Containers[0].JC_ContainerNum = "ABCD1212112";

			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			var tempPack = masterShipment.OuterPackLines.AddNew();

			var subShipment1 = masterShipment.CoLoadShipments.AddNew();
			var subPackLine1A = subShipment1.OuterPackLines.AddNew();
			var subPackLine1B = subShipment1.OuterPackLines.AddNew();
			var subPackLine1C = subShipment1.OuterPackLines.AddNew();

			subPackLine1A.JL_PackageCount = 1;
			subPackLine1B.JL_PackageCount = 1;
			subPackLine1C.JL_PackageCount = 1;

			var subShipment2 = masterShipment.CoLoadShipments.AddNew();
			var subPackLine2 = subShipment2.OuterPackLines.AddNew();
			subPackLine2.JL_PackageCount = 1;

			var unrelatedShipment = Factory.New<CommonShipment>();
			unrelatedShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			var unrelatedPackLine1 = unrelatedShipment.OuterPackLines.AddNew();
			var unrelatedPackLine2 = unrelatedShipment.OuterPackLines.AddNew();

			unrelatedPackLine1.JL_PackageCount = 1;
			unrelatedPackLine2.JL_PackageCount = 1;

			AssertEquals("PackLines from subshipments should be packed on consol", 0, consol.RelatedPackLines.Count);

			consol.Shipments.Add(masterShipment);
			AssertEquals("PackLines from subshipments should be packed on consol", 4, consol.RelatedPackLines.Count);
			AssertEquals("No packlines should be unallocated", 0, consol.UnAllocatedPackLines.Count);

			consol.Shipments.Add(unrelatedShipment);
			AssertEquals("One mastershipment, two subshipments and a other shipment should be attached to consol", 4, consol.Shipments.Count);
			AssertEquals("Packlines should be added to consol", 6, consol.RelatedPackLines.Count);
			AssertEquals("All packlines should still be allocated", 0, consol.UnAllocatedPackLines.Count);
			Factory.Save();

			consol.Shipments.Remove(masterShipment);
			AssertEquals("Consol should only remove the master shipment, not it's subshipments", 3, consol.Shipments.Count);
			AssertEquals("All packlines should remain on consol", 6, consol.RelatedPackLines.Count);
			AssertEquals("All packlines should still be allocated", 0, consol.UnAllocatedPackLines.Count);

			consol.Shipments.Remove(subShipment1);
			AssertEquals("SubShipment2 and unrelatedShipment packlines should remain on consol", 3, consol.RelatedPackLines.Count);
			AssertEquals("All packlines should still be allocated", 0, consol.UnAllocatedPackLines.Count);
		}

		#endregion

		#region MostInterestingTransportDescription

		public void TestMostInterestingTransportDescription()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			Transport transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Voyage", consol.MostInterestingTransportDescription);

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Voyage (Departure Leg)", consol.MostInterestingTransportDescription);

			transport1.JW_TransportMode = Constants.TransportModes.Air;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Flight (First 'AIR' Leg)", consol.MostInterestingTransportDescription);

			consol.Transports[1].Delete();
			AssertEquals("Flight", consol.MostInterestingTransportDescription);

			transport1.JW_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Journey", consol.MostInterestingTransportDescription);

			Transport transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Journey (Departure Leg)", consol.MostInterestingTransportDescription);

			consol.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Journey (First 'ROA' Leg)", consol.MostInterestingTransportDescription);
		}

		public void TestMostInterestingTransportDescription_WhenTransportModeIsIWT()
		{
			var consol = Factory.New<CommonConsol>();
			var transport1 = consol.Transports[0];

			transport1.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			AssertEquals("Voyage", consol.MostInterestingTransportDescription);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			AssertEquals("Voyage (Departure Leg)", consol.MostInterestingTransportDescription);

			consol.Transports[1].Delete();

			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.HomePort.Code;
			AssertEquals("Voyage (Arrival Leg)", consol.MostInterestingTransportDescription);
		}

		public void TestMostInterestingTransportDescriptionWithNullTransport()
		{
			var mockConsol = Factory.NewMoq<CommonConsol>();

			mockConsol.Setup(m => m.Transports).Returns((ConsolTransportCollection)null);

			AssertEquals(string.Empty, mockConsol.Object.MostInterestingTransportDescription);
		}

		#endregion

		#region Transport Language

		public void TestMostInterestingTransportDescriptionLanguage()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				var resKey_Flight = "Freight.TransportDescription.Flight";
				var resKey_Voyage = "Freight.TransportDescription.Voyage";
				var resKey_LegInfo = "99b3e4de-3db6-419b-9161-11a3d0409fab";

				mockRes.Put(resKey_Flight, new ResourceStringData(resKey_Flight, "Flug"));
				mockRes.Put(resKey_Voyage, new ResourceStringData(resKey_Voyage, "Reise"));
				mockRes.Put(resKey_LegInfo, new ResourceStringData(resKey_LegInfo, "Letzte '{0}' Strecke"));

				consol.JK_RL_NKLoadPort = "USCHI";
				consol.JK_RL_NKDischargePort = "AUBNE";
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.Transports.AddNew();

				AssertEquals("Flug (Letzte 'AIR' Strecke)", consol.MostInterestingTransportDescription);
			}

			AssertEquals("Flight (Last 'AIR' Leg)", consol.MostInterestingTransportDescription);

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports.AddNew();

			AssertEquals("Voyage (Last 'SEA' Leg)", consol.MostInterestingTransportDescription);

			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.German))
			{
				AssertEquals("Reise (Letzte 'SEA' Strecke)", consol.MostInterestingTransportDescription);
			}
		}

		#endregion

		#region Test Suspending Shipments Events

		public void TestSuspendingShipmentsEvents()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "CONSOL1";

			var shipmentA = Factory.New<CommonShipment>();
			shipmentA.JS_UniqueConsignRef = "SHIPMENTA";
			shipmentA.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var shipmentB = Factory.New<CommonShipment>();
			shipmentB.JS_UniqueConsignRef = "SHIPMENTB";
			shipmentB.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			consol.Shipments.AddRange(shipmentA, shipmentB);

			Factory.Save();

			int numberOfShipmentsCountChanged = 0;
			int numberOfShipmentsForTotalingCountChanged = 0;

			consol.Shipments.CountChanged += (s, e) => { numberOfShipmentsCountChanged++; };

			consol.ShipmentsForTotalling.CountChanged += (s, e) => { numberOfShipmentsForTotalingCountChanged++; };

			consol.Shipments.Remove(shipmentA);
			consol.Shipments.Remove(shipmentB);

			var shipmentC = Factory.New<CommonShipment>();
			shipmentB.JS_UniqueConsignRef = "SHIPMENTB";
			shipmentB.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			consol.Shipments.Add(shipmentC);

			AssertEquals(3, numberOfShipmentsCountChanged);
			AssertEquals(3, numberOfShipmentsForTotalingCountChanged);

			consol.Shipments.Add(shipmentA);
			consol.Shipments.Add(shipmentB);
			consol.Shipments.Remove(shipmentC);

			numberOfShipmentsCountChanged = 0;
			numberOfShipmentsForTotalingCountChanged = 0;
			using (consol.SuspendShipmentsCountChanged())
			{
				consol.Shipments.Remove(shipmentA);
				consol.Shipments.Remove(shipmentB);
				shipmentC = Factory.New<CommonShipment>();
				shipmentB.JS_UniqueConsignRef = "SHIPMENTB";
				shipmentB.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
				consol.Shipments.Add(shipmentC);
			}

			AssertEquals(0, numberOfShipmentsCountChanged);
			AssertEquals(0, numberOfShipmentsForTotalingCountChanged);
		}

		public void TestShipmentsForTotallingAfterSuspendingShipmentsEvents()
		{
			var consol = FreightTestHelper.GetConsol<CommonConsol>("CON", Factory);
			CommonShipment sTD, bCN_SUB, aSM_MAS, aSM_SUB, cLD_SUB;
			using (consol.SuspendShipmentsCountChanged())
			{
				sTD = FreightTestHelper.GetShipment<CommonShipment>("STD", Constants.ShipmentTypes.StandardHouse, Factory);
				sTD.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
				sTD.JS_OuterPacks = 100;
				sTD.JS_ActualWeight = 200;
				sTD.JS_ActualVolume = 300;
				sTD.JS_LoadingMeters = 4m;

				sTD.Consols.Add(consol);

				var bCN_MAS = FreightTestHelper.GetShipment<CommonShipment>("BCN_MAS", Constants.ShipmentTypes.BuyersConsolLead, Factory);
				bCN_MAS.JS_OuterPacks = 11;
				bCN_MAS.JS_ActualWeight = 22;
				bCN_MAS.JS_ActualVolume = 33;

				bCN_SUB = FreightTestHelper.GetShipment("BCN_SUB", bCN_MAS, Constants.ShipmentTypes.StandardHouse, Factory);
				bCN_SUB.JS_OuterPacks = 10;
				bCN_SUB.JS_ActualWeight = 20;
				bCN_SUB.JS_ActualVolume = 30;

				bCN_MAS.Consols.Add(consol);

				aSM_MAS = FreightTestHelper.GetShipment<CommonShipment>("ASM_MAS", Constants.ShipmentTypes.AssemblyMaster, Factory);
				aSM_MAS.JS_OuterPacks = 20;
				aSM_MAS.JS_ActualWeight = 40;
				aSM_MAS.JS_ActualVolume = 80;

				aSM_SUB = FreightTestHelper.GetShipment("ASM_SUB", aSM_MAS, Constants.ShipmentTypes.StandardHouse, Factory);
				aSM_SUB.JS_OuterPacks = 13;
				aSM_SUB.JS_ActualWeight = 17;
				aSM_SUB.JS_ActualVolume = 19;

				aSM_MAS.Consols.Add(consol);

				consol.Shipments.Remove(bCN_MAS);

				aSM_SUB.JS_JS_ColoadMasterShipment = ZGuid.Empty;

				var cLD_MAS = FreightTestHelper.GetShipment<CommonShipment>("CLD_MAS", Constants.ShipmentTypes.CoLoadMaster, Factory);
				cLD_MAS.JS_OuterPacks = 1000;
				cLD_MAS.JS_ActualWeight = 2000;
				cLD_MAS.JS_ActualVolume = 3000;

				cLD_SUB = FreightTestHelper.GetShipment("CLD_SUB", cLD_MAS, Constants.ShipmentTypes.StandardHouse, Factory);
				cLD_SUB.JS_OuterPacks = 10000;
				cLD_SUB.JS_ActualWeight = 20000;
				cLD_SUB.JS_ActualVolume = 30000;

				cLD_MAS.Consols.AddNew();
				cLD_SUB.Consols.Add(consol);
			}

			FreightTestHelper.AssertShipmentCollection("Shipments", consol.Shipments, sTD, bCN_SUB, aSM_MAS, aSM_SUB, cLD_SUB);
			FreightTestHelper.AssertShipmentCollection("ShipmentsForTotalling", consol.ShipmentsForTotalling, sTD, bCN_SUB, aSM_MAS, aSM_SUB, cLD_SUB);
			AssertEquals("Total Shipment Quantity", (ZDecimal)10143, consol.JK_TotalShipmentQuantity);
			AssertEquals("Total Shipment Weight", (ZDecimal)20277, consol.JK_TotalShipmentWeight);
			AssertEquals("Total Shipment Volume", (ZDecimal)30429, consol.JK_TotalShipmentVolume);
		}

		public void TestRefreshRelatedPackLinesAfterSuspendingShipmentsEvents()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_RL_NKLoadPort = "NZAKL";
			consol.Transports[0].JW_RL_NKDiscPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			CommonContainer newContainer = consol.Containers.AddNew();
			newContainer.JC_ContainerNum = "GFCU0292036";

			OrgHeader forwarderClient = Factory.New<OrgHeader>();
			forwarderClient.OH_FullName = "Test Forwarder";
			forwarderClient.MainAddress.OA_Address1 = "Forwarders Address";
			forwarderClient.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			consol.JK_OA_ReceivingForwarderAddress = forwarderClient.MainAddress.PK;
			newContainer.JC_OH_CFSClient = forwarderClient.PK;

			using (consol.SuspendShipmentsCountChanged())
			{
				CommonShipment shipment1 = Factory.New<CommonShipment>();

				PackLine line1 = shipment1.OuterPackLines.AddNew();

				consol.Shipments.Add(shipment1);

				consol.Shipments.Remove(shipment1);

				CommonShipment shipment2 = Factory.New<CommonShipment>();
				PackLine line2 = shipment2.OuterPackLines.AddNew();
				PackLine line3 = shipment2.OuterPackLines.AddNew();

				consol.Shipments.Add(shipment2);

				CommonShipment shipment3 = Factory.New<CommonShipment>();
				PackLine line4 = shipment3.OuterPackLines.AddNew();

				consol.Shipments.Add(shipment3);
			}

			AssertEquals(2, consol.Shipments.Count);
			AssertEquals(3, consol.RelatedPackLines.Count);
		}

		#endregion

		#region IsAttachedToStandAloneShipment

		public void TestClearConsolIsAttachedToStandAloneShipmentAfterFactoryIsSaved()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_UniqueConsignRef = "ConsolRef";

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "ShipmentRef";

			shipment.Consols.Add(consol);
			Factory.Save();

			consol.IsAttachedToStandAloneShipment = true;
			Factory.Save();

			AssertEquals(false, consol.IsAttachedToStandAloneShipment);
		}

		#endregion

		#region Job Header Deactivation Tests

		public void TestJobHeaderIsNotDeactivatedWhenFactoryHasInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(true);
		}

		public void TestJobHeaderIsDeactivatedWhenFactoryDoesNotHaveInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(false);
		}

		void AssertJobHeaderDeactivation(bool hasInvoicingPlugInGUIContext)
		{
			var consol = Factory.New<CommonConsolForTest>();
			var jobLoader = new JobHeader.Loader(consol);
			var job = jobLoader.TryCreate();
			Factory.Save();

			consol.IsCancelled = true;
			if (hasInvoicingPlugInGUIContext)
			{
				Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			}

			var assertionMessage1 = string.Format("consol {0} have InvoicingPluginGUI Business Context", hasInvoicingPlugInGUIContext ? "should" : "should not");
			var assertionMessage2 = string.Format("Job {0} be deleted by OnSaving method", hasInvoicingPlugInGUIContext ? "should not" : "should");

			Assert("Deactivating consol, IsCancelled flag should be set to true", consol.IsCancelled);
			Assert("Deactivating consol, IsCancelledInfo should have changes", consol.IsCancelledHasChanged);
			AssertEquals(assertionMessage1, hasInvoicingPlugInGUIContext, consol.HasContext(BusinessContext.InvoicingPlugInGUI));
			Assert("Job is not yet Cancelled", !job.IsCancelled);

			Factory.Save();

			AssertEquals(assertionMessage2, !hasInvoicingPlugInGUIContext, job.IsCancelled);
		}

		#endregion

		#region Job Delete

		public void TestReloadJobWhenJobIsDeleted()
		{
			var consol = GetNewConsol();
			if (consol is IJobHeaderParent)
			{
				Factory.Save();

				var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job.JH_ParentID = consol.PK;
				job.JH_ParentTableCode = "JK";
				job.JH_JobNum = "C00001234";

				_ = consol.Job;

				job.Delete();

				AssertNull(consol.Job);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region JK_JX_JV_AircraftType

		public void TestJK_JX_JV_AircraftType()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals(ZString.Empty, consol.JK_JX_JV_AircraftType);

			var transport1 = consol.Transports[0];
			transport1.JW_AircraftType = "331";
			transport1.JW_TransportMode = Constants.TransportModes.Air;
			AssertEquals("331", consol.JK_JX_JV_AircraftType);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_AircraftType = "332";
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			AssertEquals("331", consol.JK_JX_JV_AircraftType);

			consol.Transports[0].Delete();
			AssertEquals("332", consol.JK_JX_JV_AircraftType);
		}

		#endregion

		#region JK_JX_JB_E_LastARV

		public void TestJK_JX_JB_E_LastARV()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var arrivalDate1 = ZDateTime.Now;
			var transport1 = consol.Transports.AddNew();
			transport1.JW_ETA = arrivalDate1;
			AssertEquals(consol.JK_JX_JB_E_LastARV, transport1.JW_ETA);

			var arrivalDate2 = ZDateTime.Now.AddDays(1);
			var transport2 = consol.Transports.AddNew();
			transport2.JW_ETA = arrivalDate2;
			AssertEquals(transport2.JW_ETA, consol.JK_JX_JB_E_LastARV);
		}

		public void TestJK_JX_JB_E_LastARV_WithDifferentTransportMode()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var arrivalDate1 = ZDateTime.Now;
			var transport1 = consol.Transports.AddNew();
			transport1.JW_ETA = arrivalDate1;
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(ZDateTime.Empty, consol.JK_JX_JB_E_LastARV);
		}

		#endregion

		#region Advance Cargo Reporting Self-Filer

		public void TestIsAdvanceCargoReportingSelfFiler()
		{
			var consol = Factory.New<CommonConsolForTest>();
			Assert(!consol.IsAdvanceCargoReportingSelfFiler);

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			var misc = receivingForwarder.MiscServ;
			misc.OM_FWAdvanceCargoReportingSelfFiler = true;
			Assert(consol.IsAdvanceCargoReportingSelfFiler);

			misc.OM_FWAdvanceCargoReportingSelfFiler = false;
			Assert(!consol.IsAdvanceCargoReportingSelfFiler);
		}

		#endregion

		#region eHub Interchange Reference

		public void TestEHubInterchangeReferenceNumberCannotBeModifiedOrDeleted()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryType = CusEntryNumLookups.HIR;
			entryNum.CE_EntryNum = "12345";

			Factory.Save();

			var reloadedConsol = new BusinessObjectFactory().Load<CommonConsol>(consol.PK);
			var reloadedEntryNum = reloadedConsol.Numbers.OfType<CusEntryNumber>().First(n => n.CE_EntryType == CusEntryNumLookups.HIR);
			Assert("HIR is read-only", reloadedEntryNum.ReadOnly);
			Assert("HIR cannot be deleted", !reloadedEntryNum.CanDelete);
			AssertEquals("The HIR is system generated and cannot be deleted.", reloadedEntryNum.ReasonForNotAbleToDelete);
		}

		#endregion

		#region Test Action Field Attribute

		public void TestActionFieldAttribute_JK_RL_NKLastForeignPort()
		{
			var lastForeignPortInfo = typeof(CommonConsol).GetProperty(nameof(CommonConsol.JK_RL_NKLastForeignPort));
			var actionFieldAttribute = ActionFieldAttribute.Get(lastForeignPortInfo);

			AssertNotNull(actionFieldAttribute);
			AssertEquals("JK_RL_NKLastForeignPort should be configured to be settable via workflow.", false, actionFieldAttribute.ReadOnly);
		}

		#endregion

		#region CommonConsolForBaseTest

		class CommonConsolForBaseTest : CommonConsol
		{
			public CommonConsolForBaseTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			public ZDecimal BaseJK_CorrectedConsolVolume => new ZDecimal(GetValueFromRowSafely(JobConsolSchema.JK_CorrectedConsolVolume));

			public ZDecimal BaseJK_CorrectedConsolWeight => new ZDecimal(GetValueFromRowSafely(JobConsolSchema.JK_CorrectedConsolWeight));
		}

		#endregion

		#region Implementation
		UnmatchOrgRecord CreditorRec
		{
			get
			{
				if (creditorRec == null)
				{
					creditorRec = UnmatchOrgRecordTestHelper.CreateUnmatchOrgRecord(OrganisationTypes.Creditor,
						nameof(OrganisationTypes.Creditor),
						"creditor addr 1",
						"creditor addr 2",
						"creditorName",
						"3333",
						"VIC",
						"melbourne",
						"creditor",
						"cownerCode",
						"");
				}
				return creditorRec;
			}
		}
		UnmatchOrgRecord creditorRec;

		UnmatchOrgRecord CarrierRec
		{
			get
			{
				if (carrierRec == null)
				{
					carrierRec = UnmatchOrgRecordTestHelper.CreateUnmatchOrgRecord(OrganisationTypes.Carrier,
						nameof(OrganisationTypes.Carrier),
						"carrier addr 1",
						"carrier addr 2",
						"carrierName",
						"4444",
						"VIC",
						"melbourne",
						"carrier",
						"caownerCode",
						"");
				}
				return carrierRec;
			}
		}
		UnmatchOrgRecord carrierRec;

		readonly ZString AUSYDLoco = "AUSYD";
		readonly ZString USLAXLoco = "USLAX";

		CommonShipment CreateShipment(ZString transportMode, ZDecimal actualWeight, ZDecimal actualVolume, decimal freightRate = 0m, bool isCollect = true)
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ActualWeight = actualWeight;
			shipment.JS_ActualVolume = actualVolume;
			shipment.JS_TransportMode = transportMode;
			shipment.JS_UnitFreightRate = freightRate;
			shipment.JS_RX_NKFrtRateCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			shipment.JS_INCO = isCollect ? Constants.IncoTerms.FreeOnBoard : Constants.IncoTerms.CostInsuranceAndFreight;

			return shipment;
		}

		CommonShipment AddShipment(CommonConsol relatedConsol, ZDecimal actualWeight, ZDecimal actualVolume, decimal freightRate = 0m, bool isCollect = true)
		{
			var shipment = CreateShipment(consol.JK_TransportMode, actualWeight, actualVolume, freightRate, isCollect);
			relatedConsol.Shipments.Add(shipment);

			return shipment;
		}

		protected virtual CommonConsol GetNewConsol()
		{
			return Factory.New<CommonConsol>();
		}

		void AssertCustomsManifestVisibilityChanged(CommonConsol consol, ZPropertyInfo propertyInfo, ZString valueToFireEvent, ZString valueToCheckEventUnhooked)
		{
			visibilityChanged = false;
			IManifestProvider manifestProvider = consol;
			manifestProvider.CustomsManifestVisibilityChanged += new EventHandler(OnManifestProvider_CustomsManifestVisibilityChanged);

			CombineAssertions(() =>
			{
				propertyInfo.Value = valueToFireEvent;
				AssertEquals("Visibility should change", true, visibilityChanged);

				manifestProvider.CustomsManifestVisibilityChanged -= new EventHandler(OnManifestProvider_CustomsManifestVisibilityChanged);
				visibilityChanged = false;

				propertyInfo.Value = valueToCheckEventUnhooked;
				AssertEquals("Visibility should NOT change for any reason after the event handler is unhooked", false, visibilityChanged);
			});
		}

		bool visibilityChanged;

		void OnManifestProvider_CustomsManifestVisibilityChanged(object sender, EventArgs e)
		{
			visibilityChanged = true;
		}

		#endregion
	}
}
