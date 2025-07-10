using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business
{
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsPutawayJob_PreventFinalizeIfLinesAreUnfinalized, WhsValidationHelper.WhsCheckPreventFinalizeIfLinesAreUnfinalized, WhsPutawayJobSchema.Constants.PK, typeof(IWhsCheckPreventFinalizeIfLinesAreUnfinalized_DeferTriggerStrategy))]
	public class WhsPutawayJob : AutoWhsPutawayJob, IWhsLogEventParent
	{
		public WhsPutawayJob(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		#region Warehouse

		public WhsWarehouse Warehouse => Factory.Load<WhsWarehouse>(WPJ_WW_Warehouse);

		#endregion

		#region Lines

		public WhsPutawayLineCollection Lines => lines ?? (lines = GetWhsPutawayLinesCollection());
		WhsPutawayLineCollection lines;

		WhsPutawayLineCollection GetWhsPutawayLinesCollection()
		{
			var putawayLines = new WhsPutawayLineCollection(this);
			putawayLines.ApplySort(WhsPutawayLineSchema.Constants.WPL_PalletID, ListSortDirection.Ascending);
			return putawayLines;
		}

		#endregion

		#endregion

		#region BizoOverrides

		public override void Delete()
		{
			Lines.DeleteAll();
			base.Delete();
		}

		#endregion

		#region Properties

		[RelatedBusinessObject("Warehouse")]
		public override ZGuid WPJ_WW_Warehouse
		{
			get => base.WPJ_WW_Warehouse;
			set => base.WPJ_WW_Warehouse = value;
		}

		public ZString WPJ_EventFreeTextReference => FormattableString.Invariant($"{Warehouse.WW_WarehouseCode} {User.GS_Code} {WPJ_SystemCreateTimeUtc.ToSmallDateTime()}"); // Log value

		public bool IsFinalised => WPJ_FinalizedTimeUtc.IsValid;

		#endregion

		#region IWhsLogEventParent Members

		ZString IWhsLogEventParent.EventFreeTextReference => WPJ_EventFreeTextReference;

		string IWhsLogEventParent.EventReferenceParameterType => Constants.EventReferenceParameterTypes.PutawayJob;

		#endregion
	}
}
