using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class RunSheetSecurityProvider : IRunSheetSecurityQueryProvider
	{
		public static IRunSheetSecurityQueryProvider GetProvider(BusinessObjectFactory factory)
		{
			return factory.GetValue<IRunSheetSecurityQueryProvider>() ?? new RunSheetSecurityProvider();
		}

		RunSheetSecurityProvider() { }

		void IRunSheetSecurityQueryProvider.TryAuthorise(CommonCartageLeg leg)
		{
			if (leg != null)
			{
				leg.AuthoriseRunSheet("");
			}
		}
	}
}
