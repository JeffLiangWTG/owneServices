using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(OnlineApplicationDocTypeCollection))]
	sealed class OnlineApplicationDocTypeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<OnlineApplicationDocTypeCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override OnlineApplicationDocTypeCollection GetCollectionToTest()
		{
			return new OnlineApplicationDocTypeCollection(NewFallback(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OnlineApplicationDocType(NewFallback(), Factory);
		}

		FallbackLevel NewFallback()
		{
			return new FallbackLevel(Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment);
		}

		#endregion
	}
}
