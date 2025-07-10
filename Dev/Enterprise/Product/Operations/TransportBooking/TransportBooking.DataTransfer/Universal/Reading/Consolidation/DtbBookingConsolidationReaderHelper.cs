using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public static class DtbBookingConsolidationReaderHelper
	{
		public static void ReadInCartageAdvice(DtbBookingConsolidation consolidation, Shipment sourceDO, IEnumerable<ExistingBookingInfo> existingBookingInfos, Shipment topLevelDO, IXmlImportLogger logger, UniversalObjectFactory factory, Dictionary<ZInt, PkgPackage> packageContainerLinks, IEnumerable<PkgPackage> assignedPackages = null)
		{
			var emptyContainerPks = new List<ZGuid>();
			var emptyBookings = new List<ZGuid>();
			var bookingDOs = GetBookingDataObjectsForCartageAdvice(sourceDO);

			foreach (var bookingDO in bookingDOs)
			{
				var template = bookingDO.LocalTransportJobType.GetCodeAsUpperCase();
				var containerIDs = DtbBookingDataObjectReader.GetContainerIDs(bookingDO);

				var datasource = sourceDO?.DataContext?.DataSourceCollection?.FirstOrDefault();
				var anyEmptyContainers = (!containerIDs.Any() && bookingDO.ContainerCollection != null && bookingDO.ContainerCollection.Any());
				var existingBookings = existingBookingInfos
					.Where(b => b.Template == template
					&& b.Booking.KM_IsActive
					&& (b.IsLooseOnly || (anyEmptyContainers ? (b.HasEmptyContainers && !emptyBookings.Contains(b.Booking.PK)) : !b.ContainerIDs.Except(containerIDs).Any()))
					&& IsDataSourceNotFromDCNOrExistingBookingMatchesDCNKey(b.Booking, datasource));

				var first = existingBookings.FirstOrDefault();

				if (anyEmptyContainers && first != null)
				{
					emptyBookings.Add(first.Booking.PK);
				}

				var bookingReader = new DtbBookingDataObjectReaderFromCartageAdvice(bookingDO, logger, factory, consolidation, sourceDO, topLevelDO, first != null ? first.Booking : null, packageContainerLinks, emptyContainerPks, assignedPackages);
				bookingReader.ReadIntoBusinessObject();
			}

			// existing

			foreach (var bookingInfo in existingBookingInfos)
			{
				bookingInfo.RepopulateOrCancel(existingBookingInfos);
			}
		}

		static bool IsDataSourceNotFromDCNOrExistingBookingMatchesDCNKey(DtbBooking booking, IDataSourceDataObject dataSource)
		{
			return dataSource == null || !dataSource.Type.HasValue || !dataSource.Key.HasValue || dataSource.Type.Value != nameof(DataContextType.TransitDispatch)
			|| !booking.JobLinks.Any() || booking.JobLinks.Any(link => link.UCL_SourceType == dataSource.Type.Value && link.UCL_SourceKey == dataSource.Key.Value);
		}

		/// <summary>
		/// We can import from the 
		///		Source Data Object
		///		or
		///		Child Transport Bookings (where each Container has it's own Booking)
		///	If there are Child Transport Bookings, import those, otherwise fallback to the Source Data Object
		/// </summary>
		/// <param name="sourceDO"></param>
		/// <returns></returns>
		public static IEnumerable<Shipment> GetBookingDataObjectsForCartageAdvice(Shipment sourceDO)
		{
			var bookingDOs = new List<Shipment>();

			var subShipments = sourceDO.SubShipmentCollection;
			if (subShipments != null)
			{
				bookingDOs.AddRange(subShipments.Where(s => s.ShipmentType.GetCodeAsUpperCase() == "TBK"));
			}

			return bookingDOs.Any() ? bookingDOs.ToArray() : new[] { sourceDO };
		}

		public static IEnumerable<ExistingBookingInfo> GetExistingBookingInfos(DtbBookingConsolidation consolidation)
		{
			var existingBookingInfos = new List<ExistingBookingInfo>();
			foreach (var booking in consolidation.Bookings)
			{
				existingBookingInfos.Add(new ExistingBookingInfo(booking));
			}

			return existingBookingInfos;
		}
	}
}
