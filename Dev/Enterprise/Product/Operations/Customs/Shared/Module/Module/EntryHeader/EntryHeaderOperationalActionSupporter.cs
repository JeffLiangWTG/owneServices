using System;
using CargoWise.Definitions;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.Module
{
	public class EntryHeaderOperationalActionSupporter : OperationalActionSupporter
	{
		public sealed override BusinessContext BusinessContext => BusinessContext.CusEntryHeader;

		public override Type RootType => typeof(CusEntryHeader);

		public sealed override SecurityCheckpoint BaseCheckpoint => Env.Security.CustomsDeclarationEnquiry;
	}
}
