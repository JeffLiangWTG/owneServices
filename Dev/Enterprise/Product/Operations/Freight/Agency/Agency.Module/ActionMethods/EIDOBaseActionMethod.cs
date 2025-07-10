using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Agency.Module
{
	internal abstract class EIDOBaseActionMethod : OperationalActionMethod
	{
		public EIDOBaseActionMethod(ZGuid methodID)
			: base(methodID) { }

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
			FilterRequirementList result = base.GetFilterRequirements();
			result.Add(FilterConstants.Country, new string[] { Constants.CountryCodes.Australia });
			return result;
		}
	}
}
