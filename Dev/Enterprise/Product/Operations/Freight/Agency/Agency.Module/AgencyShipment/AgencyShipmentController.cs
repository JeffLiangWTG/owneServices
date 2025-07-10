using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module
{
	public abstract class AgencyShipmentController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			if (((AgencyShipment)sourceEntity).CanDelete)
			{
				return base.ShowDeleteForm(sourceEntity);
			}
			else
			{
				ShowPrincipalSecurityError();
				return null;
			}
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			if (((AgencyShipment)sourceEntity).PrincipalSecurityAllowed)
			{
				return base.ShowEditForm(sourceEntity);
			}
			else
			{
				ShowPrincipalSecurityError();
				return null;
			}
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject sourceEntity)
		{
			if (((AgencyShipment)sourceEntity).PrincipalSecurityAllowed)
			{
				return base.ShowTemplateCopyForm(sourceEntity);
			}
			else
			{
				ShowPrincipalSecurityError();
				return null;
			}
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			if (((AgencyShipment)sourceEntity).PrincipalSecurityAllowed)
			{
				return base.ShowViewForm(sourceEntity);
			}
			else
			{
				ShowPrincipalSecurityError();
				return null;
			}
		}

		void ShowPrincipalSecurityError()
		{
			Globals.Message.ShowError(AgencyShipment.NotAuthorizedForThisPrincipal);
		}
	}
}



