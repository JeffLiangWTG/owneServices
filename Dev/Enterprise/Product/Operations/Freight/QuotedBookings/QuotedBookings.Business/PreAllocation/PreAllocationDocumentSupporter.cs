using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class PreAllocationDocumentSupporter : DocumentSupporter
	{
		public PreAllocationDocumentSupporter(PreAllocation preAllocation)
			: base(preAllocation)
		{
			PreAllocation = preAllocation;
		}

		protected PreAllocation PreAllocation { get; private set; }

		#region BusinessContext

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.PreAllocation; }
		}

		#endregion

		#region GetChildCollection

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommand)
		{
			IDocumentSupportable[] result = null;

			if (businessContext == BusinessContext.Shipment)
			{
				result = PreAllocation.Bookings.Cast<IDocumentSupportable>().ToArray();
			}
			else
			{
				result = base.GetChildCollection(menuToBeRun, businessContext, childCommand);
			}

			return result;
		}

		#endregion

		#region GetSupportedDataContexts

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Core.Constants.DataContext.Shipment };
		}

		public override BusinessContext[] SupportedChildBusinessContexts
		{
			get { return new[] { BusinessContext.Shipment }; }
		}

		#endregion

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return null;
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != Core.Constants.DataContext.Shipment && base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}
	}
}
