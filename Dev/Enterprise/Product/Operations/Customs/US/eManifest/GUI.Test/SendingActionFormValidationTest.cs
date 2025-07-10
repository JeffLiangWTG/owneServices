using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.eManifest.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.GUI.Testing
{
	[TestedType(typeof(SendingActionForm.Validation))]
	sealed class SendingActionFormValidationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new SendingActionForm.Validation(Factory.New<Trip>());
	}
}
