//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgContactItemValidation
//
//    This class should be used for overriding validation in AutoOrgContactItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgContactItemValidation : AutoOrgContactItemValidation
	{
		public OrgContactItemValidation(AutoOrgContactItem parent)
			: base(parent)
		{
		}

		new OrgContactItem Parent
		{
			get { return (OrgContactItem)base.Parent; }
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			if (info == Parent.OI_OCInfo)
			{
				return false;
			}

			return base.ShouldValidateFKToCancelledRecord(info);
		}
	}
}
