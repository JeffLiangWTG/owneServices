using CargoWise.Common;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class GeneratePickManager
	{
		#region GeneratePick

		public static void GeneratePick(WhsPickableDocket pickableDocket, IGeneratePickLogger logger, bool saveFactory = true)
		{
			Argument.NotNull(logger, nameof(logger));
			Argument.NotNull(pickableDocket, nameof(pickableDocket));

			if (pickableDocket.WD_WP.IsValid)
			{
				logger.LogError(Res.GetString("f830123e-0573-4a3d-aefb-3ed16b3d072b", "Order already has a Pick."));
			}
			else
			{
				PickOrder(pickableDocket, logger, saveFactory);
			}
		}

		#endregion

		#region PickOrder

		static void PickOrder(WhsPickableDocket pickableDocket, IGeneratePickLogger logger, bool saveFactory)
		{
			var pick = pickableDocket.Factory.New<WhsPick>();
			pick.PickOrdersFailed += (sender, args) => HandlePickFailure(pick, logger, args.Message.TrimEnd());
			pick.SaveFailureEvent += (sender, e) => HandleSaveFailure(pick, logger, pickableDocket.WD_DocketID); // this will catch concurrency exceptions too
			pick.AutoPickAttempt += (sender, e) =>
			{
				if (!e.StockWasAllocated && !e.IsPickWaitingReplenishment)
				{
					HandlePickFailure(pick, logger, Res.GetString("eaf99584-1e67-4aff-9c29-02c2529e2a4f", "No Stock was allocated.") + (e.AutoAllocateErrorMessage.IsEmpty ? "" : " " + e.AutoAllocateErrorMessage));
				}
				else if (e.IsPickWaitingReplenishment)
				{
					logger.LogWarning(e.Message);
				}
			};

			pick.PickOrders(pickableDocket, saveFactory);
		}

		#endregion

		#region HandlePickFailure

		static void HandlePickFailure(WhsPick pick, IGeneratePickLogger logger, string message)
		{
			logger.LogError(pick, message);
			UndoPick(pick);
		}

		static void HandleSaveFailure(WhsPick pick, IGeneratePickLogger logger, string orderID)
		{
			logger.LogSaveConcurrencyException(pick, orderID);
			UndoPick(pick);
		}

		static void UndoPick(WhsPick pick)
		{
			pick.CancelPick();
			pick.Delete();
		}

		#endregion
	}
}
