using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.TransportBookings.Business
{
	public class ConfirmationCollection_PackageView : NonPersistentBusinessObjectCollection<Confirmation_PackageView>
	{
		public ConfirmationCollection_PackageView(DtbBookingPackage_PackageView packageView)
			: base(packageView.Factory)
		{
			PackageView = packageView;
		}

		readonly DtbBookingPackage_PackageView PackageView;

		public void Initialise()
		{
			if (initialise)
			{
				throw new InvalidOperationException("ConfirmationCollection_PackageView has already been initialised.");
			}

			initialise = true;

			// if packageView (wrapped package) is deleted we should unhook the Confirmations collection
			PackageView.Confirmations.CountChanged += new EventHandler(Confirmations_CountChanged);
			RefreshCollection();
		}

		bool initialise;

		void Confirmations_CountChanged(object sender, EventArgs e)
		{
			RefreshCollection();
		}

		void RefreshCollection()
		{
			if (!SuspendRefreshCollection.IsSuspended)
			{
				RemoveUnassignedConfirmations();
				AddUnassignedConfirmations();
			}
		}

		void RemoveUnassignedConfirmations()
		{
			foreach (var confirmationView in this.ToArray<Confirmation_PackageView>())
			{
				if (!PackageView.Confirmations.Contains(confirmationView.Confirmation))
				{
					Remove(confirmationView);
				}
			}
		}

		void AddUnassignedConfirmations()
		{
			var wrappedConfirmations = Array.ConvertAll(this.ToArray<Confirmation_PackageView>(), p => p.Confirmation);

			foreach (DtbBookingConfirmation confirmation in PackageView.Confirmations)
			{
				if (!wrappedConfirmations.Contains(confirmation))
				{
					var confirmationInPackageView = new Confirmation_PackageView(confirmation, PackageView);
					Add(confirmationInPackageView);
				}
			}
		}

		Semaphore SuspendRefreshCollection
		{
			get { return suspendRefreshCollection ?? (suspendRefreshCollection = new Semaphore()); }
		}

		Semaphore suspendRefreshCollection;

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			DtbBookingConfirmation confirmation;

			using (new SemaphoreManager(SuspendRefreshCollection))
			{
				confirmation = PackageView.Confirmations.AddNew();
			}

			return new Confirmation_PackageView(confirmation, PackageView);
		}
	}
}
