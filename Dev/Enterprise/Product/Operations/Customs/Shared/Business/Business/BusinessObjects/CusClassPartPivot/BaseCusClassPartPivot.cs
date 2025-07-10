using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public partial class BaseCusClassPartPivot
		: AutoCusClassPartPivot,
		Integration.Customs.IBaseCusClassPartPivot,
		IPartProvider,
		ICusLineTariffDetailParent,
		ITariffFormatProvider,
		ITypeDeciderContext,
		ISetterSuspenderSupporter,
		IWorkflowTriggerEventSource,
		IDataModelSupporter
	{
		public BaseCusClassPartPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			this.HasChangesChanged += ClearAuditOnChanged;
		}

		public static readonly TypeDecider TypeDecider = new BaseCusClassPartPivotTypeDecider();

		public new partial class Schema : AutoCusClassPartPivot.Schema
		{
			public const string CI_ChildTypeDescription = "CI_ChildTypeDescription";
			public const string CI_FormattedSupplementalTariff = "CI_FormattedSupplementalTariff";
			public const string CI_FormattedTariffNum = "CI_FormattedTariffNum";
		}

		#region New Properties

		public ZString TariffNumber
		{
			get
			{
				var result = CI_TariffNum;
				if (result.IsEmpty)
				{
					result = Classification?.CC_TariffNum ?? GoodsCatalog?.CGC_Tariff ?? ZString.Empty;
				}
				return result;
			}
		}

		protected ZString FormattedTariffNumber
		{
			get
			{
				var result = new ZStringBuilder();
				if (!TariffNumber.IsEmpty)
				{
					var tariffType = CI_ChildType.IsEmpty ? "" : CI_ChildType + ":";
					result.Append(tariffType + TariffNumber + "(" + CI_SupplementalTariff + ")");
				}

				return result.ToString().Replace("()", "");
			}
		}

		public ZString TariffNumbersIncludingComponents
		{
			get { return GetTariffNumbersIncludingComponents(); }
		}

		protected virtual ZString GetTariffNumbersIncludingComponents()
		{
			return FormattedTariffNumber;
		}

		public ZString DutyRateForCurrentCountry
		{
			get { return GetDutyRateForCurrentCountry(); }
		}

		protected virtual ZString GetDutyRateForCurrentCountry()
		{
			return "";
		}

		public ZString TaxRateForCurrentCountry
		{
			get { return GetTaxRateForCurrentCountry(); }
		}

		protected virtual ZString GetTaxRateForCurrentCountry()
		{
			return "";
		}

		#region CI_ChildTypeDescription

		public virtual ZString CI_ChildTypeDescription
		{
			get { return Lookups.ClassificationTypes.GetDescriptionFromCode(CI_ChildType) ?? ZString.Empty; }
		}

		public ZPropertyInfo CI_ChildTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CI_ChildTypeDescription); }
		}

		#endregion

		public ZString LastAuditedUserFullName
		{
			get
			{
				var result = base.CI_LastAuditedUser;
				var staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, base.CI_LastAuditedUser);
				if (staff != null)
				{
					result = string.Format(CultureInfo.CurrentCulture, "{0}({1})", staff.GS_FullName, staff.GS_Code);
				}
				return result;
			}
		}

		#endregion

		#region Override Properties

		public override ZString CI_RN_NKCountryOfOrigin
		{
			get { return base.CI_RN_NKCountryOfOrigin; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CI_RN_NKCountryOfOrigin))
				{
					base.CI_RN_NKCountryOfOrigin = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.PrimaryPreferenceList))]
		public override ZString CI_PrimaryPreference
		{
			get { return base.CI_PrimaryPreference; }
			set { base.CI_PrimaryPreference = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.RelatedIndicatorList))]
		public override ZString CI_RelatedIndicator
		{
			get { return base.CI_RelatedIndicator; }
			set { base.CI_RelatedIndicator = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.RelatedOrgs))]
		[RelatedBusinessObject("RelatedOrganisation")]
		public override ZGuid CI_OH
		{
			get { return base.CI_OH; }
			set { base.CI_OH = value; }
		}

		public OrgHeader RelatedOrganisation => Factory.Load<OrgHeader>(CI_OH);
		public ZString RelatedOrganisationDescription => RelatedOrganisation?.OH_Code ?? "NONE";

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.ClassificationTypes))]
		public override ZString CI_ChildType
		{
			get { return base.CI_ChildType; }
			set
			{
				var oldValue = CI_ChildType;
				base.CI_ChildType = value.Left(CI_ChildTypeInfo.MaxLength);
				if (!IsCopying && oldValue != CI_ChildType && !IsValidationSuspended)
				{
					(Part as OrgSupplierPart)?.GetPivots<BaseCusClassPartPivot>(CI_RN_NKCountry).Where(o => o.PK != PK).ForEach(o => o.Validation.ValidateCI_ChildType());
				}
			}
		}

		[ReadOnly(true)]
		public override ZString CI_RN_NKCountry
		{
			get { return base.CI_RN_NKCountry; }
			set
			{
				var hasChanged = value != CI_RN_NKCountry;
				var oldPart = hasChanged ? Part as OrgSupplierPart : null;
				var oldParent = hasChanged ? Parent : null;
				if (hasChanged)
				{
					classTypeProvider = null;
				}
				base.CI_RN_NKCountry = value;
				UpdatePivotsOnPart(oldPart, oldParent);
			}
		}

		[ReadOnly(true)]
		public override ZGuid CI_OP
		{
			get { return base.CI_OP; }
			set
			{
				if (!IsSettingCI_OPSuspended)
				{
					var hasChanged = value != CI_OP;
					var oldPart = hasChanged ? Part as OrgSupplierPart : null;
					var oldParent = hasChanged ? Parent : null;
					base.CI_OP = value;
					UpdatePivotsOnPart(oldPart, oldParent);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Staffs))]
		public override ZString CI_LastAuditedUser
		{
			get => base.CI_LastAuditedUser;
			set => base.CI_LastAuditedUser = value;
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var code = Classification?.CC_LookupCode ?? CI_TariffNum;
				return Res.GetString("7BD64515-43EF-49C3-B2D5-7E629D4229EA", "Classification ({0} {1})", CI_ChildType, code);
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[ResourceStringData("14dc27b9-2452-4174-ab4c-44867d03e4ab", Caption = "Classification Description", ShortCaption = "Class. Desc.")]
		public override ZString CI_Description { get => base.CI_Description; set => base.CI_Description = value; }

		#endregion

		IDisposable GetSettingCI_OPSuspender()
		{
			return new SettingCI_OPSuspender(this);
		}

		bool IsSettingCI_OPSuspended
		{
			get { return settingCI_OPSuspenderIndex > 0; }
		}

		byte settingCI_OPSuspenderIndex;
		class SettingCI_OPSuspender : IDisposable
		{
			public SettingCI_OPSuspender(BaseCusClassPartPivot pivot)
			{
				this.pivot = pivot;
				pivot.settingCI_OPSuspenderIndex++;
			}

			readonly BaseCusClassPartPivot pivot;

			public void Dispose()
			{
				pivot.settingCI_OPSuspenderIndex--;
			}
		}

		bool updatePivotInProgress;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void UpdatePivotsOnPart(OrgSupplierPart oldPart, BaseCusClassPartPivot oldParent)
		{
			if (!IsCopying && !updatePivotInProgress)
			{
				try
				{
					updatePivotInProgress = true;
					var parentPK = CI_CI_Parent;
					var isCurrentCountry = CI_RN_NKCountry == GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					var oldPartPivots = isCurrentCountry && oldPart != null && oldPart.HasLoadedPivotsForBinding ? oldPart.PivotsForBinding : null;
					if (oldPartPivots != null && oldPartPivots.Contains(this) && (!parentPK.IsEmpty || !MatchesFilter(oldPartPivots.CompleteFilter)))
					{
						using (GetSettingCI_OPSuspender())
						{
							oldPartPivots.RemoveFromRelationship(this);
						}
					}
					if (!IsDeleting && parentPK.IsEmpty)
					{
						var currentPart = Part as OrgSupplierPart;
						var pivots = isCurrentCountry && currentPart != null && currentPart.HasLoadedPivotsForBinding ? currentPart.PivotsForBinding : null;
						if (pivots != null && !pivots.Contains(this) && MatchesFilter(pivots.CompleteFilter))
						{
							pivots.Add(this);
						}
					}
					var currentParent = Parent;
					var children = currentParent != null && currentParent.HasLoadedChildren ? currentParent.Children : null;
					if (children != null && CI_RN_NKCountry == children.CountryCode && !children.Contains(this) && MatchesFilter(children.CompleteFilter))
					{
						children.Add(this);
					}
					if (oldParent != null && oldParent.PK != CI_CI_Parent)
					{
						var oldParentChildren = oldParent.HasLoadedChildren ? oldParent.Children : null;
						if (oldParentChildren != null && CI_RN_NKCountry == oldParentChildren.CountryCode && oldParentChildren.Contains(this) && !MatchesFilter(oldParentChildren.CompleteFilter))
						{
							oldParentChildren.RemoveFromRelationship(this);
						}
					}
				}
				finally
				{
					updatePivotInProgress = false;
				}
			}
		}

		[BusinessObjectTestExclude]
		public override ZString CI_SupplementalTariff
		{
			get => base.CI_SupplementalTariff;
			set { base.CI_SupplementalTariff = IsCopying ? value : FormatTariffForSaving(value).Left(CI_SupplementalTariffInfo.MaxLength); }
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(IsTariffNumReadOnly))]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Tariffs))]
		public override ZString CI_TariffNum
		{
			get { return base.CI_TariffNum; }
			set { base.CI_TariffNum = IsCopying ? value : FormatTariffForSaving(value).Left(CI_TariffNumInfo.MaxLength); }
		}

		protected virtual ZString FormatTariffForSaving(ZString unformattedTariff) => CurrentTariffFormatter.Format(unformattedTariff);

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(IsTariffNumReadOnly))]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Tariffs))]
		public virtual ZString CI_FormattedTariffNum
		{
			get { return CurrentTariffFormatter.DisplayFormat(CI_TariffNum); }
			set { CI_TariffNum = value; }
		}

		public ZPropertyInfo CI_FormattedTariffNumInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CI_FormattedTariffNum, x => CI_TariffNumInfo); }
		}

		protected internal bool IsTariffNumReadOnly
		{
			get { return IsTariffNumReadOnlyCore; }
		}

		protected virtual bool IsTariffNumReadOnlyCore
		{
			get { return !CI_CC.IsEmpty; }
		}

		[BusinessObjectTestExclude]
		public virtual ZString CI_FormattedSupplementalTariff
		{
			get { return CurrentTariffFormatter.DisplayFormat(CI_SupplementalTariff); }
			set { CI_SupplementalTariff = value; }
		}

		[ReadOnly(true)]
		public override ZGuid CI_CI_Parent
		{
			get { return base.CI_CI_Parent; }
			set
			{
				var changed = value != CI_CI_Parent;
				var oldPart = changed ? Part as OrgSupplierPart : null;
				var oldParent = changed ? Parent : null;
				var oldValue = CI_CI_Parent;
				base.CI_CI_Parent = value;
				if (oldValue != CI_CI_Parent)
				{
					if (!CI_CI_Parent.IsEmpty && Parent == this)
					{
						ErrorReporter.ReportOnce("CusClassPartPivot.CI_CI_Parent is pointing to itself.");
					}
					UpdatePivotsOnPart(oldPart, oldParent);
				}
			}
		}

		public BaseCusClassPartPivot Parent
		{
			get { return CI_CI_Parent.IsEmpty ? null : Factory.Load<BaseCusClassPartPivot>(CI_CI_Parent); }
		}

		public ZPropertyInfo CI_FormattedSupplementalTariffInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CI_FormattedSupplementalTariff, x => CI_SupplementalTariffInfo); }
		}

		#region AddInfo & NAddInfo items

		[BusinessObjectTestExclude]
		public override ZString CI_AddInfo
		{
			get => this is INAddInfoSupporter ? AddInfoParser.ConcatAddInfoStrings(base.CI_AddInfo, CI_NAddInfo) : base.CI_AddInfo;
			set
			{
				if (this.GetBaseAddInfoWithNAddInfoSupport() is BaseAddInfo addInfo)
				{
					var addInfoStrings = addInfo.SplitAddInfoString(value);

					base.CI_AddInfo = addInfoStrings.Item1;
					CI_NAddInfo = addInfoStrings.Item2;
				}
				else
				{
					base.CI_AddInfo = value;
				}
			}
		}

		#endregion

		[RelatedBusinessObject("Classification")]
		[ReadOnlyMember(nameof(CI_CC_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.ClassificationList))]
		public override ZGuid CI_CC
		{
			get { return base.CI_CC; }
			set
			{
				bool hasChanged = CI_CC != value;
				base.CI_CC = value;
				if (hasChanged && CI_CC.IsValid)
				{
					var classCached = Classification;
					if (classCached != null && CI_RN_NKCountry != classCached.CC_RN_NKCountryCode)
					{
						ErrorReporter.ReportOnce("Invalid part pivot", string.Format("Part ({0}) has a {1} pivot with a {2} classification attached.", (Part != null ? Part.OP_PartNum : ZString.Empty), CI_RN_NKCountry, classCached.CC_RN_NKCountryCode));
					}
				}
			}
		}

		protected internal bool CI_CC_ReadOnly
		{
			get { return CI_CC_ReadOnlyCore; }
		}

		protected virtual bool CI_CC_ReadOnlyCore
		{
			get { return !CI_TariffNum.IsEmpty; }
		}

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.GoodsCatalogList))]
		[RelatedBusinessObject(nameof(GoodsCatalog))]
		public override ZGuid CI_CGC_Catalog { get => base.CI_CGC_Catalog; set => base.CI_CGC_Catalog = value; }

		public BaseCusGoodsCatalog GoodsCatalog => Factory.Load<BaseCusGoodsCatalog>(CI_CGC_Catalog);

		public BaseCusClassification Classification
		{
			get { return CI_CC.IsEmpty ? null : Factory.Load<BaseCusClassification>(CI_CC); }
		}

		public MasterFiles.Business.OrgSupplierPart Part
		{
			get
			{
				return (MasterFiles.Business.OrgSupplierPart)Factory.Load(OrgSupplierPartTypeDecider.GetOrgSupplierPartType(CI_RN_NKCountry), CI_OP);
			}
		}

		bool wasInDatabaseBeforeSave;
		public override void OnLoaded()
		{
			base.OnLoaded();
			wasInDatabaseBeforeSave = true;
		}

		public virtual void OnPartNumberChanged(ZString oldPartNum, ZString newPartNum)
		{
		}

		public void LogIfPropertyValueChange(ZString propertyDisplayName, ZString oldValue, ZString newValue)
		{
			if (oldValue != newValue && !propertyDisplayName.IsEmpty)
			{
				if (!oldValue.IsEmpty && !newValue.IsEmpty)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, ZString.Format("{0} '{1}' changed to '{2}'", propertyDisplayName, oldValue, newValue), ZDateTimeOffset.Now, false);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				else if (oldValue.IsEmpty)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, ZString.Format("{0} '{1}' added", propertyDisplayName, newValue), ZDateTimeOffset.Now, false);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				else if (newValue.IsEmpty)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, ZString.Format("{0} '{1}' removed", propertyDisplayName, oldValue), ZDateTimeOffset.Now, false);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		protected void ClearAuditOnChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (e.ObjectJustWasChanged && IsInDatabase && !IsDeleted
				&& !CI_LastAuditedUser.IsEmpty && !CI_LastAuditedUserInfo.HasChanges && !CI_LastAuditedDateInfo.HasChanges)
			{
				CI_LastAuditedUser = ZString.Empty;
				CI_LastAuditedDate = ZDateTime.Empty;
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded && wasInDatabaseBeforeSave)
			{
				Factory.ForcePublishForDataRefreshByTableName(this);
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			Attributes1.DeleteAll();
			Attributes2.DeleteAll();
			Attributes3.DeleteAll();
			Children?.RemoveAndDeleteAll();
			(Part as OrgSupplierPart)?.PivotsForBinding.RemoveFromRelationship(this);
			Parent?.Children?.RemoveFromRelationship(this);

			if (SupportCusClassPartPivotRef)
			{
				CusClassPartPivotRefs.DeleteAll();
			}

			if (SupportsAdditionalTariffs)
			{
				CusLineTariffDetails.DeleteAll();
			}

			base.Delete();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CI_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			CI_TariffNum = ZString.Empty;

			var defaultChildType = DefaultChildType;
			if (!defaultChildType.IsEmpty && Lookups.ClassificationTypes.ContainsCode(defaultChildType))
			{
				CI_ChildType = defaultChildType;
			}
		}

		protected virtual ZString DefaultChildType
		{
			get { return GetClassificationTypeProvider().HTBCode; }
		}

		public IClassificationTypeProvider GetClassificationTypeProvider() => ClassificationTypeProvider.GetProviderFor(CI_RN_NKCountry);

		[ChildEditable(true)]
		public CusAttributeFilterCollection Attributes1
		{
			get
			{
				if (attribute1 == null)
				{
					attribute1 = GetNewAttributeFilterCollection(nameof(CusAttributeFilter.AttributeFilterName.AT1));
					RegisterEditableChildObject(attribute1);
				}

				return attribute1;
			}
		}
		CusAttributeFilterCollection attribute1;

		[ChildEditable(true)]
		public CusAttributeFilterCollection Attributes2
		{
			get
			{
				if (attribute2 == null)
				{
					attribute2 = GetNewAttributeFilterCollection(nameof(CusAttributeFilter.AttributeFilterName.AT2));
					RegisterEditableChildObject(attribute2);
				}

				return attribute2;
			}
		}
		CusAttributeFilterCollection attribute2;

		[ChildEditable(true)]
		public CusAttributeFilterCollection Attributes3
		{
			get
			{
				if (attribute3 == null)
				{
					attribute3 = GetNewAttributeFilterCollection(nameof(CusAttributeFilter.AttributeFilterName.AT3));
					RegisterEditableChildObject(attribute3);
				}

				return attribute3;
			}
		}
		CusAttributeFilterCollection attribute3;

		[BusinessObjectTestExclude]
		public ICusClassPartPivotCollection<BaseCusClassPartPivot> Children
		{
			get { return GetCusClassPartPivotCollection(); }
		}

		protected virtual ICusClassPartPivotCollection<BaseCusClassPartPivot> GetCusClassPartPivotCollection()
		{
			return null;
		}

		public bool HasLoadedChildren
		{
			get { return HasLoadedChildrenCore; }
		}

		protected virtual bool HasLoadedChildrenCore
		{
			get { return false; }
		}

		protected virtual TariffFormatter GetTariffFormatter()
		{
			return new TariffFormatter();
		}

		public TariffFormatter CurrentTariffFormatter
		{
			get { return GetTariffFormatter(); }
		}

		ITariffFormatter ITariffFormatProvider.TariffFormatter => CurrentTariffFormatter;

		protected virtual CusAttributeFilterCollection GetNewAttributeFilterCollection(ZString attributeName)
		{
			return new CusAttributeFilterCollection(this, attributeName);
		}

		protected override CusClassPartPivotValidation GetNewValidation()
		{
			return new BaseCusClassPartPivotValidation(this);
		}

		public new BaseCusClassPartPivotValidation Validation
		{
			get { return (BaseCusClassPartPivotValidation)GetNewValidation(); }
		}

		public bool IsImportClassification => CI_ChildType == ClassTypeProvider.HTICode || IsHTB;

		public bool IsExportClassification => IsExportClassificationCore;

		protected virtual bool IsExportClassificationCore => CI_ChildType == ClassTypeProvider.HTECode || IsHTB;

		public bool IsHTB => CI_ChildType == ClassTypeProvider.HTBCode && !CI_ChildType.IsEmpty;

		IClassificationTypeProvider ClassTypeProvider => classTypeProvider ?? (classTypeProvider = ClassificationTypeProvider.GetProviderFor(CI_RN_NKCountry));

		IClassificationTypeProvider classTypeProvider;

		#region CusLineTariffDetails

		[ChildEditable(true)]
		[BusinessObjectTestExclude]
		public ICusLineTariffDetailCollection<CusLineTariffDetail> CusLineTariffDetails
		{
			get
			{
				if (cusLineTariffDetails == null)
				{
					cusLineTariffDetails = GetCusLineTariffDetails();
					if (SupportsAdditionalTariffs)
					{
						cusLineTariffDetails.Load();
						RegisterEditableChildObject(cusLineTariffDetails);
					}
					else
					{
						((ILegacyBusinessObjectCollectionInternals)cusLineTariffDetails).SetOverriddenAdditionalFilter(ZQuery.NoResultQuery);
						cusLineTariffDetails.SetCountedReadOnlyIncludingChildren(true);
					}
				}
				return cusLineTariffDetails;
			}
		}
		ICusLineTariffDetailCollection<CusLineTariffDetail> cusLineTariffDetails;

		protected virtual ICusLineTariffDetailCollection<CusLineTariffDetail> GetCusLineTariffDetails() => new CusLineTariffDetailCollection<CusLineTariffDetail>(this);

		internal protected virtual bool SupportsAdditionalTariffs => false;

		ZString ICusLineTariffDetailParent.CustomsCountryCode => CI_RN_NKCountry;
		ZDateTime ICusLineTariffDetailParent.EffectiveAssessmentDate => ZDateTime.Today;

		void IDataModelSupporter.PopulateDataModelIfNeeded() => this.PopulateDataModelFromCountryCodeIfNeeded(CI_RN_NKCountry);

		ZString IDataModelSupporter.DataModel { get; set; }

		ZString ITariffProvider.Tariff => CI_TariffNum;
		ZPropertyInfo ITariffProvider.TariffInfo => CI_TariffNumInfo;

		#endregion

		#region Clone

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			BaseCusClassPartPivot result = (BaseCusClassPartPivot)base.CloneInternal(args);
			using (result.GetValidationSuspender())
			{
				foreach (CusAttributeFilter attrib in Attributes1)
				{
					result.Attributes1.Add((CusAttributeFilter)attrib.Clone());
				}
				foreach (CusAttributeFilter attrib in Attributes2)
				{
					result.Attributes2.Add((CusAttributeFilter)attrib.Clone());
				}
				foreach (CusAttributeFilter attrib in Attributes3)
				{
					result.Attributes3.Add((CusAttributeFilter)attrib.Clone());
				}
			}
			if (this is INAddInfoSupporter)
			{
				result.CI_AddInfo = CI_AddInfo;
			}
			return result;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			List<string> result = new List<string>(base.GetPropertiesToExcludeFromCloning());

			result.Add(CusClassPartPivotSchema.Constants.CI_LastAuditedDate);
			result.Add(CusClassPartPivotSchema.Constants.CI_LastAuditedUser);
			result.Add(CusClassPartPivotSchema.Constants.CI_OP);
			result.Add(CusClassPartPivotSchema.Constants.CI_CI_Parent);
			result.Add(CusClassPartPivotSchema.Constants.CI_CGC_Catalog);

			return result;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new PivotFetchStrategy(this);
		}

		protected class PivotFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			public PivotFetchStrategy(BaseCusClassPartPivot pivot)
				: base(pivot)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				BaseCusClassPartPivot pivot = (BaseCusClassPartPivot)BusinessObject;
				Factory.AddFetchHint(CusClassificationSchema.PK, pivot.CI_CC);
				if (pivot.SupportCusClassPartPivotRef)
				{
					Factory.AddFetchHint(CusClassPartPivotRefSchema.CIR_CI, BusinessObject.PK);
				}
				Factory.AddFetchHint(CusAttributeFilterSchema.BG_CI, BusinessObject.PK);
			}
		}

		#endregion

		#region CusClassPartPivotRef

		internal protected virtual bool SupportCusClassPartPivotRef
		{
			get { return false; }
		}

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public CusClassPartPivotRefCollection CusClassPartPivotRefs
		{
			get
			{
				if (cusClassPartPivotRefs == null)
				{
					cusClassPartPivotRefs = GetNewCusClassPartPivotRefs();
					if (SupportCusClassPartPivotRef)
					{
						cusClassPartPivotRefs.Load();
						RegisterEditableChildObject(cusClassPartPivotRefs);
					}
					else
					{
						((ILegacyBusinessObjectCollectionInternals)cusClassPartPivotRefs).SetOverriddenAdditionalFilter(ZQuery.NoResultQuery);
						cusClassPartPivotRefs.SetCountedReadOnlyIncludingChildren(true);
					}
				}
				return cusClassPartPivotRefs;
			}
		}
		CusClassPartPivotRefCollection cusClassPartPivotRefs;

		internal bool IsCusClassPartPivotRefsLoaded => cusClassPartPivotRefs != null && cusClassPartPivotRefs.IsLoaded;

		protected virtual CusClassPartPivotRefCollection GetNewCusClassPartPivotRefs()
		{
			return new CusClassPartPivotRefCollection(this);
		}
		#endregion

		#region UniversalTariff

		public virtual TariffView UniversalTariff
		{
			get
			{
				TariffView universalTariff = null;
				if (UseUniversalTariff && !TariffNumber.IsEmpty)
				{
					universalTariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(DefaultDataGroupingForTariffs, UniversalTariffType, TariffNumber, ZDateTime.Today);
				}
				return universalTariff;
			}
		}

		protected internal virtual bool UseUniversalTariff => true;

		public ZString DefaultDataGroupingForTariffs => DefaultDataGroupingForTariffsCore;

		protected virtual ZString DefaultDataGroupingForTariffsCore => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CI_RN_NKCountry);

		protected internal virtual ZString UniversalTariffType => Constants.TariffTypes.HarmonizedSystem;

		#endregion

		public IZZRateSelectionCriteria AllApplicableRatesSelectionCriteria => (allApplicableRatesSelectionCriteria ?? (allApplicableRatesSelectionCriteria = new CachedProperty<IZZRateSelectionCriteria>(Factory, GetAllApplicableRatesSelectionCriteriaCore))).Value;
		CachedProperty<IZZRateSelectionCriteria> allApplicableRatesSelectionCriteria;

		protected virtual IZZRateSelectionCriteria GetAllApplicableRatesSelectionCriteriaCore() => new RateSelectionCriteria<BaseCusClassPartPivot>(this, ZString.Empty, ZString.Empty);

		public class RateSelectionCriteria<T> : IZZRateSelectionCriteria where T : BaseCusClassPartPivot
		{
			public RateSelectionCriteria(T pivot, ZString rateType, ZString rateCode)
			{
				EffectiveDate = ZDateTime.Today;
				this.TradeGroupCountry = pivot.CI_RN_NKCountryOfOrigin;
				this.SecondTradeGroups = new HashSet<ZString>();
				this.DataGrouping = pivot.DefaultDataGroupingForTariffs;
				this.PrimaryPreference = pivot.CI_PrimaryPreference;
				this.AdditionalCodes = new HashSet<ZString>() { ZString.Empty };
				this.ConcessionOrder = pivot.CI_ConcessionOrder;
				this.RateType = rateType;
				this.RateCode = rateCode;
				this.Direction = RateDirection.Both;
			}

			public ZDateTime EffectiveDate { get; }
			public ZString TradeGroupCountry { get; }
			public ISet<ZString> SecondTradeGroups { get; }
			public ZString DataGrouping { get; }
			public ZString PrimaryPreference { get; }
			public ISet<ZString> AdditionalCodes { get; }
			public ZString ConcessionOrder { get; }
			public ZString RateType { get; }
			public ZString RateCode { get; }
			public RateDirection Direction { get; }
		}

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country
		{
			get
			{
				var result = CI_RN_NKCountry;
				return result.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : result;
			}
		}

		#endregion

		#region IWorkflowTriggerEventSource Members

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var list = new List<IWorkflowProviderCore>();
				var part = Part;
				if (part != null)
				{
					list.Add(part);
				}
				return list;
			}
		}

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany => GlbCompany.CurrentCompany;

		#endregion

		#region SetterSuspender

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());
		SetterSuspender setterSuspender;

		IEnumerable<string> ISetterSuspenderSupporter.SupportedFields => supportedFields ?? (supportedFields = GetSetterSuspenderSupportedFields());
		IEnumerable<string> supportedFields;

		protected virtual IEnumerable<string> GetSetterSuspenderSupportedFields()
		{
			yield return Schema.CI_RN_NKCountryOfOrigin;
		}

		#endregion
	}
}
