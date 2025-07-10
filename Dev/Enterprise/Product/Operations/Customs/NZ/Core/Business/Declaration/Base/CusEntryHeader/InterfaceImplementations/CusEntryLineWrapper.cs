using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CusEntryLineWrapper : IGoodsItems
	{
		public CusEntryLineWrapper(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, "EntryLine cannot be null");
		}

		readonly CusEntryLine entryLine;

		#region IGoodsItems

		public ZDecimal ValueForDutyInNZD
		{
			get { return entryLine.VFDWholeNZD; }   //CL_CustomsValue
		}

		public ZString TransitionalFacilityCode
		{
			get { return entryLine.Declaration.JE_ATFOtherInfoValue; }
		}

		public ZString GoodsDescription
		{
			get { return entryLine.Description; }
		}

		public ZString LotNumber
		{
			get { return entryLine.LotNumber; }
		}

		public ZDate DateMarking
		{
			get { return entryLine.DateMarking.Date; }
		}

		public ZBool HasForeignCurrency
		{
			get { return !entryLine.RandomLine.JI_RX_NKLinePriceCurr.IsEmpty && entryLine.RandomLine.JI_RX_NKLinePriceCurr != Core.Constants.CurrencyCodes.NewZealand; }
		}

		public ZDecimal ValueInForeignCurrency
		{
			get
			{
				if (entryLine.Declaration.IsExport)
				{
					if (entryLine.Declaration.IsTSWDeclaration && LineInvoiceIsForwardCoverInvoice)
					{
						return entryLine.ForeignAmount;
					}
					else
					{
						return entryLine.FOB.Amount;
					}
				}
				else
				{
					return HasForeignCurrency ? entryLine.FOB.Amount : ZDecimal.Zero;
				}
			}
		}

		bool LineInvoiceIsForwardCoverInvoice
		{
			get
			{
				if (!entryLine.OSCurrencyCode.IsEmpty)
				{
					foreach (JobComInvoiceHeader invoiceHeader in entryLine.EntryHeader.InvoiceHeaders)
					{
						if (invoiceHeader.JZ_RX_NKInvoice_Currency == entryLine.OSCurrencyCode)
						{
							return invoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable;
						}
					}
				}

				return false;
			}
		}

		public ZString ForeignCurrencyCode
		{
			get
			{
				if (entryLine.Declaration.IsExport)
				{
					if (entryLine.Declaration.IsTSWDeclaration && LineInvoiceIsForwardCoverInvoice)
					{
						return entryLine.OSCurrencyCode;
					}
					else
					{
						return entryLine.FOB.Currency.Code;
					}
				}
				else
				{
					return HasForeignCurrency ? entryLine.FOB.Currency.Code : string.Empty;
				}
			}
		}

		public ZString IntendedUse
		{
			get { return entryLine.IntendedUse; }
		}

		public ZString IntendedUseCode
		{
			get { return entryLine.IntendedUseCode; }
		}

		public IEnumerable<IClassification> Classifications
		{
			get
			{
				if (!entryLine.CL_AdValoremTariff.IsEmpty)
				{
					yield return new ICRClassification(entryLine.CL_AdValoremTariff, ClassificationTypeList.Codes.HS);
				}

				if (!entryLine.ConcessionCode.IsEmpty)
				{
					yield return new ICRClassification(entryLine.ConcessionCode, ClassificationTypeList.Codes.CV);
				}

				if (!entryLine.UNDGNo.IsEmpty)
				{
					yield return new ICRClassification(entryLine.UNDGNo, ClassificationTypeList.Codes.SSO);
				}

				foreach (CommodityLine commodityLine in entryLine.CommodityLines)
				{
					yield return new ICRClassification(commodityLine.NZ_Classification, commodityLine.NZ_ClassificationType);
				}
			}
		}

		public IEnumerable<IConstituent> Constituents
		{
			get
			{
				foreach (CommodityConstituent commodityConstituent in entryLine.CommodityConstituents)
				{
					var constituent = new Constituent(commodityConstituent.NZ_ConstituentQty, commodityConstituent.NZ_ConstituentName);
					yield return constituent;
				}
			}
		}

		public ZString PreferenceClaimed
		{
			get
			{
				var result = (ZString)PreferenceClaimedList.Codes.NML;
				if (entryLine.PreferentialDutyIndicator == QualifiesForPreferentialDutyList.Codes.Qualifies)
				{
					if (entryLine.PreferentialDutyGroup.IsEmpty)
					{
						result = entryLine.BestPreferentialDutyGroup;
					}
					else
					{
						result = entryLine.PreferentialDutyGroup;
					}
				}

				return result;
			}
		}

		public IEnumerable<IDutyTaxFee> LineDutyTaxFees
		{
			get
			{
				var aLACLevyDutyTaxFee = GetDutyTaxFee(entryLine.ALACLevyAmount, DutyTaxFeeTypeList.Codes.AL);
				if (aLACLevyDutyTaxFee != null)
				{
					yield return aLACLevyDutyTaxFee;
				}

				var hERALevyDutyTaxFee = GetDutyTaxFee(entryLine.HERALevyAmount, DutyTaxFeeTypeList.Codes.SL);
				if (hERALevyDutyTaxFee != null)
				{
					yield return hERALevyDutyTaxFee;
				}

				var pFMLLevyDutyTaxFee = GetDutyTaxFee(entryLine.PFMLFuelLevyAmount, DutyTaxFeeTypeList.Codes.PF);
				if (pFMLLevyDutyTaxFee != null)
				{
					yield return pFMLLevyDutyTaxFee;
				}

				var aCCFuelLevyDutyTaxFee = GetDutyTaxFee(entryLine.ACCFuelLevyAmount, DutyTaxFeeTypeList.Codes.AC);
				if (aCCFuelLevyDutyTaxFee != null)
				{
					yield return aCCFuelLevyDutyTaxFee;
				}

				var syntheticGreenhouseGasesDutyTaxFee = GetDutyTaxFee(entryLine.SyntheticGreenhouseGasesLevyAmount, DutyTaxFeeTypeList.Codes.GG);
				if (syntheticGreenhouseGasesDutyTaxFee != null)
				{
					yield return syntheticGreenhouseGasesDutyTaxFee;
				}

				var antiDumpingDutyDutyTaxFee = GetDutyTaxFee(entryLine.AntiDumpingDutyAmount, DutyTaxFeeTypeList.Codes.ADD);
				if (antiDumpingDutyDutyTaxFee != null)
				{
					yield return antiDumpingDutyDutyTaxFee;
				}

				var countervailingDutyDutyTaxFee = GetDutyTaxFee(entryLine.CountervailingDutyAmount, DutyTaxFeeTypeList.Codes.CVD);
				if (countervailingDutyDutyTaxFee != null)
				{
					yield return countervailingDutyDutyTaxFee;
				}

				var entryHeaderDuty = GetDutyTaxFee(entryLine.DutyAmount, DutyTaxFeeTypeList.Codes.CUD);
				if (entryHeaderDuty != null)
				{
					yield return entryHeaderDuty;
				}

				var entryHeaderGST = GetDutyTaxFee(entryLine.GSTAmount, DutyTaxFeeTypeList.Codes.GST);
				if (entryHeaderGST != null)
				{
					yield return entryHeaderGST;
				}

				// Advice from Customs, total not required at GAGI level

				if (entryLine.Declaration.IsDrawback)
				{
					var dutyCredit = GetDutyTaxFee(entryLine.DutyCreditAmount, DutyTaxFeeTypeList.Codes.CUD);
					if (dutyCredit != null)
					{
						yield return dutyCredit;
					}

					var aLACLevyCredit = GetDutyTaxFee(entryLine.ALACLevyCreditAmount, DutyTaxFeeTypeList.Codes.AL);
					if (aLACLevyCredit != null)
					{
						yield return aLACLevyCredit;
					}

					var aCCLevyCredit = GetDutyTaxFee(entryLine.ACCLevyCreditAmount, DutyTaxFeeTypeList.Codes.AC);
					if (aCCLevyCredit != null)
					{
						yield return aCCLevyCredit;
					}

					var hERALevyCredit = GetDutyTaxFee(entryLine.HERALevyCreditAmount, DutyTaxFeeTypeList.Codes.SL);
					if (hERALevyCredit != null)
					{
						yield return hERALevyCredit;
					}

					var pFMLFuelLevyCredit = GetDutyTaxFee(entryLine.PFMLFuelLevyCreditAmount, DutyTaxFeeTypeList.Codes.PF);
					if (pFMLFuelLevyCredit != null)
					{
						yield return pFMLFuelLevyCredit;
					}

					var gSTCredit = GetDutyTaxFee(entryLine.GSTCreditAmount, DutyTaxFeeTypeList.Codes.GST);
					if (gSTCredit != null)
					{
						yield return gSTCredit;
					}
				}
			}
		}

		DutyTaxFee GetDutyTaxFee(ZDecimal amount, ZString typeCode)
		{
			DutyTaxFee result = null;
			if (amount > 0)
			{
				result = new DutyTaxFee(amount, typeCode, Core.Constants.CurrencyCodes.NewZealand);
			}

			return result;
		}

		//Conditional – Specify the details of the grower of crops for Tariff chapters 6, 7, 8, 10, 12
		//				Must be transmitted to state the name of the Grower if different to the Supplier
		public IOrganisation Grower
		{
			get
			{
				if (!entryLine.RandomLine.GrowerOrgPK.IsEmpty)
				{
					var grower = entryLine.Factory.Load<OrgHeader>(entryLine.RandomLine.GrowerOrgPK);
					if (grower != null && grower != entryLine.Supplier)
					{
						return new OrgHeaderWrapper(grower);
					}
				}

				return null;
			}
		}

		//Conditional – Must be transmitted where goods have passed through multiple countries enroute to New Zealand
		public IEnumerable<ZString> RoutingCountryCodes
		{
			get
			{
				LoadRoutingLegs();
				foreach (CommodityItinerary itinerary in entryLine.CommodityItineraries)
				{
					if (!itinerary.NZ_RoutingCountry.IsEmpty)
					{
						yield return itinerary.NZ_RoutingCountry;
					}
				}
			}
		}

		void LoadRoutingLegs()
		{
			entryLine.CommodityItineraries.RemoveAndDeleteAll();
			if (entryLine.Declaration.Transports.Count > 1)
			{
				var originalLoadPort = entryLine.Declaration.JE_RL_NKPortOfLoading;
				foreach (ITransport leg in entryLine.Declaration.Transports)
				{
					if (leg.JW_RL_NKLoadPort != originalLoadPort)
					{
						var itineraryLeg = entryLine.CommodityItineraries.AddNew();
						itineraryLeg.NZ_RoutingCountry = leg.JW_RL_NKLoadPort.SubstringSafe(0, 2);
					}
				}
			}
		}

		//Conditional - Manufacturer must be provided where different to supplier
		public IOrganisation Manufacturer
		{
			get
			{
				if (!entryLine.RandomLine.ManufacturerOrgPK.IsEmpty)
				{
					var manufacturer = entryLine.Factory.Load<OrgHeader>(entryLine.RandomLine.ManufacturerOrgPK);
					if (manufacturer != null && manufacturer != entryLine.Supplier)
					{
						return new OrgHeaderWrapper(manufacturer);
					}
				}

				return null;
			}
		}

		//Conditional – Must be transmitted to specify the producer details for processed food of plant origin and processed food of animal origin within Tariff chapters 2 - 22
		//		Must be transmitted to state the name of the Producer if different to the Supplier
		//		Note: A producer includes someone involved in the production and harvesting of animal and plant products
		public IOrganisation Producer
		{
			get
			{
				if (!entryLine.RandomLine.ProducerOrgPK.IsEmpty)
				{
					var producer = entryLine.Factory.Load<OrgHeader>(entryLine.RandomLine.ProducerOrgPK);
					if (producer != null && producer != entryLine.Supplier)
					{
						return new OrgHeaderWrapper(producer);
					}
				}

				return null;
			}
		}

		//Conditional - Must be stated for motor vehicles, animals, used machinery. May optionally be transmitted to state any unique number for the product assigned by the manufacturer, producer or grower
		//	values:	Id, IdType, ProductName, ProductNameType, CharacteristicCode, CharacteristicType
		public IEnumerable<IProduct> Products
		{
			get
			{
				foreach (CommodityProduct commodityProduct in entryLine.CommodityProducts)
				{
					yield return new ProductDetails(commodityProduct.NZ_ProductID, commodityProduct.NZ_ProductIDType);
				}
			}
		}

		public ZString BrandName
		{
			get { return entryLine.BrandName; }
		}

		public ZString CommonName
		{
			get { return entryLine.CommonName; }
		}

		public ZString RegisteredName
		{
			get { return entryLine.RegisteredName; }
		}

		public ZString TradeName
		{
			get { return entryLine.TradeName; }
		}

		public ZBool UsedGoods
		{
			get { return entryLine.UsedGoods; }
		}

		public ZBool GeneticallyModified
		{
			get { return entryLine.GeneticallyModified; }
		}

		public ZString ExportCountry
		{
			get { return entryLine.CountryOfExport; }
		}

		//Optional - use to state any special temperature information for the goods. 
		public ITemperatureRequirements Temperatures
		{
			get
			{
				ITemperatureRequirements result = null;
				if (entryLine.RandomLine.JI_TemperatureDetailsToBeSent)
				{
					result = new TemperatureRequirements(entryLine.RandomLine.JI_StorageTemp, entryLine.RandomLine.JI_MinTemp, entryLine.RandomLine.JI_MaxTemp);
				}

				return result;
			}
		}

		public IEnumerable<ZString> ContainerNumbers
		{
			get
			{
				if (entryLine.Declaration.CusContainers.Count > 0)
				{
					foreach (JobComInvoiceLine invLine in entryLine.InvoiceLines)
					{
						foreach (Customs.Business.NonPersistentCusContainer lineContainer in invLine.ContainersForInvoiceLinesForBindingOnly)
						{
							if (lineContainer.IsForInvoiceLine)
							{
								yield return lineContainer.ContainerNumber;
							}
						}
					}
				}
			}
		}

		public IOrganisationSimple TreatmentProvider
		{
			get { return OrgHeaderWrapper.New(entryLine.TreatmentProvider); }
		}

		public ZDecimal ItemGrossWeightInKGM
		{
			get { return entryLine.EffectiveGrossWeight.IsValid ? entryLine.EffectiveGrossWeight.InKilograms : 0; }
		}

		public ZDecimal ItemNetWeightInKGM
		{
			get { return entryLine.EffectiveNetWeight.IsValid ? entryLine.EffectiveNetWeight.InKilograms : 0; }
		}

		public ZDecimal StatisticalQty
		{
			get { return entryLine.StatisticalQty; }
		}

		public ZString StatisticalQtyUnit
		{
			get { return entryLine.StatisticalUnit; }
		}

		public ZDecimal SupplementaryQty
		{
			get { return entryLine.SupplementaryQty; }
		}

		public ZString SupplementaryQtyUnit
		{
			get { return entryLine.SupplementaryUQ; }
		}

		public ZString OriginCountry
		{
			get { return entryLine.CountryOfOrigin; }
		}

		public ZString OriginRegion
		{
			get { return entryLine.OriginRegion; }
		}

		//package type to be 2 char code from UN EDIFACT Recommendation 21 Annex VI, not current enterprise 3 char code
		public IEnumerable<IPackaging> Packaging
		{
			get
			{
				foreach (ItemPackaging packaging in entryLine.ItemPackaging)
				{
					yield return new LinePackaging(packaging.NZ_ShippingMarks, packaging.NZ_NumberOfPackages, packaging.NZ_PackageUQ, packaging.NZ_PackingMaterial, packaging.NZ_PackageVolume);
				}
			}
		}

		//	Must be transmitted to state the type of valuation adjustment. Freight and Insurance must be specified in all instances.
		public IEnumerable<IValuationAdjustment> Adjustments
		{
			get
			{
				//151 = Freight
				var adjustment = new ValuationAdjustment(entryLine.FreightWholeNZD, ValuationAdjustmentTypeList.Codes.V151);
				yield return adjustment;

				//150 = Insurance
				adjustment = new ValuationAdjustment(entryLine.InsuranceWholeNZD, ValuationAdjustmentTypeList.Codes.V150);
				yield return adjustment;

				//149 = Commissions
				var commissionAmount = entryLine.CommissionInNZD;
				if (commissionAmount > 0)
				{
					adjustment = new ValuationAdjustment(commissionAmount, ValuationAdjustmentTypeList.Codes.V149);
					yield return adjustment;
				}

				//146 = Royalties
				var royaltiesAmount = entryLine.RoyaltiesInNZD;
				if (royaltiesAmount > 0)
				{
					adjustment = new ValuationAdjustment(royaltiesAmount, ValuationAdjustmentTypeList.Codes.V146);
					yield return adjustment;
				}
			}
		}

		public IEnumerable<ZString> Permits
		{
			get
			{
				foreach (PermitCode permit in entryLine.PermitCodes)
				{
					// Format Permit Authority,Permit Number (e.g. MEL,nnnnn)
					yield return permit.ZO_Code + "," + permit.ZO_Data;
				}
			}
		}

		public IEnumerable<ZString> ProhibitedCodes
		{
			get
			{
				foreach (ProhibitedCode prohibitedCode in entryLine.ProhibitedCodes)
				{
					yield return prohibitedCode.ZO_Code;
				}
			}
		}

		public IEnumerable<IOtherInfo> OtherInfoCodes
		{
			get
			{
				foreach (Business.OtherInfo otherInfoDetail in entryLine.OtherInfos)
				{
					if (otherInfoDetail.ZO_Code != HeaderOtherInfoList.Codes.MAFContainerDeclaration && otherInfoDetail.ZO_Code != HeaderOtherInfoList.Codes.ApprovedTransitionalFacility)
					{
						yield return new OtherInfo(otherInfoDetail.ZO_Code, otherInfoDetail.ZO_Data);
					}
				}
			}
		}

		public ZString RelationshipIndicator
		{
			get
			{
				var result = "135"; // Parties are not related
				if (entryLine.RelationshipIndicator == "Y")
				{
					result = "136"; // Parties related, affects price paid or payable
				}
				else if (entryLine.RelationshipIndicator == "R")
				{
					result = "137"; // Parties related, does not affect price
				}

				return result;
			}
		}

		public ZInt SupplierLineIsRelatedTo
		{
			get
			{
				var result = 1;
				var relatedInvoiceSupplier = entryLine.Supplier;
				var supplierPosition = 0;
				foreach (JobComInvoiceHeader invoice in entryLine.EntryHeader.InvoiceHeaders)
				{
					supplierPosition++;
					if (relatedInvoiceSupplier == invoice.Supplier)
					{
						result = supplierPosition;
						break;
					}
				}

				return result;
			}
		}

		public ZBool IsPartsRelated
		{
			get
			{
				foreach (Business.OtherInfo otherInfoDetail in entryLine.OtherInfos)
				{
					if (otherInfoDetail.ZO_Code == LineOtherInfoList.Codes.Parts)
					{
						return true;
					}
				}

				return false;
			}
		}

		public ZString VendorIdentifier => entryLine.RandomLine.InvoiceHeader?.JZ_SupplierGSTNumber.KeepAlphanumericCharacters() ?? ZString.Empty;

		public ZString IsGSTPrePaid => entryLine.RandomLine.InvoiceHeader?.JZ_IsGSTPrePaid ?? ZString.Empty;

		#endregion

		#region Constituent

		class Constituent : IConstituent
		{
			public Constituent(ZDecimal constituentQty, ZString constituentName)
			{
				this.constituentQty = constituentQty;
				this.constituentName = constituentName;
			}
			readonly ZDecimal constituentQty;
			readonly ZString constituentName;

			public ZDecimal ConstituentQuantity
			{
				get { return constituentQty; }
			}

			public ZString ConstituentName
			{
				get { return constituentName; }
			}
		}

		#endregion

		#region DutyTaxFee

		class DutyTaxFee : IDutyTaxFee
		{
			public DutyTaxFee(ZDecimal value, ZString typeCode, ZString currencyCode)
			{
				this.value = value;
				this.typeCode = typeCode;
				this.currencyCode = currencyCode;
			}
			readonly ZDecimal value;
			readonly ZString typeCode;
			readonly ZString currencyCode;

			public ZDecimal Amount
			{
				get { return value.Round(2); }
			}

			public ZString DutyTaxFeeType
			{
				get { return typeCode; }
			}

			public ZString CurrencyCode
			{
				get { return currencyCode; }
			}
		}

		#endregion

		#region Packaging

		class LinePackaging : IPackaging
		{
			public LinePackaging(ZString marks, ZInt numberOfPackages, ZString packageType, ZString packingMaterial, ZDecimal packageVolume)
			{
				shippingMarks = marks;
				this.numberOfPackages = numberOfPackages;
				this.packageType = packageType;
				packingMaterialDesc = packingMaterial;
				packageVolumeInMTQ = packageVolume;
			}

			readonly ZString shippingMarks;
			readonly ZInt numberOfPackages;
			readonly ZString packageType;
			readonly ZString packingMaterialDesc;
			readonly ZDecimal packageVolumeInMTQ;

			public ZString ShippingMarks
			{
				get { return shippingMarks; }
			}

			public ZInt NumberOfPackages
			{
				get { return numberOfPackages; }
			}

			public ZString PackageType
			{
				get { return packageType; }
			}

			public ZString PackingMaterialDesc
			{
				get { return packingMaterialDesc; }
			}

			public ZDecimal PackageVolumeInMTQ
			{
				get { return packageVolumeInMTQ; }
			}

			public ZInt MessageSequence
			{
				get { return fMessageSequence; }
				set { fMessageSequence = value; }
			}
			ZInt fMessageSequence;

			public ZString RelatedHB
			{
				get { return ZString.Empty; }
			}

			public ZString RelatedContainer
			{
				get { return ZString.Empty; }
			}

			public ZGuid PK
			{
				get { return ZGuid.Empty; }
			}
		}

		#endregion

		#region Temperature Requirements

		class TemperatureRequirements : ITemperatureRequirements
		{
			public TemperatureRequirements(ZDecimal store, ZDecimal min, ZDecimal max)
			{
				storageTemp = store;
				minStorageTemp = min;
				maxStorageTemp = max;
			}
			readonly ZDecimal storageTemp;
			readonly ZDecimal minStorageTemp;
			readonly ZDecimal maxStorageTemp;
			const string tempUnit = "CEL";

			public ZDecimal StorageTemp
			{
				get { return storageTemp; }
			}

			public ZString StorageTempUnit
			{
				get { return tempUnit; }
			}

			public ZDecimal MinStorageTemp
			{
				get { return minStorageTemp; }
			}

			public ZString MinStorageTempUnit
			{
				get { return tempUnit; }
			}

			public ZDecimal MaxStorageTemp
			{
				get { return maxStorageTemp; }
			}

			public ZString MaxStorageTempUnit
			{
				get { return tempUnit; }
			}
		}

		#endregion

		#region Product Details

		class ProductDetails : IProduct
		{
			public ProductDetails(ZString id, ZString idType)
			{
				this.id = id;
				this.idType = idType;
			}

			readonly ZString id;
			readonly ZString idType;

			public ZString Id
			{
				get { return id; }
			}

			public ZString IdType
			{
				get { return idType; }
			}
		}

		#endregion

		#region DutyTaxFee

		class ValuationAdjustment : IValuationAdjustment
		{
			public ValuationAdjustment(ZDecimal amount, ZString code)
			{
				this.amount = amount;
				this.code = code;
			}
			readonly ZDecimal amount;
			readonly ZString code;

			public ZString AdjustmentQualifier
			{
				get { return code; }
			}

			public ZDecimal AdjustmentAmountInNZD
			{
				get { return amount; }
			}
		}

		#endregion

		#region OtherInfo

		class OtherInfo : IOtherInfo
		{
			public OtherInfo(ZString code, ZString data)
			{
				this.code = code;
				this.data = data;
			}
			readonly ZString code;
			readonly ZString data;

			public ZString Code
			{
				get { return code; }
			}

			public ZString Data
			{
				get { return data; }
			}
		}

		#endregion
	}
}
