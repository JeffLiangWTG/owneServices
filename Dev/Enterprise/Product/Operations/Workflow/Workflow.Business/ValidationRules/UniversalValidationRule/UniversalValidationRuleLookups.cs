//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUniversalValidationRuleLookups
//
//    This class should be used for overriding collections in AutoUniversalValidationRuleLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Workflow.Business
{
	public class UniversalValidationRuleLookups : AutoUniversalValidationRuleLookups
	{
		public UniversalValidationRuleLookups(AutoUniversalValidationRule parent) : base(parent)
		{
		}

		public CodeDescriptionPairList StatusList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(StatusCodes.Error, Res.GetString("8560BBB6-34CF-4A76-A942-278DA852F43B", "ERROR"));
				result.AddPair(StatusCodes.Warning, Res.GetString("8142F476-726E-474B-97E0-18768C850F5D", "WARNING"));
				return result;
			}
		}

		public static class StatusCodes
		{
			public const string Error = "ERR";
			public const string Warning = "WRN";
		}
	}
}
