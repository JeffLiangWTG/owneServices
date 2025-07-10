using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPackageAudit : AutoWhsPackageAudit, IWhsPackageAudit
	{
		public WhsPackageAudit(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			WPA_AuditCompleteTime = ZDateTimeOffset.Now;
			WPA_GS_NKAuditor = GlbStaff.CurrentUser.GS_Code;
		}

		#endregion

		#region Related Entities

		#region Order

		public WhsOrder Order
		{
			get { return Factory.Load<WhsOrder>(WPA_WD_Order); }
		}

		#endregion

		#region PackageAuditFailureLines

		[ChildEditable(true)]
		public WhsPackageAuditLineFailureCollection PackageAuditFailureLines
		{
			get
			{
				if (packageAuditFailureLines == null)
				{
					packageAuditFailureLines = new WhsPackageAuditLineFailureCollection(this);
					RegisterEditableChildObject(packageAuditFailureLines);
				}
				return packageAuditFailureLines;
			}
		}

		WhsPackageAuditLineFailureCollection packageAuditFailureLines;

		#endregion

		#endregion

		#region Properties

		#region WPA_WD_Order

		[RelatedBusinessObject("Order")]
		public override ZGuid WPA_WD_Order
		{
			get { return base.WPA_WD_Order; }
			set { base.WPA_WD_Order = value; }
		}

		#endregion

		#endregion

		#region Delete
		public override void Delete()
		{
			PackageAuditFailureLines.DeleteAll();
			base.Delete();
		}
		#endregion
	}
}
