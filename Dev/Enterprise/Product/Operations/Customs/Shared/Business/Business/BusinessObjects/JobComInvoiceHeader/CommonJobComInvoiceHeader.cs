using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

[assembly: ClusterKeyMetaData(typeof(CommonJobComInvoiceHeader), ParentTableName = JobDeclarationSchema.Constants.TableName, ParentFkColumnName = nameof(CommonJobComInvoiceHeader.JZ_JE), TableName = JobComInvoiceHeaderSchema.Constants.TableName)]

namespace Enterprise.Customs.Business
{
	public abstract class CommonJobComInvoiceHeader : AutoJobComInvoiceHeader,
		Integration.Customs.Shared.ICommonJobComInvoiceHeader,
		ITopLevelBizOProviderForJobDocAddress,
		IClusterKeyWorker,
		IClusterKeyMaster,
		IDataModelSupporter
	{
		protected CommonJobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CommonJobComInvoiceHeaderTypeDecider TypeDecider = new CommonJobComInvoiceHeaderTypeDecider();

		public void MarkAsNeedingValidationForMajorDataChange()
		{
			if (!IsMarkingAsNeedingValidationSuspended)
			{
				MarkAsNeedingValidationForMajorDataChangeCore();
			}
		}

		protected virtual void MarkAsNeedingValidationForMajorDataChangeCore()
		{
			base.MarkAsNeedingValidationIncludingChildren();
		}

		#region JobDeclaration
		public BaseJobDeclaration JobDeclaration
		{
			get
			{
				if (OverrideParent != null)
				{
					return OverrideParent;
				}
				if (fJobDeclaration == null || !IsDeleted && (fJobDeclaration.PK != JZ_JE && !JZ_JE.IsEmpty))
				{
					fJobDeclaration = Factory.Load<BaseJobDeclaration>(JZ_JE);
				}
				if (fJobDeclaration == null && !HiddenOriginalParentGuid.IsEmpty)
				{
					fJobDeclaration = Factory.Load<BaseJobDeclaration>(HiddenOriginalParentGuid);
				}
				return fJobDeclaration != null && !fJobDeclaration.IsDeleted ? fJobDeclaration : null;
			}
		}
		BaseJobDeclaration fJobDeclaration;

		internal BaseJobDeclaration OverrideParent
		{
			private get { return fOverrideParent; }
			set
			{
				if (fOverrideParent != null && value != null && fOverrideParent != value)
				{
					throw new DeveloperNotificationException("Parent has already been overriden");
				}
				else
				{
					fOverrideParent = value;
					ResetOnDeclarationChanged();
				}
			}
		}
		BaseJobDeclaration fOverrideParent;

		#endregion

		//TODO: Remove HiddenOriginalParentGuid when RemoveCollectionRelationships/delete problem fixed in Z
		internal ZGuid HiddenOriginalParentGuid;
		[BusinessObjectTestExclude]
		public override ZGuid JZ_JE
		{
			get { return base.JZ_JE; }
			set
			{
				var oldValue = JZ_JE;
				if (value.IsEmpty)
				{
					HiddenOriginalParentGuid = JZ_JE;
				}
				base.JZ_JE = value;
				if (!IsCopying && oldValue != JZ_JE)
				{
					ResetOnDeclarationChanged();
					if (!IsDataChangeSuspendedByFakeDeclaration)
					{
						MarkAsNeedingValidationForMajorDataChange();
					}
				}
			}
		}

		protected virtual void ResetOnDeclarationChanged()
		{
		}

		public override GlbBranch Branch
		{
			get
			{
				return JZ_GB.IsEmpty && JobDeclaration is BaseJobDeclaration declaration ? declaration.Branch : base.Branch;
			}
		}

		[LightValidationTestExempt]//setting this to ZGuid.NewGuid() causes JobDeclaration.JobComInvoiceGroupHeaders to have no elements in it.
		public override ZGuid JZ_JZ_GroupInvoiceFK
		{
			get { return base.JZ_JZ_GroupInvoiceFK; }
			set
			{
				bool hasChanged = base.JZ_JZ_GroupInvoiceFK != value;
				BaseJobComInvoiceGroupHeader oldGroupHeader = GroupHeader;
				base.JZ_JZ_GroupInvoiceFK = value;
				if (hasChanged && !IsDataChangeSuspendedByFakeDeclaration)
				{
					MarkAsNeedingValidationForMajorDataChange();
					RefreshInvoicesRecursively(oldGroupHeader);
					RefreshInvoicesRecursively(GroupHeader);

					JobDeclaration?.MarkApportionmentDirty();

					InvoiceStructureChangeEvent.OnInvoiceStructureChanged(Factory);
				}
			}
		}

		[MeasureUnit(Schema.JZ_NetWeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal JZ_NetWeight
		{
			get { return base.JZ_NetWeight; }
			set { base.JZ_NetWeight = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.JZ_WeightUQ_List))]
		public override ZString JZ_NetWeightUQ
		{
			get { return base.JZ_NetWeightUQ; }
			set { base.JZ_NetWeightUQ = value; }
		}

		[MeasureUnit(Schema.JZ_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal JZ_Weight
		{
			get { return base.JZ_Weight; }
			set { base.JZ_Weight = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.JZ_WeightUQ_List))]
		public override ZString JZ_WeightUQ
		{
			get { return base.JZ_WeightUQ; }
			set { base.JZ_WeightUQ = value; }
		}

		[MeasureUnit(Schema.JZ_VolumeUQ, MeasureUnitType.Volume)]
		public override ZDecimal JZ_Volume
		{
			get { return base.JZ_Volume; }
			set { base.JZ_Volume = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.JZ_VolumeUQ_List))]
		public override ZString JZ_VolumeUQ
		{
			get { return base.JZ_VolumeUQ; }
			set { base.JZ_VolumeUQ = value; }
		}

		public override ZString JZ_DataModel
		{
			get { return base.JZ_DataModel; }
			set
			{
				this.ReportDataModelErrorIfNeeded(JZ_DataModelInfo, value);
				base.JZ_DataModel = value;
			}
		}

		public override void Delete()
		{
			DetachFromAdditionalDeclarations();
			base.Delete();
			InvoiceStructureChangeEvent.OnInvoiceStructureChanged(Factory);
		}

		void RefreshInvoicesRecursively(BaseJobComInvoiceGroupHeader groupHeader)
		{
			if (groupHeader != null && groupHeader.AllJobComInvoiceLines != null)
			{
				groupHeader.AllJobComInvoiceLines.Rebuild();
				RefreshInvoicesRecursively(groupHeader.GroupHeader);
			}
		}

		public BaseJobComInvoiceGroupHeader GroupHeader
		{
			get
			{
				if (fGroupHeader == null || fGroupHeader.IsDeleted || fGroupHeader.PK != JZ_JZ_GroupInvoiceFK)
				{
					fGroupHeader = Factory.Load<BaseJobComInvoiceGroupHeader>(JZ_JZ_GroupInvoiceFK);
				}
				return fGroupHeader != null && !fGroupHeader.IsDeleted ? fGroupHeader : null;
			}
		}
		BaseJobComInvoiceGroupHeader fGroupHeader;

		protected IncoTermAndCustomsChargeFactory fIncoTermAndChargeFactory;
		public bool NeedToGetNewIncoTermAndChargeFactory;
		public IncoTermAndCustomsChargeFactory IncoTermAndChargeFactory
		{
			get
			{
				if (fIncoTermAndChargeFactory == null || NeedToGetNewIncoTermAndChargeFactory)
				{
					var declaration = JZ_JE.IsEmpty ? null : JobDeclaration;
					IApportionInvoiceHolder holder = declaration != null && declaration.IsPersistent ? declaration : null;
					fIncoTermAndChargeFactory = holder != null ? holder.IncoTermAndChargeFactory : IncoTermAndCustomsChargeFactory.GetByCountryCode(GetStandaloneIncoTermAndChargeFactoryCountryContext());
					NeedToGetNewIncoTermAndChargeFactory = false;
				}
				return fIncoTermAndChargeFactory;
			}
		}

		public string GetCustomsChargeTypeListCacheKey(ChargeParentTypes parentTypes)
		{
			var declaration = JobDeclaration;
			IApportionInvoiceHolder holder = declaration != null && declaration.IsPersistent ? declaration : null;
			var countryContext = holder == null ? GetStandaloneIncoTermAndChargeFactoryCountryContext() : holder.CountryContext;

			return "CustomsChargeTypeList_" + countryContext + parentTypes.ToString();
		}

		protected virtual string GetStandaloneIncoTermAndChargeFactoryCountryContext()
		{
			var branch = Branch ?? GlbBranch.CurrentBranch;
			return branch?.Company.GC_RN_NKCountryCode;
		}

		protected CodeDescriptionPairList GetCustomsChargeTypeList(ChargeParentTypes parentTypes)
		{
			return Factory.GetCachedValue(GetCustomsChargeTypeListCacheKey(parentTypes), () => IncoTermAndChargeFactory.GetChargeTypeList(parentTypes));
		}

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateDataModelIfNeeded();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			using (GetValidationSuspender())
			{
				JZ_IncoTerm = ZString.Empty;
			}
		}

		#region DataChangeByFakeDeclarationSuspender
		public bool IsDataChangeSuspendedByFakeDeclaration
		{
			get { return suspendDataChangeByFakeDeclarationIndex > 0; }
		}
		int suspendDataChangeByFakeDeclarationIndex;

		internal IDisposable SuspendDataChangeByFakeDeclaration()
		{
			return new DataChangeByFakeDeclarationSuspender(this);
		}

		class DataChangeByFakeDeclarationSuspender : IDisposable
		{
			public DataChangeByFakeDeclarationSuspender(CommonJobComInvoiceHeader invoice)
			{
				this.invoice = invoice;
				invoice.suspendDataChangeByFakeDeclarationIndex++;
			}

			public void Dispose()
			{
				invoice.suspendDataChangeByFakeDeclarationIndex--;
			}

			readonly CommonJobComInvoiceHeader invoice;
		}
		#endregion

		#region Additional Declarations Support

		public virtual bool SupportAdditionalDeclarations
		{
			get
			{
				return false;
			}
		}

		public virtual void AttachToAdditionalDeclaration(BaseJobDeclaration declaration)
		{
			if (SupportAdditionalDeclarations)
			{
				if (!RelatedDeclarationGenPivots.Contains(declaration))
				{
					RelatedDeclarationGenPivots.AddPivotForDeclaration(declaration);
					OnAttachedToAdditionalDeclaration(declaration);
				}
			}
		}

		public virtual void DetachFromAdditionalDeclaration(BaseJobDeclaration declaration)
		{
			if (SupportAdditionalDeclarations)
			{
				if (RelatedDeclarationGenPivots.Contains(declaration))
				{
					RelatedDeclarationGenPivots.DeletePivotFor(declaration);
					OnDetachedFromAdditionalDeclaration(declaration);
				}
			}
		}

		protected virtual void OnAttachedToAdditionalDeclaration(BaseJobDeclaration declaration)
		{
		}

		protected virtual void OnDetachedFromAdditionalDeclaration(BaseJobDeclaration declaration)
		{
		}

		public BaseJobDeclaration[] AdditionalDeclarations
		{
			get
			{
				return SupportAdditionalDeclarations ? RelatedDeclarationGenPivots.Select(p => ((RelatedDeclarationGenPivot)p).JobDeclaration).ToArray()
					: Array.Empty<BaseJobDeclaration>();
			}
		}

		[ChildEditable(true)]
		IRelatedDeclarationGenPivotCollection RelatedDeclarationGenPivots
		{
			get
			{
				if (relatedDeclarationGenPivots == null)
				{
					relatedDeclarationGenPivots = GetNewRelatedDeclarationGenPivots();
					((BusinessObjectCollection)relatedDeclarationGenPivots).Load();
					RegisterEditableChildObject(relatedDeclarationGenPivots);
				}

				return relatedDeclarationGenPivots;
			}
		}

		protected abstract IRelatedDeclarationGenPivotCollection GetNewRelatedDeclarationGenPivots();

		IRelatedDeclarationGenPivotCollection relatedDeclarationGenPivots;

		public BaseJobDeclaration FirstAdditionalDeclaration
		{
			get
			{
				var pivot = SupportAdditionalDeclarations ? RelatedDeclarationGenPivots.FirstOrDefault() : null;
				return pivot == null ? null : ((RelatedDeclarationGenPivot)pivot).JobDeclaration;
			}
		}

		void DetachFromAdditionalDeclarations()
		{
			if (SupportAdditionalDeclarations)
			{
				RelatedDeclarationGenPivots.RemoveAndDeleteAll();
			}
		}

		#endregion

		public BusinessObject GetTopBusinessObject() => JobDeclaration;

		#region IClusterKeyWorker, IClusterKeyMaster

		public sealed override ZInt JZ_ClusterKey
		{
			get => base.JZ_ClusterKey;
			set
			{
				if (base.JZ_ClusterKey != value)
				{
					this.CheckCanSetMasterClusterKey();
					base.JZ_ClusterKey = value;
				}
			}
		}

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)JZ_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(BaseJobDeclaration);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)JZ_JEInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(typeof(JobComInvoiceHeaderRefs), JobComInvoiceHeaderRefsSchema.J2_JZ);
				yield return new ClusterKeyChildInfo(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_JZ);
				yield return new ClusterKeyChildInfo(typeof(CusPackingList), CusPackingListSchema.CUL_JZ);
				yield return new ClusterKeyChildInfo(ObjectFactory.GetType<Integration.Customs.AU.IQuarantineExdocHeader>(), QuarantineExDocHeaderSchema.QH_JZ);
				yield return new ClusterKeyChildInfo(ObjectFactory.GetType<Integration.Customs.CA.IJobCAComInvoiceHeader>(), JobCAComInvoiceHeaderSchema.CAZ_JZ);
			}
		}

		#endregion

		#region IDataModelSupporter

		public void PopulateDataModelIfNeeded()
		{
			if (JobDeclaration is BaseJobDeclaration declaration && declaration.IsPersistent)
			{
				this.PopulateDataModelFromParentIfNeeded(declaration);
			}
			else
			{
				this.PopulateDataModelFromCountryCodeIfNeeded(base.Branch?.Company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			}
		}

		ZString IDataModelSupporter.DataModel { get => JZ_DataModel; set => JZ_DataModel = value; }

		#endregion
	}
}
