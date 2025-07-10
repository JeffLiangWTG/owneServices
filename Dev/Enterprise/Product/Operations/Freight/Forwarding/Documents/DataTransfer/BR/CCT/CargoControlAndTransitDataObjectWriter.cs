using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.AWB;
using Enterprise.UniversalDataBuss.Integration;
using PackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.BR
{
	sealed class CargoControlAndTransitDataObjectWriter : DataObjectWriter<CargoControlAndTransit, UniversalShipment>
	{
		public CargoControlAndTransitDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		const string decimalFormat = "0.000";
		const string totalPPD = "TotalPPD";
		const string totalCOL = "TotalCOL";
		const string mawb = "MAWB";

		protected override UniversalShipment PopulateDataObject(CargoControlAndTransit cct)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = cct.CreateUXmlDataContext();
			shipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = new DataObjectList<AdditionalReference>(CreateAdditionalReferences(cct));
				return additionalReferences.Any() ? additionalReferences : null;
			});
			shipment.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();

				PopulateAddresses(cct, shipment);
				PopulateGoodsDescription(cct, shipment);
				PopulateLocations(cct, shipment, addInfos);
				PopulateSpecialHandlingCodes(cct, shipment);
				PopulateSecurityStatus(cct, shipment);
				PopulateSpecialServiceRequestAndOtherServiceInformation(cct, shipment);
				PopulateReferences(cct, shipment, addInfos);
				PopulateValues(cct, shipment, addInfos);
				PopulateTotals(cct, shipment, addInfos);
				PopulateSignature(cct, addInfos);
				PopulateRateLines(cct, shipment);
				PopulateCustomsWarehouse(cct, addInfos);
				PopulateWoodenParts(cct, addInfos);

				if (addInfos.Count > 0)
				{
					return addInfos;
				}
				return shipment.AddInfoCollection;
			});

			return shipment;
		}

		void PopulateGoodsDescription(CargoControlAndTransit cct, UniversalShipment shipment)
		{
			if (cct.RateLines != null && cct.RateLines.Count > 0)
			{
				var natureAndQtyOfGoodsList = cct.RateLines.Where(item => item.NatureAndQtyOfGoods != ZString.Empty)
					.Select(item => item.NatureAndQtyOfGoods).ToArray();
				shipment.GoodsDescription = ZString.Join(" ", natureAndQtyOfGoodsList);
			}
		}

		#region PopulateSecurityStatus

		void PopulateSecurityStatus(CargoControlAndTransit cct, UniversalShipment shipment)
		{
			if (VerifyIfExistsSecurityStatusInSpecialHandlingCodes(cct))
			{
				var securityStatus = CreateSecurityStatus(cct);

				if (securityStatus != null)
				{
					shipment.CarrierDocumentsOverride.AWBHeader.CargoSecurityDeclaration =
						new CargoSecurityDeclaration(writeManager.WriterStrategy)
						{
							SecurityStatus = securityStatus
						};
				}
			}
		}

		CodeDescriptionPair CreateSecurityStatus(CargoControlAndTransit cct)
		{
			var securityStatus = cct.SpecialHandling.First(item =>
				item.CodeAndDescription.Code == "NSC" || item.CodeAndDescription.Code == "SCO" ||
				item.CodeAndDescription.Code == "SPX" || item.CodeAndDescription.Code == "SHR");

			return new CodeDescriptionPair()
			{
				Code = securityStatus.CodeAndDescription.Code,
				Description = securityStatus.CodeAndDescription.Description
			};
		}

		bool VerifyIfExistsSecurityStatusInSpecialHandlingCodes(CargoControlAndTransit cct)
		{
			return cct.SpecialHandling.Any(item =>
				item.CodeAndDescription.Code == "NSC" || item.CodeAndDescription.Code == "SCO" ||
				item.CodeAndDescription.Code == "SPX" || item.CodeAndDescription.Code == "SHR");
		}

		#endregion

		#region PopulateSpecialHandlingCodes

		void PopulateSpecialHandlingCodes(CargoControlAndTransit cct, UniversalShipment shipment)
		{
			var specialHandlingCodes = CreateSpecialHandlingCodes(cct).ToList();

			if (specialHandlingCodes.Count > 0)
			{
				shipment.CarrierDocumentsOverride = new CarrierDocumentsOverride
				{
					AWBHeader = new AWBHeader(writeManager.WriterStrategy)
				};

				shipment.CarrierDocumentsOverride.AWBHeader.SetSpecialHandlingCollection(() =>
					specialHandlingCodes.Count > 0 ? specialHandlingCodes : null);
			}
		}

		IEnumerable<CodeDescriptionPair> CreateSpecialHandlingCodes(CargoControlAndTransit cct)
		{
			List<CodeDescriptionPair> specialHandlingList = new List<CodeDescriptionPair>();

			foreach (var specialHandling in cct.SpecialHandling)
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

		#region PopulateSpecialServiceRequestAndOtherServiceInformation

		void PopulateSpecialServiceRequestAndOtherServiceInformation(CargoControlAndTransit cct, UniversalShipment shipment)
		{
			if (!cct.SpecialServiceRequest.IsEmpty || !cct.OtherServiceInformation.IsEmpty)
			{
				if (shipment.CarrierDocumentsOverride == null)
				{
					shipment.CarrierDocumentsOverride = new CarrierDocumentsOverride
					{
						AWBHeader = new AWBHeader(writeManager.WriterStrategy)
					};
				}

				if (!cct.SpecialServiceRequest.IsEmpty)
				{
					shipment.CarrierDocumentsOverride.AWBHeader.SpecialServiceRequest = cct.SpecialServiceRequest;
				}
				if (!cct.OtherServiceInformation.IsEmpty)
				{
					shipment.CarrierDocumentsOverride.AWBHeader.OtherServiceInformation = cct.OtherServiceInformation;
				}
			}
		}

		#endregion

		#region PopulateAddresses

		void PopulateAddresses(CargoControlAndTransit cct, UniversalShipment shipment)
		{
			shipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = CreateAddresses(cct).ToList();
				return addresses.Count > 0 ? addresses : null;
			});
		}

		IEnumerable<OrganizationAddress> CreateAddresses(CargoControlAndTransit cct)
		{
			if (!cct.Shipper.IsEmpty())
			{
				yield return cct.Shipper.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorDocumentaryAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(cct.Shipper.RegistrationNumbers));
			}

			if (!cct.Consignee.IsEmpty())
			{
				yield return cct.Consignee.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(cct.Consignee.RegistrationNumbers));
			}

			if (!cct.ImportAgent.IsEmpty())
			{
				yield return cct.ImportAgent.ToUXmlOrganizationAddress(nameof(DocAddressType.ReceivingForwarderAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(cct.ImportAgent.RegistrationNumbers));
			}

			if (!cct.Issuer.IsEmpty())
			{
				yield return cct.Issuer.ToUXmlOrganizationAddress(nameof(DocAddressType.BookingPartyDocumentaryAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(cct.Issuer.RegistrationNumbers));
			}

			if (!cct.ExportAgent.IsEmpty())
			{
				yield return cct.ExportAgent.ToUXmlOrganizationAddress(nameof(DocAddressType.SendingForwarderAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(cct.ExportAgent.RegistrationNumbers));
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

		#region PopulateReferences

		void PopulateReferences(CargoControlAndTransit cct, UniversalShipment shipment, ICollection<AddInfo> addInfos)
		{
			shipment.WayBillType = new WayBillType
			{
				Code = WayBillTypeList.Codes.House,
				Description = WayBillTypeList.Descriptions.House
			};

			shipment.WayBillNumber = cct.AWBNumber;

			var awbNumber = new AddInfo
			{
				Key = mawb,
				Value = FormattableString.Invariant($"{cct.AirlinePrefix}-{cct.SerialNo}") // programmatic constant
			};
			addInfos.Add(awbNumber);

			var referenceNumber = new AddInfo
			{
				Key = nameof(cct.ReferenceNumber),
				Value = cct.ReferenceNumber
			};
			addInfos.Add(referenceNumber);

			var optionalShippingInformation = new AddInfo
			{
				Key = nameof(cct.OptionalShippingInformation),
				Value = cct.OptionalShippingInformation
			};
			addInfos.Add(optionalShippingInformation);

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(cct.Charges)}{nameof(cct.Charges.Code)}"), // programmatic constant
				Value = MapPrepaidCollect(cct.Charges.Code)
			});

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(cct.Charges)}{nameof(cct.Charges.Description)}"), // programmatic constant
				Value = cct.Charges.Description
			});

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(cct.WeightPrepaidCollect)}{nameof(cct.WeightPrepaidCollect.Code)}"), // programmatic constant
				Value = MapPrepaidCollect(cct.WeightPrepaidCollect.Code)
			});

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(cct.WeightPrepaidCollect)}{nameof(cct.WeightPrepaidCollect.Description)}"), // programmatic constant
				Value = cct.WeightPrepaidCollect.Description
			});

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(cct.OtherPrepaidCollect)}{nameof(cct.OtherPrepaidCollect.Code)}"), // programmatic constant
				Value = MapPrepaidCollect(cct.OtherPrepaidCollect.Code)
			});

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(cct.OtherPrepaidCollect)}{nameof(cct.OtherPrepaidCollect.Description)}"), // programmatic constant
				Value = cct.OtherPrepaidCollect.Description
			});

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(cct.Currency)}{nameof(cct.Currency.Code)}"), // programmatic constant
				Value = cct.Currency.Code
			});

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(cct.Currency)}{nameof(cct.Currency.Description)}"), // programmatic constant
				Value = cct.Currency.Description
			});
		}

		#endregion

		#region PopulateLocations

		void PopulateLocations(CargoControlAndTransit cct, UniversalShipment shipment, ICollection<AddInfo> addInfos)
		{
			if (cct.AirportOfDeparture != null)
			{
				shipment.PortOfOrigin = new UNLOCO
				{
					Code = cct.AirportOfDeparture.Code,
					Name = cct.AirportOfDeparture.Description
				};
			}

			if (cct.AirportOfDestination != null)
			{
				shipment.PortOfDestination = new UNLOCO
				{
					Code = cct.AirportOfDestination.Code,
					Name = cct.AirportOfDestination.Description
				};
			}

			if (cct.PortOfFirstArrival != null)
			{
				shipment.PortOfFirstArrival = cct.PortOfFirstArrival.ToUXmlUnloco();

				addInfos.Add(cct.PortOfFirstArrival.ToUXmlAddInfos("OperationalPort").First());
			}

			if (cct.To1st != null)
			{
				addInfos.Add(new AddInfo
				{
					Key = FormattableString.Invariant($"{nameof(cct.To1st)}{nameof(cct.To1st.Code)}"), // programmatic constant
					Value = cct.To1st.Code
				});

				addInfos.Add(new AddInfo
				{
					Key = FormattableString.Invariant($"{nameof(cct.To1st)}{nameof(cct.To1st.Description)}"), // programmatic constant
					Value = cct.To1st.Description
				});
			}
		}

		#endregion

		#region PopulateValues

		void PopulateValues(CargoControlAndTransit cct, UniversalShipment shipment, ICollection<AddInfo> addInfos)
		{
			shipment.GoodsValue = cct.CarriageValue.Amount;
			shipment.GoodsValueCurrency = new Currency
			{
				Code = cct.CarriageValue.Currency.Code,
				Description = cct.CarriageValue.Currency.Description
			};

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(cct.CustomsValue)}{nameof(cct.CustomsValue.Amount)}"), // programmatic constant
				Value = cct.CustomsValue.Amount.ToString(decimalFormat, CultureInfo.InvariantCulture)
			});

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(cct.CustomsValue)}{nameof(cct.CustomsValue.Currency)}{nameof(cct.CustomsValue.Currency.Code)}"), // programmatic constant
				Value = cct.CustomsValue.Currency?.Code
			});

			addInfos.Add(new AddInfo
			{
				Key = FormattableString.Invariant($"{nameof(cct.CustomsValue)}{nameof(cct.CustomsValue.Currency)}{nameof(cct.CustomsValue.Currency.Description)}"), // programmatic constant
				Value = cct.CustomsValue.Currency?.Description
			});

			shipment.InsuranceValue = cct.InsuranceValue.Amount;
			shipment.InsuranceValueCurrency = new Currency
			{
				Code = cct.InsuranceValue.Currency.Code,
				Description = cct.InsuranceValue.Currency.Description
			};
		}

		#endregion

		#region PopulateTotals

		void PopulateTotals(CargoControlAndTransit cct, UniversalShipment shipment, ICollection<AddInfo> addInfos)
		{
			shipment.TotalNoOfPacks = cct.RateLines.Sum(rl => rl.NoOfPieces);

			shipment.TotalWeight = cct.RateLines.Sum(rl =>
			{
				if (!rl.GrossWeight?.Unit?.Code.IsEmpty ?? false)
				{
					return Core.Constants.Weight.Convert(
						rl.GrossWeight.Value,
						MapWeightUnit(rl.GrossWeight.Unit.Code),
						Core.Constants.Weight.Kilograms);
				}

				return decimal.Zero;
			});

			shipment.TotalWeightUnit = GetUnitOfWeight(Core.Constants.Weight.Kilograms);

			foreach (var totalCharge in CreateTotalCharges(cct))
			{
				addInfos.Add(totalCharge);
			}
		}

		IEnumerable<AddInfo> CreateTotalCharges(CargoControlAndTransit cct)
		{
			if (cct.TotalWeightCOL > 0)
			{
				yield return new AddInfo
				{
					Key = nameof(cct.TotalWeightCOL),
					Value = cct.TotalWeightCOL.ToString(decimalFormat, CultureInfo.InvariantCulture)
				};

				yield return new AddInfo
				{
					Key = nameof(cct.ValuationCOL),
					Value = cct.ValuationCOL.ToString(decimalFormat, CultureInfo.InvariantCulture)
				};

				yield return new AddInfo
				{
					Key = nameof(cct.TaxesCOL),
					Value = cct.TaxesCOL.ToString(decimalFormat, CultureInfo.InvariantCulture)
				};

				yield return new AddInfo
				{
					Key = nameof(cct.OtherChargesDueAgentCOL),
					Value = cct.OtherChargesDueAgentCOL.ToString(decimalFormat, CultureInfo.InvariantCulture)
				};

				yield return new AddInfo
				{
					Key = nameof(cct.OtherChargesDueCarrierCOL),
					Value = cct.OtherChargesDueCarrierCOL.ToString(decimalFormat, CultureInfo.InvariantCulture)
				};

				yield return new AddInfo
				{
					Key = totalCOL,
					Value = cct.TotalCollect.ToString(decimalFormat, CultureInfo.InvariantCulture)
				};
			}

			if (cct.TotalWeightPPD > 0)
			{
				yield return new AddInfo
				{
					Key = nameof(cct.TotalWeightPPD),
					Value = cct.TotalWeightPPD.ToString(decimalFormat, CultureInfo.InvariantCulture)
				};

				yield return new AddInfo
				{
					Key = nameof(cct.ValuationPPD),
					Value = cct.ValuationPPD.ToString(decimalFormat, CultureInfo.InvariantCulture)
				};

				yield return new AddInfo
				{
					Key = nameof(cct.TaxesPPD),
					Value = cct.TaxesPPD.ToString(decimalFormat, CultureInfo.InvariantCulture)
				};

				yield return new AddInfo
				{
					Key = nameof(cct.OtherChargesDueAgentPPD),
					Value = cct.OtherChargesDueAgentPPD.ToString(decimalFormat, CultureInfo.InvariantCulture)
				};

				yield return new AddInfo
				{
					Key = nameof(cct.OtherChargesDueCarrierPPD),
					Value = cct.OtherChargesDueCarrierPPD.ToString(decimalFormat, CultureInfo.InvariantCulture)
				};

				yield return new AddInfo
				{
					Key = totalPPD,
					Value = cct.TotalPrepaid.ToString(decimalFormat, CultureInfo.InvariantCulture)
				};
			}
		}

		#endregion

		#region PopulateSignature

		void PopulateSignature(CargoControlAndTransit cct, ICollection<AddInfo> addInfos)
		{
			var shippersSignature = new AddInfo
			{
				Key = nameof(cct.ShippersSignature),
				Value = cct.ShippersSignature
			};
			addInfos.Add(shippersSignature);

			var issueDate = new AddInfo
			{
				Key = nameof(cct.IssueDate),
				Value = cct.IssueDate.ToISO8601String()
			};
			addInfos.Add(issueDate);

			var issuePlace = new AddInfo
			{
				Key = nameof(cct.IssuePlace),
				Value = cct.IssuePlace
			};
			addInfos.Add(issuePlace);

			var agentsSignature = new AddInfo
			{
				Key = nameof(cct.AgentsSignature),
				Value = cct.AgentsSignature
			};
			addInfos.Add(agentsSignature);
		}

		#endregion

		#region PopulateRateLines

		void PopulateRateLines(CargoControlAndTransit cct, UniversalShipment shipment)
		{
			shipment.SetPackingLineCollection(() =>
			{
				var packingLines = new List<PackingLine>();

				var lineNumber = 0;

				foreach (var rateLine in cct.RateLines)
				{
					lineNumber++;

					if (rateLine.GrossWeight == null || rateLine.GrossWeight.Value <= ZDecimal.Zero
						|| rateLine.ChargeableWeight == null || rateLine.ChargeableWeight?.Value <= ZDecimal.Zero)
					{
						continue;
					}

					var packingLine = new PackingLine(writeManager.WriterStrategy)
					{
						PackQty = new ZLong(rateLine.NoOfPieces),
						Link = lineNumber,
						GoodsDescription = rateLine.NatureAndQtyOfGoods
					};

					if (lineNumber == 1 && cct.RateLines.Any(r => r.IsHSCodeLine))
					{
						packingLine.SetClassificationCollection(() =>
						{
							var classifications = new DataObjectList<Classification>();
							foreach (var line in cct.RateLines.Where(r => r.IsHSCodeLine))
							{
								line.NatureAndQtyOfGoods.Replace("HS Codes: ", "").ToString().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(hsCode =>
								{
									classifications.Add(new Classification
									{
										Code = hsCode,
									});
								});
							}

							return classifications;
						});
					}

					rateLine.Total = rateLine.ChargeableWeight.Value * rateLine.RateChargeOrDiscount;

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
							var weightType = Core.Constants.Weight.IsImperial(rateLine.ChargeableWeight.Unit.Code) ? Core.Constants.Weight.Pounds : Core.Constants.Weight.Kilograms;

							var value = Constants.Weight.Convert(rateLine.ChargeableWeight.Value, weightType,
								Constants.Weight.Kilograms);

							addInfos.Add(new AddInfo
							{
								Key = FormattableString.Invariant($"{nameof(rateLine.ChargeableWeight)}{nameof(rateLine.ChargeableWeight.Value)}"), // programmatic constant
								Value = value.ToString(decimalFormat, CultureInfo.InvariantCulture)
							});

							addInfos.Add(new AddInfo
							{
								Key = FormattableString.Invariant($"{nameof(rateLine.ChargeableWeight)}{nameof(rateLine.ChargeableWeight.Unit)}{nameof(rateLine.ChargeableWeight.Unit.Code)}"), // programmatic constant
								Value = Constants.Weight.Kilograms
							});

							addInfos.Add(new AddInfo
							{
								Key = FormattableString.Invariant($"{nameof(rateLine.ChargeableWeight)}{nameof(rateLine.ChargeableWeight.Unit)}{nameof(rateLine.ChargeableWeight.Unit.Description)}"), // programmatic constant
								Value = nameof(Constants.Weight.Kilograms)
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

		IEnumerable<AddInfo> CreateAddInfosFromRateLine(CargoControlAndTransitRateLine rateLine)
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

		#region PopulateCustomsWarehouse

		void PopulateCustomsWarehouse(CargoControlAndTransit cct, ICollection<AddInfo> addInfos)
		{
			if (!cct.CustomsWarehouse.IsEmpty)
			{
				var customsWarehouse = new AddInfo
				{
					Key = nameof(cct.CustomsWarehouse),
					Value = cct.CustomsWarehouse
				};
				addInfos.Add(customsWarehouse);
			}
		}

		#endregion

		#region PopulateWoodenParts

		void PopulateWoodenParts(CargoControlAndTransit cct, ICollection<AddInfo> addInfos)
		{
			var woodenParts = new AddInfo
			{
				Key = nameof(cct.WoodenParts),
				Value = cct.WoodenParts.ToString()
			};
			addInfos.Add(woodenParts);
		}

		#endregion

		#region Implementation

		string MapPrepaidCollect(string prepaidCollectCode)
		{
			switch (prepaidCollectCode)
			{
				case Core.Constants.AWB.PPDCollect.Prepaid:
					return Core.Constants.DomesticPaymentTerms.Prepaid;

				case Core.Constants.AWB.PPDCollect.Collect:
					return Core.Constants.DomesticPaymentTerms.Collect;

				default:
					return prepaidCollectCode;
			}
		}

		string MapWeightUnit(string unitOfWeight)
		{
			switch (unitOfWeight)
			{
				case Core.Constants.AWB.RateLineUQ.Kilos:
					return Core.Constants.Weight.Kilograms;

				case Core.Constants.AWB.RateLineUQ.Pounds:
					return Core.Constants.Weight.Pounds;

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
				Description = Core.Constants.Weight.GetDescription(mappedUnitOfWeight, Constants.PluralState.NonPlural)
			};
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(CargoControlAndTransit cct)
		{
			if (!cct.RUCReferenceNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = cct.RUCReferenceNumber,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.CargoControlAndTransitRUCNumber,
						Description = DocDataConstants.AdditionalReferences.Descriptions.CargoControlAndTransitRUCNumber
					}
				};
			}
		}
		#endregion
	}
}
