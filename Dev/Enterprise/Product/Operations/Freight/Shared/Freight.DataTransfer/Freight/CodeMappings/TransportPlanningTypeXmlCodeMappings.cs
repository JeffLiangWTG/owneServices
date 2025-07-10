using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	[Immutable]
	public class TransportPlanningTypeXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		TransportPlanningTypeXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Core.Constants.TransportPlanningType.Flight1, nameof(Xsd.PlannedLegTransportType.Flight1));
			yield return new Mapping(Core.Constants.TransportPlanningType.Flight2, nameof(Xsd.PlannedLegTransportType.Flight2));
			yield return new Mapping(Core.Constants.TransportPlanningType.Flight3, nameof(Xsd.PlannedLegTransportType.Flight3));
			yield return new Mapping(Core.Constants.TransportPlanningType.MainVessel, nameof(Xsd.PlannedLegTransportType.MainVessel));
			yield return new Mapping(Core.Constants.TransportPlanningType.OnBoardCourier, nameof(Xsd.PlannedLegTransportType.OnBoardCourier));
			yield return new Mapping(Core.Constants.TransportPlanningType.OnForwarding, nameof(Xsd.PlannedLegTransportType.OnForwarding));
			yield return new Mapping(Core.Constants.TransportPlanningType.PreCarriage, nameof(Xsd.PlannedLegTransportType.PreCarriage));
			yield return new Mapping(Core.Constants.TransportPlanningType.Unaccompanied, nameof(Xsd.PlannedLegTransportType.Unaccompanied));
			yield return new Mapping(Core.Constants.TransportPlanningType.Other, nameof(Xsd.PlannedLegTransportType.Other));
		}

		public static readonly TransportPlanningTypeXmlCodeMappings Instance = new TransportPlanningTypeXmlCodeMappings();

		protected override string Name
		{
			get { return Res.GetString("b3c4e559-4b8f-4ee6-bc87-cc20443d4512", "Transport Planning Type"); }
		}

		public new Xsd.PlannedLegTransportType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.PlannedLegTransportType.Other, errorContext, notify);
		}
	}
}
