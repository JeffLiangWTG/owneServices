using System.Collections.Generic;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.Packing.Business
{
	public interface IPackingParentDocumentSupporter
	{
		IEnumerable<DataContext> GetModuleSpecificPackageSupportedDataContexts();

		ZString GetModuleSpecificNotFoundMessage(DataContext dataContext);

		bool IsPrintablePackageForSpecificModuleDataContext(DataContext dataContext, PkgPackage package);

		bool IsAllPackLevelsEnabled(DataContext dataContext);
	}
}
