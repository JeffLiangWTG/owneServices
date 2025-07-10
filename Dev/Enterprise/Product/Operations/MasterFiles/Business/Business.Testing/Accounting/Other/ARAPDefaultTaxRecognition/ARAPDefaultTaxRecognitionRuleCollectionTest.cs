using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ARAPDefaultTaxRecognitionRuleCollection))]
	sealed class ARAPDefaultTaxRecognitionRuleCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ARAPDefaultTaxRecognitionRuleCollection>
	{
		public void TestAllowRemove()
		{
			Assert(!Collection.AllowRemove);
		}

		public void TestAllowNew()
		{
			Assert(!Collection.AllowNew);
		}

		#region Implementation

		protected override ARAPDefaultTaxRecognitionRuleCollection GetCollectionToTest()
		{
			return new ARAPDefaultTaxRecognitionRuleCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ARAPDefaultTaxRecognitionRule();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
