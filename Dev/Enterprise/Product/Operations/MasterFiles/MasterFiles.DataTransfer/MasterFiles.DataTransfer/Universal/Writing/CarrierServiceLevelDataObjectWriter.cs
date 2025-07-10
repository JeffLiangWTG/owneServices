using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class CarrierServiceLevelDataObjectWriter : DataObjectWriter<BusinessObject, ServiceLevel>
	{
		public CarrierServiceLevelDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		protected override ServiceLevel PopulateDataObject(BusinessObject serviceLevelBO)
		{
			var carrierServiceLevelBO = (OrgCarrierServiceLevel)serviceLevelBO;
			return new ServiceLevel()
			{
				Code = carrierServiceLevelBO.PL_Code,
				Description = carrierServiceLevelBO.PL_CarrierServiceLevelDescription,
				CarrierServiceCode = carrierServiceLevelBO.PL_CarrierServiceCode,
				CarrierProductCode = carrierServiceLevelBO.PL_ProductCode,
				CarrierChargeCode = carrierServiceLevelBO.PL_ChargeCode,
				CarrierProfileID = carrierServiceLevelBO.PL_APProfileID,
			};
		}
	}
}
