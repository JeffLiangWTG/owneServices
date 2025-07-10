using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public static class StmMenuItemExtensionMethods
	{
		public static DocumentDirection GetDocumentDirection(this IStmMenuItem menuItem)
		{
			switch (menuItem.SU_DocumentDirection)
			{
				case "DEP":
					return DocumentDirection.DEP;

				case "ARV":
					return DocumentDirection.ARV;

				default:
					return DocumentDirection.ANY;
			}
		}

		public static bool IsApplicable(this IStmMenuItem menuItem, IDocumentSupportable documentSupportable) => menuItem.IsApplicable(documentSupportable as BusinessObject);

		public static bool IsApplicable(this IStmMenuItem menuItem, BusinessObject bizObj)
		{
			IDocumentFilterEvaluator filterEvaluator;

			if (menuItem?.SU_MenuType.ToString() == Core.Constants.StmMenuItemTypes.Forms)
			{
				filterEvaluator = formFilterEvaluator ??= ObjectFactory.Get<IDocumentVisualizerFilterEvaluator>();
			}
			else
			{
				filterEvaluator = documentFilterEvaluator ??= ObjectFactory.Get<IDocumentFilterEvaluator>();
			}

			return filterEvaluator.IsApplicable(bizObj, menuItem?.SU_FilterList ?? ZString.Empty, menuItem as BusinessObject);
		}

		[ThreadStatic]
		static IDocumentFilterEvaluator documentFilterEvaluator;

		[ThreadStatic]
		static IDocumentFilterEvaluator formFilterEvaluator;
	}
}
