using System;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class OrganisationConsumerType : JobInvoicingConsumerType
	{
		public OrganisationConsumerType(string code, MultilingualString description)
			: base(code, description) { }

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Organisation; }
		}

		public override Type BizoType
		{
			get { return typeof(OrgHeader); }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.None; }
		}
	}
}
