using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Testing
{
	sealed class CustomsAgentCodeFindBoxTest : TestCaseWithFactory
	{
		public void TestCanReferenceByDescription()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			using (var form = new ZForm(header))
			using (var control = new CustomsAgentCodeFindBox())
			{
				control.BindTo = "AMA_GS_NKCustomsAgent";
				form.Controls.Add(control);
				form.Show();
				AssertEquals("DescriptionBox should be readonly", true, control.DescriptionBox.ReadOnly);
			}
		}
	}
}
