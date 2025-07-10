using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[GlowInterfaceReference("")]
	[GlowDataDefinition("IUSFDALot")]
	public class Lot : Customs.Business.MultiLineAddInfos.CusAddInfo<USLotAddInfo>, IFDALot, Integration.Customs.US.IUSLot, ICPSCLot
	{
		public Lot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.MultiLineAddInfos.CusAddInfo<USLotAddInfo>.Schema
		{
			public const string US_DegreeType = USFDALotAddInfoSchema.Constants.US_DegreeType;
			public const string US_EndDate = USFDALotAddInfoSchema.Constants.US_EndDate;
			public const string US_LocationOfTemp = USFDALotAddInfoSchema.Constants.US_LocationOfTemp;
			public const string US_LotNumber = USFDALotAddInfoSchema.Constants.US_LotNumber;
			public const string US_StartDate = USFDALotAddInfoSchema.Constants.US_StartDate;
			public const string US_Temperature = USFDALotAddInfoSchema.Constants.US_Temperature;
			public const string US_TemperatureQualifier = USFDALotAddInfoSchema.Constants.US_TemperatureQualifier;
			public const string US_LotNumberType = USFDALotAddInfoSchema.Constants.US_LotNumberType;
		}

		#endregion

		#region AddInfo Properties

		public ACEFDA FDA
		{
			get { return Factory.Load<ACEFDA>(B7_ParentID); }
		}

		public CPSCHeader CPSCHeader
		{
			get { return Factory.Load<CPSCHeader>(B7_ParentID); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USFDALotAddInfoLookups.LotNumberTypeCodeList))]
		public ZString US_LotNumberType
		{
			get { return AddInfo.US_LotNumberType; }
			set { AddInfo.US_LotNumberType = value; }
		}

		public ZPropertyInfo US_LotNumberTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_LotNumberType, x => AddInfo.US_LotNumberTypeInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USFDALotAddInfoLookups.DegreeTypeList))]
		public ZString US_DegreeType
		{
			get { return AddInfo.US_DegreeType; }
			set { AddInfo.US_DegreeType = value; }
		}

		public ZPropertyInfo US_DegreeTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_DegreeType, x => AddInfo.US_DegreeTypeInfo); }
		}

		public ZDateTime US_EndDate
		{
			get { return AddInfo.US_EndDate; }
			set { AddInfo.US_EndDate = value; }
		}

		public ZPropertyInfo US_EndDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_EndDate, x => AddInfo.US_EndDateInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USFDALotAddInfoLookups.LocationOfTempList))]
		public ZString US_LocationOfTemp
		{
			get { return AddInfo.US_LocationOfTemp; }
			set { AddInfo.US_LocationOfTemp = value; }
		}

		public ZPropertyInfo US_LocationOfTempInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_LocationOfTemp, x => AddInfo.US_LocationOfTempInfo); }
		}

		public ZString US_LotNumber
		{
			get { return AddInfo.US_LotNumber; }
			set { AddInfo.US_LotNumber = value; }
		}

		public ZPropertyInfo US_LotNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_LotNumber, x => AddInfo.US_LotNumberInfo); }
		}

		public ZDateTime US_StartDate
		{
			get { return AddInfo.US_StartDate; }
			set { AddInfo.US_StartDate = value; }
		}

		public ZPropertyInfo US_StartDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_StartDate, x => AddInfo.US_StartDateInfo); }
		}

		[DecimalPlaces(2)]
		[MeasureUnit(Schema.US_DegreeType, MeasureUnitType.Temperature)]
		public ZDecimal US_Temperature
		{
			get { return AddInfo.US_Temperature; }
			set { AddInfo.US_Temperature = value; }
		}

		public ZPropertyInfo US_TemperatureInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Temperature, x => AddInfo.US_TemperatureInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USFDALotAddInfoLookups.TemperatureQualifierList))]
		public ZString US_TemperatureQualifier
		{
			get { return AddInfo.US_TemperatureQualifier; }
			set { AddInfo.US_TemperatureQualifier = value; }
		}

		public ZPropertyInfo US_TemperatureQualifierInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_TemperatureQualifier, x => AddInfo.US_TemperatureQualifierInfo); }
		}

		bool AllCPSCFieldIsEmpty => GetCPSCUsedFieldsInfos().All(x => x.Value.IsEmpty);

		IEnumerable<ZPropertyInfo> GetCPSCUsedFieldsInfos()
		{
			yield return US_LotNumberTypeInfo;
			yield return US_LotNumberInfo;
			yield return US_StartDateInfo;
			yield return US_EndDateInfo;
		}

		#endregion

		#region Overrides

		public override void OnSaving()
		{
			base.OnSaving();
			if (Parent is CPSCHeader && AllCPSCFieldIsEmpty)
			{
				Delete();
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "Lot"; }
		}

		public void UpdateAddInfoProperties()
		{
			updateAddInfoProperties();
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();

			var result = (Lot)base.CloneInternal(args);
			return result;
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USFDALotAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USFDALotAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		USLotAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new USLotAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		USLotAddInfo fAddInfo;

		protected void updateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}

		Integration.Customs.US.IUSLotAddInfo Integration.Customs.US.IUSLot.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion

		#region IFDALot Members

		ZString IFDALot.TemperatureQualifier { get { return US_TemperatureQualifier; } }
		ZString IFDALot.DegreeType { get { return US_DegreeType; } }
		ZString IFDALot.LocationOfTemperatureRecording { get { return US_LocationOfTemp; } }
		ZString IFDALot.Number { get { return US_LotNumber; } }

		ZString IFDALot.NumberQualifier
		{
			get
			{
				var result = ZString.Empty;
				if (!US_LotNumber.IsEmpty)
				{
					var fda = FDA;
					result = fda != null && fda.US_ProgramCode == FDAProgramCodeList.Codes.FOO && fda.US_ProcessingCode == FDAProcessingCodeList.Codes.FOO_NSF ? "3" : "1";
				}

				return result;
			}
		}

		ZDate IFDALot.ProductionStartDate { get { return US_StartDate.Date; } }
		ZDate IFDALot.ProductionEndDate { get { return US_EndDate.Date; } }
		ZString IFDALot.Temperature
		{
			get
			{
				ZDecimal temperature = US_Temperature < 0 ? decimal.Negate(US_Temperature) : (decimal)US_Temperature;
				var values = temperature.ToString().Split('.');
				var firstValue = values[0].PadLeft(4, '0');
				var secondValue = values.Length == 2 ? values[1].PadRight(2, '0') : "00";
				return firstValue + secondValue;
			}
		}
		ZString IFDALot.NegativeTemperatureIndicator { get { return US_Temperature < 0 ? "X" : ""; } }

		#endregion

		#region ICPSCLot

		ZString ICPSCLot.NumberType { get { return US_LotNumberType; } }
		ZString ICPSCLot.Number { get { return US_LotNumber; } }
		ZDateTime ICPSCLot.StartDate { get { return US_StartDate; } }
		ZDateTime ICPSCLot.EndDate { get { return US_EndDate; } }

		#endregion
	}
}
