using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.ISF.DataTransfer
{
	[Immutable]
	class MessageTypeCodeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		MessageTypeCodeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(ISFMessageStatus.Codes.Rejected, nameof(Xsd.ISFStatusCustomsResponseMessageTypeCode.Item01));
			yield return new Mapping(ISFMessageStatus.Codes.Accepted, nameof(Xsd.ISFStatusCustomsResponseMessageTypeCode.Item02));
			yield return new Mapping(ISFMessageStatus.Codes.AcceptedWithWarning, nameof(Xsd.ISFStatusCustomsResponseMessageTypeCode.Item03));
			yield return new Mapping(ISFMessageStatus.Codes.RecordRejected, nameof(Xsd.ISFStatusCustomsResponseMessageTypeCode.Item11));
			yield return new Mapping(ISFMessageStatus.Codes.RecordAcceptedWithWarning, nameof(Xsd.ISFStatusCustomsResponseMessageTypeCode.Item13));
		}

		public static readonly MessageTypeCodeToXmlCodeMappings Instance = new MessageTypeCodeToXmlCodeMappings();

		protected override string Name
		{
			get { return "ISF Message Type Code"; }
		}

		public new Xsd.ISFStatusCustomsResponseMessageTypeCode GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ISFStatusCustomsResponseMessageTypeCode.Item01, errorContext, notify);
		}
	}
}
