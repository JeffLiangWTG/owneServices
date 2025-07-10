using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Packing.DataTransfer.Universal
{
	public class PkgPackageJobDataObjectWriterHelper : DataObjectWriter<PkgPackageJob, Shipment>
	{
		public PkgPackageJobDataObjectWriterHelper(IDataWritingManager manager, IPackingParent packingParent, PkgPackageJob packageJob = null, Dictionary<ZGuid, ZInt> orderLineDictionary = null, bool assertNotNullPackingParent = true)
			: base(manager)
		{
			this.packingParent = assertNotNullPackingParent ? Argument.NotNull(packingParent, nameof(packingParent)) : packingParent;
			this.packageJob = packageJob;
			this.orderLineDictionary = orderLineDictionary;
		}

		public PkgPackageJobDataObjectWriterHelper(IDataWritingManager manager, IEnumerable<PkgPackage> packages, IPackingParentWithOutturn packingParentWithOutturn = null, Container container = null, IEnumerable<PkgPackage> topLevelHandlingUnits = null, Dictionary<ZGuid, ZInt> orderReferenceDictionary = null, CollectionContent contentType = CollectionContent.Complete)
			: base(manager)
		{
			this.packingParentWithOutturn = packingParentWithOutturn;
			this.packages = packages;
			this.container = container;
			this.topLevelHandlingUnits = topLevelHandlingUnits;
			this.orderReferenceDictionary = orderReferenceDictionary;
			this.contentType = contentType;
		}

		readonly PkgPackageJob packageJob;
		readonly IPackingParent packingParent;
		readonly IPackingParentWithOutturn packingParentWithOutturn;
		readonly Dictionary<ZGuid, ZInt> orderLineDictionary;
		readonly IEnumerable<PkgPackage> packages;
		readonly Container container;
		readonly IEnumerable<PkgPackage> topLevelHandlingUnits;
		readonly Dictionary<ZGuid, ZInt> orderReferenceDictionary;
		readonly CodeDescriptionPairList packageScreeningMethodCodeDescriptionList = new PackageScreeningMethodList().PackageScreeningMethodCodeDescriptionList;
		readonly CollectionContent contentType;

		#region PopulateDataObject

		public void PopulateDataObject(Shipment pkgPackageJobData)
		{
			linksDictionary = null;

			var packableItemParents = packingParent is IPackingParentWithPackableItems parentWithItems
				? parentWithItems.PackableItemParents // PackableItemParents cannot be null, ensured by test case.
				: Enumerable.Empty<IPackableItemParent>();

			PopulatePackableItems(pkgPackageJobData, packableItemParents);

			if (topLevelHandlingUnits?.Count() > 0)
			{
				if (pkgPackageJobData.ParentPackingLineCollection == null)
				{
					pkgPackageJobData.SetParentPackingLineCollection(() => new DataObjectList<PackingLine>());
				}

				foreach (var package in topLevelHandlingUnits)
				{
					var packageDataObject = PopulatePkgPackageDataObject(package, GetNextHandlingUnitLink(pkgPackageJobData), isHandlingUnit: true);
					pkgPackageJobData.ParentPackingLineCollection?.Add(packageDataObject);
				}
			}

			if (packageJob != null)
			{
				PopulatePkgPackages(packageJob.Packages, pkgPackageJobData);
				PopulateLoosePackageIDs(packageJob, pkgPackageJobData);
			}
			else if (packages != null)
			{
				if (container != null)
				{
					PopulatePkgPackages(packages, pkgPackageJobData, container);
				}
				else if (packingParentWithOutturn != null)
				{
					PopulateDataObjectUsingPackingParentWithOutturn(pkgPackageJobData);
				}
				else
				{
					PopulatePkgPackages(packages, pkgPackageJobData);
				}
			}
		}

		void PopulateDataObjectUsingPackingParentWithOutturn(Shipment pkgPackageJobData)
		{
			var containersAndPackages = new Dictionary<(string ContainerNumber, PkgPackageContainer Container), List<PkgPackage>>();
			var packagesWithNoContainer = new List<PkgPackage>();

			foreach (var package in packages)
			{
				var key = packingParentWithOutturn.GetParentContainer(package);
				if (key == (null, null))
				{
					packagesWithNoContainer.Add(package);
				}
				else if (containersAndPackages.ContainsKey(key))
				{
					containersAndPackages[key].Add(package);
				}
				else
				{
					containersAndPackages.Add(key, new List<PkgPackage>(new[] { package }));
				}
			}

			if (containersAndPackages.Any() && pkgPackageJobData.SetContainerCollection(() => pkgPackageJobData.ContainerCollection ?? new DataObjectList<Container>()))
			{
				var containerLink = 0;
				foreach (var containerAndPackages in containersAndPackages)
				{
					var containerDO = PopulatePkgPackageContainer(containerAndPackages.Key.Container.Package, containerLink);
					containerDO.ContainerNumber = containerAndPackages.Key.ContainerNumber; // since container number is not stored in package header in TW's RTU yet
					pkgPackageJobData.ContainerCollection.Add(containerDO);
					PopulatePkgPackages(containerAndPackages.Value, pkgPackageJobData, containerDO);
					containerLink++;
				}
			}

			PopulatePkgPackages(packagesWithNoContainer, pkgPackageJobData);
			pkgPackageJobData.TotalNoOfPiecesLanded = packingParentWithOutturn.TotalNumberOfPiecesOutturned;
		}

		#region PopulateLoosePackages

		void PopulateLoosePackageIDs(PkgPackageJob packageJob, Shipment pkgPackageJobData)
		{
			var parentJob = packageJob.ParentJob;
			if (parentJob != null && parentJob.IsLoosePackageIDsSupported)
			{
				if (TryGetPackingLineCollectionToUpdate(pkgPackageJobData, null, out var packingLineCollection))
				{
					foreach (var loosePackage in packageJob.LoosePackageIDs)
					{
						var loosePackingLine = new PackingLine(writeManager.WriterStrategy)
						{
							ReferenceNumber = loosePackage.KPH_PackageID
						};

						packingLineCollection.Add(loosePackingLine);
					}
				}
			}
		}

		#endregion

		#endregion

		#region PopulatePackableItems

		internal void PopulatePackableItems(Shipment pkgPackageJobData, IEnumerable<IPackableItemParent> packableItemParents)
		{
			var packableItemParentDOs = new DataObjectList<CommercialInvoiceLine>();

			foreach (var packableItemParent in packableItemParents)
			{
				packableItemParentDOs.Add(PopulatePackableItem(packingParent.Factory, packableItemParent, packableItemParentDOs));
			}

			if (packableItemParentDOs.Count > 0)
			{
				var commercialInvoiceHeader = new CommercialInvoiceHeader(writeManager.WriterStrategy).AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => packableItemParentDOs));
				pkgPackageJobData.CommercialInfo = new CommercialInfo { CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() { commercialInvoiceHeader } };
			}
		}

		CommercialInvoiceLine PopulatePackableItem(BusinessObjectFactory factory, IPackableItemParent packableItemParent, DataObjectList<CommercialInvoiceLine> lines)
		{
			var businessObject = (BusinessObject)packableItemParent;

			var link = lines.Count;
			LinksDictionary.Add(businessObject.PK, link);

			var commercialInvoiceLine = new CommercialInvoiceLine(writeManager.WriterStrategy);
			commercialInvoiceLine.PartNo = packableItemParent.Code;
			commercialInvoiceLine.Description = packableItemParent.Description;
			commercialInvoiceLine.InvoiceQuantity = packableItemParent.TotalQty;
			commercialInvoiceLine.InvoiceQuantityUnit = ListHelper.GetWithDescription<CodeDescriptionPair>(packableItemParent.TotalQtyUQ, PackTypes(factory));
			commercialInvoiceLine.LineNo = link;
			commercialInvoiceLine.Link = link;
			commercialInvoiceLine.Weight = packableItemParent.WeightPerUnit * packableItemParent.TotalQty;
			commercialInvoiceLine.WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(packableItemParent.WeightUQ, factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight));
			commercialInvoiceLine.SetCustomizedFieldCollection(() =>
			{
				var customFields = new List<CustomizedField>();
				foreach (var property in packableItemParent.AdditionalProperties.CustomProperties)
				{
					var value = (IZType)property.GetValue(businessObject);
					if (!value.IsEmpty)
					{
						customFields.Add(CustomizedField.New(property.Identifier, value));
					}
				}

				AddProductCodeForBackwardsCompatibility(customFields);

				return customFields.Count == 0 ? null : customFields;
			});

			PopulateOrderLineLink(businessObject, commercialInvoiceLine);

			return commercialInvoiceLine;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant")]
			void AddProductCodeForBackwardsCompatibility(List<CustomizedField> customFields)
			{
				const string productCodeConstant = "Product Code";

				var packableItemTablePrefix = packableItemParent.PackableItems.FirstOrDefault()?.TablePrefix;
				if (packableItemTablePrefix == WhsPickLineSchema.Constants.Prefix)
				{
					customFields.Add(CustomizedField.New(productCodeConstant, packableItemParent.Code));
				}
			}
		}

		void PopulateOrderLineLink(BusinessObject businessObject, CommercialInvoiceLine commercialInvoiceLine)
		{
			ZInt result;
			if (orderLineDictionary != null && orderLineDictionary.TryGetValue(businessObject.PK, out result))
			{
				commercialInvoiceLine.OrderLineLink = result;
			}
		}

		#region PackTypes

		CodeDescriptionPairList PackTypes(BusinessObjectFactory factory)
		{
			return packTypes ?? (packTypes = new RefPackTypeCollection(factory).GetAsCodeDescriptionPairWithStandardUnits());
		}

		CodeDescriptionPairList packTypes;

		#endregion

		#endregion

		#region PopulatePkgPackages

		void PopulatePkgPackages(IEnumerable<PkgPackage> packagesToPopulate, Shipment pkgPackageJobData, Container parentContainerData = null, PackingLine parentPackageData = null)
		{
			foreach (var package in packagesToPopulate)
			{
				if (package.IsContainer)
				{
					PopulatePkgPackageContainer(pkgPackageJobData, parentContainerData, parentPackageData, package);
				}
				else
				{
					if (TryGetPackingLineCollectionToUpdate(pkgPackageJobData, parentPackageData, out var packingLineCollection))
					{
						var packageDataObject = PopulatePkgPackageDataObject(package, GetNextPackingLineLink(pkgPackageJobData));
						packingLineCollection.Add(packageDataObject);
						if (parentContainerData != null)
						{
							packageDataObject.ContainerLink = parentContainerData.Link;
						}

						if (pkgPackageJobData.ParentPackingLineCollection != null)
						{
							if (HULinksDictionary.TryGetValue(package.KP_KP_TopHandlingUnitPackage, out var value))
							{
								packageDataObject.ParentPackingLineLink = value;
							}
						}

						if (package.HandlingUnitPackedPackages.Any())
						{
							PopulatePkgPackages(package.HandlingUnitPackedPackages, pkgPackageJobData, parentContainerData: null, packageDataObject);
						}
						else
						{
							PopulatePkgPackages(package.Packages, pkgPackageJobData, parentContainerData: null, packageDataObject);
						}
					}
				}
			}
		}

		void PopulatePkgPackageContainer(Shipment pkgPackageJobData, Container parentContainerData, PackingLine parentPackageData, PkgPackage package)
		{
			CheckValidityOfParents(parentContainerData, parentPackageData);
			if (pkgPackageJobData.SetContainerCollection(() => pkgPackageJobData.ContainerCollection ?? new DataObjectList<Container>()))
			{
				var containerCollection = pkgPackageJobData.ContainerCollection;
				containerCollection.Content = CollectionContent.Complete;
				var pkgPackageContainerDataObject = PopulatePkgPackageContainer(package, containerCollection.Count);
				containerCollection.Add(pkgPackageContainerDataObject);

				var packableItems = GetPackableItems(package);
				if (packableItems != null && TryGetPackingLineCollectionToUpdate(pkgPackageJobData, null, out var packingLineCollection))
				{
					ZDecimal packedQuantity = packableItems.Sum(p => p.PackedQuantity.GetValueOrDefault());

					var packingLine = new PackingLine(writeManager.WriterStrategy)
					{
						PackType = ListHelper.GetWithDescription<PackageType>(Constants.PkgUnit.Piece, package.Lookups.PackTypes),
						PackQty = packedQuantity.ToZLong(),
						ContainerLink = pkgPackageContainerDataObject.Link,
					};
					packingLine.SetPackedItemCollection(() => packableItems);

					packingLineCollection.Add(packingLine);
				}
				PopulatePkgPackages(package.Packages, pkgPackageJobData, pkgPackageContainerDataObject);
			}
		}

		#region GetContainerCollectionToUpdate

		void CheckValidityOfParents(Container parentContainerData, PackingLine parentPackageData)
		{
			if (parentContainerData != null || parentPackageData != null)
			{
				throw new ArgumentException("Container package should never be a child package.");
			}
		}

		#endregion

		#region GetPackingLineCollectionToUpdate

		bool TryGetPackingLineCollectionToUpdate(Shipment pkgPackageJobData, PackingLine parentPackageData, out IList<PackingLine> collection)
		{
			if (parentPackageData == null)
			{
				if (pkgPackageJobData.PackingLineCollection == null)
				{
					pkgPackageJobData.SetPackingLineCollection(() => new DataObjectList<PackingLine>()
					{
						Content = contentType
					});
				}
				collection = pkgPackageJobData.PackingLineCollection;
				return collection != null;
			}
			else
			{
				parentPackageData.SetPackingLineCollection(() => parentPackageData.PackingLineCollection ?? new List<PackingLine>());
				collection = parentPackageData.PackingLineCollection;
				return true;
			}
		}

		#endregion

		#endregion

		#region PopulatePkgPackageContainer

		Container PopulatePkgPackageContainer(PkgPackage package, ZInt link)
		{
			var pkgPackageContainer = package.Container;

			Argument.NotNull(package, "package");
			Argument.NotNull(pkgPackageContainer, "package.Container");

			var containerDataObject = new Container(writeManager.WriterStrategy);
			var containerLookups = pkgPackageContainer.Lookups;
			var containerView = packingParentWithOutturn?.GetContainerView(pkgPackageContainer);

			containerDataObject.ContainerNumber = package.KP_PackageID;
			containerDataObject.ContainerCount = package.KP_PackageQty;

			//dimensions
			containerDataObject.TotalLength = package.KP_Length;
			containerDataObject.TotalHeight = package.KP_Height;
			containerDataObject.TotalWidth = package.KP_Width;
			containerDataObject.LengthUnit = ListHelper.GetWithDescription<UnitOfLength>(package.KP_DimensionUQ, package.Lookups.DimensionUQs);

			// weights
			containerDataObject.TareWeight = package.KP_TareWeight;
			containerDataObject.DunnageWeight = package.KP_DunnageWeight;
			containerDataObject.GrossWeight = package.KP_Weight;
			containerDataObject.WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(package.KP_WeightUQ, package.Lookups.WeightUQs);

			// volume
			containerDataObject.VolumeCapacity = package.KP_Volume;
			containerDataObject.VolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(package.KP_VolumeUQ, package.Lookups.VolumeUQs);

			// seals & modes
			containerDataObject.Seal = pkgPackageContainer.K0_Seal1;
			containerDataObject.SecondSeal = pkgPackageContainer.K0_Seal2;
			containerDataObject.ThirdSeal = pkgPackageContainer.K0_Seal3;
			containerDataObject.IsSealOk = pkgPackageContainer.K0_IsSealOk;
			containerDataObject.SealPartyType = ListHelper.GetWithDescription<CodeDescriptionPair>(pkgPackageContainer.K0_Seal1PartyType, pkgPackageContainer.Lookups.SealParty_List);
			containerDataObject.SecondSealPartyType = ListHelper.GetWithDescription<CodeDescriptionPair>(pkgPackageContainer.K0_Seal2PartyType, pkgPackageContainer.Lookups.SealParty_List);
			containerDataObject.ThirdSealPartyType = ListHelper.GetWithDescription<CodeDescriptionPair>(pkgPackageContainer.K0_Seal3PartyType, pkgPackageContainer.Lookups.SealParty_List);

			// refrigeration
			containerDataObject.AirVentFlow = pkgPackageContainer.K0_AirVentFlowRate;
			containerDataObject.AirVentFlowRateUnit = ListHelper.GetWithDescription<CodeDescriptionPair>(pkgPackageContainer.K0_AirVentFlowRateUnit, containerLookups.AirVentFlowRateUnits);
			containerDataObject.HumidityPercent = pkgPackageContainer.K0_HumidityPercent;
			containerDataObject.IsControlledAtmosphere = pkgPackageContainer.K0_IsControlledAtmosphere;
			containerDataObject.RefrigGeneratorID = pkgPackageContainer.K0_RefrigGeneratorID;
			containerDataObject.SetPointTemp = pkgPackageContainer.K0_SetPointTemp;
			containerDataObject.SetPointTempUnit = pkgPackageContainer.K0_SetPointTempUnit;
			containerDataObject.TempRecorderSerialNo = pkgPackageContainer.K0_TempRecorderSerialNumber;

			// dates
			containerDataObject.PackDate = containerView?.PackCompleteDate;
			containerDataObject.LCLUnpack = containerView?.UnpackCompleteDate;

			// other
			containerDataObject.ContainerType = ContainerType.New(pkgPackageContainer.ContainerType);
			containerDataObject.IsDamaged = pkgPackageContainer.K0_IsDamaged;
			containerDataObject.IsEmptyContainer = pkgPackageContainer.K0_IsEmpty;
			containerDataObject.IsShipperOwned = pkgPackageContainer.K0_IsShipperOwned;
			containerDataObject.ContainerQuality = ListHelper.GetWithDescription<CodeDescriptionPair>(pkgPackageContainer.K0_Quality, containerLookups.ContainerQualities);
			containerDataObject.ContainerStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(pkgPackageContainer.K0_Status, containerLookups.ContainerStatuses);
			containerDataObject.FCL_LCL_AIR = ListHelper.GetWithDescription<ContainerMode>(pkgPackageContainer.K0_ContainerMode, containerLookups.ContainerModes);
			containerDataObject.TransportReference = package.KP_TransportRef;
			containerDataObject.GoodsDescription = package.KP_GoodsDescription;
			containerDataObject.HarmonisedCode = package.KP_HSCode;
			containerDataObject.Commodity = ListHelper.GetWithDescription<Commodity>(package.KP_RH_NKCommodityCode, package.Lookups.CommodityCodes);

			containerDataObject.IsCheckedWeighedCubed = package.KP_IsCheckedWeighedCubed;
			containerDataObject.Pillaged = package.KP_IsPillaged;
			containerDataObject.Fumigated = package.KP_IsFumigated;
			containerDataObject.HeatTreated = package.KP_IsHeatTreated;
			containerDataObject.RequiresTemperatureControl = package.KP_RequiresTemperatureControl;
			containerDataObject.RequiredTemperatureMinimum = package.KP_RequiredTemperatureMinimum;
			containerDataObject.RequiredTemperatureMaximum = package.KP_RequiredTemperatureMaximum;
			containerDataObject.RequiredTemperatureUnit = ListHelper.GetWithDescription<CodeDescriptionPair1Char>(package.KP_RequiredTemperatureUnit, package.Lookups.TemperatureUnits);
			containerDataObject.MarksAndNos = package.KP_MarksAndNumbers;

			containerDataObject.SetUNDGCollection(() => ProcessCollection(package.UNDGs, new UNDGDataObjectWriter(writeManager)));

			// Add Link so the Container could be identified by other DataObjects.
			containerDataObject.Link = link;
			LinksDictionary.Add(package.PK, link);

			return containerDataObject;
		}

		List<PackedItem> GetPackableItems(PkgPackage package)
		{
			var packableItems = new List<PackedItem>();
			if (package.PackedItems.Count > 0)
			{
				var groupedPackedItems = package.PackedItems.Typed.GroupBy(d => new { Key = d.Key, PackableItemParent = d.PackableItemParent });

				foreach (var packedItem in groupedPackedItems)
				{
					var packableItemParent = packedItem.Key.PackableItemParent;
					if (packableItemParent != null)
					{
						var description = packableItemParent.Description + (packableItemParent.DescriptionSupplement.IsEmpty
							? ""
							: string.Format(CultureInfo.InvariantCulture, " {0} {1}", packableItemParent.DescriptionSupplementSeparator, packableItemParent.DescriptionSupplement));

						var packedItemDataObject = new PackedItem
						{
							Description = description,
							PackedQuantity = packedItem.Sum(d => d.PackedQty),
							UnitOfQuantity = new PackageType { Code = packedItem.First().UQ },
							CommercialInvoiceLineLink = LinksDictionary[((BusinessObject)packableItemParent).PK],
						};

						packableItems.Add(packedItemDataObject);
					}
				}
			}
			else if (orderReferenceDictionary != null && orderReferenceDictionary.TryGetValue(package.PK, out var link))
			{
				packableItems.Add(new PackedItem { OrderLineLink = link });
			}

			return packableItems.Count == 0 ? null : packableItems;
		}

		ZInt GetNextPackingLineLink(Shipment pkgPackageJobData)
		{
			ZInt result = LinksDictionary.Count;
			if (pkgPackageJobData.ContainerCollection != null)
			{
				result -= pkgPackageJobData.ContainerCollection.Count;
			}

			if (pkgPackageJobData.CommercialInfo != null)
			{
				result -= pkgPackageJobData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection.Count;
			}

			return result;
		}

		ZInt GetNextHandlingUnitLink(Shipment pkgPackageJobData)
		{
			return HULinksDictionary.Count;
		}

		#endregion

		#region PopulatePkgPackageDataObject

		public PackingLine PopulatePkgPackageDataObject(PkgPackage package, ZInt link, IOutturnProvider outturnProviderOverride = null, bool isHandlingUnit = false)
		{
			Argument.NotNull(package, "package");

			var outturn = packingParentWithOutturn ?? package.PackageJob?.ParentJob as IPackingParentWithOutturn;
			var provider = outturn?.GetOutturnProvider(package) ?? outturnProviderOverride;
			var previousPackingLineID = FinPreviousPackingLineIDByPackge(package);

			var packageDataObject = new PackingLine(writeManager.WriterStrategy)
			{
				LengthUnit = ListHelper.GetWithDescription<UnitOfLength>(package.KP_DimensionUQ, package.Lookups.DimensionUQs),
				PackType = ListHelper.GetWithDescription<PackageType>(package.KP_F3_NKPackType, package.Lookups.PackTypes),
				Height = package.KP_Height,
				Length = package.KP_Length,
				MarksAndNos = package.KP_MarksAndNumbers,
				ReferenceNumber = package.KP_PackageID,
				PackQty = new ZLong(package.KP_PackageQty),
				TransportReference = package.KP_TransportRef,
				Volume = package.KP_Volume,
				VolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(package.KP_VolumeUQ, package.Lookups.VolumeUQs),
				Weight = package.KP_Weight,
				WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(package.KP_WeightUQ, package.Lookups.WeightUQs),
				Width = package.KP_Width,
				TareWeight = package.KP_TareWeight,
				DunnageWeight = package.KP_DunnageWeight,
				GoodsDescription = package.KP_GoodsDescription,
				HarmonisedCode = package.KP_HSCode,
				PackingLineID = package.KP_ExternalReference,
				PreviousPackingLineID = previousPackingLineID,
				Fumigated = package.KP_IsFumigated,
				NonStackable = package.KP_IsNonStackable,
				TopLoadOnly = package.KP_IsTopLoadOnly,
				HeatTreated = package.KP_IsHeatTreated,
				ISPMPallet = package.KP_IsISPMPallet,
				Pillaged = package.KP_IsPillaged,
				Commodity = ListHelper.GetWithDescription<Commodity>(package.KP_RH_NKCommodityCode, package.Lookups.CommodityCodes),
				OutturnQty = provider != null ? provider.OutturnQty : 0,
				OutturnDamagedQty = provider != null ? provider.OutturnDamagedQty : 0,
				OutturnDamagedReason = ListHelper.GetWithDescription<CodeDescriptionPair>(package.KP_DamagedReason, package.Lookups.DamagedReasons),
				OutturnPillagedQty = provider != null ? provider.OutturnPillagedQty : 0,
				OutturnedHeight = provider != null ? provider.OutturnedHeight : 0,
				OutturnedLength = provider != null ? provider.OutturnedLength : 0,
				OutturnedVolume = provider != null ? provider.OutturnedVolume : 0,
				OutturnedWeight = provider != null ? provider.OutturnedWeight : 0,
				OutturnedWidth = provider != null ? provider.OutturnedWidth : 0,
				UnloadDate = provider?.UnloadDate,
				LoadDate = provider?.LoadDate,
				IsHighRisk = (provider?.IsHighRisk ?? false) ? true : default(ZBool?),
				IsDamaged = package.KP_IsDamaged,
				IsCheckedWeighedCubed = package.KP_IsCheckedWeighedCubed,
			};

			if (packingParent?.ParentJobType == ParentJobType.WarehouseOrder)
			{
				packageDataObject.ItemNo = package.KP_Sequence;
			}

			PopulatePackageScreeningMethod(packageDataObject, package, provider);
			PopulatePackageAddInfo(packageDataObject, package);

			var nmfc = package.CommodityCode?.NMFC;
			if (nmfc != null)
			{
				packageDataObject.NMFC = new NMFC
				{
					Code = nmfc.FN_Code,
					Class = nmfc.FN_Class,
					Description = nmfc.FN_Description,
					ItemNo = nmfc.FN_ItemNo
				};
			}

			// temperatures
			packageDataObject.RequiresTemperatureControl = package.KP_RequiresTemperatureControl;
			packageDataObject.RequiredTemperatureMinimum = package.KP_RequiredTemperatureMinimum;
			packageDataObject.RequiredTemperatureMaximum = package.KP_RequiredTemperatureMaximum;
			packageDataObject.RequiredTemperatureUnit = ListHelper.GetWithDescription<CodeDescriptionPair1Char>(package.KP_RequiredTemperatureUnit, package.Lookups.TemperatureUnits);

			packageDataObject.SetUNDGCollection(() => ProcessCollection(package.UNDGs, new UNDGDataObjectWriter(writeManager)));
			packageDataObject.SetPortReferenceCollection(() => ProcessCollection(package.PortReferences, new PortReferenceDataObjectWriter(writeManager)));
			packageDataObject.SetAdditionalReferenceCollection(() => ProcessCollection(package.CusEntryNumReferences.Find(new ZQuery(CusEntryNumSchema.CE_Category, "OTH")), new AdditionalReferenceDataObjectWriter(writeManager)));

			// Add Link so the PackingLine could be identified by other DataObjects.
			packageDataObject.Link = link;
			try
			{
				if (isHandlingUnit)
				{
					HULinksDictionary.Add(package.PK, link);
				}
				else
				{
					LinksDictionary.Add(package.PK, link);
				}
			}
			catch (ArgumentException)
			{
				throw new DuplicatePackageException(package);
			}

			packageDataObject.SetPackedItemCollection(() => GetPackableItems(package));

			if (provider != null)
			{
				PopulateJobIDInReferenceNumberCollection(packageDataObject, provider.ActualTransportJobID, provider.ActualTransportJobTypeCode, provider.ActualTransportJobTypeDescription);
				PopulateJobIDInReferenceNumberCollection(packageDataObject, provider.ExpectedTransportJobID, provider.ExpectedTransportJobTypeCode, provider.ExpectedTransportJobTypeDescription);
			}

			// LinePrice Info
			GetLinePriceInfo(packageDataObject, package);

			return packageDataObject;
		}

		static ZString FinPreviousPackingLineIDByPackge(PkgPackage package)
		{
			var previousPackingLineID = package.KP_PreviousPackLineID;
			var jobPackLinePackage = (IJobPackLinePackage)package.Factory.LoadTop1(ObjectFactory.GetType(typeof(IJobPackLinePackage)), new ZQuery(JobPackLinePackageSchema.JPP_KP_Packge, package.PK));
			if (jobPackLinePackage == null || jobPackLinePackage.JPP_JL_PackLine.IsEmpty)
			{
				return previousPackingLineID;
			}

			var packLine = (IPackLine)package.Factory.LoadTop1(ObjectFactory.GetType(typeof(IPackLine)), new ZQuery(JobPackLinesSchema.PK, jobPackLinePackage.JPP_JL_PackLine));
			if (packLine == null || packLine.JL_PackLineId.IsEmpty)
			{
				return previousPackingLineID;
			}

			return packLine.JL_PackLineId;
		}

		void PopulatePackageScreeningMethod(PackingLine packageDataObject, PkgPackage package, IOutturnProvider provider)
		{
			var (screeningMethod, aviationSecurityAdditionalInspectionType) = package.GetScreeningMethod(provider);
			if (screeningMethod.HasValue)
			{
				packageDataObject.ScreeningMethod = screeningMethod.Value;
				packageDataObject.AviationSecurityInspectionType = new CodeDescriptionPair() { Code = screeningMethod.Value, Description = packageScreeningMethodCodeDescriptionList.GetDescriptionFromCode(screeningMethod.Value) };
			}
			if (aviationSecurityAdditionalInspectionType.HasValue)
			{
				packageDataObject.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair() { Code = aviationSecurityAdditionalInspectionType.Value, Description = packageScreeningMethodCodeDescriptionList.GetDescriptionFromCode(aviationSecurityAdditionalInspectionType.Value) };
			}
		}

		static void PopulatePackageAddInfo(PackingLine packageDataObject, PkgPackage package)
		{
			var attachedParent = package.PackageJob?.ParentJob as IPackingParentWithAttachedParent;
			var attachedJobNumber = attachedParent?.GetAttachedJobNumber(package) ?? ZString.Empty;
			var hasPreviousID = !package.KP_PreviousPackageID.IsEmpty;
			var hasAttachedJobNumber = !attachedJobNumber.IsEmpty;
			var bookedDimension = package.Factory.LoadFromUniqueKey<PkgPackageBookedDetail>(PkgPackageBookedDetailSchema.KPB_KP_Package, package.PK);
			var hasManifestedPackage = bookedDimension?.KPB_PackageQty > 0;
			if (hasPreviousID || hasAttachedJobNumber || hasManifestedPackage)
			{
				var addinfos = packageDataObject.AddInfoCollection ?? new List<AddInfo>();
				if (hasPreviousID)
				{
					addinfos.Add(new AddInfo { Key = AddInfoKeyTypes.Types.PreviousPackageID, Value = package.KP_PreviousPackageID });
				}
				if (hasAttachedJobNumber)
				{
					addinfos.Add(new AddInfo { Key = AddInfoKeyTypes.Types.ForwardingShipment, Value = attachedJobNumber });
				}
				if (hasManifestedPackage)
				{
					addinfos.Add(new AddInfo { Key = AddInfoKeyTypes.Types.IsManifestedPackage, Value = true.ToString() });
				}
				packageDataObject.SetAddInfoCollection(() => addinfos);
			}
		}

		static void PopulateJobIDInReferenceNumberCollection(PackingLine packageDataObject, string jobID, string jobTypeCode, string jobTypeCodeDescription)
		{
			if (!string.IsNullOrEmpty(jobID))
			{
				var transitWarehouseReferenceType = new EntryType { Code = jobTypeCode, Description = jobTypeCodeDescription };
				var transitWarehouseReference = new Reference { Type = transitWarehouseReferenceType, ReferenceNumber = jobID };
				if (packageDataObject.ReferenceNumberCollection != null || packageDataObject.SetReferenceNumberCollection(() => new List<Reference>()))
				{
					packageDataObject.ReferenceNumberCollection.Add(transitWarehouseReference);
				}
			}
		}

		#endregion

		static void GetLinePriceInfo(PackingLine packageDataObject, PkgPackage referencePackage)
		{
			var factory = referencePackage.Factory;
			var currencyConverter = CurrencyConverter.New(factory, ZDateTime.Now, ExchangeRateType.All, 7);
			var packages = new List<PkgPackage> { referencePackage };
			if (referencePackage.KP_KJ_ParentPackageJob.IsValid)
			{
				packages.AddRange(referencePackage.GetAllPackages());
			}

			var totalUnitPrice = Money.Empty;
			foreach (var package in packages)
			{
				totalUnitPrice = currencyConverter.Add(totalUnitPrice, GetPackageTotalLinePrice(package, currencyConverter));
			}

			packageDataObject.LinePrice = totalUnitPrice.Amount;
			packageDataObject.LinePriceCurrency = Currency.New(RefCurrency.LoadFromCurrencyCode(factory, totalUnitPrice.Currency.Code));
		}

		static Money GetPackageTotalLinePrice(PkgPackage package, CurrencyConverter converter)
		{
			var totalUnitPrice = Money.Empty;
			foreach (var packedItem in package.PackedItems.Typed)
			{
				var packableItemParent = packedItem.PackableItemParent;
				if (packableItemParent != null && packableItemParent.UnitPrice != null)
				{
					var packedQtyPrice = new Money(package.GetPackedQty(packableItemParent) * packableItemParent.UnitPrice.Amount, packableItemParent.UnitPrice.Currency);
					totalUnitPrice = converter.Add(totalUnitPrice, packedQtyPrice);
				}
			}

			return totalUnitPrice;
		}

		#region LinksDictionaries

		public Dictionary<ZGuid, ZInt> LinksDictionary => linksDictionary ?? (linksDictionary = new Dictionary<ZGuid, ZInt>());
		Dictionary<ZGuid, ZInt> linksDictionary;

		public Dictionary<ZGuid, ZInt> HULinksDictionary => huLinksDictionary ?? (huLinksDictionary = new Dictionary<ZGuid, ZInt>());
		Dictionary<ZGuid, ZInt> huLinksDictionary;

		#endregion

		protected override Shipment PopulateDataObject(PkgPackageJob sourceBO)
		{
			throw new InvalidOperationException("This is just a helper class, do not call 'GetDataObject()' with this class.");
		}

		[Serializable]
		public class DuplicatePackageException : Exception
		{
			public DuplicatePackageException(PkgPackage package)
			{
				this.Package = package;
			}

#if NETFRAMEWORK
			protected DuplicatePackageException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
			{
			}
#endif

			public PkgPackage Package { get; private set; }
		}
	}
}
