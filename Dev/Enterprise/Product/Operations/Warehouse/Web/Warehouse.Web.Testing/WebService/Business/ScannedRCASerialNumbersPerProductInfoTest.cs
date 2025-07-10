using System;
using System.Collections.Generic;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(ScannedRCASerialNumbersPerProductInfo))]
	public class ScannedRCASerialNumbersPerProductInfoTest : DataObjectInfoTestCase<ScannedRCASerialNumbersPerProductInfo>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var productPK = Guid.NewGuid();
			var clientPK = Guid.NewGuid();
			var serialNumbers = new List<string> { "S1", "S2" };
			var scannedRCASerialNumbersPerProductInfo = new ScannedRCASerialNumbersPerProductInfo(productPK, clientPK, serialNumbers);

			AssertEquals("ProductPK", productPK, scannedRCASerialNumbersPerProductInfo.ProductPK);
			AssertEquals("ClientPK", clientPK, scannedRCASerialNumbersPerProductInfo.ClientPK);
			AssertContainsExactElementsInAnyOrder("Scanned Serial Numbers", serialNumbers, scannedRCASerialNumbersPerProductInfo.ScannedRCASerialNumbers);
		}

		#endregion

		#region Properties

		public void TestProperties()
		{
			var scannedRCASerialNumbersPerProductInfo = new ScannedRCASerialNumbersPerProductInfo();
			AssertEquals("Empty Product PK", Guid.Empty, scannedRCASerialNumbersPerProductInfo.ProductPK);
			AssertEquals("Empty Product PK", Guid.Empty, scannedRCASerialNumbersPerProductInfo.ClientPK);
			AssertEquals("Empty Scanned Serial Numbers list", 0, scannedRCASerialNumbersPerProductInfo.ScannedRCASerialNumbers.Count);

			var productPK = Guid.NewGuid();
			var clientPK = Guid.NewGuid();
			scannedRCASerialNumbersPerProductInfo.ProductPK = productPK;
			AssertEquals("ProductPK", productPK, scannedRCASerialNumbersPerProductInfo.ProductPK);

			scannedRCASerialNumbersPerProductInfo.ClientPK = clientPK;
			AssertEquals("ClientPK", clientPK, scannedRCASerialNumbersPerProductInfo.ClientPK);

			var serialNumbers = new List<string> { "S1", "S2" };
			scannedRCASerialNumbersPerProductInfo.ScannedRCASerialNumbers = serialNumbers;
			AssertContainsExactElementsInAnyOrder("Scanned Serial Numbers", serialNumbers, scannedRCASerialNumbersPerProductInfo.ScannedRCASerialNumbers);
		}

		#endregion

		#region Implementation

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new ScannedRCASerialNumbersPerProductInfo();
		}

		#endregion
	}
}
