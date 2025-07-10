using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class RefCurrencyTestHelper
	{
		readonly BusinessObjectFactory factory;

		public RefCurrencyTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public RefLanguageText CreateRefLanguageText(ZString columnName, ZGuid parentId, ZString language, ZString parentTableCode, ZString text)
		{
			var languageText = factory.New<RefLanguageText>();
			languageText.RLT_ColumnName = columnName;
			languageText.RLT_ParentId = parentId;
			languageText.RLT_Language = language;
			languageText.RLT_ParentTableCode = parentTableCode;
			languageText.RLT_Text = text;
			return languageText;
		}
	}
}
