using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public static class OrgDeactivateHelper
	{
		public static void SecurityCheckAndDeactivateOrganization(IBusiness businessEntity, OrgHeader organisation, Action deactivateOrganisation)
		{
			var companyProxy = organisation.CompanyProxies(false);
			if (!Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed)
			{
				Globals.Message.ShowInformation(Res.GetString("9d9816fa-f029-410d-b4b5-3aaf99fd1aa9", "You do not have rights to activate/deactivate organizations."));
			}
			else if (!organisation.SecurityProvider.CanEditGlobalSupplier)
			{
				Globals.Message.ShowInformation(Res.GetString("5c139218-8c2c-4fa1-9162-69846f4e6b3e", "You do not have rights to modify organizations flagged as Global Supplier."));
			}
			else if (!organisation.OH_IsActive)
			{
				Globals.Message.ShowInformation(Res.GetString("462ede3a-b2bf-4775-83c0-9177acc6e521", "Organization is already inactive."));
			}
			else if (businessEntity.HasChanges)
			{
				Globals.Message.ShowInformation(Res.GetString("fd25281f-d86e-4285-a451-b59e14388e0f", "Save or cancel changes before deactivating organization."));
			}
			else if (companyProxy.Any())
			{
				Globals.Message.ShowInformation(Res.GetString("B6B31B0D-95AB-46DD-A937-4CB36F841C43", "You cannot De-activate this Organization because one or more companies use it as a proxy ({0}).", companyProxy.Select(x => x.GC_Code).Aggregate((x, y) => { return x + ", " + y; })));
			}
			else
			{
				var activeTransactionCollection = organisation.CompanyData.GetActiveTransactionDetails(true);
				if (activeTransactionCollection.Count > 0)
				{
					Globals.Message.ShowInformation(Res.GetString("e151f794-23df-4bdd-aa53-bd477c66aa59", @"You cannot De-activate this organization as there are active AR and/or AP transactions in the following system companies.
{0}", organisation.CompanyData.ConvertActiveTransactionCollectionToString(activeTransactionCollection)));
					return;
				}

				if (organisation.OH_ScreeningStatus == ScreeningStatusesList.Codes.Unknown || organisation.OH_ScreeningStatus == ScreeningStatusesList.Codes.NotScreened || organisation.OH_ScreeningStatus == ScreeningStatusesList.Codes.PermanentClear)
				{
					deactivateOrganisation();
				}
				else
				{
					var dialogResult = Globals.Message.Show(Res.GetString("5751c5eb-6d64-454c-93cc-5d0296446d99", @"Setting the organization to inactive will remove this entity from the Denied Party re-screening process and reset the screening status to UNK.
Would you like to continue?"),
							Res.GetString("07306b01-3ca9-4601-8f97-1c48e47d53c8", "Continue?"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (dialogResult == DialogResult.Yes)
					{
						deactivateOrganisation();
					}
				}
			}
		}
	}
}
