using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This abstract class will be used in other WI.")]
	public abstract class UEMInboundMessage : UEMEDIMessage
	{
		protected UEMInboundMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
