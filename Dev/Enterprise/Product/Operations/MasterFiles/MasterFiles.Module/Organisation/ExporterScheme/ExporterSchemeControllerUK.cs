using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.MasterFiles.Module
{
	public class ExporterSchemeControllerUK : ExporterSchemeController
	{
		public ExporterSchemeControllerUK()
		{
		}

		protected override ZPlugIn GetPlugInCore(IBusiness businessEntity)
		{
			return new ExporterSchemePluginUK((OrgHeader)businessEntity);
		}
	}
}
