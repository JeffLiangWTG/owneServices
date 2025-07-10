using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ItineraryData))]
	sealed class ItineraryDataTest : Customs.Business.Testing.CusCodeDataTest<ItineraryData>
	{
		[ExpectNoExceptions]
		public void TestValidationType()
		{
			NUnit.Framework.Assert.That(itineraryData.Validation, NUnit.Framework.Is.TypeOf<ItineraryDataValidation>(), "Validation type");
		}

		[ExpectNoExceptions]
		public void TestLookupsType()
		{
			NUnit.Framework.Assert.That(itineraryData.Lookups, NUnit.Framework.Is.TypeOf<ItineraryDataLookups>(), "Lookups type");
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(itineraryData.CY_Type, NUnit.Framework.Is.EqualTo(CusCodeDataTypeList.Codes.Itinerary).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(itineraryData.CY_ParentTableCode, NUnit.Framework.Is.EqualTo(JobDeclarationSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestCY_Code()
		{
			var info = itineraryData.CY_CodeInfo;
			NUnit.Framework.Assert.That(info.MaxLength, NUnit.Framework.Is.EqualTo(2), "Max Length");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(info, "Routing Country Code", "Indicates the country code that the goods pass through from the country of export to the final destination.");
		}

		[ExpectNoExceptions]
		public void TestRoutingCountry()
		{
			itineraryData.CY_Code = Core.Constants.CountryCodes.UnitedStates;
			NUnit.Framework.Assert.That(itineraryData.RoutingCountry, NUnit.Framework.Is.EqualTo(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates)));
		}

		[ExpectNoExceptions]
		public void TestDescription()
		{
			itineraryData.CY_Code = Core.Constants.CountryCodes.UnitedStates;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(itineraryData.Description, NUnit.Framework.Is.EqualTo(itineraryData.RoutingCountry.RN_Desc));
				BusinessObjectCaptionTestHelper.AssertCaptions(itineraryData.DescriptionInfo, "Name");
			});
		}

		protected override IEnumerable<ItineraryData> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().Itineraries.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return declaration.Itineraries.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			itineraryData = declaration.Itineraries.AddNew();
		}

		ItineraryData itineraryData;

		JobDeclaration declaration;
	}
}
