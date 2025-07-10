using CargoWise.EntityFramework;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.GUI
{
	public class USInBondSecurityAlertHelper
	{
		public USInBondSecurityAlertHelper(CusInBondHeader inBondHeader)
		{
			this.inBondHeader = inBondHeader;
		}

		public ContinueWithSave GetSecurityAlertMessage()
		{
			ContinueWithSave result = ContinueWithSave.Yes;
			if (!inBondHeader.CurrentUserHasBondedWarehouseSecurityAccess && inBondHeader.HasAtLeastOneMovementMarkedForBondedWarhousing)
			{
				Globals.Message.ShowError(Res.GetString("2676E53A-5929-4E55-B720-864109A1AE36", "You do not have security rights to save a Bonded Warehousing job. {0}", Env.Security.USInBondEditBondedWarehouse.DisplayTextPathToSecurityRight));
				result = ContinueWithSave.No;
			}
			return result;
		}

		readonly CusInBondHeader inBondHeader;
	}
}
