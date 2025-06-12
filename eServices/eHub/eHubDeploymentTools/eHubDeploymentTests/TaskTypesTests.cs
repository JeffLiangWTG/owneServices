using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using eHubDeploymentTools;

namespace eHubDeploymentTests
{
	[TestClass]
	public class TaskTypesTests
	{
		[TestMethod]
		public void GetDescriptionValidEnumValue()
		{
			Assert.AreEqual("Get Latest Version", TaskTypes.GetDescription(TaskTypes.TaskType.GetLatest));
			Assert.AreEqual("Get Latest Version", TaskTypes.GetDescription(typeof(TaskTypes.TaskType), "GetLatest"));

		}

		[TestMethod]
		public void GetDescriptionInvalidEnumValue()
		{
			Assert.AreEqual(null, TaskTypes.GetDescription("BlaBla"));
		}
	}
}
