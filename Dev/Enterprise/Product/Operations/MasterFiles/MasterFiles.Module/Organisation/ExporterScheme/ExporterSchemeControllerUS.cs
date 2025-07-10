using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.MasterFiles.Module
{
	public class ExporterSchemeControllerUS : ExporterSchemeController
	{
		public ExporterSchemeControllerUS()
		{
		}

		protected override ZPlugIn GetPlugInCore(IBusiness businessEntity)
		{
			return new ExporterSchemePluginUS((OrgHeader)businessEntity);
		}
	}
}
