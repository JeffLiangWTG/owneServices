using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public abstract class CASCCode : CusCodeData
	{
		public CASCCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public JobComInvoiceLine InvoiceLine => invoiceLine ?? (invoiceLine = Factory.Load<JobComInvoiceLine>(CY_ParentID));
		JobComInvoiceLine invoiceLine;

		public abstract string CusCodeDataType { get; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataType;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(JobComInvoiceLine)); }
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new CASCCodeValidation(this);
		}

		public override ZGuid CY_ParentID
		{
			get => base.CY_ParentID;
			set
			{
				var invLine = InvoiceLine;
				var oldValue = CY_ParentID;
				base.CY_ParentID = value;
				if (!IsCopying && oldValue != CY_ParentID)
				{
					if (!CY_ParentID.IsValid)
					{
						DetachedLine();
					}
					else
					{
						AttachedLine();
					}

					invLine?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZShort CY_Order
		{
			get => base.CY_Order;
			set
			{
				var oldValue = CY_Order;
				base.CY_Order = ShortSequenceNumberGenerator.EnsureValidSequenceNumber(value);
				if (!IsCopying && oldValue != CY_Order)
				{
					RenumberLine(oldValue);
				}
			}
		}

		protected virtual void RenumberLine(ZShort oldOrderNo)
		{
		}

		protected virtual void DetachedLine()
		{
		}

		protected virtual void AttachedLine()
		{
		}
	}
}
