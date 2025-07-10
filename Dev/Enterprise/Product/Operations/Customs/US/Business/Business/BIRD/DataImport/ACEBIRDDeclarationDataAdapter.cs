using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// This class is responsible for creating and updating declarations for BIRD 7501/3461
	/// </summary>
	public class ACEBIRDDeclarationDataAdapter
	{
		public void DoImport(JobDeclaration declaration, InputBlockControlGenerator<BRDAABIB, BRDAABIY> generator, INotifications notifications)
		{
			if (declaration.IsInDatabase)
			{
				ZString declarationReasonForNotAbleToUpdate = declaration.GetReasonForNotAbleToUpdate();
				if (!declarationReasonForNotAbleToUpdate.IsEmpty)
				{
					notifications.AddError(declarationReasonForNotAbleToUpdate);
					return;
				}
			}
			var applicationID = generator.B.ApplicationIdentifierCode;
			SetDeclarationFields(declaration, applicationID, generator, notifications);

			var birdData = CreateInvoiceLinesAndGatherDataToBeProcessed(declaration, generator, notifications);
			Merge(declaration, notifications);

			var entry = applicationID == ACEApplicationIdentifierCodeList.Codes.EntrySummary ? declaration.ActiveEntryHeaders.EntrySummaryEntry : declaration.ActiveEntryHeaders.SimplifiedEntry;

			if (entry != null)
			{
				using (declaration.SuspendDefaultingSecondaryTariffLines())
				using (declaration.SuspendDefaultingMessageMode())
				{
					declaration.IsImportingData = true;

					try
					{
						ProcessMessageBlocks(declaration, entry, birdData, notifications);
					}
					finally
					{
						declaration.IsImportingData = false;
					}
				}
				OnProcessed(declaration, generator, notifications, applicationID);
			}
			else
			{
				notifications.AddError(MergeUnsuccessful);
			}
		}

		internal const string MergeUnsuccessful = "There is no customs entry created even after merge was attempted and the system cannot proceed any more.";

		void SetDeclarationFields(JobDeclaration declaration, string applicationID, InputBlockControlGenerator<BRDAABIB, BRDAABIY> generator, INotifications notifications)
		{
			if (!declaration.IsInDatabase)
			{
				SetDefaultDataIfNewDeclaration(declaration, generator);
			}

			declaration.Bills.RemoveAndDeleteAll();
			declaration.Invoices.DeleteAll();
			declaration.LinkedEntryNumbers.RemoveAndDeleteAll();

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			if (applicationID == ACEApplicationIdentifierCodeList.Codes.EntrySummary)
			{
				declaration.US_EnableENS = true;
				if (!declaration.US_EnableCRL)
				{
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				}

				// DN: Determine whether US_NoDutyCalc is used in ACE
				//if (declaration.IsImportByExternalBroker)
				//{
				//	declaration.US_NoDutyCalc = true;
				//}

				ResetDutyFeeAmounts(declaration);
			}
			else if (applicationID == ACEApplicationIdentifierCodeList.Codes.CargoRelease)
			{
				declaration.US_EnableCRL = true;
				var se10 = generator.MessageBlocks.OfType<ASESE10>().FirstOrDefault();
				string transportMode = se10 != null ? TransportTypeList.ConvertFromTransportCode(se10.ModeOfTransportationMOTCode) : null;
				declaration.JE_TransportMode = string.IsNullOrEmpty(transportMode) ? TransportTypeList.Codes.Sea : transportMode;
			}

			if (declaration.US_EnableENS)
			{
				//This is updated during ENS40 update and having an existing value before line update causes a wrong warning.
				declaration.US_SchDLoading = ZString.Empty;
				declaration.JE_ExportDate = ZDateTime.Empty;
			}

			SetExternalBroker(declaration, generator, notifications);
		}

		void SetExternalBroker(JobDeclaration declaration, InputBlockControlGenerator<BRDAABIB, BRDAABIY> generator, INotifications notifications)
		{
			var b = generator.B;
			var entryFilerCode = b.FilerCode;

			var currentCompanyFiler = USCustomsDataRegistry.Instance.EntryFiler.Value;
			var currencyCompanyFilerCode = currentCompanyFiler != null ? currentCompanyFiler.EntryFilerCode : ZString.Empty;

			if (!entryFilerCode.IsEmpty && entryFilerCode != currencyCompanyFilerCode)
			{
				var organisations = new OrgHeader.Loader(declaration.Factory).LoadDBOrganisations(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.EntryFilerCode, entryFilerCode);

				if (organisations.Length == 1)
				{
					declaration.JE_OH_ExternalBroker = organisations[0].PK;
				}

				if (organisations.Length == 0)
				{
					notifications.AddWarning(string.Format(CultureInfo.InvariantCulture, NoOrganisationWithSpecifiedEntryFilerCode, entryFilerCode));
				}
				else if (organisations.Length > 1)
				{
					notifications.AddWarning(string.Format(CultureInfo.InvariantCulture, MultipleOrganisationsWithSpecifiedEntryFilerCode, entryFilerCode));
				}
			}
		}

		public const string NoOrganisationWithSpecifiedEntryFilerCode = "There is no organization with this entry filer code, '{0}'. The system cannot communicate with this filer automatically without this field in declarations. Please create an organization and specify its entry filer code in Organization > Config > Customs Codes.";
		public const string MultipleOrganisationsWithSpecifiedEntryFilerCode = "There are multiple organizations with this entry filer code, '{0}'. The system cannot determine which organization to use. Please consider merging the orgazation records.";

		void OnProcessed(JobDeclaration declaration, InputBlockControlGenerator<BRDAABIB, BRDAABIY> generator, INotifications notifications, string applicationID)
		{
			UpdateInvoiceDetails(declaration);

			CalculateTotalPackage(declaration, notifications);

			declaration.ResumeApportionment();

			Merge(declaration, notifications);

			CreateMessage(declaration, generator, applicationID);
		}

		void SetDefaultDataIfNewDeclaration(JobDeclaration declaration, InputBlockControlGenerator<BRDAABIB, BRDAABIY> generator)
		{
			if (!declaration.IsInDatabase)
			{
				var headerIDRecord = generator.MessageBlocks.OfType<IBIRDHeaderIDRecord>().FirstOrDefault();

				if (headerIDRecord != null && !headerIDRecord.EntryNumber.IsEmpty && headerIDRecord.EntryFilerCode != USCustomsDataRegistry.Instance.EntryFiler.Value.EntryFilerCode)
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
				}
				else
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				}
				var brokerRefernceRecord = generator.MessageBlocks.OfType<IBIRDBrokerRefernceRecord>().FirstOrDefault();

				if (brokerRefernceRecord != null)
				{
					declaration.US_BRDRefNo = brokerRefernceRecord.BrokerReferenceNumber;
				}
			}
		}

		void CreateMessage(JobDeclaration declaration, InputBlockControlGenerator<BRDAABIB, BRDAABIY> generator, ZString applicationID)
		{
			var entry = applicationID == ACEApplicationIdentifierCodeList.Codes.CargoRelease ? declaration.ActiveEntryHeaders.SimplifiedEntry : declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var message = generator.CreateMessage<MQEDIMessage>(declaration.Factory);
			message.EM_LinkedObject = (BusinessObject)entry ?? declaration;
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_Status = MQEDIMessage.Status.Received;
			message.EM_MessageSubType = applicationID == ACEApplicationIdentifierCodeList.Codes.CargoRelease ? EM_MessageSubTypeList.Codes.BIRDCargoRelease : EM_MessageSubTypeList.Codes.BIRDEntrySummary;
		}

		void UpdateInvoiceDetails(JobDeclaration declaration)
		{
			foreach (JobComInvoiceHeader invoice in declaration.Invoices)
			{
				if (invoice.JZ_InvoiceAmount == 0)
				{
					invoice.JZ_InvoiceAmount = invoice.JZ_Calc_LinesEntered;
				}

				if (invoice.JZ_RX_NKInvoice_Currency.IsEmpty)
				{
					invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				}

				invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.FOB;
			}
		}

		void CalculateTotalPackage(JobDeclaration declaration, INotifications notifications)
		{
			ZString? unit = null;
			ZDecimal result = 0;

			foreach (Bill bill in declaration.LowestBills)
			{
				result += bill.CU_NoOfPacks;

				if (!bill.CU_PackType.IsEmpty)
				{
					if (!unit.HasValue)
					{
						unit = bill.CU_PackType;
					}
					else if (unit.Value != bill.CU_PackType)
					{
						unit = ZString.Empty;
					}
				}
			}

			if (declaration.JE_TotalNoOfPacks == 0)
			{
				declaration.JE_TotalNoOfPacks = ZInt.ParseSafe(result.ToString(0), 0);
			}

			if (declaration.JE_TotalNoOfPacksPackType.IsEmpty && declaration.JE_TotalNoOfPacks > 0)
			{
				ZString value = unit ?? ZString.Empty;

				if (value.Length > JobDeclarationSchema.JE_TotalNoOfPacksPackType.MaxLength)
				{
					notifications.AddWarning(string.Format(CultureInfo.InvariantCulture, ManifestUQTooLong, value));
					value = "??";
				}

				declaration.JE_TotalNoOfPacksPackType = value.IsEmpty ? "PK" : value.ToString();
			}
		}

		internal const string ManifestUQTooLong = "Manifest UQ, '{0}' is too long to be set and system has set '??' instead.";

		void ResetDutyFeeAmounts(JobDeclaration declaration)
		{
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			if (entry != null)
			{
				foreach (CusEntryHeaderCharges charge in entry.Charges)
				{
					charge.C1_ChargeAmount = 0m;
				}

				foreach (CusEntryLine entryLine in entry.MergedLines)
				{
					foreach (CusEntryLineFee fee in entryLine.Fees)
					{
						fee.CF_ChargeAmount = 0m;
					}
				}
			}
		}

		class BIRDData
		{
			public List<IBIRDHeaderRecord> HeaderDatas
			{
				get { return headerDatas ?? (headerDatas = new List<IBIRDHeaderRecord>()); }
			}
			List<IBIRDHeaderRecord> headerDatas;

			public Dictionary<JobComInvoiceLine, List<IACEBIRDLineRecord>> LineDatas
			{
				get { return lineDatas ?? (lineDatas = new Dictionary<JobComInvoiceLine, List<IACEBIRDLineRecord>>()); }
			}
			Dictionary<JobComInvoiceLine, List<IACEBIRDLineRecord>> lineDatas;

			public Dictionary<JobComInvoiceLine, List<PGAData>> PGADatas
			{
				get { return pgaDatas ?? (pgaDatas = new Dictionary<JobComInvoiceLine, List<PGAData>>()); }
			}
			Dictionary<JobComInvoiceLine, List<PGAData>> pgaDatas;

			public Dictionary<JobComInvoiceLine, List<IACEBIRDSecondaryLineRecord>> PrimarySecondaryLineDatas
			{
				get { return primarySecondaryLineDatas ?? (primarySecondaryLineDatas = new Dictionary<JobComInvoiceLine, List<IACEBIRDSecondaryLineRecord>>()); }
			}
			Dictionary<JobComInvoiceLine, List<IACEBIRDSecondaryLineRecord>> primarySecondaryLineDatas;

			public Dictionary<JobComInvoiceLine, List<IACEBIRDSecondaryLineRecord>> SupplementarySecondaryLineDatas
			{
				get { return supplementarySecondaryLineDatas ?? (supplementarySecondaryLineDatas = new Dictionary<JobComInvoiceLine, List<IACEBIRDSecondaryLineRecord>>()); }
			}
			Dictionary<JobComInvoiceLine, List<IACEBIRDSecondaryLineRecord>> supplementarySecondaryLineDatas;

			public Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>> HeaderOrganizationsDict
			{
				get { return headerOrganizationsDict ?? (headerOrganizationsDict = new Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>()); }
			}
			Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>> headerOrganizationsDict;

			public Dictionary<JobComInvoiceLine, Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>> LineOrganizationsDict
			{
				get { return lineOrganizationsDict ?? (lineOrganizationsDict = new Dictionary<JobComInvoiceLine, Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>>()); }
			}
			Dictionary<JobComInvoiceLine, Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>> lineOrganizationsDict;

			public Dictionary<JobComInvoiceLine, List<AENSOI>> LineWithPGAGoodsDescriptionDict
			{
				get { return fLineWithPGAGoodsDescriptionDict ?? (fLineWithPGAGoodsDescriptionDict = new Dictionary<JobComInvoiceLine, List<AENSOI>>()); }
			}
			Dictionary<JobComInvoiceLine, List<AENSOI>> fLineWithPGAGoodsDescriptionDict;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		BIRDData CreateInvoiceLinesAndGatherDataToBeProcessed(JobDeclaration declaration, InputBlockControlGenerator<BRDAABIB, BRDAABIY> generator, INotifications notifications)
		{
			Argument.NotNull(notifications, "notifications"); // TODO remove this when using as it was added to silence CodeAnalysis

			var result = new BIRDData();
			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "INV" + declaration.ImportEntryNumber;
				JobComInvoiceLine parentInvoiceLine = null;
				PGAData pgaData = null;
				MessageBlock previousBlock = null;
				IPGABlock pgaBlock = null;
				List<IPGABlock> pgaBlocksList = null;
				IACEBIRDOrgCompanyRecord previousHeaderCompanyRecord = null;
				IACEBIRDOrgCompanyRecord previousLineCompanyRecord = null;
				var unsortedTariffValueDetailRecords = new List<IACEBIRDSecondaryLineRecord>();

				foreach (MessageBlock block in generator.MessageBlocks)
				{
					pgaBlock = block as IPGABlock;
					if (pgaData != null && pgaBlock == null)
					{
						pgaData = null;
					}

					var headerRecord = block as IBIRDHeaderRecord;
					if (headerRecord != null)
					{
						result.HeaderDatas.Add(headerRecord);
						parentInvoiceLine = null;
					}
					else
					{
						var lineIDRecord = block as IACEBIRDLineIDRecord;
						if (lineIDRecord != null)
						{
							PreProcessUnsortedTariffValueDetailRecords(result, parentInvoiceLine, unsortedTariffValueDetailRecords);
							unsortedTariffValueDetailRecords.Clear();
							parentInvoiceLine = invoice.JobComInvoiceLines.AddNew();
							parentInvoiceLine.IsImportViaBIRD = true;
							AddData(result.LineDatas, parentInvoiceLine, lineIDRecord);
						}
						else
						{
							if (parentInvoiceLine != null)
							{
								var tariffValueDetailRecord = block as IACEBIRDSecondaryLineRecord;
								if (tariffValueDetailRecord != null)
								{
									unsortedTariffValueDetailRecords.Add(tariffValueDetailRecord);
								}
								else
								{
									var lineRecord = block as IACEBIRDLineRecord;
									if (lineRecord != null)
									{
										AddData(result.LineDatas, parentInvoiceLine, lineRecord);
									}
									else
									{
										if (pgaBlock != null)
										{
											var pg01 = block as AEPAPG01;
											if (pg01 != null)
											{
												if (pgaData == null || !(previousBlock is AEPAPG01))
												{
													pgaData = new PGAData();
													pgaBlocksList = new List<IPGABlock>();
													AddData(result.PGADatas, parentInvoiceLine, pgaData);
												}
												pgaData.PGADataLines.Add(pg01, pgaBlocksList);
											}
											else if (pgaData != null)
											{
												pgaBlocksList.Add(pgaBlock);
											}
										}
									}

									var sese50Block = block as ASESE50;
									if (sese50Block != null)
									{
										previousLineCompanyRecord = sese50Block;
										var lineOrganizationsDict = new Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>();
										if (!result.LineOrganizationsDict.ContainsKey(parentInvoiceLine))
										{
											result.LineOrganizationsDict.Add(parentInvoiceLine, lineOrganizationsDict);
										}
										else
										{
											lineOrganizationsDict = result.LineOrganizationsDict[parentInvoiceLine];
										}

										if (!lineOrganizationsDict.ContainsKey(sese50Block))
										{
											lineOrganizationsDict.Add(sese50Block, null);
										}
									}
									else
									{
										var sese55Block = block as ASESE55;
										if (sese55Block != null)
										{
											if (previousLineCompanyRecord != null)
											{
												var lineOrganizationsDict = result.LineOrganizationsDict[parentInvoiceLine];
												if (lineOrganizationsDict.ContainsKey(previousLineCompanyRecord))
												{
													var value = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(sese55Block, sese55Block, null);
													lineOrganizationsDict[previousLineCompanyRecord] = value;
												}
											}
										}
										else
										{
											var sese56Block = block as ASESE56;
											if (sese56Block != null)
											{
												if (previousLineCompanyRecord != null)
												{
													var lineOrganizationsDict = result.LineOrganizationsDict[parentInvoiceLine];
													if (lineOrganizationsDict.ContainsKey(previousLineCompanyRecord))
													{
														var tupleValue = lineOrganizationsDict[previousLineCompanyRecord];
														if (tupleValue == null && !previousLineCompanyRecord.OrganizationType.IsEmpty && !previousLineCompanyRecord.CompanyName.IsEmpty)
														{
															notifications.AddWarning(ZString.Format("SE55 record is missing when entity code is '{0}' and entity name is '{1}'.", previousLineCompanyRecord.OrganizationType, previousLineCompanyRecord.CompanyName));
															lineOrganizationsDict[previousLineCompanyRecord] = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(null, null, sese56Block);
														}
														else
														{
															var value = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(tupleValue.Item1, tupleValue.Item2, sese56Block);
															lineOrganizationsDict[previousLineCompanyRecord] = value;
														}
													}
												}

												previousLineCompanyRecord = null;
											}
										}
									}
								}
							}
						}

						var sese30Block = block as ASESE30;
						if (sese30Block != null)
						{
							previousHeaderCompanyRecord = sese30Block;
							result.HeaderOrganizationsDict.Add(sese30Block, null);
						}
						else
						{
							var sese35Block = block as ASESE35;
							if (sese35Block != null)
							{
								if (previousHeaderCompanyRecord != null)
								{
									var value = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(sese35Block, sese35Block, null);
									result.HeaderOrganizationsDict[previousHeaderCompanyRecord] = value;
								}
							}
							else
							{
								var sese36Block = block as ASESE36;
								if (sese36Block != null)
								{
									if (previousHeaderCompanyRecord != null)
									{
										var tupleValue = result.HeaderOrganizationsDict[previousHeaderCompanyRecord];
										var value = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(tupleValue.Item1, tupleValue.Item2, sese36Block);
										result.HeaderOrganizationsDict[previousHeaderCompanyRecord] = value;
										previousHeaderCompanyRecord = null;
									}
								}
								else
								{
									var ensOIBlock = block as AENSOI;
									if (ensOIBlock != null)
									{
										AddData(result.LineWithPGAGoodsDescriptionDict, parentInvoiceLine, ensOIBlock);
									}
								}
							}
						}
					}
				}

				PreProcessUnsortedTariffValueDetailRecords(result, parentInvoiceLine, unsortedTariffValueDetailRecords);
			}

			return result;
		}

		void PreProcessUnsortedTariffValueDetailRecords(BIRDData bIRDData, JobComInvoiceLine parentInvoiceLine, List<IACEBIRDSecondaryLineRecord> unsortedTariffValueDetailsLines)
		{
			if (parentInvoiceLine != null && unsortedTariffValueDetailsLines.Count > 0)
			{
				var sortedTariffValueDetailsLines = new List<IACEBIRDSecondaryLineRecord>();
				var tariff98ValueDetailsLines = unsortedTariffValueDetailsLines.Where(x => Chapter98Helper.Is98Tariff(x.Tariff)).ToArray();
				var tariff99ValueDetailsLines = unsortedTariffValueDetailsLines.Where(x => Chapter98Helper.Is99Tariff(x.Tariff)).ToArray();
				var regularTariffValueDetailsLines = unsortedTariffValueDetailsLines.Where(x => !x.IsSupplementaryTariff()).ToArray();

				if (tariff98ValueDetailsLines.Length <= 1 && tariff99ValueDetailsLines.Length <= 1 && regularTariffValueDetailsLines.Length == 1)
				{
					sortedTariffValueDetailsLines.AddRange(tariff98ValueDetailsLines);
					sortedTariffValueDetailsLines.AddRange(tariff99ValueDetailsLines);
					sortedTariffValueDetailsLines.AddRange(regularTariffValueDetailsLines);
				}
				else
				{
					sortedTariffValueDetailsLines = unsortedTariffValueDetailsLines;
				}

				var linePriceFrom99Tariff = ZDecimal.Zero;
				JobComInvoiceLine invoiceLine = null;
				foreach (var tariffValueDetail in sortedTariffValueDetailsLines)
				{
					var tariff = tariffValueDetail.Tariff;
					var isSupplementaryTariff = IBIRDLineRecordHelper.IsSupplementaryTariff(tariff);
					if (invoiceLine == null)
					{
						invoiceLine = parentInvoiceLine;
					}
					else if (!invoiceLine.JI_Tariff.IsEmpty || (isSupplementaryTariff && !invoiceLine.HasEmptySupTariff))
					{
						invoiceLine = parentInvoiceLine.AddSecondaryInvoiceLine();
						invoiceLine.JI_Tariff = ZString.Empty;
						invoiceLine.US_SupTariff = ZString.Empty;
					}
					SetTariff(invoiceLine, isSupplementaryTariff, tariff);
					AddData(isSupplementaryTariff ? bIRDData.SupplementarySecondaryLineDatas : bIRDData.PrimarySecondaryLineDatas, invoiceLine, tariffValueDetail);
				}
			}
		}

		void AddData<T>(Dictionary<JobComInvoiceLine, List<T>> dictionary, JobComInvoiceLine invoiceLine, T lineRecord)
		{
			List<T> lineDatas;
			if (!dictionary.TryGetValue(invoiceLine, out lineDatas))
			{
				lineDatas = new List<T>();
				dictionary.Add(invoiceLine, lineDatas);
			}
			lineDatas.Add(lineRecord);
		}

		void SetTariff(JobComInvoiceLine invoiceLine, bool isSupplementaryTariff, ZString tariff)
		{
			if (isSupplementaryTariff)
			{
				invoiceLine.US_SupTariff = tariff;
			}
			else
			{
				invoiceLine.JI_Tariff = tariff;
			}
		}

		void ProcessMessageBlocks(JobDeclaration declaration, CusEntryHeader entry, BIRDData birdData, INotifications notifications)
		{
			ProcessHeaderData(declaration, birdData.HeaderDatas, notifications);
			ProcessHeaderOrganizations(declaration, birdData.HeaderOrganizationsDict, notifications);
			ProcessLineOrganizations(declaration, birdData.LineOrganizationsDict, notifications);
			ProcessLineData(entry, birdData.LineDatas, notifications);
			ProcessPGAData(birdData.PGADatas, notifications);
			ProcessPrimarySecondaryLineData(birdData.PrimarySecondaryLineDatas, notifications);
			ProcessSupplementarySecondaryLineData(birdData.SupplementarySecondaryLineDatas, notifications);
			ProcessPGAGoodsDescriptionData(birdData.LineWithPGAGoodsDescriptionDict);
		}

		void ProcessPGAGoodsDescriptionData(Dictionary<JobComInvoiceLine, List<AENSOI>> lineWithPGAGoodsDescriptionDict)
		{
			foreach (var lineWithPGAGoodsDescription in lineWithPGAGoodsDescriptionDict)
			{
				var invoiceLine = lineWithPGAGoodsDescription.Key;
				var firstOIWithDescription = lineWithPGAGoodsDescription.Value.FirstOrDefault();

				if (invoiceLine != null && firstOIWithDescription != null)
				{
					var regularInvoiceLine = invoiceLine;
					if (invoiceLine.IsCombinedLine() && invoiceLine.ChildLines.FirstOrDefault(x => x.IsNormalTariffLine()) is JobComInvoiceLine regularLine)
					{
						regularInvoiceLine = regularLine;
					}

					regularInvoiceLine.JI_Description = firstOIWithDescription.CommercialDescriptionText;
				}
			}
		}

		void SetCottonFeeExemptIndicator(CusEntryHeader entry, JobComInvoiceLine invoiceLine, IChargeBlock cottonFeeRecord, AENS40 a40Record)
		{
			if (invoiceLine != null && entry.IsFormalEntry && invoiceLine.CusEntryLine != null)
			{
				SetCottonFeeExemptIndicator(invoiceLine, cottonFeeRecord, a40Record);

				if (invoiceLine.HasSecondaryTariffLines)
				{
					foreach (var secondaryLine in invoiceLine.SecondaryTariffLines)
					{
						SetCottonFeeExemptIndicator(secondaryLine, cottonFeeRecord, a40Record);
					}
				}
			}
		}

		void SetCottonFeeExemptIndicator(JobComInvoiceLine invoiceLine, IChargeBlock cottonFeeRecord, AENS40 a40Record)
		{
			if (invoiceLine.ShouldCottonFeeExemptBeIndicated() || invoiceLine.HasCottonCertificate)
			{
				if (a40Record != null && a40Record.FeeExemptionCode == "1" && (cottonFeeRecord == null || cottonFeeRecord.UserFeeAmount == 0m))
				{
					invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
				}
				else
				{
					invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
				}
			}
		}

		void ProcessPGAData(Dictionary<JobComInvoiceLine, List<PGAData>> pgaDatas, INotifications notifications)
		{
			foreach (var pgaDataPair in pgaDatas)
			{
				new ACEBIRDPGAAdapter().DoImport(pgaDataPair.Key, pgaDataPair.Value, notifications);
			}
		}

		void ProcessSupplementarySecondaryLineData(Dictionary<JobComInvoiceLine, List<IACEBIRDSecondaryLineRecord>> supplementarySecondaryLineDatas, INotifications notifications)
		{
			foreach (var supplementarySecondaryLineDataPair in supplementarySecondaryLineDatas)
			{
				if (!supplementarySecondaryLineDataPair.Key.Is98SecondaryOrParentTariffLine)
				{
					foreach (var supplementarySecondaryLineData in supplementarySecondaryLineDataPair.Value)
					{
						supplementarySecondaryLineData.Update(supplementarySecondaryLineDataPair.Key, true, notifications);
					}
				}
			}
		}

		void ProcessPrimarySecondaryLineData(Dictionary<JobComInvoiceLine, List<IACEBIRDSecondaryLineRecord>> primarySecondaryLineDatas, INotifications notifications)
		{
			foreach (var primarySecondaryLineDataPair in primarySecondaryLineDatas)
			{
				foreach (var primarySecondaryLineData in primarySecondaryLineDataPair.Value)
				{
					primarySecondaryLineData.Update(primarySecondaryLineDataPair.Key, false, notifications);
				}
			}
		}

		void ProcessLineData(CusEntryHeader entry, Dictionary<JobComInvoiceLine, List<IACEBIRDLineRecord>> lineDatas, INotifications notifications)
		{
			foreach (var lineDataPair in lineDatas)
			{
				var invoiceLine = lineDataPair.Key;
				IChargeBlock cottonFeeRecord = null;
				AENS40 a40Record = null;
				foreach (var lineData in lineDataPair.Value)
				{
					var aens40 = lineData as AENS40;
					if (aens40 != null)
					{
						a40Record = aens40;
					}
					else
					{
						var aens54 = lineData as AENS54;
						if (aens54 != null
							&& (aens54.ImportersAdditionalDeclarationTypeCode == AdditionalDeclarationTypeCodeList.Codes._02
								|| aens54.ImportersAdditionalDeclarationTypeCode == AdditionalDeclarationTypeCodeList.Codes._03))
						{
							invoiceLine.US_ProductExclusion = aens54.ImportersAdditionalDeclarationTypeCode;
							invoiceLine.US_ExclusionNumber = aens54.ImportersAdditionalDeclarationInformation;
						}
						else
						{
							var chargeBlock = lineData as IChargeBlock;
							if (chargeBlock != null)
							{
								if (chargeBlock.AccountingClassCode == Core.Constants.USCustoms.FeeCodes.Cotton)
								{
									cottonFeeRecord = chargeBlock;
								}
							}
						}
					}
					lineData.Update(invoiceLine, notifications);
				}
				SetCottonFeeExemptIndicator(entry, invoiceLine, cottonFeeRecord, a40Record);
			}
		}

		void ProcessLineOrganizations(JobDeclaration declaration, Dictionary<JobComInvoiceLine, Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>> lineOrganizations, INotifications notifications)
		{
			foreach (var invoiceLineOrganizationDict in lineOrganizations)
			{
				var invoiceLine = invoiceLineOrganizationDict.Key;
				foreach (var lineOrganization in invoiceLineOrganizationDict.Value)
				{
					var organizationKey = lineOrganization.Key;
					var organizationValue = lineOrganization.Value;
					if (organizationKey != null && organizationValue != null)
					{
						(ZString addressLine1, ZString addressLine2, ZString city, ZString countryCode, ZString postCode) = GetOrganizationDetail(organizationValue);
						SetOrganizationAddress(declaration, invoiceLine, organizationKey.OrganizationType, organizationKey.CustomsNoType, organizationKey.CustomsNumber, organizationKey.CompanyName, addressLine1, addressLine2, city, countryCode, postCode, notifications);
					}
				}
			}
		}

		void ProcessHeaderData(JobDeclaration declaration, List<IBIRDHeaderRecord> headerDatas, INotifications notifications)
		{
			foreach (var headerData in headerDatas)
			{
				headerData.Update(declaration, notifications);
			}
		}

		(ZString addressLine1, ZString addressLine2, ZString city, ZString countryCode, ZString postCode) GetOrganizationDetail(Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord> organizationData)
		{
			var addressLine1 = ZString.Empty;
			var addressLine2 = ZString.Empty;
			var countryCode = ZString.Empty;
			var city = ZString.Empty;
			var postCode = ZString.Empty;
			if (organizationData != null)
			{
				addressLine1 = organizationData.Item1?.Address1 ?? ZString.Empty;
				addressLine2 = organizationData.Item2?.Address2 ?? ZString.Empty;
				var item3 = organizationData.Item3;
				if (item3 != null)
				{
					countryCode = item3.Country;
					city = item3.City;
					postCode = item3.ZipCode;
				}
			}

			return (addressLine1, addressLine2, city, countryCode, postCode);
		}

		void ProcessHeaderOrganizations(JobDeclaration declaration, Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>> headerOrganizations, INotifications notifications)
		{
			foreach (var headerOrganization in headerOrganizations)
			{
				(ZString addressLine1, ZString addressLine2, ZString city, ZString countryCode, ZString postCode) = GetOrganizationDetail(headerOrganization.Value);
				SetOrganizationAddress(declaration, null, headerOrganization.Key.OrganizationType, headerOrganization.Key.CustomsNoType, headerOrganization.Key.CustomsNumber, headerOrganization.Key.CompanyName, addressLine1, addressLine2, city, countryCode, postCode, notifications);
			}
		}

		void SetOrganizationAddress(JobDeclaration declaration, JobComInvoiceLine invoiceLine, ZString organizationType, ZString customsNoType, ZString customsNumber, ZString companyName, ZString address1, ZString address2, ZString city, ZString country, ZString postCode, INotifications notifications)
		{
			var isDeclarationData = invoiceLine == null;
			var factory = isDeclarationData ? declaration.Factory : invoiceLine.Factory;
			ZPropertyInfo orgAddressPropertyInfo = null;
			ZString organizationCode = ZString.Empty;

			switch (organizationType)
			{
				case EntityCodeList.Codes.SellingParty:
					organizationCode = "Seller";
					orgAddressPropertyInfo = isDeclarationData ? declaration.JE_OA_SellerAddressInfo : invoiceLine.JI_OA_SellerInfo;
					break;
				case EntityCodeList.Codes.ManufacturerSupplier:
					organizationCode = "Manufacturer";
					orgAddressPropertyInfo = isDeclarationData ? declaration.JE_OA_ManufacturerAddressInfo : invoiceLine.JI_OA_ManufacturerAddressInfo;
					break;
				case EntityCodeList.Codes.Consignee:
					organizationCode = "Consignee";
					orgAddressPropertyInfo = isDeclarationData ? declaration.JE_OA_ConsigneeAddressInfo : invoiceLine.JI_OA_ConsigneeAddressInfo;
					break;
				case EntityCodeList.Codes.BuyingParty:
					organizationCode = "Sold To Party";
					orgAddressPropertyInfo = isDeclarationData ? declaration.JE_OA_SoldToPartyAddressInfo : invoiceLine.JI_OA_SoldToPartyAddressInfo;
					break;
			}

			if (!organizationCode.IsEmpty && orgAddressPropertyInfo != null)
			{
				var address = BIRDOrganisationMatching.FindMatchedOrgAddress(factory, organizationCode, customsNoType, customsNumber, companyName, address1, address2, country, city, postCode, isDeclarationData ? "Invoice" : "Invoice Line", notifications);
				if (address != null)
				{
					var existingAddressPK = orgAddressPropertyInfo.Value;
					if (existingAddressPK.IsEmpty)
					{
						orgAddressPropertyInfo.Value = address.PK;
					}
					else if (!existingAddressPK.Equals(address.PK))
					{
						var existingAddress = factory.Load<OrgAddress>((ZGuid)existingAddressPK);
						var existingOrgHeader = factory.Load<OrgHeader>(existingAddress.OA_OH);
						if (existingOrgHeader != null && address.Header is OrgHeader matchedOrgHeader)
						{
							notifications.AddWarning(ZString.Format(OrganizationHasBeenSetWarning, organizationCode, existingOrgHeader.OH_Code, matchedOrgHeader.OH_Code));
						}
					}
				}
			}
		}
		internal const string OrganizationHasBeenSetWarning = "{0} for this line has been already set to {1}, but another organization {2} is found to be set to the field. Please check {0}.";

		void Merge(JobDeclaration declaration, INotifications notifications)
		{
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			if (declaration.IsImport && !declaration.US_EntryFilerCode.IsEmpty && !declaration.IsImportByExternalBroker && !declaration.IsFTZAdmission
				&& (declaration.US_EnableENS || declaration.US_EnableCRL) && declaration.ImportEntryNumber.IsEmpty)
			{
				declaration.ReloadImportEntryNumber();
				if (declaration.ImportEntryNumber.IsEmpty && !declaration.LockImportEntryNumberAllocationMutex)
				{
					notifications.AddWarning(CannotMergeImportEntryNumberAllocationInProgress(declaration.GetImportEntryNumberAllocationMutexLockInfo(), declaration.JE_DeclarationReference));
					return;
				}
			}
			Customs.Business.SendsMessagesToCustomsReturningResultsAsProperties mergeNotifier = new Customs.Business.SendsMessagesToCustomsReturningResultsAsProperties(true);
			declaration.DoMerge(mergeNotifier);

			string mergeWarning = mergeNotifier.MergeResult;
			if (!string.IsNullOrEmpty(mergeWarning))
			{
				notifications.AddWarning(mergeWarning);
			}
		}

		public static string CannotMergeImportEntryNumberAllocationInProgress(string lockInfo, string jobNumber)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} is in the process of allocating Import Entry Number for Job ('{1}'); system cannot merge the data as it will result in a different Import Entry Number being allocated.\r\nPlease retry merging when the other user has finished.", lockInfo, jobNumber);
		}
	}

	public class PGAData
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<AEPAPG01, List<IPGABlock>> PGADataLines
		{
			get { return fPGADataLines ?? (fPGADataLines = new Dictionary<AEPAPG01, List<IPGABlock>>()); }
		}
		Dictionary<AEPAPG01, List<IPGABlock>> fPGADataLines;
	}
}
