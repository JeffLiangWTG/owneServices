using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class PartyLookups : JobDocAddressLookups
	{
		public PartyLookups(Party parent)
			: base(parent)
		{
		}

		new Party Parent
		{
			get { return (Party)base.Parent; }
		}

		public ICodeDescriptionPairList PartyTypes
		{
			get
			{
				return Factory.GetCachedValue(
					"US.eManifest.PartyTypes",
					() =>
						{
							var result = new PartyTypes();
							foreach (var partyType in PartyCollection.PartiesExposedAsSeparateFields)
							{
								result.RemoveCode(partyType);
							}
							return result;
						});
			}
		}

		public ICodeDescriptionPairList PartyIdTypes
		{
			get { return Factory.GetCachedValue<PartyIdTypes>(); }
		}

		public IBusinessObjectCollection Organizations
		{
			get { return Parent.Shipment.Trip.Lookups.Organizations; }
		}
	}
}
