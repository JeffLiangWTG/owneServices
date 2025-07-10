using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public static class DataWriterExtensions
	{
		public static void WriteGenAddOnColumnIntoAddInfoCollection(this List<AddInfo> addInfoCollection, IEnumerable<GenAddOnDetail> genAddOnDetails, BusinessObject parentBO)
		{
			foreach (var genAddOnDetail in genAddOnDetails)
			{
				addInfoCollection.Add(new AddInfo { Key = genAddOnDetail.AddInfoKey, Value = parentBO[genAddOnDetail.PropertyName].ToString() });
			}
		}
	}
}
