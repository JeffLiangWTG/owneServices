//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobCartageRunSheetValidation
//
//    This class should be used for overriding validation in AutoJobCartageRunSheetValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Common.Business
{
	public class LocalCartageJobOrgValidation : AutoLocalCartageJobOrgValidation
	{
		public LocalCartageJobOrgValidation(AutoLocalCartageJobOrg parent)
			: base(parent)
		{
		}

		protected override void CheckE5_UsageComment()
		{
			base.CheckE5_UsageComment();
			//check if other field has this same error otherwise the same error appears for both fields
			if (!IsOrgTypeAndUsageCommentUnique() &&
				!Parent.E5_OrgTypeInfo.HasError(UniqueOrgTypeAndUsageCommentError))
			{
				Parent.E5_UsageCommentInfo.AddError(UniqueOrgTypeAndUsageCommentError);
			}
		}
		protected override void CheckE5_OrgType()
		{
			base.CheckE5_OrgType();
			ListValidation.ErrorIfInvalidCode(Parent.E5_OrgTypeInfo, LocalCartageJobOrgTypeList.Instance);

			//check if other field has this same error otherwise the same error appears for both fields
			if (!IsOrgTypeAndUsageCommentUnique() &&
				!Parent.E5_UsageCommentInfo.HasError(UniqueOrgTypeAndUsageCommentError))
			{
				Parent.E5_OrgTypeInfo.AddError(UniqueOrgTypeAndUsageCommentError);
			}
		}

		protected override void CheckE5_IsBillToParty()
		{
			base.CheckE5_IsBillToParty();

			if (CommonCartageType != null)
			{
				CommonCartageOrgCollection orgs = CommonCartageType.CommonCartageOrganisations;

				int count = 0;
				foreach (CommonCartageOrg org in orgs)
				{
					if (org.E5_IsBillToParty)
					{
						count++;
					}
				}

				if (count > 1 && Parent.E5_IsBillToParty)
				{
					Parent.E5_IsBillToPartyInfo.AddError(OnlyOneIsBillToPartyError);
				}
				else if (count == 0 && !Parent.E5_IsBillToParty)
				{
					Parent.E5_IsBillToPartyInfo.AddWarning(AtleastOneIsBillToPartyWarning);
				}
			}
		}

		internal static string UniqueOrgTypeAndUsageCommentError
		{
			get { return Res.GetString("ab67e459-c014-437e-99b7-3f58d7423f01", "Type and Usage need to be unique in the organizations list."); }
		}
		internal static string OnlyOneIsBillToPartyError
		{
			get { return Res.GetString("820f2b44-6cc4-4a26-95fa-477f8259efed", "Only one of the organizations should have 'Is Bill To Party' ticked."); }
		}
		internal static string AtleastOneIsBillToPartyWarning
		{
			get { return Res.GetString("06f852f7-9172-4b39-b7ae-cbd14c468b89", "If no Organization is nominated as the Billing Party, no Local Client will default on a Port Transport."); }
		}

		#region implementation

		CommonCartageType CommonCartageType
		{
			get { return ((CommonCartageOrg)Parent).CommonCartageType; }
		}

		bool IsOrgTypeAndUsageCommentUnique()
		{
			bool result = true;

			if (CommonCartageType != null)
			{
				CommonCartageOrgCollection orgs = CommonCartageType.CommonCartageOrganisations;
				if (orgs != null)
				{
					foreach (CommonCartageOrg org in orgs)
					{
						if (org.PK != Parent.PK && org.E5_UsageComment == Parent.E5_UsageComment
								&& org.E5_OrgType == Parent.E5_OrgType)
						{
							result = false;
							break;
						}
					}
				}
			}
			return result;
		}

		#endregion

	}
}
