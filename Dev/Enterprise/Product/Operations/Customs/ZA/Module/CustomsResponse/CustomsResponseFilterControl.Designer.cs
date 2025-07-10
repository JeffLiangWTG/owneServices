using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.Module
{
	public partial class CustomsResponseFilterControl
	{


		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.grid.SuspendLayout();
            this.AddStripButton.SuspendLayout();
            this.RecentItemsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // grid
            // 
            this.BindingSource.SetBindingMember(this.grid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CUSRESEDIMessage)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CUSRESEDIMessage)(null)).LocalReferenceNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ZA.Business.CUSRESEDIMessage)(null)).EM_MessageDateTime)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CUSRESEDIMessage)(null)).ReceivingProfile)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CUSRESEDIMessage)(null)).AgentCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CUSRESEDIMessage)(null)).MRNNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CUSRESEDIMessage)(null)).TransportDocumentNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CUSRESEDIMessage)(null)).CustomsOfficeCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CUSRESEDIMessage)(null)).ContainerNumbers)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CUSRESEDIMessage)(null)).EntryStatus)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CUSRESEDIMessage)(null)).EntryStatusDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CUSRESEDIMessage)(null)).LinkedObjectReference)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CUSRESEDIMessage)(null)).EM_MessageType)));
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("329916cd-9d6d-4a69-8e8f-3bc6470485c2", "LRN", "Local Reference Number", "");
            zTextBoxColumnStyleInfo1.ColumnName = "LocalReferenceNumber";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
            zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("ae2800c4-073a-4b2b-8dae-1fa5d5f24f34", "Time Received");
            zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("e1efbb44-01b9-4b2e-b1b0-dc24be951b3a", "Receiving Profile");
            zTextBoxColumnStyleInfo2.ColumnName = "ReceivingProfile";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("911fe2df-ddee-4b06-9b16-1b472e6cb4a7", "Agent");
            zTextBoxColumnStyleInfo3.ColumnName = "AgentCode";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("b51ab7f2-130c-4ab6-83ab-edb8dfe95556", "MRN", "Movement Reference Number", "");
            zTextBoxColumnStyleInfo4.ColumnName = "MRNNumber";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
            zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("44b58ecf-a492-4b12-be19-bc1add6db7b4", "Master Transport Document");
            zTextBoxColumnStyleInfo5.ColumnName = "TransportDocumentNumber";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
            zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("e2a06f92-e0cd-4190-b08a-ce3aef5e12b3", "Customs Office");
            zTextBoxColumnStyleInfo6.ColumnName = "CustomsOfficeCode";
            zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("c0d31123-dcea-48b7-bf66-1e72bb4850ad", "Container");
            zTextBoxColumnStyleInfo7.ColumnName = "ContainerNumbers";
            zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("9daf2938-b4fd-45b3-a3f2-9c91e5ac2d40", "Entry Status");
            zTextBoxColumnStyleInfo8.ColumnName = "EntryStatus";
            zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("811027a4-b978-44dd-b285-e0ebb0c633b0", "Entry Status Description");
            zTextBoxColumnStyleInfo9.ColumnName = "EntryStatusDescription";
            zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
            zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("100a9dd4-4982-44c2-ba48-f7b0352664b1", "Job");
            zTextBoxColumnStyleInfo10.ColumnName = "LinkedObjectReference";
            zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo11.ColumnName = "EM_MessageType";
            zTextBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.CUSRESEDIMessage);
            // 
            // CustomsResponseFilterControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Name = "CustomsResponseFilterControl";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.grid.ResumeLayout(false);
            this.grid.PerformLayout();
            this.AddStripButton.ResumeLayout(true);
            this.AddStripButton.PerformLayout();
            this.RecentItemsPanel.ResumeLayout(false);
            this.RecentItemsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

	}
}