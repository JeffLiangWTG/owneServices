using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.OrgMatching;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class TransportLegCarrierAddressMatcher : OrganisationMatcher
	{
		public TransportLegCarrierAddressMatcher(BusinessObjectFactory factory, IfUnmatched unmatchedBehaviour, ISimpleLogger logger, DataContextType? type = null)
			: base(factory, unmatchedBehaviour, logger, type)
		{
			this.factory = factory;
			this.logger = logger;
		}

		readonly BusinessObjectFactory factory;
		readonly ISimpleLogger logger;

		protected override OrgCusCode GetMatchingRegistrationNumberIfOnlyRegistrationDetailIsSpecified(IOrgHeaderForMatching orgMatchingData, ZString? shortCode)
		{
			OrgCusCode result = null;
			var customsCode = orgMatchingData.CustomsCodes?.FirstOrDefault(x => x.OK_CodeType == OrgCusCode.CodeTypes.CarrierCode);

			if (customsCode != null && !customsCode.OK_CustomsRegNo.IsEmpty)
			{
				var cusCodes = factory.Load<OrgCusCode>(OrgCusCodeValidation.GetDuplicateQuery(customsCode.OK_RN_NKCodeCountry, customsCode.OK_CodeType, customsCode.OK_CustomsRegNo)).Where(u => u.Organisation?.OH_IsActive ?? false).Take(2).ToArray();
				if (cusCodes.Length == 1)
				{
					result = cusCodes[0];
				}
				else if (cusCodes.Length > 1)
				{
					var warning = Res.GetString("ce690ee8-6a86-471a-9e2c-44e1477ad7fd", "Unable to determine a match as multiple organizations were matched with registration detail (Country/Region='{0}', Type='{1}', Number='{2}').", customsCode.OK_RN_NKCodeCountry, customsCode.OK_CodeType, customsCode.OK_CustomsRegNo);
					logger.Log(LogType.Warning, warning);
				}
			}

			return result ?? base.GetMatchingRegistrationNumberIfOnlyRegistrationDetailIsSpecified(orgMatchingData, shortCode);
		}
	}
}
