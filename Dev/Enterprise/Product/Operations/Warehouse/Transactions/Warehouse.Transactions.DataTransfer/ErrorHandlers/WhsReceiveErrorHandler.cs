using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsReceiveErrorHandler : WhsDocketErrorHandler<WhsReceive>
	{
		#region Implementation

		#region Receive

		protected override bool DocketDataHasErrors(WhsReceive docket, Xsd.WhsDocket value)
		{
			var docketHasErrors = base.DocketDataHasErrors(docket, value);
			if (value.DocketDetail.Item is Xsd.WhsCustomerInwardsDetail)
			{
				var xsdInwardsDetail = (Xsd.WhsCustomerInwardsDetail)value.DocketDetail.Item;
				if (WD_ArrivalDateHasErrors(docket, xsdInwardsDetail))
				{
					docketHasErrors = true;
				}

				if (WD_BookingDateHasErrors(docket, xsdInwardsDetail))
				{
					docketHasErrors = true;
				}

				if (WD_ETDHasErrors(docket, xsdInwardsDetail))
				{
					docketHasErrors = true;
				}

				if (WD_ETAHasErrors(docket, xsdInwardsDetail))
				{
					docketHasErrors = true;
				}
			}

			return docketHasErrors;
		}

		protected override bool WE_PackQuantityHasErrors(WhsReceive docket, WhsDocketLine line, Xsd.WhsDocketLine value)
		{
			var result = true;
			var expectedUnits = ZDecimal.Zero;
			var quantity = (value.QuantityFromClientOrder == 0 ? value.QuantityActuallyOrdered : value.QuantityFromClientOrder);
			if (line.SupplierPart != null)
			{
				if (line.SupplierPart.UnitConverter.Convertible(value.ProductUQ, line.ProductUQ))
				{
					expectedUnits = ZArchitecture.Core.Utilities.Round(line.SupplierPart.UnitConverter.Convert(quantity, value.ProductUQ, line.ProductUQ), line.SupplierPart.OP_CountDecimalPlaces);
					if (line.WE_ClientOrderedUnits == expectedUnits)
					{
						result = false;
					}
					if (result)
					{
						AddEventLog(docket, GetLineErrorPrefix(line) + " " + Res.GetString("c7183e85-dab8-4969-bf6a-dfc16179b6e3", "Error (Exp. Qty): {0}", expectedUnits));
					}
				}
				else
				{
					AddEventLog(docket, GetLineErrorPrefix(line) + " " + Res.GetString("08db05f9-2be9-4d26-9407-89159f40bb25", "Error (Exp. Qty): No Conversion {0} to {1}", value.ProductUQ, line.ProductUQ));
					AddEventLog(docket, GetLineErrorPrefix(line) + " " + Res.GetString("3094427f-1d70-452d-809b-d1dc6572128a", "Original Unit Qty: {0}", quantity));
				}
			}
			else
			{
				if (line.ProductUQ != value.ProductUQ)
				{
					AddEventLog(docket, GetLineErrorPrefix(line) + " " + Res.GetString("08db05f9-2be9-4d26-9407-89159f40bb25", "Error (Exp. Qty): No Conversion {0} to {1}", value.ProductUQ, line.ProductUQ));
					AddEventLog(docket, GetLineErrorPrefix(line) + " " + Res.GetString("3094427f-1d70-452d-809b-d1dc6572128a", "Original Unit Qty: {0}", quantity));
				}
			}
			return result;
		}

		protected virtual bool WD_ETAHasErrors(WhsReceive docket, Xsd.WhsCustomerInwardsDetail value)
		{
			var result = true;
			if (docket.WD_ETA.IsEmpty && value.ETA.IsEmpty)
			{
				result = false;
			}
			if (!docket.WD_ETA.IsEmpty && !value.ETA.IsEmpty)
			{
				if (docket.WD_ETA == docket.Warehouse.GetWarehouseBranchLocalDateTimeOffset(value.ETA))
				{
					result = false;
				}
			}
			if (result)
			{
				AddEventLog(docket, Res.GetString("61b0bdd3-f43a-471f-8428-bddc4b52dc82", "Error (ETA):") + " " + GetZDateValueForError(value.ETA));
			}
			return result;
		}

		protected virtual bool WD_ETDHasErrors(WhsReceive docket, Xsd.WhsCustomerInwardsDetail value)
		{
			var result = true;
			if (docket.WD_ETD.IsEmpty && value.ETD.IsEmpty)
			{
				result = false;
			}
			if (!docket.WD_ETD.IsEmpty && !value.ETD.IsEmpty)
			{
				if (docket.WD_ETD == docket.Warehouse.GetWarehouseBranchLocalDateTimeOffset(value.ETD))
				{
					result = false;
				}
			}
			if (result)
			{
				AddEventLog(docket, Res.GetString("caeef263-e8b4-4ffc-9720-4a17991d6fe0", "Error (ETD):") + " " + GetZDateValueForError(value.ETD));
			}
			return result;
		}

		protected virtual bool WD_BookingDateHasErrors(WhsReceive docket, Xsd.WhsCustomerInwardsDetail value)
		{
			var result = true;
			if (docket.WD_BookingDate.IsEmpty && value.BookingDate.IsEmpty)
			{
				result = false;
			}
			if (!docket.WD_BookingDate.IsEmpty && !value.BookingDate.IsEmpty)
			{
				if (docket.WD_BookingDate == docket.Warehouse.GetWarehouseBranchLocalDateTimeOffset(value.BookingDate))
				{
					result = false;
				}
			}
			if (result)
			{
				AddEventLog(docket, Res.GetString("d7a53877-d6df-49c7-baef-a4dbb95859bb", "Error (Booking Date):") + " " + GetZDateValueForError(value.BookingDate));
			}
			return result;
		}

		protected virtual bool WD_ArrivalDateHasErrors(WhsReceive docket, Xsd.WhsCustomerInwardsDetail value)
		{
			var result = true;
			if (docket.WD_ArrivalDate.IsEmpty && value.ArrivalDate.IsEmpty)
			{
				result = false;
			}
			if (!docket.WD_ArrivalDate.IsEmpty && !value.ArrivalDate.IsEmpty)
			{
				if (docket.WD_ArrivalDate == docket.Warehouse.GetWarehouseBranchLocalDateTimeOffset(value.ArrivalDate))
				{
					result = false;
				}
			}
			if (result)
			{
				AddEventLog(docket, Res.GetString("bd2b3240-7c6d-45d4-98a0-6be1aae7ca9d", "Error (Arrival Date):") + " " + GetZDateValueForError(value.ArrivalDate));
			}
			return result;
		}

		#endregion

		#endregion
	}
}
