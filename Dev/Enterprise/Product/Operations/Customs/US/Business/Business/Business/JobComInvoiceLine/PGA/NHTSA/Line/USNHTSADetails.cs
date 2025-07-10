using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public abstract class USNHTSADetails : Customs.Business.MultiLineAddInfos.CusAddInfo<USNHTSADetailsAddInfo>
	{
		public USNHTSADetails(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.MultiLineAddInfos.CusAddInfo<USNHTSADetailsAddInfo>.Schema
		{
			public const string US_NHTBrandName = USNHTSADetailsAddInfoSchema.Constants.US_NHTBrandName;
			public const string US_NHTCategoryCode = USNHTSADetailsAddInfoSchema.Constants.US_NHTCategoryCode;
			public const string US_NHTDriveSide = USNHTSADetailsAddInfoSchema.Constants.US_NHTDriveSide;
			public const string US_NHTModel = USNHTSADetailsAddInfoSchema.Constants.US_NHTModel;
			public const string US_NHTModelYear = USNHTSADetailsAddInfoSchema.Constants.US_NHTModelYear;
			public const string US_NHTMonthOfMFR = USNHTSADetailsAddInfoSchema.Constants.US_NHTMonthOfMFR;
			public const string US_NHTYearOfMFR = USNHTSADetailsAddInfoSchema.Constants.US_NHTYearOfMFR;
		}

		#endregion

		#region Related

		public NHTSAHeader Header
		{
			get { return Factory.Load<NHTSAHeader>(B7_ParentID); }
		}

		#endregion

		#region AddInfo Properties

		public ZString US_NHTBrandName
		{
			get { return AddInfo.US_NHTBrandName; }
			set { AddInfo.US_NHTBrandName = value; }
		}

		public ZPropertyInfo US_NHTBrandNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTBrandName, x => AddInfo.US_NHTBrandNameInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSADetailsAddInfoLookups.CategoryCodes))]
		public ZString US_NHTCategoryCode
		{
			get { return AddInfo.US_NHTCategoryCode; }
			set { AddInfo.US_NHTCategoryCode = value; }
		}

		public ZPropertyInfo US_NHTCategoryCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTCategoryCode, x => AddInfo.US_NHTCategoryCodeInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSADetailsAddInfoLookups.DriveSides))]
		public ZString US_NHTDriveSide
		{
			get { return AddInfo.US_NHTDriveSide; }
			set { AddInfo.US_NHTDriveSide = value; }
		}

		public ZPropertyInfo US_NHTDriveSideInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTDriveSide, x => AddInfo.US_NHTDriveSideInfo); }
		}

		public ZString US_NHTModel
		{
			get { return AddInfo.US_NHTModel; }
			set { AddInfo.US_NHTModel = value; }
		}

		public ZPropertyInfo US_NHTModelInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTModel, x => AddInfo.US_NHTModelInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSADetailsAddInfoLookups.ModelYearList))]
		public ZString US_NHTModelYear
		{
			get { return AddInfo.US_NHTModelYear; }
			set { AddInfo.US_NHTModelYear = value; }
		}

		public ZPropertyInfo US_NHTModelYearInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTModelYear, x => AddInfo.US_NHTModelYearInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSADetailsAddInfoLookups.MonthList))]
		public ZString US_NHTMonthOfMFR
		{
			get { return AddInfo.US_NHTMonthOfMFR; }
			set { AddInfo.US_NHTMonthOfMFR = value; }
		}

		public ZPropertyInfo US_NHTMonthOfMFRInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTMonthOfMFR, x => AddInfo.US_NHTMonthOfMFRInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSADetailsAddInfoLookups.ManufacturerYearList))]
		public ZString US_NHTYearOfMFR
		{
			get { return AddInfo.US_NHTYearOfMFR; }
			set { AddInfo.US_NHTYearOfMFR = value; }
		}

		public ZPropertyInfo US_NHTYearOfMFRInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTYearOfMFR, x => AddInfo.US_NHTYearOfMFRInfo); }
		}

		#endregion

		#region New Properties

		public ZString US_NHTCategoryType
		{
			get
			{
				var header = Header;
				return header != null && header.US_NHTProgramCode.IsValid ? header.US_NHTProgramCode + "TYP" : string.Empty;
			}
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USNHTSADetailsAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new USNHTSADetailsAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		USNHTSADetailsAddInfo fAddInfo;

		public void UpdateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}

		public USNHTSADetailsAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		#endregion
	}
}
