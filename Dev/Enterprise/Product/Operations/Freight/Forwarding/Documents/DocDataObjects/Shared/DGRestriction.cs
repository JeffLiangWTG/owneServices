using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class DGRestriction : DocDataObject
	{
		#region EmergencyScheduleFire

		public ICodeDescription EmergencyScheduleFire
		{
			get => emergencyScheduleFire;
			set => emergencyScheduleFire = SetChild(emergencyScheduleFire, value);
		}

		ICodeDescription emergencyScheduleFire;

		#endregion

		#region EmergencyScheduleSpillage

		public ICodeDescription EmergencyScheduleSpillage
		{
			get => emergencyScheduleSpillage;
			set => emergencyScheduleSpillage = SetChild(emergencyScheduleSpillage, value);
		}

		ICodeDescription emergencyScheduleSpillage;

		#endregion

		#region ExceptedQuantityCode

		public ZString ExceptedQuantityCode
		{
			get => exceptedQuantityCode;
			set
			{
				if (SetNonPersistentPropertyValue(ExceptedQuantityCodeInfo, ref exceptedQuantityCode, value))
				{
					Validate(ExceptedQuantityCodeInfo);
				}
			}
		}

		ZString exceptedQuantityCode;

		public ZPropertyInfo ExceptedQuantityCodeInfo => GetZPropertyInfo(nameof(ExceptedQuantityCode));

		#endregion

		#region FlashPoint

		public ZString FlashPoint
		{
			get => flashPoint;
			set
			{
				if (SetNonPersistentPropertyValue(FlashPointInfo, ref flashPoint, value))
				{
					Validate(FlashPointInfo);
				}
			}
		}

		ZString flashPoint;

		public ZPropertyInfo FlashPointInfo => GetZPropertyInfo(nameof(FlashPoint));

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

		#region MarinePollutant

		public ZString MarinePollutantCode
		{
			get => marinePollutantCode;
			set
			{
				if (SetNonPersistentPropertyValue(MarinePollutantCodeInfo, ref marinePollutantCode, value))
				{
					Validate(MarinePollutantCodeInfo);
				}
			}
		}

		ZString marinePollutantCode;

		public ZPropertyInfo MarinePollutantCodeInfo => GetZPropertyInfo(nameof(MarinePollutantCode));

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
	}
}
