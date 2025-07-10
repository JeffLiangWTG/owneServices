using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class ReleaseInstance : AutoReleaseInstance, IDocumentSupportable
	{
		public ReleaseInstance(ReleaseHeader header)
			: base(header.Factory)
		{
			this.header = header;
		}

		#region Related Business Objects

		public ReleaseHeader Header
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return header; }
		}

		public AgencyBooking Shipment
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return Header.Shipment; }
		}

		public OrgAddress ContainerYard
		{
			get { return Factory.Load<OrgAddress>(ContainerYardAddress); }
		}

		#endregion

		#region Strategy Objects

		public ReleaseInstanceLookups Lookups
		{
			get { return lookups ?? (lookups = new ReleaseInstanceLookups(this)); }
		}
		ReleaseInstanceLookups lookups;

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new ReleaseInstanceDocumentSupporter(this); }
		}

		#endregion

		readonly ReleaseHeader header;
	}
}



