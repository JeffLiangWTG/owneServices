using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business
{
	public class AMSBrokerDownloadMQEDIMessage : MQEDIMessage
	{
		public new class Schema : MQEDIMessage.Schema
		{
			public const string IssuerAndBillNumber = "IssuerAndBillNumber";
		}

		public AMSBrokerDownloadMQEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString IssuerAndBillNumber
		{
			get
			{
				if (issuerAndBillNumber.IsEmpty)
				{
					IssuerAndBillNumber relatedIssuerAndBillNumber = new IssuerAndBillNumber.Loader(Factory).LoadTop1(this);
					issuerAndBillNumber = relatedIssuerAndBillNumber != null ? relatedIssuerAndBillNumber.CY_Data : ZString.Empty;
				}

				return issuerAndBillNumber;
			}
		}
		ZString issuerAndBillNumber;

		public ZPropertyInfo IssuerAndBillNumberInfo
		{
			get { return GetZPropertyInfo(Schema.IssuerAndBillNumber); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = ApplicationIdentifierCodeList.Codes.BrokerManifestDownload;
		}
	}
}
