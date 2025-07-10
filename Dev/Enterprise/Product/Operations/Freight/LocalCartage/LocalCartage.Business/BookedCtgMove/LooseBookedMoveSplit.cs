using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class LooseBookedMoveSplit : AutoLooseBookedMoveSplit
	{
		public LooseBookedMoveSplit(LooseBookedMoveSplitMaster master)
			: base(master.Factory)
		{
			Master = master;
		}
		public readonly LooseBookedMoveSplitMaster Master;

		public override void Delete()
		{
			if (!IsDeleted)
			{
				Packs = 0;
				Weight = 0;
				Volume = 0;
			}
			base.Delete();
		}

		public override ZInt Packs
		{
			get { return base.Packs; }
			set
			{
				if (!settingPacks)
				{
					try
					{
						settingPacks = true;
						base.Packs = value;
						Master.TotalPacksInfo.RefreshBinding();
						Master.Validation.ValidateRemainingPacks();
						Weight = Master.Packs == 0 ? 0 : Master.Weight * value / Master.Packs;
						Volume = Master.Packs == 0 ? 0 : Master.Volume * value / Master.Packs;

						if (Master.Splits.Count == 2)
						{
							LooseBookedMoveSplit splitToChange = Master.Splits[0] == this ? Master.Splits[1] : Master.Splits[0];
							splitToChange.Packs = Master.Packs - value;
						}
					}
					finally
					{
						settingPacks = false;
					}
				}
			}
		}
		bool settingPacks;

		public override ZDecimal Weight
		{
			get { return base.Weight; }
			set
			{
				if (!settingWeight)
				{
					try
					{
						settingWeight = true;
						base.Weight = value;
						Master.TotalWeightInfo.RefreshBinding();
						Master.Validation.ValidateRemainingWeight();
						if (Master.Splits.Count == 2)
						{
							LooseBookedMoveSplit splitToChange = Master.Splits[0] == this ? Master.Splits[1] : Master.Splits[0];
							splitToChange.Weight = Master.Weight - value;
						}
					}
					finally
					{
						settingWeight = false;
					}
				}
			}
		}
		bool settingWeight;

		public override ZDecimal Volume
		{
			get { return base.Volume; }
			set
			{
				if (!settingVolume)
				{
					try
					{
						settingVolume = true;
						base.Volume = value;
						Master.TotalVolumeInfo.RefreshBinding();
						Master.Validation.ValidateRemainingVolume();
						if (Master.Splits.Count == 2)
						{
							LooseBookedMoveSplit splitToChange = Master.Splits[0] == this ? Master.Splits[1] : Master.Splits[0];
							splitToChange.Volume = Master.Volume - value;
						}
					}
					finally
					{
						settingVolume = false;
					}
				}
			}
		}
		bool settingVolume;

		[List("BindingLists.OuterPackTypes")]
		public override ZString PackType
		{
			get { return Master.PackType; }
		}

		[List("BindingLists.WeightUnits")]
		public override ZString WeightUnit
		{
			get { return Master.WeightUnit; }
		}

		[List("BindingLists.VolumeUnits")]
		public override ZString VolumeUnit
		{
			get { return Master.VolumeUnit; }
		}

		public BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}
	}
}
