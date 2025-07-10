using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Freight.QuotedBookings.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.QuotedBookings.Module
{
	public class PreAllocationController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.QuotedBookings; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.PreAllocations; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ViewQuotedBooking); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new PreAllocationForm((PreAllocation)businessEntity);
		}

		#region New

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			ZGuid bookingOnlyPK = QuotedBooking.CreateNewBooking(Factory).PK;
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, bookingOnlyPK, Factory);
			return new PreAllocation(quotedBooking, PreAllocation.PreAllocationState.New);
		}

		#endregion

		#region Load

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			ViewQuotedBooking viewQuotedBooking = (ViewQuotedBooking)sourceEntity;
			QuotedBooking quotedBooking = viewQuotedBooking.QuotedBooking;
			return new PreAllocation(quotedBooking, PreAllocation.PreAllocationState.Existing);
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.QuickBooking; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.QuickBookingNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.QuickBookingEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.QuickBookingDelete; }
		}

		#endregion
	}
}
