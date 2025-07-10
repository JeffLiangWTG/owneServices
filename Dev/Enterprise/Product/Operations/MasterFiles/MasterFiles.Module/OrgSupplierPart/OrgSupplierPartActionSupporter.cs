using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	public class OrgSupplierPartActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.OrgSupplierPart; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.CustomsSupplierPart;

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.MasterFiles);
			list.Add(ActionMethodProviderIDs.FRProduct);
		}

		public override Type RootType
		{
			get { return typeof(OrgSupplierPart); }
		}

		public override string SingularElementNoun
		{
			get { return Res.GetString("37b6beb4-39ce-4874-b699-09de0e54cd59", "product"); }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("2de263cf-73a7-4e84-b91f-653bb8c71fdb", "products"); }
		}
	}
}
