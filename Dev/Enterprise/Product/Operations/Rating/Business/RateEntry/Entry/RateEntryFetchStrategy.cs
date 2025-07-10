using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class RateEntryFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public RateEntryFetchStrategy(RateEntry entry)
			: base(entry)
		{
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();

			AddFetchHintForLocation(Entry.TI_OriginLRC);
			AddFetchHintForLocation(Entry.TI_ViaLRC);
			AddFetchHintForLocation(Entry.TI_DestinationLRC);
			Entry.ShouldRatingHeaderSkipFetchHints = true;
		}

		void AddFetchHintForLocation(ZString locationCode)
		{
			SchemaColumn locationSchemaColumn = null;

			switch (LocationHelper.GetLocationType(locationCode))
			{
				case LocationHelper.LocationType.Port:
					locationSchemaColumn = RefUNLOCOSchema.RL_Code;
					break;
				case LocationHelper.LocationType.IATACityCode:
					locationSchemaColumn = RefUNLOCOSchema.RL_IATARegionCode;
					break;
				case LocationHelper.LocationType.Country:
					locationSchemaColumn = RefCountrySchema.RN_Code;
					break;
				case LocationHelper.LocationType.Zone:
					locationSchemaColumn = RefUNLOCOSchema.RL_Code;
					break;
				default:
					return;
			}

			Factory.AddFetchHint(locationSchemaColumn, locationCode);
		}

		RateEntry Entry => (RateEntry)BusinessObject;
	}
}

