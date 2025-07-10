using System.Windows.Forms;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.GUI.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.GUI.Test.Forms
{
	[TestedType(typeof(TelPreDriveChecklistTemplateForm))]
	class TelPreDriveChecklistTemplateFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new TelPreDriveChecklistTemplateForm(Factory.New<TelPreDriveChecklistTemplateHeader>());
		}
	}
}
