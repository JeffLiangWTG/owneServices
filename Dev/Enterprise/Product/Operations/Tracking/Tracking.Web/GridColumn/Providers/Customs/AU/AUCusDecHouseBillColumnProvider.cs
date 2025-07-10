using System;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Bill = Enterprise.Customs.AU.Declaration.Business.Bill;
using JobDeclaration = Enterprise.Customs.AU.Declaration.Business.JobDeclaration;

namespace Enterprise.Tracking.Web
{
	public class AUCusDecHouseBillColumnProvider : BaseCusDecHouseBillColumnProvider
	{
		public AUCusDecHouseBillColumnProvider(JobDeclaration declaration)
		{
			this.declaration = declaration ?? throw new ArgumentNullException(nameof(declaration));
		}
		readonly JobDeclaration declaration;

		protected override void AddCountrySpecificColumns()
		{
			if (declaration.IsPost)
			{
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("F0B534D8-9E37-424D-BE97-B19BAF860730", "Parcel Post Number"), nameof(Bill.CU_BillNum)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.BillNum });
			}
			else
			{
				base.AddCountrySpecificColumns();
			}

			if (declaration.IsPartShipConsignmentReferenceRelevant)
			{
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("9DE9C52A-9192-440E-8DB2-BA2A92C4FB49", "Consign. Ref. No."), nameof(Bill.CU_fPartShipConsignmentReference)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.ConsignRefNo });
			}
		}
	}
}
