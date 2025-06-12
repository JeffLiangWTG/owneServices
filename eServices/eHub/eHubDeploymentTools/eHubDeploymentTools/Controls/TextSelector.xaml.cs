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

namespace eHubDeploymentTools.Controls
{
	/// <summary>
	/// Interaction logic for TextSelector.xaml
	/// </summary>
	public partial class TextSelector : UserControl, IProperty
	{
		public event PropertyUpdated Updated;

		public bool IsValid
		{
			get { return true; }
		}


		public TextSelector()
		{
			InitializeComponent();
		}

		public TextSelector(Property property)
			: this()
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

			ValueCustomixation();
			Validate(InputText.Text);
		}

		void ValueCustomixation()
		{
			if (property.Name == "BizTalkMachineName")
			{
				InputText.Text = System.Environment.MachineName;
			}

			if (property.Name == "ConnectionString")
			{
				InputText.Text = String.Format("Server={0};Integrated Security=True", System.Environment.MachineName);
			}
		}

		void InputText_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (Validate(InputText.Text))
			{
				property.Value = InputText.Text;
				if(Updated != null) Updated(this);
			}
		}

		bool Validate(string p)
		{
			return true;
		}

		Property property;

		Property IProperty.property
		{
			get { return property; }
		}
	}
}
