using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.N5203
{
	class Consignee : PartyDetails
	{
		public Consignee(JobDeclaration declaration, TWConsignorOrConsigneeAddress consignee) : base(declaration, consignee.Organisation)
		{
			this.typeCode = GetTypeCodeBasedOnOrgCusCode(consignee.E2_GovRegNumType);
			this.id = SharedHelper.GetIDStartWithNO(consignee.E2_GovRegNum, typeCode);
		}

		readonly string typeCode;
		readonly string id;

		protected override ZString IDCore => id;

		protected override ZString TypeCodeCore => typeCode;
	}
}
