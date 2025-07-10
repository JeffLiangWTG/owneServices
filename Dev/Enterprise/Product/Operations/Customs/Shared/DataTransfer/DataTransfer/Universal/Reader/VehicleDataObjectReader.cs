using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class VehicleDataObjectReader : DataObjectReader<UniversalCustoms.Vehicle>
	{
		internal VehicleDataObjectReader(UniversalCustoms.Vehicle vehicle, IXmlImportLogger logger, UniversalObjectFactory factory, ZGuid parentPK, ZString parentTable)
			: base(vehicle, logger, factory)
		{
			this.parentPK = parentPK;
			this.parentTable = parentTable;
		}

		public IColumnIndexer ReadIntoDataRow()
		{
			IColumnIndexer row = null;
			if (parentPK != ZGuid.Empty && parentTable != ZString.Empty)
			{
				row = (CusVehicle)factory.New(new CusVehicleTypeDecider().GetTypeForCountryCode(MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode));

				if (row != null)
				{
					SetValue(row, CusVehicleSchema.CVH_ParentID, parentPK);
					SetValue(row, CusVehicleSchema.CVH_ParentTableCode, parentTable);
					SetValue(row, CusVehicleSchema.CVH_BrandName, dataObject.Brand);
					SetValue(row, CusVehicleSchema.CVH_ModelName, dataObject.Model);
					SetValue(row, CusVehicleSchema.CVH_VehicleIdentificationNumber, dataObject.VIN);
					SetValue(row, CusVehicleSchema.CVH_RegistrationNumber, dataObject.RegistrationNumber);
				}
			}
			return row;
		}

		readonly ZGuid parentPK;
		readonly ZString parentTable;
	}
}
