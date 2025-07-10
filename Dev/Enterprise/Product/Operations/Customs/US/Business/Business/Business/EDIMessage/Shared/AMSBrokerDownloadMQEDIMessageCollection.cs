
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class AMSBrokerDownloadMQEDIMessageCollection : NonDependentEDIMessageCollection
	{
		public AMSBrokerDownloadMQEDIMessageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new AMSBrokerDownloadMQEDIMessage AddNew()
		{
			return (AMSBrokerDownloadMQEDIMessage)base.AddNew();
		}

		public new AMSBrokerDownloadMQEDIMessage this[int index]
		{
			get { return (AMSBrokerDownloadMQEDIMessage)base[index]; }
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = base.CreateAdditionalFilter();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.USCustomsImport);
			result.AddToFilter(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.BrokerManifestDownload);
			return result;
		}
	}
}
