using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Module
{
	public class AgencyBookingFilterStrip : AgencyShipmentFilterStrip
	{
		#region Implementation

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			SecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);
			return filters;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			helpers.Add(new WorkflowFilterStripsHelper(typeof(AgencyBooking), WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode, Factory));

			return helpers;
		}

		protected override ZBool AllowSearchOfUnlocoOutsideLoginBranch
		{
			get { return Env.Security.AgencyBookingAllowSearchOfUnlocoOutsideLoginBranch.IsAllowed; }
		}

		protected override CodeDescriptionPairList NewShipmentStatusList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair("UCF", ResString.GetMultilingualString("2120a5a3-d1fa-4e20-a14e-b67d7b160c17", "Unconfirmed"));
			result.AddRange(new AgencyShipmentStatusList(false));
			return result;
		}

		protected override ZString DefaultShipmentStatusFilter
		{
			get { return "UCF"; }
		}

		protected override SecurityCheckpoint JobInvoicingSecurity
		{
			get { return Env.Security.AgencyBookingJobInvoicing; }
		}

		readonly AgencyBookingCRMSecurityProvider SecurityProvider = new AgencyBookingCRMSecurityProvider();

		#endregion
	}
}


