using CargoWise.Integration;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class CusSCAContainerDataObjectWriter : CusSCAContainerDataObjectWriter<CusSCAContainer>
	{
		public CusSCAContainerDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override ICodeDescriptionPairList GetContainerModes(CusSCAContainer bizObj)
		{
			return bizObj.Lookups.ContainerModeList;
		}
	}
}
