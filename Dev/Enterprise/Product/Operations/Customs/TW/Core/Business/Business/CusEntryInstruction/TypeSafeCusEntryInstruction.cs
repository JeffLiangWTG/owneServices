namespace Enterprise.Customs.TW.Business
{
	public partial class CusEntryInstruction : AutoTWCusEntryInstruction
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		public new CusEntryInstruction Clone()
		{
			return (CusEntryInstruction)base.Clone();
		}

		public new CusEntryInstructionValidation Validation
		{
			get { return (CusEntryInstructionValidation)base.Validation; }
		}

		public new CusEntryInstructionLookups Lookups
		{
			get { return (CusEntryInstructionLookups)base.Lookups; }
		}

		#endregion

		#region Implementation

		#region protected override

		protected override Customs.Business.CusEntryInstructionValidation GetNewValidation()
		{
			return new CusEntryInstructionValidation(this);
		}

		protected override Customs.Business.CusEntryInstructionLookups GetNewLookups()
		{
			return new CusEntryInstructionLookups(this);
		}
		#endregion

		#endregion
	}
}
