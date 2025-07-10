using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Integration.Forwarding;
using Constants = Enterprise.Core.Constants;
using UniversalPackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;

namespace Enterprise.Freight.Business
{
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	[UniversalCopyIgnoreElement(AutoJobPackLines.Schema.JL_PackLineId, AutoJobPackLines.Schema.JL_OriginTransitWarehouseStatus)]
	[UniversalCopyWithExtendedEntities]
	[CodeProperty(nameof(JL_PackLineId))]
	public class PackLine : AutoJobPackLines,
		IPackLine,
		IPackLineInfo,
		IGoods,
		IUNDGDataItemProvider,
		IPackTypeDafaultable,
		IDefaultNumberOfDecimalsSupporterWithSchemaColumn,
		IHarmonisedCodesProvider,
		ITariffFormatProvider,
		IRequiredTemperature,
		ISupportInspectionType
	{
		#region Schema

		public new class Schema : AutoJobPackLines.Schema
		{
			public const string JL_JC = "JL_JC";
			public const string JL_Calc_JS_UniqueConsignRef = "JL_Calc_JS_UniqueConsignRef";
			public const string JL_Calc_JS_RH = "JL_Calc_JS_RH";
			public const string JL_Calc_ConsignorPK = "JL_Calc_ConsignorPK";
			public const string JL_Calc_ConsigneePK = "JL_Calc_ConsigneePK";
			public const string JL_Calc_JV_Vessel = "JL_Calc_JV_Vessel";
			public const string JL_Calc_JV_VoyageNo = "JL_Calc_JV_VoyageNo";
			public const string JL_Calc_JX_NKLoadPort = "JL_Calc_JX_NKLoadPort";
			public const string JL_Calc_JX_NKDischPort = "JL_Calc_JX_NKDischPort";
			public const string JL_Calc_JS_GoodsDescription = "JL_Calc_JS_GoodsDescription";
			public const string JL_Calc_JX_ETD = "JL_Calc_JX_ETD";
			public const string JL_Calc_IsReceived = "JL_Calc_IsReceived";
			public const string JL_Calc_SealNumber = "JL_Calc_SealNumber";
			public const string JL_JS_HouseBill = "JL_JS_HouseBill";
			public const string JL_JS_InterimReceipt = "JL_JS_InterimReceipt";
			public const string JL_Calc_InStock = "JL_Calc_InStock";
			public const string JL_Calc_FirstImportContainerNum = "JL_Calc_FirstImportContainerNum";
			public const string JL_Calc_JS_OH_HandledOnBehalfOfForwarder = "JL_Calc_JS_OH_HandledOnBehalfOfForwarder";
			public const string JL_Calc_FirstImportContainer = "JL_Calc_FirstImportContainer";
			public const string JL_Calc_OutturnedInStock = "JL_Calc_OutturnedInStock";
			public const string JL_Calc_JS_MarksAndNumbers = "JL_Calc_JS_MarksAndNumbers";
			public const string JL_Calc_JS_Destination = "JL_Calc_JS_Destination";
			public const string JL_Calc_PackagesToDeliver = "JL_Calc_PackagesToDeliver";
			public const string JL_Calc_WeightToDeliver = "JL_Calc_WeightToDeliver";
			public const string JL_Calc_VolumeToDeliver = "JL_Calc_VolumeToDeliver";
			public const string JL_Calc_PacklineToPkgPackage_QtyDiscrepancy = "JL_Calc_PacklineToPkgPackage_QtyDiscrepancy";
			public const string JL_Calc_PacklineToPkgPackage_VolumeDiscrepancy = "JL_Calc_PacklineToPkgPackage_VolumeDiscrepancy";
			public const string JL_Calc_PacklineToPkgPackage_WeightDiscrepancy = "JL_Calc_PacklineToPkgPackage_WeightDiscrepancy";
			public const string JL_InspectionTypeCode = "JL_InspectionTypeCode";
			public const string JL_AdditionalInspectionTypeCode = "JL_AdditionalInspectionTypeCode";
			public const string JL_Calc_OriginTransitWarehouse = "JL_Calc_OriginTransitWarehouse";
			public const string JL_OutturnUD = "JL_OutturnUD";
			public const string JL_OutturnVolumeUQ = "JL_OutturnVolumeUQ";
			public const string JL_OutturnWeightUQ = "JL_OutturnWeightUQ";
			public const string JL_Calc_DGClass = "JL_Calc_DGClass";
			public const string JL_Calc_DGSubstance = "JL_Calc_DGSubstance";
			public const string JL_Calc_Girth = "JL_Calc_Girth";
			public const string JL_Calc_LargestDimension = "JL_Calc_LargestDimension";

			public const string PackagesToDeliver = "PackagesToDeliver";
			public const string DangerousGoodsCollection = "UNDGs";
			public const string HarmonisedCodes = "HarmonisedCodes";

			public const string CalculatedVolume = "CalculatedVolume";
			public const string CalculatedOutturnedVolume = "CalculatedOutturnedVolume";

			//public const string PackagesConfirmedDispatchFromDestinationCFS = "PackagesConfirmedDispatchFromDestinationCFS";
		}
		#endregion

		public PackLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		public override void OnLoaded()
		{
			base.OnLoaded();

			ShipmentLinkLogs.Add(string.Format(CultureInfo.InvariantCulture, "Initialized New PackLine: PK: {0}, JL_JS: {1}, IsInDB: {2}", PK, JL_JS, IsInDatabase));

			if (JL_JS.IsEmpty)
			{
				stackTraceForMissingDivotErrorReport = System.Environment.StackTrace;
				ShipmentLinkLogs.Add(stackTraceForMissingDivotErrorReport);
			}
		}

		#region Type Decider

		public static readonly PackLineTypeDecider TypeDecider = new PackLineTypeDecider();

		#endregion

		#region Validation

		public new PackLineValidation Validation
		{
			get { return (PackLineValidation)base.Validation; }
		}

		protected override JobPackLinesValidation GetNewValidation()
		{
			return new PackLineValidation(this);
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			JL_FreightMode = FreightConstants.InnerPackType;
			JL_Outturn = 0;
			JL_Pillaged = 0;
			JL_Damaged = 0;
			JL_Length = 0;
			JL_Height = 0;
			JL_Width = 0;
			JL_UnitOfDimension = Env.Registry.OuterPacklinesMeasurementDefaultUnit;
			JL_ActualVolumeUQ = Env.Registry.FreightVolumeUnit;
			JL_ActualWeightUQ = Env.Registry.FreightWeightUnit;
		}

		#endregion

		#region GetAdditionalInfoForZSaveExceptionCore

		protected override ZString GetAdditionalInfoForZSaveExceptionCore()
		{
			var additionalInfo = base.GetAdditionalInfoForZSaveExceptionCore();
			if (IsDeleted)
			{
				return additionalInfo;
			}
			if (!string.IsNullOrEmpty(LastKnownTransitWarehouseAddressInvalidTrace))
			{
				var builder = new ZStringBuilder();
				builder.Append((NoResString)"LastKnownTransitWarehouseAddress is invalid:");
				builder.Append(LastKnownTransitWarehouseAddressInvalidTrace);
				additionalInfo += builder.ToStringWithNewLineBetweenAppends();
			}
			return additionalInfo;
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clonedArgs = new BusinessObjectCloneArgs(
					args.AlternativeFactoryToInstantiateCloneIn,
					args.GetExcludedColumns(),
					args.TypeToCloneAs,
					args.PerformRowCopyWithoutTriggeringValidationAndSetter,
					args.CopyDecider);

			bool shouldCloneWeight = !clonedArgs.IsExcludedFromCloning(Schema.JL_ActualWeight);
			bool shouldCloneVolume = !clonedArgs.IsExcludedFromCloning(Schema.JL_ActualVolume);
			bool shouldCloneJL_JS = !clonedArgs.IsExcludedFromCloning(Schema.JL_JS);

			clonedArgs.AddExcludedColumns(new string[]
			{
				Schema.JL_ActualWeight,
				Schema.JL_ActualVolume,
				Schema.JL_JS
			});

			PackLine clonedPackLine = (PackLine)base.CloneInternal(clonedArgs);

			if (shouldCloneWeight)
			{
				clonedPackLine.JL_ActualWeight = JL_ActualWeight;
			}
			if (shouldCloneVolume)
			{
				clonedPackLine.JL_ActualVolume = JL_ActualVolume;
			}
			if (shouldCloneJL_JS)
			{
				clonedPackLine.JL_JS = JL_JS;
			}

			if (IsOuterPackType && !clonedArgs.IsExcludedFromCloning(Schema.DangerousGoodsCollection))
			{
				foreach (UNDGDataItem dgItem in UNDGs)
				{
					List<string> properties = new List<string>();
					properties.Add(UNDGDataItemSchema.DI_ParentID.Name);
					UNDGDataItem clonedDg = (UNDGDataItem)dgItem.Clone(new BusinessObjectCloneArgs(properties));
					clonedDg.DI_ParentID = clonedPackLine.PK;
				}
			}

			if (IsOuterPackType && !clonedArgs.IsExcludedFromCloning(Schema.HarmonisedCodes))
			{
				foreach (JobPackLineHarmonisedCode hc in HarmonisedCodes)
				{
					List<string> properties = new List<string>();
					properties.Add(JobPackLineHarmonisedCodeSchema.JLH_JL.Name);
					var clonedHC = (JobPackLineHarmonisedCode)hc.Clone(new BusinessObjectCloneArgs(properties));
					clonedHC.JLH_JL = clonedPackLine.PK;
				}
			}

			return clonedPackLine;
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted)
			{
				ShipmentLinkLogs.Add(string.Format(CultureInfo.InvariantCulture, "Deleting: JL_JS: {0}", JL_JS));
				ShipmentLinkLogs.Add(System.Environment.StackTrace);
			}

			CommonPickupDeliveryConfirmDivotCollection divots = new CommonPickupDeliveryConfirmDivotCollection(this);
			divots.DeleteAll();

			var packagesToBeDeleted = new List<PkgPackage>();
			PkgPackageCollection.ForEach(packagesToBeDeleted.Add);

			if (FreightConfigurationRegistry.Instance.EnableTWPackageLinking.Value)
			{
				if (packagesToBeDeleted.Any())
				{
					PkgPackageCollection.RemoveAllFromRelationship();

					var packagesForSHP = packagesToBeDeleted.Where(pkg => pkg?.PackageJob?.KJ_ParentTableCode.EqualsIgnoringCase(JobShipmentSchema.Constants.Prefix) ?? false).ToList();
					foreach (PkgPackage package in packagesForSHP)
					{
						var packageJob = package.PackageJob;
						package.Delete();
						if (packageJob.Packages.Count == 0)
						{
							packageJob.Delete();
						}
					}
				}
			}
			else
			{
				PkgPackageCollection.RemoveAllFromRelationship();
				packagesToBeDeleted.ForEach(p => p.Delete());
			}

			Containers.RemoveAll();
			PackLocations.RemoveAndDeleteAll();
			InspectionTypeCusEntryNumbersForAllCountries.RemoveAndDeleteAll();
			AdditionalInspectionTypeCusEntryNumbersForAllCountries.RemoveAndDeleteAll();
			HarmonisedCodes.DeleteAll();

			try
			{
				base.Delete();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (!IsDeleted && JL_JS.IsEmpty)
				{
					ShipmentLinkLogs.Add(string.Format(CultureInfo.InvariantCulture, "Called base delete but an exception has ocurred: {0}, JL_JS: {1}", ex.Message, JL_JS));
				}

				throw;
			}

			if (!IsDeleted)
			{
				ShipmentLinkLogs.Add(string.Format(CultureInfo.InvariantCulture, "Called base delete but Packline is not marked as deleted: JL_JS: {0}", JL_JS));
			}
		}

		#endregion

		#region Name

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = "";

				switch (JL_FreightMode)
				{
					case FreightConstants.InnerPackType:
						result = Res.GetString("ee312b51-1c47-473c-9a2b-3b2f6a0a2142", "Inner Package");
						break;

					case FreightConstants.OuterPackType:
						result = Res.GetString("dd1dca4f-6670-4236-85fd-f35bc6a40d90", "Outer Package");
						break;

					case FreightConstants.DeliveryPackType:
						result = Res.GetString("1e0a31fe-7474-449f-8ea1-72c2b31c67c6", "Delivery Package");
						break;
				}

				return result;
			}
		}

		#endregion

		#region FreightMode

		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public override ZString JL_FreightMode
		{
			get
			{
				return base.JL_FreightMode;
			}
			set
			{
				base.JL_FreightMode = value;
			}
		}

		#endregion

		#region DataRefresh

		protected override void OnBeforeUpdatedByDataRefresh()
		{
			base.OnBeforeUpdatedByDataRefresh();
			JL_JS_BeforeRefresh = JL_JS;
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			if (JL_JS != JL_JS_BeforeRefresh)
			{
				ShipmentLinkLogs.Add(string.Format(CultureInfo.InvariantCulture, "Updated JL_JS by DataRefresh: {0} -> {1}", JL_JS_BeforeRefresh, JL_JS));
			}

			isHarmonisedCodeTariffSet = false;
		}
		ZGuid JL_JS_BeforeRefresh;

		#endregion

		protected override ZAddress GetNewJL_OA_LastKnownTransitWarehouseAddress_ZAddress()
		{
			var lastKnownTransitWarehouseAddress = base.GetNewJL_OA_LastKnownTransitWarehouseAddress_ZAddress();
			lastKnownTransitWarehouseAddress.AddressIsMandatoryIfOrgIsValid = true;

			if (ZArchitecture.Environment.Globals.IsUserInteractive && !Env.CurrentUser.IsBatchProcessor)
			{
				lastKnownTransitWarehouseAddress.GetDefaultAddress = LastKnownTransitWarehouseAddress_GetDefaultAddress;
			}

			return lastKnownTransitWarehouseAddress;
		}

		ZGuid LastKnownTransitWarehouseAddress_GetDefaultAddress(MasterFiles.Integration.IOrgHeader org)
		{
			return GetDefaultLastKnownTransitWarehouseAddress(org);
		}

		public ZGuid GetDefaultLastKnownTransitWarehouseAddress(MasterFiles.Integration.IOrgHeader org, OrgAddress defaultAddress = null, CommonConsol consol = null)
		{
			if (org == null || (defaultAddress != null && defaultAddress.OA_OH != org.PK))
			{
				return ZGuid.Empty;
			}

			var matchedAddresses = GetMatchedOrgAddressForLastKnownTransitWarehouseAddress(consol).Where(x => x != null && x.OA_OH == org.PK).ToList();

			if (defaultAddress != null && matchedAddresses.Any(address => address.PK == defaultAddress.PK))
			{
				return defaultAddress.PK;
			}

			if (matchedAddresses.Any() && matchedAddresses.All(x => x.PK == matchedAddresses[0].PK))
			{
				return matchedAddresses[0].PK;
			}

			var orgHeader = org as OrgHeader;
			if (orgHeader.Addresses.Count == 1)
			{
				return orgHeader.Addresses[0].PK;
			}

			return ZGuid.Empty;
		}

		IEnumerable<OrgAddress> GetMatchedOrgAddressForLastKnownTransitWarehouseAddress(CommonConsol consol = null)
		{
			if (JL_LastKnownTransitWarehouseStatus == FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received)
			{
				yield return Shipment?.ExportReceivingDepot;
				yield return (consol ?? CurrentConsol)?.PackDepotAddress;
			}
			else if (JL_LastKnownTransitWarehouseStatus == FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched)
			{
				yield return Shipment?.ImportReleaseDepot;
				yield return (consol ?? CurrentConsol)?.UnpackDepotAddress;
			}
			else
			{
				yield return Shipment?.ExportReceivingDepot;
				yield return (consol ?? CurrentConsol)?.PackDepotAddress;
				yield return Shipment?.ImportReleaseDepot;
				yield return (consol ?? CurrentConsol)?.UnpackDepotAddress;
			}
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var propertiesToExcludeFromCloning =
				new List<string>(base.GetPropertiesToExcludeFromCloning())
				{
					JobPackLinesSchema.Constants.JL_PackLineId,
					JobPackLinesSchema.Constants.JL_OriginTransitWarehouseStatus,
					JobPackLinesSchema.Constants.JL_JL_OuterPackLine,
					JobPackLinesSchema.Constants.JL_OA_LastKnownTransitWarehouseAddress,
					JobPackLinesSchema.Constants.JL_LastKnownTransitWarehouseStatus,
					JobPackLinesSchema.Constants.JL_LastKnownTransitWarehouseStatusDateTime
				};

			return propertiesToExcludeFromCloning;
		}

		#region Saving

		public override void OnSaving()
		{
			base.OnSaving();

			PopulatePackLineIdIfNeeded(Factory, this);

			ShipmentLinkLogs.Add(string.Format(CultureInfo.InvariantCulture, "Saving, JL_JS: {0}", JL_JS));

			if (!IsDeleted && JL_JS.IsEmpty && (!Enterprise.ZArchitecture.Environment.Globals.IsTest
#if DEBUG
 || ForcePacklineWithoutShipmentErrorReporting
#endif
))
			{
				var log = string.Join(System.Environment.NewLine, ShipmentLinkLogs);

				var errorMessage = string.Format(CultureInfo.InvariantCulture, (NoResString)"Saving packline without a shipment. Please inform IL team. WI00070274.\r\n{0}", log);
				ErrorReporter.ReportOnce("PacklineWithoutShipment", errorMessage);
			}
		}

		public static void PopulatePackLineIdIfNeeded(BusinessObjectFactory factory, params PackLine[] allPackLines)
		{
			if (allPackLines == null)
			{
				return;
			}

			var packLines = allPackLines.Where(p => !p.IsDeleted && p.JL_PackLineId.IsEmpty).ToList();
			if (packLines == null || packLines.Count == 0)
			{
				return;
			}

			var fountain = Env.NumberFountains.JobPackLineId(GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID, Schema.JL_PackLineIdMaxLength);

			if (factory is IDbConnected connected)
			{
				var packIdList = fountain.GetNextsFormatted(connected.Connection, packLines.Count);
				for (var inx = 0; inx < packLines.Count; inx++)
				{
					var line = packLines[inx];
					line.JL_PackLineId = packIdList[inx];
					line.PkgPackageCollection.ForEach(p => p.KP_ExternalReference = packIdList[inx]);
				}
			}
		}

