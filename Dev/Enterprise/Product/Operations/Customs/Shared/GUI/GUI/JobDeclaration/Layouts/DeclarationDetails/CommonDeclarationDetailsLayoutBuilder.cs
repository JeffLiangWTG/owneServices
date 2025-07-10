using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class CommonDeclarationDetailsLayoutBuilder<TBizObject> : ColumnLayoutBuilder<TBizObject, CommonDeclarationDetailsControlBag>
		where TBizObject : BaseJobDeclaration
	{
		public override CommonDeclarationDetailsControlBag CommonBag => CommonDeclarationDetailsControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
