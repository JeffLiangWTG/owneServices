using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ViewLocationCollection))]
	sealed class ViewLocationCollectionTest : ActiveBusinessObjectCollectionTestCase<ViewLocationCollection>
	{
		protected override ViewLocationCollection GetCollectionToTest()
		{
			var collection = new ViewLocationCollection(Factory);
			collection.AdditionalFilter = new ZQuery(ViewLocationSchema.VLO_CountryCode, "ZZ");
			return collection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var element = (ViewLocation)base.GetNewElementToAddToTheCollection();
			element.VLO_CountryCode = "ZZ";
			return element;
		}
	}
}
