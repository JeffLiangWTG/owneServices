using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USAIILine)]
	public class USAIILineAddInfo : AutoUSAIILineAddInfo
	{
		public USAIILineAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new AIILine Parent
		{
			get { return (AIILine)base.Parent; }
			protected set { base.Parent = value; }
		}

		[List(nameof(Lookups) + "." + nameof(USAIILineAddInfoLookups.US_QtyDiffReasonCodeList))]
		public override ZString US_QtyDiffRsnCode
		{
			get { return base.US_QtyDiffRsnCode; }
			set { base.US_QtyDiffRsnCode = value; }
		}

		[List(nameof(Lookups) + "." + nameof(USAIILineAddInfoLookups.US_UnitOfMeasureList))]
		public override ZString US_InvUQDisp
		{
			get { return base.US_InvUQDisp; }
			set { base.US_InvUQDisp = value; }
		}
	}
}
