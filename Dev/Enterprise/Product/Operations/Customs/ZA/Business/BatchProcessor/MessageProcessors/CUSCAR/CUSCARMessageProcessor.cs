using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Business.BatchProcessor.MessageProcessors;
using Enterprise.Edifact.D95B.Elements;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	class CUSCARMessageProcessor : ZACApplicationTypeMessageProcessor
	{
		public CUSCARMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => "CUSCAR Message Processor";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { SARSEDIMessage.MessageTypes.CUSCAR };

		protected override bool RequiresPreProcessingCore => true;

		protected override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason, MessageHelper Helper) TryFindLinkedObject(EDIMessage message)
		{
			var branchPK = message.EM_GB;
			BusinessObject linkedObject = null;
			MultilingualString discardReason = (NoResString)string.Empty;
			D16AMessageHelper helper = null;

			var cuscarMessage = message as CUSCAREDIMessage;
			if (cuscarMessage == null)
			{
				discardReason = GetMessageProcessorCannotProcessMessage("CUSCAR", message);
			}
			else
			{
				discardReason = GetDiscardReasonIfMessageIsInvalid(cuscarMessage, Logger);
				if (string.IsNullOrEmpty(discardReason))
				{
					helper = cuscarMessage.CUSCARD16AHelper;
					if (helper != null)
					{
						if (GetExistingManifest(cuscarMessage.Factory, helper) is AsycudaManifestHeader header)
						{
							branchPK = header.AMA_GB;
							linkedObject = header;
						}
					}
				}
			}

			return (branchPK, linkedObject, discardReason, helper);
		}

		protected override void ProcessMessageMain(EDIMessage message)
		{
			var cuscarMessage = (CUSCAREDIMessage)message;
			var header = ((AsycudaManifestHeader)cuscarMessage.EM_LinkedObject) ?? CreateManifest(cuscarMessage);
			var success = false;
			if (header != null && CheckHeaderNotSentToCustoms(header))
			{
				UpdateManifest(cuscarMessage.Factory, cuscarMessage.CUSCARD16AHelper, header);
				success = true;
			}

			message.EM_Status = success ? ZAMessage.Status.ProcessedOK : ZAMessage.Status.Discarded;
		}

		static MultilingualString GetDiscardReasonIfMessageIsInvalid(CUSCAREDIMessage cuscarMessage, LoggingInformation logger)
		{
			MultilingualString reason = (NoResString)string.Empty;
			var helper = cuscarMessage.CUSCARD16AHelper;

			if (helper == null)
			{
				reason = ResString.GetMultilingualString("7da0d006-0402-4bd7-b5f2-46854bc8b64c", "CUSCAR Message Processor cannot process message; D16B Decoder not found.");
			}
			else
			{
				var validationErrors = helper.ValidateMessage();
				if (!validationErrors.IsEmpty)
				{
					reason = ResString.GetMultilingualString("efa20928-d054-44e5-991f-fd7df57ee2eb", "CUSCAR Message Processor cannot process a message with Invalid or Missing parts: {0}", validationErrors);
				}
				else
				{
					var manifestType = helper.ManifestType;
					var success = manifestType == nameof(ManifestDocumentType.AND) || manifestType == nameof(ManifestDocumentType.ANT);

					if (!success)
					{
						reason = ResString.GetMultilingualString("545098f3-0a68-432e-874b-889c2bf78af7", "CUSCAR Message Processor cannot process a {0} manifest type", manifestType);
					}
				}
			}

			return reason;
		}

		static AsycudaManifestHeader GetExistingManifest(BusinessObjectFactory factory, D16AMessageHelper helper)
		{
#pragma warning disable IDE0058 // Expression value is never used
			var manifestQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			manifestQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, AsycudaManifestHeader.ApplicationCode_Out);

			if (helper.TransportCode != TransportModeCodeList.Codes.Air)
			{
				manifestQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_Voyage, helper.VoyageFlightNo);
			}

			var masterBillSubQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA, AsycudaManifestHeaderSchema.PK);
			masterBillSubQuery.AddToFilter(AsycudaBillSchema.ABL_BillNumber, helper.DocumentNumber.Left(AsycudaBill.Schema.ABL_BillNumberMaxLength));
			masterBillSubQuery.AddToFilter(AsycudaBillSchema.ABL_BillIssueDate, helper.DocumentDate);
			masterBillSubQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);

			manifestQuery.AddSubQuery(masterBillSubQuery, JoinCondition.And);
			manifestQuery.OrderBy = AsycudaManifestHeaderSchema.Constants.AMA_SystemCreateTimeUtc;
