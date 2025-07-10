using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public class BorderLineReleaseMessageCollection : NonDependentEDIMessageCollection
	{
		public BorderLineReleaseMessageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new LineReleaseMQEDIMessage AddNew() => (LineReleaseMQEDIMessage)base.AddNew();

		public new LineReleaseMQEDIMessage this[int index] => (LineReleaseMQEDIMessage)base[index];

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.USCustomsImport);
			result.AddToFilter(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.LineRelease);
			result.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessageStatusList.Codes.Received);
			return result;
		}
	}
}
