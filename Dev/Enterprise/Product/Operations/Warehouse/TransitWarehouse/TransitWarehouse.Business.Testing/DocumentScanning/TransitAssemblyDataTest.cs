using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public abstract class TransitAssemblyDataTest<T, U> : AssemblyDataTest
			where T : BusinessObject
			where U : AssemblyData, new()
	{
		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(T), TransitData.BusinessObjectType);
		}

		public void TestGetBusinessObjectCollection()
		{
			AssertEquals(ExpectedBusinessObjectCollectionType, TransitData.GetBusinessObjectCollection(Factory).GetType());
		}

		public void TestModuleID()
		{
			AssertEquals(ExpectedModuleID, TransitData.ModuleID);
		}

		public void TestReferenceType()
		{
			AssertEquals(Constants.ReferenceTypes.SupplyChainLogistics, TransitData.ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals(ExpectedHumanReadableName, TransitData.HumanReadableName.ToString());
		}

		public void TestIsAllowedForUnallocatedeDocs()
		{
			AssertEquals(true, TransitData.IsAllowedForUnallocatedeDocs);
		}

		U TransitData => transitData ?? (transitData = new U());
		U transitData;

		protected abstract string ExpectedHumanReadableName { get; }

		protected abstract Type ExpectedBusinessObjectCollectionType { get; }

		protected abstract ModuleIdentifier ExpectedModuleID { get; }
	}
}
