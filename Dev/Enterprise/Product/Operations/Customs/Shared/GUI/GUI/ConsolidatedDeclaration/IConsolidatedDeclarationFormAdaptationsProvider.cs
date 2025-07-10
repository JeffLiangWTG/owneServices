using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public interface IConsolidatedDeclarationFormAdaptationsProvider
	{
		IPanelLayoutProvider HeaderDetailsLayout { get; }
		IEnumerable<ZGridColumnInfo> DeclarationGridExtraColumnInfos { get; }
		ZUserControl MessagesTabUserControl { get; }
		IConsolidatedDeclarationMenuBuilder EDIMenuBuilder { get; }
		bool EnableDocumentMenuItem { get; }
	}

	public interface IConsolidatedDeclarationMenuBuilder
	{
		ConsolidatedDeclaration ConsolidatedDeclaration { get; set; }

		ZMenuItem BuildMenu();
	}
}
