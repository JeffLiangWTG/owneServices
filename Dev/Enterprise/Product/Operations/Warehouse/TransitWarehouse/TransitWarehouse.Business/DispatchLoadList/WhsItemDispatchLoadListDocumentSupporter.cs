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
	public class WhsItemDispatchLoadListDocumentSupporter : DocumentSupporter
	{
		public WhsItemDispatchLoadListDocumentSupporter(WhsItemDispatchLoadList dispatchLoadList) : base(dispatchLoadList)
		{
		}

		#region BusinessContext

		public override BusinessContext BusinessContext => BusinessContext.TransitDspLoadList;

		#endregion

		#region SecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.WhsItemDispatchLoadListCustomizeDocuments;

		#endregion

		#region GetDocumentWrappersInternal

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			if (dataContext == Constants.DataContext.GenericFreightJob && commandBeingRun != null)
			{
				result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, WhsItemDispatchLoadList);
			}

			return result;
		}

		#endregion

		#region ShowReasonForNotPrinting

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		#endregion

		#region GetSupportedDataContexts

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.GenericFreightJob };
		}

		#endregion

		#region GetChildCollection

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommand)
		{
			IDocumentSupportable[] result;
			switch (businessContext)
			{
				case BusinessContext.TransitDspConsignmnt:
					result = WhsItemDispatchLoadList.PackageStates
						.Select(p => p.DispatchConsignment)
						.Where(c => c != null)
						.OrderBy(c => string.IsNullOrWhiteSpace(c.HouseBillNumber) ? c.WDC_ConsignmentID : c.HouseBillNumber)
						.Distinct()
						.ToArray<IDocumentSupportable>();
					break;
				case BusinessContext.TransitDispTranspUnt:
					result = WhsItemDispatchLoadList.DispatchTransportationUnits.ToArray<IDocumentSupportable>();
					break;
				default:
					result = base.GetChildCollection(menuToBeRun, BusinessContext, childCommand);
					break;
			}

			return result;
		}

		#endregion

		#region SupportedChildBusinessContexts

		public override BusinessContext[] SupportedChildBusinessContexts => new[] { BusinessContext.TransitDspConsignmnt, BusinessContext.TransitDispTranspUnt };

		#endregion

		#region WhsItemDispatchLoadList

		protected WhsItemDispatchLoadList WhsItemDispatchLoadList
		{
			get { return (WhsItemDispatchLoadList)BusinessObject; }
		}

		#endregion

	}
}
