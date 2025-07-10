using System.Diagnostics;

using CargoWise.Application;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.Business.Accounting.CriticalValidation
{
	public static class ConstructorStackTraceExtensions
	{
		public static void SetConstructorStackTrace(this IHaveConstructorStackTrace parent)
		{
			parent.ConstructorStackTrace = ObjectFactory.Get<IAccounting>().CollectConstructorCallStackDetails ? new StackTrace(1) : null;
		}
	}
}
