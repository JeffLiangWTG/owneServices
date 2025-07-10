using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class RefCusProcedureHelper
	{
		const string Norway = Core.Constants.CountryCodes.Norway;
		public static void CreateRefCusProcedureList(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateRefCusProcedure(Norway, ZString.Empty, "40", "50", ZString.Empty, ZString.Empty, "IMP", group: "4");
			helper.CreateRefCusProcedure(Norway, ZString.Empty, "40", "52", ZString.Empty, ZString.Empty, "IMP", group: "4");
			helper.CreateRefCusProcedure(Norway, ZString.Empty, "40", "00", ZString.Empty, ZString.Empty, "IMP", group: "4");
			helper.CreateRefCusProcedure(Norway, ZString.Empty, "40", "10", ZString.Empty, ZString.Empty, "IMP", group: "4");
			helper.CreateRefCusProcedure(Norway, ZString.Empty, "41", "10", ZString.Empty, ZString.Empty, "IMP", group: "4");
			helper.CreateRefCusProcedure(Norway, ZString.Empty, "41", "11", ZString.Empty, ZString.Empty, "IMP", group: "4");
			helper.CreateRefCusProcedure(Norway, ZString.Empty, "50", "11", ZString.Empty, ZString.Empty, "IMP", group: "5");
			helper.CreateRefCusProcedure(Norway, ZString.Empty, "50", "71", ZString.Empty, ZString.Empty, "IMP", group: "5");
			helper.CreateRefCusProcedure(Norway, ZString.Empty, "10", "00", ZString.Empty, ZString.Empty, "EXP", group: "1");
			helper.CreateRefCusProcedure(Norway, ZString.Empty, "10", "10", ZString.Empty, ZString.Empty, "EXP", group: "1");
			helper.CreateRefCusProcedure(Norway, ZString.Empty, "11", "10", ZString.Empty, ZString.Empty, "EXP", group: "1");
			helper.CreateRefCusProcedure(Norway, ZString.Empty, "11", "11", ZString.Empty, ZString.Empty, "EXP", group: "1");
			factory.Save();
		}

		public static void CreateCusPreference(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreatePreferenceForCountry("A", "The EEA agreement, i.e. all EU countries incl. Iceland.", Norway);
			helper.CreatePreferenceForCountry("B", "The EC-Norway bilateral trade agreement.", Norway);
			helper.CreatePreferenceForCountry("C", "EFTA Convention, i.e. Switzerland (incl. Liechtenstein) and Iceland.", Norway);
			helper.CreatePreferenceForCountry("G", "The Norwegian GSP system.", Norway);
			helper.CreatePreferenceForCountry("N", "If the goods do not have originating status, preference shall not be claimed.", Norway);
			helper.CreatePreferenceForCountry("P", "Other free trade agreements, such as Morocco, Chile and others.", Norway);
		}
	}
}
