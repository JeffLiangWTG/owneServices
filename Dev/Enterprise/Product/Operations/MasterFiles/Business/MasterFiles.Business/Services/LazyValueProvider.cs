using System;

namespace Enterprise.MasterFiles.Business
{
	public class LazyValueProvider<T>
	{
		public LazyValueProvider()
		{
		}

		public LazyValueProvider(Func<T> valueProvider)
			: this()
		{
			ValueProvider = valueProvider;
		}

		public T Value
		{
			get
			{
				if (!IsValueCreated)
				{
					value = valueProvider != null ? valueProvider() : default(T);
					IsValueCreated = true;
				}

				return value;
			}
		}
		T value;

		public bool IsValueCreated { get; private set; }

		public Func<T> ValueProvider
		{
			get { return valueProvider; }
			set
			{
				if (valueProvider != value)
				{
					valueProvider = value;
					IsValueCreated = false;
				}
			}
		}
		Func<T> valueProvider;
	}
}
