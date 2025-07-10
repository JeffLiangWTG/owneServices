using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonContainerTest : BaseFreightTest
	{
		public void TestShouldNotContainerPenaltyWhenSetEmptyValue()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_EmptyReturnedBy = ZDateTime.Empty;
			AssertEquals(0, container.ImportPenalties.Count);

			container.JC_FCLWharfGateOut = ZDateTime.Empty;
			AssertEquals(0, container.ImportPenalties.Count);

			container.JC_FCLWharfGateIn = ZDateTime.Empty;
			AssertEquals(0, container.ExportPenalties.Count);

			container.JC_ContainerYardEmptyReturnGateIn = ZDateTime.Empty;
			AssertEquals(0, container.ImportPenalties.Count);

			container.JC_FCLOnBoardVessel = ZDateTime.Empty;
			AssertEquals(0, container.ExportPenalties.Count);

			container.JC_FCLAvailable = ZDateTime.Empty;
			AssertEquals(0, container.ImportPenalties.Count);

			container.JC_ArrivalCTOStorageStartDate = ZDateTime.Empty;
			AssertEquals(0, container.ImportPenalties.Count);
		}

		public void TestJC_IsNonOperativeReefer()
		{
			var container = Factory.New<CommonContainer>();
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-7");
			container.JC_IsNonOperativeReefer = false;
			container.JC_RC = refContainer.PK;

			AssertEquals(false, container.JC_IsNonOperativeReefer);
			AssertEquals(true, container.JC_IsNonOperativeReefer_ReadOnly);
			AssertEquals(false, container.JC_TempRecorderSerialNo_ReadOnly);
			AssertEquals(false, container.JC_HumidityPercent_ReadOnly);
			AssertEquals(false, container.JC_RefrigGeneratorID_ReadOnly);
			AssertEquals(false, container.JC_SetPointTempUnit_ReadOnly);
			AssertEquals(false, container.JC_IsControlledAtmosphere_ReadOnly);
			AssertEquals(false, container.JC_AirVentFlow_ReadOnly);
			AssertEquals(false, container.JC_AirVentFlowRateUnit_ReadOnly);
			AssertEquals(false, container.IsFreezer_ReadOnly);
			AssertEquals(false, container.IsChiller_ReadOnly);

			container.JC_IsNonOperativeReefer = false;
			refContainer.RC_ContainerType = Constants.ContainerTypes.Refrigerated;
			refContainer.RC_ISOType = "22R1";
			container.JC_RC = refContainer.PK;

			AssertEquals(false, container.JC_IsNonOperativeReefer);
			AssertEquals(false, container.JC_IsNonOperativeReefer_ReadOnly);
			AssertEquals(false, container.JC_TempRecorderSerialNo_ReadOnly);
			AssertEquals(false, container.JC_HumidityPercent_ReadOnly);
			AssertEquals(false, container.JC_RefrigGeneratorID_ReadOnly);
			AssertEquals(false, container.JC_SetPointTempUnit_ReadOnly);
			AssertEquals(false, container.JC_IsControlledAtmosphere_ReadOnly);
			AssertEquals(false, container.JC_AirVentFlow_ReadOnly);
			AssertEquals(false, container.JC_AirVentFlowRateUnit_ReadOnly);
			AssertEquals(false, container.IsFreezer_ReadOnly);
			AssertEquals(false, container.IsChiller_ReadOnly);

			container.JC_IsNonOperativeReefer = false;
			container.JC_SetPointTemp = 1.0m;
			container.JC_SetPointTempUnit = container.JC_SetPointTempUnit = Constants.Temperature.Fahrenheit;
			refContainer.RC_ISOType = "22R1";
			refContainer.RC_ContainerType = Constants.ContainerTypes.DryStorage;
			container.JC_RC = refContainer.PK;

			AssertEquals(true, container.JC_IsNonOperativeReefer_ReadOnly);
			AssertEquals(true, container.JC_IsNonOperativeReefer);
			AssertEquals(ZString.Empty, container.JC_SetPointTempUnit);
			AssertEquals(0m, container.JC_SetPointTemp);
			AssertEquals(true, container.JC_TempRecorderSerialNo_ReadOnly);
			AssertEquals(true, container.JC_HumidityPercent_ReadOnly);
			AssertEquals(true, container.JC_RefrigGeneratorID_ReadOnly);
			AssertEquals(true, container.JC_SetPointTempUnit_ReadOnly);
			AssertEquals(true, container.JC_IsControlledAtmosphere_ReadOnly);
			AssertEquals(true, container.JC_AirVentFlow_ReadOnly);
			AssertEquals(true, container.JC_AirVentFlowRateUnit_ReadOnly);
			AssertEquals(true, container.IsFreezer_ReadOnly);
			AssertEquals(true, container.IsChiller_ReadOnly);

			container.JC_IsNonOperativeReefer = false;
			refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE");
			refContainer.RC_ISOType = "22R1";
			refContainer.RC_ContainerType = Constants.ContainerTypes.DryStorage;
			container.JC_RC = refContainer.PK;

			AssertEquals(true, container.JC_IsNonOperativeReefer_ReadOnly);
			AssertEquals(true, container.JC_IsNonOperativeReefer);
			AssertEquals(true, container.JC_TempRecorderSerialNo_ReadOnly);
			AssertEquals(true, container.JC_HumidityPercent_ReadOnly);
			AssertEquals(true, container.JC_RefrigGeneratorID_ReadOnly);
			AssertEquals(true, container.JC_SetPointTempUnit_ReadOnly);
			AssertEquals(true, container.JC_IsControlledAtmosphere_ReadOnly);
			AssertEquals(true, container.JC_AirVentFlow_ReadOnly);
			AssertEquals(true, container.JC_AirVentFlowRateUnit_ReadOnly);
			AssertEquals(true, container.IsFreezer_ReadOnly);
			AssertEquals(true, container.IsChiller_ReadOnly);

			refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE");
			refContainer.RC_ContainerType = Constants.ContainerTypes.Refrigerated;
			container.JC_IsNonOperativeReefer = true;
			container.JC_IsControlledAtmosphere = false;
			container.IsFreezer = false;
			container.JC_RC = refContainer.PK;
			AssertEquals(false, container.JC_IsNonOperativeReefer);
			AssertEquals(true, container.JC_IsControlledAtmosphere);
			AssertEquals(true, container.IsFreezer);

			container.JC_IsNonOperativeReefer = false;
			AssertEquals(false, container.JC_IsNonOperativeReefer);
			AssertEquals(true, container.JC_IsControlledAtmosphere);
			AssertEquals(true, container.IsFreezer);
		}

		public void TestUserDefinedDeliveryCode()
		{
			var itemSet = FreightDataRegistry.Instance.ContainerDeliveryModeList.Value;
			itemSet[Constants.DeliveryModes.Codes.CFS_CFS].UserDefinedCode = "DR/DR";
			itemSet[Constants.DeliveryModes.Codes.CFS_CFS].UserDefinedDescription = (NoResString)"DOOR/DOOR";
			itemSet[Constants.DeliveryModes.Codes.CFS_CY].UserDefinedCode = "DR/CY";
			itemSet[Constants.DeliveryModes.Codes.CFS_CY].UserDefinedDescription = (NoResString)"DOOR/CY";
			itemSet[Constants.DeliveryModes.Codes.CY_CFS].UserDefinedCode = "CY/DR";
			itemSet[Constants.DeliveryModes.Codes.CY_CFS].UserDefinedDescription = (NoResString)"CY/DOOR";

			using (FreightDataRegistry.Instance.ContainerDeliveryModeList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, itemSet))
			{
				using (FreightDataRegistry.Instance.ContainerDeliveryModeOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var container = Factory.New<CommonContainer>();

					container.DeliveryModeForBinding = "CY/DR";
					AssertEquals(Constants.DeliveryModes.Codes.CY_CFS, container.JC_DeliveryMode);
					Assert(!container.DeliveryModeForBindingInfo.HasErrors());

					container.DeliveryModeForBinding = "DD/DD";
					AssertEquals("DD/DD", container.JC_DeliveryMode);
					Assert(container.DeliveryModeForBindingInfo.HasError("Enter a valid Delivery Mode."));
				}

				using (FreightDataRegistry.Instance.ContainerDeliveryModeOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var container = Factory.New<CommonContainer>();

					container.DeliveryModeForBinding = "CY/DR";
					AssertEquals("CY/DR", container.JC_DeliveryMode);
					Assert(container.DeliveryModeForBindingInfo.HasError("Enter a valid Delivery Mode."));

					container.DeliveryModeForBinding = "DD/DD";
					AssertEquals("DD/DD", container.JC_DeliveryMode);
					Assert(container.DeliveryModeForBindingInfo.HasError("Enter a valid Delivery Mode."));
				}
			}
		}

		public void TestGetEventLocationForTerminal()
		{
			var container = Factory.New<CommonContainer>();
			AssertEquals("", container.GetEventLocationForTerminal(Events.GateInCode));
			AssertEquals("", container.GetEventLocationForTerminal(Events.GateOutCode));

			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			container = consol.Containers.AddNew();
			AssertEquals("AUSYD", container.GetEventLocationForTerminal(Events.GateInCode));
			AssertEquals("NZAKL", container.GetEventLocationForTerminal(Events.GateOutCode));
		}

		public void TestGetEventLocationForContainerYard()
		{
			var container = Factory.New<CommonContainer>();
			AssertEquals("", container.GetEventLocationForContainerYard(Events.GateInCode));
			AssertEquals("", container.GetEventLocationForContainerYard(Events.GateOutCode));

			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			container = consol.Containers.AddNew();
			AssertEquals("NZAKL", container.GetEventLocationForContainerYard(Events.DehireCode));
			AssertEquals("NZAKL", container.GetEventLocationForContainerYard(Events.GateInCode));
			AssertEquals("AUSYD", container.GetEventLocationForContainerYard(Events.GateOutCode));
		}

		public void TestIsMeasurementsOutOfRange()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_GrossWeight = 0;
			container.JC_GrossVolume = 0;

			AssertEquals(false, container.IsMeasurementsOutOfRange);

			container.JC_GrossWeight = 1000000;
			AssertEquals(true, container.IsMeasurementsOutOfRange);

			container.JC_GrossWeight = 0;
			container.JC_GrossVolume = 1000000;
			AssertEquals(true, container.IsMeasurementsOutOfRange);
		}

		public void TestTypeDecider()
		{
			AssertEquals(typeof(ContainerTypeDecider), CommonContainer.TypeDecider.GetType());
		}

		public void TestJC_LastFreeDay()
		{
			var testContainer = Factory.New<CommonContainer>();
			AssertEquals(ZDateTime.Empty, testContainer.JC_LastFreeDay);

			testContainer.JC_LCLStorageCommences = new ZDateTime(2010, 3, 12, 1, 1, 1);
			AssertEquals(new ZDateTime(2010, 3, 11), testContainer.JC_LastFreeDay);

			testContainer.JobContainer.JC_ArrivalCTOStorageStartDate = new ZDateTime(2010, 3, 15, 1, 1, 1);
			AssertEquals(new ZDateTime(2010, 3, 14), testContainer.JC_LastFreeDay);
		}

		public void TestAllocationLineCollectionResolves()
		{
			var container = Factory.New<CommonContainer>();
			AssertNoExceptionThrown("ObjectFactory should resolve collection", () => _ = container.AllocationLineCollection);
		}

		#region IAdditionalReferenceNumberTypeProvider

		public void TestIAdditionalReferenceNumberTypeProvider()
		{
			var numberTypeProvider = Factory.New<CommonContainer>() as IAdditionalReferenceNumberTypeProvider;

			foreach (var countryCode in new[]
				{
					Constants.CountryCodes.Australia,
					Constants.CountryCodes.China,
					Constants.CountryCodes.HongKong,
					Constants.CountryCodes.Taiwan,
					Constants.CountryCodes.Iceland,
					Constants.CountryCodes.UnitedArabEmirates,
					Constants.CountryCodes.UnitedStates,
					Constants.CountryCodes.Germany
				})
			{
				var actualList = numberTypeProvider.GetAdditionalReferenceNumberTypeList(CusEntryNumber.Categories.AdditionalReferenceNumber, countryCode);
				foreach (ICodeDescription pair in CusEntryNumLookups.GetAdditionalReferenceNumberTypes(countryCode))
				{
					Assert($"Should contain {countryCode}/{pair.Code}", actualList.ContainsCode(pair.Code));
				}

				var nonCustomsAdditionalReferences = new ContainerNonCustomsAdditionalReferenceCodesCodeList();
				foreach (ICodeDescription pair in nonCustomsAdditionalReferences)
				{
					Assert($"Should contain {countryCode}/{pair.Code}", actualList.ContainsCode(pair.Code));
				}
			}
		}

		#endregion

		#region Confirms

		public void TestDeletingContainerDeletesConfirmDivots()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			CommonPickupDeliveryConfirm confirm = container.OriginCFSArrival;
			container.Delete();
			Assert("Container is deleted", container.IsDeleted);
			Assert("Confirm is deleted", confirm.IsDeleted);
		}

		public void TestShouldNotCreateDivotOnContainerConfirmation()
		{
			var container = Factory.New<CommonContainer>();
			var confirm = container.OriginCFSArrival;

			var divotQuery = new ZQuery(JobTransportLegPackLineDivotSchema.J8_EU_PickupDeliverConfirm, confirm.PK);
			var divot = Factory.LoadTop1<CommonConfirmDivot>(divotQuery);

			AssertNull("Should not create any divots on the confirmation of container.", divot);
		}

		public void TestDeleteForDataRefresh()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var container = consol.Containers.AddNew();
			var confirm = container.OriginCFSArrival;

			IBusiness bizoToDelete = container;
			bizoToDelete.DeleteForDataRefresh();

			CombineAssertions("When DeleteForDataRefresh is triggered, there should be no enumeration errors", () =>
			{
				Assert(ErrorReporter.LastKeyReported.IsNullOrEmpty());
				Assert(ErrorReporter.LastMessageReported.IsNullOrEmpty());
			});
		}

		public void TestFindConfirmOfType()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var container = consol.Containers.AddNew();
			var confirm = container.OriginCFSArrival;

			var foundConfirm = container.FindConfirmOfType(Constants.PickupDeliveryConfirmTypes.OriginCFSArrival);
			var notFound = container.FindConfirmOfType("FakeType");

			AssertEquals("OriginCFSArrival confirm type should be found", confirm, foundConfirm);
			AssertNull("Confirm of faketype is not found", notFound);
		}

		#endregion

		#region TestScheduleValues

		public void TestScheduleValues()
		{
			ZDateTime now = ZDateTime.Now;

			CommonContainer container = Factory.New<CommonContainer>();

			AssertEquals("", container.JC_JV_NKVessel);
			AssertEquals("", container.JC_JV_VoyageFlight);
			AssertEquals(ZDateTime.Empty, container.JC_JA_E_DEP);
			AssertEquals(ZDateTime.Empty, container.JC_JB_E_ARV);

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = "1234";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Origins[0].JA_E_DEP = now.AddDays(1);
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.Destinations[0].JB_E_ARV = now.AddDays(10);
			voyage.GenerateSailings();

			container.JC_JX = voyage.Sailings[0].PK;

			AssertEquals(TestVessel1.RV_FK, container.JC_JV_NKVessel);
			AssertEquals("1234", container.JC_JV_VoyageFlight);
			AssertEquals(now.AddDays(1), container.JC_JA_E_DEP);
			AssertEquals(now.AddDays(10), container.JC_JB_E_ARV);

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUCNS";
			consol.JK_RL_NKDischargePort = "NLAMS";
			consol.Transports[0].JW_IsLinked = false;
			consol.Transports[0].JW_Vessel = TestVessel2.RV_FK;
			consol.Transports[0].JW_VoyageFlight = "41233";
			consol.Transports[0].JW_ETD = now.AddDays(20);
			consol.Transports[0].JW_ETA = now.AddDays(25);

			container.JC_JK = consol.PK;

			AssertEquals(TestVessel2.RV_FK, container.JC_JV_NKVessel);
			AssertEquals("41233", container.JC_JV_VoyageFlight);
			AssertEquals(now.AddDays(20), container.JC_JA_E_DEP);
			AssertEquals(now.AddDays(25), container.JC_JB_E_ARV);
		}

		#endregion

		#region TestNotIncludingChildEditableObjectsIfViewingFromPortTransportLegPlanner

		public void TestNotIncludingChildEditableObjectsIfViewingFromPortTransportLegPlanner()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var container = shipment.Consols.AddNew().Containers.AddNew();

			//Pickline
			container.AddPackLine(packline);

			//service
			var service = container.Services.AddNew();
			service.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;

			//confirm
			var originConfirm = container.OriginConfirm;

			//Workflow
			var milestone = container.WorkflowItems.Milestones.AddNew();

			var address = container.DocAddresses.AddNew(DocAddressType.LocalCartageYard);

			Factory.Save();

			var isFromCartageLegPlannerControl = FactoryCacheHelper.GetIsViewingFromPortTransportLegPlanner(Factory);
			AssertEquals(isFromCartageLegPlannerControl, false);

			packline.JL_ActualWeight = 1m;
			AssertEquals(true, container.HasChanges);
			Factory.Save();

			service.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			AssertEquals(true, container.HasChanges);
			Factory.Save();

			address.Address1 = "TEST";
			AssertEquals(true, container.HasChanges);
			Factory.Save();

			originConfirm.EU_Distance = 3;
			AssertEquals(true, container.HasChanges);
			Factory.Save();

			milestone.SetMilestoneActualDateForTest(ZDateTimeOffset.Now);
			AssertEquals(true, container.HasChanges);
			Factory.Save();

			var newFactory2 = new BusinessObjectFactory();
			FactoryCacheHelper.SetIsViewingFromPortTransportLegPlanner(Factory);

			var container_NewFactory = newFactory2.Load<CommonContainer>(container.PK);

			var address_NewFactory = (JobDocAddress)container.DocAddresses.FirstOrDefault();
			var packline_NewFactory = (PackLine)container.PackLines.FirstOrDefault();
			var service_NewFactory = (JobService)container.Services.FirstOrDefault();
			var confirm_NewFactory = container.Confirms.FirstOrDefault();
			var milestone_NewFactory = (ProcessTask)container.WorkflowItems.FirstOrDefault();

			service_NewFactory.ES_ServiceCode = Constants.FreightServiceType.Codes.CustomsHold;
			packline_NewFactory.JL_ActualWeight = 2m;
			address_NewFactory.Address1 = "TEST1";
			confirm_NewFactory.EU_Distance = 1;
			milestone_NewFactory.SetMilestoneActualDateForTest(ZDateTimeOffset.Now);
			AssertEquals(false, container_NewFactory.HasChanges);
		}

		#endregion

		#region Defaults

		public void TestSetDefaultValues()
		{
			CommonContainer newContainer = Factory.New<CommonContainer>();
			AssertEquals("ContainerCount should default to 1.", (short)1, newContainer.JC_ContainerCount);
			AssertEquals(ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS, newContainer.JC_Purpose);
			AssertEquals(string.Empty, newContainer.JC_SetPointTempUnit);
		}

		#endregion

		#region Related Business Objects

		public void TestPackLines()
		{
			CommonContainer testContainer = Factory.New<CommonContainer>();
			AssertNotNull("Packlines should be overriden in child classes to implement behavior", testContainer.PackLines);
			AssertEquals("Packlines should not be read only", false, testContainer.PackLines.ReadOnly);
		}

		public void TestSailing()
		{
			SailingsForTestClasses helper = new SailingsForTestClasses(Factory);

			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_JX = helper.SydLaxSailing.PK;

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_JX = helper.MelSydFlightLeg.PK;

			AssertEquals("Expecting Sailing on Container to return sydlax sailing.", helper.SydLaxSailing.PK, container.Sailing.PK);

			consol.Containers.Add(container);

			AssertEquals("Expecting Sailing on Container to return melsyd flight", helper.MelSydFlightLeg.PK, container.Sailing.PK);
		}

		public void TestJobServices()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			AssertNotNull("Container should contain a job services collection", container.Services);

			JobService fumigation = container.Services.AddNew();
			fumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonContainer loadedContainer = newFactory.Load<CommonContainer>(container.PK);
			AssertEquals("Job services of container were not loaded correctly", 1, loadedContainer.Services.Count);

			AssertEquals("Precondition - LoadedContainer.Services should be false.", false, container.Services.ReadOnly);
			container.SetReadOnlyIncludingChildren(true);
			AssertEquals("LoadedContainer.Services should be true (JobServices is registered editable).", true, container.Services.ReadOnly);
		}

		public void TestServiceBranch()
		{
			var container = Factory.New<CommonContainer>();
			var iHaveServices = (IHaveServices)container;
			AssertEquals("Service branch", Env.CurrentBranch.PK, iHaveServices.ServiceBranch.PK);
		}

		#endregion

		#region Business Object Overrides

		public void TestHumanReadableName()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			AssertEquals("Container without number", "Container", container.HumanReadableName);
			container.JC_ContainerNum = "1234";
			AssertEquals("Container with container number", "Container '1234'", container.HumanReadableName);
		}

		#endregion

		#region Property Overrides

		public void TestJC_JK_PacklinesAreRemoved()
		{
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			CommonContainer container = consol.Containers.AddNew();

			PackLine packline1 = shipment.OuterPackLines.AddNew();
			PackLine packline2 = shipment.OuterPackLines.AddNew();

			container.AddPackLine(packline1);
			container.AddPackLine(packline2);

			AssertEquals("Container references to consol", consol.PK, container.JC_JK);
			AssertEquals("Container has 2 packlines", 2, container.PackLines.Count);

			container.JC_JK = Guid.Empty;
			AssertEquals("Container does not have packlines", 0, container.PackLines.Count);
		}

		public void TestJC_JS_FCLBookingOnlyLink_PacklinesAreRemoved()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			PackLine packline1 = shipment.OuterPackLines.AddNew();
			PackLine packline2 = shipment.OuterPackLines.AddNew();
			CommonContainer container = shipment.Consols.AddNew().Containers.AddNew();

			container.AddPackLine(packline1);
			container.AddPackLine(packline2);

			container.JC_JS_FCLBookingOnlyLink = shipment.PK;
			AssertEquals("Container references to shipment", shipment.PK, container.JC_JS_FCLBookingOnlyLink);
			AssertEquals("Container has 2 packlines", 2, container.PackLines.Count);

			container.JC_JS_FCLBookingOnlyLink = Guid.Empty;
			AssertEquals("Container does not have packlines", 0, container.PackLines.Count);
		}

		public void TestLogsSavedAgainstParentsOnContainerCountChange()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var cusDeclaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var cusContainer = ((BusinessObjectCollection)cusDeclaration["CusContainers"]).AddNew();

			var container1 = (CommonContainer)cusContainer["JobContainer"];
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_ContainerCount = 5;
			consol.Containers.Add(container1);

			var container2 = Factory.NewWithValidTestData<CommonContainer>();
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container2.JC_ContainerCount = 3;
			container2.JC_ContainerNum = "";
			container2.JC_JS_FCLBookingOnlyLink = shipment.PK;

			Factory.Save();

			AssertLogExists(shipment, "20RE)", false);
			AssertLogExists(consol, "20RE", false);
			AssertLogExists(cusDeclaration, "20RE", false);

			AssertLogExists(shipment, "20GP", false);
			AssertLogExists(consol, "20GP", false);
			AssertLogExists(cusDeclaration, "20GP", false);

			container1.JC_ContainerCount = 6;
			container2.JC_ContainerCount = 4;

			Factory.Save();

			AssertLogExists(shipment, "20RE (4) COUNT CHANGED FROM 3 TO 4", true);
			AssertLogExists(consol, "20RE (4) COUNT CHANGED FROM 3 TO 4", false);
			AssertLogExists(cusDeclaration, "20RE (4) COUNT CHANGED FROM 3 TO 4", false);

			AssertLogExists(shipment, "20GP (6) COUNT CHANGED FROM 5 TO 6", false);
			AssertLogExists(consol, "20GP (6) COUNT CHANGED FROM 5 TO 6", true);
			AssertLogExists(cusDeclaration, "20GP (6) COUNT CHANGED FROM 5 TO 6", true);
		}

		public void TestLogsSavedAgainstParentsOnDelete()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var cusDeclaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var cusContainer = ((BusinessObjectCollection)cusDeclaration["CusContainers"]).AddNew();

			var container1 = (CommonContainer)cusContainer["JobContainer"];
			container1.JC_ContainerNum = "KIKI1111111";
			consol.Containers.Add(container1);

			var container2 = Factory.NewWithValidTestData<CommonContainer>();
			container2.JC_ContainerNum = "KIKI2222222";
			container2.JC_JS_FCLBookingOnlyLink = shipment.PK;

			Factory.Save();

			AssertLogExists(shipment, "KIKI1111111 DELETED", false);
			AssertLogExists(consol, "KIKI1111111 DELETED", false);
			AssertLogExists(cusDeclaration, "KIKI1111111 DELETED", false);
			AssertLogExists(shipment, "KIKI2222222 DELETED", false);
			AssertLogExists(consol, "KIKI2222222 DELETED", false);
			AssertLogExists(cusDeclaration, "KIKI2222222 DELETED", false);

			container1.Delete();

			AssertLogExists(shipment, "KIKI1111111 DELETED", false);
			AssertLogExists(consol, "KIKI1111111 DELETED", true);
			AssertLogExists(cusDeclaration, "KIKI1111111 DELETED", true);
			AssertLogExists(shipment, "KIKI2222222 DELETED", false);
			AssertLogExists(consol, "KIKI2222222 DELETED", false);
			AssertLogExists(cusDeclaration, "KIKI2222222 DELETED", false);

			container2.Delete();

			AssertLogExists(shipment, "KIKI1111111 DELETED", false);
			AssertLogExists(consol, "KIKI1111111 DELETED", true);
			AssertLogExists(cusDeclaration, "KIKI1111111 DELETED", true);
			AssertLogExists(shipment, "KIKI2222222 DELETED", true);
			AssertLogExists(consol, "KIKI2222222 DELETED", false);
			AssertLogExists(cusDeclaration, "KIKI2222222 DELETED", false);

			var container3 = Factory.NewWithValidTestData<CommonContainer>();
			container3.JC_ContainerNum = "KIKI3333333";
			container3.JC_JS_FCLBookingOnlyLink = shipment.PK;
			container3.Delete();

			AssertLogExists(shipment, "KIKI3333333 DELETED", false);
			AssertLogExists(consol, "KIKI3333333 DELETED", false);
			AssertLogExists(cusDeclaration, "KIKI3333333 DELETED", false);
		}

		void AssertLogExists(BusinessObject bizO, string reference, bool exists)
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, bizO.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "EDT");
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, reference);

			AssertEquals(exists, Factory.LoadTop1<StmALog>(query) != null);
		}

		public void TestJC_RC()
		{
			CommonContainer container = Factory.New<CommonContainer>();

			container.JC_RC = ContainerRef.PK;
			AssertEquals("Expecting JC_TotalLength to be 6", 6m, container.JC_TotalLength);
			AssertEquals("Expecting JC_TotalHeight to be 3", 3m, container.JC_TotalHeight);
			AssertEquals("Expecting JC_TotalWidth to be 2", 2m, container.JC_TotalWidth);
			AssertEquals("Expecting JC_TareWeight to be 1000", 1000m, container.JC_TareWeight);

			container.JC_RC = ContainerRef2.PK;
			AssertEquals("Expecting JC_TotalLength to be 12", 12m, container.JC_TotalLength);
			AssertEquals("Expecting JC_TotalHeight to be 5", 5m, container.JC_TotalHeight);
			AssertEquals("Expecting JC_TotalWidth to be 2.5", 2.5m, container.JC_TotalWidth);
			AssertEquals("Expecting JC_TareWeight to be 1800", 1800m, container.JC_TareWeight);

			container.JC_RC = ZGuid.Empty;
			AssertEquals("Expecting JC_TotalLength to be 12", 12m, container.JC_TotalLength);
			AssertEquals("Expecting JC_TotalHeight to be 5", 5m, container.JC_TotalHeight);
			AssertEquals("Expecting JC_TotalWidth to be 2.5", 2.5m, container.JC_TotalWidth);
			AssertEquals("Expecting JC_TareWeight to be 1800", 1800m, container.JC_TareWeight);
		}

		public void TestRoundingRoundDownecimalToItsPrecisionAndScaleOnDataImport()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Enterprise.Registry.Business.Module.Freight);
			var registryEntryWeight = new DefaultNumberOfDecimals();
			registryEntryWeight.UnitOfMeasure = Constants.Weight.Kilograms;
			registryEntryWeight.TransportMode = Constants.TransportModes.Air;
			registryEntryWeight.NumberOfDecimals = 1;
			registryEntryWeight.RoundingMode = RoundingModes.Up;
			collection.Add(registryEntryWeight);

			var registryEntryVolume = new DefaultNumberOfDecimals();
			registryEntryVolume.UnitOfMeasure = Constants.Volume.CubicMetres;
			registryEntryVolume.TransportMode = Constants.TransportModes.Air;
			registryEntryVolume.NumberOfDecimals = 1;
			registryEntryVolume.RoundingMode = RoundingModes.Up;
			collection.Add(registryEntryVolume);

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_UniqueConsignRef = "Shipment1001";

			var container = shipment.Consols.AddNew().Containers.AddNew();
			container.JC_GrossVolumeUQ = Constants.Volume.CubicMetres;
			container.JC_JS_FCLBookingOnlyLink = shipment.PK;

			using (container.GetValidationSuspender())
			{
				container.JC_GrossWeight = 999999.999m;
			}

			AssertEquals((ZDecimal)999999.9, container.JC_GrossWeight);

			using (container.GetValidationSuspender())
			{
				container.JC_TareWeight = 999999.999m;
				container.JC_DunnageWeight = 999999.999m;
				container.JC_GrossVolume = 999999.999m;
				container.JC_VolumeCapacity = 999999.999m;
				container.JC_WeightCapacity = 999999.999m;
			}
			AssertEquals((ZDecimal)999999.9, container.JC_TareWeight);
			AssertEquals((ZDecimal)999999.9, container.JC_DunnageWeight);
			AssertEquals((ZDecimal)999999.9, container.JC_GrossVolume);

			AssertEquals((ZDecimal)999999.9, container.JC_VolumeCapacity);
			AssertEquals((ZDecimal)999999.9, container.JC_WeightCapacity);
		}

		public void TestIsImportExport()
		{
			ExportConsol.JK_TransportMode = Constants.TransportModes.Sea;
			Transport exportTransport = ExportConsol.Transports[0];
			exportTransport.JW_Vessel = "ARAFURA";
			exportTransport.JW_VoyageFlight = "22";
			exportTransport.JW_ETD = ZDateTime.Today;

			ImportConsol.JK_TransportMode = Constants.TransportModes.Sea;
			Transport importTransport = ImportConsol.Transports[0];
			importTransport.JW_Vessel = "ARAFURA";
			importTransport.JW_VoyageFlight = "23";
			importTransport.JW_ETA = ZDateTime.Today;

			CommonContainer container1 = Factory.New<CommonContainer>();
			ExportConsol.Containers.Add(container1);
			CommonContainer container2 = Factory.New<CommonContainer>();
			ImportConsol.Containers.Add(container2);

			Assert("Expecting Container 1 to be export", container1.IsExport());
			Assert("Not expecting Container 1 to be import", !container1.IsImport());
			Assert("Expecting Container 2 to beimport", container2.IsImport());
			Assert("Not expecting Container 2 to be export", !container2.IsExport());
		}

		public void TestDates_DateTimeKind_Unspecified()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var container = consol.Containers.AddNew();

			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_DepartureCartageComplete.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_ArrivalEstimatedDelivery.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_DepartureEstimatedPickup.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_EmptyReturnedBy.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_EmptyRequired.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_ArrivalSlotDateTime.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_DepartureSlotDateTime.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_FCLWharfGateOut.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_FCLWharfGateIn.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_ContainerYardEmptyPickupGateOut.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_ContainerYardEmptyReturnGateIn.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_FCLOnBoardVessel.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_FCLUnloadFromVessel.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_EmptyReadyForReturn.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_FCLAvailable.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_ArrivalCTOStorageStartDate.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_LCLAvailable.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_LCLStorageCommences.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_ArrivalCartageComplete.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_DepartureCartageAdvised.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_ArrivalCartageAdvised.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_PackDate.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.ArrivalTruckWaitTime.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, container.JC_LCLUnpack.Kind);
		}

		#endregion

		#region TestJC_PivotBreak

		public void TestJC_PivotBreak_UpdatingBreaksPerValueOnOneContainerUpdatesValueOfAllOtherContainersOfSameType()
		{
			var refContainerLD6 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-6");
			var refContainerLD7 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-7");

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.ULD;

			var container1 = AddContainer(refContainerLD6);
			var container2 = AddContainer(refContainerLD6);
			var container3 = AddContainer(refContainerLD7);
			var container4 = AddContainer(refContainerLD7);

			AssertContainers("Preconditions: Initially all containers' pivot break should be zero", 0, 0, 0, 0);

			container1.JC_PivotBreak = 200;
			AssertContainers("Updating container1 pivot break should also update container 2 pivot break.", 200, 200, 0, 0);

			container2.JC_PivotBreak = 300;
			AssertContainers("Updating container2 pivot break should also update container 1 pivot break.", 300, 300, 0, 0);

			container3.JC_PivotBreak = 400;
			AssertContainers("Updating container3 pivot break should also update container 4 pivot break.", 300, 300, 400, 400);

			container4.JC_PivotBreak = 500;
			AssertContainers("Updating container3 pivot break should also update container 4 pivot break.", 300, 300, 500, 500);

			void AssertContainers(string messages, ZDecimal expectedPivotBreak1, ZDecimal expectedPivotBreak2, ZDecimal expectedPivotBreak3, ZDecimal expectedPivotBreak4)
			{
				CombineAssertions(messages, () =>
				{
					AssertEquals("container1.JC_PivotBreak", expectedPivotBreak1, container1.JC_PivotBreak);
					AssertEquals("container2.JC_PivotBreak", expectedPivotBreak2, container2.JC_PivotBreak);
					AssertEquals("container3.JC_PivotBreak", expectedPivotBreak3, container3.JC_PivotBreak);
					AssertEquals("container4.JC_PivotBreak", expectedPivotBreak4, container4.JC_PivotBreak);
				});
			}

			CommonContainer AddContainer(RefContainer refContainer)
			{
				var container = consol.Containers.AddNew();
				container.JC_RC = refContainer.PK;
				container.JC_ContainerMode = Constants.ContainerModes.ULD;

				return container;
			}
		}

		public void TestJC_PivotBreak_WhenConsolIsNull()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			AssertNull("Precondiion:consol is null", container.Consol);
			container.JC_PivotBreak = 100m;
			AssertEquals(100m, container.JC_PivotBreak);
		}

		#endregion

		#region IContainerExtra

		public void TestGoodsWeight()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			CommonContainer container = Factory.New<CommonConsol>().Containers.AddNew();
			container.Consol.Shipments.Add(shipment);
			AssertEquals("goods weight should be 0", 0m, container.GoodsWeight);

			PackLine packline = shipment.OuterPackLines.AddNew();
			packline.SetContainer(container.Consol, container);
			packline.JL_ActualWeight = 44m;

			PackLine packline2 = shipment.OuterPackLines.AddNew();
			packline2.SetContainer(container.Consol, container);
			packline2.JL_ActualWeight = 55m;

			AssertEquals("Goods weight should be 99", 99m, container.GoodsWeight);
		}

		public void TestGoodsWeightUQ()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			AssertEquals("GoodsWeightUQ", Constants.Weight.Kilograms, container.GoodsWeightUQ);
		}

		#endregion

		#region TestIsMandatoryContainerType

		public void TestIsMandatoryContainerType()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_ContainerMode = Constants.ContainerModes.AIR;
			Assert(!container.IsMandatoryContainerType);
			container.JC_ContainerMode = Constants.ContainerModes.ULD;
			Assert(container.IsMandatoryContainerType);
			container.JC_ContainerMode = Constants.ContainerModes.Bulk;
			Assert(!container.IsMandatoryContainerType);
			container.JC_ContainerMode = Constants.ContainerModes.Liquid;
			Assert(!container.IsMandatoryContainerType);
			container.JC_ContainerMode = Constants.ContainerModes.BreakBulk;
			Assert(!container.IsMandatoryContainerType);
			container.JC_ContainerMode = Constants.ContainerModes.Loose;
			Assert(!container.IsMandatoryContainerType);
			container.JC_ContainerMode = Constants.ContainerModes.RollOnRollOff;
			Assert(!container.IsMandatoryContainerType);
			container.JC_ContainerMode = Constants.ContainerModes.NonContainerised;
			Assert(!container.IsMandatoryContainerType);
		}

		#endregion

		#region Validation Tests

		public void TestValidateJC_ContainerNum()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_ContainerNum = "123456123456";
			Assert("Container number does not have more than 12 characters, no errors expected", !container.JC_ContainerNumInfo.HasErrors());

			container.JC_ContainerNum = ZString.Empty;
			Assert("Container number can be empty, not expecting errors", !container.JC_ContainerNumInfo.HasErrors());
		}

		public void TestValidateJC_ContainerCount()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_ContainerCount = 0;
			Assert("Expecting Container Count to be 0 and have errors", container.JC_ContainerCountInfo.HasErrors());

			container.JC_ContainerCount = -17;
			Assert("Expecting Container Count to be negative and have errors", container.JC_ContainerCountInfo.HasErrors());

			container.JC_ContainerCount = 3;
			Assert("Expecting Container Count to be valid, not expecting errors", !container.JC_ContainerCountInfo.HasErrors());

			container.JC_ContainerNum = "Container";
			container.JC_ContainerCount = -6;

			Assert("Container count must be 1 if the container number isn't empty, errors expected", container.JC_ContainerCountInfo.HasErrors());

			container.JC_ContainerNum = ZString.Empty;
			container.JC_ContainerCount = -6;

			Assert("If the container number is empty, container count must be greater than 0, errors expected", container.JC_ContainerCountInfo.HasErrors());

			container.JC_ContainerNum = "Container";
			container.JC_ContainerCount = 6;

			Assert("If the container number isn't empty, container count must be 1, errors expected", container.JC_ContainerCountInfo.HasErrors());

			container.JC_ContainerNum = "Container";
			container.JC_ContainerCount = 1;

			Assert("If the container number isn't empty, container count must be 1, no errors expected", !container.JC_ContainerCountInfo.HasErrors());
		}

		public void TestValidateJC_GrossWeightVerificationTypeOnSettingJC_IsEmptyContainer()
		{
			var commonConsol = Factory.New<CommonConsol>();
			var container = commonConsol.Containers.AddNew();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			container.JC_IsEmptyContainer = true;
			AssertHasError("Checking 'Empty' box when 'PKG' is verified method should set 'PKG' as having errors", container.JC_GrossWeightVerificationTypeInfo, CommonContainerValidation.CannotAllowMethod2PackagesForEmptyContainer);
		}

		#endregion

		#region TestHasChanges

		public void TestHasChangesAfterSave()
		{
			//Setup a Consol with a Container and CommonShipment with Outer Pack allocated to the Container
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CCCC1111111";
			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_OuterPacks = 1;
			shipment1.JS_ActualWeight = 100m;
			shipment1.OuterPackLines[0].SetContainer(consol, container1);
			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_OuterPacks = 2;
			shipment2.JS_ActualWeight = 200m;
			shipment2.OuterPackLines[0].SetContainer(consol, container1);
			AssertEquals("Start condition. Container should be allocated to 2 OuterPacks", 2, container1.PackLines.Count);
			AssertEquals("Container.HasChanges before Save.", true, container1.HasChanges);
			Factory.Save();

			AssertEquals("Container.HasChanges after save.", false, container1.HasChanges);
		}

		#endregion

		#region Deleting A Container

		[ExpectNoExceptions]
		public void TestDeletingAContainerThatBelongsToAConsol()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			Assert("Should be able to delete it", ((ICanDelete)container).CanDelete);
			container.Delete();
		}

		public void TestDeletingAContainerWithDeclarationAttached()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "C1";

			// Set up CusContainer with same Container
			var shipment = consol.Shipments.AddNew();
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS.Name] = shipment.PK;

			var cusContainer = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer[CusContainerSchema.CO_JE.Name] = declaration.PK;
			cusContainer[CusContainerSchema.CO_JC.Name] = container.PK;

			var cusContainer2 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer2[CusContainerSchema.CO_JE.Name] = declaration.PK;
			cusContainer2[CusContainerSchema.CO_JC.Name] = container.PK;

			Factory.Save();

			var containerFilter = new ZQuery(CusContainerSchema.CO_JC, container.PK);
			AssertEquals("Should be 1 CusContainer with this JobContainer", 2, Factory.GetDatabaseCount(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(), containerFilter));
			AssertEquals("PreCondition: JobContainer is not deleted", false, container.IsDeleted);

			Assert("Should not be able to delete it, but I will anyway", !((ICanDelete)container).CanDelete);

			container.Delete();

			AssertEquals(true, container.IsDeleted);
			AssertEquals(false, cusContainer.IsDeleted);
			AssertEquals(false, cusContainer2.IsDeleted);
			AssertEquals(ZGuid.Empty, cusContainer[CusContainerSchema.CO_JC]);
			AssertEquals(ZGuid.Empty, cusContainer2[CusContainerSchema.CO_JC]);
		}

		public void TestLinkedCusContainerNotReset_WhenJobContianerAlreadyDeleted()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "C1";

			var shipment = consol.Shipments.AddNew();
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS.Name] = shipment.PK;

			var cusContainer = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer[CusContainerSchema.CO_JE.Name] = declaration.PK;
			cusContainer[CusContainerSchema.CO_JC.Name] = container.PK;
			Factory.Save();

			AssertEquals("Precondtion", container.PK, cusContainer[CusContainerSchema.CO_JC]);

			((IBusinessObjectInternals)container).MarkAsDeleted();
			container.Delete();
			AssertEquals("The linked CusContainer not be reset when JobContianer is already deleted and avoid the issue when Delete is called multiple time.", container.PK, cusContainer[CusContainerSchema.CO_JC]);
		}

		public void TestDeletedCusContainerNotReset_WhenCusContainerMarkedAsDeleted()
		{
			var extraFactory = new BusinessObjectFactory() { NameForDebugging = "TestDeletedCusContainerNotReset" };
			var consol = extraFactory.New<CommonConsol>();
			var jobContainer = consol.Containers.AddNew();
			jobContainer.JC_ContainerNum = "C1";
			jobContainer.JC_JK = consol.PK;

			var shipment = consol.Shipments.AddNew();
			var declaration = (BusinessObject)extraFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS.Name] = shipment.PK;
			var cusContainer = (BusinessObject)extraFactory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer[CusContainerSchema.CO_JE.Name] = declaration.PK;
			cusContainer[CusContainerSchema.CO_JC.Name] = jobContainer.PK;
			extraFactory.Save();

			CombineAssertions(() =>
			{
				var cusContainers = (BusinessObject[])extraFactory.Load<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(new ZQuery(CusContainerSchema.CO_JC, jobContainer.PK));
				((IBusinessObjectInternals)cusContainer).MarkAsDeleted();
				var row = ((INeedRow)cusContainer).Row;
				AssertEquals("MarkAsDeleted: IsDeleted", true, cusContainer.IsDeleted);
				AssertEquals("MarkAsDeleted is not actual delete and would not change row.RowState", DataRowState.Unchanged, row.RowState);
				var cusContainers2 = (BusinessObject[])extraFactory.Load<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(new ZQuery(CusContainerSchema.CO_JC, jobContainer.PK));
				AssertContainsExactElementsInAnyOrder("As CreateBusinessObjectsFromRows inside factory.Load() use BusinessObjectCache which has catched the deleted cusContainer and so it still loaded.", cusContainer, cusContainers2);
				AssertEquals("TestDeletedCusContainerNotReset", jobContainer.Factory.NameForDebugging);
				AssertNoExceptionThrown("No accessing deleted business object Error as the Row is Accessible", () => jobContainer.Delete());

				row = ((INeedRow)cusContainer).Row;
				row.Delete();
				AssertEquals("Actual deleted", true, cusContainer.IsDeleted);
				AssertEquals("The Row deleted", DataRowState.Deleted, row.RowState);
				var cusContainers3 = (BusinessObject[])extraFactory.Load<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(new ZQuery(CusContainerSchema.CO_JC, jobContainer.PK));
				AssertEquals("the deleted cusContainer would not be loaded when the ROW is deleted and would not be loaded in Rows.", 0, cusContainers3.Length);

				AssertEquals("TestDeletedCusContainerNotReset", jobContainer.Factory.NameForDebugging);
				AssertNoExceptionThrown("No accessing deleted business object Error as the deleted container would be not loaded and touched", () => jobContainer.Delete());
			});
		}

		public void TestDeletedCusContainerNotReset_WhenCusContainerIsActuallyDeleted()
		{
			var extraFactory = new BusinessObjectFactory() { NameForDebugging = "TestDeletedCusContainerNotReset" };
			var consol = extraFactory.New<CommonConsol>();
			var jobContainer = consol.Containers.AddNew();
			jobContainer.JC_ContainerNum = "C1";
			jobContainer.JC_JK = consol.PK;

			var shipment = consol.Shipments.AddNew();
			var declaration = (BusinessObject)extraFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS.Name] = shipment.PK;
			var cusContainer = (BusinessObject)extraFactory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer[CusContainerSchema.CO_JE.Name] = declaration.PK;
			cusContainer[CusContainerSchema.CO_JC.Name] = jobContainer.PK;
			extraFactory.Save();

			CombineAssertions(() =>
			{
				var cusContainers = (BusinessObject[])extraFactory.Load<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(new ZQuery(CusContainerSchema.CO_JC, jobContainer.PK));
				cusContainer.Delete();
				var row = ((INeedRow)cusContainer).Row;
				AssertEquals("IsDeleted", true, cusContainer.IsDeleted);
				AssertEquals("row.RowState", DataRowState.Deleted, row.RowState);
				var cusContainers3 = (BusinessObject[])extraFactory.Load<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(new ZQuery(CusContainerSchema.CO_JC, jobContainer.PK));
				AssertEquals("the deleted cusContainer would not be loaded as it removed from BusinessObjectCache when Delete().", 0, cusContainers3.Length);

				AssertEquals("TestDeletedCusContainerNotReset", jobContainer.Factory.NameForDebugging);
				AssertNoExceptionThrown("No Error: Should not be accessing a property on a deleted business object", () => jobContainer.Delete());
			});
		}

		public void TestDeletedCusContainerNoExceptionThrow_CusContainersDeletedWhenChanging()
		{
			var consol = Factory.New<CommonConsol>();
			var jobContainer = consol.Containers.AddNew();
			jobContainer.JC_ContainerNum = "C1";
			jobContainer.JC_JK = consol.PK;

			var shipment = consol.Shipments.AddNew();
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS.Name] = shipment.PK;
			var cusContainer = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer[CusContainerSchema.CO_JE.Name] = declaration.PK;
			cusContainer[CusContainerSchema.CO_JC.Name] = jobContainer.PK;

			var cusContainer2 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer2[CusContainerSchema.CO_JE.Name] = declaration.PK;
			cusContainer2[CusContainerSchema.CO_JC.Name] = jobContainer.PK;
			Factory.Save();

			cusContainer.HasChangesChanged += (object sender, HasChangesChangedEventArgs e) => cusContainer2.Delete();
			AssertNoExceptionThrown("No Error: Should not be accessing a property on a deleted business object", jobContainer.Delete);
		}

		public void TestDeletingAContainerWithShipmentAttached()
		{
			// Create the Consolation and add two containers
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CRUX1";

			// Set up CusContainer with same Container
			var shipment = consol.Shipments.AddNew();
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS.Name] = shipment.PK;

			var cusContainer = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer[CusContainerSchema.CO_JE.Name] = declaration.PK;
			cusContainer[CusContainerSchema.CO_JC.Name] = container.PK;

			var cusContainer2 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer2[CusContainerSchema.CO_JE.Name] = declaration.PK;

			Factory.Save();

			Assert("Container one should not be deletable", !container.CanDelete);
			Assert("Container two should be deletable", container2.CanDelete);
		}

		public void TestDeletingAContainerWithSupplierBookingAttached_AndRegistryEnabled()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var consol = Factory.New<CommonConsol>();
				var container = consol.Containers.AddNew();
				var supplierBooking = Factory.New<IJobSupplierBooking>();
				container.JC_JSB_SupplierBooking = (supplierBooking as BusinessObject).PK;

				foreach (var status in typeof(Constants.SupplierBookingStatus).GetFields().Select(x => x.GetValue(null)).OfType<string>())
				{
					supplierBooking.JSB_Status = status;

					var nonEditableStatuses = new[] { Constants.SupplierBookingStatus.Planned, Constants.SupplierBookingStatus.Converted };
					var expectedCanDelete = !nonEditableStatuses.Contains(status);
					var expectedMessage = container.CanDelete ? string.Empty : container.Validation.CanNotDeleteContainerWhenItHasLoadListLineMessage;

					AssertEquals("Should be able to delete containers if Supplier Booking is not PLN or CNV", expectedCanDelete, container.CanDelete);
					AssertEquals(container.ReasonForNotAbleToDelete, expectedMessage);
				}
			});
		}

		public void TestDeletingAContainerWithSupplierBookingAttached_AndRegistryDisabled()
		{
			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				var consol = Factory.New<CommonConsol>();
				var container = consol.Containers.AddNew();
				var supplierBooking = Factory.New<IJobSupplierBooking>();
				container.JC_JSB_SupplierBooking = (supplierBooking as BusinessObject).PK;

				foreach (var status in typeof(Constants.SupplierBookingStatus).GetFields().Select(x => x.GetValue(null)).OfType<string>())
				{
					supplierBooking.JSB_Status = status;
					Assert("Should allow delete with any status if registry disabled", container.CanDelete);
					AssertEquals(string.Empty, container.ReasonForNotAbleToDelete);
				}
			});
		}

		public void TestDeletingAContainerWithSupplierBookingAttached_AfterRemovingUneditableSupplierBooking()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var consol = Factory.New<CommonConsol>();
				var container = Factory.New<DummyContainerWithAttachedContainerLoadList>();
				consol.Containers.Add(container);
				var supplierBooking = Factory.New<IJobSupplierBooking>();
				supplierBooking.JSB_BookingId = "SB1000001";
				supplierBooking.JSB_Status = Constants.SupplierBookingStatus.Planned;
				supplierBooking.JSB_TransportMode = Constants.TransportModes.Sea;
				supplierBooking.JSB_LoadMode = Constants.SupplierBookingLoadMode.ContainerYard;
				supplierBooking.JSB_OH_BookingParty = Factory.NewWithValidTestData<OrgHeader>().PK;
				container.JC_JSB_SupplierBooking = (supplierBooking as BusinessObject).PK;

				Factory.Save();

				Assert("Pre-Condition: Should not be able to delete as Status is PLN", !container.CanDelete);

				container.JC_JSB_SupplierBooking = ZGuid.Empty;

				Assert("Should check original value and should not be deletable", !container.CanDelete);
			});
		}

		#endregion

		#region Calculated Properties Test

		#region TestJC_ContainerCode

		public void TestJC_ContainerCode()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			AssertEquals("new container", ZString.Empty, container.JC_ContainerCode);

			container.JC_ContainerCount = 4;
			container.JC_RC = RC_20GP_PK;
			AssertEquals("Has type and count set", "20GP (4)", container.JC_ContainerCode);

			container.JC_ContainerNum = "FAKE4100011";
			AssertEquals("Has JC_ContainerNum set", "FAKE4100011", container.JC_ContainerCode);
		}

		#endregion

		public void TestJC_Calc_TEUCount()
		{
			CommonContainer container = Factory.New<CommonContainer>();

			container.JC_ContainerCount = 0;
			AssertEquals("TEU Count should be 1", 1m, container.JC_Calc_TEUCount);

			container.JC_RC = ContainerRef2.PK;
			AssertEquals("TEU Count should be 2", 2m, container.JC_Calc_TEUCount);

			container.JC_ContainerCount = 5;
			AssertEquals("TEU Count should be 10", 10m, container.JC_Calc_TEUCount);

			container.JC_RC = ContainerRef.PK;
			AssertEquals("TEU Count should be 5", 5m, container.JC_Calc_TEUCount);
		}

		public void TestRefContainer()
		{
			CommonContainer container = Factory.New<CommonContainer>();

			AssertNull("Expecting ref container to be null.", container.RefContainer);

			container.JC_RC = ContainerRef.PK;

			AssertNotNull("Not expecting ref container to be null.", container.RefContainer);
		}

		public void TestJC_Calc_ContainerCapacity()
		{
			CommonContainer container = Factory.New<CommonContainer>();

			Assert("Expecting JC_Calc_ContainerCapacity to be empty.", container.JC_Calc_ContainerCapacity.IsEmpty);

			container.JC_RC = ContainerRef.PK;

			AssertEquals("Expecting JC_Calc_ContainerCapacity to be 16m.", 16m, container.JC_Calc_ContainerCapacity);
		}

		public void TestJC_Calc_GrossWeight()
		{
			CommonContainer container = Factory.New<CommonContainer>();

			Assert("Expecting JC_Calc_GrossWeight to be empty.", container.JC_Calc_MaxGrossWeight.IsEmpty);

			container.JC_RC = ContainerRef.PK;

			AssertEquals("Expecting JC_Calc_GrossWeight to be 10000m.", 10000m, container.JC_Calc_MaxGrossWeight);

			container.JC_ContainerCount = 3;

			AssertEquals("Expecting JC_Calc_GrossWeight to be 30000m (3 * 10000m)", 30000m, container.JC_Calc_MaxGrossWeight);

			container.JC_ContainerCount = 0;

			AssertEquals("Expecting JC_Calc_GrossWeight to be 0m", 0m, container.JC_Calc_MaxGrossWeight);
		}

		public void TestJC_Calc_TareWeight()
		{
			CommonContainer container = Factory.New<CommonContainer>();

			Assert("Expecting JC_Calc_TareWeight to be empty.", container.JC_Calc_TareWeight.IsEmpty);

			container.JC_RC = ContainerRef.PK;

			AssertEquals("Expecting JC_Calc_TareWeight to be 1000m.", 1000m, container.JC_Calc_TareWeight);

			container.JC_ContainerCount = 6;

			AssertEquals("Expecting JC_Calc_TareWeight to be 6000m. (6 * 1000m)", 6000m, container.JC_Calc_TareWeight);
		}

		public void TestJC_Calc_Length()
		{
			CommonContainer container = Factory.New<CommonContainer>();

			Assert("Expecting JC_Calc_Length to be empty.", container.JC_Calc_Length.IsEmpty);

			container.JC_RC = ContainerRef.PK;

			AssertEquals("Expecting JC_Calc_Length to be 6m.", 6m, container.JC_Calc_Length);
		}

		public void TestJC_Calc_Height()
		{
			CommonContainer container = Factory.New<CommonContainer>();

			Assert("Expecting JC_Calc_Height to be empty.", container.JC_Calc_Height.IsEmpty);

			container.JC_RC = ContainerRef.PK;

			AssertEquals("Expecting JC_Calc_Height to be 3m.", 3m, container.JC_Calc_Height);
		}

		public void TestJC_Calc_Width()
		{
			CommonContainer container = Factory.New<CommonContainer>();

			Assert("Expecting JC_Calc_Width to be empty.", container.JC_Calc_Width.IsEmpty);

			container.JC_RC = ContainerRef.PK;

			AssertEquals("Expecting JC_Calc_Width to be 2m.", 2m, container.JC_Calc_Width);
		}

		public void TestJC_Calc_OverhangLength()
		{
			CommonContainer container = Factory.New<CommonContainer>();

			AssertEquals(true, container.JC_Calc_OverhangLength.IsEmpty);

			container.JC_TotalLength = 2m;

			AssertEquals(2m, container.JC_Calc_OverhangLength);
		}

		public void TestJC_Calc_OverhangHeight()
		{
			CommonContainer container = Factory.New<CommonContainer>();

			AssertEquals(true, container.JC_Calc_OverhangHeight.IsEmpty);

			container.JC_TotalHeight = 3m;

			AssertEquals(3m, container.JC_Calc_OverhangHeight);
		}

		public void TestJC_Calc_OverhangWidth()
		{
			CommonContainer container = Factory.New<CommonContainer>();

			AssertEquals(true, container.JC_Calc_OverhangWidth.IsEmpty);

			container.JC_TotalWidth = 4m;

			AssertEquals(4m, container.JC_Calc_OverhangWidth);
		}

		public void TestJC_Calc_OverhangFront()
		{
			var container = Factory.New<CommonContainer>();
			AssertEquals(0m, container.JC_Calc_OverhangFront);

			container.JC_RC = ContainerRef.PK;
			AssertEquals(0m, container.JC_Calc_OverhangFront);

			container.JC_TotalLength = 8m;
			AssertEquals(2m, container.JC_Calc_OverhangFront);

			container.JC_OverhangBack = 0.5m;
			AssertEquals(1.5m, container.JC_Calc_OverhangFront);

			container.JC_OverhangBack = 2.5m;
			AssertEquals(0m, container.JC_Calc_OverhangFront);
		}

		public void TestJC_Calc_OverhangLeft()
		{
			var container = Factory.New<CommonContainer>();
			AssertEquals(0m, container.JC_Calc_OverhangLeft);

			container.JC_RC = ContainerRef.PK;
			AssertEquals(0m, container.JC_Calc_OverhangLeft);

			container.JC_TotalWidth = 4m;
			AssertEquals(2m, container.JC_Calc_OverhangLeft);

			container.JC_OverhangRight = 0.5m;
			AssertEquals(1.5m, container.JC_Calc_OverhangLeft);

			container.JC_OverhangRight = 2.5m;
			AssertEquals(0m, container.JC_Calc_OverhangLeft);
		}

		public void TestJC_Calc_MasterBillNum()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			AssertNull("Precondition - container not attached to a consol ", container.Consol);
			Assert(container.JC_Calc_MasterBillNum.IsEmpty);

			CommonConsol consol = Factory.New<CommonConsol>();
			container.JC_JK = consol.PK;
			consol.JK_MasterBillNum = "12938";

			AssertNotNull("Precondition - container attached to a consol ", container.Consol);
			AssertEquals("12938", container.JC_Calc_MasterBillNum);
		}

		public void TestContainerModeListBasedOnTransport()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = "1234";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Origins[0].JA_E_DEP = ZDateTime.Now.AddDays(1);
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.Destinations[0].JB_E_ARV = ZDateTime.Now.AddDays(10);
			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];

			CommonContainer container = sailing.Containers.AddNew();
			AssertEquals("Sea Items", 7, container.JC_ContainerMode_List.Count);
			AssertEquals("Sea Items", "LCL", container.JC_ContainerMode_List[0].Code);

			sailing.Voyage.JV_AirSeaRoad = "ROA";
			AssertEquals("Road Items", 7, container.JC_ContainerMode_List.Count);
			AssertEquals("Road Items", "FCL", container.JC_ContainerMode_List[0].Code);

			sailing.Voyage.JV_AirSeaRoad = "AIR";
			AssertEquals("Air Items", 1, container.JC_ContainerMode_List.Count);
			AssertEquals("Air Items", "ULD", container.JC_ContainerMode_List[0].Code);
		}

		public void TestIsGrossWeightVerified()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_GrossWeightVerificationType = "XYZ";
			Assert("XYZ", !container.IsGrossWeightVerified);

			container.JC_GrossWeightVerificationType = "NON";
			Assert("NON", !container.IsGrossWeightVerified);

			container.JC_GrossWeightVerificationType = "CNT";
			Assert("CNT", container.IsGrossWeightVerified);

			container.JC_GrossWeightVerificationType = "PKG";
			Assert("PKG", container.IsGrossWeightVerified);

			container.JC_GrossWeightVerificationType = "WTA";
			Assert("WTA", container.IsGrossWeightVerified);

			container.JC_GrossWeightVerificationType = "RTL";
			Assert("RTL", container.IsGrossWeightVerified);

			container.JC_GrossWeightVerificationType = "WTA";
			Assert("WTA", container.IsGrossWeightVerified);
		}

		public void TestJC_GrossWeight_ReadOnly()
		{
			var container = Factory.New<CommonContainer>();
			AssertEquals(false, container.JC_GrossWeightInfo.ReadOnly);

			container.JC_JK = Factory.New<CommonConsol>().PK;
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			AssertEquals(false, container.JC_GrossWeightInfo.ReadOnly);

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			AssertEquals(true, container.JC_GrossWeightInfo.ReadOnly);

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			AssertEquals(false, container.JC_GrossWeightInfo.ReadOnly);
		}

		public void TestJC_GrossWeight_NotWithinSqlPrecisionAndScale()
		{
			var container = Factory.NewWithValidTestData<CommonContainer>();
			container.JC_ContainerNum = "CONTAINER1";
			container.JC_GrossWeight = 100m;

			ErrorReporter.Clear();

			using (container.GetValidationSuspender())
			{
				container.JC_GrossWeight = 1995840m;
				AssertExceptionThrown<ZSaveException>("JC_GrossWeight is out of decimal range", Factory.Save);
			}

			AssertEquals("CommonContainer_NotWithinSqlPrecisionAndScale", ErrorReporter.LastKeyReported);
			AssertContains("Error report should be generated.", $@"Container PK = {container.PK}
JC_ContainerNum = CONTAINER1
JC_ContainerCount = 1
JC_TareWeight = 0
JC_Calc_TareWeight = 0
JC_GrossWeight previous value = 100, new value = 1995840", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestGrossWeightRecalculation_NonVerificationType_TareWeightChanged()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			container.JC_GrossWeight = 33.2;
			container.JC_TareWeight = 22.3;
			AssertEquals((ZDecimal)55.5, container.JC_GrossWeight);
		}

		public void TestGrossWeightRecalculation_NonVerificationType_DunnageWeightChanged()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			container.JC_GrossWeight = 33.2;
			container.JC_DunnageWeight = 22.3;
			AssertEquals((ZDecimal)55.5, container.JC_GrossWeight);
		}

		public void TestGrossWeightRecalculation_NonVerificationType_PackLineChanged()
		{
			var packline = Factory.New<PackLine>();
			packline.JL_FreightMode = FreightConstants.OuterPackType;
			var container = packline.Containers.AddNew();
			container.JC_GrossWeight = 33;
			packline.JL_ActualWeight = 22;
			AssertEquals((ZDecimal)22, container.JC_GrossWeight);
		}

		public void TestGrossWeightRecalculation_ContainerVerificationType_TareWeightChanged()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.JC_GrossWeight = 33.2;
			container.JC_TareWeight = 22.3;
			AssertEquals((ZDecimal)33.2, container.JC_GrossWeight);
		}

		public void TestGrossWeightRecalculation_ContainerVerificationType_DunnageWeightChanged()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.JC_GrossWeight = 33.2;
			container.JC_DunnageWeight = 22.3;
			AssertEquals((ZDecimal)33.2, container.JC_GrossWeight);
		}

		public void TestGrossWeightRecalculation_ContainerVerificationType_PackLineChanged()
		{
			var packline = Factory.New<PackLine>();
			packline.JL_FreightMode = FreightConstants.OuterPackType;
			var container = packline.Containers.AddNew();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.JC_GrossWeight = 33;
			packline.JL_ActualWeight = 22;
			AssertEquals((ZDecimal)33, container.JC_GrossWeight);
		}

		public void TestGrossWeightRecalculation_PackageVerificationType_TareWeightChanged()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			container.JC_GrossWeight = 33.2;
			container.JC_TareWeight = 22.3;
			AssertEquals((ZDecimal)33.2, container.JC_GrossWeight);
		}

		public void TestGrossWeightRecalculation_PackageVerificationType_DunnageWeightChanged()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			container.JC_GrossWeight = 33.2;
			container.JC_DunnageWeight = 22.3;
			AssertEquals((ZDecimal)33.2, container.JC_GrossWeight);
		}

		public void TestGrossWeightRecalculation_PackageVerificationType_PackLineChanged()
		{
			var packline = Factory.New<PackLine>();
			packline.JL_FreightMode = FreightConstants.OuterPackType;
			var container = packline.Containers.AddNew();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			container.JC_GrossWeight = 33;
			packline.JL_ActualWeight = 22;
			AssertEquals((ZDecimal)33, container.JC_GrossWeight);
		}

		public void TestGrossWeightRecalculation_ResettingVerificationTypeToNoneRecalculatesGrossWeight()
		{
			var packline = Factory.New<PackLine>();
			packline.JL_FreightMode = FreightConstants.OuterPackType;
			packline.JL_ActualWeight = 22;

			var container = Factory.New<CommonContainer>();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.JC_GrossWeight = 22;
			container.JC_TareWeight = 10;
			container.JC_DunnageWeight = 11;

			packline.Containers.Add(container);

			AssertEquals("Precondition: no recalculation on method 1 and 2", (ZDecimal)22, container.JC_GrossWeight);

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			AssertEquals((ZDecimal)43, container.JC_GrossWeight);
		}

		[TestDate(2016, 04, 08, 14, 43, 22)]
		public void TestDefaultVerifiedDateWhenVerifiedTypedChanged()
		{
			var container = Factory.New<CommonContainer>();

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			AssertEquals(TestDateAttribute.Date, container.JC_GrossWeightVerificationDateTime);

			container.GrossWeightVerifiedByAddress.OrganisationPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			AssertEquals(ZDateTime.Empty, container.JC_GrossWeightVerificationDateTime);
			AssertEquals(Guid.Empty, container.GrossWeightVerifiedByAddress.OrganisationPK);
		}

		public void TestVerifiedByValidation_NoOverriden_OrgPK()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			AssertNoWarnings(container.GrossWeightVerifiedByAddress.OrganisationPKInfo);

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			AssertHasWarning(container.GrossWeightVerifiedByAddress.OrganisationPKInfo, "The party designated to ascertain the weight is not mandatory but is recommended to be sent in the VGM message.");

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			container.GrossWeightVerifiedByAddress.OrganisationPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			AssertHasError(container.GrossWeightVerifiedByAddress.OrganisationPKInfo, "Please do not enter a VGM Verified By: Organization.");

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			container.GrossWeightVerifiedByAddress.OrganisationPK = Guid.Empty;
			AssertHasWarning(container.GrossWeightVerifiedByAddress.OrganisationPKInfo, "The party designated to ascertain the weight is not mandatory but is recommended to be sent in the VGM message.");
		}

		public void TestVerifiedByValidation_OrganisationNameOrPK_Validation()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			AssertNoErrors(container.GrossWeightVerifiedByAddress.OrganisationPKInfo);
			AssertNoErrors(container.GrossWeightVerifiedByAddress.OrganisationNameOrPKInfo);

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			AssertHasWarning(container.GrossWeightVerifiedByAddress.OrganisationPKInfo, "The party designated to ascertain the weight is not mandatory but is recommended to be sent in the VGM message.");
			AssertHasWarning(container.GrossWeightVerifiedByAddress.OrganisationNameOrPKInfo, "The party designated to ascertain the weight is not mandatory but is recommended to be sent in the VGM message.");

			container.GrossWeightVerifiedByAddress.OrganisationPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			AssertNoWarnings(container.GrossWeightVerifiedByAddress.OrganisationPKInfo);
			AssertNoWarnings(container.GrossWeightVerifiedByAddress.OrganisationNameOrPKInfo);
		}

		public void TestVerifiedByValidation_Overriden_CompanyName()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			container.GrossWeightVerifiedByAddress.E2_AddressOverride = true;
			AssertNoErrors(container.GrossWeightVerifiedByAddress.E2_CompanyNameInfo);

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.GrossWeightVerifiedByAddress.E2_AddressOverride = true;
			AssertHasError(container.GrossWeightVerifiedByAddress.E2_CompanyNameInfo, "Please enter a VGM Verified By: Company Name.");

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			container.GrossWeightVerifiedByAddress.E2_AddressOverride = true;
			container.GrossWeightVerifiedByAddress.E2_CompanyName = "Test";
			AssertHasError(container.GrossWeightVerifiedByAddress.E2_CompanyNameInfo, "Please do not enter a VGM Verified By: Company Name.");

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			container.GrossWeightVerifiedByAddress.E2_AddressOverride = true;
			container.GrossWeightVerifiedByAddress.E2_CompanyName = "";
			AssertHasError(container.GrossWeightVerifiedByAddress.E2_CompanyNameInfo, "Please enter a VGM Verified By: Company Name.");
		}

		public void TestVerifiedByValidation_VGM()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "GBLON";
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var transport = consol.Transports[0];
			transport.JW_JX = ExportSailing1.PK;

			consol.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			consol.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.TaxIDNumber;
			orgCusCode.OK_CustomsRegNo = "1234/AA/0111";
			orgCusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedKingdom;
			orgCusCode.OK_OA_PremisesAddress = address.PK;

			var container = consol.Containers.AddNew();
			container.GrossWeightVerifiedByAddress.OrganisationPK = orgHeader.PK;
			container.GrossWeightVerifiedByAddress.E2_OA_Address = address.PK;
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			AssertHasWarning(container.GrossWeightVerifiedByAddress.OrganisationPKInfo, "The 'VGM Verified By' party is not approved to use Method 2. Press F3 on the VGM Verified By organization code to view their Organization and check their status (Config tab > Registration Numbers & Codes tab, select Type = VGM).");

			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VGMRegistrationNumber;
			container.GrossWeightVerifiedByAddress.OrganisationPK = Guid.Empty;
			container.GrossWeightVerifiedByAddress.OrganisationPK = orgHeader.PK;
			AssertNoWarnings(container.GrossWeightVerifiedByAddress.OrganisationPKInfo);
		}

		public void TestGrossWeightVerifiedByFieldType()
		{
			var container = Factory.New<CommonContainer>();
			container.GrossWeightVerifiedByAddress.E2_AddressOverride = true;
			AssertEquals(nameof(FieldType.Text), container.GrossWeightVerifiedByFieldType);

			container.GrossWeightVerifiedByAddress.E2_AddressOverride = false;
			AssertEquals(nameof(FieldType.OrganisationGuid), container.GrossWeightVerifiedByFieldType);
		}

		public void TestGrossWeightVerifiedBy()
		{
			var org = Factory.New<OrgHeader>();
			var container = Factory.New<CommonContainer>();
			container.GrossWeightVerifiedByAddress.OrganisationPK = org.PK;

			AssertEquals(org, container.GrossWeightVerifiedBy);
		}

		public void TestOnSaving_GrossWeightVerificationTypeChanged()
		{
			var container = Factory.NewWithValidTestData<CommonContainer>();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			Factory.Save();

			var logNON = container.Logs.Find(l => l.SL_SE_NKEvent == Events.QuantityVerified.Code).FirstOrDefault();
			AssertNull("Precondition - no log was added", logNON);

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.JC_GrossWeight = 55;
			Factory.Save();

			var logCNT = container.Logs.Find(l => l.SL_SE_NKEvent == Events.QuantityVerified.Code).FirstOrDefault();
			AssertNotNull(logCNT);
			Assert(!logCNT.IsCancelled);

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			var logPKG = container.Logs.Find(l => l.SL_SE_NKEvent == Events.QuantityVerified.Code).FirstOrDefault();
			AssertNotNull(logPKG);
			Assert(!logPKG.IsCancelled);

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired;
			var logNRQ = container.Logs.Find(l => l.SL_SE_NKEvent == Events.QuantityVerified.Code).FirstOrDefault();
			AssertNotNull(logNRQ);
			Assert(!logNRQ.IsCancelled);

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			Factory.Save();
			container.Logs.Find(l => l.SL_SE_NKEvent == Events.QuantityVerified.Code).ForEach(x => Assert(x.SL_IsCancelled));
		}

		public void TestOnSaving_QTVEventCreatedOnNewContainer()
		{
			var container = Factory.NewWithValidTestData<CommonContainer>();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.JC_GrossWeight = 33;

			Factory.Save();

			var log = container.Logs.Find(l => l.SL_SE_NKEvent == Events.QuantityVerified.Code).FirstOrDefault();
			AssertNotNull(log);
			Assert(!log.IsCancelled);
		}

		public void TestVGMVerfiedByOrg_Default()
		{
			var consignor1 = Factory.New<OrgHeader>();
			Shipment1.ConsignorPK = consignor1.PK;

			ExportConsol.JK_AgentType = Constants.AgentType.Direct;
			ExportConsol.Shipments.Add(Shipment1);

			var container1 = Factory.NewWithValidTestData<CommonContainer>();
			ExportConsol.Containers.Add(container1);

			container1.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			AssertEquals("Not defaulted", ZGuid.Empty, container1.GrossWeightVerifiedByAddress.OrganisationPK);

			container1.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			AssertEquals("Defaulted", consignor1.PK, container1.GrossWeightVerifiedByAddress.OrganisationPK);

			container1.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			AssertEquals("Not defaulted", ZGuid.Empty, container1.GrossWeightVerifiedByAddress.OrganisationPK);

			container1.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			AssertEquals("Defaulted", consignor1.PK, container1.GrossWeightVerifiedByAddress.OrganisationPK);
		}

		public void TestVGMVerfiedByContact_Default()
		{
			var consignor1 = Factory.New<OrgHeader>();

			var contact1 = consignor1.Contacts.AddNew();
			contact1.OC_ContactName = "Bob";
			var document1 = contact1.Documents.AddNew();
			document1.OD_DocumentGroup = ContactType.All.Code;

			var contact2 = consignor1.Contacts.AddNew();
			contact2.OC_ContactName = "Frank";
			var document2 = contact2.Documents.AddNew();
			document2.OD_DocumentGroup = ContactType.VerifiedGrossWeightContact.Code;

			Shipment1.ConsignorPK = consignor1.PK;

			var container1 = Factory.NewWithValidTestData<CommonContainer>();
			container1.GrossWeightVerifiedByAddress.OrganisationPK = consignor1.PK;
			AssertEquals("Defaulted", "Frank", container1.GrossWeightVerifiedByAddress.E2_Contact);

			var consignor3 = Factory.New<OrgHeader>();
			var contact3 = consignor3.Contacts.AddNew();
			contact3.OC_ContactName = "Tom";
			Shipment1.ConsignorPK = consignor3.PK;

			var container2 = Factory.NewWithValidTestData<CommonContainer>();
			container2.GrossWeightVerifiedByAddress.OrganisationPK = consignor3.PK;
			AssertEquals("Not defaulted", ZString.Empty, container2.GrossWeightVerifiedByAddress.E2_Contact);
		}

		public void TestVGMVerfiedByContact_Default_FallbackToAllDocumentGroup()
		{
			var consignor1 = Factory.New<OrgHeader>();

			var contact1 = consignor1.Contacts.AddNew();
			contact1.OC_ContactName = "Bob";
			var document1 = contact1.Documents.AddNew();
			document1.OD_DocumentGroup = ContactType.All.Code;

			Shipment1.ConsignorPK = consignor1.PK;

			var container1 = Factory.NewWithValidTestData<CommonContainer>();
			container1.GrossWeightVerifiedByAddress.OrganisationPK = consignor1.PK;
			AssertEquals("Defaulted", "Bob", container1.GrossWeightVerifiedByAddress.E2_Contact);

			var consignor2 = Factory.New<OrgHeader>();
			var contact2 = consignor2.Contacts.AddNew();
			contact2.OC_ContactName = "Tom";
			Shipment1.ConsignorPK = consignor2.PK;

			var container2 = Factory.NewWithValidTestData<CommonContainer>();
			container2.GrossWeightVerifiedByAddress.OrganisationPK = consignor2.PK;
			AssertEquals("Not defaulted", ZString.Empty, container2.GrossWeightVerifiedByAddress.E2_Contact);
		}

		public void TestVGMVerifiedByOrg_DefaultForWeightAtTerminal()
		{
			var consignor1 = Factory.New<OrgHeader>();
			Shipment1.ConsignorPK = consignor1.PK;

			ExportConsol.JK_AgentType = Constants.AgentType.Agent;
			ExportConsol.Shipments.Add(Shipment1);

			var container1 = Factory.NewWithValidTestData<CommonContainer>();
			ExportConsol.Containers.Add(container1);

			container1.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal;
			AssertEquals("Not defaulted as no CTO", ZGuid.Empty, container1.GrossWeightVerifiedByAddress.OrganisationPK);

			var ctoAddress = Factory.LoadTop1<OrgAddress>(new ZQuery());

			ExportConsol.JK_OA_DepartureCTOAddress = ctoAddress.PK;

			container1.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			container1.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal;

			AssertEquals("Defaulted to departure CTO", ctoAddress.OA_OH, container1.GrossWeightVerifiedByAddress.OrganisationPK);
		}

		public void TestVGMVerifiedByDefaults_ReportsOwnVGM()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.Addresses[0].OA_VerifiesContainerGrossWeight = true;

			Shipment1.ConsignorPK = consignor.PK;
			ExportConsol.JK_AgentType = Constants.AgentType.Direct;
			ExportConsol.Shipments.Add(Shipment1);

			var container = Factory.NewWithValidTestData<CommonContainer>();
			ExportConsol.Containers.Add(container);

			AssertEquals(container.JC_GrossWeightVerificationType, Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired);
		}

		#endregion

		#region New Properties

		public void TestJC_IsRefrigerated()
		{
			CommonContainer reefer = Factory.New<CommonContainer>();
			CommonContainer nonReefer = Factory.New<CommonContainer>();
			CommonContainer empty = Factory.New<CommonContainer>();

			RefContainer reeferContainer = Factory.New<RefContainer>();
			reefer.JC_RC = reeferContainer.PK;
			RefContainer nonReeferContainer = Factory.New<RefContainer>();
			nonReefer.JC_RC = nonReeferContainer.PK;

			reefer.Container.RC_ContainerType = Constants.ContainerTypes.Refrigerated;
			nonReefer.Container.RC_ContainerType = Constants.ContainerTypes.DryStorage;

			Assert("Should not be refrigerated", !nonReefer.JC_IsRefrigerated);
			Assert("Should be refrigerated", reefer.JC_IsRefrigerated);
			Assert("Should not be refrigerated", !empty.JC_IsRefrigerated);
		}

		public void TestIsContainerised()
		{
			var container = Factory.New<CommonContainer>();
			AssertEquals(true, container.IsContainerised);

			container.JC_ContainerMode = Constants.ContainerModes.BreakBulk;
			AssertEquals(false, container.IsContainerised);

			container.JC_ContainerMode = Constants.ContainerModes.Bulk;
			AssertEquals(false, container.IsContainerised);

			container.JC_ContainerMode = Constants.ContainerModes.RollOnRollOff;
			AssertEquals(false, container.IsContainerised);

			container.JC_ContainerMode = Constants.ContainerModes.Liquid;
			AssertEquals(false, container.IsContainerised);

			container.JC_ContainerMode = Constants.ContainerModes.FCL;
			AssertEquals(true, container.IsContainerised);

			container.JC_ContainerMode = Constants.ContainerModes.ULD;
			AssertEquals(true, container.IsContainerised);

			container.JC_ContainerMode = Constants.ContainerModes.AIR;
			AssertEquals(true, container.IsContainerised);
		}

		#endregion

		#region TestAddressSplitProperties

		public void TestAddressSplitProperties()
		{
			Factory.AllowMultipleBusinessObjectsAroundOneRow = false;
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG111";
			OrgAddress address11 = org1.Addresses.AddNew();
			address11.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			address11.OA_Code = "Test Comment 11";
			OrgAddress address12 = org1.Addresses.AddNew();
			address12.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			address12.OA_Code = "Test Comment 12";

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG222";
			OrgAddress address21 = org2.Addresses.AddNew();
			address21.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			address21.OA_Code = "Test Comment 21";
			OrgAddress address22 = org2.Addresses.AddNew();
			address22.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			address22.OA_Code = "Test Comment 22";

			ExportConsol.Containers.Add(TestContainer);
			// Departure Pack Address
			TestContainer.JC_Calc_DeparturePackAddressOrg = org1.PK;
			TestContainer.JC_Calc_DeparturePackAddressCode = address11.OA_Code;
			AssertEquals("Address 11", address11.PK, TestContainer.Consol.JK_OA_PackDepotAddress);

			TestContainer.JC_Calc_DeparturePackAddressOrg = org2.PK;
			TestContainer.JC_Calc_DeparturePackAddressCode = address22.OA_Code;
			AssertEquals("Address 22", address22.PK, TestContainer.Consol.JK_OA_PackDepotAddress);
			TestContainer.JC_Calc_DeparturePackAddressCode = address21.OA_Code;
			AssertEquals("Address 21", address21.PK, TestContainer.Consol.JK_OA_PackDepotAddress);

			TestContainer.JC_Calc_DeparturePackAddressOrg = org2.PK;
			TestContainer.JC_Calc_DeparturePackAddressCode = address12.OA_Code;
			AssertEquals("Address Code not in the list - Address should be empty", ZGuid.Empty, TestContainer.Consol.JK_OA_PackDepotAddress);

			// Departure CTO Address
			TestContainer.JC_Calc_DepartureCTOAddressOrg = org1.PK;
			TestContainer.JC_Calc_DepartureCTOAddressCode = address11.OA_Code;
			AssertEquals("Address 11", address11.PK, TestContainer.Consol.JK_OA_DepartureCTOAddress);

			// Departure Container Yard Address
			TestContainer.JC_Calc_DepartureContainerYardAddressOrg = org1.PK;
			TestContainer.JC_Calc_DepartureContainerYardAddressCode = address11.OA_Code;
			AssertEquals("Address 11", address11.PK, TestContainer.JC_OA_DepartureContainerYardAddress);

			ExportConsol.Containers.Remove(TestContainer);
			ImportConsol.Containers.Add(TestContainer);
			// Arrival Unpack Address
			TestContainer.JC_Calc_ArrivalUnpackAddressOrg = org1.PK;
			TestContainer.JC_Calc_ArrivalUnpackAddressCode = address11.OA_Code;
			AssertEquals("Address 11", address11.PK, TestContainer.JC_JK_OA_ArrivalUnpackAddress);

			// Arrival CTO Address
			TestContainer.JC_Calc_ArrivalCTOAddressOrg = org1.PK;
			TestContainer.JC_Calc_ArrivalCTOAddressCode = address11.OA_Code;
			AssertEquals("Address 11", address11.PK, TestContainer.Consol.JK_OA_ArrivalCTOAddress);

			// Arrival Container Yard Address
			TestContainer.JC_Calc_ArrivalContainerYardAddressOrg = org1.PK;
			TestContainer.JC_Calc_ArrivalContainerYardAddressCode = address11.OA_Code;
			AssertEquals("Address 11", address11.PK, TestContainer.JC_OA_ArrivalContainerYardAddress);
		}

		#endregion

		#region Add Remove PackLines

		public void TestAddPackLine_UpdateExistingLineDivot()
		{
			var commonConsol = Factory.New<CommonConsol>();
			commonConsol.Containers.QueryReJoinPackLines += (x, y) => { };

			var container = commonConsol.Containers.AddNew();
			var shipment = commonConsol.Shipments.AddNew();

			AssertEquals("Expecting no packlines in the container.", 0, TestContainer.PackLines.Count);

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 11;

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_PackageCount = 22;

			var pickupConfirm1 = shipment.PickupConfirms.AddNew();
			var pickupConfirm2 = shipment.PickupConfirms.AddNew();

			var deliveryConfirm1 = shipment.DeliveryConfirms.AddNew();
			var deliveryConfirm2 = shipment.DeliveryConfirms.AddNew();

			AssertEquals(4, packLine1.ConfirmDivots.Count);

			container.PackLines.RemoveAll();
			container.AddPackLine(packLine1);
			container.AddPackLine(packLine2);
			container.AddPackLine(packLine3);

			AssertEquals("Expecting one packline to be in the container.", 1, container.PackLines.Count);

			var existingPackLine = container.PackLines[0];
			AssertEquals("Expecting keep the packlien1", packLine1.PK, existingPackLine.PK);

			AssertEquals(4, existingPackLine.ConfirmDivots.Count);

			AssertEquals("Expecting only add the PackageCount on the first pickup divot", 43, pickupConfirm1.GetDivot(existingPackLine).J8_PackagesDelivered);
			AssertEquals("Expecting does not add the PackageCount on other pickup divots", 0, pickupConfirm2.GetDivot(existingPackLine).J8_PackagesDelivered);

			AssertEquals("Expecting only add the PackageCount on the first delivery divot", 43, deliveryConfirm1.GetDivot(existingPackLine).J8_PackagesDelivered);
			AssertEquals("Expecting does not add the PackageCount on other delivery divots", 0, deliveryConfirm2.GetDivot(existingPackLine).J8_PackagesDelivered);
		}

		public void TestMergeMarksAndNumbers()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.Containers.QueryReJoinPackLines += (x, y) => { };
			CommonContainer container = consol.Containers.AddNew();
			CommonShipment shipment = consol.Shipments.AddNew();

			AssertEquals("Expecting no packlines in the container.", 0, TestContainer.PackLines.Count);

			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_MarksAndNumbers = "Marks1";

			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_MarksAndNumbers = "Marks2";

			PackLine packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_MarksAndNumbers = "Marks3";

			container.AddPackLine(packLine1);
			container.AddPackLine(packLine2);
			container.AddPackLine(packLine3);

			AssertEquals("Expecting one packline to be in the container.", 1, container.PackLines.Count);
			AssertEquals("Expecting MarksAndNumbers to merge", "Marks3\r\nMarks2\r\nMarks1", container.PackLines[0].JL_MarksAndNumbers);
		}

		public void TestAddPackLine()
		{
			AssertEquals("Expecting no packlines in the container.", 0, TestContainer.PackLines.Count);

			TestContainer.AddPackLine(Shipment1.OuterPackLines[0]);
			AssertEquals("Expected one packline to be in the container.", 1, TestContainer.PackLines.Count);
			AssertEquals("Expecting packline in the container to be from shipment1.", Shipment1.PK, TestContainer.PackLines[0].JL_JS);
		}

		public void TestRemovePackLine()
		{
			AssertEquals("Expecting no shipments in the container", 0, TestContainer.PackLines.Count);

			TestContainer.AddPackLine(Shipment1.OuterPackLines[0]);
			TestContainer.AddPackLine(Shipment2.OuterPackLines[0]);
			AssertEquals("Expected two packlines to be in the Container.", 2, TestContainer.PackLines.Count);

			TestContainer.RemovePackLine(TestContainer.PackLines[0]);
			AssertEquals("Expected one packline to be in the container.", 1, TestContainer.PackLines.Count);
		}

		public void TestAddPackLines()
		{
			BusinessObject[] packLines = new BusinessObject[2];
			packLines[0] = Shipment1.OuterPackLines[0];
			packLines[1] = Shipment2.OuterPackLines[0];
			TestContainer.AddPackLines(packLines);
			AssertEquals("Expecting two PackLines to be in the container.", 2, TestContainer.PackLines.Count);
		}

		public void TestRemovePackLines()
		{
			AssertEquals("Expecting no PackLines in the container.", 0, TestContainer.PackLines.Count);

			BusinessObject[] packLines = new BusinessObject[3];
			packLines[0] = Shipment1.OuterPackLines[0];
			packLines[1] = Shipment2.OuterPackLines[0];
			packLines[2] = Shipment3.OuterPackLines[0];
			TestContainer.AddPackLines(packLines);
			AssertEquals("Expecting three PackLines to be in the container.", 3, TestContainer.PackLines.Count);

			BusinessObject[] packLinesToBeRemoved = new BusinessObject[2];
			packLinesToBeRemoved[0] = TestContainer.PackLines[0];
			packLinesToBeRemoved[1] = TestContainer.PackLines[1];
			TestContainer.RemovePackLines(packLinesToBeRemoved);
			AssertEquals("Expecting one PackLine in the container.", 1, TestContainer.PackLines.Count);
		}

		public void TestEnsureAddingPackLineIsRemovedFromPreviousContainerOnSameConsol()
		{
			CommonContainer container1 = ExportConsol.Containers.AddNew();
			CommonContainer container2 = ExportConsol.Containers.AddNew();

			AssertEquals("Expected NO packlines to be in the container1.", 0, container1.PackLines.Count);
			AssertEquals("Expected NO packlines to be in the container2.", 0, container2.PackLines.Count);

			ExportConsol.Shipments.Add(Shipment1);
			PackLine packline = Shipment1.OuterPackLines[0];
			container1.AddPackLine(packline);
			AssertEquals("Expected one packline to be in the container1.", 1, container1.PackLines.Count);
			AssertEquals("Expected NO packlines to be in the container2.", 0, container2.PackLines.Count);

			container2.AddPackLine(packline);
			AssertEquals("Expected NO packlines to be in the container1.", 0, container1.PackLines.Count);
			AssertEquals("Expected one packline to be in the container2.", 1, container2.PackLines.Count);
		}

		#endregion

		#region TestTrainWagonNumberReadOnly

		public void TestTrainWagonNumberReadOnly()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			CommonContainer container1 = consol.Containers.AddNew();
			AssertEquals("JC_TrainWagonNumberInfo.ReadOnly", true, container1.JC_TrainWagonNumberInfo.ReadOnly);

			consol.JK_TransportMode = Constants.TransportModes.Rail;
			CommonContainer container2 = consol.Containers.AddNew();
			AssertEquals("JC_TrainWagonNumberInfo.ReadOnly", false, container1.JC_TrainWagonNumberInfo.ReadOnly);
			AssertEquals("JC_TrainWagonNumberInfo.ReadOnly", false, container2.JC_TrainWagonNumberInfo.ReadOnly);

			Factory.Save();

			var loadedContainer = Factory.Load<CommonContainer>(container1.PK);
			AssertEquals("JC_TrainWagonNumberInfo.ReadOnly", false, loadedContainer.JC_TrainWagonNumberInfo.ReadOnly);

			consol.JK_TransportMode = Constants.TransportModes.Air;
			loadedContainer = Factory.Load<CommonContainer>(container2.PK);
			AssertEquals("JC_TrainWagonNumberInfo.ReadOnly", true, loadedContainer.JC_TrainWagonNumberInfo.ReadOnly);
		}

		#endregion

		#region Test Addresses Default From Consol Address

		public void TestJC_OA_DepartureContainerYardAddress()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_OA_ContainerYardEmptyPickupAddress = LocalContainerYard.MainAddress.PK;
			CommonContainer container = Factory.New<CommonContainer>();
			consol.Containers.Add(container);

			AssertEquals("Expecting container's dep yard address to be local container yard", LocalContainerYard.MainAddress.PK, container.JC_OA_DepartureContainerYardAddress);

			container.JC_OA_DepartureContainerYardAddress = OverseasContainerYard.MainAddress.PK;
			AssertEquals("Expecting container's dep yard address to be overseas container yard", OverseasContainerYard.MainAddress.PK, container.JC_OA_DepartureContainerYardAddress);
		}

		public void TestJC_OA_DeparturePackAddress()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_OA_PackDepotAddress = LocalDepot.MainAddress.PK;
			CommonContainer container = Factory.New<CommonContainer>();
			consol.Containers.Add(container);

			AssertEquals("Expecting container's dep depot address to be local depot", LocalDepot.MainAddress.PK, container.Consol.JK_OA_PackDepotAddress);

			container.Consol.JK_OA_PackDepotAddress = OverseasDepot.MainAddress.PK;
			AssertEquals("Expecting container's dep depot address to be overseas depot", OverseasDepot.MainAddress.PK, container.Consol.JK_OA_PackDepotAddress);
		}

		public void TestJC_OA_ArrivalContainerYardAddress()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_OA_ContainerYardEmptyReturnAddress = LocalContainerYard.MainAddress.PK;
			CommonContainer container = Factory.New<CommonContainer>();
			consol.Containers.Add(container);

			AssertEquals("Expecting container's arv yard address to be local container yard", LocalContainerYard.MainAddress.PK, container.JC_OA_ArrivalContainerYardAddress);

			container.JC_OA_ArrivalContainerYardAddress = OverseasContainerYard.MainAddress.PK;
			AssertEquals("Expecting container's arv yard address to be overseas container yard", OverseasContainerYard.MainAddress.PK, container.JC_OA_ArrivalContainerYardAddress);
		}

		public void TestJC_OA_ArrivalUnpackAddress()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_OA_UnpackDepotAddress = LocalDepot.MainAddress.PK;
			CommonContainer container = Factory.New<CommonContainer>();
			consol.Containers.Add(container);

			AssertEquals("Expecting container's arv depot address to be local depot", LocalDepot.MainAddress.PK, container.JC_JK_OA_ArrivalUnpackAddress);

			container.JC_JK_OA_ArrivalUnpackAddress = OverseasDepot.MainAddress.PK;
			AssertEquals("Expecting container's arv depot address to be overseas depot", OverseasDepot.MainAddress.PK, container.JC_JK_OA_ArrivalUnpackAddress);
		}

		#endregion

		#region TestPackLinesRefreshes

		public void TestPackLinesRefreshes()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer consolContainer = consol.Containers.AddNew();
			consolContainer.JC_ContainerNum = "CONT1234567";
			CommonShipment consolShipment = consol.Shipments.AddNew();
			consolShipment.JS_OuterPacks = 1;
			consolShipment.JS_ActualWeight = 100;
			AssertEquals("Packline should be created.", 1, consolShipment.OuterPackLines.Count);
			AssertEquals("Packline should be allocated to container.", 1, consolContainer.PackLines.Count);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CommonShipment shipment = factory2.Load<CommonShipment>(consolShipment.PK);
			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_ActualWeight = 200;
			AssertEquals("CommonShipment should have 2 outer packs.", 2, shipment.OuterPackLines.Count);
			factory2.Save();

			AssertEquals("Container should refresh Packlines", 2, consol.Containers[0].PackLines.Count);
		}

		#endregion

		#region TestSettingGrossWeight

		public void TestSettingGrossWeight()
		{
			CommonContainer container = Factory.New<CommonConsol>().Containers.AddNew();
			container.JC_RC = RC_20GP_PK;
			container.JC_GrossWeightUQ = Constants.Weight.Tonnes;
			container.JC_GrossWeight = 20;
			container.JC_DunnageWeight = 20;

			PackLine packLine = container.Consol.Shipments.AddNew().OuterPackLines.AddNew();
			packLine.SetContainer(container.Consol, container);
			packLine.JL_ActualWeightUQ = Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 10;

			container.JC_TareWeight = 4500;

			AssertEquals("JC_GrossWeight should not be altered", Constants.Weight.Tonnes, container.JC_GrossWeightUQ);
			AssertEquals("Container.JC_GrossWeight", new ZDecimal(4530), container.JC_GrossWeight);
		}

		public void TestIsGrossWeightOverrideAvailable()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.ULD;
			CommonContainer container = consol.Containers.AddNew();
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "LD-3")).PK;
			AssertEquals("not available for base class CommonContainer", false, container.IsGrossWeightOverrideAvailable);
		}

		#endregion

		#region TestSettingWeightsWhileSuspendedFromUniversalShipment

		public void TestSettingWeightIsSuspendedWhenImportingFromUniversalShipment()
		{
			CommonContainer container = Factory.New<CommonConsol>().Containers.AddNew();
			container.JC_RC = RC_20GP_PK;
			container.JC_ContainerCount = 3;
			container.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container.JC_TareWeight = 300;
			container.JC_DunnageWeight = 200;
			container.JC_GrossWeight = 1000;

			using (container.SuppressSettingRelatedWeightsFromUniversalShipment())
			{
				#region Test Changing Container Type

				container.JC_RC = RC_40RE_PK;

				AssertEquals("ContainerType changed correctly", "40RE", container.Container.RC_Code);
				AssertEquals("ContainerCount Remains Unchanged when changing container type", (ZShort)3, container.JC_ContainerCount);
				AssertEquals("TareWeight Remains Unchanged when changing container type", 300m, container.JC_TareWeight);
				AssertEquals("GrossWeight Remains Unchanged when changing container type", 1000m, container.JC_GrossWeight);
				AssertEquals("DunnageWeight Remains Unchanged when changing container type", 200m, container.JC_DunnageWeight);
				AssertEquals("WeightUnit Remains Unchanged when changing container type", "KG", container.JC_GrossWeightUQ);

				#endregion

				#region Test Changing Container Count

				container.JC_ContainerCount = (ZShort)4;

				AssertEquals("ContainerType Remains Unchanged when changing container count", "40RE", container.Container.RC_Code);
				AssertEquals("ContainerCount changed correctly", (ZShort)4, container.JC_ContainerCount);
				AssertEquals("TareWeight Remains Unchanged when changing container count", 300m, container.JC_TareWeight);
				AssertEquals("GrossWeight Remains Unchanged when changing container count", 1000m, container.JC_GrossWeight);
				AssertEquals("DunnageWeight Remains Unchanged when changing container count", 200m, container.JC_DunnageWeight);
				AssertEquals("WeightUnit Remains Unchanged when changing container count", "KG", container.JC_GrossWeightUQ);

				#endregion

				#region Test Changing TareWeight

				container.JC_TareWeight = 350;

				AssertEquals("ContainerType Remains Unchanged when changing Tare Weight", "40RE", container.Container.RC_Code);
				AssertEquals("ContainerCount Remains Unchanged when changing Tare Weight", (ZShort)4, container.JC_ContainerCount);
				AssertEquals("TareWeight changed correctly", 350m, container.JC_TareWeight);
				AssertEquals("GrossWeight Remains Unchanged when changing Tare Weight", 1000m, container.JC_GrossWeight);
				AssertEquals("DunnageWeight Remains Unchanged when changing Tare Weight", 200m, container.JC_DunnageWeight);
				AssertEquals("WeightUnit Remains Unchanged when changing Tare Weight", "KG", container.JC_GrossWeightUQ);

				#endregion

				#region Test Changing GrossWeight

				container.JC_GrossWeight = 1050;

				AssertEquals("ContainerType Remains Unchanged when changing Gross Weight", "40RE", container.Container.RC_Code);
				AssertEquals("ContainerCount Remains Unchanged when changing Gross Weight", (ZShort)4, container.JC_ContainerCount);
				AssertEquals("TareWeight Remains Unchanged when changing Gross Weight", 350m, container.JC_TareWeight);
				AssertEquals("GrossWeight changed correctly", 1050m, container.JC_GrossWeight);
				AssertEquals("DunnageWeight Remains Unchanged when changing Gross Weight", 200m, container.JC_DunnageWeight);
				AssertEquals("WeightUnit Remains Unchanged when changing Gross Weight", "KG", container.JC_GrossWeightUQ);

				#endregion

				#region Test Changing DunnageWeight

				container.JC_DunnageWeight = 250;

				AssertEquals("ContainerType Remains Unchanged when changing Dunnage Weight", "40RE", container.Container.RC_Code);
				AssertEquals("ContainerCount Remains Unchanged when changing Dunnage Weight", (ZShort)4, container.JC_ContainerCount);
				AssertEquals("TareWeight Remains Unchanged when changing Dunnage Weight", 350m, container.JC_TareWeight);
				AssertEquals("GrossWeight Remains Unchanged when changing Dunnage Weight", 1050m, container.JC_GrossWeight);
				AssertEquals("DunnageWeight changed correctly", 250m, container.JC_DunnageWeight);
				AssertEquals("WeightUnit Remains Unchanged when changing Dunnage Weight", "KG", container.JC_GrossWeightUQ);

				#endregion

				#region Test Changing Weight Unit

				container.JC_GrossWeightUQ = Constants.Weight.Tonnes;

				AssertEquals("ContainerType Remains Unchanged when changing Weight Unit", "40RE", container.Container.RC_Code);
				AssertEquals("ContainerCount Remains Unchanged when changing Weight Unit", (ZShort)4, container.JC_ContainerCount);
				AssertEquals("TareWeight Updates when changing Weight Unit", 0.350m, container.JC_TareWeight);
				AssertEquals("GrossWeight Updates when changing Weight Unit", 1.050m, container.JC_GrossWeight);
				AssertEquals("DunnageWeight Updates when changing Weight Unit", 0.250m, container.JC_DunnageWeight);
				AssertEquals("WeightUnit changed correctly", "T", container.JC_GrossWeightUQ);

				#endregion
			}
		}
		#endregion

		#region TestCalculatingTareWeightAndGrossWeightByContainerCountAndDunnageWeightAndPackLines

		public void TestCalculatingTareWeightAndGrossWeightByContainerCountAndDunnageWeightAndPackLines()
		{
			var containerType = Factory.LoadTop1<RefContainer>(new ZQuery());

			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_ContainerCount = 5;
			container.JC_DunnageWeight = 5;

			PackLine packLine = Factory.New<CommonShipment>().OuterPackLines.AddNew();
			packLine.Containers.Add(container);
			packLine.JL_ActualWeightUQ = Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = (ZDecimal)3;

			container.JC_RC = containerType.PK;

			AssertEquals("Tare Weight", containerType.RC_TareWeight * 5, container.JC_TareWeight);
			AssertEquals("Gross Weight", containerType.RC_TareWeight * 5 + 3005, container.JC_GrossWeight);

			container.JC_ContainerCount = 3;

			AssertEquals("Tare Weight", containerType.RC_TareWeight * 3, container.JC_TareWeight);
			AssertEquals("Gross Weight", containerType.RC_TareWeight * 3 + 3005, container.JC_GrossWeight);
		}

		#endregion

		#region Tests that were previously in JobContainerExtra class

		#region TestCartageCompanyDeliveringToCTO

		public void TestCartageCompanyDeliveringToCTO()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			CommonContainer container = consol.Containers.AddNew();
			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);

			OrgHeader shipmentCartageCo = Factory.NewWithValidTestData<OrgHeader>();
			shipmentCartageCo.OH_FullName = "Shipment Cartage Co";

			OrgHeader containerOverrideCartageCo = Factory.New<OrgHeader>();
			containerOverrideCartageCo.OH_FullName = "Container Override Cartage Co";

			ZString firstResult = container.CartageCompanyDeliveringToCTO == null ? ZString.Empty : container.CartageCompanyDeliveringToCTO.OH_FullName;
			AssertEquals("Should be no cartage company yet", ZString.Empty, firstResult);

			shipment.DocsAndCartage.PickupCartageCoPK = shipmentCartageCo.PK;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			AssertEquals("Should get cartage company from the CommonShipment even when LCL", shipmentCartageCo.OH_FullName, container.CartageCompanyDeliveringToCTO.OH_FullName);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AssertEquals("Should get cartage company from the CommonShipment - FCL", shipmentCartageCo.OH_FullName, container.CartageCompanyDeliveringToCTO.OH_FullName);
		}

		#endregion

		#region TestWeightAdjustment

		public void TestWeightAdjustment()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CommonContainer container = Factory.New<CommonConsol>().Containers.AddNew();

			AssertWeights("precondition: ", 0m, 0m, 0m, 0m, container);

			container.JC_TareWeight = 2500m;
			AssertWeights("Tare weight set to 2500", 2500m, 0m, 0m, 2500m, container);

			container.JC_Calc_NetWeight = 8000m;
			AssertWeights("Net weight set to 8000", 10500m, 8000m, 0m, 2500m, container);

			container.JC_DunnageWeight = 100m;
			AssertWeights("Dunnage weight set to 100 (Dunnage count towards gross *and* net)", 10600m, 8100m, 100m, 2500m, container);

			container.JC_GrossWeight = 12000m;
			AssertWeights("Gross weight set to 12000", 12000m, 9500m, 100m, 2500m, container);

			container.JC_TareWeight = 2550;
			AssertWeights("Tare weight increased to 2550", 12050m, 9500m, 100m, 2550m, container);

			container.JC_Calc_NetWeight = 6000m;
			AssertWeights("Net weight decreased to 6000", 8550m, 6000m, 100m, 2550m, container);

			container.JC_GrossWeightUQ = Constants.Weight.Tonnes;
			AssertWeights("Changing the unit of weight should update the weights.", 8550m, 6000m, 100m, 2550m, container);

			PackLine packLine = shipment.OuterPackLines.AddNew();
			container.Consol.Shipments.Add(shipment);
			packLine.SetContainer(container.Consol, container);
			packLine.JL_ActualWeight = 5000m;
			AssertWeights("Changing a packline should update the net/gross weights", 2655m, 105m, 100m, 2550m, container);

			Factory.Load<RefContainer>(RC_20GP_PK).RC_TareWeight = 2550;
			container.JC_RC = RC_20GP_PK;
			AssertWeights("Setting the container updates the tare weight.", 107.55m, 105m, 100m, 2.55m, container);

			Factory.Save();

			container.JC_GrossWeight = 200m;
			AssertWeights("GrossWeight set to 200", 200m, 197.45m, 100m, 2.55m, container);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CommonContainer container2 = factory2.Load<CommonContainer>(container.PK);
			AssertWeights("Loaded in another factory", 107.55m, 105m, 100m, 2.55m, container2);

			Factory.Save();
			AssertWeights("Net weight should be data refresh safe", 200m, 197.45m, 100m, 2.55m, container2);
		}

		void AssertWeights(string message, ZDecimal grossWeight, ZDecimal netWeight, ZDecimal dunnageWeight, ZDecimal tareWeight, CommonContainer container)
		{
			AssertEquals(message + " JC_DunnageWeight", dunnageWeight, container.JC_DunnageWeight);
			AssertEquals(message + " JC_GrossWeight", grossWeight, container.JC_GrossWeight);
			AssertEquals(message + " JC_Calc_NetWeight", netWeight, container.JC_Calc_NetWeight);
			AssertEquals(message + " JC_TareWeight", tareWeight, container.JC_TareWeight);
		}

		#endregion

		#region TestJC_Calc_NetWeight

		public void TestJC_Calc_NetWeight()
		{
			CommonContainer container = Factory.New<CommonConsol>().Containers.AddNew();
			PackLine packline = container.Consol.Shipments.AddNew().OuterPackLines.AddNew();
			packline.SetContainer(container.Consol, container);
			packline.JL_ActualWeight = 44m;

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container.JC_RC = refContainer.PK;
			AssertEquals("Ref Container set, 44 - 0", 44m, container.JC_Calc_NetWeight);

			container.JC_TareWeight = 55m;
			container.JC_DunnageWeight = 10m;
			AssertEquals("Tareweight manually set, so gross(44 + 55 + 10) - tare(55) = 54", 54m, container.JC_Calc_NetWeight);
		}

		#endregion

		#region TestJC_Calc_NetWeightSetter

		public void TestJC_Calc_NetWeightSetter()
		{
			var container = Factory.New<CommonContainer>();

			container.JC_TareWeight = 20m;
			container.JC_Calc_NetWeight = 15m;
			AssertEquals("GrossWeight Should be Updated", 35m, container.JC_GrossWeight);
			AssertEquals("TareWeight Should not be Updated", 20m, container.JC_TareWeight);
		}

		#endregion

		#region TestJC_Calc_NetWeightInfo

		public void TestJC_Calc_NetWeightInfo()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			AssertEquals("JC_Calc_NetWeight", container.JC_Calc_NetWeightInfo.Name);
			AssertEquals("Should not be ReadOnly", false, container.JC_Calc_NetWeightInfo.ReadOnly);
		}

		#endregion

		#region TestEffectiveGrossWeight

		public void TestEffectiveGrossWeight()
		{
			CommonContainer container = Factory.New<CommonConsol>().Containers.AddNew();
			container.JC_TareWeight = 1000m;
			AssertEquals("EffectiveGrossWeight only with TareWeight", 1000m, container.EffectiveGrossWeightInternal());

			PackLine packLine = Factory.New<CommonShipment>().OuterPackLines.AddNew();
			container.Consol.Shipments.Add(packLine.Shipment);
			packLine.SetContainer(container.Consol, container);
			packLine.JL_ActualWeight = 200m;
			packLine.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			container.JC_GrossWeightUQ = ZString.Empty;
			AssertEquals("PackLine Actual Weight is Set but no UQ is specified yet", 1200m, container.EffectiveGrossWeightInternal());
			container.JC_GrossWeightUQ = Constants.Weight.Tonnes;
			AssertEquals("A particular UQ is specified", 1000.2m, container.EffectiveGrossWeightInternal());
		}

		#endregion

		#region TestValidateJC_HumidityPercent

		public void TestValidateJC_HumidityPercent()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_RC = RC_20RE_PK;

			container.JC_HumidityPercent = 0;
			Assert("Valid percent - No error expected", !container.JC_HumidityPercentInfo.HasErrors());

			container.JC_HumidityPercent = 50;
			Assert("Valid percent - No error expected", !container.JC_HumidityPercentInfo.HasErrors());

			container.JC_HumidityPercent = 100;
			Assert("Valid percent - No error expected", !container.JC_HumidityPercentInfo.HasErrors());

			container.JC_HumidityPercent = 150;
			Assert("Invalid percent - Error expected", container.JC_HumidityPercentInfo.HasErrors());
		}

		#endregion

		#region TestChillerAndFrozen

		public void TestChillerAndFrozen()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_IsControlledAtmosphere = true;
			AssertCalculatedReeferFields(container, "HFIASNV", false, true);
			AssertCalculatedReeferFields(container, "23 C", true, false);
			AssertCalculatedReeferFields(container, "4 C", true, false);
			AssertCalculatedReeferFields(container, "-5 C", false, true);
			AssertCalculatedReeferFields(container, "YUIOSH", false, true);
			AssertCalculatedReeferFields(container, "C398", false, true);
			AssertCalculatedReeferFields(container, "5C", true, false);
			AssertCalculatedReeferFields(container, "3", true, false);
			AssertCalculatedReeferFields(container, "-40", false, true);
			AssertCalculatedReeferFields(container, "0", false, true);
		}

		void AssertCalculatedReeferFields(CommonContainer container, ZString setPoint, bool isChiller, bool isFreezer)
		{
			decimal setPointValue = 0m;
			try
			{
				setPointValue = decimal.Parse(setPoint.TrimEnd('C', ' '));
			}
			catch
			{
				setPointValue = 0m;
			}
			container.JC_SetPointTemp = setPointValue;
			AssertEquals(setPoint + " : Chiller ? ", isChiller, container.IsChiller);
			AssertEquals(setPoint + " : Freezer ? ", isFreezer, container.IsFreezer);
		}

		#endregion

		#region TestIsChillerCanBeSet

		public void TestIsChillerCanBeSet()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.IsChiller = true;
			AssertEquals(true, container.IsChiller);
			container.IsChiller = false;
			AssertEquals(false, container.IsChiller);
		}

		#endregion

		#region TestReeferDefaults

		public void TestReeferDefaults()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			AssertEquals("Default Set Point Initial Value", 0.0m, container.JC_SetPointTemp);
			AssertEquals("Default Set Point Unit", string.Empty, container.JC_SetPointTempUnit);

			container.IsChiller = true;

			AssertEquals("Default Set Point - Chiller", 5.0m, container.JC_SetPointTemp);
			AssertEquals("Default Set Point Unit - Chiller", Constants.Temperature.Centigrade, container.JC_SetPointTempUnit);

			container.IsFreezer = true;

			AssertEquals("Default Set Point - Freezer", -5.0m, container.JC_SetPointTemp);
			AssertEquals("Default Set Point Unit - Chiller", Constants.Temperature.Centigrade, container.JC_SetPointTempUnit);

			container.JC_SetPointTempUnit = Constants.Temperature.Fahrenheit;
			container.JC_SetPointTemp = 1.0m;

			AssertEquals(1.0m, container.JC_SetPointTemp);
			AssertEquals(Constants.Temperature.Fahrenheit, container.JC_SetPointTempUnit);
		}

		#endregion

		#region TestJC_IsControlledAtmosphereNonReefer

		public void TestJC_IsControlledAtmosphereNonReefer()
		{
			CommonContainer container1NonReefer = Factory.New<CommonContainer>();
			container1NonReefer.JC_RC = RC_20GP_PK;
			AssertEquals("By Default IsControlled Atmosphere depends upon the parent container being a reefer - 20GP", false, container1NonReefer.JC_IsControlledAtmosphere);
			container1NonReefer.JC_IsControlledAtmosphere = true;
			AssertEquals("Any type of container can claim to have a controlled atmosphere", true, container1NonReefer.JC_IsControlledAtmosphere);
		}

		#endregion

		#region TestJC_IsControlledAtmosphereChiller

		public void TestJC_IsControlledAtmosphereChiller()
		{
			CommonContainer chillerContainer = Factory.New<CommonContainer>();
			chillerContainer.JC_RC = RC_20RE_PK;
			AssertEquals("By Default IsControlled Atmosphere depends upon the parent container being a reefer - 20RE", true, chillerContainer.JC_IsControlledAtmosphere);
			chillerContainer.JC_IsControlledAtmosphere = false;
			AssertEquals("Just because it is a reefer does not mean that the controlled atmosphere cant be turned off", false, chillerContainer.JC_IsControlledAtmosphere);
			chillerContainer.IsChiller = true;
			AssertEquals("Setting is Chiller should set the controlled atmosphere", true, chillerContainer.JC_IsControlledAtmosphere);
			chillerContainer.JC_IsControlledAtmosphere = false;
			AssertEquals("Temperature Setting Defaulted back to Zero", 0.0m, chillerContainer.JC_SetPointTemp);
			chillerContainer.JC_IsControlledAtmosphere = true;
			AssertEquals("Temperature Setting at 0.0m", 0.0m, chillerContainer.JC_SetPointTemp);
			AssertEquals("Temperature Setting is still controlled atmosphere", true, chillerContainer.JC_IsControlledAtmosphere);
			chillerContainer.JC_IsControlledAtmosphere = false;

			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonContainer loadedContainer = newFactory.Load<CommonContainer>(chillerContainer.PK);
			AssertEquals("IsControlledAtmosphere should be false when loaded", false, loadedContainer.JC_IsControlledAtmosphere);
		}

		#endregion

		#region TestJC_IsControlledAtmosphereFreezer

		public void TestJC_IsControlledAtmosphereFreezer()
		{
			CommonContainer freezerContainer = Factory.New<CommonContainer>();
			freezerContainer.JC_RC = RC_20RE_PK;
			AssertEquals("By Default IsControlled Atmosphere depends upon the parent container being a reefer - 20RE", true, freezerContainer.JC_IsControlledAtmosphere);
			freezerContainer.JC_IsControlledAtmosphere = false;
			AssertEquals("Did not turn off the controlled atmosphere flag", false, freezerContainer.JC_IsControlledAtmosphere);
			freezerContainer.IsFreezer = true;
			AssertEquals("Setting the freezer tick box should turn on the controlled atmosphere", true, freezerContainer.JC_IsControlledAtmosphere);
			freezerContainer.JC_IsControlledAtmosphere = false;
			freezerContainer.JC_SetPointTemp = -3.4m;
			AssertEquals("Setting the temperature of the container should force the Is Controlled atmosphere on", true, freezerContainer.JC_IsControlledAtmosphere);
		}

		#endregion

		#region TestJC_OA_ArrivalContainerYardAddressFromExtra

		public void TestJC_OA_ArrivalContainerYardAddressFromExtra()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			AssertEquals("Expecting JC_OA_ArrivalContainerYardAddress to be empty.", ZGuid.Empty, container.JC_OA_ArrivalContainerYardAddress);

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_OA_ContainerYardEmptyReturnAddress = LocalContainerYard.MainAddress.PK;
			consol.Containers.Add(container);

			AssertEquals("Expecting JC_OA_ArrivalContainerYardAddress to show info from consol.", consol.JK_OA_ContainerYardEmptyReturnAddress, container.JC_OA_ArrivalContainerYardAddress);
		}

		#endregion

		#region TestJC_OA_DepartureContainerYardAddressFromExtra

		public void TestJC_OA_DepartureContainerYardAddressFromExtra()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			AssertEquals("Expecting JC_OA_DepartureContainerYardAddress to be empty.", ZGuid.Empty, container.JC_OA_DepartureContainerYardAddress);

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_OA_ContainerYardEmptyPickupAddress = LocalContainerYard.MainAddress.PK;
			consol.Containers.Add(container);

			AssertEquals("Expecting JC_OA_DepartureContainerYardAddress to show info from consol.", consol.JK_OA_ContainerYardEmptyPickupAddress, container.JC_OA_DepartureContainerYardAddress);
		}

		#endregion

		#region TestJC_TareWeight

		public void TestJC_TareWeight()
		{
			CommonContainer container = Factory.New<CommonConsol>().Containers.AddNew();
			var shipment = container.Consol.Shipments.AddNew();
			PackLine packline = shipment.OuterPackLines.AddNew();
			packline.SetContainer(container.Consol, container);
			packline.JL_ActualWeight = 44m;

			container.JC_TareWeight = 55m;
			container.JC_DunnageWeight = 66m;
			AssertEquals("Should be set, TareWeight should be 55", 55m, container.JC_TareWeight);
			AssertEquals("Should be set, Total Gross should be 155", 165m, container.JC_GrossWeight);

			container.JC_TareWeight = 1m;
			AssertEquals("Should be set, TareWeight should be 1", 1m, container.JC_TareWeight);
			AssertEquals("Should be set, Total Gross should be 111", 111m, container.JC_GrossWeight);
		}

		#endregion

		#region TestJC_DunnageWeight

		public void TestJC_DunnageWeight()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			CommonContainer container = Factory.New<CommonConsol>().Containers.AddNew();
			container.Consol.Shipments.Add(shipment);
			PackLine packline = shipment.OuterPackLines.AddNew();
			packline.SetContainer(container.Consol, container);
			packline.JL_ActualWeight = 44m;
			container.JC_TareWeight = 55m;
			container.JC_DunnageWeight = 66m;
			AssertEquals("Should be set, Dunnage should be 66", 66m, container.JC_DunnageWeight);
			AssertEquals("Should be set, Total Gross should be 165", 165m, container.JC_GrossWeight);

			container.JC_DunnageWeight = 1m;
			AssertEquals("Should be set, Dunnage should be 1", 1m, container.JC_DunnageWeight);
			AssertEquals("Should be set, Total Gross should be 100", 100m, container.JC_GrossWeight);
		}

		#endregion

		#region TestSetGrossWeightFromCombinedWeights

		public void TestSetGrossWeightFromCombinedWeights()
		{
			CommonContainer container = Factory.New<CommonConsol>().Containers.AddNew();
			CommonShipment shipment = container.Consol.Shipments.AddNew();

			PackLine packline = shipment.OuterPackLines.AddNew();
			packline.SetContainer(container.Consol, container);
			packline.JL_ActualWeight = 44m;
			container.JC_TareWeight = 55m;
			container.JC_DunnageWeight = 66m;
			AssertEquals("Should be set, Total Gross should be 165", 165m, container.JC_GrossWeight);
		}

		public void TestJC_Calc_TotalWeight()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container = consol.Containers.AddNew();
			CommonShipment shipment1 = consol.Shipments.AddNew();
			CommonShipment shipment2 = consol.Shipments.AddNew();
			CommonShipment shipment3 = consol.Shipments.AddNew();
			CommonShipment shipment4 = consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			shipment3.JS_JS_ColoadMasterShipment = shipment2.PK;
			shipment4.JS_JS_ColoadMasterShipment = shipment2.PK;

			PackLine packline1 = shipment1.OuterPackLines.AddNew();
			PackLine packline2 = shipment2.OuterPackLines.AddNew();
			PackLine packline3 = shipment3.OuterPackLines.AddNew();
			PackLine packline4 = shipment4.OuterPackLines.AddNew();

			packline1.JL_ActualWeight = 1000;
			packline2.JL_ActualWeight = 5000;
			packline3.JL_ActualWeight = 1110;
			packline4.JL_ActualWeight = 3889;

			container.JC_TareWeight = 10m;
			container.JC_DunnageWeight = 20m;
			AssertEquals("Should be set, Total Gross should be 5999", 5999m, container.JC_Calc_TotalWeight);
			AssertEquals("Should be set, Total Gross should be 6029", 6029m, container.JC_GrossWeight);
		}
		#endregion

		#region

		#region Set Confirm Time

		public void TestJC_DepartureCartageCompleteSetsConfirm()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			shipment.JS_RL_NKDestination = OverseasPort;
			shipment.JS_RL_NKOrigin = HomePort;

			CommonConsol consol = shipment.Consols.AddNew();
			shipment.Consols.Add(consol);

			CommonContainer container = consol.Containers.AddNew();
			PackLine pl = shipment.OuterPackLines.AddNew();
			container.PackLines.Add(pl);

			container.JC_DepartureCartageComplete = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.Empty, container.OriginConfirm.EU_PickupDeliveryTime);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			container.JC_DepartureCartageComplete = ZDateTime.BrettsBirthday.AddDays(1);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), container.OriginConfirm.EU_PickupDeliveryTime);
		}

		public void TestJC_DepartureCartageCompleteGetOriginConfirm()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			shipment.JS_RL_NKDestination = OverseasPort;
			shipment.JS_RL_NKOrigin = HomePort;

			CommonConsol consol = shipment.Consols.AddNew();
			shipment.Consols.Add(consol);

			CommonContainer container = consol.Containers.AddNew();
			PackLine pl = shipment.OuterPackLines.AddNew();
			container.PackLines.Add(pl);
			container.JC_DepartureCartageComplete = ZDateTime.BrettsBirthday.AddDays(1);

			var originConfirm = container.OriginConfirm;
			originConfirm.Delete();
			Factory.Save();

			AssertNull(container.OriginGetConfirm);
			AssertNotNull(container.OriginConfirm);
		}

		public void TestJC_ArrivalCartageCompleteGetDestinationConfirm()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			shipment.JS_RL_NKDestination = OverseasPort;
			shipment.JS_RL_NKOrigin = HomePort;

			CommonConsol consol = shipment.Consols.AddNew();
			shipment.Consols.Add(consol);

			CommonContainer container = consol.Containers.AddNew();
			PackLine pl = shipment.OuterPackLines.AddNew();
			container.PackLines.Add(pl);
			container.JC_ArrivalCartageComplete = ZDateTime.BrettsBirthday.AddDays(1);

			var destinationConfirm = container.DestinationConfirm;
			destinationConfirm.Delete();
			Factory.Save();

			AssertNull(container.DestinationGetConfirm);
			AssertNotNull(container.DestinationConfirm);
		}

		public void TestJC_ArrivalCartageCompleteSetsConfirm()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			shipment.JS_RL_NKDestination = OverseasPort;
			shipment.JS_RL_NKOrigin = HomePort;

			CommonConsol consol = shipment.Consols.AddNew();
			shipment.Consols.Add(consol);

			CommonContainer container = consol.Containers.AddNew();
			PackLine pl = shipment.OuterPackLines.AddNew();
			container.PackLines.Add(pl);

			container.JC_ArrivalCartageComplete = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.Empty, container.OriginConfirm.EU_PickupDeliveryTime);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			container.JC_ArrivalCartageComplete = ZDateTime.BrettsBirthday.AddDays(1);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), container.DestinationConfirm.EU_PickupDeliveryTime);
		}

		public void TestJC_ArrivalEstimatedDeliverySetsConfirm()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			shipment.JS_RL_NKDestination = OverseasPort;
			shipment.JS_RL_NKOrigin = HomePort;

			CommonConsol consol = shipment.Consols.AddNew();
			shipment.Consols.Add(consol);

			CommonContainer container = consol.Containers.AddNew();
			PackLine pl = shipment.OuterPackLines.AddNew();
			container.PackLines.Add(pl);

			container.JC_ArrivalEstimatedDelivery = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.Empty, container.DestinationConfirm.EU_PlannedPickupDeliveryTime);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			container.JC_ArrivalEstimatedDelivery = ZDateTime.BrettsBirthday.AddDays(1);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), container.DestinationConfirm.EU_PlannedPickupDeliveryTime);
		}

		public void TestJC_DepartureEstimatedPickupSetsConfirm()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			shipment.JS_RL_NKDestination = OverseasPort;
			shipment.JS_RL_NKOrigin = HomePort;

			CommonConsol consol = shipment.Consols.AddNew();
			shipment.Consols.Add(consol);

			CommonContainer container = consol.Containers.AddNew();
			PackLine pl = shipment.OuterPackLines.AddNew();
			container.PackLines.Add(pl);

			container.JC_DepartureEstimatedPickup = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.Empty, container.OriginConfirm.EU_PlannedPickupDeliveryTime);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			container.JC_DepartureEstimatedPickup = ZDateTime.BrettsBirthday.AddDays(1);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), container.OriginConfirm.EU_PlannedPickupDeliveryTime);
		}

		#endregion

		#endregion

		#region TestPropagateDepartureEstimatedPickupToShipmentIfContainerIsOnDepartureConsol

		public void TestPropagateDepartureEstimatedPickupToShipmentIfContainerIsOnDepartureConsol()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_RL_NKDestination = OverseasPort;
			shipment.JS_RL_NKOrigin = HomePort;
			CommonConsol arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_TransportMode = Constants.TransportModes.Sea;
			arrivalConsol.JK_UniqueConsignRef = "ArrivalConsol";

			Transport arrivalTransport = arrivalConsol.Transports[0];
			arrivalTransport.JW_RL_NKDiscPort = HomePort;
			arrivalTransport.JW_RL_NKLoadPort = OverseasPort;

			CommonConsol departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_TransportMode = Constants.TransportModes.Sea;
			departureConsol.JK_UniqueConsignRef = "DepartureConsol";

			Transport departureTransport = departureConsol.Transports[0];
			departureTransport.JW_RL_NKDiscPort = OverseasPort;
			departureTransport.JW_RL_NKLoadPort = HomePort;

			CommonContainer container1 = arrivalConsol.Containers.AddNew();
			CommonContainer container2 = departureConsol.Containers.AddNew();

			PackLine pack1 = shipment.OuterPackLines.AddNew();
			container1.PackLines.Add(pack1);
			container2.PackLines.Add(pack1);

			ZDateTime testDate = ZDateTime.Now;
			container1.JC_DepartureEstimatedPickup = testDate;
			AssertEquals("Should not have propagated the date to shipment.", ZDateTime.Empty, shipment.DocsAndCartage.JP_EstimatedPickup);

			container2.JC_DepartureEstimatedPickup = testDate;
			AssertEquals("Should not have propagated the date to shipment.", ZDateTime.Empty, shipment.DocsAndCartage.JP_EstimatedPickup);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			container2.JC_DepartureEstimatedPickup = ZDateTime.Empty;
			container2.JC_DepartureEstimatedPickup = testDate;
			Factory.Save();
			AssertEquals("Should have propagated the date to shipment.", testDate, shipment.DocsAndCartage.JP_EstimatedPickup);

			container2.JC_DepartureEstimatedPickup = testDate.AddDays(1);
			shipment.DocsAndCartage.JP_EstimatedPickup = ZDateTime.Empty;
			Factory.Save();
			AssertEquals("Should have propagated the date to shipment.", testDate.AddDays(1), shipment.DocsAndCartage.JP_EstimatedPickup);
		}

		#endregion

		#region TestIsEffectiveGrossWeightValid

		public void TestIsEffectiveGrossWeightValid()
		{
			CommonContainer container = Factory.New<CommonConsol>().Containers.AddNew();
			PackLine line = container.Consol.Shipments.AddNew().OuterPackLines.AddNew();

			container.PackLines.Add(line);

			line.JL_ActualWeight = 10;
			AssertEquals(true, container.IsEffectiveGrossWeightValid);

			line.JL_ActualWeight = 1000000;
			AssertEquals(false, container.IsEffectiveGrossWeightValid);
		}

		#endregion

		#endregion

		#region TestJC_JA_NKPortOfLoading

		public void TestJC_JA_NKPortOfLoading()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			AssertEquals("Container has no Consol or Sailing, JC_JA_NKPortOfLoading should be empty.", "", container.JC_JA_NKPortOfLoading);

			JobSailing sailing = Factory.New<JobSailing>();
			VoyageOrigin originVoyage = Factory.New<VoyageOrigin>();
			originVoyage.JA_RL_NKPortOfLoading = "AUSYD";
			sailing.JX_JA = originVoyage.PK;
			container.JC_JX = sailing.PK;
			AssertEquals("Container has Sailing but no Consol, JC_JA_NKPortOfLoading should come from the Sailing.", "AUSYD", container.JC_JA_NKPortOfLoading);

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			container.JC_JK = consol.PK;
			AssertEquals("Container has Sailing and Consol, JC_JA_NKPortOfLoading should come from the Consol.", "AUMEL", container.JC_JA_NKPortOfLoading);

			container.JC_JX = ZGuid.Empty;
			AssertEquals("Container has Consol but no Sailing, JC_JA_NKPortOfLoading should come from the Consol.", "AUMEL", container.JC_JA_NKPortOfLoading);
		}

		#endregion

		#region TestJC_JB_NKPortOfDischarge

		public void TestJC_JB_NKPortOfDischarge()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			AssertEquals("Container has no Consol or Sailing, JC_JB_NKPortOfDischarge should be empty.", "", container.JC_JB_NKPortOfDischarge);

			JobSailing sailing = Factory.New<JobSailing>();
			VoyageDestination destVoyage = Factory.New<VoyageDestination>();
			destVoyage.JB_RL_NKPortOfDischarge = "AUSYD";
			sailing.JX_JB = destVoyage.PK;
			container.JC_JX = sailing.PK;
			AssertEquals("Container has Sailing but no Consol, JC_JB_NKPortOfDischarge should come from the Sailing.", "AUSYD", container.JC_JB_NKPortOfDischarge);

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKDischargePort = "AUMEL";
			container.JC_JK = consol.PK;
			AssertEquals("Container has Sailing and Consol, JC_JB_NKPortOfDischarge should come from the Consol.", "AUMEL", container.JC_JB_NKPortOfDischarge);

			container.JC_JX = ZGuid.Empty;
			AssertEquals("Container has Consol but no Sailing, JC_JB_NKPortOfDischarge should come from the Consol.", "AUMEL", container.JC_JB_NKPortOfDischarge);
		}

		#endregion

		#region TestJC_DeliveryMode_List

		public void TestJC_DeliveryMode_List()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			CommonConsol consol = Factory.New<CommonConsol>();
			container.JC_JK = consol.PK;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Consol IsAir - JC_DeliveryMode_List should be empty.", 0, container.JC_DeliveryMode_List.Count);

			consol.JK_TransportMode = Constants.TransportModes.Rail;
			AssertEquals("Should be of valid type", typeof(CodeDescriptionPairList), container.JC_DeliveryMode_List.GetType());
			AssertEquals("Should contain correct count of records", FreightDataRegistry.Instance.ContainerDeliveryModeList.Value.Count, container.JC_DeliveryMode_List.Count);
			Assert("Should contain an item from registry list", container.JC_DeliveryMode_List.ContainsCode(FreightDataRegistry.Instance.ContainerDeliveryModeList.Value[0].Code));
		}

		#endregion

		#region Available / Storage

		#region Container on Stand Alone Declaration

		public void TestJC_FCLAvailable_OnStandAloneDeclaration()
		{
			TestAvailableOrStorage_OnStandAloneDeclaration(JobConsolTransportSchema.JW_TerminalAvailabilityDate, JobContainerSchema.JC_FCLAvailable);
		}

		public void TestJC_ArrivalCTOStorageStartDate_OnStandAloneDeclaration()
		{
			TestAvailableOrStorage_OnStandAloneDeclaration(JobConsolTransportSchema.JW_TerminalStorageDate, JobContainerSchema.JC_ArrivalCTOStorageStartDate);
		}

		public void TestJC_LCLAvailable_OnStandAloneDeclaration()
		{
			TestAvailableOrStorage_OnStandAloneDeclaration(JobConsolTransportSchema.JW_DepotAvailabilityDate, JobContainerSchema.JC_LCLAvailable);
		}

		public void TestJC_LCLStorageCommences_OnStandAloneDeclaration()
		{
			TestAvailableOrStorage_OnStandAloneDeclaration(JobConsolTransportSchema.JW_DepotStorageDate, JobContainerSchema.JC_LCLStorageCommences);
		}

		void TestAvailableOrStorage_OnStandAloneDeclaration(SchemaDateTimeColumn transportDate, SchemaDateTimeColumn containerDate)
		{
			JobSailing arrivalSailing = SeaVoyage.Sailings[3];
			AssertEquals("Arrival sailing correct", true, arrivalSailing.Destination.JB_RL_NKPortOfDischarge.StartsWith(GlbBranch.CurrentBranch.Country.Code));

			Declaration[JobDeclarationSchema.JE_TransportMode] = SeaVoyage.JV_AirSeaRoad;
			Declaration[JobDeclarationSchema.JE_VesselName] = SeaVoyage.JV_RV_NKVessel;
			Declaration[JobDeclarationSchema.JE_VoyageFlightNo] = SeaVoyage.JV_VoyageFlight;
			Declaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = SeaVoyage.Sailings[2].Destination.JB_RL_NKPortOfDischarge;

			var transportParent = (ITransportParent)Declaration;
			var transport2 = transportParent.Transports.AddNew();
			transport2.JW_IsLinked = true;
			transport2.JW_TransportMode = "SEA";
			transport2.JW_Vessel = SeaVoyage.JV_RV_NKVessel;
			transport2.JW_VoyageFlight = SeaVoyage.JV_VoyageFlight;
			transport2.JW_RL_NKLoadPort = arrivalSailing.Origin.JA_RL_NKPortOfLoading;
			transport2.JW_RL_NKDiscPort = arrivalSailing.Destination.JB_RL_NKPortOfDischarge;
			transport2.JW_LegOrder = 2;

			CommonContainer declarationContainer = (CommonContainer)DeclarationCusContainers.AddNew()["JobContainer"];

			ZDateTime newValue = new ZDateTime(2010, 1, 2);

			if (transportDate.TableName == JobConsolTransportSchema.Constants.TableName)
			{
				transport2[transportDate] = newValue;
			}
			else
			{
				throw new InvalidOperationException("unknown table: " + transportDate.TableName);
			}

			AssertEquals("Correct " + containerDate.Name + " date", newValue, declarationContainer[containerDate]);
		}

		#endregion

		#region Container on Consol

		public void TestJC_FCLAvailable_OnConsol()
		{
			TestAvailableOrStorage_OnConsol(JobConsolTransportSchema.JW_TerminalAvailabilityDate, JobContainerSchema.JC_FCLAvailable);
		}

		public void TestJC_ArrivalCTOStorageStartDate_OnConsol()
		{
			TestAvailableOrStorage_OnConsol(JobConsolTransportSchema.JW_TerminalStorageDate, JobContainerSchema.JC_ArrivalCTOStorageStartDate);
		}

		public void TestJC_LCLAvailable_OnConsol()
		{
			TestAvailableOrStorage_OnConsol(JobConsolTransportSchema.JW_DepotAvailabilityDate, JobContainerSchema.JC_LCLAvailable);
		}

		public void TestJC_LCLStorageCommences_OnConsol()
		{
			TestAvailableOrStorage_OnConsol(JobConsolTransportSchema.JW_DepotStorageDate, JobContainerSchema.JC_LCLStorageCommences);
		}

		void TestAvailableOrStorage_OnConsol(SchemaDateTimeColumn transportDate, SchemaDateTimeColumn containerDate)
		{
			JobSailing arrivalSailing = SeaVoyage.Sailings[3];
			arrivalSailing.JX_IsPublished = true;
			AssertEquals("Arrival sailing correct", true, arrivalSailing.Destination.JB_RL_NKPortOfDischarge.StartsWith(GlbBranch.CurrentBranch.Country.Code));

			Consol.Transports[0].JW_JX = arrivalSailing.PK;
			CommonContainer consolContainer = Consol.Containers.AddNew();

			ZDateTime newValue = new ZDateTime(2010, 1, 2);

			if (transportDate.TableName == JobConsolTransportSchema.Constants.TableName)
			{
				Consol.Transports[0][transportDate] = newValue;
			}
			else
			{
				throw new InvalidOperationException("unknown table: " + transportDate.TableName);
			}

			AssertEquals("Correct " + containerDate.Name + " date", newValue, consolContainer[containerDate]);
		}

		#endregion

		#region Overriding

		public void TestJC_FCLAvailable_Override()
		{
			TestAvailableOrStorageOverride(CommonContainer.Schema.JC_FCLAvailable, JobContainerSchema.JC_OverrideFCLAvailableStorage, JobDocsAndCartageSchema.JP_FCLAvailable);
		}

		public void TestJC_FCLAvailable_Override_WhenConsolIsNotArrivalConsol()
		{
			Shipment1.JS_PackingMode = Constants.ContainerModes.FCL;
			Shipment1.JS_RL_NKOrigin = "AUSYD";
			Shipment1.JS_RL_NKDestination = "CNSHG";

			var consol1 = Shipment1.Consols.AddNew();
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol1.JK_RL_NKDischargePort = "CNQDG";
			var container1 = consol1.Containers.AddNew();

			var consol2 = Shipment1.Consols.AddNew();
			consol2.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol2.JK_RL_NKDischargePort = "CNSHG";
			var container2 = consol2.Containers.AddNew();

			Assert(Shipment1.ArrivalConsol != consol1);
			Assert(Shipment1.ArrivalConsol == consol2);

			container1.JC_OverrideFCLAvailableStorage = true;
			container1.JC_FCLAvailable = new ZDateTime(2000, 1, 1);

			Factory.Save();

			AssertEquals(new ZDateTime(2000, 1, 1), container1.JC_FCLAvailable);
			AssertNotEquals(new ZDateTime(2000, 1, 1), Shipment1.DocsAndCartage.JP_FCLAvailable);
			Assert(!Shipment1.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.CargoAvailableCode && x.SL_Reference.Contains("CNSHG")));

			container2.JC_OverrideFCLAvailableStorage = true;
			container2.JC_FCLAvailable = new ZDateTime(2000, 2, 1);

			Factory.Save();

			AssertEquals(new ZDateTime(2000, 2, 1), container2.JC_FCLAvailable);
			AssertEquals(new ZDateTime(2000, 2, 1), Shipment1.DocsAndCartage.JP_FCLAvailable);
			Assert(Shipment1.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.CargoAvailableCode && x.SL_Reference.Contains("CNSHG")));
		}

		public void TestJC_ArrivalCTOStorageStartDate_Override()
		{
			TestAvailableOrStorageOverride(CommonContainer.Schema.JC_ArrivalCTOStorageStartDate, JobContainerSchema.JC_OverrideFCLAvailableStorage, JobDocsAndCartageSchema.JP_FCLStorageCommences);
		}

		public void TestJC_LCLAvailable_Override()
		{
			TestAvailableOrStorageOverride(JobContainerSchema.JC_LCLAvailable, JobContainerSchema.JC_OverrideLCLAvailableStorage, JobDocsAndCartageSchema.JP_LCLAvailable);
		}

		public void TestJC_LCLAvailable_Override_WhenConsolIsNotArrivalConsol()
		{
			Shipment1.JS_PackingMode = Constants.ContainerModes.LCL;
			Shipment1.JS_RL_NKOrigin = "AUSYD";
			Shipment1.JS_RL_NKDestination = "CNSHG";

			var consol1 = Shipment1.Consols.AddNew();
			consol1.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol1.JK_RL_NKDischargePort = "CNQDG";
			var container1 = consol1.Containers.AddNew();

			var consol2 = Shipment1.Consols.AddNew();
			consol2.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol2.JK_RL_NKDischargePort = "CNSHG";
			var container2 = consol2.Containers.AddNew();

			Assert(Shipment1.ArrivalConsol != consol1);
			Assert(Shipment1.ArrivalConsol == consol2);

			container1.JC_OverrideLCLAvailableStorage = true;
			container1.JC_LCLAvailable = new ZDateTime(2000, 1, 1);

			Factory.Save();

			AssertEquals(new ZDateTime(2000, 1, 1), container1.JC_LCLAvailable);
			AssertNotEquals(new ZDateTime(2000, 1, 1), Shipment1.DocsAndCartage.JP_LCLAvailable);
			Assert(!Shipment1.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.CargoAvailableCode && x.SL_Reference.Contains("CNSHG")));

			container2.JC_OverrideLCLAvailableStorage = true;
			container2.JC_LCLAvailable = new ZDateTime(2000, 2, 1);

			Factory.Save();

			AssertEquals(new ZDateTime(2000, 2, 1), container2.JC_LCLAvailable);
			AssertEquals(new ZDateTime(2000, 2, 1), Shipment1.DocsAndCartage.JP_LCLAvailable);
			Assert(Shipment1.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.CargoAvailableCode && x.SL_Reference.Contains("CNSHG")));
		}

		public void TestJC_LCLStorageCommences_Override()
		{
			TestAvailableOrStorageOverride(JobContainerSchema.JC_LCLStorageCommences, JobContainerSchema.JC_OverrideLCLAvailableStorage, JobDocsAndCartageSchema.JP_LCLStorageCommences);
		}

		void TestAvailableOrStorageOverride(SchemaDateTimeColumn containerDateProperty, SchemaBoolColumn containerDateOverrideProperty, SchemaDateTimeColumn docsAndCartageDateProperty)
		{
			TestAvailableOrStorageOverride(containerDateProperty.Name, containerDateOverrideProperty, docsAndCartageDateProperty);
		}
		void TestAvailableOrStorageOverride(string containerDatePropertyName, SchemaBoolColumn containerDateOverrideProperty, SchemaDateTimeColumn docsAndCartageDateProperty)
		{
			CommonContainer container = Consol.Containers.AddNew();
			Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			Shipment1.JS_PackingMode = Constants.ContainerModes.FCL;
			Consol.Shipments.Add(Shipment1);

			AssertEquals(false, (ZBool)container[containerDateOverrideProperty]);
			container[containerDatePropertyName] = new ZDateTime(2000, 1, 1);
			AssertEquals(containerDateOverrideProperty.Name + " automatically set to true when " + containerDatePropertyName + " set to a valid date", true, (ZBool)container[containerDateOverrideProperty]);
			AssertEquals(containerDatePropertyName + " returns overridden date when " + containerDateOverrideProperty.Name + " true", new ZDateTime(2000, 1, 1), container[containerDatePropertyName]);
			AssertEquals(docsAndCartageDateProperty.Name + " returns overridden date when " + containerDateOverrideProperty.Name + " true", new ZDateTime(2000, 1, 1), Shipment1.DocsAndCartage[docsAndCartageDateProperty]);

			container[containerDateOverrideProperty] = false;
			AssertEquals(containerDatePropertyName + " emptied when override is unticked", ZDateTime.Empty, (ZDateTime)container[containerDatePropertyName]);
			AssertEquals(docsAndCartageDateProperty.Name + " emptied when override is unticked", ZDateTime.Empty, (ZDateTime)Shipment1.DocsAndCartage[docsAndCartageDateProperty]);
		}

		public void TestFCLAvailableSetOnContainerWithMultipleShipmentsOnlyUpdatesShipmentPackedInContainer()
		{
			Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			Shipment1.JS_PackingMode = Constants.ContainerModes.FCL;
			Shipment2.JS_PackingMode = Constants.ContainerModes.FCL;

			CommonContainer container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "Container1";
			CommonContainer container2 = Consol.Containers.AddNew();
			container2.JC_ContainerNum = "Container2";

			Consol.Shipments.Add(Shipment1);
			Consol.Shipments.Add(Shipment2);
			Shipment1.UnpackFromContainer(container1);
			Shipment1.UnpackFromContainer(container2);
			Shipment2.UnpackFromContainer(container1);
			Shipment2.UnpackFromContainer(container2);
			PackLine packLine = Shipment1.OuterPackLines.AddNew();
			packLine.JL_Calc_ContainerNumber = "Container1";

			container1.JC_FCLAvailable = ZDateTime.Now;
			AssertEquals("Only shipments that are packed into the container should be updated", true, Shipment1.DocsAndCartage.JP_FCLAvailable.IsValid);
			AssertEquals("Only shipments that are packed into the container should be updated", false, Shipment2.DocsAndCartage.JP_FCLAvailable.IsValid);
		}

		#endregion

		#endregion

		#region Import Release

		public void TestImportReleaseStrings()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				AssertEquals("E-IDO Status", CommonContainer.ImportReleaseOrderStatusStringData.Caption);
				AssertEquals("Carrier/DO Release", CommonContainer.ImportReleaseNumberStringData.Caption);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.NewZealand))
			{
				AssertEquals("Import Release Order Status", CommonContainer.ImportReleaseOrderStatusStringData.Caption);
				AssertEquals("Import Release Number", CommonContainer.ImportReleaseNumberStringData.Caption);
			}
		}

		#endregion

		#region Event Properties

		public void TestJC_FCLOnBoardVessel_IsEventDateProperty()
		{
			AssertEquals(TestContainer.JC_FCLOnBoardVesselInfo.Name, EventDatePropertyAttribute.FindPropertyInfos(TestContainer, Events.FreightLoaded, EstimateActual.Actual).FirstOrDefault().Property.Name);
		}

		public void TestJC_FCLUnloadFromVessel_IsEventDateProperty()
		{
			AssertEquals(TestContainer.JC_FCLUnloadFromVesselInfo.Name, EventDatePropertyAttribute.FindPropertyInfos(TestContainer, Events.FreightUnloaded, EstimateActual.Actual).FirstOrDefault().Property.Name);
		}

		public void TestJC_EmptyReadyForReturn_IsEventDateProperty()
		{
			AssertEquals(TestContainer.JC_EmptyReadyForReturnInfo.Name, EventDatePropertyAttribute.FindPropertyInfos(TestContainer, Events.ContainerReadyForEmptyReturn, EstimateActual.Actual).FirstOrDefault().Property.Name);
		}

		public void TestJC_FCLOnBoardVessel_EventIsCreated_EventParameters()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_FCLOnBoardVessel = ZDateTime.Now;

			var eventLog = container.Logs.MostRecentLogByEventTime(Events.FreightLoaded);
			AssertEquals(false, eventLog.SL_IsEstimate);
			AssertEquals("Facility", "|FAC=CTO", eventLog.SL_Reference);

			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			container = consol.Containers.AddNew();
			container.JC_FCLOnBoardVessel = ZDateTime.Now;

			eventLog = container.Logs.MostRecentLogByEventTime(Events.FreightLoaded);
			AssertEquals(false, eventLog.SL_IsEstimate);
			AssertEquals("Location should be consol's first load port", "|FAC=CTO|LOC=AUSYD", eventLog.SL_Reference);
		}

		public void TestJC_FCLOnBoardVessel_InboundEvent_ShouldMatchLocation()
		{
			var eventTime = ZDateTimeOffset.Now;

			var container = Factory.New<CommonContainer>();

			container.Logs.AddNew(Events.FreightLoaded, "|LOC=USLAX", eventTime);
			AssertEquals("Location not matched, date not updated", ZDateTime.Empty, container.JC_FCLOnBoardVessel);

			container.Logs.AddNew(Events.FreightLoaded, "", eventTime);
			AssertEquals("Location matched - both are empty, date updated", eventTime.ToZDateTime(), container.JC_FCLOnBoardVessel);

			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			container = consol.Containers.AddNew();

			container.Logs.AddNew(Events.FreightLoaded, "|LOC=NZAKL", eventTime);
			AssertEquals("Location not matched, date not updated", ZDateTime.Empty, container.JC_FCLOnBoardVessel);

			container.Logs.AddNew(Events.FreightLoaded, "", eventTime);
			AssertEquals("Location not matched, date not updated", ZDateTime.Empty, container.JC_FCLOnBoardVessel);

			container.Logs.AddNew(Events.FreightLoaded, "|LOC=AUSYD", eventTime);
			AssertEquals("Location matched, date updated", eventTime.ToZDateTime(), container.JC_FCLOnBoardVessel);
		}

		public void TestJC_FCLOnBoardVessel_InboundEvent_ShouldMatchFacility()
		{
			var eventTime = ZDateTimeOffset.Now;

			var container = Factory.New<CommonContainer>();

			container.Logs.AddNew(Events.FreightLoaded, "|FAC=Office", eventTime);
			AssertEquals("Facility not matched, date not updated", ZDateTime.Empty, container.JC_FCLOnBoardVessel);

			container.Logs.AddNew(Events.FreightLoaded, "|FAC=CTO", eventTime);
			AssertEquals("Facility matched, date updated", eventTime.ToZDateTime(), container.JC_FCLOnBoardVessel);
		}

		public void TestJC_FCLUnloadFromVessel_EventIsCreated_EventParameters()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_FCLUnloadFromVessel = ZDateTime.Now;

			var eventLog = container.Logs.MostRecentLogByEventTime(Events.FreightUnloaded);
			AssertEquals(false, eventLog.SL_IsEstimate);
			AssertEquals("Facility", "|FAC=CTO", eventLog.SL_Reference);

			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			container = consol.Containers.AddNew();
			container.JC_FCLUnloadFromVessel = ZDateTime.Now;

			eventLog = container.Logs.MostRecentLogByEventTime(Events.FreightUnloaded);
			AssertEquals(false, eventLog.SL_IsEstimate);
			AssertEquals("Location should be consol's last discharge port", "|FAC=CTO|LOC=NZAKL", eventLog.SL_Reference);
		}

		public void TestJC_FCLUnloadFromVessel_InboundEvent_ShouldMatchLocation()
		{
			var eventTime = ZDateTimeOffset.Now;

			var container = Factory.New<CommonContainer>();

			container.Logs.AddNew(Events.FreightUnloaded, "|LOC=USLAX", eventTime);
			AssertEquals("Location not matched, date not updated", ZDateTime.Empty, container.JC_FCLUnloadFromVessel);

			container.Logs.AddNew(Events.FreightUnloaded, "", eventTime);
			AssertEquals("Location matched - both are empty, date updated", eventTime.ToZDateTime(), container.JC_FCLUnloadFromVessel);

			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			container = consol.Containers.AddNew();

			container.Logs.AddNew(Events.FreightUnloaded, "|LOC=AUSYD", eventTime);
			AssertEquals("Location not matched, date not updated", ZDateTime.Empty, container.JC_FCLUnloadFromVessel);

			container.Logs.AddNew(Events.FreightUnloaded, "", eventTime);
			AssertEquals("Location not matched, date not updated", ZDateTime.Empty, container.JC_FCLUnloadFromVessel);

			container.Logs.AddNew(Events.FreightUnloaded, "|LOC=NZAKL", eventTime);
			AssertEquals("Location matched, date updated", eventTime.ToZDateTime(), container.JC_FCLUnloadFromVessel);
		}

		public void TestJC_FCLUnloadFromVessel_InboundEvent_ShouldMatchFacility()
		{
			var eventTime = ZDateTimeOffset.Now;

			var container = Factory.New<CommonContainer>();

			container.Logs.AddNew(Events.FreightUnloaded, "|FAC=Office", eventTime);
			AssertEquals("Facility not matched, date not updated", ZDateTime.Empty, container.JC_FCLUnloadFromVessel);

			container.Logs.AddNew(Events.FreightUnloaded, "|FAC=CTO", eventTime);
			AssertEquals("Facility matched, date updated", eventTime.ToZDateTime(), container.JC_FCLUnloadFromVessel);
		}

		#endregion

		#region Properties

		public void TestJC_EmptyReturnedBy()
		{
			var consol = Factory.New<IForwardingConsol>() as CommonConsol;
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.Transports[0].JW_ETDForBinding = new ZDateTime(2021, 1, 1);
			consol.Transports[0].JW_ETAForBinding = new ZDateTime(2021, 1, 3);
			consol.Transports[0].JW_TerminalAvailabilityDateForBinding = new ZDateTime(2022, 1, 5);
			var container = Factory.New<IForwardingContainer>() as CommonContainer;
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 0, UnlimitedFreeDays = true }))
			{
				container.JC_ContainerNum = "CC10000";
				consol.Containers.Add(container);

				Factory.Save();

				AssertEquals(0, container.ImportPenalties.Count(p => p.CPY_PenaltyType == Constants.ContainerPenaltyPenaltyType.Codes.Detention));
			}

			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10, UnlimitedFreeDays = false }))
			using (Env.CurrentUser.BatchProcessorOverride = new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				var factoryOther = new BusinessObjectFactory();
				consol = factoryOther.Load<CommonConsol>(consol.PK);
				consol.JK_MasterBillNum = consol.Containers[0].JC_ContainerNum;
				Factory.Save();

				AssertEquals(0, container.ImportPenalties.Count(p => p.CPY_PenaltyType == Constants.ContainerPenaltyPenaltyType.Codes.Detention));
			}
		}

		public void TestJC_ContainerJobID()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			AssertEquals("precondition", true, container.JC_ContainerJobID.IsEmpty);
			Factory.Save();
			AssertEquals("should be set in OnSaving()", false, container.JC_ContainerJobID.IsEmpty);
		}

		public void TestJC_ContainerJobID_OnSaved()
		{
			var container1 = Factory.NewWithValidTestData<CommonContainer>();
			container1.JC_ContainerJobID = "D00020001";
			Factory.Save();

			AssertEquals(true, container1.IsInDatabase);

			container1.OnSaved(false);
			AssertEquals("D00020001", container1.JC_ContainerJobID);
			container1.OnSaved(true);
			AssertEquals("D00020001", container1.JC_ContainerJobID);

			var container2 = Factory.NewWithValidTestData<CommonContainer>();
			container2.JC_ContainerJobID = "D00020002";

			AssertEquals(false, container2.IsInDatabase);

			container2.OnSaved(true);
			AssertEquals("D00020002", container2.JC_ContainerJobID);

			container2.OnSaved(false);
			AssertEquals(ZString.Empty, container2.JC_ContainerJobID);
		}

		public void TestCustomsContainersRelatedProperties()
		{
			AssertEquals(false, TestContainer.CreatedFromCusContainer);
			AssertEquals(false, TestContainer.StandAloneCustomsContainer);

			TestContainer.CreatedFromCusContainer = true;
			TestContainer.StandAloneCustomsContainer = true;
			AssertEquals(true, TestContainer.CreatedFromCusContainer);
			AssertEquals(true, TestContainer.StandAloneCustomsContainer);
		}

		public void TestAdditionalReferenceNumbersAsString()
		{
			var container = Factory.New<CommonContainer>();

			var cusNumber1 = container.AdditionalReferenceNumbers.AddNew();
			cusNumber1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			cusNumber1.CE_RN_NKCountryCode = "";
			cusNumber1.CE_EntryNum = "XXXABILL1";
			AssertEquals("AMS: XXXABILL1", container.AdditionalReferenceNumbers.AllNumbersAsString); //change this to the property introduced when doing the fetch strategy

			cusNumber1.CE_RN_NKCountryCode = "AU";
			AssertEquals("AMS: XXXABILL1/AU", container.AdditionalReferenceNumbers.AllNumbersAsString);

			var cusNumber2 = container.AdditionalReferenceNumbers.AddNew();
			cusNumber2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			cusNumber2.CE_EntryNum = "CV00001";
			cusNumber2.CE_RN_NKCountryCode = "CN";
			AssertEquals("AMS: XXXABILL1/AU, BKG: CV00001/CN", container.AdditionalReferenceNumbers.AllNumbersAsString);
		}

		#endregion

		#region IDocManagerSupport

		public void TestDocManagerImplemented()
		{
			IDocManagerSupport supporter = TestContainer;
			AssertNotNull("Container class should implement IDocManagerSupport", supporter);
		}

		#endregion

		#region INoteSource

		public void TestNoteSourceName()
		{
			TestContainer.JC_ContainerNum = "ContainerNumber";
			AssertEquals("Container: CONTAINERNUMBER", TestContainer.NoteSourceName);
		}

		#endregion

		#region ICreditControlledDocumentDelivery

		public void TestDescriptionOfOrganisationBeingCheckedForCredit()
		{
			var container = Factory.New<CommonContainer>();
			var creditOnHoldDocument = (ICreditControlledDocumentDelivery)container;
			AssertContains("Sending Agent, Receiving Agent of the associated Consol or Consignee, Consignor, Local Client or any Debtors in the linked Shipment", creditOnHoldDocument.DescriptionOfOrganisationBeingCheckedForCredit);
		}

		public void TestOrganisationsForCreditChecks()
		{
			var containerWithLinkedShipment = Factory.New<CommonContainer>();
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(0, ((ICreditControlledDocumentDelivery)containerWithLinkedShipment).OrganisationsForCreditChecks.Length);
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);

			var consignor = Factory.LoadTop1<OrgHeader>(FreightTestHelper.ConsigneeOrConsignorFilter(OrgHeaderSchema.OH_IsConsignor));
			var consignee = Factory.LoadTop1<OrgHeader>(FreightTestHelper.ConsigneeOrConsignorFilter(OrgHeaderSchema.OH_IsConsignee, consignor.PK));
			var shipment = Factory.New<CommonShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_UniqueConsignRef = "shipment3";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			containerWithLinkedShipment.LinkedShipment = shipment;
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(2, ((ICreditControlledDocumentDelivery)containerWithLinkedShipment).OrganisationsForCreditChecks.Length);
			Assert(((ICreditControlledDocumentDelivery)containerWithLinkedShipment).OrganisationsForCreditChecks.Contains(consignee));
			Assert(((ICreditControlledDocumentDelivery)containerWithLinkedShipment).OrganisationsForCreditChecks.Contains(consignor));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);

			var containerWithConsol = Factory.New<CommonContainer>();
			Factory.Save();

			var consignor2 = Factory.LoadTop1<OrgHeader>(FreightTestHelper.ConsigneeOrConsignorFilter(OrgHeaderSchema.OH_IsConsignor).AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, consignor.PK));
			var consignee2 = Factory.LoadTop1<OrgHeader>(FreightTestHelper.ConsigneeOrConsignorFilter(OrgHeaderSchema.OH_IsConsignee, consignor2.PK).AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, consignee.PK));
			AssertNotEquals("Precondition: consignor", consignor, consignor2);
			AssertNotEquals("Precondition: consignee", consignee, consignee2);
			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.AWBMaster;
			consol.JK_OA_SendingForwarderAddress = consignor2.Addresses[0].PK;
			consol.JK_OA_ReceivingForwarderAddress = consignee2.Addresses[0].PK;
			containerWithConsol.JC_JK = consol.PK;
			consol.Shipments.Add(shipment);

			var packline = shipment.OuterPackLines.AddNew();
			packline.Containers.Add(containerWithConsol);

			Factory.Save();

			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(4, ((ICreditControlledDocumentDelivery)containerWithConsol).OrganisationsForCreditChecks.Length);
			Assert(((ICreditControlledDocumentDelivery)containerWithConsol).OrganisationsForCreditChecks.Contains(consignee2));
			Assert(((ICreditControlledDocumentDelivery)containerWithConsol).OrganisationsForCreditChecks.Contains(consignor2));
			Assert(((ICreditControlledDocumentDelivery)containerWithConsol).OrganisationsForCreditChecks.Contains(consignee));
			Assert(((ICreditControlledDocumentDelivery)containerWithConsol).OrganisationsForCreditChecks.Contains(consignor));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);
		}

		#endregion

		#region IJobNumberForWorkflow

		public void TestJobNumberForWorkflow()
		{
			var container = Factory.New<DummyContainerWithJobNumber>();
			container.JC_ContainerNum = "TEST1231231";
			container.JC_ContainerCount = 3;

			var milestone = container.WorkflowItems.Milestones.AddNew();

			AssertEquals("TEST1231231", ((IJobNumberForWorkflow)container).JobNumber);
			AssertEquals("TEST1231231", milestone.JobNumber);

			container.JC_ContainerNum = ZString.Empty;

			AssertEquals("JOB1234", ((IJobNumberForWorkflow)container).JobNumber);
			AssertEquals("JOB1234", milestone.JobNumber);

			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			AssertEquals("20GP (3)", ((IJobNumberForWorkflow)container).JobNumber);
			AssertEquals("20GP (3)", milestone.JobNumber);
		}

		class DummyContainerWithJobNumber : CommonContainer, IJobNumber
		{
			public DummyContainerWithJobNumber(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			string IJobNumber.JobNumber
			{
				get { return "JOB1234"; }
			}
		}

		#endregion

		#region Container Spot Rates

		public void TestContainerSpotRatesDefaultValues()
		{
			var container = Factory.NewWithValidTestData<CommonContainer>();

			AssertEquals(0m, container.JC_SellSpotRate);
			AssertEquals(ZString.Empty, container.JC_RX_NKSellSpotRateCurrency);
			AssertEquals(Constants.FreightRateAutoratingModes.Code.StandardRate, container.JC_SellSpotRateMode);

			AssertEquals(0m, container.JC_CostSpotRate);
			AssertEquals(ZString.Empty, container.JC_RX_NKCostSpotRateCurrency);
			AssertEquals(Constants.FreightRateAutoratingModes.Code.StandardRate, container.JC_CostSpotRateMode);

			AssertEquals(0m, container.JC_GatewaySellSpotRate);
			AssertEquals(ZString.Empty, container.JC_RX_NKGatewaySellSpotRateCurrency);
			AssertEquals(Constants.FreightRateAutoratingModes.Code.StandardRate, container.JC_GatewaySellSpotRateMode);
		}

		public void TestStandardRateAutoratingModeDefaultsToZeroContainerSpotRate()
		{
			var container = Factory.NewWithValidTestData<CommonContainer>();

			container.JC_SellSpotRate = 120.2570m;
			container.JC_SellSpotRateMode = Constants.FreightRateAutoratingModes.Code.AllInRate;
			container.JC_CostSpotRate = 100.2600m;
			container.JC_CostSpotRateMode = Constants.FreightRateAutoratingModes.Code.FreightPlusRate;
			container.JC_GatewaySellSpotRate = 69.1500m;
			container.JC_GatewaySellSpotRateMode = Constants.FreightRateAutoratingModes.Code.FreightPlusRate;

			container.JC_SellSpotRateMode = Constants.FreightRateAutoratingModes.Code.StandardRate;
			container.JC_CostSpotRateMode = Constants.FreightRateAutoratingModes.Code.StandardRate;
			container.JC_GatewaySellSpotRateMode = Constants.FreightRateAutoratingModes.Code.StandardRate;

			CombineAssertions("When a StandardRate autorating mode is entered, rate should default to zero", () =>
				{
					AssertEquals(ZDecimal.Zero, container.JC_SellSpotRate);
					AssertEquals(ZDecimal.Zero, container.JC_CostSpotRate);
					AssertEquals(ZDecimal.Zero, container.JC_GatewaySellSpotRate);
				});
		}

		public void TestNonZeroContainerSpotRateDefaultsToFreightPlusRateAutoratingMode()
		{
			var container = Factory.NewWithValidTestData<CommonContainer>();

			CombineAssertions("Pre-condition: by default, freight spot rates are set to zero, autorating modes are set to StandardRate", () =>
			{
				AssertEquals(ZDecimal.Zero, container.JC_SellSpotRate);
				AssertEquals(ZDecimal.Zero, container.JC_CostSpotRate);
				AssertEquals(ZDecimal.Zero, container.JC_GatewaySellSpotRate);

				AssertEquals(Constants.FreightRateAutoratingModes.Code.StandardRate, container.JC_SellSpotRateMode);
				AssertEquals(Constants.FreightRateAutoratingModes.Code.StandardRate, container.JC_CostSpotRateMode);
				AssertEquals(Constants.FreightRateAutoratingModes.Code.StandardRate, container.JC_GatewaySellSpotRateMode);
			});

			container.JC_SellSpotRate = 120.2570m;
			container.JC_CostSpotRate = 100.2600m;
			container.JC_GatewaySellSpotRate = 69.1500m;

			CombineAssertions("When a non-zero rate is entered, autorating mode should default from StandardRate to FreightPlusRate", () =>
			{
				AssertEquals(Constants.FreightRateAutoratingModes.Code.FreightPlusRate, container.JC_SellSpotRateMode);
				AssertEquals(Constants.FreightRateAutoratingModes.Code.FreightPlusRate, container.JC_CostSpotRateMode);
				AssertEquals(Constants.FreightRateAutoratingModes.Code.FreightPlusRate, container.JC_GatewaySellSpotRateMode);
			});

			container.JC_SellSpotRateMode = string.Empty;
			container.JC_CostSpotRateMode = string.Empty;
			container.JC_GatewaySellSpotRateMode = string.Empty;

			container.JC_SellSpotRate = 121.2570m;
			container.JC_CostSpotRate = 101.2600m;
			container.JC_GatewaySellSpotRate = 70.1500m;

			CombineAssertions("When a non-zero rate is entered, autorating mode should default from an empty value to FreightPlusRate", () =>
			{
				AssertEquals(Constants.FreightRateAutoratingModes.Code.FreightPlusRate, container.JC_SellSpotRateMode);
				AssertEquals(Constants.FreightRateAutoratingModes.Code.FreightPlusRate, container.JC_CostSpotRateMode);
				AssertEquals(Constants.FreightRateAutoratingModes.Code.FreightPlusRate, container.JC_GatewaySellSpotRateMode);
			});

			container.JC_SellSpotRateMode = Constants.FreightRateAutoratingModes.Code.AllInRate;
			container.JC_CostSpotRateMode = Constants.FreightRateAutoratingModes.Code.AllInRate;
			container.JC_GatewaySellSpotRateMode = Constants.FreightRateAutoratingModes.Code.AllInRate;

			container.JC_SellSpotRate = 122.2570m;
			container.JC_CostSpotRate = 102.2600m;
			container.JC_GatewaySellSpotRate = 71.1500m;

			CombineAssertions("Autorating mode should not re-default from non-empty, non-StardardRate", () =>
			{
				AssertEquals(Constants.FreightRateAutoratingModes.Code.AllInRate, container.JC_SellSpotRateMode);
				AssertEquals(Constants.FreightRateAutoratingModes.Code.AllInRate, container.JC_CostSpotRateMode);
				AssertEquals(Constants.FreightRateAutoratingModes.Code.AllInRate, container.JC_GatewaySellSpotRateMode);
			});
		}

		public void TestContainerSpotRatesCurrencyDefaultsBasedOnPackingMode()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var container = consol.Containers.AddNew();

			container.JC_SellSpotRateMode = Constants.FreightRateAutoratingModes.Code.AllInRate;
			container.JC_CostSpotRateMode = Constants.FreightRateAutoratingModes.Code.FreightPlusRate;
			container.JC_GatewaySellSpotRateMode = Constants.FreightRateAutoratingModes.Code.FreightPlusRate;

			CombineAssertions("Spot rate currency should be blank by default", () =>
				{
					AssertEquals(ZString.Empty, container.JC_RX_NKSellSpotRateCurrency);
					AssertEquals(ZString.Empty, container.JC_RX_NKCostSpotRateCurrency);
					AssertEquals(ZString.Empty, container.JC_RX_NKGatewaySellSpotRateCurrency);
				});

			ResetCurrencyToZeroAndSetRates(container);

			CombineAssertions("Spot rate currency should be USD for LCL/FCL Sea shipments", () =>
				{
					AssertEquals(Constants.CurrencyCodes.UnitedStates, container.JC_RX_NKSellSpotRateCurrency);
					AssertEquals(Constants.CurrencyCodes.UnitedStates, container.JC_RX_NKCostSpotRateCurrency);
					AssertEquals(Constants.CurrencyCodes.UnitedStates, container.JC_RX_NKGatewaySellSpotRateCurrency);
				});

			consol.JK_ConsolMode = Constants.ContainerModes.Liquid;
			ResetCurrencyToZeroAndSetRates(container);

			CombineAssertions("Spot rate currency should be blank for NON-LCL/FCL Sea shipments", () =>
				{
					AssertEquals(ZString.Empty, container.JC_RX_NKSellSpotRateCurrency);
					AssertEquals(ZString.Empty, container.JC_RX_NKCostSpotRateCurrency);
					AssertEquals(ZString.Empty, container.JC_RX_NKGatewaySellSpotRateCurrency);
				});

			consol.JK_ConsolMode = Constants.ContainerModes.Groupage;
			ResetCurrencyToZeroAndSetRates(container);

			CombineAssertions("Spot rate currency should be USD for GRP Sea shipments", () =>
				{
					AssertEquals(Constants.CurrencyCodes.UnitedStates, container.JC_RX_NKSellSpotRateCurrency);
					AssertEquals(Constants.CurrencyCodes.UnitedStates, container.JC_RX_NKCostSpotRateCurrency);
					AssertEquals(Constants.CurrencyCodes.UnitedStates, container.JC_RX_NKGatewaySellSpotRateCurrency);
				});

			consol.JK_ConsolMode = Constants.ContainerModes.Other;
			ResetCurrencyToZeroAndSetRates(container);

			CombineAssertions("Spot rate currency should be USD for OTH Sea shipments", () =>
				{
					AssertEquals(Constants.CurrencyCodes.UnitedStates, container.JC_RX_NKSellSpotRateCurrency);
					AssertEquals(Constants.CurrencyCodes.UnitedStates, container.JC_RX_NKCostSpotRateCurrency);
					AssertEquals(Constants.CurrencyCodes.UnitedStates, container.JC_RX_NKGatewaySellSpotRateCurrency);
				});

			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "CNDLC";
			ResetCurrencyToZeroAndSetRates(container);

			CombineAssertions("Spot rate currency should be local currency of Origin for Air shipments", () =>
				{
					AssertEquals(Constants.CurrencyCodes.China, container.JC_RX_NKSellSpotRateCurrency);
					AssertEquals(Constants.CurrencyCodes.China, container.JC_RX_NKCostSpotRateCurrency);
					AssertEquals(Constants.CurrencyCodes.China, container.JC_RX_NKGatewaySellSpotRateCurrency);
				});

			consol.JK_RL_NKLoadPort = "";
			ResetCurrencyToZeroAndSetRates(container);

			CombineAssertions("Spot rate currency should fallback to local currency of current company, Origin is blank for Air shipments", () =>
				{
					AssertEquals(Constants.CurrencyCodes.Australia, container.JC_RX_NKSellSpotRateCurrency);
					AssertEquals(Constants.CurrencyCodes.Australia, container.JC_RX_NKCostSpotRateCurrency);
					AssertEquals(Constants.CurrencyCodes.Australia, container.JC_RX_NKGatewaySellSpotRateCurrency);
				});

			consol.JK_TransportMode = Constants.TransportModes.Road;
			consol.JK_RL_NKLoadPort = "CNDLC";
			consol.JK_RL_NKDischargePort = "NZAKL";
			ResetCurrencyToZeroAndSetRates(container);

			CombineAssertions("Spot rate currency should be local currency of Destination for NON-Air, NON-Sea shipments", () =>
				{
					AssertEquals(Constants.CurrencyCodes.NewZealand, container.JC_RX_NKSellSpotRateCurrency);
					AssertEquals(Constants.CurrencyCodes.NewZealand, container.JC_RX_NKCostSpotRateCurrency);
					AssertEquals(Constants.CurrencyCodes.NewZealand, container.JC_RX_NKGatewaySellSpotRateCurrency);
				});

			consol.JK_RL_NKDischargePort = "";
			ResetCurrencyToZeroAndSetRates(container);

			CombineAssertions("Spot rate currency should fallback to local currency of current company, if Destination is blank for NON-Air, NON-Sea shipments", () =>
				{
					AssertEquals(Constants.CurrencyCodes.Australia, container.JC_RX_NKSellSpotRateCurrency);
					AssertEquals(Constants.CurrencyCodes.Australia, container.JC_RX_NKCostSpotRateCurrency);
					AssertEquals(Constants.CurrencyCodes.Australia, container.JC_RX_NKGatewaySellSpotRateCurrency);
				});
		}

		void ResetCurrencyToZeroAndSetRates(CommonContainer container)
		{
			container.JC_RX_NKSellSpotRateCurrency = ZString.Empty;
			container.JC_RX_NKCostSpotRateCurrency = ZString.Empty;
			container.JC_RX_NKGatewaySellSpotRateCurrency = ZString.Empty;

			container.JC_SellSpotRate += 10.2500m;
			container.JC_CostSpotRate += 25.2600m;
			container.JC_GatewaySellSpotRate += 23.1400m;
		}

		#endregion

		#region IRoutingSupport

		public void TestIRoutingSupport()
		{
			var container1 = Factory.NewWithValidTestData<CommonContainer>();
			var routingProvider1 = container1 as IRoutingSupport;
			AssertNotNull(routingProvider1);

			AssertNull(routingProvider1.TransportsIncludingRelated);
			AssertNull(routingProvider1.Transports);
			AssertEquals(ZString.Empty, routingProvider1.TransportMode);
			AssertNull(routingProvider1.AdditionalETAUpdateMsg);
			AssertNull(routingProvider1.AdditionalETDUpdateMsg);

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "AUMEL";
			var transport1 = consol.Transports[0];
			transport1.JW_Vessel = "Vessel1";
			transport1.JW_VoyageFlight = "23";
			transport1.JW_ETD = new ZDateTime(2008, 1, 1);
			transport1.JW_ETA = new ZDateTime(2008, 1, 15);
			transport1.JW_RL_NKDiscPort = "AUSYD";
			consol.Containers.Add(container1);

			AssertEquals(1, routingProvider1.TransportsIncludingRelated.Count);
			AssertEquals(1, routingProvider1.Transports.Count);
			AssertEquals("SEA", routingProvider1.TransportMode);
			AssertEquals(((IRoutingSupport)consol).AdditionalETAUpdateMsg, routingProvider1.AdditionalETAUpdateMsg);
			AssertEquals(((IRoutingSupport)consol).AdditionalETDUpdateMsg, routingProvider1.AdditionalETDUpdateMsg);

			var container2 = Factory.NewWithValidTestData<CommonContainer>();
			var routingProvider2 = container2 as IRoutingSupport;
			AssertNotNull(routingProvider2);

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_TransportMode] = Constants.TransportModes.Sea;
			((IRoutingSupport)declaration).Transports.AddNew();
			((IRoutingSupport)declaration).TransportsIncludingRelated.AddNew();
			var cusContainer = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer[CusContainerSchema.CO_JE] = declaration.PK;
			cusContainer[CusContainerSchema.CO_JC] = container2.PK;

			AssertEquals(2, routingProvider2.TransportsIncludingRelated.Count);
			AssertEquals(2, routingProvider2.Transports.Count);
			AssertEquals("SEA", routingProvider2.TransportMode);
			AssertEquals(((IRoutingSupport)declaration).AdditionalETAUpdateMsg, routingProvider2.AdditionalETAUpdateMsg);
			AssertEquals(((IRoutingSupport)declaration).AdditionalETDUpdateMsg, routingProvider2.AdditionalETDUpdateMsg);
		}

		#endregion

		#region Implementation

		CommonConsol Consol
		{
			get { return consol ?? (consol = Factory.New<CommonConsol>()); }
		}
		CommonConsol consol;

		BusinessObject Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
					declaration[JobDeclarationSchema.Constants.JE_TransportMode] = Constants.TransportModes.Sea;
				}
				return declaration;
			}
		}
		BusinessObject declaration;

		BusinessObjectCollection DeclarationCusContainers
		{
			get { return (BusinessObjectCollection)Declaration["CusContainers"]; }
		}

		RefContainer fContainerRef;
		RefContainer ContainerRef
		{
			get
			{
				if (fContainerRef == null)
				{
					fContainerRef = Factory.New<RefContainer>();
					fContainerRef.RC_Code = "20PP";
					fContainerRef.RC_CubicCapacity = 16m;
					fContainerRef.RC_GrossWeight = 10000m;
					fContainerRef.RC_Height = 3m;
					fContainerRef.RC_Length = 6m;
					fContainerRef.RC_Width = 2m;
					fContainerRef.RC_TareWeight = 1000m;
					fContainerRef.RC_TEU = 1;
				}
				return fContainerRef;
			}
		}

		RefContainer fContainerRef2;
		RefContainer ContainerRef2
		{
			get
			{
				if (fContainerRef2 == null)
				{
					fContainerRef2 = Factory.New<RefContainer>();
					fContainerRef2.RC_Code = "40PP";
					fContainerRef2.RC_CubicCapacity = 16m;
					fContainerRef2.RC_GrossWeight = 1000m;
					fContainerRef2.RC_Height = 5m;
					fContainerRef2.RC_Length = 12m;
					fContainerRef2.RC_Width = 2.5m;
					fContainerRef2.RC_TareWeight = 1800m;
					fContainerRef2.RC_TEU = 2;
				}
				return fContainerRef2;
			}
		}

		CommonShipment Shipment1;
		CommonShipment Shipment2;
		CommonShipment Shipment3;
		CommonContainer TestContainer;
		CommonConsol ExportConsol;
		CommonConsol ImportConsol;

		protected override void SetUp()
		{
			base.SetUp();

			Shipment1 = CommonShipment.New(Factory);
			Shipment2 = CommonShipment.New(Factory);
			Shipment3 = CommonShipment.New(Factory);

			TestContainer = Factory.New<CommonContainer>();

			Shipment1.JS_A_RCV = ZDateTime.Today;
			Shipment1.JS_OuterPacks = 20;
			Shipment1.JS_ActualVolume = 4m;
			Shipment1.JS_ActualWeight = new ZDecimal(2000.0);

			Shipment2.JS_A_RCV = ZDateTime.Today;
			Shipment2.JS_OuterPacks = 10;
			Shipment2.JS_ActualVolume = new ZDecimal(4.0);
			Shipment2.JS_ActualWeight = new ZDecimal(2000.0);

			Shipment3.JS_OuterPacks = 40;
			Shipment3.JS_ActualVolume = new ZDecimal(4.0);
			Shipment3.JS_ActualWeight = new ZDecimal(2000.0);

			Factory.Save();

			ExportConsol = GetExportConsol(typeof(CommonConsol));
			ImportConsol = GetImportConsol(typeof(CommonConsol));
		}

		class DummyContainerWithAttachedContainerLoadList : CommonContainer
		{
			public DummyContainerWithAttachedContainerLoadList(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override bool IsAttachedToContainerLoadList() => true;
		}
		#endregion

		#region AddressAdditionalInfo

		public void TestAddressAdditionalInfoItemsIsBindedToDependentAdresses()
		{
			var commonContainer = Factory.New<CommonContainer>();
			commonContainer.JC_OA_ArrivalContainerYardAddress = LocalContainerYard.MainAddress.PK;
			commonContainer.JC_OA_DepartureContainerYardAddress = OverseasContainerYard.MainAddress.PK;
			commonContainer.EmptyPickupByTransportMode = "ROA";
			commonContainer.EmptyReturnToTransportMode = "IWT";

			Factory.Save();

			AssertEquals("ROA", commonContainer.EmptyPickupByTransportMode);
			AssertEquals("IWT", commonContainer.EmptyReturnToTransportMode);

			Assert(!commonContainer.EmptyPickupByTransportMode_ReadOnly);
			Assert(!commonContainer.EmptyReturnToTransportMode_ReadOnly);
		}

		public void TestAddressAdditionalInfo_Is_Deleted_When_DependentAdress_Is_Cleared()
		{
			var commonContainer = Factory.New<CommonContainer>();
			commonContainer.JC_OA_ArrivalContainerYardAddress = LocalContainerYard.MainAddress.PK;
			commonContainer.JC_OA_DepartureContainerYardAddress = OverseasContainerYard.MainAddress.PK;
			commonContainer.EmptyPickupByTransportMode = "ROA";
			commonContainer.EmptyReturnToTransportMode = "IWT";

			Factory.Save();

			var rowsInDB = Factory.Load<JobAddressAdditionalInfo>(new ZQuery(JobAddressAdditionalInfoSchema.JAI_ParentID, commonContainer.PK));

			AssertEquals(2, rowsInDB.Length);

			AssertEquals("ROA", commonContainer.EmptyPickupByTransportMode);
			AssertEquals("IWT", commonContainer.EmptyReturnToTransportMode);

			commonContainer.JC_OA_ArrivalContainerYardAddress = Guid.Empty;
			commonContainer.JC_OA_DepartureContainerYardAddress = Guid.Empty;

			Factory.Save();

			rowsInDB = Factory.Load<JobAddressAdditionalInfo>(new ZQuery(JobAddressAdditionalInfoSchema.JAI_ParentID, commonContainer.PK));

			AssertEquals(0, rowsInDB.Length);

			AssertEquals(string.Empty, commonContainer.EmptyPickupByTransportMode);
			AssertEquals(string.Empty, commonContainer.EmptyReturnToTransportMode);

			Assert(commonContainer.EmptyPickupByTransportMode_ReadOnly);
			Assert(commonContainer.EmptyReturnToTransportMode_ReadOnly);
		}

		public void TestAddressAdditionalInfo_Has_Default_Value_When_DependentAdress_Is_Reset()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.MainAddress.AddAddressType(OrgAddressType.Office);
			org1.OH_Code = "ORG1";
			org1.MainAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var org2 = Factory.New<OrgHeader>();
			org2.MainAddress.AddAddressType(OrgAddressType.Office);
			org2.OH_Code = "ORG2";
			org2.MainAddress.OA_RL_NKRelatedPortCode = "AUML";

			var commonContainer = Factory.New<CommonContainer>();
			commonContainer.JC_OA_ArrivalContainerYardAddress_ZAddress.OrgPK = org1.PK;
			commonContainer.JC_OA_DepartureContainerYardAddress_ZAddress.OrgPK = org1.PK;
			commonContainer.EmptyPickupByTransportMode = "RAI";
			commonContainer.EmptyReturnToTransportMode = "RAI";

			Factory.Save();

			AssertEquals("RAI", commonContainer.EmptyPickupByTransportMode);
			AssertEquals("RAI", commonContainer.EmptyReturnToTransportMode);

			commonContainer.JC_OA_ArrivalContainerYardAddress_ZAddress.OrgPK = org2.PK;
			commonContainer.JC_OA_DepartureContainerYardAddress_ZAddress.OrgPK = org2.PK;
			commonContainer.EmptyPickupByTransportMode = "IWT";
			commonContainer.EmptyReturnToTransportMode = "IWT";

			Factory.Save();

			AssertEquals("IWT", commonContainer.EmptyPickupByTransportMode);
			AssertEquals("IWT", commonContainer.EmptyReturnToTransportMode);

			commonContainer.JC_OA_ArrivalContainerYardAddress_ZAddress.OrgPK = org1.PK;
			commonContainer.JC_OA_DepartureContainerYardAddress_ZAddress.OrgPK = org1.PK;

			AssertEquals(string.Empty, commonContainer.EmptyPickupByTransportMode);
			AssertEquals(string.Empty, commonContainer.EmptyReturnToTransportMode);

			commonContainer.JC_OA_ArrivalContainerYardAddress_ZAddress.OrgPK = org2.PK;
			commonContainer.JC_OA_DepartureContainerYardAddress_ZAddress.OrgPK = org2.PK;

			AssertEquals(string.Empty, commonContainer.EmptyPickupByTransportMode);
			AssertEquals(string.Empty, commonContainer.EmptyReturnToTransportMode);
		}

		#endregion

		#region TestEmptyPickupByTransportMode

		public void TestEmptyPickupByTransportModeIsReadOnlyWhenContainerCYAndConsolCYIsEmpty()
		{
			// Arrange
			var container = Factory.New<CommonContainer>();
			var consol = Factory.New<CommonConsol>();
			consol.Containers.Add(container);
			container.EmptyReturnToTransportMode = "RAI";

			// Action
			container.JC_OA_DepartureContainerYardAddress = Guid.Empty;
			consol.JK_OA_ContainerYardEmptyPickupAddress = Guid.Empty;

			// Assert
			Assert("EmptyPickupByTransportMode is read only", container.EmptyPickupByTransportMode_ReadOnly);
			AssertEquals("EmptyPickupByTransportMode is empty", string.Empty, container.EmptyPickupByTransportMode);
		}

		public void TestEmptyPickupByTransportModeIsEditableWhenContainerCYIsPopulatedOrConsolCYIsPopulated()
		{
			// Arrange
			var container = Factory.New<CommonContainer>();
			var consol = Factory.New<CommonConsol>();
			consol.Containers.Add(container);

			// Action
			container.JC_OA_DepartureContainerYardAddress = Factory.New<OrgAddress>().PK;
			consol.JK_OA_ContainerYardEmptyPickupAddress = Guid.Empty;

			// Assert
			Assert("EmptyPickupByTransportMode is editable", !container.EmptyPickupByTransportMode_ReadOnly);

			// Action
			container.JC_OA_DepartureContainerYardAddress = Guid.Empty;
			consol.JK_OA_ContainerYardEmptyPickupAddress = Factory.New<OrgAddress>().PK;

			// Assert
			Assert("EmptyPickupByTransportMode is editable", !container.EmptyPickupByTransportMode_ReadOnly);

			// Action
			container.JC_OA_DepartureContainerYardAddress = Factory.New<OrgAddress>().PK;
			consol.JK_OA_ContainerYardEmptyPickupAddress = Factory.New<OrgAddress>().PK;

			// Assert
			Assert("EmptyPickupByTransportMode is editable", !container.EmptyPickupByTransportMode_ReadOnly);
		}

		public void TestEmptyPickupByTransportModeIsClearedWhenContainerCYChangesValues()
		{
			// Arrange
			var container = Factory.New<CommonContainer>();
			var consol = Factory.New<CommonConsol>();
			consol.Containers.Add(container);

			container.JC_OA_DepartureContainerYardAddress = Guid.Empty;
			consol.JK_OA_ContainerYardEmptyPickupAddress = Factory.New<OrgAddress>().PK;
			container.EmptyPickupByTransportMode = "RAI";

			AssertEquals("RAI", container.EmptyPickupByTransportMode);

			// Action
			container.JC_OA_DepartureContainerYardAddress = Factory.New<OrgAddress>().PK;

			// Assert
			AssertEquals("EmptyPickupByTransportMode is cleared", string.Empty, container.EmptyPickupByTransportMode);

			// Arrange
			container.JC_OA_DepartureContainerYardAddress = Factory.New<OrgAddress>().PK;
			consol.JK_OA_ContainerYardEmptyPickupAddress = Factory.New<OrgAddress>().PK;
			container.EmptyPickupByTransportMode = "RAI";

			AssertEquals("RAI", container.EmptyPickupByTransportMode);

			// Action
			container.JC_OA_DepartureContainerYardAddress = Guid.Empty;

			// Assert
			AssertEquals("EmptyPickupByTransportMode is cleared", string.Empty, container.EmptyPickupByTransportMode);

			// Arrange
			container.JC_OA_DepartureContainerYardAddress = Factory.New<OrgAddress>().PK;
			consol.JK_OA_ContainerYardEmptyPickupAddress = Guid.Empty;
			container.EmptyPickupByTransportMode = "RAI";

			AssertEquals("RAI", container.EmptyPickupByTransportMode);

			// Action
			container.JC_OA_DepartureContainerYardAddress = Guid.Empty;

			// Assert
			AssertEquals("EmptyPickupByTransportMode is cleared", string.Empty, container.EmptyPickupByTransportMode);
		}

		#endregion

		#region TestEmptyReturnToTransportMode

		public void TestEmptyReturnToTransportModeIsReadOnlyWhenContainerCYIsEmptyAndContainerCYIsEmpty()
		{
			// Arrange
			var container = Factory.New<CommonContainer>();
			var consol = Factory.New<CommonConsol>();
			consol.Containers.Add(container);
			container.EmptyReturnToTransportMode = "RAI";

			// Action
			container.JC_OA_ArrivalContainerYardAddress = Guid.Empty;
			consol.JK_OA_ContainerYardEmptyReturnAddress = Guid.Empty;

			// Assert
			Assert("EmptyReturnToTransportMode is readonly", container.EmptyReturnToTransportMode_ReadOnly);
			AssertEquals("EmptyPickupByTransportMode is empty", string.Empty, container.EmptyReturnToTransportMode);
		}

		public void TestEmptyReturnToTransportModeIsEditableWhenContainerCYIsPopulatedOrConsolCYIsPopulated()
		{
			// Arrange
			var container = Factory.New<CommonContainer>();
			var consol = Factory.New<CommonConsol>();
			consol.Containers.Add(container);

			// Action
			container.JC_OA_ArrivalContainerYardAddress = Guid.Empty;
			consol.JK_OA_ContainerYardEmptyReturnAddress = Factory.New<OrgAddress>().PK;

			// Assert
			Assert("EmptyReturnToTransportMode is editable", !container.EmptyReturnToTransportMode_ReadOnly);

			// Action
			container.JC_OA_ArrivalContainerYardAddress = Factory.New<OrgAddress>().PK;
			consol.JK_OA_ContainerYardEmptyReturnAddress = Guid.Empty;

			// Assert
			Assert("EmptyReturnToTransportMode is editable", !container.EmptyReturnToTransportMode_ReadOnly);

			// Action
			container.JC_OA_ArrivalContainerYardAddress = Factory.New<OrgAddress>().PK;
			consol.JK_OA_ContainerYardEmptyReturnAddress = Factory.New<OrgAddress>().PK;

			// Assert
			Assert("EmptyReturnToTransportMode is editable", !container.EmptyReturnToTransportMode_ReadOnly);
		}

		public void TestEmptyReturnToTransportModeIsClearedWhenContainerCYChangesValues()
		{
			// Arrange
			var container = Factory.New<CommonContainer>();
			var consol = Factory.New<CommonConsol>();
			consol.Containers.Add(container);

			container.JC_OA_ArrivalContainerYardAddress = Guid.Empty;
			consol.JK_OA_ContainerYardEmptyReturnAddress = Factory.New<OrgAddress>().PK;
			container.EmptyReturnToTransportMode = "RAI";

			AssertEquals("RAI", container.EmptyReturnToTransportMode);

			// Action
			container.JC_OA_ArrivalContainerYardAddress = Factory.New<OrgAddress>().PK;

			// Assert
			AssertEquals("EmptyReturnToTransportMode is cleared", string.Empty, container.EmptyReturnToTransportMode);

			// Arrange
			container.JC_OA_ArrivalContainerYardAddress = Factory.New<OrgAddress>().PK;
			consol.JK_OA_ContainerYardEmptyReturnAddress = Factory.New<OrgAddress>().PK;
			container.EmptyReturnToTransportMode = "RAI";

			AssertEquals("RAI", container.EmptyReturnToTransportMode);

			// Action
			container.JC_OA_ArrivalContainerYardAddress = Guid.Empty;

			// Assert
			AssertEquals("EmptyReturnToTransportMode is cleared", string.Empty, container.EmptyReturnToTransportMode);

			// Arrange
			container.JC_OA_ArrivalContainerYardAddress = Factory.New<OrgAddress>().PK;
			consol.JK_OA_ContainerYardEmptyReturnAddress = Guid.Empty;
			container.EmptyReturnToTransportMode = "RAI";

			AssertEquals("RAI", container.EmptyReturnToTransportMode);

			// Action
			container.JC_OA_ArrivalContainerYardAddress = Guid.Empty;

			// Assert
			AssertEquals("EmptyReturnToTransportMode is cleared", string.Empty, container.EmptyReturnToTransportMode);
		}

		#endregion

		#region  TestJC_EPANStatus

		public void TestJC_EPANStatus()
		{
			var container = Factory.New<CommonContainer>();
			var consol = Factory.New<CommonConsol>();
			consol.Containers.Add(container);

			AssertEquals("Message status - No events created", "Not Sent", container.JC_EPANStatus);

			var interchangeSentEventParameters = new[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, Constants.EventReferenceMessageTypes.ExportPreAdviceNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"),
			};
			CreateContainerEventLog(container, Events.InterchangeSent, new ZDateTime(2024, 10, 14), interchangeSentEventParameters);

			AssertEquals("Message status - No event linked", "Not Sent", container.JC_EPANStatus);

			AddMessageSentEvent(container);
			AssertEquals("Message status - MSN", "Sent", container.JC_EPANStatus);

			AddMessageAcceptedEvent(container);
			AssertEquals("Message status - MAA", "Accepted", container.JC_EPANStatus);

			AddMessageRejectedEvent(container);
			AssertEquals("Message status - MRJ", "Rejected", container.JC_EPANStatus);

			AddMessageWithdrawCancelRequestEvent(container);
			AssertEquals("Message status - MWR", "Withdrawal Sent", container.JC_EPANStatus);

			AddInterchangeRejectedEvent(container);
			AssertEquals("Message status - IRJ", "Rejected", container.JC_EPANStatus);

			AddMessageWithdrawalAcceptedEvent(container);
			AssertEquals("Message status - MWA", "Withdrawn", container.JC_EPANStatus);

			CreateContainerEventLog(container, Events.InterchangeSent, new ZDateTime(2024, 10, 21), interchangeSentEventParameters);

			AssertEquals("ISN", container.Logs.MostRecentLogByPostedDate.SL_SE_NKEvent);
			AssertEquals("If latest response is other event type, keep searching until one of the above events is found.", "Withdrawn", container.JC_EPANStatus);
		}

		void AddMessageSentEvent(CommonContainer container)
		{
			CreateContainerEventLog(container, Events.MessageSent, new ZDateTime(2024, 10, 15), new[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, Constants.EventReferenceMessageTypes.ExportPreAdviceNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"),
			});
		}

		void AddMessageAcceptedEvent(CommonContainer container)
		{
			CreateContainerEventLog(container, Events.MessageAccepted, new ZDateTime(2024, 10, 16), new[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, Constants.EventReferenceMessageTypes.ExportPreAdviceNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, "NZAKL"),
			});
		}

		void AddMessageRejectedEvent(CommonContainer container)
		{
			CreateContainerEventLog(container, Events.MessageRejected, new ZDateTime(2024, 10, 17), new[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, Constants.EventReferenceMessageTypes.ExportPreAdviceNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, "NZAKL"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, "Reason of message rejected"),
			});
		}

		void AddMessageWithdrawCancelRequestEvent(CommonContainer container)
		{
			CreateContainerEventLog(container, Events.MessageWithdrawCancelRequest, new ZDateTime(2024, 10, 18), new[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, Constants.EventReferenceMessageTypes.ExportPreAdviceNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, "NZAKL"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, "Reason of withdraw accepted"),
			});
		}

		void AddInterchangeRejectedEvent(CommonContainer container)
		{
			CreateContainerEventLog(container, Events.InterchangeRejected, new ZDateTime(2024, 10, 19), new[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, Constants.EventReferenceMessageTypes.ExportPreAdviceNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "WiseTech Global"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, "Reason of interchange rejection"),
			});
		}

		void AddMessageWithdrawalAcceptedEvent(CommonContainer container)
		{
			CreateContainerEventLog(container, Events.MessageWithdrawCancelAccepted, new ZDateTime(2024, 10, 20), new[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, Constants.EventReferenceMessageTypes.ExportPreAdviceNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, "NZAKL"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, "Reason of withdraw accepted"),
			});
		}

		void CreateContainerEventLog(CommonContainer container, Event @event, ZDateTime time, params KeyValuePair<string, string>[] parameters)
		{
			container.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, time.ToOffset(), string.Empty, parameters);
			Thread.Sleep(10);
			Factory.Save();
		}

		#endregion
	}
}
