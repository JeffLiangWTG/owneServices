using System;
using System.ComponentModel;
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
	public class DisclaimApplicablePGAsActionMethod : OperationalActionMethod
	{
		public DisclaimApplicablePGAsActionMethod()
			: base(new Guid("f1ee2282-3380-4e1d-a12d-2a77411cc4e7"))
		{
		}

		public override string Name => DisclaimApplicablePGAsHelper.DisclaimApplicablePGAsMenuCaption;

		public override string Description => Res.GetString("41fae299-9add-429b-89dc-534307711f7f", "Disclaim PGAs for all selected Low Value bills");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new DisclaimApplicablePGAsApplicator(factory);
		}

		public override bool HasControl => true;

		public override IComponent NewGuiControl() => new DisclaimApplicablePGAsUserControl();

		public override bool HasSettings => false;
	}
}
