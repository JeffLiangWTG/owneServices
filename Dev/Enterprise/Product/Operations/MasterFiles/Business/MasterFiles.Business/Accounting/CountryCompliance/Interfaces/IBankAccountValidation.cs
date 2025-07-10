using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IBankAccountValidation
	{
		void ValidateFullAccountNumber(ZPropertyInfo propertyInfo);
	}
}
