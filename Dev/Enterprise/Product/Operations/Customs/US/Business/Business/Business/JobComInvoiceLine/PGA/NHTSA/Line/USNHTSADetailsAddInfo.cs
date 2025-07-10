using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USNHTSADetails)]
	public class USNHTSADetailsAddInfo : AutoUSNHTSADetailsAddInfo, Integration.Customs.US.INHTSADetailsAddInfo
	{
		public USNHTSADetailsAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public USNHTSADetails Details => Parent as USNHTSADetails;

		protected override ZString HumanReadableNameCore
		{
			get { return "NHTSA Details"; }
		}

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				if (HasChanges && Parent != null && !Parent.IsMarkingAsNeedingValidationSuspended)
				{
					Parent.MarkAsNeedingValidation();
				}
			}
		}
	}
}
