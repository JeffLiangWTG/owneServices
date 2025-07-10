using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(OrgReceiveController))]
	internal class OrgReceiveControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsConfigOrgReceive;
		}

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var orgHeader = base.GetBusinessObjectWithoutValidationErrors() as OrgHeader;
			orgHeader.MainAddress.OA_Address1 = "Test Address 1";
			orgHeader.MainAddress.OA_PostCode = "123456";
			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			orgHeader.OH_FullName = "Test Consignor";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			return orgHeader;
		}
	}
}
