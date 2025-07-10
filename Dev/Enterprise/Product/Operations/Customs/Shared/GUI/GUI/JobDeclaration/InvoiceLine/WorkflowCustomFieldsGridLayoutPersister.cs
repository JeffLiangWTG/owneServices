using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GUI
{
	public class WorkflowCustomFieldsGridLayoutPersister : IDisposable
	{
		public WorkflowCustomFieldsGridLayoutPersister(ZGrid grid, BaseJobDeclaration declaration)
		{
			this.grid = grid;
			this.declaration = declaration;
			HookEvents();
			UpdateGridLayout();
		}
		readonly ZGrid grid;
		readonly BaseJobDeclaration declaration;

		void UpdateGridLayoutForInvoiceHeader(object sender, EventArgs e)
		{
			if (sender is BaseJobComInvoiceHeader invoice && invoice.InvoiceLines.Count > 0)
			{
				UpdateGridLayout(invoice, e);
			}
		}

		void UpdateGridLayout(object sender, EventArgs e)
		{
			bool shouldRegister = sender != null;

			if (ShouldUpdate(e))
			{
				var customFields = new Dictionary<string, ICustomColumnDefinition>();

				var invoiceLines = GetFirstLineOfEachInvoiceHeader();
				foreach (var line in invoiceLines)
				{
					var customBusinessObject = GetCustomBusinessObject(line, shouldRegister);
					if (customBusinessObject is ICustomPropertyContainer container)
					{
						foreach (var property in container.CustomProperties)
						{
							if (!customFields.ContainsKey(property.Identifier))
							{
								customFields.Add(property.Identifier, property.CustomColumnDefinition);
							}
						}
					}
				}

				if (previousCustomFields == null || previousCustomFields.Count != customFields.Count || !customFields.Keys.OrderBy(key => key).SequenceEqual(previousCustomFields.Keys.OrderBy(key => key)))
				{
					WorkflowCustomFieldsGridEditableInitializer.DisplayActiveCustomFieldsAndHideInactiveColumnFields(grid, declaration.InvoiceLines, previousCustomFields, customFields);
					previousCustomFields = customFields;
				}
			}
		}
		Dictionary<string, ICustomColumnDefinition> previousCustomFields;

		bool ShouldUpdate(EventArgs e)
		{
			var result = true;
			var countChangedEventArgs = e as CollectionCountChangedEventArgs;
			if (countChangedEventArgs != null)
			{
				if (countChangedEventArgs.BizObject is BaseJobComInvoiceLine invoiceLine)
				{
					var count = invoiceLine.InvoiceHeader.InvoiceLines.Count;
					if ((countChangedEventArgs.ItemAdded && count != 1) || (countChangedEventArgs.ItemRemoved && count != 0))
					{
						result = false;
					}
				}
			}
			return result;
		}

		List<BaseJobComInvoiceLine> GetFirstLineOfEachInvoiceHeader()
		{
			return declaration.Invoices.
				Select(x => x.InvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault())
				.Where(x => x != null)
				.ToList();
		}

		public void UpdateGridLayout()
		{
			UpdateGridLayout(null, EventArgs.Empty);
		}

		void HookEvents()
		{
			foreach (var invoice in declaration.Invoices)
			{
				HookEventsForInvoiceHeader(invoice);
			}
			if (declaration.IsPersistent)
			{
				HookEventsForDeclaration();
			}
		}

		void UnhookEvents()
		{
			foreach (var invoice in declaration.Invoices)
			{
				UnhookEventsforInvoiceHeader(invoice);
			}
			if (declaration.IsPersistent)
			{
				UnhookEventsForDeclaration();
			}
		}

		void HookEventsForInvoiceHeader(BaseJobComInvoiceHeader invoice)
		{
			foreach (var propertyInfo in BaseJobComInvoiceLine.PropertyThatAffectWorkflowChangedFromInvoiceHeader(invoice))
			{
				propertyInfo.ValueChanged -= UpdateGridLayoutForInvoiceHeader;
				propertyInfo.ValueChanged += UpdateGridLayoutForInvoiceHeader;
			}

			invoice.InvoiceLines.CountChanged -= UpdateGridLayout;
			invoice.InvoiceLines.CountChanged += UpdateGridLayout;
		}

		void UnhookEventsforInvoiceHeader(BaseJobComInvoiceHeader invoice)
		{
			foreach (var propertyInfo in BaseJobComInvoiceLine.PropertyThatAffectWorkflowChangedFromInvoiceHeader(invoice))
			{
				propertyInfo.ValueChanged -= UpdateGridLayoutForInvoiceHeader;
			}

			invoice.InvoiceLines.CountChanged -= UpdateGridLayout;
		}

		void HookEventsForDeclaration()
		{
			foreach (var propertyInfo in BaseJobComInvoiceLine.PropertyThatAffectWorkflowChangedFromDeclaration(declaration))
			{
				propertyInfo.ValueChanged -= UpdateGridLayout;
				propertyInfo.ValueChanged += UpdateGridLayout;
			}

			declaration.Invoices.CollectionCountChange -= HookOrUnhookEventsForInvoiceHeaderWhenCountChanged;
			declaration.Invoices.CollectionCountChange += HookOrUnhookEventsForInvoiceHeaderWhenCountChanged;
		}

		void UnhookEventsForDeclaration()
		{
			foreach (var propertyInfo in BaseJobComInvoiceLine.PropertyThatAffectWorkflowChangedFromDeclaration(declaration))
			{
				propertyInfo.ValueChanged -= UpdateGridLayout;
			}
			declaration.Invoices.CollectionCountChange -= HookOrUnhookEventsForInvoiceHeaderWhenCountChanged;
		}

		void HookOrUnhookEventsForInvoiceHeaderWhenCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.BizObject is BaseJobComInvoiceHeader invoice)
			{
				if (e.ItemAdded)
				{
					HookEventsForInvoiceHeader(invoice);
				}
				else if (e.ItemRemoved)
				{
					UnhookEventsforInvoiceHeader(invoice);
				}
			}
		}

		CustomBusinessObject GetCustomBusinessObject(BaseJobComInvoiceLine line, bool shouldRegister)
		{
			if (shouldRegister)
			{
				line.UnRegisterCustomBusinessObject();
			}

			var customBusinessObject = ((ICustomFieldProvider)line).GetCustomBusinessObject(true);
			if (shouldRegister)
			{
				line.RegisterEditableChildObject(customBusinessObject);
			}
			return customBusinessObject;
		}

		public void Dispose()
		{
			UnhookEvents();
		}
	}
}
