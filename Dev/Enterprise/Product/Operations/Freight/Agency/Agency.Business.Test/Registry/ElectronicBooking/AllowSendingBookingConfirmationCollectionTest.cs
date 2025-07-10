using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AllowSendingBookingConfirmationCollection))]
	public class AllowSendingBookingConfirmationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<AllowSendingBookingConfirmationCollection>
	{
		#region Implementation

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override AllowSendingBookingConfirmationCollection GetCollectionToTest()
		{
			return new AllowSendingBookingConfirmationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AllowSendingBookingConfirmation();
		}

		#endregion

	}
}
