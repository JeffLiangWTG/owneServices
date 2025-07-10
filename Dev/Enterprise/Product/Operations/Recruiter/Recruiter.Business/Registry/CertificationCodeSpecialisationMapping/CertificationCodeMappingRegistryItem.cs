using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	public class CertificationCodeMappingRegistryItem : StronglyTypedRegistryItem<CertificationCodeMappingCollection>
	{
		public CertificationCodeMappingRegistryItem(string name, MultilingualString category)
			: base(new RegistryItemImpl(
				name,
				category,
				ResString.GetMultilingualString("43BC694C-39B7-4531-A22A-0C556BB450DA", "Certificate Code Mappings"),
				ResString.GetMultilingualString("FE82FF62-63C3-4BC2-A5AB-7133A9963854", "Mappings between certificate codes and related specialization codes"),
				new CertificationCodeMappingDataType(),
				RegistryStorageFlags.System,
				RegistryOptions.NotCached))
		{
		}
	}
}