#if DEBUG

		[ThreadStatic]
		public static bool ForcePacklineWithoutShipmentErrorReporting;

#endif

		#endregion

		#region Saved

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				ShipmentLinkLogs.Clear();
				ShipmentLinkLogs.Add(string.Format(CultureInfo.InvariantCulture, "Saved, JL_JS: {0}", JL_JS));
				IsMarkingSecuredValid = false;
				JL_InspectionTypeCodeHasChanges = false;
			}
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new PackLineFetchStrategy(this);
		}

		#endregion

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			if ((kind & TestBusinessObjectKind.PopulateStrings) != 0)
			{
				JL_F3_NKPackType = Core.Constants.PkgUnit.Basket;
				JL_UnitOfDimension = Core.Constants.Length.Millimetres;
				JL_ActualWeightUQ = Core.Constants.Weight.Grams;
				JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;
			}

			if ((kind & TestBusinessObjectKind.PopulateRelatedObjects) != 0)
			{
				UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "2478", "c", "IMO").First().PK;
			}
		}

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new MyBusinessObjectTestDataHelper(this);
		}

		class MyBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
		{
			public MyBusinessObjectTestDataHelper(PackLine packLine)
			{
				this.packLine = packLine;
			}

			protected override void FillDependentCollectionWithData(IBusinessObjectCollection collection, TestBusinessObjectKind kind, PropertyDescriptor collectionProperty, PropertyDescriptor[] propertyPath)
			{
				if (packLine.JL_FreightMode == FreightConstants.OuterPackType || collectionProperty.Name != "Containers")
				{
					base.FillDependentCollectionWithData(collection, kind, collectionProperty, propertyPath);
				}
			}

			readonly PackLine packLine;
		}

