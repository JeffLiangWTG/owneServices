namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaPackCollection : ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>
	{
		public AsycudaPackCollection(AsycudaBill master) : base(master)
		{
			MaxCountValidationEnable(maxAllowed);
		}

		protected override bool AllowNewCore => Count < maxAllowed;

		const int maxAllowed = 9999;
	}
}
