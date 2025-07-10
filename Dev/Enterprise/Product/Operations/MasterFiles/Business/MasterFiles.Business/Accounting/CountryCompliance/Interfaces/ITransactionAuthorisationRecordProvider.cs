using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Accounting.CountryCompliance
{
	public interface ITransactionAuthorisationRecordProvider
	{
		ZString GetDecodedAuthorationData(ZBlob authorisationData);
	}
}
