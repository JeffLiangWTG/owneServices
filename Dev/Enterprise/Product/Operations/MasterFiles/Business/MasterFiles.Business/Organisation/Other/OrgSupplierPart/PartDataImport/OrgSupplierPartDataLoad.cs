using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.MasterFiles.Business
{
	// NOTE WELL:  ANY CHANGES TO THIS FORMAT MUST BE REFLECTED IN DOCUMENTATION AND SAMPLE FILES AS PUBLISHED ON OUR WEBSITE.
	//
	//						Update this document and email to Kirsten who will review and publish on web.
	//						G:\Eagle\data\eServices Team\ediEnterprise Standard Interfaces\Product Data Import from csv File
	//						G:\Eagle\data\eServices Team\ediEnterprise Standard Interfaces\Product Sample File - Australia & New Zealand (.csv)
	//						G:\Eagle\data\eServices Team\ediEnterprise Standard Interfaces\Product Sample File - USA (.csv)
	//			where G: is \\corporate.cargowise.com
	//

	#region PartsDataToLoad

	public class PartsDataToLoad
	{
		public PartsDataToLoad()
		{
			UnitConversions = new List<UnitConversion>();
			PartBarcodes = new List<PartBarcode>();
		}

		public ZString PartNo;
		public ZString PartDesc;
		public ZString PartFullDesc;
		public ZString PartDepartment;
		public ZString PartDivision;
		public List<ZString> PartSupplierCode;
		public List<ZGuid> PartSupplierCodePK;
		public List<ZString> PartOwnerCode;
		public List<ZGuid> PartOwnerCodePK;
		public decimal PartCount;
		public ZString PartUQ;
		public decimal PartWeight;
		public decimal PartWeightNet;
		public ZString PartWeightUnit;
		public decimal PartVolume;
		public ZString PartVolumeUnit;
		public ZString PartOrigin;
		public decimal PartLastCost;
		public decimal PartWeightedCost;
		public ZString PartCostCurrency;
		public ZString PartUNDGCode;
		public ZGuid PartUNDGPK;
		public decimal PartDepth;
		public decimal PartWidth;
		public decimal PartHeight;
		public ZString PartMeasureUQ;
		public bool KeepUpright;
		public bool Use_Attribute1;
		public bool Use_Attribute2;
		public bool Use_Attribute3;
		public bool Use_SerialNumber;
		public bool IsPartAttrib1ReleaseCaptured;
		public bool IsPartAttrib2ReleaseCaptured;
		public bool IsPartAttrib3ReleaseCaptured;
		public bool IsSerialNumberReleaseCaptured;
		public ZShort Hi;
		public ZShort Ti;
		public bool UseExpiryDate;
		public bool UsePackingDate;
		public ZString Commodity;
		public ZString PartBrandName;
		public ZString PartModel;
		public ZString LocalPartNumber;
		public ZString LocalPartDescription;
		public ZByte DecimalPlaces;
		public ZString ClientUQ;
		public ZGuid DefaultHoldCodePK;
		public Money UnitPrice;
		public ZGuid CartonGroupPK;

		public List<UnitConversion> UnitConversions;
		public List<PartBarcode> PartBarcodes;

		public bool HasNoValidOwnerOrSupplier;
		public bool HasInvalidClassificationType;

		// Old Classification Fields
		public ZString PartClassification;
		public ZString PartExportClassification;
		public ZString ImportTariff;
		public ZString ExportTariff;

		// New Classification Fields
		public ZString ClassificationLookup;
		public ZString ClassificationType;
		public ZString Tariff;
		public ZString UsageComment;
		public ZString ClassificationDescription;

		public bool RFCompletePalletPicking;
		public bool RollUpAttributes;
		public ZString RFPackingDateFormat;
		public ZString RFExpiryDateFormat;
		public ZShort ConsigneeMinShelfLifeAcceptedDays;
		public ZString JulianBatchNumberFormat;
		public ZString RFConfirm;
		public ZString PickMode;

		public bool Barcode1_UseForDocuments;
		public bool Barcode2_UseForDocuments;
		public bool Barcode3_UseForDocuments;
		public bool Barcode4_UseForDocuments;
		public bool Barcode5_UseForDocuments;
	}

	#endregion

	#region UnitConversion

	public class UnitConversion : IUnitConverter
	{
		public UnitConversion(decimal quantityInParent, string packageType, string parentPackageType)
		{
			QuantityInParent = quantityInParent;
			PackageType = packageType?.ToUpper();
			ParentPackageType = parentPackageType?.ToUpper();
		}

		public readonly decimal QuantityInParent;
		public readonly string PackageType;
		public readonly string ParentPackageType;
		public decimal Cubic { get; set; }
		public decimal Depth { get; set; }
		public decimal Height { get; set; }
		public decimal Width { get; set; }
		public decimal Weight { get; set; }

		ZString IUnitConverter.ParentUnit
		{
			get { return ParentPackageType; }
		}

		ZString IUnitConverter.ChildUnit
		{
			get { return PackageType; }
		}

		ZDecimal IUnitConverter.ConversionFactor
		{
			get { return QuantityInParent; }
		}
	}

	#endregion

	#region PartBarcode

	public class PartBarcode
	{
		public PartBarcode(string barcode, string package, bool useForDocuments)
		{
			Barcode = barcode;
			Package = package;
			UseForDocuments = useForDocuments;
		}

		public readonly string Package;
		public readonly string Barcode;
		public readonly bool UseForDocuments;
	}

	#endregion

	#region OrgSupplierPartDataLoad

	public class OrgSupplierPartDataLoad : DataLoadWithFlexibleColumns
	{
		#region Constructors

		public static OrgSupplierPartDataLoad New()
		{
			Type type;

			var country = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString());
			if (ObjectFactory.Get<Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(country))
			{
				if (country == Enterprise.Core.Constants.CountryCodes.UnitedKingdom)
				{
					type = ObjectFactory.GetType<GB.IGBOrgSupplierPartDataLoad>();
				}
				else
				{
					type = ObjectFactory.GetType<EU.IEUOrgSupplierPartDataLoad>();
				}
			}
			else
			{
				switch (country)
				{
					case Enterprise.Core.Constants.CountryCodes.Australia:
						type = ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IAUCusOrgSupplierPartDataLoad>();
						break;

					case Enterprise.Core.Constants.CountryCodes.NewZealand:
						type = ObjectFactory.GetType<NZ.INZCusOrgSupplierPartDataLoad>();
						break;

					case Enterprise.Core.Constants.CountryCodes.UnitedStates:
						type = ObjectFactory.GetType<US.IOrgSupplierPartDataLoad>();
						break;

					case Enterprise.Core.Constants.CountryCodes.Canada:
						type = ObjectFactory.GetType<CA.ICAOrgSupplierPartDataLoad>();
						break;

					case Enterprise.Core.Constants.CountryCodes.Singapore:
						type = ObjectFactory.GetType<SG.ISGOrgSupplierPartDataLoad>();
						break;

					case Enterprise.Core.Constants.CountryCodes.SouthAfrica:
						type = ObjectFactory.GetType<ZA.IOrgSupplierPartDataLoad>();
						break;

					case Enterprise.Core.Constants.CountryCodes.Taiwan:
						type = ObjectFactory.GetType<TW.IOrgSupplierPartDataLoad>();
						break;

					default:
						type = ObjectFactory.GetType<IGlobalOrgSupplierPartDataLoad>();
						break;
				}
			}
			return (OrgSupplierPartDataLoad)Activator.CreateInstance(type);
		}

		#endregion

		#region ImportProductData

		public void ImportProductData(string dataLocation, bool updateParts, bool legacyCodes, bool isAudited = false)
		{
			UpdatePartRecords = updateParts;
			UseLegacyCodes = legacyCodes;
			IsAudited = isAudited;
			ImportData(dataLocation, (NoResString)"Product");
		}

		#endregion

		#region DataFields PartsData

		bool UpdatePartRecords;
		bool UseLegacyCodes;
		bool IsAudited;
		
		#endregion

		#region ImportFrom .csv file

		protected override void ProcessDataForThisLine(OCsvLine line)
		{
			try
			{
				disposables = new List<IDisposable>();
				CheckElementCount(line);
				PartsDataToLoad data = GetPartsDataToLoad();
				PopulatePartsDataToLoad(line, data);
				ProcessPartData(data);
			}
			catch (ArgumentException ex)
			{
				RunCounters.RecsExcluded++;
				DisplayLogMessage(Res.GetString("c040bf09-e3f0-4f75-8d16-b7b3810946bf", "Row {0} excluded... data is inconsistent with required format.", RunCounters.CurrentRow.ToString()));
				DisplayLogMessage(ex.Message);
			}
			finally
			{
				disposables.ForEach(d => d.Dispose());
				disposables = null;
			}
			OnProgressChanged();
		}

		List<IDisposable> disposables;

		protected void AddToDisposableList(IDisposable disposable)
		{
			if (disposable != null)
			{
				disposables.Add(disposable);
			}
		}

		protected virtual PartsDataToLoad GetPartsDataToLoad()
		{
			return new PartsDataToLoad();
		}

		protected virtual OrgSupplierPart ProcessPartData(PartsDataToLoad partData)
		{
			OrgSupplierPart result = null;

			try
			{
				if (ClassificationLookupExistsOrIsNotRequired(partData.PartClassification, partData.PartExportClassification))
				{
					if (partData.HasNoValidOwnerOrSupplier)
					{
						RunCounters.RecsExcluded++;
						DisplayFormattedLogMessage(partData.PartNo, partData.PartDesc, Res.GetString("1f22e3e9-abae-44a1-b6ca-3f842de836ce", "Owner or Supplier not found"));
					}
					else if (partData.HasInvalidClassificationType)
					{
						RunCounters.RecsExcluded++;
						DisplayFormattedLogMessage(partData.PartNo, partData.PartDesc, Res.GetString("c80a5ef5-7f09-4da5-ad7b-af171a256958", "Classification Type length exceeds the maximum length of {0}", CusClassPartPivotSchema.CI_ChildType.MaxLength));
					}
					else
					{
						if (IsValidate(partData))
						{
							result = LoadEnterpriseProduct(partData);
							UnlinkPreviousExistingClassificationsFromProduct(partData, result);
							if (IsAudited)
							{
								SetAuditMessageForClassificationLines(result, AuditMessage);
							}
						}
					}
				}
				else
				{
					RunCounters.RecsExcluded++;
					DisplayFormattedLookupError(partData.PartNo, partData.PartClassification, partData.PartExportClassification);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				RunCounters.RecsExcluded++;
				DisplayFormattedLogMessage(partData.PartNo, partData.PartDesc, ex.Message);
			}

			return result;
		}

		bool IsDuplicate(PartsDataToLoad partData, OrgSupplierPart existingPart, out string errorMessage)
		{
			var partPK = existingPart?.PK ?? ZGuid.NewZGuid();

			var partiesOnPart = GetPartiesExpectedAfterImport(partData, existingPart);

			var duplicateDetector = new DuplicateProductDetectorNoBizO(
				partPK,
				partData.PartNo,
				true,
				partiesOnPart,
				null /* otherPartsToConsiderNotYetInDb parameter is null, because OrgSupplierPartDataLoad saves factory after each row*/
			);
			duplicateDetector.CheckInactiveProducts = false;
			duplicateDetector.Validate();
			errorMessage = duplicateDetector.ErrorMessage;
			return duplicateDetector.HasError;
		}

		List<RelatedPartyWithCode> GetPartiesExpectedAfterImport(PartsDataToLoad partData, OrgSupplierPart existingPart)
		{
			var ownerCodes = new HashSet<ZString>();
			var supplierCodes = new HashSet<ZString>();

			if (existingPart != null)
			{
				foreach (var rel in existingPart.RelatedOrganisations.Cast<OrgPartRelation>())
				{
					if (rel.IsOwner)
					{
						ownerCodes.Add(rel.Organisation.OH_Code);
					}

					if (rel.IsSupplier)
					{
						supplierCodes.Add(rel.Organisation.OH_Code);
					}
				}
			}

			if (existingPart == null || UpdatePartRecords)
			{
				foreach (var code in ToOrgCodes(partData.PartOwnerCodePK))
				{
					ownerCodes.Add(code);
				}

				foreach (var code in ToOrgCodes(partData.PartSupplierCodePK))
				{
					supplierCodes.Add(code);
				}
			}

			var partiesOnPart = new List<RelatedPartyWithCode>();
			partiesOnPart.AddRange(ownerCodes.Select(c => new RelatedPartyWithCode(OrgPartRelation.RelationshipTypes.Owner, c)));
			partiesOnPart.AddRange(supplierCodes.Select(c => new RelatedPartyWithCode(OrgPartRelation.RelationshipTypes.Supplier, c)));
			return partiesOnPart;
		}

		IEnumerable<ZString> ToOrgCodes(List<ZGuid> orgHeaderPKs)
		{
			if (orgHeaderPKs == null)
			{
				return new List<ZString>();
			}

			return orgHeaderPKs
				.Where(pk => !pk.IsEmpty)
				.Select(pk => Factory.Load<OrgHeader>(pk))
				.Where(org => org != null)
				.Select(org => org.OH_Code);
		}

		protected virtual void UnlinkPreviousExistingClassificationsFromProduct(PartsDataToLoad partData, OrgSupplierPart result)
		{
		}

		bool IsValidate(PartsDataToLoad product)
		{
			var result = false;
			var clients = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, product.PartOwnerCodePK));

			if (IsPartAttributesNotMatchesWithClients(clients, product))
			{
				RunCounters.RecsExcluded++;
				DisplayFormattedLogMessage(product.PartNo, product.PartDesc, Res.GetString("cbbbea23-fc29-42c8-b2a7-b087e8bddae7", "Part attributes doesn't match with client."));
			}
			else if (IsProductCanNotBeUpdated(clients, product))
			{
				RunCounters.RecsExcluded++;
			}
			else if (HasInvalidUnitConversionPackTypesAndErrorRegistryEnabled(product))
			{
				RunCounters.RecsExcluded++;
			}
			else
			{
				result = true;
			}

			return result;
		}

		bool HasInvalidUnitConversionPackTypesAndErrorRegistryEnabled(PartsDataToLoad product)
		{
			var isValid = OrgPartUnitValidationHelper.ValidatePackTypes(
				Factory,
				product,
				p => p.UnitConversions,
				c => c.PackageType,
				c => c.ParentPackageType,
				(p, message) => DisplayFormattedLogMessage(p.PartNo, p.PartDesc, message));

			return !isValid;
		}

		bool IsProductCanNotBeUpdated(OrgHeader[] clients, PartsDataToLoad product)
		{
			if (clients.Length > 0)
			{
				var parts = GetExistingParts(product.PartNo, product.PartOwnerCodePK, product.PartSupplierCodePK);
				if (parts != null)
				{
					foreach (var client in clients)
					{
						foreach (var part in parts)
						{
							var clientPartRelation = part.RelatedOrganisations.FindByOrganisationPKAndRelationship(client.PK, OrgPartRelation.RelationshipTypes.Owner);

							if (clientPartRelation != null)
							{
								var isInvalid = !IsValidChangeOnUsePartAttribute(product, client, clientPartRelation)
									|| !IsValidChangeOnPartAttribReleaseCaptured(product, clientPartRelation)
									|| !IsValidChangeOnAdditionalFields(product, clientPartRelation);
								if (isInvalid)
								{
									clientPartRelation.CancelChanges();
								}
								return isInvalid;
							}
						}
					}
				}
			}

			return false;
		}

		bool IsValidChangeOnUsePartAttribute(PartsDataToLoad product, OrgHeader client, OrgPartRelation clientPartRelation)
		{
			var hasInventory = new Lazy<bool>(() => { return clientPartRelation.HasCurrentStockIncludingInTransit(); });
			var hasUnfinalisedASNLines = new Lazy<bool>(() => { return clientPartRelation.HasAsnLineOnUnfinalisedReceive; });
			var isPartAttribute1CanBeUpdated = IsPartAttributeCanBeUpdatedWithExistingStockOrUnfinalisedASNLines(clientPartRelation.OU_UsePartAttrib1, product.Use_Attribute1, 1, hasInventory, client, hasUnfinalisedASNLines);
			var isPartAttribute2CanBeUpdated = IsPartAttributeCanBeUpdatedWithExistingStockOrUnfinalisedASNLines(clientPartRelation.OU_UsePartAttrib2, product.Use_Attribute2, 2, hasInventory, client, hasUnfinalisedASNLines);
			var isPartAttribute3CanBeUpdated = IsPartAttributeCanBeUpdatedWithExistingStockOrUnfinalisedASNLines(clientPartRelation.OU_UsePartAttrib3, product.Use_Attribute3, 3, hasInventory, client, hasUnfinalisedASNLines);
			var isSerialNumberCanBeUpdated = SerialNumberCanBeUpdatedWithExistingStock(clientPartRelation.OU_UseSerialNumber, product.Use_SerialNumber, hasInventory, hasUnfinalisedASNLines);

			clientPartRelation.OU_UseExpiryDate = product.UseExpiryDate;
			clientPartRelation.OU_UsePackingDate = product.UsePackingDate;

			clientPartRelation.Validation.ValidateOU_UseExpiryDate();
			DisplayFormattedLog(Logger(product), "UseExpiryDate", clientPartRelation.OU_UseExpiryDateInfo.GetErrors());

			clientPartRelation.Validation.ValidateOU_UsePackingDate();
			DisplayFormattedLog(Logger(product), "UsePackingDate", clientPartRelation.OU_UsePackingDateInfo.GetErrors());

			var isValid = isPartAttribute1CanBeUpdated && isPartAttribute2CanBeUpdated && isPartAttribute3CanBeUpdated && isSerialNumberCanBeUpdated
				&& !clientPartRelation.OU_UseExpiryDateInfo.HasErrors()
				&& !clientPartRelation.OU_UsePackingDateInfo.HasErrors();

			if (!isValid)
			{
				DisplayFormattedLogMessage(product.PartNo, product.PartDesc, Res.GetString("8fe4c8ae-199f-439d-b023-daec1ab9dfd6", "Inventory or Un-finalized ASNs exist(s) for this product with attribute settings different from those in the CSV file."));
			}

			return isValid;
		}

		bool IsPartAttributeCanBeUpdatedWithExistingStockOrUnfinalisedASNLines(ZBool currentAttributeUse, bool useAttributeInCsvFile, int attributeNumber, Lazy<bool> hasInventory, OrgHeader org, Lazy<bool> hasUnfinalisedASNLines)
		{
			var result = true;

			var hasChange = useAttributeInCsvFile != currentAttributeUse;
			if (hasChange)
			{
				var onlyCanCheckNonMandatoryAttribute = !org.PartAttributeManager.IsPartAttributeMandatory(attributeNumber) && useAttributeInCsvFile;
				result = (onlyCanCheckNonMandatoryAttribute || !hasInventory.Value) && !hasUnfinalisedASNLines.Value;
			}
			return result;
		}

		bool SerialNumberCanBeUpdatedWithExistingStock(ZBool currentAttributeUse, bool useAttributeInCsvFile, Lazy<bool> hasInventory, Lazy<bool> hasUnfinalisedASNLines)
		{
			var result = true;

			var hasChange = useAttributeInCsvFile != currentAttributeUse;
			if (hasChange)
			{
				result = !hasInventory.Value && !hasUnfinalisedASNLines.Value;
			}

			return result;
		}

		Action<string> Logger(PartsDataToLoad product) => (msg) => DisplayFormattedLogMessage(product.PartNo, product.PartDesc, msg);

		bool IsValidChangeOnPartAttribReleaseCaptured(PartsDataToLoad product, OrgPartRelation clientPartRelation)
		{
			clientPartRelation.OU_PickMode = product.PickMode.IsEmpty ? WhsPickMode.Codes.AttributeSpecified : product.PickMode;
			clientPartRelation.OU_RFAttributeConfirm = product.RFConfirm;
			clientPartRelation.OU_IsPartAttrib1ReleaseCaptured = product.IsPartAttrib1ReleaseCaptured;
			clientPartRelation.OU_IsPartAttrib2ReleaseCaptured = product.IsPartAttrib2ReleaseCaptured;
			clientPartRelation.OU_IsPartAttrib3ReleaseCaptured = product.IsPartAttrib3ReleaseCaptured;
			clientPartRelation.OU_IsSerialNumberReleaseCaptured = product.IsSerialNumberReleaseCaptured;

			clientPartRelation.Validation.ValidateOU_IsPartAttrib1ReleaseCaptured();
			DisplayFormattedLog(Logger(product), nameof(product.IsPartAttrib1ReleaseCaptured), clientPartRelation.OU_IsPartAttrib1ReleaseCapturedInfo.GetErrors());

			clientPartRelation.Validation.ValidateOU_IsPartAttrib2ReleaseCaptured();
			DisplayFormattedLog(Logger(product), nameof(product.IsPartAttrib2ReleaseCaptured), clientPartRelation.OU_IsPartAttrib2ReleaseCapturedInfo.GetErrors());

			clientPartRelation.Validation.ValidateOU_IsPartAttrib3ReleaseCaptured();
			DisplayFormattedLog(Logger(product), nameof(product.IsPartAttrib3ReleaseCaptured), clientPartRelation.OU_IsPartAttrib3ReleaseCapturedInfo.GetErrors());

			clientPartRelation.Validation.ValidateOU_IsSerialNumberReleaseCaptured();
			DisplayFormattedLog(Logger(product), nameof(product.IsSerialNumberReleaseCaptured), clientPartRelation.OU_IsSerialNumberReleaseCapturedInfo.GetErrors());

			clientPartRelation.Validation.ValidateOU_PickMode();
			DisplayFormattedLog(Logger(product), nameof(product.PickMode), clientPartRelation.OU_PickModeInfo.GetErrors());

			clientPartRelation.Validation.ValidateOU_RFAttributeConfirm();
			DisplayFormattedLog(Logger(product), nameof(product.RFConfirm), clientPartRelation.OU_RFAttributeConfirmInfo.GetErrors());

			return !clientPartRelation.OU_PickModeInfo.HasErrors()
				&& !clientPartRelation.OU_RFAttributeConfirmInfo.HasErrors()
				&& !clientPartRelation.OU_IsPartAttrib1ReleaseCapturedInfo.HasErrors()
				&& !clientPartRelation.OU_IsPartAttrib2ReleaseCapturedInfo.HasErrors()
				&& !clientPartRelation.OU_IsPartAttrib3ReleaseCapturedInfo.HasErrors()
				&& !clientPartRelation.OU_IsSerialNumberReleaseCapturedInfo.HasErrors();
		}

		bool IsValidChangeOnAdditionalFields(PartsDataToLoad product, OrgPartRelation clientPartRelation)
		{
			clientPartRelation.OU_RollUpAttributesOnDocuments = product.RollUpAttributes;
			clientPartRelation.OU_PackingDateFormatString = product.RFPackingDateFormat;
			clientPartRelation.OU_ExpiryDateFormatString = product.RFExpiryDateFormat;
			clientPartRelation.OU_CompletePalletPicking = product.RFCompletePalletPicking;
			clientPartRelation.OU_ConsigneeMinShelfLifeAccepted = product.ConsigneeMinShelfLifeAcceptedDays;
			clientPartRelation.OU_JulianBatchNoFormat = product.JulianBatchNumberFormat;

			clientPartRelation.Validation.ValidateOU_RollUpAttributesOnDocuments();
			DisplayFormattedLog(Logger(product), nameof(product.RollUpAttributes), clientPartRelation.OU_RollUpAttributesOnDocumentsInfo.GetErrors());

			clientPartRelation.Validation.ValidateOU_PackingDateFormatString();
			DisplayFormattedLog(Logger(product), nameof(product.RFPackingDateFormat), clientPartRelation.OU_PackingDateFormatStringInfo.GetErrors());

			clientPartRelation.Validation.ValidateOU_ExpiryDateFormatString();
			DisplayFormattedLog(Logger(product), nameof(product.RFExpiryDateFormat), clientPartRelation.OU_ExpiryDateFormatStringInfo.GetErrors());

			clientPartRelation.Validation.ValidateOU_CompletePalletPicking();
			DisplayFormattedLog(Logger(product), nameof(product.RFCompletePalletPicking), clientPartRelation.OU_CompletePalletPickingInfo.GetErrors());

			clientPartRelation.Validation.ValidateOU_ConsigneeMinShelfLifeAccepted();
			DisplayFormattedLog(Logger(product), nameof(product.ConsigneeMinShelfLifeAcceptedDays), clientPartRelation.OU_ConsigneeMinShelfLifeAcceptedInfo.GetErrors());

			clientPartRelation.Validation.ValidateOU_JulianBatchNoFormat();
			DisplayFormattedLog(Logger(product), nameof(product.JulianBatchNumberFormat), clientPartRelation.OU_JulianBatchNoFormatInfo.GetErrors());

			return !clientPartRelation.OU_PackingDateFormatStringInfo.HasErrors()
				&& !clientPartRelation.OU_ExpiryDateFormatStringInfo.HasErrors()
				&& !clientPartRelation.OU_CompletePalletPickingInfo.HasErrors()
				&& !clientPartRelation.OU_RollUpAttributesOnDocumentsInfo.HasErrors()
				&& !clientPartRelation.OU_ConsigneeMinShelfLifeAcceptedInfo.HasErrors()
				&& !clientPartRelation.OU_JulianBatchNoFormatInfo.HasErrors();
		}

		void DisplayFormattedLog(Action<string> action, string extraInfo, IEnumerable<INotification> errors)
		{
			errors.ForEach(e => action(FormattableString.Invariant($"{extraInfo}: {e.Message}")));
		}

		bool IsPartAttributesNotMatchesWithClients(OrgHeader[] clients, PartsDataToLoad product)
		{
			return clients.Length > 0 && clients.Any(c =>
				(!c.PartAttributeManager.IsPartAttributeUsedByOrganisation(1) && product.Use_Attribute1) ||
				(!c.PartAttributeManager.IsPartAttributeUsedByOrganisation(2) && product.Use_Attribute2) ||
				(!c.PartAttributeManager.IsPartAttributeUsedByOrganisation(3) && product.Use_Attribute3) ||
				(!c.PartAttributeManager.IsSerialNumberUsedByOrganisation && product.Use_SerialNumber));
		}

		#endregion

		#region LoadValues

		protected OrgSupplierPart LoadEnterpriseProduct(PartsDataToLoad dataToLoad)
		{
			var product = GetPartIfItExists(dataToLoad.PartNo, dataToLoad.PartOwnerCodePK, dataToLoad.PartSupplierCodePK);
			if (product == null)
			{
				var existingParts = GetExistingParts(dataToLoad.PartNo, dataToLoad.PartOwnerCodePK, dataToLoad.PartSupplierCodePK);
				if (existingParts.Length > 0)
				{
					product = existingParts[0];
				}
			}
			OrgSupplierPart result = null;

			if (IsDuplicate(dataToLoad, product, out var errorMessage))
			{
				DisplayFormattedLogMessage(dataToLoad.PartNo, dataToLoad.PartDesc, errorMessage);
				RunCounters.RecsExcluded++;
			}
			else if (product == null)
			{
				try
				{
					product = Factory.New<OrgSupplierPart>();
					product.SuspendValidation();
					LoadImportedPartValues(product, dataToLoad);
					if (ValidateProduct(product, Logger(dataToLoad)))
					{
						RunCounters.RecsToUpdate++;
						RunCounters.RecsCreated++;
						result = product;
					}
					else
					{
						product.Delete();
						RunCounters.RecsExcluded++;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					product.Delete();
					RunCounters.RecsExcluded++;
					DisplayFormattedLogMessage(dataToLoad.PartNo, dataToLoad.PartDesc, Res.GetString("544d5458-4d6d-4c1a-8796-5555b36e7a9b", "Unable to load this part - {0}", ex.Message));
				}
			}
			else
			{
				if (!product.OP_IsActive)
				{
					product.OP_IsActive = ZBool.True;
					var firstOwner = Res.GetString("54f859ec-b0e5-4f65-9a3d-804c858a65e2", "<No Owner>");
					var firstSupplier = Res.GetString("f5f4bdf7-dfc5-446c-8e8c-d36455f2eee3", "<No Supplier>");
					if (dataToLoad.PartOwnerCode != null && dataToLoad.PartOwnerCode.Count > 0)
					{
						firstOwner = dataToLoad.PartOwnerCode[0];
					}
					if (dataToLoad.PartSupplierCode != null && dataToLoad.PartSupplierCode.Count > 0)
					{
						firstSupplier = dataToLoad.PartSupplierCode[0];
					}
					DisplayFormattedLogMessage(dataToLoad.PartNo, dataToLoad.PartDesc, Res.GetString("7181D594-90F5-43B6-A274-1A377D43AF76", @"Owner [{0}] Supplier [{1}]: Inactive Part has been re-activated.", firstOwner, firstSupplier));
				}

				if (UpdatePartRecords)
				{
					try
					{
						UpdatePartDetails(product, dataToLoad);
						if (ValidateProduct(product, Logger(dataToLoad)))
						{
							UpdatePartRelatedRecords(product, dataToLoad);
							RunCounters.RecsToUpdate++;
							RunCounters.RecsUpdated++;
							result = product;
							DisplayUpdatedPartLogMessage(dataToLoad.PartNo);
						}
						else
						{
							product.CancelChanges();
							RunCounters.RecsExcluded++;
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						product.CancelChanges();
						RunCounters.RecsExcluded++;
						DisplayFormattedLogMessage(dataToLoad.PartNo, dataToLoad.PartDesc, Res.GetString("93CCE243-9B15-4F19-9A26-1A85F4BC4727", "Unable to update this part - {0}", ex.Message));
					}
				}
				else
				{
					RunCounters.RecsExcluded++;
					DisplayFormattedLogMessage(dataToLoad.PartNo, dataToLoad.PartDesc, Res.GetString("16b371f5-f465-465d-90b1-7493a5e9aad0", "Part No. excluded - already exists in {0}", BrandingFactory.Instance.ProductName));
					Factory.ClearQueryCache();
				}
			}
			if (result != null)
			{
				result.IsTopLevel = true;
			}
			UpdateAndDisplayIfRequired(result != null ? result.PK.ToGuid() : Guid.Empty, OrgSupplierPartSchema.Constants.TableName);

			return result;
		}

		void SetupProductSetterSuspenderIfNeeded(OrgSupplierPart enterprisePart, PartsDataToLoad dataToLoad)
		{
			var properties = ProductPropertiesToSuspendSetting;
			if (properties.Length > 0)
			{
				AddToDisposableList(enterprisePart.SetterSuspender.SuspendSetting(properties));
			}
		}

		ZString[] ProductPropertiesToSuspendSetting => productPropertiesToSuspendSetting ?? (productPropertiesToSuspendSetting = GetProductPropertiesToSuspendSetting().ToArray());
		ZString[] productPropertiesToSuspendSetting;

		protected virtual IEnumerable<ZString> GetProductPropertiesToSuspendSetting()
		{
			if (HasColumn(FieldNames.BrandName))
			{
				yield return OrgSupplierPart.Schema.OP_Brand;
			}
			if (HasColumn(FieldNames.Model))
			{
				yield return OrgSupplierPart.Schema.OP_Model;
			}
		}

		bool ValidateProduct(OrgSupplierPart product, Action<string> logger)
		{
			product.ResumeValidation();
			product.Validation.ValidateOP_MeasureUQ();
			var errors = product.OP_MeasureUQInfo.GetErrors();
			DisplayFormattedLog(logger, "PartMeasureUQ", errors);
			return !errors.Any();
		}

		protected void LoadImportedPartValues(OrgSupplierPart enterprisePart, PartsDataToLoad dataToLoad)
		{
			enterprisePart.OP_PartNum = dataToLoad.PartNo;
			UpdatePartDetails(enterprisePart, dataToLoad);
			SetUNDG(enterprisePart, dataToLoad);
			SetTariffClassificationDetailsAndOtherCountrySpecificDetails(enterprisePart, dataToLoad);
			UpdateOrganisationRelationships(enterprisePart, dataToLoad);
			UpdateUnitConversions(enterprisePart, dataToLoad.UnitConversions);
			UpdateAndValidatePartBarcodes(enterprisePart, dataToLoad.PartBarcodes);
		}

		void UpdateUnitConversions(OrgSupplierPart enterprisePart, IEnumerable<UnitConversion> unitConversions)
		{
			foreach (var unitConversion in unitConversions)
			{
				var packageType = GetStringValue(unitConversion.PackageType, Res.GetString("4f06fdc8-f918-494b-9590-b4100af20b6b", "Package type"), OrgPartUnit.Schema.OF_PackTypeMaxLength);
				var parentPackageType = GetStringValue(unitConversion.ParentPackageType, Res.GetString("0dc3928e-48de-4089-a581-b5b0e2ae2e6d", "Parent package type"), OrgPartUnit.Schema.OF_ParentPackTypeMaxLength);

				var partUnit = enterprisePart.PartUnits.GetUnitConversion(parentPackageType, packageType);
				if (partUnit == null)
				{
					partUnit = enterprisePart.PartUnits.AddNew();
					partUnit.OF_PackType = packageType;
					partUnit.OF_ParentPackType = parentPackageType;
				}

				partUnit.OF_QuantityInParent = unitConversion.QuantityInParent;
				partUnit.OF_Cubic = unitConversion.Cubic;
				partUnit.OF_Depth = unitConversion.Depth;
				partUnit.OF_Height = unitConversion.Height;
				partUnit.OF_Width = unitConversion.Width;
				partUnit.OF_Weight = unitConversion.Weight;
			}
		}

		void UpdateAndValidatePartBarcodes(OrgSupplierPart enterprisePart, IEnumerable<PartBarcode> partBarcodes)
		{
			if (partBarcodes.Any())
			{
				var newBarcodes = new List<OrgSupplierPartBarcode>();
				var barcodesChanged = new List<OrgSupplierPartBarcode>();
				var orgPartBarcodes = enterprisePart.PartBarcodes.Cast<OrgSupplierPartBarcode>();
				foreach (var partBarcode in partBarcodes)
				{
					var barcode = GetStringValue(partBarcode.Barcode, Res.GetString("da5c2893-d497-4313-b861-df06d576293e", "Part barcode"), OrgSupplierPartBarcode.Schema.PH_BarcodeMaxLength);
					var packType = GetStringValue(partBarcode.Package, Res.GetString("751d812d-0756-4a15-847f-ff4b89b10864", "Part barcode package type"), OrgSupplierPartBarcode.Schema.PH_F3_NKPackTypeMaxLength);

					var orgPartBarcode = orgPartBarcodes.FirstOrDefault(bc => bc.PH_Barcode.EqualsIgnoringCase(barcode));
					if (orgPartBarcode == null)
					{
						if (!OrgSupplierPartBarcodeValidationHelper.CheckIfPackTypeIsConvertible(packType, enterprisePart))
						{
							DisplayLogMessage(Res.GetString("48956b11-9276-4b4b-b64b-36e3031d70da", "Part barcode '{0}' cannot find conversion to Stock Unit from pack type '{1}', skipping import of this barcode.", barcode, packType));
						}
						else if (OrgSupplierPartBarcodeValidationHelper.CheckIfPackTypeIsVolumeOrWeightAndNotStockUnit(packType, enterprisePart))
						{
							DisplayLogMessage(Res.GetString("d9eeefed-0adb-4482-9fff-8d959080a2c0", "Part barcode '{0}'s pack type '{1}' is a weight or volume unit but it doesn't match the Stock Unit, skipping import of this barcode.", barcode, packType));
						}
						else if (!OrgSupplierPartBarcodeValidationHelper.CheckIfPackTypeIsStockUnit(packType, partBarcode.UseForDocuments, enterprisePart))
						{
							DisplayLogMessage(Res.GetString("56317d71-4b67-4958-b71d-415b4613c7ae", "Part barcode '{0}' cannot set Use for Documents as its pack type '{1}' is not the Stock Unit, skipping import of this barcode.", barcode, packType));
						}
						else
						{
							orgPartBarcode = enterprisePart.PartBarcodes.AddNew();
							orgPartBarcode.PH_Barcode = barcode;
							orgPartBarcode.PH_F3_NKPackType = packType;
							orgPartBarcode.PH_UseForDocuments = partBarcode.UseForDocuments;
							newBarcodes.Add(orgPartBarcode);
						}
					}
					else if (!orgPartBarcode.PH_F3_NKPackType.EqualsIgnoringCase(packType))
					{
						DisplayLogMessage(Res.GetString("3870c52a-de1b-40ee-b4b9-9eae76d3a79a", "Part barcode '{0}' already exists with package type '{1}', skipping import of this barcode with the pack type '{2}'.", barcode, orgPartBarcode.PH_F3_NKPackType, packType));
					}
					else if (orgPartBarcode.PH_UseForDocuments ^ partBarcode.UseForDocuments)
					{
						orgPartBarcode.PH_UseForDocuments = partBarcode.UseForDocuments;
						barcodesChanged.Add(orgPartBarcode);
					}
				}

				IEnumerable<INotification> finalErrors = null;
				var isValid = !orgPartBarcodes.Any(barcode =>
				{
					barcode.ResumeValidation();
					barcode.Validation.ValidatePH_UseForDocuments();
					finalErrors = barcode.PH_UseForDocumentsInfo.GetErrors();
					return finalErrors.Any();
				});

				// revert changes to PartBarcodes
				if (!isValid)
				{
					newBarcodes.ForEach(nb => enterprisePart.PartBarcodes.RemoveAndDelete(nb));
					barcodesChanged.ForEach(bc => bc.CancelChanges());
					DisplayLogMessage(Res.GetString("f794f78b-8681-4d2e-8d1a-5ca3cadace1d", "Validation failed for Part barcodes: {0} Skipping import of barcodes section.", finalErrors.FirstOrDefault().Message));
				}
			}
		}

#if DEBUG
		public
#endif
		void UpdateOrganisationRelationships(OrgSupplierPart enterprisePart, PartsDataToLoad dataToLoad)
		{
			if (dataToLoad.PartOwnerCodePK.Count > 0)
			{
				foreach (ZGuid onePartOwnerPK in dataToLoad.PartOwnerCodePK)
				{
					if (!onePartOwnerPK.IsEmpty)
					{
						AddOrgPartRelation(enterprisePart, onePartOwnerPK, Business.OrgPartRelation.RelationshipTypes.Owner, dataToLoad);
					}
				}
			}

			if (dataToLoad.PartSupplierCodePK.Count > 0)
			{
				foreach (ZGuid onePartSupplierCodePK in dataToLoad.PartSupplierCodePK)
				{
					if (!onePartSupplierCodePK.IsEmpty)
					{
						AddOrgPartRelation(enterprisePart, onePartSupplierCodePK, Business.OrgPartRelation.RelationshipTypes.Supplier, dataToLoad);
					}
				}
			}
		}

		protected void UpdatePartRelatedRecords(OrgSupplierPart enterprisePart, PartsDataToLoad dataToLoad)
		{
			SaveAndClearPreviousClassificationLookupDetailsIfPresentAndDifferent(enterprisePart, dataToLoad.PartClassification, dataToLoad.PartExportClassification, dataToLoad);
			SetUNDG(enterprisePart, dataToLoad);
			SetTariffClassificationDetailsAndOtherCountrySpecificDetails(enterprisePart, dataToLoad);

			if (dataToLoad.PartSupplierCodePK.Count > 0 || dataToLoad.PartOwnerCodePK.Count > 0)
			{
				UpdateOrganisationRelationships(enterprisePart, dataToLoad);
			}

			if (dataToLoad.UnitConversions.Count > 0)
			{
				UpdateUnitConversions(enterprisePart, dataToLoad.UnitConversions);
			}

			UpdateAndValidatePartBarcodes(enterprisePart, dataToLoad.PartBarcodes);
		}

		void UpdatePartDetails(OrgSupplierPart enterprisePart, PartsDataToLoad dataToLoad)
		{
			var suspender = enterprisePart.SetterSuspender;
			SetupProductSetterSuspenderIfNeeded(enterprisePart, dataToLoad);
			SetValue(enterprisePart.OP_DescInfo, dataToLoad.PartDesc);
			SetValue(enterprisePart.OP_DepartmentInfo, dataToLoad.PartDepartment);
			SetValue(enterprisePart.OP_DivisionInfo, dataToLoad.PartDivision);
			SetValue((ZPropertyInfoDecimal)enterprisePart.OP_QtyInStockInfo, dataToLoad.PartCount);
			SetValue(enterprisePart.OP_StockKeepingUnitInfo, dataToLoad.PartUQ.IsEmpty ? GetDefaultStockKeepingUnit() : dataToLoad.PartUQ);
			SetValue((ZPropertyInfoDecimal)enterprisePart.OP_WeightInfo, dataToLoad.PartWeight);
			SetValue((ZPropertyInfoDecimal)enterprisePart.OP_NetWeightInfo, dataToLoad.PartWeightNet);
			SetValue(enterprisePart.OP_WeightUQInfo, dataToLoad.PartWeightUnit);
			SetValue((ZPropertyInfoDecimal)enterprisePart.OP_CubicInfo, dataToLoad.PartVolume);
			SetValue(enterprisePart.OP_CubicUQInfo, dataToLoad.PartVolumeUnit);
			SetValue((ZPropertyInfoDecimal)enterprisePart.OP_LastCostInfo, dataToLoad.PartLastCost);
			SetValue((ZPropertyInfoDecimal)enterprisePart.OP_WeightedCostInfo, dataToLoad.PartWeightedCost);
			SetValue(enterprisePart.OP_RX_NKLastWeightedCostCurrInfo, dataToLoad.PartCostCurrency);
			SetValue((ZPropertyInfoDecimal)enterprisePart.OP_DepthInfo, dataToLoad.PartDepth);
			SetValue((ZPropertyInfoDecimal)enterprisePart.OP_WidthInfo, dataToLoad.PartWidth);
			SetValue((ZPropertyInfoDecimal)enterprisePart.OP_HeightInfo, dataToLoad.PartHeight);
			SetValue(enterprisePart.OP_MeasureUQInfo, dataToLoad.PartMeasureUQ);
			SetValue((ZPropertyInfoBool)enterprisePart.OP_KeepUprightInfo, dataToLoad.KeepUpright);
			SetValue(enterprisePart.OP_RH_NKCommodityCodeInfo, dataToLoad.Commodity);
			SetValue(enterprisePart.OP_BrandInfo, dataToLoad.PartBrandName, true, FieldNames.BrandName, setterSuspender: suspender, resumeSuspender: true);
			SetValue(enterprisePart.OP_ModelInfo, dataToLoad.PartModel, true, FieldNames.Model, setterSuspender: suspender, resumeSuspender: true);
			SetValue((ZPropertyInfoByte)enterprisePart.OP_CountDecimalPlacesInfo, dataToLoad.DecimalPlaces);

			ZString GetDefaultStockKeepingUnit()
			{
				const string untPackType = "UNT";

				var registryDefault = DataRegistry.Instance.DefaultStockUnit;
				return string.IsNullOrEmpty(registryDefault) ? untPackType : registryDefault;
			}
		}

		protected void SetValue(ZPropertyInfo info, ZInt? value, SetterSuspender setterSuspender = null, bool resumeSuspender = false)
		{
			SetValueCore(info, value.HasValue, () => value.Value, setterSuspender, resumeSuspender);
		}

		protected void SetValue(ZPropertyInfo info, ZBool? value, SetterSuspender setterSuspender = null, bool resumeSuspender = false)
		{
			SetValueCore(info, value.HasValue, () => value.Value, setterSuspender, resumeSuspender);
		}

		protected void SetValue(ZPropertyInfo info, ZDate? value, SetterSuspender setterSuspender = null, bool resumeSuspender = false)
		{
			SetValueCore(info, value.HasValue, () => value.Value, setterSuspender, resumeSuspender);
		}

		protected void SetValue(ZPropertyInfo info, ZDateTime? value, SetterSuspender setterSuspender = null, bool resumeSuspender = false)
		{
			SetValueCore(info, value.HasValue, () => value.Value, setterSuspender, resumeSuspender);
		}

		protected void SetValue(ZPropertyInfo info, ZDecimal? value, SetterSuspender setterSuspender = null, bool resumeSuspender = false)
		{
			SetValueCore(info, value.HasValue, () => value.Value, setterSuspender, resumeSuspender);
		}

		protected void SetValue(ZPropertyInfo info, ZString? value, bool logMaxLengthViolation = true, string propertyIdentifier = null, SetterSuspender setterSuspender = null, bool resumeSuspender = false)
		{
			SetValueCore(info, value.HasValue, () => GetStringValue(info, value.Value, logMaxLengthViolation, propertyIdentifier), setterSuspender, resumeSuspender);
		}

		protected void SetValue(ZPropertyInfo info, ZByte? value, SetterSuspender setterSuspender = null, bool resumeSuspender = false)
		{
			SetValueCore(info, value.HasValue, () => value.Value, setterSuspender, resumeSuspender);
		}

		ZString GetStringValue(ZPropertyInfo info, ZString value, bool logMaxLengthViolation = true, string propertyIdentifier = null)
		{
			if (string.IsNullOrEmpty(propertyIdentifier))
			{
				propertyIdentifier = info.HumanReadableName;
			}

			return GetStringValue(value, propertyIdentifier, info.MaxLength, logMaxLengthViolation);
		}

		ZString GetStringValue(ZString value, string propertyIdentifier, int maxLength, bool logMaxLengthViolation = true)
		{
			var actualValue = value;
			var truncatedValue = actualValue.Left(maxLength);
			if (logMaxLengthViolation && maxLength < actualValue.Length)
			{
				DisplayLogMessage(Res.GetString("{09F56544-F66F-468F-914F-D4C4C786936C}", "{0} '{1}' is too long. Storing '{2}' instead.", propertyIdentifier, value, truncatedValue));
			}

			return truncatedValue;
		}

		void SetValueCore<T>(ZPropertyInfo info, bool hasValue, Func<T> getValue, SetterSuspender setterSuspender = null, bool resumeSuspender = false)
			where T : IZType
		{
			if (hasValue)
			{
				if (setterSuspender != null && setterSuspender.IsSetterSuspended(info.Name))
				{
					if (resumeSuspender)
					{
						using (setterSuspender.ResumeSetting(info.Name))
						{
							info.Value = getValue();
						}
					}
				}
				else
				{
					info.Value = getValue();
				}
			}
		}

		void SetUNDG(OrgSupplierPart enterprisePart, PartsDataToLoad dataToLoad)
		{
			var subs = Factory.Load<UNDGSubstance>(dataToLoad.PartUNDGPK);
			if (subs != null && !enterprisePart.UNDGs.Any(u => u.DI_DG == subs.PK))
			{
				var dg = enterprisePart.UNDGs.AddNew();
				dg.DI_DG = subs.PK;
			}
		}

		void SetTariffClassificationDetailsAndOtherCountrySpecificDetails(OrgSupplierPart enterprisePart, PartsDataToLoad dataToLoad)
		{
			SetTariffAndClassificationDetails(enterprisePart, dataToLoad);
			LoadCountrySpecificProductData(enterprisePart, dataToLoad);
		}

		protected virtual void SetTariffAndClassificationDetails(OrgSupplierPart enterprisePart, PartsDataToLoad dataToLoad)
		{
			// Tariff must be loaded first if it has been provided in the data - lookups are being deprecated and should only be used when no tariff value is provided and where the lookup already exists
			// Pivot has both the tariff number and the classification lookup, but the lookup will be removed if a tariff number is loaded.
			if (!dataToLoad.ImportTariff.IsEmpty)
			{
				LoadCountrySpecificDataForTariffNum(enterprisePart, dataToLoad.ImportTariff, "IMP", dataToLoad);
			}
			else if (!dataToLoad.PartClassification.IsEmpty)
			{
				LoadCountrySpecificDataForLookup(enterprisePart, dataToLoad.PartClassification, "IMP", dataToLoad);
			}

			if (!dataToLoad.ExportTariff.IsEmpty)
			{
				LoadCountrySpecificDataForTariffNum(enterprisePart, dataToLoad.ExportTariff, "EXP", dataToLoad);
			}
			else if (!dataToLoad.PartExportClassification.IsEmpty)
			{
				LoadCountrySpecificDataForLookup(enterprisePart, dataToLoad.PartExportClassification, "EXP", dataToLoad);
			}

			if (!dataToLoad.PartFullDesc.IsEmpty)
			{
				AddDescriptionNote(enterprisePart, dataToLoad.PartFullDesc, (NoResString)"Full Product Description");
			}
		}

		protected virtual void SaveAndClearPreviousClassificationLookupDetailsIfPresentAndDifferent(OrgSupplierPart enterprisePart, ZString importLookup, ZString exportLookup, PartsDataToLoad dataToLoad)
		{
		}

		protected virtual void LoadCountrySpecificDataForLookup(OrgSupplierPart enterprisePart, ZString lookupCode, ZString importOrExport, PartsDataToLoad dataToLoad)
		{
		}

		protected virtual void LoadCountrySpecificDataForTariffNum(OrgSupplierPart enterprisePart, ZString tariffNum, ZString importOrExport, PartsDataToLoad dataToLoad)
		{
		}

		protected virtual void LoadCountrySpecificProductData(OrgSupplierPart enterprisePart, PartsDataToLoad dataToLoad)
		{
		}

		protected void AddOrgPartRelation(OrgSupplierPart enterprisePart, ZGuid ownerSupplierPK, string relationship, PartsDataToLoad dataToLoad)
		{
			OrgPartRelation orgPartLink = enterprisePart.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(ownerSupplierPK, relationship);
			if (orgPartLink == null)
			{
				orgPartLink = enterprisePart.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(ownerSupplierPK, Business.OrgPartRelation.RelationshipTypes.Both);
				if (orgPartLink == null)
				{
					ZString oppositeRelationship = relationship == Business.OrgPartRelation.RelationshipTypes.Owner ? Business.OrgPartRelation.RelationshipTypes.Supplier : Business.OrgPartRelation.RelationshipTypes.Owner;
					orgPartLink = enterprisePart.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(ownerSupplierPK, oppositeRelationship);
					if (orgPartLink != null)
					{
						orgPartLink.OU_Relationship = Business.OrgPartRelation.RelationshipTypes.Both;
					}
					else
					{
						orgPartLink = enterprisePart.RelatedOrganisations.AddNew();
						orgPartLink.OU_OH = ownerSupplierPK;
						orgPartLink.OU_Relationship = relationship;
					}
				}
			}

			OrgHeader ownerSupplier = Factory.Load<OrgHeader>(ownerSupplierPK);
			if (ownerSupplier != null && GlbBranch.CurrentBranch.Country.RN_Code == ownerSupplier.CountryCode)
			{
				if (!dataToLoad.LocalPartNumber.IsEmpty)
				{
					orgPartLink.OU_LocalPartNumber = dataToLoad.LocalPartNumber.Left(orgPartLink.OU_LocalPartNumberInfo.MaxLength);
				}

				if (!dataToLoad.LocalPartDescription.IsEmpty)
				{
					orgPartLink.OU_LocalPartDescription = dataToLoad.LocalPartDescription.Left(orgPartLink.OU_LocalPartDescriptionInfo.MaxLength);
				}
			}

			orgPartLink.OU_UsePartAttrib1 = dataToLoad.Use_Attribute1;
			orgPartLink.OU_UsePartAttrib2 = dataToLoad.Use_Attribute2;
			orgPartLink.OU_UsePartAttrib3 = dataToLoad.Use_Attribute3;
			orgPartLink.OU_IsPartAttrib1ReleaseCaptured = dataToLoad.IsPartAttrib1ReleaseCaptured;
			orgPartLink.OU_IsPartAttrib2ReleaseCaptured = dataToLoad.IsPartAttrib2ReleaseCaptured;
			orgPartLink.OU_IsPartAttrib3ReleaseCaptured = dataToLoad.IsPartAttrib3ReleaseCaptured;
			orgPartLink.OU_UseSerialNumber = dataToLoad.Use_SerialNumber;
			orgPartLink.OU_IsSerialNumberReleaseCaptured = dataToLoad.IsSerialNumberReleaseCaptured;
			orgPartLink.OU_Ti = dataToLoad.Ti;
			orgPartLink.OU_Hi = dataToLoad.Hi;
			orgPartLink.OU_UseExpiryDate = dataToLoad.UseExpiryDate;
			orgPartLink.OU_UsePackingDate = dataToLoad.UsePackingDate;
			orgPartLink.OU_ClientUQ = dataToLoad.ClientUQ;
			orgPartLink.OU_WHC_DefaultInventoryHoldCode = dataToLoad.DefaultHoldCodePK;
			orgPartLink.OU_UnitPrice = dataToLoad.UnitPrice?.Amount ?? 0m;
			orgPartLink.OU_RX_NKUnitPriceCurrency = dataToLoad.UnitPrice?.Currency?.Code ?? string.Empty;
			orgPartLink.OU_WCG_CartonGroup = dataToLoad.CartonGroupPK;
			orgPartLink.OU_CompletePalletPicking = dataToLoad.RFCompletePalletPicking;
			orgPartLink.OU_PackingDateFormatString = dataToLoad.RFPackingDateFormat;
			orgPartLink.OU_ExpiryDateFormatString = dataToLoad.RFExpiryDateFormat;
			orgPartLink.OU_ConsigneeMinShelfLifeAccepted = dataToLoad.ConsigneeMinShelfLifeAcceptedDays;
			orgPartLink.OU_JulianBatchNoFormat = dataToLoad.JulianBatchNumberFormat;
			orgPartLink.OU_RFAttributeConfirm = dataToLoad.RFConfirm;
			orgPartLink.OU_PickMode = dataToLoad.PickMode.IsEmpty ? WhsPickMode.Codes.AttributeSpecified : dataToLoad.PickMode;
			orgPartLink.OU_RollUpAttributesOnDocuments = dataToLoad.RollUpAttributes;
		}

		protected void AddDescriptionNote(OrgSupplierPart enterprisePart, ZString description, string noteType)
		{
			StmNote[] notes = enterprisePart.Notes.FindByDescription(noteType);
			if (notes.Length == 0)
			{
				StmNote newOrgNote = enterprisePart.Notes.AddNew();
				newOrgNote.ST_Description = noteType;
				newOrgNote.ST_Table = "OrgSupplierPart";
				newOrgNote.ST_NoteDataAsText = description;
			}
		}

		#endregion

		#region ExtractData

		protected void SetOwnerAndSupplierPKs(PartsDataToLoad record)
		{
			if ((record.PartOwnerCode.Count + record.PartSupplierCode.Count) == 0)
			{
				record.HasNoValidOwnerOrSupplier = true;
			}
			else
			{
				SetOwnerAndSupplierPKsCore(record.PartOwnerCode, out record.PartOwnerCodePK);
				SetOwnerAndSupplierPKsCore(record.PartSupplierCode, out record.PartSupplierCodePK);
				record.HasNoValidOwnerOrSupplier = (record.PartOwnerCodePK.Count + record.PartSupplierCodePK.Count) == 0;
			}
		}

		void SetOwnerAndSupplierPKsCore(List<ZString> ownerOrSupplierCodes, out List<ZGuid> ownerOrSupplierPKs)
		{
			ownerOrSupplierPKs = new List<ZGuid>();
			if (ownerOrSupplierCodes.Count > 0)
			{
				ZGuid ownerOrSupplierPK;
				foreach (ZString onePartOwnerCode in ownerOrSupplierCodes)
				{
					ownerOrSupplierPK = GetOwnerSupplierCodePK(onePartOwnerCode);
					if (!ownerOrSupplierPK.IsEmpty)
					{
						ownerOrSupplierPKs.Add(ownerOrSupplierPK);
					}
				}
			}
		}

		public static class FieldNames
		{
			public const string Code = nameof(Code);
			public const string Description = nameof(Description);
			public const string UQ = nameof(UQ);
			public const string Owner = nameof(Owner);
			public const string Supplier = nameof(Supplier);
			public const string Unit_Weight = nameof(Unit_Weight);
			public const string Unit_NetWeight = nameof(Unit_NetWeight);
			public const string Weight_Unit = nameof(Weight_Unit);
			public const string Unit_Volume = nameof(Unit_Volume);
			public const string Volume_Unit = nameof(Volume_Unit);
			public const string Department = nameof(Department);
			public const string Division = nameof(Division);
			public const string QtyInStock = nameof(QtyInStock);
			public const string Origin = nameof(Origin);
			public const string Last_Cost = nameof(Last_Cost);
			public const string UNDG_Code = nameof(UNDG_Code);
			public const string LocalPartNumber = nameof(LocalPartNumber);
			public const string LocalPartDescription = nameof(LocalPartDescription);
			public const string PartDepth = nameof(PartDepth);
			public const string PartWidth = nameof(PartWidth);
			public const string PartHeight = nameof(PartHeight);
			public const string PartMeasureUQ = nameof(PartMeasureUQ);
			public const string KeepUpright = nameof(KeepUpright);
			public const string DecimalPlaces = nameof(DecimalPlaces);
			public const string ClientUQ = nameof(ClientUQ);
			public const string DefaultHoldCode = nameof(DefaultHoldCode);
			public const string UnitPrice = nameof(UnitPrice);
			public const string UnitPriceCurrency = nameof(UnitPriceCurrency);
			public const string CartonGroup = nameof(CartonGroup);
			public const string UsageComment = nameof(UsageComment);
			public const string ClassificationDescription = nameof(ClassificationDescription);

			public const string Use_Attribute1 = nameof(Use_Attribute1);
			public const string Use_Attribute2 = nameof(Use_Attribute2);
			public const string Use_Attribute3 = nameof(Use_Attribute3);
			public const string Use_SerialNumber = nameof(Use_SerialNumber);
			public const string IsPartAttrib1ReleaseCaptured = nameof(IsPartAttrib1ReleaseCaptured);
			public const string IsPartAttrib2ReleaseCaptured = nameof(IsPartAttrib2ReleaseCaptured);
			public const string IsPartAttrib3ReleaseCaptured = nameof(IsPartAttrib3ReleaseCaptured);
			public const string IsSerialNumberReleaseCaptured = nameof(IsSerialNumberReleaseCaptured);
			public const string Ti = nameof(Ti);
			public const string Hi = nameof(Hi);
			public const string UseExpiryDate = nameof(UseExpiryDate);
			public const string UsePackingDate = nameof(UsePackingDate);

			public const string UC1_QtyParent = nameof(UC1_QtyParent);
			public const string UC2_QtyParent = nameof(UC2_QtyParent);
			public const string UC3_QtyParent = nameof(UC3_QtyParent);
			public const string UC4_QtyParent = nameof(UC4_QtyParent);
			public const string UC5_QtyParent = nameof(UC5_QtyParent);

			public const string UC1_Package = nameof(UC1_Package);
			public const string UC2_Package = nameof(UC2_Package);
			public const string UC3_Package = nameof(UC3_Package);
			public const string UC4_Package = nameof(UC4_Package);
			public const string UC5_Package = nameof(UC5_Package);

			public const string UC1_ParentPackage = nameof(UC1_ParentPackage);
			public const string UC2_ParentPackage = nameof(UC2_ParentPackage);
			public const string UC3_ParentPackage = nameof(UC3_ParentPackage);
			public const string UC4_ParentPackage = nameof(UC4_ParentPackage);
			public const string UC5_ParentPackage = nameof(UC5_ParentPackage);

			public const string UC1_Cubic = nameof(UC1_Cubic);
			public const string UC2_Cubic = nameof(UC2_Cubic);
			public const string UC3_Cubic = nameof(UC3_Cubic);
			public const string UC4_Cubic = nameof(UC4_Cubic);
			public const string UC5_Cubic = nameof(UC5_Cubic);

			public const string UC1_Depth = nameof(UC1_Depth);
			public const string UC2_Depth = nameof(UC2_Depth);
			public const string UC3_Depth = nameof(UC3_Depth);
			public const string UC4_Depth = nameof(UC4_Depth);
			public const string UC5_Depth = nameof(UC5_Depth);

			public const string UC1_Height = nameof(UC1_Height);
			public const string UC2_Height = nameof(UC2_Height);
			public const string UC3_Height = nameof(UC3_Height);
			public const string UC4_Height = nameof(UC4_Height);
			public const string UC5_Height = nameof(UC5_Height);

			public const string UC1_Width = nameof(UC1_Width);
			public const string UC2_Width = nameof(UC2_Width);
			public const string UC3_Width = nameof(UC3_Width);
			public const string UC4_Width = nameof(UC4_Width);
			public const string UC5_Width = nameof(UC5_Width);

			public const string UC1_Weight = nameof(UC1_Weight);
			public const string UC2_Weight = nameof(UC2_Weight);
			public const string UC3_Weight = nameof(UC3_Weight);
			public const string UC4_Weight = nameof(UC4_Weight);
			public const string UC5_Weight = nameof(UC5_Weight);

			public const string Commodity = nameof(Commodity);
			public const string BrandName = nameof(BrandName);
			public const string Model = nameof(Model);

			public const string Barcode1 = nameof(Barcode1);
			public const string Barcode2 = nameof(Barcode2);
			public const string Barcode3 = nameof(Barcode3);
			public const string Barcode4 = nameof(Barcode4);
			public const string Barcode5 = nameof(Barcode5);

			public const string Barcode1_Package = nameof(Barcode1_Package);
			public const string Barcode2_Package = nameof(Barcode2_Package);
			public const string Barcode3_Package = nameof(Barcode3_Package);
			public const string Barcode4_Package = nameof(Barcode4_Package);
			public const string Barcode5_Package = nameof(Barcode5_Package);

			public const string Barcode1_UseForDocuments = nameof(Barcode1_UseForDocuments);
			public const string Barcode2_UseForDocuments = nameof(Barcode2_UseForDocuments);
			public const string Barcode3_UseForDocuments = nameof(Barcode3_UseForDocuments);
			public const string Barcode4_UseForDocuments = nameof(Barcode4_UseForDocuments);
			public const string Barcode5_UseForDocuments = nameof(Barcode5_UseForDocuments);

			public const string Weighted_Cost = nameof(Weighted_Cost);
			public const string Cost_Currency = nameof(Cost_Currency);

			//EU-only fields for Customs Procedure Code, EC-SUPPLEMENT-1 and EC-SUPPLEMENT-2, Third quantity
			public const string ECSUPPLEMENT1 = nameof(ECSUPPLEMENT1);
			public const string ECSUPPLEMENT2 = nameof(ECSUPPLEMENT2);
			public const string CPC = nameof(CPC);
			public const string THIRDQTY = nameof(THIRDQTY);

			// Old Classification Fields
			public const string ExportClassification = nameof(ExportClassification);
			public const string ImportClassification = nameof(ImportClassification);
			public const string ImportTariff = nameof(ImportTariff);
			public const string ExportTariff = nameof(ExportTariff);

			// Old Classification Fields
			public const string ClassificationLookup = nameof(ClassificationLookup);
			public const string ClassificationType = nameof(ClassificationType);
			public const string Tariff = nameof(Tariff);

			public const string RFCompletePalletPicking = nameof(RFCompletePalletPicking);
			public const string RollUpAttributes = nameof(RollUpAttributes);
			public const string RFPackingDateFormat = nameof(RFPackingDateFormat);
			public const string RFExpiryDateFormat = nameof(RFExpiryDateFormat);
			public const string ConsigneeMinShelfLifeAcceptedDays = nameof(ConsigneeMinShelfLifeAcceptedDays);
			public const string JulianBatchNumberFormat = nameof(JulianBatchNumberFormat);
			public const string RFConfirm = nameof(RFConfirm);
			public const string PickMode = nameof(PickMode);
		}

		protected virtual bool UseOldClassificationFields => true;

		/// <summary>
		/// Override to copy data from Line to Record object - do not post to any other business objects, just Record
		/// </summary>
		/// <param name="line"></param>
		/// <param name="record"></param>
		protected virtual void PopulatePartsDataToLoad(OCsvLine line, PartsDataToLoad record)
		{
			record.PartSupplierCode = new List<ZString>();
			record.PartSupplierCodePK = new List<ZGuid>();
			record.PartOwnerCode = new List<ZString>();
			record.PartOwnerCodePK = new List<ZGuid>();
			record.UnitConversions = new List<UnitConversion>();
			record.PartBarcodes = new List<PartBarcode>();

			record.PartNo = TryGetStringValue(line, FieldNames.Code).Left(OrgSupplierPart.Schema.OP_PartNumMaxLength);
			TryGetValue(line, FieldNames.Description, out record.PartDesc);
			TryGetValue(line, FieldNames.LocalPartNumber, out record.LocalPartNumber);
			TryGetValue(line, FieldNames.LocalPartDescription, out record.LocalPartDescription);

			if (record.PartDesc.Length > OrgSupplierPart.Schema.OP_DescMaxLength)
			{
				record.PartFullDesc = record.PartDesc;
				record.PartDesc = record.PartDesc.SubstringSafe(0, OrgSupplierPart.Schema.OP_DescMaxLength);
			}
			else
			{
				record.PartFullDesc = "";
			}

			if (record.PartDesc == "")
			{
				record.PartDesc = record.PartNo;
			}

			record.PartUQ = TryGetStringValue(line, FieldNames.UQ).Left(3).ToUpper();

			if (UseOldClassificationFields)
			{
				TryGetValue(line, FieldNames.ExportClassification, out record.PartExportClassification);
				TryGetValue(line, FieldNames.ImportClassification, out record.PartClassification);
				TryGetValue(line, FieldNames.ImportTariff, out record.ImportTariff);
				TryGetValue(line, FieldNames.ExportTariff, out record.ExportTariff);
			}
			else
			{
				TryGetValue(line, FieldNames.ClassificationType, out ZString classficationType);
				if (classficationType.Length > CusClassPartPivotSchema.CI_ChildType.MaxLength)
				{
					record.HasInvalidClassificationType = true;
				}
				else
				{
					record.ClassificationType = classficationType;
				}

				TryGetValue(line, FieldNames.ClassificationLookup, out record.ClassificationLookup);
				TryGetValue(line, FieldNames.Tariff, out record.Tariff);
			}

			TryGetValue(line, FieldNames.UsageComment, out record.UsageComment);
			TryGetValue(line, FieldNames.ClassificationDescription, out record.ClassificationDescription);

			if (HasColumn(FieldNames.Owner))
			{
				record.PartOwnerCode = new List<ZString>();
				foreach (var ownerCode in TryGetStringValue(line, FieldNames.Owner).Split(';'))
				{
					var trimmedOwnerCode = ownerCode.Trim();
					if (!trimmedOwnerCode.IsEmpty)
					{
						record.PartOwnerCode.Add(trimmedOwnerCode);
					}
				}
			}

			if (HasColumn(FieldNames.Supplier))
			{
				record.PartSupplierCode = new List<ZString>();
				foreach (var supplierCode in TryGetStringValue(line, FieldNames.Supplier).Split(';'))
				{
					var trimmedSupplierCode = supplierCode.Trim();
					if (!trimmedSupplierCode.IsEmpty)
					{
						record.PartSupplierCode.Add(trimmedSupplierCode);
					}
				}
			}

			record.PartWeight = TryGetDecimalValue(line, FieldNames.Unit_Weight, 6, 3);
			record.PartWeightNet = TryGetDecimalValue(line, FieldNames.Unit_NetWeight, 6, 3);

			if (TryGetValue(line, FieldNames.Weight_Unit, out ZString weightUnit))
			{
				record.PartWeightUnit = weightUnit.Left(2);

				if (!weightUnit.IsEmpty && !Constants.Weight.ContainsCode(record.PartWeightUnit))
				{
					DisplayFormattedLogMessage(record.PartNo, record.PartDesc, Res.GetString("A60247D2-A5D4-450A-93D3-070085D240EE", "Warning: Invalid Weight Code '{0}'", weightUnit));
				}
			}

			record.PartVolume = TryGetDecimalValue(line, FieldNames.Unit_Volume, 6, 3);

			if (TryGetValue(line, FieldNames.Volume_Unit, out ZString volumeUnit))
			{
				record.PartVolumeUnit = volumeUnit.Left(2);

				if (!volumeUnit.IsEmpty && !Constants.Volume.ContainsCode(record.PartVolumeUnit))
				{
					DisplayFormattedLogMessage(record.PartNo, record.PartDesc, Res.GetString("8A0BBFAD-3A96-4476-A88B-411A42384E0F", "Warning: Invalid Volume Code '{0}'", volumeUnit));
				}
			}

			TryGetValue(line, FieldNames.Department, out record.PartDepartment);
			TryGetValue(line, FieldNames.Division, out record.PartDivision);

			record.PartCount = TryGetDecimalValue(line, FieldNames.QtyInStock, 7, 2);

			TryGetValue(line, FieldNames.Origin, out record.PartOrigin);
			record.PartLastCost = TryGetDecimalValue(line, FieldNames.Last_Cost, 10, 4);
			record.PartWeightedCost = TryGetDecimalValue(line, FieldNames.Weighted_Cost, 10, 4);
			record.PartDepth = TryGetDecimalValue(line, FieldNames.PartDepth, 9, 3);
			record.PartHeight = TryGetDecimalValue(line, FieldNames.PartHeight, 9, 3);
			record.PartWidth = TryGetDecimalValue(line, FieldNames.PartWidth, 9, 3);
			TryGetValue(line, FieldNames.KeepUpright, out record.KeepUpright);
			record.PartMeasureUQ = TryGetStringValue(line, FieldNames.PartMeasureUQ).Left(2);

			if (TryGetValue(line, FieldNames.Cost_Currency, out ZString currencyCode))
			{
				record.PartCostCurrency = RefCurrency.LoadFromCurrencyCode(Factory, currencyCode)?.RX_Code ?? ZString.Empty;
			}

			if (TryGetValue(line, FieldNames.UNDG_Code, out record.PartUNDGCode))
			{
				var unno = record.PartUNDGCode.SubstringSafe(0, 4);
				var variant = record.PartUNDGCode.SubstringSafe(4, 2);
				var standard = UNDGSubstanceStandardTypes.IMO;
				record.PartUNDGPK = UNDGSubstanceLoader.LoadSubstances(Factory, unno, variant, standard).FirstOrDefault()?.PK ?? ZGuid.Empty;
			}

			TryGetValue(line, FieldNames.Use_Attribute1, out record.Use_Attribute1);
			TryGetValue(line, FieldNames.Use_Attribute2, out record.Use_Attribute2);
			TryGetValue(line, FieldNames.Use_Attribute3, out record.Use_Attribute3);
			TryGetValue(line, FieldNames.IsPartAttrib1ReleaseCaptured, out record.IsPartAttrib1ReleaseCaptured);
			TryGetValue(line, FieldNames.IsPartAttrib2ReleaseCaptured, out record.IsPartAttrib2ReleaseCaptured);
			TryGetValue(line, FieldNames.IsPartAttrib3ReleaseCaptured, out record.IsPartAttrib3ReleaseCaptured);
			TryGetValue(line, FieldNames.Use_SerialNumber, out record.Use_SerialNumber);
			TryGetValue(line, FieldNames.IsSerialNumberReleaseCaptured, out record.IsSerialNumberReleaseCaptured);
			TryGetValue(line, FieldNames.Ti, out record.Ti);
			TryGetValue(line, FieldNames.Hi, out record.Hi);
			TryGetValue(line, FieldNames.UseExpiryDate, out record.UseExpiryDate);
			TryGetValue(line, FieldNames.UsePackingDate, out record.UsePackingDate);

			record.DecimalPlaces = TryGetByteValue(line, FieldNames.DecimalPlaces, (ZByte)9);
			AddUnitPrice(line, record);

			record.ClientUQ = TryGetStringValue(line, FieldNames.ClientUQ).Left(3);
			if (TryGetValue(line, FieldNames.DefaultHoldCode, out ZString defaultHoldCode))
			{
				var whsInventoryHeldCode = Factory.LoadFromNaturalKey<IWhsInventoryHeldCode>(WhsInventoryHeldCodeSchema.WHC_Code, defaultHoldCode);
				record.DefaultHoldCodePK = whsInventoryHeldCode?.PK ?? ZGuid.Empty;
			}

			if (TryGetValue(line, FieldNames.CartonGroup, out ZString cartonGroupCode))
			{
				var cartonGroup = Factory.LoadFromNaturalKey<IWhsCartonGroup>(WhsCartonGroupSchema.WCG_Code, cartonGroupCode);
				record.CartonGroupPK = cartonGroup?.PK ?? ZGuid.Empty;
			}

			AddUnitConversion(line, record, FieldNames.UC1_QtyParent, FieldNames.UC1_Package, FieldNames.UC1_ParentPackage, FieldNames.UC1_Cubic, FieldNames.UC1_Depth, FieldNames.UC1_Height, FieldNames.UC1_Width, FieldNames.UC1_Weight);
			AddUnitConversion(line, record, FieldNames.UC2_QtyParent, FieldNames.UC2_Package, FieldNames.UC2_ParentPackage, FieldNames.UC2_Cubic, FieldNames.UC2_Depth, FieldNames.UC2_Height, FieldNames.UC2_Width, FieldNames.UC2_Weight);
			AddUnitConversion(line, record, FieldNames.UC3_QtyParent, FieldNames.UC3_Package, FieldNames.UC3_ParentPackage, FieldNames.UC3_Cubic, FieldNames.UC3_Depth, FieldNames.UC3_Height, FieldNames.UC3_Width, FieldNames.UC3_Weight);
			AddUnitConversion(line, record, FieldNames.UC4_QtyParent, FieldNames.UC4_Package, FieldNames.UC4_ParentPackage, FieldNames.UC4_Cubic, FieldNames.UC4_Depth, FieldNames.UC4_Height, FieldNames.UC4_Width, FieldNames.UC4_Weight);
			AddUnitConversion(line, record, FieldNames.UC5_QtyParent, FieldNames.UC5_Package, FieldNames.UC5_ParentPackage, FieldNames.UC5_Cubic, FieldNames.UC5_Depth, FieldNames.UC5_Height, FieldNames.UC5_Width, FieldNames.UC5_Weight);
			AddPartBarcode(line, record, FieldNames.Barcode1, FieldNames.Barcode1_Package, FieldNames.Barcode1_UseForDocuments);
			AddPartBarcode(line, record, FieldNames.Barcode2, FieldNames.Barcode2_Package, FieldNames.Barcode2_UseForDocuments);
			AddPartBarcode(line, record, FieldNames.Barcode3, FieldNames.Barcode3_Package, FieldNames.Barcode3_UseForDocuments);
			AddPartBarcode(line, record, FieldNames.Barcode4, FieldNames.Barcode4_Package, FieldNames.Barcode4_UseForDocuments);
			AddPartBarcode(line, record, FieldNames.Barcode5, FieldNames.Barcode5_Package, FieldNames.Barcode5_UseForDocuments);

			if (record.PartUQ.IsEmpty)
			{
				SetRecordUQFromTariff(record);
			}

			if (TryGetValue(line, FieldNames.Commodity, out ZString value))
			{
				record.Commodity = Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, value)?.RH_Code ?? ZString.Empty;
			}

			TryGetValue(line, FieldNames.BrandName, out record.PartBrandName);
			TryGetValue(line, FieldNames.Model, out record.PartModel);
			SetOwnerAndSupplierPKs(record);

			TryGetValue(line, FieldNames.RFCompletePalletPicking, out record.RFCompletePalletPicking);
			TryGetValue(line, FieldNames.RollUpAttributes, out record.RollUpAttributes);
			TryGetValue(line, FieldNames.RFPackingDateFormat, out record.RFPackingDateFormat);
			TryGetValue(line, FieldNames.RFExpiryDateFormat, out record.RFExpiryDateFormat);
			TryGetValue(line, FieldNames.ConsigneeMinShelfLifeAcceptedDays, out record.ConsigneeMinShelfLifeAcceptedDays);
			TryGetValue(line, FieldNames.JulianBatchNumberFormat, out record.JulianBatchNumberFormat);
			TryGetValue(line, FieldNames.RFConfirm, out record.RFConfirm);
			TryGetValue(line, FieldNames.PickMode, out record.PickMode);
		}

		void AddUnitConversion(OCsvLine line, PartsDataToLoad record, string qtyField, string packageField, string inPackageField, string cubic, string depth, string height, string width, string weight)
		{
			if (HasColumn(qtyField) && HasColumn(packageField) && HasColumn(inPackageField))
			{
				decimal qtyInParent = TryGetDecimalValue(line, qtyField, 8, 5);
				if (qtyInParent != 0)
				{
					var unitConvertion = new UnitConversion(qtyInParent, TryGetStringValue(line, packageField), TryGetStringValue(line, inPackageField));
					unitConvertion.Cubic = TryGetDecimalValue(line, cubic, 9, 3);
					unitConvertion.Depth = TryGetDecimalValue(line, depth, 9, 3);
					unitConvertion.Height = TryGetDecimalValue(line, height, 9, 3);
					unitConvertion.Width = TryGetDecimalValue(line, width, 9, 3);
					unitConvertion.Weight = TryGetDecimalValue(line, weight, 9, 3);

					record.UnitConversions.Add(unitConvertion);
				}
			}
		}

		void AddPartBarcode(OCsvLine line, PartsDataToLoad record, string barcodeField, string packageField, string useForDocumentsField)
		{
			if (HasColumn(packageField) && HasColumn(barcodeField))
			{
				var barcode = TryGetStringValue(line, barcodeField);
				var package = TryGetStringValue(line, packageField);
				if (barcode != "" && package != "")
				{
					var useForDocuments = false;
					if (HasColumn(useForDocumentsField))
					{
						TryGetValue(line, useForDocumentsField, out useForDocuments);
					}
					record.PartBarcodes.Add(new PartBarcode(barcode, package, useForDocuments));
				}
				else
				{
					var logMessage = Res.GetString("34B3C7A6-7833-4279-B025-6DEEA821D681", "{0} (Barcode: '{1}', Pack Type: '{2}') for Product '{3}' is invalid. This Part Barcode will not be imported. Please enter a non-empty Barcode and Pack Type for each Part Barcode.", barcodeField, barcode, package, record.PartNo);
					DisplayLogMessage(logMessage);
				}
			}
		}

		void AddUnitPrice(OCsvLine line, PartsDataToLoad record)
		{
			record.UnitPrice = null;
			RefCurrency unitPriceCurrency = null;
			if (TryGetValue(line, FieldNames.UnitPriceCurrency, out ZString unitPriceCurrencyCode))
			{
				unitPriceCurrency = RefCurrency.LoadFromCurrencyCode(Factory, unitPriceCurrencyCode);
			}

			if (HasColumn(FieldNames.UnitPrice) && unitPriceCurrency == null)
			{
				var logMessage = Res.GetString("56227c05-0ea8-4e3d-b973-80b7879292fe",
						"The Unit Price is invalid because '{0}' is not a valid currency. The unit price will not be imported. Please enter a valid currency to import a unit price.", unitPriceCurrencyCode);
				DisplayLogMessage(logMessage);
			}
			else
			{
				var amount = TryGetDecimalValue(line, FieldNames.UnitPrice, 15, 2, checkDecimalPlaces: true);
				if (IsValidMoneyAmount(amount))
				{
					record.UnitPrice = new Money(amount, unitPriceCurrency);
				}
				else
				{
					var logMessage = Res.GetString("10ee29d2-b26c-4ce9-91ac-11191cb5af48",
						"The Unit Price of {0} is invalid. Valid value range for unit price is between 0 and {1}. The unit price will not be imported.", amount, MaxMoneyAmount);
					DisplayLogMessage(logMessage);
				}
			}
		}

		bool IsValidMoneyAmount(ZDecimal amount)
		{
			return amount >= 0 && amount <= MaxMoneyAmount;
		}

		const decimal MaxMoneyAmount = 922337203685477.58m;

		#endregion

		#region SetRecordUQFromTariff

		protected virtual void SetRecordUQFromTariff(PartsDataToLoad record)
		{
		}

		#endregion

		#region Utilities

		void DisplayFormattedLogMessage(ZString partNo, ZString partDesc, string detailedExceptionMessage)
		{
			var ouputRowNo = Res.GetString("45a31565-f7e8-4f65-ba1a-ec8be18f4f1f", "Line {0}:", RunCounters.CurrentRow.ToString());
			var logMessage = Res.GetString("ed086d35-c07c-409d-9cf7-ee06f4b3a4a1", "{0} PART NO/DESC: {1} / {2}  {3}", ouputRowNo, partNo, partDesc, detailedExceptionMessage);

			DisplayLogMessage(logMessage);
		}

		void DisplayUpdatedPartLogMessage(ZString partNo)
		{
			string logMessage = Res.GetString("7cccc474-d2c1-477b-92b2-07c10789a8b8", "PART NO: {0} - Part has been UPDATED", partNo);

			DisplayLogMessage(logMessage);
		}

		void DisplayFormattedLookupError(ZString partNo, ZString impLookup, ZString expLookup)
		{
			var ouputRowNo = Res.GetString("45a31565-f7e8-4f65-ba1a-ec8be18f4f1f", "Line {0}:", RunCounters.CurrentRow.ToString());
			var logMessage = Res.GetString("0c03a6d2-bebd-41ad-a376-5cb8d7a309b0", "{0} PART NO: {1} excluded, Tariff Lookup", ouputRowNo, partNo) + " ";

			if (!impLookup.IsEmpty && !expLookup.IsEmpty)
			{
				logMessage = Res.GetString("3e81a87f-18ee-47f2-95d9-d8fb278b2dcf", "{0}{1} or {2} was not found", logMessage, impLookup, expLookup);
			}
			else if (!impLookup.IsEmpty)
			{
				logMessage = Res.GetString("faa7413f-0694-45da-aaf9-5b68a04e82a7", "{0}{1} was not found", logMessage, impLookup);
			}
			else
			{
				logMessage = Res.GetString("faa7413f-0694-45da-aaf9-5b68a04e82a7", "{0}{1} was not found", logMessage, expLookup);
			}

			DisplayLogMessage(logMessage);
		}

		/// <summary>
		/// Search for organisation will only match using the code provided directly as the Enterprise OrgCode
		/// Enhanced Nov 06 to allow search on Legacy Codes as well
		/// </summary>
		/// <param name="ownerSupplierCode"></param>
		/// <returns></returns>
		ZGuid GetOwnerSupplierCodePK(string ownerSupplierCode)
		{
			if (UseLegacyCodes)
			{
				return GetOwnerSupplierCodeFromLegacyCode(ownerSupplierCode);
			}
			return GetOwnerSupplierCodeFromEnterpriseCode(ownerSupplierCode);
		}

		ZGuid GetOwnerSupplierCodeFromEnterpriseCode(string ownerSupplierCode)
		{
			ZGuid ownerSupplierPK = ZGuid.Empty;

			OrgHeader enterpriseOrg = OrgHeader.LoadFromCode(Factory, ownerSupplierCode);
			if (enterpriseOrg != null)
			{
				ownerSupplierPK = enterpriseOrg.PK;
			}

			return ownerSupplierPK;
		}

		ZGuid GetOwnerSupplierCodeFromLegacyCode(string ownerSupplierCode)
		{
			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			OrgHeader result = new OrgHeader.Loader(Factory).LoadFromLegacyCode(countryCode, ownerSupplierCode);
			return result != null ? result.PK : ZGuid.Empty;
		}

		protected virtual ZGuid GetClassificationPK(string partClassificationCode, string classType)
		{
			return ZGuid.Empty;
		}

		protected virtual string GetUQFromTariff(ZString classificationLookup, ZString classType)
		{
			return "";
		}

		OrgSupplierPart[] GetExistingParts(ZString partNo, List<ZGuid> ownerPKs, List<ZGuid> supplierPKs)
		{
			ZDBOnlyQuery partQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			partQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, partNo);
			var hasOwners = ownerPKs != null && ownerPKs.Count > 0;
			var hasSuppliers = supplierPKs != null && supplierPKs.Count > 0;
			if (hasOwners)
			{
				ZDBOnlySubQuery ownerSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
				ownerSubQuery.AddToFilter(OrgPartRelationSchema.OU_OH, ownerPKs);
				ownerSubQuery.AddToFilter(OrgPartRelationSchema.OU_Relationship, new ZString[] { OrgPartRelation.RelationshipTypes.Owner, OrgPartRelation.RelationshipTypes.Both });
				partQuery.AddSubQuery(ownerSubQuery, JoinCondition.And);
			}

			if (hasSuppliers)
			{
				ZDBOnlySubQuery supplierSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
				supplierSubQuery.AddToFilter(OrgPartRelationSchema.OU_OH, supplierPKs);
				supplierSubQuery.AddToFilter(OrgPartRelationSchema.OU_Relationship, new ZString[] { OrgPartRelation.RelationshipTypes.Supplier, OrgPartRelation.RelationshipTypes.Both });
				partQuery.AddSubQuery(supplierSubQuery, JoinCondition.And);
			}

			partQuery.OrderBy = OrgSupplierPartSchema.OP_IsActive.Name + " DESC";

			var result = Factory.Load<OrgSupplierPart>(partQuery);
			if (result.Length > 0 && hasOwners != hasSuppliers)
			{
				var filteredResult = new List<OrgSupplierPart>(result.Length);
				foreach (var part in result)
				{
					if (hasOwners)
					{
						var supplierQuery = new ZQuery(OrgPartRelationSchema.OU_Relationship, new ZString[] { OrgPartRelation.RelationshipTypes.Supplier, OrgPartRelation.RelationshipTypes.Both });
						supplierQuery.AddToFilter(OrgPartRelationSchema.OU_OH, SQLComparisonOperator.NotEqual, ownerPKs);
						if (part.RelatedOrganisations.Find(supplierQuery).Length == 0)
						{
							filteredResult.Add(part);
						}
					}
					else
					{
						var ownerQuery = new ZQuery(OrgPartRelationSchema.OU_Relationship, new ZString[] { OrgPartRelation.RelationshipTypes.Owner, OrgPartRelation.RelationshipTypes.Both });
						ownerQuery.AddToFilter(OrgPartRelationSchema.OU_OH, SQLComparisonOperator.NotEqual, supplierPKs);
						if (part.RelatedOrganisations.Find(ownerQuery).Length == 0)
						{
							filteredResult.Add(part);
						}
					}
				}
				result = filteredResult.ToArray();
			}

			return result;
		}

		OrgSupplierPart GetPartIfItExists(ZString partNo, List<ZGuid> ownerPKs, List<ZGuid> supplierPKs)
		{
			ZGuid firstOwnerPk = ZGuid.Empty;
			ZGuid firstSupplierPk = ZGuid.Empty;
			if (ownerPKs != null && ownerPKs.Count > 0)
			{
				firstOwnerPk = ownerPKs[0];
			}

			if (supplierPKs != null && supplierPKs.Count > 0)
			{
				firstSupplierPk = supplierPKs[0];
			}

			return new OrgSupplierPart.Loader(Factory).Load(partNo, firstOwnerPk, firstSupplierPk, throwIfAmbiguousMatchDetected: true);
		}

		protected virtual bool ClassificationLookupExistsOrIsNotRequired(ZString impLookup, ZString expLookup)
		{
			bool classOK = true;

			if (!impLookup.IsEmpty)
			{
				if (GetClassificationPK(impLookup, "IMP").IsEmpty)
				{
					classOK = false;
				}
			}

			if (!expLookup.IsEmpty && classOK)
			{
				if (GetClassificationPK(expLookup, "EXP").IsEmpty)
				{
					classOK = false;
				}
			}

			return classOK;
		}

		#endregion

		#region Validation

		public override string CSVTemplateHeading
		{
			get { return string.Join(",", CSVTemplateHeaders); }
		}

		protected virtual IEnumerable<string> CSVTemplateHeaders
		{
			get
			{
				yield return FieldNames.Code;
				yield return FieldNames.Description;
				yield return FieldNames.UQ;
				if (UseOldClassificationFields)
				{
					yield return FieldNames.ExportClassification;
					yield return FieldNames.ImportClassification;
				}
				else
				{
					yield return FieldNames.ClassificationType;
					yield return FieldNames.ClassificationLookup;
					yield return FieldNames.Tariff;
				}
				yield return FieldNames.Owner;
				yield return FieldNames.Supplier;
				yield return FieldNames.Unit_Weight;
				yield return FieldNames.Unit_NetWeight;
				yield return FieldNames.Weight_Unit;
				yield return FieldNames.Unit_Volume;
				yield return FieldNames.Volume_Unit;
				yield return FieldNames.Department;
				yield return FieldNames.Division;
				yield return FieldNames.QtyInStock;
				yield return FieldNames.Origin;
				yield return FieldNames.Last_Cost;
				yield return FieldNames.UNDG_Code;
				yield return FieldNames.LocalPartNumber;
				yield return FieldNames.LocalPartDescription;
				yield return FieldNames.PartDepth;
				yield return FieldNames.PartWidth;
				yield return FieldNames.PartHeight;
				yield return FieldNames.PartMeasureUQ;
				yield return FieldNames.Use_Attribute1;
				yield return FieldNames.Use_Attribute2;
				yield return FieldNames.Use_Attribute3;
				yield return FieldNames.Use_SerialNumber;
				yield return FieldNames.IsPartAttrib1ReleaseCaptured;
				yield return FieldNames.IsPartAttrib2ReleaseCaptured;
				yield return FieldNames.IsPartAttrib3ReleaseCaptured;
				yield return FieldNames.IsSerialNumberReleaseCaptured;
				yield return FieldNames.Ti;
				yield return FieldNames.Hi;
				yield return FieldNames.UseExpiryDate;
				yield return FieldNames.UsePackingDate;
				yield return FieldNames.UC1_QtyParent;
				yield return FieldNames.UC1_Package;
				yield return FieldNames.UC1_ParentPackage;
				yield return FieldNames.UC1_Cubic;
				yield return FieldNames.UC1_Depth;
				yield return FieldNames.UC1_Height;
				yield return FieldNames.UC1_Width;
				yield return FieldNames.UC1_Weight;
				yield return FieldNames.UC2_QtyParent;
				yield return FieldNames.UC2_Package;
				yield return FieldNames.UC2_ParentPackage;
				yield return FieldNames.UC2_Cubic;
				yield return FieldNames.UC2_Depth;
				yield return FieldNames.UC2_Height;
				yield return FieldNames.UC2_Width;
				yield return FieldNames.UC2_Weight;
				yield return FieldNames.UC3_QtyParent;
				yield return FieldNames.UC3_Package;
				yield return FieldNames.UC3_ParentPackage;
				yield return FieldNames.UC3_Cubic;
				yield return FieldNames.UC3_Depth;
				yield return FieldNames.UC3_Height;
				yield return FieldNames.UC3_Width;
				yield return FieldNames.UC3_Weight;
				yield return FieldNames.UC4_QtyParent;
				yield return FieldNames.UC4_Package;
				yield return FieldNames.UC4_ParentPackage;
				yield return FieldNames.UC4_Cubic;
				yield return FieldNames.UC4_Depth;
				yield return FieldNames.UC4_Height;
				yield return FieldNames.UC4_Width;
				yield return FieldNames.UC4_Weight;
				yield return FieldNames.UC5_QtyParent;
				yield return FieldNames.UC5_Package;
				yield return FieldNames.UC5_ParentPackage;
				yield return FieldNames.UC5_Cubic;
				yield return FieldNames.UC5_Depth;
				yield return FieldNames.UC5_Height;
				yield return FieldNames.UC5_Width;
				yield return FieldNames.UC5_Weight;
				yield return FieldNames.Commodity;
				yield return FieldNames.BrandName;
				yield return FieldNames.Model;
				if (UseOldClassificationFields)
				{
					yield return FieldNames.ImportTariff;
					yield return FieldNames.ExportTariff;
				}
				yield return FieldNames.Barcode1;
				yield return FieldNames.Barcode1_Package;
				yield return FieldNames.Barcode1_UseForDocuments;
				yield return FieldNames.Barcode2;
				yield return FieldNames.Barcode2_Package;
				yield return FieldNames.Barcode2_UseForDocuments;
				yield return FieldNames.Barcode3;
				yield return FieldNames.Barcode3_Package;
				yield return FieldNames.Barcode3_UseForDocuments;
				yield return FieldNames.Barcode4;
				yield return FieldNames.Barcode4_Package;
				yield return FieldNames.Barcode4_UseForDocuments;
				yield return FieldNames.Barcode5;
				yield return FieldNames.Barcode5_Package;
				yield return FieldNames.Barcode5_UseForDocuments;
				yield return FieldNames.Weighted_Cost;
				yield return FieldNames.Cost_Currency;
				yield return FieldNames.KeepUpright;
				yield return FieldNames.DecimalPlaces;
				yield return FieldNames.ClientUQ;
				yield return FieldNames.DefaultHoldCode;
				yield return FieldNames.UnitPrice;
				yield return FieldNames.UnitPriceCurrency;
				yield return FieldNames.CartonGroup;
				yield return FieldNames.UsageComment;
				yield return FieldNames.ClassificationDescription;
				yield return FieldNames.RFCompletePalletPicking;
				yield return FieldNames.RollUpAttributes;
				yield return FieldNames.RFPackingDateFormat;
				yield return FieldNames.RFExpiryDateFormat;
				yield return FieldNames.ConsigneeMinShelfLifeAcceptedDays;
				yield return FieldNames.JulianBatchNumberFormat;
				yield return FieldNames.RFConfirm;
				yield return FieldNames.PickMode;
			}
		}

		#endregion

		#region Audit

		public string AuditMessage { get; set; }

		void SetAuditMessageForClassificationLines(OrgSupplierPart enterprisePart, ZString message)
		{
			foreach (var pivot in GetPivots(enterprisePart))
			{
				SecurityCheckpoint security = null;

				if (pivot.IsHTB)
				{
					security = Env.Security.CustomsSupplierPartAuditImport.IsAllowed ? Env.Security.CustomsSupplierPartAuditExport : Env.Security.CustomsSupplierPartAuditImport;
				}
				else if (pivot.IsImportClassification)
				{
					security = Env.Security.CustomsSupplierPartAuditImport;
				}
				else if (pivot.IsExportClassification)
				{
					security = Env.Security.CustomsSupplierPartAuditExport;
				}

				if (security != null)
				{
					if (security.IsAllowed)
					{
						var businessObjectLogger = new BusinessObjectLogger(enterprisePart, (BusinessObject)pivot);
						businessObjectLogger.Reference = message;
						businessObjectLogger.WriteToLog();
					}
					else
					{
						DisplayFormattedLogMessage(enterprisePart.OP_PartNum, enterprisePart.OP_Desc, Res.GetString("f68b1149-f4ae-4494-9b22-ff86808eb3e4", "Failed to audit Classification Line. Reason: {0}.", security.ErrorMessageForNotAllowed));
					}
				}
			}
		}

		IBaseCusClassPartPivot[] GetPivots(OrgSupplierPart orgSupplier)
		{
			var partPivotFilter = new ZQuery(CusClassPartPivotSchema.CI_OP, orgSupplier.PK);
			partPivotFilter.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, null);
			partPivotFilter.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			return orgSupplier.Factory.Load<IBaseCusClassPartPivot>(partPivotFilter);
		}

		#endregion
	}
	#endregion
}
