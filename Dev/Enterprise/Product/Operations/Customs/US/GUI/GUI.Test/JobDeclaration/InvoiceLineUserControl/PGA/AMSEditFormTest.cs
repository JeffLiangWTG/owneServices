using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(AMSEditForm))]
	sealed class AMSEditFormTest : ZFormBasherTest
	{
		public void TestProgramDropEditReadOnly()
		{
			var ams = Factory.New<AMS>();
			using (var form = new AMSEditForm(ams))
			{
				form.Show();
				AssertEquals(true, form.US_ProgramDropEdit.ReadOnly);
			}
		}

		protected override System.Collections.Generic.IEnumerable<Form> FormsToBash
		{
			get
			{
				yield return GetFormToBashCore();
				var header = Factory.New<AMS>();
				header.Data.HasChanges = false;
				yield return new AMSEditForm(header);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<AMS>();
			header.Data.HasChanges = false;
			return new AMSEditForm(header);
		}
	}
}
