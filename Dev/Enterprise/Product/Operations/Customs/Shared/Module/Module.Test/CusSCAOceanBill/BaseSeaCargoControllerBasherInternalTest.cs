using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(BaseSeaCargoController))]
	sealed class BaseSeaCargoControllerBasherInternalTest : ZControllerBasherTest
	{
		public override void TestViewForm()
		{
			Assert(true);
		}

		public override void TestEditForm()
		{
			Assert(true);
		}

		public override void TestDeleteForm()
		{
			Assert(true);
		}

		public override void TestNewForm()
		{
			Assert(true);
		}

		public override void TestSaveFormWithCustomsPlugIns()
		{
			Assert(true);
		}

		public override void TestTemplateCopyForm()
		{
			Assert(true);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.BaseSeaCargo;

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;
	}
}
