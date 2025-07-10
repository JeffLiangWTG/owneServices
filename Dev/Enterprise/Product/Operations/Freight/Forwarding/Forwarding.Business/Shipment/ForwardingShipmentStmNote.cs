using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentStmNote : StmNote
	{
		public ForwardingShipmentStmNote(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
#if DEBUG
			SetUpForTest();
#endif
			if (ST_Description == PredefinedNoteTypes.Instance.OrderUpdateHistory.Code)
			{
				ErrorReporter.ReportOnce("ForwardingShipmentStmNote_ST_Description_Is_OrderUpdateHistoryCode", $"ForwardingShipmentStmNote constructed from an OrderUpdateHistoryStmNote's DataRow.\r\nFactory Name: {factory.NameForDebugging}\r\nRow Data: {string.Join(";", row.ItemArray)}"); // Developer Info
			}
		}

#if DEBUG
		protected virtual void SetUpForTest()
		{
		}
#endif

		#region Implementation

		protected override StmNoteValidation GetNewValidation()
		{
			return new ForwardingShipmentStmNoteValidation(this);
		}

		protected new bool ST_NoteText_ReadOnly
		{
			get
			{
				if (ST_Description.EqualsIgnoringCase(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Code))
				{
					return Shipment.IsPropertyReadOnlyDueToPhase(Enterprise.Freight.Business.CommonShipment.Schema.DetailedGoodsDescriptionNoteText);
				}
				else if (ST_Description.EqualsIgnoringCase(PredefinedNoteTypes.Instance.MarksAndNumbers.Code))
				{
					return Shipment.IsPropertyReadOnlyDueToPhase(Enterprise.Freight.Business.CommonShipment.Schema.JS_MarksAndNumbers);
				}
				else if (ST_Description.EqualsIgnoringCase(PredefinedNoteTypes.Instance.OriginalBillNotes.Code))
				{
					return true;
				}

				return base.ST_NoteText_ReadOnly;
			}
		}

		ForwardingShipment Shipment => shipment ?? (shipment = Factory.Load<ForwardingShipment>(ST_ParentID));

		ForwardingShipment shipment;

		public override void OnSaving()
		{
			base.OnSaving();

			if (ST_IsTextOnly
				&& ST_NoteText.IsEmpty
				&& (ST_Description == PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description
					|| ST_Description == PredefinedNoteTypes.Instance.MarksAndNumbers.Description))
			{
				this.Delete();
			}
		}

		#endregion
	}
}
