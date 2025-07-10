using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class GlbGroupForPluginWrapperProviderForTesting : GlbGroupForPluginWrapperProvider
	{
		protected override IBusiness CreateNewGlbGroupWrapper(GlbGroup group)
		{
			return new GlbGroupForPluginWrapperForTesting(group);
		}

		public override ZUserControl GetNewUserControl()
		{
			return new ZUserControl();
		}
	}
}
