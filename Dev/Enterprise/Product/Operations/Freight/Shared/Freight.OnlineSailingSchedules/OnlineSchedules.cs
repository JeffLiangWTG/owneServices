using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public class OnlineSchedules : NonPersistentBusinessObject
	{
		readonly IRoutesProvider routesProvider;

		public OnlineSchedules(BusinessObjectFactory factory, IRoutesProvider routesProvider)
			: base(factory)
		{
			Argument.NotNull(factory, "factory");
			Argument.NotNull(routesProvider, "routesProvider");
			this.routesProvider = routesProvider;
		}

		public OnlineSchedulesFilterRequest Request { get; set; }

		[ChildEditable]
		public RoutesCollection Routes
		{
			get
			{
				if (routes == null)
				{
					routes = new RoutesCollection(Factory);
					RegisterEditableChildObject(routes);
				}

				return routes;
			}
		}
		RoutesCollection routes;

		protected override void RunPreSaveValidationCore()
		{
			Routes.RunPreSaveValidation();
		}

		public void LoadRoutes(INotifications notifications)
		{
			if (Request == null)
			{
				return;
			}

			Routes.RemoveAndDeleteAll();

			routesProvider.GetRoutes(UrlHelper.ConvertToParams(Request), new UserInitiatedServiceRequestManager(notifications))
				.ForEach(Routes.Add);

			Routes.ReSort();
		}

		public OnlineSchedules CloneSelectedSchedules(Route[] selectedRoutes, BusinessObjectFactory factoryToUse = null)
		{
			Argument.NotNull(selectedRoutes, "selectedRoutes");

			var newFactory = factoryToUse ?? new BusinessObjectFactory();
			var onlineSchedules = new OnlineSchedules(newFactory, new RoutesProvider(newFactory));
			onlineSchedules.Routes.AddRange(selectedRoutes.Select(x => x.Clone(newFactory)));

			return onlineSchedules;
		}

		public void CreateEnterpriseVoyages()
		{
			if (HasErrors)
			{
				return;
			}

			foreach (Route route in Routes)
			{
				route.Legs.ForEach(x => ((Leg)x).CreateEnterpriseVoyage());
			}
		}
	}
}
