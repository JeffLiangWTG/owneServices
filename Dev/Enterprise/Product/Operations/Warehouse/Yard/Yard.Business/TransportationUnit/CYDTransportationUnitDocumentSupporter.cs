using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDTransportationUnitDocumentSupporter : DocumentSupporter
	{
		public CYDTransportationUnitDocumentSupporter(CYDTransportationUnit transportationUnit)
			: base(transportationUnit)
		{
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject) ??
				   new[] { DocumentWrapperFactory.CreateWrapper(DataContext.CYDReceiveAdvice, BusinessObject) };
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new[] { DataContext.CYDTransportationUnit, DataContext.GenericFreightJob };
		}

		#region Transportation Unit

		protected CYDTransportationUnit TransportationUnit
		{
			get { return (CYDTransportationUnit)BusinessObject; }
		}

		#endregion

		#region GetContactOrganisation

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return new DocumentEngine.OrgHeaderContact(TransportationUnit.TransportCompanyDocAddress.Address?.Header, TransportationUnit.TransportCompanyDocAddress.Address);
		}

		#endregion

		public override BusinessContext BusinessContext => BusinessContext.CYDTransportUnit;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.None;
	}
}
