using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;

namespace Enterprise.TransportCommon.Business
{
	public abstract class PackageCollection_PackageView : NonPersistentBusinessObjectCollection<Package_PackageView>
	{
		protected PackageCollection_PackageView(DtbTransport transport)
			: base(transport.Factory)
		{
			Transport = transport;
		}
		readonly DtbTransport Transport;

		#region Initialise

		public void Initialise()
		{
			if (initialised)
			{
				throw new InvalidOperationException("PackageInPackageViewCollection has already been initialied.");
			}

			initialised = true;

			// if booking is deleted we should unhook the Packages collection
			// but we shouldn't need to as Booking.Packages.CountChanged will never be called after the booking is deleted
			Transport.AssignedPackages.CountChanged += new EventHandler(Packages_CountChanged);
			RefreshCollection();
		}

		bool initialised;

		void Packages_CountChanged(object sender, EventArgs e)
		{
			RefreshCollection();
		}

		#endregion

		#region RefreshCollection

		public void RefreshCollection()
		{
			RemoveUnassignedPackages();
			AddNewlyAssignedPackages();
		}

		void RemoveUnassignedPackages()
		{
			foreach (var package_packageView in this.ToArray<Package_PackageView>())
			{
				if (!Transport.AssignedPackages.Contains(package_packageView.Package))
				{
					Remove(package_packageView);
				}
			}
		}

		void AddNewlyAssignedPackages()
		{
			var wrappedPackages = Array.ConvertAll(this.ToArray<Package_PackageView>(), p => p.Package);

			foreach (var package in Transport.AssignedPackages)
			{
				if (!wrappedPackages.Contains(package))
				{
					var packageInPackageView = GetPackage_PackageView(package, Transport);
					Add(packageInPackageView);
				}
			}
		}

		protected abstract Package_PackageView GetPackage_PackageView(PkgPackage package, DtbTransport transport);

		#endregion

		#region AllowNew

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion

		#region Find

		public Package_PackageView Find(PkgPackage package)
		{
			return this.ToArray<Package_PackageView>().FirstOrDefault(p => p.Package == package);
		}

		#endregion
	}
}
