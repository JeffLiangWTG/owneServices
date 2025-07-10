using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Agency.Module
{
	internal class ContainerMoveActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.AgencyContainerMove; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.AgencyContainerMovements;

		public override Type RootType
		{
			get { return typeof(ContainerMovement); }
		}

		public override string SingularElementNoun
		{
			get { return Res.GetString("000d285d-b69c-4a99-9750-f5740ffd75cd", "movement"); }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("3b424a84-4c29-4908-8722-b5736c8f0f66", "movements"); }
		}

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.Shipping);
		}
	}
}


