using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using static Enterprise.Core.Constants.ShipmentTypes;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVConsignmentHeaderForTest : HVLVConsignmentHeader
	{
		public HVLVConsignmentHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			Shipment.JS_ShipmentType = HighVolumeLowValue;
		}
	}
}
