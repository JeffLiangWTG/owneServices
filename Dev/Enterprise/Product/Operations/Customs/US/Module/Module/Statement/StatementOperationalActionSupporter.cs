using System;
using CargoWise.Definitions;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.Module
{
	public class StatementOperationalActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext => BusinessContext.CustomsStatementHdr;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.USCustomsImportStatement;

		public override Type RootType => typeof(CusStatementHeader);

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.USStatement);
		}
	}
}
