using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class MenuPivotHelper
	{
		public static bool CreateMenuAndPivotWithEmptyMenuPathIfNotInDatabase(BusinessObjectFactory newFactory, StmTemplate clientTemplate, BusinessContext copyFromContext, ZString copyFromMenuName, ZString newBusinessContext)
		{
			bool shouldSaveFactory = false;
			StmMenuItem menu = StmMenuItem.FindDocumentMenu(newFactory, clientTemplate, copyFromContext, ZString.Empty);

			if (menu == null)
			{
				menu = StmMenuItem.CreateDocumentMenu(newFactory, clientTemplate, copyFromContext, copyFromMenuName, newBusinessContext);
				shouldSaveFactory = true;
			}

			StmMenuTemplatePivot pivot = StmMenuTemplatePivot.FindDocumentPivot(newFactory, clientTemplate, menu);
			if (pivot == null)
			{
				pivot = StmMenuTemplatePivot.CreateDocumentPivot(newFactory, clientTemplate, menu, copyFromMenuName);
				shouldSaveFactory = true;
			}

			return shouldSaveFactory;
		}
	}
}
