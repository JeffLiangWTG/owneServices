using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.MarketingManager.Business
{
	public class BulkCommunicationValidation : ZValidation
	{
		public BulkCommunicationValidation(BulkCommunication parent)
			: base(parent)
		{
			if (Object.ReferenceEquals(parent, null))
			{
				throw new ArgumentNullException(nameof(parent));
			}
			this.Parent = parent;
			this.ParentListInternals = parent;
			this.ZValidationInternals = this;
		}

		public override Type AutoValidationType
		{
			get { return typeof(BulkCommunicationValidation); }
		}

		#region TypeOfCall Validation

		public void ValidateTypeOfCall()
		{
			ZValidationInternals.Validate(Parent.TypeOfCallInfo, GetTypeOfCallValidationInvoker());
		}

		RunValidationInvoker GetTypeOfCallValidationInvoker()
		{
			return delegate
			{
				CheckTypeOfCallIsWesternEuropean();
				CheckTypeOfCall();
			};
		}

		void CheckTypeOfCallIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.TypeOfCallInfo);
		}

		void CheckTypeOfCall()
		{
			if (OrganisationsDataRegistry.Instance.CommunicationMandatoryFields.Value.GetBoolFromCode(Parent.TypeOfCall))
			{
				MandatoryValidation.CheckEntered(Parent.TypeOfCallInfo);
			}

			ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(Parent.TypeOfCallInfo, Parent.TypeOfCall_List, Parent.TypeOfCall_ActiveList);
		}

		#endregion

		#region Category Validation

		public void ValidateCategory()
		{
			ZValidationInternals.Validate(Parent.CategoryInfo, GetCategoryValidationInvoker());
		}

		RunValidationInvoker GetCategoryValidationInvoker()
		{
			return delegate
			{
				CheckCategory();
			};
		}

		void CheckCategory()
		{
			if (OrganisationsDataRegistry.Instance.CommunicationMandatoryFields.Value.GetBoolFromCode(Parent.Category))
			{
				MandatoryValidation.CheckEntered(Parent.CategoryInfo);
			}

			ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(Parent.CategoryInfo, Parent.Category_List, Parent.Category_ActiveList);
		}

		#endregion

		#region Status Validation

		public void ValidateStatus()
		{
			ZValidationInternals.Validate(Parent.StatusInfo, GetStatusValidationInvoker());
		}

		RunValidationInvoker GetStatusValidationInvoker()
		{
			return delegate
			{
				CheckStatus();
			};
		}

		void CheckStatus()
		{
			if (OrganisationsDataRegistry.Instance.CommunicationMandatoryFields.Value.GetBoolFromCode(Parent.Status))
			{
				MandatoryValidation.CheckEntered(Parent.StatusInfo);
			}

			if (OrganisationsDataRegistry.Instance.CommunicationMandatoryFields.Value.GetBoolFromCode(OrganisationsDataRegistry.CommunicationMandatoryFieldsClosedStatusRequiredCode))
			{
				if (!Parent.CallDate.IsEmpty && !Parent.IsClosed)
				{
					Parent.StatusInfo.AddError(Res.GetString("f0a59758-3fc9-4f3f-8b6e-3aa89498e7b2", "Please enter a closed status."));
				}
			}

			ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(Parent.StatusInfo, Parent.Status_List, Parent.Status_ActiveList);
		}

		#endregion

		#region StaffCoordinator Validation

		public void ValidateStaffCoordinator()
		{
			ZValidationInternals.Validate(Parent.StaffCoordinatorInfo, GetStaffCoordinatorValidationInvoker());
		}

		RunValidationInvoker GetStaffCoordinatorValidationInvoker()
		{
			return delegate
			{
				CheckStaffCoordinator();
			};
		}

		void CheckStaffCoordinator()
		{
			ListValidation.ErrorIfInvalidCode(Parent.StaffCoordinatorInfo);
		}

		#endregion

		#region NextCallLocal

		public void ValidateNextCallLocal()
		{
			ZValidationInternals.Validate(Parent.NextCallLocalInfo, GetNextCallLocalValidationInvoker());
		}

		RunValidationInvoker GetNextCallLocalValidationInvoker()
		{
			return delegate
			{
				CheckNextCallLocalIsValidZDateTime();
				CheckNextCallLocalIsValidZDateTimeRange();
				CheckNextCallLocal();
			};
		}

		protected void CheckNextCallLocalIsValidZDateTime()
		{
			TypeValidation.CheckValidSmallDateTime(Parent.NextCallLocalInfo);
		}

		protected void CheckNextCallLocalIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.NextCallLocalInfo);
		}

		protected void CheckNextCallLocal()
		{
			if (Parent.NextCallLocal.IsEmpty && Parent.CallDate.IsEmpty)
			{
				Parent.NextCallLocalInfo.AddError(Res.GetString("8321b5f2-53e2-44f7-b2c0-ee310a0369b0", "Please enter a scheduled date or actual date."));
			}
		}

		#endregion

		#region CallDate

		public void ValidateCallDate()
		{
			ZValidationInternals.Validate(Parent.CallDateInfo, GetCallDateValidationInvoker());
		}

		RunValidationInvoker GetCallDateValidationInvoker()
		{
			return delegate
			{
				CheckCallDateIsValidZDateTime();
				CheckCallDateIsValidZDateTimeRange();
				CheckCallDate();
			};
		}

		protected void CheckCallDateIsValidZDateTime()
		{
			TypeValidation.CheckValidSmallDateTime(Parent.CallDateInfo);
		}

		protected void CheckCallDateIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.CallDateInfo);
		}

		protected void CheckCallDate()
		{
			if (Parent.NextCallLocal.IsEmpty && Parent.CallDate.IsEmpty)
			{
				Parent.CallDateInfo.AddError(Res.GetString("be24f55c-9571-46c4-88a4-9097d8a60c5c", "Please enter a scheduled date or actual date."));
			}
		}

		#endregion

		public override void ValidateAll()
		{
			using (ParentListInternals.SuspendListChanged())
			{
				ValidateTypeOfCall();
				ValidateCategory();
				ValidateStatus();
				ValidateStaffCoordinator();
				ValidateNextCallLocal();
				ValidateCallDate();
			}
		}

		void ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(ZPropertyInfo info, ICodeDescriptionPairList allCodes, ICodeDescriptionPairList activeCodes)
		{
			if (!info.BizObj.IsInDatabase || info.HasChanges || !allCodes.ContainsCode(info.Value))
			{
				ListValidation.ErrorIfInvalidCode(info, activeCodes);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(info, activeCodes, ListValidation.InactiveCodeMessage);
			}
		}

		#region Implementation

		protected readonly BulkCommunication Parent;
		readonly ISingleElementListInternal ParentListInternals;
		readonly IValidationInternals ZValidationInternals;

		#endregion
	}
}
