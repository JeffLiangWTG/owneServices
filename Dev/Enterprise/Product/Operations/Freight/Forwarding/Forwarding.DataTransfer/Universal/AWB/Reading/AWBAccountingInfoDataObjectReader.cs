using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.AWB;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class AWBAccountingInfoDataObjectReader : DataObjectReader<AWBAccountingInfo, ExportAWBAccountingInformation>
	{
		public AWBAccountingInfoDataObjectReader(AWBAccountingInfo accountingInfoDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(accountingInfoDataObject, logger, factory)
		{
		}

		protected override ExportAWBAccountingInformation GetExistingBusinessObject()
		{
			return null;
		}

		protected override void PopulateBusinessObject(ExportAWBAccountingInformation accountingInfoBO)
		{
			SetValue(accountingInfoBO, ExportAWBAccountingInformationSchema.EA_Information, dataObject.Information);
			SetValue(accountingInfoBO, ExportAWBAccountingInformationSchema.EA_InformationID, dataObject.Type);
		}
	}
}
