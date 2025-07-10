using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(DocumentActionReasonModel))]
	sealed class DocumentActionReasonModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateOption()
		{
			var model = (DocumentActionReasonModel)GetNewBusinessObject();

			model.ReasonCode = "!?!";
			AssertHasError(model.ReasonCodeInfo, "Enter a valid selection.");

			model.ReasonCode = "aaa";
			AssertNoErrors(model.ReasonCodeInfo);

			model.ReasonCode = "";
			AssertHasError(model.ReasonCodeInfo, "Please enter a value.");

			model.ReasonCode = "bbb";
			AssertNoErrors(model.ReasonCodeInfo);

			model.ReasonText = "test reason";
			AssertNoErrors(model.ReasonTextInfo);

			model.ReasonText = "";
			AssertHasError(model.ReasonTextInfo, "Please enter a value.");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("aaa", "aaa desc");
			list.AddPair("bbb", "bbb desc");
			list.AddPair("ccc", "ccc desc");

			return new DocumentActionReasonModel(list);
		}
	}
}
