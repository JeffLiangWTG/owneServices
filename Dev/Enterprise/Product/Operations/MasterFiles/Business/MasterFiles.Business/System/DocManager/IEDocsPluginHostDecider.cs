
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface IEDocsPluginHostDecider
	{
		IBusiness HostBusinessEntity { get; }
	}
}
