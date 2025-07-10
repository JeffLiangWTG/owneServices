using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	public static class Extensions
	{
		#region Consol

		public static CommonConsol WithContainerMode(this CommonConsol consol, string containerMode)
		{
			consol.JK_ConsolMode = containerMode;

			return consol;
		}

		public static CommonConsol WithTransport(this CommonConsol consol, string transportMode)
		{
			consol.JK_TransportMode = transportMode;

			return consol;
		}

		public static CommonContainer AddContainer(
			this CommonConsol consol,
			string containerType = "20GP",
			string commodity = "GEN",
			short count = 1,
			string number = null,
			string containerMode = "FCL",
			decimal pivotBreak = 0,
			IEnumerable<PackLine> packLines = null)
		{
			var container = consol.Containers.AddNew();
			container.JC_ContainerMode = containerMode;
			container.JC_RC = consol.Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType).PK;
			container.JC_RH_NKContainerCommodityCode = commodity;
			container.JC_ContainerNum = number;
			container.JC_ContainerCount = count;
			container.JC_PivotBreak = pivotBreak;

			if (packLines != null)
			{
				container.PackLines.RemoveAll();
				container.PackLines.AddRange(packLines);
			}

			return container;
		}

		public static Transport AddTransport(this CommonConsol consol, string origin, string destination, OrgHeader departureFrom = null, OrgHeader arrivalAt = null, string bookingReference = null, OrgHeader carrier = null, OrgHeader creditor = null)
		{
			var transport = consol.Transports.Count == 1 && consol.Transports[0].JW_RL_NKLoadPort.IsEmpty && consol.Transports[0].JW_RL_NKDiscPort.IsEmpty
				? consol.Transports[0]
				: consol.Transports.AddNew();

			transport.JW_RL_NKLoadPort = origin;
			transport.JW_RL_NKDiscPort = destination;
			transport.JW_TransportMode = consol.JK_TransportMode;
			transport.JW_IsLinked = false;
			transport.JW_CarrierBookingReference = bookingReference;

			if (departureFrom != null)
			{
				transport.JW_OA_DepartureLocation = departureFrom.MainAddress.PK;
			}

			if (arrivalAt != null)
			{
				transport.JW_OA_ArrivalLocation = arrivalAt.MainAddress.PK;
			}

			transport.JW_OA_CarrierAddress = carrier != null ? carrier.MainAddress.PK : ZGuid.Empty;
			transport.JW_OA_CreditorAddress = creditor != null ? creditor.MainAddress.PK : ZGuid.Empty;

			return transport;
		}

		#endregion

		#region Shipment

		public static TShipment With<TShipment>(this TShipment shipment, ZGuid jS_JX) where TShipment : CommonShipment
		{
			shipment.JS_JX = jS_JX;
			return shipment;
		}

		public static PackLine AddPackLine(
			this CommonShipment shipment,
			int count = 1,
			string packType = Constants.PkgUnit.Pallet,
			decimal weight = 100,
			decimal volume = 0,
			string weightUnit = "KG",
			string volumeUnit = "M3",
			string commodity = "GEN",
			IEnumerable<PackLine> innerPacks = null)
		{
			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_ActualWeight = weight;
			packline.JL_ActualWeightUQ = weightUnit;
			packline.JL_ActualVolume = volume;
			packline.JL_ActualVolumeUQ = volumeUnit;
			packline.JL_F3_NKPackType = packType;
			packline.JL_RH_NKCommodityCode = commodity;
			packline.JL_PackageCount = count;
			packline.JL_JC = ZGuid.Empty;

			if (innerPacks != null)
			{
				foreach (var innerPack in innerPacks)
				{
					innerPack.JL_JL_OuterPackLine = packline.PK;
				}
			}

			return packline;
		}

		public static PackLine AddInnerPackLine(
			this CommonShipment shipment,
			int count = 1,
			string packType = Constants.PkgUnit.Pallet,
			decimal weight = 100,
			decimal volume = 0,
			string weightUnit = "KG",
			string volumeUnit = "M3")
		{
			var packline = shipment.InnerPackLines.AddNew();
			packline.JL_ActualWeight = weight;
			packline.JL_ActualWeightUQ = weightUnit;
			packline.JL_ActualVolume = volume;
			packline.JL_ActualVolumeUQ = volumeUnit;
			packline.JL_F3_NKPackType = packType;
			packline.JL_PackageCount = count;

			return packline;
		}

		#endregion
	}
}
