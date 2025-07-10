//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgAddressAdditionalInfoValidation
//
//    This class should be used for overriding validation in AutoOrgAddressAdditionalInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgAddressAdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAdditionalInformation_MustHaveSingleMainAdditionalAddressInformation()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			Factory.Save();

			var addressInfo1 = address.AdditionalInfos.AddNew();
			addressInfo1.OAI_IsPrimary = true;
			addressInfo1.OAI_AdditionalInfo = "Main Additional Information";

			var addressInfo2 = address.AdditionalInfos.AddNew();
			addressInfo2.OAI_IsPrimary = true;
			addressInfo2.OAI_AdditionalInfo = "Second Additional Information";

			Assert("Should give an error", addressInfo2.HasErrors());
			AssertHasErrorContaining(addressInfo2.OAI_IsPrimaryInfo, "Must specified one main address additional information.");
		}

		public void TestAdditionalInformation_MustSpecifiedUniqueAdditionalAddressInformation()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			Factory.Save();

			var addressInfo1 = address.AdditionalInfos.AddNew();
			addressInfo1.OAI_IsPrimary = true;
			addressInfo1.OAI_AdditionalInfo = "Unique Additional information";

			var addressInfo2 = address.AdditionalInfos.AddNew();
			addressInfo2.OAI_AdditionalInfo = "Unique additional Information";

			var addressInfo3 = address.AdditionalInfos.AddNew();
			addressInfo3.OAI_AdditionalInfo = "unique additional information";

			CombineAssertions(() =>
			{
				Assert("Should give an error", addressInfo2.HasErrors());
				AssertHasErrorContaining(addressInfo2.OAI_AdditionalInfoInfo, "Must specified a unique address additional information.");
				Assert("Should give an error", addressInfo3.HasErrors());
				AssertHasErrorContaining(addressInfo3.OAI_AdditionalInfoInfo, "Must specified a unique address additional information.");
			});
		}

		public void TestAdditionalInformation_WithNoTranslatedAdditionalInformation()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;
			var translatedAddress = address.TranslatedAddresses.AddNew();
			translatedAddress.Address1 = "Translated Address1";
			translatedAddress.Language = Core.SharedConstants.Languages.ChineseSimplified;
			translatedAddress.OTA_AdditionalAddressInformation = string.Empty;

			Factory.Save();

			var addressInfo1 = address.AdditionalInfos.AddNew();
			addressInfo1.OAI_AdditionalInfo = "Main Additional Information";
			addressInfo1.OAI_IsPrimary = true;

			AssertEquals("Main Additional Information", address.OA_AdditionalAddressInformation);
			AssertEquals(string.Empty, translatedAddress.OTA_AdditionalAddressInformation);
		}

		public void TestAdditionalInformation_ShouldRemoveIsPrimaryError()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			Factory.Save();

			var addressInfo1 = address.AdditionalInfos.AddNew();
			addressInfo1.OAI_AdditionalInfo = "Main Additional Information";
			addressInfo1.OAI_IsPrimary = true;

			var addressInfo2 = address.AdditionalInfos.AddNew();
			addressInfo2.OAI_AdditionalInfo = "Second Additional Information";
			addressInfo2.OAI_IsPrimary = true;

			Assert("Should give an error", addressInfo2.HasErrors());

			addressInfo1.OAI_IsPrimary = false;

			Assert("Should not have an error", !addressInfo2.HasErrors());
			Assert("Should not have an error", !addressInfo1.HasErrors());
		}

		public void TestDeleteMainAdditionalInformation_ErrorWhenThereAreOtherAdditionalInfosWithoutMain()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			Factory.Save();

			var addressInfo1 = address.AdditionalInfos.AddNew();
			addressInfo1.OAI_IsPrimary = true;
			addressInfo1.OAI_AdditionalInfo = "Main additional information";

			var addressInfo2 = address.AdditionalInfos.AddNew();
			addressInfo2.OAI_AdditionalInfo = "Second additional information";

			var addressInfo3 = address.AdditionalInfos.AddNew();
			addressInfo3.OAI_AdditionalInfo = "Third additional information";

			var additionalInfos = address.AdditionalInfos;

			Assert("Pre-condition: Has main additional info", additionalInfos.Any(x => x.OAI_IsPrimary));

			addressInfo1.Delete();

			CombineAssertions(() =>
			{
				Assert("Does not have main additional info", !additionalInfos.Any(x => x.OAI_IsPrimary));
				Assert("All have error", additionalInfos.All(x => x.HasErrors()));
				Assert("All have error message", additionalInfos.All(x => x.OAI_IsPrimaryInfo.HasNotification("Must specified one main address additional information.")));
			});
		}

		public void TestDeleteMainAdditionalInformation_OkWhenThereAreOtherAdditionalInfosWithMain()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			Factory.Save();

			var addressInfo1 = address.AdditionalInfos.AddNew();
			addressInfo1.OAI_IsPrimary = true;
			addressInfo1.OAI_AdditionalInfo = "Main additional information";

			var addressInfo2 = address.AdditionalInfos.AddNew();
			addressInfo2.OAI_AdditionalInfo = "Second additional information";
			addressInfo2.OAI_IsPrimary = true;

			var addressInfo3 = address.AdditionalInfos.AddNew();
			addressInfo3.OAI_AdditionalInfo = "Third additional information";

			var additionalInfos = address.AdditionalInfos;

			Assert("Pre-condition: Has main additional info", additionalInfos.Any(x => x.OAI_IsPrimary));

			addressInfo1.Delete();

			CombineAssertions(() =>
			{
				Assert("Has main additional info", additionalInfos.Count(x => x.OAI_IsPrimary) == 1);
				Assert("All have no error", additionalInfos.All(x => !x.HasErrors()));
				Assert("All have no error message", additionalInfos.All(x => !x.OAI_IsPrimaryInfo.HasNotification("Must specified one main address additional information.")));
			});
		}

		public void TestDeleteMainAdditionalInformation_OkWhenThereIsNoOtherAdditionalInfos()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			Factory.Save();

			var addressInfo1 = address.AdditionalInfos.AddNew();
			addressInfo1.OAI_IsPrimary = true;
			addressInfo1.OAI_AdditionalInfo = "Main additional information";

			var additionalInfos = address.AdditionalInfos;

			Assert("Pre-condition: Has main additional info", additionalInfos.Any(x => x.OAI_IsPrimary));

			addressInfo1.Delete();

			CombineAssertions(() =>
			{
				Assert("Does not have main additional info", !additionalInfos.Any(x => x.OAI_IsPrimary));
				Assert("All have no error", additionalInfos.All(x => !x.HasErrors()));
				Assert("All have no error message", additionalInfos.All(x => !x.OAI_IsPrimaryInfo.HasNotification("Must specified one main address additional information.")));
			});
		}
	}
}
