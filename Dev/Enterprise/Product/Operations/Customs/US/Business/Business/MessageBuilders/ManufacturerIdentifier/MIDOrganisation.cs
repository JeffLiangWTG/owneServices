using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.US;
using static Enterprise.Integration.Customs.US;

namespace Enterprise.Customs.US.Business
{
	public class MIDOrganisation : IMIDOrganisation
	{
		public BusinessObject CreateMIDOrganizationIfNecessary(ZString midNo, BusinessObjectFactory factory)
		{
			OrgAddress result = null;
			if (USCustomsDataRegistry.Instance.CreateMIDOrganizationOnUnmatchedImport.GetValueWithoutFallback(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty) && !AddressParser.GetAddressPKFromMatchingMIDCode(midNo, factory).IsValid)
			{
				result = OrganisationCreator.CreateManufacturerAndSendNameAddressQueryMessage(factory, midNo);
				if (result != null)
				{
					result.OA_Code = ((ZString)AutocreatefromMID.FullName).Left(OrgAddress.Schema.OA_CodeMaxLength);
				}
			}
			return result;
		}
	}
}
