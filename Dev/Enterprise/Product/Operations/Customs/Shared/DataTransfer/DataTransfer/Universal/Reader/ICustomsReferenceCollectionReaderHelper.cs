using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public interface ICustomsReferenceCollectionReaderHelper
	{
		ZString[] GetSupportedCusCodeDataCY_TypesFor(ZString parentTableCode, string dataContext);

		ZString[] GetSupportedCusReferenceCFR_TypesFor(ZString parentTableCode, string dataContext);

		UniversalObjectFactory Factory { get; }
	}
}
