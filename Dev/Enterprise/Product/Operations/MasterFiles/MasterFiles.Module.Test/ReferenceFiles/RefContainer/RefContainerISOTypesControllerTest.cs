using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefContainerISOTypesController))]
	sealed class RefContainerISOTypesControllerTest : ZSingletonControllerBasherTest
	{
		public override void TestNewForm()
		{
			Assert(true);
		}

		public override void TestViewForm()
		{
			Assert(true);
		}

		public override void TestEditForm()
		{
			Assert(true);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefContainerISOTypes;
		}
	}
}
