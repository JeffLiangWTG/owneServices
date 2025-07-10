using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class OrderFilterControlTest : WhsFilterControlDBHitsTestCase<WhsOrderCollection, OrderFilterBusinessObject>
	{
		#region TestNewZFilterStrip

		public void TestNewZFilterStrip()
		{
			var collection = new WhsOrderCollection(Factory);
			var filterBizO = new OrderFilterBusinessObject();
			using (var filterControl = GetNewFilterControl(collection, filterBizO))
			{
				var strip = filterBizO.FilterStrips.AddNew();
				using (var filterStrip = filterControl.AddFilterStrip(strip))
				{
					AssertType<WhsWorkflowFilterStrip>(filterStrip);
				}
			}
		}

		#endregion

		#region Column Initialisation

		public void TestProfitLossReasonColumn()
		{
			using (var filterControl = GetNewFilterStripControl())
			{
				var profitLossReason = nameof(WhsOrder.JobHeader) + "+" + nameof(WhsOrder.JobHeader.JH_ProfitLossReasonCode);
				var column = filterControl.FilteredGrid.GetColumnStyle(profitLossReason);
				AssertNotNull("Profit/Loss Reason column", column);
				Assert("Profit/Loss Reason column should be hidden by default", !column.IsVisible);
			}
		}

		public void TestMarginColumn()
		{
			using (var filterControl = GetNewFilterStripControl())
			{
				var margin = nameof(WhsOrder.JobHeader) + "+" + nameof(WhsOrder.JobHeader.JH_TotalProfitRevenueMargin);
				var column = filterControl.FilteredGrid.GetColumnStyle(margin);
				AssertNotNull("Margin% column", column);
				Assert("Margin% column should be hidden by default", !column.IsVisible);
			}
		}

		public void TestDistributionCentreColumn()
		{
			using (var filterControl = GetNewFilterStripControl())
			{
				var column = filterControl.FilteredGrid.GetColumnStyle(nameof(WhsOrder.DistributionCentreNameOrPK));
				AssertNotNull("Distribution Center column", column);
				Assert("Distribution Center column should be shown by default", column.IsVisible);
			}
		}

		#endregion

		#region GetNewFilterStripControl

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var orders = new WhsOrderCollection(Factory);
			var filterBO = new OrderFilterBusinessObject();
			return new OrderFilterControl(orders, filterBO);
		}

		#endregion

		#region WorkflowDescriptorCode

		protected override string WorkflowDescriptorCode => WorkflowDescriptors.WhsOrderWorkflowDescriptorCode;

		#endregion

		WhsDocket SetupForDocketWithLine(ZGuid whsPK, ZGuid clientPK, ZGuid partPK, ZGuid salesChannelPK, ZGuid distributionCentrePK, string postfix)
		{
			var order = Factory.New<WhsOrder>();
			order.WD_OH_Client = clientPK;
			order.WD_WW_Whs = whsPK;
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneeAddressPK = order.Client.MainAddress.PK;
			order.NotificationManager.Push(Notify);
			order.WD_ExternalReference = "ORD" + postfix;
			order.ConsigneePK = order.Client.PK;
			order.ConsigneeAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			order.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			order.WD_RequiredDate = ZDateTimeOffset.Today;
			order.TransportCoPK = clientPK;
			order.WD_WSH_SalesChannel = salesChannelPK;
			order.DistributionCentreDocAddress.OrganisationPK = distributionCentrePK;

			var docketLine = (WhsDocketLine)order.Lines.AddNew();
			docketLine.WE_OP = partPK;
			docketLine.WE_TransactionQuantity = 10;

			Helper.CreatePickNew(order);
			AssertEquals("Order not picked", true, order.IsAttachedToPickButNotFinalised);

			return order;
		}

		void SetupMainOrgAddress(OrgHeader org, ZString address1, ZString address2, ZString city, ZString postCode, ZString state, ZString relatedPortCode)
		{
			var address = org.MainAddress;
			address.OA_Address1 = address1;
			address.OA_Address2 = address2;
			address.OA_City = city;
			address.OA_PostCode = postCode;
			address.OA_State = state;
			address.OA_RL_NKRelatedPortCode = relatedPortCode;
		}

		#region AddressInfo

		class AddressInfo
		{
			public string City;
			public string PostCode;
			public string StateCode;
			public string PortCode;
		}

		Dictionary<int, AddressInfo> AddressInfoSet
		{
			get { return set ?? (set = BuildAddressSet()); }
		}
		Dictionary<int, AddressInfo> set;

		Dictionary<int, AddressInfo> BuildAddressSet()
			=> new Dictionary<int, AddressInfo>()
			{
				{ 0, new AddressInfo() { City = "Sydney", PostCode = "2000", StateCode = "NSW", PortCode = "AUSYD" } },
				{ 1, new AddressInfo() { City = "Canberra", PostCode = "2601", StateCode = "ACT", PortCode = "AUCBR" } },
				{ 2, new AddressInfo() { City = "Melbourne", PostCode = "3000", StateCode = "VIC", PortCode = "AUMEL" } },
				{ 3, new AddressInfo() { City = "Hobart", PostCode = "7000", StateCode = "TAS", PortCode = "AUHBA" } },
				{ 4, new AddressInfo() { City = "Adelaide", PostCode = "5000", StateCode = "SA", PortCode = "AUADL" } },
				{ 5, new AddressInfo() { City = "Perth", PostCode = "6000", StateCode = "WA", PortCode = "AUPER" } },
				{ 6, new AddressInfo() { City = "Darwin", PostCode = "0800", StateCode = "NT", PortCode = "AUDRW" } },
				{ 7, new AddressInfo() { City = "Brisbane", PostCode = "4000", StateCode = "QLD", PortCode = "AUBNE" } },
				{ 8, new AddressInfo() { City = "Alice Springs", PostCode = "0870", StateCode = "NT", PortCode = "AUASP" } },
				{ 9, new AddressInfo() { City = "Dubbo", PostCode = "2830", StateCode = "NSW", PortCode = "AUDBO" } },
			};

		#endregion

		protected override void SetupData()
		{
			var numberOfOrdersToCreate = 10;

			var clients = new List<OrgHeader>();
			var parts = new List<OrgSupplierPart>();
			var warehouses = new List<WhsWarehouse>();
			var salesChannels = new List<WhsSalesChannel>();
			var distributionCentres = new List<OrgHeader>();

			for (int index = 0; index < numberOfOrdersToCreate; index++)
			{
				var postfix = index.ToString();
				var client = Helper.CreateClient("O" + postfix);
				clients.Add(client);

				var part = Helper.CreateProduct(client, "p1" + postfix);
				parts.Add(part);

				var warehouse = Helper.CreateWarehouse("W" + postfix, "L" + postfix);
				warehouses.Add(warehouse);

				var salesChannel = Helper.CreateWhsSalesChannel($"WS{postfix}", $"Sales Channel {postfix}");
				salesChannels.Add(salesChannel);

				var distributionCentre = Helper.CreateClient("DC" + postfix);
				distributionCentres.Add(distributionCentre);

				Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", part, 20m);
			}
			Factory.Save();

			for (int index = 0; index < numberOfOrdersToCreate; index++)
			{
				var postfix = index.ToString();
				var rateTransportProvider = Helper.SetUpRateTransportProvider(true, "OPS", "AU", "ALL", clients[index]);

				var addressInfo = AddressInfoSet[index];
				SetupMainOrgAddress(clients[index], "20 Maxwell Street", "", addressInfo.City, addressInfo.PostCode, addressInfo.StateCode, addressInfo.PortCode);

				var rateTransportZone = Helper.SetUpRateTransportZone(rateTransportProvider, true, addressInfo.City);
				Helper.SetUpRateTransportZoneItem(rateTransportZone, "AU", addressInfo.PostCode);

				Helper.CreateStock(warehouses[index], clients[index], parts[index], 100m, "A");
				var order = SetupForDocketWithLine(warehouses[index].PK, clients[index].PK, parts[index].PK, salesChannels[index].PK, distributionCentres[index].PK, postfix);

				if (index % 2 == 0)
				{
					var load = Helper.CreateWhsLoad(clients[index], warehouses[index].DefaultInboundDockDoorLocation, "L" + postfix);
					order.WD_WLO_PlannedLoad = load.PK;

					var order1 = Helper.CreateWhsOrderWithOrderLine(clients[index], warehouses[index], parts[index], 5m);
					var pick1 = Helper.CreatePickNew(order1);
					var package = order1.PackageJob.Packages.AddNew();
					var trolley1 = Helper.CreateTrolley("T" + postfix);
					var trolleyJob1 = Helper.CreateWhsPickTrolleyJob(trolley1.PK, "PIC");
					Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package.PK, 1);

					var pickLine = order1.Lines[0].PickLines.Single();
					var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
					transferLine.FinaliseDocketLine();

					var consol1 = Factory.New<IDtbBookingConsolidation>();
					consol1.KB_ParentID = order.PK;
					consol1.KB_ParentTableCode = WhsDocketSchema.Constants.Prefix;

					var booking1 = Factory.New<IDtbBooking>();
					booking1.KM_KB_Booking = consol1.PK;
					booking1.KM_JobID = index + "B";

					// Consignment is worst case scenario for db hits
					var consignmentConsol = Factory.New<IDtbConsignmentConsolidation>();
					consignmentConsol.KB_ParentID = booking1.PK;
					consignmentConsol.KB_ParentTableCode = DtbBookingSchema.Constants.Prefix;

					var consignment1 = Factory.New<IDtbBookingConsignment>();
					consignment1.KM_KB_Booking = consignmentConsol.PK;
					consignment1.KM_JobID = index + "C1";

					if (index % 4 == 0)
					{
						var consignment2 = Factory.New<IDtbBookingConsignment>();
						consignment2.KM_KB_Booking = consignmentConsol.PK;
						consignment2.KM_JobID = index + "C2";
					}
				}
			}
			Factory.Save();
		}

		protected override Dictionary<string, int> GetBaseHits()
		{
			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(WhsDocketSchema.Constants.TableName, 1);
			return expectedDbHits;
		}

		protected override bool ShouldCheckForUnusedFetchHints(string columnName) => false;

		protected override Dictionary<string, Dictionary<string, int>> GetExpectedHitsDictionary()
		{
			var baseHits = GetBaseHits();
			var hitsDictionary = new Dictionary<string, Dictionary<string, int>>();
			var clientHits = new Dictionary<string, int>();
			clientHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			clientHits.Add(WhsDocketSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.WD_OH_Client), clientHits);

			var whsHits = new Dictionary<string, int>(baseHits);
			whsHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.WD_WW_Whs), whsHits);

			var nameorPKHits = new Dictionary<string, int>(baseHits);
			nameorPKHits.Add(JobDocAddressSchema.Constants.TableName, 1);
			nameorPKHits.Add(OrgAddressSchema.Constants.TableName, 3);
			nameorPKHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			nameorPKHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.ConsigneeNameOrPK), nameorPKHits);
			hitsDictionary.Add(nameof(WhsOrder.TransportCoNameOrPK), nameorPKHits);
			hitsDictionary.Add(nameof(WhsOrder.TransportCoName), nameorPKHits);
			hitsDictionary.Add(nameof(WhsOrder.DistributionCentreNameOrPK), nameorPKHits);

			var pickNoHits = new Dictionary<string, int>(baseHits);
			pickNoHits.Add(WhsPickSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.Pick) + "+" + nameof(WhsPick.WP_PickNo), pickNoHits);

			var wdTotalUnitsFromLines = new Dictionary<string, int>(baseHits);
			wdTotalUnitsFromLines.Add(WhsDocketLineSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.WD_TotalUnitsFromLines), wdTotalUnitsFromLines);

			var containerHits = new Dictionary<string, int>(baseHits);
			containerHits.Add(WhsDocketContainerSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.ContainerID), containerHits);
			hitsDictionary.Add(nameof(WhsOrder.ContainerType), containerHits);

			var consigneeAddressHits = new Dictionary<string, int>(baseHits);
			consigneeAddressHits.Add(JobDocAddressSchema.Constants.TableName, 1);
			consigneeAddressHits.Add(OrgAddressSchema.Constants.TableName, 3);
			consigneeAddressHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.ConsigneeAddress) + "+" + nameof(OrgAddress.OA_DeliveryRoute), consigneeAddressHits);
			hitsDictionary.Add(nameof(WhsOrder.ConsigneeAddress) + "+" + nameof(OrgAddress.OA_DeliveryRouteSequence), consigneeAddressHits);
			hitsDictionary.Add(nameof(WhsOrder.ConsigneeDocAddress) + "+" + nameof(JobDocAddress.E2_RN_NKCountryCode), consigneeAddressHits);

			var consigneeDocAddressHits = new Dictionary<string, int>(consigneeAddressHits);
			consigneeDocAddressHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.ConsigneeDocAddress) + "+" + nameof(JobDocAddress.E2_CompanyName), consigneeDocAddressHits);

			var distributionCentreDocAddressHits = new Dictionary<string, int>(baseHits);
			distributionCentreDocAddressHits.Add(JobDocAddressSchema.Constants.TableName, 1);
			distributionCentreDocAddressHits.Add(OrgAddressSchema.Constants.TableName, 3);
			distributionCentreDocAddressHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			distributionCentreDocAddressHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.DistributionCentreDocAddress) + "+" + nameof(JobDocAddress.E2_CompanyName), distributionCentreDocAddressHits);

			var transportJobNoHits = new Dictionary<string, int>(baseHits);
			transportJobNoHits.Add(DtbBookingSchema.Constants.TableName, 1);
			transportJobNoHits.Add(DtbBookingConsolidationSchema.Constants.TableName, 2);
			transportJobNoHits.Add(JobCartageSchema.Constants.TableName, 2);
			hitsDictionary.Add(nameof(WhsOrder.TransportJobNumber), transportJobNoHits);

			var wdWlCrossDockHits = new Dictionary<string, int>(baseHits);
			wdWlCrossDockHits.Add(WhsWarehouseSchema.Constants.TableName, 2);
			hitsDictionary.Add(nameof(WhsOrder.WD_WL_CrossDock), wdWlCrossDockHits);

			var auditStatusHits = new Dictionary<string, int>(baseHits);
			auditStatusHits.Add(PkgPackageSchema.Constants.TableName, 1);
			auditStatusHits.Add(PkgPackageJobSchema.Constants.TableName, 1);
			auditStatusHits.Add(WhsPickSchema.Constants.TableName, 1);
			auditStatusHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.AuditStatus), auditStatusHits);

			var orderGoodsHandlingInstructionsHits = new Dictionary<string, int>(baseHits);
			orderGoodsHandlingInstructionsHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			orderGoodsHandlingInstructionsHits.Add(StmNoteSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.OrderGoodsHandlingInstructions), orderGoodsHandlingInstructionsHits);

			var productCountHits = new Dictionary<string, int>(baseHits);
			productCountHits.Add(WhsDocketLineSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.ProductCount), productCountHits);

			var hasDangerousGoodsHits = new Dictionary<string, int>(baseHits);
			hasDangerousGoodsHits.Add(OrgSupplierPartSchema.Constants.TableName, 3);
			hasDangerousGoodsHits.Add(UNDGDataItemSchema.Constants.TableName, 1);
			hasDangerousGoodsHits.Add(WhsDocketLineSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsPickableDocket.HasDangerousGoods), hasDangerousGoodsHits);

			var workflowItemsHits = new Dictionary<string, int>(baseHits);
			workflowItemsHits.Add(ProcessTasksSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.WorkflowItems) + "+" + nameof(ProcessTaskCollection.Milestones) + "+" + nameof(MilestoneCollectionView.LastMilestone) + "+" + nameof(ProcessTask.P9_SE_NKMilestoneEvent), workflowItemsHits);
			hitsDictionary.Add(nameof(WhsOrder.WorkflowItems) + "+" + nameof(ProcessTaskCollection.Milestones) + "+" + nameof(MilestoneCollectionView.LastMilestone) + "+" + nameof(ProcessTask.DescriptionWithReference), workflowItemsHits);
			hitsDictionary.Add(nameof(WhsOrder.WorkflowItems) + "+" + nameof(ProcessTaskCollection.Milestones) + "+" + nameof(MilestoneCollectionView.NextMilestone) + "+" + nameof(ProcessTask.P9_SE_NKMilestoneEvent), workflowItemsHits);
			hitsDictionary.Add(nameof(WhsOrder.WorkflowItems) + "+" + nameof(ProcessTaskCollection.Milestones) + "+" + nameof(MilestoneCollectionView.NextMilestone) + "+" + nameof(ProcessTask.DescriptionWithReference), workflowItemsHits);
			hitsDictionary.Add(nameof(WhsOrder.WorkflowItems) + "+" + nameof(ProcessTaskCollection.Milestones) + "+" + nameof(MilestoneCollectionView.NextMilestone) + "+" + nameof(ProcessTask.P9_ScheduledDateForBinding), workflowItemsHits);
			hitsDictionary.Add(LastMilestoneActualDate, workflowItemsHits);

			var clientNameHits = new Dictionary<string, int>(baseHits);
			clientNameHits.Add(OrgHeaderSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.ClientName), clientNameHits);

			var carrierBookingAgentDocAddressHits = new Dictionary<string, int>(baseHits);
			carrierBookingAgentDocAddressHits.Add(JobDocAddressSchema.Constants.TableName, 1);
			carrierBookingAgentDocAddressHits.Add(OrgAddressSchema.Constants.TableName, 3);
			carrierBookingAgentDocAddressHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.CarrierBookingAgentDocAddress) + "+" + nameof(JobDocAddress.E2_CompanyName), carrierBookingAgentDocAddressHits);
			var vehicleNoHits = new Dictionary<string, int>(baseHits);
			vehicleNoHits.Add(WhsDocketReferenceSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.VehicleNo), vehicleNoHits);

			var salesChannelHits = new Dictionary<string, int>(baseHits)
			{
				{ WhsSalesChannelSchema.Constants.TableName, 1 }
			};
			hitsDictionary.Add($"{nameof(WhsOrder.SalesChannel)}+{nameof(WhsOrder.SalesChannel.WSH_Description)}", salesChannelHits);

			var loadIDHits = new Dictionary<string, int>(baseHits);
			loadIDHits.Add(WhsLoadOrderSchema.Constants.TableName, 1);
			loadIDHits.Add(WhsLoadSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.LoadID), loadIDHits);

			var jhHits = new Dictionary<string, int>(baseHits);
			jhHits.Add(JobHeaderSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.JobHeader) + "+" + nameof(WhsOrder.JobHeader.JH_ProfitLossReasonCode), jhHits);
			hitsDictionary.Add(nameof(WhsOrder.JobHeader) + "+" + nameof(WhsOrder.JobHeader.JH_TotalProfitRevenueMargin), jhHits);

			var loadTrolleyNumberHits = new Dictionary<string, int>(baseHits);
			loadTrolleyNumberHits.Add(WhsOrderTrolleyViewSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.TrolleyNumber), loadTrolleyNumberHits);

			var whsOrderStatusDescriptionHits = new Dictionary<string, int>(baseHits);
			whsOrderStatusDescriptionHits.Add( WhsOrderStatusViewSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.WarehouseOrderStatusDescription), whsOrderStatusDescriptionHits);

			var outboundLocationHits = new Dictionary<string, int>(baseHits);
			outboundLocationHits.Add(WhsOutboundLocationViewSchema.Constants.TableName, 1);
			hitsDictionary.Add(nameof(WhsOrder.OutboundLocation), outboundLocationHits);

			return hitsDictionary;
		}

		protected override WhsOrderCollection GetNewCollection(BusinessObjectFactory factory)
		{
			return new WhsOrderCollection(factory);
		}

		protected override OrderFilterBusinessObject GetNewFilterBusinessObject() => new OrderFilterBusinessObject();

		protected override ZFilterStripControl GetNewFilterControl(WhsOrderCollection collection, OrderFilterBusinessObject filterBizO)
		{
			return new OrderFilterControl(collection, filterBizO);
		}

		protected override HashSet<string> GetTableColumnsWithIgnoreUnspecifiedTablesFromTableHitsTestEnabled()
		{
			return new HashSet<string>(new[] { LastMilestoneActualDate });
		}

		protected override void AssertAdditionalDbHitsDetails(BusinessObjectFactory factory, string columnName)
		{
			if (columnName.Equals(LastMilestoneActualDate))
			{
				var jobDocAddressTableHitCount = factory.TableSelects.FirstOrDefault(t => t.TableName == JobDocAddressSchema.Constants.TableName).Value;
				Assert("JobDocAddress expected hit count.", jobDocAddressTableHitCount <= 1);
				var orgAddressTableHitCount = factory.TableSelects.FirstOrDefault(t => t.TableName == OrgAddressSchema.Constants.TableName).Value;
				Assert("OrgAddress expected hit count.", orgAddressTableHitCount <= 2);
				var whsWarehouseTableHitCount = factory.TableSelects.FirstOrDefault(t => t.TableName == WhsWarehouseSchema.Constants.TableName).Value;
				Assert("WhsWarehouse expected hit count.", whsWarehouseTableHitCount <= 1);
			}
		}

		const string LastMilestoneActualDate = nameof(WhsOrder.WorkflowItems) + "+" + nameof(ProcessTaskCollection.Milestones) + "+" + nameof(MilestoneCollectionView.LastMilestone) + "+" + nameof(ProcessTask.P9_ActualDateForBinding);

		protected override IEnumerable<string> GetTableNamesToIgnoreForUnusedFetchHints(string columnName)
		{
			var tableNamesToIgnore = base.GetTableNamesToIgnoreForUnusedFetchHints(columnName).Append(GenAddOnColumnSchema.Constants.TableName);
			if (columnName == nameof(WhsOrder.ConsigneeNameOrPK)
				|| columnName == nameof(WhsOrder.TransportCoNameOrPK)
				|| columnName == nameof(WhsOrder.TransportCoName)
				|| columnName == nameof(WhsOrder.ConsigneeDocAddress) + "+" + nameof(JobDocAddress.E2_CompanyName))
			{
				tableNamesToIgnore = tableNamesToIgnore.Append(
					new[]
					{
						OrgAddressCapabilitySchema.Constants.TableName,
						GenCustomAddOnRuleAckSchema.Constants.TableName,
						OrgCompanyDataSchema.Constants.TableName
					});
			}
			else if (columnName == nameof(WhsOrder.CarrierBookingAgentDocAddress) + "+" + nameof(JobDocAddress.E2_CompanyName)
				|| columnName == nameof(WhsOrder.ConsigneeAddress) + "+" + nameof(OrgAddress.OA_DeliveryRoute)
				|| columnName == nameof(WhsOrder.ConsigneeAddress) + "+" + nameof(OrgAddress.OA_DeliveryRouteSequence)
				|| columnName == nameof(WhsOrder.ConsigneeDocAddress) + "+" + nameof(JobDocAddress.E2_RN_NKCountryCode)
				|| columnName == nameof(WhsOrder.WorkflowItems) + "+" + nameof(ProcessTaskCollection.Milestones) + "+" + nameof(MilestoneCollectionView.LastMilestone) + "+" + nameof(ProcessTask.P9_ActualDateForBinding))
			{
				tableNamesToIgnore = tableNamesToIgnore.Append(
					new[]
					{
						OrgAddressCapabilitySchema.Constants.TableName,
						GenCustomAddOnRuleAckSchema.Constants.TableName,
						OrgHeaderSchema.Constants.TableName
					});
			}
			else if (columnName == nameof(WhsOrder.AuditStatus)
				|| columnName == nameof(WhsOrder.HasDangerousGoods))
			{
				tableNamesToIgnore = tableNamesToIgnore.Append(
					new[]
					{
						OrgPartBOMSchema.Constants.TableName,
						CusClassPartPivotSchema.Constants.TableName,
						OrgPartRelationSchema.Constants.TableName,
						OrgPartUnitSchema.Constants.TableName,
						StmNoteSchema.Constants.TableName,
						OrgSupplierPartBarcodeSchema.Constants.TableName,
						WhsPickFaceSchema.Constants.TableName
					});

				if (columnName == nameof(WhsOrder.AuditStatus))
				{
					tableNamesToIgnore = tableNamesToIgnore.Append(UNDGDataItemSchema.Constants.TableName);
				}
			}
			else if (columnName == nameof(WhsOrder.ContainerID))
			{
				tableNamesToIgnore = tableNamesToIgnore.Append(ProcessTasksSchema.Constants.TableName);
			}
			else if (columnName == nameof(WhsOrder.OrderGoodsHandlingInstructions))
			{
				tableNamesToIgnore = tableNamesToIgnore.Append(OrgCompanyDataSchema.Constants.TableName);
			}
			else if (columnName == nameof(WhsOrder.ProductCount)
				|| columnName == nameof(WhsOrder.WD_TotalUnitsFromLines))
			{
				tableNamesToIgnore = tableNamesToIgnore.Append(OrgSupplierPartSchema.Constants.TableName);
			}
			else if (columnName == nameof(WhsOrder.TransportJobNumber))
			{
				tableNamesToIgnore = tableNamesToIgnore.Append(
					new[]
					{
						DtbBookingInstructionSchema.Constants.TableName,
						JobHeaderSchema.Constants.TableName,
						DtbBookingSchema.Constants.TableName,
						JobCO2eSchema.Constants.TableName
					});
			}

			return tableNamesToIgnore;
		}

		protected new WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;
	}
}
