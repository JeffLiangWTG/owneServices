using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Agency.DataTransfer
{
	[Immutable]
	public class AgencyReleaseTypeXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		AgencyReleaseTypeXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Core.Constants.ShipmentReleaseTypes.ExpressBofL, nameof(Xsd.AgencyReleaseType.EBL));
			yield return new Mapping(Core.Constants.ShipmentReleaseTypes.OriginalReqSurrender, nameof(Xsd.AgencyReleaseType.OBO));
			yield return new Mapping(Core.Constants.ShipmentReleaseTypes.OriginalReq, nameof(Xsd.AgencyReleaseType.OBR));
			yield return new Mapping(Core.Constants.ShipmentReleaseTypes.SeaWaybill, nameof(Xsd.AgencyReleaseType.SWB));
		}

		public static readonly AgencyReleaseTypeXmlCodeMappings Instance = new AgencyReleaseTypeXmlCodeMappings();

		protected override string Name
		{
			get { return Res.GetString("bc4c2272-37ec-467e-9610-e116916c1581", "Agency Release Type"); }
		}

		public new Xsd.AgencyReleaseType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.AgencyReleaseType.OBR, errorContext, notify);
		}
	}
}
