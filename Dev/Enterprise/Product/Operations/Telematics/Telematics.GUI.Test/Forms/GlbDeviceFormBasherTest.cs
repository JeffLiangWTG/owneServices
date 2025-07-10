using System.Windows.Forms;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.GUI.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.GUI.Test
{
	[TestedType(typeof(GlbDeviceForm))]
	public class GlbDeviceFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new GlbDeviceForm(Factory.New<GlbDevice>());
		}
	}
}
