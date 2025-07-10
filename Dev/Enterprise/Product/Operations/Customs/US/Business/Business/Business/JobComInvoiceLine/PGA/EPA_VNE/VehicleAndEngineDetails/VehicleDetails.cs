using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business
{
	public class VehicleDetails : Customs.Business.MultiLineAddInfos.CusAddInfo<USVehicleDetailsAddInfo>, IVNEDetails, Integration.Customs.US.IVehicleDetails, ICusCodeDataTypeSupporter
	{
		public VehicleDetails(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.MultiLineAddInfos.CusAddInfo<USVehicleDetailsAddInfo>.Schema
		{
			public const string US_BuildMonth = USVehicleDetailsAddInfoSchema.Constants.US_BuildMonth;
			public const string US_BuildYear = USVehicleDetailsAddInfoSchema.Constants.US_BuildYear;
			public const string US_EngineBuildDate = USVehicleDetailsAddInfoSchema.Constants.US_EngineBuildDate;
			public const string US_EngineManufacturer = USVehicleDetailsAddInfoSchema.Constants.US_EngineManufacturer;
			public const string US_EngineModel = USVehicleDetailsAddInfoSchema.Constants.US_EngineModel;
			public const string US_EngineNumber = USVehicleDetailsAddInfoSchema.Constants.US_EngineNumber;
			public const string US_IdentityNumber = USVehicleDetailsAddInfoSchema.Constants.US_IdentityNumber;
			public const string US_IdentityNumberQualifier = USVehicleDetailsAddInfoSchema.Constants.US_IdentityNumberQualifier;
			public const string US_MfrDateType = USVehicleDetailsAddInfoSchema.Constants.US_MfrDateType;
			public const string US_BuildDateExplanation = USVehicleDetailsAddInfoSchema.Constants.US_BuildDateExplanation;
			public const string US_VehicleManufacturer = USVehicleDetailsAddInfoSchema.Constants.US_VehicleManufacturer;
		}

		#endregion

		#region Related

		internal Vehicle Vehicle
		{
			get { return Factory.Load<Vehicle>(B7_ParentID); }
		}

		[ChildEditable(true)]
		public VNEAdditionalNumberCollection AdditionalNumbers
		{
			get
			{
				if (additionalNumbers == null)
				{
					additionalNumbers = new VNEAdditionalNumberCollection(this);
					additionalNumbers.Load();
					RegisterEditableChildObject(additionalNumbers);
				}
				return additionalNumbers;
			}
		}
		VNEAdditionalNumberCollection additionalNumbers;

		#endregion

		#region AddInfo Properties

		[List(nameof(AddInfoLookups) + "." + nameof(USVehicleDetailsAddInfoLookups.MonthList))]
		public ZString US_BuildMonth
		{
			get { return AddInfo.US_BuildMonth; }
			set { AddInfo.US_BuildMonth = value; }
		}

		public ZPropertyInfo US_BuildMonthInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_BuildMonth, x => AddInfo.US_BuildMonthInfo); }
		}

		public ZString US_BuildYear
		{
			get { return AddInfo.US_BuildYear; }
			set { AddInfo.US_BuildYear = value; }
		}

		public ZPropertyInfo US_BuildYearInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_BuildYear, x => AddInfo.US_BuildYearInfo); }
		}

		public ZString US_VehicleManufacturer
		{
			get { return AddInfo.US_VehicleManufacturer; }
			set { AddInfo.US_VehicleManufacturer = value; }
		}

		public ZPropertyInfo US_VehicleManufacturerInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_VehicleManufacturer, x => AddInfo.US_VehicleManufacturerInfo); }
		}

		public ZDateTime US_EngineBuildDate
		{
			get { return AddInfo.US_EngineBuildDate; }
			set { AddInfo.US_EngineBuildDate = value; }
		}

		public ZPropertyInfo US_EngineBuildDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_EngineBuildDate, x => AddInfo.US_EngineBuildDateInfo); }
		}

		public ZString US_EngineManufacturer
		{
			get { return AddInfo.US_EngineManufacturer; }
			set { AddInfo.US_EngineManufacturer = value; }
		}

		public ZPropertyInfo US_EngineManufacturerInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_EngineManufacturer, x => AddInfo.US_EngineManufacturerInfo); }
		}

		public ZString US_EngineModel
		{
			get { return AddInfo.US_EngineModel; }
			set { AddInfo.US_EngineModel = value; }
		}

		public ZPropertyInfo US_EngineModelInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_EngineModel, x => AddInfo.US_EngineModelInfo); }
		}

		public ZString US_EngineNumber
		{
			get { return AddInfo.US_EngineNumber; }
			set
			{
				var hasChanges = US_EngineNumber != value;
				AddInfo.US_EngineNumber = value;

				if (hasChanges && !IsCopying)
				{
					if (US_EngineManufacturer.IsEmpty && !US_EngineNumber.IsEmpty)
					{
						US_EngineManufacturer = GetManufacturerFrominvoiceLine();
					}
				}
			}
		}

		public ZPropertyInfo US_EngineNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_EngineNumber, x => AddInfo.US_EngineNumberInfo); }
		}

		public ZString US_IdentityNumber
		{
			get { return AddInfo.US_IdentityNumber; }
			set
			{
				var hasChanges = US_IdentityNumber != value;
				AddInfo.US_IdentityNumber = value;

				if (hasChanges && !IsCopying)
				{
					if (US_VehicleManufacturer.IsEmpty && !US_IdentityNumber.IsEmpty)
					{
						US_VehicleManufacturer = GetManufacturerFrominvoiceLine();
					}
				}
			}
		}

		ZString GetManufacturerFrominvoiceLine()
		{
			var result = ZString.Empty;
			var invoiceLine = Vehicle.InvoiceLine;
			if (invoiceLine != null)
			{
				var manufacturerDetails = invoiceLine.ManufacturerDetails;
				if (manufacturerDetails != null)
				{
					result = manufacturerDetails.CompanyName;
				}
			}
			return result;
		}

		public ZPropertyInfo US_IdentityNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_IdentityNumber, x => AddInfo.US_IdentityNumberInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USVehicleDetailsAddInfoLookups.IdentityQualifier))]
		public ZString US_IdentityNumberQualifier
		{
			get { return AddInfo.US_IdentityNumberQualifier; }
			set { AddInfo.US_IdentityNumberQualifier = value; }
		}

		public ZPropertyInfo US_IdentityNumberQualifierInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_IdentityNumberQualifier, x => AddInfo.US_IdentityNumberQualifierInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USVehicleDetailsAddInfoLookups.DateTypes))]
		public ZString US_MfrDateType
		{
			get { return AddInfo.US_MfrDateType; }
			set
			{
				var oldValue = US_MfrDateType;
				AddInfo.US_MfrDateType = value;
				if (oldValue != value)
				{
					US_BuildDateExplanation = ZString.Empty;
				}
			}
		}

		public ZPropertyInfo US_MfrDateTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_MfrDateType, x => AddInfo.US_MfrDateTypeInfo); }
		}

		public ZString US_BuildDateExplanation
		{
			get { return AddInfo.US_BuildDateExplanation; }
			set { AddInfo.US_BuildDateExplanation = value; }
		}

		public bool US_BuildDateExplanation_ReadOnly
		{
			get { return US_MfrDateType != ManufactureDateTypeList.Codes.OTH; }
		}

		public ZPropertyInfo US_BuildDateExplanationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_BuildDateExplanation, x => AddInfo.US_BuildDateExplanationInfo); }
		}

		internal void SetEmptyValueIfRequired()
		{
			US_EngineBuildDate = ZDateTime.Empty;
			US_EngineManufacturer = ZString.Empty;
			US_EngineModel = ZString.Empty;
			US_EngineNumber = ZString.Empty;
			US_MfrDateType = ZString.Empty;
			US_BuildDateExplanation = ZString.Empty;
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "VehicleDetails"; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();

			var result = (VehicleDetails)base.CloneInternal(args);
			return result;
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USVehicleDetailsAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USVehicleDetailsAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		USVehicleDetailsAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new USVehicleDetailsAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		USVehicleDetailsAddInfo fAddInfo;

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

		Integration.Customs.US.IVehicleDetailsAddInfo Integration.Customs.US.IVehicleDetails.AddInfo
		{
			get { return AddInfo; }
		}
		#endregion

		#region IVNEDetails Members

		ZString IVNEDetails.BuildMonth
		{
			get { return US_BuildMonth; }
		}

		ZString IVNEDetails.BuildYear
		{
			get { return US_BuildYear; }
		}

		ZString IVNEDetails.VehicleManufacturer
		{
			get { return US_VehicleManufacturer; }
		}

		ZString IVNEDetails.EngineBuildDate
		{
			get { return US_EngineBuildDate.IsValid ? (US_EngineBuildDate.Month.ToString().Length < 2 ? "0" + US_EngineBuildDate.Month.ToString() : US_EngineBuildDate.Month.ToString()) + US_EngineBuildDate.Year.ToString() : ""; }
		}

		ZString IVNEDetails.EngineManufacturer
		{
			get { return US_EngineManufacturer; }
		}

		ZString IVNEDetails.EngineModel
		{
			get { return US_EngineModel; }
		}

		ZString IVNEDetails.EngineNumber
		{
			get { return US_EngineNumber; }
		}

		ZString IVNEDetails.IdentityNumber
		{
			get { return US_IdentityNumber; }
		}

		ZString IVNEDetails.IdentityNumberQualifier
		{
			get { return US_IdentityNumberQualifier == ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN ? (ZString)"AKG" : US_IdentityNumberQualifier; }
		}

		ZString IVNEDetails.Model
		{
			get
			{
				var vehicle = Vehicle;
				return vehicle != null ? vehicle.US_VehicleModel : ZString.Empty;
			}
		}

		ZString IVNEDetails.ManufactureDateType
		{
			get { return US_MfrDateType; }
		}

		ZString IVNEDetails.BuildDateExplanation
		{
			get { return US_MfrDateType == ManufactureDateTypeList.Codes.OTH ? US_BuildDateExplanation : ZString.Empty; }
		}

		IEnumerable<VNEAdditionalNumbers> IVNEDetails.EngineAdditionalNumbers
		{
			get { return AdditionalNumbers.Cast<VNEAdditionalNumbers>().Where(x => x.NumberType == ItemIdentityNumberQualifierList.Codes.EngineNumber); }
		}

		IEnumerable<VNEAdditionalNumbers> IVNEDetails.VehicleAdditionalNumbers
		{
			get { return AdditionalNumbers.Cast<VNEAdditionalNumbers>().Where(x => x.NumberType == ItemIdentityNumberQualifierList.Codes.SerialNumber || x.NumberType == ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN); }
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.VNEAdditionalNumber, typeof(VNEAdditionalNumber));
			return result;
		}

		#endregion
	}
}
