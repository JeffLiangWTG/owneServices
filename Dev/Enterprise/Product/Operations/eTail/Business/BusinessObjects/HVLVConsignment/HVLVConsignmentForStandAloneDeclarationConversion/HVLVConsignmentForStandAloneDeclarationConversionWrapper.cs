using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentForStandAloneDeclarationConversionWrapper : NonPersistentBusinessObject
	{
		public HVLVConsignmentForStandAloneDeclarationConversionWrapper()
		{
			Convert = false;
		}

		public HVLVConsignmentForStandAloneDeclarationConversionWrapper(HVLVConsignment consignment) : base(consignment.Factory)
		{
			Argument.NotNull(consignment, nameof(consignment));
			Consignment = consignment;
			Convert = false;
		}

		public HVLVConsignment Consignment { get; }

		public ZString WaybillNumber => Consignment?.HVC_WaybillNumber ?? ZString.Empty;

		public ZString ImportCustomsClearanceStatusDescription => Consignment?.ImportCustomsClearanceStatusDescription ?? ZString.Empty;

		public ZString ExportCustomsClearanceStatusDescription => Consignment?.ExportCustomsClearanceStatusDescription ?? ZString.Empty;

		public ZBool Convert
		{
			get { return convert; }
			set { SetNonPersistentPropertyValue(ConvertInfo, ref convert, value); }
		}

		public ZPropertyInfo ConvertInfo => GetZPropertyInfo(nameof(Convert));

		ZBool convert;
	}
}
