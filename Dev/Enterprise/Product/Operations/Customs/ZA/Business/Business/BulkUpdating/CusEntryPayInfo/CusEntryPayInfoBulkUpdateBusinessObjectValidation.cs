using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class CusEntryPayInfoBulkUpdateBusinessObjectValidation : ZValidation
	{
		public CusEntryPayInfoBulkUpdateBusinessObjectValidation(CusEntryPayInfoBulkUpdateBusinessObject parent) : base(parent)
		{
			if (Object.ReferenceEquals(parent, null))
			{
				throw new ArgumentNullException(nameof(parent));
			}
			this.parent = parent;
			this.parentListInternals = parent;
			this.zValidationInternals = this;
		}

		readonly CusEntryPayInfoBulkUpdateBusinessObject parent;
		readonly ISingleElementListInternal parentListInternals;
		readonly IValidationInternals zValidationInternals;

		public override Type AutoValidationType
		{
			get { return typeof(CusEntryPayInfoBulkUpdateBusinessObjectValidation); }
		}

		public override void ValidateAll()
		{
			using (parentListInternals.SuspendListChanged())
			{
				ValidateReceiptNumber();
				ValidateReceiptDate();
			}
		}

		public void ValidateReceiptNumber()
		{
			zValidationInternals.Validate(parent.ReceiptNumberInfo, GetReceiptNumberValidationInvoker());
		}

		RunValidationInvoker GetReceiptNumberValidationInvoker()
		{
			return delegate
			{
				CheckReceiptNumberIsWesternEuropean();
				CheckReceiptNumber();
			};
		}

		void CheckReceiptNumberIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(parent.ReceiptNumberInfo);
		}

		void CheckReceiptNumber()
		{
			if (parent != null)
			{
				MandatoryValidation.CheckEntered(parent.ReceiptNumberInfo);
				if (parent.ReceiptNumber.IsEmpty && !parent.ReceiptDate.IsEmpty)
				{
					parent.ReceiptNumberInfo.AddError(ValidationConstants.CusEntryPayInfo.ReceiptNumberRequiredWithDate);
				}
			}
		}

		public void ValidateReceiptDate()
		{
			zValidationInternals.Validate(parent.ReceiptDateInfo, GetReceiptDateValidationInvoker());
		}

		RunValidationInvoker GetReceiptDateValidationInvoker()
		{
			return delegate
			{
				CheckReceiptDate();
			};
		}

		void CheckReceiptDate()
		{
			if (parent != null)
			{
				MandatoryValidation.CheckEntered(parent.ReceiptDateInfo);
				if (!parent.ReceiptDate.IsEmpty)
				{
					if (parent.ReceiptDate > ZDate.Today)
					{
						parent.ReceiptDateInfo.AddError(ValidationConstants.CusEntryPayInfo.ReceiptDateCannotBeGreaterThantoday);
					}
				}
				else if (!parent.ReceiptNumber.IsEmpty)
				{
					parent.ReceiptDateInfo.AddError(ValidationConstants.CusEntryPayInfo.ReceiptDateRequiredWithNumber);
				}
			}
		}
	}
}
