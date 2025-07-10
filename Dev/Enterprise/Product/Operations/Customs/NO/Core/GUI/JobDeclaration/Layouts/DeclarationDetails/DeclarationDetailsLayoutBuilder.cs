using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;

namespace Enterprise.Customs.NO.GUI;

sealed class DeclarationDetailsLayoutBuilder : CommonDeclarationDetailsLayoutBuilder<JobDeclaration>
{
	protected override int MaxColumns => 2;
}
