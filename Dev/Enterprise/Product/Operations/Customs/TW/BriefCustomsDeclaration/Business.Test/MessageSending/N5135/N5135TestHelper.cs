using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	static class N5135TestHelper
	{
		public static (AsycudaManifestHeader header, AsycudaBill bill) GetAsycudaBill(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration;
			var bill = header.Bills.AddNew();
			return (header, bill);
		}

		public static (AsycudaManifestHeader, N5135MessageSendingObject) GetAsycudaBill(BusinessObjectFactory factory, int billCount)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration;
			for (var i = 1; i <= billCount; i++)
			{
				var bill = header.Bills.AddNew();
				bill.ABL_BillNumber = $"B0001{i:000}";
			}

			return (header, new N5135MessageSendingObject(header));
		}
	}
}
