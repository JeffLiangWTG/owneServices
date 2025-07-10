using CargoWise.Integration;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class WarehouseTransactionStatusListTest : TestCase
	{
		public void TestIsAutomationDisabled()
		{
			var list = new WarehouseTransactionStatusList();
			list.RemoveCode(WarehouseTransactionStatusList.Codes.AutomationIsDisabled);

			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, false, WarehouseTransactionStatusList.IsAutomationDisabled(pair.Code));
			}
			AssertEquals("Empty code", false, WarehouseTransactionStatusList.IsAutomationDisabled(""));
			AssertEquals(WarehouseTransactionStatusList.Codes.AutomationIsDisabled, true, WarehouseTransactionStatusList.IsAutomationDisabled(WarehouseTransactionStatusList.Codes.AutomationIsDisabled));
		}

		public void TestIsInwardCode()
		{
			var list = new WarehouseTransactionStatusList();
			foreach (var code in new[]
			{
				WarehouseTransactionStatusList.Codes.InwardCanceled,
				WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal,
				WarehouseTransactionStatusList.Codes.InwardCreated,
				WarehouseTransactionStatusList.Codes.InwardCreatedPending,
				WarehouseTransactionStatusList.Codes.InwardCreationHeld,
				WarehouseTransactionStatusList.Codes.InwardUpdated,
				WarehouseTransactionStatusList.Codes.InwardUpdatedPending
			})
			{
				list.RemoveCode(code);
				AssertEquals(code, true, WarehouseTransactionStatusList.IsInwardCode(code));
			}

			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, false, WarehouseTransactionStatusList.IsInwardCode(pair.Code));
			}
		}

		public void TestIsOutwardCode()
		{
			var list = new WarehouseTransactionStatusList();
			foreach (var code in new[]
			{
				WarehouseTransactionStatusList.Codes.OutwardCanceled,
				WarehouseTransactionStatusList.Codes.OutwardCanceledPendingWithdrawal,
				WarehouseTransactionStatusList.Codes.OutwardCreated,
				WarehouseTransactionStatusList.Codes.OutwardCreatedPending,
				WarehouseTransactionStatusList.Codes.OutwardHolding,
				WarehouseTransactionStatusList.Codes.OutwardUpdated,
				WarehouseTransactionStatusList.Codes.OutwardUpdatedPending
			})
			{
				list.RemoveCode(code);
				AssertEquals(code, true, WarehouseTransactionStatusList.IsOutwardCode(code));
			}

			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, false, WarehouseTransactionStatusList.IsOutwardCode(pair.Code));
			}
		}

		public void TestIsPendingInwardOrOutward()
		{
			AssertEquals(false, WarehouseTransactionStatusList.IsPendingInwardOrOutward(ZString.Empty));
			var list = new WarehouseTransactionStatusList();
			foreach (var code in new[]
			{
				WarehouseTransactionStatusList.Codes.InwardCreatedPending,
				WarehouseTransactionStatusList.Codes.InwardCreationHeld,
				WarehouseTransactionStatusList.Codes.InwardUpdatedPending,
				WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal,
				WarehouseTransactionStatusList.Codes.OutwardCreatedPending,
				WarehouseTransactionStatusList.Codes.OutwardUpdatedPending,
				WarehouseTransactionStatusList.Codes.OutwardCanceledPendingWithdrawal,
				WarehouseTransactionStatusList.Codes.OutwardHolding
			})
			{
				list.RemoveCode(code);
				AssertEquals(code, true, WarehouseTransactionStatusList.IsPendingInwardOrOutward(code));
			}

			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, false, WarehouseTransactionStatusList.IsPendingInwardOrOutward(pair.Code));
			}
		}

		public void TestIsPendingInward()
		{
			AssertEquals(false, WarehouseTransactionStatusList.IsPendingInward(ZString.Empty));
			var list = new WarehouseTransactionStatusList();
			foreach (var code in new[]
			{
				WarehouseTransactionStatusList.Codes.InwardCreatedPending,
				WarehouseTransactionStatusList.Codes.InwardCreationHeld,
				WarehouseTransactionStatusList.Codes.InwardUpdatedPending,
				WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal
			})
			{
				list.RemoveCode(code);
				AssertEquals(code, true, WarehouseTransactionStatusList.IsPendingInward(code));
			}

			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, false, WarehouseTransactionStatusList.IsPendingInward(pair.Code));
			}
		}

		public void TestIsPendingOutward()
		{
			AssertEquals(false, WarehouseTransactionStatusList.IsPendingOutward(ZString.Empty));
			var list = new WarehouseTransactionStatusList();
			foreach (var code in new[]
			{
				WarehouseTransactionStatusList.Codes.OutwardCreatedPending,
				WarehouseTransactionStatusList.Codes.OutwardUpdatedPending,
				WarehouseTransactionStatusList.Codes.OutwardCanceledPendingWithdrawal,
				WarehouseTransactionStatusList.Codes.OutwardHolding
			})
			{
				list.RemoveCode(code);
				AssertEquals(code, true, WarehouseTransactionStatusList.IsPendingOutward(code));
			}

			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, false, WarehouseTransactionStatusList.IsPendingOutward(pair.Code));
			}
		}

		public void TestIsChangeOfOwnershipCode()
		{
			var list = new WarehouseTransactionStatusList();
			foreach (var code in new[]
			{
				WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCanceled,
				WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreated,
				WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreatedPending,
				WarehouseTransactionStatusList.Codes.ChangeOfOwnershipHolding,
				WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdated,
				WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdatedPending
			})
			{
				list.RemoveCode(code);
				AssertEquals(code, true, WarehouseTransactionStatusList.IsChangeOfOwnershipCode(code));
			}

			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, false, WarehouseTransactionStatusList.IsChangeOfOwnershipCode(pair.Code));
			}
		}

		public void TestIsPendingChangeOfOwnership()
		{
			AssertEquals(false, WarehouseTransactionStatusList.IsPendingChangeOfOwnership(ZString.Empty));
			var list = new WarehouseTransactionStatusList();
			foreach (var code in new[]
			{
				WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreatedPending,
				WarehouseTransactionStatusList.Codes.ChangeOfOwnershipHolding,
				WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdatedPending
			})
			{
				list.RemoveCode(code);
				AssertEquals(code, true, WarehouseTransactionStatusList.IsPendingChangeOfOwnership(code));
			}

			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, false, WarehouseTransactionStatusList.IsPendingChangeOfOwnership(pair.Code));
			}
		}

		public void TestIsChangeOfRegimeCode()
		{
			var list = new WarehouseTransactionStatusList();
			foreach (var code in new[]
			{
				WarehouseTransactionStatusList.Codes.ChangeOfRegimeCanceled,
				WarehouseTransactionStatusList.Codes.ChangeOfRegimeCreated,
				WarehouseTransactionStatusList.Codes.ChangeOfRegimeCreatedPending,
				WarehouseTransactionStatusList.Codes.ChangeOfRegimeHolding,
				WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdated,
				WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdatedPending
			})
			{
				list.RemoveCode(code);
				AssertEquals(code, true, WarehouseTransactionStatusList.IsChangeOfRegimeCode(code));
			}

			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, false, WarehouseTransactionStatusList.IsChangeOfRegimeCode(pair.Code));
			}
		}

		public void TestIsPendingChangeOfRegime()
		{
			AssertEquals(false, WarehouseTransactionStatusList.IsPendingChangeOfRegime(ZString.Empty));
			var list = new WarehouseTransactionStatusList();
			foreach (var code in new[]
			{
				WarehouseTransactionStatusList.Codes.ChangeOfRegimeCreatedPending,
				WarehouseTransactionStatusList.Codes.ChangeOfRegimeHolding,
				WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdatedPending
			})
			{
				list.RemoveCode(code);
				AssertEquals(code, true, WarehouseTransactionStatusList.IsPendingChangeOfRegime(code));
			}

			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, false, WarehouseTransactionStatusList.IsPendingChangeOfRegime(pair.Code));
			}
		}

		public void TestHasWHSTransaction()
		{
			AssertEquals(false, WarehouseTransactionStatusList.HasWHSTransaction(ZString.Empty));
			var list = new WarehouseTransactionStatusList();
			foreach (var code in new[]
			{
				WarehouseTransactionStatusList.Codes.InwardCanceled,
				WarehouseTransactionStatusList.Codes.OutwardCanceled,
				WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCanceled,
				WarehouseTransactionStatusList.Codes.ChangeOfRegimeCanceled,
				WarehouseTransactionStatusList.Codes.AutomationIsDisabled
			})
			{
				list.RemoveCode(code);
				AssertEquals(code, false, WarehouseTransactionStatusList.HasWHSTransaction(code));
			}

			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, true, WarehouseTransactionStatusList.HasWHSTransaction(pair.Code));
			}
		}

		public void TestGetErrorMessageIfPending()
		{
			AssertEquals(ZString.Empty, WarehouseTransactionStatusList.GetErrorMessageIfPending(ZString.Empty));
			var list = new WarehouseTransactionStatusList();
			var message = "There is a Warehouse Transaction pending, please close the form and retry when there is a response from Customs. Alternatively, disable the warehouse integration and re-enable it after receiving all response back from Customs.";
			foreach (var code in new[]
			{
				WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreatedPending,
				WarehouseTransactionStatusList.Codes.ChangeOfOwnershipHolding,
				WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdatedPending,
				WarehouseTransactionStatusList.Codes.ChangeOfRegimeCreatedPending,
				WarehouseTransactionStatusList.Codes.ChangeOfRegimeHolding,
				WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdatedPending,
				WarehouseTransactionStatusList.Codes.InwardCreatedPending,
				WarehouseTransactionStatusList.Codes.InwardCreationHeld,
				WarehouseTransactionStatusList.Codes.InwardUpdatedPending,
				WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal,
				WarehouseTransactionStatusList.Codes.OutwardCreatedPending,
				WarehouseTransactionStatusList.Codes.OutwardUpdatedPending,
				WarehouseTransactionStatusList.Codes.OutwardCanceledPendingWithdrawal,
				WarehouseTransactionStatusList.Codes.OutwardHolding
			})
			{
				list.RemoveCode(code);
				AssertEquals(code, message, WarehouseTransactionStatusList.GetErrorMessageIfPending(code));
			}

			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code, ZString.Empty, WarehouseTransactionStatusList.GetErrorMessageIfPending(pair.Code));
			}
		}
	}
}
