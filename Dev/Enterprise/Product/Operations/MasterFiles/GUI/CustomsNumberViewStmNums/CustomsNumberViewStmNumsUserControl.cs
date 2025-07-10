using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CustomsNumberViewStmNumsUserControl : ZUserControl
	{
		[Obsolete("Required by the designer on subclass. Please do not use.")]
		public CustomsNumberViewStmNumsUserControl()
		{
			InitializeComponent();
		}

		public CustomsNumberViewStmNumsUserControl(CustomsNumberViewStmNumsWrapperCollection collection)
		{
			this.collection = collection;
			InitializeComponent();
			SetDataGridGridID();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var stmNumsParent = dataSource as ICustomsNumberViewStmNumsParent;
			if (stmNumsParent != null)
			{
				dataSource = stmNumsParent.CustomsNumberProvider;
			}
			base.SetDataBinding(dataSource, dataMember);
		}

		void SetDataGridGridID()
		{
			NumberRangesGrid.GridId = (Provider?.ProviderKey ?? ZString.Empty) + dataGridGridID;
		}
		const string dataGridGridID = "e4a6e72f-f6c7-4df6-b012-25f688e3fdde";
		protected readonly CustomsNumberViewStmNumsWrapperCollection collection;
		protected CustomsNumberViewStmNumsBusinessProvider Provider => collection?.Provider;
		protected ICustomsNumberViewStmNumsGuiProvider GuiProvider => (ICustomsNumberViewStmNumsGuiProvider)Provider;
		protected ICustomsNumberViewStmNumsParent StmNumsParent => Provider?.Parent;

		#region Number Fountains Event Handlers

		void NumberRangesAddButton_Click(object sender, EventArgs e)
		{
			if (CheckThatStmNumsParentHasNoChanges())
			{
				var newStmNums = LoadOrCreateWrapperInAStandAloneFactory();

				var editorDialogResult = ZFormModaliser.ShowDialogAndDispose(GuiProvider.GetEditorForm(newStmNums));
				if (editorDialogResult == DialogResult.OK && !newStmNums.HasErrors)
				{
					SaveStmNumsStandAloneFactory(newStmNums.Factory);
				}
			}
		}

		void NumberRangesEditButton_Click(object sender, EventArgs e)
		{
			if (CheckThatStmNumsParentHasNoChanges())
			{
				var wrapper = TryGetCurrentWrapper();
				if (wrapper != null)
				{
					var stmNumsReloaded = LoadOrCreateWrapperInAStandAloneFactory(wrapper);
					if (stmNumsReloaded != null)
					{
						var reasonForNotAbleToModify = Provider.GetReasonForNotAbleToModify(wrapper);
						if (reasonForNotAbleToModify.IsEmpty)
						{
							var editorDialogResult = ZFormModaliser.ShowDialogAndDispose(GuiProvider.GetEditorForm(stmNumsReloaded));
							if (editorDialogResult == DialogResult.OK && !stmNumsReloaded.HasErrors)
							{
								SaveStmNumsStandAloneFactory(stmNumsReloaded.Factory);
							}
						}
						else
						{
							Globals.Message.ShowError(reasonForNotAbleToModify);
						}
					}
				}
			}
		}

		void NumberRangesDeleteButton_Click(object sender, EventArgs e)
		{
			if (CheckThatStmNumsParentHasNoChanges())
			{
				var wrapper = TryGetCurrentWrapper();
				if (wrapper != null)
				{
					var canDeleteWrapper = (ICanDelete)wrapper;
					if (canDeleteWrapper.CanDelete)
					{
						var result = Globals.Message.Show(
							Res.GetString("{B531B9AA-1F86-4805-8662-580A5028E0E9}", "This would delete range '{0}'. Continue?", wrapper.Detail),
							Res.GetString("{B6C33F44-934B-4051-99BB-EC289FE6BFDE}", "Confirm"),
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Warning,
							DialogResult.No);

						if (result == DialogResult.Yes)
						{
							var stmNumsReloaded = LoadOrCreateWrapperInAStandAloneFactory(wrapper);
							if (stmNumsReloaded != null)
							{
								stmNumsReloaded.StmNums.Delete();
								if (SaveStmNumsStandAloneFactory(stmNumsReloaded.Factory))
								{
									Provider.CustomsNumberWrappers.Remove(wrapper);
								}
							}
						}
					}
					else
					{
						Globals.Message.ShowError(canDeleteWrapper.ReasonForNotAbleToDelete);
					}
				}
			}
		}

		protected bool CheckThatStmNumsParentHasNoChanges()
		{
			var result = false;
			var providerParent = StmNumsParent;
			if (providerParent != null)
			{
				if (!providerParent.IsInDatabase || providerParent.HasChanges)
				{
					Globals.Message.Show(Res.GetString("{3E9F1DC7-4FA8-402E-B40F-B86B26CB8AA3}", "Please save changes first"));
				}
				else
				{
					result = true;
				}
			}
			return result;
		}

		CustomsNumberViewStmNumsWrapper TryGetCurrentWrapper()
		{
			var wrapper = NumberRangesGrid.GetCurrent() as CustomsNumberViewStmNumsWrapper;
			if (wrapper == null)
			{
				Globals.Message.Show(Res.GetString("{00881635-CFEC-4414-8DCD-3769616FE839}", "Please select a Number Range from the grid"));
			}

			return wrapper;
		}

		protected CustomsNumberViewStmNumsWrapper LoadOrCreateWrapperInAStandAloneFactory(CustomsNumberViewStmNumsWrapper existingWrapper = null)
		{
			var stmNumsStandAloneFactory = new BusinessObjectFactory() { NameForDebugging = "CustomsNumberViewStmNums Standalone Factory", RefreshEnabled = (existingWrapper != null) };
			var provider = GetProviderForStandAloneFactory(stmNumsStandAloneFactory, StmNumsParent.CustomsNumberProviderKey, StmNumsParent.PK);
			CustomsNumberViewStmNums result = null;
			if (existingWrapper == null)
			{
				result = CustomsNumberViewStmNumsHelper.NewStmNums(stmNumsStandAloneFactory, provider, provider.Parent.PK);
			}
			else
			{
				var query = new ZDBOnlyQuery(typeof(CustomsNumberViewStmNums));
				query.AddToFilter(ViewStmNumsSchema.SN_Name, existingWrapper.StmNums.SN_Name);
				query.AddToFilter(ViewStmNumsSchema.SN_Owner, existingWrapper.StmNums.SN_Owner);
				query.OrderBy = ViewStmNums.Schema.SN_SystemCreateTimeUtc;
				result = CustomsNumberViewStmNumsHelper.LoadTop1StmNums(stmNumsStandAloneFactory, query, provider);
			}
			return provider.GetOrCreateWrapper(result);
		}

		protected virtual CustomsNumberViewStmNumsBusinessProvider GetProviderForStandAloneFactory(BusinessObjectFactory factory, string providerKey, ZGuid parentPK)
			=> CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(factory, providerKey, parentPK);

		protected bool SaveStmNumsStandAloneFactory(BusinessObjectFactory standAloneFactory)
		{
			var result = false;
			try
			{
				standAloneFactory.Save();
				result = true;
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			if (Provider != null)
			{
				Provider.CustomsNumbers.RefreshFromDb();
				Provider.NumberRanges.RefreshFromDb();
				Provider.CustomsNumberWrappers.RefreshBinding();
			}
			return result;
		}

		void NumberRangesGrid_AfterBind(object sender, EventArgs e)
		{
			NumberRangesGrid.ListManager.PositionChanged += (NumberRangesGridListManager_PositionChanged);
			NumberRangesGridListManager_PositionChanged(null, null);
		}

		void NumberRangesGridListManager_PositionChanged(object sender, EventArgs e)
		{
			CurrentWrapper = (CustomsNumberViewStmNumsWrapper)NumberRangesGrid.ListManager?.GetCurrent();
		}

		protected CustomsNumberViewStmNumsWrapper CurrentWrapper { get; private set; }

		#endregion

		void ThresholdRunOutWarningGrid_AfterBind(object sender, EventArgs e)
		{
			ThresholdRunOutWarningGrid.ListManager.PositionChanged += (ThresholdRunOutWarningGridListManager_PositionChanged);
			ThresholdRunOutWarningGridListManager_PositionChanged(null, null);
		}

		void ThresholdRunOutWarningGridListManager_PositionChanged(object sender, EventArgs e)
		{
			CurrentThresholdRunOutWarning = (CustomsNumberStmNumberRange)ThresholdRunOutWarningGrid.ListManager?.GetCurrent();
		}

		protected CustomsNumberStmNumberRange CurrentThresholdRunOutWarning { get; private set; }
	}
}
