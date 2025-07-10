//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgWhsClientAccountAssociationValidation
//
//    This class should be used for overriding validation in AutoOrgWhsClientAccountAssociationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgWhsClientAccountAssociationValidation : AutoOrgWhsClientAccountAssociationValidation
	{
		public OrgWhsClientAccountAssociationValidation(AutoOrgWhsClientAccountAssociation parent)
			: base(parent)
		{
		}

		protected override void CheckOWC_WW_Warehouse()
		{
			base.CheckOWC_WW_Warehouse();
			CheckClientWhsSalesChannelCombination(Parent.OWC_WW_WarehouseInfo);
		}

		protected override void CheckOWC_OAN_CarrierAccount()
		{
			base.CheckOWC_OAN_CarrierAccount();
			CheckClientWhsSalesChannelCombination(Parent.OWC_OAN_CarrierAccountInfo);
		}

		protected override void CheckOWC_WSH_SalesChannel()
		{
			base.CheckOWC_WSH_SalesChannel();
			CheckClientWhsSalesChannelCombination(Parent.OWC_WSH_SalesChannelInfo);
		}

		protected override void CheckOWC_OH_Client()
		{
			base.CheckOWC_OH_Client();
			CheckClientWhsSalesChannelCombination(Parent.OWC_OH_ClientInfo);
		}

		void CheckClientWhsSalesChannelCombination(ZPropertyInfo info)
		{
			if (info.Value != null)
			{
				var associationQuery = new ZQuery(OrgWhsClientAccountAssociationSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				associationQuery.AddToFilter(new ZQuery(OrgWhsClientAccountAssociationSchema.OWC_OH_Client, Parent.OWC_OH_Client));
				associationQuery.AddToFilter(new ZQuery(OrgWhsClientAccountAssociationSchema.OWC_WW_Warehouse, Parent.OWC_WW_Warehouse));
				associationQuery.AddToFilter(new ZQuery(OrgWhsClientAccountAssociationSchema.OWC_WSH_SalesChannel, Parent.OWC_WSH_SalesChannel));

				var accountsQuery = new ZQuery(OrgCarrierAccountSchema.OAN_OH_Carrier, Parent.CarrierAccount?.OAN_OH_Carrier ?? ZGuid.Empty);
				var accounts = Parent.Factory.Load<OrgCarrierAccount>(accountsQuery);
				associationQuery.AddToFilter(OrgWhsClientAccountAssociationSchema.OWC_OAN_CarrierAccount, accounts.Select(acc => acc.PK));
				if (Parent.Factory.Load<OrgWhsClientAccountAssociation>(associationQuery).Any())
				{
					info.AddError(Res.GetString("5f365b88-1fb1-46b5-96d2-c405494ee2a8", "Must have unique Client, Warehouse, Carrier and Sales Channel combination."));
				}
			}
		}

		protected override void CheckOWC_BillingType()
		{
			base.CheckOWC_BillingType();

			MandatoryValidation.CheckEntered(Parent.OWC_BillingTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OWC_BillingTypeInfo, Parent.Lookups.BillingTypesList);
			if (!Parent.OWC_BillingTypeInfo.HasErrors())
			{
				if (Parent.OWC_BillingType == CarrierBillingType.BillReceiver)
				{
					Parent.OWC_BillingTypeInfo.AddWarning(Res.GetString(
						"9b60a523-ee52-4393-ac94-2b80e1e9b255",
						"The billing type selected is Bill Receiver. Please note that the bill to party organization specified in the bill to account number will be ignored as the billing party will be taken from the job."));
				}
			}
		}

		protected override void CheckOWC_OAN_BillToCarrierAccount()
		{
			base.CheckOWC_OAN_BillToCarrierAccount();
			if (!Parent.OWC_OAN_BillToCarrierAccountInfo.HasErrors())
			{
				if ((Parent.OWC_BillingType == CarrierBillingType.BillReceiver || Parent.OWC_BillingType == CarrierBillingType.BillThirdParty)
					&& Parent.OWC_OAN_BillToCarrierAccount.IsEmpty)
				{
					Parent.OWC_OAN_BillToCarrierAccountInfo.AddError(Res.GetString(
						"a9516cd4-94f5-4468-8d1b-0895d8946c00",
						"A Bill To Carrier Account must be specified if the billing type is Bill Receiver or Bill Third Party. Please select an appropriate Carrier Account to bill or select Bill Sender as the Billing Type."));
				}
			}
		}
	}
}
