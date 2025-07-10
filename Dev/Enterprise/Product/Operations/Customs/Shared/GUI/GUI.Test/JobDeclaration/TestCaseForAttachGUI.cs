using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Moq;

namespace Enterprise.Customs.GUI.Testing
{
	public abstract class TestCaseForAttachGUI : TestCaseWithFactory
	{
		protected BaseJobDeclaration GetNewMockDeclaration()
		{
			return GetNewDeclarationMock().Object;
		}

		protected Mock<BaseJobDeclarationWithEntryInstructions> GetNewDeclarationMock()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			var mockDeclaration = Factory.NewMoq<BaseJobDeclarationWithEntryInstructions>();
			return mockDeclaration;
		}

		protected BaseJobDeclaration GetNewJobDeclaration()
		{
			return GetNewDeclaration();
		}

		protected BaseJobDeclaration GetNewDeclaration()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			var mockDeclaration = Factory.New<BaseJobDeclaration>();
			var invoices = new InvoiceHeaderActiveCollection(mockDeclaration);
			mockDeclaration.Invoices.AddRange(invoices);

			return mockDeclaration;
		}

		protected bool MenuItemsContains(Menu.MenuItemCollection items, string text)
		{
			foreach (MenuItem item in items)
			{
				if (item.Text.IndexOf(text) != -1)
				{
					return true;
				}
			}
			return false;
		}

		protected bool MenuItemsContainsAndVisible(Menu.MenuItemCollection items, MenuItem requiredItem)
		{
			if (requiredItem != null && requiredItem.Visible && !string.IsNullOrEmpty(requiredItem.Text))
			{
				foreach (MenuItem item in items)
				{
					if (item.Text.IndexOf(requiredItem.Text) != -1)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
