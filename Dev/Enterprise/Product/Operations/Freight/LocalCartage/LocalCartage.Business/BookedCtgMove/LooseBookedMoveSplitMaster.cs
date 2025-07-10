using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class LooseBookedMoveSplitMaster : AutoLooseBookedMoveSplitMaster
	{
		public LooseBookedMoveSplitMaster(CommonBookedCtgMove m)
			: base(m.Factory, m.EW_BookedPackCount, m.EW_F3_NKPackType, m.EW_BookedWeight, m.EW_WeightUQ, m.EW_BookedVolume, m.EW_VolumeUQ)
		{
			MoveToSplit = m;
		}
		public readonly CommonBookedCtgMove MoveToSplit;

		public static void Group(CommonBookedCtgMove[] moves)
		{
			if (moves.Length > 1)
			{
				ZInt packageCount = 0;
				ZDecimal weight = 0;
				ZDecimal volume = 0;

				CommonBookedCtgMove masterMove = moves[0];
				for (int i = 0; i < moves.Length; i++)
				{
					CommonBookedCtgMove move = moves[i];

					packageCount += move.EW_BookedPackCount;

					if (Constants.Weight.ContainsCode(move.EW_WeightUQ) && Constants.Weight.ContainsCode(masterMove.EW_WeightUQ))
					{
						weight += Constants.Weight.Convert(move.EW_BookedWeight, move.EW_WeightUQ, masterMove.EW_WeightUQ);
					}
					else
					{
						weight += move.EW_BookedWeight;
					}

					if (Constants.Volume.ContainsCode(move.EW_VolumeUQ) && Constants.Volume.ContainsCode(masterMove.EW_VolumeUQ))
					{
						volume += Constants.Volume.Convert(move.EW_BookedVolume, move.EW_VolumeUQ, masterMove.EW_VolumeUQ);
					}
					else
					{
						volume += move.EW_BookedVolume;
					}

					if (move != masterMove)
					{
						move.Delete();
					}
				}

				masterMove.EW_BookedPackCount = packageCount;
				masterMove.EW_BookedWeight = weight;
				masterMove.EW_BookedVolume = volume;
			}
		}

		[List("BindingLists.OuterPackTypes")]
		public override ZString PackType
		{
			get { return base.PackType; }
		}

		[List("BindingLists.WeightUnits")]
		public override ZString WeightUnit
		{
			get { return base.WeightUnit; }
		}

		[List("BindingLists.VolumeUnits")]
		public override ZString VolumeUnit
		{
			get { return base.VolumeUnit; }
		}

		public override ZInt TotalPacks
		{
			get
			{
				ZInt result = 0;
				foreach (LooseBookedMoveSplit split in Splits)
				{
					result += split.Packs;
				}
				return result;
			}
		}

		public override ZDecimal TotalWeight
		{
			get
			{
				ZDecimal result = 0;
				foreach (LooseBookedMoveSplit split in Splits)
				{
					result += split.Weight;
				}
				return result;
			}
		}

		public override ZDecimal TotalVolume
		{
			get
			{
				ZDecimal result = 0;
				foreach (LooseBookedMoveSplit split in Splits)
				{
					result += split.Volume;
				}
				return result;
			}
		}

		public override ZInt RemainingPacks
		{
			get { return Packs - TotalPacks; }
		}

		public override ZDecimal RemainingWeight
		{
			get { return Weight - TotalWeight; }
		}

		public override ZDecimal RemainingVolume
		{
			get { return Volume - TotalVolume; }
		}

		public override ZBool AllowDiscrepancy
		{
			get { return base.AllowDiscrepancy; }
			set
			{
				base.AllowDiscrepancy = value;
				Validation.ValidateRemainingPacks();
				Validation.ValidateRemainingWeight();
				Validation.ValidateRemainingVolume();
			}
		}

		[ChildEditable()]
		public LooseBookedMoveSplitCollection Splits
		{
			get
			{
				if (splits == null)
				{
					splits = new LooseBookedMoveSplitCollection(this);
					RegisterEditableChildObject(splits);

					splits.AddNew();
					splits.AddNew();
					DivideEqually();
				}
				return splits;
			}
		}
		LooseBookedMoveSplitCollection splits;

		public void DivideEqually()
		{
			for (int i = 0; i < Splits.Count; i++)
			{
				LooseBookedMoveSplit split = Splits[i];
				split.Packs = (Packs + i) / Splits.Count;
			}
			Validation.ValidateAll();
			foreach (LooseBookedMoveSplit split in Splits)
			{
				split.Validation.ValidateAll();
			}
		}

		public void AddRemaining()
		{
			LooseBookedMoveSplit remainingSplit = Splits.AddNew();

			ZInt remainingPacks = RemainingPacks >= 0 ? RemainingPacks : ZInt.Zero;
			ZDecimal remainingWeight = RemainingWeight >= 0 ? RemainingWeight : 0;
			ZDecimal remainingVolume = RemainingVolume >= 0 ? RemainingVolume : 0;

			if (!remainingPacks.IsEmpty)
			{
				remainingSplit.Packs = remainingPacks;
			}

			if (!remainingWeight.IsEmpty)
			{
				remainingSplit.Weight = remainingWeight;
			}

			if (!remainingVolume.IsEmpty)
			{
				remainingSplit.Volume = remainingVolume;
			}
		}

		public void Done()
		{
			for (int i = 0; i < Splits.Count; i++)
			{
				LooseBookedMoveSplit split = Splits[i];
				CommonBookedCtgMove move = i == 0 ? MoveToSplit : MoveToSplit.Cartage.LooseBookedMoves.AddNew();

				move.EW_BookedPackCount = split.Packs;
				move.EW_F3_NKPackType = split.PackType;
				move.EW_BookedWeight = split.Weight;
				move.EW_WeightUQ = split.WeightUnit;
				move.EW_BookedVolume = split.Volume;
				move.EW_VolumeUQ = split.VolumeUnit;

				if (move != MoveToSplit)
				{
					foreach (UNDGDataItem dgItem in MoveToSplit.UNDGs)
					{
						UNDGDataItem newItem = move.UNDGs.AddNew();
						newItem.DI_DG = dgItem.DI_DG;
						newItem.DI_DGFlashPoint = dgItem.DI_DGFlashPoint;
						newItem.DI_OC_DGContact = dgItem.DI_OC_DGContact;
					}

					move.EW_DropMode = MoveToSplit.EW_DropMode;
					move.EW_E2PickupAddressID = MoveToSplit.EW_E2PickupAddressID;
					move.EW_E2WaitPointAddressID = MoveToSplit.EW_E2WaitPointAddressID;
					move.EW_E2DeliveryAddressID = MoveToSplit.EW_E2DeliveryAddressID;
					move.EW_RequestedPickupTimeStart = MoveToSplit.EW_RequestedPickupTimeStart;
					move.EW_RequestedPickupTimeEnd = MoveToSplit.EW_RequestedPickupTimeEnd;
					move.EW_RequestedDeliveryTimeStart = MoveToSplit.EW_RequestedDeliveryTimeStart;
					move.EW_RequestedDeliveryTimeEnd = MoveToSplit.EW_RequestedDeliveryTimeEnd;

					move.CartageLegs.DeleteAll();
					MoveToSplit.CartageLegs.ApplySort(JobContainerLegsSchema.Constants.JU_DisplayOrder, System.ComponentModel.ListSortDirection.Ascending);
					foreach (CommonCartageLeg cartageLegToCopy in MoveToSplit.CartageLegs)
					{
						CommonCartageLeg cartageLeg = move.CartageLegs.AddNew();
						cartageLeg.JU_PlannedPickupTime = cartageLegToCopy.JU_PlannedPickupTime;
						cartageLeg.JU_EstimatedDeliveryTime = cartageLegToCopy.JU_EstimatedDeliveryTime;
						cartageLeg.JU_E2PickupAddressID = cartageLegToCopy.JU_E2PickupAddressID;
						cartageLeg.JU_E2WaitPointAddressID = cartageLegToCopy.JU_E2WaitPointAddressID;
						cartageLeg.JU_E2DeliveryAddressID = cartageLegToCopy.JU_E2DeliveryAddressID;
						if (cartageLegToCopy.JU_EY_RunSheet.IsValid)
						{
							cartageLeg.JU_EY_RunSheet = cartageLegToCopy.JU_EY_RunSheet;
						}
						else
						{
							cartageLeg.QuickGSDriver = cartageLegToCopy.QuickGSDriver;
							cartageLeg.QuickOHTransportCompany = cartageLegToCopy.QuickOHTransportCompany;
							cartageLeg.QuickRQTruck = cartageLegToCopy.QuickRQTruck;
						}
					}
				}
			}
		}

		public BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}
	}
}
