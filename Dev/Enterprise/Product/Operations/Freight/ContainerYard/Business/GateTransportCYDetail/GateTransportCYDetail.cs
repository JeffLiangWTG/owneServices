using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.ContainerYard.Business
{
	[CodeProperty(Schema.GTC_JobNumber), DescriptionProperty(Schema.GTC_ContainerNumber)]
	public class GateTransportCYDetail : AutoGateTransportCYDetail, IEDocsProvider
	{
		public GateTransportCYDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Business Objects

		[RelatedBusinessObject("GateTransport")]
		public override ZGuid GTC_GTT
		{
			get { return base.GTC_GTT; }
			set { base.GTC_GTT = value; }
		}

		public GateTransport GateTransport
		{
			get { return Factory.Load<GateTransport>(GTC_GTT); }
		}

		#endregion

		#region IEDocsProvider

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.GateTransportCYDetail));
			}
		}
		DocManagerInfo docManagerInfo;

		public EDocsProviderSupporter GetEDocsProviderSupporter()
		{
			return eDocsProviderSupporter ?? (eDocsProviderSupporter = new EDocsProviderSupporter(this));
		}
		EDocsProviderSupporter eDocsProviderSupporter;

		public DocumentSupporter DocumentSupporter => new GateTransportCYDetailDocumentSupporter(this);

		#endregion

		public override bool IsSavedByFactory => false;
	}
}
