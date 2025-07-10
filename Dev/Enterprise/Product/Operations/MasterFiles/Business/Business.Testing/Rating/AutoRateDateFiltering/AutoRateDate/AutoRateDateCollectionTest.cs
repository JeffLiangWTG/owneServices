using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AutoRateDateCollection))]
	sealed class AutoRateDateCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<AutoRateDateCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override AutoRateDateCollection GetCollectionToTest()
		{
			return new AutoRateDateCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AutoRateDate();
		}

		#endregion
	}
}
