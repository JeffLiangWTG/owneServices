using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[CodeProperty(CusClassificationSchema.Constants.CC_LookupCode), DescriptionProperty("DescriptionIncludingTariff")]
	public class BaseCusClassification :
		AutoCusClassification,
		Integration.Customs.IBaseCusClassification,
		IDocManagerSupport,
		ITariffProvider,
		ITypeDeciderContext,
		ITariffFormatProvider,
		IAuditParent
	{
		public static readonly BaseCusClassificationTypeDecider TypeDecider = new BaseCusClassificationTypeDecider();
		public BaseCusClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			this.HasChangesChanged += OnHasChangesChanged;
		}

		public static BaseCusClassification LoadFromLookupCode(BusinessObjectFactory factory, string lookupCode, bool isImport, ZString countryCode)
		{
			string classificationType = isImport ? BaseCusClassification.ClassificationType.IMP : BaseCusClassification.ClassificationType.EXP;

			return LoadFromLookupCode(factory, lookupCode, classificationType, countryCode);
		}

		public static BaseCusClassification LoadFromLookupCode(BusinessObjectFactory factory, string lookupCode, string classificationType, ZString countryCode)
		{
			ZQuery filter = new ZQuery(CusClassificationSchema.CC_LookupCode, lookupCode);
			filter.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, countryCode);
			filter.AddToFilter(CusClassificationSchema.CC_ClassificationType, classificationType);

			return factory.LoadTop1<BaseCusClassification>(filter);
		}

		#region Constants
		public new class Schema : AutoCusClassification.Schema
		{
			public const string CC_FormattedTariffNum = "CC_FormattedTariffNum";
		}

		public class ClassificationType : Common.ClassificationType
		{
		}

		#endregion

		#region Override

		[BusinessObjectTestExclude]
		public virtual ZString CC_FormattedTariffNum
		{
			get { return CurrentTariffFormatter.DisplayFormat(CC_TariffNum); }
			set { CC_TariffNum = value; }
		}

		public ZPropertyInfo CC_FormattedTariffNumInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CC_FormattedTariffNum, x => CC_TariffNumInfo); }
		}

		[BusinessObjectTestExclude]
		public override ZString CC_TariffNum
		{
			get { return base.CC_TariffNum; }
			set
			{
				ZString oldTariffNum = CC_TariffNum;
				base.CC_TariffNum = FormatTariffForSaving(value);
				if (oldTariffNum != CC_TariffNum)
				{
					OnTariffSet(oldTariffNum);
				}
			}
		}

		protected virtual ZString FormatTariffForSaving(ZString unformattedTariff) => CurrentTariffFormatter.Format(unformattedTariff);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CC_RN_NKCountryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("b50b7440-720d-4278-925f-8a807ee3c380", "Classification"); }
		}

		protected virtual void OnTariffSet(ZString oldTariff)
		{
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

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[ReadOnly(true)]
		public override ZDateTime CC_LastAuditedDate
		{
			get { return base.CC_LastAuditedDate; }
			set { base.CC_LastAuditedDate = value; }
		}

		[ReadOnly(true)]
		public override ZString CC_LastAuditedUser
		{
			get { return base.CC_LastAuditedUser; }
			set { base.CC_LastAuditedUser = value; }
		}

		public void OnHasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (e.ObjectJustWasChanged && IsInDatabase && !IsDeleted
				&& !CC_LastAuditedUser.IsEmpty && !CC_LastAuditedUserInfo.HasChanges && !CC_LastAuditedDateInfo.HasChanges)
			{
				CC_LastAuditedUser = ZString.Empty;
				CC_LastAuditedDate = ZDateTime.Empty;
			}
		}

		public override void OnSaving()
		{
			if (CC_Description.IsEmpty)
			{
				CC_Description = CC_LookupCode;
			}
			base.OnSaving();
		}

		#endregion

		#region New Properties

		public ZString DescriptionIncludingTariff
		{
			get { return string.Format("({0}) {1}", CC_FormattedTariffNum, CC_Description); }
		}

		public bool IsBoth
		{
			get { return CC_ClassificationType == ClassificationType.Both; }
		}

		public bool IsImport
		{
			get { return CC_ClassificationType == ClassificationType.IMP; }
		}

		public bool IsExport
		{
			get { return CC_ClassificationType == ClassificationType.EXP; }
		}

		public ZBool CC_IsAudited
		{
			get { return CC_LastAuditedDate.IsValid; }
		}

		#endregion

		#region IDocManagerSupport Members

		public virtual DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.Classification);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region Implementation
		public ZQuery ClassTypeLookupCodeFilter
		{
			get
			{
				ZQuery result = new ZQuery(CusClassificationSchema.CC_ClassificationType, CC_ClassificationType);
				result.AddToFilter(JoinCondition.And, CusClassificationSchema.CC_LookupCode, SQLComparisonOperator.Equal, CC_LookupCode);
				return result;
			}
		}
		#endregion

		#region ITariffProvider Members

		ZString ITariffProvider.Tariff
		{
			get { return CC_TariffNum; }
		}

		ZPropertyInfo ITariffProvider.TariffInfo
		{
			get { return CC_TariffNumInfo; }
		}

		#endregion

		public ZString DutyRateForCurrentCountry
		{
			get { return GetDutyRateForCurrentCountry(); }
		}

		protected virtual ZString GetDutyRateForCurrentCountry()
		{
			return ZString.Empty;
		}

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country
		{
			get
			{
				var result = CC_RN_NKCountryCode;
				return result.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : result;
			}
		}

		#endregion

		#region FillWithValidTestData
#if DEBUG
		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new CusClassificationTestDataHelper();
		}

		class CusClassificationTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override string GetUniqueStringForProperty(ZPropertyInfo property, PropertyDescriptor[] propertyPath, int maxLength)
			{
				if (property.Name == BaseCusClassification.Schema.CC_ClassificationType)
				{
					return ClassificationType.Both;
				}

				return base.GetUniqueStringForProperty(property, propertyPath, maxLength);
			}
		}
#endif
		#endregion

		public IEnumerable<AuditChildInfo> RelatedAuditChildren
		{
			get
			{
				return Array.Empty<AuditChildInfo>();
			}
		}
	}
}
