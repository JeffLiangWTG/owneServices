using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.Common.US;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	public class ZoneStatusToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		ZoneStatusToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(ZoneStatusList.Codes.Domestic, nameof(Xsd.USProductClassificationZoneStatus.D));
			yield return new Mapping(ZoneStatusList.Codes.NonPrivilegedForeign, nameof(Xsd.USProductClassificationZoneStatus.N));
			yield return new Mapping(ZoneStatusList.Codes.PrivilegedForeign, nameof(Xsd.USProductClassificationZoneStatus.P));
			yield return new Mapping(ZoneStatusList.Codes.ZoneRestricted, nameof(Xsd.USProductClassificationZoneStatus.Z));
		}

		public static readonly ZoneStatusToXmlCodeMappings Instance = new ZoneStatusToXmlCodeMappings();

		protected override string Name
		{
			get { return "Zone Status"; }
		}

		public new Xsd.USProductClassificationZoneStatus GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USProductClassificationZoneStatus.D, errorContext, notify);
		}
	}
}
