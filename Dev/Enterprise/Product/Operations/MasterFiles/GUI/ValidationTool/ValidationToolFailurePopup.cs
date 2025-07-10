using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI;

public partial class ValidationToolFailurePopup : ZChildForm
{
	[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
	public ValidationToolFailurePopup()
	{
		InitializeComponent();
	}

	public ValidationToolFailurePopup(NonPersistentValidationFailure validationFailure) : base(validationFailure)
	{
		InitializeComponent();
		Text = validationFailure.Message;
		LabelStatus.BackColor = GetSeverityColor(validationFailure.Severity);
		ButtonProceed.Enabled = validationFailure.Severity != ProcessTemplateValidationSeverityList.Codes.Error;
	}

	new NonPersistentValidationFailure BusinessEntity => base.BusinessEntity as NonPersistentValidationFailure;

	void ButtonOK_Click(object sender, EventArgs e)
	{
		Close();
	}

	void FailedRuleResultsGrid_ColourDeciding(object sender, ZArchitecture.ColourDecidingEventArgs e)
	{
		if (e.ObjectAtRow is not NonPersistentRuleValidationResult ruleResult)
		{
			return;
		}

		e.Colour = GetSeverityColor(ruleResult.Severity);
	}

	static Color GetSeverityColor(string severity) => severity switch
	{
		ProcessTemplateValidationSeverityList.Codes.Error => Color.Red,
		ProcessTemplateValidationSeverityList.Codes.Message => Color.LightBlue,
		ProcessTemplateValidationSeverityList.Codes.Warning => Color.LightYellow,
		_ => Color.Gray
	};

	void ButtonProceed_Click(object sender, EventArgs e)
	{
		var entity = BusinessEntity;
		var severity = entity?.Severity ?? ZString.Empty;
		switch ((string)severity)
		{
			case ProcessTemplateValidationSeverityList.Codes.Warning:
				DialogResult = DialogResult.Yes;
				break;
			case ProcessTemplateValidationSeverityList.Codes.Message:
				DialogResult = (entity?.HandleMessageErrors() ?? false) ? DialogResult.Yes : DialogResult.No;
				break;
		}
	}

	void ButtonDeliver_Click(object sender, EventArgs e)
	{
		var docWrapper = new NonPersistentValidationFailureDocumentWrapper(BusinessEntity.FailedRuleResults);
		ReportPrintRunner.DeliverWithNotifyModes(WorkflowValidationToolsErrorTemplateReportName, docWrapper, GetNotifyModesList());
	}

	internal static CodeDescriptionPairList GetNotifyModesList()
	{
		var result = new CodeDescriptionPairList();
		result.AddPair(Core.Constants.ContactNotifyModeDescriptions.Email, Core.Constants.ContactNotifyModeDescriptions.Email);
		result.AddPair(Core.Constants.ContactNotifyModeDescriptions.Print, Core.Constants.ContactNotifyModeDescriptions.Print);
		return result;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
	const string WorkflowValidationToolsErrorTemplateReportName = "Workflow Validation Failed Report";
}
