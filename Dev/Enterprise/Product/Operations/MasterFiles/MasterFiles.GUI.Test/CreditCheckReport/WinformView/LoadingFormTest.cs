using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(LoadingForm))]
	public class LoadingFormTest : ZFormBasherTest
	{
		protected override bool AllowFormSizeFixed => true;
		protected override Form GetFormToBashCore()
		{
			var model = new LoadingFormModel<object>(ResourceStringHelper.CompanyLookup, null);
			var form = new LoadingForm(model);
			model.RunWorkerCompleted -= form.ViewModel_RunWorkerCompleted;
			return form;
		}
	}
}
