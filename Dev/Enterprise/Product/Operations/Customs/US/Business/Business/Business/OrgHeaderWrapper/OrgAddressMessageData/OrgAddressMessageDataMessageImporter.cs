using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class OrgAddressMessageDataMessageImporter
	{
		public OrgAddressMessageDataMessageImporter(MQEDIMessage message, OrgAddressMessageData messageData)
		{
			this.message = message;
			this.messageData = messageData;
		}
		readonly MQEDIMessage message;
		readonly OrgAddressMessageData messageData;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void Populate()
		{
			PersonIdentityInformationData currentPIIData = null;
			RelatedBusinessData currentRelatedBusinessData = null;

			ZBool piiNamePopulateFromMessage = false;
			ZBool piiEmailPopulateFromMessage = false;
			ZBool individualNamePopulateFromMessage = false;
			ZBool brokerNamePopulateFromMessage = false;

			foreach (var block in message.MessageBlock.MessageBlocks)
			{
				var t1Block = block as ADDT1;
				if (t1Block != null)
				{
					PopulateFieldsFromT1Block(t1Block);
				}
				else
				{
					var taBlock = block as ADDTA;
					if (taBlock != null)
					{
						PopulateFieldsFromTABlock(taBlock);
					}
					else
					{
						var t3Block = block as AICCT3;
						if (t3Block != null)
						{
							PopulateFieldsFromT3Block(t3Block);
						}
						else
						{
							var tbBlock = block as ADDTB;
							if (tbBlock != null)
							{
								PopulateFieldsFromTBBlock(tbBlock);
							}
							else
							{
								var tdBlock = block as AICCTD;
								if (tdBlock != null)
								{
									PopulateFieldsFromTDBlock(tdBlock);
								}
								else
								{
									var teBlock = block as AICCTE;
									if (teBlock != null)
									{
										PopulateFieldsFromTEBlock(teBlock);
									}
									else
									{
										var tfBlock = block as AICCTF;
										if (tfBlock != null)
										{
											PopulateFieldsFromTFBlock(tfBlock);
										}
										else
										{
											var tgBlock = block as AICCTG;
											if (tgBlock != null)
											{
												PopulateFieldsFromTGBlock(tgBlock);
											}
											else
											{
												var thBlock = block as AICCTH;
												if (thBlock != null)
												{
													PopulateFieldsFromTHBlock(thBlock);
												}
												else
												{
													var tiBlock = block as AICCTI;
													if (tiBlock != null)
													{
														currentPIIData = messageData.PIIs.AddNew();
														var contactName = tiBlock.Name;
														currentPIIData.US_OC_Contact = GetMatchedContactPKByName(contactName);
														PopulateValueIfEmpty(currentPIIData, PersonIdentityInformationData.Schema.US_Name, contactName, () => { piiNamePopulateFromMessage = true; });
														PopulateValueIfEmpty(currentPIIData, PersonIdentityInformationData.Schema.US_Title, tiBlock.Title);

														if (!tiBlock.SSN.IsEmpty)
														{
															currentPIIData.US_SSN = tiBlock.SSN.Left(3) + "-" + tiBlock.SSN.SubstringSafe(3, 2) + "-" + tiBlock.SSN.SubstringSafe(5);
														}
													}
													else
													{
														var tjBlock = block as AICCTJ;
														if (tjBlock != null)
														{
															if (currentPIIData != null)
															{
																PopulateValueIfEmpty(currentPIIData, PersonIdentityInformationData.Schema.US_PassportNo, tjBlock.Passport);
																PopulateValueIfEmpty(currentPIIData, PersonIdentityInformationData.Schema.US_ExpirationDate, tjBlock.ExpirationDate);
																PopulateValueIfEmpty(currentPIIData, PersonIdentityInformationData.Schema.US_CountryOfIssuance, tjBlock.CountryOfIssuance);
																PopulateValueIfEmpty(currentPIIData, PersonIdentityInformationData.Schema.US_PassportType, tjBlock.PassportType);
																PopulateValueIfEmpty(currentPIIData, PersonIdentityInformationData.Schema.US_PhoneNumber, tjBlock.Phone);
																PopulateValueIfEmpty(currentPIIData, PersonIdentityInformationData.Schema.US_Extension, tjBlock.Extension);
																PopulateValueIfEmpty(currentPIIData, PersonIdentityInformationData.Schema.US_Email, tjBlock.Email, () => { piiEmailPopulateFromMessage = true; });
															}
														}
														else
														{
															var tkBlock = block as AICCTK;
															if (tkBlock != null)
															{
																currentRelatedBusinessData = messageData.RelatedBusinessItems.AddNew();
																currentRelatedBusinessData.US_RelatedBusiness = tkBlock.RelatedBusiness;
																currentRelatedBusinessData.US_NameOfEntity = tkBlock.NameOfTheEntity;
																currentRelatedBusinessData.US_Number = tkBlock.TINEINSSNCBPAssigned;
															}
															else
															{
																var tlBlock = block as AICCTL;
																if (tlBlock != null)
																{
																	var certifyIndividualName = tlBlock.CertifyingIndividualFullName;
																	messageData.US_OC_CertifyIndividual = GetMatchedContactPKByName(certifyIndividualName);
																	PopulateValueIfEmpty(messageData, OrgAddressMessageData.Schema.US_IndividualName, certifyIndividualName, () => { individualNamePopulateFromMessage = true; });
																	PopulateValueIfEmpty(messageData, OrgAddressMessageData.Schema.US_IndividualTitle, tlBlock.Title);
																}
																else
																{
																	var tmBlock = block as AICCTM;
																	if (tmBlock != null)
																	{
																		var brokerName = tmBlock.BrokersName;
																		messageData.US_GS_Broker = GetMatchedBrokerByName(brokerName);
																		PopulateValueIfEmpty(messageData, OrgAddressMessageData.Schema.US_BrokerName, brokerName, () => { brokerNamePopulateFromMessage = true; });
																		PopulateValueIfEmpty(messageData, OrgAddressMessageData.Schema.US_BrokerPhone, tmBlock.BrokersPhone);
																		PopulateValueIfEmpty(messageData, OrgAddressMessageData.Schema.US_IndividualPhone, tmBlock.CertifyingIndividualsPhone);
																	}
																	else
																	{
																		var tnBlock = block as AICCTN;
																		if (tnBlock != null)
																		{
																			switch (tnBlock.AdditionalInformationQualifierCode)
																			{
																				case ImporterAdditionalInformationQualifierCodeList.Codes.IN1:
																					AppendValueToFieldIfNotEmpty(messageData, OrgAddressMessageData.Schema.US_ImporterName, true, tnBlock.AdditionalInformation);
																					break;
																				case ImporterAdditionalInformationQualifierCodeList.Codes.CE1:
																					AppendValueToFieldIfNotEmpty(messageData, OrgAddressMessageData.Schema.US_ImporterEmail, true, tnBlock.AdditionalInformation);
																					break;
																				case ImporterAdditionalInformationQualifierCodeList.Codes.CW1:
																					AppendValueToFieldIfNotEmpty(messageData, OrgAddressMessageData.Schema.US_ImporterWebsite, true, tnBlock.AdditionalInformation);
																					break;
																				case ImporterAdditionalInformationQualifierCodeList.Codes.CN1:
																					AppendValueToFieldIfNotEmpty(currentPIIData, PersonIdentityInformationData.Schema.US_Name, piiNamePopulateFromMessage, tnBlock.AdditionalInformation);
																					var contactPersonIdentityPK = GetMatchedContactPKByName(currentPIIData[PersonIdentityInformationData.Schema.US_Name].ToString());
																					if (!contactPersonIdentityPK.IsEmpty)
																					{
																						currentPIIData.US_OC_Contact = contactPersonIdentityPK;
																					}
																					break;
																				case ImporterAdditionalInformationQualifierCodeList.Codes.CE2:
																					AppendValueToFieldIfNotEmpty(currentPIIData, PersonIdentityInformationData.Schema.US_Email, piiEmailPopulateFromMessage, tnBlock.AdditionalInformation);
																					break;
																				case ImporterAdditionalInformationQualifierCodeList.Codes.NE1:
																					AppendValueToFieldIfNotEmpty(currentRelatedBusinessData, RelatedBusinessData.Schema.US_NameOfEntity, true, tnBlock.AdditionalInformation);
																					break;
																				case ImporterAdditionalInformationQualifierCodeList.Codes.IN2:
																					AppendValueToFieldIfNotEmpty(messageData, OrgAddressMessageData.Schema.US_IndividualName, individualNamePopulateFromMessage, tnBlock.AdditionalInformation);
																					var contactCertifyIndividualPK = GetMatchedContactPKByName(messageData[OrgAddressMessageData.Schema.US_IndividualName].ToString());
																					if (!contactCertifyIndividualPK.IsEmpty)
																					{
																						messageData.US_OC_CertifyIndividual = GetMatchedContactPKByName(messageData[OrgAddressMessageData.Schema.US_IndividualName].ToString());
																					}
																					break;
																				case ImporterAdditionalInformationQualifierCodeList.Codes.BN2:
																					AppendValueToFieldIfNotEmpty(messageData, OrgAddressMessageData.Schema.US_BrokerName, brokerNamePopulateFromMessage, tnBlock.AdditionalInformation);
																					break;
																				case ImporterAdditionalInformationQualifierCodeList.Codes.BN1:
																					AppendValueToFieldIfNotEmpty(messageData, OrgAddressMessageData.Schema.US_BankName, true, tnBlock.AdditionalInformation);
																					break;
																				case ImporterAdditionalInformationQualifierCodeList.Codes.BC1:
																					AppendValueToFieldIfNotEmpty(messageData, OrgAddressMessageData.Schema.US_BankCity, true, tnBlock.AdditionalInformation);
																					break;
																			}
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		void PopulateFieldsFromT1Block(ADDT1 t1Block)
		{
			messageData.US_ActionCode = t1Block.UpdateActionCode;
			messageData.US_ImporterType = t1Block.ImporterType;
			messageData.US_ImporterName = t1Block.LineOneOfTheImporterName;

			var mailingAddress1 = t1Block.LineOneOfTheMailingAddress;
			if (!mailingAddress1.IsEmpty)
			{
				var matchedMailingAddressPK = GetMatchedAddressPKByName(mailingAddress1);
				if (!matchedMailingAddressPK.IsEmpty)
				{
					messageData.US_OA_Address1 = matchedMailingAddressPK;
				}
			}
		}

		void PopulateFieldsFromTABlock(ADDTA taBlock)
		{
			messageData.US_NameQualifier = taBlock.NameQualifier;
			messageData.US_AlternativeImporterName = taBlock.LineTwoOfTheImporterName;
		}

		void PopulateFieldsFromT3Block(AICCT3 t3Block)
		{
			messageData.US_ImporterName = t3Block.FullLegalImporterName;
		}

		void PopulateFieldsFromTBBlock(ADDTB tbBlock)
		{
			var physicalAddress1 = tbBlock.LineOneOfTheSecondaryAddress;
			if (!physicalAddress1.IsEmpty)
			{
				var matchedPhysicalAddressPK = GetMatchedAddressPKByName(physicalAddress1);
				if (!matchedPhysicalAddressPK.IsEmpty)
				{
					messageData.US_OA_Address2 = matchedPhysicalAddressPK;
				}
			}
		}

		void PopulateFieldsFromTDBlock(AICCTD tdBlock)
		{
			messageData.US_NumberOfEntries = tdBlock.NumberOfEntriesPlanningPerYear;
			messageData.US_UtlIORIndicator = tdBlock.IdentificationNoUtilizationImporterOfRecord == "X";
			messageData.US_UtlConsigneeIndicator = tdBlock.IdentificationNoUtilizationConsignee == "X";
			messageData.US_UtlDrawbackIndicator = tdBlock.IdentificationNoUtilizationDrawbackClaimant == "X";
			messageData.US_UtlRefundsIndicator = tdBlock.IdentificationNoUtilizationRefundsBills == "X";
			messageData.US_UtlOtherIndicator = tdBlock.IdentificationNoUtilizationOther == "X";
			messageData.US_UtlOtherDescription = tdBlock.UtilizationOfIdentificationNoDescription;
			messageData.US_ProgramCode1 = tdBlock.ProgramCode1;
			messageData.US_ProgramCode2 = tdBlock.ProgramCode2;
			messageData.US_ProgramCode3 = tdBlock.ProgramCode3;
			messageData.US_ProgramCode4 = tdBlock.ProgramCode4;
			messageData.US_ImporterPhoneNumber = tdBlock.Phone;
			messageData.US_PhoneExtension = tdBlock.Extension;
			messageData.US_HaveSSNIndicator = tdBlock.CBPAssignedNumberRequestReasonIndicator == "X";
			messageData.US_NoSSNIndicator = tdBlock.SSNIndicator == "X";
			messageData.US_NoIRSIndicator = tdBlock.IRSIndicator == "X";
			messageData.US_NotAppliedIndicator = tdBlock.IRSOrSSNIndicator == "X";
			messageData.US_NotResidentIndicator = tdBlock.USResidentIndicator == "X";
		}

		void PopulateFieldsFromTEBlock(AICCTE teBlock)
		{
			messageData.US_AddressType1 = teBlock.MailingAddressType;
			messageData.US_AddressExplanation1 = teBlock.MailingAddressExplanation;
			messageData.US_AddressType2 = teBlock.PhysicalAddressType;
			messageData.US_AddressExplanation2 = teBlock.PhysicalAddressExplanation;
			messageData.US_BusinessDescription = teBlock.BusinessDescription;
		}

		void PopulateFieldsFromTFBlock(AICCTF tfBlock)
		{
			messageData.US_ImporterEmail = tfBlock.Email;
			messageData.US_ImporterWebsite = tfBlock.Website;
			messageData.US_ImporterFaxNumber = tfBlock.Fax;
		}

		void PopulateFieldsFromTGBlock(AICCTG tgBlock)
		{
			messageData.US_NAICSCode = tgBlock.NAICSCode;
			messageData.US_DUNS = tgBlock.DUNS;
			messageData.US_FilerCode = tgBlock.FilerCode;
			messageData.US_YearEstablished = tgBlock.YearEstablished;
			messageData.US_StateCode = tgBlock.State;
			messageData.US_CountryISOCode = tgBlock.CountryISOCode;
			messageData.US_CertificateReference = tgBlock.Reference;
		}

		void PopulateFieldsFromTHBlock(AICCTH thBlock)
		{
			messageData.US_BankName = thBlock.PrimaryBank;
			messageData.US_BankRoutingNo = thBlock.Routing;
			messageData.US_BankCity = thBlock.City;
			messageData.US_BankState = thBlock.State;
			messageData.US_BankCountry = thBlock.CountryISOCode;
		}

		ZGuid GetMatchedAddressPKByName(ZString addressName)
		{
			var addressQuery = new ZQuery();
			addressQuery.AddToFilter(OrgAddressSchema.OA_Address1, SQLComparisonOperator.StartsWith, addressName);
			addressQuery.AddToFilter(OrgAddressSchema.OA_IsActive, true);

			var matchedMailingAddresses = messageData.wrapper.organisation.Addresses.Find(addressQuery);
			return matchedMailingAddresses != null && matchedMailingAddresses.Length == 1 ? matchedMailingAddresses[0].PK : ZGuid.Empty;
		}

		ZGuid GetMatchedContactPKByName(ZString contactName)
		{
			var result = ZGuid.Empty;
			var names = contactName.Split(',').Select(s => s.Trim()).ToArray();
			if (names.Length > 1)
			{
				var contactQuery = new ZQuery();
				contactQuery.AddToFilter(OrgContactSchema.OC_ContactName, SQLComparisonOperator.Contains, names[0]);
				contactQuery.AddToFilter(OrgContactSchema.OC_ContactName, SQLComparisonOperator.StartsWith, names[1]);
				contactQuery.AddToFilter(OrgContactSchema.OC_IsActive, true);

				var matchedContacts = messageData.wrapper.organisation.Contacts.Find(contactQuery);
				if (matchedContacts != null && matchedContacts.Length == 1)
				{
					result = matchedContacts[0].PK;
				}
			}
			return result;
		}

		ZString GetMatchedBrokerByName(ZString brokerFullName)
		{
			var brokerQuery = new ZQuery();
			brokerQuery.AddToFilter(GlbStaffSchema.GS_FullName, SQLComparisonOperator.StartsWith, brokerFullName);
			brokerQuery.AddToFilter(GlbStaffSchema.GS_IsActive, true);

			var matchedBrokers = messageData.Factory.Load<GlbStaff>(brokerQuery);
			return matchedBrokers != null && matchedBrokers.Length == 1 ? matchedBrokers[0].GS_Code : ZString.Empty;
		}

		void PopulateValueIfEmpty(BusinessObject bizObj, string propertyName, IZType valueToSet, Action fieldPopulateFromMessageAction = null)
		{
			if (bizObj != null && !valueToSet.IsEmpty)
			{
				var existingValue = (IZType)bizObj[propertyName];
				if (existingValue.IsEmpty)
				{
					bizObj[propertyName] = valueToSet;
					fieldPopulateFromMessageAction?.Invoke();
				}
			}
		}

		void AppendValueToFieldIfNotEmpty(BusinessObject bizObj, string propertyName, ZBool shouldAppend, ZString valueToAppend)
		{
			if (bizObj != null && !valueToAppend.IsEmpty)
			{
				var existingValue = (ZString)bizObj[propertyName];
				if (!existingValue.IsEmpty && shouldAppend)
				{
					bizObj[propertyName] = existingValue.PadRight(30) + valueToAppend;
				}
			}
		}
	}
}
