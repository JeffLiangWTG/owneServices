using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IAdditionalReferenceNumberValidationProvider
	{
		void ValidateEntryType(ZPropertyInfo entryTypeInfo, ZString category, ZString countryCode);
		bool EntryTypeShouldBeUnique(ZString entryType, ZString category, ZString countryCode);
	}
}