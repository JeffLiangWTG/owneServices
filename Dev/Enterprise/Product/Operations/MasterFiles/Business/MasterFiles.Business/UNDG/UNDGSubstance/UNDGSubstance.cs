using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(UNDGSubstance.Schema.DG_Code), DescriptionProperty(UNDGSubstance.Schema.DG_PSN)]
	[ProvideMetaDataProperty("ReadOnly", MetaDataTypes.ReadOnly)]
	public class UNDGSubstance : AutoUNDGSubstance, IUNDGSubstance, IDGSubstance
	{
		public UNDGSubstance(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values / Loading

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			DG_IsSystem = false;
			SetDefaultDetailsLanguage();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			SetDefaultDetailsLanguage();
		}

		#endregion

		#region Properties

		#region DG_Class

		[List("Lookups.DGClassList")]
		public override ZString DG_Class
		{
			get { return base.DG_Class; }
			set { base.DG_Class = value; }
		}

		#endregion

		#region DG_IsSystem

		[ReadOnly(true)]
		public override ZBool DG_IsSystem
		{
			get { return base.DG_IsSystem; }
			set { base.DG_IsSystem = value; }
		}

		#endregion

		#region Details Language

		[List("Lookups.Languages")]
		[MaxLength(7)]
		public ZString DetailsLanguage
		{
			get { return detailsLanguage; }
			set
			{
				if (detailsLanguage != value)
				{
					CheckMaximumLength(DetailsLanguageInfo, value); // you are likely missing this line!
					detailsLanguage = value;
					DetailsLanguageInfo.RefreshBinding();
				}
			}
		}

		ZString detailsLanguage;

		public ZPropertyInfo DetailsLanguageInfo
		{
			get { return GetZPropertyInfo(nameof(DetailsLanguage)); }
		}

		void SetDefaultDetailsLanguage()
		{
			DetailsLanguage = ZString.Empty;
		}

		#endregion

		#region DG_MP

		[List("Lookups.MarinePollutantList")]
		public override ZString DG_MP
		{
			get { return base.DG_MP; }
			set { base.DG_MP = value; }
		}

		#endregion

		#region DG_State

		[List("Lookups.StateList")]
		public override ZString DG_State
		{
			get { return base.DG_State; }
			set { base.DG_State = value; }
		}

		#endregion

		#region DG_Code
		public override ZString DG_Code
		{
			get { return base.DG_Code; }
			set
			{
				base.DG_Code = value;
				base.DG_UNNO = value.SubstringSafe(0, 4);
				base.DG_Variant = value.SubstringSafe(4, 2);
			}
		}
		#endregion

		#region DG_UNNO

		public override ZString DG_UNNO
		{
			get { return base.DG_UNNO; }
			set
			{
				base.DG_UNNO = value;
				base.DG_Code = DG_UNNO + DG_Variant;
			}
		}

		#endregion

		#region DG_Variant

		public override ZString DG_Variant
		{
			get { return base.DG_Variant; }
			set
			{
				base.DG_Variant = value;
				base.DG_Code = DG_UNNO + DG_Variant;
			}
		}

		#endregion

		#region DG_Country

		[MaxLength(UNDGSubstance.Schema.DG_CountryMaxLength)]
		public override ZString DG_Country { get => base.DG_Country; set => base.DG_Country = value; }

		#endregion

		#region DG_Mode

		[MaxLength(UNDGSubstance.Schema.DG_ModeMaxLength)]
		public override ZString DG_Mode
		{
			get => base.DG_Mode;
			set => base.DG_Mode = value;
		}

		#endregion

		#region DG_ExceptedQuantityCode

		[List("Lookups.ExceptedQuantityList")]
		public override ZString DG_ExceptedQuantityCode
		{
			get { return base.DG_ExceptedQuantityCode; }
			set { base.DG_ExceptedQuantityCode = value; }
		}

		#endregion

		#region DG_PG

		[List("Lookups.PackingGroupList")]
		public override ZString DG_PG
		{
			get { return base.DG_PG; }
			set { base.DG_PG = value; }
		}

		#endregion

		#region DG_EmergencyResponseGuide

		[List("Lookups.EmergencyResponseGuideList")]
		public override ZString DG_EmergencyResponseGuide
		{
			get { return base.DG_EmergencyResponseGuide; }
			set { base.DG_EmergencyResponseGuide = value; }
		}

		#endregion

		#region DG_SpecialHandleCodes

		public ZString DG_SpecialHandlingCode1
		{
			get
			{
				var handlingCodes = base.DG_SpecialHandlingCodes;
				if (!handlingCodes.IsEmpty)
				{
					var split = handlingCodes.Split(' ');
					return split[0];
				}
				return ZString.Empty;
			}
		}

		public ZString DG_SpecialHandlingCode2
		{
			get
			{
				var handlingCodes = base.DG_SpecialHandlingCodes;
				if (!handlingCodes.IsEmpty)
				{
					var split = handlingCodes.Split(' ');
					if (split.Length > 1)
					{
						return split[1];
					}
				}
				return ZString.Empty;
			}
		}

		public ZString DG_SpecialHandlingCode3
		{
			get
			{
				var handlingCodes = base.DG_SpecialHandlingCodes;
				if (!handlingCodes.IsEmpty)
				{
					var split = handlingCodes.Split(' ');
					if (split.Length > 2)
					{
						return split[2];
					}
				}
				return ZString.Empty;
			}
		}
		#endregion

		#region Limited Quantity Types

		[List("Lookups.LimitedQuantityTypesList")]
		public override ZString DG_LQ2OrPaxMaxAmtType
		{
			get { return base.DG_LQ2OrPaxMaxAmtType; }
			set { base.DG_LQ2OrPaxMaxAmtType = value; }
		}

		[List("Lookups.LimitedQuantityTypesList")]
		public override ZString DG_LQMaxAmtType
		{
			get { return base.DG_LQMaxAmtType; }
			set { base.DG_LQMaxAmtType = value; }
		}

		[List("Lookups.LimitedQuantityTypesList")]
		public override ZString DG_CargoPackAmtType
		{
			get { return base.DG_CargoPackAmtType; }
			set { base.DG_CargoPackAmtType = value; }
		}

		#endregion

		#region Segregation Summary

		public override ZString DG_EXVector
		{
			get { return base.DG_EXVector; }
			set
			{
				base.DG_EXVector = value;
				CalcSegregation();
			}
		}

		public void CalcSegregation()
		{
			if (!DG_EXVector.IsEmpty)
			{
				for (int i = 0; i < DG_EXVector.Length; i++)
				{
					ZString classToSegeregate = SegregateAwayFromClass[i];
					if (i == 0)
					{
						classToSegeregate += "; 1.6";
					}

					if (i == DG_EXVector.Length - 1)
					{
						classToSegeregate += "; 1.2; 1.5";
					}

					if (DG_EXVector[i] == '1')
					{
						DG_AwayFrom += AddDelimitedClassToSegregate(DG_AwayFrom, classToSegeregate);
					}
					else if (DG_EXVector[i] == '2')
					{
						DG_SeperateFrom += AddDelimitedClassToSegregate(DG_SeperateFrom, classToSegeregate);
					}
					else if (DG_EXVector[i] == '3')
					{
						DG_SeperateCompartmentFrom += AddDelimitedClassToSegregate(DG_SeperateCompartmentFrom, classToSegeregate);
					}
					else if (DG_EXVector[i] == '4')
					{
						DG_SeperatedLongitudinallyFrom += AddDelimitedClassToSegregate(DG_SeperatedLongitudinallyFrom, classToSegeregate);
					}
					else if (DG_EXVector[i] == '9')
					{
						DG_NotOnSameShipAs += AddDelimitedClassToSegregate(DG_NotOnSameShipAs, classToSegeregate);
					}
				}
			}
		}

		ZString AddDelimitedClassToSegregate(ZString existingValue, ZString classToSegeregate)
		{
			return (existingValue.IsEmpty ? "" : " ") + classToSegeregate + ";";
		}

		readonly string[] SegregateAwayFromClass = { "1.3", "1.4", "2.1", "2.2", "2.3", "3", "4.1", "4.2", "4.3", "5.1", "5.2", "6.1", "6.2", "7", "8", "9", "1.1" };

		#region DG_AwayFrom

		public ZString DG_AwayFrom { get; private set; }

		public ZPropertyInfo DG_AwayFromInfo
		{
			get { return GetZPropertyInfo(nameof(DG_AwayFrom)); }
		}

		#endregion

		#region DG_SeperateFrom

		public ZString DG_SeperateFrom { get; private set; }

		public ZPropertyInfo DG_SeperateFromInfo
		{
			get { return GetZPropertyInfo(nameof(DG_SeperateFrom)); }
		}

		#endregion

		#region DG_SeperateCompartmentFrom

		public ZString DG_SeperateCompartmentFrom { get; private set; }

		public ZPropertyInfo DG_SeperateCompartmentFromInfo
		{
			get { return GetZPropertyInfo(nameof(DG_SeperateCompartmentFrom)); }
		}

		#endregion

		#region DG_SeperatedLongitudinallyFrom

		public ZString DG_SeperatedLongitudinallyFrom { get; private set; }

		public ZPropertyInfo DG_SeperatedLongitudinallyFromInfo
		{
			get { return GetZPropertyInfo(nameof(DG_SeperatedLongitudinallyFrom)); }
		}

		#endregion

		#region DG_NotOnSameShipAs

		public ZString DG_NotOnSameShipAs { get; private set; }

		public ZPropertyInfo DG_NotOnSameShipAsInfo
		{
			get { return GetZPropertyInfo(nameof(DG_NotOnSameShipAs)); }
		}

		#endregion

		#endregion

		#region DG_ULineEmsCheckBox

		public ZBool DG_ULineEmsCheckBox
		{
			get { return DG_UlineEMS != "0" && !DG_UlineEMS.IsEmpty; }
			set
			{
				DG_UlineEMS = value ? "1" : "";
				DG_ULineEmsCheckBoxInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DG_ULineEmsCheckBoxInfo
		{
			get { return GetZPropertyInfo(nameof(DG_ULineEmsCheckBox)); }
		}

		#endregion

		#region SelectedTextBox

		public ZString SelectedTextBox
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo SelectedTextBoxInfo
		{
			get { return GetZPropertyInfo(nameof(SelectedTextBox)); }
		}

		#endregion

		#region Lowest Flashpoint Temperature

		/// <summary>
		/// If empty, it means that flash point temp is not relevant for this substance
		/// </summary>
		public ZString LowestFlashPointTemp
		{
			get
			{
				if (!IsLowestFlashpointTempCalculated)
				{
					IsLowestFlashpointTempCalculated = true;
					if (!DG_FlashPoint.IsEmpty)
					{
						int indexOfTo = DG_FlashPoint.ToUpper().IndexOf("TO");
						if (indexOfTo < 0)
						{
							fLowestFlashPointTemp = DG_FlashPoint.KeepChars("0123456789-");
						}
						else
						{
							fLowestFlashPointTemp = DG_FlashPoint.ToUpper().SubstringSafe(0, indexOfTo).KeepChars("0123456789-");
						}
					}
				}
				return fLowestFlashPointTemp;
			}
		}

		ZString fLowestFlashPointTemp;
		bool IsLowestFlashpointTempCalculated;

		#endregion

		#region Limited Quantity Provision Text

		public ZString LQSpecProvData
		{
			get
			{
				if (!DG_LQSpecProvIndex.IsEmpty)
				{
					ZQuery query = new ZQuery(UNDGCommonDataSchema.DC_Type, UNDGCommonDataLookups.TypeConstants.SpecialProvisions);
					query.AddToFilter(UNDGCommonDataSchema.DC_Index, DG_LQSpecProvIndex);
					UNDGCommonData specialProvision = Factory.LoadTop1<UNDGCommonData>(query);
					return specialProvision != null ? specialProvision.DC_Descriptor : ZString.Empty;
				}
				return ZString.Empty;
			}
		}

		public ZPropertyInfo LQSpecProvDataInfo
		{
			get { return GetZPropertyInfo(nameof(LQSpecProvData)); }
		}

		#endregion

		#region Technical Name Requirement

		[BusinessObjectMaxLengthTestExclude]
		[List("Lookups.TechNameList")]
		public ZString DG_Calc_TechnicalNameRequirement
		{
			get => GetTechNameFromCharacter(base.DG_TechName);
			set
			{
				var character = GetCharacterForTechName(value);

				if (character != null)
				{
					DG_TechName = character;
				}

				DG_Calc_TechnicalNameRequirementInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DG_Calc_TechnicalNameRequirementInfo
		{
			get { return GetZPropertyInfo(nameof(DG_Calc_TechnicalNameRequirement)); }
		}

		#endregion

		protected override ZString HumanReadableNameCore => Res.GetString("ED151012-25F1-482B-8EC7-9695D15FACCE", "Dangerous Goods Substance - {0}", CalculateShortcutName());

		#endregion

		#region UNDGPermissibleQuantity

		public IEnumerable<UNDGPermissableQuantitiesHelper.UNDGPermissibleQuantity> CaoPermissibleQuantities
		{
			get => UNDGPermissableQuantitiesHelper.GetPermissibleQuantitiesForPackingInstruction(DG_CargoPackIns);
		}

		public IEnumerable<UNDGPermissableQuantitiesHelper.UNDGPermissibleQuantity> PaxPermissibleQuantities
		{
			get => UNDGPermissableQuantitiesHelper.GetPermissibleQuantitiesForPackingInstruction(DG_PaxPackIns);
		}

		public ZString PaxPackInsSec1
		{
			get
			{
				var paxPermissibleQuantitiesList = PaxPermissibleQuantities.ToList();
				return paxPermissibleQuantitiesList.Count > 0 ? paxPermissibleQuantitiesList[0].PackingInstructionSection : ZString.Empty;
			}
		}

		public ZString PaxPackInsSec2
		{
			get
			{
				var paxPermissibleQuantitiesList = PaxPermissibleQuantities.ToList();
				return paxPermissibleQuantitiesList.Count > 1 ? paxPermissibleQuantitiesList[1].PackingInstructionSection : ZString.Empty;
			}
		}

		public ZDecimal PaxMaxAmt1
		{
			get
			{
				var paxPermissibleQuantitiesList = PaxPermissibleQuantities.ToList();
				return paxPermissibleQuantitiesList.Count > 0 ? paxPermissibleQuantitiesList[0].PaxLimit : DG_LQ2OrPaxMaxAmt;
			}
		}

		public ZDecimal PaxMaxAmt2
		{
			get
			{
				var paxPermissibleQuantitiesList = PaxPermissibleQuantities.ToList();
				return paxPermissibleQuantitiesList.Count > 1 ? paxPermissibleQuantitiesList[1].PaxLimit : DG_LQ2OrPaxMaxAmt;
			}
		}

		public ZString PaxMaxAmtUQ1
		{
			get
			{
				var paxPermissibleQuantitiesList = PaxPermissibleQuantities.ToList();
				return paxPermissibleQuantitiesList.Count > 0 ? paxPermissibleQuantitiesList[0].PaxLimitUnits : DG_LQ2OrPaxMaxAmtUQ;
			}
		}

		public ZString PaxMaxAmtUQ2
		{
			get
			{
				var paxPermissibleQuantitiesList = PaxPermissibleQuantities.ToList();
				return paxPermissibleQuantitiesList.Count > 1 ? paxPermissibleQuantitiesList[1].PaxLimitUnits : DG_LQ2OrPaxMaxAmtUQ;
			}
		}

		public ZString CaoPackInsSec1
		{
			get
			{
				var caoPermissibleQuantitiesList = CaoPermissibleQuantities.ToList();
				return caoPermissibleQuantitiesList.Count > 0 ? caoPermissibleQuantitiesList[0].PackingInstructionSection : ZString.Empty;
			}
		}

		public ZString CaoPackInsSec2
		{
			get
			{
				var caoPermissibleQuantitiesList = CaoPermissibleQuantities.ToList();
				return caoPermissibleQuantitiesList.Count > 1 ? caoPermissibleQuantitiesList[1].PackingInstructionSection : ZString.Empty;
			}
		}

		public ZDecimal CaoMaxAmt1
		{
			get
			{
				var caoPermissibleQuantitiesList = CaoPermissibleQuantities.ToList();
				return caoPermissibleQuantitiesList.Count > 0 ? caoPermissibleQuantitiesList[0].CaoLimit : DG_CargoMaxAmt;
			}
		}

		public ZDecimal CaoMaxAmt2
		{
			get
			{
				var caoPermissibleQuantitiesList = CaoPermissibleQuantities.ToList();
				return caoPermissibleQuantitiesList.Count > 1 ? caoPermissibleQuantitiesList[1].CaoLimit : DG_CargoMaxAmt;
			}
		}

		public ZString CaoMaxAmtUQ1
		{
			get
			{
				var caoPermissibleQuantitiesList = CaoPermissibleQuantities.ToList();
				return caoPermissibleQuantitiesList.Count > 0 ? caoPermissibleQuantitiesList[0].CaoLimitUnits : DG_CargoMaxAmtUQ;
			}
		}

		public ZString CaoMaxAmtUQ2
		{
			get
			{
				var caoPermissibleQuantitiesList = CaoPermissibleQuantities.ToList();
				return caoPermissibleQuantitiesList.Count > 1 ? caoPermissibleQuantitiesList[1].CaoLimitUnits : DG_CargoMaxAmtUQ;
			}
		}

		#endregion

		#region UsrUSDOTShippingNames

		[ChildEditable(true)]
		public ViewUNDGAttributeCollection UsrUSDOTShippingNames
		{
			get
			{
				if (usrUSDOTShippingNames == null)
				{
					usrUSDOTShippingNames = new ViewUNDGAttributeCollection(this, ViewUNDGAttributeLookups.TypeConstants.UsrUSDOTShippingNames);
					RegisterEditableChildObject(usrUSDOTShippingNames);
					CreateCollectionForValidationIfNotExists();
				}

				return usrUSDOTShippingNames;
			}
		}

		ViewUNDGAttributeCollection usrUSDOTShippingNames;

		public override ZString DG_UsrUSDOTShippingName
		{ 
			get { return UsrUSDOTShippingNames.FirstOrDefault()?.DA_Descriptor ?? ZString.Empty; }
			set
			{
				base.DG_UsrUSDOTShippingName = value;

				if (value == ZString.Empty)
				{
					UsrUSDOTShippingNames.DeleteAll();
				}
				else if (UsrUSDOTShippingNames.Count == 0)
				{
					var newUsrAttrib = UsrUSDOTShippingNames.AddNew();
					newUsrAttrib.DA_Descriptor = value;
					newUsrAttrib.DA_Language = Core.Constants.Languages.English;
				}
				else
				{
					UsrUSDOTShippingNames.First().DA_Descriptor = value;
				}
			}
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Read Only

		protected bool GetReadOnly(PropertyDescriptor property)
		{
			bool result = property.Name != Schema.DG_UsrUSDOTShippingName && property.Name != "DetailsLanguage" && property.Name != Schema.DG_IsActive && DG_IsSystem;
			result = result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			return result;
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			var attributes = new ViewUNDGAttribute.Loader(Factory).LoadFromSubstancePK(PK);
			foreach (var attribute in attributes)
			{
				attribute.Delete();
			}

			StowageSegmentationDangerous.DeleteAll();
			StowageSegmentationCargo.DeleteAll();

			base.Delete();
		}

		#endregion

		#region Related Business Objects

		#region Collection for Validation

		void CreateCollectionForValidationIfNotExists()
		{
			var dummy = TheOneToRuleThemAll;
		}

		[ChildEditable(true)]
		ViewUNDGAttributeCollection TheOneToRuleThemAll
		{
			get
			{
				if (theOneToRuleThemAll == null)
				{
					theOneToRuleThemAll = new ViewUNDGAttributeCollection(this, string.Empty);
					RegisterEditableChildObject(theOneToRuleThemAll);
				}

				return theOneToRuleThemAll;
			}
		}

		ViewUNDGAttributeCollection theOneToRuleThemAll;

		#endregion

		#region Names

		[ChildEditable(true)]
		public ViewUNDGAttributeCollection Names
		{
			get
			{
				if (names == null)
				{
					names = new ViewUNDGAttributeCollection(this, ViewUNDGAttributeLookups.TypeConstants.ProperShippingName);
					RegisterEditableChildObject(names);
					CreateCollectionForValidationIfNotExists();
				}

				return names;
			}
		}

		ViewUNDGAttributeCollection names;

		#endregion

		#region Properties

		[ChildEditable(true)]
		public ViewUNDGAttributeCollection Properties
		{
			get
			{
				if (properties == null)
				{
					properties = new ViewUNDGAttributeCollection(this, ViewUNDGAttributeLookups.TypeConstants.Properties);
					RegisterEditableChildObject(properties);
					CreateCollectionForValidationIfNotExists();
				}

				return properties;
			}
		}

		ViewUNDGAttributeCollection properties;

		#endregion

		#region Observations

		[ChildEditable(true)]
		public ViewUNDGAttributeCollection Observations
		{
			get
			{
				if (observations == null)
				{
					observations = new ViewUNDGAttributeCollection(this, ViewUNDGAttributeLookups.TypeConstants.Observations);
					RegisterEditableChildObject(observations);
					CreateCollectionForValidationIfNotExists();
				}

				return observations;
			}
		}

		ViewUNDGAttributeCollection observations;

		#endregion

		#region Qualifying Descriptive Text

		[ChildEditable(true)]
		public ViewUNDGAttributeCollection QualifyingDescriptiveTexts
		{
			get
			{
				if (qualifyingDescriptiveTexts == null)
				{
					qualifyingDescriptiveTexts = new ViewUNDGAttributeCollection(this, ViewUNDGAttributeLookups.TypeConstants.QualifyingDescriptiveText);
					RegisterEditableChildObject(qualifyingDescriptiveTexts);
					CreateCollectionForValidationIfNotExists();
				}

				return qualifyingDescriptiveTexts;
			}
		}

		ViewUNDGAttributeCollection qualifyingDescriptiveTexts;

		#endregion

		#region UNDGCountryReferences

		[List("Lookups.UNDGCountryReferences")]
		public UNDGCountryReferenceManyToManyCollection<UNDGSubstance> UNDGCountryReferences
		{
			get
			{
				if (undgCountryReferences == null)
				{
					undgCountryReferences = GetUNDGCountryReferenceManyToManyCollection(this);
				}

				return undgCountryReferences;
			}
		}

		UNDGCountryReferenceManyToManyCollection<UNDGSubstance> undgCountryReferences;

		public static UNDGCountryReferenceManyToManyCollection<T> GetUNDGCountryReferenceManyToManyCollection<T>(T substance) where T : BusinessObject, IDGSubstance
		{
			var undgCountryReferences = new UNDGCountryReferenceManyToManyCollection<T>(substance);
			undgCountryReferences.Load();
			return undgCountryReferences;
		}

		#endregion

		#region Cross References

		[ChildEditable(true)]
		public ViewUNDGAttributeCollection CrossReferences
		{
			get
			{
				if (crossReferences == null)
				{
					crossReferences = new ViewUNDGAttributeCollection(this, ViewUNDGAttributeLookups.TypeConstants.CrossReferences);
					RegisterEditableChildObject(crossReferences);
					CreateCollectionForValidationIfNotExists();
				}

				return crossReferences;
			}
		}

		ViewUNDGAttributeCollection crossReferences;

		#endregion

		#region Special Provisions

		[ChildEditable(true)]
		[List("Lookups.SpecialProvisions")]
		public UNDGCommonDataCollection SpecialProvisions
		{
			get
			{
				if (specialProvisions == null)
				{
					specialProvisions = new UNDGCommonDataCollection(this, UNDGCommonDataLookups.TypeConstants.SpecialProvisions, ViewUNDGAttributeLookups.TypeConstants.SpecialProvisions);
					SpecialProvisions.ResetFilterFromAttributes(SpecialProvisionsAttributes);
					RegisterEditableChildObject(specialProvisions);
				}

				return specialProvisions;
			}
		}

		UNDGCommonDataCollection specialProvisions;

		internal ViewUNDGAttributeCollection SpecialProvisionsAttributes
		{
			get
			{
				if (specialProvisionsAttributes == null)
				{
					specialProvisionsAttributes = new ViewUNDGAttributeCollection(this, ViewUNDGAttributeLookups.TypeConstants.SpecialProvisions);
					specialProvisionsAttributes.CountChanged += delegate
					{ SpecialProvisions.ResetFilterFromAttributes(specialProvisionsAttributes); };
					RegisterEditableChildObject(specialProvisionsAttributes);
				}

				return specialProvisionsAttributes;
			}
		}

		ViewUNDGAttributeCollection specialProvisionsAttributes;

		public ZString SpecialProvisionDescriptor
		{
			get
			{
				return SpecialProvisionsAttributes?.FirstOrDefault()?.DA_Descriptor ?? ZString.Empty;
			}
		}

		public ZString[] SpecialProvisionDescriptors
		{
			get
			{
				return SpecialProvisionDescriptor.Split(' ');
			}
		}

		#endregion

		#region Segregation Groups

		public ZString[] SegregationGroups
		{
			get
			{
				return SegregationGroupAttributes
					.Where(attribute => !attribute.DA_Index.IsEmpty && attribute.DA_Index.StartsWith(ViewUNDGAttributeLookups.TypeConstants.SegregationGroups, StringComparison.Ordinal))
					.Select(attribute =>
						(
							Code: attribute.DA_Index,
							Number: int.TryParse(attribute.DA_Index.Substring(ViewUNDGAttributeLookups.TypeConstants.SegregationGroups.Length), out var number) ? number : (int?)null
						))
					.OrderBy(segregationGroup => segregationGroup.Number)
					.Select(segregationGroup => segregationGroup.Code)
					.ToArray();
			}
		}

		public ZString SegregationGroupsForBinding
		{
			get
			{
				return ZString.Join(", ", SegregationGroups);
			}
		}

		public ZPropertyInfo SegregationGroupsForBindingInfo => GetZPropertyInfo(nameof(SegregationGroupsForBinding));

		#endregion

		#region Segregation Codes

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Regex pattern")]
		const string RegexSG = "(SG([0-9]{0,2})([a-z]{0,1})) (Stow)?";

		public ZString[] SegregationCodes
		{
			get
			{
				return StowageSegmentationDangerous.
					Select(commonData => Regex.Match(commonData.DC_Descriptor, RegexSG)).
					Where(match => match.Success).
					Select(match =>
						(
							Code: new ZString(match.Groups[1].Value),
							Number: int.TryParse(match.Groups[2].Value, out var number) ? number : (int?)null,
							Variant: match.Groups[3].Value
						)).
					OrderBy(segregationCode => segregationCode.Number).
					ThenBy(segregationCode => segregationCode.Variant).
					Select(segregationCode => segregationCode.Code).
					ToArray();
			}
		}

		public ZString SegregationCodesForBinding
		{
			get
			{
				return ZString.Join(", ", SegregationCodes);
			}
		}

		public ZPropertyInfo SegregationCodesForBindingInfo => GetZPropertyInfo(nameof(SegregationCodesForBinding));

		#endregion

		#region Stowage/Segmentation - Dangerous Goods

		[ChildEditable(true)]
		[List("Lookups.StowageSegmentationRequirements")]
		public UNDGCommonDataCollection StowageSegmentationDangerous
		{
			get
			{
				if (stowageSegmentationDangerous == null)
				{
					stowageSegmentationDangerous = new UNDGCommonDataCollection(
						this,
						UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements,
						new[] { ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods, ViewUNDGAttributeLookups.TypeConstants.SegregationGroups },
						commonData => commonData != null && commonData.DC_Index.StartsWith(ViewUNDGAttributeLookups.TypeConstants.SegregationGroups, StringComparison.Ordinal)
								? ViewUNDGAttributeLookups.TypeConstants.SegregationGroups
								: ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods);
					stowageSegmentationDangerous.ResetFilterFromAttributes(StowageSegmentationDangerousAttributes.Concat(SegregationGroupAttributes));
					RegisterEditableChildObject(stowageSegmentationDangerous);
				}

				return stowageSegmentationDangerous;
			}
		}

		UNDGCommonDataCollection stowageSegmentationDangerous;

		internal ViewUNDGAttributeCollection StowageSegmentationDangerousAttributes
		{
			get
			{
				if (stowageSegmentationDangerousAttributes == null)
				{
					stowageSegmentationDangerousAttributes = new ViewUNDGAttributeCollection(this, ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods);
					stowageSegmentationDangerousAttributes.CountChanged += delegate
					{
						StowageSegmentationDangerous.ResetFilterFromAttributes(stowageSegmentationDangerousAttributes.Concat(SegregationGroupAttributes));
						SegregationCodesForBindingInfo.RefreshBinding();
					};
					RegisterEditableChildObject(stowageSegmentationDangerousAttributes);
				}

				return stowageSegmentationDangerousAttributes;
			}
		}

		ViewUNDGAttributeCollection stowageSegmentationDangerousAttributes;

		internal ViewUNDGAttributeCollection SegregationGroupAttributes
		{
			get
			{
				if (segregationGroupAttributes == null)
				{
					segregationGroupAttributes = new ViewUNDGAttributeCollection(this, ViewUNDGAttributeLookups.TypeConstants.SegregationGroups);
					segregationGroupAttributes.CountChanged += delegate
					{
						StowageSegmentationDangerous.ResetFilterFromAttributes(StowageSegmentationDangerousAttributes.Concat(segregationGroupAttributes));
						SegregationGroupsForBindingInfo.RefreshBinding();
					};
					RegisterEditableChildObject(segregationGroupAttributes);
				}

				return segregationGroupAttributes;
			}
		}

		ViewUNDGAttributeCollection segregationGroupAttributes;

		#endregion

		#region Stowage/Segmentation - Cargo

		[ChildEditable(true)]
		[List("Lookups.StowageSegmentationRequirements")]
		public UNDGCommonDataCollection StowageSegmentationCargo
		{
			get
			{
				if (stowageSegmentationCargo == null)
				{
					stowageSegmentationCargo = new UNDGCommonDataCollection(this, UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements, ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_Cargo);
					StowageSegmentationCargo.ResetFilterFromAttributes(StowageSegmentationCargoAttributes);
					RegisterEditableChildObject(stowageSegmentationCargo);
				}

				return stowageSegmentationCargo;
			}
		}

		UNDGCommonDataCollection stowageSegmentationCargo;

		internal ViewUNDGAttributeCollection StowageSegmentationCargoAttributes
		{
			get
			{
				if (stowageSegmentationCargoAttributes == null)
				{
					stowageSegmentationCargoAttributes = new ViewUNDGAttributeCollection(this, ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_Cargo);
					stowageSegmentationCargoAttributes.CountChanged += delegate
					{ StowageSegmentationCargo.ResetFilterFromAttributes(stowageSegmentationCargoAttributes); };
					RegisterEditableChildObject(stowageSegmentationCargoAttributes);
				}

				return stowageSegmentationCargoAttributes;
			}
		}

		ViewUNDGAttributeCollection stowageSegmentationCargoAttributes;

		#endregion

		#region UNDGSubstanceOfStandard

		public IUNDGStandardSubstance StandardSubstance
		{
			get
			{
				switch (DG_Standard)
				{
					case UNDGSubstanceStandardTypes.ADN:
						return Factory.Load<UNDGSubstanceADN>(PK);

					case UNDGSubstanceStandardTypes.ADR:
						return Factory.Load<UNDGSubstanceADR>(PK);

					case UNDGSubstanceStandardTypes.RID:
						return Factory.Load<UNDGSubstanceRID>(PK);

					case UNDGSubstanceStandardTypes.CFR:
						return Factory.Load<UNDGSubstanceCFR>(PK);

					case UNDGSubstanceStandardTypes.JTT:
						return Factory.Load<UNDGSubstanceJTT>(PK);

					default:
						return null;
				}
			}
		}

		#endregion

		#endregion

		#region ADR Properties for binding

		public ZString DG_Calc_ADRFormattedLabels => StandardSubstance is UNDGSubstanceADR adrSubstance
			? adrSubstance.ADR_FormattedLabels
			: ZString.Empty;

		public ZString DG_Calc_ADRTransportCategory => StandardSubstance is UNDGSubstanceADR adrSubstance
			? adrSubstance.ADR_TransportCategory
			: ZString.Empty;

		public ZString DG_Calc_ADRClassificationCode => StandardSubstance is UNDGSubstanceADR adrSubstance
			? adrSubstance.ADR_ClassificationCode
			: ZString.Empty;

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get
			{
				return !DG_IsSystem;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return ResString.GetMultilingualString("87a73e2e-dcde-4b1f-9ca1-d05a4cce00fb", "Cannot delete a system Dangerous Goods record.");
			}
		}

		#endregion

		#region Field Type

		#region Pack Max. Qty Field Type
		public string PackMaxQtyFieldType
		{
			get { return GetFieldColumnType(UNDGSubstanceSchema.DG_LQMaxAmt, false, false); }
		}
		#endregion

		#region Pack UQ Field Type
		public string PackUQFieldType
		{
			get { return GetFieldColumnType(UNDGSubstanceSchema.DG_LQMaxAmtUQ, false, false); }
		}
		#endregion

		#region Packing Instructions Field Type
		public string PackingInstructionsFieldType
		{
			get { return GetFieldColumnType(UNDGSubstanceSchema.DG_PackIns, false, false); }
		}
		#endregion

		public string GetFieldColumnType(SchemaColumn column, bool hasList, bool useNaturalKey)
		{
			var result = FieldType.Text;
			switch (column.ColumnType)
			{
				case SchemaColumnType.String:
					{
						if (!hasList)
						{
							result = FieldType.Text;
						}
						else if (useNaturalKey)
						{
							result = FieldType.TextDropEdit;
						}
						else
						{
							result = FieldType.TextCodeFindBox;
						}
						break;
					}
				case SchemaColumnType.Int:
					result = FieldType.Integer;
					break;
				case SchemaColumnType.Decimal:
					result = FieldType.Decimal;
					break;
				case SchemaColumnType.Bool:
					result = FieldType.Boolean;
					break;
				case SchemaColumnType.Guid:
					result = FieldType.Guid;
					break;
			}

			return result.ToString();
		}
		#endregion

		#region IDGSubstance

		ZString IDGSubstance.UNNO => DG_UNNO;

		ZString IDGSubstance.Variant => DG_Variant;

		ZString IDGSubstance.Standard => DG_Standard;

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			DG_UNNO = RandomDGUNNOForTesting();
			DG_Variant = RandomDGVariantForTesting();
			DG_Standard = UNDGSubstanceStandardTypes.IMO;
		}

		string RandomDGUNNOForTesting()
		{
			return new Random().Next(9000, 9999).ToString();
		}

		string RandomDGVariantForTesting()
		{
			return new Random().Next(0, 99).ToString();
		}
#endif
	}
}
