using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("Incremental check-in - will be used for PortMappings service.")]
	public class RefUNLOCORelatedPort : AutoRefUNLOCORelatedPort
	{
		public RefUNLOCORelatedPort(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
