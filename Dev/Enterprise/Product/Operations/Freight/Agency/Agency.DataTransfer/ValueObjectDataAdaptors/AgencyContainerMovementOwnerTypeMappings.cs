using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Agency.DataTransfer
{
	[Immutable]
	class AgencyContainerMovementOwnerTypeMappings : EnterpriseCodeExternalCodeMappings
	{
		AgencyContainerMovementOwnerTypeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Core.Constants.ContainerOwnership.Codes.CarrierOwned, nameof(Xsd.OwnerType.CAR));
			yield return new Mapping(Core.Constants.ContainerOwnership.Codes.Leased, nameof(Xsd.OwnerType.LEA));
			yield return new Mapping(Core.Constants.ContainerOwnership.Codes.ShipperOwned, nameof(Xsd.OwnerType.SHP));
		}

		public static readonly AgencyContainerMovementOwnerTypeMappings Instance = new AgencyContainerMovementOwnerTypeMappings();

		protected override string Name
		{
			get { return Res.GetString("7620afed-b171-4dbe-938b-bd1be63ee192", "Owner Type"); }
		}

		public new Xsd.OwnerType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.OwnerType.CAR, errorContext, notify);
		}
	}
}
