using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	/// <summary>
	/// This class is shared by both the Inventory and Receive Operational Actions.
	/// </summary>
	public abstract class GenerateOrderFromExistingInventoryActionMethodApplicator : WhsOperationalActionMethodApplicator
	{
		protected GenerateOrderFromExistingInventoryActionMethodApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{
			Consignees = new ConsigneeCollection(Factory);
			CrossDockLocations = new WhsLocationCollection(Factory, dockDoorLocationsOnly: true);
		}

		#region Settings and Validation

		ZGuid consigneePK;

		public abstract class Schema
		{
			public const string ConsigneePK = nameof(ConsigneePK);
			public const string CrossDockLocationPK = nameof(CrossDockLocationPK);
		}

		public ZGuid ConsigneePK
		{
			get => consigneePK;
			set => SetNonPersistentPropertyValue(ConsigneePKInfo, ref consigneePK, value);
		}

		public ZPropertyInfo ConsigneePKInfo => GetZPropertyInfo(
			Schema.ConsigneePK,
			Res.GetString("7a65cbe4-296f-41d2-ba88-0ad36254b521", "Consignee"));

		public ConsigneeCollection Consignees { get; }

		public ZGuid CrossDockLocationPK
		{
			get => crossDockLocationPK;
			set => SetNonPersistentPropertyValue(CrossDockLocationPKInfo, ref crossDockLocationPK, value);
		}

		public ZPropertyInfo CrossDockLocationPKInfo => GetZPropertyInfo(
			Schema.CrossDockLocationPK,
			Res.GetString("b10b68f7-0037-449d-97c3-aad2edec67f1", "Cross Dock Location"));

		ZGuid crossDockLocationPK;
		ZGuid crossDockLocationWarehousePK;

		Dictionary<(ZGuid WhsPK, ZGuid OrgPK, ZString ExternalReference, ZDateTimeOffset RequiredDate), List<(JobDocAddress consigneeDocAddress, ZGuid orderPK)>> CreatedOrderPKHolder;

		public WhsLocationCollection CrossDockLocations { get; }

		protected override void InitialiseBeforeAllBatchesRunCore()
		{
			base.InitialiseBeforeAllBatchesRunCore();

			CreatedOrderPKHolder = new Dictionary<(ZGuid WhsPK, ZGuid OrgPK, ZString ExternalReference, ZDateTimeOffset RequiredDate), List<(JobDocAddress consigneeDocAddress, ZGuid orderPK)>>();

			if (crossDockLocationPK.IsValid)
			{
				crossDockLocationWarehousePK = Factory.Load<WhsLocation>(crossDockLocationPK).WLV_WW_Whs;
			}
			else
			{
				crossDockLocationWarehousePK = ZGuid.Empty;
			}
		}

		protected override void InitialiseBeforeIndividiualBatchRunCore()
		{
			base.InitialiseBeforeIndividiualBatchRunCore();
			GeneratedOrderPKs.Clear();
		}

		#endregion

		#region Generate Order

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] bizoList)
		{
			var docketLineList = GetDocketLineList(log, bizoList);

			if (docketLineList.Length > 0)
			{
				ApplyCore(log, docketLineList[0].Factory, docketLineList);
			}
		}

		void ApplyCore(IOperationalActionSectionLog log, BusinessObjectFactory factory, WhsDocketLine[] docketLineList)
		{
			log.SetSectionProgressMax(docketLineList.Length);
			docketLinesWithNoConsignee.Clear();
			docketsWithCrossDockWarehouseMismatch.Clear();

			var builder = new WhsOrderCollectionBuilder(factory);

			foreach (WhsDocketLine docketLine in docketLineList)
			{
				BuildLine(log, builder, docketLine);
				log.BumpSectionProgress();
			}

			foreach (var order in builder.Dockets)
			{
				// it is acceptable to ignore this error and leave this to user to enter manually
				if (order.WD_TotalCubicInfo.HasErrors())
				{
					AddOrderWarningLog(order.PK, Res.GetString("aad5eb57-f07d-4c70-a281-34ea2706083d", "'Total Line Volume' has been changed from {0} to 0 to skip the error[{1}], you may need to correct it manually.", order.WD_TotalCubic, order.WD_TotalCubicInfo.Notifications.GetErrors().ToUniqueMessageListString()));
					order.WD_TotalCubic = 0m;
				}
				// it is acceptable to ignore this error and leave this to user to enter manually
				if (order.WD_TotalWeightInfo.HasErrors())
				{
					AddOrderWarningLog(order.PK, Res.GetString("c0ee1657-b2a3-4000-9449-d396d89f78dc", "'Total Line Weight' has been changed from {0} to 0 to skip the error[{1}], you may need to correct it manually.", order.WD_TotalWeight, order.WD_TotalWeightInfo.Notifications.GetErrors().ToUniqueMessageListString()));
					order.WD_TotalWeight = 0m;
				}

				GeneratedOrderPKs.Add(order.PK);
			}
		}

		List<ZGuid> GeneratedOrderPKs => generatedOrderPKs ?? (generatedOrderPKs = new List<ZGuid>());
		List<ZGuid> generatedOrderPKs;

		readonly List<WhsDocketLine> docketLinesWithNoConsignee = new List<WhsDocketLine>();
		readonly HashSet<WhsDocket> docketsWithCrossDockWarehouseMismatch = new HashSet<WhsDocket>();

		#region BuildLine

		void BuildLine(IOperationalActionSectionLog log, WhsOrderCollectionBuilder builder, WhsDocketLine docketLine)
		{
			var data = GetLineData(docketLine);

			if (!HasValidInternalState(data))
			{
				return;
			}

			if (HasConsignee(data))
			{
				if (MatchesCrossDockLocationWarehouse(data))
				{
					WhsOrderLine line;
					ZGuid existingOrderPK;
					var key = (data.WhsPK, data.OrgPK, data.ExternalReference, data.RequiredDateOffset);
					var hasPotentialHit = CreatedOrderPKHolder.TryGetValue(key, out var orderPKWithJobDocAddress);
					if (hasPotentialHit)
					{
						var foundOrder = orderPKWithJobDocAddress.FirstOrDefault(o => WhsOrderCollectionBuilder.IsTheSameDocAddress(o.consigneeDocAddress, data.ConsigneeDocAddress));
						existingOrderPK = foundOrder.orderPK;
					}
					else
					{
						orderPKWithJobDocAddress = new List<(JobDocAddress jobDocAddress, ZGuid orderPK)>();
						CreatedOrderPKHolder.Add(key, orderPKWithJobDocAddress);
						existingOrderPK = ZGuid.Empty;
					}

					if (existingOrderPK.IsValid)
					{
						line = (WhsOrderLine)builder.AddLine(data, WhsDocketCollectionBuilderLineOptions.MergeLine, existingOrderPK);
					}
					else
					{
						line = (WhsOrderLine)builder.AddLine(data, WhsDocketCollectionBuilderLineOptions.MergeLine);
						orderPKWithJobDocAddress.Add((data.ConsigneeDocAddress, line.WE_WD));
					}

					if (line.ReserveStockIfAbleTo(docketLine.Inventory[0], data.Quantity) == null)
					{
						// This should never happen
						var docketOriginal = docketLine.DocketOriginal;
						var externalReference = docketOriginal != null ? docketOriginal.WD_ExternalReference : ZString.Empty;
						log.NotifyFormat(OperationalActionLogErrorLevel.Warning, Res.GetString("2ff516fa-75c0-4320-9912-364b1dd29e14",
							"Receipt Line {0} from Receipt {1} could not be Reserved.", docketLine.WE_LineNo, externalReference));
					}
				}
				else
				{
					docketsWithCrossDockWarehouseMismatch.Add(docketLine.Docket);
				}
			}
			else
			{
				docketLinesWithNoConsignee.Add(docketLine);
			}
		}

		#region GetLineData

		OrderLineData GetLineData(WhsDocketLine line)
		{
			var data = new OrderLineData();
			data.WhsPK = line.Docket.WD_WW_Whs;
			data.OrgPK = line.Docket.WD_OH_Client;
			data.PartPK = line.WE_OP;
			data.Attributes = line;
			data.DocketSubType = GetOrderDocketSubType(line);

			data.Quantity = line.Inventory.Cast<WhsInventoryView>().Sum(i => i.WI_AvailableForCrossDockQuantity);
			data.ExternalReference = line.WE_ReceiveCrossDockOrderNo;
			data.RequiredDateOffset = line.WE_RequiredByDate;

			JobDocAddress consigneeNonPersistentClone = null;
			if (line is WhsReceiveLine receiveLine)
			{
				consigneeNonPersistentClone = (JobDocAddress)receiveLine?.ConsigneeDocAddress.Clone();
				if (consigneeNonPersistentClone != null)
				{
					consigneeNonPersistentClone.MakeNonPersistent();
					if (consigneeNonPersistentClone.IsEmpty && !ConsigneePK.IsEmpty)
					{
						consigneeNonPersistentClone.OrganisationPK = ConsigneePK;
					}
				}
			}
			else if (!ConsigneePK.IsEmpty)
			{
				consigneeNonPersistentClone = JobDocAddress.New(line, DocAddressType.ConsigneeAddress);
				consigneeNonPersistentClone.MakeNonPersistent();
				consigneeNonPersistentClone.OrganisationPK = ConsigneePK;
			}

			data.ConsigneeDocAddress = consigneeNonPersistentClone;

			if (crossDockLocationPK.IsValid && crossDockLocationWarehousePK == data.WhsPK)
			{
				data.CrossDockLocationPK = crossDockLocationPK;
			}

			return data;
		}

		static ZString GetOrderDocketSubType(WhsDocketLine line) =>
			line.LocationAreaType == AreaTypes.Codes.Bonded
				? GetCustomsDocketSubType(line.Docket.CountryCode)
				: OrderType.Codes.Order;

		static string GetCustomsDocketSubType(ZString countryCode)
			=> BondedHelper.IsCountrySupportedForFTZPermits(countryCode)
					? OrderType.Codes.CustomsReleaseWithPermit
					: OrderType.Codes.Customs;

		#endregion

		#region IsLineFitForBuild

		bool HasValidInternalState(OrderLineData data)
		{
			return data.Quantity > 0m;
		}

		protected bool HasConsignee(OrderLineData data)
		{
			return data.ConsigneeDocAddress != null && !data.ConsigneeDocAddress.IsEmpty;
		}

		bool MatchesCrossDockLocationWarehouse(OrderLineData data)
		{
			return crossDockLocationWarehousePK.IsEmpty || data.CrossDockLocationPK.IsValid;
		}

		#endregion

		#endregion

		#region AddOrderWarningLog

		void AddOrderWarningLog(ZGuid orderPK, ZString log)
		{
			if (orderWarningsLog.ContainsKey(orderPK))
			{
				orderWarningsLog[orderPK].Add(log);
			}
			else
			{
				orderWarningsLog.Add(orderPK, new List<ZString> { log });
			}
		}

		readonly Dictionary<ZGuid, List<ZString>> orderWarningsLog = new Dictionary<ZGuid, List<ZString>>();

		#endregion

		protected abstract WhsDocketLine[] GetDocketLineList(IOperationalActionSectionLog log, BusinessObject[] bizoList);

		#endregion

		#region Logging

		protected override bool SupportsSummaryCore
		{
			get { return true; }
		}

		protected override void SummaryLogCore(IOperationalActionSectionLog log)
		{
			base.SummaryLogCore(log);

			if (GeneratedOrderPKs.Count > 0)
			{
				var factory = new BusinessObjectFactory();

				foreach (var order in factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.PK, GeneratedOrderPKs.ToArray())).OrderBy(o => o.WD_DocketID))
				{
					LogGeneratedOrder(order, log);
				}
			}

			foreach (var invalidDocketLine in docketLinesWithNoConsignee)
			{
				var docketLink = GetDocketIdLink(invalidDocketLine.Docket);

				var message = Res.GetString(
					"794afe9d-306a-4727-9e78-abe8ec040be3",
					"Could not generate an Order for Product {0} from Receive {1} because Receive Line {2} has no Consignee Address.",
					invalidDocketLine.SupplierPart.OP_PartNum,
					"{0}", // Hyperlink must be injected directly from NotifyFormat()
					invalidDocketLine.WE_LineNo) + "\r\n";

				log.NotifyFormat(OperationalActionLogErrorLevel.Error, message, docketLink);
			}

			foreach (var invalidDocket in docketsWithCrossDockWarehouseMismatch)
			{
				var docketLink = GetDocketIdLink(invalidDocket);

				var message = Res.GetString(
					"627ad2d4-f47d-4702-a763-64eeae38c80a",
					"Could not generate an Order for Receive {0} because the Order's Warehouse does not match the selected Cross-Dock Location's Warehouse.",
					"{0}") + "\r\n"; // Hyperlink must be injected directly from NotifyFormat()

				log.NotifyFormat(OperationalActionLogErrorLevel.Error, message, docketLink);
			}
		}

		void LogGeneratedOrder(WhsOrder order, IOperationalActionSectionLog log)
		{
			LogControllerLink docketLink = GetDocketIdLink(order);
			if (orderWarningsLog.ContainsKey(order.PK))
			{
				foreach (var warningLog in orderWarningsLog[order.PK])
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "{0} {1} {2}", order.Description, docketLink, warningLog);
				}
			}

			log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "{0} {1} {2}.", order.Description, docketLink, Res.GetString("645bad6f-efdf-43a8-8a9f-525e51735845", "has been generated"));
		}

		#endregion
	}
}
