using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Packing.Business;
using Enterprise.Security;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Module;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.Module
{
	public class DtbBookingConsignmentController : DtbTransportController
	{
		#region Controller / Module ID

		public override ControllerID ID
		{
			get { return ControllerIDs.DtbBookingConsignment; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.DtbBookingConsignment; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(DtbBookingConsignment); }
		}

		#endregion

		#region Form

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new DtbBookingConsignmentForm((DtbBookingConsignment)businessEntity);
		}

		#endregion

		#region GetNewBusinessEntityInLocalFactory

		protected override Type TransportConsolidationType
		{
			get { return typeof(DtbConsignmentConsolidation); }
		}

		protected override void SetupNewBusinessEntity(DtbTransport transport)
		{
			base.SetupNewBusinessEntity(transport);
			PkgPackageJob.LoadOrCreatePackageJobWithNoChanges((DtbBookingConsignment)transport);
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.DtbBookingConsignmentDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.DtbBookingConsignmentEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.DtbBookingConsignmentNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.DtbBookingConsignmentView; }
		}

		#endregion
	}
}
