using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	public class OrgContactsActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.OrganisationContacts; }
		}

		public override SecurityCheckpoint BaseCheckpoint
		{
			get { return Env.Security.OrgContact; }
		}

		public override Type RootType
		{
			get { return typeof(OrgContact); }
		}
	}
}
