using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	sealed class RefCurrencyTranslatableDataFieldAttribute : TranslatableDataFieldAttribute, ICustomizableDataCaptionSource
	{
		public RefCurrencyTranslatableDataFieldAttribute(string columnName)
			: base(RefCurrency.Schema.TableName, columnName, DataXmlFilePaths.RefCountry)
		{
			Type = typeof(RefCurrency);
		}

		public override ZQuery Filter
		{
			get
			{
				var filter = new ZQuery(RefCurrencySchema.RX_IsActive, true);

				return filter;
			}
		}
	}
}
