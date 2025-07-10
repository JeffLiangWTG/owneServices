using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business.MessageProcessors
{
	public abstract class BondedWarehouseMessageProcessor
	{
		protected BondedWarehouseMessageProcessor(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed)
		{
			factory = new BusinessObjectFactory();
			this.message = Argument.NotNull(factory.Load<EDIMessage>(messagePK), "messagePK is missing or invalid");
			supporter = GetSupporter();
			this.emailReportThatHasBeenDelayed = emailReportThatHasBeenDelayed;
		}

		public void ProcessAfterSaved(bool savedSuccessfully)
		{
			using (factory.AddDisposableService())
			{
				if (savedSuccessfully)
				{
					if (supporter != null)
					{
						var warehouseTransactionStatus = supporter.WarehouseTransactionStatus;

						if (supporter.IsChangeOfRegimeWarehousingEnabled)
						{
							if (WarehouseTransactionStatusList.IsPendingChangeOfRegime(warehouseTransactionStatus))
							{
								if (HasBeenWithdrawn || IsOriginalError)
								{
									PublishUniversalCancelEventToInventoryChangeOfRegime();
								}
								else if (IsAmendmentError || IsWithdrawalError)
								{
									PublishUniversalShipmentToInventoryChangeOfRegimeFromLatestClearedData();
								}
								else if (IsAmendmentClear)
								{
									PublishUniversalShipmentToInventoryChangeOfRegimeFromLastHoldOrLatestData();
								}
								else
								{
									PublishUniversalShipmentToInventoryChangeOfRegime();
								}
							}
							else
							{
								ReportInvalidState(ChangeOfRegime, supporter.EntryNumber, warehouseTransactionStatus);
							}
						}
						else if (supporter.IsChangeOfOwnershipBondedWarehousingEnabled)
						{
							if (WarehouseTransactionStatusList.IsPendingChangeOfOwnership(warehouseTransactionStatus))
							{
								if (HasBeenWithdrawn || IsOriginalError)
								{
									PublishUniversalCancelEventToBondedWarehouseChangeOfOwnership();
								}
								else if (IsAmendmentError || IsWithdrawalError)
								{
									PublishUniversalShipmentToBondedWarehouseChangeOfOwnershipFromLatestClearedData();
								}
								else if (IsAmendmentClear)
								{
									PublishUniversalShipmentToBondedWarehouseChangeOfOwnershipFromLastHoldOrLatestData();
								}
								else
								{
									PublishUniversalShipmentToBondedWarehouseChangeOfOwnership();
								}
							}
							else
							{
								ReportInvalidState(ChangeOfOwnership, supporter.EntryNumber, warehouseTransactionStatus);
							}
						}
						else if (supporter.IsOutwardBondedWarehousingEnabled)
						{
							if (WarehouseTransactionStatusList.IsPendingOutward(warehouseTransactionStatus))
							{
								if (HasBeenWithdrawn || IsOriginalError)
								{
									PublishUniversalCancelEventToBondedWarehouseOutward();
								}
								else if (IsAmendmentError)
								{
									HandleOutwardAmendmentError();
								}
								else if (IsAmendmentClear)
								{
									HandleOutwardAmendmentClear();
								}
								else
								{
									PublishUniversalShipmentToBondedWarehouseOutward();
								}
							}
							else
							{
								ReportInvalidState(Outward, supporter.EntryNumber, warehouseTransactionStatus);
							}
						}
						else
						{
							if (IsValidBondedWarehousingStatus(warehouseTransactionStatus))
							{
								if (HasBeenWithdrawn)
								{
									PublishUniversalCancelEventToBondedWarehouseInward();
								}
								else if (IsAmendmentClear)
								{
									HandleInwardAmendmentClear();
								}
								else if (IsAmendmentError)
								{
									HandleInwardAmendmentError();
								}
								else if (IsWithdrawalError)
								{
									HandleInwardWithdrawalError();
								}
								else if (!IsOriginalError)
								{
									PublishUniversalShipmentToBondedWarehouseInward();
								}
								else
								{
									HandleInwardOriginalError();
								}
							}
							else
							{
								ReportInvalidState(Inward, supporter.EntryNumber, warehouseTransactionStatus);
							}
						}

						if (supporter.HasChanges)
						{
							try
							{
								factory.Save();
							}
							catch (ZException e)
							{
								ZExceptionReporting.HandleSaveException(e);
							}
						}
					}
				}
				if (emailReportThatHasBeenDelayed != null)
				{
					SendEmail(emailReportThatHasBeenDelayed);
				}
			}
		}

		protected virtual bool IsValidBondedWarehousingStatus(ZString warehouseTransactionStatus) => WarehouseTransactionStatusList.IsPendingInward(warehouseTransactionStatus) || warehouseTransactionStatus == WarehouseTransactionStatusList.Codes.InwardCreated;

		void ReportInvalidState(string transactionType, ZString entryNumber, ZString warehouseTransactionStatus)
		{
			ErrorReporter.ReportOnce(
				FormattableString.Invariant($"Cannot Handle Inventory {transactionType} when warehouse status is not pending in {GetType().FullName}"),
				FormattableString.Invariant($@"EDIMessage (PK='{message.PK}', Application='{message.EM_ApplicationCode}', Type='{message.EM_MessageType}', SubType='{message.EM_MessageSubType}', Table='{message.EM_LinkTable}', LinkUniqueID='{message.EM_LinkUniqueID}')
Warehouse Job (Status='{warehouseTransactionStatus}', EntryNumber='{entryNumber}', TableName='{supporter.TableName}', PK='{supporter.PK}')"));
		}

		public static string ChangeOfOwnership => Res.GetString("CB5A86BF-8C88-4D50-8FC1-FAECEF61E3B7", "Change Of Ownership");
		public static string ChangeOfRegime => Res.GetString("{847361B1-8F62-4C65-8123-361C59D873D5}", "Change Of Regime");
		public static string Outward => Res.GetString("6F832091-3D38-49FC-9097-505C175F70DC", "Outward");
		public static string Inward => Res.GetString("9BB68C3A-AC76-4415-995D-D2BD346CCEBD", "Inward");

		#region Inward

		void HandleInwardOriginalError()
		{
			var whsStatus = supporter.WarehouseTransactionStatus;
			switch (whsStatus)
			{
				case WarehouseTransactionStatusList.Codes.InwardCreationHeld:
					supporter.PublishCancelEventForWHSInwardAndSaveIfNeeded(false, ZString.Empty, isManualWhsUpdate: false);
					break;
				case WarehouseTransactionStatusList.Codes.InwardCreatedPending:
					PublishUniversalCancelEventToBondedWarehouseInward();
					break;
				case WarehouseTransactionStatusList.Codes.InwardUpdatedPending:
					PublishUniversalShipmentOfPreviouslyClearedFollowByAcceptEventToBondedWarehouseInward();
					break;
			}
		}

		protected virtual void HandleInwardAmendmentClear()
		{
			if (supporter.SupportModificationState && supporter.IsInwardOrOutwardSecondTryPending())
			{
				supporter.ElevateModificationToHold();
			}
			else
			{
				PublishUniversalShipmentToBondedWarehouseInward();
			}
		}

		protected virtual void HandleInwardAmendmentError()
		{
			if (supporter.SupportModificationState && supporter.IsInwardOrOutwardSecondTryPending())
			{
				supporter.DeleteModificationNotes();
			}
			else
			{
				HandleInwardError();
			}
		}

		protected virtual void HandleInwardWithdrawalError()
		{
			HandleInwardError();
		}

		void HandleInwardError()
		{
			var whsStatus = supporter.WarehouseTransactionStatus;
			switch (whsStatus)
			{
				case WarehouseTransactionStatusList.Codes.InwardCreationHeld:
					supporter.PublishCancelEventForWHSInwardAndSaveIfNeeded(false, ZString.Empty, isManualWhsUpdate: false);
					break;
				default:
					PublishUniversalShipmentOfPreviouslyClearedFollowByAcceptEventToBondedWarehouseInward();
					break;
			}
		}

		protected virtual void PublishUniversalShipmentToBondedWarehouseInward()
		{
			var result = supporter.PublishShipmentForWHSInwardFromLastHoldOrLatestData();
			if (result != null)
			{
				var warehouseJob = result.FindJobIfExists() as IRelatedJob;
				switch (result.ResultType)
				{
					case UniversalResult.HadErrors:
						SendEmail(GetWHSEmail(Res.GetString("8e3e1a01-f660-4158-b0f9-de11a2ab1307", "Failed to create Stock Levels Update"), Res.GetString("b43d2bc6-a7e7-4a3e-9970-b4fbf56df25b", @"<p><font color=""red"">Cannot create Stock Levels Update due to the following errors.{0}</font></p><font color=""red"">{1}</font>", GetInwardWarehouseJobInfo(warehouseJob), ReplaceNewLineWithHTMLLineBreak(result.ErrorMessage))));
						break;
					case UniversalResult.Internal:
					case UniversalResult.External:
						result = PublishUniversalAcceptEventToBondedWarehouseInward(GetWHSEmail(Res.GetString("7643f2af-3535-4c27-b4f9-2c5abace3c88", "Stock Levels Update"), Res.GetString("f4745cc5-76b7-42d7-a1ca-34244fff86fd", "<p>Stock Levels have been updated.{0}</p>", GetInwardWarehouseJobInfo(warehouseJob))));
						break;
				}
			}
		}

		PublishToUniversalResult PublishUniversalAcceptEventToBondedWarehouseInward(EmailDef email = null)
		{
			var result = supporter.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
			if (result != null)
			{
				var warehouseJob = result.FindJobIfExists() as IRelatedJob;
				switch (result.ResultType)
				{
					case UniversalResult.HadErrors:
						email = GetWHSEmail(Res.GetString("9a970b30-d3d8-418c-a7b9-7cb43e0829e8", "Failed to finalize Stock Levels Update"), Res.GetString("cf3a4234-5925-4efd-8610-3d57be858e4e", @"<p><font color=""red"">Cannot finalize Stock Levels Update due to the following errors.{0}</font></p><font color=""red"">{1}</font>", GetInwardWarehouseJobInfo(warehouseJob), ReplaceNewLineWithHTMLLineBreak(result.ErrorMessage)), email);
						break;
					case UniversalResult.Internal:
					case UniversalResult.External:
						email = GetWHSEmail(Res.GetString("32f4b377-b38b-4db9-be4a-527ec7e6c146", "Finalized Stock Levels Update"), Res.GetString("a0aa2543-7ed6-4a33-aafe-fcd4da5fe0ca", "<p>Stock Levels Update can be finalized.{0}</p>", GetInwardWarehouseJobInfo(warehouseJob)), email);
						break;
				}
			}
			if (email != null)
			{
				SendEmail(email);
			}
			return result;
		}

		protected void PublishUniversalShipmentOfPreviouslyClearedFollowByAcceptEventToBondedWarehouseInward()
		{
			var result = supporter.PublishShipmentForWHSInwardFromLatestClearedData();
			if (result != null)
			{
				var warehouseJob = result.FindJobIfExists() as IRelatedJob;
				switch (result.ResultType)
				{
					case UniversalResult.HadErrors:
						SendEmail(GetWHSEmail(Res.GetString("0e65158f-de9c-473c-b900-84a135a5328f", "Failed to Restore Previous Stock Levels"), Res.GetString("22f92e02-3d9f-473d-995f-dff148c9eb78", @"<p><font color=""red"">Cannot restore Previous Stock Levels due to the following errors.{0}</font></p><font color=""red"">{1}</font>", GetInwardWarehouseJobInfo(warehouseJob), ReplaceNewLineWithHTMLLineBreak(result.ErrorMessage))));
						break;
					case UniversalResult.Internal:
					case UniversalResult.External:
						PublishUniversalAcceptEventToBondedWarehouseInward(GetWHSEmail(Res.GetString("befde65a-14a9-45b2-800d-0f5693bc555f", "Restored Previous Stock Levels"), Res.GetString("47fb7c25-e432-4b31-b94d-43b082fcf9f5", "<p>Previous Stock Levels have been restored.{0}</p>", GetInwardWarehouseJobInfo(warehouseJob))));
						break;
				}
			}
		}

		protected void PublishUniversalCancelEventToBondedWarehouseInward()
		{
			if (supporter.WarehouseTransactionStatus == WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal)
			{
				supporter.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCanceled;
				SendEmail(GetWHSEmail(Res.GetString("467da754-291a-43cf-81b8-6937d135322e", "Canceled Stock Levels Update"), Res.GetString("4458004a-a304-4a00-9384-9c3cbbf8a6f0", "<p>Stock Levels Update has been canceled.</p>")));
			}
			else if (supporter.WarehouseTransactionStatus != WarehouseTransactionStatusList.Codes.InwardCanceled)
			{
				var result = supporter.PublishCancelEventForWHSInwardAndSaveIfNeeded(false);
				if (result != null)
				{
					var warehouseJob = result.FindJobIfExists() as IRelatedJob;
					switch (result.ResultType)
					{
						case UniversalResult.HadErrors:
							SendEmail(GetWHSEmail(Res.GetString("ab942261-041f-440a-b16f-8e7a16c7b54a", "Failed to cancel Stock Levels Update"), Res.GetString("4bd73763-66ea-4a41-acb9-ccd5d950f069", @"<p><font color=""red"">Cannot cancel Stock Levels Update due to the following errors.{0}</font></p><font color=""red"">{1}</font>", GetInwardWarehouseJobInfo(warehouseJob), ReplaceNewLineWithHTMLLineBreak(result.ErrorMessage))));
							break;
						case UniversalResult.Internal:
						case UniversalResult.External:
							SendEmail(GetWHSEmail(Res.GetString("467da754-291a-43cf-81b8-6937d135322e", "Canceled Stock Levels Update"), Res.GetString("9aef3a91-c6e9-4af6-9623-fd0bab44fb35", "<p>Stock Levels Update has been canceled.{0}</p>", GetInwardWarehouseJobInfo(warehouseJob))));
							break;
					}
				}
			}
		}

		#endregion

		#region Change Of Ownership

		string GetChangeOfOwnershipWarehouseJobInfo(IRelatedJob warehouseJob)
		{
			return warehouseJob == null ? "" : " " + Res.GetString("{043E45DA-624A-42D2-A13F-91FC6D937D65}", "(WHS Change of Ownership:") + " " + EmailDefBuilder.GetJobLink(warehouseJob, warehouseJob.JobNumber) + ")";
		}

		protected void PublishUniversalCancelEventToBondedWarehouseChangeOfOwnership()
		{
			if (supporter.WarehouseTransactionStatus != WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCanceled)
			{
				var result = supporter.PublishCancelEventForWHSChangeOfOwnershipAndSaveIfNeeded(true);
				if (result != null)
				{
					var warehouseJob = result.FindJobIfExists() as IRelatedJob;
					switch (result.ResultType)
					{
						case UniversalResult.HadErrors:
							SendEmail(GetWHSEmail(Res.GetString("{2693C8FA-EC66-4D4A-B9AD-6BBB45928B8F}", "Failed to cancel Bonded Warehouse Change Of Ownership"), Res.GetString("{84F6F4D6-08F1-49C8-A701-4B18DF70FAB0}", @"<p><font color=""red"">Cannot cancel Bonded Warehouse Change Of Ownership due to the following errors.{0}</font></p><font color=""red"">{1}</font>", GetChangeOfOwnershipWarehouseJobInfo(warehouseJob), ReplaceNewLineWithHTMLLineBreak(result.ErrorMessage))));
							break;
						case UniversalResult.Internal:
						case UniversalResult.External:
							SendEmail(GetWHSEmail(Res.GetString("{C6795F84-8FC3-4EA2-8F96-3987E4F70D38}", "Canceled Bonded Warehouse Change Of Ownership"), Res.GetString("{4DFCC5AF-269D-42D3-9F6A-16B5A83D28D8}", "<p>Bonded Warehouse Change Of Ownership has been canceled.{0}</p>", GetChangeOfOwnershipWarehouseJobInfo(warehouseJob))));
							break;
					}
				}
			}
		}

		protected void PublishUniversalShipmentToBondedWarehouseChangeOfOwnershipFromLatestClearedData()
		{
			var result = supporter.PublishShipmentForWHSChangeOfOwnershipFromLatestClearedDataAndSaveIfNeeded();
			if (result != null)
			{
				var warehouseJob = result.FindJobIfExists() as IRelatedJob;
				switch (result.ResultType)
				{
					case UniversalResult.HadErrors:
						SendEmail(GetWHSEmail(Res.GetString("{906D532E-C431-4DEC-BD46-DABF6B410A42}", "Failed To Restore Previous Bonded Warehouse Change Of Ownership"), Res.GetString("{766683AC-74C1-46DB-8CB1-C5F71E7A58BF}", @"<p><font color=""red"">Cannot restore previous Bonded Warehouse Change Of Ownership due to the following errors.{0}</font></p><font color=""red"">{1}</font>", GetChangeOfOwnershipWarehouseJobInfo(warehouseJob), ReplaceNewLineWithHTMLLineBreak(result.ErrorMessage))));
						break;
					case UniversalResult.Internal:
					case UniversalResult.External:
						SendEmail(GetWHSEmail(Res.GetString("{CD2606FF-5A38-4F42-8900-5F2AD22E5EED}", "Restored Previous Bonded Warehouse Change Of Ownership"), Res.GetString("{DAC4D6A0-1C5E-454E-987B-2A3797D29605}", "<p>Previous Bonded Warehouse Change Of Ownership has been restored.{0}</p>", GetChangeOfOwnershipWarehouseJobInfo(warehouseJob))));
						break;
				}
			}
		}

		protected void PublishUniversalShipmentToBondedWarehouseChangeOfOwnershipFromLastHoldOrLatestData()
		{
			var result = supporter.PublishShipmentForWHSChangeOfOwnershipFromLastHoldOrLatestDataAndSaveIfNeeded();
			if (result != null)
			{
				var warehouseJob = result.FindJobIfExists() as IRelatedJob;
				switch (result.ResultType)
				{
					case UniversalResult.HadErrors:
						SendEmail(GetWHSEmail(Res.GetString("{3737B500-337A-44D3-A0CB-8BDF5967A486}", "Failed to Update Bonded Warehouse Change Of Ownership"), Res.GetString("{C763C51B-55B4-4129-BA9C-33C512D3290E}", @"<p><font color=""red"">Cannot update Bonded Warehouse Change Of Ownership due to the following errors.{0}</font></p><font color=""red"">{1}</font>", GetChangeOfOwnershipWarehouseJobInfo(warehouseJob), ReplaceNewLineWithHTMLLineBreak(result.ErrorMessage))));
						break;
					case UniversalResult.Internal:
					case UniversalResult.External:
						SendEmail(GetWHSEmail(Res.GetString("{566DA454-07BD-4503-8831-165814DC7624}", "Updated Bonded Warehouse Change Of Ownership"), Res.GetString("{A3D4FF12-C107-4F15-969D-C78143A388BF}", "<p>Bonded Warehouse Change Of Ownership has been updated.{0}</p>", GetChangeOfOwnershipWarehouseJobInfo(warehouseJob))));
						break;
				}
			}
		}

		protected void PublishUniversalShipmentToBondedWarehouseChangeOfOwnership()
		{
			var result = supporter.PublishShipmentForWHSChangeOfOwnershipFromLastDataAndUpdateEntryDetailsAndSaveIfNeeded();
			if (result != null)
			{
				var warehouseJob = result.FindJobIfExists() as IRelatedJob;
				switch (result.ResultType)
				{
					case UniversalResult.HadErrors:
						SendEmail(GetWHSEmail(Res.GetString("{FC0C6749-9DD4-49D7-AD7F-FC02B467FC61}", "Failed to Create Bonded Warehouse Change Of Ownership"), Res.GetString("{CB60013D-9092-422F-B6C4-A434BEC0927E}", @"<p><font color=""red"">Cannot created Bonded Warehouse Change Of Ownership due to the following errors.{0}</font></p><font color=""red"">{1}</font>", GetChangeOfOwnershipWarehouseJobInfo(warehouseJob), ReplaceNewLineWithHTMLLineBreak(result.ErrorMessage))));
						break;
					case UniversalResult.Internal:
					case UniversalResult.External:
						SendEmail(GetWHSEmail(Res.GetString("{E4CB4DC3-ACBD-4FE9-96AF-D7A9147D8D01}", "Created Bonded Warehouse Change Of Ownership"), Res.GetString("{55016F1F-B48D-41E4-ADEF-295EF9EE01BB}", "<p>Bonded Warehouse Change Of Ownership has been created.{0}</p>", GetChangeOfOwnershipWarehouseJobInfo(warehouseJob))));
						break;
				}
			}
		}

		#endregion

		#region Change Of Regime

		string GetChangeOfRegimeWarehouseJobInfo(IRelatedJob warehouseJob)
		{
			return warehouseJob == null ? "" : " " + Res.GetString("{A2688F11-A7B8-4157-9760-C48EEC31CAEB}", "(WHS Change of Regime:") + " " + EmailDefBuilder.GetJobLink(warehouseJob, warehouseJob.JobNumber) + ")";
		}

		protected void PublishUniversalCancelEventToInventoryChangeOfRegime()
		{
			if (supporter.WarehouseTransactionStatus != WarehouseTransactionStatusList.Codes.ChangeOfRegimeCanceled)
			{
				var result = supporter.PublishCancelEventForWHSChangeOfRegimeAndSaveIfNeeded(true);
				if (result != null)
				{
					var warehouseJob = result.FindJobIfExists() as IRelatedJob;
					switch (result.ResultType)
					{
						case UniversalResult.HadErrors:
							SendEmail(GetWHSEmail(Res.GetString("{17846518-8A8A-4CE4-A61C-5C2845C6273C}", "Failed to cancel Inventory Change Of Regime"), Res.GetString("{9E4B9622-0635-45F2-82A8-0629D03F6B55}", @"<p><font color=""red"">Cannot cancel Inventory Change Of Regime due to the following errors.{0}</font></p><font color=""red"">{1}</font>", GetChangeOfRegimeWarehouseJobInfo(warehouseJob), ReplaceNewLineWithHTMLLineBreak(result.ErrorMessage))));
							break;
						case UniversalResult.Internal:
						case UniversalResult.External:
							SendEmail(GetWHSEmail(Res.GetString("{7A84CC6A-952E-447E-97A5-AFF73A473E81}", "Canceled Inventory Change Of Regime"), Res.GetString("{37E152EC-A84F-4F88-992B-A7C136A05C36}", "<p>Inventory Change Of Regime has been canceled.{0}</p>", GetChangeOfRegimeWarehouseJobInfo(warehouseJob))));
							break;
					}
				}
			}
		}

		protected void PublishUniversalShipmentToInventoryChangeOfRegimeFromLatestClearedData()
		{
			var result = supporter.PublishShipmentForWHSChangeOfRegimeFromLatestClearedDataAndSaveIfNeeded();
			if (result != null)
			{
				var warehouseJob = result.FindJobIfExists() as IRelatedJob;
				switch (result.ResultType)
				{
					case UniversalResult.HadErrors:
						SendEmail(GetWHSEmail(Res.GetString("{2A929FC0-CB3B-4717-8FB6-41E6E6F21C87}", "Failed To Restore Previous Inventory Change Of Regime"), Res.GetString("{DC1498EA-81B3-4665-8E38-1CAACD525DBB}", @"<p><font color=""red"">Cannot restore previous Inventory Change Of Regime due to the following errors.{0}</font></p><font color=""red"">{1}</font>", GetChangeOfRegimeWarehouseJobInfo(warehouseJob), ReplaceNewLineWithHTMLLineBreak(result.ErrorMessage))));
						break;
					case UniversalResult.Internal:
					case UniversalResult.External:
						SendEmail(GetWHSEmail(Res.GetString("{D80821DA-6C6E-45C7-9498-16DC99D4DECA}", "Restored Previous Inventory Change Of Regime"), Res.GetString("{DF891346-2309-4FCA-A542-79923BD9C7A6}", "<p>Previous Inventory Change Of Regime has been restored.{0}</p>", GetChangeOfRegimeWarehouseJobInfo(warehouseJob))));
						break;
				}
			}
		}

		protected void PublishUniversalShipmentToInventoryChangeOfRegimeFromLastHoldOrLatestData()
		{
			var result = supporter.PublishShipmentForWHSChangeOfRegimeFromLastHoldOrLatestDataAndSaveIfNeeded();
			if (result != null)
			{
				var warehouseJob = result.FindJobIfExists() as IRelatedJob;
				switch (result.ResultType)
				{
					case UniversalResult.HadErrors:
						SendEmail(GetWHSEmail(Res.GetString("{17E743E9-6AC5-4C2E-8581-E6D6CE4AA5E7}", "Failed to Inventory Change Of Regime"), Res.GetString("{98EB0F81-3D4E-406C-ACBC-EA4DED2CB783}", @"<p><font color=""red"">Cannot update Inventory Change Of Regime due to the following errors.{0}</font></p><font color=""red"">{1}</font>", GetChangeOfRegimeWarehouseJobInfo(warehouseJob), ReplaceNewLineWithHTMLLineBreak(result.ErrorMessage))));
						break;
					case UniversalResult.Internal:
					case UniversalResult.External:
						SendEmail(GetWHSEmail(Res.GetString("{BADFF29F-048B-4FD1-AEC4-8711BC1C5BFD}", "Updated Inventory Change Of Regime"), Res.GetString("{158DA7D8-150C-46BA-91AE-9670657580C6}", "<p>Inventory Change Of Regime has been updated.{0}</p>", GetChangeOfRegimeWarehouseJobInfo(warehouseJob))));
						break;
				}
			}
		}

		protected void PublishUniversalShipmentToInventoryChangeOfRegime()
		{
			var result = supporter.PublishShipmentForWHSChangeOfRegimeFromLastDataAndUpdateEntryDetailsAndSaveIfNeeded();
			if (result != null)
			{
				var warehouseJob = result.FindJobIfExists() as IRelatedJob;
				switch (result.ResultType)
				{
					case UniversalResult.HadErrors:
						SendEmail(GetWHSEmail(Res.GetString("{3F23E823-1F13-44DE-B14D-76648515333F}", "Failed to Create Inventory Change Of Regime"), Res.GetString("{8854AD03-0F3B-4617-A88E-4799A491701E}", @"<p><font color=""red"">Cannot created Inventory Change Of Regime due to the following errors.{0}</font></p><font color=""red"">{1}</font>", GetChangeOfRegimeWarehouseJobInfo(warehouseJob), ReplaceNewLineWithHTMLLineBreak(result.ErrorMessage))));
						break;
					case UniversalResult.Internal:
					case UniversalResult.External:
						SendEmail(GetWHSEmail(Res.GetString("{3CC318AF-2516-476C-848D-124A34EE579E}", "Created Inventory Change Of Regime"), Res.GetString("{7800989E-CD95-480A-B08C-AA8936BF1D4F}", "<p>Inventory Change Of Regime has been created.{0}</p>", GetChangeOfRegimeWarehouseJobInfo(warehouseJob))));
						break;
				}
			}
		}

		#endregion

		#region Outward

		protected virtual void HandleOutwardAmendmentError()
		{
			if (supporter.SupportModificationState && supporter.IsInwardOrOutwardSecondTryPending())
			{
				if (supporter.WarehouseTransactionStatus == WarehouseTransactionStatusList.Codes.OutwardCreatedPending)
				{
					supporter.PublishUniversalShipmentOfPreviouslyHoldFollowByDeletingModificationForWHSOutward(true);
				}
			}
			else
			{
				PublishUniversalShipmentOfPreviouslyClearedFollowByAcceptEventToBondedWarehouseOutward();
			}
		}

		protected void PublishUniversalShipmentOfPreviouslyClearedFollowByAcceptEventToBondedWarehouseOutward()
		{
			var result = supporter.PublishShipmentForWHSOutwardFromLatestClearedData();
			if (result != null)
			{
				var warehouseJob = result.FindJobIfExists() as IRelatedJob;
				switch (result.ResultType)
				{
					case UniversalResult.HadErrors:
						SendEmail(GetWHSEmail(Res.GetString("ea328c50-c356-42a0-b522-9504ae2e9681", "Failed to Restore Previous Stock Release"), Res.GetString("846c4878-9bcf-4430-8b0b-4e26fe7e2eb9", @"<p><font color=""red"">Cannot restore Previous Stock Release due to the following errors.{0}</font></p><font color=""red"">{1}</font>", GetOutwardWarehouseJobInfo(warehouseJob), ReplaceNewLineWithHTMLLineBreak(result.ErrorMessage))));
						break;
					case UniversalResult.Internal:
					case UniversalResult.External:
						PublishUniversalAcceptEventToBondedWarehouseOutward(GetWHSEmail(Res.GetString("45373ad8-8c70-42e6-a061-9980f4ac9f6c", "Restored Previous Stock Release"), Res.GetString("23f70f90-09a8-481b-a9bb-5ade182b7633", "<p>Previous Stock Release has been restored.{0}</p>", GetOutwardWarehouseJobInfo(warehouseJob))));
						break;
				}
			}
		}

		string GetOutwardWarehouseJobInfo(IRelatedJob warehouseJob)
		{
			return warehouseJob == null ? "" : " " + Res.GetString("1e2b93f3-5478-41a8-82b0-7026079b9cd5", "(WHS Order:") + " " + EmailDefBuilder.GetJobLink(warehouseJob, warehouseJob.JobNumber) + ")";
		}

		protected string GetInwardWarehouseJobInfo(IRelatedJob warehouseJob)
		{
			return warehouseJob == null ? "" : " " + Res.GetString("4221c962-a410-4d1d-905e-93fd07c0468c", "(WHS Receipt:") + " " + EmailDefBuilder.GetJobLink(warehouseJob, warehouseJob.JobNumber) + ")";
		}

		protected virtual void HandleOutwardAmendmentClear()
		{
			if (supporter.SupportModificationState && supporter.IsInwardOrOutwardSecondTryPending())
			{
				if (supporter.WarehouseTransactionStatus == WarehouseTransactionStatusList.Codes.OutwardCreatedPending)
				{
					supporter.PublishUniversalShipmentOfLatestModificationFollowByElevatingModificationToHoldForWHSOutward();
				}
			}
			else
			{
				PublishUniversalShipmentFollowByAcceptEventToBondedWarehouseOutward();
			}
		}

		protected void PublishUniversalShipmentFollowByAcceptEventToBondedWarehouseOutward()
		{
			var result = supporter.PublishShipmentForWHSOutwardFromLastHoldOrLatestData();
			if (result != null)
			{
				PublishAcceptEventToBondedWarehouseOutwardIfPossible(result);
			}
		}

		protected void PublishAcceptEventToBondedWarehouseOutwardIfPossible(PublishToUniversalResult result)
		{
			var warehouseJob = result.FindJobIfExists() as IRelatedJob;
			switch (result.ResultType)
			{
				case UniversalResult.HadErrors:
					SendEmail(GetWHSEmail(Res.GetString("8fa3e7a8-00c0-40fa-97ae-b650f41a3a19", "Failed to Update Stock Release"), Res.GetString("d130d13a-44b3-494f-834e-d33448f00b4b", @"<p><font color=""red"">Cannot update Stock Release due to the following errors.{0}</font></p><font color=""red"">{1}</font>", GetOutwardWarehouseJobInfo(warehouseJob), ReplaceNewLineWithHTMLLineBreak(result.ErrorMessage))));
					break;
				case UniversalResult.Internal:
				case UniversalResult.External:
					PublishUniversalAcceptEventToBondedWarehouseOutward(GetWHSEmail(Res.GetString("e1bbe976-a751-4bd0-84ea-df2b1181d577", "Updated Stock Release"), Res.GetString("d834cf81-df0b-473c-9818-7522e80ff14c", "<p>Stock Release has been updated.{0}</p>", GetOutwardWarehouseJobInfo(warehouseJob))));
					break;
			}
		}

		protected void PublishUniversalShipmentToBondedWarehouseOutward()
		{
			if (NeedToPublishEntryDetailsForOutward && supporter.WarehouseTransactionStatus == WarehouseTransactionStatusList.Codes.OutwardCreatedPending)
			{
				var result = supporter.PublishShipmentForWHSOutwardFromLastDataAndUpdateEntryDetails();
				if (result != null)
				{
					PublishAcceptEventToBondedWarehouseOutwardIfPossible(result);
				}
			}
			else
			{
				PublishUniversalAcceptEventToBondedWarehouseOutward();
			}
		}

		protected virtual bool NeedToPublishEntryDetailsForOutward => true;

		protected void PublishUniversalAcceptEventToBondedWarehouseOutward(EmailDef email = null)
		{
			var result = supporter.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(false);
			if (result != null)
			{
				var warehouseJob = result.FindJobIfExists() as IRelatedJob;
				switch (result.ResultType)
				{
					case UniversalResult.HadErrors:
						email = GetWHSEmail(Res.GetString("6d9a8cd9-acd1-468c-be39-4f8b92f62c17", "Failed to finalize Stock Release"), Res.GetString("204dd518-af43-4cf0-9365-0cdeff39c9af", @"<p><font color=""red"">Cannot finalize Stock Release due to the following errors.{0}</font></p><font color=""red"">{1}</font>", GetOutwardWarehouseJobInfo(warehouseJob), ReplaceNewLineWithHTMLLineBreak(result.ErrorMessage)), email);
						break;
					case UniversalResult.Internal:
					case UniversalResult.External:
						email = GetWHSEmail(Res.GetString("8b71db7c-05e8-42d4-b053-ec31c7c33432", "Finalized Stock Release"), Res.GetString("4216ca55-4e95-4c77-93bc-77b9f8345342", "<p>Stock Release can be finalized.{0}</p>", GetOutwardWarehouseJobInfo(warehouseJob)), email);
						break;
				}
			}
			if (email != null)
			{
				SendEmail(email);
			}
		}

		protected void PublishUniversalCancelEventToBondedWarehouseOutward()
		{
			if (supporter.WarehouseTransactionStatus != WarehouseTransactionStatusList.Codes.OutwardCanceled)
			{
				var result = supporter.PublishCancelEventForWHSOutwardAndSaveIfNeeded(true);
				if (result != null)
				{
					var warehouseJob = result.FindJobIfExists() as IRelatedJob;
					switch (result.ResultType)
					{
						case UniversalResult.HadErrors:
							SendEmail(GetWHSEmail(Res.GetString("bb182228-b6f0-4766-850b-ef743eef5d5b", "Failed to cancel Stock Release"), Res.GetString("40e37ca4-e1f0-4a99-bd99-7d4529ee794c", @"<p><font color=""red"">Cannot cancel Stock Release due to the following errors.{0}</font></p><font color=""red"">{1}</font>", GetOutwardWarehouseJobInfo(warehouseJob), ReplaceNewLineWithHTMLLineBreak(result.ErrorMessage))));
							break;
						case UniversalResult.Internal:
						case UniversalResult.External:
							SendEmail(GetWHSEmail(Res.GetString("6501736c-4c1c-4401-b240-fdd76f6fa71c", "Canceled Stock Release"), Res.GetString("bdccc2b0-1dd0-4e8b-8800-d1fce3f8397d", "<p>Stock Release has been canceled.{0}</p>", GetOutwardWarehouseJobInfo(warehouseJob))));
							break;
					}
				}
			}
		}

		#endregion

		#region Implementation

		protected abstract bool HasBeenWithdrawn { get; }
		protected abstract bool IsOriginalError { get; }
		protected abstract bool IsAmendmentError { get; }
		protected abstract bool IsAmendmentClear { get; }
		protected abstract bool IsWithdrawalError { get; }

		protected void SendEmail(EmailDef email)
		{
			SendEmailCore(email);
			if (email == emailReportThatHasBeenDelayed)
			{
				emailReportThatHasBeenDelayed = null;
			}
		}
		protected abstract void SendEmailCore(EmailDef email);
		protected abstract string GetReferenceDetail();
		protected abstract string GetSubject(string subjectPrefix);
		protected abstract IWarehouseIntegrationSupporter GetSupporter();

		protected EmailDef GetWHSEmail(string subjectPrefix, string message, EmailDef existingEmail = null)
		{
			var email = existingEmail ?? emailReportThatHasBeenDelayed;
			if (email == null)
			{
				return EmailDefBuilder.GetEmail(GetSubject(subjectPrefix),
					Res.GetString("5b15269e-662d-46f0-ba57-025fa11be6ea", @"<br />
{0}
</strong>{1}", GetReferenceDetail(), message));
			}
			else
			{
				return UpdateBodyText(email, message);
			}
		}

		EmailDef UpdateBodyText(EmailDef email, ZString message)
		{
			var bodyText = email.Body;
			if (bodyText.Contains(EmailDefBuilder.HtmlTemplates.DynamicHtml1))
			{
				email.Body = bodyText.Replace(EmailDefBuilder.HtmlTemplates.DynamicHtml1, message);
			}
			else if (bodyText.Contains(EmailDefBuilder.HtmlTemplates.DynamicHtml2))
			{
				email.Body = bodyText.Replace(EmailDefBuilder.HtmlTemplates.DynamicHtml2, message);
			}
			else if (bodyText.Contains(EmailDefBuilder.HtmlTemplates.DynamicHtml3))
			{
				email.Body = bodyText.Replace(EmailDefBuilder.HtmlTemplates.DynamicHtml3, message);
			}
			else if (bodyText.Contains(EmailDefBuilder.HtmlTemplates.DynamicHtml4))
			{
				email.Body = bodyText.Replace(EmailDefBuilder.HtmlTemplates.DynamicHtml4, message);
			}
			else if (bodyText.Contains(EmailDefBuilder.HtmlTemplates.DynamicHtml5))
			{
				email.Body = bodyText.Replace(EmailDefBuilder.HtmlTemplates.DynamicHtml5, message);
			}
			else
			{
				email.Body = bodyText.Replace("Regards,", message + "<p>Regards,");
			}
			return email;
		}

		protected ZString ReplaceNewLineWithHTMLLineBreak(ZString data)
		{
			return data.Replace("\r\n", "<br />").Replace("\r\n", "<br />");
		}

		protected readonly BusinessObjectFactory factory;
		protected readonly EDIMessage message;
		protected readonly IWarehouseIntegrationSupporter supporter;
		protected EmailDef emailReportThatHasBeenDelayed;

		#endregion
	}
}
