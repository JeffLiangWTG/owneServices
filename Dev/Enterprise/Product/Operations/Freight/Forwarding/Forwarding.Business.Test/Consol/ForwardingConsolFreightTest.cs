using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using FluentAssertions;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using AWBSpecialHandlingCodeDescriptionPairList = Enterprise.Freight.Forwarding.AWB.Business.AWBSpecialHandlingCodeDescriptionPairList;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	class ForwardingConsolFreightTest : BaseFreightTest
	{
		#region GetNewValidation

		public virtual void TestGetNewValidation()
		{
			AssertEquals("Type of Validation", typeof(ForwardingConsolValidation), Consol.Validation.GetType());
		}

		#endregion

		#region Verification / Pre-Allocation Details

		public void TestPreAllocationUnits()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_TotalShipmentActVolumeCheck = 10m;
			consol.JK_TotalShipmentActWeightCheck = 1000m;
			consol.WeightVerificationUnit = "KG";
			consol.VolumeVerificationUnit = "M3";
			AssertEquals("KG", consol.JK_TotalShipmentChargeableUnit);
			AssertEquals("M3", consol.JK_TotalShipmentActOtherUnit);

			consol.WeightVerificationUnit = "LB";
			consol.VolumeVerificationUnit = "M3";
			AssertEquals("LB", consol.JK_TotalShipmentChargeableUnit);
			AssertEquals("M3", consol.JK_TotalShipmentActOtherUnit);

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("M3", consol.JK_TotalShipmentChargeableUnit);
			AssertEquals("LB", consol.JK_TotalShipmentActOtherUnit);

			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			ForwardingConsol reloadedConsol = anotherFactory.Load<ForwardingConsol>(consol.PK);
			AssertEquals("M3", reloadedConsol.JK_TotalShipmentChargeableUnit);
			AssertEquals("LB", reloadedConsol.JK_TotalShipmentActOtherUnit);
		}

		readonly List<string> agentCodeList = new List<string>
		{
			AgentStatusList.Codes.Appointed,
			AgentStatusList.Codes.GatewayAgent,
			AgentStatusList.Codes.GatewayAgentWithTariff,
			AgentStatusList.Codes.Handles,
			AgentStatusList.Codes.Published
		};

		public void TestPreAllocationPropertiesReadOnly_WithConsolAndGatewayConsolEditAllowed()
		{
			Env.Security.ConsolPreAllocationEditing.IsAllowed = true;
			Env.Security.GatewayConsolPreAllocationEditing.IsAllowed = true;

			AssertEquals("Precondition: user has right to edit pre-allocations in standard consol", true, Env.Security.ConsolPreAllocationEditing.IsAllowed);
			AssertEquals("Precondition: user has right to edit pre-allocations in gateway consol", true, Env.Security.GatewayConsolPreAllocationEditing.IsAllowed);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			GlbDepartment.CurrentDepartment.GE_Code = "FDA";
			using (new TemporaryUserContext { DepartmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid() }.Set())
			{
				foreach (var agentCode in agentCodeList)
				{
					consol.JK_SendingForwarderHandlingType = agentCode;

					Assert("Precondition: not gateway department", !GlbDepartment.CurrentDepartment.IsGatewayDepartment);
					AssertPreAllocationPropertiesReadOnly(consol, false);
				}
			}

			GlbDepartment.CurrentDepartment.GE_Code = "GDA";
			using (new TemporaryUserContext { DepartmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid() }.Set())
			{
				foreach (var agentCode in agentCodeList)
				{
					consol.JK_SendingForwarderHandlingType = agentCode;

					Assert("Precondition: gateway department", GlbDepartment.CurrentDepartment.IsGatewayDepartment);
					AssertPreAllocationPropertiesReadOnly(consol, false);
				}
			}
		}

		public void TestPreAllocationPropertiesReadOnly_WithConsolAndGatewayConsolEditDisallowed()
		{
			Env.Security.ConsolPreAllocationEditing.IsAllowed = false;
			Env.Security.GatewayConsolPreAllocationEditing.IsAllowed = false;

			AssertEquals("Precondition: user has no right to edit pre-allocations in standard consol", false, Env.Security.ConsolPreAllocationEditing.IsAllowed);
			AssertEquals("Precondition: user has no right to edit pre-allocations in gateway consol", false, Env.Security.GatewayConsolPreAllocationEditing.IsAllowed);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			GlbDepartment.CurrentDepartment.GE_Code = "FDA";
			using (new TemporaryUserContext { DepartmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid() }.Set())
			{
				foreach (var agentCode in agentCodeList)
				{
					consol.JK_SendingForwarderHandlingType = agentCode;

					Assert("Precondition: not gateway department", !GlbDepartment.CurrentDepartment.IsGatewayDepartment);
					AssertPreAllocationPropertiesReadOnly(consol, true);
				}
			}

			GlbDepartment.CurrentDepartment.GE_Code = "GDA";
			using (new TemporaryUserContext { DepartmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid() }.Set())
			{
				foreach (var agentCode in agentCodeList)
				{
					consol.JK_SendingForwarderHandlingType = agentCode;

					Assert("Precondition: gateway department", GlbDepartment.CurrentDepartment.IsGatewayDepartment);
					AssertPreAllocationPropertiesReadOnly(consol, true);
				}
			}
		}

		public void TestPreAllocationPropertiesReadOnly_WithConsolEditAllowedAndGatewayConsolDisallowed()
		{
			Env.Security.ConsolPreAllocationEditing.IsAllowed = true;
			Env.Security.GatewayConsolPreAllocationEditing.IsAllowed = false;

			var gatewayAgentCodeList = new List<string>
			{
				AgentStatusList.Codes.GatewayAgent,
				AgentStatusList.Codes.GatewayAgentWithTariff,
			};

			AssertEquals("Precondition: user has right to edit pre-allocations in standard consol", true, Env.Security.ConsolPreAllocationEditing.IsAllowed);
			AssertEquals("Precondition: user has no right to edit pre-allocations in gateway consol", false, Env.Security.GatewayConsolPreAllocationEditing.IsAllowed);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			GlbDepartment.CurrentDepartment.GE_Code = "FDA";
			using (new TemporaryUserContext { DepartmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid() }.Set())
			{
				foreach (var agentCode in agentCodeList)
				{
					consol.JK_SendingForwarderHandlingType = agentCode;

					Assert("Precondition: not gateway department", !GlbDepartment.CurrentDepartment.IsGatewayDepartment);
					AssertPreAllocationPropertiesReadOnly(consol, gatewayAgentCodeList.Contains(agentCode));
				}
			}

			GlbDepartment.CurrentDepartment.GE_Code = "GDA";
			using (new TemporaryUserContext { DepartmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid() }.Set())
			{
				foreach (var agentCode in agentCodeList)
				{
					consol.JK_SendingForwarderHandlingType = agentCode;

					Assert("Precondition: gateway department", GlbDepartment.CurrentDepartment.IsGatewayDepartment);
					AssertPreAllocationPropertiesReadOnly(consol, gatewayAgentCodeList.Contains(agentCode));
				}
			}
		}

		public void TestPreAllocationPropertiesReadOnly_WithConsolEditDisallowedAndGatewayConsolAllowed()
		{
			Env.Security.ConsolPreAllocationEditing.IsAllowed = false;
			Env.Security.GatewayConsolPreAllocationEditing.IsAllowed = true;

			var gatewayAgentCodeList = new List<string>
			{
				AgentStatusList.Codes.GatewayAgent,
				AgentStatusList.Codes.GatewayAgentWithTariff,
			};

			AssertEquals("Precondition: user has no right to edit pre-allocations in standard consol", false, Env.Security.ConsolPreAllocationEditing.IsAllowed);
			AssertEquals("Precondition: user has right to edit pre-allocations in gateway consol", true, Env.Security.GatewayConsolPreAllocationEditing.IsAllowed);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			GlbDepartment.CurrentDepartment.GE_Code = "FDA";
			using (new TemporaryUserContext { DepartmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid() }.Set())
			{
				foreach (var agentCode in agentCodeList)
				{
					consol.JK_SendingForwarderHandlingType = agentCode;

					Assert("Precondition: not gateway department", !GlbDepartment.CurrentDepartment.IsGatewayDepartment);
					AssertPreAllocationPropertiesReadOnly(consol, !gatewayAgentCodeList.Contains(agentCode));
				}
			}

			GlbDepartment.CurrentDepartment.GE_Code = "GDA";
			using (new TemporaryUserContext { DepartmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid() }.Set())
			{
				foreach (var agentCode in agentCodeList)
				{
					consol.JK_SendingForwarderHandlingType = agentCode;

					Assert("Precondition: gateway department", GlbDepartment.CurrentDepartment.IsGatewayDepartment);
					AssertPreAllocationPropertiesReadOnly(consol, !gatewayAgentCodeList.Contains(agentCode));
				}
			}
		}

		void AssertPreAllocationPropertiesReadOnly(ForwardingConsol consol, bool expectedReadOnly)
		{
			AssertEquals(expectedReadOnly, consol.JK_TotalShipmentActWeightCheckInfo.ReadOnly);
			AssertEquals(expectedReadOnly, consol.JK_TotalShipmentActVolumeCheckInfo.ReadOnly);
			AssertEquals(expectedReadOnly, consol.JK_TotalShipmentChargableCheckInfo.ReadOnly);
			AssertEquals(expectedReadOnly, consol.JK_TotalShipmentCountCheckInfo.ReadOnly);
			AssertEquals(expectedReadOnly, consol.WeightVerificationUnitInfo.ReadOnly);
			AssertEquals(expectedReadOnly, consol.VolumeVerificationUnitInfo.ReadOnly);
			AssertEquals(expectedReadOnly, consol.JK_MaximumAllowablePackageHeightInfo.ReadOnly);
			AssertEquals(expectedReadOnly, consol.JK_MaximumAllowablePackageWidthInfo.ReadOnly);
			AssertEquals(expectedReadOnly, consol.JK_MaximumAllowablePackageLengthInfo.ReadOnly);
			AssertEquals(expectedReadOnly, consol.JK_MaximumAllowablePackageUnitInfo.ReadOnly);
		}

		#endregion

		#region Buyer/Supplier Relationship Link

		DisposableAction AllowNewSuppilerBuyerLink(BusinessObjectFactory factory)
		{
			var handler = new EventHandler<NewSupplierBuyerLinkEventArgs>((s, args) => args.Result = ZDialogResult.Yes);
			return new DisposableAction(
				() => ShipmentDomainService.GetInstance(factory).NewSupplierBuyerLink += handler,
				() => ShipmentDomainService.GetInstance(factory).NewSupplierBuyerLink -= handler);
		}

		public void TestBuyerSupplier()
		{
			// Arrange
			var shipment = Consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S1010";
			shipment.Consols.Cast<ForwardingConsol>().ForEach(c => c.IsRoot = true);

			var newConsignor = Factory.New<OrgHeader>();
			newConsignor.OH_Code = "*CNOR*";
			newConsignor.OH_FullName = "Test Consignor";
			newConsignor.MainAddress.OA_Address1 = "Consignor Address";
			newConsignor.OH_IsConsignor = true;
			var newConsignee = Factory.New<OrgHeader>();
			newConsignee.OH_Code = "*CNEE*";
			newConsignee.OH_FullName = "Test Consignee";
			newConsignee.MainAddress.OA_Address1 = "Consignee Address";
			newConsignee.OH_IsConsignee = true;
			Factory.Save();

			// Act
			using (AllowNewSuppilerBuyerLink(Factory))
			{
				shipment.ConsigneePK = newConsignee.PK;
				shipment.ConsignorPK = newConsignor.PK;
			}
			Factory.Save();

			// Assert
			var filter = new ZQuery();
			filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Buyer, newConsignee.PK);
			filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, newConsignor.PK);
			var links = Factory.Load<OrgSupplierBuyerLink>(filter);
			AssertEquals("Supplier Buyer Link should be saved.", 1, links.Length);

			var link = links[0];
			AssertEquals("Buyer supplier link - buyer", newConsignee.PK, link.Buyer.PK);
			AssertEquals("Buyer supplier link - supplier", newConsignor.PK, link.Supplier.PK);

			Assert("Still Contains Key", shipment.CurrentRootConsol.BuyerSupplierContainsPair(newConsignee, newConsignor, ZString.Empty));

			Consol.JK_MasterBillNum = "0103345";
			Factory.Save(); //Test doesn't blow up
		}

		public void TestBuyerSupplierLinkSaveWithDifferentCountry()
		{
			// Arrange
			var shipment1 = Consol.Shipments.AddNew();
			shipment1.JS_RL_NKDestination = "AUSYD";
			shipment1.Consols.Cast<ForwardingConsol>().ForEach(c => c.IsRoot = true);
			var shipment2 = Consol.Shipments.AddNew();
			shipment2.JS_RL_NKDestination = "CNSHA";
			shipment2.Consols.Cast<ForwardingConsol>().ForEach(c => c.IsRoot = true);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			Factory.Save();

			// Act & Assert
			using (AllowNewSuppilerBuyerLink(Factory))
			{
				shipment1.ConsigneePK = consignee.PK;
				shipment1.ConsignorPK = consignor.PK;
			}
			Factory.Save();

			var filter = new ZQuery();
			filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Buyer, consignee.PK);
			filter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, consignor.PK);

			var links = Factory.Load<OrgSupplierBuyerLink>(filter);
			AssertEquals("Supplier Buyer Link should be saved.", 1, links.Length);
			Assert("Still Contains Key", shipment1.CurrentRootConsol.BuyerSupplierContainsPair(consignee, consignor, "AU"));

			// Act & Assert
			using (AllowNewSuppilerBuyerLink(Factory))
			{
				shipment2.ConsigneePK = consignee.PK;
				shipment2.ConsignorPK = consignor.PK;
			}
			Factory.Save();

			links = Factory.Load<OrgSupplierBuyerLink>(filter);
			AssertEquals("New Supplier Buyer Link should be saved, because it has different import country code from the one in database.", 2, links.Length);
			Assert("Still Contains Key", shipment2.CurrentRootConsol.BuyerSupplierContainsPair(consignee, consignor, "CN"));
			AssertEquals(consignee.PK, links[0].OL_OH_Buyer);
			AssertEquals(consignor.PK, links[0].OL_OH_Supplier);
			AssertEquals(consignee.PK, links[1].OL_OH_Buyer);
			AssertEquals(consignor.PK, links[1].OL_OH_Supplier);

			AssertContainsExactElementsInAnyOrder(new List<ZString> { "CN", "AU" }, links.Select(o => o.OL_RN_NKImporterCountry));
		}

		#endregion

		#region Caclulated Properties

		#region JK_TotalManifestedWeight

		public void TestJK_TotalManifestedWeight()
		{
			ForwardingShipment shipment1 = Consol.Shipments.AddNew();
			shipment1.JS_ManifestedWeight = 10.5M;
			shipment1.JS_UnitOfWeight = Consol.JK_TotalShipmentWeightUnit;

			ForwardingShipment shipment2 = Consol.Shipments.AddNew();
			shipment2.JS_ManifestedWeight = 5M;
			shipment2.JS_UnitOfWeight = Consol.JK_TotalShipmentWeightUnit;

			ForwardingShipment subShipment = Consol.Shipments.AddNew();
			subShipment.JS_JS_ColoadMasterShipment = shipment2.PK;
			subShipment.JS_ManifestedWeight = 10M;
			subShipment.JS_UnitOfWeight = Consol.JK_TotalShipmentWeightUnit;

			AssertEquals(15.5M, Consol.JK_TotalManifestedWeight);
		}

		#endregion

		#region JK_TotalManifestedVolume

		public void TestJK_TotalManifestedVolume()
		{
			ForwardingShipment shipment1 = Consol.Shipments.AddNew();
			shipment1.JS_ManifestedVolume = 20.5M;
			shipment1.JS_UnitOfVolume = Consol.JK_TotalShipmentVolumeUnit;

			ForwardingShipment shipment2 = Consol.Shipments.AddNew();
			shipment2.JS_ManifestedVolume = 3M;
			shipment2.JS_UnitOfVolume = Consol.JK_TotalShipmentVolumeUnit;

			ForwardingShipment subShipment = Consol.Shipments.AddNew();
			subShipment.JS_JS_ColoadMasterShipment = shipment2.PK;
			subShipment.JS_ManifestedVolume = 10M;
			subShipment.JS_UnitOfVolume = Consol.JK_TotalShipmentVolumeUnit;

			AssertEquals(23.5M, Consol.JK_TotalManifestedVolume);
		}

		#endregion

		#region JK_TotalManifestedChargeable

		public void TestJK_TotalManifestedChargeable()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			ForwardingShipment shipment1 = Consol.Shipments.AddNew();
			shipment1.JS_ManifestedVolume = 15M;

			ForwardingShipment shipment2 = Consol.Shipments.AddNew();
			shipment2.JS_ManifestedVolume = 10M;

			ForwardingShipment subShipment = Consol.Shipments.AddNew();
			subShipment.JS_JS_ColoadMasterShipment = shipment2.PK;
			subShipment.JS_ManifestedVolume = 5M;

			AssertEquals("JK_TotalManifestedChargeable should exclude sub shipments", 25M, Consol.JK_TotalManifestedChargeable);
		}

		#endregion

		#endregion

		#region Findbox Filter Properties

		#region ShowSubHouseBillShipments

		public void TestShowSubHouseBillShipments()
		{
			Assert("Should be initially false", !Consol.ShowSubHouseBillShipments);

			CommonShipment masterShipment = Consol.Shipments.AddNew();
			CommonShipment subColoadShipment = Consol.Shipments.AddNew();
			subColoadShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			AssertEquals("In view", true, Consol.GridShipments.Contains(masterShipment));
			AssertEquals("Out of view", false, Consol.GridShipments.Contains(subColoadShipment));

			Consol.ShowSubHouseBillShipments = true;
			AssertEquals("All in view 1", true, Consol.GridShipments.Contains(masterShipment));
			AssertEquals("All in view 2", true, Consol.GridShipments.Contains(subColoadShipment));
		}

		#endregion

		#endregion

		#region Shipment Collections

		public void TestTopLevelShipmentsChangingAllShipmentsCollection()
		{
			AssertNotNull(Consol.TopLevelShipments);
			AssertEquals("None to start with", 0, Consol.TopLevelShipments.Count);

			CommonShipment inViewShipment = Consol.Shipments.AddNew();
			AssertEquals("In view", true, Consol.TopLevelShipments.Contains(inViewShipment));

			CommonShipment nonInViewShipment = Consol.Shipments.AddNew();
			nonInViewShipment.JS_JS_ColoadMasterShipment = inViewShipment.PK;
			AssertEquals("Out of view", false, Consol.TopLevelShipments.Contains(nonInViewShipment));
		}

		public void TestGridShipmentsChangingAllShipmentsCollection()
		{
			AssertNotNull(Consol.GridShipments);
			AssertEquals("None to start with", 0, Consol.GridShipments.Count);

			CommonShipment topLevelShipment = Consol.Shipments.AddNew();
			AssertEquals("In view", true, Consol.GridShipments.Contains(topLevelShipment));

			CommonShipment subColoadShipment = Consol.Shipments.AddNew();
			subColoadShipment.JS_JS_ColoadMasterShipment = topLevelShipment.PK;
			AssertEquals("Out of view", false, Consol.GridShipments.Contains(subColoadShipment));

			Consol.ShowSubHouseBillShipments = true;
			AssertEquals("Should show all 1", true, Consol.GridShipments.Contains(topLevelShipment));
			AssertEquals("Should show all 2", true, Consol.GridShipments.Contains(subColoadShipment));
		}

		public void TestAllShipmentsChangingTopLevelShipments()
		{
			AssertNotNull(Consol.Shipments);
			AssertEquals("None to start with", 0, Consol.Shipments.Count);

			CommonShipment inViewShipment = Consol.TopLevelShipments.AddNew();
			AssertEquals("In Shipments", true, Consol.Shipments.Contains(inViewShipment));
			AssertEquals("In TopLevelShipments", true, Consol.TopLevelShipments.Contains(inViewShipment));
			AssertEquals("In GridShipments", true, Consol.GridShipments.Contains(inViewShipment));
		}

		public void TestSubShipmentsHaveErrors()
		{
			OrgHeader header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.OH_IsConsignee = true;
			ForwardingShipment standaloneShipment = Consol.Shipments.AddNew();
			standaloneShipment.ConsigneePK = header.PK;

			standaloneShipment.JS_TransportMode = "ERR";
			ForwardingShipment masterShipment = Consol.Shipments.AddNew();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals("No subshipments.", false, Consol.SubShipmentsHaveErrors);

			ForwardingShipment subShipment1 = Consol.Shipments.AddNew();
			subShipment1.ConsigneePK = header.PK;
			subShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;
			ForwardingShipment subShipment2 = Consol.Shipments.AddNew();
			subShipment2.ConsigneePK = header.PK;
			subShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;
			AssertEquals("No errors on subshipments.", false, Consol.SubShipmentsHaveErrors);

			subShipment2.JS_TransportMode = "ERR";
			AssertEquals("A sub shipment has errors.", true, Consol.SubShipmentsHaveErrors);
		}

		#endregion

		#region JobMawb

		public void TestCarrierAddressIsUpdatingFromMAWB()
		{
			RefAirline airline = Factory.New<RefAirline>();
			airline.RM_AirlineName1 = "Best Airline Ever";
			airline.RM_TwoCharacterCode = "XD";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "666";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "AIRLINE_WW";
			org.MainAddress.OA_Address1 = "742 EVERGREEN TERRACE";
			org.OH_IsShippingProvider = true;
			org.OH_IsAirLine = true;
			OrgMiscServ miscServ = org.MiscServ;
			miscServ.OM_RM_Airline = airline.PK;

			Factory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			consol.MasterBillAirlinePrefix = "666";

			AssertEquals("Carrier should populate", "AIRLINE_WW", ((OrgHeader)consol.JK_OA_ShippingLineAddress_ZAddress.OrgHeader).OH_Code);
			AssertEquals("Carrier address should populate", "742 EVERGREEN TERRACE", consol.JK_OA_ShippingLineAddress_ZAddress.AddressFull);

			consol.MasterBillAirlinePrefix = "333";

			AssertNull("Carrier should be empty", consol.JK_OA_ShippingLineAddress_ZAddress.OrgHeader);
			AssertEquals("Carrier address should not be populated", "* NO ORGANIZATION IS SELECTED", consol.JK_OA_ShippingLineAddress_ZAddress.AddressFull);
		}

		public void TestMAWBLabelDesc()
		{
			JobMawb jobMawb = TestJobMawb;
			SetConsolAirLoadingMasterBill();
			jobMawb.JM_IsPrinted = true;
			Factory.Save();
			AssertEquals("Neutral Master Printed", Consol.MAWBLabelDesc);

			jobMawb.JM_IsPrinted = false;
			Factory.Save();
			Consol.JK_IsNeutralMaster = true;
			AssertEquals("Neutral Master", Consol.MAWBLabelDesc);

			Consol.JK_IsNeutralMaster = false;
			Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AssertEquals("Master House", Consol.MAWBLabelDesc);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("Carrier Master", Consol.MAWBLabelDesc);

			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AssertEquals("Carrier Master", Consol.MAWBLabelDesc);

			Consol.JK_AgentType = Core.Constants.AgentType.AWBCoload;
			AssertEquals("Carrier Master", Consol.MAWBLabelDesc);
		}

		public void TestMasterBillAirlinePrefix()
		{
			JobMawb jobMawb = AddMawb("176", "10000001", GlbBranch.CurrentBranch, "STD");
			Factory.Save();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.JK_AgentType = "AGT";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			consol.JK_MasterBillNum = "17610000001";
			consol.MasterBillAirlinePrefix = "083";
			AssertEquals("No allocation before save", "08310000001", consol.JK_MasterBillNum);
			AssertEquals("083", consol.MasterBillAirlinePrefix);
			consol.JK_IsNeutralMaster = true;
			consol.MasterBillAirlinePrefix = "176";
			consol.Factory.Save();
			AssertEquals("17610000001", consol.JK_MasterBillNum);
			AssertEquals("176", consol.MasterBillAirlinePrefix);
			consol.JK_IsNeutralMaster = false;
			consol.MasterBillAirlinePrefix = "618";
			AssertEquals("618", consol.JK_MasterBillNum);
			AssertEquals("618", consol.MasterBillAirlinePrefix);
		}

		public void TestMasterBillMAWB()
		{
			JobMawb jobMawb = AddMawb("176", "10000001", GlbBranch.CurrentBranch, "STD");
			Factory.Save();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.JK_AgentType = "AGT";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			consol.JK_MasterBillNum = "17610000001";

			consol.JK_IsNeutralMaster = true;
			consol.MasterBillAirlinePrefix = "083";
			AssertEquals("083", consol.JK_MasterBillNum);
			AssertEquals("", consol.MasterBillMAWB);
			consol.MasterBillAirlinePrefix = "176";
			consol.Factory.Save();
			AssertEquals("17610000001", consol.JK_MasterBillNum);
			AssertEquals("10000001", consol.MasterBillMAWB);
			consol.JK_IsNeutralMaster = false;
			consol.MasterBillAirlinePrefix = "618";
			AssertEquals("618", consol.JK_MasterBillNum);
			AssertEquals("", consol.MasterBillMAWB);
		}

		public void TestMasterBillMAWBWithCrossSavedConsol()
		{
			JobMawb jobMawb = AddMawb("176", "10000001", GlbBranch.CurrentBranch, "STD");
			Factory.Save();

			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_AWBServiceLevel = "STD";
			consol1.JK_RL_NKLoadPort = HomePort;
			consol1.JK_RL_NKDischargePort = OverseasPort;
			consol1.JK_AgentType = "AGT";
			consol1.JK_TransportMode = Constants.TransportModes.Air;
			consol1.MasterBillAirlinePrefix = "176";
			consol1.MasterBillMAWB = "10000001";
			consol1.JK_IsNeutralMaster = true;

			var factory2 = new BusinessObjectFactory();
			var consol2 = factory2.New<ForwardingConsol>();
			consol2.JK_AWBServiceLevel = "STD";
			consol2.JK_RL_NKLoadPort = HomePort;
			consol2.JK_RL_NKDischargePort = OverseasPort;
			consol2.JK_AgentType = "AGT";
			consol2.JK_TransportMode = Constants.TransportModes.Air;
			consol2.JK_IsNeutralMaster = true;
			consol2.MasterBillAirlinePrefix = "176";
			factory2.Save();

			var mawb = new FreightJobMawbLink(Factory).LoadFromParentPK("JK", consol2.PK);

			consol1.JK_IsNeutralMaster = false;
			consol1.JK_IsNeutralMaster = true;
			consol1.MAWBAllocation.AllocatedMawb = mawb;
			consol1.MAWBAllocation.SetAllocatedMAWBToParent(mawb, true);

			AssertExceptionThrown<MAWBAllocationException>("Unable to allocate the specified MAWB.", () => Factory.Save());

			consol1.JK_IsNeutralMaster = false;
			AssertNoExceptionThrown("Should save consol successfully after de-allocate.", () => Factory.Save());
		}

		public void TestMasterBillAirlinePrefixSetter_ConsolIsDirectOrAgentAndHasAllocatedMAWBs_AssignAllocatedMAWB()
		{
			var mawb = AddMawb("555", "10000001", GlbBranch.CurrentBranch, "STD");

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			Factory.Save();

			consol.JK_IsNeutralMaster = false;
			consol.JK_MasterBillNum = ZString.Empty;
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.MasterBillAirlinePrefix = "555";
			consol.Factory.Save();
			AssertEquals("MAWB should be allocated to consol", "55510000001", consol.JK_MasterBillNum);
			AssertEquals("JK_IsNeutralMaster", true, consol.JK_IsNeutralMaster);

			consol.JK_IsNeutralMaster = false;
			consol.JK_MasterBillNum = ZString.Empty;
			consol.Factory.Save();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.MasterBillAirlinePrefix = "555";
			consol.Factory.Save();
			AssertEquals("MAWB should still be allocated to consol", "55510000001", consol.JK_MasterBillNum);
			AssertEquals("JK_IsNeutralMaster", true, consol.JK_IsNeutralMaster);
		}

		public void TestMasterBillAirlinePrefixSetter_ConsolIsNotDirectOrAgentAndHasAllocatedMAWBs_DoNotAssignAllocatedMAWB()
		{
			var mawb = AddMawb("555", "10000001", GlbBranch.CurrentBranch, "STD");

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			Factory.Save();

			consol.JK_IsNeutralMaster = false;
			consol.JK_MasterBillNum = ZString.Empty;
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.MasterBillAirlinePrefix = "555";
			AssertNotEquals("MAWB should not be allocated to consol", "55510000001", consol.JK_MasterBillNum);
			AssertEquals("JK_IsNeutralMaster", false, consol.JK_IsNeutralMaster);

			consol.JK_IsNeutralMaster = false;
			consol.JK_MasterBillNum = ZString.Empty;
			consol.JK_AgentType = Constants.AgentType.Charter;
			consol.MasterBillAirlinePrefix = "555";
			AssertNotEquals("MAWB should not be allocated to consol", "55510000001", consol.JK_MasterBillNum);
			AssertEquals("JK_IsNeutralMaster", false, consol.JK_IsNeutralMaster);
		}

		public void TestChangeMasterAirLinePrefixDoesNotClearMasterBillMAWB()
		{
			var mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "081";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = "55555625";
			mawb.JM_ServiceLevel = "STD";

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.Transports[0].JW_VoyageFlight = "QF987";
			consol.MasterBillAirlinePrefix = "081";

			Factory.Save();

			AssertEquals("prerequisite: NeutralMaster is true.", true, consol.JK_IsNeutralMaster);
			AssertEquals("prerequisite: MasterBillMAWB allocated.", "55555625", consol.MasterBillMAWB);
			AssertEquals("Consol master bill number.", "08155555625", consol.JK_MasterBillNum);

			var loadedConsol = Factory.Load<ForwardingConsol>(consol.PK);
			loadedConsol.JK_RL_NKLoadPort = "SGSIN";
			loadedConsol.MasterBillAirlinePrefix = "001";

			AssertEquals("Neutral Master will be set to false as allocation of MAWB not allowed.", false, loadedConsol.JK_IsNeutralMaster);
			AssertEquals("MasterBillMAWB will be empty as no MAWB allocated.", string.Empty, consol.MasterBillMAWB);
			AssertEquals("Consol master bill number will be the airline prefix.", "001", consol.JK_MasterBillNum);
		}

		public void TestJK_MasterBillNum()
		{
			JobMawb jobMawb = TestJobMawb;
			SetConsolAirLoadingMasterBill();
			Consol.JK_IsNeutralMaster = true;
			Consol.JK_MasterBillNum = "17610000001";
			AssertEquals("17610000001", Consol.JK_MasterBillNum);
			AssertEquals("176", Consol.MasterBillAirlinePrefix);
			AssertEquals("10000001", Consol.MasterBillMAWB);
			AssertNotNull("JobMawb object should be loaded.", jobMawb);
		}

		public void TestReprintedNeutralMAWB()
		{
			var mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "176";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = "10000001";
			mawb.JM_ServiceLevel = "STD";

			Factory.Save();
			var consol = Factory.New<ForwardingConsol>();

			consol.JK_AgentType = "AGT";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.MasterBillAirlinePrefix = "176";

			mawb.JM_IsPrinted = false;
			consol.JK_IsNeutralMaster = true;
			Factory.Save();
			AssertEquals("Mawb allocated", mawb, consol.MAWBAllocation.AllocatedMawb);
			AssertEquals("Neutral MAWB not yet Printed", ZBool.False, consol.IsNeutralMAWBPrinted);

			mawb.JM_IsPrinted = true;
			consol.JK_IsNeutralMaster = false;
			AssertEquals("Mawb marked to be unallocated", true, consol.MAWBAllocation.GetShouldDeallocate());
			AssertEquals("Not a Neutral Master", ZBool.False, consol.IsNeutralMAWBPrinted);

			consol.JK_IsNeutralMaster = true;
			consol.JK_MasterBillNum = "17610000001";
			mawb.JM_IsPrinted = true;
			AssertEquals("Mawb re-allocated although is marked as printed (can never happen in UI as setting consol as neutral master makes JK_MasterBillNum readonly)",
				mawb, consol.MAWBAllocation.AllocatedMawb);
			AssertEquals("Neutral MAWB printed", ZBool.True, consol.IsNeutralMAWBPrinted);
		}

		const string TestOceanBill1 = "OBL28928729";
		const string TestOceanBill2 = "OBL28541247";
		public void TestDuplicateOceanBillWarning()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_MasterBillNum = TestOceanBill1;
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			ForwardingConsol consol2 = factory2.New<ForwardingConsol>();
			consol2.JK_MasterBillNum = TestOceanBill1;
			AssertEquals("Duplicated Ocean Bills should be a warning", true, consol2.JK_MasterBillNumInfo.HasWarnings());
			consol2.JK_MasterBillNum = TestOceanBill2;
			AssertEquals("Ocean Bills should not have any warnings", false, consol2.JK_MasterBillNumInfo.HasWarnings());
		}

		public void TestSpecialHandlingCodeDefaultedFromAirTransport()
		{
			AddMawb("176", "10000001", GlbBranch.CurrentBranch, "STD");
			var carrier = RefAirline.LoadFromAirlinePrefix(Factory, "176");
			carrier.HasSignedEAWBAgreement = true;
			var rule = carrier.EFreightStatusCollection.AddNew();
			rule.RME_OriginLocation = HomePort;
			rule.RME_DestinationLocation = OverseasPort;
			rule.RME_EFreightStatus = Constants.EFreightStatus.Code.ECC;
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = "AGT";
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_IsNeutralMaster = true;

			AssertEquals("consol.AWBSpecialHandlingItems.Count", 0, consol.AWBSpecialHandlingItems.Count);

			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;

			AssertEquals("consol.AWBSpecialHandlingItems.Count", 1, consol.AWBSpecialHandlingItems.Count);
			AssertEquals("Special Handling Item should be ECC"
				, Constants.EFreightStatus.Code.ECC
				, consol.AWBSpecialHandlingItems[0].JKH_Code);
		}

		public void TestSpecialHandlingCodeDefaultedFromLoadPort()
		{
			AddMawb("176", "10000001", GlbBranch.CurrentBranch, "STD");
			var carrier = RefAirline.LoadFromAirlinePrefix(Factory, "176");
			carrier.HasSignedEAWBAgreement = true;
			var rule = carrier.EFreightStatusCollection.AddNew();
			rule.RME_OriginLocation = HomePort;
			rule.RME_DestinationLocation = OverseasPort;
			rule.RME_EFreightStatus = Constants.EFreightStatus.Code.ECC;
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = "AGT";
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_IsNeutralMaster = true;
			consol.JK_MasterBillNum = "17610000001";

			AssertEquals("consol.AWBSpecialHandlingItems.Count", 1, consol.AWBSpecialHandlingItems.Count);
			AssertEquals("Special Handling Item should be EAW"
				, AWBSpecialHandlingCodeDescriptionPairList.Codes.EFreightConsignmentWithNoAccompanyingPaperDocuments
				, consol.AWBSpecialHandlingItems[0].JKH_Code);

			consol.JK_RL_NKLoadPort = HomePort;

			AssertEquals("consol.AWBSpecialHandlingItems.Count", 1, consol.AWBSpecialHandlingItems.Count);
			AssertEquals("Special Handling Item should be ECC"
				, AWBSpecialHandlingCodeDescriptionPairList.Codes.ConsignmentEstablishedWithAnElectronicallyConcludedCargoContractEccWithNoAccompanyingPaperAirWaybill
				, consol.AWBSpecialHandlingItems[0].JKH_Code);
		}

		public void TestSpecialHandlingCodeDefaultedFromDischargePort()
		{
			AddMawb("176", "10000001", GlbBranch.CurrentBranch, "STD");
			var carrier = RefAirline.LoadFromAirlinePrefix(Factory, "176");
			carrier.HasSignedEAWBAgreement = true;
			var rule = carrier.EFreightStatusCollection.AddNew();
			rule.RME_OriginLocation = HomePort;
			rule.RME_DestinationLocation = OverseasPort;
			rule.RME_EFreightStatus = Constants.EFreightStatus.Code.ECC;
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = "AGT";
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_IsNeutralMaster = true;
			consol.JK_MasterBillNum = "17610000001";

			AssertEquals("consol.AWBSpecialHandlingItems.Count", 1, consol.AWBSpecialHandlingItems.Count);
			AssertEquals("Special Handling Item should be EAW"
				, AWBSpecialHandlingCodeDescriptionPairList.Codes.EFreightConsignmentWithNoAccompanyingPaperDocuments
				, consol.AWBSpecialHandlingItems[0].JKH_Code);

			consol.JK_RL_NKDischargePort = OverseasPort;

			AssertEquals("consol.AWBSpecialHandlingItems.Count", 1, consol.AWBSpecialHandlingItems.Count);
			AssertEquals("Special Handling Item should be ECC"
				, AWBSpecialHandlingCodeDescriptionPairList.Codes.ConsignmentEstablishedWithAnElectronicallyConcludedCargoContractEccWithNoAccompanyingPaperAirWaybill
				, consol.AWBSpecialHandlingItems[0].JKH_Code);
		}

		public void TestSpecialHandlingCodeDefaultedFromCarrier()
		{
			AddMawb("176", "10000001", GlbBranch.CurrentBranch, "STD");
			var carrier = RefAirline.LoadFromAirlinePrefix(Factory, "176");
			carrier.HasSignedEAWBAgreement = true;
			var rule = carrier.EFreightStatusCollection.AddNew();
			rule.RME_OriginLocation = HomePort;
			rule.RME_DestinationLocation = OverseasPort;
			rule.RME_EFreightStatus = Constants.EFreightStatus.Code.ECC;
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = "AGT";
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_IsNeutralMaster = true;
			consol.JK_MasterBillNum = "17610000001";

			AssertEquals("consol.AWBSpecialHandlingItems.Count", 1, consol.AWBSpecialHandlingItems.Count);
			AssertEquals("Special Handling Item should be EAW"
				, AWBSpecialHandlingCodeDescriptionPairList.Codes.EFreightConsignmentWithNoAccompanyingPaperDocuments
				, consol.AWBSpecialHandlingItems[0].JKH_Code);

			ZGuid randomGuid = Guid.NewGuid();
			consol.JK_OA_ShippingLineAddress = randomGuid;

			AssertEquals("consol.AWBSpecialHandlingItems.Count", 1, consol.AWBSpecialHandlingItems.Count);
			AssertEquals("Special Handling Item should be EAW"
				, AWBSpecialHandlingCodeDescriptionPairList.Codes.EFreightConsignmentWithNoAccompanyingPaperDocuments
				, consol.AWBSpecialHandlingItems[0].JKH_Code);
		}

		public void TestSpecialHandlingCodeDefaultedFromMawbAirlinePrefix()
		{
			AddMawb("176", "10000001", GlbBranch.CurrentBranch, "STD");
			var carrier = RefAirline.LoadFromAirlinePrefix(Factory, "176");
			carrier.HasSignedEAWBAgreement = true;
			var rule = carrier.EFreightStatusCollection.AddNew();
			rule.RME_OriginLocation = HomePort;
			rule.RME_DestinationLocation = OverseasPort;
			rule.RME_EFreightStatus = Constants.EFreightStatus.Code.ECC;
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = "AGT";
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_OA_ShippingLineAddress = Guid.NewGuid();
			consol.JK_IsNeutralMaster = true;

			AssertEquals("consol.AWBSpecialHandlingItems.Count", 0, consol.AWBSpecialHandlingItems.Count);
			consol.JK_MasterBillNum = "17610000001";

			AssertEquals("consol.AWBSpecialHandlingItems.Count", 1, consol.AWBSpecialHandlingItems.Count);
			AssertEquals("Special Handling Item should be EAW"
				, AWBSpecialHandlingCodeDescriptionPairList.Codes.EFreightConsignmentWithNoAccompanyingPaperDocuments
				, consol.AWBSpecialHandlingItems[0].JKH_Code);
		}

		#region Duplicate Master Bill Tests

		public void TestDuplicateMasterBills()
		{
			var mawb = AddMawb("081", "87443521", GlbBranch.CurrentBranch, "STD");
			mawb.Factory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.Loose;
			consol.MasterBillAirlinePrefix = "081";
			consol.JK_IsNeutralMaster = true;

			Transport transport = consol.Transports[0];
			transport.JW_JX = ExportSailing.PK;

			consol.Factory.Save();

			ZString masterBillNumber = "08187443521";

			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_AgentType = Enterprise.Core.Constants.AgentType.Agent;
			consol2.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol2.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.Loose;
			consol2.JK_MasterBillNum = masterBillNumber;

			Transport transport2 = consol2.Transports[0];
			transport2.JW_JX = ExportSailing.PK;

			AssertEquals("Expecting Duplicate Master Bill Error", true, consol2.MasterBillMAWBInfo.HasErrors());
			consol2.JK_MasterBillNum = "53535879654";
			AssertNoErrors("Expecting No error on Master Bill", consol2.MasterBillMAWBInfo);

			consol2.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol2.JK_MasterBillNum = masterBillNumber;
			AssertEquals("Expecting Error for Duplicate Master Bill on Co-Loads", true, consol2.MasterBillMAWBInfo.HasErrors());

			consol2.JK_MasterBillNum = "53535879654";
			Factory.Save();

			consol.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			Factory.Save();

			consol2.JK_MasterBillNum = masterBillNumber;
			Factory.Save();

			consol.JK_MasterBillNum = "08187443522";

			consol2.JK_MasterBillNum = "08621542154";
			AssertNoErrors("Expecting No error on Master Bill", consol2.MasterBillMAWBInfo);

			consol2.JK_MasterBillNum = "08187443522";
			AssertEquals("Expecting No Error for Duplicate Master Bill on Co-Loads", false, consol2.MasterBillMAWBInfo.HasErrors());
		}

		#endregion

		#region TestSettingSailingSetsMasterBill

		public void TestSettingSailingSetsMasterBill()
		{
			Consol.JK_AgentType = Enterprise.Core.Constants.AgentType.Agent;
			Consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.Loose;
			Consol.JK_MasterBillNum = "08187443521";

			AssertEquals("08187443521", Consol.JK_MasterBillNum);

			ExportSailing.Voyage.JV_VoyageFlight = "QF123";

			Transport transport = Consol.Transports[0];
			transport.JW_IsLinked = true;
			transport.JW_JX = ExportSailing.PK;

			AssertEquals("081", Consol.JK_MasterBillNum.Substring(0, 3));

			Consol.MasterBillMAWB = ZString.Empty;
			ExportSailing1.Voyage.JV_VoyageFlight = "QR123";
			transport.JW_JX = ExportSailing1.PK;
			AssertEquals("157", Consol.JK_MasterBillNum.Substring(0, 3));
		}

		#endregion

		public void TestReadOnlyStateOfMasterBillMAWB()
		{
			JobMawb jobMawb = TestJobMawb;
			SetConsolAirLoadingMasterBill();
			Consol.JK_IsNeutralMaster = false;
			AssertEquals(false, Consol.MasterBillMAWBInfo.ReadOnly);

			Consol.JK_IsNeutralMaster = true;
			AssertEquals(true, Consol.MasterBillMAWBInfo.ReadOnly);

			Consol.JK_AgentType = "DRT";
			AssertEquals(true, Consol.MasterBillMAWBInfo.ReadOnly);

			Consol.JK_AgentType = "AGT";
			AssertEquals(true, Consol.MasterBillMAWBInfo.ReadOnly);

			Consol.JK_AgentType = "CLD";
			AssertEquals(false, Consol.MasterBillMAWBInfo.ReadOnly);

			Consol.JK_IsNeutralMaster = true;
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "ZACPT";
			AssertEquals(false, Consol.MasterBillMAWBInfo.ReadOnly);

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_AgentType = "AGT";
			Consol.JK_IsNeutralMaster = true;
			Consol.JK_TransportMode = Constants.TransportModes.Air;

			AssertEquals(true, Consol.MasterBillMAWBInfo.ReadOnly);
		}

		public void TestReadOnlyStateOfMasterBillAirlinePrefix()
		{
			JobMawb jobMawb = TestJobMawb;
			SetConsolAirLoadingMasterBill();
			jobMawb.JM_IsPrinted = true;
			Consol.JK_IsNeutralMaster = true;
			AssertEquals(true, Consol.MasterBillMAWBInfo.ReadOnly);
		}

		public void TestJK_IsNeutralMaster()
		{
			JobMawb jobMawb = TestJobMawb;
			SetConsolAirLoadingMasterBill();
			Consol.JK_IsNeutralMaster = true;
			Consol.JK_MasterBillNum = "17610000001";
			AssertEquals("17610000001", Consol.JK_MasterBillNum);
			Consol.JK_IsNeutralMaster = false;
			AssertEquals("176", Consol.JK_MasterBillNum);
			Consol.JK_IsNeutralMaster = true;
			Consol.JK_AgentType = "CLD";
			AssertEquals(false, Consol.JK_IsNeutralMaster);
			Consol.JK_IsNeutralMaster = true;
			Consol.JK_AgentType = "DRT";
			AssertEquals(true, Consol.JK_IsNeutralMaster);
		}

		public void TestJK_IsNeutralMaster_ReadOnly()
		{
			JobMawb jobMawb = TestJobMawb;
			SetConsolAirLoadingMasterBill();

			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "NZAKL";
			Consol.JK_IsNeutralMaster = true;
			AssertEquals("JK_IsNeutralMaster should not be readonly", false, Consol.JK_IsNeutralMasterInfo.ReadOnly);
			Consol.JK_IsNeutralMaster = false;
			AssertEquals("JK_IsNeutralMaster should not be readonly", false, Consol.JK_IsNeutralMasterInfo.ReadOnly);

			Consol.JK_RL_NKLoadPort = "DEHAM";
			Consol.JK_IsNeutralMaster = true;
			AssertEquals("JK_IsNeutralMaster should not be readonly", false, Consol.JK_IsNeutralMasterInfo.ReadOnly);
			Consol.JK_IsNeutralMaster = false;
			AssertEquals("JK_IsNeutralMaster should be readonly", true, Consol.JK_IsNeutralMasterInfo.ReadOnly);
		}

		public void TestMarkForReallocationIfPrefixChanged()
		{
			var mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "176";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = "10000001";
			mawb.JM_ServiceLevel = "STD";

			Factory.Save();
			var consol = Factory.New<ForwardingConsol>();

			consol.JK_AgentType = "AGT";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.MasterBillAirlinePrefix = "176";

			consol.JK_IsNeutralMaster = true;
			Factory.Save();
			consol.JK_IsNeutralMaster = false;
			consol.MAWBAllocation.AllocatedMawb = mawb;
			mawb.JM_ParentID = Guid.NewGuid();
			AssertNoExceptionThrown("Should not have Null Ref Exception during this step", () => consol.JK_IsNeutralMaster = true);
		}

		public void TestNeutralMasterForAgentConsol()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.JK_AgentType = Constants.AgentType.Agent;

			AssertEquals(false, Consol.JK_IsNeutralMasterInfo.ReadOnly);

			Consol.JK_RL_NKLoadPort = "USCHI";
			AssertEquals(true, Consol.JK_IsNeutralMasterInfo.ReadOnly);
		}

		public void TestVoyageFlight()
		{
			JobMawb jobMawb = TestJobMawb;
			SetConsolAirLoadingMasterBill();
			Consol.MasterBillMAWB = ZString.Empty;

			Transport transport = Consol.Transports[0];

			transport.JW_VoyageFlight = "SA111";
			Consol.JK_IsNeutralMaster = true;
			AssertEquals("083", Consol.MasterBillAirlinePrefix);

			transport.JW_VoyageFlight = "SA222";
			Consol.JK_IsNeutralMaster = true;
			AssertEquals("083", Consol.MasterBillAirlinePrefix);

			transport.JW_VoyageFlight = "EK222";
			Consol.JK_IsNeutralMaster = true;
			AssertEquals("176", Consol.MasterBillAirlinePrefix);

			Consol.JK_MasterBillNum = "17610000001";
			transport.JW_VoyageFlight = "SA222";
			AssertEquals("176", Consol.MasterBillAirlinePrefix);

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_MasterBillNum = "MasterBill";
			transport.JW_VoyageFlight = "voyage";
			AssertEquals("When setting voyage Masterbill number will not have airline prefix appended when transport mode is not air.", "MasterBill", Consol.JK_MasterBillNum);

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			transport.JW_VoyageFlight = "SA222";
			AssertEquals("Master bill num gets airline prefix when transport mode is air.", "083", Consol.JK_MasterBillNum);
		}

		public void TestDefaultingCarrierFromMasterBillAirlinePrefix()
		{
			ZString currentCountry = GlbBranch.CurrentBranch.Country.Code;

			RefAirline airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_AirlineName1 = "Test Airline";
			airline.RM_TwoCharacterCode = "ZZ";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "1XX";
			airline.RM_AirlinePrefix = "1XX";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgMiscServ miscServ = org.MiscServ;
			miscServ.OM_RM_Airline = airline.PK;
			miscServ.OM_RN_NKEXDefaultCntryOfOrigin = Enterprise.Core.Constants.CountryCodes.NewZealand;

			Factory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_IsNeutralMaster = true;

			Transport transport1 = consol.Transports[0];
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;

			GlbBranch.CurrentBranch.SetCountry(Enterprise.Core.Constants.CountryCodes.NewZealand);

			try
			{
				AssertEquals("Precondition: Carriers should be empty", ZGuid.Empty, transport1.JW_OA_CarrierAddress);
				AssertEquals("Precondition: Carriers should be empty", ZGuid.Empty, transport2.JW_OA_CarrierAddress);

				consol.MasterBillAirlinePrefix = miscServ.Airline.RM_EagleAddedAirlinePrefixOrAccountingCode;
				AssertEquals("Carriers should be defaulted for Flight1 Transport", org.PK, transport1.CarrierPK);
				AssertEquals("Carriers should not be defaulted for Flight2 Transport", ZGuid.Empty, transport2.CarrierPK);
				AssertEquals("Carriers should be defaulted for Consol Transport", org.PK, consol.ShippingLinePK);
			}
			finally
			{
				GlbBranch.CurrentBranch.SetCountry(currentCountry);
			}
		}

		public void TestJK_RL_NKLoadPort()
		{
			JobMawb sampleJobMawb = Factory.New<JobMawb>();
			sampleJobMawb.JM_GB = GlbBranch.CurrentBranch.PK;
			sampleJobMawb.JM_Airline3DigitPrefix = "176";
			sampleJobMawb.JM_MAWB = "10000001";
			sampleJobMawb.JM_ServiceLevel = "STD";
			sampleJobMawb.JM_IsPaper = false;
			sampleJobMawb.JM_IsPrinted = false;
			AssertEquals("JobMawb should not be allocated", true, sampleJobMawb.JM_ParentID.IsEmpty);
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_IsNeutralMaster = true;
			AssertEquals(Core.Constants.AgentType.Agent, consol.JK_AgentType);
			AssertEquals(true, ImportExportHelper.IsBranchCountry(consol.JK_RL_NKLoadPort));
			AssertEquals(true, consol.IsAir);
			consol.MasterBillAirlinePrefix = "176";
			consol.Factory.Save();
			AssertEquals("17610000001", consol.JK_MasterBillNum);
		}

		void SetConsolAirLoadingMasterBill()
		{
			Consol.JK_AgentType = "AGT";
			Consol.JK_TransportMode = Constants.TransportModes.Air;

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";

			Consol.JK_MasterBillNum = "17610000001";
		}

		public void TestOnLoaded()
		{
			Consol.JK_IsNeutralMaster = true;
			JobMawb jobMawb = TestJobMawb;
			SetConsolAirLoadingMasterBill();
			Consol.OnLoaded();
			AssertEquals(jobMawb, Consol.MAWBAllocation.AllocatedMawb);
		}

		[ExpectNoExceptions]
		public void TestDeletingConsolWhileHavingMAWBAllocated()
		{
			JobMawb jobMawb = TestJobMawb;
			jobMawb.JM_ParentID = Consol.PK;
			Factory.Save();
			try
			{
				Consol.Delete();
				Fail("CannotDeleteException was expected to be thrown.");
			}
			catch (CannotDeleteException)
			{
			}
			catch (Exception)
			{
				Fail("Expected exception was not thrown.");
			}
			jobMawb.JM_ParentID = ZGuid.Empty;
			Consol.Delete();
		}

		public void TestAllocatedMawbNotDeletedWhenIsNeutralChanges()
		{
			JobMawb sampleJobMawb = Factory.New<JobMawb>();
			sampleJobMawb.JM_GB = GlbBranch.CurrentBranch.PK;
			sampleJobMawb.JM_Airline3DigitPrefix = "111";
			sampleJobMawb.JM_MAWB = "00000011";
			sampleJobMawb.JM_ServiceLevel = "STD";
			sampleJobMawb.JM_IsPaper = false;
			sampleJobMawb.JM_IsPrinted = false;
			AssertEquals("JobMawb should not be allocated", true, sampleJobMawb.JM_ParentID.IsEmpty);
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.MasterBillAirlinePrefix = "111";
			consol.Factory.Save();

			AssertEquals("JobMawb should be allocated", consol.PK, sampleJobMawb.JM_ParentID);
			AssertEquals("JK_MasterBillNum should be populated", "11100000011", consol.JK_MasterBillNum);

			consol.JK_IsNeutralMaster = false;
			AssertEquals("MasterBillNum should be cleared", "", consol.MasterBillMAWB);

			consol.JK_IsNeutralMaster = true;
			AssertEquals("MasterBillNum should be restored", "00000011", consol.MasterBillMAWB);
		}

		public void TestAllocatedMAWBCanBeReleasedAndAttachedToOtherConsols()
		{
			JobMawb sampleJobMawb = Factory.New<JobMawb>();
			sampleJobMawb.JM_GB = GlbBranch.CurrentBranch.PK;
			sampleJobMawb.JM_Airline3DigitPrefix = "111";
			sampleJobMawb.JM_MAWB = "00000011";
			sampleJobMawb.JM_ServiceLevel = "STD";
			sampleJobMawb.JM_IsPaper = false;
			sampleJobMawb.JM_IsPrinted = false;
			AssertEquals("JobMawb should not be allocated", true, sampleJobMawb.JM_ParentID.IsEmpty);
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.MasterBillAirlinePrefix = "111";
			consol.Factory.Save();

			AssertEquals("JobMawb should be allocated", consol.PK, sampleJobMawb.JM_ParentID);
			consol.MasterBillAirlinePrefix = "";

			consol.Factory.Save();
			sampleJobMawb.Reload();
			AssertEquals("Reference to the parent should remove when detaching JobMawb from a Consol", true, sampleJobMawb.JM_ParentID.IsEmpty);
			AssertEquals("TableCode should remove when detaching a JobMawb from Consol", true, sampleJobMawb.JM_ParentTableCode.IsEmpty);
			AssertEquals("JobMawb is not expected to be printed.", false, sampleJobMawb.JM_IsPrinted);

			var anotherConsol = Factory.New<ForwardingConsol>();
			anotherConsol.JK_RL_NKLoadPort = "AUSYD";
			anotherConsol.JK_TransportMode = Constants.TransportModes.Air;
			anotherConsol.JK_IsNeutralMaster = true;
			anotherConsol.MasterBillAirlinePrefix = "111";

			anotherConsol.Factory.Save();
			AssertEquals("JobMawb is attached to the Consol again", anotherConsol.PK, sampleJobMawb.JM_ParentID);
		}

		public void TestPreventAllocationOfTheSameMAWB()
		{
			JobMawb mawb1 = AddMawb("081", "00000011", GlbBranch.CurrentBranch, "STD");
			JobMawb mawb2 = AddMawb("081", "00000022", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			ForwardingConsol consol1 = factory1.NewWithValidTestData<ForwardingConsol>();
			ForwardingConsol consol2 = factory2.NewWithValidTestData<ForwardingConsol>();

			consol1.JK_UniqueConsignRef = "C0001";
			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol1.JK_RL_NKLoadPort = "AUBNE";
			consol1.JK_IsNeutralMaster = true;
			consol1.MasterBillAirlinePrefix = "081";
			consol1.Factory.Save();

			AssertNotNull(consol1.MAWBAllocation.AllocatedMawb);
			AssertEquals(consol1.MAWBAllocation.AllocatedMawb.JM_ParentID, consol1.PK);
			AssertEquals(consol1.MAWBAllocation.AllocatedMawb.JM_ParentTableCode, consol1.TablePrefix);
			AssertEquals(consol1.MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix, "081");
			AssertEquals(consol1.MAWBAllocation.AllocatedMawb.JM_MAWB, "00000011");

			consol2.JK_UniqueConsignRef = "C0002";
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol2.JK_RL_NKLoadPort = "AUBNE";
			consol2.JK_IsNeutralMaster = true;
			consol2.MasterBillAirlinePrefix = "081";
			consol2.Factory.Save();

			AssertNotNull(consol2.MAWBAllocation.AllocatedMawb);
			AssertEquals(consol2.MAWBAllocation.AllocatedMawb.JM_ParentID, consol2.PK);
			AssertEquals(consol2.MAWBAllocation.AllocatedMawb.JM_ParentTableCode, consol2.TablePrefix);
			AssertEquals(consol2.MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix, "081");
			AssertEquals(consol2.MAWBAllocation.AllocatedMawb.JM_MAWB, "00000022");

			factory1.Save();
			factory2.Save();

			BusinessObjectFactory factory3 = new BusinessObjectFactory();

			consol1 = factory3.Load<ForwardingConsol>(consol1.PK);
			consol2 = factory3.Load<ForwardingConsol>(consol2.PK);

			AssertNotEquals("consols have different MAWBs", consol1.JK_MasterBillNum, consol2.JK_MasterBillNum);

			mawb1 = factory3.LoadTop1<JobMawb>(new ZQuery(JobMawbSchema.JM_ParentID, consol1.PK));
			AssertNotNull(string.Format("expected JobMawb allocated to consol {0} MAWB {1}", consol1.JK_UniqueConsignRef, consol1.JK_MasterBillNum), mawb1);

			mawb2 = factory3.LoadTop1<JobMawb>(new ZQuery(JobMawbSchema.JM_ParentID, consol2.PK));
			AssertNotNull(string.Format("expected JobMawb allocated to consol {0} MAWB {1}", consol2.JK_UniqueConsignRef, consol2.JK_MasterBillNum), mawb2);
		}

		public void TestAWBReloadedWhenSavedInAnotherFactory()
		{
			var factory1 = new BusinessObjectFactory();
			var consol1 = factory1.New<ForwardingConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Air;
			factory1.Save();

			consol1.IsAWBValuesOverriddenProperty = true;

			var factory2 = new BusinessObjectFactory();
			var consol2 = factory2.Load<ForwardingConsol>(consol1.PK);

			Assert("Two separate AWBHeader objects created in two different factories.", consol1.AWBHeader.PK != consol2.AWBHeader.PK);

			factory1.Save();

			Assert("AWBHeader is reloaded from object saved in another factory.", consol1.AWBHeader.PK == consol2.AWBHeader.PK);
			AssertNoExceptionThrown("No exception expected, as AWBHeader was reloaded.", () => factory2.Save());
		}

		JobMawb TestJobMawb
		{
			get
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

				JobMawb mawb = AddMawb("176", "10000001", GlbBranch.CurrentBranch, "STD");
				mawb.JM_GB = GlbBranch.CurrentBranch.PK;
				mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
				mawb.JM_ParentID = Consol.PK;

				return mawb;
			}
		}

		JobMawb AddMawb(string prefix, string mawbNo, GlbBranch branch, string serviceLevel)
		{
			JobMawb mawb = Factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = prefix;
			mawb.JM_MAWB = mawbNo;
			mawb.JM_GB = branch.PK;
			mawb.JM_ServiceLevel = serviceLevel;

			return mawb;
		}

		#endregion

		#region Test Messaging

		public void TestConsolMessages()
		{
			Consol.Messages.AddNew();
			Consol.Messages.AddNew();
			AssertEquals("Expected 2 EDI Messages", 2, Consol.Messages.Count);
		}

		#endregion

		#region Test IDocManagerCode members

		public void TestDocManagerCode()
		{
			AssertEquals("Code should be CON. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "CON", Consol.DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region Document Events

		public void TestAnyGoodsTravelToOrThrough_US()
		{
			TestAndAssertGoodsTravelToOrThroughDifferCountries("USLAX");
		}

		public void TestAnyGoodsTravelToOrThrough_Canada()
		{
			TestAndAssertGoodsTravelToOrThroughDifferCountries("CAVAN");
		}

		public void TestAnyGoodsTravelToOrThrough_Indonesia()
		{
			TestAndAssertGoodsTravelToOrThroughDifferCountries("IDSBG");
		}

		public void TestAnyGoodsTravelToOrThrough_Malaysia()
		{
			TestAndAssertGoodsTravelToOrThroughDifferCountries("MYKUL");
		}

		public void TestAnyGoodsTravelToOrThrough_SouthAfrica()
		{
			TestAndAssertGoodsTravelToOrThroughDifferCountries("ZAJNB");
		}

		public void TestAnyGoodsTravelToOrThrough_India()
		{
			TestAndAssertGoodsTravelToOrThroughDifferCountries("INDEL");
		}

		public void TestAnyGoodsTravelToOrThrough_China()
		{
			TestAndAssertGoodsTravelToOrThroughDifferCountries("CNBJS");
		}

		public void TestAnyGoodsTravelToOrThrough_KoreaSouth()
		{
			TestAndAssertGoodsTravelToOrThroughDifferCountries("KRICN");
		}

		public void TestAnyGoodsTravelToOrThrough_EUN()
		{
			TestAndAssertGoodsTravelToOrThroughDifferCountries("ITROM");
		}

		public void TestAnyGoodsTravelToOrThrough_Thailand()
		{
			TestAndAssertGoodsTravelToOrThroughDifferCountries("THBAI");
		}

		public void TestAnyGoodsTravelToOrThrough_Taiwan()
		{
			TestAndAssertGoodsTravelToOrThroughDifferCountries("TWANG");
		}

		public void TestAnyGoodsTravelToOrThrough_Vietnam()
		{
			TestAndAssertGoodsTravelToOrThroughDifferCountries("VNAGG");
		}

		public void TestAnyGoodsTravelToOrThrough_Japan()
		{
			TestAndAssertGoodsTravelToOrThroughDifferCountries("JPAAE");
		}

		void TestAndAssertGoodsTravelToOrThroughDifferCountries(ZString port)
		{
			Transport mainTransport = Consol.Transports[0];

			mainTransport.JW_RL_NKDiscPort = "SGSIN";
			AssertEquals(false, Consol.AnyGoodsTravelToOrThroughFHLCountry);

			mainTransport.JW_RL_NKDiscPort = port;
			AssertEquals(true, Consol.AnyGoodsTravelToOrThroughFHLCountry);

			mainTransport.JW_RL_NKDiscPort = "SGSIN";
			mainTransport.JW_RL_NKLoadPort = port;
			AssertEquals(true, Consol.AnyGoodsTravelToOrThroughFHLCountry);

			mainTransport.JW_RL_NKLoadPort = "SGSIN";

			Transport otherTransport = Consol.Transports.AddNew();

			otherTransport.JW_RL_NKDiscPort = port;
			AssertEquals(true, Consol.AnyGoodsTravelToOrThroughFHLCountry);

			otherTransport.JW_RL_NKDiscPort = "SGSIN";
			AssertEquals(false, Consol.AnyGoodsTravelToOrThroughFHLCountry);

			ForwardingShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_RL_NKOrigin = "SGSIN";
			AssertEquals(false, Consol.AnyGoodsTravelToOrThroughFHLCountry);

			shipment.JS_RL_NKDestination = port;
			AssertEquals(true, Consol.AnyGoodsTravelToOrThroughFHLCountry);

			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_RL_NKOrigin = port;
			AssertEquals(true, Consol.AnyGoodsTravelToOrThroughFHLCountry);
		}

		[TestDate(2006, 6, 6)]
		public void TestUpdateAWBPrinted()
		{
			var consol = Factory.New<ForwardingConsolForTest>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.MasterBillAirlinePrefix = "999";

			var mawb = Factory.New<JobMawb>();
			mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
			mawb.JM_ParentID = consol.PK;
			mawb.JM_IsPrinted = ZBool.False;
			mawb.JM_MAWB = "10000001";
			mawb.JM_Airline3DigitPrefix = "999";

			Factory.Save();

			consol.JK_IsNeutralMaster = true;

			AssertEquals("Preconfiguration: Ensure Issue date is empty", ZDateTime.Empty, consol.JK_MasterBillIssueDate);
			AssertEquals("Preconfiguration: Ensure FinalMAWBPrintedDate is empty", ZDateTime.Empty, consol.FinalMAWBPrintedDate);
			AssertEquals("Preconfiguration: Ensure IsFinalMAWBPrinted is false", false, consol.IsFinalMAWBPrinted);

			consol.UpdateAWBPrinted();

			var log = StmALogEntryLocator.Instance.GetLastPostEventOfType(consol, AutoEvents.DocumentSent);

			AssertEquals("MAWB document is printed", ZBool.True, mawb.JM_IsPrinted);
			AssertEquals("MAWBPrinted log is generated", ForwardingConsol.MAWBPrintedLogReference, log.SL_Reference);
			AssertEquals("Issue date has been set", ZDateTime.Today, consol.JK_MasterBillIssueDate);
			AssertEquals("FinalMAWBPrintedDate has been set", ZDateTime.Today, consol.FinalMAWBPrintedDate);
			AssertEquals("FinalMAWBPrinted has been set", true, consol.IsFinalMAWBPrinted);
			AssertEquals(true, consol.GetAWBIssueDateCalled);

			consol.JK_IsNeutralMaster = false;
			Factory.Save();

			AssertEquals("Ensure FinalMAWBPrintedDate is empty when printed mawb has been unallocated", ZDateTime.Empty, consol.FinalMAWBPrintedDate);
		}

		[ExpectNoExceptions]
		public void TestConcurrencyErrorOnUpdateAWBPrintedCore()
		{
			var factory1 = new BusinessObjectFactory();
			var consol = factory1.New<ForwardingConsol>();

			JobMawb mawb = factory1.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "081";
			mawb.JM_MAWB = "00000011";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_ServiceLevel = "STD";
			factory1.Save();

			consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals("prerequisite", ZDateTime.Empty, consol.JK_MasterBillIssueDate);

			consol.JK_AgentType = "CLD";
			consol.JK_IsNeutralMaster = true;
			consol.JK_DatePortOfFirstArrival = new ZDateTime(2012, 10, 25);
			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			var consol1 = factory2.Load<ForwardingConsol>(consol.PK);
			factory2.RefreshEnabled = false;
			consol1.JK_DatePortOfFirstArrival = new ZDateTime(2012, 10, 24);

			consol.JK_MasterBillIssueDateInfo.ValueChanged += (s, e) => AssertNoExceptionThrown("No concurrency exception will be thrown", factory2.Save);
			consol.UpdateAWBPrinted();
			AssertEquals("MasterBillIssueDate has not been set", ZDateTime.Empty, Consol.JK_MasterBillIssueDate);
		}

		public void TestUpdateAWBPrintedSavesNotOverriddenAWB()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = "CLD";
			consol.JK_TransportMode = Constants.TransportModes.Air;

			AssertEquals(false, consol.AWBHeader.IsInDatabase);

			Factory.Save();
			AssertEquals("Still not in database because not overridden", false, consol.AWBHeader.IsInDatabase);

			consol.UpdateAWBPrinted();
			AssertEquals("In database after printing final master", true, consol.AWBHeader.IsInDatabase);

			consol.AWBHeader.EH_AlsoNotifyAddress = "AAA";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedConsol = newFactory.Load<ForwardingConsol>(consol.PK);
			AssertNotEquals("The system doesn't save changed AWB even after the final master was printed", "AAA", loadedConsol.AWBHeader.EH_AlsoNotifyAddress);

			consol.AWBHeader.EH_AlsoNotifyAddress = "BBB";
			consol.UpdateAWBPrinted();

			newFactory = new BusinessObjectFactory();
			loadedConsol = newFactory.Load<ForwardingConsol>(consol.PK);
			AssertEquals("Printing the final master second time re-saves AWB", "BBB", loadedConsol.AWBHeader.EH_AlsoNotifyAddress);
		}

		public void TestLaserHAWBPrintedLog()
		{
			ForwardingConsolForTest consol = base.Factory.New<ForwardingConsolForTest>();

			StmALog log = StmALogEntryLocator.Instance.GetLastPostEventOfType(consol, AutoEvents.DocumentSent);
			AssertNull(log);

			DocumentZQuery filter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Laser HAWB");
			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);
			DocumentPrintedEventArgs eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem);

			ForwardingConsolForTest.ForwardingConsolForTestDocumentSupporter documentSupporter = consol.DocumentSupporterForForwardingConsolTest;
			documentSupporter.SetDocumentEventSource_DocumentPrinted(this, eventArgs);

			log = StmALogEntryLocator.Instance.GetLastPostEventOfType(consol, AutoEvents.DocumentSent);
			AssertEquals(ForwardingConsol.HAWBPrintedLogReference, log.SL_Reference);
		}

		public void TestNeutralHAWBPrintedLog()
		{
			ForwardingConsolForTest consol = base.Factory.New<ForwardingConsolForTest>();

			StmALog log = StmALogEntryLocator.Instance.GetLastPostEventOfType(consol, AutoEvents.DocumentSent);
			AssertNull(log);

			DocumentZQuery filter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Neutral HAWB");
			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);
			DocumentPrintedEventArgs eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem);

			ForwardingConsolForTest.ForwardingConsolForTestDocumentSupporter documentSupporter = consol.DocumentSupporterForForwardingConsolTest;
			documentSupporter.SetDocumentEventSource_DocumentPrinted(this, eventArgs);

			log = StmALogEntryLocator.Instance.GetLastPostEventOfType(consol, AutoEvents.DocumentSent);
			AssertEquals(ForwardingConsol.HAWBPrintedLogReference, log.SL_Reference);
		}

		#endregion

		public void TestShipmentCount()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AssertEquals("Shipmentcount Should be zero", 0, consol.ShipmentCount);

			consol.Shipments.AddNew();
			AssertEquals("Shipmentcount Should be 1", 1, consol.ShipmentCount);

			consol.Shipments.AddNew();
			AssertEquals("Shipmentcount Should be 2", 2, consol.ShipmentCount);

			consol.Shipments.RemoveAll();
			AssertEquals("Shipmentcount Should be zero", 0, consol.ShipmentCount);
		}

		public void TestCarrierServiceLevels_NeutralAirWaybillServiceLevelList()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			OrgCarrierServiceLevel lvl = carrier.MiscServ.CarrierServiceLevels.AddNew();
			lvl.PL_Code = "XXX";
			lvl.PL_CarrierServiceLevelDescription = "XXX";
			lvl = carrier.MiscServ.CarrierServiceLevels.AddNew();
			lvl.PL_Code = "DEF";
			lvl.PL_CarrierServiceLevelDescription = "DEF";

			OrgHeader carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier2.OH_IsAirLine = true;
			lvl = carrier2.MiscServ.CarrierServiceLevels.AddNew();
			lvl.PL_Code = "MON";
			lvl.PL_CarrierServiceLevelDescription = "Danny ate a little lamb";

			Factory.Save();

			AssertEquals("STD Only", 1, consol.NeutralAirWaybillServiceLevelList.Count);

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			AssertEquals("Two defined service levels + STD", 3, consol.NeutralAirWaybillServiceLevelList.Count);

			consol.JK_OA_ShippingLineAddress = carrier2.MainAddress.PK;
			AssertEquals("One defined service level + STD", 2, consol.NeutralAirWaybillServiceLevelList.Count);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			AssertEquals("STD Only", 1, consol.NeutralAirWaybillServiceLevelList.Count);
		}

		[GuiTest]
		public void TestWorkflowRelationshipsWhenShipmentAdded()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Misc = false;

			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, WorkflowDescriptors.JobConsolWorkflowDescriptorCode, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);

			var conTemplate = Factory.New<ProcessTaskTemplate>();
			conTemplate.P0_Name = "Consol Template for Workflow Links";
			conTemplate.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;
			var conTemplateWorkflow = helper.CreateWorkflow(conTemplate, "Consol Workflow");
			helper.CreateTask(conTemplate, conTemplateWorkflow);

			var shpTemplate = Factory.New<ProcessTaskTemplate>();
			shpTemplate.P0_Name = "Shipment Template for Workflow Links";
			shpTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			var shpTemplateWorkflow = helper.CreateWorkflow(shpTemplate, "Shipment Workflow");
			helper.CreateTask(shpTemplate, shpTemplateWorkflow);

			helper.CreateDependencyLink(conTemplate, conTemplateWorkflow, shpTemplateWorkflow);

			Factory.Save();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_IsConsignee = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";

			Factory.Save();

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ActualWeight = 20m;
			shipment.JS_ActualVolume = 1m;
			shipment.CreateShipmentJobHeaderWithMutex();
			shipment.JobHeader.JH_GE = department.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			Factory.Save();

			var shipmentHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			var shpWorkflows = shipmentHeader.ProcessHeaders.Cast<IProcessHeader>().ToArray();
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Shipment Workflow" }, shipmentHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));

			consol.Shipments.Add(shipment);

			Factory.Save();

			conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);

			shipmentHeader = helper.GetJobHeaderForParent(shipment, Factory, addDefaultProcessHeaderIfNone: false);
			shpWorkflows = shipmentHeader.ProcessHeaders.Cast<IProcessHeader>().ToArray();
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Shipment Workflow" }, shipmentHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));

			var shpWorkflow = shpWorkflows.First(x => x.FH_CompletionStatement == "Shipment Workflow");
			var links = shpWorkflow.Links.ToArray();
			AssertEquals("There should be one link between workflows.", 1, links.Length);

			var link = links.Single();
			AssertEquals("The link should be between the shipment and the consol.", shpWorkflow.PK, link.FP_FH_HeaderTo);

			var conWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single();
			AssertEquals("The link should be between the shipment and the consol.", conWorkflow.PK, link.FP_FH_HeaderFrom);
		}

		[GuiTest]
		public void TestWorkflowRelationshipsWhenContainerAdded()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, WorkflowDescriptors.JobConsolWorkflowDescriptorCode, WorkflowDescriptors.ContainerWorkflowDescriptorCode);

			var conTemplate = Factory.New<ProcessTaskTemplate>();
			conTemplate.P0_Name = "Consol Template for Workflow Links";
			conTemplate.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;
			var conTemplateWorkflow = helper.CreateWorkflow(conTemplate, "Consol Workflow");
			helper.CreateTask(conTemplate, conTemplateWorkflow);

			var cntTemplate = Factory.New<ProcessTaskTemplate>();
			cntTemplate.P0_Name = "Container Template for Workflow Links";
			cntTemplate.P0_ProcessType = WorkflowDescriptors.ContainerWorkflowDescriptorCode;
			var cntTemplateWorkflow = helper.CreateWorkflow(cntTemplate, "Container Workflow");
			helper.CreateTask(cntTemplate, cntTemplateWorkflow);

			helper.CreateDependencyLink(conTemplate, conTemplateWorkflow, cntTemplateWorkflow);

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";

			Factory.Save();

			var conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Consol Workflow" }, conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));

			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			container.JC_ContainerNum = "CNTR001";
			container.JC_RC = Factory.LoadTop1(typeof(RefContainer), new ZQuery(RefContainerSchema.RC_Code, "PM-2H")).PK;
			container.JC_ContainerMode = Constants.ContainerModes.ULD;

			consol.Containers.Add(container);

			Factory.Save();

			conJobHeader = helper.GetJobHeaderForParent(consol, Factory, addDefaultProcessHeaderIfNone: false);

			var cntJobHeader = helper.GetJobHeaderForParent(container, Factory, addDefaultProcessHeaderIfNone: false);
			var cntWorkflows = cntJobHeader.ProcessHeaders.Cast<IProcessHeader>().ToArray();
			AssertContainsExactElementsInAnyOrder("The template workflow should be applied.", new[] { "Container Workflow" }, cntJobHeader.ProcessHeaders.Cast<IProcessHeader>().Select(x => x.FH_CompletionStatement));

			var cntWorkflow = cntWorkflows.First(x => x.FH_CompletionStatement == "Container Workflow");
			var links = cntWorkflow.Links.ToArray();
			AssertEquals("There should be one link between workflows.", 1, links.Length);

			var link = links.Single();
			AssertEquals("The link should be between the container and the consol.", cntWorkflow.PK, link.FP_FH_HeaderTo);

			var conWorkflow = conJobHeader.ProcessHeaders.Cast<IProcessHeader>().Single();
			AssertEquals("The link should be between the container and the consol.", conWorkflow.PK, link.FP_FH_HeaderFrom);
		}

		public void TestAWBDimsCodeDescriptionPairList()
		{
			AssertNotNull(Consol.AWBDimsCodeDescriptionPairList);
			AssertEquals(OLookUpEditType.AWBDimensions, Consol.AWBDimsCodeDescriptionPairList.LookupEditType);
		}

		public void TestSetDefaultValues()
		{
			Env.Registry.Freight.AirWaybill.MAWBDimensionsDefault = Core.Constants.AWB.Dimensions.DEF;
			DocumentsDataRegistry.Instance.ReleaseType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "EBL");

			try
			{
				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				AssertEquals(Core.Constants.AgentType.Agent, consol.JK_AgentType);
				AssertEquals(Core.Constants.AWB.Dimensions.DEF, consol.JK_PrintOptionForPackagesOnAWB);
				AssertEquals(consol.JK_ReleaseType, "EBL");

				Env.Registry.Freight.AirWaybill.MAWBDimensionsDefault = Core.Constants.AWB.Dimensions.M3;
				DocumentsDataRegistry.Instance.ReleaseType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NON");
				ForwardingConfigurationRegistry.Instance.ConsolTypeDefaultForConsol.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.AgentType.Direct);

				consol = Factory.New<ForwardingConsol>();
				AssertEquals(Core.Constants.AgentType.Direct, consol.JK_AgentType);
				AssertEquals(Core.Constants.AWB.Dimensions.M3, consol.JK_PrintOptionForPackagesOnAWB);
				AssertEquals(consol.JK_ReleaseType, "NON");
				AssertEquals("IsNeutral Master is defaulted to false", false, consol.JK_IsNeutralMaster);
			}
			finally
			{
				Env.Registry.Freight.AirWaybill.MAWBDimensionsDefault = Core.Constants.AWB.Dimensions.DEF;
			}
		}

		public void TestSetDefaultValues_JK_SendingForwarderHandlingType()
		{
			var forwarder = GetForwarder();
			GlbDepartment.CurrentDepartment.GE_Sea = true;
			GlbDepartment.CurrentDepartment.GE_Export = true;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			using (ForwardingConfigurationRegistry.Instance.ConsolTypeDefaultForConsol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AgentType.Agent))
			{
				var consol = Factory.New<ForwardingConsol>();

				CombineAssertions("Pre-Conditions - not a gateway department, no gateway or freight handling details", () =>
				{
					Assert("Not gateway department", !GlbDepartment.CurrentDepartment.IsGatewayDepartment);
					AssertEquals("AUSYD", consol.JK_RL_NKLoadPort);
					AssertEquals(TransportModes.Sea, consol.JK_TransportMode);
					AssertEquals(AgentType.Agent, consol.JK_AgentType);
					AssertEquals(
						"Sending forwarder address defaults to current branch address proxy",
						GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK,
						consol.JK_OA_SendingForwarderAddress
					);

					AssertEquals("Sending forwarder handler type is not defaulted", ZString.Empty, consol.JK_SendingForwarderHandlingType);
				});

				var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GES"));
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, department.PK.ToGuid()))
				{
					GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

					var address = AddForwarderAgentAddress(
						forwarder,
						"AUSYD",
						TransportModes.Sea,
						AgentDirectionList.Codes.Both,
						AgentStatusList.Codes.GatewayAgent
					);

					GlbBranch.CurrentBranch.GB_OH_OrgProxy = address.Header.PK;
					GlbBranch.CurrentBranch.Factory.Save();

					consol = new BusinessObjectFactory().New<ForwardingConsol>();

					CombineAssertions("Gateway department, Gateway address without freight handling details", () =>
					{
						Assert("Department is Gateway", GlbDepartment.CurrentDepartment.IsGatewayDepartment);
						AssertEquals("AUSYD", consol.JK_RL_NKLoadPort);
						AssertEquals(TransportModes.Sea, consol.JK_TransportMode);
						AssertEquals(AgentType.Agent, consol.JK_AgentType);
						AssertEquals(
							"Sending forwarder address defaults to current branch address proxy",
							GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK,
							consol.JK_OA_SendingForwarderAddress
						);

						AssertEquals("Sending forwarder handler type is not defaulted", ZString.Empty, consol.JK_SendingForwarderHandlingType);
					});

					AddForwarderAgentAddress(
						forwarder,
						address,
						"AUSYD",
						TransportModes.Sea,
						AgentDirectionList.Codes.Both,
						AgentStatusList.Codes.Published
					);

					consol = new BusinessObjectFactory().New<ForwardingConsol>();

					CombineAssertions("Gateway department + Gateway address has Freight Handling Details", () =>
					{
						AssertEquals(
							"Sending forwarder address defaults to gateway",
							address.PK,
							consol.JK_OA_SendingForwarderAddress
						);

						AssertEquals(
							"Receiving forwarder handler type is defaulted",
							AgentStatusList.Codes.GatewayAgent,
							consol.JK_SendingForwarderHandlingType
						);
					});
				}
			}
		}

		public void TestSetDefaultValues_Sending_Receiving_Agents()
		{
			var forwarder = GetForwarder();
			var exportOrgAddress = AddForwarderAgentAddress(
				forwarder,
				HomePort,
				TransportModes.Air,
				AgentDirectionList.Codes.Export,
				AgentStatusList.Codes.Published
			);

			var importOrgAddress = AddForwarderAgentAddress(
				forwarder,
				HomePort,
				TransportModes.Air,
				AgentDirectionList.Codes.Import,
				AgentStatusList.Codes.Published
			);

			TestSetDefaultValues_Sending_Receiving_Agents_Case(AgentType.Agent, "domestic", exportOrgAddress, importOrgAddress);
			TestSetDefaultValues_Sending_Receiving_Agents_Case(AgentType.Agent, "export", exportOrgAddress, importOrgAddress);
			TestSetDefaultValues_Sending_Receiving_Agents_Case(AgentType.Agent, "import", exportOrgAddress, importOrgAddress);
			TestSetDefaultValues_Sending_Receiving_Agents_Case(AgentType.Direct, "domestic", exportOrgAddress, importOrgAddress);
			TestSetDefaultValues_Sending_Receiving_Agents_Case(AgentType.Direct, "export", exportOrgAddress, importOrgAddress);
			TestSetDefaultValues_Sending_Receiving_Agents_Case(AgentType.Direct, "import", exportOrgAddress, importOrgAddress);
		}

		void TestSetDefaultValues_Sending_Receiving_Agents_Case(string agentType, string departmentDirection, OrgAddress exportOrgAddress, OrgAddress importOrgAddress)
		{
			using (ForwardingConfigurationRegistry.Instance.ConsolTypeDefaultForConsol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, agentType))
			{
				var isDomestic = string.Equals(departmentDirection, "domestic", StringComparison.OrdinalIgnoreCase);
				var isExport = string.Equals(departmentDirection, "export", StringComparison.OrdinalIgnoreCase);
				var isImport = string.Equals(departmentDirection, "import", StringComparison.OrdinalIgnoreCase);

				GlbDepartment.CurrentDepartment.GE_Domestic = isDomestic;
				GlbDepartment.CurrentDepartment.GE_Export = isExport;
				GlbDepartment.CurrentDepartment.GE_Import = isImport;
				GlbDepartment.CurrentDepartment.GE_Air = true;

				var consol = Factory.New<ForwardingConsol>();

				if (agentType == AgentType.Direct)
				{
					AssertEquals("Default sending agent should be empty when consol type is direct",
						ZGuid.Empty, consol.JK_OA_SendingForwarderAddress);
					AssertEquals("Default receiving agent should be empty when consol type is direct",
						ZGuid.Empty, consol.JK_OA_ReceivingForwarderAddress);
				}
				else
				{
					if (isDomestic)
					{
						AssertEquals("Domestic sending agent should default to branch main address",
							GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, consol.JK_OA_SendingForwarderAddress);
						AssertEquals("Domestic receiving agent should default to branch main address",
							GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, consol.JK_OA_ReceivingForwarderAddress);
					}

					if (isExport)
					{
						AssertEquals("Export sending agent should default to export agent address",
							exportOrgAddress.PK, consol.JK_OA_SendingForwarderAddress);
					}

					if (isImport)
					{
						AssertEquals("Import receiving agent should default to import agent address",
							importOrgAddress.PK, consol.JK_OA_ReceivingForwarderAddress);
					}
				}
			}
		}

		public void TestSetDefaultValues_JK_ReceivingForwarderHandlingType()
		{
			var forwarder = GetForwarder();
			GlbDepartment.CurrentDepartment.GE_Sea = true;
			GlbDepartment.CurrentDepartment.GE_Import = true;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			using (ForwardingConfigurationRegistry.Instance.ConsolTypeDefaultForConsol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AgentType.Agent))
			{
				var consol = Factory.New<ForwardingConsol>();

				CombineAssertions("Pre-Conditions - not a gateway department, no gateway or freight handling details", () =>
				{
					Assert("Not gateway department", !GlbDepartment.CurrentDepartment.IsGatewayDepartment);
					AssertEquals("AUSYD", consol.JK_RL_NKDischargePort);
					AssertEquals(TransportModes.Sea, consol.JK_TransportMode);
					AssertEquals(AgentType.Agent, consol.JK_AgentType);
					AssertEquals(
						"Receiving forwarder address defaults to current branch address proxy",
						GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK,
						consol.JK_OA_ReceivingForwarderAddress
					);

					AssertEquals("Receiving forwarder handler type is not defaulted", ZString.Empty, consol.JK_ReceivingForwarderHandlingType);
				});

				var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GIS"));
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, department.PK.ToGuid()))
				{
					GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

					var address = AddForwarderAgentAddress(
						forwarder,
						"AUSYD",
						TransportModes.Sea,
						AgentDirectionList.Codes.Both,
						AgentStatusList.Codes.GatewayAgentWithTariff
					);

					GlbBranch.CurrentBranch.GB_OH_OrgProxy = address.Header.PK;
					GlbBranch.CurrentBranch.Factory.Save();

					consol = new BusinessObjectFactory().New<ForwardingConsol>();

					CombineAssertions("Gateway department, Gateway address without freight handling details", () =>
					{
						Assert("Department is Gateway", GlbDepartment.CurrentDepartment.IsGatewayDepartment);
						AssertEquals("AUSYD", consol.JK_RL_NKDischargePort);
						AssertEquals(TransportModes.Sea, consol.JK_TransportMode);
						AssertEquals(AgentType.Agent, consol.JK_AgentType);
						AssertEquals(
							"Receiving forwarder address defaults to current branch address proxy",
							GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK,
							consol.JK_OA_ReceivingForwarderAddress
						);

						AssertEquals("Receiving forwarder handler type is not defaulted", ZString.Empty, consol.JK_ReceivingForwarderHandlingType);
					});

					AddForwarderAgentAddress(
						forwarder,
						address,
						"AUSYD",
						TransportModes.Sea,
						AgentDirectionList.Codes.Both,
						AgentStatusList.Codes.Published
					);

					consol = new BusinessObjectFactory().New<ForwardingConsol>();

					CombineAssertions("Gateway department + Gateway address has Freight Handling Details", () =>
					{
						AssertEquals(
							"Receiving forwarder address defaults to gateway",
							address.PK,
							consol.JK_OA_ReceivingForwarderAddress
						);

						AssertEquals(
							"Receiving forwarder handler type is defaulted",
							AgentStatusList.Codes.GatewayAgentWithTariff,
							consol.JK_ReceivingForwarderHandlingType
						);
					});
				}
			}
		}

		public void TestShipmentGatewaySynchronization_JK_SendingForwarderHandlingType_Changed()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			shipment.Consols.Add(consol);
			AssertEquals("Consol count", 1, shipment.Consols.Count);
			AssertEquals("Gateways count", 0, shipment.Gateways.Count);

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			AssertEquals("Gateways count", 1, shipment.Gateways.Count);
			AssertEquals("Gateway shipment: SendingForwarder", shipment.PK, shipment.Gateways[0].JSG_JS_Shipment);
			AssertEquals("Gateway Sequence: SendingForwarder", 1, shipment.Gateways[0].JSG_Sequence.ToZInt());
			AssertEquals("Gateway: SendingForwarderAddress", consol.SendingForwarderAddress.PK, shipment.Gateways[0].JSG_OA_ForwarderAddress);

			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			AssertEquals("Gateways count", 1, shipment.Gateways.Count);
			AssertEquals("Gateway shipment: SendingForwarder", shipment.PK, shipment.Gateways[0].JSG_JS_Shipment);
			AssertEquals("Gateway Sequence: SendingForwarder", 1, shipment.Gateways[0].JSG_Sequence.ToZInt());
			AssertEquals("Gateway: SendingForwarderAddress", consol.SendingForwarderAddress.PK, shipment.Gateways[0].JSG_OA_ForwarderAddress);

			consol.JK_SendingForwarderHandlingType = ZString.Empty;
			AssertEquals("Gateways count", 0, shipment.Gateways.Count);
		}

		public void TestShipmentGatewaySynchronization_JK_ReceivingForwarderHandlingType_Changed()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			shipment.Consols.Add(consol);
			AssertEquals("Consol count", 1, shipment.Consols.Count);
			AssertEquals("Gateways count", 0, shipment.Gateways.Count);

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			AssertEquals("Gateways count", 1, shipment.Gateways.Count);
			AssertEquals("Gateway shipment: ReceivingForwarder", shipment.PK, shipment.Gateways[0].JSG_JS_Shipment);
			AssertEquals("Gateway Sequence: ReceivingForwarder", 1, shipment.Gateways[0].JSG_Sequence.ToZInt());
			AssertEquals("Gateway: ReceivingForwarderAddress", consol.ReceivingForwarderAddress.PK, shipment.Gateways[0].JSG_OA_ForwarderAddress);

			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			AssertEquals("Gateways count", 1, shipment.Gateways.Count);
			AssertEquals("Gateway shipment: SendingForwarder", shipment.PK, shipment.Gateways[0].JSG_JS_Shipment);
			AssertEquals("Gateway Sequence: ReceivingForwarder", 1, shipment.Gateways[0].JSG_Sequence.ToZInt());
			AssertEquals("Gateway: ReceivingForwarderAddress", consol.ReceivingForwarderAddress.PK, shipment.Gateways[0].JSG_OA_ForwarderAddress);

			consol.JK_ReceivingForwarderHandlingType = ZString.Empty;
			AssertEquals("Gateways count", 0, shipment.Gateways.Count);
		}

		public void TestShipmentGatewaySynchronization_JK_OA_SendingForwarderAddress_Changed()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			shipment.Consols.Add(consol);
			AssertEquals("Consol count", 1, shipment.Consols.Count);
			AssertEquals("Gateways count", 0, shipment.Gateways.Count);

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			AssertEquals("Gateways count", 1, shipment.Gateways.Count);
			AssertEquals("Gateway shipment: SendingForwarder", shipment.PK, shipment.Gateways[0].JSG_JS_Shipment);
			AssertEquals("Gateway Sequence: SendingForwarder", 1, shipment.Gateways[0].JSG_Sequence.ToZInt());
			AssertEquals("Gateway: SendingForwarderAddress", sendingForwarder.MainAddress.PK, shipment.Gateways[0].JSG_OA_ForwarderAddress);

			var sendingForwarder2 = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_SendingForwarderAddress = sendingForwarder2.MainAddress.PK;

			AssertEquals("Gateways count", 1, shipment.Gateways.Count);
			AssertEquals("Gateway shipment: SendingForwarder", shipment.PK, shipment.Gateways[0].JSG_JS_Shipment);
			AssertEquals("Gateway Sequence: SendingForwarder", 1, shipment.Gateways[0].JSG_Sequence.ToZInt());
			AssertEquals("Gateway: SendingForwarderAddress", sendingForwarder2.MainAddress.PK, shipment.Gateways[0].JSG_OA_ForwarderAddress);

			var sendingForwarder3 = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_SendingForwarderAddress = sendingForwarder3.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = ZString.Empty;
			AssertEquals("Gateways count", 0, shipment.Gateways.Count);
		}

		public void TestShipmentGatewaySynchronization_JK_OA_ReceivingForwarderAddress_Changed()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			shipment.Consols.Add(consol);
			AssertEquals("Consol count", 1, shipment.Consols.Count);
			AssertEquals("Gateways count", 0, shipment.Gateways.Count);

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			AssertEquals("Gateways count", 1, shipment.Gateways.Count);
			AssertEquals("Gateway shipment: ReceivingForwarder", shipment.PK, shipment.Gateways[0].JSG_JS_Shipment);
			AssertEquals("Gateway Sequence: ReceivingForwarder", 1, shipment.Gateways[0].JSG_Sequence.ToZInt());
			AssertEquals("Gateway: ReceivingForwarderAddress", receivingForwarder.MainAddress.PK, shipment.Gateways[0].JSG_OA_ForwarderAddress);

			var receivingForwarder2 = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder2.MainAddress.PK;
			AssertEquals("Gateways count", 1, shipment.Gateways.Count);
			AssertEquals("Gateway shipment: SendingForwarder", shipment.PK, shipment.Gateways[0].JSG_JS_Shipment);
			AssertEquals("Gateway Sequence: ReceivingForwarder", 1, shipment.Gateways[0].JSG_Sequence.ToZInt());
			AssertEquals("Gateway: ReceivingForwarderAddress", receivingForwarder2.MainAddress.PK, shipment.Gateways[0].JSG_OA_ForwarderAddress);

			var receivingForwarder3 = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder3.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = ZString.Empty;
			AssertEquals("Gateways count", 0, shipment.Gateways.Count);
		}

		public void TestSetDefaultValues_JK_Phase()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			AssertEquals("Default phase", PhaseConstants.Phase.ALL, consol1.JK_Phase);

			PhaseSecurity security = new PhaseSecurity(PhaseConstants.GetShipmentLocationsList());

			Phase phase = security.Phases.AddNew();
			phase.Code = "AAA";
			phase.Description = (NoResString)"Phase AAA";

			PhaseRule rule = phase.Rules.AddNew();
			rule.Location = PhaseConstants.Locations.AnyLocation;
			rule.DepartmentPK = GlbDepartment.CurrentDepartment.PK;

			ForwardingConfigurationRegistry.Instance.ConsolPhaseSecurity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, security);
			ForwardingConfigurationRegistry.Instance.ConsolDefaultPhase.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "AAA");

			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			AssertEquals("Default phase updated", "AAA", consol2.JK_Phase);
		}

		public void TestSetJK_ReleaseType()
		{
			ReleaseTypes releaseTypes = FreightDataRegistry.Instance.ReleaseTypes.DefaultValue;
			releaseTypes.OriginalsNumber = 5;
			releaseTypes.CopiesNumber = 6;

			ReleaseType expressReleaseType = releaseTypes.Types.FindByCode(Core.Constants.ShipmentReleaseTypes.ExpressBofL);
			expressReleaseType.OriginalsNumber = 0;
			expressReleaseType.CopiesNumber = 4;

			FreightDataRegistry.Instance.ReleaseTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, releaseTypes);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AssertEquals("Default from ReleaseTypes registry value.", (byte)5, consol.JK_NoOriginalBills);
			AssertEquals("Default from ReleaseTypes registry value.", (byte)6, consol.JK_NoCopyBills);

			consol.JK_ReleaseType = Core.Constants.ShipmentReleaseTypes.ExpressBofL;
			AssertEquals("Originals should be 0", (byte)0, consol.JK_NoOriginalBills);
			AssertEquals("Default Copies from Express Bills registry value.", (byte)4, consol.JK_NoCopyBills);
		}

		public void TestJK_IsNeutralMasterDefaultedWhenMAWBInStock()
		{
			JobMawb jobMawb = Factory.New<JobMawb>();
			jobMawb.JM_IsPaper = false;
			jobMawb.JM_Airline3DigitPrefix = "160";
			jobMawb.JM_IsPrinted = false;
			jobMawb.JM_MAWB = "00012353";
			jobMawb.JM_ServiceLevel = "STD";
			jobMawb.JM_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			ForwardingConsol newConsol = Factory.New<ForwardingConsol>();
			newConsol.JK_AgentType = Constants.AgentType.CoLoad;
			newConsol.JK_TransportMode = Constants.TransportModes.Air;
			newConsol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			newConsol.JK_AWBServiceLevel = "STD";

			newConsol.MasterBillAirlinePrefix = "CX";

			AssertEquals("Jk_IsNeutralMaster won't set to true for a coload consol even though there is mawb stock ", false, newConsol.JK_IsNeutralMaster);

			newConsol = Factory.New<ForwardingConsol>();
			newConsol.JK_AgentType = Constants.AgentType.Agent;
			newConsol.JK_TransportMode = Constants.TransportModes.Air;
			newConsol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			newConsol.JK_AWBServiceLevel = "STD";

			AssertEquals("Precondition: JK_IsNeutralMaster is defaulted to be false", false, newConsol.JK_IsNeutralMaster);
			AssertEquals("Precondition: Service Level is standard", "STD", newConsol.JK_AWBServiceLevel);

			newConsol.MasterBillAirlinePrefix = "160";

			AssertEquals("Jk_IsNeutralMaster is set to true", true, newConsol.JK_IsNeutralMaster);
		}

		public void TestLoadFromRef()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "135";
			AssertNotNull(ForwardingConsol.LoadFromRef(Factory, "135"));
		}

		public void TestResetAddressPickerDropLists()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_OA_SendingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			consol.SendingForwarder.MainAddress.OA_Address1 = "TEST";

			AssertEquals(2, consol.AWBHeader.ShipperAddressPickList.Count);

			OrgAddress orgAddress = consol.SendingForwarder.Addresses.AddNew();
			orgAddress.OA_Address1 = "TEST2";
			consol.ResetAddressPickerDropLists();
			AssertEquals(3, consol.AWBHeader.ShipperAddressPickList.Count);
		}

		public void TestSetReleaseTypeWhileAttachingShipmentToConsol()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_ReleaseType = "ABC";

			ForwardingShipment shipment = consol.Shipments.AddNew();
			consol.Shipments.Add(shipment);

			AssertEquals(shipment.JS_ReleaseType, "ABC");

			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			shipment = consol.Shipments.AddNew();
			consol.Shipments.Add(shipment);

			AssertEquals(shipment.JS_ReleaseType, "");
		}

		public void TestLocalTransportList()
		{
			AssertNotNull("Local transport list collection", Consol.LocalTransportList);
		}

		public void TestMAWBConcurrencyForMAWBNumber()
		{
			var mawb1 = CreateMAWB(Factory, "001", "00000011");
			var mawb2 = CreateMAWB(Factory, "001", "00000022");
			var mawb3 = CreateMAWB(Factory, "001", "00000033");

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.MasterBillAirlinePrefix = "001";

			Factory.Save();

			AssertEquals("mawb 00100000011 allocated", "00100000011", consol.JK_MasterBillNum);
			AssertEquals("mawb 00100000011 allocated", consol.PK, mawb1.JM_ParentID);
			AssertEquals("mawb 00100000022 not allocated", ZGuid.Empty, mawb2.JM_ParentID);
			AssertEquals("mawb 00100000033 not allocated", ZGuid.Empty, mawb3.JM_ParentID);

			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;

			var consolFactory1 = factory1.Load<ForwardingConsol>(consol.PK);
			var mawb1Factory1 = factory1.Load<JobMawb>(mawb1.PK);
			var mawb2Factory1 = factory1.Load<JobMawb>(mawb2.PK);
			var mawb3Factory1 = factory1.Load<JobMawb>(mawb3.PK);

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var consolFactory2 = factory2.Load<ForwardingConsol>(consol.PK);
			var mawb1Factory2 = factory2.Load<JobMawb>(mawb1.PK);
			var mawb2Factory2 = factory2.Load<JobMawb>(mawb2.PK);
			var mawb3Factory2 = factory2.Load<JobMawb>(mawb3.PK);

			consolFactory1.JK_IsNeutralMaster = false;
			factory1.Save();
			consolFactory1.JK_BookingReference = "00100000022";
			consolFactory1.JK_IsNeutralMaster = true;
			factory1.Save();

			consolFactory2.JK_IsNeutralMaster = false;
			consolFactory2.JK_BookingReference = "00100000033";
			consolFactory2.JK_IsNeutralMaster = true;

			for (int i = 0; i < 2; i++)
			{
				try
				{
					factory2.Save();
				}
				catch (ZSaveConcurrencyException exc)
				{
					ZExceptionReporting.HandleSaveException(exc);
				}

				AssertEquals("consol causing concurrency exception hasn't been saved", true, consolFactory2.HasChanges);
				AssertEquals("master bill number hasn't been merged", "00100000022", consolFactory2.JK_MasterBillNum);
				mawb1Factory1.Reload();
				AssertEquals("mawb 00100000011 deallocated", ZGuid.Empty, mawb1Factory1.JM_ParentID);
				AssertEquals("mawb 00100000022 allocated", consolFactory1.PK, mawb2Factory1.JM_ParentID);
				AssertEquals("mawb 00100000033 not allocated", ZGuid.Empty, mawb3Factory1.JM_ParentID);

				AssertContains("The system cannot automatically merge your changes because there are conflicts with critical fields",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNoDuplicateMAWBSAreAllocated()
		{
			var mawb1 = CreateMAWB(Factory, "001", "00000011");
			var mawb2 = CreateMAWB(Factory, "001", "00000022");

			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;

			var consol1Factory1 = factory1.New<ForwardingConsol>();
			consol1Factory1.JK_TransportMode = Constants.TransportModes.Air;
			consol1Factory1.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol1Factory1.MasterBillAirlinePrefix = "001";
			consol1Factory1.JK_IsNeutralMaster = true;

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var consol2Factory2 = factory2.New<ForwardingConsol>();
			consol2Factory2.JK_TransportMode = Constants.TransportModes.Air;
			consol2Factory2.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol2Factory2.MasterBillAirlinePrefix = "001";
			consol2Factory2.JK_IsNeutralMaster = true;

			var mawb1Factory1 = factory1.Load<JobMawb>(mawb1.PK);
			var mawb2Factory1 = factory1.Load<JobMawb>(mawb2.PK);
			var mawb1Factory2 = factory2.Load<JobMawb>(mawb1.PK);
			var mawb2Factory2 = factory2.Load<JobMawb>(mawb2.PK);

			factory1.Save();
			factory2.Save();

			AssertEquals("MAWB1 allocated to ConsolFactory1", consol1Factory1.PK, mawb1Factory1.JM_ParentID);
			AssertEquals("MAWB2 allocated to ConsolFactory2", consol2Factory2.PK, mawb2Factory2.JM_ParentID);
		}

		public void TestCanAllocateMAWBAfterAddingStock()
		{
			var firstFactory = new BusinessObjectFactory();
			var consol = firstFactory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.MasterBillAirlinePrefix = "001";
			consol.JK_IsNeutralMaster = true;

			AssertExceptionThrown(typeof(MAWBAllocationException), () => firstFactory.Save());

			var anotherFactory = new BusinessObjectFactory();

			var mawb1 = CreateMAWB(anotherFactory, "001", "00000011");
			var mawb2 = CreateMAWB(anotherFactory, "001", "00000022");

			anotherFactory.Save();
			firstFactory.Save();
			mawb1.Reload(); //Need to reload it, since the Notification Bus doesn't fire - there were no changes in MAWB upon Factory.Save()
			AssertEquals("MAWB1 allocated to Consol", consol.PK, mawb1.JM_ParentID);
		}

		public void TestDoNotSendErrorReporterWhenAnotherUserLoggedOnADifferentBrachAllocatedMawb()
		{
			Factory.RefreshEnabled = false;

			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

			var melBranch = company.Branches.AddNew();
			melBranch.GB_Code = "MEL";
			melBranch.GB_RL_NKHomePort = "AUMEL";

			var mawb = AddMawb("020", "00000001", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_MasterBillNum = "020";

			AssertNull("mawb not yet allocated to consol", consol.MAWBAllocation.AllocatedMawb);

			Factory.Save();

			consol.JK_IsNeutralMaster = true;

			Factory.Save();

			AssertEquals("correct mawb has been allocated to consol", mawb, consol.MAWBAllocation.AllocatedMawb);

			var otherFactory = new BusinessObjectFactory();
			otherFactory.RefreshEnabled = false;

			using (melBranch.SetAsTemporaryContext())
			{
				var consolOnOtherFactory = otherFactory.Load<ForwardingConsol>(consol.PK);

				Factory.Save();

				var query = new ZQuery(JobMawbSchema.PK, mawb.PK);
				query.FetchOnlyFromLocalCache = true;

				AssertNull("Prerequisite - mawb has not yet been loaded into other factory", otherFactory.LoadTop1<JobMawb>(query));
				consolOnOtherFactory.MAWBAllocation.MarkForAllocation();
				AssertExceptionThrown(typeof(MAWBAllocationException), () => consolOnOtherFactory.MAWBAllocation.PerformMAWBAllocation());
				consolOnOtherFactory.Validation.ValidateMasterBillNeutralMAWB();
				AssertHasWarning("Prerequisite - MasterBillMAWB is invalid due to lack of stock", consolOnOtherFactory.MasterBillNeutralMAWBInfo,
					"There are no MAWBs left for this Airline. Please add more numbers to your stock.");
			}
		}

		public void TestMAWBConcurrencyForMAWBNumber_AllocateMawbInDifferentSessions()
		{
			var mawb1 = CreateMAWB(Factory, "001", "00000011");
			var mawb2 = CreateMAWB(Factory, "001", "00000022");

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			AssertEquals("mawb not yet allocated", ZString.Empty, consol.JK_MasterBillNum);

			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;

			var consolFactory1 = factory1.Load<ForwardingConsol>(consol.PK);

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var consolFactory2 = factory2.Load<ForwardingConsol>(consol.PK);

			consolFactory1.JK_BookingReference = "00100000011";
			consolFactory2.JK_BookingReference = "00100000022";

			consolFactory1.MasterBillAirlinePrefix = "001";
			consolFactory2.MasterBillAirlinePrefix = "001";
			factory1.Save();

			for (int i = 0; i < 2; i++)
			{
				try
				{
					factory2.Save();
				}
				catch (ZSaveConcurrencyException exc)
				{
					ZExceptionReporting.HandleSaveException(exc);
				}

				AssertEquals("consol causing concurrency exception hasn't been saved", true, consolFactory2.HasChanges);
				AssertEquals("master bill number hasn't been merged", "001", consolFactory2.JK_MasterBillNum);

				AssertContains("The system cannot automatically merge your changes because there are conflicts with critical fields",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMAWBConcurrencyForMAWBNumber_Sea()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.JK_MasterBillNum = "BOL0001";

			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;

			var consolFactory1 = factory1.Load<ForwardingConsol>(consol.PK);

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var consolFactory2 = factory2.Load<ForwardingConsol>(consol.PK);

			consolFactory1.JK_MasterBillNum = "BOL002";
			consolFactory2.JK_MasterBillNum = "BOL003";

			factory1.Save();

			try
			{
				factory2.Save();
			}
			catch (ZSaveConcurrencyException exc)
			{
				ZExceptionReporting.HandleSaveException(exc);
			}

			AssertEquals("consol causing concurrency exception hasn't been saved", true, consolFactory2.HasChanges);
			AssertEquals("master bill number has been merged", "BOL002", consolFactory2.JK_MasterBillNum);

			factory2.Save();

			AssertEquals("consol causing concurrency exception has been saved", false, consolFactory2.HasChanges);
			AssertEquals("master bill number has been merged", "BOL002", consolFactory2.JK_MasterBillNum);
		}

		public void TestJK_MasterBillNumInfo_ConcurrencyPolicy()
		{
			var mawb1 = CreateMAWB(Factory, "001", "00000011");

			Factory.Save();

			int assertedScenarios = 0;

			var consol = Factory.New<ForwardingConsolForTest>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.MasterBillAirlinePrefix = "001";

			consol.OnSavingAction = () =>
			{
				assertedScenarios++;
				AssertEquals("default policy for new consols", true, consol.JK_MasterBillNumInfo.ConcurrencyPolicy.AllowMerge);
				AssertEquals("default policy for new consols", CollisionCheck.Always, consol.JK_MasterBillNumInfo.ConcurrencyPolicy.CollisionCheck);
			};

			Factory.Save();

			consol.JK_BookingReference = "XXX";

			consol.OnSavingAction = () =>
			{
				assertedScenarios++;
				AssertEquals("disallow merge for air consols", false, consol.JK_MasterBillNumInfo.ConcurrencyPolicy.AllowMerge);
				AssertEquals("default collision check for air consols", CollisionCheck.Always, consol.JK_MasterBillNumInfo.ConcurrencyPolicy.CollisionCheck);
			};

			Factory.Save();

			consol.JK_TransportMode = Constants.TransportModes.Sea;

			consol.OnSavingAction = () =>
			{
				assertedScenarios++;
				AssertEquals("allow merge for sea consols", true, consol.JK_MasterBillNumInfo.ConcurrencyPolicy.AllowMerge);
				AssertEquals("default collision check", CollisionCheck.Always, consol.JK_MasterBillNumInfo.ConcurrencyPolicy.CollisionCheck);
			};

			Factory.Save();

			AssertEquals("all 3 scenarios have been asserted", 3, assertedScenarios);
		}

		public void TestIGateway()
		{
			var gateway = Consol as IGateway;

			Assert("should be true for consol", gateway.IsContainerNegotiatedCostApplicable(CostSell.Cost));
			Assert("should be true for consol", gateway.IsContainerNegotiatedCostApplicable(CostSell.Revenue));
			Assert("should be true for consol", gateway.IsGatewaySellApplicableToGatewayConsol(CostSell.Cost));
			Assert("should be true for consol", gateway.IsGatewaySellApplicableToGatewayConsol(CostSell.Revenue));
		}

		#region Implementation

		ForwardingConsolForTest Consol;

		protected override void SetUp()
		{
			base.SetUp();

			Consol = Factory.New<ForwardingConsolForTest>();
			Consol.JK_IsNeutralMaster = true;
			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "USLAX";
		}

		JobMawb CreateMAWB(BusinessObjectFactory factory, string prefix, string mawbNumber)
		{
			var mawb = factory.New<JobMawb>();
			mawb.JM_IsPaper = false;
			mawb.JM_Airline3DigitPrefix = prefix;
			mawb.JM_IsPrinted = false;
			mawb.JM_MAWB = mawbNumber;
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;

			return mawb;
		}

		#endregion

		#region TestRetainLoadPortWhenSettingConsignor

		public void TestRetainLoadPortWhenSettingConsignorOnSavedShipment()
		{
			RefUNLOCO loadPort = Factory.New<RefUNLOCO>();
			loadPort.RL_Code = "AUFAK";

			RefUNLOCO consignorPort = Factory.New<RefUNLOCO>();
			consignorPort.RL_Code = "SGFAK";

			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "Test Consignor";
			consignor.OH_RL_NKClosestPort = consignorPort.RL_Code;
			consignor.MainAddress.OA_Address1 = "Test Consignor Address 1";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = loadPort.RL_Code;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = loadPort.RL_Code;
			Factory.Save();
			shipment.ConsignorPK = consignor.PK;

			AssertEquals("A Shipment that exists in the database should not updated the Origin on setting the consignee", loadPort.RL_Code, shipment.JS_RL_NKOrigin);
		}

		#endregion

		#region TestForwardingConsolAWBDocumentEventsHandlers

		public void TestForwardingConsolAWBDocumentEventsHandlers_CanHandleMenuItem()
		{
			var (_, documentSupporter, awbHandler, _) = SetUpForTestForwardingConsolAWBDocumentEventsHandlers();

			void AssertMenuItem(IStmMenuItem menuItem, bool expectedResult)
			{
				var result = awbHandler.CanHandleMenuItem(menuItem);

				if (expectedResult)
				{
					Assert(string.Format("[{0}] can not handle MenuItem; Name: [{1}] Path: [{2}].",
						documentSupporter.GetType().ToString(), menuItem?.SU_MenuName, menuItem?.SU_MenuPath), result == expectedResult);
				}
				else
				{
					Assert(string.Format("[{0}] can handle MenuItem; Name: [{1}] Path: [{2}].",
							documentSupporter.GetType().ToString(), menuItem?.SU_MenuName, menuItem?.SU_MenuPath), result == expectedResult);
				}
			}

			void AssertMenuItems(IEnumerable<(string menuPath, string menuName)> menuItems, bool expectedResult)
			{
				foreach (var (menuPath, menuName) in menuItems)
				{
					var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
					menuItem.SU_MenuName = menuName;
					menuItem.SU_MenuPath = menuPath;

					AssertMenuItem(menuItem, expectedResult);
				}
			}

			AssertMenuItems(new List<(string menuPath, string menuName)>
			{
				("AWB", "Print Final Master"),
				("AWB", "Laser MAWB"),
				("AWB", "Neutral MAWB"),
				("AWB", "Neutral HAWB"),
				("AWB", "Carrier MAWB"),
				("AWB", "AWB Barcode Label"),
				("AWB", "HAWB Barcode Label 5 Inch"),
				("AWB", "AWB Security Declaration"),
				("Departure/Document Pack", "Consol Agent Pack (Air)")
			}, expectedResult: true);

			AssertMenuItems(new List<(string menuPath, string menuName)>
			{
				("Other", "Dummy AWB"),
				("Other", "AWBRACHEN"),
				("Other", "awbrachen"),
				("Other", "Dummy"),
				("Departure/Document Pack", "Consol Agent Pack (Sea)")
			}, expectedResult: false);

			AssertMenuItem(null, expectedResult: false);
		}

		public void TestForwardingConsolAWBDocumentEventsHandlers_HandleDocumentPrintRequested()
		{
			var (consol, _, awbHandler, _) = SetUpForTestForwardingConsolAWBDocumentEventsHandlers();

			var shipments = consol.Shipments.OfType<ForwardingShipment>().ToList();
			foreach (var shipment in shipments)
			{
				shipment.AWBForDocumentsPopulated = true;
			}

			Env.Registry.Freight.AirWaybill.MAWBDefaultCarrierText = "MAWB Dummy Carrier";
			var dummyMenuItem = Factory.NewWithValidTestData<StmMenuItem>();
			awbHandler.HandleDocumentPrintRequested(null, new DocumentCancelEventArgs(dummyMenuItem));
			Assert(string.Format("Consol AWB - Invalid EH_ExtraCarrierInfoLine2 value: [{0}], should start with: [{1}]", consol.AWBHeader.EH_ExtraCarrierInfoLine2, Env.Registry.Freight.AirWaybill.MAWBDefaultCarrierText),
				consol.AWBHeader.EH_ExtraCarrierInfoLine2.Contains(Env.Registry.Freight.AirWaybill.MAWBDefaultCarrierText));

			foreach (var shipment in shipments)
			{
				Assert(!shipment.AWBForDocumentsPopulated);
			}
		}

		public void TestForwardingConsolAWBDocumentEventsHandlers_HandleDocumentPrePreviewed()
		{
			var (consol, _, awbHandler, printSet) = SetUpForTestForwardingConsolAWBDocumentEventsHandlers(true);
			using (printSet)
			{
				Env.Registry.Freight.AirWaybill.HAWBDefaultCarrierText = "HAWB Dummy Carrier";
				var dummyMenuItem = Factory.NewWithValidTestData<StmMenuItem>();
				awbHandler.HandleDocumentPrePreviewed(null, new DocumentPrintedEventArgs(DeliveryInstructionDestination.Preview, dummyMenuItem, false, printSet[0]));
				AssertAWBForDocumentsPopulated(consol.Shipments[0], expected: true);
				AssertAWBForDocumentsPopulated(consol.Shipments[1], expected: false);
				AssertAWBForDocumentsPopulated(consol.Shipments[2], expected: false);
				AssertAWBForDocumentsPopulated(consol.Shipments[3], expected: false);

				awbHandler.HandleDocumentPrePreviewed(null, new DocumentPrintedEventArgs(DeliveryInstructionDestination.Preview, dummyMenuItem, false, printSet));
				AssertAWBForDocumentsPopulated(consol.Shipments[0], expected: true);
				AssertAWBForDocumentsPopulated(consol.Shipments[1], expected: false);
				AssertAWBForDocumentsPopulated(consol.Shipments[2], expected: true);
				AssertAWBForDocumentsPopulated(consol.Shipments[3], expected: false);
			}
		}

		public void TestForwardingConsolAWBDocumentEventsHandlers_HandleDocumentPrePrinted()
		{
			var (consol, _, awbHandler, printSet) = SetUpForTestForwardingConsolAWBDocumentEventsHandlers(true);
			using (printSet)
			{
				Env.Registry.Freight.AirWaybill.HAWBDefaultCarrierText = "HAWB Dummy Carrier";
				var dummyMenuItem = Factory.NewWithValidTestData<StmMenuItem>();
				awbHandler.HandleDocumentPrePrinted(null, new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, dummyMenuItem, false, printSet[1]));
				AssertAWBForDocumentsPopulated(consol.Shipments[0], expected: false);
				AssertAWBForDocumentsPopulated(consol.Shipments[1], expected: false);
				AssertAWBForDocumentsPopulated(consol.Shipments[2], expected: true);
				AssertAWBForDocumentsPopulated(consol.Shipments[3], expected: false);

				awbHandler.HandleDocumentPrePrinted(null, new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, dummyMenuItem, false, printSet));
				AssertAWBForDocumentsPopulated(consol.Shipments[0], expected: true);
				AssertAWBForDocumentsPopulated(consol.Shipments[1], expected: false);
				AssertAWBForDocumentsPopulated(consol.Shipments[2], expected: true);
				AssertAWBForDocumentsPopulated(consol.Shipments[3], expected: false);
			}
		}

		public void TestForwardingConsolAWBDocumentEventsHandlers_HandleDocumentPrinted()
		{
			var (consol, _, awbHandler, printSet) = SetUpForTestForwardingConsolAWBDocumentEventsHandlers(true);
			using (printSet)
			{
				var shipments = consol.Shipments.OfType<ForwardingShipment>().ToList();
				foreach (var shipment in shipments)
				{
					shipment.AWBForDocumentsPopulated = true;
				}

				var dummyMenuItem = Factory.NewWithValidTestData<StmMenuItem>();
				awbHandler.HandleDocumentPrinted(null, new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, dummyMenuItem, false, printSet[0]));
				Assert(!consol.Shipments[0].AWBForDocumentsPopulated);
				Assert(!consol.Shipments[1].AWBForDocumentsPopulated);
				Assert(consol.Shipments[2].AWBForDocumentsPopulated);
				Assert(consol.Shipments[3].AWBForDocumentsPopulated);

				awbHandler.HandleDocumentPrinted(null, new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, dummyMenuItem, false, printSet));
				Assert(!consol.Shipments[0].AWBForDocumentsPopulated);
				Assert(!consol.Shipments[1].AWBForDocumentsPopulated);
				Assert(!consol.Shipments[2].AWBForDocumentsPopulated);
				Assert(!consol.Shipments[3].AWBForDocumentsPopulated);
			}
		}

		sealed class DummyBODocDataProviderWithBOForPrintJob : IBODocDataProviderWithBOForPrintJob
		{
			public DocWrapperCopyInfo AdditionalCopyInfo => null;
			public string[] ImageNamesToRemove => Array.Empty<string>();
			public BusinessObject BusinessObjectToLogAgainst { get; set; }
			public BusinessObject ParentBusinessObject { get; set; }
			public void SetDocWrapperContext(Dictionary<string, object> constants) { }
			public ZString GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue) => ZString.Empty;
			public IZType GetCustomField(string fieldName, string typeName) => ZString.Empty;
			public string GetCustomFieldCodeDescription(string fieldName, string typeName) => string.Empty;
			public ZDateTime GetEventLastDateTime(string eventCode) => ZDateTime.UtcNow;
			public BusinessObject BusinessObjectForPrintJob { get; set; }
		}

		(ForwardingConsol consol, ForwardingConsolDocumentSupporter documentSupporter, IDocumentEventsHandler awbHandler, DocumentPrintSet printSet) SetUpForTestForwardingConsolAWBDocumentEventsHandlers(bool createPrintSet = false)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_OverrideWaybillDefaults = false;
			AssertNotNull("ForwardingConsol must have one and one only AWB Header", consol.AWBHeader);

			for (var index = 0; index < 4; index++)
			{
				var shipment = consol.Shipments.AddNew();
				shipment.AWBForDocumentsPopulated = false;
				AssertNotNull("ForwardingShipment must have one and one only AWB Header", shipment.AWBHeader);
			}

			DocumentPrintSet printSet = null;
			if (createPrintSet)
			{
				var biz1 = new DummyBODocDataProviderWithBOForPrintJob();
				biz1.BusinessObjectForPrintJob = consol.Shipments[0];
				var biz2 = new DummyBODocDataProviderWithBOForPrintJob();
				biz2.BusinessObjectForPrintJob = consol.Shipments[1];
				var biz3 = new DummyBODocDataProviderWithBOForPrintJob();
				biz3.BusinessObjectForPrintJob = consol.Shipments[2];
				var biz4 = new DummyBODocDataProviderWithBOForPrintJob();
				biz4.BusinessObjectForPrintJob = consol.Shipments[3];

				var pack1 = new DocumentPack();
				pack1.Add(new Report(pack1, null, null, "a", null, DocumentDirection.ANY, false));
				pack1.Add(new Report(pack1, null, new DataProviderList(biz1), "b", null, DocumentDirection.ANY, false) { IncludedInPrint = true });
				pack1.Add(new Report(pack1, null, new DataProviderList(biz2), "c", null, DocumentDirection.ANY, false) { IncludedInPrint = false });

				var pack2 = new DocumentPack();
				pack2.Add(new Report(pack2, null, null, "d", null, DocumentDirection.ANY, false));
				pack2.Add(new Report(pack2, null, new DataProviderList(biz3), "e", null, DocumentDirection.ANY, false) { IncludedInPrint = true });
				pack2.Add(new Report(pack2, null, new DataProviderList(biz4), "f", null, DocumentDirection.ANY, false) { IncludedInPrint = false });

				var documentCommand = DocumentCommand.New(Factory);
				printSet = new DocumentPrintSet(documentCommand, null);
				printSet.Add(pack1);
				printSet.Add(pack2);
			}

			var documentSupporter = new ForwardingConsolDocumentSupporter(consol);
			var awbHandler = (from documentHandler in documentSupporter.DocumentEventsHandlers
							  where documentHandler.GetType().ToString().EndsWith("AWBDocumentEventsHandler")
							  select documentHandler).FirstOrDefault();
			AssertNotNull("ForwardingConsol -> DocumentEventsHandlers collection; Could not find AWBDocumentEventsHandler", awbHandler);

			return (consol, documentSupporter, awbHandler, printSet);
		}

		void AssertAWBForDocumentsPopulated(ForwardingShipment shipment, bool expected)
		{
			AssertEquals(expected, shipment.AWBHeader.EH_ExtraCarrierInfoLine2.Contains(Env.Registry.Freight.AirWaybill.HAWBDefaultCarrierText));
			AssertEquals(expected, shipment.AWBForDocumentsPopulated);
		}

		#endregion

		#region TestNoDuplicateSpecialHandlingItems

		public void TestNoDuplicateSpecialHandlingItems()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "1234";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_SpecialHandlingCodes = "ROX";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var shipment = factory2.New<ForwardingShipment>();

			var shipmentCollection = new ModuleShipmentCollection(factory2);
			shipmentCollection.ParentConsol = consol;
			shipmentCollection.AddRelationshipToNewObject(shipment);

			var packline = shipment.OuterPackLines.AddNew();
			var dg = packline.UNDGs.AddNew();
			dg.LinkDefault(subs);

			factory2.Save();

			AssertEquals("Only 1 special handling code was added", 1, consol.AWBSpecialHandlingItems.Count);
		}

		#endregion

		public void TestRoutingReadOnlyDueToPhase()
		{
			PhaseSecurity security = new PhaseSecurity(PhaseConstants.GetShipmentLocationsList());
			security.IsEnabled = true;

			Phase phase = security.Phases.AddNew();
			phase.Code = "AAA";
			phase.Description = (NoResString)"Phase AAA";

			PhaseRule rule = phase.Rules.AddNew();
			rule.Location = PhaseConstants.Locations.Consol.LoadCountry;
			rule.DepartmentPK = GlbDepartment.CurrentDepartment.PK;
			ForwardingConfigurationRegistry.Instance.ConsolPhaseSecurity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, security);

			ForwardingConsolWithoutSetDefaultForTest consol = Factory.New<ForwardingConsolWithoutSetDefaultForTest>();
			consol.BeforeTransportsLoad = (consol) =>
			{
				((IRoutingSupport)consol).TransportsIncludingRelated.Any();
			};
			consol.JK_Phase = "AAA";

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			consol.JK_Phase = PhaseConstants.Phase.ALL;
			consol.JK_Phase = "AAA";
			AssertEquals("Editable due to phase", false, ((IRoutingSupport)consol).TransportsIncludingRelated.ReadOnly);
		}

		internal class ForwardingConsolWithoutSetDefaultForTest : ForwardingConsol
		{
			public ForwardingConsolWithoutSetDefaultForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void SetDefaultValues()
			{
			}
		}
	}
}
