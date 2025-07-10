using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	/// <summary>
	/// This is a BO to enable bulk test message sending.
	/// To avoid out-of-memory problems, a light object is used to pass into EntrySummaryMessageBuilder and DutyFeeCalculationManager
	/// </summary>
	public class DummyICusEntryHeader : NonPersistentBusinessObject, ICusEntryHeader, IDutyDataLineHeader, ICusEntryHeaderMessageAttachee, IDutyDataLineHeaderProvider, IObsoleteValidation
	{
		public DummyICusEntryHeader(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public MQEDIMessage Message;

		#region ICusEntryHeader Members

		public ZBool IsPerishable
		{
			get { return false; }
		}

		public ZBool IsSplitShipment
		{
			get { return false; }
		}

		public ZString SplitShipmentReleaseCode
		{
			get { return ZString.Empty; }
		}

		public bool IsRemoteLocationFiling
		{
			get { return false; }
		}

		public ZString PreparerDistrictPort
		{
			get { return ZString.Empty; }
		}

		public ZString PreparerFilerCode
		{
			get { return ZString.Empty; }
		}

		public ZString PreparerOfficeCode
		{
			get { return ZString.Empty; }
		}

		public ZString DistrictPortOfEntry
		{
			get { return "8888"; }
		}

		public ZString ImporterOfRecordNumber
		{
			get { return "91-013199000"; }
		}

		public ZString UltimateConsigneeNumber
		{
			get { return "91-013199000"; }
		}

		public ZString CBPF4811ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		public ZString ImportFTZNumber
		{
			get { return ZString.Empty; }
		}

		public ZBool LiveEntry
		{
			get { return ZBool.False; }
		}

		public ZString MissingDocumentCodes
		{
			get { return ZString.Empty; }
		}

		public ZString BondType
		{
			get { return BondTypeList.Codes.ContinuousBond; }
		}

		public ZString DesignationCode
		{
			get { return ZString.Empty; }
		}

		public ZDate EstimatedEntryDate
		{
			get { return ZDate.Empty; }
		}

		public ZString EntryNumber
		{
			get;
			set;
		}

		public ZString EntryType
		{
			get { return EntryTypeList.Codes.ConsumptionQuotaVisa; }
		}

		public ZString SuretyCode
		{
			get { return "891"; }
		}

		public ZString StateOfDestination
		{
			get { return "IL"; }
		}

		public ZBool OGALineReleaseIndicator
		{
			get { return ZBool.False; }
		}

		public ZBool IsElectronicInvoicing
		{
			get { return false; }
		}

		public ZString ImportingVesselName
		{
			get { return "APL EMERALD"; }
		}

		public ZString ModeOfTransportationCode
		{
			get { return TransportModeCodes.Codes.VesselNonContainer; }
		}

		public ZString DistrictPortOfUnlading
		{
			get { return "8888"; }
		}

		public ZDate DateOfImportation
		{
			get { return ZDateTime.Today.AddDays(1).Date; }
		}

		public ZString BrokerReferenceNumber
		{
			get { return ZString.Empty; }
		}

		public ZString ClientBranchDesignation
		{
			get { return ZString.Empty; }
		}

		public ZString VoyageNumber
		{
			get { return "V123W"; }
		}

		public ZDate EstimatedDateOfArrival
		{
			get { return ZDate.Empty; }
		}

		public ZString LocationOfGoods
		{
			get { return ZString.Empty; }
		}

		public ZBool NAFTAReconciliation
		{
			get { return false; }
		}

		public ZString OtherReconciliationIndicator
		{
			get { return ZString.Empty; }
		}

		public IEnumerable<IBillDetails> LowestBillDetails
		{
			get { yield return new DummyIBillDetail(); }
		}

		public ZDecimal BondAmount
		{
			get { return ZDecimal.Zero; }
		}

		public ZString BondProducerAccountNumber
		{
			get { return ZString.Empty; }
		}

		public ZString EntryFilerCodeOfWarehouseEntry
		{
			get { return ZString.Empty; }
		}

		public ZString WarehouseEntryNumber
		{
			get { return ZString.Empty; }
		}

		public ZString DistrictPortCodeOfWarehouseEntry
		{
			get { return ZString.Empty; }
		}

		public ZBool FinalWarehouseIndicator
		{
			get { return ZBool.False; }
		}

		public ZString ConsolidatedInformalIndicator
		{
			get { return ZString.Empty; }
		}

		public ZString DesignatedExamPort
		{
			get { return ZString.Empty; }
		}

		public ZString CarrierCode
		{
			get { return "MSCU"; }
		}

		public ZString TeamNumber
		{
			get { return ZString.Empty; }
		}

		public ZDecimal InformalFee
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal DutiableMailFee
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal ManualSurcharge
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal BondedADDDuty
		{
			get { return ZDecimal.Zero; }
		}

		public ZBool BondedADDIndicator
		{
			get { return ZBool.False; }
		}

		public ZDecimal PayableADDDuty
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal BondedCVDDuty
		{
			get { return ZDecimal.Zero; }
		}

		public ZBool BondedCVDIndicator
		{
			get { return ZBool.False; }
		}

		public ZDecimal PayableCVDDuty
		{
			get { return ZDecimal.Zero; }
		}

		public ZString ADDCVDSuretyCode
		{
			get { return ZString.Empty; }
		}

		IEnumerable<ICusEntryLine> ICusEntryHeader.EntryLines
		{
			get { return new TypedEnumerable<ICusEntryLine>(EntryLines); }
		}

		public List<DummyICusEntryLine> EntryLines
		{
			get { return entryLines ?? (entryLines = new List<DummyICusEntryLine>()); }
		}
		List<DummyICusEntryLine> entryLines;

		public ZBool IsInvoiceByRequest
		{
			get { return ZBool.False; }
		}

		IEnumerable<IFee> ICusEntryHeader.Fees
		{
			get
			{
				foreach (IFee fee in Fees)
				{
					if (!CusFeeCodeConstants.IsExciseTax(fee.Code))
					{
						yield return fee;
					}
				}
			}
		}

		public List<DummyFee> Fees
		{
			get { return fees ?? (fees = new List<DummyFee>()); }
		}
		List<DummyFee> fees;

		public bool BuildEmpty89EvenIfNoFeeExists
		{
			get { return false; }
		}

		public IAddressDetails UltimateConsignee
		{
			get;
			set;
		}

		public IEnumerable<IContainer> Containers
		{
			get { return Array.Empty<IContainer>(); }
		}

		public ZDecimal TotalEstimatedDuty
		{
			get
			{
				ZDecimal result = 0m;

				foreach (ICusEntryLine entryLine in EntryLines)
				{
					result += entryLine.DutyAmount;

					foreach (ISecondaryTariffLine secondaryLine in entryLine.SecondaryTariffLines)
					{
						result += secondaryLine.Duty;
					}
				}

				return result;
			}
		}

		public ZDecimal TotalEstimatedTax
		{
			get
			{
				ZDecimal result = 0m;

				foreach (ICusEntryLine entryLine in EntryLines)
				{
					result += entryLine.ExciseTax;
				}

				return result;
			}
		}

		public ZString DeferredTaxIndicator
		{
			get { return ZString.Empty; }
		}

		public ZDecimal TotalCountervailingDuty
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal TotalAntidumpingDuty
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal GrandTotalFee
		{
			get
			{
				ZDecimal result = 0;

				foreach (IFee fee in Fees)
				{
					result += fee.Amount;
				}

				return result;
			}
		}

		public ZDecimal GrandTotalOtherRevenueAmount
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal TotalValueOfEntrySummary
		{
			get
			{
				ZDecimal result = 0m;

				foreach (ICusEntryLine entryLine in EntryLines)
				{
					result += entryLine.CL_CustomsValue;
				}

				return result;
			}
		}

		public ZString PaymentTypeIndicator
		{
			get { return PaymentTypeList.Codes.IndividualBasis; }
		}

		public ZDate PreliminaryStatementPrintDate
		{
			get { return ZDate.Empty; }
		}

		public ZString PeriodicStatementMonth
		{
			get { return ZString.Empty; }
		}

		public void CreateDocPrintingDetails(ZGuid outgoingMsgPK)
		{
		}

		public ZBool IsACECargoReleaseCertification
		{
			get { return false; }
		}

		public ZBool IsNonAMS
		{
			get { return false; }
		}

		public ZBool IsSelfCertification
		{
			get { return false; }
		}

		#endregion

		#region IDutyDataLineHeader Members

		bool IDutyDataLineHeader.CalculateChangedLinesOnly => false;

		ZDecimal IDutyDataLineHeader.OriginalTotalCV => 0m;

		void IDutyDataLineHeader.OnCalculating()
		{
		}

		public bool IsHMFApplicable
		{
			get { return false; }
		}

		public bool IsInformalFeeApplicable
		{
			get { return false; }
		}

		public bool IsDutiableMailFeeApplicable
		{
			get { return false; }
		}

		public bool IsHMFDeMinimisApplicable
		{
			get { return false; }
		}

		public ZDateTime DateForMPFCalculation
		{
			get { return ZDateTime.Today; }
		}

		public ZDateTime DateForFeeCalculation
		{
			get { return ZDateTime.Today; }
		}

		public bool AreDutyFeeKnownAndImported
		{
			get { return false; }
		}

		void IDutyDataLineHeader.UpdateAfterHMFDeMinimusRuleApplied()
		{
		}

		public IEnumerable<IEntryLineOrInvoiceLineDutyData> DutyDataLines
		{
			get
			{
				foreach (DummyICusEntryLine line in EntryLines)
				{
					yield return line;

					foreach (DummyICusEntryLine secondaryLine in line.SecondaryLines)
					{
						yield return secondaryLine;
					}
				}
			}
		}

		public void DeleteDetachedEntryLines()
		{
			//TODO For derived duty calculation
		}

		public IFees FeeAndCharges
		{
			get { return new FeeAndCharges(Fees); }
		}

		public ZDecimal? OverridenTotalMPFPayable => null;

		#endregion

		#region IMessageAttacheeInDeclaration Members

		public bool IsActive
		{
			get { return true; }
		}

		public MessageAttacheeRecordType RecordType
		{
			get { return MessageAttacheeRecordType.Entry; }
		}

		public ZString AdditionalReferenceInformation
		{
			get { return ZString.Empty; }
		}

		public ZString RecordTypeDescription
		{
			get { return ZString.Empty; }
		}

		public ZString EntryFilerCode
		{
			get { return "XJ5"; }
		}

		public ZString ProcessingDistrictPort
		{
			get { return "8888"; }
		}

		public ZString ProcessingOfficeCode
		{
			get { return ZString.Empty; }
		}

		public ZString EntryStatus
		{
			get { return ZString.Empty; }
		}

		public ZString HumanFriendlyReference
		{
			get { return ZString.Empty; }
		}

		public ZString JobReferenceNumber
		{
			get { return ZString.Empty; }
		}

		public ZDateTime ReleaseDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime TIBExpiryDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZInt TIBNumOfExtensions
		{
			get { return 0; }
		}

		public IEnumerable<CargoWise.ComponentModel.INotification> GetBusinessLayerNotificationsToAddToWrapper()
		{
			return Array.Empty<CargoWise.ComponentModel.INotification>();
		}

		public ValidationModes ValidationModes
		{
			get;
			set;
		}

		public ZGuid DeclarationPK
		{
			get { return ZGuid.Empty; }
		}

		public Guid CompanyPK
		{
			get { return MasterFiles.Business.GlbCompany.CurrentCompany.PK.ToGuid(); }
		}

		#endregion

		#region IMessageAttachee Members

		public ZString MessageStatus
		{
			get;
			set;
		}

		public ZString MessageStatusDescription
		{
			get { return ZString.Empty; }
		}

		public CBPEDIMessageCollection Messages
		{
			get { return new EDIMessageCollection(this); }
		}

		IReadOnlyList<ZGuid> IMessageAttacheeInDeclaration.ParentPKsOfMessages
		{
			get { return new ZGuid[] { PK }; }
		}

		BusinessObjectFactory IMessageAttachee.Factory
		{
			get { return Factory; }
		}

		public MasterFiles.Business.GlbBranch Branch
		{
			get { return Factory.Load<MasterFiles.Business.GlbBranch>(MasterFiles.Business.GlbBranch.CurrentBranch.PK); }
		}

		public BusinessObject TopLevelBusinessObject
		{
			get { return this; }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return null; }
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return Guid.Empty; }
		}

		Logs IMessageAttachee.TopLevelBusinessObjectLogs
		{
			get { return null; }
		}

		string IMessageAttachee.TopLevelBizObjReferenceNumber
		{
			get { return ""; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.TransportMode
		{
			get { return ZString.Empty; }
		}

		ASESE10 ICusEntryHeader.LastCRAcceptedMessageBlock
		{
			get { return new ASESE10(); }
		}

		AENS10 ICusEntryHeader.LastENSAcceptedMessageBlock
		{
			get { return new AENS10(); }
		}
		#endregion

		#region IDutyDataLineHeaderProvider Members

		public IEnumerable<IDutyDataLineHeader> EntriesToCalculateDutyFeeTax
		{
			get { yield return this; }
		}

		BusinessObjectFactory IDutyDataLineHeaderProvider.Factory
		{
			get { return Factory; }
		}

		bool IDutyDataLineHeaderProvider.IsCustomsChargeRelevantForDecType(string code)
		{
			return true;
		}

		bool IDutyDataLineHeader.DoesMPFSurchargeApply
		{
			get { return false; }
		}

		bool IDutyDataLineHeader.IsCottonFeeDeMinimusApplicable
		{
			get { return true; }
		}

		ZDecimal? IDutyDataLineHeaderProvider.OverridenTotalMPFPayable
		{
			get { return null; }
		}

		#endregion

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (Message != null)
			{
				ZString entryNumber;
				if (EntryNumberGenerator.TryGetNextEntryNumber(Branch, EntryFilerCode, out entryNumber))
				{
					Message.EM_MessageText = Message.EM_MessageText.Replace(MQEDIMessage.USEntryNumberPlaceHolder, entryNumber);
				}
			}
		}

		#region IMessageResponseNotificator Members

		ZString IMessageResponseNotificator.GetFallbackEmailAddressRecipient()
		{
			return ZString.Empty;
		}

		#endregion

	}

	class FeeAndCharges : IFees
	{
		public FeeAndCharges(List<DummyFee> fees)
		{
			this.fees = fees;
		}

		readonly List<DummyFee> fees;

		#region IFees Members

		public IFee GetFeeFor(ZString code)
		{
			return fees.Find(x => x.Code == code);
		}

		public IFee AddNew()
		{
			DummyFee fee = new DummyFee();

			fees.Add(fee);

			return fee;
		}

		#endregion

		#region IEnumerable Members

		public System.Collections.IEnumerator GetEnumerator()
		{
			return fees.GetEnumerator();
		}

		#endregion
	}
}
