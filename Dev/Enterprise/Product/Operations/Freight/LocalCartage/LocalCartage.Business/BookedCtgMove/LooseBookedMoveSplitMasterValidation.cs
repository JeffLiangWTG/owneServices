namespace Enterprise.Freight.LocalCartage.Business
{
	public class LooseBookedMoveSplitMasterValidation : AutoLooseBookedMoveSplitMasterValidation
	{
		public LooseBookedMoveSplitMasterValidation(AutoLooseBookedMoveSplitMaster parent)
			: base(parent) { }

		public void ValidateRemainingPacks()
		{
			ValidateCalculatedProperty(Parent.RemainingPacksInfo);
		}
		protected virtual void CheckRemainingPacks()
		{
			if (!Parent.RemainingPacks.IsEmpty)
			{
				if (Parent.AllowDiscrepancy)
				{
					Parent.RemainingPacksInfo.AddWarning(Res.GetString("d0219633-f0c8-48ab-892d-1a1a93dbf41d", "{0} doesn't match original.", Parent.RemainingPacksInfo.HumanReadableName));
				}
				else
				{
					Parent.RemainingPacksInfo.AddError(Res.GetString("92102159-63b4-4f7a-8a98-1dc23abb49d4", "{0} doesn't match original.", Parent.RemainingPacksInfo.HumanReadableName));
				}
			}
		}

		public void ValidateRemainingWeight()
		{
			ValidateCalculatedProperty(Parent.RemainingWeightInfo);
		}
		protected virtual void CheckRemainingWeight()
		{
			if (!Parent.RemainingWeight.IsEmpty)
			{
				if (Parent.AllowDiscrepancy)
				{
					Parent.RemainingWeightInfo.AddWarning(Res.GetString("1fc5873c-697b-4dc2-aef9-b4a5fa4b2947", "{0} doesn't match original.", Parent.RemainingWeightInfo.HumanReadableName));
				}
				else
				{
					Parent.RemainingWeightInfo.AddError(Res.GetString("1b95e5bb-7bd5-4337-b91e-773af90a2ecf", "{0} doesn't match original.", Parent.RemainingWeightInfo.HumanReadableName));
				}
			}
		}

		public void ValidateRemainingVolume()
		{
			ValidateCalculatedProperty(Parent.RemainingVolumeInfo);
		}
		protected virtual void CheckRemainingVolume()
		{
			if (!Parent.RemainingVolume.IsEmpty)
			{
				if (Parent.AllowDiscrepancy)
				{
					Parent.RemainingVolumeInfo.AddWarning(Res.GetString("7404be60-cca7-424e-af0b-da7eebd2115c", "{0} doesn't match original.", Parent.RemainingVolumeInfo.HumanReadableName));
				}
				else
				{
					Parent.RemainingVolumeInfo.AddError(Res.GetString("13094fe0-e2a5-40d5-a817-2c607cc6bb57", "{0} doesn't match original.", Parent.RemainingVolumeInfo.HumanReadableName));
				}
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateRemainingPacks();
			ValidateRemainingVolume();
			ValidateRemainingWeight();
		}

		public new LooseBookedMoveSplitMaster Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (LooseBookedMoveSplitMaster)base.Parent; }
		}
	}
}
