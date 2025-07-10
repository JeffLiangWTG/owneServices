using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.ISF.DataTransfer
{
	[Immutable]
	class SendContainerDetailsToCustomsXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		SendContainerDetailsToCustomsXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(YesNoDefaultList.Codes.Default, nameof(Xsd.ISFActionMessagingSendContainerDetailsToCustoms.Default));
			yield return new Mapping(YesNoDefaultList.Codes.Yes, nameof(Xsd.ISFActionMessagingSendContainerDetailsToCustoms.Yes));
			yield return new Mapping(YesNoDefaultList.Codes.No, nameof(Xsd.ISFActionMessagingSendContainerDetailsToCustoms.No));
		}

		public static readonly SendContainerDetailsToCustomsXmlCodeMappings Instance = new SendContainerDetailsToCustomsXmlCodeMappings();

		protected override string Name
		{
			get { return "ISF No. Of HTS Digits To Send Type"; }
		}

		public new Xsd.ISFActionMessagingSendContainerDetailsToCustoms GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ISFActionMessagingSendContainerDetailsToCustoms.Default, errorContext, notify);
		}
	}
}
