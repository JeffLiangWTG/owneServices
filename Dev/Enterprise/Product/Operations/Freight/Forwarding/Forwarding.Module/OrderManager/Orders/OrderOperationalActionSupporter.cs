using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	public class OrderOperationalActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Order; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.OrderTracking;

		public override string PluralElementNoun
		{
			get { return Res.GetString("Forwarding|OrderOperationalActionSupporter|PluralElementNoun", "Orders"); }
		}

		public override string SingularElementNoun
		{
			get { return Res.GetString("Forwarding|OrderOperationalActionSupporter|SingularElementNoun", "Order"); }
		}

		public override Type RootType
		{
			get { return typeof(Order); }
		}
	}
}