#pragma warning restore IDE0058 // Expression value is never used

			return factory.LoadTop1<AsycudaManifestHeader>(manifestQuery);
		}

		bool CheckHeaderNotSentToCustoms(AsycudaManifestHeader header)
		{
			var sentBill = header.Bills.Cast<AsycudaBill>().FirstOrDefault(bill => !bill.ABL_MessageStatus.IsEmpty);
			var success = sentBill == null;
			if (!success)
			{
				Logger.LogError(Res.GetString("f7d5361d-e5d3-46d9-b609-ab682e1e7650", "CUSCAR Message Processor cannot update Manifest {0}.  Bill {1} on the Manifest has been sent to Customs. Message Status: {2}.", header.MasterBill.ABL_BillNumber, sentBill.ABL_BillNumber, sentBill.ABL_MessageStatus));
			}

			return success;
		}

		#region Manifest

		AsycudaManifestHeader CreateManifest(CUSCAREDIMessage message)
		{
			var factory = message.Factory;
			var helper = message.CUSCARD16AHelper;

			var header = CreateManifestHeaderAndMasterBill(factory, helper);
			message.EM_LinkedObject = header;

			UpdateManifestHeaderAndMasterBill(factory, helper, header);

			return header;
		}

		void UpdateManifest(BusinessObjectFactory factory, D16AMessageHelper helper, AsycudaManifestHeader header)
		{
			var isChangeMessage = helper.MessageFunction == MessageFunctionCodedList.Change;

			if (isChangeMessage)
			{
				UpdateManifestHeaderAndMasterBill(factory, helper, header);
			}

			var containersChanged = CreateOrUpdateContainers(helper, header, isChangeMessage);
			var billsChanged = CreateOrUpdateHouseBills(helper, header, isChangeMessage);

			if (containersChanged || billsChanged)
			{
				UpdateContainerPackCount(header);
			}
		}

		AsycudaManifestHeader CreateManifestHeaderAndMasterBill(BusinessObjectFactory factory, D16AMessageHelper helper)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_AgentType = Core.Constants.AgentType.Agent;
			var masterBill = header.MasterBill;
			masterBill.ABL_BillNumber = helper.DocumentNumber.Left(AsycudaBill.Schema.ABL_BillNumberMaxLength);
			masterBill.ABL_BillIssueDate = helper.DocumentDate;

			return header;
		}

		void UpdateManifestHeaderAndMasterBill(BusinessObjectFactory factory, D16AMessageHelper helper, AsycudaManifestHeader header)
		{
			header.AMA_Voyage = helper.VoyageFlightNo;

			header.AMA_TransportMode = new TransportModeTranslator().TranslateToCargoWiseCode(helper.TransportCode);
			header.AMA_RadioCallSign = helper.RadioCallSign;
			header.AMA_VesselName = GetVesselNameFromCallSign(factory, header.AMA_RadioCallSign);
			header.AMA_ContainerMode = helper.GetContainers().Length > 0 ? ContainerModes.Containerised : ContainerModes.BreakBulk;
			header.AMA_ManifestType = helper.ManifestType;
		}

		ZString GetVesselNameFromCallSign(BusinessObjectFactory factory, ZString callSign)
		{
			RefVessel vessel = null;

			if (!callSign.IsEmpty)
			{
				vessel = factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_RadioCallSign, callSign));
			}

			return vessel?.RV_Code ?? ZString.Empty;
		}

		#endregion

		#region Containers

		bool CreateOrUpdateContainers(D16AMessageHelper helper, AsycudaManifestHeader header, bool isChangeMessage)
		{
			bool changed = false;
			var containers = header.Containers.Cast<AsycudaContainer>().ToArray();
			var messageContainers = helper.GetContainers();

			foreach (var containerDetail in messageContainers)
			{
				bool canUpdate = isChangeMessage;

				var container = containers.FirstOrDefault(c => c.ACN_ContainerNumber == containerDetail.ContainerNumber);
				if (container == null)
				{
					container = header.Containers.AddNew();
					container.ACN_ContainerNumber = containerDetail.ContainerNumber;
					canUpdate = true;
				}

				if (canUpdate)
				{
					UpdateContainer(container, containerDetail);
					changed = true;
				}
			}

			return changed;
		}

		void UpdateContainer(AsycudaContainer asycudaContainer, D16AMessageContainerDetail container)
		{
			asycudaContainer.ACN_GoodsWeight = ZDecimal.ParseSafe(container.GoodsWeight, 0);
			asycudaContainer.ACN_GoodsWeightUQ = container.GoodsWeightUQ;
			asycudaContainer.ACN_Seal1 = container.SealNumber;
		}

		void UpdateContainerPackCount(AsycudaManifestHeader header)
		{
			foreach (AsycudaContainer container in header.Containers)
			{
				var links = header.Factory.Load<AsycudaContainerBillOrPackageLink>(new ZQuery(AsycudaContainerBillOrPackageLinkSchema.APC_ACN_Container, container.PK));
				container.ACN_NumberOfPackages = links.Length;
			}
		}

		#endregion

		#region House Bills

		bool CreateOrUpdateHouseBills(D16AMessageHelper helper, AsycudaManifestHeader header, bool isChangeMessage)
		{
			bool changed = false;
			var bills = header.Bills.Cast<AsycudaBill>().ToArray();
			var messageBills = helper.GetBills();

			foreach (var billDetail in messageBills)
			{
				bool canUpdate = isChangeMessage;

				var bill = bills.FirstOrDefault(b => b.ABL_BillNumber == billDetail.BillNumber);
				if (bill == null)
				{
					bill = header.Bills.AddNew();
					bill.ABL_BillNumber = billDetail.BillNumber;
					bill.ABL_BolType = AsycudaBill.HouseBillCode;
					canUpdate = true;
				}

				if (canUpdate)
				{
					UpdateHouseBill(helper, bill, billDetail);
					changed = true;
				}

				changed |= CreateOrUpdatePacksOnBill(bill, billDetail, isChangeMessage);
			}

			return changed;
		}

		void UpdateHouseBill(D16AMessageHelper helper, AsycudaBill bill, D16AMessageHouseBillDetail billDetail)
		{
			bill.ABL_GoodsDescription = billDetail.BillGoodsDescription;
			bill.ABL_ManifestQty = billDetail.TotalNumberOfPackages;
			bill.ABL_ManifestUQ = billDetail.TypeOfPackages;

			var manifestType = helper.ManifestType;
			if (manifestType == nameof(ManifestDocumentType.AND))
			{
				bill.ABL_GoodsLocation = billDetail.PlaceOfDeconsolidation;
			}
			else if (manifestType == nameof(ManifestDocumentType.ANT))
			{
				bill.ABL_GoodsLocation = billDetail.TerminalOfDischarge;
			}
		}

		bool CreateOrUpdatePacksOnBill(AsycudaBill bill, D16AMessageHouseBillDetail billDetails, bool isChangeMessage)
		{
			bool changed = false;
			var packs = bill.Packs.Cast<AsycudaPack>().ToArray();
			var messagePacks = billDetails.GetPackLines();

			foreach (var packDetail in messagePacks)
			{
				bool canUpdate = isChangeMessage;

				var pack = packs.FirstOrDefault(p => p.APA_LineNo == packDetail.GoodsLineNumber);
				if (pack == null)
				{
					pack = bill.Packs.AddNew();
					canUpdate = true;
				}

				if (canUpdate)
				{
					UpdatePack(pack, packDetail);
					changed = true;
				}
			}

			return changed;
		}

		void UpdatePack(AsycudaPack pack, D16AMessagePackLineDetail packDetail)
		{
			var containerLinkNum = packDetail.ContainerPackageLink;
			if (pack.Container?.ACN_ContainerNumber != (ZString?)containerLinkNum)
			{
				var container = pack.Bill.Header.Containers.Cast<AsycudaContainer>().FirstOrDefault(c => c.ACN_ContainerNumber == containerLinkNum);
				if (container != null)
				{
					pack.ContainerPK = container.PK;
				}
				else
				{
					Logger.Log(Res.GetString("5a9053a8-66b4-45d8-a401-85f3a557dd2f", "CUSCAR Message Processor cannot locate container {0} referenced on Pack Line {1}.", containerLinkNum, packDetail.GoodsLineNumber));
				}
			}

			pack.APA_Weight = ZDecimal.ParseSafe(packDetail.PackWeight, 0);
			pack.APA_WeightUQ = new CustomsWeightUnitToSARSWeightUnit().GetCodeFromDescription(packDetail.PackWeightUQ);

			pack.APA_GoodsDescription = packDetail.GoodsDescription;
			pack.APA_MarksAndNumbers = packDetail.MarksAndNumbers;

			pack.APA_VINNumber = GetVINFromMarksAndNumbers(pack.APA_MarksAndNumbers);
		}

		ZString GetVINFromMarksAndNumbers(ZString marksAndNumbers)
		{
			var parts = marksAndNumbers.Split('=');
			return parts.Length == 2 && parts[0] == PartAttributeTypeList.Codes.VIN ? parts[1] : ZString.Empty;
		}

		#endregion
	}
}
