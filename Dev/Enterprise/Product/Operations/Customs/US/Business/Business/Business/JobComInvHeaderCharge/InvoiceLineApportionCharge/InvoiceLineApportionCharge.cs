using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public partial class InvoiceLineApportionCharge : AutoInvoiceLineApportionCharge, Integration.Customs.US.IInvoiceLineApportionCharge
	{
		public InvoiceLineApportionCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString J7_ChargeType
		{
			get { return base.J7_ChargeType; }
			set
			{
				bool hasChanges = base.J7_ChargeType != value;
				base.J7_ChargeType = value;
				if (hasChanges && !IsCopying && InvoiceLine != null)
				{
					InvoiceLine.MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool J7_IsIncludedInITOT
		{
			get { return base.J7_IsIncludedInITOT; }
			set
			{
				bool hasChanges = base.J7_IsIncludedInITOT != value;
				base.J7_IsIncludedInITOT = value;
				if (hasChanges && !IsCopying && InvoiceLine != null)
				{
					InvoiceLine.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid J7_ParentID
		{
			get { return base.J7_ParentID; }
			set
			{
				bool hasChanges = base.J7_ParentID != value;
				base.J7_ParentID = value;
				if (hasChanges && !IsCopying && InvoiceLine != null)
				{
					InvoiceLine.MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool J7_IsDutiable
		{
			get { return base.J7_IsDutiable; }
			set
			{
				bool hasChanges = base.J7_IsDutiable != value;
				base.J7_IsDutiable = value;
				if (hasChanges && !IsCopying && InvoiceLine != null)
				{
					InvoiceLine.MarkAsNeedingValidation();
				}
			}
		}
	}
}
