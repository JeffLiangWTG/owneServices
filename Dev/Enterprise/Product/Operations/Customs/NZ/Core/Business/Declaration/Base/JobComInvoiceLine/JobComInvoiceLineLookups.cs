using System.Collections;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using OrgSupplierPartCollection = Enterprise.Customs.NZ.Business.MasterFiles.OrgSupplierPartCollection;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class JobComInvoiceLineLookups : Customs.Business.JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		protected new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		public override CodeDescriptionPairList InvoiceUQList
		{
			get { return (Parent.Declaration?.IsTSWWriteOff ?? ZBool.False) ? UniversalReferenceHelper.GetUNEPackageTypeList(Factory) : base.InvoiceUQList; }
		}

		public NonDependentNZCClassificationCollection PartsOfClassificationList
		{
			get { return new NonDependentNZCClassificationCollection(Factory, new ZQuery(NZCClassificationSchema.U0_IsManual, SQLComparisonOperator.Equal, 'Y')); }
		}

		public override IBaseClassificationCollection<BaseCusClassification> ClassificationList
		{
			get
			{
				ZQuery filter = new ZQuery(CusClassificationSchema.CC_RN_NKCountryCode, SQLComparisonOperator.Equal, Core.Constants.CountryCodes.NewZealand);
				var result = new BaseClassificationCollection<CusClassification>(Factory, filter);
				result.SetOverrideNotificationWhenAdditionalFilterNotMet("This lookup code is a client specific one and this invoice line doesn't have the client as Supplier or Importer.");
				return result;
			}
		}

		public Enterprise.MasterFiles.Business.OrgHeaderCollection ManufacturersGrowersProducers
		{
			get { return new Enterprise.MasterFiles.Business.OrgHeaderCollection(Factory); }
		}

		public NonDependentNZCClassificationCollection TariffList
		{
			get { return new NonDependentNZCClassificationCollection(Factory); }
		}

		[SuppressWeaklyTypedCollectionMessage]
		public IList ConcessionList
		{
			get
			{
				return UniversalTariffHelper.GetConcessionList(Factory, Parent.JI_Tariff, Parent.DateForDutyRate, Parent.JI_CountryOfOrigin, Parent.JI_QualifiesForPreferentialDuty);
			}
		}

		public SupplementaryUQList SupplementaryUQList
		{
			get { return Factory.GetCachedValue<SupplementaryUQList>(); }
		}

		public QualifiesForPreferentialDutyList QualifiesForPreferentialDutyList
		{
			get { return Factory.GetCachedValue<QualifiesForPreferentialDutyList>(); }
		}

		public PreferentialCountryGroupCodeList PreferentialCountryGroupCodeList
		{
			get { return PreferentialCountryGroupCodeList.GetListFor(Parent.EffectiveCountryOfOrigin, Parent.DateForDutyRate, Factory, Parent.JI_QualifiesForPreferentialDuty); }
		}

		protected override Customs.Business.OrgSupplierPartCollection GetNewPartCollection()
		{
			return Parent.Declaration != null ? new OrgSupplierPartCollection(Factory, Parent, InvoiceLine.InvoiceHeader?.IsExport ?? ZBool.False) : new OrgSupplierPartCollection(Factory);
		}

		public GoodsTypeList GoodsTypes
		{
			get { return Factory.GetCachedValue<GoodsTypeList>(); }
		}

		public MeasurementUQList MeasurementUQs
		{
			get { return Factory.GetCachedValue<MeasurementUQList>(); }
		}

		public YesNoList YesNoList
		{
			get { return Factory.GetCachedValue<YesNoList>(); }
		}

		#region IntendedUseCodes

		public virtual CodeDescriptionPairList IntendedUseCodeList
		{
			get { return Factory.GetCachedValue<IntendedUseCodeList>(); }
		}

		#endregion

		#region ManufacturerAddresses

		public virtual Enterprise.MasterFiles.Business.OrgAddressCollection ManufacturerAddressesList
		{
			get { return new Enterprise.MasterFiles.Business.OrgAddressCollection(Factory); }
		}

		#endregion

		public CodeDescriptionPairList PackageUQList => UniversalReferenceHelper.GetUNEPackageTypeList(Factory);

		public CodeDescriptionPairList LevyTypeCodeList
		{
			get { return Factory.GetCachedValue<LevyCodesList>(); }
		}
	}
}
