//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbPersonPrimaryRelationshipValidation
//
//    This class should be used for overriding validation in AutoGlbPersonPrimaryRelationshipValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbPersonPrimaryRelationshipValidation : AutoGlbPersonPrimaryRelationshipValidation
	{
		public GlbPersonPrimaryRelationshipValidation(AutoGlbPersonPrimaryRelationship parent) : base(parent)
		{
		}

		protected new GlbPersonPrimaryRelationship Parent
		{
			get { return (GlbPersonPrimaryRelationship)base.Parent; }
		}

		protected override void CheckPPR_PER()
		{
			if (Parent.Primary?.PersonPK != Parent.PPR_PER)
			{
				Parent.PPR_PERInfo.AddError(Res.GetString("9726f292-cc3e-4e87-ae9b-d069b7a8f389", "The primary's person does not match the primary relationship's person. This must be fixed before saving."));
			}
		}

		protected override void CheckPPR_PrimaryId()
		{
			if (Parent.Primary == null || Parent.Primary.TableCode != Parent.PPR_PrimaryTableCode)
			{
				if (Parent.PPR_PrimaryId.IsValid)
				{
					Parent.PPR_PrimaryIdInfo.AddError(Res.GetString("cdf0e590-5dd6-4bec-a2e9-ab050589236d", "The primary Id ({0}) does not match the primary table code ({1}).", Parent.PPR_PrimaryId, Parent.PPR_PrimaryTableCode));
				}
			}
		}

		protected override void CheckPPR_PrimaryTableCode()
		{
			if (Parent.PPR_PrimaryTableCode != OrgContactSchema.Constants.Prefix || Parent.PPR_PrimaryTableCode != GlbStaffSchema.Constants.Prefix)
			{
				Parent.PPR_PrimaryTableCodeInfo.AddError(Res.GetString("d0fb8e2a-d64b-4916-94c0-eba2b6e77551", "The primary table code must be either {0} or {1}.", OrgContactSchema.Constants.Prefix, GlbStaffSchema.Constants.Prefix));
			}
		}
	}
}
