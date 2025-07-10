using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.NX201_01;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	public class NX201_01MessageBuilder : BaseTWMessageBuilder<INX201_01Declaration, Declaration>
	{
		public override Declaration PopulateDeclaration(INX201_01Declaration input, string functionCode = null)
		{
			var declaration = new Declaration();

			if (input != null)
			{
				declaration.FunctionalReferenceId = new DeclarationFunctionalReferenceId { Value = input.FunctionalReferenceID };
				declaration.FunctionCode = new DeclarationFunctionCode { Value = input.FunctionCode };
				PopulateValueIfNodeValueIsNotEmpty(input.ID, () => declaration.Id = new DeclarationId { Value = input.ID });
				PopulateValueIfNodeValueIsNotEmpty(input.IssueDateTime, () => declaration.IssueDateTime = input.IssueDateTime.ToISO8601String());
				PopulateAdditionalDocument(declaration, input.AdditionalDocument);
				PopulateAdditionalInformation(declaration, input.AdditionalInformation);
				PopulateGoodsShipment(declaration, input.GoodsShipment);
				PopulateAdditionalDeclaration(declaration, input.AdditionalDeclarationID);
				PopulateApplication(declaration, input.Application);
			}

			return declaration;
		}

		#region PopulateApplication

		void PopulateApplication(Declaration declaration, IApplication application)
		{
			if (application != null)
			{
				var declarationApplication = new DeclarationTwApplication
				{
					TwTypeCode = new DeclarationTwApplicationTwTypeCode { Value = application.TypeCode },
					ContactOffice = new DeclarationTwApplicationContactOffice { Id = new DeclarationTwApplicationContactOfficeId { Value = application.ContactOffice } },
				};
				PopulateApplicationAdditionalDocuments(declarationApplication, application.AdditionalDocuments);
				PopulateApplicationAgent(declarationApplication, application.Agent);
				PopulateApplicationApplicant(declarationApplication, application.Applicant);
				declaration.TwApplication = declarationApplication;
			}
		}

		void PopulateApplicationApplicant(DeclarationTwApplication declarationApplication, IPartyDetails applicant)
		{
			if (applicant != null)
			{
				var applicationApplicant = new DeclarationTwApplicationTwApplicant
				{
					TwChineseName = new DeclarationTwApplicationTwApplicantTwChineseName { Value = applicant.ChineseName },
					TwId = new DeclarationTwApplicationTwApplicantTwId { Value = applicant.ID },
					TwTypeCode = new DeclarationTwApplicationTwApplicantTwTypeCode { Value = applicant.TypeCode },
					Contact = new DeclarationTwApplicationTwApplicantContact { Name = new DeclarationTwApplicationTwApplicantContactName { Value = applicant.ContactName } },
				};

				PopulateValueIfNodeValueIsNotEmpty(applicant.Name, () => applicationApplicant.TwName = new DeclarationTwApplicationTwApplicantTwName { Value = applicant.Name });
				PopulateValueIfNodeValueIsNotEmpty(applicant.OwnerName, () => applicationApplicant.TwOwnerName = new DeclarationTwApplicationTwApplicantTwOwnerName { Value = applicant.OwnerName });

				if (applicant.Address is IAddress address)
				{
					var applicantAddress = new DeclarationTwApplicationTwApplicantAddress { TwChineseLine = new DeclarationTwApplicationTwApplicantAddressTwChineseLine { Value = address.ChineseLine } };
					PopulateValueIfNodeValueIsNotEmpty(address.Line, () => applicantAddress.Line = new DeclarationTwApplicationTwApplicantAddressLine { Value = address.Line });
					applicationApplicant.Address = applicantAddress;
				}

				if (applicant.Communications is IEnumerable<ICommunication> communications)
				{
					var applicantCommunications = new Collection<DeclarationTwApplicationTwApplicantCommunication>();
					foreach (var communication in communications)
					{
						applicantCommunications.Add(new DeclarationTwApplicationTwApplicantCommunication
						{
							Id = new DeclarationTwApplicationTwApplicantCommunicationId { Value = communication.ID },
							TypeId = new DeclarationTwApplicationTwApplicantCommunicationTypeId { Value = communication.TypeID },
						});
					}
					applicationApplicant.Communication = applicantCommunications;
				}

				declarationApplication.TwApplicant = applicationApplicant;
			}
		}

		void PopulateApplicationAgent(DeclarationTwApplication declarationApplication, IPartyDetails agent)
		{
			if (agent != null)
			{
				var applicationAgent = new DeclarationTwApplicationAgent
				{
					Id = new DeclarationTwApplicationAgentId { Value = agent.ID },
					Name = new DeclarationTwApplicationAgentName { Value = agent.Name },
					TwTypeCode = new DeclarationTwApplicationAgentTwTypeCode { Value = agent.TypeCode },
					Contact = new DeclarationTwApplicationAgentContact { Name = new DeclarationTwApplicationAgentContactName { Value = agent.ContactName } },
				};

				if (agent.Address is IAddress address)
				{
					applicationAgent.Address = new DeclarationTwApplicationAgentAddress { TwChineseLine = new DeclarationTwApplicationAgentAddressTwChineseLine { Value = address.ChineseLine } };
				}

				var communication = agent.Communications?.FirstOrDefault();
				if (communication != null)
				{
					applicationAgent.Communication = new DeclarationTwApplicationAgentCommunication
					{
						Id = new DeclarationTwApplicationAgentCommunicationId { Value = communication.ID },
						TypeId = new DeclarationTwApplicationAgentCommunicationTypeId { Value = communication.TypeID },
					};
				}

				declarationApplication.Agent = applicationAgent;
			}
		}

		void PopulateApplicationAdditionalDocuments(DeclarationTwApplication declarationApplication, IEnumerable<IAdditionalDocument> additionalDocuments)
		{
			if (additionalDocuments != null)
			{
				var additionalDocumentCollection = new Collection<DeclarationTwApplicationAdditionalDocument>();
				foreach (var additionalDocument in additionalDocuments)
				{
					var document = new DeclarationTwApplicationAdditionalDocument
					{
						TwSequenceNumeric = additionalDocument.SequenceNumeric,
						TypeCode = new DeclarationTwApplicationAdditionalDocumentTypeCode { Value = additionalDocument.TypeCode },
					};
					PopulateValueIfNodeValueIsNotEmpty(additionalDocument.ID, () => document.Id = new DeclarationTwApplicationAdditionalDocumentId { Value = additionalDocument.ID });
					PopulateValueIfNodeValueIsNotEmpty(additionalDocument.ImageFileFormat, () => document.TwImageFileFormat = new DeclarationTwApplicationAdditionalDocumentTwImageFileFormat { Value = additionalDocument.ImageFileFormat });
					PopulateValueIfNodeValueIsNotEmpty(additionalDocument.ImageFileName, () => document.TwImageFileName = new DeclarationTwApplicationAdditionalDocumentTwImageFileName { Value = additionalDocument.ImageFileName });
					additionalDocumentCollection.Add(document);
				}
				declarationApplication.AdditionalDocument = additionalDocumentCollection;
			}
		}

		#endregion

		void PopulateAdditionalDeclaration(Declaration declaration, ZString additionalDeclarationId)
		{
			if (!additionalDeclarationId.IsEmpty)
			{
				declaration.TwAdditionalDeclaration = new DeclarationTwAdditionalDeclaration { TwId = new DeclarationTwAdditionalDeclarationTwId { Value = additionalDeclarationId } };
			}
		}

		#region PopulateGoodsShipment

		void PopulateGoodsShipment(Declaration declaration, IGoodsShipment goodsShipment)
		{
			if (goodsShipment != null)
			{
				var declarationGoodsShipment = new DeclarationGoodsShipment();
				PopulateGoodsShipmentBuyer(declarationGoodsShipment, goodsShipment.Buyer);
				PopulateGoodsShipmentConsignee(declarationGoodsShipment, goodsShipment.Consignee);
				PopulateGoodsShipmentConsignment(declarationGoodsShipment, goodsShipment.Consignment);
				PopulateGoodsShipmentGovernmentAgencyGoodsItems(declaration, declarationGoodsShipment, goodsShipment.GovernmentAgencyGoodsItems);
				PopulateGoodsShipmentSeller(declarationGoodsShipment, goodsShipment.Seller);
				declaration.GoodsShipment = declarationGoodsShipment;
			}
		}

		void PopulateGoodsShipmentSeller(DeclarationGoodsShipment declarationGoodsShipment, IPartyDetails seller)
		{
			if (seller != null)
			{
				var declarationGoodsShipmentSeller = new DeclarationGoodsShipmentSeller { Name = new DeclarationGoodsShipmentSellerName { Value = seller.Name } };
				if (seller.Address is IAddress address)
				{
					declarationGoodsShipmentSeller.Address = new DeclarationGoodsShipmentSellerAddress
					{
						CountryCode = new DeclarationGoodsShipmentSellerAddressCountryCode { Value = address.CountryCode },
						Line = new DeclarationGoodsShipmentSellerAddressLine { Value = address.Line },
					};
				}
				declarationGoodsShipment.Seller = declarationGoodsShipmentSeller;
			}
		}

		const string ActionCode = "52";

		void PopulateGoodsShipmentGovernmentAgencyGoodsItems(Declaration declaration, DeclarationGoodsShipment declarationGoodsShipment, IEnumerable<IGovernmentAgencyGoodsItem> governmentAgencyGoodsItems)
		{
			if (governmentAgencyGoodsItems != null)
			{
				var governmentAgencyGoodsItemCollection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>();

				foreach (var governmentAgencyGoodsItem in governmentAgencyGoodsItems)
				{
					var item = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();

					if (declaration.FunctionCode.Value == ActionCode)
					{
						item.SequenceNumeric = governmentAgencyGoodsItem.SequenceNumeric;
					}
					item.AdditionalDocument = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument { TwSequenceNumeric = governmentAgencyGoodsItem.AdditionalDocuments?.FirstOrDefault()?.SequenceNumeric ?? ZInt.Zero };
					PopulateGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation(item, governmentAgencyGoodsItem.AdditionalInformations);
					PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity(item, governmentAgencyGoodsItem.Commodity);

					item.GoodsMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure
					{
						TariffQuantity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureTariffQuantity { Value = governmentAgencyGoodsItem.GoodsMeasure?.TariffQuantity ?? ZDecimal.Zero },
						TwUnitCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureTwUnitCode { Value = governmentAgencyGoodsItem.GoodsMeasure?.UnitCode ?? ZString.Empty },
					};

					var originCountryCode = governmentAgencyGoodsItem.Origin?.CountryCode ?? ZString.Empty;
					PopulateValueIfNodeValueIsNotEmpty(originCountryCode, () => item.Origin = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin
					{
						CountryCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginCountryCode { Value = originCountryCode }
					});

					governmentAgencyGoodsItemCollection.Add(item);
				}

				declarationGoodsShipment.GovernmentAgencyGoodsItem = governmentAgencyGoodsItemCollection;
			}
		}

		void PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity(DeclarationGoodsShipmentGovernmentAgencyGoodsItem item, ICommodity commodity)
		{
			if (commodity != null)
			{
				var itemCommodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();

				PopulateValueIfNodeValueIsNotEmpty(commodity.CommercialCategorizationID, () => itemCommodity.CommercialCategorizationId = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommercialCategorizationId
				{
					Value = commodity.CommercialCategorizationID
				});

				itemCommodity.Description = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDescription { Value = commodity.Description };

				PopulateValueIfNodeValueIsNotEmpty(commodity.Name, () => itemCommodity.Name = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityName { Value = commodity.Name });

				itemCommodity.Classification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification
				{
					Id = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassificationId { Value = commodity.Classification?.ID ?? ZString.Empty }
				};

				var elementDescription = commodity.Constituent?.ElementDescription ?? ZString.Empty;
				PopulateValueIfNodeValueIsNotEmpty(elementDescription, () => itemCommodity.Constituent = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituent
				{
					ElementDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentElementDescription
					{
						Value = elementDescription
					}
				});

				PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine(itemCommodity, commodity.InvoiceLine);

				item.Commodity = itemCommodity;
			}
		}

		void PopulateDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity itemCommodity, IInvoiceLine invoiceLine)
		{
			if (invoiceLine != null)
			{
				var itemCommodityInvoiceLine = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine
				{
					TwChargesTypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLineTwChargesTypeCode { Value = invoiceLine.ChargesTypeCode },
					TwCurrencyTypeCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLineTwCurrencyTypeCode { Value = invoiceLine.CurrencyTypeCode },
					TwSubTotalAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLineTwSubTotalAmount { Value = invoiceLine.SubTotalAmount },
				};

				PopulateValueIfNodeValueIsNotEmpty(invoiceLine.UnitPriceAmount, () =>
				{
					itemCommodityInvoiceLine.TwUnitPriceAmount = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLineTwUnitPriceAmount { Value = invoiceLine.UnitPriceAmount };
				});

				itemCommodity.InvoiceLine = itemCommodityInvoiceLine;
			}
		}

		void PopulateGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation(DeclarationGoodsShipmentGovernmentAgencyGoodsItem item, IEnumerable<IAdditionalInformation> additionalInformations)
		{
			if (additionalInformations != null)
			{
				var additionalInformationCollection = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation>();

				foreach (var additionalDocument in additionalInformations)
				{
					additionalInformationCollection.Add(new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation
					{
						StatementCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformationStatementCode { Value = additionalDocument.StatementCode },
						StatementDescription = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformationStatementDescription { Value = additionalDocument.StatementDescription }
					});
				}

				item.AdditionalInformation = additionalInformationCollection;
			}
		}

		void PopulateGoodsShipmentConsignment(DeclarationGoodsShipment declarationGoodsShipment, IConsignment consignment)
		{
			if (consignment != null)
			{
				var declarationGoodsShipmentConsignment = new DeclarationGoodsShipmentConsignment();

				if (consignment.AdditionalInformations is IEnumerable<IAdditionalInformation> additionalInformations)
				{
					var additionalInformationCollection = new Collection<DeclarationGoodsShipmentConsignmentAdditionalInformation>();
					foreach (var additionalInformation in additionalInformations)
					{
						additionalInformationCollection.Add(new DeclarationGoodsShipmentConsignmentAdditionalInformation
						{
							StatementCode = new DeclarationGoodsShipmentConsignmentAdditionalInformationStatementCode { Value = additionalInformation.StatementCode },
							StatementDescription = new DeclarationGoodsShipmentConsignmentAdditionalInformationStatementDescription { Value = additionalInformation.StatementDescription }
						});
					}
					declarationGoodsShipmentConsignment.AdditionalInformation = additionalInformationCollection;
				}

				PopulateValueIfNodeValueIsNotEmpty(consignment.LoadingLocation?.ID ?? ZString.Empty, () => declarationGoodsShipmentConsignment.LoadingLocation = new DeclarationGoodsShipmentConsignmentLoadingLocation { Id = new DeclarationGoodsShipmentConsignmentLoadingLocationId { Value = consignment.LoadingLocation.ID } });
				PopulateValueIfNodeValueIsNotEmpty(consignment.TranshipmentLocation?.ID ?? ZString.Empty, () => declarationGoodsShipmentConsignment.TranshipmentLocation = new DeclarationGoodsShipmentConsignmentTranshipmentLocation { Id = new DeclarationGoodsShipmentConsignmentTranshipmentLocationId { Value = consignment.TranshipmentLocation.ID } });
				PopulateValueIfNodeValueIsNotEmpty(consignment.TransitDeparture?.ID ?? ZString.Empty, () => declarationGoodsShipmentConsignment.TransitDeparture = new DeclarationGoodsShipmentConsignmentTransitDeparture { Id = new DeclarationGoodsShipmentConsignmentTransitDepartureId { Value = consignment.TransitDeparture.ID } });
				PopulateValueIfNodeValueIsNotEmpty(consignment.UnloadingLocation?.ID ?? ZString.Empty, () => declarationGoodsShipmentConsignment.UnloadingLocation = new DeclarationGoodsShipmentConsignmentUnloadingLocation { Id = new DeclarationGoodsShipmentConsignmentUnloadingLocationId { Value = consignment.UnloadingLocation.ID } });

				declarationGoodsShipment.Consignment = declarationGoodsShipmentConsignment;
			}
		}

		void PopulateGoodsShipmentConsignee(DeclarationGoodsShipment declarationGoodsShipment, IPartyDetails consignee)
		{
			if (consignee != null)
			{
				var declarationGoodsShipmentConsignee = new DeclarationGoodsShipmentConsignee { Name = new DeclarationGoodsShipmentConsigneeName { Value = consignee.Name } };
				if (consignee.Address is IAddress address)
				{
					declarationGoodsShipmentConsignee.Address = new DeclarationGoodsShipmentConsigneeAddress { CountryCode = new DeclarationGoodsShipmentConsigneeAddressCountryCode { Value = address.CountryCode } };
					PopulateValueIfNodeValueIsNotEmpty(address.Line, () => declarationGoodsShipmentConsignee.Address.Line = new DeclarationGoodsShipmentConsigneeAddressLine { Value = address.Line });
				}
				declarationGoodsShipment.Consignee = declarationGoodsShipmentConsignee;
			}
		}

		void PopulateGoodsShipmentBuyer(DeclarationGoodsShipment declarationGoodsShipment, IPartyDetails buyer)
		{
			if (buyer != null)
			{
				var declarationGoodsShipmentBuyer = new DeclarationGoodsShipmentBuyer { Name = new DeclarationGoodsShipmentBuyerName { Value = buyer.Name } };
				if (buyer.Address is IAddress address)
				{
					declarationGoodsShipmentBuyer.Address = new DeclarationGoodsShipmentBuyerAddress { CountryCode = new DeclarationGoodsShipmentBuyerAddressCountryCode { Value = address.CountryCode } };
					PopulateValueIfNodeValueIsNotEmpty(address.Line, () => declarationGoodsShipmentBuyer.Address.Line = new DeclarationGoodsShipmentBuyerAddressLine { Value = address.Line });
				}
				declarationGoodsShipment.Buyer = declarationGoodsShipmentBuyer;
			}
		}

		#endregion

		void PopulateAdditionalInformation(Declaration declaration, IAdditionalInformation additionalInformation)
		{
			if (additionalInformation != null)
			{
				var declarationAdditionalInformation = new DeclarationAdditionalInformation();
				PopulateValueIfNodeValueIsNotEmpty(additionalInformation.Content, () => declarationAdditionalInformation.Content = new DeclarationAdditionalInformationContent { Value = additionalInformation.Content });
				PopulateValueIfNodeValueIsNotEmpty(additionalInformation.DelProcessNumber, () => declarationAdditionalInformation.TwDelProcessNumber = new DeclarationAdditionalInformationTwDelProcessNumber { Value = additionalInformation.DelProcessNumber });
				PopulateValueIfNodeValueIsNotEmpty(additionalInformation.ProcessNumber, () => declarationAdditionalInformation.TwProcessNumber = new DeclarationAdditionalInformationTwProcessNumber { Value = additionalInformation.ProcessNumber });
				declaration.AdditionalInformation = declarationAdditionalInformation;
			}
		}

		void PopulateAdditionalDocument(Declaration declaration, IDeclarationAdditionalDocument additionalDocument)
		{
			if (additionalDocument != null && !additionalDocument.ID.IsEmpty)
			{
				declaration.AdditionalDocument = new DeclarationAdditionalDocument { Id = new DeclarationAdditionalDocumentId { Value = additionalDocument.ID } };
			}
		}
	}
}
