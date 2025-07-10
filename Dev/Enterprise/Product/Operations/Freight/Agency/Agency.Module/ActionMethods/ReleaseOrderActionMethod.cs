
namespace Enterprise.Freight.Agency.Module
{
	using System.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Environment;
	using Enterprise.Freight.Agency.Business;
	using Enterprise.Freight.Agency.GUI;
	using Enterprise.Licensing;
	using Enterprise.Security;
	using Enterprise.Services.OperationalActions.Support;

	internal abstract class ReleaseOrderActionMethod : OperationalActionMethod
	{
		public ReleaseOrderActionMethod(ZGuid methodID)
			: base(methodID)
		{
		}

		public override LicenceCheckpoint[] GetRequiredLicenceCheckpoints()
		{
			return new LicenceCheckpoint[] { Env.Licence.ShippingManagerBillOfLading };
		}

		public override SecurityCheckpoint[] GetRequiredSecurityCheckpoints()
		{
			return new SecurityCheckpoint[] { Env.Security.AgencyBillOfLadingReleaseDeliverOrderMessaging };
		}

		public override bool HasControl
		{
			get { return true; }
		}

		public override IComponent NewGuiControl()
		{
			return new ReleaseImportOrderControl();
		}

		public override bool HasSettings
		{
			get { return true; }
		}

		public override OperationalActionMethodSettings NewSetting(BusinessObjectFactory factory)
		{
			return new ReleaseImportOrderSettings();
		}

		public override IComponent NewSettingsControl()
		{
			return new ReleaseImportOrderSettingsControl();
		}

		public override FilterRequirementList GetFilterRequirements()
		{
			var result = base.GetFilterRequirements();
			result.Add(FilterConstants.IsImportReleaseOrderEnabled, new string[] { "Y" });

			return result;
		}
	}
}
