//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHVLVBookingHeaderLookups
//
//    This class should be used for overriding collections in AutoHVLVBookingHeaderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business
{
	public class HVLVBookingHeaderLookups : AutoHVLVBookingHeaderLookups
	{
		public HVLVBookingHeaderLookups(AutoHVLVBookingHeader parent)
			: base(parent)
		{ }

		HVLVBookingHeader BookingHeader => (HVLVBookingHeader)Parent;

		public CodeDescriptionPairList VolumeUQList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		public CodeDescriptionPairList WeightUQList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public OrganisationsFindBoxCollection OrgList => orgList ?? (orgList = new OrganisationsFindBoxCollection(Factory));
		OrganisationsFindBoxCollection orgList;

		public OrganisationsFindBoxCollection DebtorOrgList => debtorOrgList ?? (debtorOrgList = new DebtorCollection(Factory));
		OrganisationsFindBoxCollection debtorOrgList;

		public OrganisationsFindBoxCollection PackDepotOrgList => packDepotOrgList ?? (packDepotOrgList = new PackDepotCollection(Factory));
		OrganisationsFindBoxCollection packDepotOrgList;

		public override OrgContactCollection BookedBys
		{
			get
			{
				var billToPartyHeader = BookingHeader.BillToParty?.Header;

				if (bookedBys == null || billToPartyHeader != BookedByContactHeader)
				{
					BookedByContactHeader = billToPartyHeader;
					bookedBys = new OrgContactCollection(Factory, new ZQuery(OrgContactSchema.OC_OH, billToPartyHeader?.PK ?? CargoWise.Types.ZGuid.Empty));
					bookedBys.Load();
				}

				return bookedBys;
			}
		}

		OrgContactCollection bookedBys;
		OrgHeader BookedByContactHeader;

		public CodeDescriptionPairList BookingStatusList => bookingStatusList ?? (bookingStatusList = HVLVBookingStatus.GetAll());
		CodeDescriptionPairList bookingStatusList;

		public CodeDescriptionPairList DeniedPartyScreeningStatusList => Factory.GetCachedValue<ScreeningStatusesList>();
	}
}
