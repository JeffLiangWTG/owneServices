using System;
using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class BulkRateUpdaterContainerTypeValidation : ZValidation
	{
		public BulkRateUpdaterContainerTypeValidation(BulkRateUpdaterContainerType parent)
			: base(parent)
		{
			if (ReferenceEquals(parent, null))
			{
				throw new ArgumentNullException(nameof(parent));
			}

			Parent = parent;
			ParentListInternals = parent;
			ZValidationInternals = this;
		}

		#region Overrides

		public override Type AutoValidationType => typeof(BulkRateUpdaterContainerTypeValidation);

		public override void ValidateAll()
		{
			using (ParentListInternals.SuspendListChanged())
			{
				ValidateRC_Code();
			}
		}

		public void ValidateRC_Code() => ZValidationInternals.Validate(Parent.RC_CodeInfo, GetRC_CodeValidationInvoker());

		RunValidationInvoker GetRC_CodeValidationInvoker()
			=> delegate
			{
				MandatoryValidation.CheckEntered(Parent.RC_CodeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.RC_CodeInfo, Parent.modeRestrictedContainersForValidation);
			};

		#endregion

		#region Implementation

		protected readonly BulkRateUpdaterContainerType Parent;
		readonly ISingleElementListInternal ParentListInternals;
		readonly IValidationInternals ZValidationInternals;

		#endregion
	}
}
