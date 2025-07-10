using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalDateType = Enterprise.UniversalDataBuss.DataObjects.Universal.DateType;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	internal class JobSupplierBookingLineDataObjectReader : ShipmentDataObjectReader<JobSupplierBookingLine>
	{
		readonly JobSupplierBooking supplierBooking;

		public JobSupplierBookingLineDataObjectReader(UniversalShipment dataObject, JobSupplierBooking supplierBooking, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
			this.supplierBooking = supplierBooking;
		}

		#region Implementation

		public override DataContextType DataContextType => DataContextType.JobSupplierBooking;

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(JobSupplierBookingLine targetBO)
		{
			if ((dataObject?.PackingLineCollection?.Count ?? 0) == 0)
			{
				return Res.GetString("1f53b610-6648-4134-a377-682108c4caa7", "There is no packline");
			}

			if ((dataObject?.Order?.OrderLineCollection?.Count ?? 0) == 0)
			{
				return Res.GetString("efba689e-4492-4842-aec5-7297f0558b5a", "There is no related order line");
			}

			if (GetMatchedOrderLine() == null)
			{
				return Res.GetString("d91927fc-99d8-4a0e-9d66-79ba6dcd41e4", "There is no matched order line");
			}

			var bookingLineId = (dataObject.PackingLineCollection?.FirstOrDefault()?.PackingLineID ?? ZString.Empty);
			if (!bookingLineId.IsEmpty && supplierBooking.SupplierBookingLines.All(supplierBookingLine => supplierBookingLine.JSL_BookingLineId != bookingLineId))
			{
				return Res.GetString("f8cb68a5-10ac-4db8-9083-144aca67a727", "Supplier Booking Line {0} is not matched", bookingLineId);
			}

			return base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
		}

		protected override void PopulateBusinessObject(JobSupplierBookingLine targetBO)
		{
			PopulateBusinessObjectCore(targetBO);
			PopulateOrganisations(targetBO);
			PopulateDates(targetBO);
		}

		protected override JobSupplierBookingLine GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			if (supplierBooking?.SupplierBookingLines?.Count == 0)
			{
				return null;
			}

			var matchedOrderLine = GetMatchedOrderLine();
			if (matchedOrderLine == null)
			{
				return null;
			}

			if (dataObject.PackingLineCollection?.Count == 0)
			{
				return null;
			}

			var bookingLineId = dataObject.PackingLineCollection[0].PackingLineID.GetValueOrDefault();
			if (!bookingLineId.IsEmpty)
			{
				return supplierBooking.SupplierBookingLines.FirstOrDefault(supplierBookingLine => supplierBookingLine.JSL_BookingLineId == bookingLineId);
			}

			return supplierBooking.SupplierBookingLines.FirstOrDefault(supplierBookingLine => supplierBookingLine.JSL_JO_OrderLine == matchedOrderLine.PK);
		}

		protected override IMatchingBusinessEntityFinder<JobSupplierBookingLine> GetCombinedReferenceMatcher() => null;

		#endregion

		#region Helpers

		void PopulateBusinessObjectCore(JobSupplierBookingLine targetBO)
		{
			var packLine = dataObject.PackingLineCollection[0];

			SetValue(targetBO, JobSupplierBookingLineSchema.JSL_BookedQuantity, dataObject.TotalNoOfPacksDecimal);
			SetValue(targetBO, JobSupplierBookingLineSchema.JSL_BookedPackages, Convert.ToDecimal(packLine.PackQty ?? ZLong.Zero));
			SetValue(targetBO, JobSupplierBookingLineSchema.JSL_F3_NKBookedPackagesUnit, packLine.PackType);
			SetValue(targetBO, JobSupplierBookingLineSchema.JSL_Description, packLine.GoodsDescription);
			SetValue(targetBO, JobSupplierBookingLineSchema.JSL_MarksAndNumbers, packLine.MarksAndNos);
			SetValue(targetBO, JobSupplierBookingLineSchema.JSL_GrossWeight, packLine.Weight);
			SetValue(targetBO, JobSupplierBookingLineSchema.JSL_GrossWeightUnit, packLine.WeightUnit);
			SetValue(targetBO, JobSupplierBookingLineSchema.JSL_Volume, packLine.Volume);
			SetValue(targetBO, JobSupplierBookingLineSchema.JSL_VolumeUnit, packLine.VolumeUnit);
			SetValue(targetBO, JobSupplierBookingLineSchema.JSL_JO_OrderLine, GetMatchedOrderLine()?.PK ?? ZGuid.Empty);
			SetValue(targetBO, JobSupplierBookingLineSchema.JSL_RH_NKCommodityCode, packLine.Commodity?.Code.GetValueOrDefault() ?? ZString.Empty);
			SetValue(targetBO, JobSupplierBookingLineSchema.JSL_HarmonisedCode, packLine.HarmonisedCode);

			if (targetBO.OrderLine.JO_F3_NKPackType != (dataObject.TotalNoOfPacksPackageType?.Code ?? ZString.Empty))
			{
				logger.Log(Enterprise.Integration.LogType.Warning,
					Res.GetString("57123A07-4506-490F-9C6A-BF997A0B3BF7",
						"Unit of Qty of {0}~{1}~{2}~{3} cannot be updated when adding or editing a supplier booking line.",
						targetBO.OrderLine.Order.JD_OrderNumber,
						targetBO.OrderLine.Order.JD_OrderNumberSplit,
						targetBO.OrderLine.JO_LineNo,
						targetBO.OrderLine.JO_SubLineNo));
			}
		}

		void PopulateOrganisations(JobSupplierBookingLine targetBO)
		{
			if ((dataObject.OrganizationAddressCollection?.Count ?? 0) == 0)
			{
				return;
			}

			var matchedAddress = JobSupplierBookingDataObjectHelper.FindByDocAddressTypeAndGetMatched(dataObject, DocAddressType.Manufacturer, logger, factory);
			if (matchedAddress != null)
			{
				targetBO.ManufacturerAddress.E2_OA_Address = matchedAddress.PK;
			}
		}

		void PopulateDates(JobSupplierBookingLine targetBO)
		{
			if (dataObject.DateCollection != null)
			{
				foreach (var dateDataObject in dataObject.DateCollection)
				{
					switch (dateDataObject.Type)
					{
						case UniversalDateType.ShipmentWindowStart:
							SetValue(targetBO, JobSupplierBookingLineSchema.JSL_ShipmentWindowStart, dateDataObject.Value);
							break;
						case UniversalDateType.ShipmentWindowEnd:
							SetValue(targetBO, JobSupplierBookingLineSchema.JSL_ShipmentWindowEnd, dateDataObject.Value);
							break;
					}
				}
			}
		}

		OrderLine GetMatchedOrderLine()
		{
			if ((dataObject?.Order?.OrderLineCollection?.Count ?? 0) == 0)
			{
				return null;
			}

			var matchedOrder = new OrderDataObjectReader(dataObject, logger, factory).TryGetExistingBusinessObject();
			if (matchedOrder == null)
			{
				return null;
			}

			return new OrderLineDataObjectReader(dataObject.Order.OrderLineCollection[0], logger, factory, matchedOrder).TryGetExistingBusinessObject();
		}

		#endregion
	}
}
