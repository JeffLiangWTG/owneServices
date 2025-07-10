using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V4.GUI
{
	public class GlbGroupForPluginWrapperProvider : MasterFiles.GUI.GlbGroupForPluginWrapperProvider
	{
		protected override IBusiness CreateNewGlbGroupWrapper(GlbGroup group)
		{
			return new GlbGroupForPluginWrapper(group);
		}

		public override ZUserControl GetNewUserControl()
		{
			return new GroupBrokerageUserControl();
		}
	}
}
