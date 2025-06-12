using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using eHubDeploymentTools.Controls;

namespace eHubDeploymentTools
{
	class PropertiesCreator
	{
	
		public UIElementCollection PropertiesContainerElement { get; private set; }
		public TaskTypes.TaskType Section { get; private set; }

		public PropertiesCreator(UIElementCollection propertiesContainerElement, TaskTypes.TaskType type)
		{
			PropertiesContainerElement = propertiesContainerElement;
			Section = type;
		}

	}
}
