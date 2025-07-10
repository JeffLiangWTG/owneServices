using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingPackageCollection_PackageView : NonPersistentBusinessObjectCollection<DtbBookingPackage_PackageView>
	{
		public DtbBookingPackageCollection_PackageView(DtbBooking booking)
			: base(booking.Factory)
		{
			Booking = booking;
		}
		readonly DtbBooking Booking;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DtbBookingPackage_PackageView(Factory);
		}

		public IEnumerable<DtbBookingPackage_PackageView> Typed
		{
			get
			{
				foreach (DtbBookingPackage_PackageView packageView in this)
				{
					yield return packageView;
				}
			}
		}

		public void Initialise()
		{
			if (initialised)
			{
				throw new InvalidOperationException("PackageInPackageViewCollection has already been initialied.");
			}

			initialised = true;

			// if booking is deleted we should unhook the Packages collection
			// but we shouldn't need to as Booking.Packages.CountChanged will never be called after the booking is deleted
			Booking.AssignedPackages.CountChanged += new EventHandler(Packages_CountChanged);
			RefreshCollection();
		}

		bool initialised;

		void Packages_CountChanged(object sender, EventArgs e)
		{
			RefreshCollection();
		}

		public void RefreshCollection()
		{
			RemoveUnassignedPackages();
			AddNewlyAssignedPackages();
		}

		void RemoveUnassignedPackages()
		{
			foreach (var package_packageView in this.ToArray<DtbBookingPackage_PackageView>())
			{
				if (!Booking.AssignedPackages.Contains(package_packageView.Package))
				{
					Remove(package_packageView);
				}
			}
		}

		void AddNewlyAssignedPackages()
		{
			var wrappedPackages = Array.ConvertAll(this.ToArray<DtbBookingPackage_PackageView>(), p => p.Package);

			foreach (var package in Booking.AssignedPackages)
			{
				if (!wrappedPackages.Contains(package))
				{
					var packageInPackageView = GetPackage_PackageView(package, Booking);
					Add(packageInPackageView);
				}
			}
		}

		DtbBookingPackage_PackageView GetPackage_PackageView(PkgPackage package, DtbBooking booking)
		{
			return new DtbBookingPackage_PackageView(package, booking);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public DtbBookingPackage_PackageView Find(PkgPackage package)
		{
			return this.ToArray<DtbBookingPackage_PackageView>().FirstOrDefault(p => p.Package == package);
		}
	}
}
