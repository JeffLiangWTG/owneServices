using System;
using System.Xml.Schema;

using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;

using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class DebtorBalanceValueObjectDataAdapter : ValueObjectDataAdapter<DebtorBalanceRecordForExport, Xsd.DebtorBalance>
	{
		public override string RootCollectionElementName
		{
			get { return "DebtorBalances"; }
		}

		public override string RootElementName
		{
			get { return "DebtorBalance"; }
		}

		public override XmlSchema Schema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.SingleDebtorBalanceSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.DebtorBalancesSchema; }
		}

		protected override void ExportToValueObjectCore(DebtorBalanceRecordForExport debtorBalanceRec, Xsd.DebtorBalance debtorBalanceXSD, IValueObjectExportContext context)
		{
			if (debtorBalanceRec.Debtor != null)
			{
				debtorBalanceXSD.Debtor = ExportDebtorDetails(debtorBalanceRec, context);

				debtorBalanceXSD.OutstandingBalance = Xsd.FinancialValue.FromAmountAndCurrency(debtorBalanceRec.OutstandingBalanceAmount, GlbCompany.CurrentCompany.LocalCurrency);
				debtorBalanceXSD.OutstandingWIPAmount = Xsd.FinancialValue.FromAmountAndCurrency(debtorBalanceRec.WIPsAmount, GlbCompany.CurrentCompany.LocalCurrency);
			}
		}

		Xsd.Organisation ExportDebtorDetails(DebtorBalanceRecordForExport debtorBalanceRec, IValueObjectExportContext context)
		{
			Xsd.Organisation debtorXSD = OrganisationDataAdapter.ExportToValueObject(debtorBalanceRec.Debtor, context);

			debtorXSD.OrganisationDetails.IsActiveClient = debtorBalanceRec.Debtor.OH_IsActive;
			debtorXSD.OrganisationDetails.IsActiveClientSpecified = true;

			Xsd.AccountsReceivable accRevXSD = new Xsd.AccountsReceivable();

			OrgRelatedPartyCompanySpecificCollection relatedParties = new OrgRelatedPartyCompanySpecificCollection(debtorBalanceRec.Debtor, debtorBalanceRec.Debtor.Factory);
			relatedParties.Load();

			OrgRelatedParty settlementGrp = relatedParties.GetRelatedParty(RelatedPartyTypeList.Codes.ARSettlementGroup, RelatedPartyDirectionList.Codes.AR);

			if (settlementGrp != null)
			{
				accRevXSD.SettlementDetails.SettlementGroup = ExportARSettlementGroup(debtorBalanceRec.Debtor.ARSettlementGroup);
			}

			if (debtorBalanceRec.Debtor.MiscServ.OM_OJ_ARDebtorGroup != ZGuid.Empty)
			{
				OrgDebtorGroup debtorGrp = debtorBalanceRec.Debtor.Factory.Load<OrgDebtorGroup>(debtorBalanceRec.Debtor.MiscServ.OM_OJ_ARDebtorGroup);

				if (debtorGrp != null)
				{
					accRevXSD.AccountGroup = debtorGrp.OJ_Code;
				}
			}

			accRevXSD.CreditLimit = ZArchitecture.Core.Utilities.Round(debtorBalanceRec.Debtor.MiscServ.OM_ARCreditLimit, GlbCompany.CurrentCompany.LocalCurrency.Decimals);
			accRevXSD.CreditLimitSpecified = true;

			accRevXSD.CreditOnHold = debtorBalanceRec.Debtor.CompanyData.OB_AROnCreditHold;
			accRevXSD.CreditOnHoldSpecified = true;

			if (accRevXSD.IsSpecified)
			{
				debtorXSD.OrganisationDetails.AccountsReceivables.Add(accRevXSD);
			}

			return debtorXSD;
		}

		Xsd.OrganisationDetail ExportARSettlementGroup(OrgHeader settlementGroupOrg)
		{
			Xsd.Organisation settlementGrpXSD = new Xsd.Organisation();
			OrganisationDataAdapter.ExportToValueObject(settlementGroupOrg, settlementGrpXSD, new ValueObjectExportContext(new NotificationBuffer()));

			settlementGrpXSD.OrganisationDetails.EDICode = settlementGroupOrg.OH_Code;
			settlementGrpXSD.OrganisationDetails.OwnerCode = settlementGroupOrg.OH_Code;
			return settlementGrpXSD.OrganisationDetails;
		}

		protected override void ImportFromValueObjectCore(DebtorBalanceRecordForExport bizObj, Xsd.DebtorBalance value, IValueObjectImportContext context)
		{
			throw new NotSupportedException("Debtor 's Balances Import is currently not Supported");
		}

		OrganisationValueObjectDataAdapter OrganisationDataAdapter
		{
			get { return organisationDataAdapter ?? (organisationDataAdapter = new OrganisationValueObjectDataAdapter()); }
		}
		OrganisationValueObjectDataAdapter organisationDataAdapter;
	}
}

