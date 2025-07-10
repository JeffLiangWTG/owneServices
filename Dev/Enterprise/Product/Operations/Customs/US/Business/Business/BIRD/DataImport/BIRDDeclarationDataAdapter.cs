using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// This class is responsible for creating and updating declarations for BIRD 7501/3461
	/// </summary>
	public class BIRDDeclarationDataAdapter
	{
		public void DoImport(JobDeclaration declaration, InputBlockControlGenerator<BRDAA, BRDZZ> generator, INotifications notifications)
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

			List<MessageBlock> messageBlocks = generator.MessageBlocks;
			BRDAA aa = generator.B;
			string applicationID = aa.ApplicationCode;

			SetDeclarationFields(declaration, applicationID, generator, notifications);

			CreateInvoiceLines(declaration, messageBlocks, notifications);

			Merge(declaration, notifications);

			CusEntryHeader entry = applicationID == BIRDApplicationCodeList.Codes.EntrySummary ? declaration.ActiveEntryHeaders.EntrySummaryEntry : declaration.ActiveEntryHeaders.CargoReleaseEntry;

			if (entry != null)
			{
				using (declaration.SuspendDefaultingSecondaryTariffLines())
				using (declaration.SuspendDefaultingMessageMode())
				{
					CreateUltimateConsigneeIfRequiredAndPossible(declaration, messageBlocks, notifications);

					declaration.IsImportingData = true;

					try
					{
						ProcessMessageBlocks(declaration, entry, messageBlocks, notifications);
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

		void SetDeclarationFields(JobDeclaration declaration, string applicationID, InputBlockControlGenerator<BRDAA, BRDZZ> generator, INotifications notifications)
		{
			BRDAA aa = generator.B;

			if (!declaration.IsInDatabase)
			{
				SetMessageTypeOfIfNewDeclaration(declaration, generator);

				declaration.US_BRDRefNo = aa.OriginatingBrokerRef;
			}

			declaration.Bills.RemoveAndDeleteAll();
			declaration.Invoices.DeleteAll();

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			if (applicationID == BIRDApplicationCodeList.Codes.EntrySummary)
			{
				declaration.US_EnableENS = true;

				if (declaration.IsImportByExternalBroker)
				{
					declaration.US_NoDutyCalc = true;
				}

				ResetDutyFeeAmounts(declaration);
			}
			else if (applicationID == BIRDApplicationCodeList.Codes.CargoRelease)
			{
				declaration.US_EnableCRL = true;

				List<MessageBlock> messageBlocks = generator.MessageBlocks;

				//Whether it is Border Cargo Release or Cargo Release, transport mode should be set to produce a right type CusEntryHeader
				declaration.JE_TransportMode = messageBlocks.OfType<BCR01>().Any()
											 ? TransportTypeList.Codes.Truck
											 : TransportTypeList.Codes.Sea;
			}

			if (declaration.US_EnableENS)
			{
				//This is updated during ENS40 update and having an existing value before line update causes a wrong warning.
				declaration.US_SchDLoading = ZString.Empty;
				declaration.JE_ExportDate = ZDateTime.Empty;
			}

			SetExternalBroker(declaration, aa, notifications);
		}

		void SetExternalBroker(JobDeclaration declaration, BRDAA aa, INotifications notifications)
		{
			ZString entryFilerCode = aa.SendersFilerCode;

			var currentCompanyFiler = USCustomsDataRegistry.Instance.EntryFiler.Value;
			ZString currencyCompanyFilerCode = currentCompanyFiler != null ? currentCompanyFiler.EntryFilerCode : ZString.Empty;

			if (!entryFilerCode.IsEmpty && entryFilerCode != currencyCompanyFilerCode)
			{
				OrgHeader[] organisations = new OrgHeader.Loader(declaration.Factory).LoadDBOrganisations(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.EntryFilerCode, entryFilerCode);

				if (organisations.Length == 1)
				{
					declaration.JE_OH_ExternalBroker = organisations[0].PK;
				}

				if (organisations.Length == 0)
				{
					notifications.AddWarning(string.Format(NoOrganisationWithSpecifiedEntryFilerCode, entryFilerCode));
				}
				else if (organisations.Length > 1)
				{
					notifications.AddWarning(string.Format(MultipleOrganisationsWithSpecifiedEntryFilerCode, entryFilerCode));
				}
			}
		}

		public const string NoOrganisationWithSpecifiedEntryFilerCode = "There is no organization with this entry filer code, '{0}'. The system cannot communicate with this filer automatically without this field in declarations. Please create an organization and specify its entry filer code in Organization > Config > Customs Codes.";
		public const string MultipleOrganisationsWithSpecifiedEntryFilerCode = "There are multiple organizations with this entry filer code, '{0}'. The system cannot determine which organization to use. Please consider merging the orgazation records.";

		void OnProcessed(JobDeclaration declaration, InputBlockControlGenerator<BRDAA, BRDZZ> generator, INotifications notifications, string applicationID)
		{
			UpdateInvoiceDetails(declaration, generator.MessageBlocks);

			CalculateTotalPackage(declaration, notifications);

			declaration.ResumeApportionment();

			Merge(declaration, notifications);

			CreateMessage(declaration, generator, applicationID);
		}

		void SetMessageTypeOfIfNewDeclaration(JobDeclaration declaration, InputBlockControlGenerator<BRDAA, BRDZZ> generator)
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
			}
		}

		void CreateMessage(JobDeclaration declaration, InputBlockControlGenerator<BRDAA, BRDZZ> generator, ZString applicationID)
		{
			CusEntryHeader entry = applicationID == BIRDApplicationCodeList.Codes.CargoRelease ? declaration.ActiveEntryHeaders.CargoReleaseEntry : declaration.ActiveEntryHeaders.EntrySummaryEntry;

			MQEDIMessage message = generator.CreateMessage<MQEDIMessage>(declaration.Factory);
			message.EM_LinkedObject = entry ?? (BusinessObject)declaration;
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_Status = MQEDIMessage.Status.Received;
			message.EM_MessageSubType = applicationID == BIRDApplicationCodeList.Codes.CargoRelease ? EM_MessageSubTypeList.Codes.BIRDCargoRelease : EM_MessageSubTypeList.Codes.BIRDEntrySummary;
		}

		void UpdateInvoiceDetails(JobDeclaration declaration, List<MessageBlock> messageBlocks)
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

				if (invoice.JZ_InvoiceNumber.IsEmpty)
				{
					invoice.JZ_InvoiceNumber = "INV" + invoice.JZ_InvoiceDisplaySequence.ToString().PadLeft(3, '0');
				}
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
					notifications.AddWarning(string.Format(ManifestUQTooLong, value));
					value = "??";
				}

				declaration.JE_TotalNoOfPacksPackType = value.IsEmpty ? "PK" : value.ToString();
			}
		}

		internal const string ManifestUQTooLong = "Manifest UQ, '{0}' is too long to be set and system has set '??' instead.";

		void ResetDutyFeeAmounts(JobDeclaration declaration)
		{
			CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

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

		void CreateInvoiceLines(JobDeclaration declaration, List<MessageBlock> messageBlocks, INotifications notifications)
		{
			JobComInvoiceLine invoiceLine = null;

			ZShort currentInvIndex = 1;
			bool createSecondaryInvoiceLine = true;

			foreach (MessageBlock block in messageBlocks)
			{
				IBIRDLineRecord lineRecord = block as IBIRDLineRecord;

				if (lineRecord != null)
				{
					//ENS40
					IBIRDLineIDRecord lineIDRecord = block as IBIRDLineIDRecord;

					if (lineIDRecord != null)
					{
						createSecondaryInvoiceLine = true;
						ZShort invSeq = (ZShort)lineIDRecord.DelimiterInvSequence;

						if (invSeq == 0)
						{
							invSeq = currentInvIndex;
						}

						invoiceLine = CreateInvoiceLine(declaration, invSeq);

						if (lineIDRecord.DelimiterInvSequence != 0)
						{
							currentInvIndex++;
						}
					}

					lineRecord.SetTariffs(invoiceLine);

					if (lineRecord.IsSupplementaryTariff())
					{
						createSecondaryInvoiceLine = false;
					}
				}
				else
				{
					IBIRDSecondaryLineRecord secondaryLineRecord = block as IBIRDSecondaryLineRecord;

					bool shouldCreateSecondaryLine = secondaryLineRecord != null && invoiceLine != null &&
											(createSecondaryInvoiceLine || IsRepairTariff(secondaryLineRecord.Tariff));

					if (shouldCreateSecondaryLine)
					{
						JobComInvoiceLine secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
						secondaryLineRecord.SetTariffs(secondaryLine);
					}
					else if (secondaryLineRecord != null)
					{
						secondaryLineRecord.SetTariffs(invoiceLine);
					}
				}
			}
		}

		bool IsRepairTariff(ZString tariffNumber)
		{
			return tariffNumber.StartsWith("9802");
		}

		void CreateUltimateConsigneeIfRequiredAndPossible(JobDeclaration declaration, List<MessageBlock> messageBlocks, INotifications notifications)
		{
			var ens10 = messageBlocks.OfType<ENS10>().FirstOrDefault();

			if (ens10 != null)
			{
				var ultimateConsignee = BIRDOrganisationMatching.GetOrganisation(declaration.Factory, OrgMatchedCustomsRegNoType.EIN, ens10.UltimateConsigneeNumber);

				if (ultimateConsignee == null)
				{
					var za = messageBlocks.OfType<BRDZA>().FirstOrDefault();
					var zb = messageBlocks.OfType<BRDZB>().FirstOrDefault();

					if (za != null && zb != null)
					{
						OrganisationCreator.CreateUltimateConsignee(declaration.Factory, za.ConsigneeName, za.ConsigneeAddress1, zb.ConsigneeAddress2, zb.ConsigneeCity, zb.ConsigneeState, zb.ConsigneePostalCode, "US", ens10.UltimateConsigneeNumber);

						notifications.AddWarning(string.Format(OrganisationCreator.OrganisationCreated, ens10.UltimateConsigneeNumber, za.ConsigneeName));
					}
				}
			}
		}

		void ProcessMessageBlocks(JobDeclaration declaration, CusEntryHeader entry, List<MessageBlock> messageBlocks, INotifications notifications)
		{
			CusEntryLine currentNonSecondaryLine = null;
			JobComInvoiceLine invoiceLine = null;
			int numberOfSecondaryLine = 0;
			ZInt numberOfLine = 0;
			IBIRDOGAStartRecord ogaStart = null;

			List<MessageBlock> ogas = new List<MessageBlock>();
			ENS51 ens51Record = null;
			ENS62 cottoFeeRecord = null;

			foreach (MessageBlock block in messageBlocks)
			{
				IBIRDHeaderRecord headerRecord = block as IBIRDHeaderRecord;

				if (headerRecord != null)
				{
					headerRecord.Update(declaration, notifications);
				}
				else
				{
					IBIRDLineRecord lineRecord = block as IBIRDLineRecord;

					if (lineRecord != null)
					{
						ProcessOGABlocksIfNecessary(invoiceLine, ogas, notifications);
						ogaStart = null;

						//ENS40, CRL05, BCR02
						IBIRDLineIDRecord lineIDRecord = block as IBIRDLineIDRecord;

						if (lineIDRecord != null)
						{
							SetCottonFeeExemptIndicator(entry, invoiceLine, cottoFeeRecord, ens51Record);
							cottoFeeRecord = null;
							ens51Record = null;

							numberOfSecondaryLine = 0;
							numberOfLine++;
							ZInt lineNumber = lineIDRecord.LineNumber > 0 ? lineIDRecord.LineNumber : numberOfLine;
							currentNonSecondaryLine = entry.MergedLines.FindNonSecondaryLineByLineNumber(lineNumber);
							invoiceLine = currentNonSecondaryLine.RandomLine;
						}

						if (invoiceLine != null)
						{
							lineRecord.Update(invoiceLine, notifications);
						}

						var recordAs51 = block as ENS51;
						if (recordAs51 != null)
						{
							ens51Record = recordAs51;
						}
						else
						{
							var recordAs62 = block as ENS62;
							if (recordAs62 != null && recordAs62.ClassCode == Core.Constants.USCustoms.FeeCodes.Cotton)
							{
								cottoFeeRecord = recordAs62;
							}
						}
					}
					else
					{
						IBIRDSecondaryLineRecord secondaryLineRecord = block as IBIRDSecondaryLineRecord;

						if (secondaryLineRecord != null)
						{
							ProcessOGABlocksIfNecessary(invoiceLine, ogas, notifications);
							SetCottonFeeExemptIndicator(entry, invoiceLine, cottoFeeRecord, ens51Record);

							ogaStart = null;

							CusEntryLine currentSecondaryLine =
								currentNonSecondaryLine.ChildLines.Count > numberOfSecondaryLine ?
								currentNonSecondaryLine.ChildLines[numberOfSecondaryLine] : null;

							if (currentSecondaryLine != null)
							{
								numberOfSecondaryLine++;

								invoiceLine = currentSecondaryLine.RandomLine;

								if (invoiceLine != null)
								{
									secondaryLineRecord.Update(invoiceLine, notifications);
								}
							}
						}
					}

					IBIRDOGAStartRecord ogaStartline = block as IBIRDOGAStartRecord;

					if (ogaStartline != null)
					{
						ProcessOGABlocksIfNecessary(invoiceLine, ogas, notifications);

						ogaStart = ogaStartline;
					}

					//collecting OGA blocks until we see non-OGA blocks
					if (ogaStart != null)
					{
						ogas.Add(block);
					}
				}
			}

			ProcessOGABlocksIfNecessary(invoiceLine, ogas, notifications);
			SetCottonFeeExemptIndicator(entry, invoiceLine, cottoFeeRecord, ens51Record);
		}

		void SetCottonFeeExemptIndicator(CusEntryHeader entry, JobComInvoiceLine invoiceLine, ENS62 cottonFeeRecord, ENS51 ens51Record)
		{
			if (invoiceLine != null && entry.IsFormalEntry && invoiceLine.CusEntryLine != null)
			{
				if (invoiceLine.ShouldCottonFeeExemptBeIndicated() || invoiceLine.HasCottonCertificate)
				{
					if (cottonFeeRecord == null || cottonFeeRecord.UserFeeAmount == 0m)
					{
						if (ens51Record != null && ens51Record.CottonCertificateNumberOrganicExemptionCertificateNumber == CottonFeeCalculator.ExemptCottonFeeCertificate)
						{
							invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
						}
						else
						{
							invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
						}
					}
					else if (cottonFeeRecord.TariffNumber == invoiceLine.JI_Tariff)
					{
						var parentLine = invoiceLine.ParentTariffLine ?? invoiceLine;

						if (IsUniqueTariffLine(cottonFeeRecord.TariffNumber, parentLine))
						{
							invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
						}
					}
				}
			}
		}

		static bool IsUniqueTariffLine(ZString tariffNumber, JobComInvoiceLine parentLine)
		{
			var lines = Enumerable.Empty<JobComInvoiceLine>();

			lines = lines.Concat(new JobComInvoiceLine[] { parentLine });
			lines = lines.Concat(parentLine.SecondaryTariffLines);

			return lines.Where(x => x.JI_Tariff == tariffNumber).Take(2).Count() == 1;
		}

		void ProcessOGABlocksIfNecessary(JobComInvoiceLine invoiceLine, List<MessageBlock> ogaBlocks, INotifications notifications)
		{
			if (invoiceLine != null && ogaBlocks.Count > 0)
			{
				new BIRDOGAAdapter().DoImport(invoiceLine, ogaBlocks, notifications);

				ogaBlocks.Clear();
			}
		}

		JobComInvoiceLine CreateInvoiceLine(JobDeclaration declaration, ZShort invSequenceNo)
		{
			JobComInvoiceHeader invoice = (JobComInvoiceHeader)declaration.Invoices.Find(invSequenceNo);

			if (invoice == null)
			{
				using (declaration.GetInvoiceNumberRenumberingSuspender())
				{
					invoice = declaration.Invoices.AddNew();

					invoice.JZ_InvoiceDisplaySequence = invSequenceNo;
				}
			}

			return invoice.JobComInvoiceLines.AddNew();
		}

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
			return string.Format("{0} is in the process of allocating Import Entry Number for Job ('{1}'); system cannot merge the data as it will result in a different Import Entry Number being allocated.\r\nPlease retry merging when the other user has finished.", lockInfo, jobNumber);
		}
	}
}
