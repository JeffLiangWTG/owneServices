using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.GUI;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(JobConsolModule))]
	internal sealed class JobConsolModuleTest : ZModuleBasherTest
	{
		[RequiresSTA]
		public void TestSetGuiProviders()
		{
			using (JobConsolModuleForTest module = new JobConsolModuleForTest())
			{
				IForwardingConsolDocumentSupporterQueryProvider consolDocumentSupporterQueryProvider = module.Factory.GetValue<IForwardingConsolDocumentSupporterQueryProvider>();
				IForwardingShipmentDocumentSupporterQueryProvider shipmentDocumentSupporterQueryProvider = module.Factory.GetValue<IForwardingShipmentDocumentSupporterQueryProvider>();
				IServicesSelectionProvider servicesSelectionProvider = module.Factory.GetValue<IServicesSelectionProvider>();

				module.PerformSearch();

				consolDocumentSupporterQueryProvider = module.Factory.GetValue<IForwardingConsolDocumentSupporterQueryProvider>();
				AssertNotNull(consolDocumentSupporterQueryProvider);
				Assert(consolDocumentSupporterQueryProvider is ForwardingConsolDocumentSupporterGuiQueryProvider);

				shipmentDocumentSupporterQueryProvider = module.Factory.GetValue<IForwardingShipmentDocumentSupporterQueryProvider>();
				AssertNotNull(shipmentDocumentSupporterQueryProvider);
				Assert(shipmentDocumentSupporterQueryProvider is ForwardingShipmentDocumentSupporterGuiQueryProvider);

				servicesSelectionProvider = module.Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull(servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);

				module.PerformSearch();

				consolDocumentSupporterQueryProvider = module.Factory.GetValue<IForwardingConsolDocumentSupporterQueryProvider>();
				AssertNotNull(consolDocumentSupporterQueryProvider);
				Assert(consolDocumentSupporterQueryProvider is ForwardingConsolDocumentSupporterGuiQueryProvider);

				shipmentDocumentSupporterQueryProvider = module.Factory.GetValue<IForwardingShipmentDocumentSupporterQueryProvider>();
				AssertNotNull(shipmentDocumentSupporterQueryProvider);
				Assert(shipmentDocumentSupporterQueryProvider is ForwardingShipmentDocumentSupporterGuiQueryProvider);

				servicesSelectionProvider = module.Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull(servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);
			}
		}
		public void TestModuleIDAndSupportsWorkflow()
		{
			using (JobConsolModule module = new JobConsolModule())
			{
				AssertEquals(ModuleIDs.JobConsol, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		[RequiresSTA]
		public void TestDomainContextIsSet()
		{
			using (var module = new JobConsolModuleForTest())
			{
				AssertEquals(FreightDomainContext.Forwarding, module.GridCollection.Factory.GetFreightDomainContext());

				module.PerformSearch();
				AssertEquals(FreightDomainContext.Forwarding, module.GridCollection.Factory.GetFreightDomainContext());

				module.PerformSearch();
				AssertEquals(FreightDomainContext.Forwarding, module.GridCollection.Factory.GetFreightDomainContext());
			}
		}

		[RequiresSTA]
		public void TestChildEditableServiceSetToConsol()
		{
			using (JobConsolModuleForTest module = new JobConsolModuleForTest())
			{
				AssertEquals(ChildEditableServiceStates.Consol, ChildEditableService.GetStateDirectly(module.GridCollection.Factory));

				module.PerformSearch();
				AssertEquals(ChildEditableServiceStates.Consol, ChildEditableService.GetStateDirectly(module.Factory));

				module.PerformSearch();
				AssertEquals(ChildEditableServiceStates.Consol, ChildEditableService.GetStateDirectly(module.Factory));
			}
		}

		[RequiresSTA]
		public void TestCopyPerSchedule()
		{
			CommonConsol consol1 = Factory.NewWithValidTestData<CommonConsol>();
			CommonConsol consol2 = Factory.NewWithValidTestData<CommonConsol>();
			consol1.JK_AgentType = Constants.AgentType.Charter;
			consol2.JK_AgentType = Constants.AgentType.Direct;
			BaseJobSailing sailing = Factory.NewWithValidTestData<JobSailing>();
			consol1.Transports[0].JW_JX = sailing.PK;
			consol2.Transports[0].JW_IsLinked = false;
			Factory.Save();

			using (ZForm form = new ZForm())
			using (JobConsolModuleForTest module = new JobConsolModuleForTest())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				Application.DoEvents();

				module.GridCollection.Load();
				AssertEquals("Pre-condition", 2, module.GridCollection.Count);
				AssertEquals("Pre-condition", 2, module.Grid.List.Count);

				module.FormActionMenu.FindByText("&Copy").MenuItems[1].PerformClick();
				module.GridCollection.Load();
				AssertEquals("No new items created as none selected", 2, module.GridCollection.Count);

				module.Grid.SelectSingleElement(consol1);
				module.FormActionMenu.FindByText("&Copy").MenuItems[1].PerformClick();
				AssertEquals("You can only copy a Consol based on a Schedule for Direct, Agent, Gateway Agent, Courier or Buyers Consolidations.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				consol1.JK_AgentType = Constants.AgentType.Agent;
				consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				Factory.Save();
				module.FormActionMenu.FindByText("&Copy").MenuItems[1].PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("BulkScheduleCopyForm", ZFormModaliser.LastFormShownDialogForTest.Name);
				AssertEquals(true, ((BulkCopyCriteria)(ZFormModaliser.LastFormShownDialogForTest as BulkScheduleCopyForm).LastDataSourceForTest).ConsolDetails.CreateConsol);
				AssertEquals(true, ((BulkCopyCriteria)(ZFormModaliser.LastFormShownDialogForTest as BulkScheduleCopyForm).LastDataSourceForTest).ConsolDetails.CreateConsolInfo.ReadOnly);
				AssertEquals(consol1.PK, ((BulkCopyCriteria)(ZFormModaliser.LastFormShownDialogForTest as BulkScheduleCopyForm).LastDataSourceForTest).ConsolDetails.TemplateConsolPK);
				AssertEquals(true, ((BulkCopyCriteria)(ZFormModaliser.LastFormShownDialogForTest as BulkScheduleCopyForm).LastDataSourceForTest).ConsolDetails is ForwardingBulkSailingConsolGenerator);

				consol1.JK_AgentType = Constants.AgentType.Courier;
				Factory.Save();
				module.FormActionMenu.FindByText("&Copy").MenuItems[1].PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("BulkScheduleCopyForm", ZFormModaliser.LastFormShownDialogForTest.Name);
				AssertEquals(true, ((BulkCopyCriteria)(ZFormModaliser.LastFormShownDialogForTest as BulkScheduleCopyForm).LastDataSourceForTest).ConsolDetails.CreateConsol);
				AssertEquals(true, ((BulkCopyCriteria)(ZFormModaliser.LastFormShownDialogForTest as BulkScheduleCopyForm).LastDataSourceForTest).ConsolDetails.CreateConsolInfo.ReadOnly);
				AssertEquals(consol1.PK, ((BulkCopyCriteria)(ZFormModaliser.LastFormShownDialogForTest as BulkScheduleCopyForm).LastDataSourceForTest).ConsolDetails.TemplateConsolPK);
				AssertEquals(true, ((BulkCopyCriteria)(ZFormModaliser.LastFormShownDialogForTest as BulkScheduleCopyForm).LastDataSourceForTest).ConsolDetails is ForwardingBulkSailingConsolGenerator);

				consol1.JK_AgentType = Constants.AgentType.Direct;
				Factory.Save();
				module.FormActionMenu.FindByText("&Copy").MenuItems[1].PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("BulkScheduleCopyForm", ZFormModaliser.LastFormShownDialogForTest.Name);
				AssertEquals(true, ((BulkCopyCriteria)(ZFormModaliser.LastFormShownDialogForTest as BulkScheduleCopyForm).LastDataSourceForTest).ConsolDetails.CreateConsol);
				AssertEquals(true, ((BulkCopyCriteria)(ZFormModaliser.LastFormShownDialogForTest as BulkScheduleCopyForm).LastDataSourceForTest).ConsolDetails.CreateConsolInfo.ReadOnly);
				AssertEquals(consol1.PK, ((BulkCopyCriteria)(ZFormModaliser.LastFormShownDialogForTest as BulkScheduleCopyForm).LastDataSourceForTest).ConsolDetails.TemplateConsolPK);
				AssertEquals(true, ((BulkCopyCriteria)(ZFormModaliser.LastFormShownDialogForTest as BulkScheduleCopyForm).LastDataSourceForTest).ConsolDetails is ForwardingBulkSailingConsolGenerator);

				consol1.Transports[0].JW_IsLinked = false;
				Factory.Save();
				module.FormActionMenu.FindByText("&Copy").MenuItems[1].PerformClick();
				AssertEquals("You can only copy a Consol that is linked to a Schedule.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertNotificationByDifferentTransportMode(consol1, module, Constants.TransportModes.Air);
				AssertNotificationByDifferentTransportMode(consol1, module, Constants.TransportModes.Rail);
				AssertNotificationByDifferentTransportMode(consol1, module, Constants.TransportModes.Road);

				module.Grid.SelectAllElements();
				AssertEquals("Two items should be selected", 2, module.Grid.SelectedElements.Length);
				module.FormActionMenu.FindByText("&Copy").MenuItems[1].PerformClick();
				AssertEquals("You can only select a single item to perform this action on as it requires user intervention to complete the action.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				consol1.JK_TransportMode = Constants.TransportModes.Sea;
				Factory.Save();
				module.Grid.SelectSingleElement(consol1);
				module.FormActionMenu.FindByText("&Copy").MenuItems[1].PerformClick();
				AssertEquals("You cannot copy SEA Consolidation because \"Per Schedule\" actions attempts to copy Schedules as well, however, the same vessel-voyage combination cannot be duplicated.  Use \"Single Copy\" instead to copy a Consol for the same Sailing Schedule.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		void AssertNotificationByDifferentTransportMode(CommonConsol consol1, JobConsolModuleForTest module, ZString transportMode)
		{
			consol1.JK_TransportMode = transportMode;
			Factory.Save();
			module.FormActionMenu.FindByText("&Copy").MenuItems[1].PerformClick();
			AssertEquals("You can only copy a Consol that is linked to a Schedule.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		[TestDate(2013, 7, 1)]
		[RequiresSTA]
		public void TestCopyPerSchedule_CopyTransports()
		{
			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage1.JV_VoyageFlight = "QF001";
			voyage1.JV_FlightDate = ZDateTime.Today.AddDays(1);

			var origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "HKHKG";
			origin1.JA_E_DEP = ZDateTime.Today.AddDays(1);

			var destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "NZAKL";

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage2.JV_VoyageFlight = "QF002";
			voyage2.JV_FlightDate = ZDateTime.Today.AddDays(3);

			var origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "NZAKL";
			origin2.JA_E_DEP = ZDateTime.Today.AddDays(3);

			var destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "AUSYD";

			Factory.Save();

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Direct;

			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var transport1 = consol.Transports[0];
			transport1.JW_IsLinked = true;
			transport1.JW_JX = voyage1.Sailings[0].PK;

			var transport2 = consol.Transports.AddNew();
			transport2.JW_IsLinked = true;
			transport2.JW_JX = voyage2.Sailings[0].PK;

			AssertEquals("prerequisite - most interesting transport", transport2, consol.Transports.MostInterestingTransport);

			Factory.Save();

			using (var form = new ZForm())
			using (var module = new JobConsolModuleForTest())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				Application.DoEvents();

				module.GridCollection.Load();
				module.Grid.SelectSingleElement(consol);

				BulkCopyCriteria bulkCopyCriteria = null;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
				{
					var bulkScheduleCopyForm = f as BulkScheduleCopyForm;

					if (bulkScheduleCopyForm != null)
					{
						bulkScheduleCopyForm.Shown += (s, e) => bulkScheduleCopyForm.PerformActionClick();
						bulkScheduleCopyForm.BulkCopy.RepititionSelection.UseDailyPattern = false;
						bulkScheduleCopyForm.BulkCopy.RepititionSelection.UseMonthlyPattern = true;
						bulkScheduleCopyForm.BulkCopy.RepititionSelection.FromDate = ZDateTime.Today;
						bulkScheduleCopyForm.BulkCopy.RepititionSelection.ToDate = ZDateTime.Today.AddDays(10);
						bulkScheduleCopyForm.BulkCopy.ConsolDetails.AllocateNeutralMaster = false;
						bulkScheduleCopyForm.BulkCopy.ConsolDetails.CopyRoutings = true;
						bulkCopyCriteria = bulkScheduleCopyForm.BulkCopy;
					}

					var schedulesForm = f as SchedulesForm;

					if (schedulesForm != null)
					{
						schedulesForm.FormClosed += (s, e) => schedulesForm.Dispose();
					}
				});

				ZFormModaliser.ShowDialogsInTest = true;

				module.FormActionMenu.FindByText("&Copy").MenuItems[1].PerformClick();

				var createdConsol = (ForwardingConsol)bulkCopyCriteria.ConsolDetails.CreatedConsols[0];

				AssertContainsExactElementsInAnyOrder("transports", new[]
				{
					"HKHKG->NZAKL",
					"NZAKL->AUSYD"
				},
				createdConsol.Transports.Cast<Transport>().Select(t => string.Format("{0}->{1}", t.JW_RL_NKLoadPort, t.JW_RL_NKDiscPort)));
			}
		}

		[RequiresSTA]
		public void TestCopyPerScheduleMawbAllocation()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			JobMawb mawb1 = AddMawb(factory, "081", "00000011", GlbBranch.CurrentBranch, "STD");
			JobMawb mawb2 = AddMawb(factory, "081", "00000022", GlbBranch.CurrentBranch, "STD");
			JobSailing sailing = CreateSailing(factory);

			factory.Save();

			ForwardingConsol consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_UniqueConsignRef = "C0001";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_MasterBillNum = "081";
			consol.JK_IsNeutralMaster = true;

			consol.Transports[0].JW_JX = sailing.PK;

			factory.Save();

			AssertEquals("expected mawb 08100000011 allocated to consol", mawb1.PK, consol.MAWBAllocation.AllocatedMawb.PK);

			using (ZForm moduleHostForm = new ZForm())
			using (JobConsolModuleForTest module = new JobConsolModuleForTest())
			{
				moduleHostForm.Controls.Add(module.EmbeddedControl);
				moduleHostForm.Show();
				Application.DoEvents();

				module.GridCollection.Load();
				module.Grid.SelectSingleElement(consol);

				BulkCopyCriteria bulkCopyCriteria = null;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					BulkScheduleCopyForm bulkScheduleCopyForm = form as BulkScheduleCopyForm;

					if (bulkScheduleCopyForm != null)
					{
						bulkScheduleCopyForm.Shown += (s, e) => bulkScheduleCopyForm.PerformActionClick();
						bulkScheduleCopyForm.BulkCopy.RepititionSelection.ToDate = bulkScheduleCopyForm.BulkCopy.RepititionSelection.FromDate.AddDays(1);
						bulkScheduleCopyForm.BulkCopy.ConsolDetails.AllocateNeutralMaster = true;
						bulkCopyCriteria = bulkScheduleCopyForm.BulkCopy;
					}

					SchedulesForm schedulesForm = form as SchedulesForm;

					if (schedulesForm != null)
					{
						schedulesForm.FormClosed += (s, e) => schedulesForm.Dispose();
					}
				});

				ZFormModaliser.ShowDialogsInTest = true;

				module.FormActionMenu.FindByText("&Copy").MenuItems[1].PerformClick();

				Assert(ZFormModaliser.LastFormShownDialogForTest is BulkScheduleCopyForm);

				ForwardingConsol createdConsol = (ForwardingConsol)bulkCopyCriteria.ConsolDetails.CreatedConsols[0];
				createdConsol.Factory.Save();
				AssertNotNull("mawb allocated", createdConsol.MAWBAllocation.AllocatedMawb);
				AssertEquals("correct mawb allocated", "08100000022", createdConsol.MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix + createdConsol.MAWBAllocation.AllocatedMawb.JM_MAWB);
			}
		}

		public void TestInvalidColumnExceptionIsReported()
		{
			using (var module = new JobConsolModuleForTest())
			{
				var query = new ZQuery(JobConsolSchema.JK_RL_NKLoadPort, "AUBNE");
				query.AddToFilter(JobConShipLinkSchema.JN_JS, ZGuid.Empty);

				var collection = new ForwardingConsolCollection(new BusinessObjectFactory { RefreshEnabled = false });

				ErrorReporter.Clear();

				try
				{
					module.LoadCollectionExposed(collection, query);
					Assert("LoadCollectionExposed should throw exception", false);
				}
				catch (Exception ex)
				{
					AssertEquals("SqlException", ex.GetType().Name);
					AssertEquals("Invalid column name 'JN_JS'.", ex.Message); // We know this is reported because the top-level exception handler reports unhandled exceptions.
				}
			}
		}

		public void TestCO2eFiltersFunctionWorksWhenGreenhouseGasEmissionCalculationIsEnabled()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (var module = new JobConsolModuleForTest())
			{
				var consol1 = Factory.New<ForwardingConsol>();
				consol1.SetTotalCO2e(1000m);
				consol1.SetCO2eStatus(CO2eStatusList.Codes.Current);

				var consol2 = Factory.New<ForwardingConsol>();
				consol2.SetTotalCO2e(300);
				consol2.SetCO2eStatus(CO2eStatusList.Codes.Current);

				var consol3 = Factory.New<ForwardingConsol>();
				consol3.SetTotalCO2e(100m);
				consol3.SetCO2eStatus(CO2eStatusList.Codes.Current);

				var consol4 = Factory.New<ForwardingConsol>();
				consol4.SetTotalCO2e(500);
				consol4.SetCO2eStatus(CO2eStatusList.Codes.Current);

				var consol5 = Factory.New<ForwardingConsol>();
				consol5.SetTotalCO2e(1000m);
				consol5.SetCO2eStatus(CO2eStatusList.Codes.Current);

				var consol6 = Factory.New<ForwardingConsol>();
				consol6.SetTotalCO2e(1500m);
				consol6.SetCO2eStatus(CO2eStatusList.Codes.Current);

				var consol7 = Factory.New<ForwardingConsol>();
				consol7.SetTotalCO2e(0.2m);
				consol7.SetCO2eStatus(CO2eStatusList.Codes.Current);

				var consol8 = Factory.New<ForwardingConsol>();
				consol8.SetTotalCO2e(0m);
				consol8.SetCO2eStatus(CO2eStatusList.Codes.Pending);

				var consol9 = Factory.New<ForwardingConsol>();
				consol9.SetTotalCO2e(800m);
				consol9.SetCO2eStatus(CO2eStatusList.Codes.Rejected);

				Factory.Save();

				var filter = (ModuleNumberRangeFilter)module.FilterBusinessObject.ModuleFilters.FirstOrDefault(f => f.Description == "CO2e");

				AssertWeightRangeResult(filter, 0, 450, new ForwardingConsol[] { consol2, consol3, consol7 });
				AssertWeightRangeResult(filter, 0, 390, new ForwardingConsol[] { consol2, consol3, consol7 });
				AssertWeightRangeResult(filter, 0, 1000, new ForwardingConsol[] { consol1, consol2, consol3, consol4, consol5, consol7 });
				AssertWeightRangeResult(filter, 500, 1000, new ForwardingConsol[] { consol1, consol4, consol5 });
				AssertWeightRangeResult(filter, 800, 1000, new ForwardingConsol[] { consol1, consol5 });
				AssertWeightRangeResult(filter, 1000, 1500, new ForwardingConsol[] { consol1, consol5, consol6 });
			}

			void AssertWeightRangeResult(ModuleNumberRangeFilter filter, ZDecimal property1, ZDecimal property2, ForwardingConsol[] expectedConsols)
			{
				filter.IsActive = true;
				filter.Property1 = property1;
				filter.Property2 = property2;

				var result = Factory.Load<ForwardingConsol>(filter.Query);
				var collectionIds = result.Cast<ForwardingConsol>().Select(t => t.JK_MasterBillNum);
				AssertContainsExactElementsInAnyOrder(expectedConsols.Select(t => t.JK_MasterBillNum), collectionIds);
			}
		}

		#region Create Consol from dbo.eLoadList

		public void TestCreateConsolFromELoadListMenuItem_MenuItemVisibility()
		{
			using (var module = new JobConsolModuleForTest())
			{
				var actionsMenuItem = module.FormActionMenu.FindByText("&Actions");
				var menuItem = actionsMenuItem.MenuItems.FindByText("HVLV Origin Load List");
				AssertNotNull(menuItem);
			}
		}

		[RequiresSTA]
		public void TestCreateConsolFromELoadListMenuItem_ClickCreateAction_OpensHVLVOriginLoadListModule()
		{
			using (var module = new JobConsolModuleForTest())
			{
				var actionsMenuItem = module.FormActionMenu.FindByText("&Actions");
				var parentMenuItem = actionsMenuItem.MenuItems.FindByText("HVLV Origin Load List");
				var menuItem = parentMenuItem.MenuItems.FindByText("Create Consolidation from Origin Load List");

				AssertNotNull(menuItem);
				menuItem.PerformClick();

				var lastShownModuleID = ((EmbeddedModulePopup)ZFormModaliser.LastFormShownDialogForTest).CurrentModule.ModuleID;
				AssertEquals(ModuleIDs.HVLVOriginLoadList, lastShownModuleID);
			}
		}

		[RequiresSTA]
		public void TestCreateConsolFromELoadListMenuItem_NoMatchedDestinationUNLOCO_ShipmentCreatedOnConsolWithWarning()
		{
			TestCase_TestCreateConsolFromELoadListMenuItem_NoMatchedDestinationUNLOCO_ShipmentCreatedOnConsolWithWarning(registryValue: true);
		}
		[RequiresSTA]
		public void TestCreateConsolFromELoadListMenuItem_NoMatchedDestinationUNLOCO_ShipmentCreatedOnConsolWithWarning_Legacy()
		{
			TestCase_TestCreateConsolFromELoadListMenuItem_NoMatchedDestinationUNLOCO_ShipmentCreatedOnConsolWithWarning(registryValue: false);
		}

		void TestCase_TestCreateConsolFromELoadListMenuItem_NoMatchedDestinationUNLOCO_ShipmentCreatedOnConsolWithWarning(bool registryValue)
		{
			using (HVLVDataRegistry.Instance.ProcessLoadListUsingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var loadList = Factory.New<IHVLVOriginLoadList>();
				((BusinessObject)loadList).FillWithValidTestData();
				loadList.HVL_Status = "LDG";
				loadList.HVL_MasterBillNumber = "MBN20181227";
				loadList.HVL_IsMasterHouse = true;
				var loadListDestinationDepot = Factory.NewWithValidTestData<OrgAddress>();
				loadList.HVL_OA_DestinationDepot = loadListDestinationDepot.PK;

				var bookingHeader = Factory.New<IHVLVBookingHeader>();
				((BusinessObject)loadList).FillWithValidTestData();
				var billToParty = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
				bookingHeader.HVH_OA_BillToParty = billToParty;
				CreateConsignmentWithItem(bookingHeader, "Waybill1", loadList.PK);

				Factory.Save();

				var warningMessageText = string.Empty;
				var warningCaption = string.Empty;
				using (var module = new JobConsolModuleForTest())
				{
					var actionsMenuItem = module.FormActionMenu.FindByText("&Actions");
					var parentMenuItem = actionsMenuItem.MenuItems.FindByText("HVLV Origin Load List");
					var menuItem = parentMenuItem.MenuItems.FindByText("Create Consolidation from Origin Load List");

					AssertNotNull(menuItem);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
					{
						var popup = obj as EmbeddedModulePopup;
						var decisionProvider = popup.Module_ForTest.ModuleDecisionProvider;
						decisionProvider.HandleDefaultAction(new[] { (BusinessObject)loadList });
						warningCaption = UnitTestUserNotification.Instance.LastMessage.Caption;
						warningMessageText = UnitTestUserNotification.Instance.LastMessage.Text;
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					});
					menuItem.PerformClick();

					var forwardingConsol = Factory.LoadTop1<ForwardingConsol>(new ZQuery());
					CombineAssertions(() =>
					{
						AssertNotNull("Consol should be created from loadlist", forwardingConsol);
						AssertEquals("Warning message caption", "Action completed with warning", warningCaption);
						AssertContains("Warning message contents",
							"Warning: Cannot determine suitable Destination Port for HVL Shipment(s) on Consol 'MBN20181227'. There are zero or multiple UNLOCOs for country(s) 'US'.",
							warningMessageText);
					});
				}

				Application.OpenForms.OfType<ConsolForm>().Single().Close();
			}
		}

		[RequiresSTA]
		public void TestCreateConsolFromELoadListMenuItem_ClickCreateActionWithDuplicateWaybill_ErrorMessageBox()
		{
			TestCase_TestCreateConsolFromELoadListMenuItem_ClickCreateActionWithDuplicateWaybill_ErrorMessageBox(registryValue: true);
		}

		[RequiresSTA]
		public void TestCreateConsolFromELoadListMenuItem_ClickCreateActionWithDuplicateWaybill_ErrorMessageBox_Legacy()
		{
			TestCase_TestCreateConsolFromELoadListMenuItem_ClickCreateActionWithDuplicateWaybill_ErrorMessageBox(registryValue: false);
		}

		void TestCase_TestCreateConsolFromELoadListMenuItem_ClickCreateActionWithDuplicateWaybill_ErrorMessageBox(bool registryValue)
		{
			using (HVLVDataRegistry.Instance.ProcessLoadListUsingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var masterBillNumber1 = "MBN20181227";

				var loadList = Factory.New<IHVLVOriginLoadList>();
				((BusinessObject)loadList).FillWithValidTestData();
				loadList.HVL_Status = "LDG";
				loadList.HVL_MasterBillNumber = masterBillNumber1;

				var bookingHeader1 = Factory.New<IHVLVBookingHeader>();
				((BusinessObject)loadList).FillWithValidTestData();
				var bookingHeader2 = Factory.New<IHVLVBookingHeader>();
				((BusinessObject)loadList).FillWithValidTestData();
				bookingHeader1.HVH_BookingReference = "TestHeader1";
				bookingHeader2.HVH_BookingReference = "TestHeader2";
				var billToParty = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
				bookingHeader1.HVH_OA_BillToParty = billToParty;
				bookingHeader2.HVH_OA_BillToParty = billToParty;
				bookingHeader1.HVH_RS_NKBookingServiceLevel = "EXP";
				bookingHeader2.HVH_RS_NKBookingServiceLevel = "EXP";

				CreateConsignmentWithItem(bookingHeader1, "Waybill1", loadList.PK);
				CreateConsignmentWithItem(bookingHeader2, "Waybill1", loadList.PK);

				Factory.Save();

				var errorMessageText = string.Empty;
				using (var module = new JobConsolModuleForTest())
				{
					var actionsMenuItem = module.FormActionMenu.FindByText("&Actions");
					var parentMenuItem = actionsMenuItem.MenuItems.FindByText("HVLV Origin Load List");
					var menuItem = parentMenuItem.MenuItems.FindByText("Create Consolidation from Origin Load List");

					AssertNotNull(menuItem);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
					{
						var popup = obj as EmbeddedModulePopup;
						var decisionProvider = popup.Module_ForTest.ModuleDecisionProvider;
						decisionProvider.HandleDefaultAction(new[] { (BusinessObject)loadList });
						errorMessageText = UnitTestUserNotification.Instance.LastMessage.Text;
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					});
					menuItem.PerformClick();
				}

				var forwardingConsols = Factory.Load<ForwardingConsol>(new ZQuery());
				var expectedErrorMessage = $"Error: Error processing the HVLV Origin Load List '{loadList.HVL_UniqueReference}': Consignment Waybill number 'Waybill1' is repeated on booking headers: TestHeader1, TestHeader2";
				AssertContains("Error message contents", expectedErrorMessage, errorMessageText);
				AssertEquals("No consol should be created from loadlist", 0, forwardingConsols.Length);
			}
		}

		IHVLVConsignment CreateConsignmentWithItem(IHVLVBookingHeader header, string waybillNumber, ZGuid loadListPK)
		{
			var consignment = (IHVLVConsignment)header.Consignments.AddNew();
			consignment.HVC_WaybillNumber = waybillNumber;
			consignment.HVC_RN_NKConsigneeCountryCode = "US";
			var item = (IHVLVItem)consignment.Items.AddNew();
			item.HVI_HVL_LoadList = loadListPK;
			return consignment;
		}

		[RequiresSTA]
		public void TestCreateConsolFromELoadListMenuItem_ClickViewAction_OpensHVLVOriginLoadListModule()
		{
			using (var module = new JobConsolModuleForTest())
			{
				var actionsMenuItem = module.FormActionMenu.FindByText("&Actions");
				var parentMenuItem = actionsMenuItem.MenuItems.FindByText("HVLV Origin Load List");
				var menuItem = parentMenuItem.MenuItems.FindByText("View Origin Load List");

				AssertNotNull(menuItem);
				menuItem.PerformClick();

				var lastShownForm = ((EmbeddedModulePopup)ZFormModaliser.LastFormShownDialogForTest);
				var lastShownModuleID = lastShownForm.CurrentModule.ModuleID;
				AssertEquals(ModuleIDs.HVLVOriginLoadList, lastShownModuleID);
				AssertEquals(false, lastShownForm.RequireAtLeastOneItemToBeSelected);
			}
		}

		public void TestCreateConsolFromELoadListMenuItem_ViewOriginLoadList_Above_CreateConsolidationfromOriginLoadList()
		{
			using (var module = new JobConsolModuleForTest())
			{
				var actionsMenuItem = module.FormActionMenu.FindByText("&Actions");
				var parentMenuItem = actionsMenuItem.MenuItems.FindByText("HVLV Origin Load List");
				var menuItemViewLoadList = parentMenuItem.MenuItems.FindByText("View Origin Load List");
				var menuItemCreateLoadList = parentMenuItem.MenuItems.FindByText("Create Consolidation from Origin Load List");

				AssertGreaterThan("menuItem of View Origin Load List should above menuItem of Create Consolidation from Origin Load List in their parent control",
					parentMenuItem.MenuItems.IndexOf(menuItemCreateLoadList),
					parentMenuItem.MenuItems.IndexOf(menuItemViewLoadList));
			}
		}

		#endregion

		JobMawb AddMawb(BusinessObjectFactory factory, string prefix, string mawbNo, GlbBranch branch, string serviceLevel)
		{
			JobMawb mawb = factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = prefix;
			mawb.JM_MAWB = mawbNo;
			mawb.JM_GB = branch.PK;
			mawb.JM_ServiceLevel = serviceLevel;

			return mawb;
		}

		JobSailing CreateSailing(BusinessObjectFactory factory)
		{
			JobVoyage voyage = factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_FlightDate = ZDateTime.Today;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_JV = voyage.PK;
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Today;

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_JV = voyage.PK;
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(10);

			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		public void TestBusinessContexts()
		{
			using (JobConsolModule module = new JobConsolModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("Consol business context should be returned", BusinessContext.Consol, module.BusinessContexts[0]);
			}
		}

		public void TestFilterBusinessObjectCloneIncludesDeniedPartyFilters_CRTEnabled()
		{
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (var module = new JobConsolModuleForTest())
			{
				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.FirstOrDefault(f => f.Description.StartsWith("Legacy Screening Status_"));
				AssertNotEquals(null, filter);

				var clone = module.FilterBusinessObject.Clone();
				var filter2 = (ModuleTextFilter)clone.ModuleFilters.FirstOrDefault(f => f.Description.StartsWith("Legacy Screening Status_"));
				AssertNotEquals(null, filter2);
			}
		}

		public void TestFilterBusinessObjectCloneIncludesDeniedPartyFilters_CRTDisabled()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var module = new JobConsolModuleForTest())
			{
				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.FirstOrDefault(f => f.Description.StartsWith("Screening Status_"));
				AssertNotEquals(null, filter);

				var clone = module.FilterBusinessObject.Clone();
				var filter2 = (ModuleTextFilter)clone.ModuleFilters.FirstOrDefault(f => f.Description.StartsWith("Screening Status"));
				AssertNotEquals(null, filter2);
			}
		}

		public void TestComplianceFiltersWhenEnableComplianceRiskRegistryIsEnabled()
		{
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (var module = new JobConsolModuleForTest())
			{
				var complianceFilters = module.FilterBusinessObject.ModuleFilters.Where(c => c.Category.Description == "Compliance").Select(f => f.Description.Substring(0, f.Description.IndexOf("_"))).ToArray();
				AssertEquals("Compliance Category", 4, complianceFilters.Length);

				var expectedFilters = new string[] { "Overall", "Location", "Party", "Commodity" };
				AssertContainsExactElementsInAnyOrder(complianceFilters, expectedFilters);
			}
		}

		public void TestComplianceFiltersWhenEnableComplianceRiskRegistryIsDisabled()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var module = new JobConsolModuleForTest())
			{
				var complianceFilters = module.FilterBusinessObject.ModuleFilters.Where(c => c.Category.Description == "Compliance").ToArray();
				AssertEquals("Compliance Category", 0, complianceFilters.Length);
			}
		}

		public void TestDeniedPartyScreeningMenuAddedWhenEnableComplianceRiskIsFalse()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (var module = new JobConsolModule())
			{
				AssertNotNull(module.FormActionMenu.FindByText("View Compliance Status", true));
			}
		}

		[RequiresSTA]
		public void TestAllocateInstalmentMenu()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedArabEmirates))
			{
				var resMock = Res.UseMockData();
				resMock.SetResourceGetter((string key) => new ResourceStringData(key, "don't translate"));

				var module = new JobConsolModule();
				var allocateInstalmentMenu = module.FormActionMenu.FindByText("Allocate UAE Installment Numbers", true);
				AssertNotNull(allocateInstalmentMenu);
				allocateInstalmentMenu.PerformClick();
				AssertEquals("Please select at least ONE consolidation for installment number allocation.", UnitTestUserNotification.Instance.LastMessage.Text);

				resMock.Dispose();
				module.Dispose();
			}
		}

		#region Template Records

		[RequiresSTA]
		public void TestLoadCollection_TemplateRecords()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "ABC";
			Factory.Save();

			using (var module = new JobConsolModuleForTest())
			{
				var consol2 = module.GetNewTemplateRecordBusinessObjectCore_Exposed() as ForwardingConsol;
				consol2.JK_MasterBillNum = "DEF";
				consol2.Factory.Save();
			}

			using (var module = new JobConsolModuleForTest())
			{
				var collection = new ForwardingModuleConsolCollection(new BusinessObjectFactory { RefreshEnabled = false });
				module.LoadCollectionExposed(collection, new ZQuery());

				CombineAssertions("Pre-Condition - Should load non-template records by default", () =>
				{
					Assert(!module.AllowLoadTemplateRecords);
					AssertEquals(1, collection.Count);
					AssertEquals("ABC", collection[0].JK_MasterBillNum);
				});
			}

			AssertTemplateRecordsFilter(
				FilterStripBusinessObject.TemplateRecordsDescription,
				SQLComparisonOperator.Equal,
				FilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly,
				new string[] { "DEF" }
			);

			AssertTemplateRecordsFilter(
				FilterStripBusinessObject.TemplateRecordsDescription,
				SQLComparisonOperator.Equal,
				FilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesIncluded,
				new string[] { "ABC", "DEF" }
			);

			AssertTemplateRecordsFilter(
				FilterStripBusinessObject.TemplateRecordsDescription,
				SQLComparisonOperator.Equal,
				FilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesExcluded,
				new string[] { "ABC" }
			);
		}

		[RequiresSTA]
		public void TestLoadCollection_TemplateActive()
		{
			using (var module = new JobConsolModuleForTest())
			{
				var consolTemplate_Active = module.GetNewTemplateRecordBusinessObjectCore_Exposed() as ForwardingConsol;
				consolTemplate_Active.JK_MasterBillNum = "ABC";
				consolTemplate_Active.Factory.Save();

				var consolTemplate_Inactive = module.GetNewTemplateRecordBusinessObjectCore_Exposed() as ForwardingConsol;
				consolTemplate_Inactive.JK_MasterBillNum = "DEF";
				consolTemplate_Inactive.TemplateRecord.IsCancelled = true;
				consolTemplate_Inactive.Factory.Save();
			}

			var allLanguages = LanguageHelper.GetDefaultLanguageForOLookUpEditType().Keys.ToList();
			allLanguages.ForEach(lan =>
			{
				using (Res.TemporarilySwitchLanguage(lan))
				{
					AssertTemplateRecordsFilter(
						FilterStripBusinessObject.TemplateRecordsActive,
						SQLComparisonOperator.Equal,
						FilterStripBusinessObject.StatusActive,
						new string[] { "ABC" },
						forceShowAllTemplates: true
					);

					AssertTemplateRecordsFilter(
						FilterStripBusinessObject.TemplateRecordsActive,
						SQLComparisonOperator.Equal,
						FilterStripBusinessObject.StatusInactive,
						new string[] { "DEF" },
						forceShowAllTemplates: true
					);

					AssertTemplateRecordsFilter(
						FilterStripBusinessObject.TemplateRecordsActive,
						SQLComparisonOperator.Equal,
						FilterStripBusinessObject.StatusAll,
						new string[] { "ABC", "DEF" },
						forceShowAllTemplates: true
					);
				}
			});
		}

		[RequiresSTA]
		public void TestLoadCollection_TemplateName()
		{
			using (var module = new JobConsolModuleForTest())
			{
				var consol1 = module.GetNewTemplateRecordBusinessObjectCore_Exposed() as ForwardingConsol;
				consol1.JK_MasterBillNum = "ABC";
				consol1.TemplateRecord.STR_TemplateName = "My Template";
				consol1.Factory.Save();

				var consol2 = module.GetNewTemplateRecordBusinessObjectCore_Exposed() as ForwardingConsol;
				consol2.JK_MasterBillNum = "DEF";
				consol2.TemplateRecord.STR_TemplateName = "Other Template";
				consol2.Factory.Save();

				var consol3 = module.GetNewTemplateRecordBusinessObjectCore_Exposed() as ForwardingConsol;
				consol3.JK_MasterBillNum = "GHI";
				consol3.Factory.Save();
			}

			AssertTemplateRecordsFilter(
				FilterStripBusinessObject.TemplateRecordsTemplateName,
				SQLComparisonOperator.StartsWith,
				"My Template",
				new string[] { "ABC" },
				forceShowAllTemplates: true
			);

			AssertTemplateRecordsFilter(
				FilterStripBusinessObject.TemplateRecordsTemplateName,
				SQLComparisonOperator.StartsWith,
				"Other Template",
				new string[] { "DEF" },
				forceShowAllTemplates: true
			);

			AssertTemplateRecordsFilter(
				FilterStripBusinessObject.TemplateRecordsTemplateName,
				SQLComparisonOperator.StartsWith,
				ZString.Empty,
				new string[] { "ABC", "DEF", "GHI" },
				forceShowAllTemplates: true
			);

			AssertTemplateRecordsFilter(
				FilterStripBusinessObject.TemplateRecordsTemplateName,
				SQLComparisonOperator.IsBlank,
				ZString.Empty,
				new string[] { "GHI" },
				forceShowAllTemplates: true
			);

			AssertTemplateRecordsFilter(
				FilterStripBusinessObject.TemplateRecordsTemplateName,
				SQLComparisonOperator.IsNotBlank,
				ZString.Empty,
				new string[] { "ABC", "DEF" },
				forceShowAllTemplates: true
			);
		}

		[RequiresSTA]
		public void TestLoadCollection_TemplateNameOnBothConcreteConsolsAndTemplateRecords()
		{
			using (var module = new JobConsolModuleForTest())
			{
				var concreteConsol = module.GetNewTemplateRecordBusinessObjectCore_Exposed() as ForwardingConsol;
				concreteConsol.JK_MasterBillNum = "ABC";
				concreteConsol.TemplateRecord.STR_TemplateName = "My Template 1";
				concreteConsol.Factory.Save();

				var templateRecord = module.GetNewTemplateRecordBusinessObjectCore_Exposed() as ForwardingConsol;
				templateRecord.JK_MasterBillNum = "DEF";
				templateRecord.TemplateRecord.STR_TemplateName = "My Template 2";
				templateRecord.Factory.Save();

				concreteConsol.IsTemplateRecord = false;
				concreteConsol.Factory.Save();
			}

			AssertTemplateRecordsFilter(
				FilterStripBusinessObject.TemplateRecordsTemplateName,
				SQLComparisonOperator.StartsWith,
				"My Template",
				new string[] { "ABC", "DEF" },
				forceShowAllTemplates: true
			);
		}

		public void TestGetNewTemplateRecordBusinessObjectCore()
		{
			using (var module = new JobConsolModuleForTest())
			{
				var consol = module.GetNewTemplateRecordBusinessObjectCore_Exposed();
				var templateRecordProvider = (ITemplateRecordProvider)consol;

				Assert(templateRecordProvider.IsTemplateRecord);
				AssertNotNull(templateRecordProvider.TemplateRecord);
				AssertEquals(typeof(StmTemplateRecord), templateRecordProvider.TemplateRecord.GetType());
				var templateRecord = (StmTemplateRecord)templateRecordProvider.TemplateRecord;
				AssertNotEquals(consol.Factory._Instance, templateRecord.Factory._Instance);
				AssertEquals(module.ID.Name, templateRecord.STR_ModuleID);

				AssertEquals(typeof(TemplateRecordBusinessObjectFactory), consol.Factory.GetType());
				var templateFactory = (TemplateRecordBusinessObjectFactory)consol.Factory;
				AssertSame(consol, templateFactory.TemplateRecordProvider);
				AssertSame(templateRecord.Factory, templateFactory.TemplateRecordFactory);
			}
		}

		public void TestLoadFromTemplateRecordPkCore()
		{
			var localFactory = new BusinessObjectFactory();

			using (var module = new JobConsolModuleForTest())
			{
				var bizO = module.GetNewTemplateRecordBusinessObjectCore_Exposed() as ForwardingConsol;
				bizO.JK_MasterBillNum = "ABC";
				bizO.Factory.Save();

				var templateRecordProvider = (ITemplateRecordProvider)bizO;
				var templateRecord = (StmTemplateRecord)templateRecordProvider.TemplateRecord;

				var reloadedBizO = (ForwardingConsol)module.LoadFromTemplateRecordPk(localFactory, bizO.PK);
				AssertNull("Cannot reload template record by bizo's PK", reloadedBizO);

				reloadedBizO = (ForwardingConsol)module.LoadFromTemplateRecordPk(localFactory, templateRecord.PK);

				CombineAssertions("Should load template consol by template record's PK", () =>
				{
					AssertNotNull(reloadedBizO);
					AssertEquals("ABC", reloadedBizO.JK_MasterBillNum);
					AssertNotEquals(bizO.PK, reloadedBizO.PK);
					AssertSame("Should load in specific factory", localFactory, reloadedBizO.Factory);
				});

				var reloadedTemplateRecordProvider = (ITemplateRecordProvider)reloadedBizO;
				Assert(reloadedTemplateRecordProvider.IsTemplateRecord);

				var reloadedTemplateRecord = (StmTemplateRecord)reloadedTemplateRecordProvider.TemplateRecord;
				AssertEquals(templateRecord.PK, reloadedTemplateRecord.PK);
				AssertSame("Should load in specific factory", localFactory, reloadedTemplateRecord.Factory);
			}
		}

		#region Test Load Template Records By Filters

		[RequiresSTA]
		public void TestLoadTemplateRecordsByFilters_EndPorts()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			ForwardingConsol consol1, consol2, consol3, consol4;

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				using (var module = new JobConsolModuleForTest())
				{
					ForwardingConsol CreateConsol(string masterBillNo, string loadPort = "", string discPort = "")
					{
						var consol = module.GetNewTemplateRecordBusinessObjectCoreExposed();
						consol.JK_RL_NKLoadPort = loadPort;
						consol.JK_RL_NKDischargePort = discPort;
						consol.JK_MasterBillNum = masterBillNo;
						consol.Factory.Save();
						return consol;
					}

					consol1 = CreateConsol("consol1", "AUSYD");
					consol2 = CreateConsol("consol2", discPort: "CNSHA");
					consol3 = CreateConsol("consol3", "AUSYD", "CNSHA");
					consol4 = CreateConsol("consol4");
				}
			}

			using (var module = new JobConsolModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;
				var query = new ZQuery();
				query.AddToFilter(JobConsolSchema.JK_SystemCreateUser, user.GS_Code);
				var collection = new ForwardingConsolCollection(new BusinessObjectFactory { RefreshEnabled = false });

				var templateRecordsFilter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobConsolFilterBusinessObject.TemplateRecordsDescription);
				templateRecordsFilter.IsActive = true;
				templateRecordsFilter.Property = JobConsolFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				var filter = (ModuleLocationFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobConsolFilterBusinessObject.Descriptions.EndPorts);
				filter.IsActive = true;

				void AssertEndPortsResult(string loadPort, string discPort, IEnumerable<ForwardingConsol> expectedConsols)
				{
					collection.RemoveAll();
					filter.Property1 = loadPort;
					filter.Property2 = discPort;
					module.LoadCollectionExposed(collection, query);
					var collectionIds = collection.Cast<ForwardingConsol>().Select(t => t.JK_MasterBillNum);
					AssertContainsExactElementsInAnyOrder(expectedConsols.Select(t => t.JK_MasterBillNum), collectionIds);
				}

				AssertEndPortsResult("", "", new[] { consol1, consol2, consol3, consol4 });
				AssertEndPortsResult("AUSYD", "", new[] { consol1, consol3 });
				AssertEndPortsResult("", "CNSHA", new[] { consol2, consol3 });
				AssertEndPortsResult("AUSYD", "CNSHA", new[] { consol3 });
				AssertEndPortsResult("AU", "", new[] { consol1, consol3 });
				AssertEndPortsResult("", "CN", new[] { consol2, consol3 });
			}
		}

		[RequiresSTA]
		public void TestLoadTemplateRecordsByFilters_EndPorts_Version_2012_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				TestLoadTemplateRecordsByFilters_EndPorts();
			}
		}

		[RequiresSTA]
		public void TestLoadTemplateRecordsByFilters_TransportMode()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			ForwardingConsol consol1, consol2;

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				using (var module = new JobConsolModuleForTest())
				{
					ForwardingConsol CreateConsol(string masterBillNo, string transportMode)
					{
						var consol = module.GetNewTemplateRecordBusinessObjectCoreExposed();
						consol.JK_TransportMode = transportMode;
						consol.JK_MasterBillNum = masterBillNo;
						consol.Factory.Save();
						return consol;
					}

					consol1 = CreateConsol("consol1", Constants.TransportModes.Sea);
					consol2 = CreateConsol("08122223333", Constants.TransportModes.Air);
				}
			}

			using (var module = new JobConsolModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;
				var query = new ZQuery();
				query.AddToFilter(JobConsolSchema.JK_SystemCreateUser, user.GS_Code);
				var collection = new ForwardingConsolCollection(new BusinessObjectFactory { RefreshEnabled = false });

				var templateRecordsFilter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobConsolFilterBusinessObject.TemplateRecordsDescription);
				templateRecordsFilter.IsActive = true;
				templateRecordsFilter.Property = JobConsolFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobConsolFilterBusinessObject.Descriptions.TransportMode);
				filter.IsActive = true;

				void AssertTransportModeResult(string transportMode, IEnumerable<ForwardingConsol> expectedConsols)
				{
					collection.RemoveAll();
					filter.Property = transportMode;
					module.LoadCollectionExposed(collection, query);
					var collectionIds = collection.Cast<ForwardingConsol>().Select(t => t.JK_MasterBillNum);
					AssertContainsExactElementsInAnyOrder(expectedConsols.Select(t => t.JK_MasterBillNum), collectionIds);
				}

				AssertTransportModeResult("", new[] { consol1, consol2 });
				AssertTransportModeResult(Constants.TransportModes.Sea, new[] { consol1 });
				AssertTransportModeResult(Constants.TransportModes.Air, new[] { consol2 });
			}
		}

		[RequiresSTA]
		public void TestLoadTemplateRecordsByFilters_TransportMode_Version_2012_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				TestLoadTemplateRecordsByFilters_TransportMode();
			}
		}

		[RequiresSTA]
		public void TestLoadTemplateRecordsByFilters_LoadPortAndDischargePort()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			ForwardingConsol consol1, consol2, consol3, consol4;

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				using (var module = new JobConsolModuleForTest())
				{
					ForwardingConsol CreateConsol(ZString masterBillNum, ZString loadPort, ZString dischargePort)
					{
						var consol = module.GetNewTemplateRecordBusinessObjectCoreExposed();
						consol.JK_TransportMode = Constants.TransportModes.Sea;
						consol.JK_MasterBillNum = masterBillNum;
						consol.JK_RL_NKLoadPort = loadPort;
						consol.JK_RL_NKDischargePort = dischargePort;
						consol.Factory.Save();
						return consol;
					}

					consol1 = CreateConsol("consol1", "AUSYD", "NZAKL");
					consol2 = CreateConsol("consol2", "AUSYD", "HKHKG");
					consol3 = CreateConsol("consol3", "AUBNE", "HKHKG");
					consol4 = CreateConsol("consol4", "AUMEL", "SGSIN");
				}
			}

			using (var module = new JobConsolModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;
				var query = new ZQuery();
				query.AddToFilter(JobConsolSchema.JK_SystemCreateUser, user.GS_Code);
				var collection = new ForwardingConsolCollection(new BusinessObjectFactory { RefreshEnabled = false });

				var templateRecordsFilter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobConsolFilterBusinessObject.TemplateRecordsDescription);
				templateRecordsFilter.IsActive = true;
				templateRecordsFilter.Property = JobConsolFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				var filter = (ModuleLocationFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobConsolFilterBusinessObject.Descriptions.LoadDischarge);
				filter.IsActive = true;

				void AssertLoadPortAndDischargePortResult(ZString loadPort, ZString dischargePort, IEnumerable<ForwardingConsol> expectedConsols)
				{
					collection.RemoveAll();
					filter.Property1 = loadPort;
					filter.Property2 = dischargePort;
					module.LoadCollectionExposed(collection, query);
					var collectionIds = collection.Cast<ForwardingConsol>().Select(t => t.JK_MasterBillNum);
					AssertContainsExactElementsInAnyOrder(expectedConsols.Select(t => t.JK_MasterBillNum), collectionIds);
				}

				AssertLoadPortAndDischargePortResult("AUSYD", ZString.Empty, new[] { consol1, consol2 });
				AssertLoadPortAndDischargePortResult(ZString.Empty, "HKHKG", new[] { consol2, consol3 });
				AssertLoadPortAndDischargePortResult("AUSYD", "HKHKG", new[] { consol2 });
				AssertLoadPortAndDischargePortResult(ZString.Empty, ZString.Empty, new[] { consol1, consol2, consol3, consol4 });
				AssertLoadPortAndDischargePortResult("SGSIN", ZString.Empty, Array.Empty<ForwardingConsol>());
				AssertLoadPortAndDischargePortResult("AUSYD", "AUSYD", Array.Empty<ForwardingConsol>());
			}
		}

		[RequiresSTA]
		public void TestLoadTemplateRecordsByFilters_LoadPortAndDischargePort_Version_2012_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				TestLoadTemplateRecordsByFilters_LoadPortAndDischargePort();
			}
		}

		[RequiresSTA]
		public void TestLoadCollection_TemplateName_ShouldUseSubQuery()
		{
			using (var module = new JobConsolModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;

				var templateNameFilter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters
					.First(f => f.Description == FilterStripBusinessObject.TemplateRecordsTemplateName);
				templateNameFilter.IsActive = true;
				templateNameFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				templateNameFilter.Property = "A";

				var collection = new ForwardingConsolCollection(new BusinessObjectFactory { RefreshEnabled = false });
				var query = new ZQuery();
				module.LoadCollectionExposed(collection, query);

				AssertContains("JK_STR IN (SELECT STR_PK FROM dbo.StmTemplateRecord WHERE STR_TemplateName like 'A%')", query.LiteralTextADO);
			}
		}

		#endregion

		#endregion

		public void TestSetInspectionStatusMenuAdded()
		{
			using (var module = new JobConsolModule())
			{
				var menuItem = module.FormActionMenu.FindByText("Set Inspection Status", true);
				AssertNotNull("A 'Set Inspection Status' menu item was expected to be present.", menuItem);
			}
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.JobConsol;
		}

		class JobConsolModuleForTest : JobConsolModule
		{
			public new ZDisplayGrid Grid => base.Grid;

			public new BusinessObjectCollection GridCollection => (BusinessObjectCollection)base.GridCollection;

			public new BusinessObjectFactory Factory => base.Factory;

			public void PerformSearch()
			{
				base.PerformSearch();
			}

			public void LoadCollectionExposed(IBusinessObjectCollection collection, ZQuery query)
			{
				var result = base.PerformSearchCore(collection.TypeOfElements, query);
				BindSearchResultToGrid(collection, result);
			}

			public BusinessObject GetNewTemplateRecordBusinessObjectCore_Exposed() => base.GetNewTemplateRecordBusinessObjectCore();

			internal ForwardingConsol GetNewTemplateRecordBusinessObjectCoreExposed()
			{
				return (ForwardingConsol)GetNewTemplateRecordBusinessObjectCore();
			}
		}

		void AssertTemplateRecordsFilter(
			string filterType,
			SQLComparisonOperator filterOperation,
			string filterValue,
			string[] expectedMasterBillNums,
			bool forceShowAllTemplates = false)
		{
			using (var module = new JobConsolModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;

				if (forceShowAllTemplates)
				{
					var showTemplatesFilter = module
						.FilterBusinessObject
						.ModuleFilters
						.First(f => f.Description == FilterStripBusinessObject.TemplateRecordsDescription) as ModuleTextFilter;

					showTemplatesFilter.IsActive = true;
					showTemplatesFilter.Property = FilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;
				}

				var filter = module
					.FilterBusinessObject
					.ModuleFilters
					.First(f => f.Description == filterType) as ModuleTextFilter;

				filter.IsActive = true;
				filter.SqlComparisonOperator = filterOperation;
				filter.Property = filterValue;

				var collection = new ForwardingModuleConsolCollection(new BusinessObjectFactory { RefreshEnabled = false });
				module.LoadCollectionExposed(collection, new ZQuery());

				CombineAssertions($"Should load expected consols (Filter: {filterType}, Operation: {filterOperation.ToString()} Value: {filterValue})", () =>
				{
					AssertEquals(expectedMasterBillNums.Length, collection.Count);
					AssertContainsExactElementsInAnyOrder(expectedMasterBillNums, collection.Cast<ForwardingConsol>().ToArray().Select(c => c.JK_MasterBillNum));
				});
			}
		}

		#endregion
	}
}
