using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbLinehaulManifestLookups : AutoDtbLinehaulManifestLookups
	{
		public DtbLinehaulManifestLookups(AutoDtbLinehaulManifest parent) : base(parent)
		{
		}

		public OrganisationsFindBoxCollection Depots
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		public CodeDescriptionPairList StatusList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("PLN", Res.GetString("1c0a5e65-3ed0-4e91-b323-ed46a6dc0eab", "Planned / Scheduled"));
				result.AddPair("INP", Res.GetString("730d2746-9729-4259-a7e3-f4723ccf4b28", "In Progress / Open"));
				result.AddPair("ARV", Res.GetString("fb7f8182-907c-4226-8819-6d05569d029e", "Arrived / Closed"));
				return result;
			}
		}

		public OrganisationsFindBoxCollection TransportCompanies
		{
			get { return new LineHaulShippingProviderCollection(Factory); }
		}

		public DtbBookingConsignmentCollection Consignments
		{
			get { return new DtbBookingConsignmentCollection(Factory); }
		}

		#region CarrierServiceLevel_List

		public override OrgCarrierServiceLevelCollection CarrierServiceLevels
		{
			get { return Factory.GetCachedValue("ConsignmentRunSheet|BindToLists", () => new CachedProperty<OrgCarrierServiceLevelCollection>(Factory, GetCarrierServiceLevels)).Value; }
		}

		OrgCarrierServiceLevelCollection GetCarrierServiceLevels()
		{
			var carrier = ((DtbLinehaulManifest)Parent).TransportCompany;
			var result = carrier != null ? new OrgCarrierServiceLevelCollection(carrier.MiscServ) : new OrgCarrierServiceLevelCollection(Factory);
			result.Load();

			return result;
		}

		#endregion
	}
}
