using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.MasterFiles.Business.UNDGSubstanceCFRLookups;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(Schema.CFR_UNNO), DescriptionProperty(Schema.CFR_PSN)]
	public class UNDGSubstanceCFR : AutoUNDGSubstanceCFR,
		IUNDGAttributeParent,
		IUNDGStandardSubstance,
		IDGSubstance
	{
		public UNDGSubstanceCFR(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region OnLoaded

		public override void OnLoaded()
		{
			base.OnLoaded();
			SetDefaultDetailsLanguage();
		}

		#endregion

		#region IDGSubstance

		ZString IDGSubstance.UNNO => CFR_UNNO;

		ZString IDGSubstance.Variant => CFR_Variant;

		ZString IDGSubstance.Standard => TablePrefix;

		#endregion

		#region IUNDGAttributeParent

		[List("Lookups.Languages")]
		[MaxLength(7)]
		public ZString DetailsLanguage
		{
			get { return detailsLanguage; }
			set
			{
				if (detailsLanguage != value)
				{
					CheckMaximumLength(DetailsLanguageInfo, value);
					detailsLanguage = value;
					DetailsLanguageInfo.RefreshBinding();
				}
			}
		}

		ZString detailsLanguage;

		public ZPropertyInfo DetailsLanguageInfo => GetZPropertyInfo(nameof(DetailsLanguage));

		void SetDefaultDetailsLanguage()
		{
			DetailsLanguage = ZString.Empty;
		}

		public ZString TableCode => TablePrefix;

		#endregion

		#region Properties

		public ZBool CFR_IsSystemDefined => true;

		[List("Lookups.PrimaryClassList")]
		public override ZString CFR_PrimaryClass
		{
			get { return base.CFR_PrimaryClass; }
			set { base.CFR_PrimaryClass = value; }
		}

		[List("Lookups.SecondaryClassList")]
		public override ZString CFR_SecondaryClass
		{
			get { return base.CFR_SecondaryClass; }
			set { base.CFR_SecondaryClass = value; }
		}

		[List("Lookups.TertiaryClassList")]
		public override ZString CFR_TertiaryClass
		{
			get { return base.CFR_TertiaryClass; }
			set { base.CFR_TertiaryClass = value; }
		}

		[List("Lookups.PackingGroupList")]
		public override ZString CFR_PackingGroup
		{
			get { return base.CFR_PackingGroup; }
			set { base.CFR_PackingGroup = value; }
		}

		[List("Lookups.ExceptedQuantityList")]
		public override ZString CFR_ExceptedQuantity
		{
			get { return base.CFR_ExceptedQuantity; }
			set { base.CFR_ExceptedQuantity = value; }
		}

		[List("Lookups.TechnicalNameList")]
		[BusinessObjectMaxLengthTestExclude]
		public ZString CFR_Calc_TechnicalName
		{
			get => UNDGSubstanceLookups.GetTechNameFromCharacter(base.CFR_TechnicalName);
			set
			{
				var character = UNDGSubstanceLookups.GetCharacterForTechName(value);

				if (character != null)
				{
					CFR_TechnicalName = character;
				}

				CFR_Calc_TechnicalNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CFR_Calc_TechnicalNameInfo => GetZPropertyInfo(nameof(CFR_Calc_TechnicalName));

		[List("Lookups.MarinePollutantList")]
		public override ZString CFR_MarinePollutant
		{
			get { return base.CFR_MarinePollutant; }
			set { base.CFR_MarinePollutant = value; }
		}

		[List("Lookups.StateList")]
		public override ZString CFR_State
		{
			get { return base.CFR_State; }
			set { base.CFR_State = value; }
		}

		[List("Lookups.AirRailLimitTypeList")]
		public override ZString CFR_PAXAirRailLimitType
		{
			get => base.CFR_PAXAirRailLimitType;
			set => base.CFR_PAXAirRailLimitType = value;
		}

		[List("Lookups.AirRailLimitTypeList")]
		public override ZString CFR_CargoAirRailLimitType
		{
			get => base.CFR_CargoAirRailLimitType;
			set => base.CFR_CargoAirRailLimitType = value;
		}

		#region QualifyingDescriptiveTexts

		[ChildEditable(true)]
		public UNDGAttributeZZCollection QualifyingDescriptiveTexts
		{
			get
			{
				if (qualifyingDescriptiveTexts == null)
				{
					qualifyingDescriptiveTexts = new UNDGAttributeZZCollection(Factory, this, ViewUNDGAttributeLookups.TypeConstants.QualifyingDescriptiveText);
					RegisterEditableChildObject(qualifyingDescriptiveTexts);
				}

				return qualifyingDescriptiveTexts;
			}
		}

		UNDGAttributeZZCollection qualifyingDescriptiveTexts;

		#endregion

		[List("Lookups.PoisonInhalationHazardList")]
		public ZString CFR_Calc_PoisonInhalationHazardReadable
		{
			get
			{
				return PoisonInhalationHazardReadableDictionary.TryGetValue(CFR_PoisonInhalationHazard, out ZString readableValue)
					? readableValue
					: ZString.Empty;
			}
		}

		IDictionary<ZString, ZString> PoisonInhalationHazardReadableDictionary
			=> poisonInhalationHazardReadableDictionary ?? (poisonInhalationHazardReadableDictionary = new Dictionary<ZString, ZString>()
			{
				{ PoisonInhalationHazardProvisions.Codes.SP1Raw, PoisonInhalationHazardProvisions.Codes.SP1Readable },
				{ PoisonInhalationHazardProvisions.Codes.SP2Raw, PoisonInhalationHazardProvisions.Codes.SP2Readable },
				{ PoisonInhalationHazardProvisions.Codes.SP3Raw, PoisonInhalationHazardProvisions.Codes.SP3Readable },
				{ PoisonInhalationHazardProvisions.Codes.SP4Raw, PoisonInhalationHazardProvisions.Codes.SP4Readable },
				{ PoisonInhalationHazardProvisions.Codes.SP5Raw, PoisonInhalationHazardProvisions.Codes.SP5Readable },
				{ PoisonInhalationHazardProvisions.Codes.SP6Raw, PoisonInhalationHazardProvisions.Codes.SP6Readable }
			});

		IDictionary<ZString, ZString> poisonInhalationHazardReadableDictionary;

		public ZDecimal CFR_Calc_ReportableKiloQuantity
		{
			get => Core.Constants.Weight.Convert(CFR_ReportableQuantity, CFR_ReportableQuantityUnit, Core.Constants.Weight.Kilograms);
		}

		public ZString CFR_Calc_ReportableKiloQuantityUnit
		{
			get => Core.Constants.Weight.Kilograms.ToLower();
		}

		#endregion

		#region Prefixes

		public static class Prefixes
		{
			public const string NA = "NA";
			public const string UN = "UN";
			public const string ID = "ID";
		}

		#endregion
	}
}
