using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

[assembly: ClusterKeyMetaData(typeof(CusPackingList), ParentTableName = JobDeclarationSchema.Constants.TableName, ParentFkColumnName = nameof(CusPackingList.CUL_JE),
	SecondaryParentTableName = JobComInvoiceHeaderSchema.Constants.TableName, SecondaryParentFkColumnName = nameof(CusPackingList.CUL_JZ))]

namespace Enterprise.Customs.Business
{
	[CodeProperty("JobNumber")]
	public class CusPackingList : AutoCusPackingList, IPackingParent, IClusterKeyWorker, IEDocsProvider, IPackingParentCustomPackTypes, ISequenceNumberHeader, ICustomLabelsConfigOrgProvider, Integration.Customs.ICusPackingList
	{
		public CusPackingList(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			packageJob = CusPackageJob.LoadOrCreatePackageJobWithNoChanges(this) as CusPackageJob;
		}

		public new class Schema : AutoCusPackingList.Schema
		{
			public const string TotalPackedQty = "TotalPackedQty";
			public const string TotalNetWeight = "TotalNetWeight";
			public const string TotalGrossWeight = "TotalGrossWeight";
		}

		public static readonly CusPackingListTypeDecider TypeDecider = new CusPackingListTypeDecider();

		readonly CusPackageJob packageJob;

		public CusPackageJob PackageJob => GetCusPackageJobCore();

		protected virtual CusPackageJob GetCusPackageJobCore() => packageJob;

		public BaseJobDeclaration Declaration => GetOrLoad(ref fDeclaration, CUL_JE);
		BaseJobDeclaration fDeclaration;

		public BaseJobComInvoiceHeader Invoice => GetOrLoad(ref fInvoice, CUL_JZ);
		BaseJobComInvoiceHeader fInvoice;

