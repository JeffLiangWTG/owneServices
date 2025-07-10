using System;
using System.Text.RegularExpressions;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.DataRegistry.Business
{
	public class LinkToZABrokerageRegistryItem : StronglyTypedRegistryItem<string>
	{
		public LinkToZABrokerageRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new LinkToZABrokerageImpl(name, category, caption, hint, storage))
		{
		}

		class LinkToZABrokerageImpl : RegistryItemImpl
		{
			public LinkToZABrokerageImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
				: base(name, category, caption, hint, new LinkToZABrokerageOverrideRegistryDataType(), storage)
			{
			}
		}
	}

	public class LinkToZABrokerageOverrideRegistryDataType : StringRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			var regex = new Regex(@"(http|ftp|https):\/\/.+$");
			if (!regex.IsMatch(proposedValue))
			{
				throw new RegistryValidationException(ZA.Business.Res.GetString("D65992EB-E47E-43C7-BBE1-6A700F87DE52", "It should be entered in the following format: {0}", "http://{SERVER_NAME}/cClearing/CWLink"));
			}
		}
	}
}
