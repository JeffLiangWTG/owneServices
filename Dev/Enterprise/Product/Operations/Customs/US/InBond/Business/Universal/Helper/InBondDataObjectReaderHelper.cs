using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class InBondDataObjectReaderHelper : DataTransfer.Universal.InBondDataObjectReaderHelper
	{
		public InBondDataObjectReaderHelper(UniversalObjectFactory factory, string dataProviderForCodeMapping = null)
			: base(factory, dataProviderForCodeMapping)
		{
		}

		public void SetCusInBondCargoDescCustomLabelsProvider(CusInBondHeader header)
		{
			if (header != null)
			{
				cusInBondCargoDescCustomLabelsProvider = new CusInBondCargoDescCustomLabelsProvider(header);
			}
		}

		public ICustomLabelsProvider GetCusInBondCargoDescCustomLabelsProvider()
		{
			return cusInBondCargoDescCustomLabelsProvider;
		}

		ICustomLabelsProvider cusInBondCargoDescCustomLabelsProvider;
	}
}
