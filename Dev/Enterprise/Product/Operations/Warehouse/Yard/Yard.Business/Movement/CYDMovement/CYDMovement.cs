using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New bizo for Container Yard project")]
	public class CYDMovement : AutoCYDMovement
	{
		public CYDMovement(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("MovementHeader")]
		public override ZGuid YML_YMH_MovementHeader { get => base.YML_YMH_MovementHeader; set => base.YML_YMH_MovementHeader = value; }
		public CYDMovementHeader MovementHeader
		{
			get => Factory.Load<CYDMovementHeader>(YML_YMH_MovementHeader);
		}

		[RelatedBusinessObject("YardUnitState")]
		public override ZGuid YML_YUS_YardUnitState { get => base.YML_YUS_YardUnitState; set => base.YML_YUS_YardUnitState = value; }
		public CYDYardUnitState YardUnitState
		{
			get => Factory.Load<CYDYardUnitState>(YML_YUS_YardUnitState);
		}

		#endregion
	}
}
