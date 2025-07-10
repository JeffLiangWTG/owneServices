using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New BusinessObject for Container Yard project")]
	public class CYDMachineryLineItem : AutoCYDMachineryLineItem
	{
		public CYDMachineryLineItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
