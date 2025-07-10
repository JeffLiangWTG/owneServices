namespace Enterprise.Customs.TW.Business
{
	public class AllocateNumberValidation : AutoAllocateNumberValidation
	{
		public AllocateNumberValidation(AutoAllocateNumber parent) : base(parent)
		{
			entryNumberGenerator = Parent.EntryNumberGenerator;
		}

		protected BaseEntryNumberGenerator entryNumberGenerator;

		public new AllocateNumber Parent => (AllocateNumber)base.Parent;

		protected override void CheckPart5Number()
		{
			base.CheckPart5Number();
			var parent = Parent;
			var targetInfo = parent.Part5NumberInfo;
			var part5Number = parent.Part5Number;
			var sequenceformatter = entryNumberGenerator?.Sequenceformatter;
			if (!part5Number.IsEmpty && sequenceformatter != null)
			{
				var nextNumber = sequenceformatter.FormatStringToInt(part5Number);
				if (nextNumber <= 0)
				{
					targetInfo.AddError(sequenceformatter.AllowedFormatDescription);
				}
				else
				{
					var description = entryNumberGenerator.CheckNextNumberIsInRange(nextNumber, parent.allowPart5NumberOutrangeWhenEntered);
					if (!description.IsEmpty)
					{
						targetInfo.AddError(description);
					}
					else if (entryNumberGenerator.ExistingEntry(parent.Number))
					{
						targetInfo.AddError(ValidationConstants.AllocateNumber.AlreadyExists);
					}
				}
			}
		}
	}
}
