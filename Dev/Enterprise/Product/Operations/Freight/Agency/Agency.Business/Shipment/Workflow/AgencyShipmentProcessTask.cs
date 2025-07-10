using System;
using System.Data;

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentProcessTask : ProcessTask
	{
		public AgencyShipmentProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(AgencyShipment); }
		}

		public new AgencyShipment Parent
		{
			get { return (AgencyShipment)base.Parent; }
		}
	}
}
