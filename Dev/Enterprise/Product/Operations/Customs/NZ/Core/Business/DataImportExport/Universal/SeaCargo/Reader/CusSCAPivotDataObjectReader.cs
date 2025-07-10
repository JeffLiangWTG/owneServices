using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class CusSCAPivotDataObjectReader : CusSCAPivotDataObjectReader<CusSCAPackingLine>
	{
		public CusSCAPivotDataObjectReader(ZInt lineNo, IColumnIndexer houseBill, HVLVShipmentDataObjectWrapper hvlvConsolidatorShipmentWrapper, PackingLine data, IXmlImportLogger logger, UniversalObjectFactory factory) : base(lineNo, houseBill, hvlvConsolidatorShipmentWrapper, data, logger, factory)
		{
		}

		protected override ZString ConvertCustomsPackageType(ZString packType)
		{
			return PackageTypeConverter.GetCustomsPackageType(packType);
		}

		protected override void ReadUNDGCollection(CusSCAPackingLine targetBO)
		{
			if (dataObject.UNDGCollection != null)
			{
				foreach (var undg in dataObject.UNDGCollection)
				{
					var reader = new UNDGDataObjectReader(undg, logger, factory);
					var undgDataItem = reader.ReadIntoBusinessObject();
					if (undgDataItem != null)
					{
						targetBO.UNDGs.Add(undgDataItem);
					}
				}
			}
		}
	}
}