#endif
		#endregion

		#region Related Business Objects

		#region JL_JL_OuterPackLine

		[List(nameof(ParentShipmentOuterPackLines))]
		public override ZGuid JL_JL_OuterPackLine
		{
			get => base.JL_JL_OuterPackLine;
			set
			{
				base.JL_JL_OuterPackLine = value;
				outerPackLine = null;
			}
		}

		public PackLine OuterPackLine => outerPackLine ?? (outerPackLine = Factory.Load<PackLine>(JL_JL_OuterPackLine));

		public ParentShipmentOuterPackLineCollection ParentShipmentOuterPackLines
		{
			get
			{
				if (shipmentOuterPackLines == null)
				{
					shipmentOuterPackLines = new ParentShipmentOuterPackLineCollection(Factory, Shipment);
				}

				return shipmentOuterPackLines;
			}
		}

		ParentShipmentOuterPackLineCollection shipmentOuterPackLines;
		PackLine outerPackLine;

		#endregion

		#region PackLocations

		[ChildEditable(true)]
		public PackLocationCollection PackLocations
		{
			get
			{
				if (fPackLocations == null)
				{
					fPackLocations = GetNewPackLocationCollection();
					RegisterEditableChildObject(fPackLocations);
					fPackLocations.Load();
				}
				return fPackLocations;
			}
		}

		protected virtual PackLocationCollection GetNewPackLocationCollection()
		{
			return new PackLocationCollection(this, Factory);
		}

		PackLocationCollection fPackLocations;

		#endregion

		#region Containers

		public CommonContainerManyToManyCollection Containers
		{
			get
			{
				if (fContainers == null)
				{
					fContainers = GetNewContainersCollection();
					fContainers.Load();
					fContainers.IsManagedForDataRefresh = true;
				}
				return fContainers;
			}
		}

		protected virtual CommonContainerManyToManyCollection GetNewContainersCollection()
		{
			return new CommonContainerManyToManyCollection(this);
		}

		CommonContainerManyToManyCollection fContainers;

		#endregion

		#region PkgPackageCollection

		public void UpdateTotalsFromPkgPackageCollection(PackLineConfirmDiscrepancyAction dialogResult = PackLineConfirmDiscrepancyAction.UpdatePacklineAndConfirm)
		{
			switch (JL_OriginTransitWarehouseStatus)
			{
				case FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies:
					ConfirmPacklineFromDiscrepancy(dialogResult);
					break;

				case FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.ShortShipped:
					GetInnerPackLinesFromShipment().DeleteAll();
					Delete();
					break;

				case FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus:
					if (PkgPackageCollection.Count > 0)
					{
						JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed;
						SetQuantityWeightAndVolumeFromPackageTotals();
					}

					break;
			}
		}

		void ConfirmPacklineFromDiscrepancy(PackLineConfirmDiscrepancyAction dialogResult)
		{
			switch (dialogResult)
			{
				case PackLineConfirmDiscrepancyAction.NoAction:
					return;

				case PackLineConfirmDiscrepancyAction.UpdatePacklineAndConfirm:
					using (Shipment.SuspendRedefaultingInspectionTypeCodes())
					using (Shipment.SetIsMarkingPackLinesAsSecuredAllowed())
					{
						UpdateAndSplitPacklineFromPkgPackageCollection();
					}
					Shipment.UpdateInspectionTypeFromPackLines();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Shipment.Logs.AddNew(Events.EditedARecord, $"Packline ID: {JL_PackLineId}, packline updated");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					break;

				case PackLineConfirmDiscrepancyAction.AcceptDiscrepancyAndConfirm:
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Shipment.Logs.AddNew(Events.EditedARecord, $"Packline ID: {JL_PackLineId}, discrepancy accepted");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					break;
			}

			JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed;
		}

		string CreateAGroupKey(PkgPackage pkgPackage, bool isOuterPackage, Func<Dictionary<ZString, ZString>> getPackLineIDDictionary)
		{
			var dims = new ZString[]
			{
				pkgPackage.KP_F3_NKPackType.ToUpperInvariant(),
				pkgPackage.KP_GoodsDescription.ToUpperInvariant(),
				pkgPackage.KP_MarksAndNumbers.ToUpperInvariant(),
				pkgPackage.KP_DimensionUQ.ToUpperInvariant(),
				pkgPackage.KP_WeightUQ.ToUpperInvariant(),
				pkgPackage.KP_VolumeUQ.ToUpperInvariant(),
				pkgPackage.KP_HSCode.ToUpperInvariant(),
				pkgPackage.KP_RH_NKCommodityCode.ToUpperInvariant(),
				pkgPackage.KP_Length.ToString(),
				pkgPackage.KP_Height.ToString(),
				pkgPackage.KP_Width.ToString(),
				pkgPackage.KP_RequiresTemperatureControl.ToString(),
				(pkgPackage.KP_RequiresTemperatureControl ? pkgPackage.KP_RequiredTemperatureUnit : (ZString)Constants.Temperature.Centigrade).ToUpperInvariant(),
				(pkgPackage.KP_RequiresTemperatureControl ? pkgPackage.KP_RequiredTemperatureMaximum : 0).ToString(),
				(pkgPackage.KP_RequiresTemperatureControl ? pkgPackage.KP_RequiredTemperatureMinimum : 0).ToString(),
				GetCalculatedInspectionTypeCode(this, pkgPackage).ToUpperInvariant(),
				(PkgPackageHandlingUnitDivotHelper.GetAllInnerPackagesViaDivots(pkgPackage).Count > 0).ToString(),
				GetOriginalPackLineID(isOuterPackage, pkgPackage, getPackLineIDDictionary).ToUpperInvariant(),
				GetPackageIsHighRisk(this, pkgPackage).ToString(),
				GetCalculatedAdditionalInspectionTypeCode(this, pkgPackage).ToUpperInvariant()
			};

			return string.Join("|", dims);
		}

		static Dictionary<ZString, ZString> CreatePackLineIDDictionary(IEnumerable<PkgPackage> packages)
		{
			return packages?
				.Where(p => p.KP_PackageID.IsEmpty)
				.ToDictionary(p => p.KP_ExternalReference, p => p.KP_PreviousPackLineID);
		}

		public static Dictionary<ZString, ZString> CreatePackLineIDDictionary(IEnumerable<UniversalPackingLine> packingLines)
		{
			return packingLines?
				.Where(p => p.ReferenceNumber.GetValueOrDefault().IsEmpty)
				.ToDictionary(p => p.PackingLineID.GetValueOrDefault(), p => p.PreviousPackingLineID.GetValueOrDefault());
		}

		static ZString GetOriginalPackLineID(bool isOuterPackage, PkgPackage package, Func<Dictionary<ZString, ZString>> getPackLineIDDictionary)
		{
			Argument.NotNull(package, nameof(package));

			return GetOriginalPackLineID(isOuterPackage, package.KP_PackageID, package.KP_ExternalReference, getPackLineIDDictionary);
		}

		public static ZString GetOriginalPackLineID(bool isOuterPackage, UniversalPackingLine packingLine, Func<Dictionary<ZString, ZString>> getPackLineIDDictionary)
		{
			Argument.NotNull(packingLine, nameof(packingLine));

			return GetOriginalPackLineID(isOuterPackage, packingLine.ReferenceNumber.GetValueOrDefault(), packingLine.PackingLineID.GetValueOrDefault(), getPackLineIDDictionary);
		}

		static ZString GetOriginalPackLineID(bool isOuterPackage, ZString packageID, ZString packLineID, Func<Dictionary<ZString, ZString>> getPackLineIDDictionary)
		{
			if (!isOuterPackage || !packageID.IsEmpty)
			{
				return ZString.Empty;
			}

			var packLineIDDictionary = getPackLineIDDictionary?.Invoke();
			var originalPackLineID = packLineID;

			if (packLineIDDictionary != null)
			{
				while (packLineIDDictionary.TryGetValue(originalPackLineID, out var previousPackLineID) && !previousPackLineID.IsEmpty && previousPackLineID != originalPackLineID)
				{
					originalPackLineID = previousPackLineID;
				}
			}
			return originalPackLineID;
		}

		IEnumerable<IGrouping<string, PkgPackage>> GetGroupedPackages(IList<PkgPackage> packagesToGroup, bool isOuterPackage)
		{
			Dictionary<ZString, ZString> packLineIDDictionary = null;
			var getPackLineIDDictionary = () => packLineIDDictionary ??= CreatePackLineIDDictionary(packagesToGroup);
			return packagesToGroup.GroupBy(p => CreateAGroupKey(p, isOuterPackage, getPackLineIDDictionary));
		}

		public void UpdateAndSplitPacklineFromPkgPackageCollection(Action<PackLine> preCallback = null, Action<PackLine> postCallback = null)
		{
			if (PkgPackageCollection.Count > 0)
			{
				var packGroups = GetGroupedPackages(PkgPackageCollection, isOuterPackage: true).ToArray();
				var shipment = GetParentShipment();
				preCallback?.Invoke(this);

				if (shipment != null)
				{
					foreach (var packGroup in packGroups.Skip(1))
					{
						var packLine = (PackLine)Clone();
						packLine.JL_PackLineId = ZString.Empty;
						packLine.JL_LastKnownTransitWarehouseStatus = JL_LastKnownTransitWarehouseStatus;
						packLine.JL_OA_LastKnownTransitWarehouseAddress = JL_OA_LastKnownTransitWarehouseAddress;
						packLine.JL_LastKnownTransitWarehouseStatusDateTime = JL_LastKnownTransitWarehouseStatusDateTime;
						shipment.OuterPackLines.Add(packLine);

						// OuterPackLineCollection#OnAdded can change the container of the added packline, so we must set it again here
						packLine.SetContainer(CurrentConsol, GetContainer(CurrentConsol));

						var wasPackLineUpdated = false;

						foreach (var package in packGroup)
						{
							if (!wasPackLineUpdated)
							{
								packLine.CopyValuesFromPackage(package);
								wasPackLineUpdated = true;
							}

							PkgPackageCollection.RemoveFromRelationship(package);
							packLine.PkgPackageCollection.Add(package);
						}

						packLine.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed;
						packLine.SetQuantityWeightAndVolumeFromPackageTotals();
						packLine.SetUNDGsFromPackageCollection();
						packLine.SetInnerPackLinesFromPackageCollection();
						postCallback?.Invoke(packLine);
					}
				}

				var firstGroup = packGroups.First();
				var firstPackage = firstGroup.First();

				CopyValuesFromPackage(firstPackage);
				SetQuantityWeightAndVolumeFromPackageTotals();
				SetUNDGsFromPackageCollection();
				SetInnerPackLinesFromPackageCollection();
				postCallback?.Invoke(this);
			}
		}

		public void CopyValuesFromPackage(PkgPackage package)
		{
			JL_F3_NKPackType = package.KP_F3_NKPackType;
			JL_Description = package.KP_GoodsDescription;
			JL_MarksAndNumbers = package.KP_MarksAndNumbers;
			JL_UnitOfDimension = package.KP_DimensionUQ;
			JL_ActualWeightUQ = package.KP_WeightUQ;
			JL_ActualVolumeUQ = package.KP_VolumeUQ;
			JL_HarmonisedCode = package.KP_HSCode;
			JL_RH_NKCommodityCode = package.KP_RH_NKCommodityCode;
			JL_Length = package.KP_Length;
			JL_Height = package.KP_Height;
			JL_Width = package.KP_Width;
			JL_ActualWeight = package.KP_Weight;
			JL_ActualVolume = package.KP_Volume;
			JL_RequiredTemperatureMinimum = package.KP_RequiresTemperatureControl ? package.KP_RequiredTemperatureMinimum : 0;
			JL_RequiredTemperatureMaximum = package.KP_RequiresTemperatureControl ? package.KP_RequiredTemperatureMaximum : 0;
			JL_RequiredTemperatureUnit = package.KP_RequiresTemperatureControl ? package.KP_RequiredTemperatureUnit : (ZString)Constants.Temperature.Centigrade;
			JL_RequiresTemperatureControl = package.KP_RequiresTemperatureControl;
			JL_InspectionTypeCode = GetCalculatedInspectionTypeCode(this, package);
			if (JL_FreightMode == FreightConstants.OuterPackType && package.KP_PackageID.IsEmpty)
			{
				JL_PackLineId = package.KP_ExternalReference;
			}
			JL_IsHighRisk = GetPackageIsHighRisk(this, package);
			JL_AdditionalInspectionTypeCode = GetCalculatedAdditionalInspectionTypeCode(this, package);
		}

		public static ZString GetCalculatedInspectionTypeCode(PackLine packLine, PkgPackage package)
		{
			return GetCalculatedInspectionTypeCode(packLine, (package?.ScreeningMethod).GetValueOrDefault());
		}

		public static ZString GetCalculatedInspectionTypeCode(PackLine packLine, UniversalPackingLine packingLine)
		{
			return GetCalculatedInspectionTypeCode(packLine, (packingLine?.ScreeningMethod).GetValueOrDefault());
		}

		static ZString GetCalculatedInspectionTypeCode(PackLine packLine, ZString screeningMethod)
		{
			if (packLine.Shipment.RequiresSecuredCargoFromWarehouse)
			{
				return screeningMethod.IsEmpty
					? FreightDataRegistry.AviationSecurity_Unknown_Code
					: screeningMethod;
			}

			return packLine.JL_InspectionTypeCodeInfo.ReadOnly || screeningMethod.IsEmpty
				? packLine.JL_InspectionTypeCode
				: screeningMethod;
		}

		public static ZBool GetPackageIsHighRisk(PackLine packLine, PkgPackage package)
		{
			return GetPackageIsHighRisk(packLine, (package?.IsHighRisk).GetValueOrDefault());
		}

		public static ZBool GetPackageIsHighRisk(PackLine packLine, UniversalPackingLine packingLine)
		{
			return GetPackageIsHighRisk(packLine, (packingLine?.IsHighRisk).GetValueOrDefault());
		}

		static ZBool GetPackageIsHighRisk(PackLine packLine, ZBool isHighRisk)
		{
			if (!packLine.JL_IsHighRiskInfo.ReadOnly)
			{
				return isHighRisk;
			}

			return packLine.JL_IsHighRisk;
		}

		public static ZString GetCalculatedAdditionalInspectionTypeCode(PackLine packLine, PkgPackage package)
		{
			return GetCalculatedAdditionalInspectionTypeCode(packLine, (package?.AdditionalScreeningMethod).GetValueOrDefault());
		}

		public static ZString GetCalculatedAdditionalInspectionTypeCode(PackLine packLine, UniversalPackingLine packingLine)
		{
			return GetCalculatedAdditionalInspectionTypeCode(packLine, (packingLine?.AviationSecurityAdditionalInspectionType?.Code).GetValueOrDefault());
		}

		static ZString GetCalculatedAdditionalInspectionTypeCode(PackLine packLine, ZString additionalScreeningMethod)
		{
			return packLine.JL_AdditionalInspectionTypeCodeInfo.ReadOnly || additionalScreeningMethod.IsEmpty
				? packLine.JL_AdditionalInspectionTypeCode
				: additionalScreeningMethod;
		}

		bool SuspendDefaultWeightAndDimensionsForPackType { get; set; }
		public void SetQuantityWeightAndVolumeFromPackageTotals() => SetQuantityWeightAndVolumeFromPackageTotals(PkgPackageCollection);

		void SetQuantityWeightAndVolumeFromPackageTotals(IEnumerable<PkgPackage> packages)
		{
			using (new DisposableAction(() => SuspendDefaultWeightAndDimensionsForPackType = true, () => SuspendDefaultWeightAndDimensionsForPackType = false))
			{
				JL_PackageCount = GetTotalQtyFromPkgPackageCollection(packages);
			}
			JL_ActualWeight = GetTotalWeightFromPkgPackageCollection(packages);
			JL_ActualVolume = GetTotalVolumeFromPkgPackageCollection(packages);
			JL_Damaged = GetTotalDamagedPacksFromPkgPackageCollection(packages);
		}

		public void SetUNDGsFromPackageCollection()
		{
			UNDGs.DeleteAll();

			foreach (var package in PkgPackageCollection)
			{
				SetUNDGsFromPackage(package);
				PkgPackageHandlingUnitDivotHelper.GetAllInnerPackagesViaDivots(package)
					.ForEach(innerPackage => SetUNDGsFromPackage(innerPackage, package));
			}
		}

		public void SetUNDGsFromPackage(PkgPackage package, PkgPackage parentHandlingUnitPackage = null, bool addOrUpdateUndg = true)
		{
			if (package.UNDGs == null || package.UNDGs.Count <= 0)
			{
				return;
			}

			foreach (var item in package.UNDGs)
			{
				var dg = GetExistingPacklineUNDG(item);

				if (addOrUpdateUndg)
				{
					if (dg != null)
					{
						using (dg.TempSetIsImportingData())
						{
							dg.DI_DGVolume += item.DI_DGVolume;
							dg.DI_DGWeight += item.DI_DGWeight;
							dg.DI_PackageCount += item.DI_PackageCount;
						}
					}
					else
					{
						dg = UNDGs.AddNew();
						using (dg.TempSetIsImportingData())
						{
							dg.DI_DG = item.DI_DG;
							dg.DI_IsCombustible = item.DI_IsCombustible;
							dg.DI_DGFlashPoint = item.DI_DGFlashPoint;
							dg.DI_IMOClass = item.DI_IMOClass;
							dg.DI_MPMarinePollutant = item.DI_MPMarinePollutant;
							dg.DI_TechnicalName = item.DI_TechnicalName;
							dg.DI_UnitOfVolume = item.DI_UnitOfVolume;
							dg.DI_UnitOfWeight = item.DI_UnitOfWeight;
							dg.DI_F3_NKPackType = item.DI_F3_NKPackType;
							dg.DI_OC_DGContact = item.DI_OC_DGContact;
							dg.DI_DGVolume = item.DI_DGVolume;
							dg.DI_DGWeight = item.DI_DGWeight;
							dg.DI_PackageCount = item.DI_PackageCount;
							dg.DI_IsLimitedQuantity = item.DI_IsLimitedQuantity;
							dg.DI_OverpackID = item.DI_OverpackID;
							dg.DI_HasOverpack = item.DI_HasOverpack;
							dg.DI_RadionuclideElementSuffix = item.DI_RadionuclideElementSuffix;
							dg.DI_RadionuclideElement = item.DI_RadionuclideElement;
							dg.DI_RadioactiveMaximumActivity = item.DI_RadioactiveMaximumActivity;
							dg.DI_RadioactiveMaximumActivityUnit = item.DI_RadioactiveMaximumActivityUnit;
							dg.DI_RadioactiveLabelCategory = item.DI_RadioactiveLabelCategory;
							dg.DI_RadioactiveTransportIndex = item.DI_RadioactiveTransportIndex;
							dg.DI_MaterialFormDescription = item.DI_MaterialFormDescription;
							dg.DI_PackingInstructionSection = item.DI_PackingInstructionSection;
							dg.DI_IsFissileExcepted = item.DI_IsFissileExcepted;
							dg.DI_IsExclusiveUse = item.DI_IsExclusiveUse;
							dg.DI_IsHighwayRouteControlledQuantity = item.DI_IsHighwayRouteControlledQuantity;
							dg.DI_PackingInstructionSection = item.DI_PackingInstructionSection;
						}
					}
				}

				if (dg != null && parentHandlingUnitPackage != null)
				{
					dg.DI_HasOverpack = true;
					dg.DI_OverpackID = parentHandlingUnitPackage.KP_PackageID.Left(AutoUNDGDataItem.Schema.DI_OverpackIDMaxLength);
				}
			}
		}

		UNDGDataItem GetExistingPacklineUNDG(UNDGDataItem undg)
		{
			return UNDGs.FirstOrDefault(x => x.DI_DG == undg.DI_DG
				&& x.DI_DGFlashPoint == undg.DI_DGFlashPoint
				&& x.DI_IsCombustible == undg.DI_IsCombustible
				&& x.DI_IMOClass == undg.DI_IMOClass
				&& x.DI_MPMarinePollutant == undg.DI_MPMarinePollutant
				&& x.DI_TechnicalName == undg.DI_TechnicalName
				&& x.DI_UnitOfVolume == undg.DI_UnitOfVolume
				&& x.DI_UnitOfWeight == undg.DI_UnitOfWeight
				&& x.DI_F3_NKPackType == undg.DI_F3_NKPackType
				&& x.DI_IsLimitedQuantity == undg.DI_IsLimitedQuantity
				&& x.DI_OC_DGContact == undg.DI_OC_DGContact
				&& x.DI_RadionuclideElementSuffix == undg.DI_RadionuclideElementSuffix
				&& x.DI_RadionuclideElement == undg.DI_RadionuclideElement
				&& x.DI_RadioactiveMaximumActivity == undg.DI_RadioactiveMaximumActivity
				&& x.DI_RadioactiveMaximumActivityUnit == undg.DI_RadioactiveMaximumActivityUnit
				&& x.DI_RadioactiveLabelCategory == undg.DI_RadioactiveLabelCategory
				&& x.DI_RadioactiveTransportIndex == undg.DI_RadioactiveTransportIndex
				&& x.DI_MaterialFormDescription == undg.DI_MaterialFormDescription
				&& x.DI_IsFissileExcepted == undg.DI_IsFissileExcepted
				&& x.DI_IsExclusiveUse == undg.DI_IsExclusiveUse
				&& x.DI_IsHighwayRouteControlledQuantity == undg.DI_IsHighwayRouteControlledQuantity
				&& x.DI_PackingInstructionSection == undg.DI_PackingInstructionSection);
		}

		IEnumerable<PackLine> GetInnerPackLinesFromShipment()
		{
			var shipment = GetParentShipment();
			return shipment == null ? Enumerable.Empty<PackLine>() : shipment.InnerPackLines.Where(l => l.JL_JL_OuterPackLine == PK);
		}

		public void SetInnerPackLinesFromPackageCollection()
		{
			var shipment = GetParentShipment();
			if (shipment == null)
			{
				return;
			}

			var innerPackages = PkgPackageCollection.SelectMany(PkgPackageHandlingUnitDivotHelper.GetAllInnerPackagesViaDivots).ToList();
			var innerPackageGroups = GetGroupedPackages(innerPackages, isOuterPackage: false);
			var innerPackLines = GetInnerPackLinesFromShipment().ToList();
			var processedInnerPackLines = new HashSet<PackLine>();
			foreach (var innerPackageGroup in innerPackageGroups)
			{
				var innerPackage = innerPackageGroup.First();
				var innerPackLine = innerPackLines.FirstOrDefault(l => DoesPackageHaveTheSameAttributes(innerPackage, l));
				if (innerPackLine == null)
				{
					innerPackLine = shipment.InnerPackLines.AddNew();
					innerPackLine.JL_JL_OuterPackLine = PK;
					innerPackLine.CopyValuesFromPackage(innerPackage);
				}
				innerPackLine.SetQuantityWeightAndVolumeFromPackageTotals(innerPackageGroup.ToList());
				processedInnerPackLines.Add(innerPackLine);
			}
			innerPackLines.Except(processedInnerPackLines).DeleteAll();
		}

		static bool DoesPackageHaveTheSameAttributes(PkgPackage package, PackLine packline)
		{
			return package.KP_RH_NKCommodityCode == packline.JL_RH_NKCommodityCode
				&& package.KP_HSCode == packline.JL_HarmonisedCode
				&& package.KP_DimensionUQ == packline.JL_UnitOfDimension
				&& package.KP_F3_NKPackType == packline.JL_F3_NKPackType
				&& package.KP_Height == packline.JL_Height
				&& package.KP_Length == packline.JL_Length
				&& package.KP_MarksAndNumbers == packline.JL_MarksAndNumbers
				&& package.KP_VolumeUQ == packline.JL_ActualVolumeUQ
				&& package.KP_WeightUQ == packline.JL_ActualWeightUQ
				&& package.KP_Width == packline.JL_Width
				&& GetPackageInfoHelper.GetCleanSingleLineText(package.KP_GoodsDescription) == GetPackageInfoHelper.GetCleanSingleLineText(packline.JL_Description)
				&& package.KP_RequiresTemperatureControl == packline.JL_RequiresTemperatureControl
				&& (package.KP_RequiresTemperatureControl ? package.KP_RequiredTemperatureMinimum : 0) == packline.JL_RequiredTemperatureMinimum
				&& (package.KP_RequiresTemperatureControl ? package.KP_RequiredTemperatureMaximum : 0) == packline.JL_RequiredTemperatureMaximum
				&& (package.KP_RequiresTemperatureControl ? package.KP_RequiredTemperatureUnit : (ZString)Constants.Temperature.Centigrade) == packline.JL_RequiredTemperatureUnit
				&& GetCalculatedInspectionTypeCode(packline, package) == packline.JL_InspectionTypeCode
				&& GetPackageIsHighRisk(packline, package) == packline.JL_IsHighRisk
				&& GetCalculatedAdditionalInspectionTypeCode(packline, package) == packline.JL_AdditionalInspectionTypeCode;
		}

		public PkgPackageCollection PkgPackageCollection
		{
			get
			{
				if (pkgPackageCollection == null)
				{
					pkgPackageCollection = GetPackageCollectionCore();
				}

				return pkgPackageCollection;
			}
		}
		PkgPackageCollection pkgPackageCollection;

		protected virtual PkgPackageCollection GetPackageCollectionCore() => new (this);

		public ZString PkgPackageCollection_PackType
		{
			get
			{
				if (PkgPackageCollection.Count > 0)
				{
					var firstPkgPackageType = PkgPackageCollection[0].KP_F3_NKPackType;

					return PkgPackageCollection.All(pkg => pkg.KP_F3_NKPackType == firstPkgPackageType)
						? firstPkgPackageType
						: (ZString)Constants.PkgUnit.Package;
				}

				return Constants.PkgUnit.Package;
			}
		}

		public ZInt PkgPackageCollection_TotalQty => GetTotalQtyFromPkgPackageCollection(PkgPackageCollection);

		ZInt GetTotalQtyFromPkgPackageCollection(IEnumerable<PkgPackage> packages) => packages.Sum(pkg => pkg.KP_PackageQty);

		public ZDecimal PkgPackageCollection_TotalWeight => GetTotalWeightFromPkgPackageCollection(PkgPackageCollection);

		ZDecimal GetTotalWeightFromPkgPackageCollection(IEnumerable<PkgPackage> packages)
		{
			var pkgPackageWeightSum = packages.Sum(pkg => Constants.Weight.ConvertSafe(pkg.KP_Weight, pkg.KP_WeightUQ, JL_ActualWeightUQ, applyDefaultRounding: false));
			var roundedValue = this.GetRoundedValue(JobPackLinesSchema.JL_ActualWeight, JL_ActualWeightInfo, pkgPackageWeightSum);

			return roundedValue;
		}

		public ZDecimal PkgPackageCollection_TotalVolume => GetTotalVolumeFromPkgPackageCollection(PkgPackageCollection);

		ZDecimal GetTotalVolumeFromPkgPackageCollection(IEnumerable<PkgPackage> packages)
		{
			var pkgPackageVolumeSum = packages.Sum(pkg => Constants.Volume.ConvertSafe(pkg.KP_Volume, pkg.KP_VolumeUQ, JL_ActualVolumeUQ, applyDefaultRounding: false));
			var roundedValue = this.GetRoundedValue(JobPackLinesSchema.JL_ActualVolume, JL_ActualVolumeInfo, pkgPackageVolumeSum);

			return roundedValue;
		}

		public ZInt PkgPackageCollection_TotalDamagedPacks => GetTotalDamagedPacksFromPkgPackageCollection(PkgPackageCollection);

		ZInt GetTotalDamagedPacksFromPkgPackageCollection(IEnumerable<PkgPackage> packages) => packages.Sum(pkg => pkg.KP_IsDamaged ? pkg.KP_PackageQty : ZInt.Zero);

		#endregion

		#region Parent Collection

		internal OuterPackLineCollection ParentCollection
		{
			get
			{
				if (parentCollection == null)
				{
					parentCollection = ((IBusinessObjectInternals)this).ParentCollections
					.OfType<OuterPackLineCollection>()
					.FirstOrDefault(collection => collection.Master.PK == JL_JS);

					if (parentCollection == null)
					{
						var shipment = Factory.Load<CommonShipment>(JL_JS);
						if (shipment != null)
						{
							parentCollection = shipment.OuterPackLines;
						}
					}
				}
				return parentCollection;
			}
		}
		OuterPackLineCollection parentCollection;

		#endregion

		#region Shipment

		public CommonShipment Shipment
		{
			get { return IsDeleted ? null : GetParentShipment(); }
		}

		protected virtual CommonShipment GetParentShipment()
		{
			CommonShipment result = null;

			foreach (BusinessObjectCollection collection in ((IBusinessObjectInternals)this).ParentCollections)
			{
				if (collection is IDependentBusinessObjectCollection dependentCollection)
				{
					CommonShipment shipment = dependentCollection.Master as CommonShipment;
					if (shipment != null)
					{
						result = shipment;
					}
				}
			}

			if (result == null || (!JL_JS.IsEmpty && result.PK != JL_JS))
			{
				result = Factory.Load<CommonShipment>(JL_JS);
			}
			return result;
		}

		#endregion

		#region UNDGs

		public void DeactivateActiveBusinessObjectCollections()
		{
			if (fUNDGs != null)
			{
				fUNDGs.Deactivate();
			}
			if (deliveryConfirms != null)
			{
				deliveryConfirms.Deactivate();
			}
			if (pickupConfirms != null)
			{
				pickupConfirms.Deactivate();
			}
		}

		[ChildEditable(true)]
		public UNDGDataItemCollection UNDGs
		{
			get
			{
				if (fUNDGs == null)
				{
					fUNDGs = GetNewUNDGs();
					RegisterEditableChildObject(fUNDGs);
				}
				return fUNDGs;
			}
		}

		UNDGDataItemCollection fUNDGs;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;

		protected virtual UNDGDataItemCollection GetNewUNDGs()
		{
			return new UNDGDataItemCollection(this);
		}
		#endregion

		#region Pickup/Delivery Confirmations

		internal CommonPickupDeliveryConfirmCollection PickupConfirms
		{
			get { return pickupConfirms ?? (pickupConfirms = GetConfirmCollectionFromPickupDeliveryType(Constants.PickupDeliveryConfirmTypes.OriginPickup)); }
		}
		CommonPickupDeliveryConfirmCollection pickupConfirms;

		internal CommonPickupDeliveryConfirmCollection DeliveryConfirms
		{
			get { return deliveryConfirms ?? (deliveryConfirms = GetConfirmCollectionFromPickupDeliveryType(Constants.PickupDeliveryConfirmTypes.DestinationDelivery)); }
		}
		CommonPickupDeliveryConfirmCollection deliveryConfirms;

		#region CFS

		protected CommonPickupDeliveryConfirmCollection OriginCFSArrivalConfirms
		{
			get { return originCFSArrivalConfirms ?? (originCFSArrivalConfirms = GetConfirmCollectionFromPickupDeliveryType(Constants.PickupDeliveryConfirmTypes.OriginCFSArrival)); }
		}
		CommonPickupDeliveryConfirmCollection originCFSArrivalConfirms;

		CommonPickupDeliveryConfirmCollection OriginCFSDispatchConfirms
		{
			get { return originCFSDispatchConfirms ?? (originCFSDispatchConfirms = GetConfirmCollectionFromPickupDeliveryType(Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture)); }
		}
		CommonPickupDeliveryConfirmCollection originCFSDispatchConfirms;

		CommonPickupDeliveryConfirmCollection DestinationCFSArrivalConfirms
		{
			get { return destinationCFSArrivalConfirms ?? (destinationCFSArrivalConfirms = GetConfirmCollectionFromPickupDeliveryType(Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival)); }
		}
		CommonPickupDeliveryConfirmCollection destinationCFSArrivalConfirms;

		CommonPickupDeliveryConfirmCollection DestinationCFSDispatchConfirms
		{
			get { return destinationCFSDispatchConfirms ?? (destinationCFSDispatchConfirms = GetConfirmCollectionFromPickupDeliveryType(Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture)); }
		}
		CommonPickupDeliveryConfirmCollection destinationCFSDispatchConfirms;

		CommonPickupDeliveryConfirmCollection GetConfirmCollectionFromPickupDeliveryType(string pickupDeliveryType)
		{
			var query = new ZQuery(JobPickupDeliveryConfirmSchema.EU_PickupDeliveryType, pickupDeliveryType);
			var relationship = new CommonPickupDeliveryConfirmCollection.CommonPickupDeliveryConfirmRelationship(this, typeof(CommonPickupDeliveryConfirm), typeof(CommonConfirmDivot), query, JobTransportLegPackLineDivotSchema.J8_JL, JobTransportLegPackLineDivotSchema.J8_EU_PickupDeliverConfirm);

			return new CommonPickupDeliveryConfirmCollection(Factory, relationship);
		}

		#endregion

		[ChildEditable(true)]
		public CommonPickupDeliveryConfirmDivotCollection ConfirmDivots
		{
			get
			{
				if (confirmDivots == null)
				{
					confirmDivots = new CommonPickupDeliveryConfirmDivotCollection(this);
					RegisterEditableChildObject(confirmDivots);
				}
				return confirmDivots;
			}
		}
		CommonPickupDeliveryConfirmDivotCollection confirmDivots;

		#region Confirmed Packages

		public ZInt PackagesConfirmed_PickedupFromConsignor
		{
			get { return PackagesConfirmed(PickupConfirms); }
		}

		public ZInt PackagesConfirmed_DeliveredToConsignee
		{
			get { return PackagesConfirmed(DeliveryConfirms); }
		}

		#region Used in CFS

		public ZInt PackagesConfirmed_DeliveredToOriginCFS
		{
			get { return PackagesConfirmed(OriginCFSArrivalConfirms); }
		}

		public ZInt PackagesConfirmed_DispatchedFromOriginCFS
		{
			get { return PackagesConfirmed(OriginCFSDispatchConfirms); }
		}

		public ZInt PackagesConfirmed_DeliveredToDestinationCFS
		{
			get { return PackagesConfirmed(DestinationCFSArrivalConfirms); }
		}

		public ZInt PackagesConfirmed_DispatchedFromDestinationCFS
		{
			get { return PackagesConfirmed(DestinationCFSDispatchConfirms); }
		}

		#endregion

		public ZInt PackagesConfirmed(CommonPickupDeliveryConfirmCollection confirms)
		{
			int result = 0;

			foreach (CommonPickupDeliveryConfirm confirm in confirms)
			{
				var divot = confirm.Divots.FirstOrDefault(d => d.J8_JL == PK);
				if (divot != null)
				{
					result += divot.J8_PackagesDelivered;
				}
			}

			return result;
		}

		#endregion

		#region Confirmed Weight

		public ZDecimal ConfirmedWeight(string pickupDeliveryType)
		{
			switch (pickupDeliveryType)
			{
				case Constants.PickupDeliveryConfirmTypes.OriginPickup:
					return ConfirmedWeight(PickupConfirms);
				case Constants.PickupDeliveryConfirmTypes.DestinationDelivery:
					return ConfirmedWeight(DeliveryConfirms);
				case Constants.PickupDeliveryConfirmTypes.OriginCFSArrival:
					return ConfirmedWeight(OriginCFSArrivalConfirms);
				case Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture:
					return ConfirmedWeight(OriginCFSDispatchConfirms);
				case Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival:
					return ConfirmedWeight(DestinationCFSArrivalConfirms);
				case Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture:
					return ConfirmedWeight(DestinationCFSDispatchConfirms);
				case "":
					return ZDecimal.Zero;
				default:
					throw new NotSupportedException(pickupDeliveryType + " Pickup/Delivery Confirm Type is not supported.");
			}
		}

		public ZDecimal ConfirmedWeight(CommonPickupDeliveryConfirmCollection confirms)
		{
			ZDecimal result = 0;

			foreach (CommonPickupDeliveryConfirm confirm in confirms)
			{
				ZQuery pivotQuery = new ZQuery(JobTransportLegPackLineDivotSchema.J8_JL, PK);
				pivotQuery.AddToFilter(JobTransportLegPackLineDivotSchema.J8_EU_PickupDeliverConfirm, confirm.PK);
				CommonConfirmDivot divot = Factory.LoadTop1<CommonConfirmDivot>(pivotQuery);
				result += divot.J8_DeliveryWeight;
			}

			return result;
		}

		#endregion

		#region Confirmed Volume

		public ZDecimal ConfirmedVolume(string pickupDeliveryType)
		{
			switch (pickupDeliveryType)
			{
				case Constants.PickupDeliveryConfirmTypes.OriginPickup:
					return ConfirmedVolume(PickupConfirms);
				case Constants.PickupDeliveryConfirmTypes.DestinationDelivery:
					return ConfirmedVolume(DeliveryConfirms);
				case Constants.PickupDeliveryConfirmTypes.OriginCFSArrival:
					return ConfirmedVolume(OriginCFSArrivalConfirms);
				case Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture:
					return ConfirmedVolume(OriginCFSDispatchConfirms);
				case Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival:
					return ConfirmedVolume(DestinationCFSArrivalConfirms);
				case Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture:
					return ConfirmedVolume(DestinationCFSDispatchConfirms);
				case "":
					return ZDecimal.Zero;
				default:
					throw new NotSupportedException(pickupDeliveryType + " Pickup/Delivery Confirm Type is not supported.");
			}
		}

		public ZDecimal ConfirmedVolume(CommonPickupDeliveryConfirmCollection confirms)
		{
			ZDecimal result = 0;

			foreach (CommonPickupDeliveryConfirm confirm in confirms)
			{
				ZQuery pivotQuery = new ZQuery(JobTransportLegPackLineDivotSchema.J8_JL, PK);
				pivotQuery.AddToFilter(JobTransportLegPackLineDivotSchema.J8_EU_PickupDeliverConfirm, confirm.PK);
				CommonConfirmDivot divot = Factory.LoadTop1<CommonConfirmDivot>(pivotQuery);
				result += divot.J8_DeliveryVolume;
			}

			return result;
		}

		#endregion

		#endregion

		#region HarmonisedCodes

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(JobPackLineHarmonisedCodeSchema.Constants.TableName, JobPackLineHarmonisedCodeSchema.Constants.JLH_JL)]
		public JobPackLineHarmonisedCodeCollection HarmonisedCodes
		{
			get
			{
				if (harmonisedCodes == null)
				{
					harmonisedCodes = new JobPackLineHarmonisedCodeCollection(this);
					RegisterEditableChildObject(harmonisedCodes);
				}

				return harmonisedCodes;
			}
		}
		JobPackLineHarmonisedCodeCollection harmonisedCodes;

		#endregion

		#endregion

		#region Properties

		#region  JL_OA_LastKnownTransitWarehouseAddress

		string LastKnownTransitWarehouseAddressInvalidTrace = string.Empty;

		public override ZGuid JL_OA_LastKnownTransitWarehouseAddress
		{
			get { return base.JL_OA_LastKnownTransitWarehouseAddress; }
			set
			{
				base.JL_OA_LastKnownTransitWarehouseAddress = value;

				if (LastKnownTransitWarehouseAddress == null)
				{
					LastKnownTransitWarehouseAddressInvalidTrace = System.Environment.StackTrace;
				}
			}
		}

		#endregion

		#region JL_RH_NKCommodityCode

		[List("RefCommodity_List")]
		public override ZString JL_RH_NKCommodityCode
		{
			get { return base.JL_RH_NKCommodityCode; }
			set
			{
				base.JL_RH_NKCommodityCode = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateJL_RequiredTemperatureMaximum();
					Validation.ValidateJL_RequiredTemperatureMinimum();
				}
			}
		}

		public RefCommodityCode Commodity
		{
			get => Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, JL_RH_NKCommodityCode);
		}

		#endregion

		#region JL_RN_NKOrigin

		[List("CountryOfOrigin_List")]
		public override ZString JL_RN_NKOrigin
		{
			get { return base.JL_RN_NKOrigin; }
			set { base.JL_RN_NKOrigin = value; }
		}

		#endregion

		#region JL_F3_NKPackType

		[List("JL_F3_NKPackType_List")]
		public override ZString JL_F3_NKPackType
		{
			get { return base.JL_F3_NKPackType; }
			set
			{
				base.JL_F3_NKPackType = value;
				if (!JL_F3_NKPackTypeInfo.HasErrors())
				{
					SetupDefaultWeightAndDimensionsForPackType();
				}
			}
		}

		public override ZPropertyInfo JL_F3_NKPackTypeInfo
		{
			get { return PrefixInfo(base.JL_F3_NKPackTypeInfo); }
		}

		void SetupDefaultWeightAndDimensionsForPackType()
		{
			OrgBuyerSupplierLinkPackPivot packageDetails = null;

			if (Shipment != null)
			{
				var buyerSupplierLink = OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(Shipment.Consignor, Shipment.Consignee, Shipment.JS_RL_NKDestination.Left(2));
				if (buyerSupplierLink != null)
				{
					packageDetails = buyerSupplierLink.PackPivots.GetDetailsForPackType(JL_F3_NKPackType);
				}
			}

			var packType = Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, JL_F3_NKPackType);
			this.SetupDefaultWeightAndDimensions(packageDetails, packType, JL_PackageCount);
		}

		#endregion

		#region JL_JS

		[RelatedBusinessObject("Shipment")]
		public override ZGuid JL_JS
		{
			get { return base.JL_JS; }
			set
			{
				if (JL_JS != value)
				{
					ShipmentLinkLogs.Add(string.Format(CultureInfo.InvariantCulture, "Setting new JL_JS: {0} -> {1}", JL_JS, value));
					ShipmentLinkLogs.Add(System.Environment.StackTrace);

					base.JL_JS = value;
					if (IsOuterPackType)
					{
						PackLocations.MarkAsNeedingValidation();
					}

					if (!value.IsEmpty)
					{
						stackTraceForMissingDivotErrorReport = string.Empty;
					}
				}
			}
		}

		#endregion

		#region JL_PackageCount

		public override ZInt JL_PackageCount
		{
			get { return base.JL_PackageCount; }
			set
			{
				if (JL_PackageCount != value)
				{
					base.JL_PackageCount = value;

					SetJL_ActualVolume();

					PackLocations.MarkAsNeedingValidation();
					ConfirmDivots.MarkAsNeedingValidation();

					if (!SuspendDefaultWeightAndDimensionsForPackType && !JL_F3_NKPackTypeInfo.HasErrors())
					{
						SetupDefaultWeightAndDimensionsForPackType();
					}

					if (Shipment != null)
					{
						Shipment.MarkAsNeedingValidation();
						Shipment.JS_OuterPacksInfo.RefreshBinding();
						Shipment.TotalInnerPackLinePackagesInfo.RefreshBinding();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJL_F3_NKPackType();
						Validation.ValidateJL_RefNumber();
					}
				}
			}
		}

		#endregion

		#region JL_ActualVolume

		[MeasureUnit(Schema.JL_ActualVolumeUQ, MeasureUnitType.Volume)]
		public override ZDecimal JL_ActualVolume
		{
			get { return base.JL_ActualVolume; }
			set
			{
				var roundedValue = this.GetRoundedValue(JobPackLinesSchema.JL_ActualVolume, JL_ActualVolumeInfo, value);
				if (roundedValue != base.JL_ActualVolume)
				{
					base.JL_ActualVolume = roundedValue;

					if (Shipment != null)
					{
						Shipment.MarkAsNeedingValidation();
					}

					if (IsOuterPackType)
					{
						ConfirmDivots.MarkAsNeedingValidation();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJL_ActualVolumeUQ();
					}
				}
			}
		}

		#endregion

		#region JL_ActualVolumeUQ
		[List("JL_ActualVolumeUQ_List")]
		public override ZString JL_ActualVolumeUQ
		{
			get { return base.JL_ActualVolumeUQ; }
			set
			{
				base.JL_ActualVolumeUQ = value;

				this.SetRoundedValue(JobPackLinesSchema.JL_ActualVolume, JL_ActualVolumeInfo);
				this.SetRoundedValue(JobPackLinesSchema.JL_OutturnedVolume, JL_OutturnedVolumeInfo);

				SetJL_ActualVolume();
				SetJL_OutturnedVolume();

				if (Shipment != null)
				{
					Shipment.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region JL_ActualWeight

		[MeasureUnit(Schema.JL_ActualWeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal JL_ActualWeight
		{
			get { return base.JL_ActualWeight; }
			set
			{
				var roundedValue = this.GetRoundedValue(JobPackLinesSchema.JL_ActualWeight, JL_ActualWeightInfo, value);
				if (roundedValue != base.JL_ActualWeight)
				{
					base.JL_ActualWeight = roundedValue;

					if (IsOuterPackType)
					{
						ConfirmDivots.MarkAsNeedingValidation();
						UpdateGrossWeightForAllContainers();
					}

					if (Shipment != null && IsInnerPackType)
					{
						Shipment.TotalInnerPackLineWeightInfo.RefreshBinding();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJL_ActualWeightUQ();
					}
				}
			}
		}

		#endregion

		#region JL_ActualWeightUQ

		[List("JL_ActualWeightUQ_List")]
		public override ZString JL_ActualWeightUQ
		{
			get { return base.JL_ActualWeightUQ; }
			set
			{
				if (JL_ActualWeightUQ != value)
				{
					base.JL_ActualWeightUQ = value;

					this.SetRoundedValue(JobPackLinesSchema.JL_ActualWeight, JL_ActualWeightInfo);
					this.SetRoundedValue(JobPackLinesSchema.JL_OutturnedWeight,JL_OutturnedWeightInfo);

					if (JL_ActualWeight != 0)
					{
						UpdateGrossWeightForAllContainers();
					}

					if (Shipment != null)
					{
						if (JL_FreightMode == FreightConstants.OuterPackType)
						{
							Shipment.TotalOuterPacksWeightInfo.RefreshBinding();
						}
						else
						{
							Shipment.TotalInnerPackLineWeightInfo.RefreshBinding();
						}
					}
				}
			}
		}

		#endregion

		#region JL_LoadingMeters

		protected bool JL_LoadingMeters_ReadOnly
		{
			get { return Shipment == null || !Shipment.IsRoadLoadingMetersEnabled; }
		}

		#endregion

		#region JL_Length

		[MeasureUnit(Schema.JL_UnitOfDimension, MeasureUnitType.Length)]
		public override ZDecimal JL_Length
		{
			get { return base.JL_Length; }
			set
			{
				var roundedValue = this.GetRoundedValue(JobPackLinesSchema.JL_Length, JL_LengthInfo, value);
				if (roundedValue != base.JL_Length)
				{
					base.JL_Length = roundedValue;
					SetJL_ActualVolume();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJL_UnitOfDimension();
					}
				}
			}
		}

		#endregion

		#region JL_Height

		[MeasureUnit(Schema.JL_UnitOfDimension, MeasureUnitType.Length)]
		public override ZDecimal JL_Height
		{
			get { return base.JL_Height; }
			set
			{
				var roundedValue = this.GetRoundedValue(JobPackLinesSchema.JL_Height, JL_HeightInfo, value);
				if (roundedValue != base.JL_Height)
				{
					base.JL_Height = roundedValue;
					SetJL_ActualVolume();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJL_UnitOfDimension();
					}
				}
			}
		}

		#endregion

		#region JL_Width

		[MeasureUnit(Schema.JL_UnitOfDimension, MeasureUnitType.Length)]
		public override ZDecimal JL_Width
		{
			get
			{
				return base.JL_Width;
			}
			set
			{
				var roundedValue = this.GetRoundedValue(JobPackLinesSchema.JL_Width, JL_WidthInfo, value);
				if (roundedValue != base.JL_Width)
				{
					base.JL_Width = roundedValue;
					SetJL_ActualVolume();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJL_UnitOfDimension();
					}
				}
			}
		}

		#endregion

		#region JL_UnitOfDimension

		[List("JL_UnitOfDimension_List")]
		public override ZString JL_UnitOfDimension
		{
			get
			{
				return base.JL_UnitOfDimension;
			}
			set
			{
				base.JL_UnitOfDimension = value;
				SetJL_ActualVolume();
				SetJL_OutturnedVolume();
			}
		}

		#endregion

		#region JL_Outturn

		public override ZInt JL_Outturn
		{
			get { return base.JL_Outturn; }
			set
			{
				if (value != JL_Outturn)
				{
					base.JL_Outturn = value;
					PackLocations.MarkAsNeedingValidation();
					DefaultOutturnFromManifested();
					SetJL_OutturnedVolume();
					ConfirmDivots.MarkAsNeedingValidation();
				}
			}
		}

		void DefaultOutturnFromManifested()
		{
			if (IsOutturned)
			{
				if (JL_OutturnedWeight == 0)
				{
					JL_OutturnedWeight = JL_ActualWeight;
				}
				if (JL_OutturnedVolume == 0)
				{
					JL_OutturnedVolume = JL_ActualVolume;
				}
				if (JL_OutturnedLength == 0)
				{
					JL_OutturnedLength = JL_Length;
				}
				if (JL_OutturnedWidth == 0)
				{
					JL_OutturnedWidth = JL_Width;
				}
				if (JL_OutturnedHeight == 0)
				{
					JL_OutturnedHeight = JL_Height;
				}
			}
			else
			{
				JL_OutturnedHeight = 0;
				JL_OutturnedWidth = 0;
				JL_OutturnedLength = 0;
				JL_OutturnedWeight = 0;
				JL_OutturnedVolume = 0;
			}
		}

		/// <summary>
		/// 1. Import : Containerised (has import container) : Container is unpacked : Outturn > 0
		/// 2. Neither Import/Export : No CommonShipment : Outturn > 0
		/// 3. CommonShipment is Export : Outturn > 0
		/// 4. No Manifested Package Count, but there is outturn
		/// 5. Cross trade shipments (foreign -> foreign)
		/// </summary>
		public virtual bool UseOutturn
		{
			get
			{
				return JL_Outturn != 0 &&
					(JL_Calc_FirstImportContainer != null
					|| Shipment == null
					|| (Shipment != null && Shipment.IsExport())
					|| JL_PackageCount == 0
					|| (Shipment != null && Shipment.IsCrossTrade()));
			}
		}

		#endregion

		#region JL_OutturnedLength

		[MeasureUnit(Schema.JL_UnitOfDimension, MeasureUnitType.Length, "ExposeOutturn")]
		public override ZDecimal JL_OutturnedLength
		{
			get { return base.JL_OutturnedLength; }
			set
			{
				var roundedValue = this.GetRoundedValue(JobPackLinesSchema.JL_OutturnedLength, JL_OutturnedLengthInfo, value);
				if (roundedValue != base.JL_OutturnedLength)
				{
					base.JL_OutturnedLength = roundedValue;
					SetJL_OutturnedVolume();
				}
			}
		}

		#endregion

		#region JL_OutturnedHeight

		[MeasureUnit(Schema.JL_UnitOfDimension, MeasureUnitType.Length, "ExposeOutturn")]
		public override ZDecimal JL_OutturnedHeight
		{
			get { return base.JL_OutturnedHeight; }
			set
			{
				var roundedValue = this.GetRoundedValue(JobPackLinesSchema.JL_OutturnedHeight, JL_OutturnedHeightInfo, value);
				if (roundedValue != base.JL_OutturnedHeight)
				{
					base.JL_OutturnedHeight = roundedValue;
					SetJL_OutturnedVolume();
				}
			}
		}

		#endregion

		#region JL_OutturnedWidth

		[MeasureUnit(Schema.JL_UnitOfDimension, MeasureUnitType.Length, "ExposeOutturn")]
		public override ZDecimal JL_OutturnedWidth
		{
			get { return base.JL_OutturnedWidth; }
			set
			{
				var roundedValue = this.GetRoundedValue(JobPackLinesSchema.JL_OutturnedWidth, JL_OutturnedWidthInfo, value);
				if (roundedValue != base.JL_OutturnedWidth)
				{
					base.JL_OutturnedWidth = roundedValue;
					SetJL_OutturnedVolume();
				}
			}
		}

		#endregion

		#region JL_PackLineId

		[ReadOnly(true)]
		public override ZString JL_PackLineId
		{
			get { return base.JL_PackLineId; }
			set { base.JL_PackLineId = value; }
		}

		#endregion

		#region JL_RequiredTemperatureMinimum

		public override ZDecimal JL_RequiredTemperatureMinimum
		{
			get => base.JL_RequiredTemperatureMinimum;
			set
			{
				if (base.JL_RequiredTemperatureMinimum != value)
				{
					base.JL_RequiredTemperatureMinimum = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJL_RequiredTemperatureMaximum();
						Validation.ValidateJL_RequiresTemperatureControl();
					}
				}
			}
		}

		#endregion

		#region JL_RequiredTemperatureMaximum

		public override ZDecimal JL_RequiredTemperatureMaximum
		{
			get => base.JL_RequiredTemperatureMaximum;
			set
			{
				if (base.JL_RequiredTemperatureMaximum != value)
				{
					base.JL_RequiredTemperatureMaximum = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJL_RequiredTemperatureMinimum();
						Validation.ValidateJL_RequiresTemperatureControl();
					}
				}
			}
		}

		#endregion

		#region JL_RequiredTemperatureUnit

		public override ZString JL_RequiredTemperatureUnit
		{
			get => base.JL_RequiredTemperatureUnit;
			set
			{
				if (base.JL_RequiredTemperatureUnit != value)
				{
					base.JL_RequiredTemperatureUnit = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJL_RequiredTemperatureMaximum();
						Validation.ValidateJL_RequiredTemperatureMinimum();
						Validation.ValidateJL_RequiresTemperatureControl();
					}
				}
			}
		}

		#endregion

		public bool ExposeOutturn
		{
			get { return Shipment != null && Shipment.JS_IsForwardRegistered; }
		}

		#region HasHazardous

		public ZBool HasHazardous
		{
			get
			{
				return JL_RH_NKCommodityCode == "HAZ" ||
					   JL_RH_NKCommodityCode == "HAZD" ||
					   JL_RH_NKCommodityCode == "MTHZ" ||
					   UNDGs.Count > 0;
			}
		}

		#endregion

		#region CurrentConsol

		public CommonConsol CurrentConsol
		{
			get
			{
				if (!currentConsolHasBeenSet && currentConsol == null && ParentCollection != null)
				{
					CurrentConsol = ParentCollection.CurrentConsol;
				}

				return currentConsol;
			}
			set
			{
				if (currentConsol != value &&
					(value == null || Shipment?.Consols?.GetRelationshipBusinessObject(value) != null))
				{
					var oldValue = currentConsol;
					currentConsol = value;
					JL_JCInfo.RefreshBinding();
					currentConsolHasBeenSet = true;
					CurrentConsolSetterStackTrace = string.Format(
						(NoResString)"PackLine.CurrentConsol is set to {0}.",
						value == null ? "null" : $"Consol {value.JK_UniqueConsignRef} {{{value.PK}}}");
					CurrentConsolChanged?.Invoke(oldValue, value);
				}
			}
		}
		CommonConsol currentConsol;
		bool currentConsolHasBeenSet;
		internal string CurrentConsolSetterStackTrace;

		public delegate void DelegateForCurrentConsolChanged(CommonConsol oldValue, CommonConsol newValue);
		public DelegateForCurrentConsolChanged CurrentConsolChanged { get; set; }

		#endregion

		public virtual bool PerformWarehouseQuantityValidation
		{
			get { return true; }
		}

		public bool IsInnerPackType
		{
			get { return JL_FreightMode == FreightConstants.InnerPackType; }
		}

		public bool IsOuterPackType
		{
			get { return JL_FreightMode == FreightConstants.OuterPackType; }
		}

		public bool WarnNotErrorOnLocationTotals { get; set; }

		protected internal bool IsOutturned
		{
			get { return (JL_Outturn > 0); }
		}

		#region JL_CalcContainerNumber

		public const string JL_Calc_ContainerNumberName = "JL_Calc_ContainerNumber";
		protected ZString fJL_Calc_ContainerNumber = FreightConstants.AllContainers;

		public CommonContainer GetContainer(CommonConsol consol)
		{
			CommonContainer result = null;

			if (consol != null)
			{
				result = Containers
					.Cast<CommonContainer>()
					.FirstOrDefault(container => container.JC_JK == consol.PK);
			}

			return result;
		}

		public CommonContainer GetContainer(JobSailing sailing)
		{
			CommonContainer result = null;

			if (sailing != null)
			{
				foreach (CommonContainer aContainer in Containers)
				{
					if (aContainer.Sailing != null && sailing.PK == aContainer.Sailing.PK)
					{
						result = aContainer;
						break;
					}
				}
			}

			return result;
		}

		public CommonContainer GetContainer(CommonConsol consol, JobSailing sailing)
		{
			CommonContainer result = null;

			if (consol != null && sailing != null)
			{
				foreach (CommonContainer aContainer in Containers)
				{
					if (consol.PK == aContainer.JC_JK && aContainer.Sailing != null && sailing.PK == aContainer.Sailing.PK)
					{
						result = aContainer;
						break;
					}
				}
			}

			return result;
		}

		public virtual void SetContainer(CommonConsol consol, CommonContainer newValue)
		{
			CommonContainer currentValue = GetContainer(consol);

			if (newValue != currentValue)
			{
				if (currentValue != null)
				{
					currentValue.PackLines.Remove(this);
				}

				if (newValue != null)
				{
					if (!IsOuterPackType)
					{
						ErrorReporter.ReportOnce("Inner PackLine should not have Container allocated", "Inner PackLine (" + PK.ToString() + ") should not have Container allocated.");
					}
					else
					{
						if (consol != null && !consol.Containers.Contains(newValue))
						{
							ErrorReporter.ReportOnce(FormattableString.Invariant($"Packline ({PK}) should not be packed into a Container {newValue.JC_ContainerNum} which is not on the Consol {consol.JK_UniqueConsignRef}."));
						}

						newValue.PackLines.Add(this);
						this.MarkAsNeedingValidation();
					}
				}
			}
		}

		public void SetContainer(JobSailing sailing, CommonContainer newValue)
		{
			CommonContainer currentValue = GetContainer(sailing);

			if (newValue != currentValue)
			{
				if (currentValue != null)
				{
					currentValue.PackLines.Remove(this);
				}

				if (newValue != null)
				{
					newValue.PackLines.Add(this);
					this.MarkAsNeedingValidation();
				}
			}
		}

		public void SetContainer(CommonConsol consol, JobSailing sailing, CommonContainer newValue)
		{
			CommonContainer currentValue = GetContainer(consol, sailing);

			if (newValue != currentValue)
			{
				if (currentValue != null)
				{
					currentValue.PackLines.Remove(this);
				}

				if (newValue != null)
				{
					newValue.PackLines.Add(this);
				}
			}
		}

		public void SetContainer(ZGuid containerPK)
		{
			if (!containerPK.IsEmpty && !Containers.Contains(containerPK))
			{
				CommonContainer containerToAdd = LoadContainer(containerPK);
				if (containerToAdd != null && containerToAdd.Consol != null)
				{
					SetContainer(containerToAdd.Consol, containerToAdd);
				}
				else
				{
					containerToAdd.PackLines.Add(this);
				}
			}
		}

		protected virtual CommonContainer LoadContainer(ZGuid containerPK)
		{
			return Factory.Load<CommonContainer>(containerPK);
		}

		[BusinessObjectTestExclude()]
		[MaxLength(CommonContainer.Schema.JC_ContainerNumMaxLength)]
		public ZString JL_Calc_ContainerNumber
		{
			get
			{
				CommonContainer consolContainerThatPackedThisLine = GetContainer(CurrentConsol);
				fJL_Calc_ContainerNumber = consolContainerThatPackedThisLine != null ? consolContainerThatPackedThisLine.JC_ContainerNum : ZString.Empty;
				return fJL_Calc_ContainerNumber;
			}
			set
			{
				CheckMaximumLength(JL_Calc_ContainerNumberInfo, value);
				fJL_Calc_ContainerNumber = value;
				if (fJL_Calc_ContainerNumber.IsEmpty)
				{
					SetContainer(CurrentConsol, null);
				}
				else
				{
					if (CurrentConsol != null)
					{
						foreach (CommonContainer containerAvailable in CurrentConsol.Containers)
						{
							if (containerAvailable.JC_ContainerNum.ToUpper() == fJL_Calc_ContainerNumber.ToUpper())
							{
								this.SetContainer(CurrentConsol, containerAvailable);
								return;
							}
						}
						SetContainer(CurrentConsol, null);
					}
					else if (Shipment != null)
					{
						foreach (CommonContainer containerAvailable in Shipment.Consols.AllContainers)
						{
							if (containerAvailable.JC_ContainerNum.ToUpper() == fJL_Calc_ContainerNumber.ToUpper())
							{
								this.SetContainer(CurrentConsol, containerAvailable);
								return;
							}
						}
						SetContainer(CurrentConsol, null);
					}
				}
				JL_Calc_ContainerNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JL_Calc_ContainerNumberInfo
		{
			get { return GetZPropertyInfo(JL_Calc_ContainerNumberName); }
		}

		#endregion

		#region JL_JC

		[BusinessObjectTestExclude]
		[RelatedBusinessObject("ConsolOrShipmentContainer")]
		[List("AllContainersOnShipment_List")]
		public virtual ZGuid JL_JC
		{
			get
			{
				var consolContainerThatPackedThisLine = GetContainer(CurrentConsol);
				fJL_JC = consolContainerThatPackedThisLine != null ? consolContainerThatPackedThisLine.PK : ZGuid.Empty;

				return fJL_JC;
			}
			set
			{
				if (fJL_JC != value)
				{
					var originalContainer = GetConsolOrShipmentContainer(fJL_JC);

					fJL_JC = value;
					var newContainer = GetConsolOrShipmentContainer(fJL_JC);
					SetContainer(CurrentConsol, newContainer);
					
					JL_JCInfo.RefreshBinding();

					if (originalContainer != null)
					{
						originalContainer?.ContainerPenaltyCalculateHandlers?.ForEach(handler => handler.HandleContainerDeallocation(this));
					}

					if (newContainer != null)
					{
						newContainer?.ContainerPenaltyCalculateHandlers?.ForEach(handler => handler.HandleContainerAllocation(this));
					}
				}
			}
		}

		ZGuid fJL_JC;

		public ZPropertyInfo JL_JCInfo
		{
			get { return GetZPropertyInfo(Schema.JL_JC); }
		}

		public CommonContainer ConsolOrShipmentContainer
		{
			get
			{
				var guid = fJL_JC.IsValid ? fJL_JC : JL_JC;

				return GetConsolOrShipmentContainer(guid);
			}
		}

		CommonContainer GetConsolOrShipmentContainer(ZGuid containerGuid)
		{
			CommonContainer result = null;
			if (CurrentConsol != null)
			{
				result = (CommonContainer)CurrentConsol.Containers.FindByPK(containerGuid);
			}
			else if (Shipment != null)
			{
				foreach (CommonContainer containerAvailable in Shipment.Consols.AllContainers)
				{
					if (containerAvailable.PK == containerGuid)
					{
						result = containerAvailable;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#region JL_Calc_SealNumber

		public ZString JL_Calc_SealNumber
		{
			get
			{
				ZString result = ZString.Empty;
				CommonContainer consolContainerThatContainsThisPackLine = GetContainer(CurrentConsol);
				if (consolContainerThatContainsThisPackLine != null)
				{
					result = consolContainerThatContainsThisPackLine.JC_SealNum;
				}
				return result;
			}
		}

		public ZPropertyInfo JL_Calc_SealNumberInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_SealNumber); }
		}

		#endregion

		#region Dimension properties

		#region Height
		public const string JL_Calc_HeightUnitName = "JL_Calc_HeightUnit";
		public ZString JL_Calc_HeightUnit
		{
			get { return JL_UnitOfDimension; }
		}

		public ZPropertyInfo JL_Calc_HeightUnitInfo
		{
			get { return GetZPropertyInfo(JL_Calc_HeightUnitName); }
		}

		#endregion

		#region Width
		public const string JL_Calc_WidthUnitName = "JL_Calc_WidthUnit";
		public ZString JL_Calc_WidthUnit
		{
			get { return JL_UnitOfDimension; }
		}

		public ZPropertyInfo JL_Calc_WidthUnitInfo
		{
			get { return GetZPropertyInfo(JL_Calc_WidthUnitName); }
		}
		#endregion

		#endregion

		#region PackLineWeightUnit

		[List("JL_ActualWeightUQ_List")]
		public ZString PackLineWeightUnit
		{
			get { return FreightUtilities.IsValidWeightUnit(JL_ActualWeightUQ) ? JL_ActualWeightUQ.ToString() : Env.Registry.FreightWeightUnit; }
		}

		public ZPropertyInfo PackLineWeightUnitInfo
		{
			get { return GetZPropertyInfo(nameof(PackLineWeightUnit)); }
		}

		#endregion

		#region PackLineVolumeUnit

		[List("JL_ActualVolumeUQ_List")]
		public ZString PackLineVolumeUnit
		{
			get { return FreightUtilities.IsValidVolumeUnit(JL_ActualVolumeUQ) ? JL_ActualVolumeUQ.ToString() : Env.Registry.FreightVolumeUnit; }
		}

		public ZPropertyInfo PackLineVolumeUnitInfo
		{
			get { return GetZPropertyInfo(nameof(PackLineVolumeUnit)); }
		}

		#endregion

		#region JL_Calc_JS_UniqueConsignRef

		public ZPropertyInfo JL_Calc_JS_UniqueConsignRefInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_JS_UniqueConsignRef); }
		}

		public ZString JL_Calc_JS_UniqueConsignRef
		{
			get { return (Shipment != null) ? Shipment.JS_UniqueConsignRef : ZString.Empty; }
		}

		#endregion

		#region JL_Calc_ConsignorPK

		public ZPropertyInfo JL_Calc_ConsignorPKInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_ConsignorPK); }
		}

		[RelatedBusinessObject("CalcConsignor")]
		[List("Consignor_List")]
		public ZGuid JL_Calc_ConsignorPK
		{
			get { return (Shipment != null) ? Shipment.ConsignorPK : ZGuid.Empty; }
		}

		public OrgHeader CalcConsignor
		{
			get { return Factory.Load<OrgHeader>(JL_Calc_ConsignorPK); }
		}

		#endregion

		#region JL_Calc_ConsigneePK
		public ZPropertyInfo JL_Calc_ConsigneePKInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_ConsigneePK); }
		}

		[RelatedBusinessObject("CalcConsignee")]
		[List("Consignee_List")]
		public ZGuid JL_Calc_ConsigneePK
		{
			get { return (Shipment != null) ? Shipment.ConsigneePK : ZGuid.Empty; }
		}

		public OrgHeader CalcConsignee
		{
			get { return Factory.Load<OrgHeader>(JL_Calc_ConsigneePK); }
		}

		#endregion

		#region JL_Calc_JV_Vessel

		public ZPropertyInfo JL_Calc_JV_VesselInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_JV_Vessel); }
		}

		public ZString JL_Calc_JV_Vessel
		{
			get
			{
				ZString result = ZString.Empty;
				if (Containers.Count > 0)
				{
					result = Containers[0].JC_JV_NKVessel;
				}
				else if (Shipment != null)
				{
					result = Shipment.JS_Calc_CurrentVessel;
				}
				return result;
			}
		}

		#endregion

		#region JL_Calc_JV_VoyageNo

		public ZPropertyInfo JL_Calc_JV_VoyageNoInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_JV_VoyageNo); }
		}

		public ZString JL_Calc_JV_VoyageNo
		{
			get
			{
				ZString result = ZString.Empty;
				if (Containers.Count > 0)
				{
					result = Containers[0].JC_JV_VoyageFlight;
				}
				else if (Shipment != null)
				{
					result = Shipment.JS_Calc_CurrentVoyageFlight;
				}
				return result;
			}
		}

		#endregion

		#region JL_Calc_JS_GoodsDescription

		public ZPropertyInfo JL_Calc_JS_GoodsDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_JS_GoodsDescription); }
		}

		public ZString JL_Calc_JS_GoodsDescription
		{
			get { return (Shipment != null) ? Shipment.JS_GoodsDescription : ZString.Empty; }
		}

		#endregion

		#region JL_Calc_JS_MarksAndNumbers

		public ZPropertyInfo JL_Calc_JS_MarksAndNumbersInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_JS_MarksAndNumbers); }
		}

		public ZString JL_Calc_JS_MarksAndNumbers
		{
			get { return (Shipment != null) ? Shipment.JS_MarksAndNumbers : ZString.Empty; }
		}

		#endregion

		#region JL_Calc_JS_Destination

		public ZPropertyInfo JL_Calc_JS_DestinationInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_JS_Destination); }
		}

		public ZString JL_Calc_JS_Destination
		{
			get { return (Shipment != null) ? Shipment.JS_RL_NKDestination : ZString.Empty; }
		}

		#endregion

		#region JL_Calc_JX_ETD

		public ZPropertyInfo JL_Calc_JX_ETDInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_JX_ETD); }
		}

		public ZDateTime JL_Calc_JX_ETD
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (Containers.Count > 0)
				{
					result = Containers[0].JC_JA_E_DEP;
				}
				else if (Shipment != null)
				{
					result = Shipment.JS_Calc_CurrentETD;
				}
				return result;
			}
		}

		#endregion

		#region JL_Calc_IsReceived

		public ZPropertyInfo JL_Calc_IsReceivedInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_IsReceived); }
		}

		public ZBool JL_Calc_IsReceived
		{
			get
			{
				CommonShipment shipment = GetParentShipment();
				return (shipment != null) ? shipment.IsReceived : ZBool.False;
			}
		}

		#endregion

		#region JL_Calc_JX_NKLoadPort

		public ZPropertyInfo JL_Calc_JX_NKLoadPortInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_JX_NKLoadPort); }
		}

		public ZString JL_Calc_JX_NKLoadPort
		{
			get
			{
				ZString result = ZString.Empty;
				if (Containers.Count > 0)
				{
					result = Containers[0].JC_JA_NKPortOfLoading;
				}
				else if (Shipment != null)
				{
					result = Shipment.JS_Calc_CurrentLoadPort;
				}
				return result;
			}
		}

		#endregion

		#region JL_Calc_JX_NKDischPort

		public ZPropertyInfo JL_Calc_JX_NKDischPortInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_JX_NKDischPort); }
		}

		public ZString JL_Calc_JX_NKDischPort
		{
			get
			{
				ZString result = ZString.Empty;
				if (Containers.Count > 0)
				{
					result = Containers[0].JC_JB_NKPortOfDischarge;
				}
				else if (Shipment != null)
				{
					result = Shipment.JS_Calc_CurrentDischargePort;
				}
				return result;
			}
		}

		#endregion

		#region JL_Calc_JS_OH_HandledOnBehalfOfForwarder

		[RelatedBusinessObject("CalcHandledOnBehalfOfForwarder")]
		public ZGuid JL_Calc_JS_OH_HandledOnBehalfOfForwarder
		{
			get
			{
				if (JL_JS.IsValid)
				{
					if (Shipment != null)
					{
						return Shipment.JS_OH_HandledOnBehalfOfForwarder;
					}
				}

				return ZGuid.Empty;
			}
		}

		public ZPropertyInfo JL_Calc_JS_OH_HandledOnBehalfOfForwarderInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_JS_OH_HandledOnBehalfOfForwarder); }
		}

		public OrgHeader CalcHandledOnBehalfOfForwarder
		{
			get { return Factory.Load<OrgHeader>(JL_Calc_JS_OH_HandledOnBehalfOfForwarder); }
		}

		#endregion

		#region Calculated first import and export containers

		public virtual CommonContainer JL_Calc_FirstImportContainer
		{
			get
			{
				foreach (CommonContainer relatedContainer in Containers)
				{
					if (relatedContainer.JC_JB_NKPortOfDischarge.SubstringSafe(0, 2) == GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode)
					{
						return relatedContainer;
					}
				}

				return null;
			}
		}

		public CommonContainer JL_Calc_FirstExportContainer
		{
			get
			{
				foreach (CommonContainer relatedContainer in Containers)
				{
					if (relatedContainer.JC_JA_NKPortOfLoading.SubstringSafe(0, 2) == GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode)
					{
						return relatedContainer;
					}
				}

				return null;
			}
		}

		#endregion

		#region JL_Calc_FirstImportContainerNum

		public ZString JL_Calc_FirstImportContainerNum
		{
			get
			{
				if (JL_Calc_FirstImportContainer == null)
				{
					return ZString.Empty;
				}
				else
				{
					return JL_Calc_FirstImportContainer.JC_ContainerNum;
				}
			}
		}

		public ZPropertyInfo JL_Calc_FirstImportContainerNumInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_FirstImportContainerNum); }
		}

		#endregion

		#region JL_Calc_InStock

		public ZInt JL_Calc_InStock
		{
			get { return JL_PackageCount - JL_Outturn; }
		}

		public ZPropertyInfo JL_Calc_InStockInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_InStock); }
		}

		#endregion

		#region JL_Calc_OutturnedInStock

		public ZInt JL_Calc_OutturnedInStock
		{
			get { return JL_Outturn - PackagesConfirmed_DispatchedFromDestinationCFS; }
		}

		public ZPropertyInfo JL_Calc_OutturnedInStockInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_OutturnedInStock); }
		}

		#endregion

		#region JL_JS_HouseBill

		public ZString JL_JS_HouseBill
		{
			get { return (Shipment != null) ? Shipment.JS_HouseBill : ZString.Empty; }
		}

		public ZPropertyInfo JL_JS_HouseBillInfo
		{
			get { return GetZPropertyInfo(Schema.JL_JS_HouseBill); }
		}

		#endregion

		#region JL_JS_InterimReceipt

		public ZString JL_JS_InterimReceipt
		{
			get { return (Shipment != null) ? Shipment.JS_InterimReceipt : ZString.Empty; }
		}

		public ZPropertyInfo JL_JS_InterimReceiptInfo
		{
			get { return GetZPropertyInfo(Schema.JL_JS_InterimReceipt); }
		}

		#endregion

		#region JL_Calc_Surplus

		public ZPropertyInfo JL_Calc_SurplusInfo
		{
			get { return GetZPropertyInfo(nameof(JL_Calc_Surplus)); }
		}

		public ZInt JL_Calc_Surplus
		{
			get { return (JL_Outturn > JL_PackageCount) ? JL_Outturn - JL_PackageCount : 0; }
		}

		#endregion

		#region JL_Calc_Shortlanded

		public ZPropertyInfo JL_Calc_ShortlandedInfo
		{
			get { return GetZPropertyInfo(nameof(JL_Calc_Shortlanded)); }
		}

		public ZInt JL_Calc_Shortlanded
		{
			get { return (Shipment != null && !Shipment.IsCoLoadMaster && !Shipment.IsBlindCoLoadMaster && JL_PackageCount > JL_Outturn) ? JL_PackageCount - JL_Outturn : 0; }
		}

		#endregion

		#region JL_Calc_ContainerNum

		public ZString JL_Calc_ContainerNum
		{
			get
			{
				CommonContainer container = GetContainer(CurrentConsol);
				return container != null ? container.JC_ContainerNum : ZString.Empty;
			}
		}

		public ZPropertyInfo JL_Calc_ContainerNumInfo
		{
			get { return GetZPropertyInfo(nameof(JL_Calc_ContainerNum)); }
		}

		#endregion

		#region JL_Calc_PackagesToDeliver

		/// <summary>
		/// The number of packages required to be delivered.  Either JL_PackageCount or JL_Outturn
		/// depending on circumstances.
		/// </summary>
		public ZInt JL_Calc_PackagesToDeliver
		{
			get { return UseOutturn ? JL_Outturn : JL_PackageCount; }
		}

		public ZPropertyInfo JL_Calc_PackagesToDeliverInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_PackagesToDeliver); }
		}

		#endregion

		#region JL_Calc_PackLineToPkgPackage Discrepancies

		public ZInt JL_Calc_PacklineToPkgPackage_QtyDiscrepancy => JL_PackageCount - PkgPackageCollection_TotalQty;

		public ZDecimal JL_Calc_PacklineToPkgPackage_WeightDiscrepancy => JL_ActualWeight - PkgPackageCollection_TotalWeight;

		public ZDecimal JL_Calc_PacklineToPkgPackage_VolumeDiscrepancy => JL_ActualVolume - PkgPackageCollection_TotalVolume;

		#endregion

		#region PackagesToDeliver

		[BusinessObjectTestExclude]
		public ZInt PackagesToDeliver
		{
			get { return JL_Calc_PackagesToDeliver; }
			set
			{
				JL_PackageCount = value;
				PackagesToDeliverInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PackagesToDeliverInfo
		{
			get { return GetZPropertyInfo(Schema.PackagesToDeliver); }
		}

		#endregion

		protected bool HasFirstImportContainer
		{
			get { return JL_Calc_FirstImportContainer != null; }
		}

		public bool IsLooseImport
		{
			get { return !HasFirstImportContainer && IsOnImportShipment; }
		}

		public bool IsOnImportShipment
		{
			get { return Shipment != null && Shipment.IsImport(); }
		}

		public bool IsOnExportShipment
		{
			get { return Shipment != null && Shipment.IsExport(); }
		}

		public bool IsOnColoadMasterShipment
		{
			get { return Shipment != null && (Shipment.IsCoLoadMaster || Shipment.IsBlindCoLoadMaster) && Shipment.CoLoadShipments.Count > 0; }
		}

		#region JL_Calc_WeightToDeliver

		public ZDecimal JL_Calc_WeightToDeliver
		{
			get
			{
				var result = UseOutturn && JL_OutturnedWeight != 0 ? JL_OutturnedWeight : JL_ActualWeight;
				return this.GetRoundedValue(JL_Calc_WeightToDeliverInfo, result);
			}
		}

		public ZPropertyInfo JL_Calc_WeightToDeliverInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_WeightToDeliver); }
		}

		#endregion

		#region JL_Calc_VolumeToDeliver

		public ZDecimal JL_Calc_VolumeToDeliver
		{
			get
			{
				var result = UseOutturn && JL_OutturnedVolume != 0 ? JL_OutturnedVolume : JL_ActualVolume;
				return this.GetRoundedValue(JL_Calc_VolumeToDeliverInfo, result);
			}
		}

		public ZPropertyInfo JL_Calc_VolumeToDeliverInfo
		{
			get { return GetZPropertyInfo(Schema.JL_Calc_VolumeToDeliver); }
		}

		#endregion

		#region AreContainerLegsReadOnly

		protected virtual bool AreContainerLegsReadOnly
		{
			get { return (Shipment != null && Shipment.JS_IsCFSRegistered); }
		}

		#endregion

		#region JL_OutturnUD

		public ZString JL_OutturnUD
		{
			get { return JL_UnitOfDimension; }
		}

		public ZPropertyInfo JL_OutturnUDInfo
		{
			get { return GetZPropertyInfo(Schema.JL_OutturnUD); }
		}

		#endregion

		#region JL_OutturnVolumeUQ

		public ZString JL_OutturnVolumeUQ
		{
			get { return JL_ActualVolumeUQ; }
		}

		public ZPropertyInfo JL_OutturnVolumeUQInfo
		{
			get { return GetZPropertyInfo(Schema.JL_OutturnVolumeUQ); }
		}

		#endregion

		#region JL_OutturnVolume

		public override ZDecimal JL_OutturnedVolume
		{
			get { return base.JL_OutturnedVolume; }
			set { base.JL_OutturnedVolume = this.GetRoundedValue(JobPackLinesSchema.JL_OutturnedVolume, JL_OutturnedVolumeInfo, value); }
		}

		#endregion

		#region JL_OutturnWeightUQ

		public ZString JL_OutturnWeightUQ
		{
			get { return JL_ActualWeightUQ; }
		}

		public ZPropertyInfo JL_OutturnWeightUQInfo
		{
			get { return GetZPropertyInfo(Schema.JL_OutturnWeightUQ); }
		}

		#endregion

		#region JL_OutturnVolume

		public override ZDecimal JL_OutturnedWeight
		{
			get { return base.JL_OutturnedWeight; }
			set { base.JL_OutturnedWeight = this.GetRoundedValue(JobPackLinesSchema.JL_OutturnedWeight, JL_OutturnedWeightInfo, value); }
		}

		#endregion

		ZPropertyInfo PrefixInfo(ZPropertyInfo info)
		{
			if (!IsDeleted)
			{
				info.HumanReadableName = HumanReadableName + ": " + info.Description;
			}
			return info;
		}

		void SetJL_ActualVolume()
		{
			if (JL_Height > 0 && JL_Width > 0 && JL_Length > 0 && JL_PackageCount > 0)
			{
				JL_ActualVolume = CalculatedVolume;
			}
		}

		void SetJL_OutturnedVolume()
		{
			if (IsOutturned && JL_OutturnedHeight > 0 && JL_OutturnedWidth > 0 && JL_OutturnedLength > 0)
			{
				JL_OutturnedVolume = CalculatedOutturnedVolume;
			}
		}

		public ZDecimal CalculatedVolume
		{
			get
			{
				// Override the default bankers rounding if DefaultNumberOfDecimalPlaces defines new rounding.
				var result = FreightUtilities.CalculateVolume(
					JL_ActualVolume,
					JL_PackageCount,
					JL_Length,
					JL_Width,
					JL_Height,
					JL_UnitOfDimension,
					JL_ActualVolumeUQ,
					JobPackLinesSchema.JL_ActualVolume.Scale,
					overridenRoundingFunc: unroundedVolume => this.GetRoundedValue(CalculatedVolumeInfo, unroundedVolume));
				return result;
			}
		}

		public ZPropertyInfo CalculatedVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.CalculatedVolume); }
		}

		public ZDecimal CalculatedOutturnedVolume
		{
			get
			{
				var result = FreightUtilities.CalculateVolume(
					JL_OutturnedVolume,
					JL_Outturn,
					JL_OutturnedLength,
					JL_OutturnedWidth,
					JL_OutturnedHeight,
					JL_OutturnUD,
					JL_OutturnVolumeUQ,
					JobPackLinesSchema.JL_OutturnedVolume.Scale,
					overridenRoundingFunc: unroundedVolume => this.GetRoundedValue(CalculatedOutturnedVolumeInfo, unroundedVolume));
				return result;
			}
		}

		public ZPropertyInfo CalculatedOutturnedVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.CalculatedOutturnedVolume); }
		}

		void UpdateGrossWeightForAllContainers()
		{
			foreach (CommonContainer container in Containers)
			{
				container.SetGrossWeightFromCombinedWeights();
			}
		}

		#region JL_VehicleTransmission

		[List("JL_VehicleTransmission_List")]
		public override ZString JL_VehicleTransmission
		{
			get { return base.JL_VehicleTransmission; }
			set { base.JL_VehicleTransmission = value; }
		}

		public CodeDescriptionPairList JL_VehicleTransmission_List
		{
			get
			{
				return Factory.GetCachedValue("CommonContainer.JC_VehicleTransmission_List",
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Core.Constants.VehicleTransmissionType.Automatic, ResString.GetMultilingualString("9b457d85-5c8c-451b-a730-7d33bec4858d", "Automatic"));
						result.AddPair(Core.Constants.VehicleTransmissionType.Manual, ResString.GetMultilingualString("6904d896-bfdd-43a6-8daa-570c0fc5240d", "Manual"));
						return result;
					});
			}
		}

		#endregion

		#region JL_AdditionalInspectionTypeCode

		[List("AdditionalInspectionTypes")]
		[MaxLength(3)]
		public virtual ZString JL_AdditionalInspectionTypeCode
		{
			get
			{
				var number = AdditionalInspectionTypeCusEntryNumber ?? FallbackAdditionalInspectionTypeCusEntryNumberForAllCountries;
				return number != null ? number.CE_EntryNum : AdditionalInspectionTypeCodeDefault;
			}
			set
			{
				if (value != JL_AdditionalInspectionTypeCode)
				{
					CheckMaximumLength(JL_AdditionalInspectionTypeCodeInfo, value);

					if (value == AdditionalInspectionTypeCodeDefault && FallbackAdditionalInspectionTypeCusEntryNumberForAllCountries == null)
					{
						if (AdditionalInspectionTypeCusEntryNumber != null)
						{
							UnHookEntryNumber(AdditionalInspectionTypeCusEntryNumber);
							AdditionalInspectionTypeCusEntryNumbersForAllCountries.RemoveAndDelete(AdditionalInspectionTypeCusEntryNumber);
						}
					}
					else
					{
						SetAdditionalInspectionTypeCusEntryNumberValue(value);
					}

					JL_AdditionalInspectionTypeCodeHasChanges = true;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJL_AdditionalInspectionTypeCode();
					}

					JL_AdditionalInspectionTypeCodeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo JL_AdditionalInspectionTypeCodeInfo => GetZPropertyInfo(Schema.JL_AdditionalInspectionTypeCode);

		public bool JL_AdditionalInspectionTypeCodeHasChanges { get; set; }

		CusEntryNumber AdditionalInspectionTypeCusEntryNumber
		{
			get { return AdditionalInspectionTypeCusEntryNumbersForAllCountries.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_RN_NKCountryCode == CountryOrEuropeanUnionCode); }
		}

		CusEntryNumber FallbackAdditionalInspectionTypeCusEntryNumberForAllCountries
		{
			get { return AdditionalInspectionTypeCusEntryNumbersForAllCountries.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_RN_NKCountryCode.IsEmpty); }
		}

		[ChildEditable(true)]
		CusEntryNumCollection AdditionalInspectionTypeCusEntryNumbersForAllCountries => additionalInspectionTypeCusEntryNumbersForAllCountries ?? (additionalInspectionTypeCusEntryNumbersForAllCountries = GetInspectionTypeCusEntryNumberCollection(true));
		CusEntryNumCollection additionalInspectionTypeCusEntryNumbersForAllCountries;

		void SetAdditionalInspectionTypeCusEntryNumberValue(ZString value)
		{
			var number = LoadOrCreateAdditionalInspectionTypeCusEntryNumber();
			number.CE_EntryNum = value;
		}
		protected virtual ZString AdditionalInspectionTypeCodeDefault => ZString.Empty;
		protected virtual bool JL_AdditionalInspectionTypeCode_ReadOnly => true;

		CusEntryNumber LoadOrCreateAdditionalInspectionTypeCusEntryNumber()
		{
			var number = AdditionalInspectionTypeCusEntryNumber;
			if (number == null)
			{
				number = AdditionalInspectionTypeCusEntryNumbersForAllCountries.AddNew();
				number.CE_ParentTable = TableName;
				number.CE_Category = CusEntryNumber.Categories.InspectionStatus;
				number.CE_EntryType = CusEntryNumber.EntryType.AdditionalInspectionStatus;
				number.CE_EntryIsSystemGenerated = false;
				number.CE_ParentID = PK;
				number.CE_RN_NKCountryCode = CountryOrEuropeanUnionCode;
				HookEntryNumber(number);
			}

			return number;
		}

		CusEntryNumCollection GetInspectionTypeCusEntryNumberCollection(bool isAdditionalInspectionType)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(CusEntryNumSchema.CE_ParentID, PK);
			filter.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.InspectionStatus);
			filter.AddToFilter(CusEntryNumSchema.CE_EntryType, isAdditionalInspectionType ? CusEntryNumber.EntryType.AdditionalInspectionStatus : CusEntryNumber.EntryType.InspectionStatus);

			var collection = new CusEntryNumCollection(Factory, filter);
			collection.IsManagedForDataRefresh = true;
			collection.Load();
			collection.CountChanged += CusEntryNumbersCollectionChanged;

			foreach (CusEntryNumber cusEntryNumber in collection)
			{
				HookEntryNumber(cusEntryNumber);
			}

			RegisterEditableChildObject(collection);

			return collection;
		}

		ZString CountryOrEuropeanUnionCode
		{
			get
			{
				return SupplyChainSecurityConfiguration.LicenceEconomicGroupingCode.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : SupplyChainSecurityConfiguration.LicenceEconomicGroupingCode;
			}
		}

		#endregion

		#region JL_InspectionTypeCode

		[List("InspectionTypes")]
		[MaxLength(3)]
		[UniversalCopyExtraProperty]
		public virtual ZString JL_InspectionTypeCode
		{
			get
			{
				return InspectionTypeCusEntryNumber != null ? InspectionTypeCusEntryNumber.CE_EntryNum : InspectionTypeCodeDefault;
			}
			set
			{
				if (value != JL_InspectionTypeCode)
				{
					CheckMaximumLength(JL_InspectionTypeCodeInfo, value);

					if (value.IsEmpty && value == InspectionTypeCodeDefault)
					{
						if (InspectionTypeCusEntryNumber != null)
						{
							UnHookEntryNumber(InspectionTypeCusEntryNumber);
							InspectionTypeCusEntryNumbersForAllCountries.RemoveAndDelete(InspectionTypeCusEntryNumber);
						}
					}
					else
					{
						var number = LoadOrCreateInspectionTypeCusEntryNumber();
						number.CE_EntryNum = value;
					}

					JL_InspectionTypeCodeHasChanges = true;
					IsMarkingSecuredValid = Shipment?.IsMarkingPackLinesAsSecuredAllowed ?? false;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJL_InspectionTypeCode();
					}

					JL_InspectionTypeCodeInfo.RefreshBinding();
				}
			}
		}

		public bool JL_InspectionTypeCodeHasChanges { get; set; }

		public bool IsMarkingSecuredValid { get; set; }

		public ZPropertyInfo JL_InspectionTypeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JL_InspectionTypeCode); }
		}

		protected virtual ZString InspectionTypeCodeDefault => ZString.Empty;

		protected virtual bool JL_InspectionTypeCode_ReadOnly => true;

		CusEntryNumber LoadOrCreateInspectionTypeCusEntryNumber()
		{
			var number = InspectionTypeCusEntryNumber;
			if (number == null)
			{
				number = InspectionTypeCusEntryNumbersForAllCountries.AddNew();
				number.CE_ParentTable = TableName;
				number.CE_Category = CusEntryNumber.Categories.InspectionStatus;
				number.CE_EntryType = CusEntryNumber.EntryType.InspectionStatus;
				number.CE_EntryIsSystemGenerated = false;
				number.CE_ParentID = PK;
				number.CE_RN_NKCountryCode = CountryOrEuropeanUnionCode;
				HookEntryNumber(number);
			}

			return number;
		}

		protected CusEntryNumber InspectionTypeCusEntryNumber
		{
			get
			{
				return InspectionTypeCusEntryNumbersForAllCountries
					.Cast<CusEntryNumber>()
					.FirstOrDefault(x => x.CE_RN_NKCountryCode == CountryOrEuropeanUnionCode);
			}
		}

		[ChildEditable(true)]
		CusEntryNumCollection InspectionTypeCusEntryNumbersForAllCountries
		{
			get
			{
				if (inspectionTypeCusEntryNumbersForAllCountries == null)
				{
					ZQuery filter = new ZQuery();
					filter.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.InspectionStatus);
					filter.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumber.EntryType.InspectionStatus);
					filter.AddToFilter(CusEntryNumSchema.CE_ParentID, PK);

					inspectionTypeCusEntryNumbersForAllCountries = new CusEntryNumCollection(Factory, filter);
					inspectionTypeCusEntryNumbersForAllCountries.IsManagedForDataRefresh = true;
					inspectionTypeCusEntryNumbersForAllCountries.Load();
					inspectionTypeCusEntryNumbersForAllCountries.CountChanged += CusEntryNumbersCollectionChanged;

					foreach (CusEntryNumber cusEntryNumber in inspectionTypeCusEntryNumbersForAllCountries)
					{
						HookEntryNumber(cusEntryNumber);
					}

					RegisterEditableChildObject(inspectionTypeCusEntryNumbersForAllCountries);
				}

				return inspectionTypeCusEntryNumbersForAllCountries;
			}
		}
		CusEntryNumCollection inspectionTypeCusEntryNumbersForAllCountries;

		void HookEntryNumber(CusEntryNumber number)
		{
			number.CE_EntryNumInfo.ValueChanged += CusEntryNumberChanged;
		}

		void UnHookEntryNumber(CusEntryNumber number)
		{
			number.CE_EntryNumInfo.ValueChanged -= CusEntryNumberChanged;
		}

		void CusEntryNumberChanged(object sender, EventArgs e)
		{
			JL_InspectionTypeCodeInfo.RefreshBinding();
		}

		void CusEntryNumbersCollectionChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				HookEntryNumber((CusEntryNumber)e.BizObject);
			}

			if (e.ItemRemoved)
			{
				UnHookEntryNumber((CusEntryNumber)e.BizObject);
			}

			JL_InspectionTypeCodeInfo.RefreshBinding();
		}

		#endregion

		#region JL_Calcs

		public ZString JL_Calc_DGClass
		{
			get => getPropertyFromUNDGCollection(undg => undg.DI_IMOClass);
		}

		public ZString JL_Calc_DGSubstance
		{
			get => getPropertyFromUNDGCollection(undg => undg.Substance?.DG_Code ?? ZString.Empty);
		}

		public ZString JL_Calc_DIHazardousWasteCode
		{
			get => getPropertyFromUNDGCollection(undg => undg.DI_HazardousWasteCode, true);
		}

		public ZString JL_Calc_DISpecialPermitIssueDate
		{
			get => getPropertyFromUNDGCollection(undg => undg.DI_SpecialPermitIssueDate.ToString(), true);
		}

		public ZString JL_Calc_DISpecialPermitNumber
		{
			get => getPropertyFromUNDGCollection(undg => undg.DI_SpecialPermitNumber, true);
		}

		public ZString JL_Calc_DIIsSalvagePackaging
		{
			get => getPropertyFromUNDGCollection(undg => undg.DI_IsSalvagePackaging ? "Y" : "N", true);
		}

		public ZString JL_Calc_DIIsResidueLastContained
		{
			get => getPropertyFromUNDGCollection(undg => undg.DI_IsResidueLastContained ? "Y" : "N", true);
		}

		ZString getPropertyFromUNDGCollection(Func<UNDGDataItem, ZString> getPropertyFunc, bool useManyInsteadOfMixed = false)
		{
			var listOfUniqueProperties = this.UNDGs.Select(undg => getPropertyFunc(undg)).Distinct();
			switch (listOfUniqueProperties.Count())
			{
				case 0:
					return ZString.Empty;
				case 1:
					return listOfUniqueProperties.First();
				default:
					return useManyInsteadOfMixed ?
						Res.GetString("3dcf9279-db4e-4013-842d-1f75191ae9f6", "Many") :
						Res.GetString("C1A3B2BB-EB31-4EB2-9C7C-479D37BF0379", "Mixed");
			}
		}

		public ZDecimal JL_Calc_LargestDimension
		{
			get => Math.Max(Math.Max(JL_Length, JL_Height), JL_Width);
		}

		public ZDecimal JL_Calc_Girth
		{
			get
			{
				var dimensions = new[] { JL_Length, JL_Height, JL_Width }.OrderByDescending(d => d).ToArray();
				return dimensions[1] * 2 + dimensions[2] * 2;
			}
		}

		#endregion

		#region JL_Calc_OriginTransitWarehouse

		public OrgAddress JL_Calc_OriginTransitWarehouse => Shipment.ExportReceivingDepot ?? CurrentConsol?.PackDepotAddress;

		#endregion

		#region JL_OriginTransitWarehouseStatus

		[List("JL_OriginTransitWarehouseStatus_List")]
		public override ZString JL_OriginTransitWarehouseStatus
		{
			get { return base.JL_OriginTransitWarehouseStatus; }
			set { base.JL_OriginTransitWarehouseStatus = value; }
		}

		#endregion

		#region JL_LastKnownTransitWarehouseStatus

		[List("JL_LastKnownTransitWarehouseStatus_List")]
		public override ZString JL_LastKnownTransitWarehouseStatus
		{
			get { return base.JL_LastKnownTransitWarehouseStatus; }
			set { base.JL_LastKnownTransitWarehouseStatus = value; }
		}

		#endregion

		public override ZString JL_HarmonisedCode
		{
			get => base.JL_HarmonisedCode;
			set
			{
				if (value != JL_HarmonisedCode)
				{
					SetHarmonisedCodeTariff(value);
				}
				base.JL_HarmonisedCode = value;
			}
		}

		public TariffView HarmonisedCodeTariff
		{
			get
			{
				if (!isHarmonisedCodeTariffSet)
				{
					SetHarmonisedCodeTariff(JL_HarmonisedCode);
				}
				return harmonisedCodeTariff;
			}
		}

		void SetHarmonisedCodeTariff(ZString tariffCode)
		{
			var query = TariffView.Loader.GetEffectiveTariffFilter(
				Factory,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO,
				Customs.Universal.Constants.TariffTypes.HarmonizedSystem,
				tariffCode,
				TariffHelper.GetHSCodeEffectiveDate(Shipment));
			harmonisedCodeTariff = Factory.LoadTop1<TariffView>(query);
			isHarmonisedCodeTariffSet = true;
		}

		TariffView harmonisedCodeTariff;
		bool isHarmonisedCodeTariffSet;

		#endregion

		#region Lookups

		public WhsWarehouseCollection WhsWarehouse_List
		{
			get { return whsWarehouses ?? (whsWarehouses = new WhsWarehouseCollection(Factory, Warehouse.Integration.WarehouseCollectionType.TransitWarehouse)); }
		}

		WhsWarehouseCollection whsWarehouses;

		[SuppressWeaklyTypedCollectionMessage]
		public IList AllContainersOnShipment_List
		{
			get { return CurrentConsol != null ? CurrentConsol.Containers : new CommonContainerCollection(Factory); }
		}

		public CodeDescriptionPairList JL_ActualVolumeUQ_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		public CodeDescriptionPairList JL_ActualWeightUQ_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public CodeDescriptionPairList JL_UnitOfDimension_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length); }
		}

		public RefPackTypeCollection JL_F3_NKPackType_List
		{
			get { return new RefPackTypeCollection(Factory); }
		}

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		public ConsignorCollection Consignor_List
		{
			get { return BindingLists.OrgConsignor_List; }
		}

		public ConsigneeCollection Consignee_List
		{
			get { return BindingLists.OrgConsignee_List; }
		}

		public RefCommodityCodeCollection RefCommodity_List
		{
			get { return BindingLists.RefCommodityCode_List; }
		}

		public OrgHeaderCollection OrgHeader_List
		{
			get { return BindingLists.OrgHeader_List; }
		}

		public RefCountryCollection CountryOfOrigin_List
		{
			get { return BindingLists.RefCountry_List; }
		}

		public CodeDescriptionPairList InspectionTypes
		{
			get
			{
				var key = "PackLine.InspectionTypes." + GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				return Factory.GetCachedValue(key, () =>
				{
					var list = new CodeDescriptionPairList();
					foreach (CodeDescriptionPair item in FreightUtilities.SupplyChainSecurityConfiguration.InspectionTypeList)
					{
						list.Add(item);
					}

					list.AddPair(FreightDataRegistry.AviationSecurity_Unknown_Code, FreightDataRegistry.AviationSecurity_Unknown_Description);
					if (Shipment != null && Shipment.SupportsPackLineApprovedCode)
					{
						list.AddPair(FreightDataRegistry.AviationSecurity_PackLine_IsSecured_Code, FreightDataRegistry.AviationSecurity_PackLine_IsSecured_Description);
					}

					return list;
				});
			}
		}

		public CodeDescriptionPairList AdditionalInspectionTypes
		{
			get
			{
				var key = "PackLine.AdditionalInspectionTypes." + GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				return Factory.GetCachedValue(key, () =>
				{
					var list = new CodeDescriptionPairList();

					foreach (CodeDescriptionPair item in FreightUtilities.SupplyChainSecurityConfiguration.AdditionalInspectionTypeList)
					{
						list.Add(item);
					}

					list.AddPair(FreightDataRegistry.AviationSecurity_Unknown_Code, FreightDataRegistry.AviationSecurity_Unknown_Description);

					return list;
				});
			}
		}

		#region JL_OriginTransitWarehouseStatus_List

		public CodeDescriptionPairList JL_OriginTransitWarehouseStatus_List
		{
			get
			{
				var key = "PackLine.OriginTransitWarehouseStatus";
				return Factory.GetCachedValue(key, () =>
				{
					var list = new CodeDescriptionPairList();
					list.Add(new CodeDescriptionPair(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, FreightConstants.PacklineOriginTransitWarehouseStatus.Descriptions.Unknown));
					list.Add(new CodeDescriptionPair(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, FreightConstants.PacklineOriginTransitWarehouseStatus.Descriptions.Confirmed));
					list.Add(new CodeDescriptionPair(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, FreightConstants.PacklineOriginTransitWarehouseStatus.Descriptions.Discrepencies));
					list.Add(new CodeDescriptionPair(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.ShortShipped, FreightConstants.PacklineOriginTransitWarehouseStatus.Descriptions.ShortShipped));
					list.Add(new CodeDescriptionPair(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus, FreightConstants.PacklineOriginTransitWarehouseStatus.Descriptions.Surplus));
					return list;
				});
			}
		}

		#endregion

		#region JL_OriginTransitWarehouseStatus_List

		public CodeDescriptionPairList JL_LastKnownTransitWarehouseStatus_List
		{
			get
			{
				var key = "PackLine.LastKnownTransitWarehouseStatus";
				return Factory.GetCachedValue(key, () =>
				{
					var list = new CodeDescriptionPairList();
					list.Add(new CodeDescriptionPair(FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Descriptions.Received));
					list.Add(new CodeDescriptionPair(FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Descriptions.Dispatched));
					return list;
				});
			}
		}

		#endregion

		#endregion

		#region IPackLineInfo Members

		ZString IPackLineInfo.ContainerNumber
		{
			get { return JL_Calc_ContainerNumber; }
		}

		ZString IPackLineInfo.GoodsDescription
		{
			get { return (JL_Description.IsEmpty) ? JL_Calc_JS_GoodsDescription : JL_Description; }
		}

		ZDecimal IPackLineInfo.Height
		{
			get { return JL_Height; }
		}

		ZDecimal IPackLineInfo.Length
		{
			get { return JL_Length; }
		}

		ZDecimal IPackLineInfo.Width
		{
			get { return JL_Width; }
		}

		ZString IPackLineInfo.MarksAndNumbers
		{
			get { return (JL_MarksAndNumbers.IsEmpty) ? JL_Calc_JS_MarksAndNumbers : JL_MarksAndNumbers; }
		}

		ZInt IPackLineInfo.NumberOfPackages
		{
			get { return JL_PackageCount; }
		}

		ZString IPackLineInfo.PackType
		{
			get { return JL_F3_NKPackType; }
		}

		ZString IPackLineInfo.UnitOfDimension
		{
			get { return JL_UnitOfDimension; }
		}

		ZVolume IPackLineInfo.Volume
		{
			get { return new ZVolume(JL_ActualVolume, JL_ActualVolumeUQ); }
		}

		ZWeight IPackLineInfo.Weight
		{
			get { return new ZWeight(JL_ActualWeight, JL_ActualWeightUQ); }
		}

		#endregion

		#region IDefaultValuesForPackType Members

		ZDecimal IPackTypeDafaultable.Length
		{
			set { JL_Length = value; }
		}

		ZDecimal IPackTypeDafaultable.Width
		{
			set { JL_Width = value; }
		}

		ZDecimal IPackTypeDafaultable.Weight
		{
			set { JL_ActualWeight = value; }
		}

		ZString IPackTypeDafaultable.UnitOfDimension
		{
			set { JL_UnitOfDimension = value; }
		}

		ZString IPackTypeDafaultable.UnitOfWeight
		{
			set { JL_ActualWeightUQ = value; }
		}

		ZDecimal IPackTypeDafaultable.Height
		{
			set { JL_Height = value; }
		}

		#endregion

		#region IGoods Members

		ZInt IGoods.BookedPackages
		{
			get { return PackagesToDeliver; }
		}

		ZDecimal IGoods.BookedWeight
		{
			get { return JL_ActualWeight; }
		}

		ZDecimal IGoods.BookedVolume
		{
			get { return JL_ActualVolume; }
		}

		ZInt IGoods.DeliveredPackages
		{
			get { return ((IGoods)this).BookedPackages; }
		}

		ZDecimal IGoods.DeliveredWeight
		{
			get { return ((IGoods)this).BookedWeight; }
		}

		ZDecimal IGoods.DeliveredVolume
		{
			get { return ((IGoods)this).BookedVolume; }
		}

		ZString IGoods.PackagesUnit
		{
			get { return JL_F3_NKPackType; }
		}

		ZString IGoods.WeightUnit
		{
			get { return PackLineWeightUnit; }
		}

		ZString IGoods.VolumeUnit
		{
			get { return PackLineVolumeUnit; }
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get
			{
				IDefaultNumberOfDecimalsSupporter parentShipment = Shipment;
				var result = (parentShipment != null) ? parentShipment.TransportMode : ZString.Empty;

				return result;
			}
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			return GetUnitOfMeasureCore(property);
		}

		protected virtual ZString GetUnitOfMeasureCore(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.JL_ActualVolume:
				case Schema.JL_Calc_VolumeToDeliver:
				case Schema.CalculatedVolume:
				case Schema.CalculatedOutturnedVolume:
					{
						if (JL_ActualVolumeUQ_List.ContainsCode(JL_ActualVolumeUQ))
						{
							unitOfMeasure = JL_ActualVolumeUQ;
						}
						break;
					}

				case Schema.JL_OutturnedVolume:
					{
						if (JL_ActualVolumeUQ_List.ContainsCode(JL_OutturnVolumeUQ))
						{
							unitOfMeasure = JL_OutturnVolumeUQ;
						}
						break;
					}

				case Schema.JL_ActualWeight:
				case Schema.JL_Calc_WeightToDeliver:
					{
						if (JL_ActualWeightUQ_List.ContainsCode(JL_ActualWeightUQ))
						{
							unitOfMeasure = JL_ActualWeightUQ;
						}
						break;
					}

				case Schema.JL_OutturnedWeight:
					{
						if (JL_ActualWeightUQ_List.ContainsCode(JL_OutturnWeightUQ))
						{
							unitOfMeasure = JL_OutturnWeightUQ;
						}
						break;
					}

				case Schema.JL_Length:
				case Schema.JL_Width:
				case Schema.JL_Height:
				case Schema.JL_OutturnedLength:
				case Schema.JL_OutturnedWidth:
				case Schema.JL_OutturnedHeight:
					{
						if (JL_UnitOfDimension_List.ContainsCode(JL_UnitOfDimension))
						{
							unitOfMeasure = JL_UnitOfDimension;
						}
						break;
					}

				default:
					break;
			}

			return unitOfMeasure;
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return GetDefaultNumberOfDecimalsCore(property);
		}

		protected virtual int GetDefaultNumberOfDecimalsCore(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		ZDecimal IDefaultNumberOfDecimalsSupporterWithSchemaColumn.GetRoundedValue(SchemaColumn column, PropertyDescriptor property, ZDecimal value)
		{
			return GetRoundedValueCore(column, property, value);
		}

		public ZDecimal GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return GetRoundedValueCore(null, property, value);
		}

		protected virtual ZDecimal GetRoundedValueCore(SchemaColumn column, PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterWithSchemaColumnHelperForFreight.GetRoundedValue(this, column, property, value);
		}

		public void RoundMeasurePropertiesOnTransportModeChanged()
		{
			this.SetRoundedValue(JobPackLinesSchema.JL_ActualVolume, JL_ActualVolumeInfo);
			this.SetRoundedValue(JobPackLinesSchema.JL_ActualWeight, JL_ActualWeightInfo);
			this.SetRoundedValue(JobPackLinesSchema.JL_OutturnedVolume, JL_OutturnedVolumeInfo);
			this.SetRoundedValue(JobPackLinesSchema.JL_OutturnedWeight, JL_OutturnedWeightInfo);

			this.SetRoundedValue(JobPackLinesSchema.JL_Length, JL_LengthInfo);
			this.SetRoundedValue(JobPackLinesSchema.JL_Height, JL_HeightInfo);
			this.SetRoundedValue(JobPackLinesSchema.JL_Width, JL_WidthInfo);
			this.SetRoundedValue(JobPackLinesSchema.JL_OutturnedLength, JL_OutturnedLengthInfo);
			this.SetRoundedValue(JobPackLinesSchema.JL_OutturnedHeight, JL_OutturnedHeightInfo);
			this.SetRoundedValue(JobPackLinesSchema.JL_OutturnedWeight, JL_OutturnedWidthInfo);

			if (fPackLocations != null)
			{
				foreach (PackLocation location in fPackLocations)
				{
					IDefaultNumberOfDecimalsSupporterWithSchemaColumn decimalsSupporter = location;
					decimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged();
				}
			}
		}

		#endregion

		#region IRequiredTemperature Members

		public ZBool RequiresTemperatureControl
		{
			get { return JL_RequiresTemperatureControl; }
			set { JL_RequiresTemperatureControl = value; }
		}

		public ZPropertyInfo RequiresTemperatureControlInfo => GetWrappedZPropertyInfo(nameof(RequiresTemperatureControl), x => JL_RequiresTemperatureControlInfo);

		public ZDecimal RequiredTemperatureMaximum
		{
			get { return JL_RequiredTemperatureMaximum; }
			set { JL_RequiredTemperatureMaximum = value; }
		}

		public ZPropertyInfo RequiredTemperatureMaximumInfo => GetWrappedZPropertyInfo(nameof(RequiredTemperatureMaximum), x => JL_RequiredTemperatureMaximumInfo);

		public ZDecimal RequiredTemperatureMinimum
		{
			get { return JL_RequiredTemperatureMinimum; }
			set { JL_RequiredTemperatureMinimum = value; }
		}

		public ZPropertyInfo RequiredTemperatureMinimumInfo => GetWrappedZPropertyInfo(nameof(RequiredTemperatureMinimum), x => JL_RequiredTemperatureMinimumInfo);

		[List(nameof(TemperatureUnits))]
		[MaxLength(1)]
		public ZString RequiredTemperatureUnit
		{
			get { return JL_RequiredTemperatureUnit; }
			set { JL_RequiredTemperatureUnit = value; }
		}

		public ZPropertyInfo RequiredTemperatureUnitInfo => GetWrappedZPropertyInfo(nameof(RequiredTemperatureUnit), x => JL_RequiredTemperatureUnitInfo);

		#endregion

		#region Fields

		string stackTraceForMissingDivotErrorReport;
		public ICollection<string> ShipmentLinkLogs => shipmentLinkLogs;
		readonly List<string> shipmentLinkLogs = new List<string>();

		#endregion

		#region SupplyChainSecurityConfiguration

		ISupplyChainSecurityConfiguration SupplyChainSecurityConfiguration
		{
			get
			{
				return Shipment != null
					? Shipment.SupplyChainSecurityConfiguration
					: supplyChainSecurityConfiguration ?? (supplyChainSecurityConfiguration = ObjectFactory.Get<ISupplyChainSecurityConfigurationHelper>().GetConfiguration());
			}
		}

		ISupplyChainSecurityConfiguration supplyChainSecurityConfiguration;

		#endregion

		#region IHarmonisedCodesProvider

		IBusinessObjectCollection IHarmonisedCodesProvider.HarmonisedCodes => HarmonisedCodes;

		ITariffFormatter ITariffFormatProvider.TariffFormatter => packLineTariffFormatter ?? (packLineTariffFormatter = new PackLineTariffFormatter());
		ITariffFormatter packLineTariffFormatter;

		class PackLineTariffFormatter : ITariffFormatter
		{
			public ZString DisplayFormat(ZString unformattedTariff)
			{
				return unformattedTariff;
			}

			public ZString Format(ZString unformattedTariff)
			{
				return unformattedTariff.Replace(".", ZString.Empty);
			}
		}

		#endregion

		#region CusEntryNumCollection

		JobPackLineReferenceNumbersCollection cusEntryNums;

		[ChildEditable(true)]
		public JobPackLineReferenceNumbersCollection CusEntryNums
		{
			get
			{
				if (cusEntryNums == null)
				{
					cusEntryNums = new JobPackLineReferenceNumbersCollection(this);
					RegisterEditableChildObject(cusEntryNums);
				}
				return cusEntryNums;
			}
		}

		#endregion

		#region Binding

		public CodeDescriptionPairList TemperatureUnits
		{
			get { return BindToLists.GetCachedLists(Factory).TemperatureUnits; }
		}

		#endregion

		#region ISupportInspectionType

		public CusEntryNumber AdditionalInspectionType => LoadOrCreateAdditionalInspectionTypeCusEntryNumber();
		public CusEntryNumber InspectionType => LoadOrCreateInspectionTypeCusEntryNumber();

		#endregion
	}
}
