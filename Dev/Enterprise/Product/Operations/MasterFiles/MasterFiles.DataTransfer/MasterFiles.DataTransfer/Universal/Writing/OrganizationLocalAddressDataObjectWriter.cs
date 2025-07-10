using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class OrganizationLocalAddressDataObjectWriter : DataObjectWriter<OrgTranslatedAddress, OrganizationLocalAddress>
	{
		public OrganizationLocalAddressDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override OrganizationLocalAddress PopulateDataObject(OrgTranslatedAddress sourceBO)
		{
			var translatedAddress = Argument.NotNull(sourceBO, nameof(sourceBO));

			return new OrganizationLocalAddress
			{
				CompanyName = translatedAddress.CompanyName,
				Address1 = translatedAddress.Address1,
				Address2 = translatedAddress.Address2,
				City = translatedAddress.City,
				State = translatedAddress.StateCode,
				Postcode = translatedAddress.Postcode,
				Language = new Language { Code = translatedAddress.Language, Description = translatedAddress.LanguageDescription }
			};
		}
	}
}
