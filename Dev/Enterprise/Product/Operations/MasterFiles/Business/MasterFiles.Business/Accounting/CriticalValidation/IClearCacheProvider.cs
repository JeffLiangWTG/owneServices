using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Accounting.CriticalValidation
{
	public delegate void ClearCacheDelegate(BusinessObjectFactory factory);

	public interface IClearCacheProvider
	{
		ClearCacheDelegate GetClearCacheDelegate();
	}
}
