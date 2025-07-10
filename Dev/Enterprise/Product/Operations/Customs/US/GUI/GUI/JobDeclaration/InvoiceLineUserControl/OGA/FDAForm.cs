using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public partial class FDAForm : ZChildForm
	{
		public FDAForm()
		{
		}

		public FDAForm(FDA fda)
			: base(fda)
		{
		}

		FDA FDA
		{
			get { return (FDA)BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			UpdateUS_ContainerDim1CalcEditCaption();
			ShipperAddressAddressControl.Parse = AddressParser.GetAddressPKFromMatchingMIDCode;
			ManufacturerAddressAddressControl.Parse = AddressParser.GetAddressPKFromMatchingMIDCode;

			if (FDA.InvoiceLine == null)
			{
				BillsGroupBox.Visible = false;
				ContainersGroupBox.Visible = false;
				PFRTextBox.Visible = false;
			}
			else
			{
				FDA.US_FDAQty1Info.RefreshBinding();
			}

			FDA.US_FDAContainerDimTypeInfo.ValueChanged += DimType_ValueChanged;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				var fda = FDA;
				if (fda != null)
				{
					fda.US_FDAContainerDimTypeInfo.ValueChanged -= DimType_ValueChanged;
				}
			}
			base.Dispose(disposing);
		}

		void DimType_ValueChanged(object sender, EventArgs e)
		{
			UpdateUS_ContainerDim1CalcEditCaption();
		}

		void UpdateUS_ContainerDim1CalcEditCaption()
		{
			if (FDA != null)
			{
				US_ContainerDim1CalcEdit.CaptionResourceString = FDA.US_FDAContainerDimType == CylindricalRectangularList.Codes.Cylindrical ? Res.GetData("69759E5A-16FF-41BC-9C22-BE3BEFBB755B", "Diameter") : Res.GetData("31415141-0AEE-4CF2-B801-93DC85039ECA", "Width");
				US_ContainerDim1CalcEdit.UpdateCaption();
			}
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (FDA != null)
			{
				var valueFieldsVisible = FDA.ValueFieldsVisible;
				FDAValueTextBox.Visible = valueFieldsVisible;
				InvCurrFDAValueCalcEdit.Visible = valueFieldsVisible;
				FDACurrencyTextBox.Visible = valueFieldsVisible;
				USDCurrencyTextBox.Visible = valueFieldsVisible;
			}
		}
	}
}
