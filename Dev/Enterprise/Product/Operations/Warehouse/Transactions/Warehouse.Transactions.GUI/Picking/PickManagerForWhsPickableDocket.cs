using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class PickManagerForWhsPickableDocket : PickManager
	{
		public PickManagerForWhsPickableDocket(ZForm form, WhsPickableDocket pickableDocket)
			: base(form)
		{
			PickableDocket = Argument.NotNull(pickableDocket, "WhsPickableDocket pickableDocket");
		}

		readonly WhsPickableDocket PickableDocket;

		protected override WhsPick Pick => pick ?? PickableDocket.Pick;
		WhsPick pick;

		#region CanContinueWithPickOrders

		/// <summary>
		/// Ensuring the the Order is saved before picking is a *GUI* requirement. 
		/// </summary>
		protected override bool CanContinueWithPickOrders => !PickableDocket.HasChanges && PickableDocket.IsInDatabase;

		protected override void OnCannotContinueWithPickOrders()
		{
			base.OnCannotContinueWithPickOrders();
			Globals.Message.ShowError(Res.GetString("761c7805-8b7f-437f-8e29-634fc990c65d", "Please save this {0} before Picking it.", PickableDocket.Description));
		}

		#endregion

		#region PickOrders

		protected override void PickOrdersCore()
		{
			if (TryReloadingTheOrder()) // Reload order to check for existing pick, if order is null it has been deleted.
			{
				if (IsPickNullOrCancelled) // user might go to the Release screen and cancel the pick, then try re-picking with the same PickManager instance
				{
					CreatePickForConcurrencyTest();
					CreateOrShowPick();
				}
				else
				{
					// Pick exists therefore, load the pick rather than creating it again.
					ShowExistingPick();
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("a45243da-c31b-4750-b137-2bece18ab489", "{0} has been deleted by another user.", PickableDocket.Description));
				ParentForm.Close();
			}
		}

		void CreateOrShowPick()
		{
			using (var mutex = new WhsPickCreationMutex(PickableDocket))
			{
				mutex.Lock();
				if (mutex.HasLock) // if the lock is granted, no users are creating a pick.
				{
					try
					{
						CreateOrShowPick(mutex);
					}
					finally
					{
						if (mutex.HasLock)
						{
							mutex.Unlock();
						}
					}
				}
				else
				{
					Globals.Message.ShowError(mutex.GetMessage());
				}
			}
		}

		void CreateOrShowPick(WhsPickCreationMutex mutex)
		{
			if (IsPickNullOrCancelled)
			{
				var newPick = PickableDocket.Factory.New<WhsPick>();

				using (new DisposableAction(
					() => pick = newPick,
					() => { pick = null; UnhookEvents(); }))
				{
					HookEvents();

					var pickSuccess = newPick.PickOrders(new[] { PickableDocket });
					if (!pickSuccess)
					{
						mutex.Unlock();
						newPick.Delete();
					}
				}
			}
			else
			{
				mutex.Unlock();
				ShowExistingPick();
			}
		}

		void ShowExistingPick()
		{
			using (new DisposableAction(() => UnhookEvents()))
			{
				HookEvents();

				// we want the pick form opened and nothing else to occur when opening from the WorkOrder/Order form (if pick exists)
				Pick.PickOrdersOnlyIfPickNotInDb();
			}
		}

		#region IsPickNullOrCancelled

		bool IsPickNullOrCancelled
		{
			get
			{
				var pickToCheck = Pick;
				return pickToCheck == null || pickToCheck.IsCancelled;
			}
		}

		bool TryReloadingTheOrder()
		{
			var result = false;
			if (PickableDocket != null)
			{
				var query = new ZDBOnlyQuery(typeof(WhsDocket));
				query.AddToFilter(new ZQuery(WhsDocketSchema.PK, PickableDocket.PK));
				query.ReLoadExistingRows = true;
				var docket = PickableDocket.Factory.LoadTop1<WhsDocket>(query);
				result = docket != null;
			}
			return result;
		}

		#endregion

		#endregion

		partial void CreatePickForConcurrencyTest();
	}
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class PickManagerForWhsPickableDocket
	{
		public delegate void CreateNewPickForTesting();
		public CreateNewPickForTesting CreateNewPick;

		partial void CreatePickForConcurrencyTest()
		{
			CreateNewPick?.Invoke();
		}
	}
}

#endif
#endregion
