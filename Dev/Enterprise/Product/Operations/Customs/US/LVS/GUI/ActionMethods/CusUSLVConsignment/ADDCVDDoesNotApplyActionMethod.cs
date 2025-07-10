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
	public class ADDCVDDoesNotApplyActionMethod : OperationalActionMethod
	{
		public ADDCVDDoesNotApplyActionMethod()
			: base(new Guid("5891ca4d-0ee7-4108-8fc8-4c5ce2043573"))
		{
		}

		public override string Name => Res.GetString("1120a1c0-82fa-4072-a957-8fdf29804711", "ADD/CVD Does not apply");

		public override string Description => Res.GetString("25f68a61-3c3a-49a9-ace4-0a1cd56aa642", "Disable anti-dumping and countervailing on US low value consignments");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new ADDCVDDoesNotApplyApplicator();
		}
	}
}
