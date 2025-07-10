using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ChargeCodeSubGroupListTest : TestCaseWithFactory
	{
		#region TestWarehouseChargeCodeSubGroups

		public void TestWarehouseChargeCodeSubGroups()
		{
			SystemDefinableCodeDescriptionBoolCollection warehouseJobServices = new SystemDefinableCodeDescriptionBoolCollection();
			warehouseJobServices.Add("WHS", (NoResString)"Random Warehouse Service", false);
			warehouseJobServices.SetDefaultCode("WHS", true);
			WarehouseDataRegistry.Instance.JobServices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, warehouseJobServices);

			ChargeCodeSubGroupList whsInwardsList = ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.WHSInwards, Env.CurrentCompany.PK);
			ChargeCodeSubGroupList whsOutwardsList = ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.WHSOutwards, Env.CurrentCompany.PK);
			ChargeCodeSubGroupList whsAdHocServicesList = ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.WHSAdHocServiceJob, Env.CurrentCompany.PK);
			ChargeCodeSubGroupList whsStorageList = ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.WHSStorage, Env.CurrentCompany.PK);

			// inwards
			AssertEquals("Should contain items from Freight JobServices.", true, whsInwardsList.ContainsCode(Core.Constants.FreightServiceType.Codes.Cleaning));
			AssertEquals("Should contain items from Warehouse JobServices.", true, whsInwardsList.ContainsCode("WHS"));

			// outwards
			AssertEquals(whsInwardsList.CodesAsString, whsOutwardsList.CodesAsString);
			AssertEquals(whsInwardsList.CodesAsString, whsAdHocServicesList.CodesAsString);

			// storage
			AssertEquals(0, whsStorageList.Count);
		}

		#endregion

		#region TestTransitWarehouseChargeCodeSubGroups

		public void TestTransitWarehouseChargeCodeSubGroups()
		{
			const string trw = "TRW";
			var warehouseJobServices = new SystemDefinableCodeDescriptionBoolCollection();
			warehouseJobServices.Add(trw, (NoResString)"Random Transit Warehouse Service", false);
			warehouseJobServices.SetDefaultCode(trw, true);
			WarehouseDataRegistry.Instance.JobServices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, warehouseJobServices);

			var receive = ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.TRWReceive, Env.CurrentCompany.PK);
			var dispatch = ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.TRWDispatch, Env.CurrentCompany.PK);

			var expectedFreightServiceTypes = new FreightServiceTypes().ToList<CodeDescriptionPair>().Select(f => f.Code).ToList();
			expectedFreightServiceTypes.Add(trw);
			// receive
			AssertContainsExactElementsInAnyOrder("Should contain items from Freight JobServices for transit receive consignment.",
				expectedFreightServiceTypes, receive.ToList<CodeDescriptionPair>().Select(f => f.Code));

			// dispatch
			AssertContainsExactElementsInAnyOrder("Should contain items from Freight JobServices for transit dispatch consignment.",
				expectedFreightServiceTypes, dispatch.ToList<CodeDescriptionPair>().Select(f => f.Code));
		}

		#endregion

		#region TestTransitWarehouseTransportationUnitChargeCodeSubGroups

		public void TestTransitWarehouseTransportationUnitChargeCodeSubGroups()
		{
			const string twu = "TWU";
			var warehouseJobServices = new SystemDefinableCodeDescriptionBoolCollection();
			warehouseJobServices.Add(twu, (NoResString)"Random Transit Warehouse Transportation Unit Service", false);
			warehouseJobServices.SetDefaultCode(twu, true);
			WarehouseDataRegistry.Instance.JobServices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, warehouseJobServices);

			var rtu = ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit, Env.CurrentCompany.PK);
			var dtu = ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit, Env.CurrentCompany.PK);

			var expectedFreightServiceTypes = new FreightServiceTypes().ToList<CodeDescriptionPair>().Select(f => f.Code).ToList();
			expectedFreightServiceTypes.Add(twu);
			// receive transportation unit
			AssertContainsExactElementsInAnyOrder("Should contain items from Freight JobServices for transit receive transportation unit.",
				expectedFreightServiceTypes, rtu.ToList<CodeDescriptionPair>().Select(f => f.Code));

			// dispatch transportation unit
			AssertContainsExactElementsInAnyOrder("Should contain items from Freight JobServices for transit dispatch transportation unit.",
				expectedFreightServiceTypes, dtu.ToList<CodeDescriptionPair>().Select(f => f.Code));
		}

		#endregion

		#region TestTransportConsignmentChargeCodeSubGroups

		public void TestTransportConsignmentChargeCodeSubGroups()
		{
			var transortConsignmentList = ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.TransportBooking, Env.CurrentCompany.PK);
			AssertEquals(15, transortConsignmentList.Count);
		}

		#endregion

		#region TestLocalTransportChargeCodeSubGroups

		public void TestLocalTransportChargeCodeSubGroups()
		{
			var transportServices = FreightDataRegistry.Instance.JobServices.Value;
			transportServices.Add("ASE", (NoResString)"Additional Service", false);
			using (FreightDataRegistry.Instance.JobServices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, transportServices))
			{
				ChargeCodeSubGroupList localTransportSubGroupList = ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.Transport, Env.CurrentCompany.PK);

				AssertEquals(17, localTransportSubGroupList.Count);
				Assert(localTransportSubGroupList.ContainsCode(ChargeCodeSubGroupList.CartageDemurrageTotal));
				//Services
				Assert(localTransportSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.Cleaning));
				Assert(localTransportSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.CustomsHold));
				Assert(localTransportSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.ExtraInspection));
				Assert(localTransportSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.FCLContainerStorage));
				Assert(localTransportSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.FCLUnderbondStorage));
				Assert(localTransportSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.Fumigation));
				Assert(localTransportSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.QuarantineInspection));
				Assert(localTransportSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.QuarantineUnpack));
				Assert(localTransportSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.SteamCleaning));
				Assert(localTransportSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.Survey));
				Assert(localTransportSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.Tailgate));
				Assert(localTransportSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.Washing));
				Assert(localTransportSubGroupList.ContainsCode("ASE"));
			}
		}

		#endregion

		#region TestDestinationAndOriginChargeCodeSubGroups

		public void TestDestinationAndOriginChargeCodeSubGroups()
		{
			var destinationChargeCodeSubGroups = ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.Destination, Env.CurrentCompany.PK);
			AssertDestinationAndOriginChargeCodeSubGroups("Destination", 26, destinationChargeCodeSubGroups);
			var originChargeCodeSubGroups = ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.Origin, Env.CurrentCompany.PK);
			AssertDestinationAndOriginChargeCodeSubGroups("Origin", 25, originChargeCodeSubGroups);

			var originBrokerageChargeCodeSubGroups = ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.OriginBrokerage, Env.CurrentCompany.PK);
			Assert("OriginBrokerage should not have PRC as a sub group", !originBrokerageChargeCodeSubGroups.ContainsCode(ChargeCodeSubGroupList.PrincipalDetention));
			var originBrokerageOnlyChargeCodeSubGroups = ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.OriginBrokerageOnly, Env.CurrentCompany.PK);
			Assert("OriginBrokerageOnly should not have PRC as a sub group", !originBrokerageOnlyChargeCodeSubGroups.ContainsCode(ChargeCodeSubGroupList.PrincipalDetention));
			var brokerageChargeCodeSubGroups = ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.Brokerage, Env.CurrentCompany.PK);
			Assert("Brokerage should not have PRC as a sub group", !brokerageChargeCodeSubGroups.ContainsCode(ChargeCodeSubGroupList.PrincipalDetention));
			var brokerageChargeOnlyCodeSubGroups = ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.BrokerageOnly, Env.CurrentCompany.PK);
			Assert("BrokerageOnly should not have PRC as a sub group", !brokerageChargeOnlyCodeSubGroups.ContainsCode(ChargeCodeSubGroupList.PrincipalDetention));
		}

		void AssertDestinationAndOriginChargeCodeSubGroups(string nameOfGroup, int expectedNumberOfSubGroups, ChargeCodeSubGroupList chargeCodeSubGroupList)
		{
			AssertEquals(nameOfGroup + " 's Charge Code Sub Groups", expectedNumberOfSubGroups, chargeCodeSubGroupList.Count);

			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(ChargeCodeSubGroupList.Labor));
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(ChargeCodeSubGroupList.CartageDemurrageTotal));
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(ChargeCodeSubGroupList.CartageBeyondPostcode));

			if (nameOfGroup == "Destination")
			{
				Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(ChargeCodeSubGroupList.Cod));
			}

			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(ChargeCodeSubGroupList.Storage));
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(ChargeCodeSubGroupList.CarrierStorage));
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(ChargeCodeSubGroupList.ContainerDetention));
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(ChargeCodeSubGroupList.MergedDemurrageDetention));
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(ChargeCodeSubGroupList.PrincipalDetention));
			//Services
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.Cleaning));
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.CustomsHold));
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.ExtraInspection));
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.FCLContainerStorage));
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.FCLUnderbondStorage));
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.Fumigation));
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.QuarantineInspection));
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.QuarantineUnpack));
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.SteamCleaning));
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.Survey));
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.Tailgate));
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(Core.Constants.FreightServiceType.Codes.Washing));
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(ChargeCodeSubGroupList.OverweightPenalty));
			Assert(nameOfGroup, chargeCodeSubGroupList.ContainsCode(ChargeCodeSubGroupList.OverweightSurcharge));
		}

		#endregion

		#region TestCompanyLevelAndGlobalLevelRegistry

		public void TestCompanyLevelAndGlobalLevelRegistry()
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			var warehouseJobServicesGlobal = new SystemDefinableCodeDescriptionBoolCollection();
			warehouseJobServicesGlobal.Add("WHG", (NoResString)"Random Global Warehouse Service", false);
			warehouseJobServicesGlobal.SetDefaultCode("WHG", true);
			WarehouseDataRegistry.Instance.JobServices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, warehouseJobServicesGlobal);

			var warehouseJobServicesCompany = new SystemDefinableCodeDescriptionBoolCollection();
			warehouseJobServicesCompany.Add("WHC", (NoResString)"Random Company Warehouse Service", false);
			warehouseJobServicesCompany.SetDefaultCode("WHC", true);
			WarehouseDataRegistry.Instance.JobServices.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, warehouseJobServicesCompany);

			var warehouseJobServicesOtherCompany = new SystemDefinableCodeDescriptionBoolCollection();
			warehouseJobServicesOtherCompany.Add("WHO", (NoResString)"Random Other Company Warehouse Service", false);
			warehouseJobServicesOtherCompany.SetDefaultCode("WHO", true);
			WarehouseDataRegistry.Instance.JobServices.SetValue(otherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, warehouseJobServicesOtherCompany);

			var listFromGlobal = ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.WHSInwards, Guid.Empty);
			var listFromCompany = ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.WHSInwards, Env.CurrentCompany.PK);
			var listFromOtherCompany = ChargeCodeSubGroupList.GetList(ChargeCodeGroupList.Codes.WHSInwards, otherCompany.PK.ToGuid());

			AssertEquals("Retreive registry items only at global level", true, listFromGlobal.ContainsCode("WHG"));
			AssertEquals("Retreive registry items only at global level", false, listFromGlobal.ContainsCode("WHC"));
			AssertEquals("Retreive registry items only at global level", false, listFromGlobal.ContainsCode("WHO"));
			AssertEquals("Retreive registry items only at this company level", false, listFromCompany.ContainsCode("WHG"));
			AssertEquals("Retreive registry items only at this company level", true, listFromCompany.ContainsCode("WHC"));
			AssertEquals("Retreive registry items only at this company level", false, listFromCompany.ContainsCode("WHO"));
			AssertEquals("Retreive registry items only at this company level", false, listFromOtherCompany.ContainsCode("WHG"));
			AssertEquals("Retreive registry items only at this company level", false, listFromOtherCompany.ContainsCode("WHC"));
			AssertEquals("Retreive registry items only at this company level", true, listFromOtherCompany.ContainsCode("WHO"));
		}

		#endregion

		public void TestServiceTypesFromRegistryUseMultilingualDescription()
		{
			using (var grmMock = Res.GetLanguageInstance(Core.SharedConstants.Languages.German).UseMockData())
			{
				var customServices = new CodeDescriptionBoolCollection();
				customServices.Add("ZLS", (NoResString)"Localization Service", false);
				FreightDataRegistry.Instance.JobServices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customServices);
				var serviceTypes = new FreightServiceTypes();
				AssertEquals("Localization Service", serviceTypes["ZLS"].Description);
				string key = ((ResourceString)((IMultilingualDescription)serviceTypes["ZLS"]).MultilingualDescription).ResourceKey;
				grmMock.Put(key, new ResourceStringData(key, "Ecivres Noitazilacol"));
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
				{
					AssertEquals("Ecivres Noitazilacol", serviceTypes["ZLS"].Description);
				}
				AssertType(typeof(CodeDescriptionPair), serviceTypes["ZLS"]);
			}
		}
	}
}
