using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(DefaultRoundingsCollection))]
	public class DefaultRoundingsCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DefaultRoundingsCollection>
	{
		public void TestGetDefault()
		{
			DefaultRoundingsCollection @default = DefaultRoundingsCollection.GetDefault();

			AssertEquals(2, @default.Count);
			int i = 0;
			AssertDefaultRounding(@default[i++], "ALL", "NOR");
			AssertDefaultRounding(@default[i++], "WHS", "UP1");
		}

		void AssertDefaultRounding(DefaultRoundings defaultRounding, string code, string roundingType)
		{
			AssertEquals("Code", code, defaultRounding.Code);
			AssertEquals("RoundingType", roundingType, defaultRounding.RoundingType);
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

		protected override DefaultRoundingsCollection GetCollectionToTest()
		{
			return new DefaultRoundingsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DefaultRoundings();
		}

		#endregion
	}
}
