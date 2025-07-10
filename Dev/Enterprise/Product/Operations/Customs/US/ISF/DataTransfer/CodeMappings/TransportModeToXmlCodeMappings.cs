using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.ISF.DataTransfer
{
	[Immutable]
	class TransportModeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		TransportModeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(TransportModeCodes.Codes.OceanVesselNonContainerized, nameof(Xsd.ISFTransportMode.Item10));
			yield return new Mapping(TransportModeCodes.Codes.OceanVesselContainerized, nameof(Xsd.ISFTransportMode.Item11));
		}

		public static readonly TransportModeToXmlCodeMappings Instance = new TransportModeToXmlCodeMappings();

		protected override string Name
		{
			get { return "ISF Transport Mode"; }
		}

		public new Xsd.ISFTransportMode GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ISFTransportMode.Item10, errorContext, notify);
		}
	}
}
