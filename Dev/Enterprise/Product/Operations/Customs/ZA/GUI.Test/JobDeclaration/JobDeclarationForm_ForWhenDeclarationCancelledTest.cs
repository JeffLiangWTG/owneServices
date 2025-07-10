using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.ZA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class JobDeclarationForm_ForWhenDeclarationCancelledTest : BaseCustomsDeclarationFormTest_ForWhenDeclarationCancelled<JobDeclaration>
	{
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

		public override ZString MessageTypeForFormBashing => ZAJobMessageTypeList.Codes.Import;

		protected override Dictionary<string, string> GetIgnoreControlForLock()
		{
			return JobDeclarationFormTestHelper.GetIgnoreControlForLock(base.GetIgnoreControlForLock());
		}
	}
}
