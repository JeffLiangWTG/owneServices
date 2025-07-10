using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class NatureAndQuantityOfDangerousGoodsLine : DocDataObject
	{
		public NatureAndQuantityOfDangerousGoodsLine(NatureAndQuantityOfDangerousGoodsLineType lineType)
			: this(null, lineType)
		{
		}

		public NatureAndQuantityOfDangerousGoodsLine(object id, NatureAndQuantityOfDangerousGoodsLineType lineType)
			: base(id)
		{
			LineType = lineType;

			if (lineType == NatureAndQuantityOfDangerousGoodsLineType.Detail && id == null)
			{
				ErrorReporter.ReportOnce("LineType cannot be null for Detail line");
			}
		}

		public NatureAndQuantityOfDangerousGoodsLineType LineType { get; }

		#region UNCode

		public ZString UNCode
		{
			get => unCode;
			set
			{
				if (SetNonPersistentPropertyValue(UNCodeInfo, ref unCode, value))
				{
					Validate(UNCodeInfo);
				}
			}
		}

		ZString unCode;

		public ZPropertyInfo UNCodeInfo => GetZPropertyInfo(nameof(UNCode));

		#endregion

		#region UNCodePrefix

		public ZString UNCodePrefix
		{
			get => unCodePrefix;
			set
			{
				if (SetNonPersistentPropertyValue(UNCodePrefixInfo, ref unCodePrefix, value))
				{
					Validate(UNCodePrefixInfo);
				}
			}
		}

		ZString unCodePrefix;

		public ZPropertyInfo UNCodePrefixInfo => GetZPropertyInfo(nameof(UNCodePrefix));

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

		#region Class

		public ZString Class
		{
			get => _class;
			set
			{
				if (SetNonPersistentPropertyValue(ClassInfo, ref _class, value))
				{
					Validate(ClassInfo);
				}
			}
		}

		ZString _class;

		public ZPropertyInfo ClassInfo => GetZPropertyInfo(nameof(Class));

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

		#region PackCount

		public ZInt PackCount
		{
			get => packCount;
			set
			{
				if (SetNonPersistentPropertyValue(PackCountInfo, ref packCount, value))
				{
					Validate(PackCountInfo);
				}
			}
		}

		ZInt packCount;

		public ZPropertyInfo PackCountInfo => GetZPropertyInfo(nameof(PackCount));

		#endregion

		#region PackageType

		public ICodeDescription PackageType
		{
			get => packageType;
			set => packageType = SetChild(packageType, value);
		}

		ICodeDescription packageType;

		#endregion

		#region Quantity

		public IMeasurement Quantity
		{
			get => IncludePackInformation ? QuantityPerPack : TotalQuantity;
		}

		#endregion

		#region QuantityPerPack

		public IMeasurement QuantityPerPack
		{
			get => quantityPerPack;
			set => quantityPerPack = SetChild(quantityPerPack, value);
		}

		IMeasurement quantityPerPack;

		#endregion

		#region TotalQuantity

		public IMeasurement TotalQuantity
		{
			get => totalQuantity;
			set => totalQuantity = SetChild(totalQuantity, value);
		}

		IMeasurement totalQuantity;

		#endregion

		#region QuantityIndicator

		public ZString QuantityIndicator
		{
			get => quantityIndicator;
			set
			{
				if (SetNonPersistentPropertyValue(QuantityIndicatorInfo, ref quantityIndicator, value))
				{
					Validate(QuantityIndicatorInfo);
				}
			}
		}

		ZString quantityIndicator;

		public ZPropertyInfo QuantityIndicatorInfo => GetZPropertyInfo(nameof(QuantityIndicator));

		#endregion

		#region IncludePackInformation

		public ZBool IncludePackInformation
		{
			get => includePackInformation;
			set
			{
				if (SetNonPersistentPropertyValue(IncludePackInformationInfo, ref includePackInformation, value))
				{
					Validate(IncludePackInformationInfo);
				}
			}
		}

		ZBool includePackInformation;

		public ZPropertyInfo IncludePackInformationInfo => GetZPropertyInfo(nameof(IncludePackInformation));

		#endregion

		#region Description

		public ZString Description
		{
			get => description;
			set
			{
				if (SetNonPersistentPropertyValue(DescriptionInfo, ref description, value))
				{
					Validate(DescriptionInfo);
				}
			}
		}

		ZString description;

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

		#endregion

		#region LimitType

		public ZString LimitType
		{
			get => limitType;
			set
			{
				if (SetNonPersistentPropertyValue(LimitTypeInfo, ref limitType, value))
				{
					Validate(LimitTypeInfo);
				}
			}
		}

		ZString limitType;

		public ZPropertyInfo LimitTypeInfo => GetZPropertyInfo(nameof(LimitType));

		#endregion

		#region PackingInstruction

		public ZString PackingInstruction
		{
			get => packingInstruction;
			set
			{
				if (SetNonPersistentPropertyValue(PackingInstructionInfo, ref packingInstruction, value))
				{
					Validate(PackingInstructionInfo);
				}
			}
		}

		ZString packingInstruction;

		public ZPropertyInfo PackingInstructionInfo => GetZPropertyInfo(nameof(PackingInstruction));

		#endregion

		#region SpecialProvisionDescriptor

		public ZString SpecialProvisionDescriptor
		{
			get => specialProvisionDescriptor;
			set
			{
				if (SetNonPersistentPropertyValue(SpecialProvisionDescriptorInfo, ref specialProvisionDescriptor, value))
				{
					Validate(SpecialProvisionDescriptorInfo);
				}
			}
		}

		ZString specialProvisionDescriptor;

		public ZPropertyInfo SpecialProvisionDescriptorInfo => GetZPropertyInfo(nameof(SpecialProvisionDescriptor));

		#endregion

		#region SpecialProvisionDescriptors

		public ZString[] SpecialProvisionDescriptors
		{
			get => SpecialProvisionDescriptor.Split(' ');
		}

		#endregion

		#region Authorization

		public ZString Authorization
		{
			get => authorization;
			set
			{
				if (SetNonPersistentPropertyValue(AuthorizationInfo, ref authorization, value))
				{
					Validate(AuthorizationInfo);
				}
			}
		}

		ZString authorization;

		public ZPropertyInfo AuthorizationInfo => GetZPropertyInfo(nameof(Authorization));

		public bool Authorization_ReadOnly => LineType != NatureAndQuantityOfDangerousGoodsLineType.Detail;

		#endregion

		#region Technical Name

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

		#region DG_SubLabel1

		public ZString DG_SubLabel1
		{
			get => dG_SubLabel1;
			set
			{
				if (SetNonPersistentPropertyValue(DG_SubLabel1Info, ref dG_SubLabel1, value))
				{
					Validate(DG_SubLabel1Info);
				}
			}
		}

		ZString dG_SubLabel1;

		public ZPropertyInfo DG_SubLabel1Info => GetZPropertyInfo(nameof(DG_SubLabel1));

		#endregion

		#region DG_SubLabel2

		public ZString DG_SubLabel2
		{
			get => dG_SubLabel2;
			set
			{
				if (SetNonPersistentPropertyValue(DG_SubLabel2Info, ref dG_SubLabel2, value))
				{
					Validate(DG_SubLabel2Info);
				}
			}
		}

		ZString dG_SubLabel2;

		public ZPropertyInfo DG_SubLabel2Info => GetZPropertyInfo(nameof(DG_SubLabel2));

		#endregion

		#region NECWeight

		public IMeasurement NECWeight
		{
			get => necWeight;
			set => necWeight = SetChild(necWeight, value);
		}

		IMeasurement necWeight;

		#endregion
	}
}
