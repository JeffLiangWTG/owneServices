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

namespace eHubDeploymentTools
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			TaskNavigation.Changed += new Controls.TaskWasChanged(TaskNavigation_Changed);
			TaskManager.Updated += new TaskUpdated(TaskManager_Updated);

		}

		void TaskManager_Updated(object sender, bool TaskIsValid)
		{
			RunButton.IsEnabled = TaskIsValid;
		}

		void TaskNavigation_Changed(object sender, TaskTypes.TaskType type)
		{
			TaskManager.ShowTaskProperties(type);
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void RunButton_Click(object sender, RoutedEventArgs e)
		{
			TaskManager.Execute();
		}
	}
}