		public ZString CountryCode => Factory.GetValue(ref countryCode, () => Declaration is BaseJobDeclaration declaration ? declaration.CountryCode : (Invoice is BaseJobComInvoiceHeader invoice ? invoice.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
		CachedProperty<ZString> countryCode;

		protected override ZString HumanReadableNameCore => Res.GetString("BF3DFFF2-6365-4B98-9359-86D8957FAF1E", "Customs Packing List");

		protected override bool SupportsCloneCore() => true;

		public override void Delete()
		{
			PackageJob.Delete();
			using (GetPackableItemSequenceNumberRenumberingSuspender())
			{
				PackableItems.DeleteAll();
			}
			base.Delete();
		}

		T GetOrLoad<T>(ref T bizObj, ZGuid foreignKey)
			where T : BusinessObject
		{
			if (bizObj == null || bizObj.IsDeleted || (!IsDeleted && (bizObj.PK != foreignKey && !foreignKey.IsEmpty)))
			{
				bizObj = (T)Factory.Load(typeof(T), foreignKey);
			}
			return bizObj != null && !bizObj.IsDeleted ? bizObj : null;
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusPackingListFetchStrategy(this);

		[ResourceStringData("BF86AC03-3FDC-4B88-A27E-F5D4126D21BD", Caption = "Packing List Job #")]
		public ZString JobNumber => PackageJob.KJ_JobID;

		#region IShouldPackTrackedPackagesViaDivot

		bool IShouldPackTrackedPackagesViaDivot.ShouldPackTrackedPackagesViaDivot => false;

		#endregion

		#region IPackingParent Members

		IPackageActionStrategy IPackingParent.GetPackageActionStrategy(PkgPackage package) => new PackageActionStrategy(package);

		ControllerID IPackingParent.ControllerID => null;

		DocumentOptions IPackingParent.DocumentOptions => DocumentOptions.None;

		ZString IPackingParent.JobNo => PackageJob.KJ_JobID;

		ZString IPackingParent.ConnoteNo => ZString.Empty;

		ZString IPackingParent.JobDescription => ZString.Empty;

		ZString IPackingParent.GetSSCCPrefix(INotifications notify, SSCCGenerationContext context) => ZString.Empty;

		void IPackingParent.OnPackageJobReleased()
		{
		}

		void IPackingParent.OnPackageJobCreatedOrLoaded(PkgPackageJob pkgJob)
		{
		}

		void IPackingParent.OnPackageDelete(PkgPackage package)
		{
		}

		void IPackingParent.OnContainerIDChanged(PkgPackage container)
		{
		}

		bool IPackingParent.IsParentJobFinalised => false;

		bool IList.IsReadOnly => false;

		bool IPackingParent.IsPackingJobReadOnly => false;

		bool IPackingParent.IsAutoPrintAllowed => false;

		bool IPackingParent.IsScanEventsVisible => false;

		ZString IPackingParent.CarrierServiceLevelCode(PkgPackage package) => ZString.Empty;

		OrgHeader IPackingParent.CarrierBookingAgent => null;

		ZString IPackingParent.TransportReference { get => ZString.Empty; set { } }

		OrgHeader IPackingParent.GetCarrier(PkgPackage package) => null;

		bool IPackingParent.IsLoosePackageIDsSupported => false;

		void IPackingParent.BeforeUnpackingPackages(IReadOnlyList<PkgPackage> packages)
		{
		}

		bool IPackingParent.IsUXMLEventParent(IXmlEventValueObject xmlEvent) => false;

		IEnumerable<KeyValuePair<TypeWithDescription, IZType>> IPackingParent.GetAdditionalEventContextValuesFromParent() => Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();

		ParentJobType IPackingParent.ParentJobType => ParentJobType.None;

		PackageSequenceType IPackingParent.PackageSequenceType => PackageSequenceType.Standard;

		bool IPackingParent.CanReleasePackage(PkgPackage package) => false;

		ZString IPackingParent.GetCannotReleasePackageMessage(PkgPackage package) => ZString.Empty;

		void IPackingParent.OnPackageBookedViaRTUS(ZDateTime sentDateTime)
		{
		}

		NotificationTypes IPackingParent.NotificationTypeForInvalidContainerNumber => NotificationTypes.None;

		#endregion

		#region PackableItems

		[ChildEditable]
		public CusPackableItemCollection PackableItems
		{
			get
			{
				if (packableItems == null)
				{
					packableItems = GetPackageCollectionCore();
					RegisterEditableChildObject(packableItems);
				}
				return packableItems;
			}
		}
		CusPackableItemCollection packableItems;

		protected virtual CusPackableItemCollection GetPackageCollectionCore() => new CusPackableItemCollection(this);

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CUL_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => CUL_JE.IsEmpty ? typeof(BaseJobComInvoiceHeader) : typeof(BaseJobDeclaration);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)(CUL_JE.IsEmpty ? CUL_JZInfo : CUL_JEInfo);
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(typeof(CusPackableItem), CusPackableItemSchema.CUI_CUL);
			}
		}

		#endregion

		[ResourceStringData("643056B2-534A-431B-AFBB-E07D45C67608", Caption = "Packing List #")]
		public override ZString CUL_PackingListNumber { get => base.CUL_PackingListNumber; set => base.CUL_PackingListNumber = value; }

		[ResourceStringData("E3656629-9271-4007-A0B3-6C796BB9C3CA", Caption = "Packing List Date")]
		public override ZDate CUL_PackingListDate { get => base.CUL_PackingListDate; set => base.CUL_PackingListDate = value; }

		[ResourceStringData("C508FD0E-0BC1-4A47-A4D7-2B3C69B118F4", Caption = "Remarks")]
		public override ZString CUL_Remarks { get => base.CUL_Remarks; set => base.CUL_Remarks = value; }

		[ResourceStringData("Enterprise.Customs.Business.CusPackingList|CUL_Description", Caption = "Goods Description", ShortCaption = "Goods Desc.")]
		public override ZString CUL_Description { get => base.CUL_Description; set => base.CUL_Description = value; }

		[ResourceStringData("Enterprise.Customs.Business.CusPackingList|CUL_CustomAttribute1", Caption = "Custom Attribute 1", ShortCaption = "Custom Attr. 1")]
		public override ZString CUL_CustomAttribute1 { get => base.CUL_CustomAttribute1; set => base.CUL_CustomAttribute1 = value; }

