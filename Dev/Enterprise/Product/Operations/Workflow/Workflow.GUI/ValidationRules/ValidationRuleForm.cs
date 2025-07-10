using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Integration.DocumentEngine;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Workflow.GUI
{
	public partial class ValidationRuleForm : ZTemplateForm
	{
		ValidationRuleForm()
		{
			InitializeComponent();
		}

		public ValidationRuleForm(UniversalValidationRuleSet ruleset)
			: base(ruleset)
		{
			InitializeComponent();

			criteriaBox.LostFocus += CriteriaBox_LostFocus;
		}

		protected override bool SupportsEDocs => false;
		protected override bool ShowNotesTab => false;

		#region Criteria

		void CriteriaBox_ReadOnlyChanged(object sender, EventArgs e)
		{
			criteriaPopupButton.ReadOnly = criteriaBox.ReadOnly;
		}

		internal IMapTreePresentationManager mapTreePresenter;

		internal void CriteriaPopupButton_Click(object sender, EventArgs e)
		{
			if (mapTreePresenter == null)
			{
				mapTreePresenter = ObjectFactory.Get<IMapTreePresentationManager>();
				mapTreePresenter.HideDataFields = true;
				mapTreePresenter.DefaultCollectionIndex = 0;
				mapTreePresenter.UseMcrEvaluator = true;
				mapTreePresenter.ShowEditField = true;
				mapTreePresenter.ModalParent = this;
				mapTreePresenter.MacroSelected += MacroSelected;
				mapTreePresenter.MacroMaxLength = criteriaBox.MaxLength;
			}
			mapTreePresenter.ParentTypes = new[] { GetBizTypeForCurrentDataContext() };
			mapTreePresenter.InitialText = criteriaBox.Text;
			mapTreePresenter.XmlType = typeof(UniversalDataBuss.DataObjects.Universal.Shipment);
			mapTreePresenter.OpeningMacroTag = string.Empty;
			mapTreePresenter.ClosingMacroTag = string.Empty;
			mapTreePresenter.ShowPresentationManagerForm(showIndex: true, shouldEscapeAllSpecialCharacters: false);
		}

		protected Type GetBizTypeForCurrentDataContext()
		{
			Type bizType = null;
			var ruleSet = (UniversalValidationRuleSet)DataSource;
			if (Enum.TryParse<DataContextType>(ruleSet.VRS_DataContext, out var dataContextEnum))
			{
				bizType = dataContextEnum.GetUniversalDataContextManager()?.TopLevelBusinessObjectType;
			}
			if (bizType == null)
			{
				bizType = DataContextType.ForwardingShipment.GetUniversalDataContextManager().TopLevelBusinessObjectType;
			}
			return bizType;
		}

		void CriteriaBox_LostFocus(object sender, EventArgs e)
		{
			criteriaBoxLastSelectionStart = criteriaBox.SelectionStart;
			criteriaBoxLastSelectionLength = criteriaBox.SelectionLength;
		}

		int criteriaBoxLastSelectionStart;
		int criteriaBoxLastSelectionLength;

		void MacroSelected(string macro)
		{
			criteriaBox.SelectionStart = criteriaBoxLastSelectionStart;
			criteriaBox.SelectionLength = criteriaBoxLastSelectionLength;
			criteriaBox.SelectedText = macro;

			var macroExpression = criteriaBox.Text;
			if (macroExpression.Length > criteriaBox.MaxLength)
			{
				Globals.Message.Show(Res.GetString("183941A7-30FB-4834-B440-34DE1D9A80FF", "Macro length cannot be greater than {0} symbols, the excess is removed.", criteriaBox.MaxLength));
				criteriaBox.Text = macroExpression.Substring(0, criteriaBox.MaxLength);
			}
		}

		#endregion

		class RuleMacroColumnStyleInfo : ZMacrosFindBoxColumnStyleInfo
		{
			public RuleMacroColumnStyleInfo()
			{
				UsePredefinedRoots = true;
				ShowXmlFields = true;
				DefaultCollectionIndex = 0;
				UseMcrEvaluator = true;
				MacroOpeningBracket = string.Empty;
				MacroClosingBracket = string.Empty;
				IsUsedForExpressions = true;
			}
			public override Type ColumnStyleType => typeof(RuleMacroColumnStyle);

			[DefaultValue(null)]
			public Func<Type> RootTypeFunc { get; set; }

			internal void PrepareForEdit()
			{
				if (RootTypeFunc != null)
				{
					RootTypes = new[] { RootTypeFunc() };
				}
			}
		}

		class RuleMacroColumnStyle : ZMacrosFindBoxColumnStyle
		{
			public RuleMacroColumnStyle(RuleMacroColumnStyleInfo info)
				: base(info)
			{
			}

			protected new RuleMacroColumnStyleInfo ColumnInfo => (RuleMacroColumnStyleInfo)base.ColumnInfo;

			protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellVisible)
			{
				ColumnInfo.PrepareForEdit();
				base.Edit(source, rowNum, bounds, readOnly, instantText, cellVisible);
			}
		}
	}
}
