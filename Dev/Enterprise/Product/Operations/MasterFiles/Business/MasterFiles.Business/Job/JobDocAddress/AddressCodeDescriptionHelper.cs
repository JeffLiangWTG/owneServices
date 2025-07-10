using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class AddressCodeDescriptionHelper : IAddressCodeDescriptionHelper
	{
		public AddressCodeDescriptionHelper(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "factory");
		}

		readonly BusinessObjectFactory factory;

		public string GetAddressDescriptionFromCode(string code)
		{
			var pairList = DocAddressTypes.GetAddressTypesCodeDescriptionPairList(factory);
			return pairList.GetDescriptionFromCode(code) ?? DocAddressTypes.Unspecified;
		}
	}
}
