using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(RefundAdditionalMessageInformation))]
	public class REfundAdditionalInformationNonPersistentBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RefundAdditionalMessageInformation(Factory.New<JobDeclaration>(), Factory);
		}
	}
}
