using System;

namespace Enterprise.Rating.Rateable
{
	public class RefContainerInfoParts
	{
		public Guid? RefContainerComponent { get; set; }
		public Guid? RefContainerMaterial { get; set; }
		public Guid? RefContainerRepair { get; set; }
		public string RefUnitSection { get; set; }
		public Guid WorkOrderLinePK { get; set; }
	}
}
