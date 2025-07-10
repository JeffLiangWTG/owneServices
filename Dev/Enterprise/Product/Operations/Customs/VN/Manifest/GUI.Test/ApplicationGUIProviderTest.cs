using System;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.VN.Manifest.GUI.Test
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<
		ApplicationGUIProvider,
		AsycudaManifestHeader>
	{
		public void TestGetManifestLayout()
		{
			var provider = new ApplicationGUIProvider();
			AssertType<VNManifestLayout>(provider.GetManifestLayout());
		}

		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type[] ExpectedBillAdditionalTabPageUserControls =>
			new[] { typeof(AsycudaPackUserControl) };
	}
}
