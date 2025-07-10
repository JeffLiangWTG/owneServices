using System.Collections.Generic;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class CodeDescriptionBoolTreeControl : RegistryZUserControl
	{
		readonly List<CodeDescriptionBoolTreeGridControl> Grids = new List<CodeDescriptionBoolTreeGridControl>();

		public CodeDescriptionBoolTreeControl()
		{
			InitializeComponent();
			Grids.Add(Grid1);
			Grids.Add(Grid2);
			Grids.Add(Grid3);
			Grids.Add(Grid4);
			Grids.Add(Grid5);
#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(GroupBox1);
			MissingResourceStringChecker.ExcludeFromTest(GroupBox2);
			MissingResourceStringChecker.ExcludeFromTest(GroupBox3);
			MissingResourceStringChecker.ExcludeFromTest(GroupBox4);
			MissingResourceStringChecker.ExcludeFromTest(GroupBox5);
#endif
		}

		public CodeDescriptionBoolTreeControl(CodeDescriptionBoolTreeRegistryEditorInfo editorInfo)
			: this()
		{
			ZArchitecture.GUI.ZGroupBox[] groupboxArray = { GroupBox1, GroupBox2, GroupBox3, GroupBox4, GroupBox5 };
			var captions = editorInfo.Captions.ToArray();

			for (int i = 0; i < groupboxArray.Length; i++)
			{
				if (i + 1 <= captions.Length)
				{
					Grids[i].SetLevel(
						(i <= 0) ? null : Grids[i - 1],
						(i >= captions.Length - 1) ? null : Grids[i + 1]);
					groupboxArray[i].GetExtension<ILabelCaptionRenderer>().Caption = captions[i];
					Grids[i].SetupColumns(editorInfo.BoolColumnCaption, editorInfo.IsBoolColumnVisible, editorInfo.IsCodeColumnVisible);
				}
				else
				{
					groupboxArray[i].Visible = false;
				}
			}
		}

		public bool Grid1ReadOnly
		{
			get { return Grid1.CodeDescriptionBoolGridReadOnly; }
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			foreach (CodeDescriptionBoolTreeGridControl grid in Grids)
			{
				grid.ReadOnly = readOnly;
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			// Ensure parents get set first
			foreach (CodeDescriptionBoolTreeGridControl grid in Grids)
			{
				grid.SetDataBinding(dataSource, dataMember);
			}

			base.SetDataBinding(dataSource, dataMember);
		}
	}
}
