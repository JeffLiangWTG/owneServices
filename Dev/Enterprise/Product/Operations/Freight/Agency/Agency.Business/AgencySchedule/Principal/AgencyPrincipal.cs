using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class AgencyPrincipal : AgencyAllocationItem<OrgHeader>, IObsoleteValidation
	{
		#region Schema

		public new class Schema : AgencyAllocationItem.Schema
		{
			public const string Code = "Code"; // hard-coded constant
		}

		#endregion

		public AgencyPrincipal(BusinessObjectFactory factory)
			: base(factory) { }

		public AgencyPrincipal(OrgHeader principal)
			: base(principal, principal.PK) { }

		#region Properties

		public ZString Code
		{
			get
			{
				string result;

				if (PrincipalPK.IsValid)
				{
					result = Principal.OH_Code;
				}
				else
				{
					result = (NoResString)"General"; // hard-coded constant
				}

				return result;
			}
		}
		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(Schema.Code); }
		}

		#endregion

		#region Related BusinessObjects

		public VoyageCountry VoyageCountry
		{
			get { return Schedule == null ? null : Schedule.VoyageCountry; }
		}

		public AgencyCountry Schedule
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return schedule; }
			[System.Diagnostics.DebuggerStepThrough]
			set { schedule = value; }
		}
		AgencyCountry schedule;

		public AgencyOriginDependentCollection Origins
		{
			get
			{
				if (origins == null)
				{
					origins = new AgencyOriginDependentCollection(this);
					origins.Load();
					RegisterEditableChildObject(origins);
				}

				return origins;
			}
		}
		AgencyOriginDependentCollection origins;

		public AgencySailingDependentCollection Sailings
		{
			get
			{
				if (sailings == null)
				{
					sailings = new AgencySailingDependentCollection(this);
					sailings.Load();
					RegisterEditableChildObject(sailings);
				}

				return sailings;
			}
		}
		AgencySailingDependentCollection sailings;

		public OrgHeader Principal
		{
			get { return BizObj; }
		}

		#endregion

		#region Operations

		public void RefreshUsageData()
		{
			if (PrincipalPK.IsEmpty != VoyageCountry.J0_AllocationsByPrincipal)
			{
				UsageProvider.Load(Schedule.Voyage, PrincipalPK);
				RefreshUsageBindings();
			}
		}

		public IAllocationUsageProvider UsageProvider
		{
			get
			{
				if (usageProvider == null)
				{
					usageProvider = new AllocationUsageProvider();
				}

				return usageProvider;
			}
#if DEBUG
			set
			{
				usageProvider = value;
			}
#endif
		}
		IAllocationUsageProvider usageProvider;

		protected override void RefreshUsageBindingsCore()
		{
			base.RefreshUsageBindingsCore();

			foreach (AgencyOrigin origin in Origins)
			{
				origin.RefreshUsageBindings();
			}

			foreach (AgencySailing sailing in Sailings)
			{
				sailing.RefreshUsageBindings();
			}
		}

		#endregion

		#region Implementation

		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", Justification = "Calling getter")]
		protected override void RunPreSaveValidationCore()
		{
			object lazyLoadSailingsToRegisterThemEditable = Sailings;
			object lazyLoadOriginsToRegisterThemEditable = Origins;

			base.RunPreSaveValidationCore();
		}

		public override void Delete()
		{
			Origins.RemoveAndDeleteAll();
			Sailings.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override SlotAllocationDependentCollection SlotAllocations
		{
			get { return VoyageCountry == null ? null : VoyageCountry.SlotAllocations; }
		}

		protected override bool ShouldValidateUsage
		{
			get { return VoyageCountry.J0_AllocationMethod == AllocationMethodList.Codes.Country; }
		}

		protected override AllocationUsage GetUsage()
		{
			return UsageProvider.GetCountryUsage(VoyageCountry);
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		protected override int GetDefaultNumberOfDecimalsCore(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForShipping.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		#endregion
	}
}
