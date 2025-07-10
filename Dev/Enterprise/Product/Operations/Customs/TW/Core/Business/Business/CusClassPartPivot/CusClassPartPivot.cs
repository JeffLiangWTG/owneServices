using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class CusClassPartPivot : AutoTWCusClassPartPivot, Integration.Customs.TW.ICusClassPartPivot, Integration.Customs.ICusSupportingInfoTypeSupporter, ICusClassPartPivotRefTypeSupporter, ISupportDataImporting
	{
		public CusClassPartPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusClassPartPivot.Schema
		{
			public const string CI_EngineTypePrintModeDescription = "CI_EngineTypePrintModeDescription";
			public new const int CI_NDescriptionMaxLength = 512;
			public const string AssignedNumber1 = nameof(CusClassPartPivot.AssignedNumber1);
			public const string AssignedNumber2 = nameof(CusClassPartPivot.AssignedNumber2);
			public const string AssignedNumber3 = nameof(CusClassPartPivot.AssignedNumber3);
			public const string AssignedNumber4 = nameof(CusClassPartPivot.AssignedNumber4);
			public const string AssignedNumber5 = nameof(CusClassPartPivot.AssignedNumber5);
			public const string AssignedNumber6 = nameof(CusClassPartPivot.AssignedNumber6);
			public const string AssignedNumber7 = nameof(CusClassPartPivot.AssignedNumber7);
			public const string AssignedNumber8 = nameof(CusClassPartPivot.AssignedNumber8);
			public const string AssignedNumber9 = nameof(CusClassPartPivot.AssignedNumber9);
			public const string AssignedNumber10 = nameof(CusClassPartPivot.AssignedNumber10);
			public const string PermitCusSupportingNo1 = nameof(CusClassPartPivot.PermitCusSupportingNo1);
			public const string PermitCusSupportingLineNo1 = nameof(CusClassPartPivot.PermitCusSupportingLineNo1);
			public const string PermitCusSupportingNo2 = nameof(CusClassPartPivot.PermitCusSupportingNo2);
			public const string PermitCusSupportingLineNo2 = nameof(CusClassPartPivot.PermitCusSupportingLineNo2);
			public const string PermitCusSupportingNo3 = nameof(CusClassPartPivot.PermitCusSupportingNo3);
			public const string PermitCusSupportingLineNo3 = nameof(CusClassPartPivot.PermitCusSupportingLineNo3);
			public const string PermitCusSupportingNo4 = nameof(CusClassPartPivot.PermitCusSupportingNo4);
			public const string PermitCusSupportingLineNo4 = nameof(CusClassPartPivot.PermitCusSupportingLineNo4);
			public const string PermitCusSupportingNo5 = nameof(CusClassPartPivot.PermitCusSupportingNo5);
			public const string PermitCusSupportingLineNo5 = nameof(CusClassPartPivot.PermitCusSupportingLineNo5);
		}

		#region Property Overrides

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_Compositions", Caption = "Specification")]
		public override ZString CI_Compositions { get => base.CI_Compositions; set => base.CI_Compositions = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_TariffAdditionalCode", Caption = "Tariff Additional Code")]
		public override ZString CI_TariffAdditionalCode { get => base.CI_TariffAdditionalCode; set => base.CI_TariffAdditionalCode = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_AlcoholPercentage", Caption = "Alcohol %")]
		public override ZDecimal CI_AlcoholPercentage { get => base.CI_AlcoholPercentage; set => base.CI_AlcoholPercentage = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_Price", Caption = "Unit Price")]
		public override ZDecimal CI_Price { get => base.CI_Price; set => base.CI_Price = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_RN_NKCountryOfOrigin", Caption = "Goods Origin")]
		public override ZString CI_RN_NKCountryOfOrigin { get => base.CI_RN_NKCountryOfOrigin; set => base.CI_RN_NKCountryOfOrigin = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_Description", Caption = "English Description", ShortCaption = "English Desc.")]
		public override ZString CI_Description { get => base.CI_Description; set => base.CI_Description = value; }

		[MaxLength(Schema.CI_NDescriptionMaxLength)]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_NDescription", Caption = "Chinese Description", ShortCaption = "Chinese Desc.")]
		public override ZString CI_NDescription { get => base.CI_NDescription; set => base.CI_NDescription = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_ModelYear", Caption = "Model Year")]
		public override ZShort CI_ModelYear { get => base.CI_ModelYear; set => base.CI_ModelYear = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_Displacement", Caption = "Displacement (cc)")]
		public override ZString CI_Displacement { get => base.CI_Displacement; set => base.CI_Displacement = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_NumberOfDoor", Caption = "Number of Door(s)")]
		public override ZShort CI_NumberOfDoor { get => base.CI_NumberOfDoor; set => base.CI_NumberOfDoor = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_Seats", Caption = "Number of Seat(s)")]
		public override ZShort CI_Seats { get => base.CI_Seats; set => base.CI_Seats = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_Cylinders", Caption = "Number of Cylinder(s)")]
		public override ZShort CI_Cylinders { get => base.CI_Cylinders; set => base.CI_Cylinders = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_Gears", Caption = "Number of Gear(s)")]
		public override ZShort CI_Gears { get => base.CI_Gears; set => base.CI_Gears = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_CustomsOwnerPartNo", Caption = "Owner Part No.")]
		public override ZString CI_CustomsOwnerPartNo { get => base.CI_CustomsOwnerPartNo; set => base.CI_CustomsOwnerPartNo = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_CustomsSupplierPartNo", Caption = "Supplier Part No.")]
		public override ZString CI_CustomsSupplierPartNo { get => base.CI_CustomsSupplierPartNo; set => base.CI_CustomsSupplierPartNo = value; }

		public override ZString CI_TariffNum
		{
			get => base.CI_TariffNum;
			set
			{
				var oldValue = CI_TariffNum;
				base.CI_TariffNum = value;
				var hasChanged = CI_TariffNum != oldValue;
				if (!IsCopying && hasChanged)
				{
					CI_EPTDigit1 = ZString.Empty;
					CI_EPTDigit2 = ZString.Empty;
					CI_EPTDigit3 = ZString.Empty;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateCI_EPTDigit1();
					Validation.ValidateCI_EPTDigit2();
					Validation.ValidateCI_EPTDigit3();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.PartPivotUOMList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_PartPivotUOM", Caption = "Unit of Measure")]
		public override ZString CI_PartPivotUOM { get => base.CI_PartPivotUOM; set => base.CI_PartPivotUOM = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_CarType", Caption = "Car Type")]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.CarTypeCodeList))]
		public override ZString CI_CarType { get => base.CI_CarType; set => base.CI_CarType = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_Transmission", Caption = "Transmission")]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.TransmissionCodeList))]
		public override ZString CI_Transmission { get => base.CI_Transmission; set => base.CI_Transmission = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_EngineType", Caption = "Engine Type")]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.EngineTypeCodeList))]
		public override ZString CI_EngineType { get => base.CI_EngineType; set => base.CI_EngineType = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_LHD", Caption = "Left Side Steering")]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.LeftSideSteeringCodeList))]
		public override ZString CI_LHD { get => base.CI_LHD; set => base.CI_LHD = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_HasCatalystConverter", Caption = "Catalytic Converter?")]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.CatalystConverterPrintModeList))]
		public override ZString CI_HasCatalystConverter { get => base.CI_HasCatalystConverter; set => base.CI_HasCatalystConverter = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_EquipmentPrintMode", Caption = "Standard Equipment")]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.EquipmentPrintModeList))]
		public override ZString CI_EquipmentPrintMode { get => base.CI_EquipmentPrintMode; set => base.CI_EquipmentPrintMode = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_CarCondition", Caption = "Condition")]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.CarConditionCodeList))]
		public override ZString CI_CarCondition { get => base.CI_CarCondition; set => base.CI_CarCondition = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_ModeOfStatistics", Caption = "Mode Of Statistics")]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.ModeOfStatistics))]
		public override ZString CI_ModeOfStatistics { get => base.CI_ModeOfStatistics; set => base.CI_ModeOfStatistics = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_DutyTreatment", Caption = "Duty Treatment")]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.DutyTreatment))]
		public override ZString CI_DutyTreatment { get => base.CI_DutyTreatment; set => base.CI_DutyTreatment = value; }

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.CurrencyList))]
		public override ZString CI_PriceCurr { get => base.CI_PriceCurr; set => base.CI_PriceCurr = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_EPTDigit1", Caption = "Container Material", ShortCaption = "Material")]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.ContainerMaterialList))]
		public override ZString CI_EPTDigit1 { get => base.CI_EPTDigit1; set => base.CI_EPTDigit1 = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_EPTDigit2", Caption = "Container Capacity (cc)", ShortCaption = "Capacity (cc)")]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.ContainerCapacityList))]
		public override ZString CI_EPTDigit2 { get => base.CI_EPTDigit2; set => base.CI_EPTDigit2 = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_EPTDigit3", Caption = "No. of Container Material(s)", ShortCaption = "No. of Material(s)")]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.ContainerMaterialNumberList))]
		public override ZString CI_EPTDigit3 { get => base.CI_EPTDigit3; set => base.CI_EPTDigit3 = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|CI_DeclGoodsDescMode", Caption = "Declaration Goods Description Mode", ShortCaption = "Desc. Mode", FullDescription = "Indicates the mode of bringing the Goods description of the product code into the Goods description of the Invoice Line.")]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.DeclarationGoodsDescriptionModeList))]
		public override ZString CI_DeclGoodsDescMode { get => base.CI_DeclGoodsDescMode; set => base.CI_DeclGoodsDescMode = value; }

		#endregion

		#region ISupportDataImporting Members

		bool ISupportDataImporting.IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}
		bool fIsImportingData;

		#endregion

		#region PermitCusSupportingNos PermitCusSupportingLineNos
		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|PermitCusSupportingNo1", Caption = "Permit No 1")]
		[MaxLength(14)]
		public ZString PermitCusSupportingNo1
		{
			get => GetPermitCusSupportingNo(0);
			set
			{
				var oldValue = PermitCusSupportingNo1;
				SetPermitCusSupportingNo(0, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != PermitCusSupportingNo1))
				{
					PermitCusSupportingNo1Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo PermitCusSupportingNo1Info => GetPermitCusSupportingNoInfo(0, nameof(PermitCusSupportingNo1));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|PermitCusSupportingLineNo1", Caption = "Permit Line No 1")]
		public ZInt PermitCusSupportingLineNo1
		{
			get => GetPermitCusSupportingLineNo(0);
			set => SetPermitCusSupportingLineNo(0, value);
		}

		public ZPropertyInfo PermitCusSupportingLineNo1Info => GetPermitCusSupportingLineNoInfo(0, nameof(PermitCusSupportingLineNo1));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|PermitCusSupportingNo2", Caption = "Permit No 2")]
		[MaxLength(14)]
		public ZString PermitCusSupportingNo2
		{
			get => GetPermitCusSupportingNo(1);
			set
			{
				var oldValue = PermitCusSupportingNo2;
				SetPermitCusSupportingNo(1, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != PermitCusSupportingNo2))
				{
					PermitCusSupportingNo2Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo PermitCusSupportingNo2Info => GetPermitCusSupportingNoInfo(1, nameof(PermitCusSupportingNo2));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|PermitCusSupportingLineNo2", Caption = "Permit Line No 2")]
		public ZInt PermitCusSupportingLineNo2
		{
			get => GetPermitCusSupportingLineNo(1);
			set => SetPermitCusSupportingLineNo(1, value);
		}

		public ZPropertyInfo PermitCusSupportingLineNo2Info => GetPermitCusSupportingLineNoInfo(1, nameof(PermitCusSupportingLineNo2));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|PermitCusSupportingNo3", Caption = "Permit No 3")]
		[MaxLength(14)]
		public ZString PermitCusSupportingNo3
		{
			get => GetPermitCusSupportingNo(2);
			set
			{
				var oldValue = PermitCusSupportingNo3;
				SetPermitCusSupportingNo(2, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != PermitCusSupportingNo3))
				{
					PermitCusSupportingNo3Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo PermitCusSupportingNo3Info => GetPermitCusSupportingNoInfo(2, nameof(PermitCusSupportingNo3));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|PermitCusSupportingLineNo3", Caption = "Permit Line No 3")]
		public ZInt PermitCusSupportingLineNo3
		{
			get => GetPermitCusSupportingLineNo(2);
			set => SetPermitCusSupportingLineNo(2, value);
		}

		public ZPropertyInfo PermitCusSupportingLineNo3Info => GetPermitCusSupportingLineNoInfo(2, nameof(PermitCusSupportingLineNo3));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|PermitCusSupportingNo4", Caption = "Permit No 4")]
		[MaxLength(14)]
		public ZString PermitCusSupportingNo4
		{
			get => GetPermitCusSupportingNo(3);
			set
			{
				var oldValue = PermitCusSupportingNo4;
				SetPermitCusSupportingNo(3, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != PermitCusSupportingNo4))
				{
					PermitCusSupportingNo4Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo PermitCusSupportingNo4Info => GetPermitCusSupportingNoInfo(3, nameof(PermitCusSupportingNo4));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|PermitCusSupportingLineNo4", Caption = "Permit Line No 4")]
		public ZInt PermitCusSupportingLineNo4
		{
			get => GetPermitCusSupportingLineNo(3);
			set => SetPermitCusSupportingLineNo(3, value);
		}

		public ZPropertyInfo PermitCusSupportingLineNo4Info => GetPermitCusSupportingLineNoInfo(3, nameof(PermitCusSupportingLineNo4));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|PermitCusSupportingNo5", Caption = "Permit No 5")]
		[MaxLength(14)]
		public ZString PermitCusSupportingNo5
		{
			get => GetPermitCusSupportingNo(4);
			set
			{
				var oldValue = PermitCusSupportingNo5;
				SetPermitCusSupportingNo(4, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != PermitCusSupportingNo5))
				{
					PermitCusSupportingNo5Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo PermitCusSupportingNo5Info => GetPermitCusSupportingNoInfo(4, nameof(PermitCusSupportingNo5));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|PermitCusSupportingLineNo5", Caption = "Permit Line No 5")]
		public ZInt PermitCusSupportingLineNo5
		{
			get => GetPermitCusSupportingLineNo(4);
			set => SetPermitCusSupportingLineNo(4, value);
		}

		public ZPropertyInfo PermitCusSupportingLineNo5Info => GetPermitCusSupportingLineNoInfo(4, nameof(PermitCusSupportingLineNo5));

		[ChildEditable(true)]
		public ProductPermitCusSupportingCollection ProductPermitCusSupportingCollection
		{
			get
			{
				if (fProductPermitCusSupportingCollection == null)
				{
					fProductPermitCusSupportingCollection = new ProductPermitCusSupportingCollection(this);
					fProductPermitCusSupportingCollection.Load();
					RegisterEditableChildObject(fProductPermitCusSupportingCollection);
				}
				return fProductPermitCusSupportingCollection;
			}
		}

		ProductPermitCusSupportingCollection fProductPermitCusSupportingCollection;

		ZString GetPermitCusSupportingNo(int index) => ProductPermitCusSupportingCollection.Count <= index ? ZString.Empty : SortedProductPermitCusSupportingCollection[index].CSI_ReferenceNumber;

		void SetPermitCusSupportingNo(int index, ZString value)
		{
			var permitCusSupporting = ProductPermitCusSupportingCollection.Count <= index ? ProductPermitCusSupportingCollection.AddNew() : SortedProductPermitCusSupportingCollection[index];
			permitCusSupporting.CSI_ReferenceNumber = value;
		}

		ZPropertyInfo GetPermitCusSupportingNoInfo(int index, string propertyName) => ProductPermitCusSupportingCollection.Count <= index ? GetZPropertyInfo(propertyName) : GetWrappedZPropertyInfo(propertyName, x => SortedProductPermitCusSupportingCollection[index].CSI_ReferenceNumberInfo);

		ZInt GetPermitCusSupportingLineNo(int index) => ProductPermitCusSupportingCollection.Count <= index ? ZInt.Zero : SortedProductPermitCusSupportingCollection[index].CSI_LineNo;

		void SetPermitCusSupportingLineNo(int index, ZInt value)
		{
			var permitCusSupporting = ProductPermitCusSupportingCollection.Count <= index ? ProductPermitCusSupportingCollection.AddNew() : SortedProductPermitCusSupportingCollection[index];
			permitCusSupporting.CSI_LineNo = value;
		}

		ZPropertyInfo GetPermitCusSupportingLineNoInfo(int index, string propertyName) => ProductPermitCusSupportingCollection.Count <= index ? GetZPropertyInfo(propertyName) : GetWrappedZPropertyInfo(propertyName, x => SortedProductPermitCusSupportingCollection[index].CSI_LineNoInfo);

		internal List<ProductPermitCusSupporting> SortedProductPermitCusSupportingCollection
		{
			get
			{
				var collection = ProductPermitCusSupportingCollection.Cast<ProductPermitCusSupporting>();
				return (IsCopying || ((ISupportDataImporting)this).IsImportingData) ? collection.ToList() : collection.OrderBy(c => c.CSI_ItemNumber).ToList();
			}
		}

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type> {
				{ CusSupportingInfoTypeList.Codes.PermitNumber, typeof(ProductPermitCusSupporting) }
			};
			return result;
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}
		#endregion

		#region Assigned Numbers
		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|AssignedNumber1", Caption = "Assigned Number 1", MediumCaption = "Assigned No. 1", ShortCaption = "AS NO. 1")]
		[MaxLength(35)]
		public ZString AssignedNumber1
		{
			get => GetAssignedNumber(0);
			set
			{
				var oldValue = AssignedNumber1;
				SetAssignedNumber(0, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != AssignedNumber1))
				{
					AssignedNumber1Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo AssignedNumber1Info => GetAssignedNumberInfo(0, nameof(AssignedNumber1));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|AssignedNumber2", Caption = "Assigned Number 2", MediumCaption = "Assigned No. 2", ShortCaption = "AS NO. 2")]
		[MaxLength(35)]
		public ZString AssignedNumber2
		{
			get => GetAssignedNumber(1);
			set
			{
				var oldValue = AssignedNumber2;
				SetAssignedNumber(1, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != AssignedNumber2))
				{
					AssignedNumber2Info.RefreshBinding(oldValue);
				}
			}
		}
		public ZPropertyInfo AssignedNumber2Info => GetAssignedNumberInfo(1, nameof(AssignedNumber2));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|AssignedNumber3", Caption = "Assigned Number 3", MediumCaption = "Assigned No. 3", ShortCaption = "AS NO. 3")]
		[MaxLength(35)]
		public ZString AssignedNumber3
		{
			get => GetAssignedNumber(2);
			set
			{
				var oldValue = AssignedNumber3;
				SetAssignedNumber(2, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != AssignedNumber3))
				{
					AssignedNumber3Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo AssignedNumber3Info => GetAssignedNumberInfo(2, nameof(AssignedNumber3));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|AssignedNumber4", Caption = "Assigned Number 4", MediumCaption = "Assigned No. 4", ShortCaption = "AS NO. 4")]
		[MaxLength(35)]
		public ZString AssignedNumber4
		{
			get => GetAssignedNumber(3);
			set
			{
				var oldValue = AssignedNumber4;
				SetAssignedNumber(3, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != AssignedNumber4))
				{
					AssignedNumber4Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo AssignedNumber4Info => GetAssignedNumberInfo(3, nameof(AssignedNumber4));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|AssignedNumber5", Caption = "Assigned Number 5", MediumCaption = "Assigned No. 5", ShortCaption = "AS NO. 5")]
		[MaxLength(35)]
		public ZString AssignedNumber5
		{
			get => GetAssignedNumber(4);
			set
			{
				var oldValue = AssignedNumber5;
				SetAssignedNumber(4, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != AssignedNumber5))
				{
					AssignedNumber5Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo AssignedNumber5Info => GetAssignedNumberInfo(4, nameof(AssignedNumber5));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|AssignedNumber6", Caption = "Assigned Number 6", MediumCaption = "Assigned No. 6", ShortCaption = "AS NO. 6")]
		[MaxLength(35)]
		public ZString AssignedNumber6
		{
			get => GetAssignedNumber(5);
			set
			{
				var oldValue = AssignedNumber6;
				SetAssignedNumber(5, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != AssignedNumber6))
				{
					AssignedNumber6Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo AssignedNumber6Info => GetAssignedNumberInfo(5, nameof(AssignedNumber6));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|AssignedNumber7", Caption = "Assigned Number 7", MediumCaption = "Assigned No. 7", ShortCaption = "AS NO. 7")]
		[MaxLength(35)]
		public ZString AssignedNumber7
		{
			get => GetAssignedNumber(6);
			set
			{
				var oldValue = AssignedNumber7;
				SetAssignedNumber(6, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != AssignedNumber7))
				{
					AssignedNumber7Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo AssignedNumber7Info => GetAssignedNumberInfo(6, nameof(AssignedNumber7));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|AssignedNumber8", Caption = "Assigned Number 8", MediumCaption = "Assigned No. 8", ShortCaption = "AS NO. 8")]
		[MaxLength(35)]
		public ZString AssignedNumber8
		{
			get => GetAssignedNumber(7);
			set
			{
				var oldValue = AssignedNumber8;
				SetAssignedNumber(7, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != AssignedNumber8))
				{
					AssignedNumber8Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo AssignedNumber8Info => GetAssignedNumberInfo(7, nameof(AssignedNumber8));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|AssignedNumber9", Caption = "Assigned Number 9", MediumCaption = "Assigned No. 9", ShortCaption = "AS NO. 9")]
		[MaxLength(35)]
		public ZString AssignedNumber9
		{
			get => GetAssignedNumber(8);
			set
			{
				var oldValue = AssignedNumber9;
				SetAssignedNumber(8, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != AssignedNumber9))
				{
					AssignedNumber9Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo AssignedNumber9Info => GetAssignedNumberInfo(8, nameof(AssignedNumber9));

		[ResourceStringData("Enterprise.Customs.TW.Business.CusClassPartPivot|AssignedNumber10", Caption = "Assigned Number 10", MediumCaption = "Assigned No. 10", ShortCaption = "AS NO. 10")]
		[MaxLength(35)]
		public ZString AssignedNumber10
		{
			get => GetAssignedNumber(9);
			set
			{
				var oldValue = AssignedNumber10;
				SetAssignedNumber(9, value);
				if (!IsCopying && !((ISupportDataImporting)this).IsImportingData && (oldValue != AssignedNumber10))
				{
					AssignedNumber10Info.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo AssignedNumber10Info => GetAssignedNumberInfo(9, nameof(AssignedNumber10));

		ZString GetAssignedNumber(int index) => AssignedCusClassPartPivotRefCollection.Count <= index ? ZString.Empty : AssignedCusClassPartPivotRefCollection[index].CIR_ReferenceNumber;

		void SetAssignedNumber(int index, ZString value)
		{
			var assignedJobComInvLineRef = AssignedCusClassPartPivotRefCollection.Count <= index ? AssignedCusClassPartPivotRefCollection.AddNew() : AssignedCusClassPartPivotRefCollection[index];
			assignedJobComInvLineRef.CIR_ReferenceNumber = value;
		}

		ZPropertyInfo GetAssignedNumberInfo(int index, string propertyName) => AssignedCusClassPartPivotRefCollection.Count <= index ? GetZPropertyInfo(propertyName) : GetWrappedZPropertyInfo(propertyName, x => AssignedCusClassPartPivotRefCollection[index].CIR_ReferenceNumberInfo);

		[ChildEditable(true)]
		public AssignedCusClassPartPivotRefCollection AssignedCusClassPartPivotRefCollection
		{
			get
			{
				if (fAssignedCusClassPartPivotRefCollection == null)
				{
					fAssignedCusClassPartPivotRefCollection = new AssignedCusClassPartPivotRefCollection(this);
					fAssignedCusClassPartPivotRefCollection.Load();
					RegisterEditableChildObject(fAssignedCusClassPartPivotRefCollection);
				}
				return fAssignedCusClassPartPivotRefCollection;
			}
		}

		AssignedCusClassPartPivotRefCollection fAssignedCusClassPartPivotRefCollection;

		bool IsAssignedCusClassPartPivotRefCollectionLoaded => fAssignedCusClassPartPivotRefCollection != null && fAssignedCusClassPartPivotRefCollection.IsLoaded;

		public IDictionary<ZString, Type> GetCusClassPartPivotRefTypes()
		{
			var result = new Dictionary<ZString, Type> {
				{ JobComInvLineRefsType.Codes.AssignedNumber, typeof(AssignedCusClassPartPivotRef) }
			};
			return result;
		}

		protected override bool SupportCusClassPartPivotRef => true;

		void ICusClassPartPivotRefTypeSupporter.ReloadCollection(ZString referenceType)
		{
			if (referenceType == JobComInvLineRefsType.Codes.AssignedNumber)
			{
				if (IsAssignedCusClassPartPivotRefCollectionLoaded)
				{
					AssignedCusClassPartPivotRefCollection.Reload(true);
				}
			}
		}

		#endregion

		#region Lookups
		public new CusClassPartPivotLookups Lookups => (CusClassPartPivotLookups)base.Lookups;
		protected override Customs.Business.CusClassPartPivotLookups GetNewLookups() => new CusClassPartPivotLookups(this);
		#endregion

		#region Validation
		public new CusClassPartPivotValidation Validation => (CusClassPartPivotValidation)base.Validation;
		protected override Customs.Business.CusClassPartPivotValidation GetNewValidation() => new CusClassPartPivotValidation(this);

		#endregion

		public ZBool ShouldAllowEnvironmentalProtectionTariff => UniversalTariff?.HasAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.EnvironmentalProtectionTariff) ?? false;

		public ZBool IsCarRelatedTariff => CommonHelper.CheckIsCarRelatedTariff(CI_TariffNum) && IsImportClassification;

		protected override TariffFormatter GetTariffFormatter()
		{
			return new TaiwanTariffFormatter();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CI_DeclGoodsDescMode = DeclarationGoodsDescriptionModeList.Codes.BTH;
		}
	}
}
