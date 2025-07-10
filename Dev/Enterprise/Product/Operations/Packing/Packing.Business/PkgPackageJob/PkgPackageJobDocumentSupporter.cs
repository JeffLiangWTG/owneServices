using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Packing.Business
{
	public class PkgPackageJobDocumentSupporter : DocumentSupporter
	{
		public PkgPackageJobDocumentSupporter(PkgPackageJob packageJob)
			: base(packageJob)
		{
			var parentSupportable = packageJob.ParentJob as IDocumentSupportable;
			ParentSupporter = parentSupportable?.DocumentSupporter;
			ModuleSpecificParentSupporter = ParentSupporter as IPackingParentDocumentSupporter;
		}

		readonly DocumentSupporter ParentSupporter;
		readonly IPackingParentDocumentSupporter ModuleSpecificParentSupporter;

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Packing; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.PkgPackageJobCustomizeDocuments;

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			var dataContext = new List<Constants.DataContext>
			{
				Constants.DataContext.GenericFreightJob,
				Constants.DataContext.GenericBasicLabel,
				Constants.DataContext.GenericBasicLabelAll,
				Constants.DataContext.GenericProductLabel,
				Constants.DataContext.GenericProductLabelAll,
				Constants.DataContext.GenericDeliveryLabel,
				Constants.DataContext.GenericDeliveryLabelAll,
				Constants.DataContext.GenericProductDeliveryLabel,
				Constants.DataContext.GenericProductDeliveryLabelAll,
				Constants.DataContext.GenericRetailersLabel,
				Constants.DataContext.GenericRetailersLabelAll,
				Constants.DataContext.GenericDeliveryIDLabelAll
			};

			if (GlbStaff.CurrentUser.IsSupportUser)
			{
				dataContext.AddRange(new List<Constants.DataContext>
				{
					Constants.DataContext.GenericCarrierLabel,
					Constants.DataContext.GenericCarrierLabelAll
				});
			}

			dataContext.AddRange(GetModuleSpecificDataContexts());

			return dataContext.ToArray();
		}

		internal IEnumerable<Constants.DataContext> GetModuleSpecificDataContexts()
		{
			return ModuleSpecificParentSupporter != null ? ModuleSpecificParentSupporter.GetModuleSpecificPackageSupportedDataContexts() : Array.Empty<Constants.DataContext>();
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return ParentSupporter != null
				? ParentSupporter.GetContactOrganisation(menuName, contactType, direction)
				: base.GetContactOrganisation(menuName, contactType, direction);
		}

		#region ShowMenuNotFoundMessages

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var message = ModuleSpecificParentSupporter?.GetModuleSpecificNotFoundMessage(dataContextValue.DataContext) ?? ZString.Empty;
			if (message.IsEmpty)
			{
				message = base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
			}

			return message;
		}

		#endregion

		#region GetDocumentWrappersInternal

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result;
			switch (dataContext)
			{
				case Constants.DataContext.GenericFreightJob:
					result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);
					break;
				case Constants.DataContext.GenericBasicLabel:
				case Constants.DataContext.GenericBasicLabelAll:
				case Constants.DataContext.GenericDeliveryLabel:
				case Constants.DataContext.GenericDeliveryLabelAll:
				case Constants.DataContext.GenericProductDeliveryLabel:
				case Constants.DataContext.GenericProductDeliveryLabelAll:
				case Constants.DataContext.GenericRetailersLabel:
				case Constants.DataContext.GenericRetailersLabelAll:
					result = GetLabelWrappers(dataContext);
					break;
				case Constants.DataContext.GenericCarrierLabel:
				case Constants.DataContext.GenericCarrierLabelAll:

					var parentJob = PackageJob != null ? PackageJob.ParentJob : null;
					var packages = GetPackages(dataContext).Where(package => HasMatchingCarrierServiceLevel(parentJob, package)).ToArray();
					result = parentJob != null ? GetLabelWrappers(packages) : Array.Empty<DocumentWrapper>();
					break;
				case Constants.DataContext.GenericProductLabel:
				case Constants.DataContext.GenericProductLabelAll:
					result = GetProductLabelWrappers(dataContext);
					break;
				case Constants.DataContext.GenericDeliveryIDLabelAll:
					result = GetPackageHeaderWrappers();
					break;
				default:
					result = ModuleSpecificParentSupporter != null ? GetLabelWrappers(dataContext) : Array.Empty<DocumentWrapper>();
					break;
			}

			return result;
		}

		bool HasMatchingCarrierServiceLevel(IPackingParent parentJob, PkgPackage package)
		{
			var carrier = parentJob.GetCarrier(package);
			return carrier != null && carrier.MiscServ.CarrierServiceLevels.Cast<OrgCarrierServiceLevel>().Any(s => s.PL_Code.EqualsIgnoringCase(parentJob.CarrierServiceLevelCode(package)));
		}

		public DocumentWrapper[] GetLabelWrappersForWebService(Constants.DataContext dataContext, PkgPackage packageToPrintLabel)
		{
			DocumentWrapper[] result = null;

			var packagesToAdd = new List<PkgPackage>();
			packagesToAdd.Add(packageToPrintLabel);
			PackageJob.Selected.UpdateSelectedPackages(packagesToAdd);

			var packages = GetPackages(dataContext, isPackageSelected: true);
			result = GetLabelWrappers(packages);
			return result;
		}

		public DocumentWrapper[] GetLabelWrappers(Constants.DataContext dataContext, bool useSelected = false)
		{
			var isPackageSelected = PackageJob.Selected.IsPackageSelected && useSelected;

			var packages = GetPackages(dataContext, isPackageSelected);
			var filteredPackages = ModuleSpecificParentSupporter != null ? packages.Where(p => ModuleSpecificParentSupporter.IsPrintablePackageForSpecificModuleDataContext(dataContext, p)).ToArray() : packages;
			return GetLabelWrappers(filteredPackages);
		}

		DocumentWrapper[] GetLabelWrappers(PkgPackage[] packages)
		{
			var result = new List<DocumentWrapper>();
			var totalOuterPackageCount = 0;
			var uomTypeCache = new Dictionary<ZString, string>();
			var uomTypeTotals = new Dictionary<string, int>();
			var uomTypeNumbers = new Dictionary<PkgPackage, int>();

			foreach (var package in packages)
			{
				if (package.IsOuter)
				{
					totalOuterPackageCount++;
				}

				if (!uomTypeCache.TryGetValue(package.KP_F3_NKPackType, out var uomType))
				{
					uomTypeCache[package.KP_F3_NKPackType] = uomType = package.PackType?.F3_UOMType ?? ZString.Empty;
				}

				if (!string.IsNullOrEmpty(uomType))
				{
					if (uomTypeTotals.TryGetValue(uomType, out var total))
					{
						uomTypeTotals[uomType] = total + 1;
					}
					else
					{
						uomTypeTotals.Add(uomType, 1);
					}

					uomTypeNumbers[package] = uomTypeTotals[uomType];
				}
			}

			var outerIndex = 0;
			foreach (var package in packages)
			{
				var uomTypeNumber = 0;
				var uomTypeTotal = 0;
				var uomType = uomTypeCache[package.KP_F3_NKPackType];
				if (!string.IsNullOrEmpty(uomType))
				{
					uomTypeNumber = uomTypeNumbers[package];
					uomTypeTotal = uomTypeTotals[uomType];
				}

				if (package.IsOuter)
				{
					outerIndex++;
					result.AddRange(GetPackageWrapper(package, packedItems: null, documentNumber: outerIndex, documentTotal: totalOuterPackageCount, uomTypeNumber: uomTypeNumber, uomTypeTotal: uomTypeTotal));
				}
				else
				{
					result.AddRange(GetPackageWrapper(package, uomTypeNumber: uomTypeNumber, uomTypeTotal: uomTypeTotal));
				}
			}

			return result.ToArray();
		}

		PkgPackage[] GetPackages(Constants.DataContext dataContext, bool isPackageSelected = false)
		{
			var packSelection = isPackageSelected ? PackSelection.Selected : PackSelection.All;
			var considerAllLevels = ModuleSpecificParentSupporter != null && ModuleSpecificParentSupporter.IsAllPackLevelsEnabled(dataContext);

			PackLevel packLevel;
			if (considerAllLevels ||
				dataContext == Constants.DataContext.GenericBasicLabelAll || dataContext == Constants.DataContext.GenericDeliveryLabelAll ||
				dataContext == Constants.DataContext.GenericProductDeliveryLabelAll || dataContext == Constants.DataContext.GenericRetailersLabelAll ||
				dataContext == Constants.DataContext.GenericCarrierLabelAll
				)
			{
				packLevel = PackLevel.All;
			}
			else
			{
				packLevel =
					isPackageSelected || dataContext != Constants.DataContext.GenericBasicLabel
					? PackLevel.First
					: PackLevel.Second;
			}

			return GetPackages(packLevel, packSelection);
		}

		DocumentWrapper[] GetPackageHeaderWrappers()
		{
			var wrappers = new List<DocumentWrapper>();
			PackageJob.LoosePackageIDs.ToList().ForEach(p => p.CurrentPackageJob = PackageJob); // normally UI does this for single clicked Package Headers
			wrappers.AddRange(PackageJob.LoosePackageIDs.SelectMany(h => GetPackageHeaderWrapper(h)));
			wrappers.AddRange(GetLabelWrappers(PackageJob.GetAllNonContainerOutersAndFirstLevelPackagesOnContainers().Where(p => p.PackageID != null).ToArray()));
			return wrappers.ToArray();
		}

		public DocumentWrapper[] GetProductLabelWrappers(Constants.DataContext dataContext, bool useSelected = false)
		{
			var isPackageSelected = PackageJob.Selected.IsPackageSelected && useSelected;
			var packSelection = isPackageSelected ? PackSelection.Selected : PackSelection.All;
			var packLevel = (dataContext == Constants.DataContext.GenericProductLabelAll) ? PackLevel.All : PackLevel.First;

			var result = new List<DocumentWrapper>();
			Array.ForEach(GetPackages(packLevel, packSelection, true), p => result.AddRange(GetPackageWrapper(p, 2)));
			return result.ToArray();
		}

		DocumentWrapper[] GetPackageWrapper(PkgPackage package, PkgPackageItemDivotsWrapper[] packedItems = null, int documentNumber = 0, int documentTotal = 0, int uomTypeNumber = 0, int uomTypeTotal = 0)
		{
			return GetPackageOrHeaderWrapper((i) => i.SetPackageOverride(package, packedItems, documentNumber, documentTotal, uomTypeNumber, uomTypeTotal));
		}

		DocumentWrapper[] GetPackageHeaderWrapper(PkgPackageHeader packageHeader)
		{
			return GetPackageOrHeaderWrapper((i) => i.SetPackageCollectionOverride(packageHeaders: new[] { packageHeader }));
		}

		DocumentWrapper[] GetPackageOrHeaderWrapper(Action<IPackageOverrider> setPackageOverride)
		{
			DocumentWrapper[] result;
			var wrappers = DocumentWrapperFactory.GenerateGenericWrappers(Constants.DataContext.GenericFreightJob, PackageJob);
			if (wrappers.Length > 0)
			{
				var wrapper = wrappers[0];
				setPackageOverride((IPackageOverrider)wrapper);
				result = new[] { wrapper };
			}
			else
			{
				result = Array.Empty<DocumentWrapper>();
			}
			return result;
		}

		DocumentWrapper[] GetPackageWrapper(PkgPackage package, int packableItemParentsPerLabel)
		{
			DocumentWrapper[] result;

			if (packableItemParentsPerLabel >= 0)
			{
				var packageWrappers = new List<DocumentWrapper>();
				var packedItemsGrouped = package.PackedItems.Typed.GroupBy(d => d.Key).ToArray();

				for (int i = 0; i < packedItemsGrouped.Length; i += packableItemParentsPerLabel)
				{
					var packedItems = new List<PkgPackageItemDivotsWrapper>();
					for (int p = 0; p < packableItemParentsPerLabel && i + p < packedItemsGrouped.Length; p++)
					{
						packedItems.AddRange(packedItemsGrouped[i + p]);
					}

					packageWrappers.AddRange(GetPackageWrapper(package, packedItems.ToArray()));
				}

				result = packageWrappers.ToArray();
			}
			else
			{
				result = GetPackageWrapper(package);
			}

			return result;
		}

		public enum PackSelection
		{
			Selected,
			All,
		}

		public enum PackLevel
		{
			All = -1,
			First = 0,
			Second = 1,
		}

		PkgPackage[] GetPackages(PackLevel levelWanted, PackSelection packSelection, bool requirePackedItems = false)
		{
			var packages = (packSelection == PackSelection.Selected) ? (IEnumerable<PkgPackage>)PackageJob.Selected.SelectedPackages : PackageJob.Packages;
			return GetPackages(packages, levelWanted, 0, requirePackedItems);
		}

		PkgPackage[] GetPackages(IEnumerable<PkgPackage> packages, PackLevel levelWanted, PackLevel currentLevel, bool requirePackedItems = false)
		{
			var result = new List<PkgPackage>();

			foreach (var package in packages)
			{
				if (!package.KP_PackageID.IsEmpty && (!requirePackedItems || package.PackedItemDivots.Count > 0))
				{
					result.Add(package);
				}

				if (currentLevel != levelWanted)
				{
					result.AddRange(GetPackages(package.Packages, levelWanted, currentLevel + 1, requirePackedItems));
				}
			}

			return result.ToArray();
		}

		#endregion

		#region PackageJob

		PkgPackageJob PackageJob
		{
			get { return (PkgPackageJob)BusinessObject; }
		}

		#endregion
	}

	#region IPackageOverrider

	public interface IPackageOverrider
	{
		void SetPackageOverride(PkgPackage package, PkgPackageItemDivotsWrapper[] packedItems = null, int documentNumber = 0, int documentTotal = 0, int uomTypeNumber = 0, int uomTypeTotal = 0);
		void SetPackageCollectionOverride(PkgPackage[] packages = null, PkgPackageHeader[] packageHeaders = null);
		ZInt DocumentNumber { get; }
		ZInt DocumentTotal { get; }
	}

	#endregion
}
