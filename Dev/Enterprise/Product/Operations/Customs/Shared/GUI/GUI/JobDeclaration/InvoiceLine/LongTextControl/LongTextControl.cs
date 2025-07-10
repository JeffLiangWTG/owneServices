using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;

namespace Enterprise.Customs.GUI
{
	[DefaultDataSourceBindingMember(null)]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class LongTextControl /*SuppressCodeSmell reason=this is a control that could be bound to any business object*/ : ZUserControl, IExtendedControl, ITopLevelDataSourceType
	{
		readonly Container components;

		#region Custom Adornment Layout

		class LongTextControlAdornmentLayout : AdornmentLayout<LongTextControl>
		{
			public override IEnumerable<Control> GetBackroundAdornmentTargets(LongTextControl source)
			{
				yield return source.LongTextTextBox;
			}

			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(LongTextControl source)
			{
				yield return new IconLayout(source.LongTextTextBox, IconAlignment.Right);
			}
		}

		#endregion

		#region Constructors

		static LongTextControl()
		{
			NotificationAdornmentFactory.RegisterCustomLayout(new LongTextControlAdornmentLayout());
		}

		public LongTextControl()
		{
			InitializeComponent();
			InitializeEventHandlers();
			Extensions = new DefaultControlExtensionCollection(this);
#if DEBUG
			TypeDescriptor.AddAttributes(MoreButton, new SuppressControlRequiresTextBasherAttribute());
#endif
		}

		#endregion

		#region Initialization

		void InitializeEventHandlers()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				MoreButton.Click += AddInfoButton_Click;
				MoreButton.MouseUp += AddInfoButton_MouseUp;
				MoreButton.TabStop = true;
			}
		}

		#endregion

		#region IExtendedControl Members

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		public IControlExtensionCollection Extensions { get; private set; }

		#endregion

		#region IDataBoundControl Members

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			LongTextTextBox.SetDataBinding(dataSource, dataMember);
			DataBoundControl.SetDataBindingForMetadataProperties(this, dataSource, dataMember);
			Extensions.SetDataBinding(dataSource, dataMember);
		}

		#endregion

		#region

		Type ITopLevelDataSourceType.DataSourceType => base.DataSourceType;

		#endregion

		#region ReadOnly

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new object DataSource
		{
			get { return base.DataSource; }
		}

		protected virtual BusinessObject CurrentItem
		{
			get
			{
				BusinessObject result = null;
				var manager = LongTextTextBox.DataBindings.Cast<Binding>().FirstOrDefault()?.BindingManagerBase as CurrencyManager;
				if (manager != null && manager.Position != -1)
				{
					result = (BusinessObject)manager.GetCurrent();
				}
				return result;
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnly
		{
			get { return LongTextTextBox.ReadOnly; }
			set
			{
				LongTextTextBox.ReadOnly = value;
				MoreButton.ReadOnly = value;
			}
		}

		[Browsable(true)]
		public CharacterCasing CharacterCasing
		{
			get { return LongTextTextBox.CharacterCasing; }
			set
			{
				LongTextTextBox.CharacterCasing = value;
			}
		}

		public event EventHandler ReadOnlyChanged
		{
			add { LongTextTextBox.ReadOnlyChanged += value; }
			remove { LongTextTextBox.ReadOnlyChanged -= value; }
		}

		#endregion

		#region GetPropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<LongTextControl>()
				.Property("ReadOnly", true)
				.Result;
		}

		#endregion

		#region Implementation

		protected void AddInfoButton_Click(object sender, EventArgs e)
		{
			var editForm = CreateNewLongTextForm();
			var parentForm = FindForm();
			if (parentForm != null)
			{
				editForm.Icon = parentForm.Icon;
				ZFormModaliser.Show(editForm, parentForm);
			}
		}

		LongTextForm CreateNewLongTextForm()
		{
			var captionResourceString = CaptionResourceString;
			if (captionResourceString == null || captionResourceString.IsEmpty())
			{
				captionResourceString = new ResourceStringKeyCalculator(this).DataString;
			}

			var dataSource = CurrentItem;
			var dataMember = dataSource == DataSource ? DataMember : PropertyUtilities.GetPropertyName(DataMember);
			{
				return new LongTextForm(dataSource, dataMember, captionResourceString, CharacterCasing);
			}
		}

		void AddInfoButton_MouseUp(object sender, MouseEventArgs e)
		{
			LongTextTextBox.Focus();
		}

		protected internal ZButton MoreButton;

		#endregion

		protected internal ZTextBox.Bare LongTextTextBox;
	}
}
