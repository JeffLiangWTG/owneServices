using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	[TestedType(typeof(PreviousDeclarationsDocWrapperCollection))]
	public class EmanifDocWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PreviousDeclarationsDocWrapperCollection>
	{
		protected override PreviousDeclarationsDocWrapperCollection GetCollectionToTest()
		{
			return new PreviousDeclarationsDocWrapperCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PreviousDeclarationsDocWrapper(null, null, CargoWise.Types.ZString.Empty);
		}
	}
}
