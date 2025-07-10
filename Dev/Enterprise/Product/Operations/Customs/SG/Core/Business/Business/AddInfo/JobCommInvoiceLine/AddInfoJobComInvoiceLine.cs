
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoJobComInvoiceLine : AddInfo
	{
		public AddInfoJobComInvoiceLine(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		public override ZString SG_InwardHAWB
		{
			get
			{
				return base.SG_InwardHAWB;
			}
			set
			{
				base.SG_InwardHAWB = value;

				if (InvoiceLine.Declaration != null)
				{
					InvoiceLine.Declaration.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString SG_OutwardHAWB
		{
			get
			{
				return base.SG_OutwardHAWB;
			}
			set
			{
				base.SG_OutwardHAWB = value;
				if (InvoiceLine.Declaration != null)
				{
					InvoiceLine.Declaration.MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool SG_IsStrategic
		{
			get
			{
				return base.SG_IsStrategic;
			}
			set
			{
				base.SG_IsStrategic = value;
				if (InvoiceLine.Declaration != null)
				{
					InvoiceLine.Declaration.MarkAsNeedingValidation();
				}
			}
		}

		#region Validation/Lookups

		public new AddInfoJobComInvoiceLineLookups Lookups
		{
			get { return (AddInfoJobComInvoiceLineLookups)base.Lookups; }
		}

		public new AddInfoJobComInvoiceLineValidation Validation
		{
			get { return (AddInfoJobComInvoiceLineValidation)base.Validation; }
		}

		protected override SGAddInfoLookups GetNewLookups()
		{
			return new AddInfoJobComInvoiceLineLookups(this);
		}

		protected override SGAddInfoValidation GetNewValidation()
		{
			SGAddInfoValidation result = null;

			if (InvoiceLine.Declaration != null)
			{
				if (InvoiceLine.Declaration.JE_MessageType == MessageTypeCodeList.Codes.IPT)
				{
					result = new AddInfoJobComInvoiceLineValidation_IPT(this);
				}
				else if (InvoiceLine.Declaration.JE_MessageType == MessageTypeCodeList.Codes.INP)
				{
					result = new AddInfoJobComInvoiceLineValidation_INP(this);
				}
				else if (InvoiceLine.Declaration.JE_MessageType == MessageTypeCodeList.Codes.TNP)
				{
					result = new AddInfoJobComInvoiceLineValidation_TNP(this);
				}
				else if (InvoiceLine.Declaration.IsOUTDEC)
				{
					result = InvoiceLine.Declaration.SG_ApplicationProductType != "" ? new AddInfoJobComInvoiceLineValidation_OUTwCO(this) : new AddInfoJobComInvoiceLineValidation_OUT(this);
				}
				else if (InvoiceLine.Declaration.JE_MessageType == MessageTypeCodeList.Codes.COO)
				{
					result = new AddInfoJobComInvoiceLineCOValidation(this);
				}
				else
				{
					result = new AddInfoJobComInvoiceLineValidation(this);
				}
			}
			else
			{
				result = new AddInfoJobComInvoiceLineValidation(this);
			}

			return result;
		}

		#endregion
	}
}
