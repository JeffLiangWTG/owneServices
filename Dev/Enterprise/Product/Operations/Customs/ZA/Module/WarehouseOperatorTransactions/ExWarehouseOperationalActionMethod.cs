using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.Module
{
	class ExWarehouseOperationalActionMethod : OperationalActionMethod
	{
		public ExWarehouseOperationalActionMethod() : base(new ZGuid("00F20FFF-30AD-4E3C-B8B6-875858CCEB22"))
		{
		}

		public override string Name => "Ex-Warehouse (Home Consumption)";

		public override string Description => "Ex-Warehouse (Home Consumption)";

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new ExWarehouseApplicator(factory,
				() =>
				Globals.Message.Show(
					"Please click 'Yes' to confirm that an Ex-bond Home Consumption job must be generated.\nClick 'No' to cancel.",
					"Generate Ex-Bond Home Consumption Jobs", ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question, ZDialogResult.No
				) == ZDialogResult.Yes,
				(processLockedInfoText) => Globals.Message.Show(processLockedInfoText, "Generate Ex-Bond Home Consumption Jobs", ZMessageBoxButtons.OK, ZMessageBoxIcon.Warning));
		}

		public override bool RunWithoutUI => true;
	}
}
