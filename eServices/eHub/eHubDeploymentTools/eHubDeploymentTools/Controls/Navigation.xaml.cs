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
	public delegate void TaskWasChanged(object sender, TaskTypes.TaskType type);

	/// <summary>
	/// Interaction logic for Navigation.xaml
	/// </summary>
	public partial class Navigation : UserControl
	{
		public event TaskWasChanged Changed;
		

		public Navigation()
		{
			InitializeComponent();
			LoadTasks();
		}

		void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			SelectFirstTask();
		}

		void SelectFirstTask()
		{
			string[] names = Enum.GetNames(typeof(TaskTypes.TaskType));
			if (names != null && names.Length > 0) SelectTask(names[0]);
		}

		void LoadTasks()
		{
			string[] names = Enum.GetNames(typeof(TaskTypes.TaskType));
			foreach (string name in names)
			{
				string description = TaskTypes.GetDescription(typeof(TaskTypes.TaskType), name);
				Button task = new Button() { Height = 40, Name = name, Content = description };
				task.Click += new RoutedEventHandler(TaskSelected);
				TaskList.Children.Add(task);
			}
		}

		void TaskSelected(object sender, RoutedEventArgs e)
		{
			var button = sender as Button;
			SelectTask(button.Name);
		}

		void SelectTask(string taskName)
		{
			TaskTypes.TaskType type;
			if (Enum.TryParse<TaskTypes.TaskType>(taskName, out type))
			{
				UpdateUI(type);
				if (Changed != null) Changed(this, type);
			}
		}

		void UpdateUI(TaskTypes.TaskType type)
		{

			foreach (UIElement element in TaskList.Children)
			{
				if (element is Button)
				{
					var button = element as Button;
					if (button.Name == type.ToString())
					{
						SetButtonColor(button, Colors.White);
					}
					else
					{
						SetButtonColor(button, Colors.LightGray);
					}
				}
			}
		}

		static void SetButtonColor(Button button, Color color)
		{
			button.Background = new SolidColorBrush() { Color = color };
		}
	}
}
