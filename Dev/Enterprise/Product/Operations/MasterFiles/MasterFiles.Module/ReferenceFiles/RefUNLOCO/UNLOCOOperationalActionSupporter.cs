using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	class UNLOCOOperationalActionSupporter : OperationalActionSupporter
	{
		public override Type RootType
		{
			get { return typeof(RefUNLOCO); }
		}

		public override BusinessContext BusinessContext
		{
			get
			{
				return BusinessContext.UNLOCO;
			}
		}
		public override SecurityCheckpoint BaseCheckpoint => Env.Security.UNLOCO;

		public override string SingularElementNoun
		{
			get { return Res.GetString("38af1b1e-7ad0-477f-8d27-33ede9e4b9sd", "UNLOCO"); }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("b512fa2f-0db2-46e0-9a7e-75424facadvf", "UNLOCOs"); }
		}

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
		}

		protected override bool SupportsBulkUpdatesCore
		{
			get { return true; }
		}
	}
}
