using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.NL.Testing
{
	[TestedType(typeof(PortbaseImportNotification))]
	sealed class PortbaseImportNotificationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var portbase = new PortbaseImportNotification("zzz", "zzz", "Portbase Import Notification");
			portbase.Documents = new List<PortbaseDocument>();

			return portbase;
		}
	}
}
