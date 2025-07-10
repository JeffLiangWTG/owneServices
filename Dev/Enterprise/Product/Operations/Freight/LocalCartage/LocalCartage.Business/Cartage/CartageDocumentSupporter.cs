using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.LocalCartage.Business
{
	public partial class CartageDocumentSupporter : DocumentSupporter
	{
		public CartageDocumentSupporter(CommonCartage commonCartage)
			: base(commonCartage)
		{
		}

		protected CommonCartage CommonCartage
		{
			get { return (CommonCartage)BusinessObject; }
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[]
					{
						Constants.DataContext.ContainerLeg,
						Constants.DataContext.Container,
						Constants.DataContext.Cartage,
						Constants.DataContext.CombinedCartageAdvice,
						Constants.DataContext.CartageAdvice,
						Constants.DataContext.TimeSlotRequest,
						Constants.DataContext.CommonContainer,
						Constants.DataContext.GenericFreightJob,
						Constants.DataContext.GenericFreightJobByContainerIfFCL,
						Constants.DataContext.GenericFreightJobByContainer
					};
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Cartage; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.TransportCustomiseDocuments; }
		}

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommand)
		{
			//No longer need to use child menu's for Containers
			return new[] { CommonCartage };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			switch (dataContext)
			{
				case Constants.DataContext.GenericFreightJobByContainer:
					{
						return GetGenericContainerWrappers();
					}
				case Constants.DataContext.GenericFreightJobByContainerIfFCL:
					{
						if (CommonCartage.IsMixed)
						{
							var list = new List<DocumentWrapper>();
							list.AddRange(GetGenericLoosJobsWrappers(false));
							list.AddRange(GetGenericContainerWrappers());
							return list.ToArray<DocumentWrapper>();
						}
						else if (CommonCartage.IsContainerised)
						{
							return GetGenericContainerWrappers();
						}
						else
						{
							return GetGenericLoosJobsWrappers(false);
						}
					}
				case Constants.DataContext.GenericFreightJob:
					{
						if (CommonCartage.IsContainerised && commandBeingRun.SU_MenuName.Contains((NoResString)"Cartage Advice", StringComparison.Ordinal) && !commandBeingRun.SU_MenuName.Contains("Multi-Container", StringComparison.Ordinal))
						{
							return GetGenericContainerWrappers();
						}

						if (CommonCartage.IsLoose && commandBeingRun.SU_MenuName.Contains((NoResString)"Cartage Advice", StringComparison.Ordinal) && !commandBeingRun.SU_MenuName.Contains("Multi-Container", StringComparison.Ordinal))
						{
							return GetGenericLoosJobsWrappers(true);
						}
						return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);
					}
				case Constants.DataContext.ContainerLeg:
				case Constants.DataContext.GenericLocalTransportLeg:
					{
						return GetDocumentWrappersForSelectedCartageLegs(dataContext);
					}
				case Constants.DataContext.CartageAdvice:
					{
						if (CommonCartage.IsContainerised)
						{
							return GetContainerWrappers();
						}
						return new[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.CommonCartage, CommonCartage) };
					}
				case Constants.DataContext.CommonContainer:
				case Constants.DataContext.Container:
					{
						return GetContainerWrappers();
					}
				case Constants.DataContext.Cartage:
				case Constants.DataContext.CombinedCartageAdvice:
					{
						return new[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.CommonCartage, CommonCartage) };
					}
				case Constants.DataContext.TimeSlotRequest:
					{
						return GetContainersWrappersForTimeSlot();
					}

				default:
					return null;
			}
		}

		DocumentWrapper[] GetGenericLoosJobsWrappers(bool groupLegsWithSameInfo)
		{
			var listOfwrappers = new List<DocumentWrapper>();
			var options = new DocumentCartageLegOptions(new DocumentCartageLegCollection(CommonCartage.CartageLegs.Cast<CommonCartageLeg>().Where(l => l.IsLoose), groupLegsWithSameInfo));
			var eventArg = new DocumentCartageLegEventArgs(options);
			CommonCartage.RaiseOnGetNonContainerisedCartageLegsToPrint(eventArg);
			if (eventArg.ContinueToPrint)
			{
				var legsToPrint = eventArg.DocumentCartageLegOptions.CartageLegs.Cast<DocumentCartageLeg>().Where(d => d.PrintCartageLeg);
				if (legsToPrint.Any())
				{
					if (groupLegsWithSameInfo) // new page per leg group
					{
						foreach (DocumentCartageLeg documentCartageLeg in legsToPrint)
						{
							var wrappers = DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, documentCartageLeg.CartageLeg.Cartage);
							var wrapper = wrappers.First();
							((ILegsOverrider)wrapper).SetLegs(documentCartageLeg.GetCartageLegs(), true, true, true);
							SetLegsForTest(wrapper, documentCartageLeg.GetCartageLegs());
							listOfwrappers.Add(wrapper);
						}
					}
					else
					{
						var legs = legsToPrint.Select(l => l.CartageLeg);
						var wrappers = DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, legsToPrint.First().CartageLeg.Cartage);
						var wrapper = wrappers.First();
						((ILegsOverrider)wrapper).SetLegs(legs.ToArray(), false, false, true);
						SetLegsForTest(wrapper, legs.ToArray());
						listOfwrappers.Add(wrapper);
					}
				}
			}

			return listOfwrappers.ToArray();
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocumentDeliveryContact result;

			if (contact == ContactType.LocalTransport)
			{
				result = new OrgHeaderContact(CommonCartage.LocalTransportProvider, CommonCartage.LocalTransportProviderAddress);
			}
			else if (contact == ContactType.LocalClient || contact == ContactType.Receivables)
			{
				result = new OrgHeaderContact(CommonCartage.LocalClient, null);
			}
			else if (contact == ContactType.Consignee)
			{
				var consigneeAddressWithValidOrganisation = CommonCartage.DocAddresses.FindDocAddressesByType(DocAddressType.LocalCartageImporter).Where(a => a.Organisation != null).FirstOrDefault();
				result = consigneeAddressWithValidOrganisation != null ? new OrgHeaderContact(consigneeAddressWithValidOrganisation.Organisation, consigneeAddressWithValidOrganisation.Address) : null;
			}
			else
			{
				result = base.GetContactOrganisation(menuName, contact, direction);
			}

			return result;
		}

		public override IDocAddress GetOverriddenDeliveryDetails(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocAddress result;

			if (contact == ContactType.LocalTransport)
			{
				result = CommonCartage.LocalTransportProviderAddress;
			}
			else
			{
				result = base.GetOverriddenDeliveryDetails(menuName, contact, direction);
			}

			return result;
		}

		public override bool IsImport
		{
			get { return CommonCartage.IsImportOrDestination; }
		}

		DocumentWrapper[] GetGenericContainerWrappers()
		{
			DocumentContainerOptions options = new DocumentContainerOptions(new DocumentContainerCollection(CommonCartage.Containers, Factory));
			DocumentContainerEventArgs eventArg = new DocumentContainerEventArgs(options);
			CommonCartage.RaiseOnGetContainersToPrint(eventArg);

			DocumentWrapper[] wrappers = Array.Empty<DocumentWrapper>();
			if (eventArg.ContinueToPrint)
			{
				wrappers = GetDocumentWrappersForSelectedContainers(eventArg.DocumentContainerOptions.Containers, true);
			}
			return wrappers;
		}

		DocumentWrapper[] GetContainerWrappers()
		{
			DocumentContainerOptions options = new DocumentContainerOptions(new DocumentContainerCollection(CommonCartage.Containers, Factory));
			DocumentContainerEventArgs eventArg = new DocumentContainerEventArgs(options);
			CommonCartage.RaiseOnGetContainersToPrint(eventArg);

			DocumentWrapper[] wrappers = Array.Empty<DocumentWrapper>();
			if (eventArg.ContinueToPrint)
			{
				wrappers = GetDocumentWrappersForSelectedContainers(eventArg.DocumentContainerOptions.Containers, false);
			}
			return wrappers;
		}

		DocumentWrapper[] GetDocumentWrappersForSelectedContainers(DocumentContainerCollection documentContainerCollection, bool createUsingGenerics)
		{
			List<DocumentWrapper> wrappers = new List<DocumentWrapper>();
			foreach (DocumentContainer documentContainer in documentContainerCollection)
			{
				if (documentContainer.PrintContainer)
				{
					if (createUsingGenerics)
					{
						DocumentWrapper[] genericContainerWrappers = DocumentWrapperFactory.GenerateGenericWrappers(Enterprise.Core.Constants.DataContext.GenericFreightJob, CommonCartage, documentContainer.Container);
						if (genericContainerWrappers != null)
						{
							wrappers.AddRange(genericContainerWrappers);
						}
					}
					else
					{
						wrappers.Add(DocumentWrapperFactory.CreateContainerWrapperWithCartage(documentContainer.Container, CommonCartage));
					}
				}
			}

			return wrappers.ToArray();
		}

		DocumentWrapper[] GetDocumentWrappersForSelectedCartageLegs(Constants.DataContext dataContext)
		{
			var cartageLegOptions = new DocumentCartageLegOptions(new DocumentCartageLegCollection(CommonCartage.CartageLegs));
			var eventArg = new DocumentCartageLegEventArgs(cartageLegOptions);
			CommonCartage.RaiseOnGetCartageLegsToPrint(eventArg);

			List<DocumentWrapper> wrappers = new List<DocumentWrapper>();
			if (eventArg.ContinueToPrint)
			{
				foreach (var documentCartageLeg in eventArg.DocumentCartageLegOptions.CartageLegs.Cast<DocumentCartageLeg>().Where(l => l.PrintCartageLeg))
				{
					var docCartageLeg = documentCartageLeg.CartageLeg;
					if (dataContext == Constants.DataContext.GenericLocalTransportLeg)
					{
						var cartageLegWrappers = DocumentWrapperFactory.GenerateGenericWrappers(Constants.DataContext.GenericFreightJob, docCartageLeg.Cartage, docCartageLeg);
						if (cartageLegWrappers != null)
						{
							wrappers.AddRange(cartageLegWrappers);
						}
					}
					else
					{
						wrappers.Add(DocumentWrapperFactory.CreateWrapper(Constants.DataContext.CommonCartageLeg, docCartageLeg));
					}
				}
			}
			return wrappers.ToArray();
		}

		protected DocumentWrapper[] GetContainersWrappersForTimeSlot()
		{
			List<CommonContainer> containersToPrint = new List<CommonContainer>();

			foreach (var container in CommonCartage.Containers)
			{
				if (container.JC_DepartureSlotReferenceInfo.HasChanges ||
				container.JC_DepartureSlotDateTimeInfo.HasChanges ||
				container.JC_ArrivalSlotReferenceInfo.HasChanges ||
				container.JC_ArrivalSlotDateTimeInfo.HasChanges)
				{
					containersToPrint.Add(container);
				}
			}

			List<DocumentWrapper> wrappers = new List<DocumentWrapper>();

			if (containersToPrint.Count != 0)
			{
				foreach (CommonContainer container in containersToPrint)
				{
					wrappers.Add(DocumentWrapperFactory.CreateContainerWrapperWithCartage(container, CommonCartage));
				}
			}
			else
			{
				DocumentContainerOptions options = new DocumentContainerOptions(new DocumentContainerCollection(CommonCartage.Containers, Factory));
				DocumentContainerEventArgs eventArg = new DocumentContainerEventArgs(options);
				CommonCartage.RaiseOnGetContainersToPrint(eventArg);

				if (eventArg.ContinueToPrint)
				{
					return GetDocumentWrappersForSelectedContainers(eventArg.DocumentContainerOptions.Containers, false);
				}
			}
			return wrappers.ToArray();
		}

		partial void SetLegsForTest(DocumentWrapper wrapper, CommonCartageLeg[] legs);
	}
}

#if DEBUG
namespace Enterprise.Freight.LocalCartage.Business
{
	public partial class CartageDocumentSupporter
	{
		partial void SetLegsForTest(DocumentWrapper wrapper, CommonCartageLeg[] legs)
		{
			DocumentWrappersForTest.Add(wrapper, legs);
		}

		public CommonCartageLeg[] GetLegsForTest(DocumentWrapper wrapper)
		{
			CommonCartageLeg[] legs;
			DocumentWrappersForTest.TryGetValue(wrapper, out legs);
			return legs;
		}

		Dictionary<DocumentWrapper, CommonCartageLeg[]> DocumentWrappersForTest
		{
			get { return documentWrappers ?? (documentWrappers = new Dictionary<DocumentWrapper, CommonCartageLeg[]>()); }
		}

		Dictionary<DocumentWrapper, CommonCartageLeg[]> documentWrappers;
	}
}
#endif
