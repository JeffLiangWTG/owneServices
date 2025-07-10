using System.Windows.Forms;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(GroupBrokerageUserControl))]
	sealed class GroupBrokerageUserControlTest : BasherTest
	{
		public override Form GetFormToBash()
		{
			var group = Factory.New<GlbGroup>();
			var wrapper = new GlbGroupForPluginWrapper(group);
			var form = new ZForm(wrapper);
			form.CaptionRenderingEnabled = true;
			form.Controls.Add(new GroupBrokerageUserControl());
			return form;
		}
	}
}
