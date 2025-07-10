using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportConsignment.Business
{
	public class BookingConsignmentJobServiceDocumentSupporter : DocumentSupporter
	{
		public BookingConsignmentJobServiceDocumentSupporter(BookingConsignmentJobService service)
			: base(service)
		{
		}

		protected internal BookingConsignmentJobService Service
		{
			get { return (BookingConsignmentJobService)BusinessObject; }
		}

		#region Overrides

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[]
			{
				Constants.DataContext.ConsignmentJobService,
				Constants.DataContext.GenericFreightJobServices
			};
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CnsgmentJobService; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result;

			if (dataContext == Constants.DataContext.GenericFreightJob)
			{
				result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Service.RequestForServiceParent, Service);
			}
			else
			{
				var documentSupportable = Service.RequestForServiceParent as IDocumentSupportable;
				if (documentSupportable != null)
				{
					var documentWrappers = documentSupportable.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Service, null);
					var wrapper = DocumentWrapperFactory.CreateServiceWrapperWithParent(Service, documentWrappers[0]);
					result = new DocumentWrapper[] { wrapper };
				}
				else
				{
					result = System.Array.Empty<DocumentWrapper>();
				}
			}

			return result;
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var message = base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);

			switch (dataContextValue.DataContext)
			{
				case Constants.DataContext.ConsignmentJobService:
				case Constants.DataContext.GenericFreightJobServices:
					message = Res.GetString("GetBODocDataProvidersNotFoundMessage-NoBookingConsignmentJobService", "Cannot find Booking Consignment Services.");
					break;
				case Constants.DataContext.GenericFreightJob:
					message = Res.GetString("GetBODocDataProvidersNotFoundMessage-NoBookingConsignment", "Cannot find Booking Consignment.");
					break;
				default:
					break;
			}

			return message;
		}

		#endregion
	}
}
