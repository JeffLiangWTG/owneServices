using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(CertificationCodeMappingCollection))]
	sealed class CertificationCodeMappingCollectionTest : RegistryBusinessObjectCollectionTestCase<CertificationCodeMappingCollection>
	{
		public void TestAddPair()
		{
			var collection = new CertificationCodeMappingCollection();
			collection.AddPair("111", "222");

			var mapping = collection[0];
			AssertNotNull(mapping);

			AssertEquals("111", mapping.MainCode);
			AssertEquals("222", mapping.SpecialisationCode);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CertificationCodeMappingCollection GetCollectionToTest()
		{
			return new CertificationCodeMappingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CertificationCodeMapping();
		}

		#endregion
	}
}
