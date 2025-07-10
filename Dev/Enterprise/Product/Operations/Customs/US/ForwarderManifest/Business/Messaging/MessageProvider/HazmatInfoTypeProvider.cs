using System.Collections.ObjectModel;
using CargoWise.Customs.US.MessageContracts.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class HazmatInfoTypeProvider : IHazmatInfoType
	{
		readonly UNDGDataItem undg;

		public HazmatInfoTypeProvider(UNDGDataItem undg)
		{
			this.undg = undg;
		}

		public IManifestStringType HazmatCode => new ManifestStringTypeProvider(undg.DI_DG_NKSubs);

		public IManifestStringType HazmatClassCode => new ManifestStringTypeProvider(undg.DI_IMOClass);

		public IManifestStringType HazmatDescription => new ManifestStringTypeProvider(undg.Subs?.DG_PSN);

		public IManifestStringType HazmatContactName => new ManifestStringTypeProvider(undg.DGContact?.OC_ContactName);

		public IManifestStringType HazmatContactPhoneNumber => new ManifestStringTypeProvider(undg.DGContact?.OC_Phone);

		public IManifestStringType HazmatFlashpointTemperature => new ManifestStringTypeProvider(undg.DI_DGFlashPoint.ToZInt().ToString());

		public IManifestStringType TemperatureUnitOfMeasureCode => new ManifestStringTypeProvider("C");

		public Collection<IErrorType> ResponseMessage => new Collection<IErrorType>(System.Array.Empty<ErrorTypeProvider>());
	}
}
