using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.N5203
{
	class Consignor : PartyDetails
	{
		public Consignor(JobDeclaration declaration, TWConsignorOrConsigneeAddress consignor) : base(declaration, consignor.Organisation)
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
