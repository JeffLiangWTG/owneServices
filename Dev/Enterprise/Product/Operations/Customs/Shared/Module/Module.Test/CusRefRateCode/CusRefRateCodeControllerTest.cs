using System;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusRefRateCodeController))]
	sealed class CusRefRateCodeControllerTest : ZControllerBasherTest
	{
		public override void TestDeleteForm()
		{
			Assert(true);
		}

		public override void TestEditForm()
		{
			Assert(true);
		}

		public override void TestNewForm()
		{
			Assert(true);
		}

		public override void TestViewForm()
		{
			Assert(true);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CusRefRateCode;

		protected override Type GetBusinessObjectType() => typeof(CusRefRateCodeForTesting);

		public override Type ControllerToBashType => typeof(CusRefRateCodeController);
	}
}
