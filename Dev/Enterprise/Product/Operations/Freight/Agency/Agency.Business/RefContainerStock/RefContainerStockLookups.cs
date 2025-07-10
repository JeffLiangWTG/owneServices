//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefContainerStockLookups
//
//    This class should be used for overriding collections in AutoRefContainerStockLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public class RefContainerStockLookups : AutoRefContainerStockLookups
	{
		public RefContainerStockLookups(AutoRefContainerStock parent)
			: base(parent) { }

		public override OrgHeaderCollection Owners
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		public override RefContainerCollection Containers
		{
			get { return new RefContainerCollection(Factory, Core.Constants.TransportModes.Sea); }
		}

		public CodeDescriptionPairList OwnerTypes
		{
			get { return GetOwnerTypes(Factory); }
		}

		public static CodeDescriptionPairList GetOwnerTypes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("RefContainerStockLookups.OwnerTypes", () => new ContainerOwnershipList());
		}
	}
}
