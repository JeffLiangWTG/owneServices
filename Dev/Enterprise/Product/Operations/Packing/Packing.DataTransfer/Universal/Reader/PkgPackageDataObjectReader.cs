using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Packing.DataTransfer.Universal
{
	public class PkgPackageDataObjectReader : PkgPackageDataObjectReader<PkgPackage>
	{
		public PkgPackageDataObjectReader(
			PackingLine packingLineData, IXmlImportLogger logger, UniversalObjectFactory factory, PkgPackageJob packageJob, PkgPackageCollection packageCollection,
			ImportOption importOption = ImportOption.Default, PkgPackage targetPackage = null, bool isProcessingOuterPackLine = false, bool shouldCreatePackageExtension = false,
			IEnumerable<PackingLine> packingLineCollection = null, bool shouldPopulateChildPackages = true)
			: base(packingLineData, logger, factory, packageJob, packageCollection, importOption, targetPackage, isProcessingOuterPackLine, shouldCreatePackageExtension, packingLineCollection, shouldPopulateChildPackages)
		{
		}

		public PkgPackageDataObjectReader(
			PackingLine packingLineData, IXmlImportLogger logger, UniversalObjectFactory factory, PkgPackage existingPackageImportedFromContainerCollection,
			ImportOption importOption = ImportOption.Default, bool shouldPopulateChildPackages = true)
			: base(packingLineData, logger, factory, existingPackageImportedFromContainerCollection, importOption, shouldPopulateChildPackages)
		{
		}
	}

	public class PkgPackageDataObjectReader<T> : DataObjectReader<PackingLine, T> where T : PkgPackage
	{
		public PkgPackageDataObjectReader(
			PackingLine packingLineData, IXmlImportLogger logger, UniversalObjectFactory factory, PkgPackageJob packageJob, PkgPackageCollection packageCollection,
			ImportOption importOption = ImportOption.Default, T targetPackage = null, bool isProcessingOuterPackLine = false, bool shouldCreatePackageExtension = false,
			IEnumerable<PackingLine> packingLineCollection = null, bool shouldPopulateChildPackages = true)
			: base(packingLineData, logger, factory)
		{
			PackageJob = Argument.NotNull(packageJob, nameof(packageJob));
			PackageCollection = packageCollection;
			PackingLineCollection = packingLineCollection;
			ImportOptions = importOption;
			TargetPackage = targetPackage;
			IsProcessingOuterPackline = isProcessingOuterPackLine;
			ShouldCreatePackageExtension = shouldCreatePackageExtension;
			ShouldPopulateChildPackages = shouldPopulateChildPackages;
		}

		readonly T TargetPackage;
		readonly IEnumerable<PackingLine> PackingLineCollection;
		readonly ImportOption ImportOptions;
		readonly bool IsProcessingOuterPackline;
		readonly bool ShouldCreatePackageExtension;
		readonly bool ShouldPopulateChildPackages;

		/// <summary>
		/// This constructor is only used for Packages that were split across the Container and Package Collection.
		/// Int'l do this for RORO to store vehicle information. While this is technically not correct, there are live clients using it
		/// and eServices advises that this cannot be changed. Therefore we need to handle this quirk and NOT create both a Container + Package.
		/// </summary>
		public PkgPackageDataObjectReader(
			PackingLine packingLineData, IXmlImportLogger logger, UniversalObjectFactory factory, T existingPackageImportedFromContainerCollection,
			ImportOption importOption = ImportOption.Default, bool shouldPopulateChildPackages = true)
			: base(packingLineData, logger, factory)
		{
			ExistingPackageImportedFromContainerCollection = Argument.NotNull(existingPackageImportedFromContainerCollection, nameof(existingPackageImportedFromContainerCollection));
			PackageJob = ExistingPackageImportedFromContainerCollection.PackageJob;
			ImportOptions = importOption;
			ShouldPopulateChildPackages = shouldPopulateChildPackages;
		}

		readonly PkgPackageJob PackageJob;
		readonly T ExistingPackageImportedFromContainerCollection;
		readonly PkgPackageCollection PackageCollection;

		public HashSet<T> ProcessedPackages
		{
			get
			{
				if (processedPackages == null)
				{
					processedPackages = new HashSet<T>();
				}

				return processedPackages;
			}
		}
		HashSet<T> processedPackages;

		bool MatchOnPackageIDs
		{
			get { return ImportOptions.HasAnyFlag(ImportOption.MatchOnPackageIDs, ImportOption.KeepUnmatchedPackages, ImportOption.MatchAndRelabelOnPreviousPackageID); }
		}

		bool MatchAndRelabelOnPreviousPackageID
		{
			get { return ImportOptions.HasFlag(ImportOption.MatchAndRelabelOnPreviousPackageID); }
		}

		#region GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(T targetBO)
		{
			var reason = base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
			var parentJob = PackageJob.ParentJob as IPackingParentSupportsImportingUnassignedPackageIdsAsPackages;
			if (parentJob != null && dataObject.IsLoosePackageIDDataObject())
			{
				// If PackingLine is a LoosePackageID:
				// * If Package exists: Ignore it.
				// * If Package does not exits: Import it.
				if (!IsNewBO)
				{
					reason = Res.GetString("73f25e25-9a06-489c-85b3-46afcd01a441", "There is a Package with ID '{0}' in this Package Job, the new Loose Package ID will be ignored.", dataObject.ReferenceNumber.Value);
				}
			}

			return reason;
		}

		#endregion

		#region GetExistingBusinessObject

		protected override T GetExistingBusinessObject()
		{
			return TargetPackage
				?? ExistingPackageImportedFromContainerCollection
				?? GetPackageFromPreviousID()
				?? GetPackageWithID()
				?? GetPackageFromPackingLineID();
		}

		T GetPackageWithID()
		{
			T result = null;

			if (MatchOnPackageIDs)
			{
				var packageID = dataObject.ReferenceNumber.GetValueOrDefault();
				if (!packageID.IsEmpty)
				{
					result = PackageCollection.Cast<T>().FirstOrDefault(p => p.KP_PackageID.EqualsIgnoringCase(packageID));
					// This is to match existing packages not in the same collection to prevent deleting/recreating the same package
					if (result == null)
					{
						result = PackageJob.GetAllPackagesOnJob().Cast<T>().FirstOrDefault(p => p.KP_PackageID.EqualsIgnoringCase(packageID));
						if (result != null)
						{
							PackageCollection.Add(result);
						}
					}
				}

				if (result != null)
				{
					ProcessedPackages.Add(result);
				}
			}

			return result;
		}

		T GetPackageFromPreviousID()
		{
			if (MatchAndRelabelOnPreviousPackageID)
			{
				var previousPackageID = GetPreviousPackageID(dataObject);
				if (!string.IsNullOrEmpty(previousPackageID))
				{
					var matchingPackage = PackageCollection.Cast<T>().FirstOrDefault(package => package.IsInDatabase && GetPackageIDOnlyIfNotRelabelled(package) == previousPackageID);
					var newPackageID = dataObject.ReferenceNumber;
					var isNewPackageIDRenamed = PackingLineCollection?.Any(p => newPackageID.HasValue && GetPreviousPackageID(p) == newPackageID.Value) ?? false;
					var hasMatchedPackage = matchingPackage != null && newPackageID.HasValue && (isNewPackageIDRenamed || !PackageCollection.Any(package => package.KP_PackageID == newPackageID.Value && package.PK != matchingPackage.PK));
					if (hasMatchedPackage)
					{
						return matchingPackage;
					}
					else
					{
						logger.Log(
						   Enterprise.Integration.LogType.Warning,
						   string.Format("Unable to find package to rename with package id {0}.", previousPackageID)
						);
					}
				}
			}

			return null;
		}

		ZString GetPackageIDOnlyIfNotRelabelled(PkgPackage package)
		{
			return !package.PackageIDHasChanges ? package.KP_PackageID : ZString.Empty;
		}

		T GetPackageFromPackingLineID()
		{
			T result = null;
			if (ImportOptions.HasFlag(ImportOption.MatchOnPackingLineID) && IsProcessingOuterPackline)
			{
				var packingLineID = dataObject.PackingLineID.GetValueOrDefault();
				var packageID = dataObject.ReferenceNumber.GetValueOrDefault();
				if (packageID.IsEmpty && !packingLineID.IsEmpty)
				{
					result = PackageCollection.Cast<T>().FirstOrDefault(p => p.KP_PackageID.IsEmpty && p.KP_ExternalReference == packingLineID);
					if (result == null)
					{
						result = PackageJob.GetAllPackagesOnJob().Cast<T>().FirstOrDefault(p => p.KP_PackageID.IsEmpty && p.KP_ExternalReference == packingLineID);
						if (result != null)
						{
							PackageCollection.Add(result);
						}
					}
				}
				if (result != null)
				{
					ProcessedPackages.Add(result);
				}
			}

			return result;
		}
		#endregion

		#region GetNewBusinessObject

		protected override T GetNewBusinessObject()
		{
			var package = base.GetNewBusinessObject();

			PackageCollection.Add(package);
			ProcessedPackages.Add(package);
			return package;
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(T packageBO)
		{
			((ISupportDataImporting)packageBO).IsImportingData = true;

			ClearTopHandlingUnitPackageAndDeleteDivotsIfRequired();
			PopulatePackageIDIfPackageIDChanged(packageBO);
			PopulateProperties(packageBO);
			PopulateUNDG(packageBO);
			PopulatePortReferences(packageBO);
			PopulateAdditionalReferences(packageBO);
			PopulatePackageLinks(packageBO);
			PopulateInnerPackages(packageBO);
			CreatePackageExtension(packageBO);
			ThrowImportFailuresOnInvalidData(packageBO);
		}

		void CreatePackageExtension(PkgPackage package)
		{
			if (ShouldCreatePackageExtension && package.PackageJob.ParentJob is IPackingParentSupportsPackageExtensions packageExtensionSupporter)
			{
				packageExtensionSupporter.CreateOrUpdatePackageExtension(package, logger);
			}
		}

		void PopulatePortReferences(PkgPackage packageBO)
		{
			if (dataObject.PortReferenceCollection != null)
			{
				ObjectFactory.Get<IPortReferenceCollectionReader>(nameof(IPortReferenceCollectionReader), dataObject.PortReferenceCollection.ToArray(), packageBO, logger, factory).ReadIntoCollection();
			}
		}

		void PopulateAdditionalReferences(PkgPackage packageBO)
		{
			if (dataObject.AdditionalReferenceCollection != null)
			{
				ObjectFactory.Get<IAdditionalReferenceCollectionReader>(nameof(IAdditionalReferenceCollectionReader), dataObject.AdditionalReferenceCollection.ToArray(), packageBO, logger, factory).ReadIntoCollection();
			}
		}

		#endregion

		#region PopulateProperties

		void PopulateProperties(PkgPackage packageBO)
		{
			using (new SemaphoreManager(packageBO.SuspendAddWeightToParentPackageSemaphore))
			{
				PkgPackageHeaderHelper.SetPackageQuanityAndID_IfValid(factory, logger, packageBO, dataObject.PackQty, dataObject.ReferenceNumber, importInnersAsNonTrackableItem: ImportOptions == ImportOption.ImportInnersAsNonTrackableItem);
				SetValue(packageBO, PkgPackageSchema.KP_PackageQty, dataObject.PackQty);
				SetValue(packageBO, PkgPackageSchema.KP_IsUnknownQty, dataObject.IsUnknownQty);

				var isPackTypeImportedFromContainerCollectionAlready = ExistingPackageImportedFromContainerCollection != null;
				if (!isPackTypeImportedFromContainerCollectionAlready)
				{
					SetValue(packageBO, PkgPackageSchema.KP_F3_NKPackType, GetPackType());
				}

				// dimensions
				SetValue(packageBO, PkgPackageSchema.KP_Length, dataObject.Length);
				SetValue(packageBO, PkgPackageSchema.KP_Height, dataObject.Height);
				SetValue(packageBO, PkgPackageSchema.KP_Width, dataObject.Width);
				SetValue(packageBO, PkgPackageSchema.KP_DimensionUQ, dataObject.LengthUnit);

				// weights
				SetValue(packageBO, PkgPackageSchema.KP_DunnageWeight, dataObject.DunnageWeight);
				SetValue(packageBO, PkgPackageSchema.KP_TareWeight, dataObject.TareWeight);
				SetValue(packageBO, PkgPackageSchema.KP_Weight, dataObject.Weight);
				SetValue(packageBO, PkgPackageSchema.KP_WeightUQ, dataObject.WeightUnit);

				// volume
				SetValue(packageBO, PkgPackageSchema.KP_Volume, dataObject.Volume);
				SetValue(packageBO, PkgPackageSchema.KP_VolumeUQ, dataObject.VolumeUnit);

				AddLogForPackageWhenUpdate(packageBO);

				// temperature
				SetValue(packageBO, PkgPackageSchema.KP_RequiresTemperatureControl, dataObject.RequiresTemperatureControl);
				SetValue(packageBO, PkgPackageSchema.KP_RequiredTemperatureMinimum, dataObject.RequiredTemperatureMinimum);
				SetValue(packageBO, PkgPackageSchema.KP_RequiredTemperatureMaximum, dataObject.RequiredTemperatureMaximum);
				SetValue(packageBO, PkgPackageSchema.KP_RequiredTemperatureUnit, dataObject.RequiredTemperatureUnit);

				// other
				SetValue(packageBO, PkgPackageSchema.KP_TransportRef, dataObject.TransportReference);
				SetValue(packageBO, PkgPackageSchema.KP_GoodsDescription, dataObject.GetCleanSingleLineGoodsDescription());
				SetValue(packageBO, PkgPackageSchema.KP_HSCode, dataObject.HarmonisedCode);
				SetValue(packageBO, PkgPackageSchema.KP_ExternalReference, dataObject.PackingLineID);
				SetValue(packageBO, PkgPackageSchema.KP_PreviousPackLineID, dataObject.PreviousPackingLineID);
				SetValue(packageBO, PkgPackageSchema.KP_RH_NKCommodityCode, dataObject.Commodity);

				SetValue(packageBO, PkgPackageSchema.KP_MarksAndNumbers, dataObject.MarksAndNos);
				SetValue(packageBO, PkgPackageSchema.KP_IsDamaged, dataObject.OutturnDamagedQty > 0);
				SetValue(packageBO, PkgPackageSchema.KP_DamagedReason, dataObject.OutturnDamagedReason);

				SetValue(packageBO, PkgPackageSchema.KP_IsFumigated, dataObject.Fumigated);
				SetValue(packageBO, PkgPackageSchema.KP_IsNonStackable, dataObject.NonStackable);
				SetValue(packageBO, PkgPackageSchema.KP_IsTopLoadOnly, dataObject.TopLoadOnly);
				SetValue(packageBO, PkgPackageSchema.KP_IsHeatTreated, dataObject.HeatTreated);
				SetValue(packageBO, PkgPackageSchema.KP_IsISPMPallet, dataObject.ISPMPallet);
				SetValue(packageBO, PkgPackageSchema.KP_IsPillaged, dataObject.Pillaged);
				SetValue(packageBO, PkgPackageSchema.KP_IsDamaged, dataObject.IsDamaged);
				SetValue(packageBO, PkgPackageSchema.KP_IsCheckedWeighedCubed, dataObject.IsCheckedWeighedCubed);
			}

			PopulatePackageBookedDimensionsIfSupported(packageBO);
		}

		void AddLogForPackageWhenUpdate(PkgPackage package)
		{
			if (package.IsInDatabase && !string.IsNullOrEmpty(package.KP_PackageID))
			{
				var resultStringList = new List<string>();

				if (package.KP_LengthInfo.HasChanges)
				{ resultStringList.Add(Res.GetString("51d0aa22-8ddb-4918-ba50-16e6668508ce", "Length: From {0} to {1}", package.KP_LengthInfo.OriginalValue, package.KP_Length)); }
				if (package.KP_HeightInfo.HasChanges)
				{ resultStringList.Add(Res.GetString("962cd628-8494-43b4-aed5-ca1f03a59fa4", "Height: From {0} to {1}", package.KP_HeightInfo.OriginalValue, package.KP_Height)); }
				if (package.KP_WidthInfo.HasChanges)
				{ resultStringList.Add(Res.GetString("566ef7f0-8b85-4600-af79-c92e89e9d042", "Width: From {0} to {1}", package.KP_WidthInfo.OriginalValue, package.KP_Width)); }
				if (package.KP_DimensionUQInfo.HasChanges)
				{ resultStringList.Add(Res.GetString("33403a96-a0eb-4edb-ab61-173a5e11a36a", "Dimension Unit: From {0} to {1}", package.KP_DimensionUQInfo.OriginalValue, package.KP_DimensionUQ)); }
				if (package.KP_WeightInfo.HasChanges)
				{ resultStringList.Add(Res.GetString("6cdacd46-03e1-4534-9f10-6646efd9c8ad", "Weight: From {0} to {1}", package.KP_WeightInfo.OriginalValue, package.KP_Weight)); }
				if (package.KP_WeightUQInfo.HasChanges)
				{ resultStringList.Add(Res.GetString("bdc9b0cb-d14b-4dd2-b6f4-3791dd9e2e8e", "Weight Unit: From {0} to {1}", package.KP_WeightUQInfo.OriginalValue, package.KP_WeightUQ)); }
				if (package.KP_VolumeInfo.HasChanges)
				{ resultStringList.Add(Res.GetString("95b16395-8227-4a66-9aac-e0b3397f418c", "Volume: From {0} to {1}", package.KP_VolumeInfo.OriginalValue, package.KP_Volume)); }
				if (package.KP_VolumeUQInfo.HasChanges)
				{ resultStringList.Add(Res.GetString("5502bd1c-0ebd-4af9-b5a9-1b8e4290eacf", "Volume Unit: From {0} to {1}", package.KP_VolumeUQInfo.OriginalValue, package.KP_VolumeUQ)); }

				if (resultStringList.Count > 0)
				{
					var log = Res.GetString("1c7cf4d9-80bf-470c-90b0-a917f4c8f226", "Package with ID {0} has updated {1}.", package.KP_PackageID, string.Join(", ", resultStringList));
					logger.Log(Enterprise.Integration.LogType.Information, log);
				}
				else
				{
					logger.Log(Enterprise.Integration.LogType.Information, Res.GetString("519cc54d-1c08-486b-b4ad-94fb2a69d5ea", "Package with ID {0} did not update weight, volume or dimension.", package.KP_PackageID));
				}
			}
		}

		void PopulatePackageBookedDimensionsIfSupported(PkgPackage packageBO)
		{
			if (packageBO.PackageJob?.ParentJob as IPackingParentSupportsImportingBookedDimensions != null)
			{
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_PackageQty, dataObject.PackQty);

				// dimensions
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_Length, dataObject.Length);
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_Height, dataObject.Height);
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_Width, dataObject.Width);
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_DimensionUQ, dataObject.LengthUnit);

				// weights
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_Weight, dataObject.Weight);
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_WeightUQ, dataObject.WeightUnit);

				// volume
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_Volume, dataObject.Volume);
				SetValue(packageBO.BookedDimensions, PkgPackageBookedDetailSchema.KPB_VolumeUQ, dataObject.VolumeUnit);

				AddLogForPackageBookedDetailsWhenUpdate(packageBO);
			}
		}

		void AddLogForPackageBookedDetailsWhenUpdate(PkgPackage package)
		{
			if (package.BookedDimensions.IsInDatabase && !string.IsNullOrEmpty(package.KP_PackageID))
			{
				var resultStringList = new List<string>();

				if (package.BookedDimensions.KPB_LengthInfo.HasChanges)
				{ resultStringList.Add(Res.GetString("201176fe-57e8-43a4-85c2-450c62fb6498", "Length: From {0} to {1}", package.BookedDimensions.KPB_LengthInfo.OriginalValue, package.BookedDimensions.KPB_Length)); }
				if (package.BookedDimensions.KPB_HeightInfo.HasChanges)
				{ resultStringList.Add(Res.GetString("5b941906-e172-43a9-b942-30afe6ede330", "Height: From {0} to {1}", package.BookedDimensions.KPB_HeightInfo.OriginalValue, package.BookedDimensions.KPB_Height)); }
				if (package.BookedDimensions.KPB_WidthInfo.HasChanges)
				{ resultStringList.Add(Res.GetString("85607663-05b8-4425-8751-38261f91a14d", "Width: From {0} to {1}", package.BookedDimensions.KPB_WidthInfo.OriginalValue, package.BookedDimensions.KPB_Width)); }
				if (package.BookedDimensions.KPB_DimensionUQInfo.HasChanges)
				{ resultStringList.Add(Res.GetString("2e6b301c-3472-4551-9829-3816507180cb", "Dimension Unit: From {0} to {1}", package.BookedDimensions.KPB_DimensionUQInfo.OriginalValue, package.BookedDimensions.KPB_DimensionUQ)); }
				if (package.BookedDimensions.KPB_WeightInfo.HasChanges)
				{ resultStringList.Add(Res.GetString("0e13c446-7f70-4489-be65-4e62a2dc9d8d", "Weight: From {0} to {1}", package.BookedDimensions.KPB_WeightInfo.OriginalValue, package.BookedDimensions.KPB_Weight)); }
				if (package.BookedDimensions.KPB_WeightUQInfo.HasChanges)
				{ resultStringList.Add(Res.GetString("b38089b7-27b2-43fe-9de4-bcfd7be5daef", "Weight Unit: From {0} to {1}", package.BookedDimensions.KPB_WeightUQInfo.OriginalValue, package.BookedDimensions.KPB_WeightUQ)); }
				if (package.BookedDimensions.KPB_VolumeInfo.HasChanges)
				{ resultStringList.Add(Res.GetString("d581c3f8-5c5c-454e-bb52-f05db80de602", "Volume: From {0} to {1}", package.BookedDimensions.KPB_VolumeInfo.OriginalValue, package.BookedDimensions.KPB_Volume)); }
				if (package.BookedDimensions.KPB_VolumeUQInfo.HasChanges)
				{ resultStringList.Add(Res.GetString("8e394ffc-de8a-40fd-a5f2-776c6144e394", "Volume Unit: From {0} to {1}", package.BookedDimensions.KPB_VolumeUQInfo.OriginalValue, package.BookedDimensions.KPB_VolumeUQ)); }

				if (resultStringList.Count > 0)
				{
					var log = Res.GetString("2f42e754-decc-4f1b-a553-f5cd0f1162de", "Booked Package Detail with ID {0} has updated {1}.", package.KP_PackageID, string.Join(", ", resultStringList));
					logger.Log(Enterprise.Integration.LogType.Information, log);
				}
				else
				{
					logger.Log(Enterprise.Integration.LogType.Information, Res.GetString("d2e00767-abfc-4321-b95e-9a767f435f39", "Booked Package Detail with ID {0} did not update weight, volume or dimension.", package.KP_PackageID));
				}
			}
		}

		void ClearTopHandlingUnitPackageAndDeleteDivotsIfRequired()
		{
			// An outer should not reference any handling unit via divot or have top handling unit package
			// This is to clean up inners that are unpacked and became outers
			if (IsProcessingOuterPackline)
			{
				var packageID = dataObject.ReferenceNumber.GetValueOrDefault();
				if (!packageID.IsEmpty && MatchOnPackageIDs)
				{
					var outterPackageMatchingID = PackageCollection.FirstOrDefault(p => p.KP_PackageID == packageID);
					if (outterPackageMatchingID != null && outterPackageMatchingID.KP_KP_TopHandlingUnitPackage.IsValid)
					{
						outterPackageMatchingID.KP_KP_TopHandlingUnitPackage = ZGuid.Empty;
						outterPackageMatchingID.PackageHandlingUnitPackageDivots.DeleteAll();
					}
				}
			}
		}

		/// <summary>
		/// This method is required because some modules in Enterprise, e.g Forwarding, incorrectly
		/// allow Inner Packages (PackLines) to have their Pack Type set to 'CNT'.
		/// Customs send us BLANK pack types, so we default to the generic pack type of Package.
		/// </summary>
		PackageType GetPackType()
		{
			var packType = dataObject.PackType;
			switch (packType.GetCodeAsUpperCase().Trim())
			{
				case Constants.PkgUnit.Container:
					return new PackageType { Code = Constants.PkgUnit.Unit };
				case "":
					return new PackageType { Code = Constants.PkgUnit.Package };
				default:
					return packType;
			}
		}

		#endregion

		#region PopulateUNDG

		void PopulateUNDG(PkgPackage packageBO)
		{
			if (dataObject.UNDGCollection != null)
			{
				var packageRow = GetColumnIndexer(packageBO);
				var packagePK = packageRow.GetValue(PkgPackageSchema.PK);
				packageBO.UNDGs.DeleteAll();

				foreach (var source in dataObject.UNDGCollection)
				{
					var undgBO = new UNDGDataObjectReader(source, logger, factory).ReadIntoBusinessObject();
					var undgRow = GetColumnIndexer(undgBO);
					SetValue(undgRow, UNDGDataItemSchema.DI_ParentID, packagePK);
					SetValue(undgRow, UNDGDataItemSchema.DI_ParentTableCode, PkgPackageSchema.Constants.Prefix);
				}
			}
		}

		#endregion

		#region PopulatePackageLinks

		void PopulatePackageLinks(T packageBO)
		{
			var link = dataObject.Link;
			if (link.HasValue)
			{
				PackageLinks.Add(link.Value, packageBO);
			}
		}

		#endregion

		#region PopulateInnerPackages

		void PopulateInnerPackages(PkgPackage packageBO)
		{
			if (dataObject.PackingLineCollection != null && ShouldPopulateChildPackages)
			{
				foreach (var innerPackageData in dataObject.PackingLineCollection)
				{
					var importOption = ImportOption.Default;
					if (ImportOptions == ImportOption.ImportInnersAsNonTrackableItem)
					{
						importOption = ImportOption.ImportInnersAsNonTrackableItem;
					}
					else if (!string.IsNullOrEmpty(innerPackageData.ReferenceNumber.GetValueOrDefault()))
					{
						importOption = ImportOption.MatchOnPackageIDs;
					}
					else if (!string.IsNullOrEmpty(innerPackageData.PackingLineID.GetValueOrDefault()))
					{
						importOption = ImportOption.MatchOnPackingLineID;
					}
					var packageReader = new PkgPackageDataObjectReader<T>(innerPackageData, logger, factory, PackageJob, packageBO.Packages, importOption);
					packageReader.ReadIntoBusinessObject();
					ProcessedPackages.UnionWith(packageReader.processedPackages);

					Array.ForEach(packageReader.PackageLinks.ToArray(), kvp => PackageLinks.Add(kvp.Key, kvp.Value));
				}
			}
		}

		#endregion

		#region PackageLinks

		public Dictionary<ZInt, T> PackageLinks
		{
			get { return packageLinks ?? (packageLinks = new Dictionary<ZInt, T>()); }
		}

		Dictionary<ZInt, T> packageLinks;

		#endregion

		#region ThrowImportFailuresOnInvalidData

		public static void ThrowImportFailuresOnInvalidData(PkgPackage packageBO)
		{
			ThrowImportFailureExceptionIfPkgPackageQtyIsZero(packageBO);
			ThrowImportFailureExceptionIfPkgPackageDimensionUnitIsInvalid(packageBO);
			ThrowImportFailureExceptionIfPkgPackageHeightLessThanZero(packageBO);
			ThrowImportFailureExceptionIfPkgPackageLengthLessThanZero(packageBO);
			ThrowImportFailureExceptionIfPkgPackageReqMinGreaterThanReqMaxTemp(packageBO);
			ThrowImportFailureExceptionIfPkgPackageVolumeLessThanZero(packageBO);
			ThrowImportFailureExceptionIfPkgPackageVolumeUnitIsInvalid(packageBO);
			ThrowImportFailureExceptionIfPkgPackageWeightLessThanZero(packageBO);
			ThrowImportFailureExceptionIfPkgPackageWeightUnitIsInvalid(packageBO);
			ThrowImportFailureExceptionIfPkgPackageWidthLessThanZero(packageBO);
		}

		#region ThrowImportFailureExceptionIfPkgPackageQtyIsZero

		static void ThrowImportFailureExceptionIfPkgPackageQtyIsZero(PkgPackage packageBO)
		{
			if (packageBO.KP_PackageQty < 0 || (packageBO.KP_PackageQty == 0 && !packageBO.KP_IsUnknownQty))
			{
				var message = Res.GetString("e7ae30c9-9761-4af3-9247-80e80f1fa6d1", @"All package counts must be greater than zero.
ID: {0}
Pack Type: {1}
Quantity: {2}", packageBO.KP_PackageID, packageBO.KP_F3_NKPackType, packageBO.KP_PackageQty);

				throw new DataObjectReadFailureException(message);
			}
		}

		#endregion

		#region ThrowImportFailureExceptionIfPkgPackageDimensionUnitIsInvalid

		static void ThrowImportFailureExceptionIfPkgPackageDimensionUnitIsInvalid(PkgPackage packageBO)
		{
			var dimenUnitIsValid = (packageBO.KP_Length.IsEmpty && packageBO.KP_Width.IsEmpty && packageBO.KP_Height.IsEmpty) || !packageBO.KP_DimensionUQ.IsEmpty;
			if (!dimenUnitIsValid)
			{
				var message = Res.GetString("fd7d0d2b-a5a7-479f-9e39-6871a4032b2d", @"A dimension unit of measurement is required if a dimension value is entered.
ID: {0}
Pack Type: {1}
Quantity: {2}
Length: {3}
Width: {4}
Height: {5}
Dimension Unit: {6}", packageBO.KP_PackageID, packageBO.KP_F3_NKPackType, packageBO.KP_PackageQty, packageBO.KP_Length, packageBO.KP_Width, packageBO.KP_Height, packageBO.KP_DimensionUQ);

				throw new DataObjectReadFailureException(message);
			}
			else if (!Constants.Length.Codes.Append(string.Empty).Contains(packageBO.KP_DimensionUQ.ToString()))
			{
				var message = Res.GetString("58a8e362-0b6d-44f5-b47a-f8b70badd5e3", @"Package dimension unit is invalid.
ID: {0}
Pack Type: {1}
Quantity: {2}
Dimension Unit: {3}", packageBO.KP_PackageID, packageBO.KP_F3_NKPackType, packageBO.KP_PackageQty, packageBO.KP_DimensionUQ);

				throw new DataObjectReadFailureException(message);
			}
		}

		#endregion

		#region ThrowImportFailureExceptionIfPkgPackageHeightLessThanZero

		static void ThrowImportFailureExceptionIfPkgPackageHeightLessThanZero(PkgPackage packageBO)
		{
			if (packageBO.KP_Height < 0)
			{
				var message = Res.GetString("da645f6e-9156-4f12-ab0d-497fd4c867aa", @"Package height must not be negative.
ID: {0}
Pack Type: {1}
Quantity: {2}
Height: {3}", packageBO.KP_PackageID, packageBO.KP_F3_NKPackType, packageBO.KP_PackageQty, packageBO.KP_Height);

				throw new DataObjectReadFailureException(message);
			}
		}

		#endregion

		#region ThrowImportFailureExceptionIfPkgPackageLengthLessThanZero

		static void ThrowImportFailureExceptionIfPkgPackageLengthLessThanZero(PkgPackage packageBO)
		{
			if (packageBO.KP_Length < 0)
			{
				var message = Res.GetString("631bbecc-d07d-410f-b833-659df62f2ce5", @"Package length must not be negative.
ID: {0}
Pack Type: {1}
Quantity: {2}
Length: {3}", packageBO.KP_PackageID, packageBO.KP_F3_NKPackType, packageBO.KP_PackageQty, packageBO.KP_Length);

				throw new DataObjectReadFailureException(message);
			}
		}

		#endregion

		#region ThrowImportFailureExceptionIfPkgPackageReqMinGreaterThanReqMaxTemp

		static void ThrowImportFailureExceptionIfPkgPackageReqMinGreaterThanReqMaxTemp(PkgPackage packageBO)
		{
			if (packageBO.KP_RequiredTemperatureMaximum < packageBO.KP_RequiredTemperatureMinimum)
			{
				var message = Res.GetString("2d6fe06b-c43e-472b-ac6b-0838d3aa94b6", @"Minimum temperature cannot be greater than maximum temperature.
ID: {0}
Pack Type: {1}
Quantity: {2}
Required Min Temperature: {3}
Required Max Temperature: {4}", packageBO.KP_PackageID, packageBO.KP_F3_NKPackType, packageBO.KP_PackageQty, packageBO.KP_RequiredTemperatureMinimum, packageBO.KP_RequiredTemperatureMaximum);

				throw new DataObjectReadFailureException(message);
			}
		}

		#endregion

		#region ThrowImportFailureExceptionIfPkgPackageVolumeLessThanZero

		static void ThrowImportFailureExceptionIfPkgPackageVolumeLessThanZero(PkgPackage packageBO)
		{
			if (packageBO.KP_Volume < 0)
			{
				var message = Res.GetString("be09113e-b8c6-4cba-a039-e897600e5b13", @"Package volume must not be negative.
ID: {0}
Pack Type: {1}
Quantity: {2}
Volume: {3}
Volume Unit: {4}", packageBO.KP_PackageID, packageBO.KP_F3_NKPackType, packageBO.KP_PackageQty, packageBO.KP_Volume, packageBO.KP_VolumeUQ);

				throw new DataObjectReadFailureException(message);
			}
		}

		#endregion

		#region ThrowImportFailureExceptionIfPkgPackageVolumeUnitIsInvalid

		static void ThrowImportFailureExceptionIfPkgPackageVolumeUnitIsInvalid(PkgPackage packageBO)
		{
			var volumeUnitIsValid = packageBO.KP_Volume.IsEmpty || !packageBO.KP_VolumeUQ.IsEmpty;
			if (!volumeUnitIsValid)
			{
				var message = Res.GetString("03119f41-02b6-46ba-b302-2fb02b2c96e6", @"Volume Unit required if a package volume value is entered.
ID: {0}
Pack Type: {1}
Quantity: {2}
Volume: {3}
Volume Unit: {4}", packageBO.KP_PackageID, packageBO.KP_F3_NKPackType, packageBO.KP_PackageQty, packageBO.KP_Volume, packageBO.KP_VolumeUQ);

				throw new DataObjectReadFailureException(message);
			}
			else if (!Constants.Volume.Codes.Append(string.Empty).Contains(packageBO.KP_VolumeUQ.ToString()))
			{
				var message = Res.GetString("9cda2c35-b2b1-4e6a-8465-8b68631c77fe", @"Package volume unit is invalid.
ID: {0}
Pack Type: {1}
Quantity: {2}
Volume Unit: {3}", packageBO.KP_PackageID, packageBO.KP_F3_NKPackType, packageBO.KP_PackageQty, packageBO.KP_VolumeUQ);

				throw new DataObjectReadFailureException(message);
			}
		}

		#endregion

		#region ThrowImportFailureExceptionIfPkgPackageWeightLessThanZero

		static void ThrowImportFailureExceptionIfPkgPackageWeightLessThanZero(PkgPackage packageBO)
		{
			if (packageBO.KP_Weight < 0)
			{
				var message = Res.GetString("1c19fcdc-86eb-417b-b4de-17d2a4e52d66", @"Package weight must not be negative.
ID: {0}
Pack Type: {1}
Quantity: {2}
Weight: {3}", packageBO.KP_PackageID, packageBO.KP_F3_NKPackType, packageBO.KP_PackageQty, packageBO.KP_Weight);

				throw new DataObjectReadFailureException(message);
			}
		}

		#endregion

		#region ThrowImportFailureExceptionIfPkgPackageWeightUnitIsInvalid

		static void ThrowImportFailureExceptionIfPkgPackageWeightUnitIsInvalid(PkgPackage packageBO)
		{
			var weightUnitIsValid = packageBO.KP_Weight.IsEmpty || !packageBO.KP_WeightUQ.IsEmpty;
			if (!weightUnitIsValid)
			{
				var message = Res.GetString("32516b9e-5461-43e5-87e0-14b972d977e0", @"Weight Unit required if a package weight value is entered.
ID: {0}
Pack Type: {1}
Quantity: {2}
Weight: {3}
Weight Unit: {4}", packageBO.KP_PackageID, packageBO.KP_F3_NKPackType, packageBO.KP_PackageQty, packageBO.KP_Weight, packageBO.KP_WeightUQ);

				throw new DataObjectReadFailureException(message);
			}
			else if (!Constants.Weight.Codes.Append(string.Empty).Contains(packageBO.KP_WeightUQ.ToString()))
			{
				var message = Res.GetString("4707a021-215c-4f0d-9ed3-9fe5ee1b4d6c", @"Package weight unit is invalid.
ID: {0}
Pack Type: {1}
Quantity: {2}
Weight Unit: {3}", packageBO.KP_PackageID, packageBO.KP_F3_NKPackType, packageBO.KP_PackageQty, packageBO.KP_WeightUQ);

				throw new DataObjectReadFailureException(message);
			}
		}

		#endregion

		#region ThrowImportFailureExceptionIfPkgPackageWidthLessThanZero

		static void ThrowImportFailureExceptionIfPkgPackageWidthLessThanZero(PkgPackage packageBO)
		{
			if (packageBO.KP_Width < 0)
			{
				var message = Res.GetString("cf8f522b-d15e-4380-b2c4-fa7dd24f350c", @"Package width must not be negative.
ID: {0}
Pack Type: {1}
Quantity: {2}
Width: {3}", packageBO.KP_PackageID, packageBO.KP_F3_NKPackType, packageBO.KP_PackageQty, packageBO.KP_Width);

				throw new DataObjectReadFailureException(message);
			}
		}

		#endregion

		#endregion

		#region PreviousPackageID

		void PopulatePackageIDIfPackageIDChanged(PkgPackage packageBO)
		{
			if (MatchAndRelabelOnPreviousPackageID)
			{
				var newPackageId = dataObject.ReferenceNumber;
				var previousPackageId = GetPreviousPackageID(dataObject);

				if (!string.IsNullOrEmpty(packageBO.KP_PackageID) && !string.IsNullOrEmpty(newPackageId) && !string.IsNullOrEmpty(previousPackageId) && packageBO.KP_PackageID != newPackageId.Value)
				{
					logger.Log(
						Enterprise.Integration.LogType.Information,
						string.Format("Renaming package {0} to {1}.", packageBO.KP_PackageID, newPackageId)
					);
					packageBO.KP_PackageID = newPackageId.Value;
				}
			}
		}

		public static ZString GetPreviousPackageID(PackingLine packingLine)
		{
			return packingLine?.AddInfoCollection?.FirstOrDefault(t => t.Key.GetValueOrDefault() == AddInfoKeyTypes.Types.PreviousPackageID)?.Value ?? ZString.Empty;
		}

		#endregion
	}
}
