using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	public partial class AENS47 : Abstract.AENS47, IACEBIRDLineRecord
	{
		#region IACEBIRDLineRecord Members

		void IACEBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			switch (ArticlePartyTypeCode)
			{
				case "M":
					var countryOfOrigin = invoiceLine.US_UC_NKCountryOfOrigin;
					invoiceLine.JI_OA_ManufacturerAddress = GetAddressFromManufacturerID(invoiceLine.Factory, "Manufacturer/Supplier", notifications);
					invoiceLine.US_UC_NKCountryOfOrigin = countryOfOrigin;
					break;
				case "C":
					invoiceLine.JI_OA_ShipToPartyAddress = GetAddressFromEmployerIdentificationNumber(invoiceLine.Factory, "Ship To Party", notifications);
					break;
				case "S":
					invoiceLine.JI_OA_SoldToPartyAddress = GetAddressFromEmployerIdentificationNumber(invoiceLine.Factory, "Sold To Party", notifications);
					break;
				case "E":
					invoiceLine.JI_OA_ExporterAddress = GetAddressFromManufacturerID(invoiceLine.Factory, "Foreign Exporter", notifications);
					break;
			}
		}

		ZGuid GetAddressFromManufacturerID(BusinessObjectFactory factory, ZString organizationType, INotifications notifications)
		{
			if (!ArticlePartyIdentifier.IsEmpty)
			{
				var manufacturerAddress = BIRDOrganisationMatching.GetOrganisationAddress(factory, OrgMatchedCustomsRegNoType.MID, ArticlePartyIdentifier, organizationType, notifications);
				if (manufacturerAddress == null)
				{
					manufacturerAddress = OrganisationCreator.CreateManufacturerAndSendNameAddressQueryMessage(factory, ArticlePartyIdentifier);
					if (manufacturerAddress != null)
					{
						notifications.AddWarning(organizationType + ": " + ZString.Format(OrganisationCreator.ManufacturerCreated, ArticlePartyIdentifier));
					}
					else if (!ArticlePartyIdentifier.IsLettersAndNumbersOnlyOrEmpty)
					{
						notifications.AddWarning(organizationType + ": " + ZString.Format(OrganisationCreator.NoManufacturerCreatedAsMIDInvalid, ArticlePartyIdentifier));
					}
				}

				return manufacturerAddress?.PK ?? ZGuid.Empty;
			}

			return ZGuid.Empty;
		}

		ZGuid GetAddressFromEmployerIdentificationNumber(BusinessObjectFactory factory, ZString organizationType, INotifications notifications)
		{
			if (!ArticlePartyIdentifier.IsEmpty)
			{
				return BIRDOrganisationMatching.GetCustomsRecordOrMainAddressOfOrganisation(factory, OrgMatchedCustomsRegNoType.EIN, GetCompleteEINNumber(ArticlePartyIdentifier), organizationType, notifications);
			}

			return ZGuid.Empty;
		}

		ZString GetCompleteEINNumber(ZString number)
		{
			return number.Length == 10 ? number.PadRight(12, '0') : number;
		}

		#endregion
	}
}
