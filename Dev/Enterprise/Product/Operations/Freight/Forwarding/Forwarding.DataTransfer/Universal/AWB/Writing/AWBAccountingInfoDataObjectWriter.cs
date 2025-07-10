using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.AWB;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class AWBAccountingInfoDataObjectWriter : DataObjectWriter<ExportAWBAccountingInformation, AWBAccountingInfo>
	{
		internal AWBAccountingInfoDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override AWBAccountingInfo PopulateDataObject(ExportAWBAccountingInformation accountingInfo)
		{
			var accountingInfoDataObject = new AWBAccountingInfo();

			accountingInfoDataObject.Information = accountingInfo.EA_Information;
			accountingInfoDataObject.Type = ListHelper.GetWithDescription<CodeDescriptionPair>(accountingInfo.EA_InformationID, accountingInfo.Lookups.AccountingCodes);

			return accountingInfoDataObject;
		}
	}
}
