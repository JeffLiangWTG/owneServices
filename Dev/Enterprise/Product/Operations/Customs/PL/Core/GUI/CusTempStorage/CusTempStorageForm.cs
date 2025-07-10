using System.Windows.Forms;
using Enterprise.Customs.PL.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.CusTempStorage;

public partial class CusTempStorageForm : ZTemplateForm
{
	public CusTempStorageForm(CusTempStorageJobHeader header)
		: base(header)
	{
		FetchOriginalValues(header);
		InitializeComponent();
		InitializeComponentExtend(header);
		SetDataBinding(header, string.Empty);
		AddMessagingMenu();
		WorkflowTabPage.Initialize(header);
	}

	#region Form Caption
	public override string FormCaption
	{
		get
		{
			string caption = FormCaptionCore;
			var header = Header;
			if (!header.SJH_JobReference.IsEmpty && header.Customer != null && !header.Customer.OH_Code.IsEmpty && header.Branch != null && !header.Branch.GB_Code.IsEmpty)
			{
				var sb = new CargoWise.Types.ZStringBuilder();
				caption = sb.Append(FormCaptionCore).Append(header.SJH_JobReference).Append(header.Customer.OH_Code).Append(header.Branch.GB_Code).ToStringWithDelimiterBetweenAppends(" - ");
			}
			return Res.GetString("D94DE296-8C52-4D48-A795-D136A0CFDC78", "{0}", caption);
		}
	}
	protected string FormCaptionCore => (NoResString)"Temporary Storage Declaration";
	#endregion

	TemporyStorageUserControlForPlugin GetTemporyStorageUserControlForPlugin(CusTempStorageJobHeader jobHeader)
	{
		return new TemporyStorageUserControlForPlugin();
	}

	void InitializeComponentExtend(CusTempStorageJobHeader header)
	{
		userControlForPLugin = GetTemporyStorageUserControlForPlugin(header);
		this.userControlForPLugin.SuspendLayout();
		this.MainTabPage.Controls.Add(this.userControlForPLugin);
		// 
		// userControlForPLugin
		// 
		this.userControlForPLugin.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.userControlForPLugin, ".");
		this.userControlForPLugin.Dock = DockStyle.Fill;
		this.userControlForPLugin.Name = "userControlForPLugin";
		this.userControlForPLugin.TabIndex = 0;

		this.userControlForPLugin.ResumeLayout(true);
		this.userControlForPLugin.PerformLayout();
	}

	protected override bool SupportsEDocs
	{
		get { return true; }
	}

	protected override bool ShowNotesTab
	{
		get { return true; }
	}

	void AddMessagingMenu()
	{
		var messagingMenu = new CusTempStorageFormMenu() { Header = Header };
		MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(ActionsMenuItem), messagingMenu);
	}

	void FetchOriginalValues(CusTempStorageJobHeader header)
	{
		var localFactory = header.Factory.CreateNewFactory();

		localFactory.Load<CusTempStorageJobHeader>(header.PK);
	}

	CusTempStorageJobHeader Header => (CusTempStorageJobHeader)BusinessEntity;
}
