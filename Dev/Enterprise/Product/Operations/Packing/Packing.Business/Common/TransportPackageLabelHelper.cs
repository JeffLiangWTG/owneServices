using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public static class TransportPackageLabelHelper
	{
		public static DocumentWrapper[] CreateDocumentWrapperForNewPackageIDs(IPackingParent parent, int specificQty, bool returnSingleDocument)
		{
			return GetDocumentWrappersForNewPackageIDsCore(parent, specificQty, returnSingleDocument: returnSingleDocument);
		}

		public static DocumentWrapper[] CreateDocumentWrapperForNewPackageIDs(IPackingParent parent, bool returnSingleDocument)
		{
			return GetDocumentWrappersForNewPackageIDsCore(parent, 0, returnSingleDocument: returnSingleDocument);
		}

		public static DocumentWrapper[] CreateDocumentWrapperForNewPackageID(IPackingParent parent)
		{
			return GetDocumentWrappersForNewPackageIDsCore(parent, 1, true);
		}

		public static DocumentWrapper[] CreateDocumentWrapperForNewPackageID(PkgHandlingUnit handlingUnit)
		{
			return GetDocumentWrappersForNewPackageIDsCore(handlingUnit, 1, linkLooseID: handlingUnit.KPU_JobContext != "3PL", returnSingleDocument: true);
		}

		public static DocumentWrapper[] CreateDocumentWrapperForNewAndExistingPackageIDs(IPackingParent parent, IEnumerable<PkgPackage> packagesWithIDs, IEnumerable<PkgPackage> packagesWithoutIDs = null, bool returnSingleDocument = true)
		{
			var packageHeadersWithNewID = GetPackageHeadersWithNewPackageID(parent, 0, packagesWithoutIDs: packagesWithoutIDs);
			packagesWithIDs = packagesWithIDs.Where(p => !p.KP_PackageID.IsEmpty);
			var document = CreateDocumentWrappers(parent, returnSingleDocument, packagesWithIDs, packageHeadersWithNewID);

			return document.OrderBy(wrapper => ((IPackageOverrider)wrapper).DocumentNumber).ToArray();
		}

		static DocumentWrapper[] GetDocumentWrappersForNewPackageIDsCore(IPackingParent parent, int specificQty, bool ordered = true, bool linkLooseID = true, IEnumerable<PkgPackage> packagesWithoutIDs = null, bool returnSingleDocument = true)
		{
			var newHeaders = GetPackageHeadersWithNewPackageID(parent, specificQty, linkLooseID, packagesWithoutIDs);
			var documents = CreateDocumentWrappers(parent, returnSingleDocument, packageHeaders: newHeaders);
			return ordered ? documents.OrderBy(wrapper => ((IPackageOverrider)wrapper).DocumentNumber).ToArray() : documents.ToArray();
		}

		static DocumentWrapper[] CreateDocumentWrappers(IPackingParent parent, bool returnSingleDocument, IEnumerable<PkgPackage> packages = null, IEnumerable<PkgPackageHeader> packageHeaders = null)
		{
			var documents = new List<DocumentWrapper>();
			var hasPackage = false;
			var hasPackageHeader = false;

			if (packages != null && packages.Any())
			{
				packages = packages.OrderBy(p => p.KP_Sequence);
				hasPackage = true;
			}

			if (packageHeaders != null && packageHeaders.Any())
			{
				var query = new ZQuery(PkgPackageJobPackageHeaderPivotSchema.KPJ_KPH_PackageHeader,SQLComparisonOperator.Equal, packageHeaders.Select(header => header.PK));
				var pivots = parent.Factory.Load<PkgPackageJobPackageHeaderPivot>(query);
				packageHeaders = pivots.OrderBy(p => p.KPJ_Sequence)
					.Select(p => packageHeaders.First(header => header.PK == p.KPJ_KPH_PackageHeader))
					.ToArray();
				hasPackageHeader = true;
			}

			if (hasPackage || hasPackageHeader)
			{
				if (returnSingleDocument)
				{
					documents.Add(CreateDocumentWrapperForParent(parent, packages, packageHeaders));
				}
				else
				{
					if (hasPackage)
					{
						packages.ForEach(package =>
						{
							documents.Add(CreateDocumentWrapperForParent(parent, new[] { package }, null));
						});
					}

					if (hasPackageHeader)
					{
						packageHeaders.ForEach(packageHeader =>
						{
							documents.Add(CreateDocumentWrapperForParent(parent, null, new[] { packageHeader }));
						});
					}
				}
			}
			return documents.ToArray();
		}

		static DocumentWrapper CreateDocumentWrapperForParent(IPackingParent packingParent, IEnumerable<PkgPackage> packages, IEnumerable<PkgPackageHeader> packageHeaders)
		{
			var documentWrapper = DocumentWrapperFactory.GenerateGenericWrappers(Constants.DataContext.GenericFreightJob, (BusinessObject)packingParent).FirstOrDefault();
			var iParentWrapper = (IPackageOverrider)documentWrapper;
			iParentWrapper.SetPackageCollectionOverride(packages?.ToArray(), packageHeaders?.ToArray());
			return documentWrapper;
		}

		static List<PkgPackageHeader> GetPackageHeadersWithNewPackageID(IPackingParent parent, int specificQty, bool linkLooseID = true, IEnumerable<PkgPackage> packagesWithoutIDs = null)
		{
			var result = new List<PkgPackageHeader>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parent);
			var factory = packageJob.Factory;
			factory.Save();

			var qty = 0;

			if (specificQty > 0)
			{
				qty = specificQty;
			}
			else if (specificQty == 0)
			{
				var newPackages = packagesWithoutIDs ?? packageJob.GetAllNonContainerOutersAndFirstLevelPackagesOnContainers();
				var packageCount = newPackages.Where(p => p.KP_PackageID.IsEmpty).Sum(l => l.KP_PackageQty);
				var looseIdCount = packageJob.LoosePackagePivots.Count;
				qty = packageCount > looseIdCount ? packageCount - looseIdCount : 0;

				foreach (var header in packageJob.LoosePackageIDs)
				{
					header.CurrentPackageJob = packageJob;
					result.Add(header);
				}
			}

			if (qty > 0)
			{
				var newHeaders = new List<PkgPackageHeader>();
				for (var i = 1; i <= qty; i++)
				{
					var newHeader = factory.New<PkgPackageHeader>();
					newHeader.CurrentPackageJob = packageJob;
					newHeaders.Add(newHeader);
				}

				newHeaders.ForEach(header =>
				{
					((ISupportPackageIDGeneration)header).ShouldGenerateIDOnSaving = true;
					if (linkLooseID)
					{
						packageJob.LoosePackageIDs.Add(header);
					}
					else
					{
						packageJob.Packages.Single().KP_KPH_PackageHeader = header.PK;
					}
					result.Add(header);
				});

				packageJob.Factory.Save();
			}

			return result;
		}
	}
}
