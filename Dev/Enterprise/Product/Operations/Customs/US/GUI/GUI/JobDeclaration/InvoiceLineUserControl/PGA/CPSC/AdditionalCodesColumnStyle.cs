using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.US.GUI
{
	public partial class AdditionalCodesColumnStyle : ZCodeFindBoxColumnStyle
	{
		public AdditionalCodesColumnStyle(AdditionalCodesColumnStyleInfo columnInfo)
			: this(() => new ZGridAdditionalCodesUserControl(), columnInfo)
		{
		}

		AdditionalCodesColumnStyle(Func<ZGridAdditionalCodesUserControl> control, AdditionalCodesColumnStyleInfo columnInfo)
			: base(control, columnInfo)
		{
		}

		protected override void PrepareControlData(CurrencyManager source, int rowNum)
		{
		}
	}

	[SuppressCheckControlModuleId]
	[SuppressCheckControlLookupList]
	public class AdditionalCodesColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get { return typeof(AdditionalCodesColumnStyle); }
		}
	}

	class ZGridAdditionalCodesUserControl : ZGridFindBox
	{
		public ZGridAdditionalCodesUserControl()
		{
		}

		public override void SelectFromPopupForm(bool autoSelect = false)
		{
			CommaSeparatedNumberCollection commaSeparatedNumberCollection;
			AdditionalCodesForm codesForm;
			if (ColumnStyle.MappingName == CPSCRule.Schema.US_RuleCodes)
			{
				commaSeparatedNumberCollection = new RuleCodeMultipleCodeCollection(this.Code, new BusinessObjectFactory());
				codesForm = new AdditionalCodesForm(commaSeparatedNumberCollection, AdditionalCodesShowType.ZDropEditColumnStyleInfo, ColumnStyle.HeaderText, 10);
			}
			else
			{
				commaSeparatedNumberCollection = new ItemIdentityNumbersMultipleCodeCollection(this.Code, new BusinessObjectFactory());
				codesForm = new AdditionalCodesForm(commaSeparatedNumberCollection, AdditionalCodesShowType.TextBoxColumnStyleInfo, ColumnStyle.HeaderText, 5);
			}

			ZFormModaliser.ShowDialogAndDispose(codesForm);
			if (codesForm.DialogResult == System.Windows.Forms.DialogResult.OK)
			{
				this.Code = commaSeparatedNumberCollection.GetCodesAsCommaSeparatedString();
			}
		}

		protected override System.Collections.IList GetList(object dataSource, string listMember, string dataMemberForErrorReporting)
		{
			return null;
		}
	}
}
