using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.StatusNotification, AMSEDIMessage.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.StatusNotification, US.Messaging.Business.MessageBuildingBlocks.Constants.ACE)]
	class StatusNotificationProcessor : AMSProcessor
	{
		protected override void ProcessCore()
		{
			var emailBody = new ZStringBuilder();
			var issuerCode = ZString.Empty;
			HtmlTableCreator billReferenceHTMLCreator = null;
			HtmlTableCreator containerHTMLCreator = null;
			HtmlTableCreator containerDetailHTMLCreator = null;
			OUTR02 lastOUTR02 = null;
			var carrierCode = ZString.Empty;
			var manifestSequenceNumber = ZString.Empty;
			var isNextOUTR02AContinuationType = false;
			var billOfLadingNumber = ZString.Empty;
			var vesselName = ZString.Empty;
			var voyageNumber = ZString.Empty;
			var modeOfTransport = ZString.Empty;
			var vesselCountry = ZString.Empty;
			var portOfUnlading = ZString.Empty;
			var referenceNumberQualifier = ZString.Empty;
			var referenceNumber = ZString.Empty;
			var estimatedDate = ZDate.Empty;
			var dispositionList = new List<Tuple<ZString, ZString, ZDateTime>>();
			var eventCode = ZString.Empty;
			var eventDate = ZDateTime.Empty;
			var inBondNumber = ZString.Empty;

			foreach (var block in messageBlocks)
			{
				var inpm01 = block as IINPM01;
				if (inpm01 != null)
				{
					carrierCode = inpm01.CarrierCode;
					manifestSequenceNumber = inpm01.ManifestSequenceNumber;
					vesselName = inpm01.VesselName;
					voyageNumber = inpm01.VoyageNumber;
					modeOfTransport = inpm01.ModeOfTransportation;
					vesselCountry = inpm01.VesselCountry;
				}
				else
				{
					var amsContainer = block as IAMSContainer;
					if (amsContainer != null)
					{
						if (containerHTMLCreator == null)
						{
							containerHTMLCreator = new HtmlTableCreator(GetContainerColumnTitle());
						}
						containerHTMLCreator.WriteRow(amsContainer.ContainerNumber, amsContainer.SealNumber1, amsContainer.SealNumber2);
					}
					else
					{
						if (containerHTMLCreator != null)
						{
							emailBody.Append(containerHTMLCreator.ToHtml());
							emailBody.Append("<br>");
							containerHTMLCreator = null;
						}
						var amsContainerDetail = block as IAMSContainerDetail;
						if (amsContainerDetail != null)
						{
							if (containerDetailHTMLCreator == null)
							{
								containerDetailHTMLCreator = new HtmlTableCreator(GetContainerDetailColumnTitle());
							}
							containerDetailHTMLCreator.WriteRow(amsContainerDetail.VIN, amsContainerDetail.ContainerOperatorLine, amsContainerDetail.ForeignPort, amsContainerDetail.FactoryCarOrderNumber);
						}
						else
						{
							if (containerDetailHTMLCreator != null)
							{
								emailBody.Append(containerDetailHTMLCreator.ToHtml());
								emailBody.Append("<br>");
								containerDetailHTMLCreator = null;
							}

							var outr04 = block as IOUTR04;
							if (outr04 != null)
							{
								AddOUTR04HtmlData(emailBody, outr04);
							}
							else
							{
								var outr03 = block as IOUTR03;
								if (outr03 != null)
								{
									AddOUTR03HtmlData(emailBody, outr03);
								}
								else
								{
									var inpb04 = block as IINPB04;
									if (inpb04 != null)
									{
										if (billReferenceHTMLCreator == null)
										{
											billReferenceHTMLCreator = new HtmlTableCreator(GetOUTB04ColumnTitle());
										}
										billReferenceHTMLCreator.WriteRow(ReferenceList.GetDescriptionFromCode(inpb04.ReferenceIdentifierQualifier), inpb04.ReferenceIdentifier);
									}
									else
									{
										if (billReferenceHTMLCreator != null)
										{
											emailBody.Append(billReferenceHTMLCreator.ToHtml());
											emailBody.Append("<br>");
											billReferenceHTMLCreator = null;
										}

										var outr02Combine = block as IOUTR02Combine;
										if (outr02Combine != null)
										{
											if (isNextOUTR02AContinuationType)
											{
												if (lastOUTR02 != null)
												{
													var outR02Continuation = new OUTR02Continuation();
													outR02Continuation.Deserialise(block.Serialise());
													referenceNumberQualifier = outR02Continuation.ReferenceNumberQualifier;
													referenceNumber = outR02Continuation.ReferenceNumber;
													emailBody.Append(CreateBillOfLadingDetailHtmlTable(issuerCode, lastOUTR02, outR02Continuation).ToHtml());
													emailBody.Append("<br>");
													lastOUTR02 = null;
												}
												isNextOUTR02AContinuationType = false;
											}
											else
											{
												if (lastOUTR02 != null)
												{
													emailBody.Append(CreateBillOfLadingDetailHtmlTable(issuerCode, lastOUTR02, null).ToHtml());
													emailBody.Append("<br>");
													lastOUTR02 = null;
												}

												var outr02 = new OUTR02();
												outr02.Deserialise(block.Serialise());
												billOfLadingNumber = outr02.BillOfLadingNumber;
												inBondNumber = outr02.EntryNumber;
												var dispositionDate = DateTimeParser.GetDateTimeFromZDateAndStringTime(outr02.ActionDate, outr02.ActionTime);
												var dispositionCode = outr02.DispositionCode;
												dispositionList.Add(new Tuple<ZString, ZString, ZDateTime>(billOfLadingNumber, dispositionCode, dispositionDate));

												isNextOUTR02AContinuationType = outr02.LineDelimiter == 1;
												if (isNextOUTR02AContinuationType)
												{
													lastOUTR02 = outr02;
												}
												else
												{
													emailBody.Append(CreateBillOfLadingDetailHtmlTable(issuerCode, outr02, null).ToHtml());
													emailBody.Append("<br>");
												}
											}
										}
										else
										{
											isNextOUTR02AContinuationType = false;
											if (lastOUTR02 != null)
											{
												emailBody.Append(CreateBillOfLadingDetailHtmlTable(issuerCode, lastOUTR02, null).ToHtml());
												emailBody.Append("<br>");
												lastOUTR02 = null;
											}

											var outr06 = block as IOUTR06;
											if (outr06 != null)
											{
												AddOUTR06HtmlData(emailBody, outr06);
												eventCode = outr06.EventCode;
												eventDate = DateTimeParser.GetDateTimeFromZDateAndStringTime(outr06.ActionDate, outr06.ActionTime);
											}
											else
											{
												var inpj01 = block as IINPJ01;
												if (inpj01 != null)
												{
													issuerCode = inpj01.IssuerCode;
												}
												else
												{
													var outr01 = block as IOUTR01;
													if (outr01 != null)
													{
														if (carrierCode != outr01.CarrierCode || vesselName != outr01.VesselName || voyageNumber != outr01.VoyageNumber)
														{
															carrierCode = outr01.CarrierCode;
															vesselName = outr01.VesselName;
															voyageNumber = outr01.VoyageNumber;
															vesselCountry = ZString.Empty;
															modeOfTransport = ZString.Empty;
														}
														portOfUnlading = outr01.CBPDistrictPort;
														if (manifestSequenceNumber.IsEmpty)
														{
															manifestSequenceNumber = outr01.ManifestSequenceNumber;
														}
														if (estimatedDate.IsEmpty)
														{
															estimatedDate = outr01.EstimatedDate;
														}
														emailBody.Append(CreateManifestHeaderDetailHtmlTable(carrierCode, portOfUnlading, vesselName, voyageNumber, manifestSequenceNumber, outr01.EstimatedDate, modeOfTransport, vesselCountry).ToHtml());
														emailBody.Append("<br>");
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

			if (lastOUTR02 != null)
			{
				emailBody.Append(CreateBillOfLadingDetailHtmlTable(issuerCode, lastOUTR02, null).ToHtml());
				emailBody.Append("<br>");
				lastOUTR02 = null;
			}

			if (billReferenceHTMLCreator != null)
			{
				emailBody.Append(billReferenceHTMLCreator.ToHtml());
				emailBody.Append("<br>");
				billReferenceHTMLCreator = null;
			}

			if (containerHTMLCreator != null)
			{
				emailBody.Append(containerHTMLCreator.ToHtml());
				emailBody.Append("<br>");
				containerHTMLCreator = null;
			}

			var url = "";
			var jobNumber = "";
			Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>> movementMatch = x =>
			{
				var messageAttachee = x.Item2;
				return (messageAttachee.ApplicationCode == SubApplicationCodeList.Codes.AMS &&
					(
						messageAttachee.SupApplicationCode == SubApplicationCodeList.Codes.AMS ||
						messageAttachee.SupApplicationCode == SubApplicationCodeList.Codes.PermitToTransfer ||
						((messageAttachee.SupApplicationCode == SubApplicationCodeList.Codes.MasterInBond || messageAttachee.SupApplicationCode == SubApplicationCodeList.Codes.SubsequentInBond) &&
							messageAttachee.InBondNumber == x.Item3)
					) ||
					(messageAttachee.ApplicationCode == SubApplicationCodeList.Codes.MasterInBond && messageAttachee.InBondNumber == x.Item3));
			};

			var moveHeader = LinkToMessageAttacheeAndUpdateMessageSubType(carrierCode,
																		manifestSequenceNumber,
																		vesselName,
																		voyageNumber,
																		portOfUnlading,
																		estimatedDate,
																		issuerCode,
																		billOfLadingNumber,
																		referenceNumberQualifier,
																		referenceNumber,
																		inBondNumber,
																		movementMatch);
			MasterFiles.Integration.IGlbBranch branch = null;
			if (moveHeader != null)
			{
				jobNumber = moveHeader.JobNumber;
				if (!string.IsNullOrEmpty(jobNumber))
				{
					url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(moveHeader);
					if (!billOfLadingNumber.IsEmpty)
					{
						jobNumber += " - " + billOfLadingNumber;
						moveHeader.LinkMessageToBill(issuerCode, billOfLadingNumber, Message);
					}
				}
				branch = moveHeader.Branch;
				if (dispositionList.Count > 0)
				{
					foreach (var disposition in dispositionList)
					{
						moveHeader.UpdateDispositionInformation(issuerCode, disposition.Item1, disposition.Item2, disposition.Item3);
					}
				}
				if (!eventCode.IsEmpty)
				{
					moveHeader.UpdatConveyanceEventInformation(eventCode, eventDate);
				}
			}
			else
			{
				jobNumber = billOfLadingNumber.IsEmpty ? string.Format("{0} / {1} / {2} / {3}", vesselName, voyageNumber, portOfUnlading, estimatedDate) : issuerCode + billOfLadingNumber;
			}
			var messageTypeInSubject = "Unknown";
			if (A != null)
			{
				messageTypeInSubject = ApplicationIdentifierCodeList.GetDescriptionFromCode(A.ApplicationIdentifier).Replace("Response", "").Trim();
			}
			GenerateHtmlEmailAndSendToOriginalOrGroup(url, jobNumber, messageTypeInSubject, emailBody.ToString(), false, branch, moveHeader as BusinessObject);
		}

		void AddOUTR06HtmlData(ZStringBuilder emailBody, IOUTR06 outr06)
		{
			var outr06Creator = new HtmlTableCreator(new[] { new CellWithFormatting("Conveyance Event", new NameValueCollection { { "colspan", "2" } }, true) });
			outr06Creator.WriteRow("Event Code", GetCodeAndDescription(outr06.EventCode, Factory.GetCachedValue<ConveyanceEventCodeList>()));
			outr06Creator.WriteRow("Action Date", outr06.ActionDate);
			outr06Creator.WriteRow("Action Time", outr06.ActionTime);
			emailBody.Append(outr06Creator.ToHtml());
			emailBody.Append("<br>");
		}

		ZString GetCodeAndDescription(ZString code, ICodeDescriptionPairList list)
		{
			var result = new ZStringBuilder(code);
			result.AppendIfNotEmpty(list.GetDescriptionFromCode(code));
			return result.ToStringWithDelimiterBetweenAppends(" = ");
		}

		void AddOUTR03HtmlData(ZStringBuilder emailBody, IOUTR03 outr03)
		{
			var outr03Creator = new HtmlTableCreator(new string[] { "Remarks" });
			outr03Creator.WriteRow(outr03.Remarks);

			emailBody.Append(outr03Creator.ToHtml());
			emailBody.Append("<br>");
		}

		protected HtmlTableCreator CreateManifestHeaderDetailHtmlTable(ZString carrierCode, ZString cbpDistrictPort, ZString vesselName, ZString voyageNumber, ZString manifestSequenceNumber, ZDate estimatedDate, ZString modeOfTransport, ZString vesselCountry)
		{
			var creator = new HtmlTableCreator(new[] { new CellWithFormatting("Manifest Details", new NameValueCollection { { "colspan", "2" } }, true) });
			creator.WriteRow("Carrier Code", carrierCode);
			creator.WriteRow("CBP District/Port", cbpDistrictPort);
			creator.WriteRow("Mode of Transportation", modeOfTransport);
			creator.WriteRow("Vessel Country", vesselCountry);
			creator.WriteRow("Vessel Name", vesselName);
			creator.WriteRow("Voyage Number", voyageNumber);
			creator.WriteRow("Manifest Sequence Number", manifestSequenceNumber);
			creator.WriteRow("Estimated Date", estimatedDate);
			return creator;
		}

		string[] GetContainerColumnTitle()
		{
			return new string[] {
				"Container Number",
				"Seal Number 1",
				"Seal Number 2"
			};
		}

		string[] GetContainerDetailColumnTitle()
		{
			return new string[] {
				"VIN",
				"Container Operator Line",
				"Foreign Port",
				"Factory Car Order Number"
			};
		}

		void AddOUTR04HtmlData(ZStringBuilder emailBody, IOUTR04 outr04)
		{
			var outr04Creator = new HtmlTableCreator(new[] { new CellWithFormatting("Non-AMS Paperless Releases", new NameValueCollection { { "colspan", "2" } }, true) });
			outr04Creator.WriteRow("DDPP", outr04.DDPP);
			outr04Creator.WriteRow("Filer/Entry Number", outr04.FilerEntryNumber);
			outr04Creator.WriteRow("Carrier Code", outr04.CarrierCode);
			outr04Creator.WriteRow("Vessel Name", outr04.VesselName);
			outr04Creator.WriteRow("Voyage/Flight Number", outr04.VoyageFlightNumber);
			outr04Creator.WriteRow("Bill of Lading Number", outr04.BillOfLadingNumber);
			outr04Creator.WriteRow("Entered Quantity", outr04.EnteredQuantity);
			emailBody.Append(outr04Creator.ToHtml());
			emailBody.Append("<br>");
		}

		string[] GetOUTB04ColumnTitle()
		{
			return new string[] {
				"Reference Type",
				"Reference Value"
			};
		}

		BillReferenceList ReferenceList
		{
			get { return referenceList ?? (referenceList = Factory.GetCachedValue<BillReferenceList>()); }
		}
		BillReferenceList referenceList;

		HtmlTableCreator CreateBillOfLadingDetailHtmlTable(ZString issuerCode, OUTR02 lastOUTR02Main, OUTR02Continuation outR02Continuation)
		{
			var outr02Creator = new HtmlTableCreator(new[] { new CellWithFormatting("Bill of Lading Status Notification", new NameValueCollection { { "colspan", "2" } }, true) });
			outr02Creator.WriteRow("Issuer Code", issuerCode);
			outr02Creator.WriteRow("Bill of Lading Number", lastOUTR02Main.BillOfLadingNumber);
			outr02Creator.WriteRow("Disposition Code", GetCodeAndDescription(lastOUTR02Main.DispositionCode, DispositionCodeListLoader.GetDispositionCodes(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode)));
			outr02Creator.WriteRow("Quantity", lastOUTR02Main.Quantity);
			outr02Creator.WriteRow("Entry Type", lastOUTR02Main.EntryType);
			outr02Creator.WriteRow("Entry Number", lastOUTR02Main.EntryNumber);
			outr02Creator.WriteRow("Action Date", lastOUTR02Main.ActionDate);
			outr02Creator.WriteRow("Action Time", lastOUTR02Main.ActionTime);
			outr02Creator.WriteRow("Negative Indicator", lastOUTR02Main.NegativeIndicator);
			outr02Creator.WriteRow("Resend Indicator", lastOUTR02Main.ResendIndicator);
			if (outR02Continuation != null)
			{
				outr02Creator.WriteRow("District/Port of Transaction", outR02Continuation.DistrictPortOfTransaction);
				outr02Creator.WriteRow("FIRMS Code", outR02Continuation.FIRMSCode);
				outr02Creator.WriteRow("U.S. Port of Destination/ Intermediate Destination", outR02Continuation.USPortOfDestinationIntermediateDestination);
				outr02Creator.WriteRow("Foreign Destination", outR02Continuation.ForeignDestination);
				outr02Creator.WriteRow("Container Number", outR02Continuation.ContainerNumber);
				outr02Creator.WriteRow("Reference Number Qualifier", outR02Continuation.ReferenceNumberQualifier);
				outr02Creator.WriteRow("Reference Number", outR02Continuation.ReferenceNumber);
			}

			return outr02Creator;
		}

		protected override ProcessingResult<LinkedBusinessObjectMetaData> TryFindLinkedObject(EDIMessage message)
		{
			var linkTableName = ZString.Empty;
			var linkPK = ZGuid.Empty;
			var jobNumber = ZString.Empty;
			var branchPK = message.EM_GB;
			if (QueryOriginMessage(message) is CBPEDIMessage originalMessage)
			{
				branchPK = originalMessage.EM_GB;
			}
			var issuerCode = ZString.Empty;
			var carrierCode = ZString.Empty;
			var manifestSequenceNumber = ZString.Empty;
			var billOfLadingNumber = ZString.Empty;
			var vesselName = ZString.Empty;
			var voyageNumber = ZString.Empty;
			var portOfUnlading = ZString.Empty;
			var referenceNumberQualifier = ZString.Empty;
			var referenceNumber = ZString.Empty;
			var estimatedDate = ZDate.Empty;
			var inBondNumber = ZString.Empty;

			var amsMessage = message as AMSEDIMessage;

			foreach (var block in amsMessage.MessageBlock.MessageBlocks)
			{
				if (block is IINPM01 inpm01)
				{
					carrierCode = inpm01.CarrierCode;
					manifestSequenceNumber = inpm01.ManifestSequenceNumber;
					vesselName = inpm01.VesselName;
					voyageNumber = inpm01.VoyageNumber;
				}
				else if (block is OUTR02 outr02)
				{
					billOfLadingNumber = outr02.BillOfLadingNumber;
					inBondNumber = outr02.EntryNumber;
				}
				else if (block is OUTR02Continuation outR02Continuation)
				{
					referenceNumberQualifier = outR02Continuation.ReferenceNumberQualifier;
					referenceNumber = outR02Continuation.ReferenceNumber;
				}
				else if (block is IINPJ01 inpj01)
				{
					issuerCode = inpj01.IssuerCode;
				}
				else if (block is IOUTR01 outr01)
				{
					if (carrierCode != outr01.CarrierCode || vesselName != outr01.VesselName || voyageNumber != outr01.VoyageNumber)
					{
						carrierCode = outr01.CarrierCode;
						vesselName = outr01.VesselName;
						voyageNumber = outr01.VoyageNumber;
					}
					portOfUnlading = outr01.CBPDistrictPort;
					if (manifestSequenceNumber.IsEmpty)
					{
						manifestSequenceNumber = outr01.ManifestSequenceNumber;
					}
					if (estimatedDate.IsEmpty)
					{
						estimatedDate = outr01.EstimatedDate;
					}
				}
			}
			Predicate<Tuple<IBaseBillOfLading, IManifestMessageAttachee, ZString>> movementMatch = x =>
			{
				var messageAttachee = x.Item2;
				return (messageAttachee.ApplicationCode == SubApplicationCodeList.Codes.AMS &&
					(
						messageAttachee.SupApplicationCode == SubApplicationCodeList.Codes.AMS ||
						messageAttachee.SupApplicationCode == SubApplicationCodeList.Codes.PermitToTransfer ||
						((messageAttachee.SupApplicationCode == SubApplicationCodeList.Codes.MasterInBond || messageAttachee.SupApplicationCode == SubApplicationCodeList.Codes.SubsequentInBond) &&
							messageAttachee.InBondNumber == x.Item3)
					) ||
					(messageAttachee.ApplicationCode == SubApplicationCodeList.Codes.MasterInBond && messageAttachee.InBondNumber == x.Item3));
			};
			var linker = (IManifestMessageAttacheeMessageLinker)ObjectFactory.Get("ManifestMessageAttacheeMessageLinker");
			var messageAttachee = linker.MatchManifest((AMSEDIMessage)message, carrierCode, manifestSequenceNumber, vesselName, voyageNumber, portOfUnlading, estimatedDate, issuerCode, billOfLadingNumber, referenceNumberQualifier, referenceNumber, inBondNumber, movementMatch, new AMSBillMatchingComparer());
			if (messageAttachee != null)
			{
				jobNumber = messageAttachee.JobNumber;
				var linkedObject = (BusinessObject)messageAttachee;
				linkTableName = linkedObject.TableName;
				linkPK = linkedObject.PK;
				if (messageAttachee.Branch is GlbBranch branch && branch.PK.IsValid)
				{
					branchPK = branch.PK;
				}
			}

			return ProcessingResult.New(new LinkedBusinessObjectMetaData(linkTableName, linkPK, branchPK, jobNumber), (NoResString)string.Empty);
		}

		AMSApplicationIdentifierCodeList ApplicationIdentifierCodeList
		{
			get { return Factory.GetCachedValue<AMSApplicationIdentifierCodeList>(); }
		}
	}
}
