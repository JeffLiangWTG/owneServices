using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class DeclarationDataObjectWriter : DataTransfer.Universal.DeclarationDataObjectWriter
	{
		protected internal DeclarationDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override ZString GetLocationOfGoods(BaseJobDeclaration declarationBO) => ((JobDeclaration)declarationBO).JE_Cal_GoodsLocation;

		protected override void PopulateExtraOrganisation(BaseJobDeclaration declarationBO, Shipment declarationData, bool keepExistingData)
		{
			base.PopulateExtraOrganisation(declarationBO, declarationData, keepExistingData);

			var declaration = (JobDeclaration)declarationBO;
			declarationData.AddOrgAddress(writeManager, declaration.NotifyParty, Constants.AddressType.DeliveryNotificationParty);
		}
	}
}
