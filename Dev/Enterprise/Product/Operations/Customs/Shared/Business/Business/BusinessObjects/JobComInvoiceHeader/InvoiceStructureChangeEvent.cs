using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class InvoiceStructureChangeEvent : IService
	{
		protected InvoiceStructureChangeEvent(BusinessObjectFactory factory)
		{
		}

		public static void AddInvoiceStructureChangedEventHandler(BusinessObjectFactory factory, EventHandler eventHandler)
		{
			Get(factory).InvoiceStructureChanged += eventHandler;
		}

		public static void RemoveInvoiceStructureChangedEventHandler(BusinessObjectFactory factory, EventHandler eventHandler)
		{
			InvoiceStructureChangeEvent instance = factory.ServiceContainer.GetService<InvoiceStructureChangeEvent>();

			if (instance != null)
			{
				instance.InvoiceStructureChanged -= eventHandler;
				if (instance.InvoiceStructureChanged == null)
				{
					factory.ServiceContainer.RemoveService<InvoiceStructureChangeEvent>();
				}
			}
		}

		static InvoiceStructureChangeEvent Get(BusinessObjectFactory factory)
		{
			InvoiceStructureChangeEvent result = factory.ServiceContainer.GetService<InvoiceStructureChangeEvent>();

			if (result == null)
			{
				result = new InvoiceStructureChangeEvent(factory);
				factory.ServiceContainer.AddService(result);
			}

			return result;
		}

		public static void OnInvoiceStructureChanged(BusinessObjectFactory factory)
		{
			InvoiceStructureChangeEvent instance = Get(factory);

			if (instance.InvoiceStructureChanged != null)
			{
				instance.InvoiceStructureChanged(instance, EventArgs.Empty);
			}
		}

		event EventHandler InvoiceStructureChanged;
	}

	public class InvoiceDeleteEvent : IService
	{
		protected InvoiceDeleteEvent(BusinessObjectFactory factory)
		{
		}

		public static void AddInvoiceDeletedEventHandler(BusinessObjectFactory factory, EventHandler eventHandler)
		{
			Get(factory).InvoiceDeleted -= eventHandler;
			Get(factory).InvoiceDeleted += eventHandler;
		}

		public static void RemoveInvoiceDeletedEventHandler(BusinessObjectFactory factory, EventHandler eventHandler)
		{
			Get(factory).InvoiceDeleted -= eventHandler;
		}

		static InvoiceDeleteEvent Get(BusinessObjectFactory factory)
		{
			InvoiceDeleteEvent result = factory.ServiceContainer.GetService<InvoiceDeleteEvent>();

			if (result == null)
			{
				result = new InvoiceDeleteEvent(factory);
				factory.ServiceContainer.AddService(result);
			}

			return result;
		}

		public static void OnInvoiceDeleted(BusinessObjectFactory factory)
		{
			InvoiceDeleteEvent instance = Get(factory);

			if (instance.InvoiceDeleted != null)
			{
				instance.InvoiceDeleted(instance, EventArgs.Empty);
			}
		}

		event EventHandler InvoiceDeleted;
	}
}
