using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class License : Customs.Business.MultiLineAddInfos.CusAddInfo<LicenseAddInfo>, ILicense
	{
		public License(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.MultiLineAddInfos.CusAddInfo<LicenseAddInfo>.Schema
		{
			public const string US_Date = USLicenseAddInfoSchema.Constants.US_Date;
			public const string US_DateQualifier = USLicenseAddInfoSchema.Constants.US_DateQualifier;
			public const string US_Number = USLicenseAddInfoSchema.Constants.US_Number;
			public const string US_TransType = USLicenseAddInfoSchema.Constants.US_TransType;
			public const string US_Type = USLicenseAddInfoSchema.Constants.US_Type;
		}

		#endregion

		#region AddInfo Properties

		[List(nameof(AddInfoLookups) + "." + nameof(USLicenseAddInfoLookups.LPCOTransactionTypeList))]
		public ZString US_TransType
		{
			get { return AddInfo.US_TransType; }
			set { AddInfo.US_TransType = value; }
		}

		public ZPropertyInfo US_TransTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_TransType, x => AddInfo.US_TransTypeInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USLicenseAddInfoLookups.LPCOTypeList))]
		public ZString US_Type
		{
			get { return AddInfo.US_Type; }
			set { AddInfo.US_Type = value; }
		}

		public ZPropertyInfo US_TypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Type, x => AddInfo.US_TypeInfo); }
		}

		public ZString US_Number
		{
			get { return AddInfo.US_Number; }
			set { AddInfo.US_Number = value; }
		}

		public ZPropertyInfo US_NumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Number, x => AddInfo.US_NumberInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USLicenseAddInfoLookups.DateQualifierList))]
		public ZString US_DateQualifier
		{
			get { return AddInfo.US_DateQualifier; }
			set { AddInfo.US_DateQualifier = value; }
		}

		public ZPropertyInfo US_DateQualifierInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_DateQualifier, x => AddInfo.US_DateQualifierInfo); }
		}

		public ZDateTime US_Date
		{
			get { return AddInfo.US_Date; }
			set { AddInfo.US_Date = value; }
		}

		public ZPropertyInfo US_DateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Date, x => AddInfo.US_DateInfo); }
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "License"; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();

			var result = (License)base.CloneInternal(args);
			return result;
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USLicenseAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USLicenseAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		LicenseAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new LicenseAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		LicenseAddInfo fAddInfo;

		public void UpdateAddInfoProperties()
		{
			updateAddInfoProperties();
		}

		protected void updateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}

		#endregion

		#region ILicense Members

		ZString ILicense.TransactionType
		{
			get { return US_TransType; }
		}

		ZString ILicense.Type
		{
			get { return US_Type; }
		}

		ZString ILicense.Number
		{
			get { return US_Number; }
		}

		ZString ILicense.DateQualifier
		{
			get { return US_DateQualifier; }
		}

		ZDate ILicense.Date
		{
			get { return US_Date.IsValid ? US_Date.Date : ZDate.Empty; }
		}

		#endregion
	}
}
