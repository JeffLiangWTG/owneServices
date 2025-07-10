using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Customs.NZ.Business
{
	public class ConsolidatedDeclaration : Customs.Business.ConsolidatedDeclaration
		, Integration.Customs.NZ.IConsolidatedDeclaration
	{
		public ConsolidatedDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override FilterBusinessObjectDefaults DefaultAttachDeclarationFilter => DefaultConsolidatedDeclarationFilter.GetDefaultConsolidatedDeclarationFilter(LeadDeclaration as JobDeclaration);

		[List(nameof(LeadDeclaration) + "." + nameof(BaseJobDeclaration.Lookups) + "." + nameof(Customs.Business.JobDeclarationLookups.MessageSubTypeList))]
		public ZString EntryStyle => LeadDeclaration?.JE_MessageSubType ?? ZString.Empty;
		public ZPropertyInfo EntryStyleInfo => GetZPropertyInfo(nameof(EntryStyle));

		[List(nameof(LeadDeclaration) + "." + nameof(BaseJobDeclaration.Lookups) + "." + nameof(Customs.Business.JobDeclarationLookups.Vessels))]
		public ZString VesselName => LeadDeclaration.JE_VesselName;
		public ZPropertyInfo VesselNameInfo => GetZPropertyInfo(nameof(VesselName));

		public ZString VoyageFlightNo => LeadDeclaration.JE_VoyageFlightNo;
		public ZPropertyInfo VoyageFlightNoInfo => GetZPropertyInfo(nameof(VoyageFlightNo));

		[ResourceStringData("4af4bad2-fb76-4196-a2a7-b0a3d5546bd9", Caption = "Customs Status", ShortCaption = "Status")]
		public ZString CustomsStatusDescription
		{
			get
			{
				if (ConsolidatedEntryStatusList.Codes.AppliedToConsolidation.Equals(LeadDeclaration?.JE_EntryStatus) && CRD_CustomsStatus.IsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					return LeadDeclaration?.JE_EntryStatusDescription ?? ZString.Empty;
				}
			}
		}

		public override void OnCreatedWithCustomsDeclarations()
		{
			if (JobDeclarations.Count > 0)
			{
				CRD_PeriodTo = JobDeclarations.Max(x => x.JE_EntryAuthorisationDate).Date;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CRD_ApplicationCode = ApplicationCodes.TSW;
		}

		protected override Customs.Business.CusReconBase.CusReconDeclarationValidation GetNewValidation()
		{
			return new ConsolidatedDeclarationValidation(this);
		}

		protected override DocumentSupporter CreateNewDocumentSupporter() => new ConsolidatedDeclarationDocumentSupporter(this);

		protected override IConsolidatedJobDeclarationCollection<BaseJobDeclaration> CreateNewJobDeclarationCollection() => new ConsolidatedJobDeclarationCollection<JobDeclaration>(this);

		protected override void SyncJobDeclarationsStatusCore()
		{
			var leadDeclaration = (JobDeclaration)LeadDeclaration;
			var leadEntryHeader = leadDeclaration.CusEntryHeader;
			var jobDeclarations = JobDeclarations.Cast<JobDeclaration>();
			var leadDeclarationNumber = leadDeclaration.DeclarationNumber;

			if ((!leadEntryHeader?.CusEntryNumber?.IsInDatabase ?? false) && leadDeclarationNumber == (leadEntryHeader?.CusEntryNumber?.CE_EntryNum ?? ZString.Empty))
			{
				// sync entry number from lead declaration to consolidated entry
				var entryNumberOnThis = Factory.New<CusEntryNumber>();
				entryNumberOnThis.CopyPersistentValuesFrom(leadEntryHeader.CusEntryNumber);
				entryNumberOnThis.CE_ParentID = PK;
				entryNumberOnThis.CE_ParentTable = TableName;

				var setConsolidatedEntryMemberID = leadEntryHeader.CH_ConsolidatedEntryMemberID.IsEmpty && !leadDeclarationNumber.IsEmpty;
				var consolidatedEntryMemberID = ZShort.Zero;
				if (setConsolidatedEntryMemberID)
				{
					leadEntryHeader.CH_ConsolidatedEntryMemberID = ++consolidatedEntryMemberID;
				}

				// sync entry number from lead declaration to other declarations
				foreach (var dec in jobDeclarations)
				{
					if (dec.PK != leadDeclaration.PK)
					{
						dec.DeclarationNumber = leadDeclarationNumber;

						if (setConsolidatedEntryMemberID)
						{
							dec.CusEntryHeader.CH_ConsolidatedEntryMemberID = ++consolidatedEntryMemberID;
						}
					}
				}
			}

			if (leadDeclaration.HasChanges)
			{
				jobDeclarations.ForEach(dec =>
				{
					if (dec.PK != leadDeclaration.PK)
					{
						dec.JE_TSWCombinedStatus = leadDeclaration.JE_TSWCombinedStatus;
						dec.JE_EDITransmitDate = leadDeclaration.JE_EDITransmitDate;
						dec.AgencyMessageBeingProcessed = leadDeclaration.AgencyMessageBeingProcessed;
						if (leadDeclaration.JE_PaymentMethodInfo.HasChanges)
						{
							dec.JE_PaymentMethod = leadDeclaration.JE_PaymentMethod;
						}
					}
				});
			}

			if (!leadDeclaration.ActiveEntryHeaders.Any(_ => _.IsInDatabase))
			{
				jobDeclarations.ForEach(dec =>
				{
					if (dec.PK != leadDeclaration.PK)
					{
						dec.CusEntryHeader.CH_IsActive = false;
					}
				});
			}
			else if (leadDeclaration.CusEntryHeader.HasChanges)
			{
				jobDeclarations.ForEach(dec =>
				{
					var entryHeader = dec.CusEntryHeader;
					entryHeader.CH_EntryChargeWaived = leadDeclaration.CusEntryHeader.CH_EntryChargeWaived;
					entryHeader.CH_CustomsDeliveryInstructions = leadDeclaration.CusEntryHeader.CH_CustomsDeliveryInstructions;
					entryHeader.CH_LastEntryStyle = leadDeclaration.CusEntryHeader.CH_LastEntryStyle;
					entryHeader.CH_TotalAmountReturned = leadDeclaration.CusEntryHeader.CH_TotalAmountReturned;
					entryHeader.CH_EntryStatus = leadDeclaration.CusEntryHeader.CH_EntryStatus;
					entryHeader.CH_IsEntryCancelled = leadDeclaration.CusEntryHeader.CH_IsEntryCancelled;
					entryHeader.CH_NZCSStatus = leadDeclaration.CusEntryHeader.CH_NZCSStatus;
					entryHeader.CH_NZCSResponseTime = leadDeclaration.CusEntryHeader.CH_NZCSResponseTime;
					entryHeader.CH_MPIFoodStatus = leadDeclaration.CusEntryHeader.CH_MPIFoodStatus;
					entryHeader.CH_MPIFoodResponseTime = leadDeclaration.CusEntryHeader.CH_MPIFoodResponseTime;
					entryHeader.CH_MPIBioStatus = leadDeclaration.CusEntryHeader.CH_MPIBioStatus;
					entryHeader.CH_MPIBioResponseTime = leadDeclaration.CusEntryHeader.CH_MPIBioResponseTime;
				});
			}
		}

		protected override void OnAggregateDeclarationBuilt(BaseJobDeclaration aggregateDeclaration)
		{
			var nzDeclaration = aggregateDeclaration as JobDeclaration;
			var leadDeclaration = LeadDeclaration as JobDeclaration;
			nzDeclaration.JE_VesselName = VesselName;
			nzDeclaration.JE_VoyageFlightNo = VoyageFlightNo;
			nzDeclaration.JE_RL_NKPortOfDeliveryNotify = leadDeclaration.JE_RL_NKPortOfDeliveryNotify;
			nzDeclaration.CusEntryHeader.CH_BGMReference = ZString.Empty;
			nzDeclaration.CusEntryHeader.PopulateCH_BGMReferenceIfNeeded();

			// If master bills are same, merge master bills. Otherwise remove master bills while keeping their child house bills
			if (JobDeclarations.IsCongruentOn(_ => _.JE_MasterBill))
			{
				ZGuid pkMasterBillToKeep = ZGuid.Empty;
				foreach (Customs.Business.Bill bill in nzDeclaration.Bills.ToArray())
				{
					if (bill.IsHouseBill)
					{
						if (pkMasterBillToKeep.IsEmpty)
						{
							pkMasterBillToKeep = bill.CU_CU_ParentBill;
						}
						else
						{
							bill.CU_CU_ParentBill = pkMasterBillToKeep;
						}
					}
					else if (bill.IsMasterBill)
					{
						if (pkMasterBillToKeep.IsEmpty)
						{
							pkMasterBillToKeep = bill.PK;
						}
						else if (pkMasterBillToKeep != bill.PK)
						{
							nzDeclaration.Bills.Remove(bill);
						}
					}
				}
			}
			else
			{
				var bills = nzDeclaration.Bills.Where(bill => bill.CU_BillType == Declaration.BillTypeList.Codes.MasterBill).ToArray();
				nzDeclaration.Bills.RemoveRange(bills);
			}

			var lineNumberAssigner = new LineNumberAssigner(nzDeclaration.CusEntryHeader);
			lineNumberAssigner.Execute();
		}

		protected override void OnAggregateDeclarationImporting(BaseJobDeclaration aggregateDeclaration)
		{
			aggregateDeclaration.JE_VesselName = LeadDeclaration.JE_VesselName;
			aggregateDeclaration.JE_VoyageFlightNo = LeadDeclaration.JE_VoyageFlightNo;
		}

		protected override void OnAggregateDeclarationImported(BaseJobDeclaration aggregateDeclaration)
		{
			var nzAggregateDec = aggregateDeclaration as JobDeclaration;
			if (nzAggregateDec.CusEntryHeader.Messages.Any(msg => !msg.IsInDatabase))
			{
				JobDeclarations.ForEach(dec =>
				{
					dec.LogCustomsCommencedIfNeeded();
				});
			}

			foreach (JobComInvoiceLine invoiceLineToImport in nzAggregateDec.InvoiceLines)
			{
				var invoiceLineToSave = Factory.Load<JobComInvoiceLine>(invoiceLineToImport.PK);
				invoiceLineToSave.JI_HadErrorInLastResponse = invoiceLineToImport.JI_HadErrorInLastResponse;
			}
		}
	}
}
