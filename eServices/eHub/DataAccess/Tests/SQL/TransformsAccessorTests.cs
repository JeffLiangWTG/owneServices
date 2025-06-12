using System;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.DataAccess.Tests.Sql
{
	public class TransformsAccessorTests
	{
		[Test]
		public void SelectTransformsByPartiesMessage_CallsSharedDataAccess()
		{
			var sharedTransformAccessor = new Mock<eServices.eHubDataAccess.Integration.ITransformAccessor>();
			sharedTransformAccessor.Setup(x => x.SelectTransformsByPartiesMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>()))
				.Returns(new[] { new eServices.eHubDataAccess.Integration.TransformSet(new[] { new eServices.eHubDataAccess.Integration.TransformDetail(Guid.NewGuid(), "transformType", "targetType") }.ToList(), null) }.ToList());
			var eHubTransformAccessor = new CargoWise.eHub.DataAccess.Sql.TransformAccessor(sharedTransformAccessor.Object);
			_ = eHubTransformAccessor
				.SelectTransformsByPartiesMessage("party1", "party2", "messageType", true);
			sharedTransformAccessor.Verify(x => x
				.SelectTransformsByPartiesMessage("party1", "party2", "messageType", true), Times.Once);
		}

		[Test]
		public void GetRecipientCode_CallsSharedDataAccess()
		{
			var sharedTransformAccessor = new Mock<eServices.eHubDataAccess.Integration.ITransformAccessor>();
			var eHubTransformAccessor = new CargoWise.eHub.DataAccess.Sql.TransformAccessor(sharedTransformAccessor.Object);
			eHubTransformAccessor
				.GetRecipientCode("senderClientCode", "recipientClientCode", "transformationName", "codeSet", "resultField", "key1", "key2", "key3", "key4", "key5");
			sharedTransformAccessor.Verify(x => x
				.GetRecipientCode("senderClientCode", "recipientClientCode", "transformationName", "codeSet", "resultField", "key1", "key2", "key3", "key4", "key5"), Times.Once);
		}

		[Test]
		public void IsFlatFile_CallsSharedDataAccess()
		{
			var sharedTransformAccessor = new Mock<eServices.eHubDataAccess.Integration.ITransformAccessor>();
			var eHubTransformAccessor = new CargoWise.eHub.DataAccess.Sql.TransformAccessor(sharedTransformAccessor.Object);
			eHubTransformAccessor
				.IsFlatFile("messageType", out string charset);
			sharedTransformAccessor.Verify(x => x
				.IsFlatFile("messageType", out charset), Times.Once);
		}

		[Test]
		public void IsEDI_CallsSharedDataAccess()
		{
			var sharedTransformAccessor = new Mock<eServices.eHubDataAccess.Integration.ITransformAccessor>();
			var eHubTransformAccessor = new CargoWise.eHub.DataAccess.Sql.TransformAccessor(sharedTransformAccessor.Object);
			eHubTransformAccessor
				.IsEDI("messageType");
			sharedTransformAccessor.Verify(x => x
				.IsEDI("messageType"), Times.Once);
		}

		[Test]
		public void IsJson_CallsSharedDataAccess()
		{
			var sharedTransformAccessor = new Mock<eServices.eHubDataAccess.Integration.ITransformAccessor>();
			var eHubTransformAccessor = new CargoWise.eHub.DataAccess.Sql.TransformAccessor(sharedTransformAccessor.Object);
			eHubTransformAccessor
				.IsJson("messageType");
			sharedTransformAccessor.Verify(x => x
				.IsJson("messageType"), Times.Once);
		}

		[Test]
		public void CallActionProcedure_CallsSharedDataAccess()
		{
			var sharedTransformAccessor = new Mock<eServices.eHubDataAccess.Integration.ITransformAccessor>();
			var eHubTransformAccessor = new CargoWise.eHub.DataAccess.Sql.TransformAccessor(sharedTransformAccessor.Object);
			string[] inputParms = { "param1", "param2" };
			eHubTransformAccessor
				.CallActionProcedure("procedure", "outputParm", inputParms);
			sharedTransformAccessor.Verify(x => x
				.CallActionProcedure("procedure", "outputParm", inputParms), Times.Once);
		}

		[Test]
		public void IsPostAssembleMapping_CallsSharedDataAccess()
		{
			var sharedTransformAccessor = new Mock<eServices.eHubDataAccess.Integration.ITransformAccessor>();
			var eHubTransformAccessor = new CargoWise.eHub.DataAccess.Sql.TransformAccessor(sharedTransformAccessor.Object);
			eHubTransformAccessor
				.IsPostAssembleMapping("messageType", out string postAssembleWrapper);
			sharedTransformAccessor.Verify(x => x
				.IsPostAssembleMapping("messageType", out postAssembleWrapper), Times.Once);
		}
	}
}
