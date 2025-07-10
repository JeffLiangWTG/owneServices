using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

using Enterprise.Core.Forms;
using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.Warehouse.Invoicing.Module;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(PeriodicInvoicingFilterControl))]
	public class CYDPeriodicInvoicingFilterControllerTest : TestCaseWithFactory
	{
		#region FilterControlFileds

		public void TestFilterControlFileds()
		{
			using (var periodicInvoicingFilterControl = new PeriodicInvoicingFilterControl(GetNewGridCollection(), (CYDPeriodicInvoicingFilterBusinessObject)GetNewFilterBusinessObject()))
			{
				AssertEquals("InvoicingFilterControl", periodicInvoicingFilterControl.Name);
				AssertEquals(15, periodicInvoicingFilterControl.Grid.ColumnStyles.Count);

				var warehouseColumn = periodicInvoicingFilterControl.Grid.ColumnStyles.Cast<ZGridColumnInfo>().ToList().FirstOrDefault(item => item.ColumnName == "ET_WW");
				var caption = warehouseColumn?.GetType().GetProperty("CaptionResourceString")?
					.GetValue(warehouseColumn)?.GetType().GetProperty("Caption")?
					.GetValue(warehouseColumn?.GetType().GetProperty("CaptionResourceString")?
					.GetValue(warehouseColumn))?.ToString();
				AssertEquals("Yard", caption);
			}
		}

		#endregion

		#region GetSomeThing

		protected FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CYDPeriodicInvoicingFilterBusinessObject();
		}

		protected IBusinessObjectCollection GetNewGridCollection()
		{
			return new PeriodicInvoicingCollection(Factory, PeriodicInvoicingStorageTypes.Codes.ContainerYard);
		}

		#endregion
	}
}
