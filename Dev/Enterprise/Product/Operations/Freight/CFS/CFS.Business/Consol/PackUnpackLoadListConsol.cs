
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class PackUnpackLoadListConsol : CFSLoadListConsol
	{
		public PackUnpackLoadListConsol(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Related Business Objects

		#region Containers

		public new TallyContainerDependentCollection Containers
		{
			get { return (TallyContainerDependentCollection)base.Containers; }
		}

		protected override CommonContainerCollection GetNewContainerCollection()
		{
			return new TallyContainerDependentCollection(this, Factory);
		}

		#endregion

		#region Shipments

		public new PackUnpackShipmentCollection Shipments
		{
			get { return (PackUnpackShipmentCollection)base.Shipments; }
		}

		protected override ConsolShipmentCollection GetNewConsolShipmentCollection()
		{
			return new PackUnpackShipmentCollection(this);
		}

		#endregion

		#endregion

		#region Properties

		[List("RefUNLOCO_List")]
		public override ZString JK_RL_NKLoadPort
		{
			get { return base.JK_RL_NKLoadPort; }
			set
			{
				base.JK_RL_NKLoadPort = value;
				Containers.MarkAsNeedingValidation();
			}
		}

		public override ZGuid JK_OA_ShippingLineAddress
		{
			get { return base.JK_OA_ShippingLineAddress; }
			set
			{
				base.JK_OA_ShippingLineAddress = value;
				Containers.MarkAsNeedingValidation();
			}
		}

		[List("RefUNLOCO_List")]
		public override ZString JK_RL_NKDischargePort
		{
			get { return base.JK_RL_NKDischargePort; }
			set
			{
				base.JK_RL_NKDischargePort = value;
				Containers.MarkAsNeedingValidation();
			}
		}

		public override ZString JK_AgentsReference
		{
			get { return base.JK_AgentsReference; }
			set
			{
				base.JK_AgentsReference = value;
				Containers.MarkAsNeedingValidation();
			}
		}

		public override ZString JK_ConsolMode
		{
			get { return base.JK_ConsolMode; }
			set
			{
				base.JK_ConsolMode = value;
				Containers.MarkAsNeedingValidation();
			}
		}

		#endregion
	}
}
