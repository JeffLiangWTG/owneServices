using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class GenerateRMAByOrdersActionMethodApplicator : WhsOperationalActionMethodApplicator
	{
		public GenerateRMAByOrdersActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("abf667b9-db4b-4a62-94a6-582dcd1f34f4", "Generate RMA Receive by Orders"), factory)
		{
		}

		#region Settings and Validation

		ZString rmaReference = ZString.Empty;
		ZGuid whsOverride;

		public abstract class Schema
		{
			public const string RMAReference = nameof(RMAReference);
			public const string WhsOverride = nameof(WhsOverride);
		}

		[ResourceStringData("0de97900-cb39-469f-a8c4-cfd118e679de", Caption = "RMA Reference")]
		public ZString RMAReference
		{
			get => rmaReference;
			set => SetNonPersistentPropertyValue(RMAReferenceInfo, ref rmaReference, value);
		}

		public ZPropertyInfo RMAReferenceInfo => GetZPropertyInfo(nameof(RMAReference));

		[List("Lookups.Warehouses")]
		[ResourceStringData("bc5b20be-eac2-408d-9417-f3179594f0cC", Caption = "Warehouse Override")]
		public ZGuid WhsOverride
		{
			get => whsOverride;
			set
			{
				SetNonPersistentPropertyValue(WhsOverrideInfo, ref whsOverride, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateWhsOverride();
				}
			}
		}

		public ZPropertyInfo WhsOverrideInfo => GetZPropertyInfo(nameof(WhsOverride));

		#endregion

		#region Property

		HashSet<ZGuid> GeneratedReceivePKs => generatedReceivePKs ?? (generatedReceivePKs = new HashSet<ZGuid>());
		HashSet<ZGuid> generatedReceivePKs;

		readonly List<WhsOrder> invalidOrders = new List<WhsOrder>();
		readonly List<WhsOrder> ordersWithReturnReceives = new List<WhsOrder>();
		readonly List<WhsOrder> noReleaseLinesOrders = new List<WhsOrder>();

		#endregion

		#region GenerateRMAReceive

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] bizoList)
		{
			invalidOrders.Clear();
			ordersWithReturnReceives.Clear();
			var rmaLines = GetValidWhsRMAOrderLineList(bizoList.OfType<WhsOrder>());
			if (rmaLines.Count > 0)
			{
				ApplyCore(log, rmaLines[0].Factory, rmaLines);
			}
		}

		void ApplyCore(IOperationalActionSectionLog log, BusinessObjectFactory factory, List<WhsRMAOrderLine> rmaLines)
		{
			log.SetSectionProgressMax(rmaLines.Count);

			var builder = new WhsReceiveCollectionBuilder(factory);

			foreach (var rmaLine in rmaLines)
			{
				var returnDocketLine = BuildLine(builder, rmaLine);

				if (rmaLine.ShouldCopyBOMLinks)
				{
					WhsRMAHelper.CopyBOMComponentLinks(rmaLine.ParentInventory, returnDocketLine);
				}

				var docket = returnDocketLine.Docket;
				if (GeneratedReceivePKs.Add(docket.PK))
				{
					docket.WD_CustomerReference = RMAReference;
					docket.WD_WD_ParentDocket = rmaLine.ParentOrder.PK;

					var receivePivotQuery = new ZQuery(WhsDocketJobPivotSchema.WV_WD_Docket, docket.PK);
					receivePivotQuery.AddToFilter(WhsDocketJobPivotSchema.WV_DocketType, DocketType.Codes.Receive);
					factory.AddFetchHint(WhsDocketJobPivotSchema.Instance, receivePivotQuery);

					var bookingConsolQuery = new ZQuery(DtbBookingConsolidationSchema.KB_ParentID, docket.PK);
					bookingConsolQuery.AddToFilter(DtbBookingConsolidationSchema.KB_JobType, SQLComparisonOperator.NotEqual, TransportConsolidationJobTypes.Codes.Consignment);
					factory.AddFetchHint(DtbBookingConsolidationSchema.Instance, bookingConsolQuery);
				}
				log.BumpSectionProgress();
			}
		}

		#endregion

		#region GetValidWhsRMAOrderLineList

		List<WhsRMAOrderLine> GetValidWhsRMAOrderLineList(IEnumerable<WhsOrder> orderList)
		{
			var result = new List<WhsRMAOrderLine>();
			if (orderList?.Any() ?? false)
			{
				AddFetchHints(orderList);

				foreach (var order in orderList)
				{
					if (order.HasRelatedReturnReceive)
					{
						ordersWithReturnReceives.Add(order);
					}
					else if (order?.IsPickFinalised ?? false)
					{
						var orderLines = order.Lines.Cast<WhsPickableDocketLine>().Where(orderLine => orderLine.WE_WE_ParentDocketLine.IsEmpty && orderLine.PickLines.Count > 0).ToList();
						if (orderLines.Count > 0)
						{
							result.AddRange(WhsRMAHelper.GetWhsRMAOrderLines(orderLines.Cast<WhsOrderLine>(), order));
						}
						else
						{
							noReleaseLinesOrders.Add(order);
						}
					}
					else
					{
						invalidOrders.Add(order);
					}
				}
			}

			return result;
		}

		void AddFetchHints(IEnumerable<WhsOrder> orderList)
		{
			var factory = orderList.First().Factory;
			foreach (var order in orderList)
			{
				factory.AddFetchHint(WhsPickSchema.PK, order.WD_WP);
				factory.AddFetchHint(WhsDocketSchema.WD_WP, order.WD_WP);
				factory.AddFetchHint(WhsDocketSchema.Instance, WhsOrder.GetRelatedReceivesQuery(order.PK));
			}

			var orderLines = factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, orderList.Select(o => o.PK)));
			var pickLines = factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, orderLines.Select(ol => ol.PK)));

			WhsRMAHelper.AddFetchHint(orderLines, factory);
		}

		#endregion

		#region BuildLine

		WhsDocketLine BuildLine(WhsReceiveCollectionBuilder builder, WhsRMAOrderLine rmaLine)
		{
			var data = GetLineData(rmaLine);

			return builder.AddLine(data, WhsDocketCollectionBuilderLineOptions.None);
		}

		#endregion

		#region GetLineData

		LineData GetLineData(WhsRMAOrderLine rmaOrderLine)
		{
			var data = new LineData();

			var order = rmaOrderLine.ParentOrder;
			data.WhsPK = !WhsOverride.IsEmpty ? WhsOverride : order.WD_WW_Whs;
			data.OrgPK = order.WD_OH_Client;
			data.PartPK = rmaOrderLine.ProductPK;
			data.Quantity = rmaOrderLine.Quantity;
			data.ExternalReference = order.WD_ExternalReference;

			data.PartAttrib1 = rmaOrderLine.PartAttrib1;
			data.PartAttrib2 = rmaOrderLine.PartAttrib2;
			data.PartAttrib3 = rmaOrderLine.PartAttrib3;
			data.SerialNumber = rmaOrderLine.SerialNumber;

			data.ExpiryDate = rmaOrderLine.ExpiryDate;
			data.PackingDate = rmaOrderLine.PackingDate;
			data.DocketSubType = ReceiveType.Codes.Returns;

			return data;
		}

		#endregion

		#region Logging

		protected override void InitialiseBeforeIndividiualBatchRunCore()
		{
			base.InitialiseBeforeIndividiualBatchRunCore();
			GeneratedReceivePKs.Clear();
		}

		protected override bool SupportsSummaryCore => true;

		protected override void SummaryLogCore(IOperationalActionSectionLog log)
		{
			base.SummaryLogCore(log);

			if (GeneratedReceivePKs.Count > 0)
			{
				var factory = new BusinessObjectFactory();

				foreach (var receive in factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.PK, GeneratedReceivePKs.ToArray())).OrderBy(o => o.WD_DocketID))
				{
					LogGeneratedOrder(receive, log);
				}
			}

			foreach (var noReleaseLinesOrder in noReleaseLinesOrders)
			{
				var docketLink = GetDocketIdLink(noReleaseLinesOrder);

				var message = Res.GetString(
					"9e30bcdc-36c1-4312-b579-4948159ad7b8",
					"Could not generate a Receive from Order {0}, because no Release Lines in this Order.",
					"{0}" // Hyperlink must be injected directly from NotifyFormat()
					);

				log.NotifyFormat(OperationalActionLogErrorLevel.Error, message, docketLink);
			}

			foreach (var invalidOrder in invalidOrders)
			{
				var docketLink = GetDocketIdLink(invalidOrder);

				var message = Res.GetString(
					"918c28db-f7d2-4795-ad93-50c49246eee9",
					"Could not generate a Receive from Order {0}, because its Pick is not finalized.",
					"{0}" // Hyperlink must be injected directly from NotifyFormat()
					);

				log.NotifyFormat(OperationalActionLogErrorLevel.Error, message, docketLink);
			}

			foreach (var orderWithReturnReceive in ordersWithReturnReceives)
			{
				var docketLink = GetDocketIdLink(orderWithReturnReceive);

				var message = Res.GetString(
					"9bba31b3-380e-43bd-99ae-c3eef8923ff3",
					"Could not generate a Receive from Order {0}, because a return receive is already linked to this Order.",
					"{0}" // Hyperlink must be injected directly from NotifyFormat()
					);

				log.NotifyFormat(OperationalActionLogErrorLevel.Error, message, docketLink);
			}
		}

		void LogGeneratedOrder(WhsReceive receive, IOperationalActionSectionLog log)
		{
			var docketLink = GetDocketIdLink(receive);
			log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "{0} {1} {2}.", receive.Description, docketLink, Res.GetString("645bad6f-efdf-43a8-8a9f-525e51735845", "has been generated"));
		}

		#endregion

		#region Validation

		public GenerateRMAByOrdersActionValidation Validation => new GenerateRMAByOrdersActionValidation(this);

		#endregion

		#region Lookups

		public GenerateRMAByOrdersActionLookups Lookups =>  lookups ?? (lookups = new GenerateRMAByOrdersActionLookups(this));

		GenerateRMAByOrdersActionLookups lookups;

		#endregion
	}
}
