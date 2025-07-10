using System;
using CargoWise.Definitions;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	internal class UniversalCustomsDataCreator<T> : IDataCreator<T>
	{
		public UniversalCustomsDataCreator(AssemblyMetaDataAttributeWithType attribute)
		{
			instance = new Lazy<T>(() =>
				(T)attribute
					.Type
					.GetConstructor(Array.Empty<Type>())
					.Invoke(null));
		}

		public T Create() => instance.Value;

		readonly Lazy<T> instance;
	}
}
