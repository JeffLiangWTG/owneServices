using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public interface IHaveServices
	{
		ZString TableCode { get; }
		ZGuid PK { get; }
		JobServiceDependentCollection Services { get; }
		ZString TransportMode { get; }
		ZString ContainerMode { get; }
		IHaveServices[] DependentServiceParents { get; }
		BusinessObject ServiceParent { get; }
		bool NeedsServiceEvents { get; }
		void JobServiceDeleted(ZGuid servicePK);
		IBranch ServiceBranch { get; }
	}

	public interface IHaveEventsFromServices : IHaveServices
	{
		IEnumerable<KeyValuePair<string, string>> ReferenceParameters { get; }
	}

	public interface IHaveServicesWithContext : IHaveServices
	{
		CodeDescriptionPairList ServiceCurrentContextList { get; }
		ZString GetContextTableCode(ZGuid contextID);
	}

	public interface IServicesSelectionProvider
	{
		JobService[] GetServicesToPrint(IHaveServices parent);
	}

	class ServicesSelectionDefaultProvider : IServicesSelectionProvider
	{
		JobService[] IServicesSelectionProvider.GetServicesToPrint(IHaveServices parent)
		{
			JobService[] selectedServices = null;

			if (parent != null && parent.Services != null)
			{
				selectedServices = parent.Services.Cast<JobService>().ToArray();
			}

			return selectedServices;
		}
	}

	public static class IHaveServicesExtensions
	{
		public static DocumentWrapper[] GetServiceWrappers(this IHaveServices servicesProvider, Core.Constants.DataContext dataContext)
		{
			DocumentWrapper parentWrapper = null;

			Func<JobService, DocumentWrapper> documentWrapperCreator = (service) =>
				DocumentWrapperFactory.CreateServiceWrapperWithParent(service, parentWrapper ?? (parentWrapper = DocumentWrapperFactory.CreateWrapper(dataContext, servicesProvider.ServiceParent)));

			return GetServiceWrappers(servicesProvider, documentWrapperCreator);
		}

		public static DocumentWrapper[] GetServiceWrappers(this IHaveServices servicesProvider, Func<JobService, DocumentWrapper> documentWrapperCreator)
		{
			DocumentWrapper[] serviceWrappers = null;

			if (servicesProvider != null && documentWrapperCreator != null)
			{
				IServicesSelectionProvider selectionProvider = GetServicesSelectionProvider(servicesProvider.ServiceParent.Factory);
				JobService[] selectedSevices = selectionProvider.GetServicesToPrint(servicesProvider);

				serviceWrappers = selectedSevices != null ? selectedSevices.Select(documentWrapperCreator).ToArray() : null;
			}

			return serviceWrappers;
		}

		public static DocumentWrapper[] GetServiceWrappersForDocBuilder(this IHaveServices servicesProvider, BusinessObject primaryBusinessObject, Core.Constants.DataContext dataContext)
		{
			Func<JobService, DocumentWrapper> documentWrapperCreator = (service) => CreateGenericWrapper(dataContext, primaryBusinessObject, service);

			return GetServiceWrappers(servicesProvider, documentWrapperCreator);
		}

		static DocumentWrapper CreateGenericWrapper(Core.Constants.DataContext dataContext, BusinessObject parentToWrap, JobService service)
		{
			DocumentWrapper[] wrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, parentToWrap, service);

			return wrappers != null && wrappers.Length > 0 ? wrappers[0] : null;
		}

		static IServicesSelectionProvider GetServicesSelectionProvider(BusinessObjectFactory factory)
		{
			return factory.IsInSaveTransaction
				? new ServicesSelectionDefaultProvider()
				: factory.GetValue<IServicesSelectionProvider>() ?? new ServicesSelectionDefaultProvider();
		}
	}
}
