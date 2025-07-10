using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business
{
	public class ITOTIncoTermWithOFT_ONS : Common.ITOTIncoTerm.ITOTIncoTermWithOFT_ONSCalculator
	{
		public ITOTIncoTermWithOFT_ONS(ZString incoTerm)
			: base(incoTerm)
		{ }

		protected override ZString OverseasFreightChargeCode => USCustomsChargeTypeList.Codes.OverseasFreight;
		protected override ZString OverseasInsuranceChargeCode => USCustomsChargeTypeList.Codes.OverseasInsurance;
		protected override Common.ITOTIncoTerm.IITOTIncoTermCalculator CostAndFreightIncoTermCalculator => new CAFIncoTerm(TermsOfDeliveryList.Codes.CAF);
		protected override Common.ITOTIncoTerm.IITOTIncoTermCalculator FreeOnBoardIncoTermCalculator => new FOBIncoTerm(TermsOfDeliveryList.Codes.FOB);
	}

	public class ExQuayDutyPaidIncoTerm : Common.ITOTIncoTerm.IITOTIncoTermCalculator
	{
		public ZString IncoTerm => TermsOfDeliveryList.Codes.EXQ;

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(USCustomsChargeTypeList.Codes.LandingCharges))
			{
				result = new CIFIncoTerm().Calculate(invoice);
			}
			return result;
		}
	}

	public class CAFIncoTerm : Common.ITOTIncoTerm.IITOTIncoTermCalculator
	{
		public CAFIncoTerm(ZString incoTerm)
		{
			this.IncoTerm = incoTerm;
		}

		public ZString IncoTerm { get; private set; }

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(USCustomsChargeTypeList.Codes.OverseasFreight))
			{
				result = new FOBIncoTerm(TermsOfDeliveryList.Codes.FOB).Calculate(invoice);
			}
			return result;
		}
	}

	public class CAIIncoTerm : Common.ITOTIncoTerm.IITOTIncoTermCalculator
	{
		public ZString IncoTerm => TermsOfDeliveryList.Codes.CAI;

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(USCustomsChargeTypeList.Codes.OverseasInsurance))
			{
				result = new FOBIncoTerm(TermsOfDeliveryList.Codes.FOB).Calculate(invoice);
			}
			return result;
		}
	}

	public class CFRIncoTerm : Common.ITOTIncoTerm.IITOTIncoTermCalculator
	{
		public ZString IncoTerm => TermsOfDeliveryList.Codes.CFR;

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(USCustomsChargeTypeList.Codes.OverseasFreight))
			{
				result = new FOBIncoTerm(TermsOfDeliveryList.Codes.FOB).Calculate(invoice);
			}
			return result;
		}
	}

	public class CIFIncoTerm : ITOTIncoTermWithOFT_ONS
	{
		public CIFIncoTerm()
			: base(TermsOfDeliveryList.Codes.CIF)
		{
		}
	}

	public class CIPIncoTerm : ITOTIncoTermWithOFT_ONS
	{
		public CIPIncoTerm()
			: base(TermsOfDeliveryList.Codes.CIP)
		{
		}
	}

	public class CPTIncoTerm : Common.ITOTIncoTerm.IITOTIncoTermCalculator
	{
		public ZString IncoTerm => TermsOfDeliveryList.Codes.CPT;

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(USCustomsChargeTypeList.Codes.OverseasFreight))
			{
				result = new FOBIncoTerm(TermsOfDeliveryList.Codes.FOB).Calculate(invoice);
			}
			return result;
		}
	}

	public class DAPIncoTerm : Common.ITOTIncoTerm.IITOTIncoTermCalculator
	{
		public ZString IncoTerm => TermsOfDeliveryList.Codes.DAP;

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesIncludedInITOT(USCustomsChargeTypeList.Codes.LandingCharges))
			{
				if (invoice.HasChargesExcludedInITOT(USCustomsChargeTypeList.Codes.OverseasFreight) || invoice.HasChargesExcludedInITOT(USCustomsChargeTypeList.Codes.OverseasInsurance))
				{
					result = IncoTermAndCustomsChargeFactory.ErrorIncoTermCode;
				}
			}
			else if (invoice.HasChargesExcludedInITOT(USCustomsChargeTypeList.Codes.LandingCharges))
			{
				result = new CIFIncoTerm().Calculate(invoice);
			}
			else if (!invoice.HasChargesWithCurrency(USCustomsChargeTypeList.Codes.LandingCharges))
			{
				if (invoice.HasChargesExcludedInITOT(USCustomsChargeTypeList.Codes.OverseasFreight) || invoice.HasChargesExcludedInITOT(USCustomsChargeTypeList.Codes.OverseasInsurance))
				{
					result = new ITOTIncoTermWithOFT_ONS(IncoTerm).Calculate(invoice);
				}
			}
			return result;
		}
	}

	public class DATIncoTerm : Common.ITOTIncoTerm.IITOTIncoTermCalculator
	{
		public ZString IncoTerm => TermsOfDeliveryList.Codes.DAT;

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(USCustomsChargeTypeList.Codes.LandingCharges))
			{
				result = new CIFIncoTerm().Calculate(invoice);
			}
			return result;
		}
	}

	public class FCAIncoTerm : Common.ITOTIncoTerm.IITOTIncoTermCalculator
	{
		public ZString IncoTerm => TermsOfDeliveryList.Codes.FCA;

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(USCustomsChargeTypeList.Codes.PackingCost))
			{
				result = new EXWIncoTerm().Calculate(invoice);
			}
			return result;
		}
	}

	public class FOBIncoTerm : Common.ITOTIncoTerm.IITOTIncoTermCalculator
	{
		public FOBIncoTerm(ZString incoTerm)
		{
			this.IncoTerm = incoTerm;
		}

		public ZString IncoTerm { get; private set; }

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(USCustomsChargeTypeList.Codes.ForeignInlandFreight))
			{
				result = new EXWIncoTerm().Calculate(invoice);
			}
			return result;
		}
	}

	public class DDPIncoTerm : Common.ITOTIncoTerm.IITOTIncoTermCalculator
	{
		public ZString IncoTerm => TermsOfDeliveryList.Codes.DDP;

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(USCustomsChargeTypeList.Codes.LandingCharges))
			{
				result = new CIFIncoTerm().Calculate(invoice);
			}
			return result;
		}
	}

	public class EXWIncoTerm : Common.ITOTIncoTerm.IITOTIncoTermCalculator
	{
		public ZString IncoTerm => TermsOfDeliveryList.Codes.EXW;

		public ZString Calculate(ICommonInvoice invoice)
		{
			return IncoTerm;
		}
	}

	public class FASIncoTerm : Common.ITOTIncoTerm.IITOTIncoTermCalculator
	{
		public ZString IncoTerm => TermsOfDeliveryList.Codes.FAS;

		public ZString Calculate(ICommonInvoice invoice)
		{
			var result = IncoTerm;
			if (invoice.HasChargesExcludedInITOT(USCustomsChargeTypeList.Codes.PackingCost)
				|| invoice.HasChargesExcludedInITOT(USCustomsChargeTypeList.Codes.ForeignInlandFreight))
			{
				result = new EXWIncoTerm().Calculate(invoice);
			}
			return result;
		}
	}
}
