using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[DependentBusinessObject(typeof(CusSeaManOBLHeader), "Details")]
	public class CusSeaManOBLDetail : AutoCusSeaManOBLDetail
	{
		public CusSeaManOBLDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CusSeaManOBLDetailTypeDecider TypeDecider = new CusSeaManOBLDetailTypeDecider();

		#region Fetch Strategy

		protected class CusSeaManOBLDetailFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			public CusSeaManOBLDetailFetchStrategy(CusSeaManOBLDetail detail)
				: base(detail)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(CusUnderbondSchema.C4_ParentID, detail.PK);
			}

			CusSeaManOBLDetail detail
			{
				get { return (CusSeaManOBLDetail)base.BusinessObject; }
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CusSeaManOBLDetailFetchStrategy(this);
		}

		#endregion

		public CusSeaManOBLHeader Header
		{
			get
			{
				return Factory.Load<CusSeaManOBLHeader>(BD_BO);
			}
		}

		#region Business Object Overrides

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("b0fbd756-b4fc-4c49-9e01-5b8b7d0a484f", "Container ({0}) {1}", Header.BO_OceanBill, BD_ContainerNumber).Trim();
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion
	}
}
