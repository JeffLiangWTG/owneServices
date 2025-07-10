using System;
using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.US.GUI
{
	public class SealNumberColumnStyle : ZCodeFindBoxColumnStyle
	{
		public SealNumberColumnStyle(SealNumberColumnStyleInfo columnInfo)
			: this(() => new ZGridSealNumberUserControl(), columnInfo)
		{ }

		SealNumberColumnStyle(Func<ZGridSealNumberUserControl> control, SealNumberColumnStyleInfo columnInfo)
			: base(control, columnInfo)
		{
		}

		protected override void PrepareControlData(CurrencyManager source, int rowNum)
		{
		}
	}

	[SuppressCheckControlModuleId]
	[SuppressCheckControlLookupList]
	public class SealNumberColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get { return typeof(SealNumberColumnStyle); }
		}
	}

	class ZGridSealNumberUserControl : ZGridFindBox
	{
		public ZGridSealNumberUserControl()
		{
		}

		public override void SelectFromPopupForm(bool autoSelect)
		{
			var sealNumbersAsCollection = new SealNumberBusinessObjectCollection(this.Code);
			var sealNumbersForm = new SealNumberForm(sealNumbersAsCollection);
			var parentForm = FindForm();
			sealNumbersForm.Icon = parentForm.Icon;
			if (ZFormModaliser.ShowDialogAndDispose(sealNumbersForm, parentForm) == System.Windows.Forms.DialogResult.OK)
			{
				this.Code = sealNumbersAsCollection.GetSealNumbersAsCommaSepereateString();
			}
		}

		protected override System.Collections.IList GetList(object dataSource, string listMember, string dataMemberForErrorReporting)
		{
			return null;
		}
	}
}
