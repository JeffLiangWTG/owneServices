using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UNDGCommonData))]
	sealed class UNDGCommonDataTest : EnterpriseBusinessObjectTestCase
	{
		public void TestReadOnly()
		{
			var data = Factory.New<UNDGCommonData>();
			AssertEquals(false, data.DC_DescriptorInfo.ReadOnly);

			data.DC_Language = Core.Constants.Languages.English;
			AssertEquals(true, data.DC_DescriptorInfo.ReadOnly);

			data.DC_Language = Core.Constants.Languages.Gujarati;
			AssertEquals(false, data.DC_DescriptorInfo.ReadOnly);
		}

		public void TestDelete()
		{
			var data = Factory.New<UNDGCommonData>();
			data.Delete();
			AssertEquals(true, data.IsDeleted);

			data = Factory.New<UNDGCommonData>();
			data.DC_Language = Core.Constants.Languages.Gujarati;
			data.Delete();
			AssertEquals(true, data.IsDeleted);

			data = Factory.New<UNDGCommonData>();
			data.DC_Language = Core.Constants.Languages.English;
			data.Delete();
			AssertEquals(true, data.IsDeleted);

			data = Factory.New<UNDGCommonData>();
			data.DC_Language = Core.Constants.Languages.English;
			Factory.Save();
			var cannotDeleteThrown = false;
			try
			{
				data.Delete();
			}
			catch (CannotDeleteException)
			{
				cannotDeleteThrown = true;
			}
			AssertEquals(true, cannotDeleteThrown);
			AssertEquals(false, data.IsDeleted);
		}

		public void TestHumanReadableNameCore()
		{
			var data = Factory.NewWithValidTestData<UNDGCommonData>();
			data.DC_Type = "GLD";
			data.DC_Descriptor = "Gold";

			AssertEquals("Dangerous Goods Common Provisions - GLD - Gold", data.HumanReadableName);
		}
	}
}
