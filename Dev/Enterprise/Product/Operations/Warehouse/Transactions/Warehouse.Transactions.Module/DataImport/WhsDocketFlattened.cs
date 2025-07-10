using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class WhsDocketFlattened : AutoWhsDocketFlattened
	{
		public WhsDocketFlattened()
		{
		}

		[EmailAddress]
		public override ZString ConsigneeAddress_E2_Email
		{
			get
			{
				return base.ConsigneeAddress_E2_Email;
			}

			set
			{
				base.ConsigneeAddress_E2_Email = value;
			}
		}

		[EmailAddress]
		public override ZString DropOffAddress_E2_Email
		{
			get
			{
				return base.DropOffAddress_E2_Email;
			}

			set
			{
				base.DropOffAddress_E2_Email = value;
			}
		}

		[EmailAddress]
		public override ZString PickupAddress_E2_Email
		{
			get
			{
				return base.PickupAddress_E2_Email;
			}

			set
			{
				base.PickupAddress_E2_Email = value;
			}
		}

		[EmailAddress]
		public override ZString SupplierAddress_E2_Email
		{
			get
			{
				return base.SupplierAddress_E2_Email;
			}

			set
			{
				base.SupplierAddress_E2_Email = value;
			}
		}

		public ZDate Line_WE_ExpiryDate
		{
			get => line_WE_ExpiryDate;
			set
			{
				SetNonPersistentPropertyValue(Line_WE_ExpiryDateInfo, ref line_WE_ExpiryDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateLine_WE_ExpiryDate();
				}
			}
		}

		public ZPropertyInfo Line_WE_ExpiryDateInfo => GetZPropertyInfo(nameof(Line_WE_ExpiryDate));
		ZDate line_WE_ExpiryDate;

		public ZDate Line_WE_PackingDate
		{
			get => line_WE_PackingDate;
			set
			{
				SetNonPersistentPropertyValue(Line_WE_PackingDateInfo, ref line_WE_PackingDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateLine_WE_PackingDate();
				}
			}
		}

		public ZPropertyInfo Line_WE_PackingDateInfo => GetZPropertyInfo(nameof(Line_WE_PackingDate));
		ZDate line_WE_PackingDate;
	}
}
