using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class HazardousMaterial : US.Messaging.Business.HazardousMaterial
	{
		public HazardousMaterial(UNDGDataItem undg)
			: base(undg)
		{
		}

		protected override CargoWise.Types.ZString HazMatQualifierCore
		{
			get { return HazMatQualifierList.Codes.UnitedNations; }
		}
	}
}
