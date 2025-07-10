using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public partial class JobComInvoiceLineLookups : AutoZAJobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)base.InvoiceLine; }
		}

		public override CodeDescriptionPairList InvoiceUQList
		{
			get
			{
				return Factory.GetCachedValue("ZAInvoiceUQList", delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(base.InvoiceUQList);
					result.AddPair("NX", "Number of Parts");
					result.Sort();
					return result;
				});
			}
		}

		public ICollection InvoiceUQUNE20CodeList => GetInvoiceUQUNE20CodeListCore();

		protected virtual ICollection GetInvoiceUQUNE20CodeListCore()
		{
			if (JobComInvoiceLine.ZAAddInvoiceDetailsToCUSDECMessageEnabled)
			{
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, "INVUQ", ZDateTime.Today);
			}
			else
			{
				return RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory);
			}
		}

		public override ICodeDescriptionPairList Procedures
		{
			get
			{
				var countryCode = InvoiceLine.CountryCode;
				var procedureCode = InvoiceLine.ProcedureCode;
				var shipmentType = InvoiceLine.Declaration?.JE_MessageType ?? ZString.Empty;
				var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_RefCusProcedures_PreviousProcedureCode", countryCode, shipmentType, procedureCode);
				return Factory.GetCachedValue(cacheKey, () =>
				{
					var result = new CodeDescriptionPairList();
					var collection = RefCusProcedureCollection.LoadPreviousProceduresCodesForCountryShipmentTypeProcedureCode(Factory, countryCode, shipmentType, procedureCode, ZDateTime.Today);
					foreach (var procedure in collection)
					{
						result.AddPair(procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode, procedure.ZZ6_Description);
					}
					result.Sort();
					return result;
				});
			}
		}

		public RefCusProcedureCollection CPCList
		{
			get
			{
				var declaration = InvoiceLine?.Declaration;
				var countryOfExport = declaration?.CountryCode ?? GlbCompany.CurrentCompany.Country.Code;
				var shipmentType = declaration?.JE_MessageType ?? ZString.Empty;
				var instructionType = ZString.Empty;
				if (InvoiceLine != null && InvoiceLine.JI_CEI.IsValid)
				{
					instructionType = InvoiceLine.EntryInstruction?.CEI_Style ?? ZString.Empty;
				}

				return new RefCusProcedureCollection(Factory, countryOfExport, InvoiceLine?.Declaration?.DateOfValuation ?? ZDateTime.Today, instructionType, shipmentType);
			}
		}

		public QuantityCodeList AdditionalUnitCodeList
		{
			get { return Factory.GetCachedValue<QuantityCodeList>(); }
		}

		public override CodeDescriptionPairList BondedWhsUnitQtyList
		{
			get { return Factory.GetCachedValue<CountableQuantityCodeList>(); }
		}

		public ICodeDescriptionPairList ROOTypeOrPreferenceList
		{
			get => InvoiceLine.IsImport
				? PrimaryPreferenceList
				: ZARefCusCodeListTypes.GetAddInWithROOTypeAttribute(Factory, InvoiceLine.EffectiveAssessmentDate);
		}

		public override CodeDescriptionPairList TaxOrFeeCodeList => UniversalReferenceDataHelper.GetTaxTypeList(Factory, InvoiceLine.EffectiveAssessmentDate);

		#region PartsList

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

		#endregion

		public ICodeDescriptionPairList ROOTypesList
		{
			get { return ZARefCusCodeListTypes.GetAddInWithROOTypeAttribute(Factory, InvoiceLine.EffectiveAssessmentDate); }
		}

		public ICodeDescriptionPairList CusApprovedExporterList
		{
			get { return ZARefCusCodeListTypes.GetAddInWithCusApprovedExporterAttribute(Factory, InvoiceLine.EffectiveAssessmentDate); }
		}

		public Schedule1P8CodeList Schedule1P8Items
		{
			get { return Factory.GetCachedValue<Schedule1P8CodeList>(); }
		}

		public VehicleFormatList VehicleFormats
		{
			get { return Factory.GetCachedValue<VehicleFormatList>(); }
		}

		public VehicleTypeList VehicleTypes
		{
			get { return Factory.GetCachedValue<VehicleTypeList>(); }
		}

		public CodeDescriptionPairList YearList
		{
			get
			{
				return Factory.GetCachedValue("ZAInvLineyearList",
					delegate
					{
						var yearList = new CodeDescriptionPairList();

						var currentYear = ZDateTime.Today.Year;
						for (int i = 0; i < 40; i++)
						{
							ZString yearString = (currentYear - i).ToString();
							yearList.AddPair(yearString, yearString);
						}

						return yearList;
					}
				);
			}
		}

		public GoodsTypeList GoodsTypeList
		{
			get { return Factory.GetCachedValue<GoodsTypeList>(); }
		}

		public CodeDescriptionPairList CustomsValueCurrencyOverrideList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var header = InvoiceLine?.InvoiceHeader;
				if (header != null)
				{
					var localCurr = header.LocalCurrency;
					var invoiceCurr = header.Invoice_Currency;
					if (localCurr != null)
					{
						result.Add(localCurr);
					}
					if (invoiceCurr != null && invoiceCurr != localCurr)
					{
						result.Add(invoiceCurr);
					}
				}
				return result;
			}
		}

		public Customs.Business.OrgSupplierPartCollection NewOwnerProducts
		{
			get
			{
				var invoiceLine = this.InvoiceLine;
				var owner = invoiceLine.EntryInstruction?.Owner;
				var result = new Customs.Business.OrgSupplierPartCollection(Factory, invoiceLine, null, owner, invoiceLine.InvoiceHeader?.IsExport ?? ZBool.False);
				if (!invoiceLine.JI_NewOwnerPartNo.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Product Code", "Property", invoiceLine.JI_NewOwnerPartNo));
				}
				if (owner != null)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property1", owner.PK));
				}
				return result;
			}
		}

		public PermitFindBoxCollection Permits
		{
			get
			{
				OrgHeader permitHolder;
				ZString permitType;

				if (InvoiceLine.IsImport)
				{
					permitHolder = InvoiceLine.Importer_Effective;
					permitType = PermitTypeList.Codes.IMP;
				}
				else
				{
					permitHolder = InvoiceLine.Supplier_Effective;
					permitType = PermitTypeList.Codes.EXP;
				}

				var filterRules = new Dictionary<CodeDescriptionPair, ZString>() { { new CodeDescriptionPair(PermitRuleCodeList.Codes.TAR, PermitRuleCodeList.Descriptions.TAR), InvoiceLine.JI_Tariff } };

				return PermitFindBoxCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, permitHolder, permitType, InvoiceLine.JI_PermitNumber, filterRules, InvoiceLine.EffectiveAssessmentDate.Date);
			}
		}
	}
}
