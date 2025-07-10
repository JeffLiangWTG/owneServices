using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using DefaultOptions = Enterprise.Core.Constants.Customs.ASNRefreshDefaultsOptions;

namespace Enterprise.MasterFiles.Business
{
	public class IMProductValueDefaultOptionCollection : NonPersistentBusinessObjectCollection<IMProductValueDefaultOption>
	{
		public IMProductValueDefaultOptionCollection(OrgCompanyData companyData) : base(companyData.Factory)
		{
			master = companyData;
		}
		readonly OrgCompanyData master;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new IMProductValueDefaultOption(Factory);
		}

		public void AddProductValueDefaultOptions(List<ZString> productValueDefaultOptionList)
		{
			foreach (var productValueDefaultOption in productValueDefaultOptionList)
			{
				if (!productValueDefaultOption.IsEmpty)
				{
					Add(new IMProductValueDefaultOption(productValueDefaultOption, Factory));
				}
			}
		}

		public ZString GetDefaultOptionsString()
		{
			var list = new List<ZString>();
			if (master.ImporterOverride)
			{
				list.Add(DefaultOptions.Codes.Override);
			}
			list.AddRange(this.Cast<IMProductValueDefaultOption>().Select(x => x.FieldType));
			return ZString.Join(",", list.ToArray());
		}
	}
}
