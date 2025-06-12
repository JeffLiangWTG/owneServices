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
using System.Configuration;

namespace eHubDeploymentTools.Controls
{
	/// <summary>
	/// Interaction logic for Properties.xaml
	/// </summary>
	public partial class Task : UserControl, ITask
	{
		public static Dictionary<string,string> SharedProperyList;
		TaskTypes.TaskType currentType;

		public event TaskUpdated Updated;

		public Task()
		{
			InitializeComponent();
			SharedProperyList = new Dictionary<string, string>();
		}

		public void ShowTaskProperties(TaskTypes.TaskType taskType)
		{
			currentType = taskType;
			LoadSharedProperyList();
			LoadProperties();
			Propery_Updated(null);
		}

		public void Execute()
		{
			var propertyList = SaveProperties();
			try
			{
				string message = TaskExecutor.Execute(currentType, propertyList);
				ErrorText.Text = String.Format("{0} complete successfully.", currentType.ToString());
			}
			catch (Exception ex)
			{
				ErrorText.Text = ex.Message;
			}
		}

		List<Property> SaveProperties()
		{
			var propertyList = GetPropertyListFromControls();
			PropertyHelper.SaveProperties(propertyList);
			return propertyList;
		}

		List<Property> GetPropertyListFromControls()
		{
			var propertyList = new List<Property>();

			foreach (var element in PropertiesPanel.Children)
			{
				if (element is IProperty)
				{
					propertyList.Add(((IProperty)element).property);
				}
			}

			return propertyList;
		}

		void LoadProperties()
		{
			var propertyList = PropertyHelper.GetPropertyListFromFile();
			PropertiesPanel.Children.Clear();
			foreach (var property in propertyList)
			{
				if (property.Section != currentType) continue;
				var element = CreateProperty(property);

				if (element != null)
				{
					((IProperty)element).Updated += new PropertyUpdated(Propery_Updated);
					PropertiesPanel.Children.Add(element);
				}
			}
		}

		void Propery_Updated(object sender)
		{
			if(Updated!= null) Updated(this, Validate());
			UpdateSharedProperyList();
		}


		void LoadSharedProperyList()
		{
			SharedProperyList.Clear();
			var propertyList = PropertyHelper.GetPropertyListFromFile();
			foreach (var property in propertyList)
			{
				if( !SharedProperyList.ContainsKey(property.Name) ) SharedProperyList.Add(property.Name, property.Value);
			}
		}

		void UpdateSharedProperyList()
		{
			SharedProperyList.Clear();
			foreach (var element in PropertiesPanel.Children)
			{
				if (element is IProperty)
				{
					if (((IProperty)element).IsValid)
					{
						var property = ((IProperty)element).property;
						if (SharedProperyList.ContainsKey(property.Name)) SharedProperyList[property.Name] = property.Value;
					}
				}
			}
		}

		bool Validate()
		{
			foreach( var element in PropertiesPanel.Children )
			{
				if( element is IProperty )
				{
					if( !((IProperty)element).IsValid)
					{
						return false;
					}
				}
			}

			return true;
		}

		UIElement CreateProperty(Property property)
		{
			switch (property.Type)
			{
				case TaskTypes.PropertyType.TextSelector: return new TextSelector(property);
				case TaskTypes.PropertyType.DirectorySelector: return new PathSelector(property);
				case TaskTypes.PropertyType.FileSelector: return new PathSelector(property);
				case TaskTypes.PropertyType.ValidateDistributive: return new ValidationProperty(property);
			}

			return null;
		}
	}
}
