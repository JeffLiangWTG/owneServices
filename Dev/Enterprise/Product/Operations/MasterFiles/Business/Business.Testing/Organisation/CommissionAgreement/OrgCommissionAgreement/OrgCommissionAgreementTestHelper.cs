using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class OrgCommissionAgreementTestHelper
	{
		public static OrgOpportunity GetNewEffectiveOpportunity(BusinessObjectFactory factory)
		{
			var client = factory.NewWithValidTestData<OrgHeader>();
			return GetNewEffectiveOpportunity(factory, client);
		}

		public static OrgOpportunity GetNewEffectiveOpportunity(BusinessObjectFactory factory, OrgHeader client)
		{
			var result = factory.NewWithValidTestData<OrgOpportunity>();
			using (result.GetValidationSuspender())
			{
				result.P8_OH = client.PK;
				result.P8_Status = OrganisationsDataRegistry.Instance.OpportunityStatus.Value.Cast<OpportunityStatus>().First(x => x.EffectiveAgreement).Code;
			}

			return result;
		}

		public static OrgCommissionAgreement AddNewEffectiveAgreement(OrgOpportunity opportunity, OrgHeader customer)
		{
			var agreement = opportunity.ApprovedCommissionAgreements.AddNew();
			using (agreement.GetValidationSuspender())
			{
				agreement.CA0_OH_Customer = customer.PK;
				agreement.CA0_CommissionBasis = CommissionBasisType.Codes.REV;
				agreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
				agreement.CA0_EffectiveDate = ZDateTime.MinSmallDateTimeValue.Date;
			}

			return agreement;
		}

		public static OrgCommissionAgreement AddNewEffectiveAgreement(OrgOpportunity opportunity, OrgHeader customer, ZString product, ZString service, ZString subModule)
		{
			var agreement = AddNewEffectiveAgreement(opportunity, customer);
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement, product, service, subModule);

			return agreement;
		}

		public static OrgCommissionAgreement AddNewEffectiveAgreementForAllItems(OrgOpportunity opportunity, OrgHeader customer)
		{
			return AddNewEffectiveAgreement(opportunity, customer, OrgCommissionAgreementItemLookups.AllProductsCode, OrgCommissionAgreementItemLookups.AllServicesCode, OrgCommissionAgreementItemLookups.AllSubModulesCode);
		}

		public static OrgCommissionAgreement GetNewEffectiveAgreement(BusinessObjectFactory factory, OrgHeader customer)
		{
			var opportunity = GetNewEffectiveOpportunity(factory, customer);
			return AddNewEffectiveAgreement(opportunity, customer);
		}

		public static OrgCommissionAgreement GetNewEffectiveAgreement(BusinessObjectFactory factory, OrgHeader customer, ZString product, ZString service, ZString subModule)
		{
			var agreement = GetNewEffectiveAgreement(factory, customer);
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement, product, service, subModule);

			return agreement;
		}

		public static OrgCommissionAgreement GetNewEffectiveAgreementForAllItems(BusinessObjectFactory factory, OrgHeader customer)
		{
			return GetNewEffectiveAgreement(factory, customer, OrgCommissionAgreementItemLookups.AllProductsCode, OrgCommissionAgreementItemLookups.AllServicesCode, OrgCommissionAgreementItemLookups.AllSubModulesCode);
		}

		public static OrgCommissionAgreement GetNewEffectiveAgreementForAirShipment(BusinessObjectFactory factory, OrgHeader customer)
		{
			var agreement = GetNewEffectiveAgreement(factory, customer);
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItemWithConditions(agreement, "SHP", "ALL", "ALL", "AIR", "", "");

			return agreement;
		}

		public static OrgCommissionAgreementRecipient AddPercentageRecipient(OrgCommissionAgreement agreement, ZString staffCode, ZDecimal percentage)
		{
			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = staffCode;
			recipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient.CAR_Share = 1;
			var rate = recipient.Rates.AddNew();
			rate.CAT_CommissionPercentage = percentage;

			return recipient;
		}

		public static OrgCommissionAgreementRecipient AddPercentageRecipient(OrgCommissionAgreement agreement, GlbStaff staff, ZDecimal percentage)
		{
			return AddPercentageRecipient(agreement, staff.GS_Code, percentage);
		}

		public static OrgCommissionAgreementItem SetSingleCommissionAgreementOverallItem(OrgCommissionAgreement agreement, ZString product, ZString service, ZString subModule)
		{
			agreement.ProductItems.DeleteAll();
			var productItem = agreement.ProductItems.AddNew(true, product);

			productItem.ChildServiceItems.DeleteAll();
			var serviceItem = productItem.ChildServiceItems.AddNew(true, service);

			serviceItem.ChildSubModuleItems.DeleteAll();
			return serviceItem.ChildSubModuleItems.AddNew(true, subModule);
		}

		public static OrgCommissionAgreementItem SetSingleCommissionAgreementOverallItemWithConditions(OrgCommissionAgreement agreement, ZString product, ZString service, ZString subModule, ZString mode, ZString origin, ZString destination)
		{
			agreement.ProductItems.DeleteAll();
			var productItem = agreement.ProductItems.AddNew(true, product);

			productItem.ChildServiceItems.DeleteAll();
			var serviceItem = productItem.ChildServiceItems.AddNew(true, service);

			serviceItem.ChildSubModuleItems.DeleteAll();
			serviceItem.ChildSubModuleItems.AddNew(true, subModule);

			productItem.ConditionCollection.DeleteAll();
			var condition = productItem.ConditionCollection.AddNew();
			condition.CIC_Mode = mode;
			condition.CIC_RL_NKOrigin = origin;
			condition.CIC_RL_NKDestination = destination;

			return productItem;
		}

		public static OrgCommissionAgreementItem AddInclusionCommissionAgreementOverallItemWithConditions(OrgCommissionAgreement agreement, ZString product, ZString mode, ZString origin, ZString destination)
		{
			var productItem = agreement.ProductItems.FirstOrDefault(x => x.CAI_IsInclude && x.CAI_Code == product);
			bool isNewItem = false;
			if (productItem == null)
			{
				productItem = agreement.ProductItems.AddNew(true, product);
				productItem.ChildServiceItems.DeleteAll();
				isNewItem = true;
			}

			var condition = productItem.ConditionCollection.Count > 0 && isNewItem
				? productItem.ConditionCollection[0]
				: productItem.ConditionCollection.AddNew();

			condition.CIC_Mode = mode;
			condition.CIC_RL_NKOrigin = origin;
			condition.CIC_RL_NKDestination = destination;

			return productItem;
		}

		public static OrgCommissionAgreementItem AddInclusionCommissionAgreementOverallItem(OrgCommissionAgreement agreement, ZString product, ZString service, ZString subModule)
		{
			var productItem = agreement.ProductItems.FirstOrDefault(x => x.CAI_IsInclude && x.CAI_Code == product);
			if (productItem == null)
			{
				productItem = agreement.ProductItems.AddNew(true, product);
				productItem.ChildServiceItems.DeleteAll();
			}

			var serviceItem = productItem.ChildServiceItems.FirstOrDefault(x => x.CAI_IsInclude && x.CAI_Code == service);
			if (serviceItem == null)
			{
				serviceItem = productItem.ChildServiceItems.AddNew(true, service);
				serviceItem.ChildSubModuleItems.DeleteAll();
			}

			return serviceItem.ChildSubModuleItems.AddNew(true, subModule);
		}

		public static OrgCommissionAgreementItem AddExclusionCommissionAgreementOverallItem(OrgCommissionAgreement agreement, ZString product, ZString service, ZString subModule)
		{
			var productItem = agreement.ProductItems.FirstOrDefault(x => x.CAI_IsInclude && x.CAI_Code == product);
			if (productItem == null)
			{
				productItem = agreement.ProductItems.AddNew(true, product);
				productItem.ChildServiceItems.DeleteAll();
			}

			var serviceItem = productItem.ChildServiceItems.FirstOrDefault(x => x.CAI_IsInclude && x.CAI_Code == service);
			if (serviceItem == null)
			{
				serviceItem = productItem.ChildServiceItems.AddNew(true, service);
				serviceItem.ChildSubModuleItems.DeleteAll();
			}

			return serviceItem.ChildSubModuleItems.AddNew(false, subModule);
		}

		public static OrgCommissionAgreementItem AddExclusionCommissionAgreementOverallItem(OrgCommissionAgreement agreement, ZString product, ZString service)
		{
			var productItem = agreement.ProductItems.FirstOrDefault(x => x.CAI_IsInclude && x.CAI_Code == product);
			if (productItem == null)
			{
				productItem = agreement.ProductItems.AddNew(true, product);
				productItem.ChildServiceItems.DeleteAll();
			}

			return productItem.ChildServiceItems.AddNew(false, service);
		}

		public static OrgCommissionAgreementItem AddExclusionCommissionAgreementOverallItem(OrgCommissionAgreement agreement, ZString product)
		{
			return agreement.ProductItems.AddNew(false, product);
		}
	}
}
