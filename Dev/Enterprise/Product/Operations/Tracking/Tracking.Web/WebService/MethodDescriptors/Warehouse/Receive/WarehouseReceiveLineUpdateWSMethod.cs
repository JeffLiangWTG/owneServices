using System;
using System.Linq;
using System.Web;
using CargoWise.Types;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.ServerServices
{
	public class WarehouseReceiveLineUpdateWSMethod : WarehouseDocketLineUpdateWSMethod<WarehouseReceiveLineUpdateParameters>
	{
		class ReceiveLineValues : DocketLineValues
		{
			public ZDecimal ExpectedQuantity;
			public ZDate ExpiryDate;
			public bool ExpiryDateReadOnly;
		}

		protected override DocketLineValues GetNewDocketLineValues() => new ReceiveLineValues();

		protected override DocketLineValues GetDocketLineValues(WhsDocketLine line)
		{
			var values = (ReceiveLineValues)base.GetDocketLineValues(line);
			var receiveLine = (WhsReceiveLine)line;
			values.ExpectedQuantity = receiveLine.WE_ClientOrderedUnits;
			values.ExpiryDate = receiveLine.WE_ExpiryDate;
			values.ExpiryDateReadOnly = receiveLine.WE_ExpiryDateInfo.ReadOnly;

			return values;
		}

		protected override void UpdateValues(DocketLineValues initialValues, WarehouseReceiveLineUpdateParameters parameters, WhsDocketLine line)
		{
			base.UpdateValues(initialValues, parameters, line);

			var initialReceiveLineValues = (ReceiveLineValues)initialValues;
			if (parameters.ModifiedControlID == parameters.ExpectedQuantityControlID && ZDecimal.TryParse(parameters.NewValue, out var expectedQuantity) && initialReceiveLineValues.ExpectedQuantity != expectedQuantity)
			{
				line.WE_ClientOrderedUnits = expectedQuantity;
			}
		}

		protected override void GenerateResponse(DocketLineValues initialValues, DocketLineValues updatedValues, WarehouseReceiveLineUpdateParameters parameters, WhsDocketLine line, WebServiceResponse response)
		{
			base.GenerateResponse(initialValues, updatedValues, parameters, line, response);

			var initialReceiveLineValues = (ReceiveLineValues)initialValues;
			var updatedReceiveLineValues = (ReceiveLineValues)updatedValues;

			AddUpdateToken(response, parameters, parameters.ExpectedQuantityControlID, initialReceiveLineValues.ExpectedQuantity, updatedReceiveLineValues.ExpectedQuantity);
			AddUpdateToken(response, parameters, parameters.ExpiryDateControlID, initialReceiveLineValues.ExpiryDate, updatedReceiveLineValues.ExpiryDate);

			AddSetReadOnlyToken(response, parameters.ExpiryDateControlID, initialReceiveLineValues.ExpiryDateReadOnly, updatedReceiveLineValues.ExpiryDateReadOnly);
		}

		protected override WhsDocketLine GetDocketLine(WarehouseReceiveLineUpdateParameters parameters)
		{
			var session = HttpContext.Current?.Session;
			if (session != null && Guid.TryParse(parameters.LineRef, out Guid linePK))
			{
				var receive = session[parameters.DocketRef] as TrackingWhsReceive;
				return receive?.Lines.Where(l => l.PK == linePK).OfType<TrackingWhsReceiveLine>().FirstOrDefault()?.WhsReceiveLine;
			}

			return null;
		}

		protected override string GetMethodName()
		{
			return "WarehouseReceiveLineUpdate";
		}

		protected override string GetScriptFileName()
		{
			return "WarehouseReceiveLineUpdateWSMethod.js";
		}
	}
}
