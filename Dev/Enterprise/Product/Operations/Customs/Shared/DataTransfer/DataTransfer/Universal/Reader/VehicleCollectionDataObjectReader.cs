using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class VehicleCollectionDataObjectReader
	{
		public VehicleCollectionDataObjectReader(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.helper = helper;
		}

		public IColumnIndexer[] ReadIntoDataRows(ZGuid parentPK, ZString parentTableCode, bool? isParentInDatabase, UniversalCustoms.IVehicleCollectionParent iVehicleCollectionParent)
		{
			IColumnIndexer[] result = null;
			var vehicleCollection = iVehicleCollectionParent.VehicleCollection;
			if (!parentPK.IsValid || parentTableCode.IsEmpty)
			{
				logger.Log(LogType.Error, Enterprise.Customs.DataTransfer.Res.GetString("6CEDC390-339F-434E-8915-0CAF90B59550", "Cannot process Vehicle as parent PK '{0}' or parent Table Code '{1}' is either empty or invalid.", parentPK, parentTableCode));
			}
			else
			{
				if (vehicleCollection != null)
				{
					var newRows = new List<IColumnIndexer>();
					var query = new ZQuery(CusVehicleSchema.CVH_ParentID, parentPK);
					query.FetchOnlyFromLocalCache = isParentInDatabase.HasValue && !isParentInDatabase.Value;
					var existingVehicles = helper.Factory.Load<CusVehicle>(query);
					existingVehicles.DeleteAll(true);

					foreach (var vehicle in vehicleCollection)
					{
						var row = new VehicleDataObjectReader(vehicle, logger, helper.Factory, parentPK, parentTableCode).ReadIntoDataRow();
						if (row != null)
						{
							newRows.Add(row);
						}
					}

					result = newRows.ToArray();
				}
			}

			return result;
		}

		readonly IXmlImportLogger logger;
		readonly UniversalDataObjectReaderHelper helper;
	}
}
