using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.GUI
{
	public class DV1DetailsLayoutBuilder<T> : EU.GUI.DV1DetailsLayoutBuilder<T> where T : JobDeclaration
	{
		protected override int MaxColumns => 2;
	}
}
