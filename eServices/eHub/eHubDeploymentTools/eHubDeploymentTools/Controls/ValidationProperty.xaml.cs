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

namespace eHubDeploymentTools.Controls
{
	/// <summary>
	/// Interaction logic for ValidationProperty.xaml
	/// </summary>
	public partial class ValidationProperty : UserControl, IProperty
	{
		public event PropertyUpdated Updated;

		public ValidationProperty()
		{
			InitializeComponent();
		}

		public ValidationProperty(Property property)
			: this()
		{
			this.property = property;
			TextLabel.Content = property.Name;
			InputText.Text = property.ValidateFolder;
			Validate();
		}

		void Validate()
		{
			if (IsValid)
			{
				//ValidationPanel.Visibility = Visibility.Hidden;
				SetTextBoxColor(Colors.Green);
				ActionButton.Visibility = Visibility.Hidden;
			}
			else
			{
				//ValidationPanel.Visibility = Visibility.Visible;
				SetTextBoxColor(Colors.LightCoral);
				ActionButton.Visibility = Visibility.Visible;
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
					return Directory.Exists(InputText.Text);
				}

				return false;
			}
		}

		Property property;

		Property IProperty.property
		{
			get { return property; }
		}

		void ActionButton_Click(object sender, RoutedEventArgs e)
		{
			SetupDistributive(property.DistributivePath, Directory.GetParent(property.ValidateFolder).FullName);	
			Validate();
			Updated(this);
		}

		void SetupDistributive(string distributivePath, string extractFolder)
		{
			ZipManager.UnZipFiles(distributivePath, extractFolder, null, false);
		}
	}
}
