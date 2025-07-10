using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.eTail.Business
{
	public class HVLVReturnPivot : AutoHVLVReturnPivot
	{
		public HVLVReturnPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static HVLVReturnPivot CreateFromConsignments(BusinessObjectFactory factory, HVLVConsignment formerConsignment, HVLVConsignment returnConsignment)
		{
			var returnPivot = factory.New<HVLVReturnPivot>();
			returnPivot.HVP_HVC_Former = formerConsignment.PK;
			returnPivot.HVP_HVC_Return = returnConsignment.PK;
			return returnPivot;
		}

		public HVLVConsignment FormerConsignment => Factory.Load<HVLVConsignment>(HVP_HVC_Former);
		public HVLVConsignment ReturnConsignment => Factory.Load<HVLVConsignment>(HVP_HVC_Return);

		[RelatedBusinessObject(nameof(FormerConsignment))]
		public override ZGuid HVP_HVC_Former
		{
			get => base.HVP_HVC_Former;
			set => base.HVP_HVC_Former = value;
		}

		[RelatedBusinessObject(nameof(ReturnConsignment))]
		public override ZGuid HVP_HVC_Return
		{
			get => base.HVP_HVC_Return;
			set => base.HVP_HVC_Return = value;
		}
	}
}
