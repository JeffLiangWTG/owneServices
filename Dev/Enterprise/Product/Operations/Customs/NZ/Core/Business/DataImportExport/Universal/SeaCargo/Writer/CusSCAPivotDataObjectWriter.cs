using CargoWise.Integration;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class CusSCAPivotDataObjectWriter : CusSCAPivotDataObjectWriter<CusSCAPackingLine>
	{
		public CusSCAPivotDataObjectWriter(IDataWritingManager manager) : base(manager)
		{
		}

		protected override ICodeDescriptionPairList GetPackageTypes(CusSCAPackingLine bizObj)
		{
			return bizObj.Lookups.PackageTypeList;
		}

		protected override ICodeDescriptionPairList GetUnitsOfWeight(CusSCAPackingLine bizObj)
		{
			return bizObj.Lookups.WeightUQList;
		}

		protected override void WriteUNDGCollection(CusSCAPackingLine bizObj, PackingLine data, bool keepExistingData)
		{
			data.SetUNDGCollection(() =>
			{
				if (!keepExistingData && bizObj.UNDGs.Count > 0)
				{
					return ProcessCollection(bizObj.UNDGs, new UNDGDataObjectWriter(writeManager));
				}
				return data.UNDGCollection;
			});
		}
	}
}
