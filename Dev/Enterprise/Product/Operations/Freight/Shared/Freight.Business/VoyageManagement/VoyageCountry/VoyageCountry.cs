using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	[System.Diagnostics.DebuggerDisplay("Country = {J0_RN_NKCountry}")]
	public class VoyageCountry : AutoJobVoyCountry, ISlotAllocationParent
	{
		public VoyageCountry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		public override ZString J0_RN_NKCountry
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.J0_RN_NKCountry; }
			set
			{
				ZString oldValue = J0_RN_NKCountry;
				base.J0_RN_NKCountry = value;

				if (!IsInDatabase && !IsCopying && value != oldValue)
				{
					J0_AllocationMethod = FreightConfigurationRegistry.Instance.DefaultAllocationMethods.Value.GetAllocationMethod(value);
				}
			}
		}

		#region Related BusinessObjects

		public JobVoyage Voyage
		{
			get { return Factory.Load<JobVoyage>(J0_JV); }
		}

		[ChildEditable(true)]
		public VoyageOriginByCountryView Origins
		{
			get
			{
				if (fOrigins == null && Country != null)
				{
					fOrigins = new VoyageOriginByCountryView(this);
					RegisterEditableChildObject(fOrigins);
				}

				return fOrigins;
			}
		}
		VoyageOriginByCountryView fOrigins;

		[ChildEditable(true)]
		public SailingsByCountryView Sailings
		{
			get
			{
				if (fSailings == null)
				{
					fSailings = new SailingsByCountryView(this);
					RegisterEditableChildObject(fSailings);
				}

				return fSailings;
			}
		}
		SailingsByCountryView fSailings;

		[ChildEditable(true)]
		public SlotAllocationDependentCollection SlotAllocations
		{
			get
			{
				if (fSlotAllocations == null)
				{
					fSlotAllocations = new SlotAllocationDependentCollection(this);
					fSlotAllocations.Load();
					RegisterEditableChildObject(fSlotAllocations);
				}

				return fSlotAllocations;
			}
		}
		SlotAllocationDependentCollection fSlotAllocations;

		#endregion

		#region IAllocationParent Members

		ZString ISlotAllocationParent.Code
		{
			get { return JobVoyCountrySchema.Constants.Prefix; }
		}

		#endregion

		#region Lookups

		public new BaseJobVoyCountryLookups Lookups
		{
			get { return lookups ?? (lookups = (BaseJobVoyCountryLookups)GetNewLookups()); }
		}
		BaseJobVoyCountryLookups lookups;

		protected override JobVoyCountryLookups GetNewLookups()
		{
			return new BaseJobVoyCountryLookups(this);
		}

		#endregion

		#region Implementation

		protected override JobVoyCountryValidation GetNewValidation()
		{
			return new BaseJobVoyCountryValidation(this);
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("11080f2c-6015-4b57-b5c0-31a06efaa95f", "Country/Region = '{0}'", J0_RN_NKCountry); }
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			VoyageCountry country = (VoyageCountry)base.CloneInternal(args);
			country.J0_AllocationMethod = J0_AllocationMethod;

			foreach (SlotAllocation allocation in SlotAllocations)
			{
				country.SlotAllocations.Add(allocation.Clone());
			}

			return country;
		}

		public override void Delete()
		{
			base.Delete();
			SlotAllocations.RemoveAndDeleteAll();
		}

		#endregion
	}
}
