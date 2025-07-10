using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class CommonHeaderDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, CommonHeaderDetailsControlBag> where T : Business.CusReconDeclaration
	{
		public override CommonHeaderDetailsControlBag CommonBag { get; } = CommonHeaderDetailsControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
