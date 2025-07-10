using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class OrgContactActivator : BusinessObjectActivator
	{
		protected override void AdditionalActivateDeactivateAction(bool activate, BusinessObject bizO)
		{
			var contact = (OrgContact)bizO;
			contact.CancelWebAccessSuperseded();
		}

		protected override bool RequiresAdditionalAction(bool activate, BusinessObject bizO)
		{
			return !activate && bizO is OrgContact contact && contact.WebAccessSuperseded;
		}
	}
}
