using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	[System.Diagnostics.DebuggerDisplay("{HumanReadableName}")]
	public sealed class SlotAllocation : AutoJobSlotAllocation
	{
		public SlotAllocation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		public decimal GetAspectOverallocation(string code)
		{
			return GetAspect(code) * (1 + E0_OverAllocationPercent / 100m);
		}

		public decimal GetAspect(string code)
		{
			SlotAllocationAspect aspect = Aspects[code];
			return aspect == null ? 0m : (decimal)aspect.D5_Value;
		}

		public void SetAspect(string code, decimal value)
		{
			SlotAllocationAspect aspect = Aspects[code];

			if (aspect != null)
			{
				aspect.D5_Value = value;
			}
			else if (value != 0)
			{
				aspect = Aspects.AddNew();
				aspect.D5_Type = code;
				aspect.D5_Value = value;
			}
		}

		public bool HasAllocations
		{
			get
			{
				if (hasAllocations == null)
				{
					hasAllocations = new CachedProperty<bool>(Factory, delegate
					{
						foreach (SlotAllocationAspect aspect in Aspects)
						{
							if (aspect.D5_Value != 0)
							{
								return true;
							}
						}

						return false;
					});
				}
				return hasAllocations.Value;
			}
		}
		CachedProperty<bool> hasAllocations;

		#region Properties

		public override ZBool E0_UseDefaultOverAllocation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.E0_UseDefaultOverAllocation; }
			set
			{
				base.E0_UseDefaultOverAllocation = value;
				if (value)
				{
					E0_OverAllocationPercent = 0m;
				}
				else
				{
					E0_OverAllocationPercent = DefaultOverAllocationPercent;
				}
			}
		}

		public override ZDecimal E0_OverAllocationPercent
		{
			get { return E0_UseDefaultOverAllocation ? DefaultOverAllocationPercent : base.E0_OverAllocationPercent; }
			set { base.E0_OverAllocationPercent = value; }
		}

		#endregion

		#region Related Business Objects

		public ISlotAllocationParent Parent
		{
			get { return (ISlotAllocationParent)Factory.Load(ParentType, E0_ParentID); }
		}

		[ChildEditable(true)]
		public SlotAllocationAspectCollection Aspects
		{
			get
			{
				if (aspects == null)
				{
					aspects = new SlotAllocationAspectCollection(this);
					RegisterEditableChildObject(aspects);
				}

				return aspects;
			}
		}
		SlotAllocationAspectCollection aspects;

		#endregion

		#region Implementation

		public static ZDecimal DefaultOverAllocationPercent
		{
			get { return FreightConfigurationRegistry.Instance.DefaultOverAllocationPercent.Value; }
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(new string[]
			{
				JobSlotAllocationSchema.Constants.E0_ParentID,
				JobSlotAllocationSchema.Constants.E0_ParentTableCode,
			});

			SlotAllocation other = (SlotAllocation)base.CloneInternal(args);

			foreach (SlotAllocationAspect aspect in Aspects)
			{
				if (aspect.D5_Value != 0)
				{
					other.Aspects.Add((SlotAllocationAspect)aspect.Clone());
				}
			}

			return other;
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString principalPart = (Principal == null ? Res.GetString("588503b1-c7d4-49dd-ab8d-b57fe21dd6f5", "Generic") : Principal.OH_Code.ToString());
				ZString parentHumanReadableName = (Parent == null) ? ZString.Empty : Parent.HumanReadableName;
				return principalPart + ": " + parentHumanReadableName;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			E0_UseDefaultOverAllocation = true;
		}

		public override void Delete()
		{
			Aspects.DeleteAll();
			base.Delete();
		}

		Type ParentType
		{
			get
			{
				switch (E0_ParentTableCode)
				{
					case JobVoyCountrySchema.Constants.Prefix:
						return typeof(VoyageCountry);
					case JobVoyOriginSchema.Constants.Prefix:
						return typeof(VoyageOrigin);
					case JobSailingSchema.Constants.Prefix:
						return typeof(JobSailing);
					default:
						throw new NotSupportedException("'" + E0_ParentTableCode + "' is not a supported table code");
				}
			}
		}

		#endregion
	}
}
