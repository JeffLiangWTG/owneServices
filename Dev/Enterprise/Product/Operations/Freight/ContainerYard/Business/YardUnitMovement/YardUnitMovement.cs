using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.ContainerYard.Business
{
	[DependentBusinessObject(typeof(YardUnit), "YardUnitMovements")]
	public class YardUnitMovement : AutoYardUnitMovement
	{
		public YardUnitMovement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region YardUnit

		[RelatedBusinessObject("YardUnit")]
		public override ZGuid GTM_GTY_YardUnit
		{
			get { return base.GTM_GTY_YardUnit; }
			set { base.GTM_GTY_YardUnit = value; }
		}

		public YardUnit YardUnit
		{
			get { return Factory.Load<YardUnit>(GTM_GTY_YardUnit); }
		}

		#endregion

		public override bool IsSavedByFactory => false;
	}
}
