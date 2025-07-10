using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class CustomsWriteToLogForm : ZChildForm
	{
		public CustomsWriteToLogForm(IStmALogParent topLevelBusinessObject, BusinessObject[] businessObjectList, string caption)
			: this(topLevelBusinessObject, businessObjectList, caption, null)
		{
			this.writeToLog = DefaultWriteToLog;
		}

		public CustomsWriteToLogForm(IStmALogParent topLevelBusinessObject, BusinessObject[] businessObjectList, string caption, Action<BusinessObject[], ZString> writeToLog)
			: base(new BusinessObjectLogger(topLevelBusinessObject))
		{
			this.topLevelBusinessObject = topLevelBusinessObject;
			this.businessObjectList = businessObjectList;
			this.caption = caption;
			this.writeToLog = writeToLog;

			InitializeComponent();
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		public override string FormCaption
		{
			get { return string.IsNullOrEmpty(base.FormCaption) ? caption : base.FormCaption + " " + caption; }
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void WriteToLogButton_Click(object sender, EventArgs e)
		{
			if (topLevelBusinessObject.HasChanges)
			{
				Globals.Message.ShowInformation(Res.GetString("71e87ca4-7c68-4e7a-8acb-08b57eff28f6", "This record needs to be saved. Please save the form first before write to log."));
				Close();
				return;
			}

			var businessEntity = BusinessEntity as BusinessObjectLogger;
			var reference = businessEntity?.Reference ?? ZString.Empty;

			writeToLog?.Invoke(businessObjectList, reference);

			Close();
		}

		void DefaultWriteToLog(BusinessObject[] businessObjects, ZString reference)
		{
			foreach (var businessObject in businessObjects)
			{
				var logger = new BusinessObjectLogger(topLevelBusinessObject, businessObject) { Reference = reference };
				logger.RunPreSaveValidation();

				if (!logger.HasErrors)
				{
					try
					{
						logger.WriteToLog();
					}
					catch (InvalidOperationException ex)
					{
						Globals.Message.ShowError(ex.Message);
						break;
					}
				}
			}
		}

		readonly IStmALogParent topLevelBusinessObject;
		readonly string caption;
		readonly BusinessObject[] businessObjectList;
		readonly Action<BusinessObject[], ZString> writeToLog;
	}
}
