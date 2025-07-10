//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobCartageRunSheetLookups
//
//    This class should be used for overriding collections in AutoJobCartageRunSheetLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Common.Business
{
	public class LocalCartageJobLegTypeLookups : AutoLocalCartageJobLegTypeLookups
	{
		public LocalCartageJobLegTypeLookups(AutoLocalCartageJobLegType parent)
			: base(parent)
		{
		}

		public CommonCartageOrgCollection OrganisationList
		{
			get
			{
				if (fOrganisations == null)
				{
					//ResetOrgLists();
					fOrganisations = Parent.CommonCartageType != null ? Parent.CommonCartageType.CommonCartageOrganisations : new CommonCartageOrgCollection(Factory);
				}
				return fOrganisations;
			}
		}
		CommonCartageOrgCollection fOrganisations;

		public CodeDescriptionPairList EquipmentGroupList
		{
			get
			{
				if (Parent.IsContainerised)
				{
					return new FCLEquipmentNeededList(false);
				}
				else if (Parent.IsLoose)
				{
					return new LCLAIREquipmentNeededList(false);
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}

		new CommonCartageLegType Parent
		{
			get { return (CommonCartageLegType)base.Parent; }
		}
	}
}
