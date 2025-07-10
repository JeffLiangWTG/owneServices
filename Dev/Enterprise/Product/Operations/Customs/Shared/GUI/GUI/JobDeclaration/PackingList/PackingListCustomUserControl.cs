using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class PackingListCustomUserControl : ZUserControl
	{
		public PackingListCustomUserControl()
		{
			InitializeComponent();
		}

		protected List<CustomLabelControlRenamer> fRenamers = new List<CustomLabelControlRenamer>();

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem != null)
			{
				foreach (CustomLabelControlRenamer renamer in fRenamers)
				{
					renamer.Dispose();
				}
				fRenamers.Clear();
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null && CurrentDataItem is CusPackingList packingList)
			{
				var customLabelsProvider = new CusPackingListCustomLabelsProvider(packingList);
				fRenamers.Add(new CustomLabelControlRenamer(
					CustomAttribute1Label, CustomAttribute1TextBox, customLabelsProvider, CusPackingList.Schema.CUL_CustomAttribute1, captionWithColon: false));
				fRenamers.Add(new CustomLabelControlRenamer(
					CustomAttribute2Label, CustomAttribute2TextBox, customLabelsProvider, CusPackingList.Schema.CUL_CustomAttribute2, captionWithColon: false));
				fRenamers.Add(new CustomLabelControlRenamer(
					CustomFlag1Label, CustomFlag1CheckBox, customLabelsProvider, CusPackingList.Schema.CUL_CustomFlag1, captionWithColon: false));
				fRenamers.Add(new CustomLabelControlRenamer(
					CustomFlag2Label, CustomFlag2CheckBox, customLabelsProvider, CusPackingList.Schema.CUL_CustomFlag2, captionWithColon: false));
				fRenamers.Add(new CustomLabelControlRenamer(
					CustomDate1Label, CustomDate1DateEdit, customLabelsProvider, CusPackingList.Schema.CUL_CustomDate1, captionWithColon: false));
				fRenamers.Add(new CustomLabelControlRenamer(
					CustomDate2Label, CustomDate2DateEdit, customLabelsProvider, CusPackingList.Schema.CUL_CustomDate2, captionWithColon: false));
				fRenamers.Add(new CustomLabelControlRenamer(
					CustomDecimal1Label, CustomDecimal1CalcEdit, customLabelsProvider, CusPackingList.Schema.CUL_CustomDecimal1, captionWithColon: false));
				fRenamers.Add(new CustomLabelControlRenamer(
					CustomDecimal2Label, CustomDecimal2CalcEdit, customLabelsProvider, CusPackingList.Schema.CUL_CustomDecimal2, captionWithColon: false));
			}
		}
	}
}
