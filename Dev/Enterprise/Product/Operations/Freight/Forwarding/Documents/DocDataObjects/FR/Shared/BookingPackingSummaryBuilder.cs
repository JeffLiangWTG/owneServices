using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class BookingPackingSummaryBuilder
	{
		public BookingPackingSummary Build(CommonContainer containerBO, ZString shipmentNumber, ZString icvReference, IReadOnlyCollection<BookingPackingLine> packingLines, ZBool isFallbackTotalOutturnedDetails)
		{
			if (packingLines == null)
			{
				return null;
			}

			var shipment = containerBO.Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, shipmentNumber));
			var packingLinePKs = packingLines.Select(x => x.Identifier);

			var transitWareHouseEqualsDeliveryCFSPackLines = shipment?.OuterPackLines.OfType<PackLine>()
					.Where(x => packingLinePKs.Contains(x.PK) && x.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK == shipment.JS_OA_ImportReleaseDepot_ZAddress.OrgPK && x.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.AddressFK == shipment.JS_OA_ImportReleaseDepot_ZAddress.AddressFK)
					?? new List<PackLine>();

			var context = new CommonContext(containerBO.Factory);

			var containerSummary = new BookingPackingSummary();
			containerSummary.ShipmentNumber = shipmentNumber;
			containerSummary.ICVReference = icvReference;
			containerSummary.TotalPackages = packingLines.Sum(p => p.Quantity);
			containerSummary.TotalPackagesUnit = GetTotalPackagesUnit(packingLines);

			containerSummary.TotalWeight = new Measurement
			{
				Value = packingLines.Sum(p => p.Weight.Value),
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = Core.Constants.Weight.Kilograms
				}
			};

			containerSummary.TotalVolume = new Measurement
			{
				Value = packingLines.Sum(p => p.Volume.Value),
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = Core.Constants.Volume.CubicMetres
				}
			};

			var totalOutturnedPackages = packingLines.Sum(p => p.Outturn);
			if (totalOutturnedPackages == 0 && isFallbackTotalOutturnedDetails)
			{
				totalOutturnedPackages = transitWareHouseEqualsDeliveryCFSPackLines.Sum(x => x.PkgPackageCollection_TotalQty);
			}

			containerSummary.TotalOutturnedPackages = totalOutturnedPackages;

			var totalOutturnedWeight = packingLines.Sum(p => p.OutturnWeight.Value);
			if (totalOutturnedWeight == 0 && isFallbackTotalOutturnedDetails)
			{
				totalOutturnedWeight = transitWareHouseEqualsDeliveryCFSPackLines.Sum(x => x.PkgPackageCollection_TotalWeight);
			}

			containerSummary.TotalOutturnedWeight = new Measurement
			{
				Value = totalOutturnedWeight,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = Core.Constants.Weight.Kilograms
				}
			};

			var totalOutturnedVolume = packingLines.Sum(p => p.OutturnVolume.Value);
			if (totalOutturnedVolume == 0 && isFallbackTotalOutturnedDetails)
			{
				totalOutturnedVolume = transitWareHouseEqualsDeliveryCFSPackLines.Sum(x => x.PkgPackageCollection_TotalVolume);
			}

			containerSummary.TotalOutturnedVolume = new Measurement
			{
				Value = totalOutturnedVolume,
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = Core.Constants.Volume.CubicMetres
				}
			};

			containerSummary.UnpackedReference = ZString.Empty;
			containerSummary.SurplusIndicator = (containerSummary.TotalOutturnedPackages > containerSummary.TotalPackages);
			containerSummary.UnpackingIndicator = ZBool.False;
			containerSummary.ReserveIndicator = (packingLines.Sum(p => p.Damaged) > 0);

			containerSummary.IsAnyLastKnownTWStatusEmpty = packingLines.Any(p => p.LastKnownTransitWarehouseStatus.IsEmpty);

			return containerSummary;
		}

		ZString GetTotalPackagesUnit(IReadOnlyCollection<BookingPackingLine> packingLines)
		{
			var result = ZString.Empty;
			foreach (var packLine in packingLines)
			{
				if (result.IsEmpty)
				{
					result = packLine.PackageType.Code;
				}
				else if (packLine.PackageType.Code != result)
				{
					result = Core.Constants.PkgUnit.Package;
					break;
				}
			}

			return result.IsEmpty ? (ZString)FreightPacksDataRegistry.Instance.OuterPackUnit.Value : result;
		}
	}
}
