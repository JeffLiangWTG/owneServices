using System.Windows.Forms;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.GUI.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.GUI.Test.Forms
{
	[TestedType(typeof(TelPreDriveChecklistForm))]
	class TelPreDriveChecklistFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new TelPreDriveChecklistForm(Factory.New<TelPreDriveChecklistHeader>());
		}
	}
}
