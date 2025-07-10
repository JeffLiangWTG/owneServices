using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusNomenclatureGroupNote))]
	class RefCusNomenclatureGroupNoteTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("ENG", "English");
			Factory.Save();

			var group = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "9", ZDateTime.Today, ZDateTime.MaxSmallDateTime, "Test Description", "99");
			return helper.CreateNomenclatureGroupNote(group, "ENG", "Typ", "Note");
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}
	}
}
