using System;

namespace Enterprise.MasterFiles.Business
{
	[System.Diagnostics.DebuggerDisplay("Key = {Key}")]
	public sealed class NumberGeneratorValueProvider : INumberGeneratorValueProvider
	{
		public delegate string Accessor(NumberGenerator generator, string detail);

		public NumberGeneratorValueProvider(string key, Accessor accessor)
		{
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (accessor == null)
			{
				throw new ArgumentNullException(nameof(accessor));
			}

			this.Key = key;
			this.accessor = accessor;
		}

		public string Key { get; private set; }

		public string GetValue(NumberGenerator generator, string detail)
		{
			return accessor(generator, detail);
		}

		readonly Accessor accessor;
	}
}
