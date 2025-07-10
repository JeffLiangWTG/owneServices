using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(JobPhaseSettingCollection))]
	public class JobPhaseSettingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<JobPhaseSettingCollection>
	{
		#region Implementation

		protected override JobPhaseSettingCollection GetCollectionToTest()
		{
			return new JobPhaseSettingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new JobPhaseSetting();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new JobPhaseSettingCollection Collection
		{
			get { return base.Collection; }
		}

		#endregion
	}
}
