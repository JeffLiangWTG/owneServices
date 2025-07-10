//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSLinkedEntryAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSLinkedEntryAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business
{
	public class USLinkedEntryAddInfoLookups : AutoUSLinkedEntryAddInfoLookups
	{
		public USLinkedEntryAddInfoLookups(AutoUSLinkedEntryAddInfo parent) : base(parent)
		{
		}

		USLinkedEntryAddInfo LinkedEntryAddInfo
		{
			get { return (USLinkedEntryAddInfo)Parent; }
		}

		public IBusinessObjectCollection SchDPortList
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public ModuleEntryHeaderCollection Entries
		{
			get
			{
				var result = new ModuleEntryHeaderCollection(Factory);

				if (LinkedEntryAddInfo.US_LE_EntryNumber.Length > 3)
				{
					var filterDefaults = new FilterBusinessObjectDefault(ModuleEntryHeaderCollection.USFilterConstants.EntryNumber, "Property", LinkedEntryAddInfo.US_LE_EntryNumber.SubstringSafe(3));
					result.FilterBusinessObjectDefaults.Add(filterDefaults);
				}
				else
				{
					var declaration = LinkedEntryAddInfo.LinkedEntry.Declaration;
					if (declaration != null && declaration.Protest != null && declaration.Protest.Protestant != null)
					{
						var filterBODefault = new FilterBusinessObjectDefault(ModuleEntryHeaderCollection.USFilterConstants.ImporterOfRecord, "Property", declaration.Protest.Protestant.OrganisationPK);
						result.FilterBusinessObjectDefaults.Add(filterBODefault);
					}
					var filterImportDateDefault = new FilterBusinessObjectDefault("Import Date", "Property1", ZDateTime.Empty);
					result.FilterBusinessObjectDefaults.Add(filterImportDateDefault);
				}

				return result;
			}
		}
	}
}
