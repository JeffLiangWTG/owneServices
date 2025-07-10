using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Freight.Agency.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Agency.DataTransfer.Testing
{
	internal class ContainerMovementMessageActionTest : TestCaseWithFactory
	{
		public void TestExecuteAction_ImportContainerMovement()
		{
			var factoryProvider = new BusinessObjectFactoryProvider(Factory);
			int noOfCcontainerMovements = Factory.GetDatabaseCount(typeof(ContainerMovement));
			var buffer = new NotificationBuffer();
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_MessageText = messageBody;
			var action = new ContainerMovementMessageAction(factoryProvider);
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_HeaderText = interchangeHeader;
			interchange.EI_FooterText = interchangeFooter;
			message.EM_EI = interchange.PK;
			List<ITransactionParticipant> forSave;
			action.ExecuteAction(message, buffer, out forSave);
			BusinessObjectFactory.SaveTogether(forSave.ToArray());
			INotification[] notifications = buffer.GetEventsByType(ErrorType.Error);
			AssertEquals(1, notifications.Length);
			AssertNotEquals("Wrong Exception type. Expected type: Not supported.", -1, notifications[0].Message.IndexOf("System.NotSupportedException: Specified method is not supported."));
			AssertEquals(noOfCcontainerMovements, Factory.GetDatabaseCount(typeof(ContainerMovement)));
		}

		#region XML
		const string interchangeHeader = @"<?xml version=""1.0"" encoding=""utf-8""?><XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2010-10-01T14:03:49.45+10:00</Date></InterchangeInfo><Payload><ContainerMovements>";
		const string interchangeFooter = @"</ContainerMovements></Payload></XmlInterchange>";
		const string messageBody = @"<ContainerMovement><ContainerNum>ContainerNum_0</ContainerNum></ContainerMovement>";
		#endregion
	}
}
