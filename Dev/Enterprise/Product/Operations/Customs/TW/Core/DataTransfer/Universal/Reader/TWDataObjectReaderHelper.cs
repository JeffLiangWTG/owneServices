using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.TW.DataTransfer.Universal
{
	public class TWDataObjectReaderHelper : UniversalDataObjectReaderHelper
	{
		public TWDataObjectReaderHelper(UniversalObjectFactory factory, ZString sourceCountryCode, string dataProviderForCodeMapping = null) : base(factory, Core.Constants.CountryCodes.Taiwan, sourceCountryCode, dataProviderForCodeMapping)
		{
		}

		public override ZString GetCustomsBillType(WayBillType wayBillType)
		{
			return wayBillType.Code.HasValue && wayBillType.Code.Value == Constants.WayBillTypeCode.ContainerNote ? new ZString(Business.BillTypeList.Codes.ContainerNote) : base.GetCustomsBillType(wayBillType);
		}
	}
}
