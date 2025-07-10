using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(EntryProcessingPortsMappingCollection))]
	sealed class EntryProcessingPortsMappingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<EntryProcessingPortsMappingCollection>
	{
		public void TestGetMappedProcessingPort()
		{
			EntryProcessingPortsMappingCollection coll = new EntryProcessingPortsMappingCollection();
			EntryProcessingPortsMapping mapping = coll.AddNew();
			mapping.EntryPort = "3901";
			mapping.ProcessingPort = "3902";
			mapping = coll.AddNew();
			mapping.EntryPort = "0104";
			mapping.ProcessingPort = "0105";
			AssertEquals("Mapped Processing Port", "3902", coll.GetMappedProcessingPort("3901"));
			AssertEquals("Mapped Processing Port", "", coll.GetMappedProcessingPort("0105"));
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override EntryProcessingPortsMappingCollection GetCollectionToTest() => new EntryProcessingPortsMappingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new EntryProcessingPortsMapping();
	}
}
