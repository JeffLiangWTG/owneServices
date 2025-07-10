#if DEBUG

using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Shared;
using Enterprise.Environment;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Packing.Business.Testing
{
	public class PackingTestHelper : IPkgPackageTestHelper
	{
		public PackingTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		readonly BusinessObjectFactory Factory;

		#region CreateDefaultPrinter

		public StmDefaultPrinter CreateDefaultPrinter(ZGuid menuItemPK, ZGuid printerPK, ZByte numberOfCopies)
		{
			var stmMenuItem = Factory.Load<IStmMenuItem>(menuItemPK);
			var defaultPrinter = StmDefaultPrinter.LoadOrCreateDefaultPrinter(Factory, GlbStaff.CurrentUser, stmMenuItem);
			defaultPrinter.SDP_SQ_Printer = printerPK;
			defaultPrinter.SDP_NumberOfCopies = numberOfCopies;
			return defaultPrinter;
		}

		#endregion

		#region CreateRefPackType

		public RefPackType CreateRefPackType(string code, string description, decimal height, decimal length, decimal width, string dimensionUnit, decimal weight, string weightUnit, string uomType = "")
		{
			var refPackType = Factory.New<RefPackType>();
			refPackType.F3_Code = code;
			refPackType.F3_Description = description;
			refPackType.F3_Height = height;
			refPackType.F3_Length = length;
			refPackType.F3_Width = width;
			refPackType.F3_UnitOfDimension = dimensionUnit;
			refPackType.F3_Weight = weight;
			refPackType.F3_UnitOfWeight = weightUnit;
			refPackType.F3_UOMType = uomType;
			return refPackType;
		}

		#endregion

		#region SetRefPackTypeUOM

		public void SetRefPackTypeUOM(ZString pkgType, ZString uom)
		{
			var packType = Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, pkgType));
			packType.F3_UOMType = uom;
		}

		#endregion

		#region CreatePackingParent

		public DummyWithPacking CreatePackingParent()
		{
			return Factory.New<DummyWithPacking>();
		}

		#endregion

		#region CreatePackage

		// Top Level Packages

		public PkgPackage CreatePackage(PkgPackageJob packageJob, ZInt quantity, ZString packageType, string containerType = "20GP")
		{
			return CreatePackage(packageJob, "", quantity, packageType, containerType);
		}

		public PkgPackage CreatePackage(PkgPackageJob packageJob, ZString packageID, ZInt quantity, ZString packageType, string containerType = "20GP")
		{
			var package = packageJob.Packages.AddNew(packageType, quantity);
			package.KP_PackageID = packageID;

			if (package.IsContainer)
			{
				var refContainer = LoadRefContainer(containerType);
				package.Container.K0_RC_ContainerType = refContainer.PK;
				CreatePackageExtension(package);
			}

			package.KP_Weight = quantity * 3m;
			package.KP_Volume = quantity * 0.05m;

			return package;
		}

		void CreatePackageExtension(PkgPackage package)
		{
			if (package.PackageJob.ParentJob is IPackingParentSupportsPackageExtensions packageExtensionsParent && packageExtensionsParent.PackageExtension == null)
			{
				CreatePackageExtension(packageExtensionsParent, package);
			}
		}

		public BusinessObject CreatePackage(ZGuid packageJobPK, ZString packageID, ZInt quantity, ZString packageType, ZDecimal volume, ZString volumeUQ, ZDecimal weight, ZString weightUQ)
		{
			var package = Factory.New<PkgPackage>();
			package.KP_KJ_ParentPackageJob = packageJobPK;
			package.KP_PackageID = packageID;
			package.KP_PackageQty = quantity;
			package.KP_F3_NKPackType = packageType;
			package.KP_Volume = volume;
			package.KP_VolumeUQ = volumeUQ;
			package.KP_Weight = weight;
			package.KP_WeightUQ = weightUQ;
			if (package.IsContainer)
			{
				var refContainer = LoadRefContainer("20GP");
				package.Container.K0_RC_ContainerType = refContainer.PK;
				CreatePackageExtension(package);
			}

			return package;
		}

		public PkgPackage CreatePackage(ZGuid packageJobPK, ZString packageID, ZInt quantity, ZString packageType, ZDecimal length, ZDecimal width, ZDecimal height, ZString dimUQ, ZDecimal volume, ZString volumeUQ, ZDecimal weight, ZString weightUQ)
		{
			var package = Factory.New<PkgPackage>();
			package.KP_KJ_ParentPackageJob = packageJobPK;
			package.KP_PackageID = packageID;
			package.KP_PackageQty = quantity;
			package.KP_F3_NKPackType = packageType;
			package.KP_Length = length;
			package.KP_Width = width;
			package.KP_Height = height;
			package.KP_DimensionUQ = dimUQ;
			package.KP_Volume = volume;
			package.KP_VolumeUQ = volumeUQ;
			package.KP_Weight = weight;
			package.KP_WeightUQ = weightUQ;

			return package;
		}

		public PkgPackage CreatePackage(IPackingParent packingParent, ZString packageID, ZInt quantity, ZString packageType, string containerType = "20GP")
		{
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(packingParent);
			return CreatePackage(packageJob, packageID, quantity, packageType, containerType);
		}

		// For Parent Package

		public PkgPackage CreatePackage(PkgPackage parentPackage, ZInt quantity, ZString packageType)
		{
			return CreatePackage(parentPackage, quantity, packageType, "");
		}

		public PkgPackage CreatePackage(PkgPackage parentPackage, ZInt quantity, ZString packageType, ZString packageID)
		{
			var package = parentPackage.Packages.AddNew(packageType, quantity);
			package.KP_PackageID = packageID;

			using (new SemaphoreManager(package.SuspendAddWeightToParentPackageSemaphore))
			{
				package.KP_Weight = quantity * 3m;
			}
			package.KP_Volume = quantity * 0.05m;

			return package;
		}

		#endregion

		#region CreatePackageExtension

		public PkgPackageExtension CreatePackageExtension(IPackingParentSupportsPackageExtensions packageExtensionsParent, PkgPackage package)
		{
			var packageExtension = Factory.New<PkgPackageExtension>();

			packageExtension.KPN_KP_Package = package.PK;
			packageExtension.KPN_ParentID = packageExtensionsParent.PK;
			packageExtension.KPN_ParentTableCode = packageExtensionsParent.TablePrefix;
			return packageExtension;
		}

		#endregion

		#region CreatePackableItemParent

		public DummyPackableItemParent CreatePackableItemParent()
		{
			return Factory.New<DummyPackableItemParent>();
		}

		#endregion

		#region CreatePackableItem

		public DummyPackableItem CreatePackableItem(ZDecimal qty)
		{
			var packableItem = Factory.New<DummyPackableItemParent>();
			packableItem.ZD1_NumberUnit = packableItem.PK; // apparently dummy decide if this Dummy object is a PackableItem or PackableItemParent based on this property.
			packableItem.Quantity = qty;

			return packableItem;
		}

		public DummyPackableItem CreatePackableItem(DummyPackableItemParent packableItemParent, ZDecimal qty)
		{
			var packableItem = Factory.New<DummyPackableItemParent>();
			packableItem.ZD1_NumberUnit = packableItemParent.PK; // apparently dummy decide if this Dummy object is a PackableItem or PackableItemParent based on this property.
			packableItem.Quantity = qty;

			packableItemParent.AddPackableItem(packableItem);

			return packableItem;
		}

		#endregion

		#region CreatePackageDivot

		public PkgPackageItemDivot CreatePackageDivot(PkgPackage package, IPackableItem packableItem)
		{
			var divot = package.PackedItemDivots.AddNew();
			divot.KI_ParentID = packableItem.PK;
			divot.KI_ParentTableCode = packableItem.TablePrefix;
			divot.KI_PackedQty = packableItem.Quantity;

			return divot;
		}

		#endregion

		#region CreatePackageHeader

		public PkgPackageHeader CreatePackageHeader(PkgPackageJob packageJob, string id)
		{
			var packageID = packageJob.LoosePackageIDs.AddNew();
			packageID.KPH_PackageID = id;
			return packageID;
		}

		#endregion

		#region CreatePackageHeaderPivot

		public PkgPackageJobPackageHeaderPivot CreatePackageHeaderPivot(PkgPackageJob packageJob, PkgPackageHeader packageHeader)
		{
			var packageHeaderPivot = Factory.New<PkgPackageJobPackageHeaderPivot>();
			packageHeaderPivot.KPJ_KJ_PackageJob = packageJob.PK;
			packageHeaderPivot.KPJ_KPH_PackageHeader = packageHeader.PK;

			return packageHeaderPivot;
		}

		#endregion

		#region CreateContainerTypeFilter

		public RefContainer LoadRefContainer(ZString containerType)
		{
			return Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType);
		}

		#endregion

		#region CreateDangerousGood

		public UNDGDataItem CreateDangerousGood(PkgPackage pkg, ZString substance, string dgClass, decimal weight = 0m, string weightUQ = "", decimal volume = 0m, string volumeUQ = "", OrgContact contact = null)
		{
			if (substance.IsEmpty)
			{
				return null;
			}

			var unno = substance.SubstringSafe(0, 4);
			var variant = substance.SubstringSafe(4, 2);
			var standard = UNDGSubstanceStandardTypes.IMO;
			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, unno, variant, standard).FirstOrDefault() ?? Factory.New<UNDGSubstance>();

			if (subs.DG_Code.IsEmpty)
			{
				subs.DG_UNNO = substance.SubstringSafe(0, UNDGSubstanceSchema.DG_UNNO.MaxLength);
				subs.DG_Variant = substance.SubstringSafe(UNDGSubstanceSchema.DG_UNNO.MaxLength, UNDGSubstanceSchema.DG_Variant.MaxLength);
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			var dg = pkg.UNDGs.AddNew();
			dg.DI_DG = subs.PK;
			dg.DI_IMOClass = dgClass;
			dg.DI_DGWeight = weight;
			dg.DI_UnitOfWeight = weightUQ;
			dg.DI_DGVolume = volume;
			dg.DI_UnitOfVolume = volumeUQ;
			dg.DI_OC_DGContact = contact != null ? contact.PK : ZGuid.Empty;
			return dg;
		}

		#endregion

		#region CreatePalletTransaction

		public PkgPalletTransaction CreatePalletTransaction(string transactionType, string transferType)
		{
			var palletTransaction = Factory.New<PkgPalletTransaction>();
			palletTransaction.KTR_TransactionType = transactionType;
			palletTransaction.KTR_TransferType = transferType;
			return palletTransaction;
		}

		#endregion

		#region CreatePalletTypeParent

		public void CreatePalletType(PalletTypeParent palletType, string code, string description, string equipmentCode, string providerCode)
		{
			var type = palletType.Types.AddNew();
			type.Code = code;
			type.Description = (NoResString)description;
			type.EquipmentCode = equipmentCode;
			type.ProviderCode = providerCode;
		}

		#endregion

		#region CreateCustomCode

		public OrgCusCode CreateCustomCode(OrgHeader orgHeader, string accountType, string regNo)
		{
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_CodeType = accountType;
			cusCode.OK_CustomsRegNo = regNo;
			return cusCode;
		}

		#endregion

		#region CreatePackageScreening

		public PkgPackageScreening CreatePackageScreening(PkgPackage package, string methodCode, bool passed = false)
		{
			var packageScreening = package.Screenings.AddNew();
			packageScreening.KPS_GS_NKScreenedBy = "STF";
			packageScreening.KPS_Passed = passed;
			packageScreening.KPS_Method = methodCode;
			packageScreening.KPS_Time = DateTimeOffset.Now;

			return packageScreening;
		}

		#endregion

		#region CreatePackageHold

		public PkgPackageHold CreatePackageHold(PkgPackage package, string holdCode, bool isRemoved = false)
		{
			var holdReason = Factory.New<PkgPackageHold>();
			holdReason.KHR_KP_Package = package.PK;
			holdReason.KHR_WHC_NKHoldCode = holdCode;
			holdReason.KHR_GS_NKAddedBy = "STF";
			holdReason.KHR_AddedTime = DateTime.Now;
			if (isRemoved)
			{
				holdReason.KHR_GS_NKRemovedBy = "STF";
				holdReason.KHR_RemovedTime = DateTime.Now;
			}

			return holdReason;
		}

		#endregion

		#region CreatePkgHandlingUnit

		public PkgHandlingUnit CreatePkgHandlingUnit(ZGuid branchPK, string jobContext)
		{
			var pkgHandlingUnit = Factory.New<PkgHandlingUnit>();
			pkgHandlingUnit.KPU_GB_Branch = branchPK;
			pkgHandlingUnit.KPU_JobContext = jobContext;

			return pkgHandlingUnit;
		}

		#endregion

		#region PackageHandlingUnitDivot

		public PkgPackageHandlingUnitDivot CreatePackageHandlingUnitDivot(PkgPackage handlingUnit, PkgPackage package)
		{
			var divot = Factory.New<PkgPackageHandlingUnitDivot>();
			divot.KPD_KP_HandlingUnit = handlingUnit.PK;
			divot.KPD_KP_Package = package.PK;

			return divot;
		}

		public PkgPackageHandlingUnitDivot PackHandlingUnit(PkgPackage handlingUnit, PkgPackage package, PkgPackage topHandlingUnit)
		{
			var divot = Factory.New<PkgPackageHandlingUnitDivot>();
			divot.KPD_KP_HandlingUnit = handlingUnit.PK;
			divot.KPD_KP_Package = package.PK;
			divot.KPD_PackedTime = ZDateTimeOffset.Now;
			divot.KPD_GS_NKPackedUser = Env.CurrentUser.Initials;

			package.KP_KP_TopHandlingUnitPackage = topHandlingUnit.PK;

			return divot;
		}

		public PkgPackageHandlingUnitDivot PackHandlingUnitAndUnpack(PkgPackage handlingUnit, PkgPackage package)
		{
			var now = ZDateTimeOffset.Now;

			var divot = Factory.New<PkgPackageHandlingUnitDivot>();
			divot.KPD_KP_HandlingUnit = handlingUnit.PK;
			divot.KPD_KP_Package = package.PK;

			divot.KPD_PackedTime = now;
			divot.KPD_GS_NKPackedUser = Env.CurrentUser.Initials;

			divot.KPD_UnpackedTime = now.AddHours(2);
			divot.KPD_GS_NKUnpackedUser = Env.CurrentUser.Initials;

			package.KP_KP_TopHandlingUnitPackage = ZGuid.Empty;

			return divot;
		}

		#endregion

		#region CreatePackageSeal

		public PkgPackageSeal CreatePackageSeal(PkgPackage package, string sealNumber, bool isOK = true)
		{
			var packageSeal = package.PackageSeals.AddNew();
			packageSeal.KPE_IsSealOK = isOK;
			packageSeal.KPE_Seal = sealNumber;

			return packageSeal;
		}

		#endregion

		#region CreatePackageOrderReference

		public PkgPackageOrderReference CreatePackageOrderReference(PkgPackage package, string orderNumber = "", string batchNumber = "", string commercialInvoiceNumber = "", ZDate expiryDate = default, string lineReference = "", string skuPartNumber = "", string serialNumber = "")
		{
			var packageOrderReference = Factory.New<PkgPackageOrderReference>();
			packageOrderReference.KPO_KP_Package = package.PK;
			packageOrderReference.KPO_OrderNumber = orderNumber;
			packageOrderReference.KPO_ExpiryDate = expiryDate;
			packageOrderReference.KPO_BatchNumber = batchNumber;
			packageOrderReference.KPO_CommercialInvoiceNumber = commercialInvoiceNumber;
			packageOrderReference.KPO_LineReference = lineReference;
			packageOrderReference.KPO_SKUPartNumber = skuPartNumber;
			packageOrderReference.KPO_SerialNumber = serialNumber;

			return packageOrderReference;
		}

		#endregion
	}
}

#endif
