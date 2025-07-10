using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(ApplicationStatusCollection))]
	sealed class ApplicationStatusCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ApplicationStatusCollection>
	{
		public void TestAddPair()
		{
			ApplicationStatusCollection collection = new ApplicationStatusCollection();
			collection.AddPair("MEH", "Description");
			AssertEquals(1, collection.Count);
			AssertEquals("MEH", collection[0].Code);
			AssertEquals("Description", collection[0].Description);
		}

		public void TestAsCodeDescriptionPairList()
		{
			ApplicationStatusCollection collection = new ApplicationStatusCollection();
			collection.AddPair("MEH", "Description");
			collection.AddPair("ME2", "Description2");

			CodeDescriptionPairList expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("MEH", "Description");
			expectedList.AddPair("ME2", "Description2");
			AssertEquals(expectedList.ElementsAsString, collection.AsCodeDescriptionPairList().ElementsAsString);
		}

		public void TestGetTemplateFromCode()
		{
			ApplicationStatusCollection collection = new ApplicationStatusCollection();
			collection.AddPair("MEH", "Description").EmailTemplate.EmailBody = "Template 1";
			collection.AddPair("ME2", "Description2").EmailTemplate.EmailBody = "Template 2";
			AssertEquals("Template 1", collection.GetTemplateFromCode("MEH").EmailBody);
			AssertEquals("Template 2", collection.GetTemplateFromCode("ME2").EmailBody);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ApplicationStatusCollection GetCollectionToTest()
		{
			return new ApplicationStatusCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ApplicationStatus();
		}

		#endregion
	}
}
