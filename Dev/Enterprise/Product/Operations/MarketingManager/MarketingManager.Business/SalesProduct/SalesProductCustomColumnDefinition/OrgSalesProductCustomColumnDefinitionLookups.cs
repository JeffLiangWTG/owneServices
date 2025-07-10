using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public class OrgSalesProductCustomColumnDefinitionLookups : GenCustomColumnDefinitionLookups
	{
		public OrgSalesProductCustomColumnDefinitionLookups(AutoGenCustomColumnDefinition parent)
			: base(parent)
		{
		}

		protected override ReadOnlyCodeDescriptionPairList GetTypes()
		{
			var codeDescriptionPairList = new CodeDescriptionPairList(base.GetTypes());
			codeDescriptionPairList.RemoveCode(AddOnColumnDataType.Codes.ComboBox);
			return codeDescriptionPairList;
		}
	}
}
