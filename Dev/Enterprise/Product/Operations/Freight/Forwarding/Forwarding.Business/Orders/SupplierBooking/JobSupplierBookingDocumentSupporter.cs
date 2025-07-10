using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobSupplierBookingDocumentSupporter : DocumentSupporter
	{
		public JobSupplierBookingDocumentSupporter(JobSupplierBooking supplierBooking) : base(supplierBooking)
		{
		}

		protected JobSupplierBooking SupplierBooking
		{
			get { return (JobSupplierBooking)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.JobSupplierBooking; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.None;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, SupplierBooking);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			var result = System.Array.Empty<DocumentWrapper>();

			switch (dataContext)
			{
				case Core.Constants.DataContext.JobSupplierBooking:
					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.JobSupplierBooking, SupplierBooking) };
					break;
				default:
					break;
			}

			return result;
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != Constants.DataContext.JobSupplierBooking
				&& dataContext != Constants.DataContext.GenericFreightJob
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[]
			{
				Core.Constants.DataContext.JobSupplierBooking,
				Core.Constants.DataContext.GenericFreightJob,
			};
		}
	}
}
