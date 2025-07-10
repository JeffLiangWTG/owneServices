using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public abstract class AutoDrawbackNAFTA : Customs.Business.MultiLineAddInfos.CusAddInfo<DrawbackNAFTAAddInfo>
	{
		protected AutoDrawbackNAFTA(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.MultiLineAddInfos.CusAddInfo<DrawbackNAFTAAddInfo>.Schema
		{
			public const string US_DRWNAFTACountryImportEntry = USDrawbackNAFTAAddInfoSchema.Constants.US_DRWNAFTACountryImportEntry;
			public const string US_DRWNAFTACountryImportEntryDate = USDrawbackNAFTAAddInfoSchema.Constants.US_DRWNAFTACountryImportEntryDate;
			public const string US_DRWNAFTACountryTariffNumber = USDrawbackNAFTAAddInfoSchema.Constants.US_DRWNAFTACountryTariffNumber;
			public const string US_DRWNAFTACountryDutyRate = USDrawbackNAFTAAddInfoSchema.Constants.US_DRWNAFTACountryDutyRate;
			public const string US_DRWNAFTACountryImportDuty = USDrawbackNAFTAAddInfoSchema.Constants.US_DRWNAFTACountryImportDuty;
			public const string US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty = USDrawbackNAFTAAddInfoSchema.Constants.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty;
			public const string US_DRWNAFTACountryTariffNumber2 = USDrawbackNAFTAAddInfoSchema.Constants.US_DRWNAFTACountryTariffNumber2;
			public const string US_DRWNAFTACountryTariffNumber3 = USDrawbackNAFTAAddInfoSchema.Constants.US_DRWNAFTACountryTariffNumber3;
			public const string US_DRWNAFTACountryOfExport = USDrawbackNAFTAAddInfoSchema.Constants.US_DRWNAFTACountryOfExport;
		}
		#endregion

		#region AddInfo Properties

		#region US_DRWNAFTACountryImportEntry

		public virtual ZString US_DRWNAFTACountryImportEntry
		{
			get { return AddInfo.US_DRWNAFTACountryImportEntry; }
			set { AddInfo.US_DRWNAFTACountryImportEntry = value; }
		}

		public virtual ZPropertyInfo US_DRWNAFTACountryImportEntryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_DRWNAFTACountryImportEntry, x => AddInfo.US_DRWNAFTACountryImportEntryInfo); }
		}

		#endregion

		#region US_DRWNAFTACountryImportEntryDate

		public virtual ZDateTime US_DRWNAFTACountryImportEntryDate
		{
			get { return AddInfo.US_DRWNAFTACountryImportEntryDate; }
			set { AddInfo.US_DRWNAFTACountryImportEntryDate = value; }
		}

		public virtual ZPropertyInfo US_DRWNAFTACountryImportEntryDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_DRWNAFTACountryImportEntryDate, x => AddInfo.US_DRWNAFTACountryImportEntryDateInfo); }
		}

		#endregion

		#region US_DRWNAFTACountryTariffNumber

		public virtual ZString US_DRWNAFTACountryTariffNumber
		{
			get { return AddInfo.US_DRWNAFTACountryTariffNumber; }
			set { AddInfo.US_DRWNAFTACountryTariffNumber = value; }
		}

		public virtual ZPropertyInfo US_DRWNAFTACountryTariffNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_DRWNAFTACountryTariffNumber, x => AddInfo.US_DRWNAFTACountryTariffNumberInfo); }
		}

		#endregion

		#region US_DRWNAFTACountryDutyRate

		public virtual ZDecimal US_DRWNAFTACountryDutyRate
		{
			get { return AddInfo.US_DRWNAFTACountryDutyRate; }
			set { AddInfo.US_DRWNAFTACountryDutyRate = value; }
		}

		public virtual ZPropertyInfo US_DRWNAFTACountryDutyRateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_DRWNAFTACountryDutyRate, x => AddInfo.US_DRWNAFTACountryDutyRateInfo); }
		}

		#endregion

		#region US_DRWNAFTACountryImportDuty

		public virtual ZDecimal US_DRWNAFTACountryImportDuty
		{
			get { return AddInfo.US_DRWNAFTACountryImportDuty; }
			set { AddInfo.US_DRWNAFTACountryImportDuty = value; }
		}

		public virtual ZPropertyInfo US_DRWNAFTACountryImportDutyInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_DRWNAFTACountryImportDuty, x => AddInfo.US_DRWNAFTACountryImportDutyInfo); }
		}

		#endregion

		#region US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty

		public virtual ZDecimal US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty
		{
			get { return AddInfo.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty; }
			set { AddInfo.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty = value; }
		}

		public virtual ZPropertyInfo US_DRWEquivalentUSDollarAmountOfNAFTACountryDutyInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty, x => AddInfo.US_DRWEquivalentUSDollarAmountOfNAFTACountryDutyInfo); }
		}

		#endregion

		#region US_DRWNAFTACountryTariffNumber2

		public virtual ZString US_DRWNAFTACountryTariffNumber2
		{
			get { return AddInfo.US_DRWNAFTACountryTariffNumber2; }
			set { AddInfo.US_DRWNAFTACountryTariffNumber2 = value; }
		}

		public ZPropertyInfo US_DRWNAFTACountryTariffNumber2Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_DRWNAFTACountryTariffNumber2, x => AddInfo.US_DRWNAFTACountryTariffNumber2Info); }
		}

		#endregion

		#region US_DRWNAFTACountryTariffNumber3

		public virtual ZString US_DRWNAFTACountryTariffNumber3
		{
			get { return AddInfo.US_DRWNAFTACountryTariffNumber3; }
			set { AddInfo.US_DRWNAFTACountryTariffNumber3 = value; }
		}

		public ZPropertyInfo US_DRWNAFTACountryTariffNumber3Info
		{
			get { return GetWrappedZPropertyInfo(Schema.US_DRWNAFTACountryTariffNumber3, x => AddInfo.US_DRWNAFTACountryTariffNumber3Info); }
		}

		#endregion

		#region US_DRWNAFTACountryOfExport

		public virtual ZString US_DRWNAFTACountryOfExport
		{
			get { return AddInfo.US_DRWNAFTACountryOfExport; }
			set { AddInfo.US_DRWNAFTACountryOfExport = value; }
		}

		public ZPropertyInfo US_DRWNAFTACountryOfExportInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_DRWNAFTACountryOfExport, x => AddInfo.US_DRWNAFTACountryOfExportInfo); }
		}

		#endregion

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USDrawbackNAFTAAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USDrawbackNAFTAAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		DrawbackNAFTAAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new DrawbackNAFTAAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		DrawbackNAFTAAddInfo fAddInfo;

		#endregion

		protected void updateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}
	}
}
