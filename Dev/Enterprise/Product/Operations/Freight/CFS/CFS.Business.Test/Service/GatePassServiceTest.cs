using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(GatePassService))]
	public class GatePassServiceTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			GatePassContainer container = factory.New<GatePassContainer>();
			return container.Services.AddNew();
		}

		#region TestGetTypeFromPrefix

		public void TestGetTypeFromPrefix()
		{
			JobServiceWithExposedHashTable service = Factory.New<JobServiceWithExposedHashTable>();
			AssertEquals("Hashtable should return correct type", typeof(GatePassDocsAndCartage), service.GetTypeFromPrefix("JP"));
			AssertEquals("Hashtable should return correct type", typeof(GatePassContainer), service.GetTypeFromPrefix("JC"));
		}

		class JobServiceWithExposedHashTable : GatePassService
		{
			public JobServiceWithExposedHashTable(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new Type GetTypeFromPrefix(string prefix)
			{
				return base.GetTypeFromPrefix(prefix);
			}
		}

		#endregion
	}
}
