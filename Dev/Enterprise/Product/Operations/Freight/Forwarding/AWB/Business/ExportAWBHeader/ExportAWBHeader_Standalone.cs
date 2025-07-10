using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public partial class ExportAWBHeader
	{
		CIMEDIMessageDependentCollection _messages;
		public CIMEDIMessageDependentCollection Messages
		{
			get { return _messages ?? (_messages = GetNewMessagesCollection()); }
		}

		CIMEDIMessageDependentCollection GetNewMessagesCollection()
		{
			var result = new CIMEDIMessageDependentCollection(this);
			result.Load();
			return result;
		}

		ExportAWBHeaderDependentCollection _childBills;
		[ChildEditable(true)]
		public ExportAWBHeaderDependentCollection ChildBills
		{
			get { return _childBills ?? (_childBills = GetNewChildBillsCollection()); }
		}

		ExportAWBHeaderDependentCollection GetNewChildBillsCollection()
		{
			var result = new ExportAWBHeaderDependentCollection(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		public ExportAWBHeader ParentBill
		{
			get { return EH_EH_Parent.IsEmpty ? null : Factory.Load<ExportAWBHeader>(EH_EH_Parent); }
		}

		protected virtual void SetDefaultValuesForStandaloneAWB()
		{
			EH_ParentID = PK;
			EH_Table = TableName;
			EH_AWBType = AWBTypeList.Codes.AgentMaster;
			EH_AWBStatus = AWBMessagingStatusList.Codes.NotSent;
			EH_GB_UserBranch = GlbBranch.CurrentBranch.PK;
		}

		[List("Lookups.MessagingStatusList")]
		public override ZString EH_AWBStatus
		{
			get { return base.EH_AWBStatus; }
			set { base.EH_AWBStatus = value; }
		}

		public ZString AWBMessagingStatusDescription
		{
			get { return Lookups.MessagingStatusList.GetDescriptionFromCode(EH_AWBStatus); }
		}

		public ZBool HasBeenSent
		{
			get { return EH_AWBStatus != AWBMessagingStatusList.Codes.NotSent; }
		}
	}
}
