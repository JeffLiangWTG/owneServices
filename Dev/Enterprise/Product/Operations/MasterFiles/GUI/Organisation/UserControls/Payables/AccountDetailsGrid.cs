using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI.Organisation.UserControls.Payables
{
	public partial class AccountDetailsGrid : ZUserControl
	{
		public AccountDetailsGrid()
		{
			InitializeComponent();

			RemoveColumnsForEPayment();
			UpdateGridForEPayment();
		}

		void UpdateGridForEPayment()
		{
			if (AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.Value.IsEPaymentEnabledForAnyProvider)
			{
				var menuItem = new ZMenuItem(ResString.GetMultilingualString("367f7b7b-efad-4b9e-a4c9-775aa3d777db", "Configure for E-Payment"), new EventHandler(LinkToEPaymentReceipt));
				grid.ContextMenu.MenuItems.Add(menuItem);
				grid.AllowReadOnlyRowsToBeDeleted = true;
			}
		}

		void RemoveColumnsForEPayment()
		{
			var columnsToRemove = new List<string>();
			if (!AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.Value.IsEPaymentEnabledForAnyProvider)
			{
				columnsToRemove.Add("A1_EPaymentReasonCode");
				columnsToRemove.Add("A1_EPaymentReference");
				columnsToRemove.Add("A1_EPaymentReferenceType");
			}

			RemoveColumns(columnsToRemove);
		}

		void RemoveColumns(List<string> columnsToRemove)
		{
			if (columnsToRemove.Any())
			{
				foreach (ZGridColumnInfo columnStyle in grid.ColumnStyles.ToArray())
				{
					if (columnsToRemove.Contains(columnStyle.ColumnName))
					{
						grid.ColumnStyles.Remove(columnStyle);
					}
				}
			}
		}

		void LinkToEPaymentReceipt(object sender, EventArgs e)
		{
			var accountDetails = grid?.ListManager?.GetCurrent() as AccAPAccountDetails;
			if (accountDetails != null)
			{
				if (accountDetails.A1_PaymentMethod != EPaymentMethods.EPaymentViaOFX)
				{
					Globals.Message.Show(Res.GetString("5723AA04-AF63-4476-8A13-0E4704D3B05E", "Only accounts with a payment method of EPO can be configured for E-Payment"));
				}
				else
				{
					if (accountDetails.ModifyPayableAccountDetailsEPaymentSecurity)
					{
						ZController matchRecipientsController = ZControllerFactory.Create(ControllerIDs.MatchEPaymentRecipients);
						matchRecipientsController.SetCollectionForDefaultsAndValidation(new AccountDetailsCollection(accountDetails.Factory) { accountDetails });
						matchRecipientsController.ShowChildrenAsDialog = true;
						matchRecipientsController.ShowNewForm();
					}
					else
					{
						Env.Security.OrgPayablesAccountDetailsEPaymentModify.ShowError();
					}
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("A32E9E6D-6969-436E-AEFE-01AA06B537DB", "Please choose a valid row first"));
			}
		}

		public class AccountDetailsCollection : BusinessObjectCollection<AccAPAccountDetails>
		{
			public AccountDetailsCollection(BusinessObjectFactory factory)
				: base(factory)
			{ }
		}
	}
}
