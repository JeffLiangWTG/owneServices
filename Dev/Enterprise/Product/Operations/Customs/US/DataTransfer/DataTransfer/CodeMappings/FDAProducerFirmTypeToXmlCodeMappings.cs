using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	public class FDAProducerFirmTypeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		FDAProducerFirmTypeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(ProducerFirmTypeList.Codes.C, nameof(Xsd.USManufacturerProducerDataTypeProducerFirmType.C));
			yield return new Mapping(ProducerFirmTypeList.Codes.G, nameof(Xsd.USManufacturerProducerDataTypeProducerFirmType.G));
			yield return new Mapping(ProducerFirmTypeList.Codes.M, nameof(Xsd.USManufacturerProducerDataTypeProducerFirmType.M));
		}

		public static readonly FDAProducerFirmTypeToXmlCodeMappings Instance = new FDAProducerFirmTypeToXmlCodeMappings();

		protected override string Name
		{
			get { return "FDA Producer Firm Type"; } 
		}

		public new Xsd.USManufacturerProducerDataTypeProducerFirmType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USManufacturerProducerDataTypeProducerFirmType.C, errorContext, notify);
		}
	}
}
