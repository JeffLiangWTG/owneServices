using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class GroupCredentialsPlugInForTesting : GroupCredentialsPlugIn
	{
		public GroupCredentialsPlugInForTesting(GlbGroup group)
			: base(group, new GlbGroupForPluginWrapperProviderForTesting())
		{
		}
	}
}
