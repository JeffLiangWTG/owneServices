using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportConsignment.Business;

namespace Enterprise.TransportConsignment.Module
{
	internal class DtbConsignmentRunSheetOperationalActionSupporter : OperationalActionSupporter
	{
		#region OperationalActionSupporter Members

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.DtbConsignRunSheet; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.DtbConsignmentRunSheet;

		public override Type RootType
		{
			get { return typeof(DtbConsignmentRunSheet); }
		}

		public override string SingularElementNoun
		{
			get { return Res.GetString("DtbConsignmentRunSheetOperationalActionSupporter|SingularElementNoun", "Consignment Run Sheet"); }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("DtbConsignmentRunSheetOperationalActionSupporter|PluralElementNoun", "Consignment Run Sheets"); }
		}
		#endregion
	}
}
