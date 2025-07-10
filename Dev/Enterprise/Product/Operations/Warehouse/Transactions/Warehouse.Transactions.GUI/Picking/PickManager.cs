using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Printing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.GUI
{
	/// <summary>
	/// Manages the interaction between the business and GUI for picking.
	/// </summary>
	public abstract class PickManager
	{
		#region Constructor

		protected PickManager(ZForm parentForm)
		{
			Argument.NotNull(parentForm, "parentForm");
			Argument.NotNull(parentForm as INotifications, "parentForm as INotifications");

			this.parentForm = parentForm;
		}

		readonly ZForm parentForm;

		protected abstract WhsPick Pick { get; }

		#endregion

		#region ParentForm

		protected ZForm ParentForm => parentForm;

		#endregion

		#region PickOrders

		public void PickOrders()
		{
			if (CanContinueWithPickOrders)
			{
				PickOrdersCore();
			}
			else
			{
				OnCannotContinueWithPickOrders();
			}
		}

		protected virtual bool CanContinueWithPickOrders => true;

		protected abstract void PickOrdersCore();

		protected virtual void OnCannotContinueWithPickOrders()
		{
		}

		protected void HookEvents()
		{
			Pick.AutoPickAttempt += new EventHandler<WhsPick.AutoPickEventArgs>(Pick_AutoPickAttempt);
			Pick.ManualPickSucceeded += new EventHandler(Pick_ManualPickSucceeded);
			Pick.PickOrdersFailed += new EventHandler<WhsPick.DocketPickabilityEventArgs>(Pick_PickOrdersFailed);
			Pick.SaveFailureEvent += new EventHandler<WhsPick.SaveFailureEventArgs>(Pick_SaveFailure);
		}

		protected void UnhookEvents()
		{
			if (Pick != null)
			{
				Pick.AutoPickAttempt -= new EventHandler<WhsPick.AutoPickEventArgs>(Pick_AutoPickAttempt);
				Pick.ManualPickSucceeded -= new EventHandler(Pick_ManualPickSucceeded);
				Pick.PickOrdersFailed -= new EventHandler<WhsPick.DocketPickabilityEventArgs>(Pick_PickOrdersFailed);
				Pick.SaveFailureEvent -= new EventHandler<WhsPick.SaveFailureEventArgs>(Pick_SaveFailure);
			}
		}

		#endregion

		#region Save Failure

		void Pick_SaveFailure(object sender, WhsPick.SaveFailureEventArgs e)
		{
			HandleAutoPickSaveFailure(e.Message);
		}

		#endregion

		#region Auto-Pick Success (Print Pick Documents or Show No Stock Allocated Message)

		void Pick_AutoPickAttempt(object sender, WhsPick.AutoPickEventArgs e)
		{
			AutoPickAttempt(e);
		}

		void AutoPickAttempt(WhsPick.AutoPickEventArgs e)
		{
			if (e.StockWasAllocated)
			{
				if (e.FulfillmentRulesMet)
				{
					HandlePickAutoCreateSuccess(e);
				}
				else
				{
					HandPickAutoCreateWithFulfillmentRulesNotMet(e.Message);
				}
			}
			else
			{
				HandlePickAutoCreateWithNoStockAllocated(e.Message);
			}
		}

		void HandPickAutoCreateWithFulfillmentRulesNotMet(ZString message)
		{
			Globals.Message.ShowWarning(message);
			ShowPickFormForExistingPick();
		}

		void HandlePickAutoCreateSuccess(WhsPick.AutoPickEventArgs e)
		{
			if (!e.IsPickWaitingReplenishment)
			{
				Globals.Message.ShowInformation(e.Message);
				ShowPickSlip();
			}
			else
			{
				Globals.Message.ShowWarning(e.Message);
			}
		}

		void ShowPickSlip()
		{
			var buffer = new NotificationBuffer();
			new WhsPickDocumentsAutoPrinter(Pick, buffer).PrintDocument();
			if (buffer.HasErrors)
			{
				Globals.Message.ShowError(buffer.AsString);
			}
		}

		void HandlePickAutoCreateWithNoStockAllocated(string message)
		{
			Globals.Message.ShowWarning(message);
		}

		void HandleAutoPickSaveFailure(ZString message)
		{
			Globals.Message.ShowError(message);
		}

		#endregion

		#region Manual Pick or Pick Already Exists (Open Pick Form), or Pick Failed (Show Message)

		void Pick_ManualPickSucceeded(object sender, EventArgs e)
		{
			ShowPickFormForExistingPick();
		}

		void Pick_PickOrdersFailed(object sender, WhsPick.DocketPickabilityEventArgs e)
		{
			if (e.PickAlreadyExists)
			{
				ShowPickFormForExistingPick();
			}
			else if (e.MessageType == NotificationTypes.Error)
			{
				var whsPick = sender as WhsPick;
				if (whsPick != null)
				{
					using (var messageBox = new ZErrorMessageBox(whsPick, e.Message, ""))
					{
						ZFormModaliser.ShowMessageBoxWithoutDispose(messageBox);
					}
				}
				else
				{
					Globals.Message.ShowError(e.Message);
				}
			}
			else if (e.MessageType == NotificationTypes.Warning)
			{
				Globals.Message.ShowWarning(e.Message);
			}
			else
			{
				Globals.Message.ShowInformation(e.Message);
			}
		}

		void ShowPickFormForExistingPick()
		{
			if (parentForm.DataSource is WhsDocket order) // If it's not order, then it must already be in a pick form
			{
				var pickController = ZControllerFactory.Create(ControllerIDs.WhsPicking);
				pickController.SetFormsModalTo(parentForm);

				#region Test
#if DEBUG
				if (Globals.IsTest)
				{
					LastUsedPickControllerForTest = pickController;
				}
#endif
				#endregion

				var pickForm = pickController.ShowEditForm(Pick) as PickEntryForm; // no need to load the pick in another factory as the zarc will do this

				if (pickForm != null) // form will be null if security denied
				{
					pickForm.SetInitialOrderToSelectInGrid(order.PK);
					pickForm.ShowPickSlipTab();
				}
			}
		}

		#endregion

		#region GetCartonizationProgressForm

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public static IDisposable GetCartonizationProgressForm(string status, ZForm parentForm)
		{
			var progressForm = new ProgressForm();
			progressForm.Status = status;
			progressForm.ShowProgressBar = false;
			progressForm.ShowCancelButton = false;
			ZFormModaliser.Show(progressForm, parentForm);
			Application.DoEvents();

			return progressForm;
		}

		#endregion

		#region Test
#if DEBUG

		public ZController LastUsedPickControllerForTest
		{
			get;
			private set;
		}

#endif
		#endregion
	}
}
