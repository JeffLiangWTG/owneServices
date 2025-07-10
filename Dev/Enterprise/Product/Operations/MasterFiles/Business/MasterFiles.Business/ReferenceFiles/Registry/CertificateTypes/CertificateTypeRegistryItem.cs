using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public sealed class CertificateTypeRegistryItem : TranslatableRegistryItem<CertificateTypeCollection, CertificateTypeCollection>
	{
		public CertificateTypeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CertificateTypeCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CertificateTypeRegistryDataType(defaultValue), storage, RegistryOptions.NotCached))
		{
			localDefaultValue = defaultValue;
		}

		public override bool IsTranslatable => true;
		readonly CertificateTypeCollection localDefaultValue;

		public override IEnumerable<ResourceString> DefaultStrings => localDefaultValue.OfType<CertificateType>().Select(cert => cert.DescriptionMultilingual).OfType<ResourceString>();

		public override int MaxLength => CertificateType.Schema.DescriptionMaxLength;

		public override IEnumerable<string> GetCaptions(CertificateTypeCollection value)
		{
			foreach (CertificateType item in value)
			{
				yield return item.Description;
			}
		}

		protected override CertificateTypeCollection Convert(CertificateTypeCollection value)
		{
			foreach (CertificateType item in value)
			{
				item.DescriptionMultilingual = GetMultilingualString(item.Description);
			}
			return value;
		}
	}
}
