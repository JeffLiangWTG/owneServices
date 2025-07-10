using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryQuota)]
	public partial class QTAU1 : MessageBlock, IVisaQuery
	{
		#region IVisaQuery Members

		ZString IVisaQuery.TariffNumber
		{
			//VisaQueryIndicator == "" > Only quota query, not Visa query
			get { return VisaQueryIndicator.IsEmpty ? ZString.Empty : TariffNumberTextileCategoryNumberOrVisaNumber; }
		}

		ZString IVisaQuery.OriginCountry
		{
			get { return VisaQueryIndicator.IsEmpty ? ZString.Empty : CountryOfOrigin; }
		}

		ZString IVisaQuery.SecondTariffNumber
		{
			get { return VisaQueryIndicator.IsEmpty ? ZString.Empty : SecondTariffNumber; }
		}

		#endregion
	}
}