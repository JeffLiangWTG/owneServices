using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public partial class JobComInvoiceLineLookups : Customs.Business.JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		public override ICodeDescriptionPairList Procedures
		{
			get
			{
				var declaration = InvoiceLine?.Declaration;
				var countryCode = declaration?.CountryCode ?? GlbCompany.CurrentCompany.Country.Code;
				var shipmentType = declaration?.JE_MessageType ?? ZString.Empty;
				var group = InvoiceLine?.EntryInstruction?.CEI_Style ?? ZString.Empty;

				var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_RefCusProcedures", countryCode, shipmentType, group);
				return Factory.GetCachedValue<ICodeDescriptionPairList>(cacheKey, () =>
				{
					var customsProcedures = new RefCusProcedureCollection(Factory, countryCode, declaration?.DateOfValuation ?? ZDateTime.Today, group, shipmentType);
					customsProcedures.ApplySort("FullCodeCurrentPlusPreviousPlusConcession", System.ComponentModel.ListSortDirection.Ascending);
					return customsProcedures;
				});
			}
		}

		public UNDGSubstanceCollection UNDGSubs => new UNDGSubstanceCollection(Factory);

		public CodeDescriptionPairList PartyIdentifierCodeList => Factory.GetCachedValue("Enterprise.Customs.TW.Business.PartyIdentifierCodeList", () =>
		{
			var result = new UntranslatableCodeDescriptionPairList((NoResString)"Party Identifier Code List");
			result.AddRange(new PartyIdentifierCodeList());
			return result;
		});

		public CodeDescriptionPairList ExemptionCodeList => Factory.GetCachedValue("Enterprise.Customs.TW.Business.ExemptionCodeList", () =>
		{
			var result = new UntranslatableCodeDescriptionPairList((NoResString)"Exemption Code List");
			result.AddRange(new ExemptionCodeList());
			return result;
		});

		IRefCusPackListProvider CachedCusPackListProvider => Factory.GetCachedValue<RefCusPackListProvider>();

		public override CodeDescriptionPairList CustomsUQList => CachedCusPackListProvider.GetCIPCustomsPackList(Factory, ZString.Empty);

		public override CodeDescriptionPairList InvoiceUQList => CachedCusPackListProvider.GetCommercialPackList(Factory, ZString.Empty);

		public CodeDescriptionPairList PackagingUQList => CachedCusPackListProvider.GetCommercialPackList(Factory, ZString.Empty);

		public CodeDescriptionPairList PermitUQList => Parent.IsForCMHeaderMessageTypeNX101CertificateType15
					? CachedCusPackListProvider.GetCommercialPackList(Factory, RPTypeList.Codes.PermitQuantityUnits)
					: CachedCusPackListProvider.GetCIPCustomsPackList(Factory, ZString.Empty);

		protected override Customs.Business.OrgSupplierPartCollection GetNewPartCollection()
		{
			OrgSupplierPartCollection result = null;
			var declaration = InvoiceLine.Declaration;
			if (declaration != null)
			{
				result = new OrgSupplierPartCollection(Factory, InvoiceLine, InvoiceLine.Supplier, InvoiceLine.Importer, InvoiceLine.InvoiceHeader?.IsExport ?? ZBool.False);
			}
			return result;
		}

		public OrganisationsFindBoxCollection ManufacturerList => Factory.GetCachedValue("TW.JobDeclarationLookups.OrganisationsFindBoxCollection", () => new OrganisationsFindBoxCollection(Factory));

		public TrademarkEDocList TrademarkEDocList => new TrademarkEDocList(InvoiceLine.TrademarkStorageDocs);

		public CodeDescriptionPairList TpfPaymentMethodList
		{
			get
			{
				var messageType = InvoiceLine?.Declaration?.JE_MessageType ?? ZString.Empty;
				var isROR = InvoiceLine.IsROR;
				var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_TpfPaymentMethodList", messageType, isROR);
				return Factory.GetCachedValue(cacheKey, () =>
				{
					CodeDescriptionPairList result;
					if (messageType == Common.Shared.SharedJobMessageTypeList.Codes.Export)
					{
						result = new CodeDescriptionPairList();
						result.AddPair(DutyTaxPaymentMethodList.Codes.CashPayment, DutyTaxPaymentMethodList.Descriptions.CashPayment);
					}
					else
					{
						result = new CodeDescriptionPairList(new DutyTaxPaymentMethodList());
						if (!isROR)
						{
							result.RemoveCode(DutyTaxPaymentMethodList.Codes.RorPayment);
						}
					}
					return result;
				});
			}
		}

		public IBusinessObjectCollection SpecialCodeList => ExemptionOfControllingAgenciesCusSupporting.GetSpecialCodeList(Factory);
	}
}
