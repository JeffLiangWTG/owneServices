using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class InvoiceLineLinkControllingMsgHeaderCollection : NonPersistentBusinessObjectCollection<InvoiceLineLinkControllingMsgHeader>
	{
		public InvoiceLineLinkControllingMsgHeaderCollection(JobComInvoiceLine supporter, Action validationAction)
			: base(supporter.Factory)
		{
			invoiceLine = supporter;
			this.validationAction = validationAction;
		}

		readonly Action validationAction;

		InvoiceLineRelatedControllingMsgHeadersGenPivotCollection GenPivots => invoiceLine.InvoiceLineRelatedControllingMsgHeadersGenPivots;

		readonly JobComInvoiceLine invoiceLine;

		#region Implement

		public void DeleteGenPivots()
		{
			GenPivots.RemoveAndDeleteAll();
		}

		public void SaveGenPivots()
		{
			var links = GenPivots;
			if (!invoiceLine.IsDeleted)
			{
				var controllingMessageHeaders = invoiceLine.EntryInstruction?.ControllingMessageHeaders;
				if (controllingMessageHeaders != null)
				{
					DeletePivotsIfNotExists(links, controllingMessageHeaders);
				}
			}
			HasChanges = false;
		}

		void DeletePivotsIfNotExists(InvoiceLineRelatedControllingMsgHeadersGenPivotCollection links, CusTWControllingMessageHeaderCollection controllingMessageHeaders)
		{
			var controllings = controllingMessageHeaders.Cast<CusTWControllingMessageHeader>().ToArray();
			foreach (var pivot in links.Cast<InvoiceLineRelatedControllingMsgHeadersGenPivot>().ToArray())
			{
				if (!controllings.Any(x => x.PK == pivot.Relation2ID))
				{
					pivot.Delete();
				}
			}
		}

		public void ClearAndBuildElements()
		{
			DeleteGenPivots();
			RebuildElements();
		}

		public void RebuildElements(bool shouldValidation = true)
		{
			RemoveAll();
			BuildElements(shouldValidation);
		}

		void BuildElements(bool shouldValidation = true)
		{
			var controllingMessageHeaders = invoiceLine?.EntryInstruction?.ControllingMessageHeaders;

			if (controllingMessageHeaders != null)
			{
				controllingMessageHeaders.CountChanged -= SyncBuildElements;

				var links = GenPivots;
				var controllings = controllingMessageHeaders.Cast<CusTWControllingMessageHeader>().ToArray();
				foreach (var invoiceLineLinkControllingMsgHeader in this.Cast<InvoiceLineLinkControllingMsgHeader>().ToArray())
				{
					if (!controllings.Any(x => x.PK == invoiceLineLinkControllingMsgHeader.ControllingMessageHeaderPK))
					{
						Remove(invoiceLineLinkControllingMsgHeader);
					}
				}

				foreach (var controllingMessageHeader in controllings)
				{
					if (!this.Cast<InvoiceLineLinkControllingMsgHeader>().Any(x => x.ControllingMessageHeaderPK == controllingMessageHeader.PK))
					{
						AddControllingMsgHeader(controllingMessageHeader, links.Contains(controllingMessageHeader));

						AddRefreshBindingEvent(controllingMessageHeader.TW1_ControllingAgencyInfo);
						AddRefreshBindingEvent(controllingMessageHeader.TW1_FunctionalReferenceIdInfo);
						AddRefreshBindingEvent(controllingMessageHeader.PermitNumberInfo);
						AddRefreshBindingEvent(controllingMessageHeader.TW1_ControllingMessageTypeInfo);
						AddRefreshBindingEvent(controllingMessageHeader.TW1_BusinessTypeInfo);
					}
				}

				foreach (var invoiceLineLinkControllingMsgHeader in this.Cast<InvoiceLineLinkControllingMsgHeader>().ToArray())
				{
					invoiceLineLinkControllingMsgHeader.IsLinkedCMHeaderInfo.ValueChanged -= IsLinkedCMHeaderInfo_ValueChanged;
					invoiceLineLinkControllingMsgHeader.IsLinkedCMHeaderInfo.ValueChanged += IsLinkedCMHeaderInfo_ValueChanged;
				}

				DeletePivotsIfNotExists(links, controllingMessageHeaders);
				controllingMessageHeaders.CountChanged += SyncBuildElements;
			}
			if (shouldValidation)
			{
				validationAction?.Invoke();
			}
		}

		void IsLinkedCMHeaderInfo_ValueChanged(object sender, EventArgs e)
		{
			validationAction?.Invoke();
		}

		void AddRefreshBindingEvent(ZPropertyInfo propertyInfo)
		{
			propertyInfo.ValueChanged -= SyncRefreshBinding;
			propertyInfo.ValueChanged += SyncRefreshBinding;
		}

		void SyncBuildElements(object sender, EventArgs e)
		{
			BuildElements();
			RefreshBinding();
		}

		void SyncRefreshBinding(object sender, EventArgs e)
		{
			RefreshBinding();
			validationAction?.Invoke();
		}

		void AddControllingMsgHeader(CusTWControllingMessageHeader controllingMessageHeader, ZBool isLinkedCMHeader)
		{
			var invoiceLineLinkControllingMsgHeader = new InvoiceLineLinkControllingMsgHeader(controllingMessageHeader, invoiceLine);
			using (invoiceLineLinkControllingMsgHeader.SuspendSettingHasChanges())
			using (invoiceLineLinkControllingMsgHeader.SuspendLinkSetting())
			using (invoiceLineLinkControllingMsgHeader.GetValidationSuspender())
			{
				invoiceLineLinkControllingMsgHeader.IsLinkedCMHeader = isLinkedCMHeader;
				Add(invoiceLineLinkControllingMsgHeader);
			}
		}

		#endregion

		#region override

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override bool AllowSort => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new InvoiceLineLinkControllingMsgHeader(null, invoiceLine);
		}

		public override void Load()
		{
			RebuildElements(false);
		}
		#endregion
	}
}
