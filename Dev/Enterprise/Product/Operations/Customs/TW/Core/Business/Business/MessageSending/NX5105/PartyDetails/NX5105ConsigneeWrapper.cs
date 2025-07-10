using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	class NX5105ConsigneeWrapper : NX5105PartyDetailsWrapper
	{
		public NX5105ConsigneeWrapper(JobDeclaration declaration, TWConsignorOrConsigneeAddress consignor)
			: base(consignor.Address, declaration.CusEntryInstruction.Warehouse2)
		{
			this.typeCode = GetTypeCodeBasedOnOrgCusCode(consignor.E2_GovRegNumType);
			this.id = SharedHelper.GetIDStartWithNO(consignor.E2_GovRegNum, typeCode);
		}

		readonly string typeCode;

		readonly string id;

		protected override ZString IDCore => id;

		protected override ZString TypeCodeCore => typeCode;
	}
}
