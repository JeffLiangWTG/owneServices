using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class OrgCountryDataEUStyleTest<T> : OrgCountryDataTest where T : OrgCountryDataEUStyle
	{
		protected abstract IEnumerable<string> CountriesToTest { get; }

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<T>();
		}
	}
}
