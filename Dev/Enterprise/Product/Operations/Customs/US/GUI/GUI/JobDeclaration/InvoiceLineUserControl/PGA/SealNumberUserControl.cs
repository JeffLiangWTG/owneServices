using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	[ProvideMetaDataProperty("ShouldBeReadOnly", MetaDataTypes.ReadOnly)]
	public partial class SealNumberUserControl : ZUserControl, IExtendedControl, IBindTo
	{
		public SealNumberUserControl()
		{
			Extensions = new DefaultControlExtensionCollection(this);
			InitializeComponent();
		}

		public override string Text
		{
			get { return SealNumberTextBox.Text; }
			set { SealNumberTextBox.Text = value; }
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindTo
		{
			get { return SealNumberTextBox.BindTo; }
			set { SealNumberTextBox.BindTo = value; }
		}

		void SealNumberButton_Click(object sender, EventArgs e)
		{
			var sealNumberCollection = new SealNumberBusinessObjectCollection(Text);
			var sealNumbersForm = new SealNumberForm(sealNumberCollection);
			var parentForm = FindForm();
			sealNumbersForm.Icon = parentForm.Icon;
			if (sealNumbersForm.ShowDialog(parentForm) == DialogResult.OK)
			{
				Text = sealNumberCollection.GetSealNumbersAsCommaSepereateString();
			}
		}

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			Extensions.SetDataBinding(dataSource, dataMember);
		}

		#region ReadOnly
		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			SetButtonReadOnly();
		}

		void SetButtonReadOnly()
		{
			SealNumberButton.ReadOnly = SealNumberTextBox.ReadOnly;
		}
		#endregion

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			if (disposing)
			{
				Extensions.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
