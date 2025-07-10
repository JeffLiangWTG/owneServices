using System;
using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Registry.AWB;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	[DebuggerDisplay("IATAChargeCode: {IATAChargeCode}, EntitlementCode: {EntitlementCode}")]
	public class OtherChargeTemplate
	{
		public OtherChargeTemplate(ExportAWBHeader awbHeader)
		{
			Argument.NotNull(awbHeader, "awbHeader");
			this.awbHeader = awbHeader;
		}

		readonly ExportAWBHeader awbHeader;

		public AccChargeCode AccChargeCode
		{
			get { return accChargeCode; }
			set
			{
				if (accChargeCode != value)
				{
					accChargeCode = value;
					displayOption = null;
				}
			}
		}
		AccChargeCode accChargeCode;

		public ZString PrepaidCollect
		{
			get { return prepaidCollect; }
			set
			{
				if (prepaidCollect != value)
				{
					prepaidCollect = value;
					displayOption = null;
				}
			}
		}
		ZString prepaidCollect;

		public ZDecimal ChargeAmount
		{
			get { return lazyChargeAmount.Value; }
		}
		readonly LazyValueProvider<ZDecimal> lazyChargeAmount = new LazyValueProvider<ZDecimal>();

		public Func<ZDecimal> ChargeAmountProvider
		{
			get { return lazyChargeAmount.ValueProvider; }
			set { lazyChargeAmount.ValueProvider = value; }
		}

		public ZString Currency
		{
			get { return lazyCurrency.Value; }
		}
		readonly LazyValueProvider<ZString> lazyCurrency = new LazyValueProvider<ZString>();

		public Func<ZString> CurrencyProvider
		{
			get { return lazyCurrency.ValueProvider; }
			set { lazyCurrency.ValueProvider = value; }
		}

		public ZDecimal CostAmount
		{
			get { return lazyCostAmount.Value; }
		}
		readonly LazyValueProvider<ZDecimal> lazyCostAmount = new LazyValueProvider<ZDecimal>();

		public Func<ZDecimal> CostAmountProvider
		{
			get { return lazyCostAmount.ValueProvider; }
			set { lazyCostAmount.ValueProvider = value; }
		}

		public ZDecimal ProfitAmount
		{
			get { return lazyProfitAmount.Value; }
		}
		readonly LazyValueProvider<ZDecimal> lazyProfitAmount = new LazyValueProvider<ZDecimal>();

		public Func<ZDecimal> ProfitAmountProvider
		{
			get { return lazyProfitAmount.ValueProvider; }
			set { lazyProfitAmount.ValueProvider = value; }
		}

		public ZDecimal TaxAmount
		{
			get { return taxAmount; }
			set
			{
				if (taxAmount != value)
				{
					taxAmount = value;
					displayOption = null;
				}
			}
		}
		ZDecimal taxAmount;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public ZString ChargeDescription
		{
			get { return AccChargeCode != null ? AccChargeCode.AC_Desc : ZString.Empty; }
		}

		#region IATAChargeCode

		public ZString IATAChargeCode => lazyIATAChargeCode.Value;

		readonly LazyValueProvider<ZString> lazyIATAChargeCode = new LazyValueProvider<ZString>();

		public Func<ZString> IATAChargeCodeProvider
		{
			get { return lazyIATAChargeCode.ValueProvider; }
			set { lazyIATAChargeCode.ValueProvider = value; }
		}

		#endregion

		public ZString EntitlementCode
		{
			get { return DisplayOption != null ? DisplayOption.Entitlement : ZString.Empty; }
		}

		public ZBool IsValid
		{
			get
			{
				return AccChargeCode != null &&
					!AccChargeCode.IsFreightChargeCode() &&
					DisplayOption != null && DisplayOption.Visibility == nameof(AWBDisplayOptionVisibility.Show);
			}
		}

		AWBDisplayOption DisplayOption
		{
			get { return displayOption ?? (displayOption = awbHeader.GetDisplayOption(IATAChargeCode, PrepaidCollect)); }
		}
		AWBDisplayOption displayOption;

		public NonPersistentExportAWBOtherCharge[] CreateCharges()
		{
			var result = new List<NonPersistentExportAWBOtherCharge>();

			if (IsValid)
			{
				if (EntitlementCode == Core.Constants.AWB.EntitlementCode.Split)
				{
					result.Add(CreateCharge(Core.Constants.AWB.EntitlementCode.Carrier, CostAmount));
					result.Add(CreateCharge(Core.Constants.AWB.EntitlementCode.Agent, ProfitAmount));
				}
				else
				{
					result.Add(CreateCharge(EntitlementCode, ChargeAmount));
				}
			}

			return result.ToArray();
		}

		NonPersistentExportAWBOtherCharge CreateCharge(ZString entitlementCode, ZDecimal amount)
		{
			NonPersistentExportAWBOtherCharge charge = new NonPersistentExportAWBOtherCharge(awbHeader);

			charge.ChargeCode = IATAChargeCode;
			charge.ChargeDescription = ChargeDescription;
			charge.PPDCLT = PrepaidCollect;
			charge.EntitlementCode = entitlementCode;
			charge.Amount = amount;
			charge.Currency = Currency;

			return charge;
		}
	}
}