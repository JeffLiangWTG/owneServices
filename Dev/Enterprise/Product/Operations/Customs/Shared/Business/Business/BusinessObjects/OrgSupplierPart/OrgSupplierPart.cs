using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class OrgSupplierPart : MasterFiles.Business.OrgSupplierPart, Integration.Customs.Shared.IGlobalOrgSupplierPart
	{
		public OrgSupplierPart(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static new readonly TypeDecider TypeDecider = new OrgSupplierPartTypeDecider();

		#region Schema
		public new class Schema : MasterFiles.Business.OrgSupplierPart.Schema
		{
			public const string OP_ClassificationCode = "OP_ClassificationCode";
			public const string OP_TariffCode = "OP_TariffCode";
		}
		#endregion

		#region New Properties

		public ZString Tariffs
		{
			get
			{
				if (tariffs == null)
				{
					tariffs = new CachedProperty<ZString>(Factory, delegate
					{
						var result = new ZStringBuilder();
						var pivots = GetPivots<BaseCusClassPartPivot>(CurrentCompanyCustomsCountryCode);
						pivots.OrderBy(x => x.TariffNumbersIncludingComponents).
							TakeWhile(x => !x.TariffNumbersIncludingComponents.IsEmpty).
							Take(3).
							ToList().
							ForEach(x => result.Append(x.TariffNumbersIncludingComponents));

						if (pivots.Length > 3)
						{
							result.Append("...");
						}

						return result.ToStringWithDelimiterBetweenAppends(", ");
					});
				}
				return tariffs.Value;
			}
		}
		CachedProperty<ZString> tariffs;

		#endregion

		void CallOnPartNumberChangedOnPivots()
		{
			if (IsInDatabase && !IsDeleted)
			{
				if (OP_PartNumInfo.OriginalValue is ZString oldPartNum && oldPartNum != OP_PartNum)
				{
					GetPivots<BaseCusClassPartPivot>(CountryCodesForCallingPartNumberChangedOnPivots).ForEach(pivot =>
					{
						pivot.OnPartNumberChanged(oldPartNum, OP_PartNum);
					});
				}
			}
		}

		string[] CountryCodesForCallingPartNumberChangedOnPivots => new[] { Core.Constants.CountryCodes.Brazil };

		public override void OnSaving()
		{
			if (RemoveNonEssentialValidationForBulkTariffUpdate && !hasSetConcurrencyPolicy && IsInDatabase)
			{
				hasSetConcurrencyPolicy = true;
				SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
			}

			CallOnPartNumberChangedOnPivots();

			base.OnSaving();
		}
		bool hasSetConcurrencyPolicy;

		public OrgSupplierPart LocalPart
		{
			get { return localPart; }
			set { localPart = value; }
		}
		OrgSupplierPart localPart;

		#region DeleteForDataRefresh
		protected override void DeleteForDataRefresh()
		{
			base.DeleteForDataRefresh();
			if (DeletedByDataRefresh != null)
			{
				DeletedByDataRefresh(this);
			}
		}

		public delegate void DeletedHandler(OrgSupplierPart part);
		public new event DeletedHandler DeletedByDataRefresh;
		#endregion

		#region Classifications

		public IEnumerable<TClassification> GetClassifications<TClassification, TPivot>(ZString countryCode) where TPivot : BaseCusClassPartPivot where TClassification : BaseCusClassification
		{
			return GetPivots<TPivot>(countryCode).Select(p => (TClassification)p.Classification).Distinct().Where(c => c != null);
		}

		public IEnumerable<BaseCusClassification> GetClassifications(ZString countryCode) => GetClassifications<BaseCusClassification, BaseCusClassPartPivot>(countryCode);

		[ChildEditable(true)]
		public IClassificationCollection<BaseCusClassification> ClassificationsForBinding
		{
			get
			{
				if (classificationsForBinding == null)
				{
					classificationsForBinding = GetNewClassificationCollection();
					RegisterEditableChildObject(classificationsForBinding);
				}
				return classificationsForBinding;
			}
		}
		IClassificationCollection<BaseCusClassification> classificationsForBinding;

		protected virtual IClassificationCollection<BaseCusClassification> GetNewClassificationCollection() => new ClassificationCollection<BaseCusClassification>(this, CurrentCompanyCustomsCountryCode);

		#endregion

		public ZString CurrentCompanyCustomsCountryCode
		{
			get
			{
				if (!currentCompanyCustomsCountryCode.HasValue)
				{
					currentCompanyCustomsCountryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				}
				return currentCompanyCustomsCountryCode.Value;
			}
		}
		ZString? currentCompanyCustomsCountryCode;

		#region ITemplateCopyable Members

		protected override IBusiness TemplateCopyCore()
		{
			var part = (OrgSupplierPart)base.TemplateCopyCore();
			using (part.GetValidationSuspender())
			{
				var filter = new ZQuery(CusClassPartPivotSchema.CI_RN_NKCountry, CurrentCompanyCustomsCountryCode);
				filter.AddToFilter(PivotFilter);
				var classificationsPiviotForPart = Factory.Load<BaseCusClassPartPivot>(filter);
				if (classificationsPiviotForPart != null)
				{
					foreach (BaseCusClassPartPivot classPivotForPart in classificationsPiviotForPart)
					{
						var newClassPivotForPart = (BaseCusClassPartPivot)classPivotForPart.Clone();
						newClassPivotForPart.CI_OP = part.PK;
					}
				}
			}
			return part;
		}

		public void ClearDataAndDeleteChildren()
		{
			ClearDataAndDeleteChildrenCore();
		}

		protected virtual void ClearDataAndDeleteChildrenCore()
		{
			PivotsForBinding.RemoveAndDeleteAll();
			PartUnits.RemoveAndDeleteAll();
			Locations.RemoveAndDeleteAll();
			BillOfMaterials.DeleteAll();
			RelatedOrganisations.RemoveAndDeleteAll();
			PartBarcodes.RemoveAndDeleteAll();
			WorkflowItems.RemoveAndDeleteAll();
			foreach (var column in GetColumnToClear())
			{
				var value = this[column] as IZType;
				if (value != null && !value.IsEmpty)
				{
					this[column] = value.Default;
				}
			}
		}

		IEnumerable<SchemaColumn> GetColumnToClear()
		{
			var result = OrgSupplierPartSchema.All.OfType<SchemaColumn>().ToList();
			result.Remove(OrgSupplierPartSchema.PK);
			result.Remove(OrgSupplierPartSchema.OP_IsActive);
			result.Remove(OrgSupplierPartSchema.OP_IsValid);
			result.Remove(OrgSupplierPartSchema.OP_PartNum);
			result.Remove(OrgSupplierPartSchema.OP_SystemCreateTimeUtc);
			result.Remove(OrgSupplierPartSchema.OP_SystemCreateUser);
			result.Remove(OrgSupplierPartSchema.OP_SystemLastEditTimeUtc);
			result.Remove(OrgSupplierPartSchema.OP_SystemLastEditUser);
			return result;
		}

		public override void Delete()
		{
			PivotsForBinding.RemoveAndDeleteAll();
			base.Delete();
		}

		public override bool CanDelete => base.CanDelete && !HasPivotsFromOtherCountries();
		public override MultilingualString ReasonForNotAbleToDelete => HasPivotsFromOtherCountries() ? ResString.GetMultilingualString("608ef6a1-75f4-4c6c-8d7f-bd2dd34f438d", "Unable to delete a product that has classifications related to other countries.") : base.ReasonForNotAbleToDelete;

		bool HasPivotsFromOtherCountries()
		{
			var all_pivots = new CusClassPartPivotCollection<BaseCusClassPartPivot>(this, null);
			all_pivots.Load();
			var customsCountryCode = CurrentCompanyCustomsCountryCode;
			return all_pivots.OfType<BaseCusClassPartPivot>().Any(x => x.CI_RN_NKCountry != customsCountryCode);
		}

		#endregion

		#region Loader
		public new class Loader : MasterFiles.Business.OrgSupplierPart.Loader
		{
			public Loader(BusinessObjectFactory factory, Type typeofPart)
				: base(factory, typeofPart)
			{
			}

			public new OrgSupplierPart Load(ZString partCode, OrgHeader buyer, OrgHeader supplier, bool throwIfAmbiguousMatchDetected = false, bool allowInactive = false, bool isExportJob = false)
			{
				return (OrgSupplierPart)base.Load(partCode, buyer, supplier, throwIfAmbiguousMatchDetected, allowInactive, isExportJob: isExportJob);
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeofPart;
			}
		}
		#endregion

		#region Add/Remove Pivot without touching Classifications (ManyToMany ActiveBusinessCollection)

		protected void AddClassification(BaseCusClassification classification)
		{
			if (classification != null)
			{
				ZQuery query = PivotFilter;
				query.AddToFilter(CusClassPartPivotSchema.CI_CC, classification.PK);
				query.FetchOnlyFromLocalCache = !IsInDatabase;
				BaseCusClassPartPivot pivot = Factory.LoadTop1<BaseCusClassPartPivot>(query);
				if (pivot == null)
				{
					pivot = Factory.New<BaseCusClassPartPivot>();
					pivot.CI_OP = PK;
					pivot.CI_CC = classification.PK;
				}
			}
		}

		protected void RemoveClassification(BaseCusClassification classification)
		{
			if (classification != null)
			{
				ZQuery query = PivotFilter;
				query.AddToFilter(CusClassPartPivotSchema.CI_CC, classification.PK);
				query.FetchOnlyFromLocalCache = !IsInDatabase;
				BaseCusClassPartPivot pivot = Factory.LoadTop1<BaseCusClassPartPivot>(query);
				if (pivot != null)
				{
					pivot.Delete();
				}
			}
		}

		#endregion
		#region Validation
		public new OrgSupplierPartValidation Validation
		{
			get { return (OrgSupplierPartValidation)base.Validation; }
		}

		protected override MasterFiles.Business.OrgSupplierPartValidation GetNewValidation()
		{
			return new OrgSupplierPartValidation(this);
		}
		#endregion

		#region Lookups
		public new OrgSupplierPartLookups Lookups
		{
			get { return (OrgSupplierPartLookups)base.Lookups; }
		}

		protected override MasterFiles.Business.OrgSupplierPartLookups GetNewLookups()
		{
			return new OrgSupplierPartLookups(this);
		}
		#endregion

		#region Implementation

		protected virtual ZQuery PivotFilter
		{
			get { return new ZQuery(CusClassPartPivotSchema.CI_OP, PK); }
		}

		#endregion

		#region Pivot Collection

		/// <summary>
		/// return the CusClassPartPivotCollection for designated country, if designated country code is empty or null, then returns CusClassPartPivotCollection for the current country
		/// </summary>
		/// <param name="countryCode">designated country code</param>
		/// <returns>CusClassPartPivotCollection</returns>
		public TPivot[] GetPivots<TPivot>(params string[] countryCode) where TPivot : BaseCusClassPartPivot
		{
			var pivotsQuery = CusClassPartPivotCollection<BaseCusClassPartPivot>.GetPartPivotFilter(countryCode);
			pivotsQuery.AddToFilter(CusClassPartPivotSchema.CI_OP, PK);
			pivotsQuery.FetchOnlyFromLocalCache = !IsInDatabase;
			pivotsQuery.OrderBy = CusClassPartPivotSchema.CI_SystemCreateTimeUtc.Name;
			return Factory.Load<TPivot>(pivotsQuery);
		}

		[ChildEditable(true)]
		public ICusClassPartPivotCollection<BaseCusClassPartPivot> PivotsForBinding
		{
			get
			{
				if (!HasLoadedPivotsForBinding)
				{
					pivotsForBinding = GetNewParentPivots();
					pivotsForBinding.Load();
					RegisterEditableChildObject(pivotsForBinding);
				}
				return pivotsForBinding;
			}
		}
		ICusClassPartPivotCollection<BaseCusClassPartPivot> pivotsForBinding;

		public bool HasLoadedPivotsForBinding
		{
			get { return pivotsForBinding != null; }
		}

		protected virtual ICusClassPartPivotCollection<BaseCusClassPartPivot> GetNewParentPivots()
		{
			return new CusClassPartPivotCollection<BaseCusClassPartPivot>(this, CurrentCompanyCustomsCountryCode);
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new ArrayList(base.BusinessObjectsWithRelatedEventsCore);
				this.GetPivots<BaseCusClassPartPivot>(CurrentCompanyCustomsCountryCode).ForEach(p => result.Add(p));
				return (BusinessObject[])result.ToArray(typeof(BusinessObject));
			}
		}

		#endregion

		#region ClassPivotValues

		delegate bool IsMatchedType(BaseCusClassPartPivot pivot);

		protected virtual bool IsImportChildType(BaseCusClassPartPivot pivot)
		{
			return pivot.CI_ChildType == ClassificationTypeList.Codes.HTI;
		}

		protected virtual bool IsExportChildType(BaseCusClassPartPivot pivot)
		{
			return pivot.CI_ChildType == ClassificationTypeList.Codes.HTE;
		}

		public ZString ImportLastAuditUser
		{
			get { return GetLastAuditUserOfChildType(IsImportChildType); }
		}

		public ZString ExportLastAuditUser
		{
			get { return GetLastAuditUserOfChildType(IsExportChildType); }
		}

		ZString GetLastAuditUserOfChildType(IsMatchedType isMatchedType)
		{
			var userList = GetPivots<BaseCusClassPartPivot>(CurrentCompanyCustomsCountryCode).Where(pivot => isMatchedType(pivot) && !pivot.CI_LastAuditedUser.IsEmpty).Select(pivot => pivot.CI_LastAuditedUser).Distinct();
			return !userList.Any() ? "" : userList.Count() > 1 ? MultipleValues : userList.FirstOrDefault().ToString();
		}

		public ZString ImportLastAuditDate
		{
			get { return GetLastAuditDateOfChildType(IsImportChildType); }
		}

		public ZString ExportLastAuditDate
		{
			get { return GetLastAuditDateOfChildType(IsExportChildType); }
		}

		ZString GetLastAuditDateOfChildType(IsMatchedType isMatchedType)
		{
			var dateTimeList = GetPivots<BaseCusClassPartPivot>(CurrentCompanyCustomsCountryCode).Where(pivot => isMatchedType(pivot) && !pivot.CI_LastAuditedDate.IsEmpty).Select(pivot => pivot.CI_LastAuditedDate.ToShortDateString()).Distinct();
			return !dateTimeList.Any() ? "" : dateTimeList.Count() > 1 ? MultipleValues : dateTimeList.FirstOrDefault();
		}

		protected readonly string MultipleValues = "MULTI";

		#endregion

		public bool DoesPartMatchPivotForInactiveCheck(BaseJobComInvoiceLine invoiceLine)
		{
			return DoesPartMatchPivotForInactiveCheckCore(invoiceLine);
		}

		protected virtual bool DoesPartMatchPivotForInactiveCheckCore(BaseJobComInvoiceLine invoiceLine)
		{
			var result = true;
			var pivotQuery = new ZQuery(CusClassPartPivotSchema.CI_OP, PK);
			var partPivots = Factory.Load<BaseCusClassPartPivot>(pivotQuery);
			if (partPivots.Length > 1)
			{
				result = false;
			}
			else if (partPivots.Length == 1)
			{
				var pivot = partPivots[0];
				var declaration = invoiceLine.Declaration;
				var countryCode = declaration == null ? ZString.Empty : declaration.CountryCode;
				var pivotType = pivot.CI_ChildType;
				result = pivot.CI_RN_NKCountry == countryCode && (pivotType == invoiceLine.GetPartPivotType() || pivotType == ClassificationTypeList.Codes.HTB);
			}
			return result;
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new FetchStrategies.OrgSupplierPartFetchStrategy(this);
	}
}
