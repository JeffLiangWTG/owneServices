using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Module.Testing;

[TestedType(typeof(NLOrganisationConsigneePlugInController))]
class NLOrganisationConsigneePlugInControllerTest : ZControllerBasherTest
{
	public void TestPlugIn()
	{
		var controller = new NLOrganisationConsigneePlugInController();
		using (var tabControl = new ZTabControl())
		{
			var plugIns = new PlugIns(Factory.New<OrgHeader>(), tabControl);
			plugIns.Add(controller.ID);
			using (var plugIn = plugIns.GetPlugIn(controller.ID))
			{
				AssertType<GUI.OrganisationConsigneePlugIn>(plugIn);
			}
		}
	}

	protected override ControllerID GetControllerID() => ControllerIDs.Customs.NL.OrganisationConsigneePlugIn;
}
