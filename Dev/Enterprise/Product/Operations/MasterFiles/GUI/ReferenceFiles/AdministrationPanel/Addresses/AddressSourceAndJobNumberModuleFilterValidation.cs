using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.GUI
{
	public class AddressSourceAndJobNumberModuleFilterValidation : ModuleTextFilterValidation
	{
		public AddressSourceAndJobNumberModuleFilterValidation(AddressSourceAndJobNumberModuleFilter parent) : base(parent)
		{
			this.parent = parent;
		}

		readonly AddressSourceAndJobNumberModuleFilter parent;

		public void ValidateAddressSourceCode()
		{
			ValidateCalculatedProperty(parent.AddressSourceCodeInfo);
		}

		protected void CheckAddressSourceCode()
		{
			if (parent.AddressSourceCode.IsEmpty && !parent.JobNumber.IsEmpty)
			{
				parent.AddressSourceCodeInfo.AddError(Res.GetString("53B79D42-B83B-43E9-87E7-6D27F28FA692", "Can't filter job number without address source"));
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(parent.AddressSourceCodeInfo, parent.AddressSourceList);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAddressSourceCode();
		}
	}
}
