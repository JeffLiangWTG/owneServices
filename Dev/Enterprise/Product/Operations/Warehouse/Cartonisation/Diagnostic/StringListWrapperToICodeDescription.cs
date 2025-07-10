using System;
using System.Collections.Generic;
using CargoWise.Integration;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic
{
	public class StringListWrapperToICodeDescription : List<ICodeDescription>
	{
		// ZDropEditColumnStyleInfo work only with lists which items are ICodeDescription. So we need this ugly wrapper.
		public StringListWrapperToICodeDescription(IEnumerable<string> values)
		{
			foreach (var value in values)
			{
				Add(new ICodeDescriptionForString(value));
			}
		}

		class ICodeDescriptionForString : ICodeDescription
		{
			public ICodeDescriptionForString(string value)
			{
				PK = Guid.NewGuid();
				Code = value;
				Description = value;
			}

			public object PK { get; private set; }
			public string Code { get; private set; }
			public string Description { get; private set; }
		}
	}
}

