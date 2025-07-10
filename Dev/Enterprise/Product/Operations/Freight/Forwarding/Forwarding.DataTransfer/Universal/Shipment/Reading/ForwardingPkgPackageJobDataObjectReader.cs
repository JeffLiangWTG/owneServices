using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingPkgPackageJobDataObjectReader : DataObjectReader<IDataObject, ForwardingPackageJob>
	{
		public ForwardingPkgPackageJobDataObjectReader(IPackageParentDataObject parentData, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment, bool isComplete, OrgAddress transitWarehouseAddress, Func<PackingLine, IEnumerable<PackingLine>> getValidImportedInnerPackages)
			: base(new PackageParentDataObject(parentData), logger, factory)
		{
			ParentBO = Argument.NotNull(shipment, nameof(shipment));
			TransitWarehouseAddress = Argument.NotNull(transitWarehouseAddress, nameof(transitWarehouseAddress));
			GetValidImportedInnerPackages = Argument.NotNull(getValidImportedInnerPackages, nameof(getValidImportedInnerPackages));
			IsComplete = isComplete;
		}

		readonly bool IsComplete;
		readonly ForwardingShipment ParentBO;
		readonly OrgAddress TransitWarehouseAddress;
		readonly Func<PackingLine, IEnumerable<PackingLine>> GetValidImportedInnerPackages;

		#region DataObject

		protected new IPackageParentDataObject dataObject => ((PackageParentDataObject)base.dataObject).ParentData;

		class PackageParentDataObject : IDataObject
		{
			public PackageParentDataObject(IPackageParentDataObject parentData)
			{
				ParentData = parentData;
			}

			public readonly IPackageParentDataObject ParentData;
		}

		#endregion

		#region GetExistingBusinessObject

		protected override ForwardingPackageJob GetExistingBusinessObject()
		{
			var result = ParentBO.Factory.LoadTop1<ForwardingPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, ParentBO.PK)); // there should only ever be one.

			if (result == null)
			{
				result = ParentBO.Factory.New<ForwardingPackageJob>();
				result.KJ_JobID = ParentBO.JobNumber;
				result.KJ_ParentID = ParentBO.PK;
				result.KJ_ParentTableCode = ParentBO.TablePrefix;
			}

			return result;
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(ForwardingPackageJob targetBO)
		{
			PkgPackageJobDataObjectReader.ThrowImportFailureBeforeImportChecks(targetBO, dataObject.PackingLineCollection);
			ThrowImportFailureIfThereIsNoPacklineIdAndNoReferenceNumberForOnePackageInDataObject(dataObject.PackingLineCollection);
			ThrowImportFailureIfThereAreTwoPackagesWithNoReferenceNumberButSameRefNumberInDataObject(dataObject.PackingLineCollection);

			PopulatePackages(targetBO, dataObject);

			PkgPackageJobDataObjectReader.ThrowImportFailureIfDuplicatePackageIDsInBusinessObject(targetBO);
		}

		#endregion

		#region Validations

		public static void ThrowImportFailureIfThereIsNoPacklineIdAndNoReferenceNumberForOnePackageInDataObject(DataObjectList<PackingLine> packingLineCollection)
		{
			var invalidPackages = packingLineCollection.Where(p => p.ReferenceNumber.GetValueOrDefault().IsEmpty && p.PackingLineID.GetValueOrDefault().IsEmpty);
			if (invalidPackages.Any())
			{
				var message = Res.GetString("A4AD6C2D-0BD8-424A-A459-9A4CF92C9537", "There are packages lacking both packline Id and reference number");
				throw new DataObjectReadFailureException(message);
			}
		}

		public static void ThrowImportFailureIfThereAreTwoPackagesWithNoReferenceNumberButSameRefNumberInDataObject(DataObjectList<PackingLine> packingLineCollection)
		{
			var invalidPackagesExist = packingLineCollection.Where(p => p.ReferenceNumber.GetValueOrDefault().IsEmpty).GroupBy(p => p.PackingLineID).Any(group => group.Count() > 1);
			if (invalidPackagesExist)
			{
				var message = Res.GetString("D43182E3-4D83-4A24-A876-4F74420AD333", "Packline id is not unique among packages with no reference number");
				throw new DataObjectReadFailureException(message);
			}
		}

		#endregion

		#region PopulatePackages

		void PopulatePackages(ForwardingPackageJob packageJobBO, IPackageParentDataObject packageJobParentData)
		{
			var packagesToPopulate = packageJobParentData.PackingLineCollection;
			if ((packagesToPopulate == null) || (packagesToPopulate.Count == 0 && !IsComplete))
			{
				return;
			}

			var existedPackagesInTransitWarehouse = GetExistedPackages(packageJobBO, packagesToPopulate);
			var processedPackages = PopulatePackingLines(packagesToPopulate, isOuterPackingLine: true, packageJobBO);
			var allInnerPackages = processedPackages.SelectMany(PkgPackageHandlingUnitDivotHelper.GetAllInnerPackagesViaDivots).Cast<ForwardingPackage>();
			RemovePackagesFromPackLines(allInnerPackages);

			if (IsComplete)
			{
				var deletedPackages = existedPackagesInTransitWarehouse.Where(p => !processedPackages.Contains(p));
				RemovePackagesFromPackLines(deletedPackages);
				foreach (var pkg in deletedPackages)
				{
					pkg.Delete();
				}
			}
		}

		List<ForwardingPackage> GetExistedPackages(ForwardingPackageJob packageJobBO, DataObjectList<PackingLine> packagesToPopulate)
		{
			var outerPackLines = ParentBO.OuterPackLines.Cast<ForwardingPackLine>().ToList();
			var outerPackLinesInTransitWarehouse = outerPackLines.Where(p => p.JL_OA_LastKnownTransitWarehouseAddress == TransitWarehouseAddress.PK);
			var outerPackageIDs = outerPackLinesInTransitWarehouse
				.SelectMany(p => p.PkgPackageCollection)
				.Select(pkg => pkg.PackageIDWithFallbackToExternalReference)
				.Where(packageID => !packageID.IsEmpty)
				.ToHashSet();

			var packagesWithPreviousPacklineID = packagesToPopulate.Where(package => !package.PreviousPackingLineID.GetValueOrDefault().IsEmpty);
			var previousPackingLineIDsInXml = packagesWithPreviousPacklineID
				.Where(package => package.ReferenceNumber.GetValueOrDefault().IsEmpty)
				.Select(package => package.PreviousPackingLineID.GetValueOrDefault())
				.ToHashSet();
			outerPackageIDs.UnionWith(MatchingPackageIDsWithPreviousPackingLineID(outerPackLines, previousPackingLineIDsInXml));

			var outerPackLinesInOriginTransitWarehouse = outerPackLines
				.Where(p => !p.JL_OA_LastKnownTransitWarehouseAddress.IsEmpty
					&& p.JL_OA_LastKnownTransitWarehouseAddress != TransitWarehouseAddress.PK
					&& p.JL_LastKnownTransitWarehouseStatus == FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched);
			if (outerPackLinesInOriginTransitWarehouse.Any())
			{
				var dispatchedPackinglineIDsInXml = packagesWithPreviousPacklineID
					.Where(package => !package.ReferenceNumber.GetValueOrDefault().IsEmpty)
					.Select(package => package.PreviousPackingLineID.GetValueOrDefault())
					.ToHashSet();
				outerPackageIDs.UnionWith(MatchingPackageIDsWithPreviousPackingLineID(outerPackLinesInOriginTransitWarehouse, dispatchedPackinglineIDsInXml));
			}

			var existedPackages = packageJobBO.GetAllPackagesOnJob().Cast<ForwardingPackage>();
			return existedPackages.Where(pkg => outerPackageIDs.Contains(PkgPackageHandlingUnitDivotHelper.GetTopLevelPackageViaDivots(pkg).PackageIDWithFallbackToExternalReference)).ToList();
		}

		IEnumerable<ZString> MatchingPackageIDsWithPreviousPackingLineID(IEnumerable<ForwardingPackLine> forwardingPackLines, HashSet<ZString> previousPackingLineIDs)
		{
			return forwardingPackLines
				.SelectMany(p => p.PkgPackageCollection)
				.Where(package => package.KP_PackageID.IsEmpty && (previousPackingLineIDs.Contains(package.KP_ExternalReference) || previousPackingLineIDs.Contains(package.KP_PreviousPackLineID)))
				.Select(package => package.PackageIDWithFallbackToExternalReference)
				.Where(packageID => !packageID.IsEmpty);
		}

		void RemovePackagesFromPackLines(IEnumerable<ForwardingPackage> packages)
		{
			if (packages == null || !packages.Any())
			{
				return;
			}

			var packLinePackages = packages.First().Factory.Load<JobPackLinePackage>(new ZQuery(JobPackLinePackageSchema.JPP_KP_Packge, packages.Select(pkg => pkg.PK)));
			foreach (var packLinePackage in packLinePackages)
			{
				if (!PackagePreviousPackLinesDictionary.TryGetValue(packLinePackage.JPP_KP_Packge, out var previousPackLines))
				{
					previousPackLines = new List<ZString>();
					PackagePreviousPackLinesDictionary.Add(packLinePackage.JPP_KP_Packge, previousPackLines);
				}
				previousPackLines.Add((packLinePackage.PackLine?.JL_PackLineId).GetValueOrDefault());
				packLinePackage.Delete();
			}
		}

		public IDictionary<ZGuid, IList<ZString>> PackagePreviousPackLinesDictionary { get; } = new Dictionary<ZGuid, IList<ZString>>();

		HashSet<ForwardingPackage> PopulatePackingLines(IEnumerable<PackingLine> packagesToPopulate, bool isOuterPackingLine, ForwardingPackageJob packageJobBO)
		{
			var processedPackages = new HashSet<ForwardingPackage>();
			foreach (var packageData in packagesToPopulate)
			{
				var packageReader = new ForwardingPkgPackageDataObjectReader(packageData, logger, factory, packageJobBO, packageJobBO.Packages, ImportOption.MatchAndRelabelOnPreviousPackageID | ImportOption.MatchOnPackingLineID, null, isOuterPackingLine, packingLineCollection: packagesToPopulate, shouldPopulateChildPackages: false);
				var package = ProcessPackage(packageReader, processedPackages);

				package.SetScreeningMethod(packageData.ScreeningMethod.GetValueOrDefault());
				package.SetIsHighRisk(packageData.IsHighRisk.GetValueOrDefault());
				package.SetAdditionalScreeningMethod((packageData.AviationSecurityAdditionalInspectionType?.Code).GetValueOrDefault());

				if (isOuterPackingLine && packageData.HasInnerPackingLines())
				{
					var innerPackages = PopulatePackingLines(GetValidImportedInnerPackages(packageData), isOuterPackingLine: false, packageJobBO);
					processedPackages.UnionWith(innerPackages);

					var processedPackageHandlingUnitDivots = new HashSet<PkgPackageHandlingUnitDivot>();
					foreach (var innerPackage in innerPackages)
					{
						innerPackage.KP_KP_TopHandlingUnitPackage = package.PK;

						var divot = package.PackageHandlingUnitHandlingUnitDivots.FirstOrDefault(d => d.KPD_KP_Package == innerPackage.PK);
						if (divot == null)
						{
							divot = package.PackageHandlingUnitHandlingUnitDivots.AddNew();
							divot.KPD_KP_Package = innerPackage.PK;
						}
						divot.KPD_PackedTime = ZDateTimeOffset.Now;
						divot.KPD_GS_NKPackedUser = Env.CurrentUser.Initials;
						processedPackageHandlingUnitDivots.Add(divot);
					}
					package.UnpackInnersAndUpdateTopHandlingUnitPackage(package.PackageHandlingUnitHandlingUnitDivots.Except(processedPackageHandlingUnitDivots));
				}
				else
				{
					package.UnpackInnersAndUpdateTopHandlingUnitPackage();
				}
			}
			return processedPackages;
		}

		ForwardingPackage ProcessPackage(ForwardingPkgPackageDataObjectReader packageReader, HashSet<ForwardingPackage> processedPackages)
		{
			var package = packageReader.ReadIntoBusinessObject();
			processedPackages.Add(package);
			CheckIfContainerPackedInsideOtherPackage(package);

			// else the container is empty so we ignore packlines with qty <= 0 

			var addLink = new Action<KeyValuePair<ZInt, ForwardingPackage>>((kvp) =>
			{
				if (PackageLinks.ContainsKey(kvp.Key))
				{
					var errorMessage = Res.GetString("15B00258-1F84-489A-BF7B-C1498D197EE7", "Cannot add Package {0}, has duplicate link value {1}.", kvp.Value.KP_PackageID, kvp.Key);
					throw new DataObjectReadFailureException(errorMessage);
				}
				else
				{
					PackageLinks.Add(kvp.Key, kvp.Value);
				}
			});

			Array.ForEach(packageReader.PackageLinks.ToArray(), addLink);

			return package;
		}

		static void CheckIfContainerPackedInsideOtherPackage(ForwardingPackage package)
		{
			if (package.KP_F3_NKPackType == Constants.PkgUnit.Container && !package.KP_KP_ParentPackage.IsEmpty)
			{
				var errorMessage = Res.GetString("34845657-B51D-43CB-A2A5-4418709429BD", "Package {0} must not have Pack Type 'CNT' as it is inside a container or package {1}.", package.KP_PackageID, package.ParentPackage.KP_PackageID);
				throw new DataObjectReadFailureException(errorMessage);
			}
		}

		#endregion

		#region PackageLinks

		public Dictionary<ZInt, ForwardingPackage> PackageLinks => packageLinks ?? (packageLinks = new Dictionary<ZInt, ForwardingPackage>());
		Dictionary<ZInt, ForwardingPackage> packageLinks;

		#endregion
	}
}
