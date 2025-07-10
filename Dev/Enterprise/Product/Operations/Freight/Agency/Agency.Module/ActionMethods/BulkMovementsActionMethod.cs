using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI.BulkMovements;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Agency.Module
{
	internal sealed class BulkMovementsActionMethod : OperationalActionMethod
	{
		public BulkMovementsActionMethod()
			: base(new Guid("1475edb6-034e-48a5-b117-c5a1215af5f5")) { }

		public override string Name
		{
			get { return Res.GetString("1475edb6-034e-48a5-b117-c5a1215af5f5", "Add Bulk Movements"); }
		}

		public override string Description
		{
			get
			{
				return Res.GetString(
					"904e54e2-607f-4a94-a8bc-2ed3e14d2b02",
					"Adds a new movement with the given type and date to all selected containers.");
			}
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new BulkMovementsApplicator(factory);
		}

		public override bool HasControl
		{
			get { return true; }
		}

		public override IComponent NewGuiControl()
		{
			return new BulkMovementsApplicatorControl();
		}

		public override LicenceCheckpoint[] GetRequiredLicenceCheckpoints()
		{
			return new LicenceCheckpoint[] { Env.Licence.ShippingManagerContainerControl };
		}

		public override SecurityCheckpoint[] GetRequiredSecurityCheckpoints()
		{
			return new SecurityCheckpoint[] { Env.Security.AgencyContainerManagerEdit };
		}
	}
}


