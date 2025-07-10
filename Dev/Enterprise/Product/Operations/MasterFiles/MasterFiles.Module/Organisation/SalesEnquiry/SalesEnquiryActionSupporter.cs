using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	public class SalesEnquiryActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.SalesEnquiry; }
		}

		public override Type RootType
		{
			get { return typeof(SalesEnquiry); }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.InquiryManager;

		public override string SingularElementNoun
		{
			get { return Res.GetString("fb91a254-70df-4241-821a-7f00876703b7", "inquiry"); }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("64fb220c-8a22-4ca2-b1d9-dc5356647d12", "inquiries"); }
		}
	}
}
