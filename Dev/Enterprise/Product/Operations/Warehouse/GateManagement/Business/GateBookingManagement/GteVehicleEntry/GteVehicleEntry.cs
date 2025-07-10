using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.GateManagement.Business
{
	[CodeAlive("Needed for facilities Gate Management product")]
	public sealed partial class GteVehicleEntry : AutoGteVehicleEntry
	{
		public GteVehicleEntry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		public override void OnSaving()
		{
			if (!IsDeleted)
			{
				PopulateFormattedNumberPropertyIfRequired(GVE_GateActionNumberInfo, Env.NumberFountains.GteGateActionNumber);
			}

			base.OnSaving();
		}

		#region GLN_Lane

		public GteLane GateLane => Factory.Load<GteLane>(GVE_GLN_Lane);

		[RelatedBusinessObject("GateLane")]
		public override ZGuid GVE_GLN_Lane
		{
			get => base.GVE_GLN_Lane;
			set => base.GVE_GLN_Lane = value;
		}

		#endregion

		#region GVM_VehicleMovement

		public GteVehicleMovement VehicleMovement => Factory.Load<GteVehicleMovement>(GVE_GVM_VehicleMovement);

		[RelatedBusinessObject("VehicleMovement")]
		public override ZGuid GVE_GVM_VehicleMovement
		{
			get => base.GVE_GVM_VehicleMovement;
			set => base.GVE_GVM_VehicleMovement = value;
		}

		#endregion
	}
}
