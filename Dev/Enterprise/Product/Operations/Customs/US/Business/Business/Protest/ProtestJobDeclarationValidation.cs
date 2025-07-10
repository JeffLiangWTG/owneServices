namespace Enterprise.Customs.US.Business.Protest
{
	public class ProtestJobDeclarationValidation : JobDeclarationValidation
	{
		public ProtestJobDeclarationValidation(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void CheckJE_MessageType()
		{
		}

		protected override void CheckJE_MessageSubType()
		{
		}

		public override bool IsMasterBillMandatory
		{
			get { return false; }
		}

		protected override bool JE_MergeByRequired
		{
			get { return false; }
		}

		protected override bool ShouldValidatePackagesActualPackageCount
		{
			get { return false; }
		}

		protected override void CheckJE_TransportMode()
		{
		}
	}
}
