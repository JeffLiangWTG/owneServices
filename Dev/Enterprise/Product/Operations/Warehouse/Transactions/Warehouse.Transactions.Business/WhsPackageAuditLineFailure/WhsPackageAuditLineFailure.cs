using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	[DependentBusinessObject(typeof(WhsPackageAudit), "PackageAuditFailureLines")]
	public class WhsPackageAuditLineFailure : AutoWhsPackageAuditLineFailure, IWhsPackageAuditLineFailure
	{
		public WhsPackageAuditLineFailure(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Related Entities

		#region PackageAudit

		public WhsPackageAudit PackageAudit
		{
			get { return Factory.Load<WhsPackageAudit>(WPF_WPA_WhsPackageAudit); }
		}

		#endregion

		#region Product

		public OrgSupplierPart Product
		{
			get { return Factory.Load<OrgSupplierPart>(WPF_OP); }
		}

		#endregion

		#endregion

		#region Properties

		#region WPF_OP

		[RelatedBusinessObject("Product")]
		public override ZGuid WPF_OP
		{
			get { return base.WPF_OP; }
			set { base.WPF_OP = value; }
		}

		#endregion

		#region WPF_AuditedQty

		public override ZDecimal WPF_AuditedQty
		{
			get { return base.WPF_AuditedQty; }

			set
			{
				var existingValue = base.WPF_AuditedQty;
				base.WPF_AuditedQty = value;
				if (value != existingValue)
				{
					Validation.ValidateWPF_ExpectedQty();
				}
			}
		}

		#endregion

		#region WPF_ExpectedQty

		public override ZDecimal WPF_ExpectedQty
		{
			get { return base.WPF_ExpectedQty; }

			set
			{
				var existingValue = base.WPF_ExpectedQty;
				base.WPF_ExpectedQty = value;
				if (value != existingValue)
				{
					Validation.ValidateWPF_AuditedQty();
				}
			}
		}

		#endregion

		#endregion

	}
}
