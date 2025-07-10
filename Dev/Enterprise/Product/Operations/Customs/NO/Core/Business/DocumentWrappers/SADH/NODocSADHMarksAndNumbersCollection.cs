using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.NO.Business;

public sealed class NODocSADHMarksAndNumbersCollection(BusinessObjectFactory factory) : DocBaseWrapperCollection<NODocSADHMarksAndNumbers>(factory)
{
	public void AddIfValueIsDistinct(ZString valueToAdd)
	{
		if (values.Add(valueToAdd))
		{
			Add(new NODocSADHMarksAndNumbers(valueToAdd, Factory));
		}
	}
	readonly HashSet<string> values = [];
}
