using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomFieldsDataObjectWriter<T, U> : DataObjectWriter<T, U>
		where T : BusinessObject
		where U : IDataObject, new()
	{
		public CustomFieldsDataObjectWriter(IDataWritingManager writeManager, U dataObject)
			: base(writeManager)
		{
			this.dataObject = Argument.NotNull(dataObject, "U dataObject");
		}

		readonly U dataObject;

		protected override U PopulateDataObject(T sourceBO)
		{
			return dataObject;
		}

		protected override IEnumerable<MasterFiles.Integration.IPropertyValue> GetUserDefinedValues(T sourceBO)
		{
			return sourceBO.GetUserDefinedValues();
		}
	}
}
