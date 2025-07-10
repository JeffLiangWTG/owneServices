using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class DangerousGood : DocDataObject, IDangerousGood, IAdHocValidationProvider
	{
		#region Constructor

		public DangerousGood() : this(null)
		{
		}

		public DangerousGood(object identifier) : base(identifier)
		{
		}

		#endregion

		#region Quantity

		public ZInt Quantity
		{
			get => quantity;
			set
			{
				if (SetNonPersistentPropertyValue(QuantityInfo, ref quantity, value))
				{
					Validate(QuantityInfo);
				}
			}
		}

		ZInt quantity;

		public ZPropertyInfo QuantityInfo => GetZPropertyInfo(nameof(Quantity));

		#endregion

		#region Prefix

		public ZString Prefix
		{
			get => prefix;
			set
			{
				if (SetNonPersistentPropertyValue(PrefixInfo, ref prefix, value))
				{
					Validate(PrefixInfo);
				}
			}
		}

		ZString prefix;

		public ZPropertyInfo PrefixInfo => GetZPropertyInfo(nameof(Prefix));

		#endregion

		#region Code

		public ZString Code
		{
			get => code;
			set
			{
				if (SetNonPersistentPropertyValue(CodeInfo, ref code, value))
				{
					Validate(CodeInfo);
				}
			}
		}

		ZString code;

		public ZPropertyInfo CodeInfo => GetZPropertyInfo(nameof(Code));

		#endregion

		#region Unno

		public ZString Unno
		{
			get => unno;
			set
			{
				if (SetNonPersistentPropertyValue(UnnoInfo, ref unno, value))
				{
					Validate(UnnoInfo);
				}
			}
		}

		ZString unno;

		public ZPropertyInfo UnnoInfo => GetZPropertyInfo(nameof(Unno));

		#endregion

		#region TransportMode

		public ICodeDescription TransportMode
		{
			get => transportMode;
			set => transportMode = SetChild(transportMode, value);
		}

		ICodeDescription transportMode;

		#endregion

		#region Variant

		public ZString Variant
		{
			get => variant;
			set
			{
				if (SetNonPersistentPropertyValue(VariantInfo, ref variant, value))
				{
					Validate(VariantInfo);
				}
			}
		}

		ZString variant;

		public ZPropertyInfo VariantInfo => GetZPropertyInfo(nameof(Variant));

		#endregion

		#region ProperShippingName

		public ZString ProperShippingName
		{
			get => properShippingName;
			set
			{
				if (SetNonPersistentPropertyValue(ProperShippingNameInfo, ref properShippingName, value))
				{
					Validate(ProperShippingNameInfo);
				}
			}
		}

		ZString properShippingName;

		public ZPropertyInfo ProperShippingNameInfo => GetZPropertyInfo(nameof(ProperShippingName));

		#endregion

		#region TechnicalName

		public ZString TechnicalName
		{
			get => technicalName;
			set
			{
				if (SetNonPersistentPropertyValue(TechnicalNameInfo, ref technicalName, value))
				{
					Validate(TechnicalNameInfo);
				}
			}
		}

		ZString technicalName;

		public ZPropertyInfo TechnicalNameInfo => GetZPropertyInfo(nameof(TechnicalName));

		#endregion

		#region MaterialFormDescription

		public ZString MaterialFormDescription
		{
			get => materialFormDescription;
			set
			{
				if (SetNonPersistentPropertyValue(MaterialFormDescriptionInfo, ref materialFormDescription, value))
				{
					Validate(MaterialFormDescriptionInfo);
				}
			}
		}

		ZString materialFormDescription;

		public ZPropertyInfo MaterialFormDescriptionInfo => GetZPropertyInfo(nameof(MaterialFormDescription));

		#endregion

		#region RadionuclideElementSuffix

		public ZString RadionuclideElementSuffix
		{
			get => radionuclideElementSuffix;
			set
			{
				if (SetNonPersistentPropertyValue(RadionuclideElementSuffixInfo, ref radionuclideElementSuffix, value))
				{
					Validate(RadionuclideElementSuffixInfo);
				}
			}
		}

		ZString radionuclideElementSuffix;

		public ZPropertyInfo RadionuclideElementSuffixInfo => GetZPropertyInfo(nameof(RadionuclideElementSuffix));

		#endregion

		#region SpecialPermitNumber

		public ZString SpecialPermitNumber
		{
			get => specialPermitNumber;
			set
			{
				if (SetNonPersistentPropertyValue(SpecialPermitNumberInfo, ref specialPermitNumber, value))
				{
					Validate(SpecialPermitNumberInfo);
				}
			}
		}

		ZString specialPermitNumber;

		public ZPropertyInfo SpecialPermitNumberInfo => GetZPropertyInfo(nameof(SpecialPermitNumber));

		#endregion

		#region HazardousWasteCode

		public ZString HazardousWasteCode
		{
			get => hazardousWasteCode;
			set
			{
				if (SetNonPersistentPropertyValue(HazardousWasteCodeInfo, ref hazardousWasteCode, value))
				{
					Validate(HazardousWasteCodeInfo);
				}
			}
		}

		ZString hazardousWasteCode;

		public ZPropertyInfo HazardousWasteCodeInfo => GetZPropertyInfo(nameof(HazardousWasteCode));

		#endregion

		#region PoisonInhalationHazard

		public ZString PoisonInhalationHazard
		{
			get => poisonInhalationHazard;
			set
			{
				if (SetNonPersistentPropertyValue(PoisonInhalationHazardInfo, ref poisonInhalationHazard, value))
				{
					Validate(PoisonInhalationHazard);
				}
			}
		}

		ZString poisonInhalationHazard;

		public ZPropertyInfo PoisonInhalationHazardInfo => GetZPropertyInfo(nameof(PoisonInhalationHazard));

		#endregion

		#region PSAGroup

		public ZString PSAGroup
		{
			get => psaGroup;
			set
			{
				if (SetNonPersistentPropertyValue(PSAGroupInfo, ref psaGroup, value))
				{
					Validate(PSAGroup);
				}
			}
		}

		ZString psaGroup;

		public ZPropertyInfo PSAGroupInfo => GetZPropertyInfo(nameof(PSAGroup));

		#endregion

		#region IMOClass

		public ZString IMOClass
		{
			get => imoClass;
			set
			{
				if (SetNonPersistentPropertyValue(IMOClassInfo, ref imoClass, value))
				{
					Validate(IMOClassInfo);
				}
			}
		}

		ZString imoClass;

		public ZPropertyInfo IMOClassInfo => GetZPropertyInfo(nameof(IMOClass));

		#endregion

		#region PackingGroup

		public ZString PackingGroup
		{
			get => packingGroup;
			set
			{
				if (SetNonPersistentPropertyValue(PackingGroupInfo, ref packingGroup, value))
				{
					Validate(PackingGroupInfo);
				}
			}
		}

		ZString packingGroup;

		public ZPropertyInfo PackingGroupInfo => GetZPropertyInfo(nameof(PackingGroup));

		#endregion

		#region SubLabel1

		public ZString SubLabel1
		{
			get => subLabel1;
			set
			{
				if (SetNonPersistentPropertyValue(SubLabel1Info, ref subLabel1, value))
				{
					Validate(SubLabel1Info);
				}
			}
		}

		ZString subLabel1;

		public ZPropertyInfo SubLabel1Info => GetZPropertyInfo(nameof(SubLabel1));

		#endregion

		#region SubLabel2

		public ZString SubLabel2
		{
			get => subLabel2;
			set
			{
				if (SetNonPersistentPropertyValue(SubLabel2Info, ref subLabel2, value))
				{
					Validate(SubLabel2Info);
				}
			}
		}

		ZString subLabel2;

		public ZPropertyInfo SubLabel2Info => GetZPropertyInfo(nameof(SubLabel2));

		#endregion

		#region State

		public ZString State
		{
			get => state;
			set
			{
				if (SetNonPersistentPropertyValue(StateInfo, ref state, value))
				{
					Validate(StateInfo);
				}
			}
		}

		ZString state;

		public ZPropertyInfo StateInfo => GetZPropertyInfo(nameof(State));

		#endregion

		#region Standard

		public ZString Standard
		{
			get => standard;
			set
			{
				if (SetNonPersistentPropertyValue(StandardInfo, ref standard, value))
				{
					Validate(StandardInfo);
				}
			}
		}

		ZString standard;

		public ZPropertyInfo StandardInfo => GetZPropertyInfo(nameof(Standard));

		#endregion

		#region SecondaryClass

		public ZString SecondaryClass
		{
			get => secondaryClass;
			set
			{
				if (SetNonPersistentPropertyValue(SecondaryClassInfo, ref secondaryClass, value))
				{
					Validate(SecondaryClassInfo);
				}
			}
		}

		ZString secondaryClass;

		public ZPropertyInfo SecondaryClassInfo => GetZPropertyInfo(nameof(SecondaryClass));

		#endregion

		#region TertiaryClass

		public ZString TertiaryClass
		{
			get => tertiaryClass;
			set
			{
				if (SetNonPersistentPropertyValue(TertiaryClassInfo, ref tertiaryClass, value))
				{
					Validate(TertiaryClassInfo);
				}
			}
		}

		ZString tertiaryClass;

		public ZPropertyInfo TertiaryClassInfo => GetZPropertyInfo(nameof(TertiaryClass));

		#endregion

		#region Weight

		public Measurement Weight
		{
			get => weight;
			set => weight = SetChild(weight, value);
		}

		Measurement weight;

		IMeasurement IDangerousGood.Weight => Weight;

		#endregion

		#region NetExplosiveWeight

		public Measurement NetExplosiveWeight
		{
			get => netExplosiveWeight;
			set => netExplosiveWeight = SetChild(netExplosiveWeight, value);
		}

		Measurement netExplosiveWeight;

		IMeasurement IDangerousGood.NetExplosiveWeight => NetExplosiveWeight;

		#endregion

		#region Volume

		public IMeasurement Volume
		{
			get => volume;
			set => volume = SetChild(volume, value);
		}

		IMeasurement volume;

		#endregion

		#region PackedInLimitedQuantity

		public ZBool PackedInLimitedQuantity
		{
			get => packedInLimitedQuantity;
			set
			{
				if (SetNonPersistentPropertyValue(PackedInLimitedQuantityInfo, ref packedInLimitedQuantity, value))
				{
					Validate(PackedInLimitedQuantityInfo);
				}
			}
		}

		ZBool packedInLimitedQuantity;

		public ZPropertyInfo PackedInLimitedQuantityInfo => GetZPropertyInfo(nameof(PackedInLimitedQuantity));

		#endregion

		#region IsFissileExcepted

		public ZBool IsFissileExcepted
		{
			get => isFissileExcepted;
			set
			{
				if (SetNonPersistentPropertyValue(IsFissileExceptedInfo, ref isFissileExcepted, value))
				{
					Validate(IsFissileExceptedInfo);
				}
			}
		}

		ZBool isFissileExcepted;

		public ZPropertyInfo IsFissileExceptedInfo => GetZPropertyInfo(nameof(IsFissileExcepted));

		#endregion

		#region IsExclusiveUse

		public ZBool IsExclusiveUse
		{
			get => isExclusiveUse;
			set
			{
				if (SetNonPersistentPropertyValue(IsExclusiveUseInfo, ref isExclusiveUse, value))
				{
					Validate(IsExclusiveUseInfo);
				}
			}
		}

		ZBool isExclusiveUse;

		public ZPropertyInfo IsExclusiveUseInfo => GetZPropertyInfo(nameof(IsExclusiveUse));

		#endregion

		#region IsHighwayRouteControlledQuantity

		public ZBool IsHighwayRouteControlledQuantity
		{
			get => isHighwayRouteControlledQuantity;
			set
			{
				if (SetNonPersistentPropertyValue(IsHighwayRouteControlledQuantityInfo, ref isHighwayRouteControlledQuantity, value))
				{
					Validate(IsHighwayRouteControlledQuantityInfo);
				}
			}
		}

		ZBool isHighwayRouteControlledQuantity;

		public ZPropertyInfo IsHighwayRouteControlledQuantityInfo => GetZPropertyInfo(nameof(IsHighwayRouteControlledQuantity));

		#endregion

		#region IsResidueLastContained

		public ZBool IsResidueLastContained
		{
			get => isResidueLastContained;
			set
			{
				if (SetNonPersistentPropertyValue(IsResidueLastContainedInfo, ref isResidueLastContained, value))
				{
					Validate(IsResidueLastContainedInfo);
				}
			}
		}

		ZBool isResidueLastContained;

		public ZPropertyInfo IsResidueLastContainedInfo => GetZPropertyInfo(nameof(IsResidueLastContained));

		#endregion

		#region RequiresTemperatureControl

		public ZBool RequiresTemperatureControl
		{
			get => requiresTemperatureControl;
			set
			{
				if (SetNonPersistentPropertyValue(RequiresTemperatureControlInfo, ref requiresTemperatureControl, value))
				{
					Validate(RequiresTemperatureControlInfo);
				}
			}
		}

		ZBool requiresTemperatureControl;

		public ZPropertyInfo RequiresTemperatureControlInfo => GetZPropertyInfo(nameof(RequiresTemperatureControl));

		#endregion

		#region IsSalvagePackaging

		public ZBool IsSalvagePackaging
		{
			get => isSalvagePackaging;
			set
			{
				if (SetNonPersistentPropertyValue(IsSalvagePackagingInfo, ref isSalvagePackaging, value))
				{
					Validate(IsSalvagePackagingInfo);
				}
			}
		}

		ZBool isSalvagePackaging;

		public ZPropertyInfo IsSalvagePackagingInfo => GetZPropertyInfo(nameof(IsSalvagePackaging));

		#endregion

		#region RadioactiveTransportIndex

		public ZDecimal RadioactiveTransportIndex
		{
			get => radioactiveTransportIndex;
			set
			{
				if (SetNonPersistentPropertyValue(RadioactiveTransportIndexInfo, ref radioactiveTransportIndex, value))
				{
					Validate(RadioactiveTransportIndexInfo);
				}
			}
		}

		ZDecimal radioactiveTransportIndex;

		public ZPropertyInfo RadioactiveTransportIndexInfo => GetZPropertyInfo(nameof(RadioactiveTransportIndex));

		#endregion

		#region FlashPoint

		public IMeasurement FlashPoint
		{
			get => flashPoint;
			set => flashPoint = SetChild(flashPoint, value);
		}

		IMeasurement flashPoint;

		#endregion

		public IMeasurement ReportableQuantity
		{
			get => reportableQuantity;
			set => reportableQuantity = SetChild(reportableQuantity, value);
		}

		IMeasurement reportableQuantity;

		public IMeasurement RequiredTemperatureMaximum
		{
			get => requiredTemperatureMaximum;
			set => requiredTemperatureMaximum = SetChild(requiredTemperatureMaximum, value);
		}

		IMeasurement requiredTemperatureMaximum;

		public IMeasurement RadioactiveMaximumActivity
		{
			get => radioactiveMaximumActivity;
			set => radioactiveMaximumActivity = SetChild(radioactiveMaximumActivity, value);
		}

		IMeasurement radioactiveMaximumActivity;

		public ICodeDescription PackageType
		{
			get => packageType;
			set => packageType = SetChild(packageType, value);
		}

		ICodeDescription packageType;

		public ICodeDescription MarinePollutant
		{
			get => marinePollutant;
			set => marinePollutant = SetChild(marinePollutant, value);
		}

		ICodeDescription marinePollutant;

		public Contact Contact
		{
			get => contact;
			set => contact = SetChild(contact, value);
		}

		Contact contact;

		IContact IDangerousGood.Contact => Contact;

		#region EmergencyScheduleCode

		public ICodeDescription EmergencyScheduleFire
		{
			get => emergencyScheduleFire;
			set => emergencyScheduleFire = SetChild(emergencyScheduleFire, value);
		}

		ICodeDescription emergencyScheduleFire;

		public ICodeDescription EmergencyScheduleSpillage
		{
			get => emergencyScheduleSpillage;
			set => emergencyScheduleSpillage = SetChild(emergencyScheduleSpillage, value);
		}

		ICodeDescription emergencyScheduleSpillage;

		#endregion

		#region ExceptedQuantityCode

		public ICodeDescription ExceptedQuantityCode
		{
			get => exceptedQuantityCode;
			set => exceptedQuantityCode = SetChild(exceptedQuantityCode, value);
		}

		public System.Func<IEnumerable<string>> Validator { get; set; }

		ICodeDescription exceptedQuantityCode;

		#endregion

		#region RadioactiveLabelCategory

		public ICodeDescription RadioactiveLabelCategory
		{
			get => radioactiveLabelCategory;
			set => radioactiveLabelCategory = SetChild(radioactiveLabelCategory, value);
		}

		ICodeDescription radioactiveLabelCategory;

		#endregion

		#region RadionuclideElement

		public ICodeDescription RadionuclideElement
		{
			get => radionuclideElement;
			set => radionuclideElement = SetChild(radionuclideElement, value);
		}

		ICodeDescription radionuclideElement;

		#endregion

		#region Implementation

		public override string ToString() => this.WriteSummary();

		#endregion
	}
}
