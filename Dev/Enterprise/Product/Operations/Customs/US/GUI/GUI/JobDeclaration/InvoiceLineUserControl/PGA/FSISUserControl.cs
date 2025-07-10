using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public partial class FSISUserControl : ZUserControl
	{
		public FSISUserControl()
		{
			InitializeComponent();

			CertificateGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("A57A3BB9-BD9D-4605-8BF5-5C1CA13BA00C", "&Print FSIS Form 9540-1 Inspection And Report"), PrintCertificates));
			new ZGridPGADataCorrectionSupporter(CertificateGrid).AddPGALineEditMenu();
		}

		void PrintCertificates(object sender, EventArgs e)
		{
			if (CertificateGrid.SelectedElements.Length > 0)
			{
				JobDeclaration declaration = (JobDeclaration)this.DataSource;
				if (declaration != null)
				{
					declaration.FSISLinesForPrint = CertificateGrid.GetSelectedElements<USInvoiceLineFSISLine>();
					if (AllLotsAreEmpty(declaration.FSISLinesForPrint))
					{
						string message = Res.GetString("B0599240-C8BF-40CB-951B-0F9F944FFC13", "Unable to print FSIS Form 9540-1 as the certificate(s) do not have any lot(s) entered.");
						string caption = Res.GetString("D6AE7472-E2A1-438E-887B-417269B9C30D", "Unable to Print");
						Globals.Message.ShowError(message, caption);
					}
					else
					{
						FSISPrintTask printTask = new FSISPrintTask(declaration);
						printTask.Run();
						printTask.Dispose();
					}
					declaration.FSISLinesForPrint = null;
				}
			}
			else
			{
				string message = Res.GetString("2392541E-6C83-4DE6-B677-518471F8AF2E", "Please select a certificate or certificates before printing.");
				string caption = Res.GetString("37BBC406-BCA9-48E8-A1DD-51FB255354DF", "Select a Certificate");
				Globals.Message.ShowInformation(message, caption);
			}
		}

		bool AllLotsAreEmpty(IEnumerable<USInvoiceLineFSISLine> lines)
		{
			return !lines.Any(x => x.HasFSISLots);
		}

		void ViewEditButton_Click(object sender, EventArgs e)
		{
			var declaration = (JobDeclaration)this.DataSource;
			if (declaration != null)
			{
				var fsis = CurrentDataItem as USInvoiceLineFSISLine;
				var certificateNumber = fsis != null ? fsis.US_HealthCertificateNumber : ZString.Empty;
				ZFormModaliser.ShowDialogAndDispose(new FSISDeclarationForm(declaration, certificateNumber));
			}
		}
	}
}
