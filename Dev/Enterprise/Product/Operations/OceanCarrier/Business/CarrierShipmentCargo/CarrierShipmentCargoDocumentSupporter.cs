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
	public sealed class CarrierShipmentCargoDocumentSupporter : DocumentSupporter
	{
		public CarrierShipmentCargoDocumentSupporter(CarrierShipmentCargo carrierShipmentCargo)
			: base(carrierShipmentCargo)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CarrierShipmentCargo; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);

			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Constants.DataContext.CarrierShipmentCargo, BusinessObject) };
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.CarrierShipmentCargo };
		}

		#region SecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
