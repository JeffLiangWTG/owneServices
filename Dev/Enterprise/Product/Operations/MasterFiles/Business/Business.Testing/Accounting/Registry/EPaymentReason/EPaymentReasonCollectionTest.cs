using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EPaymentReasonCollection))]
	sealed class EPaymentReasonCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<EPaymentReasonCollection>
	{
		public void TestAllowNew()
		{
			var collection = new EPaymentReasonCollection();
			Assert("Users should be able to add new items to the collection.", collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = new EPaymentReasonCollection();
			Assert("Users should be able to remove items from the collection.", collection.AllowRemove);
		}

		public void TestPopulatePaymentReasonsForAllProviders()
		{
			var collection = new EPaymentReasonCollection();
			AssertEquals(0, collection.Count);
			collection.PopulatePaymentReasonsForAllProviders();
			AssertEquals(7, collection.Count);
			var ofxReasons = collection.Cast<EPaymentReason>().Where(r => r.ProviderCode == EPaymentProviderCodes.Codes.OFX);
			AssertNotNull(ofxReasons.FirstOrDefault(r => r.ReasonCode == "ACS" && r.ReasonDescription == "Accounting services"));
			AssertNotNull(ofxReasons.FirstOrDefault(r => r.ReasonCode == "BCS" && r.ReasonDescription == "Business consultancy and PR Services"));
			AssertNotNull(ofxReasons.FirstOrDefault(r => r.ReasonCode == "EPS" && r.ReasonDescription == "Employee payment, salary/wages"));
			AssertNotNull(ofxReasons.FirstOrDefault(r => r.ReasonCode == "GPP" && r.ReasonDescription == "Goods payment, purchase"));
			AssertNotNull(ofxReasons.FirstOrDefault(r => r.ReasonCode == "HCI" && r.ReasonDescription == "Hardware consultancy/implementation"));
			AssertNotNull(ofxReasons.FirstOrDefault(r => r.ReasonCode == "SCI" && r.ReasonDescription == "Software consultancy/implementation"));
			AssertNotNull(ofxReasons.FirstOrDefault(r => r.ReasonCode == "SVT" && r.ReasonDescription == "Services trade"));
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override EPaymentReasonCollection GetCollectionToTest() => new EPaymentReasonCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new EPaymentReason();

		#endregion
	}
}
