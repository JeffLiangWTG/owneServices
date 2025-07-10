using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProfileAttribute))]
	class RefCusProfileAttributeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => Attribute;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => Attribute;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		RefCusProfileAttribute Attribute
		{
			get
			{
				if (attribute == null)
				{
					var helper = new UniversalReferenceTestDataHelper(Factory);
					var profileType = helper.CreateRefCusProfileType("PP0", "HSN", Core.Constants.CountryCodes.Brazil, "dummy Description 0");
					var profile = helper.CreateRefCusProfile(profileType, "12345678", "ATT1", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
					attribute = profile.Attributes.AddNew();
					attribute.XXY_Name = "Name";
					attribute.XXY_Value = "Value";
				}
				return attribute;
			}
		}
		RefCusProfileAttribute attribute;
	}
}
