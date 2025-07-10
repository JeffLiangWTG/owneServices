using System.Collections.Generic;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	abstract class JobDeclarationFormAbstractTest : BaseJobDeclarationFormAbstractTest<JobDeclaration>
	{
		public void TestRoutingTabIndex()
		{
			using (var form = new JobDeclarationForm())
			{
				AssertEquals("RoutingTabIndex = 2", 2, form.RoutingTabIndex);
			}
		}

		public void TestHasDocumentVisualizer()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				AssertNotNull("DocumentVaisualizer", form.PlugIns.GetPlugIn(ControllerIDs.DocumentVisualizer));
			}
		}

		public override void TestMinimumSizeNotTooBig()
		{
			int minScreenWidthSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1380);
			int minScreenHeightSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(880);
			using (var form = GetFormToBashCore())
			{
				Assert("ZA Declaration Form min size too wide (" + form.MinimumSize.Width + ") for the screen. Should be less than or equal to " + minScreenWidthSupported, form.MinimumSize.Width <= minScreenWidthSupported);
				Assert("ZA Declaration Form min size too high (" + form.MinimumSize.Height + ") for the screen. Should be less than or equal to " + minScreenHeightSupported, form.MinimumSize.Height <= minScreenHeightSupported);
			}
		}

		protected override Dictionary<string, string> GetIgnoreControlForLock()
		{
			return JobDeclarationFormTestHelper.GetIgnoreControlForLock(base.GetIgnoreControlForLock());
		}
	}
}
