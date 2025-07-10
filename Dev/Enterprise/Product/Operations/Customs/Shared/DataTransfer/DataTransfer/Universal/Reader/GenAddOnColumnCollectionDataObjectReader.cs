using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class GenAddOnColumnCollectionDataObjectReader : DataObjectReader
	{
		public GenAddOnColumnCollectionDataObjectReader(IXmlImportLogger logger)
			: base(logger)
		{
		}

		public void ReadIntoBusinessObject(IEnumerable<AddInfo> addInfoCollection, IEnumerable<GenAddOnDetail> genAddOnCanBeImported, BusinessObject parentBO)
		{
			if (addInfoCollection != null && genAddOnCanBeImported != null && parentBO != null)
			{
				var isDefaultingEnabled = IsDefaultingEnabled;
				var genAddOnImportList = GetGenAddOnImportList(addInfoCollection, genAddOnCanBeImported);
				foreach (var genAddOnColumn in genAddOnImportList)
				{
					genAddOnColumn.ReadIntoBusinessObject(isDefaultingEnabled, parentBO);
				}
			}
		}

		IEnumerable<GenAddOnDetail> GetGenAddOnImportList(IEnumerable<AddInfo> addInfoCollection, IEnumerable<GenAddOnDetail> genAddOnCanBeImported)
		{
			foreach (var genAddOn in genAddOnCanBeImported)
			{
				IZType value = null;
				switch (genAddOn.TypeCode)
				{
					case AddOnColumnDataType.Codes.Boolean:
						value = addInfoCollection.GetZBoolValue(genAddOn.AddInfoKey, logger);
						break;
					case AddOnColumnDataType.Codes.Byte:
						value = addInfoCollection.GetZByteValue(genAddOn.AddInfoKey, logger);
						break;
					case AddOnColumnDataType.Codes.Date:
						value = addInfoCollection.GetZDateValue(genAddOn.AddInfoKey, logger);
						break;
					case AddOnColumnDataType.Codes.Datetime:
						value = addInfoCollection.GetZDateTimeValue(genAddOn.AddInfoKey, logger);
						break;
					case AddOnColumnDataType.Codes.Decimal:
						value = addInfoCollection.GetZDecimalValue(genAddOn.AddInfoKey, logger);
						break;
					case AddOnColumnDataType.Codes.Guid:
						value = addInfoCollection.GetZGuidValue(genAddOn.AddInfoKey, logger);
						break;
					case AddOnColumnDataType.Codes.Integer:
						value = addInfoCollection.GetZIntValue(genAddOn.AddInfoKey, logger);
						break;
					case AddOnColumnDataType.Codes.Short:
						value = addInfoCollection.GetZShortValue(genAddOn.AddInfoKey, logger);
						break;
					case AddOnColumnDataType.Codes.String:
						value = addInfoCollection.GetZStringValue(genAddOn.AddInfoKey, logger);
						break;
				}
				if (value != null)
				{
					yield return new GenAddOnDetail()
					{
						TypeCode = genAddOn.TypeCode,
						PropertyName = genAddOn.PropertyName,
						AddInfoKey = genAddOn.AddInfoKey,
						GenAddOnColumnName = genAddOn.GenAddOnColumnName,
						Value = value
					};
				}
			}
		}
	}
}
