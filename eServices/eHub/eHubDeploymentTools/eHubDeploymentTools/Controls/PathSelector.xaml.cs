using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using wf = System.Windows.Forms;
using Microsoft.Win32;

namespace eHubDeploymentTools.Controls
{
	/// <summary>
	/// Interaction logic for DirectorySelector.xaml
	/// </summary>
	public partial class PathSelector : UserControl, IProperty
	{
		public event PropertyUpdated Updated;

		public PathSelector()
		{
			InitializeComponent();
		}

		public PathSelector(Property property) : this()
		{
			this.property = property;
			TextLabel.Content = property.Name;

			if (Task.SharedProperyList.ContainsKey(property.Name))
			{
				InputText.Text = Task.SharedProperyList[property.Name];
			}
			else
			{
				InputText.Text = property.Value;
			}

			Validate();
		}

		void Validate()
		{
			if (IsValid)
			{
				SetTextBoxColor(Colors.White);
			}
			else
			{
				SetTextBoxColor(Colors.LightCoral);			
			}
		}

		void SetTextBoxColor(Color color)
		{
			InputText.Background = new SolidColorBrush() { Color = color };
		}

		public bool IsValid
		{
			get 
			{
				if (!String.IsNullOrEmpty(InputText.Text))
				{
					if (property.Type == TaskTypes.PropertyType.DirectorySelector) return Directory.Exists(InputText.Text);
					if (property.Type == TaskTypes.PropertyType.FileSelector) return File.Exists(InputText.Text);
				}

				return false;
			}
		}

		private void OpenDialog_Click(object sender, RoutedEventArgs e)
		{
			if (property.Type == TaskTypes.PropertyType.DirectorySelector)
			{
				var dialog = new wf.FolderBrowserDialog();
				var result = dialog.ShowDialog();
				if (result == wf.DialogResult.OK) InputText.Text = dialog.SelectedPath;
			}

			if (property.Type == TaskTypes.PropertyType.FileSelector)
			{
				var dialog = new OpenFileDialog();
				if(dialog.ShowDialog() == true) InputText.Text = dialog.FileName;
			}
		}

		void InputText_TextChanged(object sender, TextChangedEventArgs e)
		{
			Validate();
			property.Value = InputText.Text;
			if (Updated != null) Updated(this);
		}

		Property property;

		Property IProperty.property
		{
			get { return property; }
		}
	}
}
