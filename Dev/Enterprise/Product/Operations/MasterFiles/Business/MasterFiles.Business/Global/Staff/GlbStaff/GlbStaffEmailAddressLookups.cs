//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbStaffEmailAddressLookups
//
//    This class should be used for overriding collections in AutoGlbStaffEmailAddressLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffEmailAddressLookups : AutoGlbStaffEmailAddressLookups
	{
		public GlbStaffEmailAddressLookups(AutoGlbStaffEmailAddress parent) : base(parent)
		{
		}

		new GlbStaffEmailAddress Parent => (GlbStaffEmailAddress)base.Parent;

		public CodeDescriptionPairList AllEmailTypeList
		{
			get
			{
				var allEmailTypeList = new CodeDescriptionPairList(SystemDataRegistry.Instance.StaffEmailTypeList.Value);
				allEmailTypeList.AddPair(Core.Constants.EmailFromAddressTypes.Codes.Main, Core.Constants.EmailFromAddressTypes.Descriptions.Main);

				return allEmailTypeList;
			}
		}

		public CodeDescriptionPairList SelectableEmailTypeList
		{
			get
			{
				var selectableEmailTypeList = new CodeDescriptionPairList(SystemDataRegistry.Instance.StaffEmailTypeList.Value);

				if (Parent.EmailTypeFromOtherCompany.HasValue)
				{
					selectableEmailTypeList.AddPair((ZString)Parent.GSE_TypeInfo.OriginalValue, Parent.EmailTypeFromOtherCompany.Value);
				}

				selectableEmailTypeList.SortByDescription();

				return selectableEmailTypeList;
			}
		}
	}
}
