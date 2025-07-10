//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefContainerCodeMapLookups
//
//    This class should be used for overriding collections in AutoRefContainerCodeMapLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RefContainerCodeMapLookups : AutoRefContainerCodeMapLookups
	{
		public RefContainerCodeMapLookups(AutoRefContainerCodeMap parent) : base(parent)
		{
		}

		public new RefContainerCodeMap Parent => (RefContainerCodeMap)base.Parent;

		#region UsageList

		public ICodeDescriptionPairList UsageList => Parent.ContainerMapProvider?.GetUsageList(Factory) ?? new CodeDescriptionPairList();

		#endregion

		#region CodeList

		public ICodeDescriptionPairList CodeList => Parent.ContainerMapProvider?.GetCodeList(Factory, Parent.RCM_Usage) ?? new CodeDescriptionPairList();

		#endregion
	}
}
