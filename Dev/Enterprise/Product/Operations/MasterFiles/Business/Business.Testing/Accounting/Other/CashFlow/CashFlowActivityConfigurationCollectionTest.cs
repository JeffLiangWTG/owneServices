using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CashFlowActivityConfigurationCollection))]
	sealed class CashFlowActivityConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CashFlowActivityConfigurationCollection>
	{
		#region Implementation

		protected override CashFlowActivityConfigurationCollection GetCollectionToTest()
		{
			return new CashFlowActivityConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CashFlowActivityConfiguration();
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
