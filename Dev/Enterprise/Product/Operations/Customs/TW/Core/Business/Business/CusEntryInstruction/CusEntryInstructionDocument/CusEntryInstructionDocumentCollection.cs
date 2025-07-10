using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class CusEntryInstructionDocumentCollection : CusCodeDataCollection<CusEntryInstructionDocument>, IEnumerable<CusEntryInstructionDocument>
	{
		public CusEntryInstructionDocumentCollection(CusEntryInstruction parent)
			: base(parent, CusCodeDataTypeList.Codes.AttachedDocumentNumber)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = base.CreateRelationshipFilter();
			filter.OrderBy = CusEntryInstructionDocument.Schema.CY_Order;
			return filter;
		}

		const int MaxCountOfDocuments = 3;

		public ZString DocumentNumbersAsString
		{
			get => ZString.Join(",", this.Cast<CusEntryInstructionDocument>().OrderBy(x => x.CY_Order).Select(x => x.CY_Data).Where(x => !x.IsEmpty).Take(MaxCountOfDocuments).ToArray());
			set
			{
				if (DocumentNumbersAsString != value)
				{
					using (SuppressDocumentNumbersChangedEvents())
					{
						RemoveAndDeleteAll();
						foreach (ZString number in value.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Take(MaxCountOfDocuments))
						{
							var document = AddNew();
							document.CY_Data = number.Left(document.CY_DataInfo.MaxLength);
						}
						ResetOrder();
					}
					OnDocumentNumbers_ValueChanged(this, EventArgs.Empty);
				}
			}
		}

		#region Event handling

		protected override void OnAdded(BusinessObject businessObject)
		{
			base.OnAdded(businessObject);
			if (businessObject is CusEntryInstructionDocument cusEntryInstructionDocument)
			{
				cusEntryInstructionDocument.CY_Order = (ZShort)Count;
				cusEntryInstructionDocument.CY_DataInfo.ValueChanged -= OnDocumentNumbers_ValueChanged;
				cusEntryInstructionDocument.CY_DataInfo.ValueChanged += OnDocumentNumbers_ValueChanged;
				OnDocumentNumbers_ValueChanged(this, EventArgs.Empty);
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (bizO is CusEntryInstructionDocument businessObject)
			{
				ResetOrder();
				businessObject.CY_DataInfo.ValueChanged -= OnDocumentNumbers_ValueChanged;
				OnDocumentNumbers_ValueChanged(this, EventArgs.Empty);
			}
		}

		void OnDocumentNumbers_ValueChanged(object sender, EventArgs e)
		{
			if (!IsDocumentNumbersChangedEventsSuspended)
			{
				DocumentNumbersChanged?.Invoke(sender, e);
			}
		}

		public event EventHandler DocumentNumbersChanged;

		public void ClearHasChanges()
		{
			foreach (CusEntryInstructionDocument obj in this)
			{
				obj.ClearHasChanges();
			}
		}

		void ResetOrder()
		{
			for (var i = 0; i < Count; i++)
			{
				this[i].CY_Order = (ZShort)i + 1;
			}
		}

		#endregion

		#region Document Numbers Changed Events Suspender

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		int documentNumbersChangedEventsSuspender;

		bool IsDocumentNumbersChangedEventsSuspended => documentNumbersChangedEventsSuspender != 0;

		IDisposable SuppressDocumentNumbersChangedEvents()
		{
			documentNumbersChangedEventsSuspender++;
			return new DisposableAction(delegate
			{ documentNumbersChangedEventsSuspender--; });
		}

		#endregion

		protected override bool AllowNewCore => Count < MaxCountOfDocuments;

		protected override bool AllowSort => false;
	}
}
