using CargoWise.EntityFramework;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Business
{
	public class BaseJobVoyageValidation : JobVoyageValidation
	{
		public BaseJobVoyageValidation(AutoJobVoyage parent)
			: base(parent)
		{
			Voyage = (JobVoyage)parent;
		}

		protected JobVoyage Voyage;

		protected override void CheckJV_OH_Line()
		{
			base.CheckJV_OH_Line();
			ListValidation.ErrorIfInvalidPK(Voyage.JV_OH_LineInfo);
		}

		public void ValidateIsArchived()
		{
			ValidateCalculatedProperty(Voyage.IsArchivedInfo);
		}

		protected virtual void CheckIsArchived()
		{
		}

		#region Implementation

		protected JobVoyage[] FindOtherVoyagesWithSameVesselVoyageCombination()
		{
			return Voyage.FindOtherVoyagesWithSameVesselVoyageCombination();
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateIsArchived();
		}

		#endregion
	}
}
