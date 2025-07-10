using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	public class CommunicationActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Communication; }
		}

		public override Type RootType
		{
			get { return typeof(OrgSalesCall); }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.CommunicationManager;

		public override string SingularElementNoun
		{
			get { return Res.GetString("c176ce77-3ea9-4122-a8b4-f6f2302b7919", "communication"); }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("adb7221c-bafc-493e-a0d9-5b5b6d5d2473", "communications"); }
		}
	}
}
