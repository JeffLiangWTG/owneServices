using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class CusInBondContainerDataObjectReader : DataTransfer.Universal.CusInBondContainerDataObjectReader<CusInBondContainer, CusInBondCargoDesc>
	{
		public CusInBondContainerDataObjectReader(Container containerDataObject, IXmlImportLogger logger, InBondDataObjectReaderHelper helper, ZGuid moveDetailPK)
			: base(containerDataObject, logger, helper, moveDetailPK)
		{
		}

		protected new InBondDataObjectReaderHelper Helper
		{
			get { return (InBondDataObjectReaderHelper)base.Helper; }
		}

		protected override DataTransfer.Universal.CusInBondCargoDescDataObjectReader<CusInBondCargoDesc> InBondCargoDescDataObjectReader(PackingLine packingLineData, IXmlImportLogger logger, DataTransfer.Universal.InBondDataObjectReaderHelper helper, ZGuid containerPK)
		{
			return new CusInBondCargoDescDataObjectReader(packingLineData, logger, Helper, containerPK);
		}
	}
}
