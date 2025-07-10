using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business
{
	public class OrgCusCodeValidation : EU.Business.OrgCusCodeValidation, Integration.Customs.TR.IOrgCusCodeValidation
	{
		public OrgCusCodeValidation(OrgCusCode parent) : base(parent)
		{
		}
		protected override void CheckOK_CustomsRegNo()
		{
			base.CheckOK_CustomsRegNo();
			var cusRegCode = Parent.OK_CustomsRegNo;
			if (Parent.OK_CodeType != OrgCusCode.CodeTypes.SupplierCode)
			{
				if (ValidateWarehouseCodeLength(cusRegCode))
				{
					if (Parent.OK_CodeType == OrgCusCode.CodeTypes.WarehouseControlledPremisesID)
					{
						Parent.OK_CustomsRegNoInfo.AddError(Res.GetString("86BCEEC6-153B-4DAB-B0FB-8626F177AFA6", "A Customs Bonded Warehouse Code must be at least 5 characters long and at most 10 characters long"));
					}

					if (Parent.OK_CodeType == OrgCusCode.CodeTypes.TerminalControlledPremisesID)
					{
						Parent.OK_CustomsRegNoInfo.AddError(Res.GetString("C5A3F570-24C9-4B2B-9DD5-A7F39F77EB53", "A Customs Bonded Terminal Code must be at least 5 characters long and at most 10 characters long"));
					}
				}
			}
			else if (cusRegCode.Length != 13)
			{
				Parent.OK_CustomsRegNoInfo.AddError(Res.GetString("11ABE7C9-B4AE-4426-8510-4238A15CF419", "A Customs Bonded CSC type (Customs Supplier Code) must be 13 characters long."));
			}
		}

		bool ValidateWarehouseCodeLength(ZString cusRegCode) => cusRegCode.Length > 10 || cusRegCode.Length < 5;
	}
}
