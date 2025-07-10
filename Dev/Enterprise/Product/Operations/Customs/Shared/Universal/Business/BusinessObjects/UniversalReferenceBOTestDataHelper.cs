#if DEBUG
using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	class UniversalReferenceBOTestDataHelper : BusinessObjectTestDataHelper
	{
		protected override void PopulateUniqueString(ZPropertyInfo property, PropertyDescriptor[] propertyPath, int maxLength)
		{
			base.PopulateUniqueString(property, propertyPath, (maxLength >= 2147483647 ? 5000 : maxLength));
		}
	}
}
#endif
