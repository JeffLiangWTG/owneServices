using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface ICusReferenceTypeSupporter
	{
		IDictionary<ZString, Type> GetCusReferenceTypes();
	}
}