		[ResourceStringData("Enterprise.Customs.Business.CusPackingList|CUL_CustomAttribute2", Caption = "Custom Attribute 2", ShortCaption = "Custom Attr. 2")]
		public override ZString CUL_CustomAttribute2 { get => base.CUL_CustomAttribute2; set => base.CUL_CustomAttribute2 = value; }

		[ResourceStringData("Enterprise.Customs.Business.CusPackingList|CUL_CustomFlag1", Caption = "Custom Flag 1")]
		public override ZBool CUL_CustomFlag1 { get => base.CUL_CustomFlag1; set => base.CUL_CustomFlag1 = value; }

		[ResourceStringData("Enterprise.Customs.Business.CusPackingList|CUL_CustomFlag2", Caption = "Custom Flag 2")]
		public override ZBool CUL_CustomFlag2 { get => base.CUL_CustomFlag2; set => base.CUL_CustomFlag2 = value; }

		[ResourceStringData("Enterprise.Customs.Business.CusPackingList|CUL_CustomDate1", Caption = "Custom Date 1")]
		public override ZDateTime CUL_CustomDate1 { get => base.CUL_CustomDate1; set => base.CUL_CustomDate1 = value; }

		[ResourceStringData("Enterprise.Customs.Business.CusPackingList|CUL_CustomDate2", Caption = "Custom Date 2")]
		public override ZDateTime CUL_CustomDate2 { get => base.CUL_CustomDate2; set => base.CUL_CustomDate2 = value; }

		[ResourceStringData("Enterprise.Customs.Business.CusPackingList|CUL_CustomDecimal1", Caption = "Custom Decimal 1")]
		public override ZDecimal CUL_CustomDecimal1 { get => base.CUL_CustomDecimal1; set => base.CUL_CustomDecimal1 = value; }

		[ResourceStringData("Enterprise.Customs.Business.CusPackingList|CUL_CustomDecimal2", Caption = "Custom Decimal 2")]
		public override ZDecimal CUL_CustomDecimal2 { get => base.CUL_CustomDecimal2; set => base.CUL_CustomDecimal2 = value; }

		[ResourceStringData("Enterprise.Customs.Business.CusPackingList|CUL_PackageDescription", Caption = "Package Description")]
		public override ZString CUL_PackageDescription { get => base.CUL_PackageDescription; set => base.CUL_PackageDescription = value; }

		public ZString TotalPackedQty
		{
			get
			{
				if (totalPackedQty == null)
				{
					totalPackedQty = new CachedProperty<ZString>(Factory, delegate
					{
						var packedItemGroups = PackageJob.Packages.Cast<CusPackage>().SelectMany(x => PackableItems.Cast<CusPackableItem>().Select(i => new { PackableUQ = i.CUI_PackableUQ, PackedQty = x.GetCustomsPackedQty(i) })).Where(c => c.PackedQty > 0).GroupBy(by => by.PackableUQ).OrderBy(o => o.Key);
						return string.Join(", ", packedItemGroups.Select(g => ZString.Format("{0:#0.00} {1}", g.Sum(i => i.PackedQty), g.Key)));
					});
				}
				return totalPackedQty.Value;
			}
		}
		CachedProperty<ZString> totalPackedQty;

		public ZPropertyInfo TotalPackedQtyInfo => GetZPropertyInfo(Schema.TotalPackedQty);

		[List(nameof(Lookups) + "." + nameof(CusPackingListLookups.WeightUQs))]
		public ZString TotalNetWeightUQ => Constants.Weight.Kilograms;

