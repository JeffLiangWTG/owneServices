using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveASNDocumentSupporter : DocumentSupporter
	{
		public WhsItemReceiveASNDocumentSupporter(WhsItemReceiveASN transitReceiveExpectedPacking)
			: base(transitReceiveExpectedPacking)
		{
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			if (dataContext == Constants.DataContext.GenericFreightJob)
			{
				result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, TransitReceiveASN);
			}

			return result;
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var message = base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
			switch (dataContextValue.DataContext)
			{
				case Constants.DataContext.GenericFreightJob:
					message = Res.GetString("GetBODocDataProvidersNotFoundMessage-NoReceiveASNs", "Cannot find Warehouse Receive ASNs.");
					break;

				default:
					break;
			}
			return message;
		}

		#region GetChildCollection

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommand)
		{
			IDocumentSupportable[] result;
			switch (businessContext)
			{
				case BusinessContext.TransitRcvConsignmnt:
					result = TransitReceiveASN.PackageStates.Select(p => p.ReceiveConsignment).Where(c => c != null).Distinct().ToArray<IDocumentSupportable>();
					break;
				case BusinessContext.TransitRecTranspUnt:
					result = TransitReceiveASN.ReceiveTransportationUnits.Where(c => c != null).Distinct().ToArray<IDocumentSupportable>();
					break;
				default:
					result = base.GetChildCollection(menuToBeRun, BusinessContext, childCommand);
					break;
			}

			return result;
		}

		#endregion

		#region GetSupportedDataContexts

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.GenericFreightJob };
		}

		#endregion

		#region BusinessContext

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.TransitReceiveASN; }
		}

		#endregion

		#region SupportedChildBusinessContexts

		public override BusinessContext[] SupportedChildBusinessContexts => new[] { BusinessContext.TransitRcvConsignmnt, BusinessContext.TransitRecTranspUnt };

		#endregion

		#region SecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.WhsItemReceiveASNCustomizeDocuments;

		#endregion

		#region TransitReceiveASN

		protected WhsItemReceiveASN TransitReceiveASN
		{
			get { return (WhsItemReceiveASN)BusinessObject; }
		}

		#endregion
	}
}
