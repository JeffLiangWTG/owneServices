using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonBookedCtgMoveValidation : JobBookedCtgMoveValidation
	{
		public CommonBookedCtgMoveValidation(CommonBookedCtgMove parent)
			: base(parent)
		{
		}

		protected override void CheckEW_F3_NKPackType()
		{
			base.CheckEW_F3_NKPackType();
			MandatoryValidation.CheckEntered(CommonBookedCtgMove.EW_F3_NKPackTypeInfo);
			ListValidation.WarnIfInvalidCode(CommonBookedCtgMove.EW_F3_NKPackTypeInfo);
		}

		protected override void CheckEW_DimUnit()
		{
			base.CheckEW_DimUnit();
			MandatoryValidation.CheckEntered(CommonBookedCtgMove.EW_DimUnitInfo);
			ListValidation.ErrorIfInvalidCode(CommonBookedCtgMove.EW_DimUnitInfo);
		}

		protected override void CheckEW_VolumeUQ()
		{
			base.CheckEW_VolumeUQ();
			MandatoryValidation.CheckEntered(CommonBookedCtgMove.EW_VolumeUQInfo);
			ListValidation.ErrorIfInvalidCode(CommonBookedCtgMove.EW_VolumeUQInfo);
		}

		protected override void CheckEW_WeightUQ()
		{
			base.CheckEW_WeightUQ();
			MandatoryValidation.CheckEntered(CommonBookedCtgMove.EW_WeightUQInfo);
			ListValidation.ErrorIfInvalidCode(CommonBookedCtgMove.EW_WeightUQInfo);
		}

		protected override void CheckEW_DropMode()
		{
			base.CheckEW_DropMode();
			ListValidation.ErrorIfInvalidCode(CommonBookedCtgMove.EW_DropModeInfo);
		}

		protected override void CheckEW_RequestedPickupTimeStart()
		{
			base.CheckEW_RequestedPickupTimeStart();
			TimeCompareFrom(CommonBookedCtgMove.EW_RequestedPickupTimeStartInfo, CommonBookedCtgMove.EW_RequestedPickupTimeEndInfo);
		}

		protected override void CheckEW_RequestedPickupTimeEnd()
		{
			base.CheckEW_RequestedPickupTimeEnd();
			TimeCompareTo(CommonBookedCtgMove.EW_RequestedPickupTimeStartInfo, CommonBookedCtgMove.EW_RequestedPickupTimeEndInfo);
		}

		protected override void CheckEW_RequestedDeliveryTimeStart()
		{
			base.CheckEW_RequestedDeliveryTimeStart();
			TimeCompareFrom(CommonBookedCtgMove.EW_RequestedDeliveryTimeStartInfo, CommonBookedCtgMove.EW_RequestedDeliveryTimeEndInfo);
		}

		protected override void CheckEW_RequestedDeliveryTimeEnd()
		{
			base.CheckEW_RequestedDeliveryTimeEnd();
			TimeCompareTo(CommonBookedCtgMove.EW_RequestedDeliveryTimeStartInfo, CommonBookedCtgMove.EW_RequestedDeliveryTimeEndInfo);
		}

		protected override void CheckEW_E2PickupAddressID()
		{
			base.CheckEW_E2PickupAddressID();

			if (RatingParty1Address.IsEmpty)
			{
				if (!RatingParty2Address.IsEmpty || !RatingParty3Address.IsEmpty)
				{
					CommonBookedCtgMove.EW_E2PickupAddressIDInfo.AddWarning(Res.GetString("42edb28f-cab9-43be-9f94-1b6a1d609dff", "First Rating Party should have a value if Second or Third Rating Party does."));
				}
			}
			else if (RatingParty1Address == RatingParty2Address)
			{
				CommonBookedCtgMove.EW_E2PickupAddressIDInfo.AddError(Res.GetString("d906f529-e7ba-4c86-9ba2-9da1cf296143", "First Rating Party cannot be the same as Second Rating Party"));
			}
		}

		protected override void CheckEW_E2WaitPointAddressID()
		{
			base.CheckEW_E2WaitPointAddressID();

			if (RatingParty2Address.IsEmpty)
			{
				if (!RatingParty3Address.IsEmpty)
				{
					CommonBookedCtgMove.EW_E2WaitPointAddressIDInfo.AddWarning(Res.GetString("307b6467-ed33-4add-8bf7-238f231c8bd6", "Second Rating Party should have a value if Third Rating Party does."));
				}
			}
			else if (RatingParty2Address == RatingParty1Address || RatingParty2Address == RatingParty3Address)
			{
				CommonBookedCtgMove.EW_E2WaitPointAddressIDInfo.AddError(Res.GetString("90ad1d41-638b-453b-905d-75d7f01053cd", "Second Rating Party cannot be the same as First or Third Rating Party"));
			}
		}

		protected override void CheckEW_E2DeliveryAddressID()
		{
			base.CheckEW_E2DeliveryAddressID();

			if (!RatingParty3Address.IsEmpty && RatingParty3Address == RatingParty2Address)
			{
				CommonBookedCtgMove.EW_E2DeliveryAddressIDInfo.AddError(Res.GetString("8fb5caee-9572-46b7-b973-67eed7d0febb", "Third Rating Party cannot be the same as Second Rating Party"));
			}
		}

		CommonBookedCtgMove CommonBookedCtgMove
		{
			get { return (CommonBookedCtgMove)Parent; }
		}

		void TimeCompareFrom(ZPropertyInfo fromInfo, ZPropertyInfo toInfo)
		{
			ZDateTime from = (ZDateTime)fromInfo.Value;
			ZDateTime to = (ZDateTime)toInfo.Value;

			if (from > to)
			{
				fromInfo.AddError(Res.GetString("f0ce89d2-12f7-4fd0-9d53-4e5bb6625735", "{0} must be the same or earlier than {1}.", fromInfo.HumanReadableName, toInfo.HumanReadableName));
			}
		}

		void TimeCompareTo(ZPropertyInfo fromInfo, ZPropertyInfo toInfo)
		{
			ZDateTime from = (ZDateTime)fromInfo.Value;
			ZDateTime to = (ZDateTime)toInfo.Value;

			if (to < from)
			{
				toInfo.AddError(Res.GetString("741df958-5f62-4873-9e50-d0a9b7bd0a5a", "{0} must be the same or later than {1}.", toInfo.HumanReadableName, fromInfo.HumanReadableName));
			}
		}

		ZString RatingParty1Address
		{
			get { return CommonBookedCtgMove.PickupFromDocAddress != null ? CommonBookedCtgMove.PickupFromDocAddress.AddressAsASingleLine : ZString.Empty; }
		}

		ZString RatingParty2Address
		{
			get { return CommonBookedCtgMove.WaitPointDocAddress != null ? CommonBookedCtgMove.WaitPointDocAddress.AddressAsASingleLine : ZString.Empty; }
		}

		ZString RatingParty3Address
		{
			get { return CommonBookedCtgMove.DeliverToDocAddress != null ? CommonBookedCtgMove.DeliverToDocAddress.AddressAsASingleLine : ZString.Empty; }
		}
	}
}
