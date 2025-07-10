using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transit.Business
{
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	public class WhsItemTransferLine : AutoWhsItemTransferLine, IWhsItemTransferLine
	{
		public WhsItemTransferLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region PackageState

		[RelatedBusinessObject("PackageState")]
		public override ZGuid WTF_WPS_PackageState
		{
			get { return base.WTF_WPS_PackageState; }
			set { base.WTF_WPS_PackageState = value; }
		}

		public WhsItemPackageState PackageState
		{
			get { return Factory.Load<WhsItemPackageState>(WTF_WPS_PackageState); }
		}

		#endregion

		#region From

		[RelatedBusinessObject("From")]
		public override ZGuid WTF_WL_From
		{
			get { return base.WTF_WL_From; }
			set { base.WTF_WL_From = value; }
		}

		public WhsLocation From
		{
			get { return Factory.Load<WhsLocation>(WTF_WL_From); }
		}

		#endregion

		#region To

		[RelatedBusinessObject("To")]
		public override ZGuid WTF_WL_To
		{
			get { return base.WTF_WL_To; }
			set { base.WTF_WL_To = value; }
		}

		public WhsLocation To
		{
			get { return Factory.Load<WhsLocation>(WTF_WL_To); }
		}

		#endregion

		#region TransferHeader

		[RelatedBusinessObject("TransferHeader")]
		public override ZGuid WTF_WTH_TransitTransferHeader
		{
			get { return base.WTF_WTH_TransitTransferHeader; }
			set { base.WTF_WTH_TransitTransferHeader = value; }
		}

		public WhsItemTransferHeader TransferHeader
		{
			get { return Factory.Load<WhsItemTransferHeader>(WTF_WTH_TransitTransferHeader); }
		}

		#endregion
	}
}
