using System;
using CargoWise.Definitions;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Module
{
	public class CusUSLVConsignmentOperationalActionSupporter : OperationalActionSupporter
	{
		public override Type RootType => typeof(USConsignmentCombined);

		public override BusinessContext BusinessContext => BusinessContext.USLowValueBill;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.USLVConsignment;

		public override string SingularElementNoun => Res.GetString("b50b1731-be55-494e-949d-54e7ace9b66b", "Low Value Entries by Bill");

		public override string PluralElementNoun => Res.GetString("96d99b0b-3941-49dc-9136-6d5ec6ece173", "Low Value Entries by Bills");

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.USLowValueBill);
		}
	}
}
