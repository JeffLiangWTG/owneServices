using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.BE
{
	[TestedType(typeof(ExportNotification))]
	sealed class ExportNotificationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var exportNotification = new ExportNotification("ForwardingConsol", "C20201026");
			exportNotification.PackLines = new List<PackingLine>();
			return exportNotification;
		}
	}
}
