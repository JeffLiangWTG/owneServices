using System;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.GUI
{
	public class ADDCVDAppliesActionMethod : OperationalActionMethod
	{
		public ADDCVDAppliesActionMethod()
			: base(new Guid("57836c47-31f7-4c00-903a-cded585a9b41"))
		{
		}

		public override string Name => Res.GetString("21caf45e-f6ac-40de-b3f5-728ff134188a", "ADD/CVD Applies");

		public override string Description => Res.GetString("3ae90434-d7d0-4f8c-abc0-146ccc462120", "Apply anti-dumping and countervailing on US low value consignments");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new ADDCVDAppliesApplicator();
		}
	}
}
