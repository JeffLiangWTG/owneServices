using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.US;

namespace Enterprise.Customs.US.Business
{
	internal class ATF6ADataBuilder : IATF6ADataBuilder
	{
		public ATF6ADataBuilder()
		{
		}

		public ATF6ADataBuilder(JobDeclaration declaration, IDocDataObjectParameters parameters)
		{
			this.declaration = declaration;
			this.permitNumbers = (parameters.Data as ZString[]) ?? Array.Empty<ZString>();
			context = new ATF6AContext(declaration.Factory);
		}
		JobDeclaration declaration;
		ZString[] permitNumbers;
		IContext context;

		public ATF6ADocDataObject Build()
		{
			var wrapper = new ATF6ADocDataObject();
			var invoiceLines = declaration.InvoiceLines.OfType<JobComInvoiceLine>().OrderBy(x => x.InvoiceNumber).ThenBy(x => x.JI_LineNo);
			wrapper.ImporterOfRecord = CreateAddress(context, declaration.IOR?.MainAddress);
			wrapper.CountryManufactured = StringJoinWithoutEmptyField(", ", invoiceLines.Select(x => x.US_UC_NKCountryOfOrigin).Distinct().ToArray());
			wrapper.EntryValue = invoiceLines.Where(x => x.US_SetInd != SecondarySpecProgIndicatorList.Codes.X).Sum(x => x.JI_CustomsValue).ToString("0.00");
			wrapper.PortOfEntryAndDesc = declaration.US_SchDEntry.IsEmpty ? ZString.Empty : $"{declaration.US_SchDEntry} - {declaration.SchDEntryDescription}";
			wrapper.EntryNumber = declaration.ImportEntryNumber.IsEmpty ? ZString.Empty : $"{declaration.US_EntryFilerCode} - {declaration.ImportEntryNumber}";
			wrapper.IsWarehouse = EntryTypeList.IsWarehouseType(declaration.US_EntryType);
			wrapper.IsInformal = EntryTypeList.IsInformal(declaration.US_EntryType);
			wrapper.IsConsumption = EntryTypeList.IsConsumptionForNAFTARecon(declaration.US_EntryType) || EntryTypeList.IsConsumptionMXCementImportLicense(declaration.US_EntryType);
			wrapper.EntryReleaseDate = declaration.JE_EntryAuthorisationDate.IsValid ? declaration.JE_EntryAuthorisationDate.ToShortDateString() : ZString.Empty;
			wrapper.OwnerRef = declaration.JE_OwnerRef;
			if (declaration.Shipment is ForwardingShipment shipment)
			{
				wrapper.TotalNoOfPacks = shipment.JS_TotalPackageCount.ToString();
				wrapper.TotalNoOfPacksPackageTypeDescription = shipment.Lookups.JS_PackType_List.GetDescriptionFromCode(shipment.JS_F3_NKTotalCountPackType);
			}
			else
			{
				wrapper.TotalNoOfPacks = declaration.JE_TotalNoOfPacks.ToString();
				wrapper.TotalNoOfPacksPackageTypeDescription = declaration.Lookups.JE_TotalNoOfPacksPackType_List.GetDescriptionFromCode(declaration.JE_TotalNoOfPacksPackType);
			}
			wrapper.DeclarationNumber = declaration.JE_DeclarationReference;
			var allATFsNeedToBePrinting = invoiceLines
				.SelectMany(x => x.ATFLines).OfType<ATF>()
				.Where(x => permitNumbers.Contains(x.US_PermitNumber));

			var atfGroups = new List<ATFGroupDocDataObject>();
			foreach (var permitNumber in permitNumbers)
			{
				var atfGroup = new ATFGroupDocDataObject();
				atfGroup.PermitNumber = permitNumber;
				atfGroups.Add(atfGroup);

				var atfDetails = new List<ATFDetailDocDataObject>();
				var atfsNeedToBePrinting = allATFsNeedToBePrinting.Where(x => x.US_PermitNumber == permitNumber);
				var fflNumber = ZString.Empty;
				var aecaNumber = ZString.Empty;
				var fflExpirationDate = ZString.Empty;
				var aecaExpirationDate = ZString.Empty;
				var isFirearms = false;
				var isImplementsOfWar = false;
				var isAmmunition = false;
				var atfDictionary = new Dictionary<ZString, List<ATF>>();
				foreach (var atf in atfsNeedToBePrinting)
				{
					var atfListForInvoice = atfDictionary.GetOrAdd(atf.InvoiceLine.InvoiceNumber, () => new List<ATF>());
					atfListForInvoice.Add(atf);
					var atfDetail = new ATFDetailDocDataObject();
					var invoiceLine = atf.InvoiceLine;
					atfDetail.InvoiceNumber = invoiceLine.InvoiceNumber;
					atfDetail.LineNumber = atfListForInvoice.Count;
					atfDetail.ManufacturerName = invoiceLine.ManufacturerAddress?.CompanyName ?? ZString.Empty;
					atfDetail.ProductCode = invoiceLine.JI_PartNo;
					atfDetail.CategoryCode = atf.US_CategoryCode;
					atfDetail.CaliberGaugeSize = atf.US_CaliberGaugeSize;
					atfDetail.Quantity = atf.US_Quantity.IsDefault ? ZString.Empty : atf.US_Quantity.ToString();
					atfDetail.BarrelLength = atf.US_BarrelLength.IsDefault ? ZString.Empty : atf.US_BarrelLength.ToString();
					atfDetail.OverallLength = atf.US_OverallLength.IsDefault ? ZString.Empty : atf.US_OverallLength.ToString();
					atfDetail.MunitionsListCategory = atf.US_MunitionsListCategory;
					atfDetail.Model = atf.US_Model;
					atfDetails.Add(atfDetail);

					if (fflNumber.IsEmpty && !atf.US_FFLNumber.IsEmpty)
					{
						fflNumber = $"FFL: {atf.US_FFLNumber}";
					}
					if (aecaNumber.IsEmpty && !atf.US_AECANumber.IsEmpty)
					{
						aecaNumber = $"AECA: {atf.US_AECANumber}";
					}
					if (fflExpirationDate.IsEmpty && atf.US_FFLExpirationDate.IsValid)
					{
						fflExpirationDate = $"FFL: {atf.US_FFLExpirationDate.ToString("MM-dd-yyyy")}";
					}
					if (aecaExpirationDate.IsEmpty && atf.US_AECAExpirationDate.IsValid)
					{
						aecaExpirationDate = $"AECA: {atf.US_AECAExpirationDate.ToString("MM-dd-yyyy")}";
					}
					if (atf.IsFirearms)
					{
						isFirearms = true;
					}
					if (atf.IsImplementsOfWar)
					{
						isImplementsOfWar = true;
					}
					if (atf.IsAmmunition)
					{
						isAmmunition = true;
					}
				}
				atfGroup.ATFDetails = atfDetails;

				var lines = atfsNeedToBePrinting.Select(x => x.InvoiceLine).Distinct().OrderBy(x => x.InvoiceNumber).ThenBy(x => x.JI_LineNo);
				var seller = GetOrgAddressInformation(lines, x => x.Seller);
				atfGroup.SellerName = seller.CompanyNmae;
				atfGroup.SellerDetails = seller.Details;
				var foreignExporter = GetOrgAddressInformation(lines, x => x.ExporterAddress);
				atfGroup.ForeignExporterName = foreignExporter.CompanyNmae;
				atfGroup.ForeignExporterDetails = foreignExporter.Details;
				atfGroup.FFLNoAndAECANo = StringJoinWithoutEmptyField("   ", new ZString[] { fflNumber, aecaNumber });
				atfGroup.FFLAndAECAExpirationDates = StringJoinWithoutEmptyField("   ", new ZString[] { fflExpirationDate, aecaExpirationDate });
				atfGroup.IsFirearms = isFirearms;
				atfGroup.IsImplementsOfWar = isImplementsOfWar;
				atfGroup.IsAmmunition = isAmmunition;
			}
			wrapper.ATFGroups = atfGroups;

			return wrapper;
		}

