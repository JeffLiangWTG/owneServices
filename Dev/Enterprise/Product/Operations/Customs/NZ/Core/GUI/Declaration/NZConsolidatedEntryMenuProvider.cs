using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	internal class NZConsolidatedEntryMenuProvider : ConsolidatedEntryMenuProvider
	{
		public NZConsolidatedEntryMenuProvider(ZForm parentForm)
			: base(parentForm)
		{
		}

		protected override bool MenuItemsVisible
		{
			get
			{
				var result = false;

				if (declaration is JobDeclaration nzDeclaration)
				{
					result = base.MenuItemsVisible && nzDeclaration.IsImport && (nzDeclaration.IsPeriodic || nzDeclaration.IsNormal);
				}

				return result;
			}
		}

		protected override void SetExternalDefaults(ZFilterGridModule filterGridModule)
		{
			filterGridModule.FilterBusinessObject.SetExternalDefaults(DefaultConsolidatedDeclarationFilter.GetDefaultConsolidatedDeclarationFilter(declaration as JobDeclaration));
		}
	}
}
