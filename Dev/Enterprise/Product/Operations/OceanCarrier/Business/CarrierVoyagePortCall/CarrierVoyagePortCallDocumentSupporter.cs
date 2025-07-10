using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierVoyagePortCallDocumentSupporter : DocumentSupporter
	{
		public CarrierVoyagePortCallDocumentSupporter(CarrierVoyagePortCall carrierVoyagePortCall)
			: base(carrierVoyagePortCall)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.OCSVoyagePortCall; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);

			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Constants.DataContext.CarrierVoyage, BusinessObject) };
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.CarrierVoyagePortCall };
		}

		#region SecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
