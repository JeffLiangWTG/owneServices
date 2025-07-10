using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class AddInfoDataObjectReaderForDrawbackAdditionalImportTariffNumber : AddInfoDataObjectReader<DrawbackAdditionalImportTariffNumber>
	{
		public AddInfoDataObjectReaderForDrawbackAdditionalImportTariffNumber(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger, helper, CusAddInfoSchema.B7_AddInfoData, DrawbackAdditionalImportTariffNumberAddInfoSchema.Instance)
		{
		}

		protected override void AfterUpdateRelatedPropertyCompletedCore(IAddInfoManager addInfoManager)
		{
			base.AfterUpdateRelatedPropertyCompletedCore(addInfoManager);

			if (addInfoManager is DrawbackAdditionalImportTariffNumber additionalImportTariff && additionalImportTariff.InvoiceLine is JobComInvoiceLine invoiceLine && invoiceLine.IsDrawback)
			{
				invoiceLine.DrawbackAdditionalImportTariffNumbers.Reload(false);
				invoiceLine.RefreshUS_DRWAdValoremRate();
			}
		}
	}
}
