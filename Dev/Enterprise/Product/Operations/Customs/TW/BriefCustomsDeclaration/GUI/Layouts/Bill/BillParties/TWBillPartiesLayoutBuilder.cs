using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	public class TWBillPartiesLayoutBuilder<T> : ColumnLayoutBuilder<T, TWBillPartiesControlBag> where T : Business.AsycudaBill
	{
		public override TWBillPartiesControlBag CommonBag { get; } = TWBillPartiesControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