		public ZDecimal TotalNetWeight
		{
			get
			{
				if (cachedTotalNetWeight == null)
				{
					cachedTotalNetWeight = new CachedProperty<ZDecimal>(Factory, () =>
					{
						var packableItemsTotalNetWeight = PackableItems?.Cast<CusPackableItem>().Sum(c => Core.Constants.Weight.ConvertSafe(c.TotalPackedNetWeight, c.CUI_NetWeightUQ, Core.Constants.Weight.Kilograms)) ?? ZDecimal.Zero;

						if (packableItemsTotalNetWeight <= ZDecimal.Zero)
						{
							packableItemsTotalNetWeight = PackageJob?.Packages?.Cast<CusPackage>().Sum(x => Core.Constants.Weight.ConvertSafe(x.NetWeight, x.KP_WeightUQ, Core.Constants.Weight.Kilograms)) ?? ZDecimal.Zero;
						}
						return packableItemsTotalNetWeight;
					});
				}
				return cachedTotalNetWeight.Value;
			}
		}
		CachedProperty<ZDecimal> cachedTotalNetWeight;

		public ZPropertyInfo TotalNetWeightInfo => GetZPropertyInfo(Schema.TotalNetWeight);

		[List(nameof(Lookups) + "." + nameof(CusPackingListLookups.WeightUQs))]
		public ZString TotalGrossWeightUQ => Constants.Weight.Kilograms;

		public ZDecimal TotalGrossWeight
		{
			get
			{
				if (cachedTotalGrossWeight == null)
				{
					cachedTotalGrossWeight = new CachedProperty<ZDecimal>(Factory, () => { return PackageJob.Packages.Cast<CusPackage>().Sum(c => Core.Constants.Weight.ConvertSafe(c.KP_Weight, c.KP_WeightUQ, Core.Constants.Weight.Kilograms)); });
				}
				return cachedTotalGrossWeight.Value;
			}
		}
		CachedProperty<ZDecimal> cachedTotalGrossWeight;

		public ZPropertyInfo TotalGrossWeightInfo => GetZPropertyInfo(Schema.TotalGrossWeight);

		#region DocumentSupporter
		public DocumentSupporter DocumentSupporter => documentSupporter ?? (documentSupporter = CreateNewDocumentSupporter());

		DocumentSupporter documentSupporter;

		protected virtual DocumentSupporter CreateNewDocumentSupporter() => new CusPackingListDocumentSupporter(this);

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter() => new JobInvoicingEDocsProviderSupporter(this);

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Enterprise.Core.Constants.DocManagerCodes.CustomsPackingList));
		DocManagerInfo docManagerInfo;
		#endregion

		#region IPackingParentCustomPackTypes
		IEnumerable<ZString> IPackingParentCustomPackTypes.PackTypesToExclude => new ZString[] { Core.Constants.PkgUnit.Container };
		#endregion

		#region ISequenceNumberHeader Members
		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => new TypedEnumerable<ISequenceNumberLine>(PackableItems);

		IDisposable GetPackableItemSequenceNumberRenumberingSuspender()
		{
			return PackableItemSequenceNumberGenerator.GetLineNumberSuspender();
		}

		internal ShortSequenceNumberGenerator PackableItemSequenceNumberGenerator => fPackableItemSequenceNumberGenerator ?? (fPackableItemSequenceNumberGenerator = new ShortSequenceNumberGenerator(this));

		ShortSequenceNumberGenerator fPackableItemSequenceNumberGenerator;
		#endregion

		public void AddDefaultPackageIfNeeded()
		{
			var packages = PackageJob.Packages;
			if (PackageJob.Packages.Count == 0)
			{
				packages.AddNew();
			}
		}

		#region ICustomLabelsConfigOrgProvider
		OrgHeader ICustomLabelsConfigOrgProvider.ConfigOrg
		{
			get
			{
				if (IsDeleted || Declaration == null)
				{
					return null;
				}
				return Declaration.Supplier;
			}
		}

		event EventHandler ICustomLabelsConfigOrgProvider.ConfigOrgChanged
		{
			add
			{
				if (Declaration != null)
				{
					Declaration.JE_OH_SupplierInfo.ValueChanged += value;
				}
			}

			remove
			{
				if (Declaration != null)
				{
					Declaration.JE_OH_SupplierInfo.ValueChanged -= value;
				}
			}
		}
		#endregion
	}
}
