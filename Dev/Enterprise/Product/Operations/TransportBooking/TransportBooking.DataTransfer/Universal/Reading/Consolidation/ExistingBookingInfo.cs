using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;

namespace Enterprise.TransportBookings.DataTransfer
{
	public class ExistingBookingInfo
	{
		public ExistingBookingInfo(DtbBooking booking)
		{
			Booking = booking;
			Template = booking.KM_KT_NKBookingTemplate;
			PackageIDs = booking.Packages_PackageView.Cast<DtbBookingPackage_PackageView>().Select(p => p.Package.PackageIDWithFallback).ToList(); // resolve now
			var containers = booking.Packages_PackageView.Cast<DtbBookingPackage_PackageView>().Where(p => p.Package.IsContainer).ToArray();
			ContainerIDs = containers.Select(p => p.Package.PackageIDWithFallback).ToArray(); // resolve now
			HasEmptyContainers = containers.Any(p => p.Package.KP_PackageID.IsEmpty);
			IsLooseOnly = booking.IsLooseOnly;
		}

		public readonly DtbBooking Booking;
		public readonly ZString Template;
		public readonly ZBool IsLooseOnly;
		public readonly IEnumerable<ZString> PackageIDs;
		public readonly IEnumerable<ZString> ContainerIDs;
		public bool HasEmptyContainers;

		public void RepopulateOrCancel(IEnumerable<ExistingBookingInfo> existingBookingInfos)
		{
			if (!Booking.Instructions.Any())
			{
				var packagesAlreadyInUse = GetMatchingPackagesWithNoIDAlreadyUsedOnOtherBookingsOfSameTemplate(existingBookingInfos);
				var matchingPackages = GetMatchingPackages(packagesAlreadyInUse);
				if (matchingPackages.Any())
				{
					using (Booking.SuspendSettingPackages())
					{
						Booking.KM_KT_NKBookingTemplate = Template;
					}

					Booking.DefaultPackages(matchingPackages, null);
				}
				else
				{
					Booking.Deactivate();
				}
			}
		}

		IEnumerable<PkgPackage> GetMatchingPackagesWithNoIDAlreadyUsedOnOtherBookingsOfSameTemplate(IEnumerable<ExistingBookingInfo> existingBookingInfos)
		{
			return existingBookingInfos.Where(b => b.Booking.PK != Booking.PK && b.Template == Template).SelectMany(b => b.Booking.AssignedPackages).Where(p => p.KP_PackageID.IsEmpty);
		}

		IEnumerable<PkgPackage> GetMatchingPackages(IEnumerable<PkgPackage> excludedPackages)
		{
			var availablePackages = Booking.ConsolidationSingleJob.PackageJob.GetAllPackagesOnJob().Except(excludedPackages).ToList();

			var matchingPackages = new List<PkgPackage>();
			foreach (var packageID in PackageIDs)
			{
				var matchingPackage = availablePackages.FirstOrDefault(p => p.PackageIDWithFallback.EqualsIgnoringCase(packageID));
				if (matchingPackage != null)
				{
					matchingPackages.Add(matchingPackage);
					availablePackages.Remove(matchingPackage);
				}
			}

			return matchingPackages;
		}
	}
}
