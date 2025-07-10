//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbAccreditationGroupValidation
//
//    This class should be used for overriding validation in AutoGlbAccreditationGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationGroupValidation : AutoGlbAccreditationGroupValidation
	{
		public GlbAccreditationGroupValidation(AutoGlbAccreditationGroup parent) : base(parent)
		{
		}

		GlbAccreditationGroupCollection AccreditationGroups
		{
			get
			{
				if (accreditationGroups == null)
				{
					accreditationGroups = new GlbAccreditationGroupCollection(Parent.Factory);
				}

				return accreditationGroups;
			}
		}

		GlbAccreditationGroupCollection accreditationGroups;

		protected override void CheckHAG_Description()
		{
			MandatoryValidation.CheckEntered(Parent.HAG_DescriptionInfo);

			if (AccreditationGroups.Any(a => a.PK != Parent.PK && a.HAG_Description.EqualsIgnoringCase(Parent.HAG_Description)))
			{
				Parent.HAG_DescriptionInfo.AddError(Res.GetString("77539ae0-a470-4448-be83-0d619a8ca44b", "Description is not unique. Please enter a unique description."));
			}
		}
	}
}
