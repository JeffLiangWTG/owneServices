using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	public class TWMessageForTest : TWMessage
	{
		public TWMessageForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override string GetMessageReferenceNumber() => MessageNumForTesting;

		public void PopulateMessageNumberIfNeededExposed() => PopulateMessageNumberIfNeeded();

		public IEnumerable<Type> AdditionalRegisteredLinkedObjectTypesExposed => base.GetAdditionalRegisteredLinkedObjectTypes();

		public IStreamFormatter MessageStreamFormatterExposed => base.MessageStreamFormatter;

		public ZString MessageNumForTesting { get; set; }
	}
}
