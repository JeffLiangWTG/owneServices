using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	public class FDAOwnerFirmTypeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		FDAOwnerFirmTypeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(OwnerFirmTypeList.Codes.Carrier, nameof(Xsd.USFDAOwnerFirmType.C));
			yield return new Mapping(OwnerFirmTypeList.Codes.I, nameof(Xsd.USFDAOwnerFirmType.I));
			yield return new Mapping(OwnerFirmTypeList.Codes.M, nameof(Xsd.USFDAOwnerFirmType.M));
			yield return new Mapping(OwnerFirmTypeList.Codes.U, nameof(Xsd.USFDAOwnerFirmType.U));
		}

		public static readonly FDAOwnerFirmTypeToXmlCodeMappings Instance = new FDAOwnerFirmTypeToXmlCodeMappings();

		protected override string Name
		{
			get { return "FDA Owner Firm Type"; } 
		}

		public new Xsd.USFDAOwnerFirmType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USFDAOwnerFirmType.I, errorContext, notify);
		}
	}
}
