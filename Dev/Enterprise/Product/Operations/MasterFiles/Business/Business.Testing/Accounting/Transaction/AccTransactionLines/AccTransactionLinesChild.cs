using System.Data;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestClass]
	sealed class AccTransactionLinesChild : AccTransactionLines
	{
		public AccTransactionLinesChild(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void OnRateChangedCore() => MethodCallCount++;

		public int MethodCallCount;
	}
}
