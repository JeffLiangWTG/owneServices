using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	internal class ForwardersCargoReceiptSummaryBuilder : ForwardersCargoReceiptBuilder
	{
		public ForwardersCargoReceiptSummaryBuilder(ForwardingShipment shipment, IDocDataObjectParameters parameters) : base(shipment, parameters)
		{
		}

		public ForwardersCargoReceiptSummary Build()
		{
			var receipt = Build(new ForwardersCargoReceiptSummary());
			if (shipment == null)
			{
				return receipt;
			}

			receipt.MarksAndNumbers = shipment.JS_MarksAndNumbers;
			receipt.ParticularFurnishedByShipper = shipment.JS_GoodsDescription + (shipment.DetailedGoodsDescriptionNoteText.IsEmpty
				? ZString.Empty
				: (ZString)(System.Environment.NewLine + shipment.DetailedGoodsDescriptionNoteText));

			var packLines = shipment.OuterPackLines.Cast<ForwardingPackLine>();
			receipt.TotalPackages = shipment.JS_OuterPacks;
			receipt.TotalPackagesUnit = new CodeDescription(shipment.Lookups.JS_PackType_List) { Code = shipment.JS_F3_NKPackType };
			receipt.TotalWeightKG = packLines.Sum(x => Core.Constants.Weight.Convert(x.JL_ActualWeight, x.JL_ActualWeightUQ, Core.Constants.Weight.Kilograms));
			receipt.TotalVolumeM3 = packLines.Sum(x => Core.Constants.Volume.Convert(x.JL_ActualVolume, x.JL_ActualVolumeUQ, Core.Constants.Volume.CubicMetres));

			SetOrderLineValues(receipt);

			receipt.ValidateAllIncludingChildren();

			return receipt;
		}

		void SetOrderLineValues(ForwardersCargoReceiptSummary receipt)
		{
			var packLinesSubQuery = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.PK);
			packLinesSubQuery.AddToFilter(JobPackLinesSchema.JL_JS, shipment.PK);

			var containerPacklineDivotSubQuery = new ZDBOnlySubQuery(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JL);

			var containerSubQuery = new ZDBOnlySubQuery(typeof(ForwardingContainer), JobContainerSchema.PK);
			containerSubQuery.AddToFilter(JobContainerSchema.JC_JSB_SupplierBooking, SQLComparisonOperator.NotEqual, ZGuid.Empty);

			containerPacklineDivotSubQuery.AddSubQuery(JobContainerPackPivotSchema.J6_JC, containerSubQuery, JoinCondition.And);
			packLinesSubQuery.AddSubQuery(containerPacklineDivotSubQuery, JoinCondition.And);

			var containerLoadListLineSubQuery = new ZDBOnlySubQuery(typeof(ContainerLoadListLine), ContainerLoadListLineSchema.CLL_JSL_BookingLine);
			containerLoadListLineSubQuery.AddSubQuery(ContainerLoadListLineSchema.CLL_JL_PackLine, packLinesSubQuery, JoinCondition.And);

			var jobSupplierBookingLineSubQuery = new ZDBOnlySubQuery(typeof(JobSupplierBookingLine), JobSupplierBookingLineSchema.JSL_JO_OrderLine);
			jobSupplierBookingLineSubQuery.AddSubQuery(JobSupplierBookingLineSchema.PK, containerLoadListLineSubQuery, JoinCondition.And);

			var orderLineQuery = new ZDBOnlyQuery(typeof(AutoJobOrderLine));
			orderLineQuery.AddSubQuery(JobOrderLineSchema.PK, jobSupplierBookingLineSubQuery, JoinCondition.And);

			var orderLines = shipment.Factory.Load<OrderLine>(orderLineQuery);

			receipt.OrderLineNumbers = string.Join(", ", orderLines.Select(x => $"{x.Order.JD_OrderNumber}-{x.JO_LineNo}").OrderBy(no => no));
			receipt.ItemNumbers = string.Join(", ", orderLines.Where(x => !x.JO_Partno.IsEmpty).Distinct().Select(x => x.JO_Partno).OrderBy(no => no));
			receipt.TotalOrderLines = orderLines.Length;
		}
	}
}
