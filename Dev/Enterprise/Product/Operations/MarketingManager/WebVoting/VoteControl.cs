using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Common;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.MarketingManager.WebVoting
{
	public class VoteControl : CompositeControl, INamingContainer
	{
		#region Static New

		public static VoteControl New(GlbCompanyCampaign campaign)
		{
			var overridden = OverridableNewDelegate.Value;
			return (overridden != null) ? overridden(campaign) : new VoteControl();
		}

#if DEBUG
		internal
#endif
		delegate VoteControl NewDelegate(GlbCompanyCampaign campaign);

#if DEBUG
		internal
#endif
		static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

#if DEBUG
		internal
#endif
		VoteControl()
		{
		}

		#endregion

		internal ZTextLabel VoteHeaderLabel;
		HtmlGenericControl VotingItemTable;
		internal ZRepeater VotingItemRepeater;
		internal HtmlGenericControl PagingPanel;
		internal ZNumericDropDownList PagingDropDownList;
		internal ZNumericLabel TotalPageLabel;

		#region CreateChildControls

		protected override void CreateChildControls()
		{
			Controls.Clear();

			VoteHeaderLabel = new ZTextLabel();
			VoteHeaderLabel.CssClass = "voteHeader";
			VoteHeaderLabel.BindTo = "CompanyCampaign.VoteHeader.HY_Question";

			CreateVotingItemControls();
			CreatePagingControls();

			Controls.Add(VoteHeaderLabel);
			Controls.Add(VotingItemTable);
			Controls.Add(PagingPanel);
		}

		void CreateVotingItemControls()
		{
			VotingItemTable = new HtmlGenericControl("TABLE");
			VotingItemTable.Attributes["border"] = "0";

			VotingItemRepeater = new ZRepeater();
			VotingItemRepeater.BindTo = "PagedAnswerWrappers";
			VotingItemRepeater.ItemDataBound += VotingItemRepeater_ItemDataBound;
			VotingItemRepeater.HeaderTemplate = new GenericTemplateImplementation(ConstructHeaderRow);

			VotingItemTable.Controls.Add(VotingItemRepeater);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		void CreatePagingControls()
		{
			PagingPanel = new HtmlGenericControl("DIV");
			PagingPanel.Attributes["class"] = "votePagingButtonPanel";

			LiteralControl literal1 = new LiteralControl("Page ");
			LiteralControl literal2 = new LiteralControl(" of ");

			PagingDropDownList = new ZNumericDropDownList();
			PagingDropDownList.BindTo = "CurrentPage";
			PagingDropDownList.DisplayStyle = OComboBoxDropDownStyle.CodeOnly;
			PagingDropDownList.BindToList = "PageNumbers";
			PagingDropDownList.AutoPostBack = true;

			TotalPageLabel = new ZNumericLabel();
			TotalPageLabel.BindTo = "PageCount";

			// Page <CurrentPage> of <PageCount>
			PagingPanel.Controls.Add(literal1);
			PagingPanel.Controls.Add(PagingDropDownList);
			PagingPanel.Controls.Add(literal2);
			PagingPanel.Controls.Add(TotalPageLabel);
		}

		#endregion

		#region Paging Support

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			PagingPanel.Visible = AnswerSet.PageCount > 1;
		}

#if DEBUG
		protected internal void OnPreRenderInternal(EventArgs e) => OnPreRender(e);
#endif

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			AnswerSet.PagedAnswerWrappers.Rebuilt += RebindVotingItemRepeater;
		}

#if DEBUG
		protected internal void OnLoadInternal(EventArgs e) => OnLoad(e);
#endif

		void RebindVotingItemRepeater(object sender, EventArgs e)
		{
			VotingItemRepeater.DataBind();
		}

		#endregion

		protected void VotingItemRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				ConstructVotingItem(e);
				BindControls(e.Item, e.Item.DataItem);
			}
		}

		#region Implementation

		bool IsVotingItemHeader(VoteExamSurveyAnswerWrapperBase answerWrapper)
		{
			return answerWrapper.Question.IsHeader;
		}

		protected virtual void ConstructHeaderRow(Control headerRowContainer)
		{
			var headerRow = new HtmlTableRow();
			var questionNumberHeaderCell = new HtmlTableCell();
			questionNumberHeaderCell.Attributes["class"] = "votingItemNumberHeader";
			questionNumberHeaderCell.InnerText = Res.GetString("059C342C-3CB5-4BFD-A159-E5C1C8B0599E", "No.");
			headerRow.Cells.Add(questionNumberHeaderCell);

			var questionHeaderCell = new HtmlTableCell();
			questionHeaderCell.Attributes["class"] = "votingItemHeader";
			questionHeaderCell.InnerText = Res.GetString("7EA41810-B963-45C6-AB7E-A13100DC98BC", "Items");
			headerRow.Cells.Add(questionHeaderCell);

			var answerHeaderCell = new HtmlTableCell();
			answerHeaderCell.Attributes["class"] = "votingAnswerHeader";
			answerHeaderCell.InnerText = VoteHeader.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.UnrankedVote
				? Res.GetString("30612C63-6609-4D81-9F37-F3446C466311", "Vote")
				: Res.GetString("EE018245-A961-4579-93F7-3B02005AE7A3", "Rank");
			headerRow.Cells.Add(answerHeaderCell);

			headerRowContainer.Controls.Add(headerRow);
		}

		protected virtual void ConstructVotingItem(RepeaterItemEventArgs e)
		{
			VoteExamSurveyAnswerWrapperBase answerWrapper = (VoteExamSurveyAnswerWrapperBase)e.Item.DataItem;
			HtmlTableRow votingItemRow = ConstructVotingItemRow(e.Item.ItemType, answerWrapper);
			e.Item.Controls.Add(votingItemRow);
		}

		HtmlTableRow ConstructVotingItemRow(ListItemType itemType, VoteExamSurveyAnswerWrapperBase answerWrapper)
		{
			HtmlTableRow votingItemRow = new HtmlTableRow();

			HtmlTableCell votingItemCell = ConstructVotingItemCell();
			votingItemRow.Cells.Add(votingItemCell);

			if (IsVotingItemHeader(answerWrapper))
			{
				votingItemCell.ColSpan = 2;
				votingItemRow.Attributes["class"] = "itemCategory";
				votingItemCell.ColSpan = 2;
			}
			else
			{
				votingItemRow.Cells.Insert(0, ConstructVotingItemNumberCell(answerWrapper));
				votingItemRow.Attributes["class"] = (itemType == ListItemType.Item) ? "votingItemRow" : "votingItemAltRow";
				ConstructAndAddVotingAnswersCells(votingItemRow, answerWrapper);
			}

			return votingItemRow;
		}

		HtmlTableCell ConstructVotingItemCell()
		{
			HtmlTableCell result = new HtmlTableCell();

			result.Attributes["class"] = "votingItemCell";
			ZTextLabel questionLabel = new ZTextLabel();
			questionLabel.EnableHtmlEncoding = false;
			questionLabel.BindTo = "QuestionTextForWeb";
			result.Controls.Add(questionLabel);

			return result;
		}

		HtmlTableCell ConstructVotingItemNumberCell(VoteExamSurveyAnswerWrapperBase answerWrapper)
		{
			HtmlTableCell result = new HtmlTableCell();
			result.Attributes["class"] = "votingItemNumberCell";
			ConstructVotingItemNumberCellContent(result, answerWrapper);
			return result;
		}

		protected virtual void ConstructVotingItemNumberCellContent(HtmlTableCell votingNumberCell, VoteExamSurveyAnswerWrapperBase answerWrapper)
		{
			var questionOrderHiddenField = new HiddenField();
			questionOrderHiddenField.Value = answerWrapper.Question.HY_SubQuestionOrder.ToString();
			var orderLabel = new ZTextLabel();
			orderLabel.BindTo = VotingItemNumberLabelBindTo;
			votingNumberCell.Controls.Add(questionOrderHiddenField);
			votingNumberCell.Controls.Add(orderLabel);
		}

		protected virtual string VotingItemNumberLabelBindTo
		{
			get { return "Answer.HZ_QuestionOrder"; }
		}

		protected virtual void ConstructAndAddVotingAnswersCells(HtmlTableRow parentRow, VoteExamSurveyAnswerWrapperBase answerWrapper)
		{
			HtmlTableCell votingAnswerCell = new HtmlTableCell();
			votingAnswerCell.Attributes["class"] = "votingAnswerCell";
			votingAnswerCell.Controls.Add(AnswerControlCreateMethod.Invoke());
			parentRow.Cells.Add(votingAnswerCell);
		}

		void BindControls(Control ctrl, object dataSource)
		{
			if (ctrl is ISelfBindingWebControl)
			{
				((ISelfBindingWebControl)ctrl).Bind(dataSource);
			}
			else
			{
				foreach (Control child in ctrl.Controls)
				{
					BindControls(child, dataSource);
				}
			}
		}

		#region AnswerControlCreateMethod

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		AnswerControlCreationDelegate AnswerControlCreateMethod
		{
			get
			{
				if (fAnswerControlCreateMethod == null)
				{
					if (VoteHeader.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.UnrankedVote)
					{
						fAnswerControlCreateMethod = delegate
						{
							ZCheckBox checkBox = new ZCheckBox();
							checkBox.BindTo = "SingleAnswer.AnswerAsBool";
							checkBox.AutoPostBack = true;
							return checkBox;
						};
					}
					else
					{
						if (VoteHeader.HY_Max <= 10)
						{
							fAnswerControlCreateMethod = delegate
							{
								ZDropDownList dropDownList = new ZDropDownList();
								dropDownList.DataTextField = "Code";
								dropDownList.BindTo = "SingleAnswer.HZ_Answer";
								dropDownList.BindToList = "SingleAnswer.Lookups.AnswerOptionList";
								dropDownList.AutoPostBack = true;
								return dropDownList;
							};
						}
						else
						{
							fAnswerControlCreateMethod = delegate
							{
								ZNumericTextBox numericTextBox = new ZNumericTextBox();
								numericTextBox.MaxLength = 5;
								numericTextBox.Width = new Unit(50);
								numericTextBox.BindTo = "SingleAnswer.AnswerAsInt";
								numericTextBox.AutoPostBack = true;
								return numericTextBox;
							};
						}
					}
				}
				return fAnswerControlCreateMethod;
			}
		}

		AnswerControlCreationDelegate fAnswerControlCreateMethod;
		delegate Control AnswerControlCreationDelegate();

		#endregion

		VoteExamSurveyQuestion VoteHeader
		{
			get { return AnswerSet.CompanyCampaign.VoteHeader; }
		}

		protected IVoteExamSurveyAnswerSet AnswerSet
		{
			get { return Page.DataSource as IVoteExamSurveyAnswerSet; }
		}

		public new ZPage Page
		{
			get { return (ZPage)base.Page; }
#if DEBUG
			set { base.Page = value; }
#endif
		}

		#endregion
	}
}
