using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class WarningAcknowledgementForm : ZForm, IPreviousNextControlProvider
	{
		public WarningAcknowledgementForm(GenCustomAddOnRuleAck warningAcknowledgement) : base(warningAcknowledgement)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			tabControl.AllowOutsideOfParent();
			PostingButtonsUserControl.AllowOutsideOfParent();
		}

		public new GenCustomAddOnRuleAck BusinessEntity
		{
			get { return (GenCustomAddOnRuleAck)base.BusinessEntity; }
		}

		[SuppressMessage("Microsoft.Usage", "CA2202:DoNotDisposeObjectsMultipleTimes", Justification = "its in a loop so i see no issues.")]
		void OpenJob_Click(object sender, EventArgs ev)
		{
			try
			{
				var bizO = BusinessEntity.Factory.Load(BusinessEntity.XK_ParentTableCode, BusinessEntity.XK_ParentID);
				SchemaColumn parentID = null;
				SchemaColumn parentCode = null;

				if (bizO != null)
				{
					var schemaColumns = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumns(bizO.TableName);
					try
					{
						foreach (var column in schemaColumns)
						{
							if (column.Name.ToUpper(CultureInfo.InvariantCulture).Contains("PARENTID"))
							{
								parentID = column;
							}
							if (column.Name.ToUpper(CultureInfo.InvariantCulture).Contains("PARENTTABLECODE"))
							{
								parentCode = column;
							}
						}

						if (parentID != null && parentCode != null)
						{
							var t = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(bizO[parentCode].ToString());
							var otherBizo = BusinessEntity.Factory.Load(t, new ZGuid(bizO[parentID]));
							var controller = ZControllerFactory.Instance.GetControllerForBizo(otherBizo);
							controller.ShowEditForm(otherBizo);
						}
						else
						{
							var controller = ZControllerFactory.Instance.GetControllerForBizo(bizO);
							controller.ShowEditForm(bizO);
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ErrorReporter.ReportOnce(String.Format(CultureInfo.InvariantCulture, "While opening a parent something gone wrong, bizO.GetType = {0}, bizO.HumanReadableName = {1}", bizO.GetType(), bizO.HumanReadableName), ex);
						Globals.Message.ShowError(Res.GetString("48E00BAB-4644-4015-A58B-ABEFCFD2011A", "Could not open the parent record for this Warning Acknowledgement. (Maybe it has been deleted from the database.)"), Res.GetString("1B364F29-C42B-4835-B909-51E76193458A", "Support"));
					}
				}
				else
				{
					ErrorReporter.ReportOnce(String.Format(CultureInfo.InvariantCulture, "While opening a parent something gone wrong, bizO is null because ParentTableCode and ParentID in query return null"), new ArgumentException("BizO is null"));
					Globals.Message.ShowError(Res.GetString("48E00BAB-4655-4015-A58B-ABEFCFD2011A", "Could not open the parent record for this Warning Acknowledgement. (Maybe it has been deleted from the database.)"), Res.GetString("1B364F29-C42B-4835-B909-51E76193458A", "Support"));
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce(String.Format(CultureInfo.InvariantCulture, "Some error occured with Warning Acknowledgment"), e);
				Globals.Message.ShowError(Res.GetString("48E00BAB-4666-4015-A58B-ABEFCFD2011A", "(In DEVELOPMENT IGNORE)Could not open the parent record for this Warning Acknowledgement. (Maybe it has been deleted from the database.)"), Res.GetString("1B364F29-C42B-4835-B909-51E76193458A", "Support"));
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ControlDpiScalingHelper.SetTop(ref PostingButtonsUserControl, MainStatusBar.Top - PostingButtonsUserControl.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
		}
	}
}
