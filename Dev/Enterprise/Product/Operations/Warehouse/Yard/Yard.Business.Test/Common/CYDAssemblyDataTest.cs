using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	public abstract class CYDAssemblyDataTest<T, U> : AssemblyDataTest
		where T : BusinessObject
		where U : AssemblyData, new()
	{
		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(T), new U().BusinessObjectType);
		}

		public void TestGetBusinessObjectCollection()
		{
			AssertNull(new U().GetBusinessObjectCollection(Factory));
		}

		public void TestReferenceType()
		{
			AssertEquals(Constants.ReferenceTypes.SupplyChainLogistics, new U().ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals(ExpectedHumanReadableName, new U().HumanReadableName.ToString());
		}

		protected abstract string ExpectedHumanReadableName { get; }
	}
}
