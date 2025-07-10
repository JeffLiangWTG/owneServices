using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public static class GUITestHelper
	{
		#region AssertQueryUserMethodInNotificationSubscriberGuiHelperIsBeingCalled

		public static void VerifyQueryUserMethodInNotificationSubscriberGuiHelperIsBeingCalled(INotificationSubscriberQueryUser caller, Mock<INotificationSubscriberQueryUser> helperMock)
		{
			var e = new TestIQueryUserEventArgs();
			caller.QueryUser(e);
			helperMock.Verify(b => b.QueryUser(e), Times.Once);
			Assertion.Assert(true);
		}

		#endregion

		#region AssertHandleSaveExceptions

		public static void AssertHandleSaveException_ShowsMessageWhenTriggersFail(Func<ZForm> getNewForm)
		{
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, "Over-pick attempt.", WhsExceptionHandler.PreventOverCommitOfStockViaPickLineTriggerMsgForUser);
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, "Attempt to over-reduce TotalUnits.", ZFormExtensionsTest.AssertLastMessageIsOverReduceStock);
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, WhsDocketLine.PreventAdjustmentInLinesWithPickLinesTriggerID, WhsExceptionHandler.WhsDocketLine_PreventAdjustmentInLinesWithPickLinesMsgForUser);
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, WarehouseErrorMessages.PreventTransactionAndPickQuantityDesyncTriggerID, WarehouseErrorMessages.TransactionAndPickedQtyIsCorrectMsgForUser);
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, WhsDocketLine.PreventOverflowLocationQuantityTriggerID, ZFormExtensionsTest.AssertLastMessagePreventOverflowLocationQuantityTriggerIDMsgForUser);
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, WhsDocketLine.PreventIncorrectFinalisedDateDocketLineTriggerID, WhsExceptionHandler.PreventIncorrectDateOrStatusDocketLineTriggerMsgForUser);
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, WhsDocketLine.PreventIncorrectStatusDocketLineTriggerID, WhsExceptionHandler.PreventIncorrectDateOrStatusDocketLineTriggerMsgForUser);
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, WhsDocketLine.PreventDesynchronisingInterWhsTransferLines, WhsExceptionHandler.PreventDesynchronisingInterWhsTransferLinesMsgForUser);
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, WhsDocketLine.PreventLocationForIncorrectWarehouseTriggerException, WhsExceptionHandler.PreventLocationForIncorrectWarehouseTriggerMsgForUser);
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, WhsPickLine.PreventPickLineFromLinkingToIncorrectTransactionLine, WhsExceptionHandler.WhsPickLine_PreventFromLinkingToIncorrectTransactionLineMsgForUser);
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, WhsPickLine.PreventCriticalFieldChangeWhenPickLineIsFinalizedTriggerID, WhsExceptionHandler.WhsPickLine_PreventCriticalFieldChangeWhenLineIsFinalizedMsgForUser);
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, WhsPickLine.PreventTransactionsPickingFromDifferentWarehouseTriggerID, WhsExceptionHandler.WhsPickLine_PreventTransactionsPickingFromDifferentWarehouseMsgForUser);
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, WhsDocket.PreventChangeOfCriticalFieldsWhenDocketHasLines, WhsExceptionHandler.PreventSubTypeOrWarehouseChangeWhenDocketHasLinesTriggerMsgForUser);
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, WhsOrder.PreventChangeOfWarehouse, WhsExceptionHandler.PreventWarehouseChangeWhenDocketHasCrossDockLocationOrReservedStockOrIsNotNewOrEnteredTriggerMsgForUser);
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, WhsDocket.PreventChangeOfClientWhenDocketHasLines, WhsExceptionHandler.PreventClientChangeWhenDocketHasLinesTriggerMsgForUser);
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, WhsDocket.PreventDetachedOrderPickLines, WhsExceptionHandler.WhsDocket_PreventDetachedOrderPickLinesTriggerMsgForUser);
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, WhsDocket.PreventOrderAndWorkOrderWarehouseNotMatchingPicksWarehouse, WhsExceptionHandler.WhsDocketAndPick_PreventNotMatchingWarehousesTriggerMsgForUser);
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, WhsDocketLine.PreventAttemptToMakeAllocatedInventoryUnallocateable, WhsExceptionHandler.PreventAttemptToMakeAllocatedInventoryUnallocateableMsgForUser);
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, WhsPickLine.PreventAttemptToAlllocateUnallocateableInventory, WhsExceptionHandler.PreventAttemptToAlllocateUnallocateableInventoryMsgForUser);
		}

		static void AssertHandleSaveException_ShowMessageWhenTriggersFail(Func<ZForm> getNewForm, string triggerFailErrorMessage, string expectedLastMessageShown)
		{
			AssertHandleSaveException_ShowMessageWhenTriggersFail(getNewForm, triggerFailErrorMessage,
				() => ZFormExtensionsTest.AssertEquals(expectedLastMessageShown, UnitTestUserNotification.Instance.LastMessage.Text));
		}

		static void AssertHandleSaveException_ShowMessageWhenTriggersFail(Func<ZForm> getNewForm, string triggerFailErrorMessage, Action assertLastMessageShown)
		{
			using (var form = getNewForm())
			{
				form.ValidatingForSave += delegate
				{
					var factory = form.BusinessEntity.Factory;
					var connection = ((IDbConnected)factory).Connection;
					throw new ZSaveException(new ZDataException(new ArgumentException(triggerFailErrorMessage), ((INeedRow)form.BusinessEntity).Row, connection), factory);
				};

				UnitTestUserNotification.Instance.ClearMessages();
				form.FireSaveButton();
			}

			assertLastMessageShown();
		}

		#endregion

		#region FindControl

		public static T FindControl<T>(Control.ControlCollection controls, string name)
			where T : Control
		{
			foreach (Control control in controls)
			{
				var controlAsT = control as T;
				if (controlAsT != null && controlAsT.Name == name)
				{
					return controlAsT;
				}

				var childControl = FindControl<T>(control.Controls, name);
				if (childControl != null)
				{
					return childControl;
				}
			}

			return null;
		}

		#endregion

		#region FindControlByText

		public static T FindControlByText<T>(Control.ControlCollection controls, string text)
			where T : Control
		{
			foreach (Control control in controls)
			{
				var controlAsT = control as T;
				if (controlAsT != null && controlAsT.Text == text)
				{
					return controlAsT;
				}

				var childControl = FindControlByText<T>(control.Controls, text);
				if (childControl != null)
				{
					return childControl;
				}
			}

			return null;
		}

		#endregion

		#region Extension Methods

		public static void SelectElements(this ZGrid grid, params BusinessObject[] bizObjs)
		{
			var list = grid.List;
			var listManager = grid.ListManager;

			grid.UnSelectAll();

			if (bizObjs != null && bizObjs.Length > 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (bizObjs.Contains(list[i]))
					{
						grid.Select(i);
					}
				}
			}
		}

		#endregion
	}
}
