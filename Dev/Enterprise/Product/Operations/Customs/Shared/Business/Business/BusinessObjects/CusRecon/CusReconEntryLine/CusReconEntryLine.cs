using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	[DependentBusinessObject(typeof(CusReconEntry), nameof(CusReconEntry.CusReconEntryLines))]
	public class CusReconEntryLine : AutoCusReconEntryLine, Integration.Customs.ICusReconEntryLine
	{
		public CusReconEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CusReconEntryLineTypeDecider TypeDecider = new CusReconEntryLineTypeDecider();

		[ResourceStringData("287308C6-6DC4-47EE-9721-7870B49035D6", Caption = "Line Number")]
		public override ZShort CRL_LineNumber
		{
			get => base.CRL_LineNumber;
			set => base.CRL_LineNumber = value;
		}

		[ResourceStringData("5CBF3BA7-E9A3-406A-A741-95756F2D356B", Caption = "Invoice Line")]
		public override ZShort CRL_OriginalEntryLineNumber
		{
			get => base.CRL_OriginalEntryLineNumber;
			set => base.CRL_OriginalEntryLineNumber = value;
		}

		[ResourceStringData("9F6F5CC1-5772-458A-8A0D-0420BEE2CC02", Caption = "Description")]
		public override ZString CRL_Description
		{
			get => base.CRL_Description;
			set => base.CRL_Description = value;
		}

		[ResourceStringData("40FDA137-FA3E-4FF6-BF2D-313BF9666BB9", Caption = "Status")]
		public override ZString CRL_CustomsStatus
		{
			get => base.CRL_CustomsStatus;
			set => base.CRL_CustomsStatus = value;
		}

		[ChildEditable(true)]
		public CusReconSnapshotCollection CusReconSnapshots
		{
			get
			{
				if (cusReconSnapshots == null)
				{
					cusReconSnapshots = CreateNewCusReconSnapshotCollection();
					RegisterEditableChildObject(cusReconSnapshots);
				}
				return cusReconSnapshots;
			}
		}
		CusReconSnapshotCollection cusReconSnapshots;

		public override void Delete()
		{
			CusReconSnapshots.DeleteAll();
			base.Delete();
		}

		public virtual ZString ProductCode => ZString.Empty;

		public virtual ZString Tariff => ZString.Empty;

		public virtual ZDecimal CustomsQuantity => ZDecimal.Zero;

		public virtual ZString CustomsUnitQty => ZString.Empty;

		protected virtual CusReconSnapshotCollection CreateNewCusReconSnapshotCollection() => new CusReconSnapshotCollection(this);
	}
}
