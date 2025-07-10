using System;
using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class DummyDataObjectWithCustomizedFields : IDataObject, ICustomizedFieldContainer
	{
		public List<CustomizedField> CustomizedFieldCollection { get; set; }

		public bool SetCustomizedFieldCollection(Func<List<CustomizedField>> list)
		{
			CustomizedFieldCollection = list();
			return true;
		}

		public void SetWriterStrategy(IDataObjectWriterStrategy strategy) { }
	}
}
