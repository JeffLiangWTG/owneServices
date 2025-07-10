using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public static class InvoiceLineProductValidationHelper
	{
		public static class Warnings
		{
			public static string PartCannotBeFoundBeforeEnteringASupplierAndImporter
			{
				get { return Res.GetString("17599530-b7c4-4ba0-8237-2730b4150d3e", "You must enter both a supplier and importer before the Classification details can be copied in from a Part."); }
			}

			public static string PartCodeNotFoundAtAll
			{
				get { return Res.GetString("163432a3-415f-4284-8375-df8119180fdf", "No Product(s) found with this code. Either add a new Product (F3) or select a valid Product (F4)."); }
			}

			public static string PartHasMultipleUNDGRecords
			{
				get { return Res.GetString("E32ECE2C-963F-475C-8A4E-1CC046E390E9", "Cannot default Dangerous Goods details as this Product has more than one Dangerous Goods record."); }
			}

			public static string MoreThanOneProductMatchFound
			{
				get
				{
					return Res.GetString("846ad1ed-6915-47a8-a353-28eed58ae2e4", @"The system has found more than one potential match for the Product Code entered with the same score, and has randomly picked one of the matched products.
	Please make sure that the right Product has been picked (F4).");
				}
			}
		}

		public static void ValidateProductCodeWhenPartSyncManagerEnabled(ZPropertyInfo productCodeInfo,
			OrgSupplierPart part,
			OrgHeader effectiveSupplier,
			OrgHeader effectiveImporter,
			JobComInvoiceLinePartSynchronisationManager partSyncManager,
			string productCodeFoundButNotRelatedWarning,
			string severalProductCodeFoundButNotRelaredWarning,
			bool isUNDGSupported = false)
		{
			if (part == null)
			{
				if (effectiveSupplier == null && effectiveImporter == null)
				{
					productCodeInfo.AddWarning(Warnings.PartCannotBeFoundBeforeEnteringASupplierAndImporter);
				}
				else if (partSyncManager?.TotalNumberOfPartsCount == 1)
				{
					productCodeInfo.AddWarning(productCodeFoundButNotRelatedWarning);
				}
				else if (partSyncManager?.TotalNumberOfPartsCount > 1)
				{
					productCodeInfo.AddWarning(severalProductCodeFoundButNotRelaredWarning);
				}
				else
				{
					productCodeInfo.AddWarning(Warnings.PartCodeNotFoundAtAll);
				}
			}
			else
			{
				if (partSyncManager?.TotalMatchCount > 1)
				{
					productCodeInfo.AddWarning(Warnings.MoreThanOneProductMatchFound);
				}

				if (isUNDGSupported && part.UNDGs.Count > 1)
				{
					productCodeInfo.AddWarning(Warnings.PartHasMultipleUNDGRecords);
				}
			}
		}
	}
}
