
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.CFS.Business
{
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	public class CoLoadWizardShipment : NonPersistentBusinessObject, IObsoleteValidation, IDefaultNumberOfDecimalsSupporter
	{
		#region Schema
		public abstract class Schema
		{
			public const string CW_OH_CoLoadForwarder = "CW_OH_CoLoadForwarder";
			public const string CW_HouseBill = "CW_HouseBill";
			public const string CW_NumberOfPackages = "CW_NumberOfPackages";
			public const string CW_NumberOfPackagesUQ = "CW_NumberOfPackagesUQ";
			public const string CW_Weight = "CW_Weight";
			public const string CW_WeightUQ = "CW_WeightUQ";
			public const string CW_Volume = "CW_Volume";
			public const string CW_VolumeUQ = "CW_VolumeUQ";
			public const string CW_GoodsDescription = "CW_GoodsDescription";
			public const string CW_MarksAndNumbers = "CW_MarksAndNumbers";
			public const string CW_OH_Consignee = "CW_OH_Consignee";
			public const string CW_OH_Consignor = "CW_OH_Consignor";
			public const string CW_TempOrgName = "CW_TempOrgName";
			public const string CW_TempOrgAddress1 = "CW_TempOrgAddress1";
			public const string CW_TempOrgAddress2 = "CW_TempOrgAddress2";
			public const string CW_TempOrgCity = "CW_TempOrgCity";
			public const string CW_TempOrgState = "CW_TempOrgState";
			public const string CW_TempOrgPostcode = "CW_TempOrgPostcode";
			public const string CW_TempOrgUNLOCO = "CW_TempOrgUNLOCO";
			public const string CW_CreateAnotherShipment = "CW_CreateAnotherShipment";
			public const string CW_EnterConsignor = "CW_EnterConsignor";
			public const string CW_TempOrgCreateNew = "CW_TempOrgCreateNew";
			public const string CW_TempOrgMarkTemporary = "CW_TempOrgMarkTemporary";
			public const string CW_TempOrgCreateActionExplaination = "CW_TempOrgCreateActionExplaination";
			public const string TableName = "CoLoadWizardShipment";
		}

		#endregion

		public CoLoadWizardShipment(BusinessObjectFactory factory, CFSShipment parentShipment) : base(factory)
		{
			fCW_OH_CoLoadForwarder = parentShipment.ConsigneePK;
			fParentShipment = parentShipment;
		}

		public CoLoadWizardShipment(PackUnpackShipment parentShipment) : base(parentShipment.Factory)
		{
			fCW_OH_CoLoadForwarder = parentShipment.ConsigneePK;
			fParentShipment = parentShipment;
		}

		#region Properties

		#region CoLoadForwarder

		[RelatedBusinessObject("CoLoadForwarder")]
		[List("Forwarders")]
		public ZGuid CW_OH_CoLoadForwarder
		{
			get { return fCW_OH_CoLoadForwarder; }
		}

		public ZPropertyInfo CW_OH_CoLoadForwarderInfo
		{
			get { return GetZPropertyInfo(Schema.CW_OH_CoLoadForwarder); }
		}

		public OrgHeader CoLoadForwarder
		{
			get
			{
				return Factory.Load<OrgHeader>(CW_OH_CoLoadForwarder);
			}
		}

		#endregion

		#region Housebill

		[MaxLength(AutoJobShipment.Schema.JS_HouseBillMaxLength)]
		public ZString CW_HouseBill
		{
			get { return fCW_HouseBill; }
			set
			{
				CheckMaximumLength(CW_HouseBillInfo, value);
				fCW_HouseBill = value;
				if (!IsValidationSuspended)
				{
					ValidateCW_HouseBill();
				}
				CW_HouseBillInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CW_HouseBillInfo
		{
			get { return GetZPropertyInfo(Schema.CW_HouseBill); }
		}

		#endregion

		#region Number of Packages

		public ZInt CW_NumberOfPackages
		{
			get { return fCW_NumberOfPackages; }
			set
			{
				fCW_NumberOfPackages = value;
				if (!IsValidationSuspended)
				{
					ValidateCW_NumberOfPackages();
				}
				CW_NumberOfPackagesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CW_NumberOfPackagesInfo
		{
			get { return GetZPropertyInfo(Schema.CW_NumberOfPackages); }
		}

		[List("CW_PackagesUQ_List")]
		[MaxLength(3)]
		public ZString CW_NumberOfPackagesUQ
		{
			get { return fCW_NumberOfPackagesUQ; }
			set
			{
				CheckMaximumLength(CW_NumberOfPackagesUQInfo, value);
				fCW_NumberOfPackagesUQ = value;
				if (!IsValidationSuspended)
				{
					ValidateCW_NumberOfPackagesUQ();
				}
				CW_NumberOfPackagesUQInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CW_NumberOfPackagesUQInfo
		{
			get { return GetZPropertyInfo(Schema.CW_NumberOfPackagesUQ); }
		}

		#endregion

		#region Weight

		[MeasureUnit(Schema.CW_WeightUQ, MeasureUnitType.Weight)]
		public ZDecimal CW_Weight
		{
			get { return fCW_Weight; }
			set
			{
				fCW_Weight = this.GetRoundedValue(CW_WeightInfo, value);

				if (!IsValidationSuspended)
				{
					ValidateCW_Weight();
				}
				CW_WeightInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CW_WeightInfo
		{
			get { return GetZPropertyInfo(Schema.CW_Weight); }
		}

		[List("CW_WeightUQ_List")]
		[MaxLength(2)]
		public ZString CW_WeightUQ
		{
			get { return fCW_WeightUQ; }
			set
			{
				CheckMaximumLength(CW_WeightUQInfo, value);
				fCW_WeightUQ = value;

				this.SetRoundedValue(CW_WeightInfo);

				if (!IsValidationSuspended)
				{
					ValidateCW_WeightUQ();
				}
				CW_WeightUQInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CW_WeightUQInfo
		{
			get { return GetZPropertyInfo(Schema.CW_WeightUQ); }
		}

		#endregion

		#region Volume

		[MeasureUnit(Schema.CW_VolumeUQ, MeasureUnitType.Volume)]
		public ZDecimal CW_Volume
		{
			get { return fCW_Volume; }
			set
			{
				fCW_Volume = this.GetRoundedValue(CW_VolumeInfo, value);

				if (!IsValidationSuspended)
				{
					ValidateCW_Volume();
				}
				CW_VolumeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CW_VolumeInfo
		{
			get { return GetZPropertyInfo(Schema.CW_Volume); }
		}

		[List("CW_VolumeUQ_List")]
		[MaxLength(2)]
		public ZString CW_VolumeUQ
		{
			get { return fCW_VolumeUQ; }
			set
			{
				CheckMaximumLength(CW_VolumeUQInfo, value);
				fCW_VolumeUQ = value;

				this.SetRoundedValue(CW_VolumeInfo);

				if (!IsValidationSuspended)
				{
					ValidateCW_VolumeUQ();
				}
				CW_VolumeUQInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CW_VolumeUQInfo
		{
			get { return GetZPropertyInfo(Schema.CW_VolumeUQ); }
		}

		#endregion

		#region Goods Description

		[MaxLength(35)]
		public ZString CW_GoodsDescription
		{
			get { return fCW_GoodsDescription; }
			set
			{
				CheckMaximumLength(CW_GoodsDescriptionInfo, value);
				fCW_GoodsDescription = value;
				if (!IsValidationSuspended)
				{
					ValidateCW_GoodsDescription();
				}
				CW_GoodsDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CW_GoodsDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CW_GoodsDescription); }
		}

		#endregion

		#region MarksAndNumbers

		[MaxLength(100)]
		public ZString CW_MarksAndNumbers
		{
			get { return fCW_MarksAndNumbers; }
			set
			{
				CheckMaximumLength(CW_MarksAndNumbersInfo, value);
				fCW_MarksAndNumbers = value;
				if (!IsValidationSuspended)
				{
					ValidateCW_MarksAndNumbers();
				}
				CW_MarksAndNumbersInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CW_MarksAndNumbersInfo
		{
			get { return GetZPropertyInfo(Schema.CW_MarksAndNumbers); }
		}

		#endregion

		#region Consignor

		[RelatedBusinessObject("Consignor")]
		[List("Consignors")]
		public ZGuid CW_OH_Consignor
		{
			get { return fCW_OH_Consignor; }
			set
			{
				fCW_OH_Consignor = value;
				OrganisationSet = value != ZGuid.Empty;
				CW_OH_ConsignorInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CW_OH_ConsignorInfo
		{
			get { return GetZPropertyInfo(Schema.CW_OH_Consignor); }
		}

		public OrgHeader Consignor
		{
			get
			{
				return Factory.Load<OrgHeader>(CW_OH_Consignor);
			}
		}

		#endregion

		#region Consignee

		[RelatedBusinessObject("Consignee")]
		[List("Consignees")]
		public ZGuid CW_OH_Consignee
		{
			get { return fCW_OH_Consignee; }
			set
			{
				fCW_OH_Consignee = value;
				OrganisationSet = value != ZGuid.Empty;
				CW_OH_ConsigneeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CW_OH_ConsigneeInfo
		{
			get { return GetZPropertyInfo(Schema.CW_OH_Consignee); }
		}

		public OrgHeader Consignee
		{
			get
			{
				var result = Factory.Load<OrgHeader>(CW_OH_Consignee);
				return result;
			}
		}

		#endregion

		#region CW_CreateAnotherShipment

		public ZBool CW_CreateAnotherShipment
		{
			get { return fCW_CreateAnotherShipment; }
			set
			{
				fCW_CreateAnotherShipment = value;
				CW_CreateAnotherShipmentInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CW_CreateAnotherShipmentInfo
		{
			get { return GetZPropertyInfo(Schema.CW_CreateAnotherShipment); }
		}

		#endregion

		#region CW_EnterConsignor

		public ZBool CW_EnterConsignor
		{
			get { return fCW_EnterConsignor; }
			set
			{
				fCW_EnterConsignor = value;
				CW_EnterConsignorInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CW_EnterConsignorInfo
		{
			get { return GetZPropertyInfo(Schema.CW_EnterConsignor); }
		}

		#endregion

		#region Temp Org Fields

		[MaxLength(50)]
		public ZString CW_TempOrgName
		{
			get { return fCW_TempOrgName; }
			set
			{
				CheckMaximumLength(CW_TempOrgNameInfo, value);
				fCW_TempOrgName = value;
				SetTempOrg();
				if (!IsValidationSuspended)
				{
					ValidateCW_TempOrgName();
				}
				CW_TempOrgNameInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CW_TempOrgNameInfo
		{
			get { return GetZPropertyInfo(Schema.CW_TempOrgName); }
		}

		[MaxLength(50)]
		public ZString CW_TempOrgAddress1
		{
			get { return fCW_TempOrgAddress1; }
			set
			{
				CheckMaximumLength(CW_TempOrgAddress1Info, value);
				fCW_TempOrgAddress1 = value;
				SetTempOrg();
				if (!IsValidationSuspended)
				{
					ValidateCW_TempOrgAddress1();
				}
				CW_TempOrgAddress1Info.RefreshBinding();
			}
		}
		public ZPropertyInfo CW_TempOrgAddress1Info
		{
			get { return GetZPropertyInfo(Schema.CW_TempOrgAddress1); }
		}

		[MaxLength(50)]
		public ZString CW_TempOrgAddress2
		{
			get { return fCW_TempOrgAddress2; }
			set
			{
				CheckMaximumLength(CW_TempOrgAddress2Info, value);
				fCW_TempOrgAddress2 = value;
				SetTempOrg();
				if (!IsValidationSuspended)
				{
					ValidateCW_TempOrgAddress2();
				}
				CW_TempOrgAddress2Info.RefreshBinding();
			}
		}
		public ZPropertyInfo CW_TempOrgAddress2Info
		{
			get { return GetZPropertyInfo(Schema.CW_TempOrgAddress2); }
		}

		[MaxLength(25)]
		public ZString CW_TempOrgCity
		{
			get { return fCW_TempOrgCity; }
			set
			{
				CheckMaximumLength(CW_TempOrgCityInfo, value);
				fCW_TempOrgCity = value;
				SetTempOrg();
				if (!IsValidationSuspended)
				{
					ValidateCW_TempOrgCity();
				}
				CW_TempOrgCityInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CW_TempOrgCityInfo
		{
			get { return GetZPropertyInfo(Schema.CW_TempOrgCity); }
		}

		[MaxLength(25)]
		public ZString CW_TempOrgState
		{
			get { return fCW_TempOrgState; }
			set
			{
				CheckMaximumLength(CW_TempOrgStateInfo, value);
				fCW_TempOrgState = value;
				SetTempOrg();
				if (!IsValidationSuspended)
				{
					ValidateCW_TempOrgState();
				}
				CW_TempOrgStateInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CW_TempOrgStateInfo
		{
			get { return GetZPropertyInfo(Schema.CW_TempOrgState); }
		}

		[MaxLength(10)]
		public ZString CW_TempOrgPostcode
		{
			get { return fCW_TempOrgPostcode; }
			set
			{
				CheckMaximumLength(CW_TempOrgPostcodeInfo, value);
				fCW_TempOrgPostcode = value;
				SetTempOrg();
				if (!IsValidationSuspended)
				{
					ValidateCW_TempOrgPostcode();
				}
				CW_TempOrgPostcodeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CW_TempOrgPostcodeInfo
		{
			get { return GetZPropertyInfo(Schema.CW_TempOrgPostcode); }
		}

		[MaxLength(5)]
		[List("TempOrgUNLOCOs")]
		public ZString CW_TempOrgUNLOCO
		{
			get { return fCW_TempOrgUNLOCO; }
			set
			{
				CheckMaximumLength(CW_TempOrgUNLOCOInfo, value);
				fCW_TempOrgUNLOCO = value;
				SetTempOrg();
				if (!IsValidationSuspended)
				{
					ValidateCW_TempOrgUNLOCO();
				}
				CW_TempOrgUNLOCOInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CW_TempOrgUNLOCOInfo
		{
			get { return GetZPropertyInfo(Schema.CW_TempOrgUNLOCO); }
		}

		protected void SetTempOrg()
		{
			if (fTempOrg != null)
			{
				fTempOrg.OH_FullName = CW_TempOrgName;
				fTempOrg.MainAddress.OA_Address1 = CW_TempOrgAddress1;
				fTempOrg.MainAddress.OA_Address2 = CW_TempOrgAddress2;
				fTempOrg.MainAddress.OA_City = CW_TempOrgCity;
				fTempOrg.MainAddress.OA_PostCode = CW_TempOrgPostcode;
				fTempOrg.MainAddress.OA_State = CW_TempOrgState;
				fTempOrg.OH_RL_NKClosestPort = CW_TempOrgUNLOCO;
				CheckForSimilarOrgs();
			}
		}

		OrgHeader fTempOrg;
		protected OrgHeader TempOrg
		{
			get
			{
				if (fTempOrg == null)
				{
					fTempOrg = Factory.New<OrgHeader>();
					fTempOrg.OH_FullName = CW_TempOrgName;
					fTempOrg.MainAddress.OA_Address1 = CW_TempOrgAddress1;
					fTempOrg.MainAddress.OA_Address2 = CW_TempOrgAddress2;
					fTempOrg.MainAddress.OA_City = CW_TempOrgCity;
					fTempOrg.MainAddress.OA_PostCode = CW_TempOrgPostcode;
					fTempOrg.MainAddress.OA_State = CW_TempOrgState;
					fTempOrg.OH_RL_NKClosestPort = CW_TempOrgUNLOCO;
				}
				return fTempOrg;
			}
		}

		public OrgPatternMatchCollection SimilarOrganisations
		{
			get
			{
				return TempOrg.SimilarOrgMatches;
			}
		}

		public ZBool CW_TempOrgCreateNew
		{
			get { return fCW_TempOrgCreateNew; }
			set
			{
				fCW_TempOrgCreateNew = value;
				CW_TempOrgCreateNewInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CW_TempOrgCreateNewInfo
		{
			get { return GetZPropertyInfo(Schema.CW_TempOrgCreateNew); }
		}

		public ZBool CW_TempOrgMarkTemporary
		{
			get { return fCW_TempOrgMarkTemporary; }
			set
			{
				fCW_TempOrgMarkTemporary = value;
				CW_TempOrgMarkTemporaryInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CW_TempOrgMarkTemporaryInfo
		{
			get { return GetZPropertyInfo(Schema.CW_TempOrgMarkTemporary); }
		}

		[MaxLength(512)]
		public ZString CW_TempOrgCreateActionExplaination
		{
			get { return fCW_TempOrgCreateActionExplaination; }
			set
			{
				CheckMaximumLength(CW_TempOrgCreateActionExplainationInfo, value);
				fCW_TempOrgCreateActionExplaination = value;
				CW_TempOrgCreateActionExplainationInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CW_TempOrgCreateActionExplainationInfo
		{
			get { return GetZPropertyInfo(Schema.CW_TempOrgCreateActionExplaination); }
		}

		#endregion

		#region CoLoad Shipments
		public PackUnpackShipmentList CoLoadShipments
		{
			get
			{
				if (fCoLoadShipments == null)
				{
					fCoLoadShipments = new PackUnpackShipmentList(Factory);
					fCoLoadShipments.SetReadOnlyIncludingChildren(true);
				}
				return fCoLoadShipments;
			}
		}
		#endregion

		#endregion

		#region Lists

		public OrgHeaderCollection Forwarders
		{
			get
			{
				return new ForwarderCollection(Factory);
			}
		}

		public OrgHeaderCollection Consignees
		{
			get
			{
				return new ConsigneeCollection(Factory);
			}
		}

		public OrgHeaderCollection Consignors
		{
			get { return new ConsignorCollection(Factory); }
		}

		public RefUNLOCOCollection TempOrgUNLOCOs
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		public CodeDescriptionPairList CW_WeightUQ_List
		{
			get
			{
				return new CodeDescriptionPairList(OLookUpEditType.Weight);
			}
		}

		public CodeDescriptionPairList CW_VolumeUQ_List
		{
			get
			{
				return new CodeDescriptionPairList(OLookUpEditType.Volume);
			}
		}

		public CodeDescriptionPairList CW_PackagesUQ_List
		{
			get
			{
				return new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits();
			}
		}

		#endregion

		#region Wizard Steps

		public void OnStepping(WizardSteppingEventArgs e)
		{
			switch (e.CurrentStep)
			{
				case CoLoadWizardSteps.WelcomeStep:
					ValidateWelcomeStep();
					break;
				case CoLoadWizardSteps.UltimateDetails:
					ValidateUltimateDetails(e);
					break;
				case CoLoadWizardSteps.ConsignorDetails:
					ValidateConsignorDetails(e);
					break;
				case CoLoadWizardSteps.AddConsignor:
					ValidateAddConsignor(e);
					break;
				case CoLoadWizardSteps.ConsigneeDetails:
					ValidateConsigneeDetails(e);
					break;
				case CoLoadWizardSteps.AddConsignee:
					ValidateAddConsignee(e);
					break;
				case CoLoadWizardSteps.AddColoadShipment:
					ValidateAddColoadShipment(e);
					break;
				case CoLoadWizardSteps.AnotherShipment:
					ValidateAnotherShipment(e);
					break;
				case CoLoadWizardSteps.FinishWizard:
					ValidateFinishWizard();
					break;
			}
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get
			{
				IDefaultNumberOfDecimalsSupporter parentShipment = fParentShipment;
				return (parentShipment != null) ? parentShipment.TransportMode : ZString.Empty;
			}
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.CW_Weight:
					unitOfMeasure = CW_WeightUQ;
					break;

				case Schema.CW_Volume:
					unitOfMeasure = CW_VolumeUQ;
					break;

				default:
					break;
			}

			return unitOfMeasure;
		}

		ZDecimal IDefaultNumberOfDecimalsSupporter.GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(this, property, value);
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			// no change in transport mode expected during the lifecycle of this bizObj
		}

		#endregion

		#region Implementation

		readonly CFSShipment fParentShipment;
		readonly ZGuid fCW_OH_CoLoadForwarder;
		ZGuid fCW_OH_Consignor;
		ZGuid fCW_OH_Consignee;
		ZString fCW_HouseBill;
		ZInt fCW_NumberOfPackages;
		ZString fCW_NumberOfPackagesUQ;
		ZDecimal fCW_Weight;
		ZString fCW_WeightUQ;
		ZDecimal fCW_Volume;
		ZString fCW_VolumeUQ;
		ZString fCW_GoodsDescription;
		ZString fCW_MarksAndNumbers;

		ZString fCW_TempOrgName;
		ZString fCW_TempOrgAddress1;
		ZString fCW_TempOrgAddress2;
		ZString fCW_TempOrgCity;
		ZString fCW_TempOrgState;
		ZString fCW_TempOrgPostcode;
		ZString fCW_TempOrgUNLOCO;

		ZBool fCW_CreateAnotherShipment;
		ZBool fCW_EnterConsignor;
		ZString fCW_TempOrgCreateActionExplaination;

		ZBool fCW_TempOrgCreateNew;
		ZBool fCW_TempOrgMarkTemporary;

		static string CreateNewConsigneeExplaination
		{
			get { return Res.GetString("a38bd968-3e2c-49f3-8ddd-fc69ec5825d2", "The following consignee will be created.  To make this a permanent record, untick the \"Mark Temporary\" check box."); }
		}
		static string UseExistingConsigneeExplaination
		{
			get { return Res.GetString("60d824c2-281a-4008-ae96-adad2012904f", "The following consignee already exists in the system and will be used for the consignee."); }
		}
		static string CreateNewConsignorExplaination
		{
			get { return Res.GetString("698f187b-6087-4a93-ac0d-b4844f3777aa", "The following consignor will be created.  To make this a permanent record, untick the \"Mark Temporary\" check box."); }
		}
		static string UseExistingConsignorExplaination
		{
			get { return Res.GetString("e47a248e-5f25-4792-9241-b72b19a59bf8", "The following consignor already exists in the system and will be used for the consignee."); }
		}

		PackUnpackShipmentList fCoLoadShipments;

		protected void CreateConsignor()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "NAME";
		}

		protected void ValidateWelcomeStep()
		{
			ResetUltimateDetails();
			ResetTempOrg();
		}

		protected void ValidateUltimateDetails(WizardSteppingEventArgs e)
		{
			if (e.NextStep > e.CurrentStep)
			{
				RunUltimateDetailsValidation();
				if (!HasUltimateDetailsErrors())
				{
					if (CW_OH_Consignor == ZGuid.Empty && fParentShipment.Consignor != null && !fParentShipment.Consignor.OH_IsForwarder)
					{
						CW_OH_Consignor = fParentShipment.ConsignorPK;
					}
					if (CW_EnterConsignor)
					{
						e.NextStep = CoLoadWizardSteps.ConsignorDetails;
						OrganisationSet = (CW_OH_Consignor != ZGuid.Empty) && (CW_OH_Consignor != fParentShipment.ConsignorPK);
						SetTempOrgToConsignor();
					}
					else
					{
						e.NextStep = CoLoadWizardSteps.ConsigneeDetails;
						OrganisationSet = CW_OH_Consignee != ZGuid.Empty;
						SetTempOrgToConsignee();
					}
				}
				else
				{
					e.NextStep = e.CurrentStep;
				}
			}
		}

		protected void ValidateConsignorDetails(WizardSteppingEventArgs e)
		{
			if (e.NextStep > e.CurrentStep)
			{
				RunEnterConsignorValidation();
				if (!HasEnterConsignorErrors())
				{
					if (OrganisationSet)
					{
						CW_TempOrgCreateNew = !Consignor.IsInDatabase;
						CW_TempOrgMarkTemporary = !Consignor.IsInDatabase;
						CW_TempOrgCreateActionExplaination = Consignor.IsInDatabase ? CreateNewConsignorExplaination : UseExistingConsignorExplaination;
						if (fTempOrg != null && !fTempOrg.IsDeleted && fTempOrg != Consignor)
						{
							ResetTempOrg();
						}
					}
					else
					{
						CW_OH_Consignor = TempOrg.PK;
						CW_TempOrgCreateNew = !TempOrg.IsInDatabase;
						CW_TempOrgMarkTemporary = !TempOrg.IsInDatabase;
						CW_TempOrgCreateActionExplaination = CreateNewConsignorExplaination;
					}
					Consignor.OH_IsTempAccount = CW_TempOrgMarkTemporary;
					ResetTempOrg();
				}
				else
				{
					e.NextStep = e.CurrentStep;
				}
			}
			else
			{
				ResetTempOrg();
				e.NextStep = CoLoadWizardSteps.UltimateDetails;
			}
		}

		protected void ValidateAddConsignor(WizardSteppingEventArgs e)
		{
			if (e.NextStep > e.CurrentStep)
			{
				OrganisationSet = CW_OH_Consignee != ZGuid.Empty;
				SetTempOrgToConsignee();
			}
			else
			{
				OrganisationSet = (CW_OH_Consignor != ZGuid.Empty) && (CW_OH_Consignor != fParentShipment.ConsignorPK);
				SetTempOrgToConsignor();
			}
		}

		protected void ValidateConsigneeDetails(WizardSteppingEventArgs e)
		{
			if (e.NextStep > e.CurrentStep)
			{
				RunEnterConsigneeValidation();
				if (!HasEnterConsigneeErrors())
				{
					if (OrganisationSet && Consignee != null)
					{
						CW_TempOrgCreateNew = !Consignee.IsInDatabase;
						CW_TempOrgMarkTemporary = !Consignee.IsInDatabase;
						CW_TempOrgCreateActionExplaination = Consignee.IsInDatabase ? CreateNewConsigneeExplaination : UseExistingConsigneeExplaination;
						if (fTempOrg != null && !fTempOrg.IsDeleted && fTempOrg != Consignee)
						{
							ResetTempOrg();
						}
					}
					else
					{
						CW_OH_Consignee = TempOrg.PK;
						CW_TempOrgCreateNew = true;
						CW_TempOrgMarkTemporary = true;
						CW_TempOrgCreateActionExplaination = CreateNewConsigneeExplaination;
					}
					Consignee.OH_IsTempAccount = CW_TempOrgMarkTemporary;
					ResetTempOrg();
				}
				else
				{
					e.NextStep = e.CurrentStep;
				}
			}
			else
			{
				ResetTempOrg();
				if (CW_EnterConsignor)
				{
					e.NextStep = CoLoadWizardSteps.AddConsignor;
				}
				else
				{
					e.NextStep = CoLoadWizardSteps.UltimateDetails;
				}
			}
		}

		protected void ValidateAddConsignee(WizardSteppingEventArgs e)
		{
			if (e.NextStep > e.CurrentStep)
			{
				this.ReadOnly = true;
			}
			else
			{
				OrganisationSet = CW_OH_Consignee != ZGuid.Empty;
				SetTempOrgToConsignee();
			}
		}

		protected void ValidateAddColoadShipment(WizardSteppingEventArgs e)
		{
			if (e.NextStep > e.CurrentStep)
			{
				RunAddColoadShipmentValidation();
				if (!HasAddColoadShipmentErrors())
				{
					if (ShipmentAlreadyExists())
					{
						AttachCoLoadShipmentToParent();
					}
					else
					{
						CreateShipmentFromCurrentValues();
					}
					this.ReadOnly = false;
					CW_CreateAnotherShipment = (RemainingPackages != 0);
				}
				else
				{
					e.NextStep = e.CurrentStep;
				}
			}
			else
			{
				this.ReadOnly = false;
			}
		}

		protected void ValidateAnotherShipment(WizardSteppingEventArgs e)
		{
			if (CW_CreateAnotherShipment)
			{
				e.NextStep = CoLoadWizardSteps.UltimateDetails;
				ResetUltimateDetails();
				ResetTempOrg();
			}
		}

		protected ZInt RemainingPackages
		{
			get
			{
				ZInt result = fParentShipment.JS_OuterPacks;
				foreach (PackUnpackShipment shipment in CoLoadShipments)
				{
					result -= shipment.JS_OuterPacks;
				}
				if (result < 0)
				{
					result = 0;
				}

				return result;
			}
		}

		protected ZDecimal RemainingWeight
		{
			get
			{
				ZDecimal result = fParentShipment.JS_ActualWeight;
				if (result == 0.0m)
				{
					result = fParentShipment.JS_DocumentedWeight;
				}
				foreach (PackUnpackShipment shipment in CoLoadShipments)
				{
					result -= shipment.JS_ActualWeight;
				}
				return result;
			}
		}

		protected ZString WeightUnitOfQuantity
		{
			get
			{
				ZString result = fParentShipment.JS_UnitOfWeight;
				return result;
			}
		}

		protected ZDecimal RemainingVolume
		{
			get
			{
				ZDecimal result = fParentShipment.JS_ActualVolume;
				if (result == 0.0m)
				{
					result = fParentShipment.JS_DocumentedVolume;
				}
				foreach (PackUnpackShipment shipment in CoLoadShipments)
				{
					result -= shipment.JS_ActualVolume;
				}
				return result;
			}
		}

		protected ZString VolumeUnitOfQuantity
		{
			get
			{
				ZString result = fParentShipment.JS_UnitOfVolume;
				return result;
			}
		}

		protected void ValidateFinishWizard()
		{
		}

		public void OnStepped()
		{
		}

		void ResetUltimateDetails()
		{
			CW_HouseBill = "";
			CW_NumberOfPackages = RemainingPackages;
			CW_NumberOfPackagesUQ = fParentShipment.JS_F3_NKPackType;
			CW_Weight = RemainingWeight;
			CW_WeightUQ = WeightUnitOfQuantity;
			CW_Volume = RemainingVolume;
			CW_VolumeUQ = VolumeUnitOfQuantity;
			CW_GoodsDescription = fParentShipment.JS_GoodsDescription;
			CW_MarksAndNumbers = fParentShipment.JS_MarksAndNumbers.Left(CW_MarksAndNumbersInfo.MaxLength);
			CW_OH_Consignee = ZGuid.Empty;
			if (CW_EnterConsignor)
			{
				CW_OH_Consignor = ZGuid.Empty;
			}
		}

		void ResetTempOrg()
		{
			if (fTempOrg != null)
			{
				fTempOrg.SimilarOrgMatches.RemoveAll();
				OnElementReset();

				if (fTempOrg.PK != CW_OH_Consignee && fTempOrg.PK != CW_OH_Consignor)
				{
					fTempOrg.Delete();
				}
			}
			fTempOrg = null;
			fCW_TempOrgName = "";
			fCW_TempOrgAddress1 = "";
			fCW_TempOrgAddress2 = "";
			fCW_TempOrgCity = "";
			fCW_TempOrgState = "";
			fCW_TempOrgPostcode = "";
			fCW_TempOrgUNLOCO = fParentShipment.JS_RL_NKDestination;
		}

		bool IsTempOrgDefault()
		{
			return (fCW_TempOrgName == "" && fCW_TempOrgAddress1 == "" && fCW_TempOrgAddress2 == "" && fCW_TempOrgCity == "" &&
			fCW_TempOrgState == "" && fCW_TempOrgPostcode == "");
		}

		bool HasUltimateDetailsErrors()
		{
			return CW_HouseBillInfo.HasErrors() ||
				CW_NumberOfPackagesInfo.HasErrors() ||
				CW_WeightInfo.HasErrors() ||
				CW_WeightUQInfo.HasErrors() ||
				CW_VolumeInfo.HasErrors() ||
				CW_VolumeUQInfo.HasErrors() ||
				CW_GoodsDescriptionInfo.HasErrors() ||
				CW_MarksAndNumbersInfo.HasErrors();
		}

		void RunUltimateDetailsValidation()
		{
			ValidateCW_HouseBill();
			ValidateCW_NumberOfPackages();
			ValidateCW_NumberOfPackagesUQ();
			ValidateCW_Weight();
			ValidateCW_WeightUQ();
			ValidateCW_Volume();
			ValidateCW_VolumeUQ();
			ValidateCW_GoodsDescription();
			ValidateCW_MarksAndNumbers();
		}

		bool HasEnterConsignorErrors()
		{
			return HasTempOrgErrors();
		}

		void RunEnterConsignorValidation()
		{
			RunTempOrgValidation();
		}

		bool HasEnterConsigneeErrors()
		{
			return HasTempOrgErrors();
		}

		void RunEnterConsigneeValidation()
		{
			RunTempOrgValidation();
		}

		void RunTempOrgValidation()
		{
			ValidateCW_TempOrgAddress1();
			ValidateCW_TempOrgAddress2();
			ValidateCW_TempOrgCity();
			ValidateCW_TempOrgName();
			ValidateCW_TempOrgPostcode();
			ValidateCW_TempOrgState();
			ValidateCW_TempOrgUNLOCO();
		}

		bool HasTempOrgErrors()
		{
			return CW_TempOrgAddress1Info.HasErrors()
				|| CW_TempOrgAddress2Info.HasErrors()
				|| CW_TempOrgCityInfo.HasErrors()
				|| CW_TempOrgNameInfo.HasErrors()
				|| CW_TempOrgPostcodeInfo.HasErrors()
				|| CW_TempOrgStateInfo.HasErrors()
				|| CW_TempOrgUNLOCOInfo.HasErrors();
		}

		bool HasAddColoadShipmentErrors()
		{
			return HasUltimateDetailsErrors()
				|| CW_OH_ConsigneeInfo.HasErrors()
				|| CW_OH_ConsignorInfo.HasErrors();
		}

		void RunAddColoadShipmentValidation()
		{
			RunUltimateDetailsValidation();
			ValidateCW_OH_Consignee();
			ValidateCW_OH_Consignor();
		}

		void CreateShipmentFromCurrentValues()
		{
			PackUnpackShipment newShipment = CoLoadShipments.AddNew();
			newShipment.JS_HouseBill = CW_HouseBill;
			CopyValuesToShipment(newShipment);
		}

		protected void CopyValuesToShipment(PackUnpackShipment shipment)
		{
			shipment.ConsigneePK = CW_OH_Consignee;
			shipment.ConsignorPK = CW_OH_Consignor;
			shipment.JS_OH_HandledOnBehalfOfForwarder = CW_OH_CoLoadForwarder;
			shipment.JS_GoodsDescription = CW_GoodsDescription;
			shipment.JS_MarksAndNumbers = CW_MarksAndNumbers;
			shipment.JS_JS_ColoadMasterShipment = fParentShipment.PK;
			shipment.JS_OuterPacks = CW_NumberOfPackages;
			shipment.JS_F3_NKPackType = CW_NumberOfPackagesUQ;
			PackLine newPackLine = null;
			if (shipment.OuterPackLines.Count == 0)
			{
				newPackLine = shipment.OuterPackLines.AddNew();
			}
			else
			{
				newPackLine = shipment.OuterPackLines[0];
			}

			newPackLine.JL_PackageCount = CW_NumberOfPackages;
			newPackLine.JL_F3_NKPackType = CW_NumberOfPackagesUQ;
			if (fParentShipment.OuterPackLines.Count > 0 && fParentShipment.OuterPackLines[0].Containers.Count > 0)
			{
				newPackLine.SetContainer(fParentShipment.OuterPackLines[0].Containers[0].PK);
				newPackLine.JL_ActualVolume = CW_Volume;
				newPackLine.JL_ActualVolumeUQ = CW_VolumeUQ;
				newPackLine.JL_ActualWeight = CW_Weight;
				newPackLine.JL_ActualWeightUQ = CW_WeightUQ;
			}
			shipment.JS_ActualWeight = CW_Weight;
			shipment.JS_UnitOfWeight = CW_WeightUQ;
			shipment.JS_ActualVolume = CW_Volume;
			shipment.JS_UnitOfVolume = CW_VolumeUQ;
			shipment.JS_RL_NKDestination = fParentShipment.JS_RL_NKDestination;
			shipment.JS_RL_NKOrigin = fParentShipment.JS_RL_NKOrigin;
		}

		protected void ValidateCW_HouseBill()
		{
			CW_HouseBillInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CW_HouseBillInfo);

			if (HousebillsMatch(fParentShipment.JS_HouseBill, CW_HouseBill))
			{
				CW_HouseBillInfo.AddError(Res.GetString("9b03ba62-871b-4d8f-8ca3-15a22b7ee502", "The House bill '{0}' belongs to the selected co-load master shipment.", CW_HouseBill));
			}
			else if (fParentShipment.ArrivalConsol != null)
			{
				foreach (CommonShipment shipment in fParentShipment.ArrivalConsol.Shipments)
				{
					if (shipment.IsMasterShipmentRepresentingAllChildShipments && HousebillsMatch(shipment.JS_HouseBill, CW_HouseBill))
					{
						CW_HouseBillInfo.AddError(Res.GetString("7fe6d904-0051-4630-ad25-e5c0c445965e", "The House bill '{0}' belongs to another co-load master shipment.", CW_HouseBill));
						break;
					}
				}
			}
		}

		protected void ValidateCW_NumberOfPackages()
		{
			CW_NumberOfPackagesInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CW_NumberOfPackagesInfo);
		}

		protected void ValidateCW_NumberOfPackagesUQ()
		{
			CW_NumberOfPackagesUQInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(CW_NumberOfPackagesUQInfo, CW_PackagesUQ_List);
		}

		protected void ValidateCW_Weight()
		{
			CW_WeightInfo.ClearAllNotifications();
		}

		protected void ValidateCW_WeightUQ()
		{
			CW_WeightUQInfo.ClearAllNotifications();
		}

		protected void ValidateCW_Volume()
		{
			CW_VolumeInfo.ClearAllNotifications();
		}

		protected void ValidateCW_VolumeUQ()
		{
			CW_VolumeUQInfo.ClearAllNotifications();
		}

		protected void ValidateCW_GoodsDescription()
		{
			CW_GoodsDescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CW_GoodsDescriptionInfo);
		}

		protected void ValidateCW_MarksAndNumbers()
		{
			CW_MarksAndNumbersInfo.ClearAllNotifications();
		}

		protected void ValidateCW_OH_Consignee()
		{
			CW_OH_ConsigneeInfo.ClearAllNotifications();
		}

		protected void ValidateCW_OH_Consignor()
		{
			CW_OH_ConsignorInfo.ClearAllNotifications();
		}

		protected bool OrganisationSet;

		protected void ValidateCW_TempOrgAddress1()
		{
			CW_TempOrgAddress1Info.ClearAllNotifications();
			if (!OrganisationSet)
			{
				MandatoryValidation.CheckEntered(CW_TempOrgAddress1Info);
			}
		}
		protected void ValidateCW_TempOrgAddress2()
		{
			CW_TempOrgAddress2Info.ClearAllNotifications();
		}
		protected void ValidateCW_TempOrgCity()
		{
			CW_TempOrgCityInfo.ClearAllNotifications();
			if (!OrganisationSet)
			{
				MandatoryValidation.WarnIfNotEntered(CW_TempOrgCityInfo);
			}
		}
		protected void ValidateCW_TempOrgName()
		{
			CW_TempOrgNameInfo.ClearAllNotifications();
			if (!OrganisationSet)
			{
				MandatoryValidation.CheckEntered(CW_TempOrgNameInfo);
			}
		}
		protected void ValidateCW_TempOrgPostcode()
		{
			CW_TempOrgPostcodeInfo.ClearAllNotifications();
			if (!OrganisationSet)
			{
				MandatoryValidation.WarnIfNotEntered(CW_TempOrgPostcodeInfo);
			}
		}
		protected void ValidateCW_TempOrgState()
		{
			CW_TempOrgStateInfo.ClearAllNotifications();
			if (!OrganisationSet)
			{
				MandatoryValidation.WarnIfNotEntered(CW_TempOrgStateInfo);
			}
		}
		protected void ValidateCW_TempOrgUNLOCO()
		{
			CW_TempOrgUNLOCOInfo.ClearAllNotifications();
			if (!OrganisationSet)
			{
				MandatoryValidation.CheckEntered(CW_TempOrgUNLOCOInfo);
				ListValidation.ErrorIfInvalidCode(CW_TempOrgUNLOCOInfo, new RefUNLOCOCollection(Factory));
			}
		}

		void SetTempOrgToConsignor()
		{
			ResetTempOrg();
			if (!CW_OH_Consignor.IsEmpty)
			{
				fCW_TempOrgAddress1 = Consignor.MainAddress.OA_Address1;
				fCW_TempOrgAddress2 = Consignor.MainAddress.OA_Address2;
				fCW_TempOrgCity = Consignor.MainAddress.OA_City;
				fCW_TempOrgName = Consignor.OH_FullNameTruncated;
				fCW_TempOrgPostcode = Consignor.MainAddress.OA_PostCode;
				fCW_TempOrgState = Consignor.MainAddress.OA_State;
				fCW_TempOrgUNLOCO = Consignor.OH_RL_NKClosestPort;
			}
			else
			{
				fCW_TempOrgUNLOCO = fParentShipment.JS_RL_NKOrigin;
			}
			CheckForSimilarOrgs();
		}

		void SetTempOrgToConsignee()
		{
			ResetTempOrg();
			if (OrganisationSet && Consignee != null)
			{
				fCW_TempOrgAddress1 = Consignee.MainAddress.OA_Address1;
				fCW_TempOrgAddress2 = Consignee.MainAddress.OA_Address2;
				fCW_TempOrgCity = Consignee.MainAddress.OA_City;
				fCW_TempOrgName = Consignee.OH_FullNameTruncated;
				fCW_TempOrgPostcode = Consignee.MainAddress.OA_PostCode;
				fCW_TempOrgState = Consignee.MainAddress.OA_State;
				fCW_TempOrgUNLOCO = Consignee.OH_RL_NKClosestPort;
			}
			else
			{
				fCW_TempOrgUNLOCO = fParentShipment.JS_RL_NKDestination;
			}
			CheckForSimilarOrgs();
		}

		bool ShipmentAlreadyExists()
		{
			if (fParentShipment.ArrivalConsol != null)
			{
				foreach (CommonShipment shipment in fParentShipment.ArrivalConsol.Shipments)
				{
					if (HousebillsMatch(shipment.JS_HouseBill, CW_HouseBill))
					{
						return true;
					}
				}
			}
			return false;
		}

		readonly string AllowableHouseBillChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		bool HousebillsMatch(ZString existingHouseBill, ZString newHouseBill)
		{
			return existingHouseBill.KeepChars(AllowableHouseBillChars) == newHouseBill.ToUpper().KeepChars(AllowableHouseBillChars);
		}

		void AttachCoLoadShipmentToParent()
		{
			foreach (PackUnpackShipment shipment in fParentShipment.ArrivalConsol.Shipments)
			{
				if (HousebillsMatch(shipment.JS_HouseBill, CW_HouseBill))
				{
					CopyValuesToShipment(shipment);
					CoLoadShipments.Add(shipment);
					break;
				}
			}
		}

		void CheckForSimilarOrgs()
		{
			// Regen the pattern matches for this org.
			// Usually this would happen automatically when saving an org, or loading an existing org,
			// but this "TempOrg" is never saved or loaded. Hence the manual call to the regenerate.
			if (TempOrg.PatternMatchRequiresRegen)
			{
				TempOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(TempOrg);
			}

			if (!IsTempOrgDefault())
			{
				TempOrg.SimilarOrgFinder.FindSimilarOrganisations();

				if (TempOrg.SimilarOrgFinder.HasTooManyMatches)
				{
					//Label: There are too many matches, please enter more information.
				}
			}
			else if (TempOrg.SimilarOrgMatches.Count > 0)
			{
				TempOrg.SimilarOrgMatches.RemoveAll();
			}
			OnElementReset();
		}

		#endregion
	}
}
