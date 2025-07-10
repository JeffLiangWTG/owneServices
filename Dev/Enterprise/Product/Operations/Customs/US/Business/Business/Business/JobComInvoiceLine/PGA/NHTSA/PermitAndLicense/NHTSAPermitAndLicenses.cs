using System.Data;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class NHTSAPermitAndLicenses : Customs.Business.MultiLineAddInfos.CusAddInfo<USNHTSAPermitAndLicenseAddInfo>, INHTSAPermitAndLicense
	{
		public NHTSAPermitAndLicenses(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.MultiLineAddInfos.CusAddInfo<USNHTSAPermitAndLicenseAddInfo>.Schema
		{
			public const string US_NHTLPCODate = USNHTSAPermitAndLicenseAddInfoSchema.Constants.US_NHTLPCODate;
			public const string US_NHTLPCODateType = USNHTSAPermitAndLicenseAddInfoSchema.Constants.US_NHTLPCODateType;
			public const string US_NHTLPCONumber = USNHTSAPermitAndLicenseAddInfoSchema.Constants.US_NHTLPCONumber;
			public const string US_NHTLPCOQuantity = USNHTSAPermitAndLicenseAddInfoSchema.Constants.US_NHTLPCOQuantity;
			public const string US_NHTLPCOType = USNHTSAPermitAndLicenseAddInfoSchema.Constants.US_NHTLPCOType;
		}

		#endregion

		#region Related

		public NHTSADetails Details
		{
			get { return Factory.Load<NHTSADetails>(B7_ParentID); }
		}

		#endregion

		#region AddInfo Properties

		public ZDateTime US_NHTLPCODate
		{
			get { return AddInfo.US_NHTLPCODate; }
			set { AddInfo.US_NHTLPCODate = value; }
		}

		public ZPropertyInfo US_NHTLPCODateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTLPCODate, x => AddInfo.US_NHTLPCODateInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSAPermitAndLicenseAddInfoLookups.LPCODateTypes))]
		public ZString US_NHTLPCODateType
		{
			get { return AddInfo.US_NHTLPCODateType; }
			set { AddInfo.US_NHTLPCODateType = value; }
		}

		public ZPropertyInfo US_NHTLPCODateTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTLPCODateType, x => AddInfo.US_NHTLPCODateTypeInfo); }
		}

		public ZString US_NHTLPCONumber
		{
			get { return AddInfo.US_NHTLPCONumber; }
			set { AddInfo.US_NHTLPCONumber = value; }
		}

		public ZPropertyInfo US_NHTLPCONumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTLPCONumber, x => AddInfo.US_NHTLPCONumberInfo); }
		}

		public ZDecimal US_NHTLPCOQuantity
		{
			get { return AddInfo.US_NHTLPCOQuantity; }
			set { AddInfo.US_NHTLPCOQuantity = value; }
		}

		public ZPropertyInfo US_NHTLPCOQuantityInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTLPCOQuantity, x => AddInfo.US_NHTLPCOQuantityInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSAPermitAndLicenseAddInfoLookups.LPCOTypes))]
		public ZString US_NHTLPCOType
		{
			get { return AddInfo.US_NHTLPCOType; }
			set { AddInfo.US_NHTLPCOType = value; }
		}

		public ZPropertyInfo US_NHTLPCOTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTLPCOType, x => AddInfo.US_NHTLPCOTypeInfo); }
		}

		#endregion

		#region Override Properties

		NHTSAHeader Header
		{
			get { return Details.Header; }
		}

		#endregion

		#region New Properties

		ZString TransactionType
		{
			get
			{
				var result = ZString.Empty;
				var boxNumber = Header.US_NHTBoxNumber;
				if (boxNumber == DepartmentOfTransportBoxNumberList.Codes._03 && (IsRegisteredImporterNumber || IsVehicleEligibilityNumber))
				{
					result = LPCOTransactionTypeList.Codes.Continuous;
				}
				else if ((boxNumber == DepartmentOfTransportBoxNumberList.Codes._07 || boxNumber == DepartmentOfTransportBoxNumberList.Codes._10) && IsNHTSAImportPermissionLetter)
				{
					result = LPCOTransactionTypeList.Codes.SingleUse;
				}
				else if (boxNumber == DepartmentOfTransportBoxNumberList.Codes._13)
				{
					if (IsRegisteredImporterNumber)
					{
						result = LPCOTransactionTypeList.Codes.Continuous;
					}
					else if (IsNHTSAImportPermissionLetter)
					{
						result = LPCOTransactionTypeList.Codes.SingleUse;
					}
				}

				return result;
			}
		}

		public ZBool IsRegisteredImporterNumber
		{
			get { return US_NHTLPCOType == NHTSALPCOTypeList.Codes.NH0 && Regex.IsMatch(US_NHTLPCONumber, @"^[A-Z]\-\d{2}\-\d{3}$"); }
		}

		public ZBool IsNHTSAImportPermissionLetter
		{
			get { return US_NHTLPCOType == NHTSALPCOTypeList.Codes.NH2 && Regex.IsMatch(US_NHTLPCONumber, @"^\d{2}\-\d{4}\-\d{4}$"); }
		}

		public ZBool IsVehicleEligibilityNumber
		{
			get { return US_NHTLPCOType == NHTSALPCOTypeList.Codes.NH3 && Regex.IsMatch(US_NHTLPCONumber, @"^[A-Z]{3}\-\d{3}$"); }
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "NHTSAPermitAndLicenses"; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			UpdateAddInfoProperties();

			var result = (NHTSAPermitAndLicenses)base.CloneInternal(args);
			return result;
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USNHTSAPermitAndLicenseAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new USNHTSAPermitAndLicenseAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		USNHTSAPermitAndLicenseAddInfo fAddInfo;

		public void UpdateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}

		public USNHTSAPermitAndLicenseAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		#endregion

		#region INHTSAPermitAndLicense Members

		ZString INHTSAPermitAndLicense.TransactionType
		{
			get { return TransactionType; }
		}

		ZString INHTSAPermitAndLicense.LPCOType
		{
			get { return US_NHTLPCOType; }
		}

		ZString INHTSAPermitAndLicense.LPCONumber
		{
			get { return US_NHTLPCONumber; }
		}

		ZString INHTSAPermitAndLicense.DateType
		{
			get { return US_NHTLPCODateType; }
		}

		ZDate INHTSAPermitAndLicense.LPCODate
		{
			get { return US_NHTLPCODate.Date; }
		}

		ZDecimal INHTSAPermitAndLicense.LPCOQuantity
		{
			get { return US_NHTLPCOQuantity; }
		}

		ZString INHTSAPermitAndLicense.UnitOfMeasure
		{
			get { return !US_NHTLPCOQuantity.IsEmpty ? ABIUnitOfMeasureList.Codes.Number : string.Empty; }
		}

		#endregion
	}
}