		(ZString CompanyNmae, ZString Details) GetOrgAddressInformation(IEnumerable<JobComInvoiceLine> invoiceLines, Func<JobComInvoiceLine, OrgAddress> getAddress)
		{
			var address = invoiceLines.Select(getAddress).FirstOrDefault(x => x != null);
			if (address != null)
			{
				var companyName = address.EffectiveCompanyName;
				var details = StringJoinWithoutEmptyField(", ", new ZString[] { address.Address1, address.Address2, address.City, address.OA_State, address.CountryName });
				return (companyName, details);
			}
			else
			{
				return (ZString.Empty, ZString.Empty);
			}
		}

		ZString StringJoinWithoutEmptyField(ZString separator, ZString[] values)
		{
			return ZString.Join(separator, values.Where(x => !x.IsEmpty).ToArray());
		}

		Address CreateAddress(IContext context, OrgAddress orgAddress)
		{
			if (orgAddress == null)
			{
				return CreateEmptyAddress(context);
			}

			var address = new Address(context.Factory);
			address.CompanyName = orgAddress.EffectiveCompanyName;
			address.AddressLine1 = orgAddress.Address1;
			address.AddressLine2 = orgAddress.Address2;
			address.City = orgAddress.City;
			address.State = orgAddress.StateCode;
			address.Postcode = orgAddress.Postcode;
			address.Country = Country.Create(context, orgAddress.Country);
			address.Unloco = Unloco.Create(context, orgAddress.RelatedPortCode);
			address.Fax = orgAddress.OA_Fax;
			address.Phone = orgAddress.OA_Phone;
			address.Email = orgAddress.OA_Email;
			return address;
		}

		Address CreateEmptyAddress(IContext context)
		{
			var address = new Address(context.Factory);
			address.Country = new Country(context.Factory, context.Countries);
			address.Unloco = new Unloco(context.Factory, context.Unlocos, context.Countries);
			address.RegistrationNumbers = Array.Empty<IRegistrationNumber>();
			return address;
		}

		public object Build(object declaration, object parameters)
		{
			if (declaration is JobDeclaration dec && parameters is IDocDataObjectParameters paras)
			{
				this.declaration = dec;
				this.permitNumbers = (paras.Data as ZString[]) ?? Array.Empty<ZString>();
				context = new ATF6AContext(dec.Factory);

				return Build();
			}
			return null;
		}
	}
}
