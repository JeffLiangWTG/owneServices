using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ShipmentCalculationLogsAnalyzer : CalculationLogsAnalyzer
	{
		public ShipmentCalculationLogsAnalyzer(ShipmentExportAWBHeader exportAWBHeader)
			: base(exportAWBHeader)
		{
			Header = exportAWBHeader;
		}

		readonly ShipmentExportAWBHeader Header;

		#region Logs Wrapper

		protected override IEnumerable<BusinessObject> GetAllBusinessObjectsWithCalculationLogs()
		{
			var freightCharges = Header.GetFreightCharges();
			if (freightCharges.Length != 1)
			{
				return null;
			}

			return new List<BusinessObject> { freightCharges[0] };
		}

		#endregion

		#region Amounts in HAWB Currency

		protected override ZDecimal GetAmountInAWBCurrency(ZDecimal originalAmount, ZString originalCurrencyCode)
		{
			RefCurrency originalCurrency = Header.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, originalCurrencyCode);
			if (originalCurrency != null)
			{
				var jobCharge = GetAllBusinessObjectsWithCalculationLogs()?.FirstOrDefault() as JobCharge;
				var debtor = jobCharge?.JR_OH_SellAccount ?? ZGuid.Empty;
				return Header.GetAmountInHAWBCurrency(new Money(originalAmount, originalCurrency), debtor, CostSell.Revenue);
			}

			return base.GetAmountInAWBCurrency(originalAmount, originalCurrencyCode);
		}

		#endregion

		#region Overrides

		protected override ZInt GetNumberOfPieces()
		{
			return Header.Shipment != null ? Header.Shipment.JS_OuterPacks : base.GetNumberOfPieces();
		}

		protected override bool IsDisabled
		{
			get
			{
				// For BCN and SCN shipment we want details to come from the shipment being autorated rather than from rating.
				// Rating autorates total amounts from all shipments (BCN/SCN lead and sub shipments) which then gets apportioned per shipment.
				// When disabling the logs analyser we instruct AWB to ignore details from autorating and it will fallback to shipment details.
				return Header.Shipment.JS_PackingMode == Constants.ContainerModes.BuyersConsol ||
					   Header.Shipment.JS_PackingMode == Constants.ContainerModes.ShippersConsol;
			}
		}

		#endregion
	}
}