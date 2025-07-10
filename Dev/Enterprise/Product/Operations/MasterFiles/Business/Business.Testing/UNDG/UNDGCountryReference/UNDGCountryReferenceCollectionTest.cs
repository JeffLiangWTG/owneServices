using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UNDGCountryReferenceCollection))]
	sealed class UNDGCountryReferenceCollectionTest : ActiveBusinessObjectCollectionTestCase<UNDGCountryReferenceCollection>
	{
		public void TestGetExtraNotification()
		{
			var undgCountryReferenceCollection = new UNDGCountryReferenceCollection(Factory);
			var undgCountryReference = Factory.New<UNDGCountryReference>();
			undgCountryReference.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			undgCountryReference.DCR_Type = Core.Constants.UNDGCountryReference.Type.PSA;
			AssertEquals("System maintained Country/Region Reference/s (SG, PSA) cannot be attached.", undgCountryReferenceCollection.GetExtraNotification(undgCountryReference).Message);

			Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed = false;

			var undgCountryReference2 = Factory.New<UNDGCountryReference>();
			undgCountryReference.DCR_RN_NKCountry = Core.Constants.CountryCodes.France;
			undgCountryReference.DCR_Type = Core.Constants.UNDGCountryReference.Type.ICPE;
			AssertEquals("You do not have the security rights to attach Country/Region Regulations.", undgCountryReferenceCollection.GetExtraNotification(undgCountryReference2).Message);

			var undgCountryReferenceCollectionForWhsUNDGLimit = new UNDGCountryReferenceCollection(Factory, Factory.New<IWhsUNDGLimit>());
			AssertNull(undgCountryReferenceCollectionForWhsUNDGLimit.GetExtraNotification(undgCountryReference));
			AssertNull(undgCountryReferenceCollectionForWhsUNDGLimit.GetExtraNotification(undgCountryReference2));
		}

		#region Implementation

		protected override UNDGCountryReferenceCollection GetCollectionToTest()
		{
			return new UNDGCountryReferenceCollection(Factory);
		}

		#endregion
	}
}
