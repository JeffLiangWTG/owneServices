using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CALINF.Testing
{
	sealed class CALINFMessageDataForTest : ICALINFMessageDataProvider
	{
		public ICALINFTransportInformation Transport { get; set; }

		public ZString CALINFMessageType { get; set; }

		public ZDateTime DocumentIssueDateTime { get; set; }

		public ZString DocumentToBeAmended { get; set; }

		public ZString MessageSender { get; set; }

		public Messaging.Business.EDIMessageCollection Messages { get; set; }

		public BusinessObjectFactory Factory { get; set; }

		public void AddMessage(EDIMessage message)
		{
		}

		public ZString MessageStatus { get; set; } = ZString.Empty;
		public ZString JobStatus { get; set; } = ZString.Empty;
		public bool HasChanges { get; set; }
		public ZString JobIdentification { get; set; } = ZString.Empty;
		public BusinessObject TopLevelBusinessObject { get; set; }
		public bool RefreshValidationBeforeSendMessage => true;
	}

	sealed class CALINFCallDataForTest : ICALINFCallInformation
	{
		public ZString CallLocation { get; set; }

		public ZString LocationCodeListIdentificationCode { get; set; }

		public CodeListResponsibleAgencyCodeList LocationCodeListResponsibleAgencyCode { get; set; }

		public ZDateTime CallDateTime { get; set; }
	}

	sealed class CALINFTransportDataForTest : ICALINFTransportInformation
	{
		public List<ICALINFCallInformation> DepartureDetails { get; set; }

		public List<ICALINFCallInformation> DischargeDetails { get; set; }

		public List<ICALINFCallInformation> CallDetails { get; set; }

		public ZString ConveyanceNumber { get; set; }

		public ZString TransportMode { get; set; }

		public ZString CarrierCode { get; set; }

		public ZString CarrierName { get; set; }

		public CodeListResponsibleAgencyCodeList CarrierCodeListResponsibleAgencyCode { get; set; }

		public ZString MeansOfTransportId { get; set; }

		public ZString MeansOfTransportName { get; set; }

		public ZString MeansOfTransportNationality { get; set; }

		public ZString MeansOfTransportCodeListIdentificationCode { get; set; }

		public ZString PrincipalCarrierConveyanceNumber { get; set; }
	}
}
