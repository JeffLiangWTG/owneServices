using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class CusTWProductLabelRangeCollection : DependentBusinessObjectCollection<CusTWProductLabelRange, CusTWControllingMessageHeader>
	{
		public CusTWProductLabelRangeCollection(CusTWControllingMessageHeader master) : base(master)
		{
			MaxCountValidationEnable(maxAllowed);
		}

		protected override bool AllowNewCore => Count < maxAllowed;

		const int maxAllowed = 99;
	}
}
