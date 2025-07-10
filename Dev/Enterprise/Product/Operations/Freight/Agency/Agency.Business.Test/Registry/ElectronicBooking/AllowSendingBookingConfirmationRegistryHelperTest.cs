using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	sealed class AllowSendingBookingConfirmationRegistryHelperTest : TestCaseWithFactory
	{
		public void TestIsAllowed()
		{
			var agencyRegistryWrapper = ObjectFactory.Get<IAgencyRegistry>();

			var principal1 = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", "AU");
			principal1.OH_IsShippingProvider = true;
			principal1.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			var principal2 = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "BBB", "AU");
			principal2.OH_IsShippingProvider = true;
			principal2.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			Factory.Save();

			var principals = new AllowSendingBookingConfirmationCollection();
			AddNewAllowSendingBookingConfirmation(principals, principal1.PK, true);

			using (AgencyRegistry.Instance.AllowSendingBookingConfirmationEDIAfterATD.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, principals))
			{
				AssertEquals(true, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal1.PK));
				AssertEquals(false, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal2.PK));
			}
			using (AgencyRegistry.Instance.AllowSendingBookingConfirmationEDIAfterATD.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, principals))
			{
				AssertEquals(true, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal1.PK));
				AssertEquals(false, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal2.PK));
			}

			AddNewAllowSendingBookingConfirmation(principals, ZGuid.Empty, true);

			using (AgencyRegistry.Instance.AllowSendingBookingConfirmationEDIAfterATD.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, principals))
			{
				AssertEquals(true, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal1.PK));
				AssertEquals(true, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal2.PK));
			}
			using (AgencyRegistry.Instance.AllowSendingBookingConfirmationEDIAfterATD.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, principals))
			{
				AssertEquals(true, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal1.PK));
				AssertEquals(true, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal2.PK));
			}

			principals.RemoveAndDeleteAll();
			AddNewAllowSendingBookingConfirmation(principals, principal1.PK, true);
			AddNewAllowSendingBookingConfirmation(principals, ZGuid.Empty, false);

			using (AgencyRegistry.Instance.AllowSendingBookingConfirmationEDIAfterATD.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, principals))
			{
				AssertEquals(false, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal1.PK));
				AssertEquals(false, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal2.PK));
			}
			using (AgencyRegistry.Instance.AllowSendingBookingConfirmationEDIAfterATD.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, principals))
			{
				AssertEquals(false, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal1.PK));
				AssertEquals(false, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal2.PK));
			}

			principals.RemoveAndDeleteAll();
			AddNewAllowSendingBookingConfirmation(principals, principal2.PK, true);

			using (AgencyRegistry.Instance.AllowSendingBookingConfirmationEDIAfterATD.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, principals))
			{
				AssertEquals(false, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal1.PK));
				AssertEquals(true, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal2.PK));
			}
			using (AgencyRegistry.Instance.AllowSendingBookingConfirmationEDIAfterATD.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, principals))
			{
				AssertEquals(false, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal1.PK));
				AssertEquals(true, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal2.PK));
			}

			AddNewAllowSendingBookingConfirmation(principals, ZGuid.Empty, true);

			using (AgencyRegistry.Instance.AllowSendingBookingConfirmationEDIAfterATD.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, principals))
			{
				AssertEquals(true, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal1.PK));
				AssertEquals(true, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal2.PK));
			}
			using (AgencyRegistry.Instance.AllowSendingBookingConfirmationEDIAfterATD.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, principals))
			{
				AssertEquals(true, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal1.PK));
				AssertEquals(true, agencyRegistryWrapper.IsDeliveryAgentAllowedSendingBookingConfirmationEDIAfterATD(principal2.PK));
			}
		}

		#region Implementation

		AllowSendingBookingConfirmationCollection AddNewAllowSendingBookingConfirmation(AllowSendingBookingConfirmationCollection principals, ZGuid principalPK, bool enabled)
		{
			var principal = principals.AddNew();
			principal.PrincipalPK = principalPK;
			principal.Enabled = enabled;

			return principals;
		}

		#endregion
	}
}
