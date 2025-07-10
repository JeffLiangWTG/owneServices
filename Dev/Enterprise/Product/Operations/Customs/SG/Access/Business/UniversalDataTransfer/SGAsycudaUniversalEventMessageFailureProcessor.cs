using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using MessageProcessorConstants = Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.AsycudaUniversalEventMessageProcessor.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer
{
	class SGAsycudaUniversalEventMessageFailureProcessor : SGAsycudaUniversalEventMessageProcessor
	{
		public SGAsycudaUniversalEventMessageFailureProcessor(UniversalEvent universalEvent, AsycudaManifestHeader manifestHeader)
			: base(universalEvent, manifestHeader)
		{
		}

		protected override void ProcessCore()
		{
			base.ProcessCore();
			var masterBillContext = GetSpecifiedContext(MessageProcessorConstants.MasterBill);
			var houseBillContexts = GetSpecifiedSubContexts(MessageProcessorConstants.HouseBill, masterBillContext);
			ErrorEventProcess(manifestHeader, houseBillContexts);
			UpdateBills(manifestHeader, houseBillContexts);
		}

		void ErrorEventProcess(AsycudaManifestHeader manifestHeader, IEnumerable<Context> houseBillContexts)
		{
			var refGrouped =
				from houseBill in houseBillContexts
				let errRefs = GetSpecifiedSubContexts(MessageProcessorConstants.ConsignmentReference, houseBill)
				where houseBill?.Value.HasValue ?? false
				group errRefs
				by new
				{
					billNumber = houseBill.Value.GetValueOrDefault(),
					MessageStatusCode = GetSpecifiedSubContext(MessageProcessorConstants.MessageStatusCode, houseBill)?.Value ?? ZString.Empty,
				}
				into grouped
				select new
				{
					grouped.Key.billNumber,
					grouped.Key.MessageStatusCode,
					errors = grouped
				};

			foreach (var reference in refGrouped)
			{
				var billCountry = GetBill(reference.billNumber, manifestHeader);
				if (billCountry != null)
				{
					var errorPairs = new CodeDescriptionPairList();

					foreach (var errors in reference.errors)
					{
						foreach (var error in errors)
						{
							var errorCode = GetSpecifiedSubContext(MessageProcessorConstants.ErrorCode, error)?.Value ?? ZString.Empty;
							if (!errorCode.IsEmpty)
							{
								var errorDesc = GetSpecifiedSubContext(MessageProcessorConstants.ErrorDescription, error)?.Value ?? ZString.Empty;
								errorPairs.AddPairIfNotExist(errorCode, errorDesc);
							}
						}
					}

					var logEntry = CreateLogEntryFromErrors(billCountry.Factory, reference.MessageStatusCode, errorPairs);
					if (!logEntry.IsEmpty)
					{
						billCountry.Logs.AddNew(Events.MessageStatusChange, logEntry);
					}
				}
			}
		}

		internal static ZString CreateLogEntryFromErrors(BusinessObjectFactory factory, ZString prefix, CodeDescriptionPairList errorPairs)
		{
			var stringBuilder = new ZStringBuilder();
			if (errorPairs != null && errorPairs.Count > 0)
			{
				foreach (ZArchitecture.Core.CodeDescriptionPair error in errorPairs)
				{
					var errorCode = (ZString)error.Code;
					if (!errorCode.IsEmpty)
					{
						var userDescription = Universal.ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, errorCode, Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GlobalManifestErrorCommentary, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty;
						stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, " - {0} {1}", errorCode, userDescription.IsEmpty ? error.Description : (string)userDescription));
					}
				}
				if (!stringBuilder.IsEmpty)
				{
					stringBuilder.Prepend(prefix);
				}
			}
			return stringBuilder.ToString();
		}

		void UpdateBills(AsycudaManifestHeader manifestHeader, IEnumerable<Context> houseBillContexts)
		{
			if (manifestHeader.IsImport)
			{
				foreach (var houseBillElement in houseBillContexts.Where(h => h.Value.HasValue))
				{
					var billCountry = GetBill(houseBillElement.Value.GetValueOrDefault(), manifestHeader);
					if (billCountry != null)
					{
						var billRegistrationEntryNumber = billCountry.RegistrationEntryNumber;
						if (billRegistrationEntryNumber != null
							&& !billRegistrationEntryNumber.IsDeleted)
						{
							billRegistrationEntryNumber.Delete();
						}

						ClearCustomsStatusAndCancelEventIfPossible(billCountry, billCountry.Logs);

						var packElements = GetSpecifiedSubContexts(ASYCUDA.Business.Constants.EventContext.ConsignmentReference, houseBillElement).ToArray();
						if (packElements.Any())
						{
							UpdatePacks(billCountry, packElements);
						}
					}
				}
			}
		}

		void UpdatePacks(AsycudaBill bill, Context[] packElements)
		{
			var consignmentReferences = bill.Packs.Cast<AsycudaPack>().GroupBy(x => x.ConsignmentReference).ToDictionary(x => x.Key);
			foreach (var packElement in packElements)
			{
				if (ZInt.TryParse(packElement.Value.GetValueOrDefault(), out var consignmentReference))
				{
					if (consignmentReferences.TryGetValue(consignmentReference, out var matchingConsignmentReferences))
					{
						var packSubContextCollection = packElement.SubContextCollection;
						if (packSubContextCollection != null)
						{
							foreach (var matchingConsignmentReference in matchingConsignmentReferences)
							{
								var packedItem = matchingConsignmentReference.PackedItem;
								if (packedItem != null)
								{
									var packedItemRegistrationEntryNumber = packedItem.RegistrationEntryNumber;
									if (packedItemRegistrationEntryNumber != null && !packedItemRegistrationEntryNumber.IsDeleted)
									{
										packedItemRegistrationEntryNumber.Delete();
									}

									ClearCustomsStatusAndCancelEventIfPossible(packedItem, packedItem.Pack.Logs);
								}
							}
						}
					}
				}
			}
		}

		void ClearCustomsStatusAndCancelEventIfPossible(ASYCUDA.Business.IStatusSupporter statusSupporter, Logs logs)
		{
			var customsStatus = statusSupporter.CustomsStatus;

			if (customsStatus == Common.SG.GlobalManifestStatusList.Codes.InspectionRequired || customsStatus == Common.SG.GlobalManifestStatusList.Codes.Clear)
			{
				statusSupporter.CustomsStatus = ZString.Empty;
				logs.MostRecentLogByEventTime(AutoEvents.StatusChange, new ZQuery(StmALogSchema.SL_Reference, customsStatus))?.Cancel();
			}
		}

		AsycudaBill GetBill(ZString billNumber, AsycudaManifestHeader manifestHeader)
		{
			return manifestHeader.Bills.OfType<AsycudaBill>().Where(x => x.ABL_BillNumber == billNumber).OrderBy(x => x.ABL_SystemCreateTimeUtc).FirstOrDefault();
		}
	}
}
