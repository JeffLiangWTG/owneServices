using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing
{
	[TestedType(typeof(MiscOptionsLayoutUserControl))]
	sealed class MiscOptionsLayoutUserControlTest : TestCaseWithFactory
	{
		public void TestRelatedDeclarationsControl()
		{
			using var control = new MiscOptionsLayoutUserControl();
			_ = control.AssertContainsControl<RelatedDeclarationsUserControl>(nameof(MiscOptionsLayoutUserControl.RelatedDeclarationsUserControl));
		}
	}
}
