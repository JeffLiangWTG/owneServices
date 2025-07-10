namespace Enterprise.Customs.ZA.Business
{
	public abstract class AsycudaBillValidation : ManifestBase.AsycudaBillValidation
	{
		protected AsycudaBillValidation(AsycudaBill parent)
			: base(parent)
		{
			header = Parent.Header;
		}

		public void ValidateCustomsCPC() => ValidateCalculatedProperty(Parent.CustomsCPCInfo);

		public void ValidateLRN() => ValidateCalculatedProperty(Parent.LRNInfo);

		public void ValidateMRN() => ValidateCalculatedProperty(Parent.MRNInfo);

		protected virtual void CheckCustomsCPC() { }

		protected virtual void CheckLRN() { }

		protected virtual void CheckMRN() { }

		public new AsycudaBill Parent => (AsycudaBill)base.Parent;

		protected bool HeaderNotNullWithType => header != null && (header.IsCOSTCO || header.IsGOVGIO);

		protected AsycudaManifestHeader header;
	}
}
