using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class RateTransportZoneItemCollection : ActiveBusinessObjectCollection<RateTransportZoneItem>, IRateTransportZoneItemCollection, IImportWizardProvider
	{
		public RateTransportZoneItemCollection(RateTransportZone parent)
			: base(parent.Factory, parent, null, RateTransportZoneItemSchema.TQ_TZ_DomesticZone)
		{
		}

		#region IImportWizardProvider
		public ImportWizard GetImportWizard(IImportCollectionInfo collectionInfo, ISettingsStorage settingsStorage, IFileMapper fileMapper)
		{
			return new RateTransportZoneItemImportWizard((Relationship.Master as RateTransportZone).TransportProvider.CountryCode, collectionInfo, settingsStorage, fileMapper);
		}
		#endregion
	}
}

