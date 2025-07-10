using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public abstract class AgencyConsumerType : JobInvoicingConsumerType
	{
		public AgencyConsumerType(string code, MultilingualString description)
			: base(code, description) { }

		public override bool ShouldCreateWIPs(IJobInvoicingPlugIn host, string invoiceType)
		{
			return base.ShouldCreateWIPs(host, invoiceType) && ShouldCreateTransactions(host, invoiceType, false).PostAllowed;
		}

		public override bool ShouldCreateAccruals(IJobInvoicingPlugIn host, string invoiceType)
		{
			return base.ShouldCreateAccruals(host, invoiceType) && ShouldCreateTransactions(host, invoiceType, true).PostAllowed;
		}

		public override PostChargesAllowedInformation ShouldPostCharges(IJobInvoicingPlugIn host, string invoiceType, bool isForAPLine)
		{
			PostChargesAllowedInformation result = base.ShouldPostCharges(host, invoiceType, isForAPLine);

			return result.PostAllowed ? ShouldCreateTransactions(host, invoiceType, isForAPLine) : result;
		}

		public override CodeDescriptionPairList InvoiceTypeList
		{
			get { return new AgencyInvoiceTypesList(); }
		}

		public override ZString GetInvoiceTypeWithNoDebtor(ZString currentInvoiceType)
		{
			return currentInvoiceType;
		}

		public override string GetOverriddenInvoiceType(IJobInvoicingPlugIn host, AccChargeCode chargeCode, RefCurrency currency, ZString postingStyle, ZString currentInvoiceType)
		{
			var result = string.Empty;
			if (host != null)
			{
				var isCollect = IsChargeCollect(currentInvoiceType) ?? IsChargeCollect(host, chargeCode);

				if (isCollect.HasValue)
				{
					bool useLocalCurrencyInvoice;
					if (IsUsingForeignPostingStyle(postingStyle))
					{
						useLocalCurrencyInvoice = currency == null || currency.RX_Code == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
					}
					else
					{
						useLocalCurrencyInvoice = true;
					}

					if (isCollect.Value)
					{
						if (IsDeferedInvoiceType(currentInvoiceType))
						{
							result = useLocalCurrencyInvoice ? AgencyInvoiceTypesList.Codes.LocalCollect_Batching : AgencyInvoiceTypesList.Codes.ForeignCollect_Batching;
						}
						else
						{
							result = useLocalCurrencyInvoice ? AgencyInvoiceTypesList.Codes.LocalCollect : AgencyInvoiceTypesList.Codes.ForeignCollect;
						}
					}
					else
					{
						if (IsDeferedInvoiceType(currentInvoiceType))
						{
							result = useLocalCurrencyInvoice ? AgencyInvoiceTypesList.Codes.LocalPrePaid_Batching : AgencyInvoiceTypesList.Codes.ForeignPrePaid_Batching;
						}
						else
						{
							result = useLocalCurrencyInvoice ? AgencyInvoiceTypesList.Codes.LocalPrePaid : AgencyInvoiceTypesList.Codes.ForeignPrePaid;
						}
					}
				}
			}

			return result;
		}

		static PostChargesAllowedInformation ShouldCreateTransactions(IJobInvoicingPlugIn host, string invoiceType, bool isForAPLine)
		{
			if (!isForAPLine && ObjectFactory.Get<IAgencyRegistry>().PostBothPrepaidAndCollectShipmentRevenueCharges)
			{
				return new PostChargesAllowedInformation();
			}

			if (isForAPLine && ObjectFactory.Get<IAgencyRegistry>().PostBothPrepaidAndCollectShipmentCostCharges)
			{
				return new PostChargesAllowedInformation();
			}

			switch (invoiceType)
			{
				case AgencyInvoiceTypesList.Codes.LocalPrePaid:
				case AgencyInvoiceTypesList.Codes.LocalPrePaid_Batching:
				case AgencyInvoiceTypesList.Codes.ForeignPrePaid:
				case AgencyInvoiceTypesList.Codes.ForeignPrePaid_Batching:
					return new PostChargesAllowedInformation(IsLocal(host.InvoicingSupporter.Origin), Res.GetString("8d5fd8fa-456e-4651-996e-dc58b67a75d8", "The Invoice Type is Pre-Paid however the origin on this job is not in the same country/region as the current company."));

				case AgencyInvoiceTypesList.Codes.LocalCollect:
				case AgencyInvoiceTypesList.Codes.LocalCollect_Batching:
				case AgencyInvoiceTypesList.Codes.ForeignCollect:
				case AgencyInvoiceTypesList.Codes.ForeignCollect_Batching:
					return new PostChargesAllowedInformation(IsLocal(host.InvoicingSupporter.Destination), Res.GetString("915d2846-e4f9-42fa-bc66-dcab3fe9aef4", "The Invoice Type is Collect however the destination on this job is not in the same country/region as the current company."));

				default:
					return new PostChargesAllowedInformation();
			}
		}

		static bool IsLocal(RefUNLOCO port)
		{
			GlbCompany company = GlbCompany.CurrentCompany;
			return port != null && company != null && port.RL_RN_NKCountryCode == company.GC_RN_NKCountryCode;
		}

		public static bool IsUsingForeignPostingStyle(ZString postingStyle)
		{
			switch (postingStyle)
			{
				case InvoicePostingOptionsList.Codes.DisbursementForeignAndFinal:
				case InvoicePostingOptionsList.Codes.DisbursementForeignOnly:
				case InvoicePostingOptionsList.Codes.DisbursementFreightForeignAndFinal:
				case InvoicePostingOptionsList.Codes.DisbursementInvoiceForeignAndFinal:
				case InvoicePostingOptionsList.Codes.ForeignCurrencyInvoiceAndFinal:
				case "":
					return true;

				default:
					return false;
			}
		}

		static bool? IsChargeCollect(ZString currentInvoiceType)
		{
			switch (currentInvoiceType)
			{
				case AgencyInvoiceTypesList.Codes.LocalCollect:
				case AgencyInvoiceTypesList.Codes.ForeignCollect:
				case AgencyInvoiceTypesList.Codes.LocalCollect_Batching:
				case AgencyInvoiceTypesList.Codes.ForeignCollect_Batching:
					return true;

				case AgencyInvoiceTypesList.Codes.LocalPrePaid:
				case AgencyInvoiceTypesList.Codes.ForeignPrePaid:
				case AgencyInvoiceTypesList.Codes.LocalPrePaid_Batching:
				case AgencyInvoiceTypesList.Codes.ForeignPrePaid_Batching:
					return false;

				default:
					return null;
			}
		}

		static bool IsDeferedInvoiceType(string invoiceType)
		{
			switch (invoiceType)
			{
				case AgencyInvoiceTypesList.Codes.LocalCollect_Batching:
				case AgencyInvoiceTypesList.Codes.ForeignCollect_Batching:
				case AgencyInvoiceTypesList.Codes.LocalPrePaid_Batching:
				case AgencyInvoiceTypesList.Codes.ForeignPrePaid_Batching:
					return true;

				default:
					return false;
			}
		}

		public static bool? IsChargeCollect(IJobInvoicingPlugIn host, AccChargeCode chargeCode)
		{
			var prepaidCollect = host.InvoicingSupporter.PaymentTerm.GetPrepaidCollect(CostSell.Revenue, chargeCode.AC_ChargeGroup);
			return prepaidCollect.In(Core.Constants.PaymentType.Collect, Core.Constants.PaymentType.Prepaid)
				? prepaidCollect == Core.Constants.PaymentType.Collect
				: null;
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceShipping; }
		}
	}
}
