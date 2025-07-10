using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveValidation : WhsDocketValidation
	{
		public WhsReceiveValidation(WhsReceive parent)
			: base(parent)
		{
		}

		protected override void CheckWD_DocketType()
		{
			base.CheckWD_DocketType();
			if (Parent.WD_DocketType != CodeLists.DocketType.Codes.Receive)
			{
				Parent.WD_DocketTypeInfo.AddError(Res.GetString("a94c8aee-9d61-4c1f-a0a8-61803c787448", "The Docket Type is not set to Receive."));
			}
		}

		protected override void CheckWD_DocketStatus()
		{
			base.CheckWD_DocketStatus();
			if ((Parent.WD_DocketStatus != DocketStatus.Codes.New) &&
				(Parent.WD_DocketStatus != DocketStatus.Codes.Entered) &&
				(Parent.WD_DocketStatus != DocketStatus.Codes.Putaway) &&
				(Parent.WD_DocketStatus != DocketStatus.Codes.Cancelled) &&
				(Parent.WD_DocketStatus != DocketStatus.Codes.Finalised) &&
				(Parent.WD_DocketStatus != DocketStatus.Codes.Error))
			{
				Parent.WD_DocketStatusInfo.AddError(Res.GetString("1eb3f5e9-7499-4be5-9614-494a91cfad1e", "The Docket Status is invalid for this docket type."));
			}

			CheckUNDG();
		}

		bool IsUNDGValidationRequired => Parent.IsFinalising;

		void CheckUNDG()
		{
			var docketStatusInfo = Parent.WD_DocketStatusInfo;
			if (!docketStatusInfo.HasErrors() && IsUNDGValidationRequired)
			{
				var receive = (WhsReceive)Parent;
				var validationMessage = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(receive.DocketUNDGValidationCache).OverLimitMessage;
				if (!validationMessage.IsEmpty)
				{
					docketStatusInfo.AddError(BuildUNDGLimitExceededError(validationMessage));
				}
			}
		}

		protected override void CheckWD_BookingDate()
		{
			base.CheckWD_BookingDate();
			var today = ZDateTimeOffset.Today;
			if (Parent.WD_BookingDate >= today.AddDays(1))
			{
				Parent.WD_BookingDateInfo.AddWarning(Res.GetString("4b0b6e14-a33a-4a67-a4f9-19c2c0788c9a", "Booking date is a future date. Please check this is correct."));
			}
			else
			{
				var oneMonthAgo = today.AddMonths(-1);
				if (Parent.WD_BookingDate < oneMonthAgo)
				{
					Parent.WD_BookingDateInfo.AddWarning(Res.GetString("b911c3f2-30f5-4cb9-a5b3-8503e96f0a4c", "Booking date is over 1 month ago. Please check this is correct."));
				}
			}
		}

		#region CheckWD_ArrivalDate

		protected override void CheckWD_ArrivalDate()
		{
			base.CheckWD_ArrivalDate();

			if (Parent.IsFinalising || Receive.StartedReceiving || HasAnyReceivedOrPutawayLines)
			{
				MandatoryValidation.CheckEntered(Parent.WD_ArrivalDateInfo);
			}

			var today = ZDateTimeOffset.Today;
			if (Parent.WD_ArrivalDate < today.AddDays(-7) &&
				(Parent.Warehouse?.WW_UseArrivalDateForInwardsFinalisedDate ?? false) &&
				!Env.Security.WhsReceivePreDate.IsAllowed)
			{
				Parent.WD_ArrivalDateInfo.AddError(Res.GetString("3b655d51-f027-4638-9882-e283ee57e7a9", "You do not have the required security rights to enter an arrival date more than a week in the past."));
			}
			else
			{
				if (Parent.WD_ArrivalDate < today.AddMonths(-1))
				{
					Parent.WD_ArrivalDateInfo.AddWarning(Res.GetString("e97f8629-6e7a-43c9-a9fa-05155d994f36", "Arrival date is over 1 month ago. Please check this is correct."));
				}
				else if (Parent.WD_ArrivalDate >= today.AddDays(1))
				{
					var arrivalDateNotification = Res.GetString("f79c9bfb-e3ca-4537-af0c-94ac9d1fce1f", "Arrival date is a future date.");

					if (Parent.IsFinalising)
					{
						Parent.WD_ArrivalDateInfo.AddError(arrivalDateNotification);
					}
					else
					{
						Parent.WD_ArrivalDateInfo.AddWarning(arrivalDateNotification);
					}
				}
			}
		}

		#endregion

		#region CheckWD_TotalUnits

		protected override void CheckWD_TotalUnits()
		{
			base.CheckWD_TotalUnits();
			if (!Parent.IsCreatedFromPickByBOM && Parent.WD_TotalUnits != Parent.WD_TotalUnitsFromLines)
			{
				var errorMessage = Res.GetString("1f11259b-38de-4244-9241-a3e068391180",
					"Total Units {0} does not equal the total of all line units {1}.", Parent.WD_TotalUnits, Parent.WD_TotalUnitsFromLines);

				if (Parent.IsFinalising && WarehouseDataRegistry.Instance.TotalUnitsValidation.Value)
				{
					Parent.WD_TotalUnitsInfo.AddError(errorMessage);
				}
				else
				{
					Parent.WD_TotalUnitsInfo.AddWarning(errorMessage);
				}
			}
		}

		#endregion

		#region CheckWD_TotalPallets

		protected override void CheckWD_TotalPallets()
		{
			base.CheckWD_TotalPallets();
			if (Receive.WD_TotalPallets != Receive.TotalPalletsReceived)
			{
				var errorMessage = Res.GetString("938a8f17-8f4f-4d6f-bfcc-a8f6b63b3577",
					"Total Pallets {0} does not equal the total of received Pallets {1}.", Receive.WD_TotalPallets, Receive.TotalPalletsReceived);

				if ((Receive.IsFinalising || Receive.Lines.Count > 0) && WarehouseDataRegistry.Instance.TotalPalletsValidation.Value)
				{
					Receive.WD_TotalPalletsInfo.AddError(errorMessage);
				}
				else
				{
					Receive.WD_TotalPalletsInfo.AddWarning(errorMessage);
				}
			}
		}

		#endregion

		#region CheckWD_ReceiveCategory

		protected override void CheckWD_ReceiveCategory()
		{
			if (!Parent.IsFinalised)
			{
				ListValidation.ErrorIfInvalidCode(Parent.WD_ReceiveCategoryInfo, Lookups.ReceiveCategories);
			}
		}

		#endregion

		#region CheckWD_IsInwardsProcessingJob

		protected override void CheckWD_IsInwardsProcessingJob()
		{
			base.CheckWD_IsInwardsProcessingJob();
			WhsValidationHelper.CheckInwardProcessingJobOnlySetOnVirtualWarehouses(Parent, Parent.WD_IsInwardsProcessingJobInfo);
			CheckTransactionType(Parent.WD_IsInwardsProcessingJobInfo);
		}

		#endregion

		#region CheckTransportCoNameOrPK

		public void ValidateTransportCoNameOrPK() // used by Grid Validation
		{
			ValidateCalculatedProperty(Receive.TransportCoNameOrPKInfo);
		}

		protected void CheckTransportCoNameOrPK()
		{
			Receive.ValidateTransportCoNameOrPKInfo();
		}

		#endregion

		#region CheckWD_HoldPalletIDPutaway 

		protected override void CheckWD_HoldPalletIDPutaway()
		{
			base.CheckWD_HoldPalletIDPutaway();

			if (Receive.WD_HoldPalletIDPutaway && !((ZBool)Receive.WD_HoldPalletIDPutawayInfo.OriginalValue))
			{
				if (Receive.StartedReceiving)
				{
					var errorMessage = Res.GetString("27c257ba-c396-48c2-920d-74ddf8f95f98", "Cannot set Hold Pallet ID For RF Putaway once Receiving has started.");
					Receive.WD_HoldPalletIDPutawayInfo.AddError(errorMessage);
				}
				else if (Receive.Lines.Cast<WhsReceiveLine>().Any(l => l.HasPutawayTransfer))
				{
					var errorMessage = Res.GetString("b00f3bc3-4bba-4d9f-b1b7-019279329455", "Cannot set Hold Pallet ID For RF Putaway once a putaway transfer has been created.");
					Receive.WD_HoldPalletIDPutawayInfo.AddError(errorMessage);
				}
			}
		}

		#endregion

		protected override void CheckWD_ExternalReference()
		{
			CheckIfValidReferenceForReturnReceive();
			if (!Receive.WD_ExternalReferenceInfo.HasErrors())
			{
				base.CheckWD_ExternalReference();
			}
		}

		protected override bool ShouldCheckForDuplicateExternalReference() => !Receive.IsReturnReceive || ((IBusinessObjectInternals)Receive).IsInPreSaveValidation;

		void CheckIfValidReferenceForReturnReceive()
		{
			if (Receive.IsReturnReceive
				&& !Receive.IsAutoCreatingReceive
				&& Receive.WD_WD_ParentDocket.IsEmpty)
			{
				if (Receive.WD_ExternalReference.IsEmpty)
				{
					Receive.WD_ExternalReferenceInfo.AddWarning(WhsReceive.ReturnReceivesRequireAnOrderToReturnWarning);
				}
				else
				{
					var orderToReturn = Receive.OrderToReturn;
					if (orderToReturn == null)
					{
						Receive.WD_ExternalReferenceInfo.AddWarning(WhsReceive.ReturnReceivesRequireAnOrderToReturnWarning);
					}
					else if (Receive.IsOrderToReturnFullyReturned(orderToReturn))
					{
						Receive.WD_ExternalReferenceInfo.AddError(Res.GetString("6a1553c8-a727-453f-89be-43116d65c73c", "This reference points to a departed order that has already been fully returned or is in the process of being fully returned."));
					}
				}
			}
		}

		#region Implementation

		WhsReceive Receive => (WhsReceive)Parent;

		protected override ZString TypeInMsg => Res.GetString("36505220-4191-420e-a616-d556646785e3", "Receive");

		bool HasAnyReceivedOrPutawayLines => Parent.Lines.Any(l => l.Location != null);

		WhsReceiveLookups Lookups => (WhsReceiveLookups)Parent.Lookups;

		#endregion
	}
}
