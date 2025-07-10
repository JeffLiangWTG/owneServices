//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGenCustomColumnDefinitionLookups
//
//    This class should be used for overriding collections in AutoGenCustomColumnDefinitionLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class GenCustomColumnDefinitionLookups : AutoGenCustomColumnDefinitionLookups
	{
		public GenCustomColumnDefinitionLookups(AutoGenCustomColumnDefinition parent)
			: base(parent)
		{
		}

		public GenCustomAddOnRuleCollection Rules
		{
			get { return rules ?? (rules = new GenCustomAddOnRuleCollection(Factory)); }
		}
		GenCustomAddOnRuleCollection rules;

		public ReadOnlyCodeDescriptionPairList Types
		{
			get { return GetTypes(); }
		}

		protected virtual ReadOnlyCodeDescriptionPairList GetTypes()
		{
			return Factory.GetCachedValue("AddOnColumnDataTypeWithoutGuid", () =>
				{
					AddOnColumnDataType list = new AddOnColumnDataType();
					list.RemoveCode(AddOnColumnDataType.Codes.Guid);
					list.RemoveCode(AddOnColumnDataType.Codes.Byte);
					list.RemoveCode(AddOnColumnDataType.Codes.Short);
					list.RemoveCode(AddOnColumnDataType.Codes.Date);
					return list;
				});
		}
	}
}
