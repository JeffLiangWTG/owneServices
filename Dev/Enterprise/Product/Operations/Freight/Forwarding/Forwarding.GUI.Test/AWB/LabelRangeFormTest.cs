using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	[TestedType(typeof(LabelRangeForm))]
	public class LabelRangeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var actions = new ConsolAWBActions(consol, AWBActions.ActionsModeType.LabelsOnly);

			return new LabelRangeForm(actions);
		}

		protected override bool AllowFormSizeFixed => true;
	}
}
