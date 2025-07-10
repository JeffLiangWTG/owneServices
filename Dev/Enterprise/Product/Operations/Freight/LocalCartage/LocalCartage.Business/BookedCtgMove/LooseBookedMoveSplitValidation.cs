using CargoWise.ComponentModel;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class LooseBookedMoveSplitValidation : AutoLooseBookedMoveSplitValidation
	{
		public LooseBookedMoveSplitValidation(AutoLooseBookedMoveSplit parent)
			: base(parent) { }

		protected override void CheckPacks()
		{
			base.CheckPacks();

			if (!Parent.PacksInfo.HasErrors() && Parent.Packs <= 0)
			{
				Parent.PacksInfo.AddError(Res.GetString("5a1f5487-73ad-46de-a5f7-96175f969da7", "{0} needs to be greater than Zero.", Parent.PacksInfo.HumanReadableName));
			}
		}

		protected override void CheckWeight()
		{
			base.CheckWeight();

			if (!Parent.WeightInfo.HasErrors() && Parent.Weight < 0)
			{
				Parent.WeightInfo.AddError(Res.GetString("973f881a-6d6c-4223-9401-821c049e1c46", "{0} needs to be greater than or equal to Zero.", Parent.WeightInfo.HumanReadableName));
			}
		}

		protected override void CheckVolume()
		{
			base.CheckVolume();

			if (!Parent.VolumeInfo.HasErrors() && Parent.Volume < 0)
			{
				Parent.VolumeInfo.AddError(Res.GetString("73b2da73-82ad-4e0b-92f6-4ba61e2bcc7d", "{0} needs to be greater than or equal to Zero.", Parent.VolumeInfo.HumanReadableName));
			}
		}

		public new LooseBookedMoveSplit Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (LooseBookedMoveSplit)base.Parent; }
		}
	}
}
