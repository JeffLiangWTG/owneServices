using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageLegDocumentSupporter : DocumentSupporter
	{
		public CartageLegDocumentSupporter(CommonCartageLeg cartageLeg)
			: base(cartageLeg)
		{
		}

		public static CartageLegDocumentSupporter New(CommonCartageLeg cartageLeg)
		{
			CartageLegDocumentSupporter result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(cartageLeg);
			}
			else if (cartageLeg != null)
			{
				result = new CartageLegDocumentSupporter(cartageLeg);
			}

			return result;
		}

		protected delegate CartageLegDocumentSupporter NewDelegate(CommonCartageLeg cartageLeg);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		protected CommonCartageLeg CartageLeg
		{
			get
			{
				return (CommonCartageLeg)BusinessObject;
			}
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[]
					{
						Constants.DataContext.CommonCartageLeg,
						Constants.DataContext.ContainerLeg,
						Constants.DataContext.CartageAdvice,
						Constants.DataContext.DocumentDailyWorkSheet,
						Constants.DataContext.GenericFreightJob
					};
		}

		public override BusinessContext BusinessContext
		{
			get
			{
				return BusinessContext.ContainerLeg;
			}
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get
			{
				return Env.Security.TransportCustomiseDocuments;
			}
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			switch (dataContext)
			{
				case Constants.DataContext.CommonCartageLeg:
				case Constants.DataContext.ContainerLeg:
				case Constants.DataContext.CartageAdvice:
					{
						DocumentWrapper cartageLegWrapper = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.CommonCartageLeg, CartageLeg);
						return new DocumentWrapper[] { cartageLegWrapper };
					}
				case Enterprise.Core.Constants.DataContext.GenericFreightJob:
					if (CartageLeg.Cartage != null)
					{
						return DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, CartageLeg.Cartage, CartageLeg);
					}
					break;
			}
			return null;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = null;

			if (contact == ContactType.LocalTransport)
			{
				if (CartageLeg.TransportCo != null)
				{
					result = new OrgHeaderContact(CartageLeg.TransportCo, null);
				}
				else
				{
					result = new OrgHeaderContact(CartageLeg.Cartage.LocalTransportProvider, CartageLeg.Cartage.LocalTransportProviderAddress);
				}
			}
			else if (contact == ContactType.Consignee)
			{
				if (CartageLeg.DeliverToDocAddressType == DocAddressType.LocalCartageImporter)
				{
					result = new OrgHeaderContact(CartageLeg.DeliverOrganisation, CartageLeg.DeliverToDocAddress?.Address);
				}
				else if (CartageLeg.WaitPointDocAddressType == DocAddressType.LocalCartageImporter)
				{
					result = new OrgHeaderContact(CartageLeg.WaitPointOrganisation, CartageLeg.WaitPointDocAddress?.Address);
				}
			}
			else if (contact == ContactType.LocalClient)
			{
				result = new OrgHeaderContact(CartageLeg.Cartage.LocalClient, null);
			}

			return result ?? base.GetContactOrganisation(menuName, contact, direction);
		}
	}
}
