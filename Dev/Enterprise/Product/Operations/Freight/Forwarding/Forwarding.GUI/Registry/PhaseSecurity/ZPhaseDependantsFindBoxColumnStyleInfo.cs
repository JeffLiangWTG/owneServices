using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class ZPhaseDependantsFindBoxColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "All overriding methods verified safe")]
		public ZPhaseDependantsFindBoxColumnStyleInfo()
		{
			CharacterCasing = CharacterCasing.Normal;
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZPhaseDependantsFindBoxColumnStyle); }
		}

		[DefaultValue(CharacterCasing.Normal)]
		public override CharacterCasing CharacterCasing
		{
			get { return base.CharacterCasing; }
			set { base.CharacterCasing = value; }
		}
	}

	public class ZPhaseDependantsFindBoxColumnStyle : ZCodeFindBoxColumnStyle
	{
		public ZPhaseDependantsFindBoxColumnStyle(ZPhaseDependantsFindBoxColumnStyleInfo columnInfo)
			: this(() => new ZPhaseDependantsFindBox(), columnInfo)
		{
		}

		protected ZPhaseDependantsFindBoxColumnStyle(Func<ZPhaseDependantsFindBox> gridFindBox, ZPhaseDependantsFindBoxColumnStyleInfo columnInfo)
			: base(gridFindBox, columnInfo)
		{
		}

		protected new ZPhaseDependantsFindBoxColumnStyleInfo ColumnInfo
		{
			get { return (ZPhaseDependantsFindBoxColumnStyleInfo)base.ColumnInfo; }
		}

		protected new ZPhaseDependantsFindBox FindBox
		{
			get { return (ZPhaseDependantsFindBox)base.FindBox; }
		}

		protected override void Edit(CurrencyManager source, int rowNum, System.Drawing.Rectangle bounds, bool readOnly, string instantText, bool cellVisible)
		{
			if (!IsEditing)
			{
				PhaseRule phaseRule = source.List[rowNum] as PhaseRule;
				if (phaseRule != null)
				{
					FindBox.PhaseRule = phaseRule;
				}
			}

			base.Edit(source, rowNum, bounds, readOnly, instantText, cellVisible);
		}

		protected override bool EditControlShownForReadOnlyCore
		{
			get { return true; }
		}

		protected override void ShowEditControl(bool cellVisible)
		{
			base.ShowEditControl(cellVisible);
			FindBox.CodeBox.ReadOnly = true;
		}
	}

	public class ZPhaseDependantsFindBox : ZGridFindBox
	{
		public ZPhaseDependantsFindBox()
		{
			PopupButtonReadonlyCanBeDifferent = true;
		}

		public PhaseRule PhaseRule { get; set; }

		public override void SelectFromPopupForm(bool autoSelect = false)
		{
			if (PhaseRule != null
				&& PhaseRule.Parent != null
				&& PhaseRule.Parent.Parent != null
				&& PhaseRule.Parent.Parent.DependantsProvider != null)
			{
				PhaseDependantsWrapper wrapper = GetPhaseDependantsWrapper();
				ZFormModaliser.ShowDialogAndDispose(new PhaseDependantsSelectionDialog(wrapper));

				if (wrapper.HasChangesInDependants)
				{
					PhaseRule.Dependants.RemoveAll();
					foreach (IPhaseDependant selectedDependant in wrapper.SelectedDependants)
					{
						PhaseRule.Dependants.Add(new PhaseDependant(selectedDependant));
					}

					PhaseRule.DependantsTextInfo.RefreshBinding();
				}
			}
		}

		protected virtual PhaseDependantsWrapper GetPhaseDependantsWrapper()
		{
			return new PhaseDependantsWrapper(PhaseRule.Parent.Parent.DependantsProvider, ((IPhaseRule)PhaseRule).Dependants);
		}

		#region Implementation

		protected override System.Collections.IList GetList(object dataSource, string listMember, string dataMemberForErrorReporting)
		{
			return null;
		}

		#endregion
	}
}
