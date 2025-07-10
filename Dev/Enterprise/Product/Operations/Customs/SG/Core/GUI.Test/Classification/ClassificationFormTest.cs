using System.Windows.Forms;
using Enterprise.Customs.SG.V4.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(ClassificationForm))]
	sealed class ClassificationFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (var form = (ClassificationForm)GetFormToBash())
			{
				AssertEquals("FormCaption", "Classification Lookup", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var classification = Factory.New<Classification>();
			return new ClassificationForm(classification);
		}
	}
}
