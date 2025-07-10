using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class AddInfoBill : AddInfo
	{
		public AddInfoBill(ZPropertyInfo addInfoPropertyInfo)
			: base(addInfoPropertyInfo)
		{
		}

		public new Bill Parent
		{
			get { return (Bill)base.Parent; }
		}

		public Bill Bill
		{
			get { return Parent; }
		}

		public new AddInfoBillLookups Lookups
		{
			get { return (AddInfoBillLookups)base.Lookups; }
		}

		public new AddInfoBillValidation Validation
		{
			get { return (AddInfoBillValidation)base.Validation; }
		}

		protected override SchemaColumn[] ColumnsForFastSearch
		{
			get { return new SchemaColumn[] { USAddInfoSchema.US_ITDate }; }
		}

		protected override USAddInfoLookups GetNewLookups()
		{
			return new AddInfoBillLookups(this);
		}

		protected override USAddInfoValidation GetNewValidation()
		{
			if (Declaration == null || Declaration.IsExport)
			{
				return new AddInfoBillValidation(this);
			}
			else if (Declaration.IsExWarehouse)
			{
				return new ExWarehouseAddInfoBillValidation(this);
			}
			else if (Declaration.IsFTZAdmission)
			{
				return new FTZAddInfoBillValidation(this);
			}
			else
			{
				return new FormalImportAddInfoBillValidation(this);
			}
		}

		protected override bool IsExportCore
		{
			get { return Parent.Declaration != null && Parent.Declaration.IsExport; }
		}

		public override ZString US_UI_NKBillIssuerSCAC
		{
			get { return base.US_UI_NKBillIssuerSCAC; }
			set
			{
				base.US_UI_NKBillIssuerSCAC = value;
				if (Parent.Declaration != null)
				{
					Parent.Declaration.MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool US_ExpressTracking
		{
			get { return base.US_ExpressTracking; }
			set
			{
				var oldValue = US_ExpressTracking;
				base.US_ExpressTracking = value;
				if (oldValue != value && !IsCopying && Parent.Declaration != null)
				{
					Parent.Declaration.MarkAsNeedingValidation();
				}
			}
		}

		protected override ZString GetTransportMode()
		{
			return Parent.Declaration != null ? Parent.Declaration.JE_TransportMode : (ZString)Core.Constants.TransportModes.Unknown;
		}
	}
}
