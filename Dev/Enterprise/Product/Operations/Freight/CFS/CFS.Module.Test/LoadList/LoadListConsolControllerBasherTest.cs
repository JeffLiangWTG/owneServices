using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Module.Testing
{
	[TestedType(typeof(LoadListConsolController))]
	sealed class LoadListConsolControllerBasherTest : ZControllerBasherTest
	{
		public void TestCFSContextService()
		{
			AssertEquals("CFS controllers must set CFSContextService on factory", FreightDomainContext.CFS, Controller.Factory.GetFreightDomainContext());
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			Factory.Save();
			return loadList;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.LoadListConsol;
		}

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var loadList = base.GetBusinessObjectWithoutValidationErrors() as CFSLoadListConsol;
			loadList.JK_AgentType = Core.Constants.AgentType.Agent;
			loadList.JK_TransportMode = Core.Constants.TransportModes.Sea;
			loadList.JK_ConsolMode = Core.Constants.ContainerModes.BreakBulk;
			loadList.JK_RL_NKLoadPort = "AUSYD";
			loadList.JK_RL_NKDischargePort = "DEFRA";
			loadList.JK_MasterBillNum = "12345678905";
			loadList.Transports[0].JW_TransportMode = Core.Constants.TransportModes.Air;
			loadList.Transports[0].JW_TransportType = "FL1";
			loadList.Transports[0].JW_ETA = DateTime.Today;
			loadList.Transports[0].JW_ETD = DateTime.Today;
			loadList.Transports[0].JW_VoyageFlight = "AA123";
			var forwarder = Factory.New<OrgHeader>();
			forwarder.OH_IsDebtor = true;
			forwarder.OH_Code = "FWD";
			loadList.JK_OH_Forwarder = forwarder.PK;
			var currentCompany = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			currentCompany.OH_IsMiscFreightServices = true;
			currentCompany.OH_IsPackDepot = true;
			currentCompany.OH_IsUnpackDepot = true;
			loadList.JK_OA_DepotAddress = currentCompany.MainAddress.PK;
			loadList.DepotPK = currentCompany.PK;
			Factory.Save();
			return loadList;
		}

		#endregion
	}
}
