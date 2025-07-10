using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Constants = Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ForwardingLibrary : MacroLibraryBase
	{
		public override IEnumerator<IMacroMetaData> GetEnumerator() => lazyMacrosRegister.Value.GetEnumerator();

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly Lazy<ICollection<IMacroMetaData>> lazyMacrosRegister = new Lazy<ICollection<IMacroMetaData>>(() => Load(MacroHandlers));

		#region SuppressResourceStringsCheckRegion

		static IEnumerable<IHandler> MacroHandlers
		{
			get
			{
				yield return new Handler<Func<IMacroScope, IDynamicData, IEnumerable>>(
					"PackingLines",
					"Returns packlines for a container.",
					(scope, data) => GetPackingLines((IDynamicData)scope.GetRootData(), data));

				yield return new Handler<Func<IMacroScope, IDynamicData, IDynamicData>>(
					"Shipment",
					"Returns Shipment which this Packing Line belongs to.",
					(scope, packingLine) => GetShipment((IDynamicData)scope.GetRootData(), packingLine));

				yield return new Handler<Func<IMacroScope, IDynamicData, IDynamicData>>(
					"Container",
					"Returns a container which this Packing Line belongs to.",
					(scope, packingLine) => GetContainer(scope, packingLine));
			}
		}

		#endregion

		#region GetPackingLines

		static IEnumerable GetPackingLines(IDynamicData shipment, IDynamicData data)
		{
			if (data != null)
			{
				if (data.Type == typeof(UniversalShipment))
				{
					return GetPackingLines(data);
				}

				if (data.Type == typeof(UniversalDataBuss.DataObjects.Universal.Container))
				{
					var containerLink = (ZInt?)data.GetDynamicProperty(nameof(UniversalDataBuss.DataObjects.Universal.Container.Link)).Value; // UXML element name

					if (containerLink.HasValue)
					{
						return GetPackingLines(shipment, containerLink.Value);
					}
				}
			}

			return null;
		}

		static IEnumerable GetPackingLines(IDynamicData shipment, int? containerLinkId = null)
		{
			if (shipment == null
				|| shipment.Type != typeof(UniversalShipment))
			{
				yield break;
			}

			var shipmentType = Convert.ToString(shipment.GetDynamicProperty($"{nameof(UniversalShipment.ShipmentType)}.{nameof(CodeDescriptionPair.Code)}"));

			// only BCN/STD/3PT shipments have their own packinglines
			if (shipmentType == Constants.ShipmentTypes.BuyersConsolLead
				|| shipmentType == Constants.ShipmentTypes.StandardHouse
				|| shipmentType == Constants.ShipmentTypes.ThirdPartyOwnershipHouse)
			{
				foreach (var packingLine in (IDynamicDataCollection)shipment.GetDynamicProperty(nameof(UniversalShipment.PackingLineCollection)))
				{
					var containerLink = (ZInt?)packingLine.GetDynamicProperty(nameof(UniversalDataBuss.DataObjects.Universal.PackingLine.ContainerLink)).Value;

					if (containerLinkId == null || containerLink == containerLinkId)
					{
						yield return packingLine;
					}
				}
			}

			foreach (var subShipment in (IDynamicDataCollection)shipment.GetDynamicProperty(nameof(UniversalShipment.SubShipmentCollection)))
			{
				foreach (var packingLine in GetPackingLines(subShipment, containerLinkId))
				{
					yield return packingLine;
				}
			}
		}

		#endregion

		#region GetShipment

		static IDynamicData GetShipment(IDynamicData shipment, IDynamicData packingLine)
		{
			if (shipment == null
				|| shipment.Type != typeof(UniversalShipment)
				|| packingLine == null
				|| packingLine.Type != typeof(UniversalDataBuss.DataObjects.Universal.PackingLine))
			{
				return null;
			}

			return packingLine.Parent?.Parent;
		}

		#endregion

		#region GetContainer

		static IDynamicData GetContainer(IMacroScope scope, IDynamicData packingLine)
		{
			if (packingLine == null || packingLine.Type != typeof(UniversalDataBuss.DataObjects.Universal.PackingLine))
			{
				return null;
			}

			var containerLink = (ZInt?)packingLine.GetDynamicProperty(nameof(UniversalDataBuss.DataObjects.Universal.PackingLine.ContainerLink)).Value;
			var rootData = scope.GetRootData() as IDynamicData;

			if (!containerLink.HasValue || string.IsNullOrEmpty(containerLink.ToString()))
			{
				return null;
			}

			return GetContainer(rootData, containerLink);
		}

		static IDynamicData GetContainer(IDynamicData shipment, int? containerLink)
		{
			if (shipment == null || shipment.Type != typeof(UniversalShipment))
			{
				return null;
			}

			foreach (var container in (IDynamicDataCollection)shipment.GetDynamicProperty(nameof(UniversalShipment.ContainerCollection)))
			{
				if (GetContainerLink(container) == containerLink)
				{
					return container;
				}
			}

			var relatedShipmentList = new List<IDynamicData>();

			relatedShipmentList.AddRange((IDynamicDataCollection)shipment.GetDynamicProperty(nameof(UniversalShipment.ParentShipmentCollection)));
			relatedShipmentList.AddRange((IDynamicDataCollection)shipment.GetDynamicProperty(nameof(UniversalShipment.SubShipmentCollection)));

			foreach (var relatedShipment in relatedShipmentList)
			{
				var result = GetContainer(relatedShipment, containerLink);

				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		static int? GetContainerLink(IDynamicData dynamicData)
		{
			if (dynamicData != null && dynamicData.Type == typeof(UniversalDataBuss.DataObjects.Universal.Container))
			{
				return (ZInt?)dynamicData.GetDynamicProperty(nameof(UniversalDataBuss.DataObjects.Universal.Container.Link)).Value; // UXML element name
			}

			return null;
		}

		#endregion
	}
}
