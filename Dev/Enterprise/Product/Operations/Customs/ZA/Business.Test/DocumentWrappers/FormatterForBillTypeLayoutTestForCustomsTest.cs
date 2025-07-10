using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	internal class FormatterForBillTypeLayoutTestForCustomsTest : TestCaseWithFactory
	{
		public void TestMarksAndNumbers()
		{
			ZString expectedResultForMarksAndNumbers =
				"          \n" +
				"          \n" +
				"          \n" +
				"This is   \n" +
				"the marks \n" +
				"and       \n" +
				"numbers   \n" +
				"to test   ";
			ZString expectedResultForFollowOn = "this formatter with";
			FormatterForBillTypeLayoutTestClassForCustoms formatter = new FormatterForBillTypeLayoutTestClassForCustoms();
			formatter.TestMarksAndNumbersWidth = 10;
			formatter.TestMarksAndNumbersAndDescriptionRowHeight = 8;
			AssertEquals("Result includes marks and numbers", expectedResultForMarksAndNumbers, formatter.MarksAndNumbers);
			AssertEquals("Result includes marks and numbers", expectedResultForFollowOn, formatter.FollowOnMarksAndNumbers);

			expectedResultForMarksAndNumbers =
				"          \n" +
				"          \n" +
				"          \n" +
				"This is   \n" +
				"the marks \n" +
				"and       \n" +
				"numbers   \n" +
				"to test   \n" +
				"this      \n" +
				"formatter \n" +
				"with      ";
			expectedResultForFollowOn = ZString.Empty;
			formatter = new FormatterForBillTypeLayoutTestClassForCustoms();
			formatter.TestMarksAndNumbersWidth = 10;
			formatter.TestMarksAndNumbersAndDescriptionRowHeight = 11;
			AssertEquals("Result includes marks and numbers", expectedResultForMarksAndNumbers, formatter.MarksAndNumbers);
			AssertEquals("Result includes marks and numbers", expectedResultForFollowOn, formatter.FollowOnMarksAndNumbers);
		}

		public void TestDescriptionColumn()
		{
			ZString expectedResultForDescription1 =
				"2 X 20FR CONTAINER(S)         \n" +
				"1 X 40OT CONTAINER(S)         \n" +
				"100 Pallet(s)                 \n" +
				"This is the goods description ";
			ZString expectedResultForDescription2 =
				"1 X 40OT CONTAINER(S)         \n" +
				"2 X 20FR CONTAINER(S)         \n" +
				"100 Pallet(s)                 \n" +
				"This is the goods description ";
			ZString expectedFollowOn = "to test this formatter with";

			FormatterForBillTypeLayoutTestClassForCustoms formatter = new FormatterForBillTypeLayoutTestClassForCustoms();
			formatter.TestDescriptionWidth = 30;
			formatter.TestMarksAndNumbersAndDescriptionRowHeight = 4;
			ZString result = formatter.GoodsDescription;
			Assert("Result includes description", result == expectedResultForDescription1 || result == expectedResultForDescription2);
			AssertEquals("Result includes description", expectedFollowOn, formatter.FollowOnGoodsDescription);

			expectedResultForDescription1 =
				"2 X 20FR CONTAINER(S)         \n" +
				"1 X 40OT CONTAINER(S)         \n" +
				"100 Pallet(s)                 \n" +
				"This is the goods description \n" +
				"to test this formatter with   ";
			expectedResultForDescription2 =
				"1 X 40OT CONTAINER(S)         \n" +
				"2 X 20FR CONTAINER(S)         \n" +
				"100 Pallet(s)                 \n" +
				"This is the goods description \n" +
				"to test this formatter with   ";
			expectedFollowOn = ZString.Empty;

			formatter = new FormatterForBillTypeLayoutTestClassForCustoms();
			formatter.TestDescriptionWidth = 30;
			formatter.TestMarksAndNumbersAndDescriptionRowHeight = 5;
			result = formatter.GoodsDescription;
			Assert("Result includes description", result == expectedResultForDescription1 || result == expectedResultForDescription2);
			AssertEquals("Result includes description", expectedFollowOn, formatter.FollowOnGoodsDescription);
		}

		public void TestWeight()
		{
			ZString expectedResultForWeight = ZString.Empty;
			FormatterForBillTypeLayoutTestClassForCustoms formatter = new FormatterForBillTypeLayoutTestClassForCustoms();
			AssertEquals("Result for weight format", expectedResultForWeight, formatter.Weight);
		}

		public void TestVolume()
		{
			ZString expectedResultForVolume = ZString.Empty;
			FormatterForBillTypeLayoutTestClassForCustoms formatter = new FormatterForBillTypeLayoutTestClassForCustoms();
			AssertEquals("Result for volume format", expectedResultForVolume, formatter.Volume);
		}

		public void TestContainerLayout()
		{
			ZString containerNumber =
				"1               \n" +
				"2               \n" +
				"3               ";
			ZString sealNumber =
				"SEAL 1                  \n" +
				"-                       \n" +
				"SEAL 3                  ";
			ZString type =
				"20FR        \n" +
				"20FR        \n" +
				"40OT        ";
			ZString weight = ZString.Empty;
			ZString volume = ZString.Empty;
			ZString packages = ZString.Empty;
			ZString deliveryMode = ZString.Empty;

			ZString followOnContainerNumber = "4               ";
			ZString followOnSealNumber = "SEAL 4                  ";
			ZString followOnType = "40OT        ";
			ZString followOnWeight = ZString.Empty;
			ZString followOnVolume = ZString.Empty;
			ZString followOnPackages = ZString.Empty;
			ZString followOnDeliveryMode = ZString.Empty;

			FormatterForBillTypeLayoutTestClassForCustoms formatter = new FormatterForBillTypeLayoutTestClassForCustoms();
			formatter.TestContainerRowHeight = 3;

			AssertEquals("Result for container number format", containerNumber, formatter.ContainerNumberColumn);
			AssertEquals("Result for container number follow on format", followOnContainerNumber, formatter.FollowOnContainerNumberColumn);

			AssertEquals("Result for seal number format", sealNumber, formatter.ContainerSealNumColumn);
			AssertEquals("Result for seal number follow on format", followOnSealNumber, formatter.FollowOnContainerSealNumColumn);

			AssertEquals("Result for type format", type, formatter.ContainerTypeColumn);
			AssertEquals("Result for type follow on format", followOnType, formatter.FollowOnContainerTypeColumn);

			AssertEquals("Result for weight format", weight, formatter.ContainerWeightColumn);
			AssertEquals("Result for weight follow on format", followOnWeight, formatter.FollowOnContainerWeightColumn);

			AssertEquals("Result for volume format", volume, formatter.ContainerVolumeColumn);
			AssertEquals("Result for volume follow on format", followOnVolume, formatter.FollowOnContainerVolumeColumn);

			AssertEquals("Result for pacakges format", packages, formatter.ContainerPackagesColumn);
			AssertEquals("Result for packages follow on format", followOnPackages, formatter.FollowOnContainerPackagesColumn);

			AssertEquals("Result for delivery mode format", deliveryMode, formatter.ContainerModeColumn);
			AssertEquals("Result for delivery follow on format", followOnDeliveryMode, formatter.FollowOnContainerModeColumn);
		}

		public void TestCombinationOfAllFields1()
		{
			ZString expectedResult =
				"           2 X 20FR CONTAINER(S)           \n" +
				"           1 X 40OT CONTAINER(S)           \n" +
				"           100 Pallet(s)                   \n" +
				"This is    This is the goods description   \n" +
				"the marks  to test this formatter with     \n" +
				"and                                        \n" +
				"numbers                                    \n" +
				"to test                                    \n" +
				"CONTAINER        SEAL                     TYPE         \n" +
				"1                SEAL 1                   20FR         \n" +
				"2                -                        20FR         \n" +
				"3                SEAL 3                   40OT         ";

			FormatterForBillTypeLayoutTestClassForCustoms formatter = new FormatterForBillTypeLayoutTestClassForCustoms();
			formatter.TestContainerRowHeight = 3;
			formatter.TestDescriptionWidth = 30;
			formatter.TestMarksAndNumbersAndDescriptionRowHeight = 8;
			formatter.TestMarksAndNumbersWidth = 10;
			AssertEquals("Fileds combined as one", expectedResult, formatter.GoodDetails);
		}

		public void TestCombinationOfAllFields2()
		{
			ZString expectedResult =
				"           2 X 20FR CONTAINER(S)           \n" +
				"           1 X 40OT CONTAINER(S)           \n" +
				"           100 Pallet(s)                   \n" +
				"This is    This is the goods description   \n" +
				"the marks  to test this formatter with     \n" +
				"and                                        \n" +
				"numbers                                    \n" +
				"to test                                    \n" +
				"this                                       \n" +
				"formatter                                  \n" +
				"with                                       \n" +
				"CONTAINER        SEAL                     TYPE         \n" +
				"1                SEAL 1                   20FR         \n" +
				"2                -                        20FR         \n" +
				"3                SEAL 3                   40OT         \n" +
				"4                SEAL 4                   40OT         ";

			FormatterForBillTypeLayoutTestClassForCustoms formatter = new FormatterForBillTypeLayoutTestClassForCustoms();
			formatter.TestContainerRowHeight = 3;
			formatter.TestDescriptionWidth = 30;
			formatter.TestMarksAndNumbersAndDescriptionRowHeight = 15;
			formatter.TestMarksAndNumbersWidth = 10;
			AssertEquals("Fileds combined as one", expectedResult, formatter.GoodDetails);
		}
		#region Implementation

		protected ZString storedCountry;

		protected override void SetUp()
		{
			storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.SouthAfrica);
			base.SetUp();
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.SetCountry(storedCountry);
			base.TearDown();
		}
		#endregion
	}
}
