using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class AdditionalMessageInformationLookups : ZLookups
	{
		public AdditionalMessageInformationLookups(AdditionalMessageInformation additionalMessageInformation)
			: base(additionalMessageInformation)
		{
		}
	}
}
