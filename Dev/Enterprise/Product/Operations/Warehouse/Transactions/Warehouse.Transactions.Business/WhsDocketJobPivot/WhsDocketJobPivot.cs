using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketJobPivot : AutoWhsDocketJobPivot, IWhsDocketJobPivot
	{
		public WhsDocketJobPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Fetch Hints

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsDocketJobPivotFetchStrategy(this);
		}

		#endregion

		#region Related Entities

		public BusinessObject Parent
		{
			get { return Factory.Load(WV_ParentTableCode, WV_ParentId); }
		}

		#endregion

		#region Properties

		#region WV_WD_Docket

		public override ZGuid WV_WD_Docket
		{
			get { return base.WV_WD_Docket; }
			set
			{
				base.WV_WD_Docket = value;
			}
		}

		#endregion

		#region WV_DocketType

		public override ZString WV_DocketType
		{
			get { return base.WV_DocketType; }
			set
			{
				base.WV_DocketType = value;
			}
		}

		#endregion

		#region WV_ParentId

		public override ZGuid WV_ParentId
		{
			get { return base.WV_ParentId; }
			set
			{
				base.WV_ParentId = value;
			}
		}

		#endregion

		#region WV_ParentTableCode

		public override ZString WV_ParentTableCode
		{
			get { return base.WV_ParentTableCode; }
			set
			{
				base.WV_ParentTableCode = value;
			}
		}

		#endregion

		#endregion

		// Testing

		#region Testing
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			WV_DocketType = DocketType.Codes.Receive;
			WV_ParentTableCode = "JS";
			WV_ParentId = ZGuid.NewZGuid();
		}
#endif
		#endregion
	}
}
