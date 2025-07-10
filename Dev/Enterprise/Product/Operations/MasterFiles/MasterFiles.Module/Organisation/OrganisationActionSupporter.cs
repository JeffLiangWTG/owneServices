using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	public class OrganisationActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Organisation; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.Organisation;

		public override Type RootType
		{
			get { return typeof(OrgHeader); }
		}

		public override string SingularElementNoun
		{
			get { return Res.GetString("f6cf3157-a540-403f-9b39-8cf782e678d7", "organization"); }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("37c0ce0e-c6ce-4043-aca1-f876e3663b71", "organizations"); }
		}
	}
}
