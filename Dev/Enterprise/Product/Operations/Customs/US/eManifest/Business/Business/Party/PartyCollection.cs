using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class PartyCollection : ActiveBusinessObjectCollection<Party>
	{
		public PartyCollection(Shipment master)
			: base(master.Factory, master, new ZQuery(JobDocAddressSchema.E2_ParentTableCode, master.TablePrefix), JobDocAddressSchema.E2_ParentID)
		{
		}

		protected override bool MatchesFilterCore(Party element, bool fetchOnlyFromLocalCache)
		{
			return base.MatchesFilterCore(element, fetchOnlyFromLocalCache)
				   && !PartiesExposedAsSeparateFields.Contains(element.E2_AddressType);
		}

		internal static IEnumerable<ZString> PartiesExposedAsSeparateFields
		{
			get { return new ZString[] { PartyTypes.Codes.Consignee, PartyTypes.Codes.Shipper }; }
		}
	}
}
