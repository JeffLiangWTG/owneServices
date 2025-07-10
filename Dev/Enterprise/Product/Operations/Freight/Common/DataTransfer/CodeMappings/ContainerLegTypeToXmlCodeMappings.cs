using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Common.DataTransfer
{
	[Immutable]
	public class ContainerLegTypeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		ContainerLegTypeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Core.Constants.PickupDeliveryConfirmTypes.OriginPickup, nameof(Xsd.ContainerLegType.PCU));
			yield return new Mapping(Core.Constants.PickupDeliveryConfirmTypes.DestinationDelivery, nameof(Xsd.ContainerLegType.DLV));
			yield return new Mapping(Core.Constants.PickupDeliveryConfirmTypes.OriginCFSArrival, nameof(Xsd.ContainerLegType.CPA));
			yield return new Mapping(Core.Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture, nameof(Xsd.ContainerLegType.CPD));
			yield return new Mapping(Core.Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival, nameof(Xsd.ContainerLegType.CDA));
			yield return new Mapping(Core.Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture, nameof(Xsd.ContainerLegType.CDD));
			yield return new Mapping(Core.Constants.CartageLegType.AirExport, nameof(Xsd.ContainerLegType.AEX));
			yield return new Mapping(Core.Constants.CartageLegType.AirImport, nameof(Xsd.ContainerLegType.AIM));
			yield return new Mapping(Core.Constants.CartageLegType.CTOToImporterToYard, nameof(Xsd.ContainerLegType.FIW));
			yield return new Mapping(Core.Constants.CartageLegType.DomesticContainerizedDelivery, nameof(Xsd.ContainerLegType.DCD));
			yield return new Mapping(Core.Constants.CartageLegType.DomesticContainerizedPickup, nameof(Xsd.ContainerLegType.DCP));
			yield return new Mapping(Core.Constants.CartageLegType.DomesticLooseDelivery, nameof(Xsd.ContainerLegType.DDL));
			yield return new Mapping(Core.Constants.CartageLegType.DomesticLoosePickup, nameof(Xsd.ContainerLegType.DPU));
			yield return new Mapping(Core.Constants.CartageLegType.EmptyCFSToYard, nameof(Xsd.ContainerLegType.FDY));
			yield return new Mapping(Core.Constants.CartageLegType.EmptyImporterToYard, nameof(Xsd.ContainerLegType.FIY));
			yield return new Mapping(Core.Constants.CartageLegType.EmptyYardToCFS, nameof(Xsd.ContainerLegType.FYD));
			yield return new Mapping(Core.Constants.CartageLegType.EmptyYardToExporter, nameof(Xsd.ContainerLegType.FYE));
			yield return new Mapping(Core.Constants.CartageLegType.FCE, nameof(Xsd.ContainerLegType.FCE));
			yield return new Mapping(Core.Constants.CartageLegType.FCL, nameof(Xsd.ContainerLegType.FCL));
			yield return new Mapping(Core.Constants.CartageLegType.FCLExport, nameof(Xsd.ContainerLegType.FPE));
			yield return new Mapping(Core.Constants.CartageLegType.FCLImport, nameof(Xsd.ContainerLegType.FUI));
			yield return new Mapping(Core.Constants.CartageLegType.FCLPack, nameof(Xsd.ContainerLegType.FPD));
			yield return new Mapping(Core.Constants.CartageLegType.FCLUnpack, nameof(Xsd.ContainerLegType.FUD));
			yield return new Mapping(Core.Constants.CartageLegType.FTL, nameof(Xsd.ContainerLegType.FTL));
			yield return new Mapping(Core.Constants.CartageLegType.FullCFSToCTO, nameof(Xsd.ContainerLegType.FDC));
			yield return new Mapping(Core.Constants.CartageLegType.FullCTOToCFS, nameof(Xsd.ContainerLegType.FCD));
			yield return new Mapping(Core.Constants.CartageLegType.FullCTOToImporter, nameof(Xsd.ContainerLegType.FCI));
			yield return new Mapping(Core.Constants.CartageLegType.FullExporterToCTO, nameof(Xsd.ContainerLegType.FEC));
			yield return new Mapping(Core.Constants.CartageLegType.LCLExport, nameof(Xsd.ContainerLegType.LED));
			yield return new Mapping(Core.Constants.CartageLegType.LCLImport, nameof(Xsd.ContainerLegType.LDI));
			yield return new Mapping(Core.Constants.CartageLegType.LTE, nameof(Xsd.ContainerLegType.LTE));
			yield return new Mapping(Core.Constants.CartageLegType.LTL, nameof(Xsd.ContainerLegType.LTL));
			yield return new Mapping(Core.Constants.CartageLegType.ReturnToCNR, nameof(Xsd.ContainerLegType.RTC));
			yield return new Mapping(Core.Constants.CartageLegType.YardToExporterToCTO, nameof(Xsd.ContainerLegType.FEW));
		}

		public static readonly ContainerLegTypeToXmlCodeMappings Instance = new ContainerLegTypeToXmlCodeMappings();

		protected override string Name
		{
			get { return Res.GetString("bf4400d6-5039-40ba-8a3d-92982f8618eb", "Confirmation Type"); }
		}

		public new Xsd.ContainerLegType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ContainerLegType.DLV, errorContext, notify);
		}
	}
}
