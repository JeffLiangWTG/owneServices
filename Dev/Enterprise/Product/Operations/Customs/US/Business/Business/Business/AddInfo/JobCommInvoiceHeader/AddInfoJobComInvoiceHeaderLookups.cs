using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class AddInfoJobComInvoiceHeaderLookups : USAddInfoLookups
	{
		public AddInfoJobComInvoiceHeaderLookups(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public new AddInfoJobComInvoiceHeader Parent
		{
			get { return (AddInfoJobComInvoiceHeader)base.Parent; }
		}

		public JobComInvoiceHeader Invoice
		{
			get { return (JobComInvoiceHeader)Parent.Parent; }
		}

		public CodeDescriptionPairList US_InvoiceTypeList
		{
			get { return Factory.GetCachedValue<InvoiceTypeList>(); }
		}

		public CodeDescriptionPairList US_PaymentTermsList
		{
			get { return Factory.GetCachedValue<PaymentTermsTypeList>(); }
		}

		public CodeDescriptionPairList US_TermsOfDeliveryLocationQualifierList
		{
			get { return Factory.GetCachedValue<TermsOfDeliveryLocQualifierList>(); }
		}

		public CodeDescriptionPairList US_TermsOfDeliveryLocationIndicatorList
		{
			get { return Factory.GetCachedValue<TermsOfDeliveryLocationCodeIndicators>(); }
		}

		public IBusinessObjectCollection US_TermsOfDeliveryLocationList
		{
			get
			{
				switch (Parent.US_TermsOfDeliveryLocationIndicator)
				{
					case TermsOfDeliveryLocationCodeIndicators.Codes.ScheduleD:
						return RegionDistrictPorts;
					case TermsOfDeliveryLocationCodeIndicators.Codes.ScheduleK:
						return ForeignPorts;
					default:
						return USCountryList;
				}
			}
		}

		public TariffTypeList US_TariffTypeList
		{
			get { return Factory.GetCachedValue<TariffTypeList>(); }
		}

		public CodeDescriptionPairList SPIList => SPICompleteList.GetCachedList(Parent.Factory);

		public CodeDescriptionPairList UltimateConsigneeTypeList
		{
			get { return Parent.Factory.GetCachedValue<UltimateConsigneeTypeList>(); }
		}

		public ConsolidatedEntryModuleEntryHeaderCollection Entries
		{
			get
			{
				var result = new ConsolidatedEntryModuleEntryHeaderCollection(Factory);

				var releaseEntryNumber = Parent.US_ReleaseEntryNumber.SubstringSafe(3);
				if (!releaseEntryNumber.IsEmpty)
				{
					var filterDefault = new FilterBusinessObjectDefault(ModuleEntryHeaderCollection.USFilterConstants.EntryNumber, "Property", releaseEntryNumber);
					result.FilterBusinessObjectDefaults.Add(filterDefault);
				}

				var declaration = Invoice.JobDeclaration;
				if (declaration != null)
				{
					if (declaration.IOROrgPK.IsValid)
					{
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ModuleEntryHeaderCollection.USFilterConstants.ImporterOfRecord, "Property", declaration.IOROrgPK));
					}

					if (declaration.US_SchDEntry.IsValid)
					{
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ModuleEntryHeaderCollection.USFilterConstants.PortOfEntry, "Property", declaration.US_SchDEntry));
					}

					var validReleaseDate = declaration.GetValidReleaseDate();
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(JobDeclaration.Constants.USFilterConstants.ReleaseDate, "PropertySearch", new ZString("Date Range")));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(JobDeclaration.Constants.USFilterConstants.ReleaseDate, "Property1", validReleaseDate));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(JobDeclaration.Constants.USFilterConstants.ReleaseDate, "Property2", ZDateTime.Today));
				}
				return result;
			}
		}

		[System.Xml.Serialization.XmlIgnore]
		public USAESLicenseCodeCollection US_LicenseType_List
		{
			get
			{
				if (uS_LicenseType_List == null)
				{
					uS_LicenseType_List = new USAESLicenseCodeCollection(Factory);
				}

				uS_LicenseType_List.FilterBusinessObjectDefaults.RemoveAll();
				uS_LicenseType_List.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.EffectiveDate, "Property1", Invoice.ExportDateForLicenseType));
				return uS_LicenseType_List;
			}
		}
		USAESLicenseCodeCollection uS_LicenseType_List;

		public CodeDescriptionPairList SplitShipmentDetailsList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				if (Invoice.JobDeclaration is JobDeclaration declaration && declaration.IsFTZSplitDetailsRelevant)
				{
					var bill = Invoice.Bill;
					if (bill != null && bill.US_SESplitShip)
					{
						foreach (var split in bill.ITAndSplitDetails.OfType<ITAndSplitDetails>().Where(split => !split.US_CarrierCode.IsEmpty && !split.US_FlightNumber.IsEmpty && !split.US_ArrivalDate.IsEmpty).Select(split => ZString.Join("/", new ZString[] { split.US_CarrierCode, split.US_FlightNumber, split.US_ArrivalDate.ToShortDateString() })))
						{
							result.AddPair(split.ToUpper(), ZString.Empty);
						}
					}
				}

				return result;
			}
		}

		public USAESECCNNumberCollection US_ECCNList
		{
			get
			{
				return Factory.GetCachedValue($"ECCNList|{Invoice.US_LicenseType}|{ZDateTime.Today.ToISO8601ShortDateString()}", delegate
				{
					var uS_ECCNList = new USAESECCNNumberCollection(Parent.Factory, Invoice.US_LicenseType);
					uS_ECCNList.FilterBusinessObjectDefaults.RemoveAll();
					uS_ECCNList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.EffectiveDate, "Property1", ZDateTime.Today));
					return uS_ECCNList;
				});
			}
		}
	}
}
