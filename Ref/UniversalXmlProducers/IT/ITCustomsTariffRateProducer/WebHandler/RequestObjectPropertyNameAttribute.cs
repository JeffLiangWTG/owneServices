using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler
{
	public sealed class RequestObjectPropertyNameAttribute : Attribute
	{
		public RequestObjectPropertyNameAttribute(string propertyName)
		{
			PropertyName = propertyName;
		}

		public string PropertyName { get; }

	}
}
