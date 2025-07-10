using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefNMFCForm))]
	public class RefNMFCFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new RefNMFCForm(fRefNMFC);
		}

		[ExpectNoExceptions()]
		[RequiresSTA]
		public void TestFormRefNMFC()
		{
			using (TestRefNMFCForm testForm = new TestRefNMFCForm(fRefNMFC))
			{
				testForm.Show();
				AssertEquals("NMFC", testForm.FormCaption);
			}
		}

		#region Implementation

		RefNMFC fRefNMFC;
		protected override void SetUp()
		{
			base.SetUp();
			fRefNMFC = Factory.New<RefNMFC>();
		}

		protected class TestRefNMFCForm : RefNMFCForm
		{
			public TestRefNMFCForm(RefNMFC topBizObj)
				: base(topBizObj)
			{
			}
		}

		#endregion
	}
}
