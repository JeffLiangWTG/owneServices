using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	sealed class RefCountryTranslatableDataFieldAttribute : TranslatableDataFieldAttribute, ICustomizableDataCaptionSource
	{
		public RefCountryTranslatableDataFieldAttribute(string columnName)
			: base(RefCountry.Schema.TableName, columnName, DataXmlFilePaths.RefCountry)
		{
			Type = typeof(RefCountry);
		}

		public override ZQuery Filter
		{
			get
			{
				var filter = new ZQuery(RefCountrySchema.RN_IsActive, true);

				return filter;
			}
		}
	}
}
