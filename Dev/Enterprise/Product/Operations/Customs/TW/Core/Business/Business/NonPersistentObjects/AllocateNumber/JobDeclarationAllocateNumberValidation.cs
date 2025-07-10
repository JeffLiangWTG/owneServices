namespace Enterprise.Customs.TW.Business
{
	public class JobDeclarationAllocateNumberValidation : AllocateNumberValidation
	{
		public JobDeclarationAllocateNumberValidation(AutoAllocateNumber parent) : base(parent)
		{
		}

		protected override void CheckPart2Number()
		{
			base.CheckPart2Number();
			var parent = Parent;
			var part2Number = parent.Part2Number;
			if (!part2Number.IsEmpty && part2Number != entryNumberGenerator.Part2)
			{
				parent.Part2NumberInfo.AddWarning(ValidationConstants.AllocateNumber.Part2NumberDoesNotMatch);
			}
		}

		protected override void CheckPart4Number()
		{
			base.CheckPart4Number();
			var parent = Parent;
			var part4Number = parent.Part4Number;
			if (!part4Number.IsEmpty && part4Number != entryNumberGenerator.Part4)
			{
				parent.Part4NumberInfo.AddWarning(ValidationConstants.AllocateNumber.Part4NumberDoesNotMatch);
			}
		}
	}
}
