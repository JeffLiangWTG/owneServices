using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	public class OrgOpportunityActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.OrgOpportunity; }
		}

		public override Type RootType
		{
			get { return typeof(OrgOpportunity); }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.OpportunityManagement;

		public override string SingularElementNoun
		{
			get { return Res.GetString("57ed6ab1-c956-4281-a97b-2eceeee18031", "opportunity"); }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("48dc5388-50fe-4392-8375-119be2d805fd", "opportunities"); }
		}
	}
}
