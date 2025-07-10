using System;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Bill = Enterprise.Customs.US.Business.Bill;
using JobDeclaration = Enterprise.Customs.US.Business.JobDeclaration;

namespace Enterprise.Tracking.Web
{
	public class USIMPCusDecHouseBillColumnProvider : BaseCusDecHouseBillColumnProvider
	{
		public USIMPCusDecHouseBillColumnProvider(JobDeclaration declaration)
		{
			this.declaration = declaration ?? throw new ArgumentNullException(nameof(declaration));

			if (!this.declaration.IsImport)
			{
				throw new ArgumentException("Declaration must be an import to use " + nameof(USIMPCusDecHouseBillColumnProvider));
			}
		}
		readonly JobDeclaration declaration;

		protected override bool ShouldDisplayHBLIssueDate => false;

		protected override void AddCountrySpecificColumns()
		{
			base.AddCountrySpecificColumns();

			AddToDictionary(new ZCodeFindBoxColumn(Res.GetString("8F936364-AAD7-4C4C-A210-E0DA4A16F2E1", "Bill Issuer SCAC"), nameof(Bill.US_UI_NKBillIssuerSCAC), "Lookups+USCarrierList", typeof(Bill))
			{
				ColumnKey = WebTracker.Grids.CusDecHouseBills.BillIssuerSCAC,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});
			AddToDictionary(new ZTextEditColumn(Res.GetString("1459DE62-ACEA-4385-BC01-B7318B05F955", "ISF Bill Status"), nameof(Bill.ISFBillStatus)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.ISFBillStatus });
			AddToDictionary(new ZTextEditColumn(Res.GetString("220B8574-FD74-4B73-A02E-B83EA9855687", "ISF Bill Status Description"), nameof(Bill.ISFBillStatusDescription)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.ISFBillStatusDescription });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("D17FEE84-7C69-4825-8DD0-A6D0F8CF1E53", "IT Number"), nameof(Bill.ITNumber)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.ITNumber });

			if (declaration.AreSplitDetailsRelevant)
			{
				AddToDictionary(new ZCheckBoxColumn(Res.GetString("CA4A38DB-6B26-4CD9-861A-800D27D68131", "Is Split"), nameof(Bill.US_SESplitShip)) { ColumnKey = WebTracker.Grids.CusDecHouseBills.IsSplit });
			}
		}
	}
}
