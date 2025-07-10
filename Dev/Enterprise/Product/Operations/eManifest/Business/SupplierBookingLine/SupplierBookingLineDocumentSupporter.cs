using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.eManifest.Business
{
	public class SupplierBookingLineDocumentSupporter : DocumentSupporter
	{
		#region Constructor

		public SupplierBookingLineDocumentSupporter(SupplierBookingLine supplierBookingLine)
			: base(supplierBookingLine)
		{
		}

		#endregion

		protected SupplierBookingLine SupplierBookingLine
		{
			get { return (SupplierBookingLine)BusinessObject; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, SupplierBookingLine);
			return genericWrappers;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.GenericFreightJob };
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.SupplierBookingLine; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.MaintainShipmentCustomiseDocuments; }
		}

		#region GetAlternativeBranding

		public override ClientAndAgentBrandingBusinessObject GetAlternativeBranding()
		{
			return GetLocalTransportCompanyBranding();
		}

		ClientAndAgentBrandingBusinessObject GetLocalTransportCompanyBranding()
		{
			return DocumentsDataRegistry.Instance.LocalTransportCompanyBrand.FindBrandingForLocalTransportCompany(SupplierBookingLine.DL_OH_LastMileCarrier);
		}

		#endregion
	}
}
