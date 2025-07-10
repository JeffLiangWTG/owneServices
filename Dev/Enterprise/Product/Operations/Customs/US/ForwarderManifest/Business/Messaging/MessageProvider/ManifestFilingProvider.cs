using System.Collections.ObjectModel;
using CargoWise.Customs.US.MessageContracts.Interfaces;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class ManifestFilingProvider : IManifestFiling
	{
		readonly USExportAsycudaBill bill;
		readonly string action;

		public ManifestFilingProvider(USExportAsycudaBill bill, string action)
		{
			this.bill = bill;
			this.action = action;
		}

		public string Version => "";

		public IFilingInfoType Filing => new FilingInfoTypeProvider(bill, "2");

		public Collection<IConveyanceInfoType> Conveyance => new Collection<IConveyanceInfoType> { new ConveyanceInfoTypeProvider(bill, action) };

		public Collection<IErrorType> ResponseMessage => new Collection<IErrorType>(System.Array.Empty<ErrorTypeProvider>());
	}
}
