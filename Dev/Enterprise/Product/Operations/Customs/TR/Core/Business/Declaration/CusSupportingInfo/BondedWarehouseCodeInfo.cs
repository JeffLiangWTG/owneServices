using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class BondedWarehouseCodeInfo : Customs.Business.CusSupportingInfo
	{
		public BondedWarehouseCodeInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.BondedWarehouse;
			CSI_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
		}

		public new JobDeclaration Parent => base.Parent as JobDeclaration;

		public override void OnSaving()
		{
			if (CSI_CustomsOffice.IsEmpty)
			{
				this.Delete();
			}

			base.OnSaving();
		}
	}
}

