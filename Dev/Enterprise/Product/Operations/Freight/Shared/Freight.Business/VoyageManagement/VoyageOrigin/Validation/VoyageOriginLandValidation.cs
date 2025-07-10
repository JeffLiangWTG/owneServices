using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Business
{
	public class VoyageOriginLandValidation : BaseJobVoyOriginValidation
	{
		public VoyageOriginLandValidation(AutoJobVoyOrigin parent)
			: base(parent)
		{
		}

		protected override bool ETDIsMandatory
		{
			get { return IsHomePort || IsETDMandatoryDueToCorrespondingPorts; }
		}
	}
}
