using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(TallyService))]
	public class TallyServiceTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			TallyContainer container = factory.New<TallyContainer>();
			return container.Services.AddNew();
		}

		#region TestGetTypeFromPrefix

		public void TestGetTypeFromPrefix()
		{
			JobServiceWithExposedHashTable service = Factory.New<JobServiceWithExposedHashTable>();
			AssertEquals("Hashtable should return correct type", typeof(PackUnpackDocsAndCartage), service.GetTypeFromPrefix("JP"));
			AssertEquals("Hashtable should return correct type", typeof(TallyContainer), service.GetTypeFromPrefix("JC"));
		}

		class JobServiceWithExposedHashTable : TallyService
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
