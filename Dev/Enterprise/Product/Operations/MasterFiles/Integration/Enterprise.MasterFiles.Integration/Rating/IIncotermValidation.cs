using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration
{
	public interface IIncotermValidation
	{
		void WarningIfExpired(ZPropertyInfo propertyInfo);
	}
}
