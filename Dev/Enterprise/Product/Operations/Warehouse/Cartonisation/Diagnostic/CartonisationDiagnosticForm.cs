using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Enterprise.Integration.Warehouse;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic
{
	public partial class CartonisationDiagnosticForm : ZChildForm, ICartonisationDiagnosticForm
	{
		public CartonisationDiagnosticForm()
			: base(new CartonisationDiagnostics())
		{
			zLabel2.AllowOverlap(ResultsTextBox);
		}

		CartonisationDiagnostics Diagnostics
		{
			get { return (CartonisationDiagnostics)BusinessEntity; }
		}

		bool TryToSerialize(Action<string> serializeMethod, string fileName, string successMessage, out string failureMessage)
		{
			failureMessage = string.Empty;
			try
			{
				serializeMethod(fileName);
				Globals.Message.Show(successMessage);
				return true;
			}
			catch (Exception e) when (e is XmlException || e is IOException || e is InvalidOperationException || e is ArgumentException)
			{
				failureMessage = e.Message;
				return false;
			}
		}

		void CartonsSavetoXML_Click(object sender, EventArgs e)
		{
			saveFileDialog.FileName = "Cartons.xml";
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				var errorMessage = string.Empty;
				var successMessage = Res.GetString("FCCAE22E-28A5-47FE-86FE-C2A382B339E1", "Carton definitions saved successfully.");
				if (!TryToSerialize(Diagnostics.SaveCartonsToXml, saveFileDialog.UnmappedFileName, successMessage, out errorMessage))
				{
					Globals.Message.ShowError((NoResString)"Couldn't save list of Cartons to xml. " + errorMessage);
				}
			}
		}

		void CartonsLoadFromXML_Click(object sender, EventArgs e)
		{
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				var errorMessage = string.Empty;
				var successMessage = Res.GetString("67D0A587-8666-4C53-B97E-5BCABE04F051", "Carton definitions loaded successfully.");
				if (!TryToSerialize(Diagnostics.LoadCartonsFromXml, openFileDialog.UnmappedFileName, successMessage, out errorMessage))
				{
					Globals.Message.ShowError((NoResString)"Couldn't load list of Cartons from xml. " + errorMessage);
				}
			}
		}

		void ProductsSaveToXML_Click(object sender, EventArgs e)
		{
			saveFileDialog.FileName = "ItemsToPack.xml";
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				var errorMessage = string.Empty;
				var successMessage = Res.GetString("2030D0E3-77CD-4B9E-9939-32FFE5C0C296", "Products definitions saved successfully.");
				if (!TryToSerialize(Diagnostics.SaveItemsToPackToXml, saveFileDialog.UnmappedFileName, successMessage, out errorMessage))
				{
					Globals.Message.ShowError((NoResString)"Couldn't save list of Items-To-Pack to xml. " + errorMessage);
				}
			}
		}

		void ProductLoadFromXML_Click(object sender, EventArgs e)
		{
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				var errorMessage = string.Empty;
				var successMessage = Res.GetString("978A9E20-7F64-44E1-89E7-D3035A7D99A8", "Products definitions loaded successfully.");
				if (!TryToSerialize(Diagnostics.LoadItemsToPackFromXml, openFileDialog.UnmappedFileName, successMessage, out errorMessage))
				{
					Globals.Message.ShowError((NoResString)"Couldn't load list of Items-To-Pack from xml. " + errorMessage);
				}
			}
		}

		void CartoniseButton_Click(object sender, EventArgs e)
		{
			if (Diagnostics.Cartons.Count == 0)
			{
				Globals.Message.ShowError((NoResString)"Please enter some cartons first.");
			}
			else if (Diagnostics.ItemsToPack.Count == 0)
			{
				Globals.Message.ShowError((NoResString)"Please enter some items to pack first.");
			}
			else
			{
				try
				{
					CartoniseButton.Enabled = false;
					Diagnostics.Cartonise();
					ResultsTextBox.Text = Diagnostics.DiagnosticCartonisationResult.ToString();
				}
				finally
				{
					CartoniseButton.Enabled = true;
				}
			}
		}
	}
}



