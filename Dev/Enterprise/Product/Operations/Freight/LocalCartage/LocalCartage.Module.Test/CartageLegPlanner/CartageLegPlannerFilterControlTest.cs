using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.Freight.LocalCartage.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.AutoRefresh;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Module
{
	[TestedType(typeof(CartageLegPlannerForm))]
	public class CartageLegPlannerFilterControlTest : ZFormBasherTest
	{
		public void TestPerformance_DBHits()
		{
			//setup
			TestDataSetupForDBHits();
			var searched = false;
			var factory = new BusinessObjectFactory();
			var planners = new CartageLegPlannerCollection(factory);
			using (var form = new ZForm(planners))
			{
				var filterControl = new CartageLegPlannerFilterControlForTest();
				ZGridControlBasher.ExposeAllColumnsInAllGrids(form, new NotificationBuffer());
				RepaintGrid(filterControl);
				var runSheetAllocatedFilter = (ModuleTextFilter)filterControl.FilterBusinessObject["RunSheet Allocated"];
				runSheetAllocatedFilter.IsActive = true;
				runSheetAllocatedFilter.Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.Unallocated;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.PerformSearch += delegate
				{
					searched = true;
				};
				filterControl.FirePerformSearch();
				RepaintGrid(filterControl);
				Assert("Should have searched", searched);
				AssertEquals("Should have found some legs", true, planners[0].CartageLegs.Count > 0);
				planners.Factory.ResetDatabaseLoadCount();
				var expectedMaxDbHits = new Dictionary<string, int>()
				{
					{ CusContainerSchema.Constants.TableName, 2 },
					{ CusEntryNumSchema.Constants.TableName, 2 },
					{ EDIMessageSchema.Constants.TableName, 2 },
					{ GenCustomAddOnRuleAckSchema.Constants.TableName, 5 },
					{ GenCustomAddOnValueSchema.Constants.TableName, 1 },
					{ GenAddOnColumnSchema.Constants.TableName, 1 },
					{ JobBookedCtgMoveSchema.Constants.TableName, 4 },
					{ JobContainerSchema.Constants.TableName, 2 },
					{ JobContainerLegsSchema.Constants.TableName, 2 },
					{ JobContainerPackPivotSchema.Constants.TableName, 2 },
					{ JobDocAddressSchema.Constants.TableName, 4 },
					{ JobServiceSchema.Constants.TableName, 4 },
					{ LocalCartageJobTypeSchema.Constants.TableName, 1 },
					{ LocalCartageVehicleActivitySchema.Constants.TableName, 4 },
					{ OrgAddressSchema.Constants.TableName, 5 },
					{ ProcessTaskRequiredSkillSchema.Constants.TableName, 4 },
					{ ProcessTaskExtraResourceSchema.Constants.TableName, 4 },
					{ ProcessTaskNotificationSchema.Constants.TableName, 63 },
					{ ProcessTasksSchema.Constants.TableName, 6 },
					{ ProcessWorkflowExceptionTypeSchema.Constants.TableName, 1 },
					{ StmALogSchema.Constants.TableName, 1 },
					{ UNDGDataItemSchema.Constants.TableName, 2 },
					{ JobContainerPenaltySchema.Constants.TableName, 2 },
					{ CusEntryHeaderSchema.Constants.TableName, 1 },
					{ QuarantineColsHeaderSchema.Constants.TableName, 1 }
				};

				form.FireValidateAllForTest();

				CombineAssertions("Check for table hits for validation are not higher than expected", () =>
				{
					foreach (var tableHitCount in planners.Factory.TableSelects)
					{
						var expectedMaxTableHits = 0;
						expectedMaxDbHits.TryGetValue(tableHitCount.TableName, out expectedMaxTableHits);
						AssertLessThanOrEqualTo(
							FormattableString.Invariant($"Table hits for {tableHitCount.TableName} ({tableHitCount.Value}) were greater than the expected number of hits {expectedMaxTableHits}"),
							tableHitCount.Value,
							expectedMaxTableHits);
					}

					AssertMaxDbHits(128, planners.Factory);
				});
			}
		}

		public void TestSearchPerformance_DBHits()
		{
			//setup
			TestDataSetupForDBHits();
			var searched = false;
			var factory = new BusinessObjectFactory();
			var planners = new CartageLegPlannerCollection(factory);
			using (var form = new ZForm(planners))
			{
				var filterControl = new CartageLegPlannerFilterControlForTest();
				ZGridControlBasher.ExposeAllColumnsInAllGrids(form, new NotificationBuffer());
				RepaintGrid(filterControl);
				var runSheetAllocatedFilter = (ModuleTextFilter)filterControl.FilterBusinessObject["RunSheet Allocated"];
				runSheetAllocatedFilter.IsActive = true;
				runSheetAllocatedFilter.Property = CartageLegFilterStripBusinessObject.CartageLegFilterConstants.Unallocated;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.PerformSearch += delegate
				{
					searched = true;
				};
				filterControl.FirePerformSearch();
				RepaintGrid(filterControl);
				Assert("Should have searched", searched);
				AssertEquals("Should have found some legs", true, planners[0].CartageLegs.Count > 0);
				var plannerFactory = planners[0].Factory;
				var dbHitsOnCusContainer = planners[0].Factory.TableSelects.SingleOrDefault(x => x.TableName == CusContainerSchema.Constants.TableName).Value;
				AssertLessThanOrEqualTo(string.Format("DB Hits on table CusContainer should less than or equal to 2. (Actual: {0})", dbHitsOnCusContainer), dbHitsOnCusContainer, 2);
				var dbHitsOnGenCustomAddOnValue = planners[0].Factory.TableSelects.SingleOrDefault(x => x.TableName == GenCustomAddOnValueSchema.Constants.TableName).Value;
				AssertEquals(string.Format("DB Hits on table GenCustomAddOnValue should less than or equal to 5. (Actual: {0})", dbHitsOnGenCustomAddOnValue), true, dbHitsOnGenCustomAddOnValue <= 5);
				var dbHitsOnJobContainerLegs = planners[0].Factory.TableSelects.SingleOrDefault(x => x.TableName == JobContainerLegsSchema.Constants.TableName).Value;
				AssertEquals(string.Format("DB Hits on table JobContainerLegs should less than or equal to 4. (Actual: {0})", dbHitsOnJobContainerLegs), true, dbHitsOnJobContainerLegs <= 4);
				var dbHitsOnJobContainer = planners[0].Factory.TableSelects.SingleOrDefault(x => x.TableName == JobContainerSchema.Constants.TableName).Value;
				AssertLessThanOrEqualTo(string.Format("DB Hits on table JobContainer should less than or equal to 2. (Actual: {0})", dbHitsOnJobContainerLegs), dbHitsOnJobContainerLegs, 2);
				var visibleRowsInGrid = filterControl.Grid.VisibleRowCount;
				var dbHitsOnViewJobCartageParents = planners[0].Factory.TableSelects.SingleOrDefault(x => x.TableName == ViewJobCartageParentsSchema.Constants.TableName).Value;
				AssertLessThanOrEqualTo(string.Format("DB Hits on table ViewJobCartageParents should be less than number of visible rows in grid ({0}), depending on how many rows have tried to calculate their parent job number and type. (Actual: {1})", visibleRowsInGrid, dbHitsOnViewJobCartageParents), dbHitsOnViewJobCartageParents, visibleRowsInGrid);
				var expectedDBHits = new Dictionary<string, int>()
				{ { CusContainerSchema.Constants.TableName, dbHitsOnCusContainer }, { GenCustomAddOnValueSchema.Constants.TableName, dbHitsOnGenCustomAddOnValue }, { GenCustomColumnDefinitionSchema.Constants.TableName, 1 }, { JobBookedCtgMoveSchema.Constants.TableName, 2 }, { JobCartageSchema.Constants.TableName, 2 }, { JobContainerLegsSchema.Constants.TableName, dbHitsOnJobContainerLegs }, { JobContainerSchema.Constants.TableName, dbHitsOnJobContainer }, { JobDeclarationSchema.Constants.TableName, 1 }, { JobHeaderSchema.Constants.TableName, 4 }, { JobShipmentSchema.Constants.TableName, 2 }, { ProcessTaskTemplateSchema.Constants.TableName, 1 }, { ViewJobCartageParentsSchema.Constants.TableName, dbHitsOnViewJobCartageParents }, { WhsDocketSchema.Constants.TableName, 1 }, };
				AssertDbHits(expectedDBHits, planners[0].Factory);
				filterControl.FirePerformSearch();
				AssertEquals("Should have found some legs", true, planners[0].CartageLegs.Count > 0);
				RepaintGrid(filterControl);
				AssertDbHits(expectedDBHits, planners[0].Factory);
				planners[0].Factory.ResetDatabaseLoadCount();
				form.WindowState = FormWindowState.Maximized;
				var maximumSize = form.Size;
				form.MinimumSize = maximumSize;
				form.MaximumSize = maximumSize;
				filterControl.Grid.MinimumSize = maximumSize;
				filterControl.Grid.MaximumSize = maximumSize;
				RepaintGrid(filterControl);
				var firstVisibleRowIndex = (int)typeof(ZGrid).GetField("firstVisibleRow", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(filterControl.Grid);
				visibleRowsInGrid = filterControl.Grid.VisibleRowCount;
				var numberOfRowsToChange = 5;
				planners[0].CartageLegs.Sort(JobContainerLegsSchema.Constants.JU_EW, ListSortDirection.Ascending);
				for (int visibleRowIndex = firstVisibleRowIndex; visibleRowIndex < firstVisibleRowIndex + numberOfRowsToChange; visibleRowIndex++)
				{
					planners[0].CartageLegs[visibleRowIndex].JU_PlannedPickupTime = ZDateTime.Now;
					planners[0].CartageLegs[visibleRowIndex].JU_EstimatedDeliveryTime = ZDateTime.Now;
					planners[0].CartageLegs[visibleRowIndex].JU_PickupTimeIn = ZDateTime.Now;
					planners[0].CartageLegs[visibleRowIndex].JU_DeliverTimeIn = ZDateTime.Now;
				}

				dbHitsOnViewJobCartageParents = planners[0].Factory.TableSelects.SingleOrDefault(x => x.TableName == ViewJobCartageParentsSchema.Constants.TableName).Value;
				AssertLessThanOrEqualTo(string.Format("DB Hits on table ViewJobCartageParents should be less than or equal to the number of visible rows in grid ({0}), depending on how many rows have tried to calculate their parent job number and type. (Actual: {1})", visibleRowsInGrid, dbHitsOnViewJobCartageParents), dbHitsOnViewJobCartageParents, visibleRowsInGrid);
				var expectedDBHits2 = new Dictionary<string, int>()
				{ { StmALogSchema.Constants.TableName, 8 }, { JobBookedCtgMoveSchema.Constants.TableName, 2 }, { JobContainerLegsSchema.Constants.TableName, 2 }, { ProcessTasksSchema.Constants.TableName, 4 }, { ViewJobCartageParentsSchema.Constants.TableName, dbHitsOnViewJobCartageParents }, };
				AssertDbHits(expectedDBHits2, planners[0].Factory);
			}
		}

		void TestDataSetupForDBHits()
		{
			int numberOfCartageJobsToCreated = 100;
			int numberOfLegsToCreatePerJob = 2;
			int numberOfprocessTaskTemplate = 20;
			int numberOfContainersPerCartage = 2;
			int numberOfJobServicesPerContainer = 2;
			var factory = new BusinessObjectFactory();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);
			var clientPK = helper.CreateClient("CLIENT");
			var whs = helper.CreateWarehouse("WHS", "A");
			var orderType = ObjectFactory.GetType<IWhsOrder>();
			for (int i = 0; i < numberOfprocessTaskTemplate; i++)
			{
				var processTaskTemplate = factory.NewWithValidTestData<ProcessTaskTemplate>();
				processTaskTemplate.P0_ProcessType = "LTL";
				processTaskTemplate.P0_IsActive = true;
			}

			for (int i = 0; i < numberOfCartageJobsToCreated; i++)
			{
				var cto = factory.NewWithValidTestData<OrgHeader>();
				var cfs = factory.NewWithValidTestData<OrgHeader>();
				var cyd = factory.NewWithValidTestData<OrgHeader>();
				var cartage = factory.NewWithValidTestData<CommonCartage>();
				cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportUnpack;
				cartage.FirstDocAddress.E2_OA_Address = cto.MainAddress.PK;
				cartage.SecondDocAddress.E2_OA_Address = cfs.MainAddress.PK;
				cartage.ThirdDocAddress.E2_OA_Address = cyd.MainAddress.PK;
				// create n legs
				var move = cartage.ContainerBookedMoves.AddNew();
				for (int l = 0; l < numberOfLegsToCreatePerJob; l++)
				{
					var leg = move.CartageLegs.AddNew();
					leg.JU_E2PickupAddressID = cto.PK;
					leg.JU_E2DeliveryAddressID = cfs.PK;
					leg.WorkflowItems.AddNew();
				}

				if (i % 30 == 0)
				{
					cartage.JJ_ConsignmentID = "ConsignID_Dec" + i;
					var declaration = (BusinessObject)factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
					cartage.JJ_ParentID = declaration.PK;
					cartage.JJ_ParentTableCode = declaration.TablePrefix;
				}
				else if (i % 20 == 0)
				{
					cartage.JJ_ConsignmentID = "ConsignID_Order" + i;
					var orderPK = helper.CreateWhsOrder(clientPK, whs.PK, "OR" + i, new NotificationBuffer());
					var order = (IWhsOrder)factory.Load(orderType, orderPK);
					cartage.JJ_ParentID = orderPK;
					cartage.JJ_ParentTableCode = "WD";
				}
				else
				{
					cartage.JJ_ConsignmentID = "ConsignID_Ship" + i;
					var shipment = Factory.NewWithValidTestData<CommonShipment>();
					cartage.JJ_ParentID = shipment.PK;
					cartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
					var packline1 = shipment.OuterPackLines.AddNew();
					var packline2 = shipment.OuterPackLines.AddNew();
					for (int c = 0; c < numberOfContainersPerCartage; c++)
					{
						var container = shipment.Consols.AddNew().Containers.AddNew();
						for (int s = 0; s < numberOfJobServicesPerContainer; s++)
						{
							var service = factory.New<JobService>();
							service.ES_ParentTableCode = JobContainerSchema.Constants.Prefix;
							service.ES_ParentID = container.PK;
						}

						container.AddPackLine(packline1);
						container.AddPackLine(packline2);
						var originConfirm = container.OriginConfirm;
						var destinationConfirm = container.DestinationConfirm;
						var address1 = container.DocAddresses.AddNew(DocAddressType.ConsignorPickupDeliveryAddress);
						var address2 = container.DocAddresses.AddNew(DocAddressType.ConsigneeAddress);
					}
				}
			}

			factory.Save();
		}

		static void RepaintGrid(CartageLegPlannerFilterControlForTest filterControl)
		{
			// scroll across the grid to bind all columns
			var gridPixelsToScroll = filterControl.Grid.Columns.Sum(c => c.ColumnStyle.Width);
			int horizontalScrollPosition = 0;
			filterControl.Grid.HorizontalScrollToOffset(0);
			while (horizontalScrollPosition < filterControl.Grid.HorizontalScrollBarMaximum)
			{
				int offset = Math.Min(filterControl.Grid.HorizontalScrollBarMaximum - horizontalScrollPosition, filterControl.Grid.ClientRectangle.Width);
				horizontalScrollPosition += offset;
				filterControl.Grid.HorizontalScrollToOffset(horizontalScrollPosition);
				System.Windows.Forms.Application.DoEvents();
			}
		}

		[StressTest, SnailTest, UseSnapshotProtection]
		public void TestNoFilterLoadCount_SlowOnSetupData()
		{
			var filterControl = new CartageLegPlannerFilterControlForTest();
			filterControl.Dock = DockStyle.Fill;
			filterControl.Grid.ExposeAllColumns();
			filterControl.Grid.ColorContextKey = "NO COLOR";
			var legPKs = CreateLegsInDBWithNewFactory(2, filterControl.MaximumAllowableQueriesPerSqlStatement / 2 + 1);
			var searched = false;
			var otherFactory = new BusinessObjectFactory();
			var planners = new CartageLegPlannerCollection(otherFactory);
			using (var form = new ZForm(planners))
			{
				AssertEquals("Should have no legs", 0, planners[0].CartageLegs.Count);
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.PerformSearch += delegate
				{
					searched = true;
				};
				filterControl.FirePerformSearch();
				Assert("Should have searched", searched);
				AssertEquals("Should not have found legs .. too many", 0, planners[0].CartageLegs.Count);
			}

			otherFactory.ResetDatabaseLoadCount();
			var query = new ZQuery(JobContainerLegsSchema.PK, legPKs);
			query.FetchOnlyFromLocalCache = true;
			AssertEquals("FUCKOVSKY", 0, otherFactory.Load<CommonCartageLeg>(query).Length);
		}

		List<ZGuid> CreateLegsInDBWithNewFactory(int numberOfCartageJobsToCreate, int numberOfLegsToCreatePerJob)
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();
			var cto = factory.NewWithValidTestData<OrgHeader>();
			var cfs = factory.NewWithValidTestData<OrgHeader>();
			var cyd = factory.NewWithValidTestData<OrgHeader>();
			for (int i = 0; i < numberOfCartageJobsToCreate; i++)
			{
				var cartage = factory.NewWithValidTestData<CommonCartage>();
				cartage.JJ_ConsignmentID = "ConsignID" + i;
				cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportUnpack;
				cartage.FirstDocAddress.E2_OA_Address = cto.MainAddress.PK;
				cartage.SecondDocAddress.E2_OA_Address = cfs.MainAddress.PK;
				cartage.ThirdDocAddress.E2_OA_Address = cyd.MainAddress.PK;
				var move = cartage.ContainerBookedMoves.AddNew();
				// create n legs
				for (int j = 0; j < numberOfLegsToCreatePerJob; j++)
				{
					var leg = move.CartageLegs.AddNew();
					leg.JU_E2PickupAddressID = cto.PK;
					leg.JU_E2DeliveryAddressID = cfs.PK;
					result.Add(leg.PK);
				}
			}

			factory.Save();
			return result;
		}

		protected override Form GetFormToBashCore()
		{
			return new CartageLegPlannerForm(Factory);
		}

		public void TestPerformance()
		{
			var now = ZDateTime.Now;
			CreateContainersDivotsAndCartage();
			var searched = false;
			var factory = new BusinessObjectFactory();
			factory.ResetDatabaseLoadCount();
			var planners = new CartageLegPlannerCollection(factory);
			using (var form = new ZForm(planners))
			{
				AssertEquals("Should have no legs", 0, planners[0].CartageLegs.Count);
				var filter = new CartageLegPlannerFilterControlForTest();
				form.Controls.Add(filter);
				filter.Grid.ExposeAllColumns();
				filter.Grid.ColorContextKey = "NO COLOR";
				form.Show();
				filter.PerformSearch += delegate
				{
					searched = true;
				};
				filter.FirePerformSearch();
				AssertEquals("Should have searched", true, searched);
				Assert("Should have found legs", planners[0].CartageLegs.Count > 0);
				RepaintGrid(filter);
				BashForm(form);
				// JobCartage: 4
				// JobContainerLegs: 4
				// CusContainer: 3
				// GenCustomAddOnValue: 2
				// JobBookedCtgMove: 2
				// LocalCartageJobType: 2
				// RefVessel: 2
				// GlbBranch: 1
				// GlbCompany: 1
				// EDIMessage: 1
				// GenCustomColumnDefinition: 1
				// JobContainer: 1
				// JobContainerPackPivot: 1
				// JobDocAddress: 1
				// JobHeader: 1
				// JobSailing: 1
				// JobTransportLegPackLineDivot: 1
				// JobVoyage: 1
				// JobVoyDestination: 1
				// JobVoyOrigin: 1
				// LocalCartageJobLegType: 1
				// LocalCartageJobOrg: 1
				// LocalCartageVehicleActivity: 1
				// ProcessTaskTemplate: 1
				// RefPackType: 1
				// RefUNLOCO: 1
				// StmData: 1
				// UNDGDataItem: 1
				//Hits: 42/41
				AssertMaxDbHits(44, planners.Factory);
			}

			planners = new CartageLegPlannerCollection(factory);
			using (var form = new ZForm(planners))
			{
				var filter = new CartageLegPlannerFilterControlForTest();
				form.Controls.Add(filter);
				filter.Grid.ExposeAllColumns();
				filter.Grid.ColorContextKey = "NO COLOR";
				form.Show();
				var plannedPickupFilter = (ModuleDateFilter)filter.FilterBusinessObject[CartageLegFilterStripBusinessObject.CartageLegFilterConstants.PlannedPickup];
				plannedPickupFilter.Property1 = now.AddDays(-1);
				plannedPickupFilter.Property2 = now.AddDays(1);
				plannedPickupFilter.IsActive = true;
				plannedPickupFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				searched = false;
				filter.PerformSearch += delegate
				{
					searched = true;
				};
				filter.FirePerformSearch();
				AssertEquals("Should have searched", true, searched);
				AssertEquals("Should have found no legs", 0, planners[0].CartageLegs.Count);
				AssertNotEquals(factory, form.BusinessEntity.Factory);
			}
		}

		static List<ZGuid> CreateContainersDivotsAndCartage()
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();
			for (int i = 0; i < 20; i++)
			{
				var cto = factory.NewWithValidTestData<OrgHeader>();
				var cfs = factory.NewWithValidTestData<OrgHeader>();
				var cyd = factory.NewWithValidTestData<OrgHeader>();
				var vessel = factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "vessel" + i;
				var voyage = factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "voyage" + i;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
				voyage.GenerateSailings();
				var cartage = factory.NewWithValidTestData<CommonCartage>();
				cartage.JJ_ConsignmentID = string.Format("T000001{0:00}", i);
				cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportUnpack;
				cartage.FirstDocAddress.E2_OA_Address = cto.MainAddress.PK;
				cartage.SecondDocAddress.E2_OA_Address = cfs.MainAddress.PK;
				cartage.ThirdDocAddress.E2_OA_Address = cyd.MainAddress.PK;
				cartage.JJ_JX_Sailing = voyage.Sailings[0].PK;
				var move = cartage.ContainerBookedMoves.AddNew();
				var container = move.Container;
				container.JC_ContainerNum = string.Format("TEST41000{0:00}", i);
				var leg1 = move.CartageLegs.AddNew();
				var leg2 = move.CartageLegs.AddNew();
				leg1.JU_E2PickupAddressID = cto.PK;
				leg1.JU_E2DeliveryAddressID = cfs.PK;
				leg2.JU_E2PickupAddressID = cfs.PK;
				leg2.JU_E2DeliveryAddressID = cyd.PK;
				result.Add(leg1.PK);
				result.Add(leg2.PK);
			}

			factory.Save();
			return result;
		}

		public void TestDragAndDropHandlersForLegPlannerFilterControl()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			var onDragEnter = typeof(ZGrid).GetMethod("OnDragEnter", BindingFlags.Instance | BindingFlags.NonPublic);
			var onDragDrop = typeof(ZGrid).GetMethod("OnDragDrop", BindingFlags.Instance | BindingFlags.NonPublic);
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			var move1 = cartage.LooseBookedMoves.AddNew();
			var move2 = cartage.LooseBookedMoves.AddNew();
			var leg1 = move1.CartageLegs.AddNew();
			var leg2 = move2.CartageLegs.AddNew();
			Factory.Save();
			var driver = Factory.New<GlbStaff>();
			driver.GS_FullName = "truck driver";
			driver.GS_IsActive = true;
			driver.GS_IsResource = false;
			var driverGroup = Factory.New<GlbGroup>();
			driver.Groups.Add(driverGroup);
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, driverGroup.PK.ToGuid());
			using (CartageLegPlannerForm plannerForm = new CartageLegPlannerForm(Factory))
			{
				plannerForm.Show();
				CartageLegPlannerFilterControl form = plannerForm.cartageLegsFilterStripUserControl as CartageLegPlannerFilterControl;
				plannerForm.cartageLegsFilterStripUserControl.FirePerformSearch();
				Point point = form.Grid.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(30, 30));
				var planners = (CartageLegPlannerCollection)plannerForm.DataSource;
				var n = new ArrayList();
				n.Add(driver);
				var d = new DataObject(n);
				var e = new DragEventArgs(d, 0, point.X, point.Y, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);
				onDragEnter.Invoke(form.Grid, new object[] { e });
				AssertEquals(DragDropEffects.Copy | DragDropEffects.Scroll, e.Effect);
				onDragDrop.Invoke(form.Grid, new object[] { e });
				AssertEquals(planners[0].CartageLegs[0].QuickGSDriver, driver.GS_Code);
				var truck = Factory.New<RefEquipment>();
				truck.RQ_IsVehicle = true;
				truck.RQ_ShortCode = "sc";
				n = new ArrayList();
				n.Add(truck);
				d = new DataObject(n);
				e = new DragEventArgs(d, 0, point.X, point.Y, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);
				onDragEnter.Invoke(form.Grid, new object[] { e });
				AssertEquals(DragDropEffects.Copy | DragDropEffects.Scroll, e.Effect);
				onDragDrop.Invoke(form.Grid, new object[] { e });
				AssertEquals(truck.PK, planners[0].CartageLegs[0].QuickRQTruck);
			}
		}

		public void TestAutoRefresh()
		{
			var now = ZDateTime.Today;
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			var move1 = cartage.LooseBookedMoves.AddNew();
			var move2 = cartage.LooseBookedMoves.AddNew();
			var leg1 = move1.CartageLegs.AddNew();
			leg1.JU_PlannedPickupTime = now;
			var leg2 = move2.CartageLegs.AddNew();
			leg2.JU_PlannedPickupTime = ZDateTime.Empty;
			Factory.Save();
			AutoRefreshManager.Instance.SetAutoRefreshTimeOut(ModuleIDs.CartageLegPlanner, true, 0);
			using (var form = new CartageLegPlannerForm(Factory))
			{
				var planners = (CartageLegPlannerCollection)form.CurrentDataItem;
				AssertEquals("Should have no legs", 0, planners[0].CartageLegs.Count);
				var filterControl = (CartageLegPlannerFilterControl)form.cartageLegsFilterStripUserControl;
				var filterBizo = filterControl.FilterBusinessObject;
				form.Show();
				var plannedPickupFilter = (ModuleDateFilter)filterBizo[CartageLegFilterStripBusinessObject.CartageLegFilterConstants.PlannedPickup];
				plannedPickupFilter.Property1 = now.AddDays(-1);
				plannedPickupFilter.Property2 = now.AddDays(1);
				plannedPickupFilter.IsActive = true;
				plannedPickupFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				AssertEquals("Should have no legs", 0, planners[0].CartageLegs.Count);
				Assert(!filterControl.AutoRefreshTimer.Enabled);
				AssertEquals(CartageLegPlannerFilterControl.AutoRefreshStatusType.Disabled, filterControl.AutoRefreshStatus);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				filterControl.AutoRefreshMenuItem.PerformClick();
				filterControl.AutoRefreshMenuItem.PerformClick();
				AssertEquals(1000, filterControl.AutoRefreshTimer.Interval);
				Assert(filterControl.AutoRefreshTimer.Enabled);
				AssertEquals(CartageLegPlannerFilterControl.AutoRefreshStatusType.Running, filterControl.AutoRefreshStatus);
				AssertEquals("Should have no legs", 0, planners[0].CartageLegs.Count);
				Thread.Sleep(1000);
				Application.DoEvents();
				AssertEquals("Should have found legs", 1, planners[0].CartageLegs.Count);
				AssertEquals("Should have found 1st leg", leg1.PK, planners[0].CartageLegs[0].PK);
			}
		}

		public void TestAutoRefreshTimerShouldBeDisabledDuringDbUpgrade()
		{
			var now = ZDateTime.Today;
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			var move1 = cartage.LooseBookedMoves.AddNew();
			var move2 = cartage.LooseBookedMoves.AddNew();
			var leg1 = move1.CartageLegs.AddNew();
			leg1.JU_PlannedPickupTime = now;
			var leg2 = move2.CartageLegs.AddNew();
			leg2.JU_PlannedPickupTime = ZDateTime.Empty;
			Factory.Save();
			AutoRefreshManager.Instance.SetAutoRefreshTimeOut(ModuleIDs.CartageLegPlanner, true, 0);
			using (DbEnv.Instance.DisableTimerDuringDbUpgrade())
			using (var form = new CartageLegPlannerForm(Factory))
			{
				var planners = (CartageLegPlannerCollection)form.CurrentDataItem;
				AssertEquals("Should have no legs", 0, planners[0].CartageLegs.Count);
				var filterControl = (CartageLegPlannerFilterControl)form.cartageLegsFilterStripUserControl;
				var filterBizo = filterControl.FilterBusinessObject;
				form.Show();
				var plannedPickupFilter = (ModuleDateFilter)filterBizo[CartageLegFilterStripBusinessObject.CartageLegFilterConstants.PlannedPickup];
				plannedPickupFilter.Property1 = now.AddDays(-1);
				plannedPickupFilter.Property2 = now.AddDays(1);
				plannedPickupFilter.IsActive = true;
				plannedPickupFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				AssertEquals("Should have no legs", 0, planners[0].CartageLegs.Count);
				Assert(!filterControl.AutoRefreshTimer.Enabled);
				AssertEquals(CartageLegPlannerFilterControl.AutoRefreshStatusType.Disabled, filterControl.AutoRefreshStatus);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				filterControl.AutoRefreshMenuItem.PerformClick();
				filterControl.AutoRefreshMenuItem.PerformClick();
				AssertEquals(1000, filterControl.AutoRefreshTimer.Interval);
				Assert(filterControl.AutoRefreshTimer.Enabled);
				AssertEquals(CartageLegPlannerFilterControl.AutoRefreshStatusType.Running, filterControl.AutoRefreshStatus);
				AssertEquals("Should have no legs", 0, planners[0].CartageLegs.Count);
				Thread.Sleep(1000);
				Application.DoEvents();
				AssertEquals("Should not have refreshed", 0, planners[0].CartageLegs.Count);
			}
		}

		public void TestSearchWithChangesInAnotherEnterpriseInstance()
		{
			var now = ZDateTime.Today;
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			var move1 = cartage.LooseBookedMoves.AddNew();
			var leg1 = move1.CartageLegs.AddNew();
			leg1.JU_PlannedPickupTime = now;
			var move2 = cartage.LooseBookedMoves.AddNew();
			var leg2 = move2.CartageLegs.AddNew();
			leg2.JU_PlannedPickupTime = ZDateTime.Empty;
			Factory.Save();
			var searched = false;
			var planners = new CartageLegPlannerCollection(Factory);
			using (var form = new ZForm(planners))
			{
				AssertEquals("Should have no legs", 0, planners[0].CartageLegs.Count);
				var filter = new CartageLegPlannerFilterControlForTest();
				var plannedPickupFilter = (ModuleDateFilter)filter.FilterBusinessObject[CartageLegFilterStripBusinessObject.CartageLegFilterConstants.PlannedPickup];
				plannedPickupFilter.Property1 = now.AddDays(-1);
				plannedPickupFilter.Property2 = now.AddDays(1);
				plannedPickupFilter.IsActive = true;
				plannedPickupFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				form.Controls.Add(filter);
				form.Show();
				filter.PerformSearch += delegate
				{
					searched = true;
				};
				filter.FirePerformSearch();
				Assert("Should have searched", searched);
				var leg1InPlannerFactory = planners.Factory.Load<CommonCartageLeg>(leg1.PK);
				AssertContainsExactElementsInAnyOrder("Should have found the first leg only.", leg1InPlannerFactory, planners[0].CartageLegs);
				searched = false;
				filter.FirePerformSearch();
				Assert("Should have searched again", searched);
				leg1InPlannerFactory = planners.Factory.Load<CommonCartageLeg>(leg1.PK);
				AssertContainsExactElementsInAnyOrder("Should have found the first leg only (again).", leg1InPlannerFactory, planners[0].CartageLegs);
				// alter legs, but in another instance with no data refresh
				var leg1_factory2 = factory2.Load<CommonCartageLeg>(leg1.PK);
				var leg2_factory2 = factory2.Load<CommonCartageLeg>(leg2.PK);
				leg1_factory2.JU_PlannedPickupTime = now;
				leg2_factory2.JU_PlannedPickupTime = now;
				factory2.Save();
				AssertEquals("Precondition - leg1 in first factory shouldn't be affected yet.", leg1.JU_PlannedPickupTime, now);
				AssertEquals("Precondition - leg2 in first factory shouldn't be affected yet.", leg2.JU_PlannedPickupTime, ZDateTime.Empty);
				searched = false;
				filter.FirePerformSearch();
				Assert("Should have searched again", searched);
				leg1InPlannerFactory = planners.Factory.Load<CommonCartageLeg>(leg1.PK);
				var leg2InPlannerFactory = planners.Factory.Load<CommonCartageLeg>(leg2.PK);
				AssertEquals("leg1 in first factory shouldn't be affected yet", leg1InPlannerFactory.JU_PlannedPickupTime, now);
				AssertEquals("leg2 in first factory shouldn't be affected yet", leg2InPlannerFactory.JU_PlannedPickupTime, now);
				AssertContainsExactElementsInAnyOrder("Should have found both legs.", new CommonCartageLeg[] { leg1InPlannerFactory, leg2InPlannerFactory }, planners[0].CartageLegs);
				leg1_factory2.JU_PlannedPickupTime = ZDateTime.Empty;
				leg2_factory2.JU_PlannedPickupTime = now;
				factory2.Save();
				searched = false;
				filter.FirePerformSearch();
				Assert("Should have searched again", searched);
				leg2InPlannerFactory = planners.Factory.Load<CommonCartageLeg>(leg2.PK);
				AssertContainsExactElementsInAnyOrder("Should have found 2nd leg.", new CommonCartageLeg[] { leg2InPlannerFactory }, planners[0].CartageLegs);
				AssertEquals("leg2", leg2InPlannerFactory.JU_PlannedPickupTime, now);
				leg1_factory2.JU_PlannedPickupTime = now;
				leg2_factory2.JU_PlannedPickupTime = now;
				leg1_factory2.JU_PickupTimeIn = now.AddDays(2);
				leg2_factory2.JU_PickupTimeIn = now.AddDays(3);
				factory2.Save();
				searched = false;
				filter.FirePerformSearch();
				Assert("Should have searched again", searched);
				leg1InPlannerFactory = planners.Factory.Load<CommonCartageLeg>(leg1.PK);
				leg2InPlannerFactory = planners.Factory.Load<CommonCartageLeg>(leg2.PK);
				AssertContainsExactElementsInAnyOrder("Should have found both legs.", new CommonCartageLeg[] { leg1InPlannerFactory, leg2InPlannerFactory }, planners[0].CartageLegs);
				AssertEquals("leg1", leg1InPlannerFactory.JU_PickupTimeIn, now.AddDays(2));
				AssertEquals("leg2", leg2InPlannerFactory.JU_PickupTimeIn, now.AddDays(3));
			}
		}

		public void TestSearchWithChangesInAnotherEnterpriseInstance_Container()
		{
			var now = ZDateTime.Today;
			var otherEnterpriseFactory = new BusinessObjectFactory();
			otherEnterpriseFactory.RefreshEnabled = false;
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_EmptyCNEtoCYD, 1);
			var move = cartage.BookedMovesCollection[0];
			var leg = move.CartageLegs[0];
			var container = move.Container;
			Factory.Save();
			var searched = false;
			var planners = new CartageLegPlannerCollection(Factory);
			using (var form = new ZForm(planners))
			{
				AssertEquals("Should have no legs", 0, planners[0].CartageLegs.Count);
				var filter = new CartageLegPlannerFilterControlForTest();
				var containerNumberFilter = (ModuleTextFilter)filter.FilterBusinessObject["Container #"];
				containerNumberFilter.Property = "";
				containerNumberFilter.IsActive = true;
				form.Controls.Add(filter);
				form.Show();
				filter.PerformSearch += delegate
				{
					searched = true;
				};
				filter.FirePerformSearch();
				Assert("Should have searched", searched);
				var leg1InPlannerFactory = planners.Factory.Load<CommonCartageLeg>(leg.PK);
				AssertContainsExactElementsInAnyOrder("Should have found the first leg only.", leg1InPlannerFactory, planners[0].CartageLegs);
				searched = false;
				filter.FirePerformSearch();
				Assert("Should have searched again", searched);
				leg1InPlannerFactory = planners.Factory.Load<CommonCartageLeg>(leg.PK);
				AssertContainsExactElementsInAnyOrder("Should have found the first leg only (again).", leg1InPlannerFactory, planners[0].CartageLegs);
				var poke = leg1InPlannerFactory.Container;
				// alter legs, but in another instance with no data refresh
				var container_otherFactory = otherEnterpriseFactory.Load<CommonContainer>(container.PK);
				container_otherFactory.JC_ContainerNum = "HEYA";
				otherEnterpriseFactory.Save();
				AssertEquals("Precondition - leg1 in first factory shouldn't be affected yet.", "", leg1InPlannerFactory.Container.JC_ContainerNum);
				searched = false;
				containerNumberFilter.Property = "Uhmm";
				filter.FirePerformSearch();
				Assert("Should have searched again", searched);
				leg1InPlannerFactory = planners.Factory.Load<CommonCartageLeg>(leg.PK);
				AssertEquals("Should not have found leg.", 0, planners[0].CartageLegs.Count);
				containerNumberFilter.Property = "HEYA";
				searched = false;
				filter.FirePerformSearch();
				Assert("Should have searched again", searched);
				leg1InPlannerFactory = planners.Factory.Load<CommonCartageLeg>(leg.PK);
				AssertEquals("container in first factory should have new number", "HEYA", leg1InPlannerFactory.Container.JC_ContainerNum);
				AssertContainsExactElementsInAnyOrder("Should have found leg.", new CommonCartageLeg[] { leg1InPlannerFactory }, planners[0].CartageLegs);
			}
		}

		public void TestShouldPerformSearch()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			CommonBookedCtgMove move1 = cartage.LooseBookedMoves.AddNew();
			CommonBookedCtgMove move2 = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg leg1 = move1.CartageLegs.AddNew();
			leg1.JU_PlannedPickupTime = ZDateTime.Today;
			CommonCartageLeg leg2 = move2.CartageLegs.AddNew();
			leg2.JU_PlannedPickupTime = ZDateTime.Today;
			Factory.Save();
			bool searched = false;
			var planners = new CartageLegPlannerCollection(Factory);
			using (var form = new ZForm(planners))
			{
				AssertEquals("Should have no legs", 0, planners[0].CartageLegs.Count);
				CartageLegPlannerFilterControlForTest filter = new CartageLegPlannerFilterControlForTest();
				form.Controls.Add(filter);
				form.Show();
				filter.PerformSearch += delegate
				{
					searched = true;
				};
				filter.FirePerformSearch();
				Assert("Should have searched", searched);
				Assert("Should have found legs", planners[0].CartageLegs.Count > 0);
				searched = false;
				filter.FirePerformSearch();
				Assert("Should have searched again", searched);
				Assert("Should have found legs", planners[0].CartageLegs.Count > 0);
				searched = false;
				planners[0].CartageLegs[0].JU_DeliverySignedFor = "Bob";
				filter.FirePerformSearch();
				Assert("Should NOT have searched cause in error", !searched);
				Assert("Should still have legs visible though", planners[0].CartageLegs.Count > 0);
			}
		}

		public void TestPerformSearch()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonCartage cartage = newFactory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			CommonBookedCtgMove move1 = cartage.LooseBookedMoves.AddNew();
			CommonBookedCtgMove move2 = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg leg1 = move1.CartageLegs.AddNew();
			CommonCartageLeg leg2 = move2.CartageLegs.AddNew();
			ZDateTime now = ZDateTime.Now;
			leg1.JU_PlannedPickupTime = now.AddDays(5);
			leg2.JU_PlannedPickupTime = now.AddDays(5);
			newFactory.Save();
			bool searched = false;
			var planners = new CartageLegPlannerCollection(Factory);
			using (var form = new ZForm(planners))
			{
				AssertEquals("Should have no legs", 0, planners[0].CartageLegs.Count);
				CartageLegPlannerFilterControlForTest filter = new CartageLegPlannerFilterControlForTest();
				form.Controls.Add(filter);
				form.Show();
				var plannedPickupFilter = (ModuleDateFilter)filter.FilterBusinessObject[CartageLegFilterStripBusinessObject.CartageLegFilterConstants.PlannedPickup];
				plannedPickupFilter.Property1 = now.AddDays(1);
				plannedPickupFilter.Property2 = now.AddDays(3);
				plannedPickupFilter.IsActive = true;
				plannedPickupFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				filter.PerformSearch += delegate
				{
					searched = true;
				};
				filter.FirePerformSearch();
				Assert("Should have searched", searched);
				AssertEquals("Should have found 0 legs", 0, planners[0].CartageLegs.Count);
				searched = false;
				plannedPickupFilter.Property1 = now.AddDays(4);
				plannedPickupFilter.Property2 = now.AddDays(6);
				filter.FirePerformSearch();
				Assert("Should have searched again", searched);
				AssertEquals("Should have found 2 legs", 2, planners[0].CartageLegs.Count);
			}
		}

		public void TestPerformSearchKeepsExistingSort()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_AirExport, 3);
			var leg1 = cartage.CartageLegs[0];
			var leg2 = cartage.CartageLegs[1];
			var leg3 = cartage.CartageLegs[2];
			var now = ZDateTime.Now;
			leg1.JU_PickupTimeIn = now.AddDays(3);
			leg2.JU_PickupTimeIn = now.AddDays(1);
			leg3.JU_PickupTimeIn = now.AddDays(5);
			Factory.Save();
			var planners = new CartageLegPlannerCollection(Factory);
			using (var form = new ZForm(planners))
			{
				var filter = new CartageLegPlannerFilterControlForTest();
				form.Controls.Add(filter);
				form.Show();
				MimicUserClickingSort(filter.Grid, leg1.JU_PickupTimeInInfo.PropertyDescriptor, ListSortDirection.Ascending);
				filter.FirePerformSearch();
				AssertGridContainsLegsInOrder(filter.Grid, new[] { leg2, leg1, leg3 });
				MimicUserClickingSort(filter.Grid, leg1.JU_PickupTimeInInfo.PropertyDescriptor, ListSortDirection.Descending);
				AssertGridContainsLegsInOrder(filter.Grid, new[] { leg3, leg1, leg2 });
				filter.FirePerformSearch();
				AssertGridContainsLegsInOrder(filter.Grid, new[] { leg3, leg1, leg2 });
			}
		}

		public void TestPerformSearchWithTooLargeSQLQueryDoesNotThrowAndDisplaysUserMessage()
		{
			var planners = new CartageLegPlannerCollection(Factory);
			using (var form = new ZForm(planners))
			{
				AssertEquals("Should have no legs", 0, planners[0].CartageLegs.Count);

				CartageLegPlannerFilterControlForTest filter = new CartageLegPlannerFilterControlForTest();
				form.Controls.Add(filter);
				form.Show();

				var containerEmptyReturnedOnStrip = filter.FilterBusinessObject.AddFilterStrip<ModuleDateFilter>("Container Empty Returned On");
				containerEmptyReturnedOnStrip.PropertySearch = "Has No Date";
				containerEmptyReturnedOnStrip.IsActive = true;

				var scheduleETAStrip = filter.FilterBusinessObject.AddFilterStrip<ModuleDateFilter>("Schedule ETA");
				scheduleETAStrip.PropertySearch = "Date range";
				scheduleETAStrip.Property1 = new ZDateTime(2021, 1, 1);
				scheduleETAStrip.Property2 = ZDateTime.Empty;
				scheduleETAStrip.IsActive = true;

				var portTransportJobTypeStrip = filter.FilterBusinessObject.AddFilterStrip<ModuleTextFilter>("Port Transport Job Type");
				portTransportJobTypeStrip.Property = "ISFC";
				portTransportJobTypeStrip.IsActive = true;

				const int NumberOfOrgsToSearchFor = 95;
				var orgHeaders = new OrgHeader[NumberOfOrgsToSearchFor];
				for (var orgStripNumber = 1; orgStripNumber <= NumberOfOrgsToSearchFor; orgStripNumber++)
				{
					var org = Factory.NewWithValidTestData<OrgHeader>();
					org.OH_Code = "LOCALORG" + orgStripNumber.ToString("D2");
					org.OH_FullName = "Local Client Org " + orgStripNumber.ToString();
					orgHeaders[orgStripNumber - 1] = org;
				}
				Factory.Save();
				foreach (var orgHeader in orgHeaders)
				{
					var orgStrip = filter.FilterBusinessObject.AddFilterStrip<ModuleGuidFilter>("Local Client");
					orgStrip.Property = orgHeader.PK;
					orgStrip.IsActive = true;
					orgStrip.GroupOrCategory = FilterOrCategory.Red;
				}

				AssertNoExceptionThrown("Creating a filter where the SQL is too large to run should not throw an exception", () => filter.FirePerformSearch());
				AssertEquals("Your query is too complicated, please simplify your search conditions.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void MimicUserClickingSort(ZFilterGrid grid, PropertyDescriptor property, ListSortDirection direction)
		{
			var listView = (IBindingListView)grid.ListManager.List;
			listView.ApplySort(new ListSortDescriptionCollection(new[] { new ListSortDescription(property, direction) }));
		}

		void AssertGridContainsLegsInOrder(ZFilterGrid grid, CommonCartageLeg[] expectedLegs)
		{
			var legsInGrid = grid.List.Cast<CommonCartageLeg>();
			AssertEquals($"Should have found {expectedLegs.Length} legs in Grid List.", expectedLegs.Length, legsInGrid.Count());
			AssertEquals("First Leg first expected Leg.", expectedLegs[0].PK, legsInGrid.ElementAt(0).PK);
			AssertEquals("Second Leg second expected Leg.", expectedLegs[1].PK, legsInGrid.ElementAt(1).PK);
			AssertEquals("Third Leg third expected Leg.", expectedLegs[2].PK, legsInGrid.ElementAt(2).PK);
		}

		public void TestDoubleClick()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			var move1 = cartage.LooseBookedMoves.AddNew();
			var leg1 = move1.CartageLegs.AddNew();
			leg1.JU_PlannedPickupTime = ZDateTime.Today;
			var move2 = cartage.LooseBookedMoves.AddNew();
			var leg2 = move2.CartageLegs.AddNew();
			leg2.JU_PlannedPickupTime = ZDateTime.Today;
			Factory.Save();
			var searched = false;
			var planners = new CartageLegPlannerCollection(Factory);
			using (var form = new ZForm(planners))
			{
				AssertEquals("Should have no legs", 0, planners[0].CartageLegs.Count);
				var filter = new CartageLegPlannerFilterControlForTest();
				form.Controls.Add(filter);
				form.Show();
				filter.PerformSearch += delegate
				{
					searched = true;
				};
				filter.FirePerformSearch();
				Assert("Should have searched", searched);
				Assert("Should have found legs", planners[0].CartageLegs.Count > 0);
				int x = 22;
				int y = 27;
				AssertEquals("Should have found ColumnResize", DataGrid.HitTestType.RowHeader, filter.Grid.HitTest(x, y).Type);
				typeof(Control).GetMethod("OnMouseDown", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(filter.Grid, new object[] { new MouseEventArgs(MouseButtons.Left, 2, x, y, 0) });
				AssertNotNull("Double click on RowHeader should open Local Transport Job Form, Controller should be null as no action has taken place.", filter.CartageForm_ForTesting);
				filter.CartageForm_ForTesting.Dispose();
			}
		}

		public void TestOpenTransportJob()
		{
			Env.Security.TransportJobCRMSecurity.DisableCRMSecurityForTesting(false);
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			CommonBookedCtgMove move1 = cartage.LooseBookedMoves.AddNew();
			CommonBookedCtgMove move2 = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg leg1 = move1.CartageLegs.AddNew();
			leg1.JU_PlannedPickupTime = ZDateTime.Today;
			CommonCartageLeg leg2 = move2.CartageLegs.AddNew();
			leg2.JU_PlannedPickupTime = ZDateTime.Today;
			Factory.Save();
			bool searched = false;
			var planners = new CartageLegPlannerCollection(Factory);
			using (var form = new ZForm(planners))
			{
				AssertEquals("Should have no legs", 0, planners[0].CartageLegs.Count);
				CartageLegPlannerFilterControlForTest filter = new CartageLegPlannerFilterControlForTest();
				form.Controls.Add(filter);
				form.Show();
				filter.PerformSearch += delegate
				{
					searched = true;
				};
				filter.FirePerformSearch();
				Assert("Should have searched", searched);
				Assert("Should have found legs", planners[0].CartageLegs.Count > 0);
				filter.Grid.Select(0);
				MenuItem menuItem = filter.Grid.ContextMenu.MenuItems.FindByText("Open Transport Job");
				AssertNotNull(menuItem);
				menuItem.PerformClick();
				AssertNotNull(filter.cartageForm);
				filter.cartageForm.Dispose();
				var cartageController = ZControllerFactory.Create(ControllerIDs.Cartage);
				cartageController.GetCheckPointForEdit(leg1).IsAllowed = false;
				cartageController.GetCheckPointForView(leg1).IsAllowed = false;
				cartageController.GetCheckPointForEdit(leg2).IsAllowed = false;
				cartageController.GetCheckPointForView(leg2).IsAllowed = false;
				menuItem.PerformClick();
				AssertNull(filter.cartageForm);
			}
		}

		[StressTest, SnailTest]
		public void TestHugeQuery_SlowOnSetupData()
		{
			var now = ZDateTime.Now;
			var newFactory = new BusinessObjectFactory();
			for (int i = 0; i < 5; i++)
			{
				var cartage = newFactory.New<CommonCartage>();
				for (int j = 0; j < 200; j++)
				{
					var move = cartage.LooseBookedMoves.AddNew();
					var leg = move.CartageLegs.AddNew();
					leg.JU_PlannedPickupTime = now.AddDays(5);
				}
			}

			newFactory.Save();
			bool searched = false;
			var planners = new CartageLegPlannerCollection(Factory);
			using (var form = new ZForm(planners))
			{
				AssertEquals("Should have no legs", 0, planners[0].CartageLegs.Count);
				var filter = new CartageLegPlannerFilterControlForTest();
				form.Controls.Add(filter);
				form.Show();
				var plannedPickupFilter = (ModuleDateFilter)filter.FilterBusinessObject[CartageLegFilterStripBusinessObject.CartageLegFilterConstants.PlannedPickup];
				plannedPickupFilter.Property1 = now.AddDays(4);
				plannedPickupFilter.Property2 = now.AddDays(6);
				plannedPickupFilter.IsActive = true;
				plannedPickupFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				filter.PerformSearch += delegate
				{
					searched = true;
				};
				filter.FirePerformSearch();
				Assert("Should have searched again", searched);
				AssertEquals("Should have found 1000 legs", 1000, planners[0].CartageLegs.Count);
			}
		}

		public void TestWorkflowCustomFields()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_AirExport, 1);
			var template = Helper.CreateWorkflowTemplate(WorkflowDescriptors.CartageLegWorkflowDescriptorCode);
			Helper.AddCustomField(template, "stringField", AddOnColumnDataType.Codes.String);
			Helper.AddCustomField(template, "intField", AddOnColumnDataType.Codes.Integer);
			Helper.AddCustomField(template, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
			Helper.AddCustomField(template, "boolField", AddOnColumnDataType.Codes.Boolean);
			Factory.Save();
			var planner = new CartageLegPlanner(Factory);
			using (var control = new CartageLegPlannerFilterControl())
			{
				var form = new ZForm(new CartageLegPlannerCollection(Factory));
				form.Controls.Add(control);
				form.Show();
				control.FirePerformSearch();
				AssertGridContainsCustomField(control.Grid, "stringField", typeof(ZTextBoxColumnStyleInfo));
				AssertGridContainsCustomField(control.Grid, "intField", typeof(ZTextBoxColumnStyleInfo));
				AssertGridContainsCustomField(control.Grid, "dateTimeField", typeof(ZTextBoxColumnStyleInfo));
				AssertGridContainsCustomField(control.Grid, "boolField", typeof(ZCheckBoxColumnStyleInfo));
				form.Dispose();
			}
		}

		void AssertGridContainsCustomField(ZGrid grid, ZString name, Type type)
		{
			bool wasFound = false;
			foreach (ZGridColumnInfo column in grid.ColumnStyles)
			{
				if (column.Caption == name)
				{
					wasFound = true;
					AssertContains(string.Format("__{0}__prop", name.ToUpperInvariant()), column.ColumnName);
					AssertEquals("Workflow Custom Fields", column.GroupName.Caption);
					AssertEquals(true, type.IsAssignableFrom(column.GetType()));
					AssertEquals(true, column.IsVisible);
				}
			}

			AssertEquals(name + " custom field was not found in the grid", true, wasFound);
		}

		public void TestGrid()
		{
			using (CartageLegPlannerFilterControl control = new CartageLegPlannerFilterControl())
			{
				AssertEquals(typeof(ZFilterGrid), control.Grid.GetType());
				Assert(string.IsNullOrEmpty(control.Grid.GridId));
				AssertEquals(nameof(ModuleId.CartageLegPlanner), control.Grid.ModuleIDName);
			}
		}

		public void TestNewZFilterStripReturnsCartageModuleStrip()
		{
			using (var filter = new CartageLegPlannerFilterControlForTest())
			using (var filterStrip = filter.AddNewFilterStrip())
			{
				AssertType(typeof(CartageModuleStrip), filterStrip);
			}
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return testHelper ?? (testHelper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper testHelper;

		public class CartageLegPlannerFilterControlForTest : CartageLegPlannerFilterControl
		{
			public CartageLegPlannerFilterControlForTest() : base()
			{
			}

			public new int MaximumAllowableQueriesPerSqlStatement
			{
				get
				{
					return base.MaximumAllowableQueriesPerSqlStatement;
				}
			}

			//MaximumAllowableQueriesPerSqlStatement
			internal bool ShouldPerformSearchExposed
			{
				get
				{
					return ShouldPerformSearch();
				}
			}
		}
	}
}
