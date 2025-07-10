//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUniversalValidationRuleSetLookups
//
//    This class should be used for overriding collections in AutoUniversalValidationRuleSetLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Workflow.Business
{
	public class UniversalValidationRuleSetLookups : AutoUniversalValidationRuleSetLookups
	{
		public UniversalValidationRuleSetLookups(AutoUniversalValidationRuleSet parent) : base(parent)
		{
		}

		public static CodeDescriptionPairList DataContexts => CreateDataContextListInAlphabeticalOrder();

		public CodeDescriptionPairList DataContextList
			=> Factory.GetCachedValue("DataContextList", () => CreateDataContextListInAlphabeticalOrder());

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Untranslatable description")]
		static CodeDescriptionPairList CreateDataContextListInAlphabeticalOrder()
		{
			var list = new UntranslatableCodeDescriptionPairList("Reason for Untranslatable: Data Context values are part of XML schema");
			foreach (var code in Enum.GetNames(typeof(UniversalDataBuss.Integration.DataContextType))
				.OrderBy(x => x))
			{
				list.AddPair(code);
			}

			return list;
		}
	}
}
