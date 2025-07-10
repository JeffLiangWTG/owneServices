using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	[DependentBusinessObject(typeof(CusWHSOperatorTransactionBatch), "WarehouseOperatorTransactions")]
	[CodeProperty(CusWHSOperatorTransaction.Schema.WOT_OwnerReference)]
	public class CusWHSOperatorTransaction : AutoCusWHSOperatorTransaction
	{
		public CusWHSOperatorTransaction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("8DBCC7D9-96E0-45AC-8D90-9A44649CAB69", "Warehouse Operator Transaction");

		[RelatedBusinessObject(nameof(Batch))]

		#region property overrides

		[ResourceStringData("52620C80-46A9-41D2-9BED-3B6C9A1CF2BE", Caption = "Batch")]
		public override ZGuid WOT_WOB_CusWHSTransactionBatch
		{
			get => base.WOT_WOB_CusWHSTransactionBatch;
			set => base.WOT_WOB_CusWHSTransactionBatch = value;
		}

		[ResourceStringData("EFA9390D-23DB-49BC-9DE4-7FA820D87E0A", Caption = "Total Value")]
		public override ZDecimal WOT_TotalValue { get => base.WOT_TotalValue; set => base.WOT_TotalValue = value; }

		[ResourceStringData("A5C252FF-339D-41B6-8B36-08AB46A07EA5", Caption = "Line Reference")]
		public override ZString WOT_LineReference { get => base.WOT_LineReference; set => base.WOT_LineReference = value; }

		[ResourceStringData("F904AD09-11FE-43A6-BC72-997D087E59B0", Caption = "Batch Line No.")]
		public override ZInt WOT_BatchLineNo { get => base.WOT_BatchLineNo; set => base.WOT_BatchLineNo = value; }

		[ResourceStringData("D0AE9356-350C-4ECA-BF60-7384C56E28E3", Caption = "Ctry/Rgn. of Origin")]
		public override ZString WOT_RN_NKOrigin { get => base.WOT_RN_NKOrigin; set => base.WOT_RN_NKOrigin = value; }

		[ResourceStringData("0E7A0BF2-480C-4AE1-96A9-2B282DD03617", Caption = "Is Customs Controlled")]
		public override ZBool WOT_IsCustomsControlled { get => base.WOT_IsCustomsControlled; set => base.WOT_IsCustomsControlled = value; }

		[ResourceStringData("6AA5E9B7-F238-4552-83B7-C99673A08EAB", Caption = "Status")]
		public override ZString WOT_Status { get => base.WOT_Status; set => base.WOT_Status = value; }

		[ResourceStringData("BF0D16B3-3CF9-467B-B78E-F3E3EA01BB2A", Caption = "Quantity")]
		public override ZDecimal WOT_Quantity { get => base.WOT_Quantity; set => base.WOT_Quantity = value; }

		[ResourceStringData("952D6239-512B-408C-8CAE-ED20AB376B41", Caption = "Owner Reference")]
		public override ZString WOT_OwnerReference { get => base.WOT_OwnerReference; set => base.WOT_OwnerReference = value; }

		[ResourceStringData("CAA3E5E3-E473-4050-AF25-684FC2B067B9", Caption = "Transaction Date")]
		public override ZDate WOT_TransactionDate { get => base.WOT_TransactionDate; set => base.WOT_TransactionDate = value; }

		[ResourceStringData("FD168551-3265-4790-9059-F3D58B809529", Caption = "Export Type")]
		public override ZString WOT_ExportType { get => base.WOT_ExportType; set => base.WOT_ExportType = value; }

		[ResourceStringData("567CC280-4B0D-4784-9A53-717EDCAE0D3F", Caption = "Type")]
		public override ZString WOT_TransactionType { get => base.WOT_TransactionType; set => base.WOT_TransactionType = value; }

		[ResourceStringData("E314879C-6091-4D84-BD9D-10C6C4D8FE4B", Caption = "Currency")]
		public override ZString WOT_RX_NKCurrency { get => base.WOT_RX_NKCurrency; set => base.WOT_RX_NKCurrency = value; }

		[ResourceStringData("5236795B-2BD9-4F3D-85E7-946BC4C392AB", Caption = "Customs Entry Number")]
		public override ZString WOT_CustomsEntryNumber { get => base.WOT_CustomsEntryNumber; set => base.WOT_CustomsEntryNumber = value; }

		[ResourceStringData("68E75227-0B22-488E-AFF1-3552A24505BF", Caption = "Into Bond Date")]
		public override ZDate WOT_IntoBondDate { get => base.WOT_IntoBondDate; set => base.WOT_IntoBondDate = value; }

		[ResourceStringData("3392C787-6867-4549-AA67-B1FE8DF194D3", Caption = "Is Final")]
		public override ZBool WOT_IsFinal { get => base.WOT_IsFinal; set => base.WOT_IsFinal = value; }

		[ResourceStringData("572B6E12-FF54-4BD0-A351-7CC40F379B24", Caption = "Created Time")]
		public override ZDateTime WOT_SystemCreateTimeUtc { get => base.WOT_SystemCreateTimeUtc; set => base.WOT_SystemCreateTimeUtc = value; }

		[ResourceStringData("028D24EA-D152-48B0-8E2D-650C1143FCCF", Caption = "Creating User")]
		public override ZString WOT_SystemCreateUser { get => base.WOT_SystemCreateUser; set => base.WOT_SystemCreateUser = value; }

		[ResourceStringData("68D25F41-988F-44AF-9495-E0DEFA3603DA", Caption = "Available Quantity")]
		public ZDecimal AvailableQuantity
		{
			get
			{
				var qty = base.WOT_Quantity;
				return qty -= TransactionLines.Cast<CusWHSOperatorTransactionLine>().Sum(x => x.WOL_Quantity);
			}
		}

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			WOT_IsCustomsControlled = false;
			WOT_RN_NKOrigin = ZString.Empty;
		}

		public CusWHSOperatorTransactionBatch Batch
		{
			get { return Factory.Load<CusWHSOperatorTransactionBatch>(WOT_WOB_CusWHSTransactionBatch); }
		}

		[ResourceStringData("5F7A5C94-FC79-464D-A074-DA529906E84E", Caption = "Canceled By")]
		public ZString CancelledBy => CancelledLog?.SL_GS_NKUser ?? ZString.Empty;

		[ResourceStringData("03FCEED7-7630-4729-B786-193337CF92B1", Caption = "Canceled Time")]
		public ZDateTime CancelledDate => CancelledLog?.SL_EventTime ?? ZDateTime.Empty;

		StmALog CancelledLog
		{
			get
			{
				if (cancelledLog == null)
				{
					cancelledLog = Logs.GetAllLogs().Where(x => x.SL_Reference == WarehouseOperatorTransactionStatusList.Codes.CAN).OrderByDescending(x => x.SL_EventTime).FirstOrDefault();
				}
				return cancelledLog;
			}
		}

		StmALog cancelledLog;

		public ZBool IsOrder => WOT_TransactionType == WarehouseOperatorTransactionTypeList.Codes.ORD;

		public ZBool IsReceipt => WOT_TransactionType == WarehouseOperatorTransactionTypeList.Codes.REC;

		public ZBool IsACancelledOrder => WOT_Status == WarehouseOperatorTransactionStatusList.Codes.CAN && WOT_TransactionType == WarehouseOperatorTransactionTypeList.Codes.ORD;

		[ChildEditable]
		public CusWHSOperatorTransactionLineCollection TransactionLines
		{
			get
			{
				if (transactionLines == null)
				{
					transactionLines = GetNewCusWHSOperatorTransactionLineCollection(Factory, this);
					transactionLines.Load();
					RegisterEditableChildObject(transactionLines);
				}
				return transactionLines;
			}
		}
		CusWHSOperatorTransactionLineCollection transactionLines;

		protected virtual CusWHSOperatorTransactionLineCollection GetNewCusWHSOperatorTransactionLineCollection(BusinessObjectFactory factory, CusWHSOperatorTransaction transaction) => new CusWHSOperatorTransactionLineCollection(factory, transaction);

		protected override bool SupportsCloneCore() => true;
	}
}
