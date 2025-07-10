using System;
using System.Collections.Generic;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class ScannedRCASerialNumbersPerProductInfo : DataObjectInfo
	{
		public ScannedRCASerialNumbersPerProductInfo()
			: this(Guid.Empty, Guid.Empty, new List<string>())
		{ }

		public ScannedRCASerialNumbersPerProductInfo(Guid productPK, Guid clientPK, List<string> scannedRCASerialNumbers)
		{
			ProductPK = productPK;
			ClientPK = clientPK;
			ScannedRCASerialNumbers = scannedRCASerialNumbers;
		}

		public Guid ProductPK { get; set; }

		public Guid ClientPK { get; set; }

		public List<string> ScannedRCASerialNumbers { get; set; }
	}
}
