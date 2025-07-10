using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;

namespace Enterprise.Customs.Business
{
	public class CusPackingListDeepCloneStrategy : CustomsBusinessObjectCloneStrategy
	{
		public CusPackingListDeepCloneStrategy(CusPackingList cusPackingListToClone, CloneType cloneType, BaseJobDeclaration clonedDeclaration, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
		: base(cusPackingListToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
			this.clonedParent = clonedDeclaration;
		}

		public CusPackingListDeepCloneStrategy(CusPackingList cusPackingListToClone, CloneType cloneType, BaseJobComInvoiceHeader clonedInvoice, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
			: base(cusPackingListToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
			this.clonedParent = clonedInvoice;
		}

		readonly BusinessObject clonedParent;

		protected CusPackingList PackingListToClone => (CusPackingList)bizObjToClone;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (CusPackingList)base.CloneInternal(args);
			using (result.GetValidationSuspender())
			using (result.SuspendSettingHasChanges())
			{
				if (clonedParent is BaseJobDeclaration)
				{
					result.CUL_JE = clonedParent.PK;
				}
				else if (clonedParent is BaseJobComInvoiceHeader)
				{
					result.CUL_JZ = clonedParent.PK;
				}

				var packagesToClone = PackingListToClone.PackageJob.Packages.OrderBy(x => x.KP_Sequence);
				var clonedPackageJob = result.PackageJob;
				using (clonedPackageJob.GetValidationSuspender())
				using (clonedPackageJob.SuspendSettingHasChanges())
				{
					foreach (CusPackage packageToClone in packagesToClone)
					{
						var cusPackageCloneStrategy = new CustomsBusinessObjectCloneStrategy(packageToClone, cloneType, alternativeFactoryToInstantiateCloneIn);
						var clonedPackage = (CusPackage)cusPackageCloneStrategy.Clone();
						using (clonedPackage.GetValidationSuspender())
						using (clonedPackage.SuspendSettingHasChanges())
						{
							clonedPackage.KP_KJ_ParentPackageJob = clonedPackageJob.PK;
							clonedPackageJob.Packages.Add(clonedPackage);
						}
					}
				}

				foreach (var packageItemToClone in PackingListToClone.PackableItems.OrderBy(x => x.CUI_Sequence))
				{
					var packageItemCloneStrategy = new CustomsBusinessObjectCloneStrategy(packageItemToClone, cloneType, alternativeFactoryToInstantiateCloneIn);
					var clonedPackageItem = (CusPackableItem)packageItemCloneStrategy.Clone();
					using (clonedPackageItem.GetValidationSuspender())
					using (clonedPackageItem.SuspendSettingHasChanges())
					{
						clonedPackageItem.CUI_CUL = result.PK;
						if (clonedParent is BaseJobDeclaration declaration)
						{
							clonedPackageItem.CUI_JI = declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>().FirstOrDefault(line => line.JI_Calc_Invoice == packageItemToClone.CUI_InvoiceNumber && line.JI_LineNo == packageItemToClone.CUI_InvoiceLineNumber)?.PK ?? ZGuid.Empty;
						}
						else if (clonedParent is BaseJobComInvoiceHeader invoiceHeader)
						{
							clonedPackageItem.CUI_JI = invoiceHeader.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().FirstOrDefault(line => line.JI_LineNo == packageItemToClone.CUI_InvoiceLineNumber)?.PK ?? ZGuid.Empty;
						}
						clonedPackageItem.Grouping = packageItemToClone.Grouping;

						foreach (CusPackage packageToClone in packagesToClone)
						{
							if (packageToClone.GetDivot(packageItemToClone) is PkgPackageItemDivot pkgPackageItemDivotToClone)
							{
								if (clonedPackageJob.Packages.FirstOrDefault(p => p.KP_Sequence == packageToClone.KP_Sequence) is CusPackage clonedPackage)
								{
									var clonedPackedItemDivot = clonedPackage.PackedItemDivots.AddNew();
									clonedPackedItemDivot.HasChanges = false;
									using (clonedPackedItemDivot.GetValidationSuspender())
									using (clonedPackedItemDivot.SuspendSettingHasChanges())
									{
										clonedPackedItemDivot.KI_KP_Package = clonedPackage.PK;
										clonedPackedItemDivot.KI_ParentID = clonedPackageItem.PK;
										clonedPackedItemDivot.KI_ParentTableCode = clonedPackageItem.PKSchemaColumn.ColumnPrefix;
										clonedPackedItemDivot.KI_PackedQty = pkgPackageItemDivotToClone.KI_PackedQty;
										clonedPackedItemDivot.PkgNetWeight = pkgPackageItemDivotToClone.PkgNetWeight;
										clonedPackedItemDivot.PkgNetWeightUQ = pkgPackageItemDivotToClone.PkgNetWeightUQ;
									}
								}
							}
						}
					}
				}
			}
			return result;
		}
	}
}
