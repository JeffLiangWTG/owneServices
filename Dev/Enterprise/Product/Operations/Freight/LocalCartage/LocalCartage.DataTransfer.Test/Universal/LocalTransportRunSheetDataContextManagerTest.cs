using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Testing
{
	[TestedType(typeof(LocalTransportRunSheetDataContextManager))]
	public class LocalTransportRunSheetDataContextManagerTest : DataContextManagerTestCase<LocalTransportRunSheetDataContextManager, CommonWorkSheet>
	{
		public void TestDataContextType()
		{
			var dataContext = new LocalTransportRunSheetDataContextManager();
			AssertEquals(DataContextType.LocalTransportRunSheet, dataContext.DataContextType);
		}

		public void TestDataContextKey()
		{
			var dataContext = new LocalTransportRunSheetDataContextManager();
			AssertEquals(ZString.Empty, dataContext.DataContextKey);
		}

		public void TestDefaultOutputDirectory()
		{
			var dataContext = new LocalTransportRunSheetDataContextManager();
			AssertEquals(null, dataContext.DefaultOutputDirectory);
		}
	}
}
