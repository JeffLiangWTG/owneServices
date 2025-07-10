using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification)]
	public class BondStatusNotificationMessageProcessor : ACEABIProcessor
	{
		ZString entryNumber, entryType, principalName, dispositionCode, designationType, bondNumber, bondType;
		ZDecimal bondAmount;
		ZDateTime evenTime;
		ZDate terminateDate;
		ZString transactionIDTypeCode;
		OrgHeader[] bondedOrganisations;

		public ICodeDescriptionPairList TransactionIDTypeCodeList
		{
			get { return Factory.GetCachedValue<TransactionIDTypeCodeList>(); }
		}

		public override void Process()
		{
			if (Message.EM_MessageNum.IsEmpty)
			{
				Message.EM_MessageNum = ZDateTime.Now.Ticks.ToString();
			}

			var messageBlocksMapToHtmlTable = ProcessBlocksAndGenerateHtmlContextCollection();
			var notificationDetail = GenerateHtmlNotificationDetails(messageBlocksMapToHtmlTable);
			Message.EM_ApplicationReference = entryNumber;
			Message.EM_MessageSubType = EM_MessageSubTypeList.Codes.eBondStatusUpdate;
			ZExceptionReporting.ProcessWithSaveExceptionHandling(Message.Factory.Save, null, true); // Probably this save should not exists; having two saves is bad

			using (Message.Factory.AddDisposableService())
			{
				UniversalXmlWorkflowProcessor.PublishUniversalXMLInternally(Message.Factory, Message, GenerateEventDataObject(notificationDetail), EDIMessageSubTypeList.Codes.XmlUniversalEvent);

				ZExceptionReporting.ProcessWithSaveExceptionHandling(Message.Factory.Save, null, true);
			}
		}

		Event GenerateEventDataObject(ZString notificationDetail)
		{
			var eventDataObject = new Event();
			eventDataObject.EventTime = evenTime.ToOffset();

			eventDataObject.AdditionalFieldsToUpdateCollection = new List<AdditionalFieldToUpdate>();

			var dataContext = DataContextFactory.New();
			var dataContextType = transactionIDTypeCode == Business.TransactionIDTypeCodeList.Codes.ISFTransactionNumber ? DataContextType.USImporterSecurityFiling : DataContextType.CustomsDeclaration;
			dataContext.AddDataTarget(dataContextType, null);
			dataContext.DataProviderForCodeMapping = "USCATAIR";
			dataContext.SetActionPurpose(new CodeDescriptionPair() { Code = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification, Description = "" });
			eventDataObject.DataContext = dataContext;

			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification,
				Department = "CBP"
			};
			eventDataObject.EventReference = Message.EM_MessageNum;
			eventDataObject.EventType = Enterprise.ZArchitecture.Business.AutoEvents.MessageReceivedCode;
			eventDataObject.ContextCollection = new List<Context>();

			if (!entryNumber.IsEmpty)
			{
				eventDataObject.ContextCollection.Add(new Context() { Type = nameof(Event.ContextTypes.EntryNumber), Value = entryNumber });
			}

			if (!entryType.IsEmpty)
			{
				eventDataObject.ContextCollection.Add(new Context() { Type = nameof(Event.ContextTypes.EntryNumberType), Value = entryType });
			}

			if (!notificationDetail.IsEmpty)
			{
				eventDataObject.ContextCollection.Add(new Context() { Type = nameof(Event.ContextTypes.NotificationDetails), Value = notificationDetail });
			}

			if (!principalName.IsEmpty)
			{
				eventDataObject.ContextCollection.Add(new Context() { Type = nameof(Event.ContextTypes.RecipientName), Value = principalName });
			}

			eventDataObject.ContextCollection.Add(new Context() { Type = nameof(Event.ContextTypes.InternalTransactionNumber), Value = Message.EM_MessageNum });

			if (!principalName.IsEmpty)
			{
				eventDataObject.AdditionalFieldsToUpdateCollection.Add(new AdditionalFieldToUpdate() { Type = PrincipalName, Value = principalName });
			}

			if (!designationType.IsEmpty)
			{
				eventDataObject.AdditionalFieldsToUpdateCollection.Add(new AdditionalFieldToUpdate() { Type = DesignationType, Value = designationType });
			}

			if (!dispositionCode.IsEmpty)
			{
				eventDataObject.AdditionalFieldsToUpdateCollection.Add(new AdditionalFieldToUpdate() { Type = DispositionCode, Value = dispositionCode });
			}

			if (!bondNumber.IsEmpty)
			{
				eventDataObject.AdditionalFieldsToUpdateCollection.Add(new AdditionalFieldToUpdate() { Type = CBPBondNumber, Value = bondNumber });
			}

			if (!bondAmount.IsEmpty)
			{
				eventDataObject.AdditionalFieldsToUpdateCollection.Add(new AdditionalFieldToUpdate() { Type = BondAmount, Value = bondAmount.ToString() });
			}
			return eventDataObject;
		}

		ZString GenerateHtmlNotificationDetails(Dictionary<string, HtmlTableCreator> messageBlocksMapToHtmlTable)
		{
			var resultBuilder = new ZStringBuilder();
			foreach (var htmlTable in messageBlocksMapToHtmlTable)
			{
				resultBuilder.Append("<b>" + htmlTable.Key + "</b>");
				resultBuilder.Append("<br>");
				resultBuilder.Append(htmlTable.Value.ToHtml());
				resultBuilder.Append("<br>");
			}
			return resultBuilder.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		Dictionary<string, HtmlTableCreator> ProcessBlocksAndGenerateHtmlContextCollection()
		{
			var messageBlocksMapToHtmlTable = new Dictionary<string, HtmlTableCreator>();
			foreach (MessageBlock block in Message.MessageBlock.MessageBlocks)
			{
				var tableTitle = GetTableTitleFromBlock(block);

				if (block is CESNB1)
				{
					var b1 = block as CESNB1;
					HtmlTableCreator bondStatusGroupingTable;
					if (!messageBlocksMapToHtmlTable.TryGetValue(tableTitle, out bondStatusGroupingTable))
					{
						bondStatusGroupingTable = new HtmlTableCreator(new string[] { "Column", "Value" });
						bondStatusGroupingTable.EnableHTMLEncoding = false;
					}
					dispositionCode = b1.DispositionCode;
					bondNumber = b1.BondNumber;
					var dispositionDescription = Factory.GetCachedValue<BondDispositionCodeList>().GetDescriptionFromCode(dispositionCode);
					bondStatusGroupingTable.WriteRow("Bond Number", bondNumber);
					bondStatusGroupingTable.WriteRow("Disposition Code", dispositionCode + " / " + (string.IsNullOrEmpty(dispositionDescription) ? (string)b1.DispositionCodeDescriptionText : dispositionDescription));
					bondStatusGroupingTable.WriteRow("Source of Action", GetSourceOfAction(b1.SourceOfAction));
					evenTime = DateTimeParser.GetDateTimeFromZDateAndStringTime(b1.DateOfAction, b1.ActionTime);
					bondStatusGroupingTable.WriteRow("Date And Time of Action", evenTime);
					bondedOrganisations = GetBondedOrganisations(bondNumber);
					if (bondedOrganisations != null)
					{
						var importerColumn = "Importer Name";
						foreach (OrgHeader org in bondedOrganisations)
						{
							bondStatusGroupingTable.WriteRow(importerColumn, org.OH_FullName);
						}
					}
					bondStatusGroupingTable.WriteRow("Re-send Indicator", b1.ResendIndicator == "Y" ? "Notification has been re-sent" : "");
					messageBlocksMapToHtmlTable[tableTitle] = bondStatusGroupingTable;
				}
				else if (block is CESNB2)
				{
					var b2 = block as CESNB2;
					HtmlTableCreator bondStatusGroupingTable;
					if (messageBlocksMapToHtmlTable.TryGetValue(tableTitle, out bondStatusGroupingTable))
					{
						bondStatusGroupingTable.WriteRow("Transaction ID Type", b2.TransactionIDTypeCode);
						bondStatusGroupingTable.WriteRow("Entry Type Code", b2.EntryTypeCode);
						bondStatusGroupingTable.WriteRow("Transaction ID", b2.TransactionID);
						bondStatusGroupingTable.WriteRow("Drawback Claim Amount", b2.DrawbackClaimAmount);
					}
				}
				else if (block is CESNB3)
				{
					var b3 = block as CESNB3;
					HtmlTableCreator bondStatusGroupingTable;
					if (messageBlocksMapToHtmlTable.TryGetValue(tableTitle, out bondStatusGroupingTable))
					{
						bondStatusGroupingTable.WriteRow("Remarks", b3.Remarks);
					}
				}
				else if (block is CESN10)
				{
					var cesn10 = block as CESN10;
					HtmlTableCreator bondHeaderinformationTable;
					if (!messageBlocksMapToHtmlTable.TryGetValue(tableTitle, out bondHeaderinformationTable))
					{
						bondHeaderinformationTable = new HtmlTableCreator(new string[] { "Bond Type Code", "Bond Activity Code", "Bond Amount", "Reconciliation Bond Rider Flag", "USVI Bond Rider Flag", "Bond Designation Type code", "Execution Date", "Surety Reference Number", "Effective Date", "Termination Date" });
						bondHeaderinformationTable.EnableHTMLEncoding = false;
					}
					designationType = cesn10.BondDesignationTypeCode;
					bondAmount = cesn10.BondAmount;
					bondType = cesn10.BondTypeCode;
					terminateDate = cesn10.TerminationDate;
					if (designationType == BondDesignationCodeList.Codes.TerminateContinuousBond && bondType == BondTypeList.Codes.ContinuousBond)
					{
						UpdateBondExpiryDates(cesn10.BondNumber, terminateDate);
					}
					bondHeaderinformationTable.WriteRow(GetBondTypeCodeDesc(bondType), GetBondActivityCodeDesc(cesn10.BondActivityCode), bondAmount, GetReconciliationBondRiderFlagDesc(cesn10.ReconciliationBondRiderFlag), GetUSVIBondRiderFlagDesc(cesn10.USVIBondRiderFlag), designationType, cesn10.ExecutionDate.ToShortDateString(), cesn10.SuretyReferenceNumber, cesn10.EffectiveDate.ToShortDateString(), terminateDate.ToShortDateString());
					messageBlocksMapToHtmlTable[tableTitle] = bondHeaderinformationTable;
				}
				else if (block is CESN12)
				{
					var cesn12 = block as CESN12;
					HtmlTableCreator secondaryNotifyPartyTable;
					if (!messageBlocksMapToHtmlTable.TryGetValue(tableTitle, out secondaryNotifyPartyTable))
					{
						secondaryNotifyPartyTable = new HtmlTableCreator(new string[] { "Column", "Value" });
						secondaryNotifyPartyTable.EnableHTMLEncoding = false;
					}
					WriteRowForSecondaryNotifyParty(secondaryNotifyPartyTable, cesn12.SecondaryNotifyPartyCode);
					WriteRowForSecondaryNotifyParty(secondaryNotifyPartyTable, cesn12.SecondaryNotifyPartyCode1);
					WriteRowForSecondaryNotifyParty(secondaryNotifyPartyTable, cesn12.SecondaryNotifyPartyCode2);
					WriteRowForSecondaryNotifyParty(secondaryNotifyPartyTable, cesn12.SecondaryNotifyPartyCode3);
					messageBlocksMapToHtmlTable[tableTitle] = secondaryNotifyPartyTable;
				}
				else if (block is CESN20)
				{
					var cesn20 = block as CESN20;
					HtmlTableCreator singleTransactionBondAndPrincipalTable;
					if (!messageBlocksMapToHtmlTable.TryGetValue(tableTitle, out singleTransactionBondAndPrincipalTable))
					{
						singleTransactionBondAndPrincipalTable = new HtmlTableCreator(new string[] { "Column", "Value" });
						singleTransactionBondAndPrincipalTable.EnableHTMLEncoding = false;
					}
					singleTransactionBondAndPrincipalTable.WriteRow("Transaction ID Type Code", TransactionIDTypeCodeList.GetDescriptionFromCode(cesn20.TransactionIDTypeCode));

					if (!cesn20.EntryTypeCode.IsEmpty)
					{
						singleTransactionBondAndPrincipalTable.WriteRow("Entry Type", cesn20.EntryTypeCode);
					}

					if (!cesn20.TransactionID.IsEmpty)
					{
						singleTransactionBondAndPrincipalTable.WriteRow("Transaction Number", cesn20.TransactionID);
						entryType = cesn20.EntryTypeCode;
						transactionIDTypeCode = cesn20.TransactionIDTypeCode;
						entryNumber = GetEffectiveTransactionID(cesn20.TransactionID, transactionIDTypeCode);
					}
					messageBlocksMapToHtmlTable[tableTitle] = singleTransactionBondAndPrincipalTable;
				}
				else if (block is CESN30)
				{
					var cesn30 = block as CESN30;
					HtmlTableCreator singleTransactionBondAndPrincipalTable;
					if (!messageBlocksMapToHtmlTable.TryGetValue(tableTitle, out singleTransactionBondAndPrincipalTable))
					{
						singleTransactionBondAndPrincipalTable = new HtmlTableCreator(new string[] { "Column", "Value" });
						singleTransactionBondAndPrincipalTable.EnableHTMLEncoding = false;
					}
					singleTransactionBondAndPrincipalTable.WriteRow("Principal ID Number Type", GetPrincipalIDNumberTypeDesc(cesn30.PrincipalIDNumberType));
					singleTransactionBondAndPrincipalTable.WriteRow("Principal ID Number", cesn30.PrincipalIDNumber);
					if (!cesn30.PrincipalName.IsEmpty)
					{
						principalName = cesn30.PrincipalName;
						singleTransactionBondAndPrincipalTable.WriteRow("Principal Name", principalName);
					}
					messageBlocksMapToHtmlTable[tableTitle] = singleTransactionBondAndPrincipalTable;
				}
				else if (block is CESN35)
				{
					var cesn35 = block as CESN35;

					HtmlTableCreator cesn35Table;
					if (!messageBlocksMapToHtmlTable.TryGetValue(tableTitle, out cesn35Table))
					{
						cesn35Table = new HtmlTableCreator(new string[] { "Co-principal ID Number Type", "Co-principal ID Number", "Co-principal Name" });
						cesn35Table.EnableHTMLEncoding = false;
					}
					cesn35Table.WriteRow(GetPrincipalIDNumberTypeDesc(cesn35.CoprincipalIDNumberType), cesn35.CoprincipalIDNumber, cesn35.CoprincipalName);
					messageBlocksMapToHtmlTable[tableTitle] = cesn35Table;
				}
				else if (block is CESN36)
				{
					var cesn36 = block as CESN36;

					HtmlTableCreator cesn36Table = null;
					if (!messageBlocksMapToHtmlTable.TryGetValue(tableTitle, out cesn36Table))
					{
						cesn36Table = new HtmlTableCreator(new string[] { "Bond User ID Number Type", "Bond User ID Number", "Bond User Name", "User Rider Action Code", "User Add Date", "User Delete Date" });
						cesn36Table.EnableHTMLEncoding = false;
					}
					cesn36Table.WriteRow(GetPrincipalIDNumberTypeDesc(cesn36.BondUserIDNumberType), cesn36.BondUserIDNumber, cesn36.BondUserName, GetUserRiderActionCode(cesn36.UserRiderActionCode), cesn36.UserAddDate.ToShortDateString(), cesn36.UserDeleteDate.ToShortDateString());
					messageBlocksMapToHtmlTable[tableTitle] = cesn36Table;
				}
				else if (block is CESN40)
				{
					var cesn40 = block as CESN40;
					HtmlTableCreator cesn40Table = null;
					if (!messageBlocksMapToHtmlTable.TryGetValue(tableTitle, out cesn40Table))
					{
						cesn40Table = new HtmlTableCreator(new string[] { "Column", "Value" });
						cesn40Table.EnableHTMLEncoding = false;
					}
					cesn40Table.WriteRow("Surety Code", cesn40.SuretyCode);
					cesn40Table.WriteRow("Agent ID Number", cesn40.AgentIDNumber);
					if (!cesn40.SuretyName.IsEmpty)
					{
						cesn40Table.WriteRow("Surety Name", cesn40.SuretyName);
					}
					if (!cesn40.SuretyLiabilityAmount.IsEmpty)
					{
						cesn40Table.WriteRow("Surety Liability Amount", cesn40.SuretyLiabilityAmount);
					}
					messageBlocksMapToHtmlTable[tableTitle] = cesn40Table;
				}
				else if (block is CESN45)
				{
					var cesn45 = block as CESN45;

					HtmlTableCreator cesn45Table = null;
					if (!messageBlocksMapToHtmlTable.TryGetValue(tableTitle, out cesn45Table))
					{
						cesn45Table = new HtmlTableCreator(new string[] { "Co-Surety Code", "Agent ID Number", "Co-Surety Name", "Co-Surety Liability Amount" });
						cesn45Table.EnableHTMLEncoding = false;
					}
					cesn45Table.WriteRow(cesn45.CoSuretyCode, cesn45.AgentIDNumber, cesn45.CoSuretyName, cesn45.CoSuretyLiabilityAmount);
					messageBlocksMapToHtmlTable[tableTitle] = cesn45Table;
				}
				else if (block is CESN46)
				{
					var cesn46 = block as CESN46;
					HtmlTableCreator cesn46Table;
					if (!messageBlocksMapToHtmlTable.TryGetValue(tableTitle, out cesn46Table))
					{
						cesn46Table = new HtmlTableCreator(new string[] { "Column", "Value" });
						cesn46Table.EnableHTMLEncoding = false;
					}
					cesn46Table.WriteRow("Surety Code for Re-insurer", cesn46.SuretyCodeForReinsurer);
					cesn46Table.WriteRow("Agent ID Number", cesn46.AgentIDNumber);
					if (!cesn46.SuretyName.IsEmpty)
					{
						cesn46Table.WriteRow("Surety Name", cesn46.SuretyName);
					}
					messageBlocksMapToHtmlTable[tableTitle] = cesn46Table;
				}
				else if (block is CESNT1)
				{
					var cesnt1 = block as CESNT1;
					HtmlTableCreator entryStatusHeaderInformationTable;
					if (!messageBlocksMapToHtmlTable.TryGetValue(tableTitle, out entryStatusHeaderInformationTable))
					{
						entryStatusHeaderInformationTable = new HtmlTableCreator(new string[] { "Column", "Value" });
						entryStatusHeaderInformationTable.EnableHTMLEncoding = false;
					}
					GenerateForCESNT1(cesnt1, entryStatusHeaderInformationTable);
					messageBlocksMapToHtmlTable[tableTitle] = entryStatusHeaderInformationTable;

					if (entryNumber.IsEmpty)
					{
						entryNumber = cesnt1.FilerCode + cesnt1.EntryNumber;
					}
					if (entryType.IsEmpty && !cesnt1.EntryType.IsEmpty)
					{
						entryType = cesnt1.EntryType;
					}
				}
				else if (block is CESNT2)
				{
					var cesnt2 = block as CESNT2;
					HtmlTableCreator entryStatusHeaderInformationTable;
					if (messageBlocksMapToHtmlTable.TryGetValue(tableTitle, out entryStatusHeaderInformationTable))
					{
						GenerateForCESNT2(cesnt2, entryStatusHeaderInformationTable);
					}
				}
				else if (block is CESNT3)
				{
					var cesnt3 = block as CESNT3;
					HtmlTableCreator entryStatusHeaderInformationTable;
					if (messageBlocksMapToHtmlTable.TryGetValue(tableTitle, out entryStatusHeaderInformationTable))
					{
						GenerateForCESNT3(cesnt3, entryStatusHeaderInformationTable);
					}
				}
				else if (block is CESNT4)
				{
					var cesnt4 = block as CESNT4;
					HtmlTableCreator entryStatusHeaderInformationTable;
					if (messageBlocksMapToHtmlTable.TryGetValue(tableTitle, out entryStatusHeaderInformationTable))
					{
						GenerateForCESNT4(cesnt4, entryStatusHeaderInformationTable);
					}
				}
			}
			return messageBlocksMapToHtmlTable;
		}

		ZString GetEffectiveTransactionID(ZString transactionID, ZString typeCode)
		{
			var result = transactionID;
			if (transactionID.Length > 35)
			{
				switch (typeCode)
				{
					case Business.TransactionIDTypeCodeList.Codes.EntryNumber:
						result = transactionID.Right(11);
						break;
					case Business.TransactionIDTypeCodeList.Codes.ISFTransactionNumber:
						result = transactionID.Right(15);
						break;
					case Business.TransactionIDTypeCodeList.Codes.SeizureCaseNumber:
						result = transactionID.Right(16);
						break;
					default:
						result = transactionID.TrimStart('0');
						break;
				}
			}
			return result;
		}

		void UpdateBondExpiryDates(ZString bondNum, ZDate date)
		{
			var bondedOrgs = bondedOrganisations ?? GetBondedOrganisations(bondNum);
			foreach (var org in bondedOrgs)
			{
				foreach (MasterFiles.Business.CusBondDetail bond in org.CusBondDetails)
				{
					if (bond.PW_BondNumber == bondNum
						&& bond.PW_BondType == BondTypeList.Codes.ContinuousBond
						&& bond.PW_ApplicationCode == ApplicationCodeList.Codes.UsaInBond)
					{
						bond.PW_BondExpiryDate = date.ToZDateTime();
					}
				}
			}
		}

		void GenerateForCESNT4(CESNT4 cesnt4, HtmlTableCreator entryStatusHeaderInformationTable)
		{
			entryStatusHeaderInformationTable.WriteRow("NAFTA Reconciliation Flag", GetNAFTAReconciliationFlagDesc(cesnt4.NAFTAReconciliationFlag));
			entryStatusHeaderInformationTable.WriteRow("NAFTA Reconciliation", cesnt4.NAFTAReconciliation);
			entryStatusHeaderInformationTable.WriteRow("Other Reconciliation Flag", GetOtherReconciliationFlagDesc(cesnt4.OtherReconciliationFlag));
			entryStatusHeaderInformationTable.WriteRow("Other Reconciliation Entry", cesnt4.OtherReconciliationEntry);
			entryStatusHeaderInformationTable.WriteRow("Single Transaction Bond Amount", cesnt4.SingleTransactionBondAmount);
			entryStatusHeaderInformationTable.WriteRow("Single Transaction Bond Producer Account Number", cesnt4.SingleTransactionBondProducerAccountNumber);
		}

		void GenerateForCESNT3(CESNT3 cesnt3, HtmlTableCreator entryStatusHeaderInformationTable)
		{
			entryStatusHeaderInformationTable.WriteRow("Estimated Fee", cesnt3.EstimatedFee);
			entryStatusHeaderInformationTable.WriteRow("Estimated Antidumping Duty", cesnt3.EstimatedAntidumpingDuty);
			entryStatusHeaderInformationTable.WriteRow("Bonded Antidumping Duty", cesnt3.BondedAntidumpingDuty);
			entryStatusHeaderInformationTable.WriteRow("Estimated Countervailing Duty", cesnt3.EstimatedCountervailingDuty);
			entryStatusHeaderInformationTable.WriteRow("Bonded Countervailing Duty", cesnt3.BondedCountervailingDuty);
			entryStatusHeaderInformationTable.WriteRow("Recon Entry NAFTA Issue Entry Type 09", cesnt3.ReconEntryNAFTAIssueEntryType09 == "1" ? "NAFTA Recon" : string.Empty);
			entryStatusHeaderInformationTable.WriteRow("Recon Entry Other Issue Entry Type 09", GetReconEntryOtherIssueEntryTypeDesc(cesnt3.ReconEntryOtherIssueEntryType09));
			entryStatusHeaderInformationTable.WriteRow("Pay Basis Indicator", cesnt3.PayBasisIndicator);
		}

		void GenerateForCESNT2(CESNT2 cesnt2, HtmlTableCreator entryStatusHeaderInformationTable)
		{
			entryStatusHeaderInformationTable.WriteRow("Entry Date", cesnt2.EntryDate.ToShortDateString());
			entryStatusHeaderInformationTable.WriteRow("Record Status", GetRecordStatusDesc(cesnt2.RecordStatus));
			entryStatusHeaderInformationTable.WriteRow("Accelerated Drawback Indicator", GetAcceleratedDrawbackIndicatorDesc(cesnt2.AcceleratedDrawbackIndicator));
			entryStatusHeaderInformationTable.WriteRow("Possible Late Indicator", GetPossibleLateIndicatorDesc(cesnt2.PossibleLateIndicator));
			entryStatusHeaderInformationTable.WriteRow("OGA Indicator", cesnt2.OGAIndicator);
			entryStatusHeaderInformationTable.WriteRow("Entry Summary Date", cesnt2.EntrySummaryDate.ToShortDateString());
			entryStatusHeaderInformationTable.WriteRow("Collection Date", cesnt2.CollectionDate.ToShortDateString());
			entryStatusHeaderInformationTable.WriteRow("Warehouse Entry Summary Number", cesnt2.WarehouseEntrySummaryNumber);
			entryStatusHeaderInformationTable.WriteRow("Tentative TIB Close Date / Liquidation Date", cesnt2.TentativeTIBCloseDateLiquidationDate.ToShortDateString());
			entryStatusHeaderInformationTable.WriteRow("Estimated Duty", cesnt2.EstimatedDuty);
			entryStatusHeaderInformationTable.WriteRow("Estimated Taxes", cesnt2.EstimatedTaxes);
		}

		void GenerateForCESNT1(CESNT1 cesnt1, HtmlTableCreator entryStatusHeaderInformationTable)
		{
			entryStatusHeaderInformationTable.WriteRow("District/Port of Entry", cesnt1.DistrictPortOfEntry);
			entryStatusHeaderInformationTable.WriteRow("Filer Code", cesnt1.FilerCode);
			entryStatusHeaderInformationTable.WriteRow("Entry Number", cesnt1.EntryNumber);
			entryStatusHeaderInformationTable.WriteRow("Surety Code", cesnt1.SuretyCode);
			entryStatusHeaderInformationTable.WriteRow("Bond Number", cesnt1.BondNumber);
			entryStatusHeaderInformationTable.WriteRow("Bond Type", GetBondTypeCodeDesc(cesnt1.BondType));
			entryStatusHeaderInformationTable.WriteRow("Entry Type", cesnt1.EntryType);
			entryStatusHeaderInformationTable.WriteRow("Release/Summary Indicator", GetReleaseSummaryIndicatorDesc(cesnt1.ReleaseSummaryIndicator));
			if (!SocialSecurityNumberValidator.IsValidSSN(cesnt1.ImporterOfRecord))
			{
				entryStatusHeaderInformationTable.WriteRow("Importer of Record", cesnt1.ImporterOfRecord);
			}
			entryStatusHeaderInformationTable.WriteRow("Entry Source", GetEntrySourceDesc(cesnt1.EntrySource));
			entryStatusHeaderInformationTable.WriteRow("Release Data", cesnt1.ReleaseData.ToShortDateString());
			entryStatusHeaderInformationTable.WriteRow("Value", cesnt1.Value);
			entryStatusHeaderInformationTable.WriteRow("Broker Reference Number", cesnt1.BrokerReferenceNumber);
			entryStatusHeaderInformationTable.WriteRow("Cancel Date", cesnt1.CancelDate);
			entryStatusHeaderInformationTable.WriteRow("Collection Status", GetCollectionStatusDesc(cesnt1.CollectionStatus));
		}

		void WriteRowForSecondaryNotifyParty(HtmlTableCreator secondaryNotifyParty, ZString secondaryNotifyPartyCode)
		{
			if (!secondaryNotifyPartyCode.IsEmpty)
			{
				secondaryNotifyParty.WriteRow("Secondary Notify Party Code", secondaryNotifyPartyCode);
			}
		}

		string GetReconciliationBondRiderFlagDesc(ZString reconciliationBondRiderFlag)
		{
			switch (reconciliationBondRiderFlag)
			{
				case "Y":
					return "Flag for reconciliation";
				case "N":
					return "Remove flag for reconciliation";
				default:
					return reconciliationBondRiderFlag;
			}
		}

		string GetOtherReconciliationFlagDesc(ZString otherReconciliationFlag)
		{
			switch (otherReconciliationFlag)
			{
				case "001":
					return "Value Recon due";
				case "002":
					return "Class Recon due";
				case "003":
					return "9802 Recon due";
				case "004":
					return "Value/Class Recon due";
				case "005":
					return "Value/9802 Recon due";
				case "006":
					return "Class/9802 Recon due";
				case "007":
					return "Value/Class/9802 Recon due";
				case "021":
					return "Value Recon filed";
				case "022":
					return "Class Recon filed";
				case "023":
					return "9802 Recon filed";
				case "024":
					return "Value/Class/Recon filed";
				case "025":
					return "Value/9802 Recon filed";
				case "026":
					return "Class/9802 Recon filed";
				case "027":
					return "Value/Class/9802 Recon filed";
				default:
					return otherReconciliationFlag;
			}
		}

		ZString GetUSVIBondRiderFlagDesc(ZString usvIBondRiderFlag)
		{
			return usvIBondRiderFlag == "Y" ? (ZString)"Flag for importation into the U.S. Virgin Islands" : usvIBondRiderFlag;
		}

		string GetUserRiderActionCode(ZString userRiderActionCode)
		{
			switch (userRiderActionCode)
			{
				case "A":
					return "Rider to Add User to Bond";
				case "D":
					return "Rider to Delete User from the Bond";
				default:
					return userRiderActionCode;
			}
		}

		string GetNAFTAReconciliationFlagDesc(ZString nAFTAReconciliationFlag)
		{
			switch (nAFTAReconciliationFlag)
			{
				case "1":
					return "NAFTA Recon. Required";
				case "2":
					return "NAFTA Recon. Filed";
				default:
					return nAFTAReconciliationFlag;
			}
		}

		string GetPossibleLateIndicatorDesc(ZString possibleLateIndicator)
		{
			switch (possibleLateIndicator)
			{
				case "1":
					return "More than 10 days late";
				case "2":
					return "More than 30 days late";
				default:
					return possibleLateIndicator.ToString();
			}
		}

		string GetReconEntryOtherIssueEntryTypeDesc(string reconEntryOtherIssueEntryType)
		{
			switch (reconEntryOtherIssueEntryType)
			{
				case "021":
					return "Value Recon";
				case "022":
					return "Class Recon";
				case "023":
					return "9802 Recon";
				case "024":
					return "Value/Class Recon";
				case "025":
					return "Value/9802 Recon";
				case "026":
					return "Class/9802 Recon";
				case "027":
					return "Value/Class/9802 Recon";
				default:
					return reconEntryOtherIssueEntryType;
			}
		}

		string GetAcceleratedDrawbackIndicatorDesc(string acceleratedDrawbackIndicator)
		{
			switch (acceleratedDrawbackIndicator)
			{
				case "A":
					return "Accelerated drawback approved";
				case "N":
					return "Accelerated drawback not claimed or not approved";
				default:
					return acceleratedDrawbackIndicator;
			}
		}

		string GetRecordStatusDesc(ZString recordStatus)
		{
			switch (recordStatus)
			{
				case "00":
					return "Entry accepted not liquidated";
				case "01":
					return "Open TIB entry";
				case "02":
					return "Closed TIB entry penalty";
				case "03":
					return "Closed TIB entry with compliance";
				case "04":
					return "Cancelled";
				case "05":
					return "On Customs not follow up summary report";
				case "06":
					return "On Customs unpaid report";
				case "07":
					return "ADD/CVD information only";
				case "08":
					return "Entry rejected";
				case "09":
					return "Entry liquidated";
				case "10":
					return "Entry reliquidated";
				case "11":
					return "Release only";
				default:
					return recordStatus.ToString();
			}
		}

		string GetSourceOfAction(ZString sourceAction)
		{
			switch (sourceAction)
			{
				case "1":
					return "Action taken by Surety/Surety Agent";
				case "2":
					return "Action by CBP - Office of Administration or An Automated ACE Process";
				case "3":
					return "Action by Other Trade Part";
				default:
					return sourceAction.ToString();
			}
		}

		string GetCollectionStatusDesc(string collectionStatus)
		{
			switch (collectionStatus)
			{
				case "0":
					return "Unpaid";
				case "1":
					return "Partial paid";
				case "2":
					return "Fully paid";
				case "3":
					return "Duty free";
				case "4":
					return "Shell";
				case "5":
					return "Drawback";
				default:
					return collectionStatus;
			}
		}

		string GetEntrySourceDesc(string entrySource)
		{
			switch (entrySource)
			{
				case "0":
					return "Customs";
				case "1":
					return "ABI";
				default:
					return entrySource;
			}
		}

		string GetReleaseSummaryIndicatorDesc(ZString releaseSummaryIndicator)
		{
			switch (releaseSummaryIndicator)
			{
				case "1":
					return "Release data";
				case "2":
					return "Entry summary data";
				case "3":
					return "Release summary data";
				default:
					return releaseSummaryIndicator.ToString();
			}
		}

		string GetBondTypeCodeDesc(ZString bondTypeCode)
		{
			switch (bondTypeCode)
			{
				case "8":
					return "Continuous bond";
				case "9":
					return "Single transaction bond";
				default:
					return bondTypeCode.ToString();
			}
		}

		string GetBondActivityCodeDesc(ZString bondActivityCode)
		{
			switch (bondActivityCode.Trim())
			{
				case "1A":
					return "Drawback Payments Refunds";
				case "2":
					return "Custodian of Bonded Merchandise";
				case "3":
					return "International Carrier";
				case "3A":
					return "Instruments of International Traffic";
				case "3A3":
					return "Carrier of International Traffic";
				case "4":
					return "Foreign Trade Zone Operator";
				case "5":
					return "Public Gauger";
				case "6":
					return "Wool & Fur Products";
				case "7":
					return "Bill of Lading";
				case "8":
					return "Detention of Copyrighted Material";
				case "10":
					return "Court Costs for Condemned Goods";
				case "11":
					return "Airport Security Bond";
				case "16":
					return "Importer Security Filing (ISF)";
				case "17":
					return "Marine Terminal Operator";
				case "19":
					return "User Fee Facility Bond";
				default:
					return bondActivityCode;
			}
		}

		string GetPrincipalIDNumberTypeDesc(ZString principalIDNumberType)
		{
			switch (principalIDNumberType.Trim())
			{
				case "EI":
					return "Employer Identification Number (IRS #)";
				case "ANI":
					return "CBP-assigned Number";
				case "34":
					return "Social Security Number";
				default:
					return principalIDNumberType;
			}
		}

		string GetTableTitleFromBlock(MessageBlock block)
		{
			if ((block is CESNB1) || (block is CESNB2) || (block is CESNB3))
			{
				return "Bond Status Grouping";
			}
			if (block is CESN10)
			{
				return "Bond Header information";
			}
			if (block is CESN12)
			{
				return "Secondary Notify Party";
			}
			if (block is CESN20)
			{
				return "Single Transaction Bond information";
			}
			if (block is CESN30)
			{
				return "Principal information";
			}
			if (block is CESN35)
			{
				return "Co-Principal information";
			}
			if (block is CESN36)
			{
				return "User information";
			}
			if (block is CESN40)
			{
				return "Surety information";
			}
			if (block is CESN45)
			{
				return "Co-Surety information";
			}
			if (block is CESN46)
			{
				return "Re-Insurer information";
			}
			if ((block is CESNT1) || (block is CESNT2) || (block is CESNT3) || (block is CESNT4))
			{
				return "Entry Status Header Information";
			}
			return string.Empty;
		}

		OrgHeader[] GetBondedOrganisations(ZString bondNum)
		{
			var orgFilter = new ZDBOnlyQuery(typeof(OrgHeader));
			if (!bondNum.IsEmpty)
			{
				var bondNumberSubQuery = new ZDBOnlySubQuery(typeof(CusBondDetail), CusBondDetailSchema.PW_ParentID);
				bondNumberSubQuery.AddToFilter(CusBondDetailSchema.PW_ParentTableCode, OrgHeaderSchema.Constants.Prefix);
				bondNumberSubQuery.AddToFilter(CusBondDetailSchema.PW_BondNumber, bondNum);
				bondNumberSubQuery.AddToFilter(CusBondDetailSchema.PW_ApplicationCode, ApplicationCodeList.Codes.UsaInBond);
				orgFilter.AddSubQuery(bondNumberSubQuery, JoinCondition.And);

				return Factory.Load<OrgHeader>(orgFilter);
			}
			return null;
		}

		public const string PrincipalName = "PrincipalName";
		public const string DesignationType = "DesignationType";
		public const string DispositionCode = "DispositionCode";
		public const string CBPBondNumber = "CBPBondNumber";
		public const string BondAmount = "BondAmount";
	}
}
