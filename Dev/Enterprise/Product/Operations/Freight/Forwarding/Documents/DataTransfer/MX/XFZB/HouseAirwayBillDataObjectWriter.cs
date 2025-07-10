using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.MX;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.AWB;
using Enterprise.UniversalDataBuss.Integration;
using PackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.MX
{
	sealed class HouseAirwayBillDataObjectWriter : DataObjectWriter<HouseAirwayBill, UniversalShipment>
	{
		public HouseAirwayBillDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		const string decimalFormat = "0.000";

		protected override UniversalShipment PopulateDataObject(HouseAirwayBill hawb)
		{
			var uxmlShipment = CreateUniveralShipment(hawb);

			uxmlShipment.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();

				PopulateGoodsDescription(hawb, uxmlShipment);
				PopulateValues(hawb, uxmlShipment, addInfos);
				PopulateLocations(hawb, uxmlShipment, addInfos);
				PopulateTotals(hawb, uxmlShipment, addInfos);
				PopulateReferences(hawb, uxmlShipment, addInfos);

				PopulateSpecialHandlingCodes(hawb, uxmlShipment);
				PopulateSecurityStatus(hawb, uxmlShipment);
				PopulateSignature(hawb, addInfos);
				PopulateAddresses(hawb, uxmlShipment);
				PopulateRateLines(hawb, uxmlShipment);

				if (addInfos.Count > 0)
				{
					return addInfos;
				}
				return uxmlShipment.AddInfoCollection;
			});

			return uxmlShipment;
		}

		UniversalShipment CreateUniveralShipment(HouseAirwayBill hawb)
		{
			var universalShipment = new UniversalShipment(writeManager.WriterStrategy);
			universalShipment.DataContext = hawb
				.CreateUXmlDataContext()
				.AddDataProvider()
				.AddUserBranchAndDepartment();

			return universalShipment;
		}

		#region PopulateGoodsDescription

		void PopulateGoodsDescription(HouseAirwayBill hawb, UniversalShipment uxmlShipment)
		{
			if (hawb.RateLines != null && hawb.RateLines.Count > 0)
			{
				var natureAndQtyOfGoodsList = hawb.RateLines.Where(item => item.NatureAndQtyOfGoods != ZString.Empty)
																	.Select(item => item.NatureAndQtyOfGoods).ToArray();
				uxmlShipment.GoodsDescription = ZString.Join(" ", natureAndQtyOfGoodsList);
			}
		}

		#endregion

		#region PopulateValues

		void PopulateValues(HouseAirwayBill hawb, UniversalShipment uxmlShipment, ICollection<AddInfo> addInfos)
		{
			uxmlShipment.GoodsValue = hawb.CarriageValue.Amount;
			uxmlShipment.GoodsValueCurrency = new Currency
			{
				Code = hawb.CarriageValue.Currency.Code,
				Description = hawb.CarriageValue.Currency.Description
			};

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(hawb.CustomsValue)}{nameof(hawb.CustomsValue.Amount)}"), // programmatic constant
				Value = hawb.CustomsValue.Amount.ToString(decimalFormat, CultureInfo.InvariantCulture)
			});

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(hawb.CustomsValue)}{nameof(hawb.CustomsValue.Currency)}{nameof(hawb.CustomsValue.Currency.Code)}"), // programmatic constant
				Value = hawb.CustomsValue.Currency?.Code
			});

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(hawb.CustomsValue)}{nameof(hawb.CustomsValue.Currency)}{nameof(hawb.CustomsValue.Currency.Description)}"), // programmatic constant
				Value = hawb.CustomsValue.Currency?.Description
			});

			uxmlShipment.InsuranceValue = hawb.InsuranceValue.Amount;
			uxmlShipment.InsuranceValueCurrency = new Currency
			{
				Code = hawb.InsuranceValue.Currency.Code,
				Description = hawb.InsuranceValue.Currency.Description
			};
		}

		#endregion

		#region PopulateLocations

		void PopulateLocations(HouseAirwayBill hawb, UniversalShipment uxmlShipment, ICollection<AddInfo> addInfos)
		{
			if (hawb.AirportOfDeparture != null)
			{
				uxmlShipment.PortOfOrigin = new UNLOCO
				{
					Code = hawb.AirportOfDeparture.Code,
					Name = hawb.AirportOfDeparture.Name
				};
			}

			if (hawb.AirportOfDestination != null)
			{
				uxmlShipment.PortOfDestination = new UNLOCO
				{
					Code = hawb.AirportOfDestination.Code,
					Name = hawb.AirportOfDestination.Name
				};
			}
		}

		#endregion

		#region PopulateTotals

		void PopulateTotals(HouseAirwayBill hawb, UniversalShipment uxmlShipment, ICollection<AddInfo> addInfos)
		{
			uxmlShipment.TotalNoOfPacks = hawb.RateLines.Sum(rateline => rateline.NoOfPieces);
			uxmlShipment.TotalWeight = hawb.RateLines.Sum(rateline =>
			{
				if (!rateline.GrossWeight?.Unit?.Code.IsEmpty ?? false)
				{
					return Constants.Weight.Convert(rateline.GrossWeight.Value, MapWeightUnit(rateline.GrossWeight.Unit.Code), Constants.Weight.Kilograms);
				}
				return decimal.Zero;
			});
			uxmlShipment.TotalWeightUnit = GetUnitOfWeight(Constants.Weight.Kilograms);

			foreach (var totalCharge in CreateTotalCharges(hawb))
			{
				addInfos.Add(totalCharge);
			}
		}

		IEnumerable<AddInfo> CreateTotalCharges(HouseAirwayBill hawb)
		{
			if (!hawb.AirportOfDeparture.IsEmpty())
			{
				foreach (var addInfo in hawb.AirportOfDeparture.ToUXmlAddInfos("OperationalPort"))
				{
					yield return addInfo;
				}
			}

			yield return new AddInfo
			{
				Key = nameof(hawb.TotalWeightCOL),
				Value = hawb.TotalWeightCOL.ToString(decimalFormat, CultureInfo.InvariantCulture)
			};

			yield return new AddInfo
			{
				Key = nameof(hawb.ValuationCOL),
				Value = hawb.ValuationCOL.ToString(decimalFormat, CultureInfo.InvariantCulture)
			};

			yield return new AddInfo
			{
				Key = nameof(hawb.TaxesCOL),
				Value = hawb.TaxesCOL.ToString(decimalFormat, CultureInfo.InvariantCulture)
			};

			yield return new AddInfo
			{
				Key = nameof(hawb.OtherChargesDueAgentCOL),
				Value = hawb.OtherChargesDueAgentCOL.ToString(decimalFormat, CultureInfo.InvariantCulture)
			};

			yield return new AddInfo
			{
				Key = nameof(hawb.OtherChargesDueCarrierCOL),
				Value = hawb.OtherChargesDueCarrierCOL.ToString(decimalFormat, CultureInfo.InvariantCulture)
			};

			yield return new AddInfo
			{
				Key = "TotalCOL",
				Value = hawb.TotalCollect.ToString(decimalFormat, CultureInfo.InvariantCulture)
			};

			yield return new AddInfo
			{
				Key = nameof(hawb.TotalWeightPPD),
				Value = hawb.TotalWeightPPD.ToString(decimalFormat, CultureInfo.InvariantCulture)
			};

			yield return new AddInfo
			{
				Key = nameof(hawb.ValuationPPD),
				Value = hawb.ValuationPPD.ToString(decimalFormat, CultureInfo.InvariantCulture)
			};

			yield return new AddInfo
			{
				Key = nameof(hawb.TaxesPPD),
				Value = hawb.TaxesPPD.ToString(decimalFormat, CultureInfo.InvariantCulture)
			};

			yield return new AddInfo
			{
				Key = nameof(hawb.OtherChargesDueAgentPPD),
				Value = hawb.OtherChargesDueAgentPPD.ToString(decimalFormat, CultureInfo.InvariantCulture)
			};

			yield return new AddInfo
			{
				Key = nameof(hawb.OtherChargesDueCarrierPPD),
				Value = hawb.OtherChargesDueCarrierPPD.ToString(decimalFormat, CultureInfo.InvariantCulture)
			};

			yield return new AddInfo
			{
				Key = "TotalPPD",
				Value = hawb.TotalPrepaid.ToString(decimalFormat, CultureInfo.InvariantCulture)
			};
		}

		#endregion

		#region PopulateReferences

		void PopulateReferences(HouseAirwayBill hawb, UniversalShipment uxmlShipment, ICollection<AddInfo> addInfos)
		{
			uxmlShipment.WayBillNumber = hawb.AWBNumber;
			uxmlShipment.WayBillType = new WayBillType
			{
				Code = WayBillTypeList.Codes.House,
				Description = WayBillTypeList.Descriptions.House
			};

			var awbNumber = new AddInfo
			{
				Key = "MAWB",
				Value = FormattableString.Invariant($"{hawb.AirlinePrefix}-{hawb.SerialNo}") // programmatic constant
			};
			addInfos.Add(awbNumber);

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(hawb.Currency)}{nameof(hawb.Currency.Code)}"), // programmatic constant
				Value = hawb.Currency.Code
			});

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(hawb.Currency)}{nameof(hawb.Currency.Description)}"), // programmatic constant
				Value = hawb.Currency.Description
			});

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(hawb.WeightPrepaidCollect)}{nameof(hawb.WeightPrepaidCollect.Code)}"), // programmatic constant
				Value = MapPrepaidCollect(hawb.WeightPrepaidCollect.Code)
			});

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(hawb.WeightPrepaidCollect)}{nameof(hawb.WeightPrepaidCollect.Description)}"), // programmatic constant
				Value = hawb.WeightPrepaidCollect.Description
			});

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(hawb.OtherPrepaidCollect)}{nameof(hawb.OtherPrepaidCollect.Code)}"), // programmatic constant
				Value = MapPrepaidCollect(hawb.OtherPrepaidCollect.Code)
			});

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(hawb.OtherPrepaidCollect)}{nameof(hawb.OtherPrepaidCollect.Description)}"), // programmatic constant
				Value = hawb.OtherPrepaidCollect.Description
			});
		}

		#endregion

		#region PopulateSpecialHandlingCodes

		void PopulateSpecialHandlingCodes(HouseAirwayBill hawb, UniversalShipment uxmlShipment)
		{
			var specialHandlingCodes = CreateSpecialHandlingCodes(hawb).ToList();

			if (specialHandlingCodes.Count > 0)
			{
				uxmlShipment.CarrierDocumentsOverride = new CarrierDocumentsOverride
				{
					AWBHeader = new AWBHeader(writeManager.WriterStrategy)
				};

				uxmlShipment.CarrierDocumentsOverride.AWBHeader.SetSpecialHandlingCollection(() =>
					specialHandlingCodes.Count > 0 ? specialHandlingCodes : null);
			}
		}

		IEnumerable<CodeDescriptionPair> CreateSpecialHandlingCodes(HouseAirwayBill hawb)
		{
			List<CodeDescriptionPair> specialHandlingList = new List<CodeDescriptionPair>();

			foreach (var specialHandling in hawb.SpecialHandling)
			{
				if (specialHandling.CodeAndDescription.Code != ZString.Empty)
				{
					var codeDescriptionPair = new CodeDescriptionPair()
					{
						Code = specialHandling.CodeAndDescription.Code,
						Description = specialHandling.CodeAndDescription.Description
					};

					specialHandlingList.Add(codeDescriptionPair);
				}
			}

			return specialHandlingList;
		}

		#endregion

		#region PopulateSecurityStatus

		void PopulateSecurityStatus(HouseAirwayBill hawb, UniversalShipment uxmlShipment)
		{
			if (VerifyIfExistsSecurityStatusInSpecialHandlingCodes(hawb))
			{
				var securityStatus = CreateSecurityStatus(hawb);

				if (securityStatus != null)
				{
					uxmlShipment.CarrierDocumentsOverride.AWBHeader.CargoSecurityDeclaration =
						new CargoSecurityDeclaration(writeManager.WriterStrategy)
						{
							SecurityStatus = securityStatus
						};
				}
			}
		}

		CodeDescriptionPair CreateSecurityStatus(HouseAirwayBill hawb)
		{
			var securityStatus = hawb.SpecialHandling.First(item =>
				item.CodeAndDescription.Code == "NSC" || item.CodeAndDescription.Code == "SCO" ||
				item.CodeAndDescription.Code == "SPX" || item.CodeAndDescription.Code == "SHR");

			return new CodeDescriptionPair()
			{
				Code = securityStatus.CodeAndDescription.Code,
				Description = securityStatus.CodeAndDescription.Description
			};
		}

		bool VerifyIfExistsSecurityStatusInSpecialHandlingCodes(HouseAirwayBill hawb)
		{
			return hawb.SpecialHandling.Any(item =>
				item.CodeAndDescription.Code == "NSC" || item.CodeAndDescription.Code == "SCO" ||
				item.CodeAndDescription.Code == "SPX" || item.CodeAndDescription.Code == "SHR");
		}

		#endregion

		#region PopulateSignature

		void PopulateSignature(HouseAirwayBill hawb, ICollection<AddInfo> addInfos)
		{
			var shippersSignature = new AddInfo
			{
				Key = nameof(hawb.ShippersSignature),
				Value = hawb.ShippersSignature
			};
			addInfos.Add(shippersSignature);

			var issueDate = new AddInfo
			{
				Key = nameof(hawb.IssueDate),
				Value = hawb.IssueDate.ToISO8601String()
			};
			addInfos.Add(issueDate);

			var issuePlace = new AddInfo
			{
				Key = nameof(hawb.IssuePlace),
				Value = hawb.IssuePlace
			};
			addInfos.Add(issuePlace);

			var agentsSignature = new AddInfo
			{
				Key = nameof(hawb.AgentsSignature),
				Value = hawb.AgentsSignature
			};
			addInfos.Add(agentsSignature);
		}

		#endregion

		#region PopulateAddresses

		void PopulateAddresses(HouseAirwayBill hawb, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = CreateAddresses(hawb).ToList();
				return addresses.Count > 0 ? addresses : null;
			});
		}

		IEnumerable<OrganizationAddress> CreateAddresses(HouseAirwayBill hawb)
		{
			if (!hawb.Shipper.IsEmpty())
			{
				yield return hawb.Shipper.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorDocumentaryAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(hawb.Shipper.RegistrationNumbers));
			}

			if (!hawb.Consignee.IsEmpty())
			{
				yield return hawb.Consignee.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(hawb.Consignee.RegistrationNumbers));
			}

			if (!hawb.ExportAgent.IsEmpty())
			{
				yield return hawb.ExportAgent.ToUXmlOrganizationAddress(nameof(DocAddressType.SendingForwarderAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(hawb.ExportAgent.RegistrationNumbers));
			}

			if (!hawb.SendingParty.IsEmpty())
			{
				yield return hawb.SendingParty.ToUXmlOrganizationAddress(nameof(DocAddressType.BookingPartyDocumentaryAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(hawb.SendingParty.RegistrationNumbers));
			}
		}

		IEnumerable<UniversalDataBuss.DataObjects.Universal.RegistrationNumber> CreateRegistrationNumbers(IReadOnlyCollection<IRegistrationNumber> registrationNumbers)
		{
			if (registrationNumbers != null)
			{
				foreach (var number in registrationNumbers.Where(x => x != null && !x.Value.IsEmpty))
				{
					yield return number.ToUXmlRegistrationNumber();
				}
			}
		}

		#endregion

		#region PopulateRateLines

		void PopulateRateLines(HouseAirwayBill hawb, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetPackingLineCollection(() =>
			{
				var packingLines = new List<PackingLine>();

				var i = 0;
				foreach (var rateLine in hawb.RateLines)
				{
					i++;

					if (rateLine.GrossWeight == null || rateLine.GrossWeight.Value <= ZDecimal.Zero || rateLine.ChargeableWeight == null || rateLine.ChargeableWeight?.Value <= ZDecimal.Zero)
					{
						continue;
					}

					var packingLine = new PackingLine(writeManager.WriterStrategy)
					{
						PackQty = new ZLong(rateLine.NoOfPieces),
						Link = i,
						GoodsDescription = rateLine.NatureAndQtyOfGoods
					};

					rateLine.Total = rateLine.Total;

					if (rateLine.GrossWeight != null)
					{
						packingLine.Weight = rateLine.GrossWeight.Value;
						packingLine.WeightUnit = GetUnitOfWeight(rateLine.GrossWeight.Unit?.Code);
					}

					packingLine.SetAddInfoCollection(() =>
					{
						var addInfos = new List<AddInfo>(CreateAddInfosFromRateLine(rateLine));

						if (rateLine.ChargeableWeight != null)
						{
							var weightType = Constants.Weight.IsImperial(rateLine.ChargeableWeight.Unit.Code) ? Constants.Weight.Pounds : Constants.Weight.Kilograms;

							addInfos.Add(new AddInfo
							{
								Key = FormattableString.Invariant($"{nameof(rateLine.ChargeableWeight)}{nameof(rateLine.ChargeableWeight.Value)}"), // programmatic constant
								Value = rateLine.ChargeableWeight.Value.ToString(decimalFormat, CultureInfo.InvariantCulture)
							});

							addInfos.Add(new AddInfo
							{
								Key = FormattableString.Invariant($"{nameof(rateLine.ChargeableWeight)}{nameof(rateLine.ChargeableWeight.Unit)}{nameof(rateLine.ChargeableWeight.Unit.Code)}"), // programmatic constant
								Value = weightType
							});

							addInfos.Add(new AddInfo
							{
								Key = FormattableString.Invariant($"{nameof(rateLine.ChargeableWeight)}{nameof(rateLine.ChargeableWeight.Unit)}{nameof(rateLine.ChargeableWeight.Unit.Description)}"), // programmatic constant
								Value = nameof(weightType)
							});
						}
						return addInfos;
					});

					packingLines.Add(packingLine);
				}

				if (packingLines.Count > 0)
				{
					return new DataObjectList<PackingLine>(packingLines);
				}
				return null;
			});
		}

		IEnumerable<AddInfo> CreateAddInfosFromRateLine(HouseAirwayBillRateLine rateLine)
		{
			yield return new AddInfo
			{
				Key = nameof(rateLine.RateClass),
				Value = rateLine.RateClass.ToString()
			};

			yield return new AddInfo
			{
				Key = nameof(rateLine.CommodityItemNumber),
				Value = rateLine.CommodityItemNumber.ToString()
			};

			yield return new AddInfo
			{
				Key = nameof(rateLine.RateChargeOrDiscount),
				Value = rateLine.RateChargeOrDiscount.ToString()
			};

			yield return new AddInfo
			{
				Key = nameof(rateLine.Total),
				Value = rateLine.Total.ToString()
			};
		}

		#endregion

		#region Implementation

		string MapPrepaidCollect(string prepaidCollectCode)
		{
			switch (prepaidCollectCode)
			{
				case Constants.AWB.PPDCollect.Prepaid:
					return Constants.DomesticPaymentTerms.Prepaid;

				case Constants.AWB.PPDCollect.Collect:
					return Constants.DomesticPaymentTerms.Collect;

				default:
					return prepaidCollectCode;
			}
		}

		string MapWeightUnit(string unitOfWeight)
		{
			switch (unitOfWeight)
			{
				case Constants.AWB.RateLineUQ.Kilos:
					return Constants.Weight.Kilograms;

				case Constants.AWB.RateLineUQ.Pounds:
					return Constants.Weight.Pounds;

				default:
					return unitOfWeight;
			}
		}

		UnitOfWeight GetUnitOfWeight(string unitOfWeight)
		{
			var mappedUnitOfWeight = MapWeightUnit(unitOfWeight);

			return new UnitOfWeight
			{
				Code = mappedUnitOfWeight,
				Description = Constants.Weight.GetDescription(mappedUnitOfWeight, Constants.PluralState.NonPlural)
			};
		}

		#endregion
	}
}
