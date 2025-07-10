//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefEquipmentLookups
//
//    This class should be used for overriding collections in AutoRefEquipmentLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefEquipmentLookups : AutoRefEquipmentLookups
	{
		public RefEquipmentLookups(AutoRefEquipment parent)
			: base(parent)
		{
		}

		protected new RefEquipment Parent
		{
			get { return (RefEquipment)base.Parent; }
		}

		public CodeDescriptionPairList RQ_EquipmentGroup_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.EquipmentGroup); }
		}

		#region RQ_F3_NKPackType_List

		public RefPackTypeCollection RQ_F3_NKPackType_List
		{
			get { return Factory.GetCachedValue("RefPackTypeCollection", () => new RefPackTypeCollection(Factory, false)); }
		}

		public CodeDescriptionPairList GPSProviders
		{
			get { return Factory.GetCachedValue<GPSProviderList>(); }
		}

		public OrgHeaderCollection RQ_OH_OwnerList
		{
			get { return Factory.GetCachedValue("OrgHeaderCollection", () => new OrgHeaderCollection(Factory)); }
		}

		public RefCountryStatesDependentCollection RefCountryStatesList
		{
			get
			{
				var regoCountry = Parent.RQ_RN_NKRegistrationCountry;
				return Factory.GetCachedValue("RefCountryStatesDependentCollection-" + regoCountry, () =>
				{
					var refCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, regoCountry);
					var refCountryStates = new RefCountryStatesDependentCollection(refCountry, Factory);
					refCountryStates.Load();
					return refCountryStates;
				});
			}
		}

		#endregion
	}
}
