using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDPickupHeaderDocumentSupporter : DocumentSupporter
	{
		public CYDPickupHeaderDocumentSupporter(CYDPickupHeader pickUpHeader)
			: base(pickUpHeader)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CYDPickupHeader; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, PickupHeader);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}
			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(DataContext.CYDPickupHeader, PickupHeader) };
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.CYDPickupHeader };
		}

		#region SecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		#endregion

		#region ReleaseAdvice

		protected CYDPickupHeader PickupHeader
		{
			get { return (CYDPickupHeader)BusinessObject; }
		}

		#endregion
	}
}
