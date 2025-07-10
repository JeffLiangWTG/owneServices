using System.Collections.Generic;
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
	class CartageWorkSheetDocumentSupporter : DocumentSupporter
	{
		public CartageWorkSheetDocumentSupporter(CommonWorkSheet workSheet)
			: base(workSheet)
		{
		}

		protected CommonWorkSheet CommonWorkSheet
		{
			get { return (CommonWorkSheet)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.JobCartageRunSheet; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.TransportCustomiseDocuments; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			switch (dataContext)
			{
				case Constants.DataContext.GenericFreightJob:
					{
						return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);
					}
				case Constants.DataContext.GenericLocalTransportLeg:
					{
						DocumentCartageLegOptions options = new DocumentCartageLegOptions(new DocumentCartageLegCollection(CommonWorkSheet.CartageLegs));
						DocumentCartageLegEventArgs eventArg = new DocumentCartageLegEventArgs(options);

						CommonWorkSheet.RaiseOnGetCartageLegsToPrint(eventArg);

						DocumentWrapper[] wrappers = System.Array.Empty<DocumentWrapper>();
						if (eventArg.ContinueToPrint)
						{
							wrappers = GetDocumentWrappersForSelectedCartageLegs(eventArg.DocumentCartageLegOptions.CartageLegs, true);
						}
						return wrappers;
					}
				case Constants.DataContext.CartageAdvice:
					{
						DocumentCartageLegOptions options = new DocumentCartageLegOptions(new DocumentCartageLegCollection(CommonWorkSheet.CartageLegs));
						DocumentCartageLegEventArgs eventArg = new DocumentCartageLegEventArgs(options);

						CommonWorkSheet.RaiseOnGetCartageLegsToPrint(eventArg);

						DocumentWrapper[] wrappers = System.Array.Empty<DocumentWrapper>();
						if (eventArg.ContinueToPrint)
						{
							wrappers = GetDocumentWrappersForSelectedCartageLegs(eventArg.DocumentCartageLegOptions.CartageLegs, false);
						}
						return wrappers;
					}
				case Constants.DataContext.CommonWorkSheet:
				case Constants.DataContext.DocumentDailyWorkSheet:
					{
						DocumentWrapper wrapper = DocumentWrapperFactory.CreateWrapper(Constants.DataContext.CommonWorkSheet,
							(CommonWorkSheet)BusinessObject);
						return new DocumentWrapper[] { wrapper };
					}
				default:
					return null;
			}
		}

		DocumentWrapper[] GetDocumentWrappersForSelectedCartageLegs(DocumentCartageLegCollection documentCartageLegCollection, bool createUsingGenerics)
		{
			List<DocumentWrapper> wrappers = new List<DocumentWrapper>();
			foreach (DocumentCartageLeg documentCartageLeg in documentCartageLegCollection)
			{
				if (documentCartageLeg.PrintCartageLeg)
				{
					if (createUsingGenerics)
					{
						DocumentWrapper[] genericCartageLegWrappers = DocumentWrapperFactory.GenerateGenericWrappers(Enterprise.Core.Constants.DataContext.GenericFreightJob, documentCartageLeg.CartageLeg.Cartage, documentCartageLeg.CartageLeg);
						if (genericCartageLegWrappers != null)
						{
							foreach (DocumentWrapper legWrapper in genericCartageLegWrappers)
							{
								wrappers.Add(legWrapper);
							}
						}
					}
					else
					{
						wrappers.Add(DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.CommonCartageLeg, documentCartageLeg.CartageLeg));
					}
				}
			}

			return wrappers.ToArray();
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[]
			{
				Constants.DataContext.CartageAdvice,
				Constants.DataContext.CommonWorkSheet,
				Constants.DataContext.DocumentDailyWorkSheet,
				Constants.DataContext.GenericFreightJob,
			};
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = null;

			if (contact == ContactType.LocalTransport && CommonWorkSheet.TransportCo != null)
			{
				result = new OrgHeaderContact(CommonWorkSheet.TransportCo, null);
			}
			else
			{
				result = base.GetContactOrganisation(menuName, contact, direction);
			}

			return result;
		}
	}
}
