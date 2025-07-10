using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsOrderErrorHandler : WhsDocketErrorHandler<WhsOrder>
	{
		#region Implementation

		#region Order

		protected override bool DocketDataHasErrors(WhsOrder docket, Xsd.WhsDocket value)
		{
			var docketHasErrors = base.DocketDataHasErrors(docket, value);
			if (value.DocketDetail.Item is Xsd.WhsCustomerOrderDetail)
			{
				var xsdOrderDetail = (Xsd.WhsCustomerOrderDetail)value.DocketDetail.Item;

				if (WD_RequiredDateHasErrors(docket, xsdOrderDetail))
				{
					docketHasErrors = true;
				}

				if (WD_DocketSubTypeHasErrors(docket, xsdOrderDetail))
				{
					docketHasErrors = true;
				}

				if (WD_ShipperCODAmountHasErrors(docket, value))
				{
					docketHasErrors = true;
				}

				if (WD_CODPayMethodHasErrors(docket, value))
				{
					docketHasErrors = true;
				}

				if (WD_PalletsSentHasErrors(docket, value))
				{
					docketHasErrors = true;
				}

				if (WD_PackagesSentHasErrors(docket, value))
				{
					docketHasErrors = true;
				}

				if (WD_F3_NKTotalPackTypeHasErrors(docket, value))
				{
					docketHasErrors = true;
				}

				if (WD_TotalWeightHasErrors(docket, value))
				{
					docketHasErrors = true;
				}

				if (WD_TotalWeightUnitHasErrors(docket, value))
				{
					docketHasErrors = true;
				}

				if (WD_TotalCubicHasErrors(docket, value))
				{
					docketHasErrors = true;
				}

				if (WD_TotalCubicUnitHasErrors(docket, value))
				{
					docketHasErrors = true;
				}

				if (WD_LocalCartInsuranceCostHasErrors(docket, value))
				{
					docketHasErrors = true;
				}
			}
			return docketHasErrors;
		}

		protected virtual bool WD_ShipperCODAmountHasErrors(WhsOrder docket, Xsd.WhsDocket value)
		{
			var result = true;
			if (docket.WD_ShipperCODAmount == value.DocketDetail.ShipperCODAmount)
			{
				result = false;
			}
			if (result)
			{
				AddEventLog(docket, (NoResString)"Error (COD Amount): " + value.DocketDetail.ShipperCODAmount); // Developer only string
			}
			return result;
		}

		protected virtual bool WD_CODPayMethodHasErrors(WhsOrder docket, Xsd.WhsDocket value)
		{
			var result = true;
			if (docket.WD_CODPayMethod == value.DocketDetail.ShipperCODType)
			{
				result = false;
			}
			if (result)
			{
				AddEventLog(docket, (NoResString)"Error (COD Method): " + value.DocketDetail.ShipperCODType); // Developer only string
			}
			return result;
		}

		protected virtual bool WD_LocalCartInsuranceCostHasErrors(WhsOrder docket, Xsd.WhsDocket value)
		{
			var result = true;
			if (docket.WD_LocalCartInsuranceCost == value.DocketDetail.TransportInsurance)
			{
				result = false;
			}
			if (result)
			{
				AddEventLog(docket, (NoResString)"Error (Transport Ins.): " + value.DocketDetail.TransportInsurance); // Developer only string
			}
			return result;
		}

		protected virtual bool WD_PalletsSentHasErrors(WhsOrder docket, Xsd.WhsDocket value)
		{
			var result = true;
			if (docket.WD_PalletsSent == value.DocketDetail.Pallets)
			{
				result = false;
			}
			if (result)
			{
				AddEventLog(docket, (NoResString)"Error (Pallets Sent): " + value.DocketDetail.Pallets); // Developer only string
			}
			return result;
		}

		protected virtual bool WD_PackagesSentHasErrors(WhsOrder docket, Xsd.WhsDocket value)
		{
			var result = true;
			if (docket.WD_PackagesSent == (ZInt)value.DocketDetail.Packages.Value)
			{
				result = false;
			}
			if (result)
			{
				AddEventLog(docket, (NoResString)"Error (Packages Sent): " + value.DocketDetail.Packages.Value); // Developer only string
			}
			return result;
		}

		protected virtual bool WD_F3_NKTotalPackTypeHasErrors(WhsOrder docket, Xsd.WhsDocket value)
		{
			var result = true;
			if (docket.WD_F3_NKTotalPackType == value.DocketDetail.Packages.DimensionType)
			{
				result = false;
			}
			if (result)
			{
				AddEventLog(docket, (NoResString)"Error (Packages Sent UQ): " + value.DocketDetail.Packages.DimensionType); // Developer only string
			}
			return result;
		}

		protected virtual bool WD_TotalWeightHasErrors(WhsOrder docket, Xsd.WhsDocket value)
		{
			var result = value.DocketDetail.Weight.IsSpecified;
			if (value.DocketDetail.Weight.IsSpecified)
			{
				if (docket.WD_TotalWeight == value.DocketDetail.Weight.Value)
				{
					result = false;
				}
				if (result)
				{
					AddEventLog(docket, (NoResString)"Error (Total Weight): " + value.DocketDetail.Weight.Value); // Developer only string
				}
			}
			return result;
		}

		protected virtual bool WD_TotalWeightUnitHasErrors(WhsOrder docket, Xsd.WhsDocket value)
		{
			var result = value.DocketDetail.Weight.IsSpecified;
			if (value.DocketDetail.Weight.IsSpecified)
			{
				if (docket.WD_TotalWeightUnit == value.DocketDetail.Weight.DimensionType)
				{
					result = false;
				}
				if (result)
				{
					AddEventLog(docket, (NoResString)"Error (Total Weight UQ): " + value.DocketDetail.Weight.DimensionType); // Developer only string
				}
			}
			return result;
		}

		protected virtual bool WD_TotalCubicHasErrors(WhsOrder docket, Xsd.WhsDocket value)
		{
			var result = value.DocketDetail.Cubic.IsSpecified;
			if (value.DocketDetail.Cubic.IsSpecified)
			{
				if (docket.WD_TotalCubic == value.DocketDetail.Cubic.Value)
				{
					result = false;
				}
				if (result)
				{
					AddEventLog(docket, (NoResString)"Error (Total Cubic): " + value.DocketDetail.Cubic.Value); // Developer only string
				}
			}
			return result;
		}

		protected virtual bool WD_TotalCubicUnitHasErrors(WhsOrder docket, Xsd.WhsDocket value)
		{
			var result = value.DocketDetail.Cubic.IsSpecified;
			if (value.DocketDetail.Cubic.IsSpecified)
			{
				if (docket.WD_TotalCubicUnit == value.DocketDetail.Cubic.DimensionType)
				{
					result = false;
				}
				if (result)
				{
					AddEventLog(docket, (NoResString)"Error (Total Cubic UQ): " + value.DocketDetail.Cubic.DimensionType); // Developer only string
				}
			}
			return result;
		}

		protected virtual bool WD_DocketSubTypeHasErrors(WhsOrder docket, Xsd.WhsCustomerOrderDetail value)
		{
			var result = true;
			if (docket.WD_DocketSubType == value.OrderType)
			{
				result = false;
			}
			if (result)
			{
				AddEventLog(docket, (NoResString)"Error (Order Type): " + value.OrderType); // Developer only string
			}
			return result;
		}

		protected virtual bool WD_RequiredDateHasErrors(WhsOrder docket, Xsd.WhsCustomerOrderDetail value)
		{
			var result = docket.WD_RequiredDate.IsEmpty;

			if (!docket.WD_RequiredDate.IsEmpty && !value.DateRequired.IsEmpty)
			{
				if (docket.WD_RequiredDate != docket.Warehouse.GetWarehouseBranchLocalDateTimeOffset(value.DateRequired))
				{
					result = true;
				}
			}
			if (result)
			{
				AddEventLog(docket, (NoResString)"Error (Required Date): " + GetZDateValueForError(value.DateRequired)); // Developer only string
			}
			return result;
		}

		#endregion

		#endregion
	}
}
