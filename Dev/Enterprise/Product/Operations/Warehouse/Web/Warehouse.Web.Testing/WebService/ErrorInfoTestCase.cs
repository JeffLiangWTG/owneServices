using System;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	[TestedType(typeof(ErrorInfo))]
	public class ErrorInfoTestCase : DataObjectInfoTestCase<ErrorInfo>
	{
		#region WhsInventoryError

		public void TestWhsInventoryError()
		{
			var response = new WhsInventoryWebServiceResponse();
			response.InventoryErrorInfos = new ErrorInfo[] { new ErrorInfo(0, "ErrorMessage", "ErrorType") };

			AssertEquals(0, response.InventoryErrorInfos[0].Sequence);
			AssertEquals("ErrorMessage", response.InventoryErrorInfos[0].ErrorMessage);
			AssertEquals("ErrorType", response.InventoryErrorInfos[0].ErrorType);
			var inventoryPK = Guid.NewGuid();
			response.InventoryLinePK = inventoryPK;
			AssertEquals(inventoryPK, response.InventoryLinePK);
		}

		#endregion

		protected new ErrorInfo Parent => (ErrorInfo)base.Parent;

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new ErrorInfo();
		}
	}
}
