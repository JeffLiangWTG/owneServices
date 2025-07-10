using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	[CodeProperty(USCForeignAndRegionPort.Schema.US_PortCode), DescriptionProperty(USCForeignAndRegionPort.Schema.US_PortName)]
	public class USCForeignAndRegionPort : AutoUSCForeignAndRegionPort
	{
		public USCForeignAndRegionPort(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool CanDelete
		{
			get { return false; }
		}

		public override bool IsSavedByFactory
		{
			get { return false; }
		}
	}
}
