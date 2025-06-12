using System;
using CargoWise.eHub.Products.AirMessaging.PipelineComponents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.AirMessaging.Tests
{
	[TestClass]
	public class EnvelopContextTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CreateEnvelopContext()
		{
			Assert.IsInstanceOfType(EnvelopContextFactory.Create(ServiceProvider.BT), typeof(BTEnvelopContext));
			Assert.IsInstanceOfType(EnvelopContextFactory.Create(ServiceProvider.CCSJ), typeof(CCSJEnvelopContext));
			Assert.IsInstanceOfType(EnvelopContextFactory.Create(ServiceProvider.Delta), typeof(DeltaEnvelopContext));
			Assert.IsInstanceOfType(EnvelopContextFactory.Create(ServiceProvider.Traxon), typeof(TraxonEnvelopContext));
			Assert.IsInstanceOfType(EnvelopContextFactory.Create(ServiceProvider.GLSHK), typeof(GLSHKEnvelopContext));
            Assert.IsInstanceOfType(EnvelopContextFactory.Create(ServiceProvider.CargoStart), typeof(GLSHKEnvelopContext));
            Assert.IsInstanceOfType(EnvelopContextFactory.Create(ServiceProvider.Cargonaut), typeof(CargonautEnvelopContext));
            Assert.IsInstanceOfType(EnvelopContextFactory.Create(ServiceProvider.CCN), typeof(CCNEnvelopContext));
			Assert.IsInstanceOfType(EnvelopContextFactory.Create(ServiceProvider.Descartes), typeof(DescartesEnvelopContext));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CreateEnvelopContextForTestServiceProviderID()
		{
		    Assert.AreEqual(AirMessageDisassembleComponent.GetServiceProvider("BT_Test"), ServiceProvider.BT);
		    Assert.AreEqual(AirMessageDisassembleComponent.GetServiceProvider("CCSJ_Test"), ServiceProvider.CCSJ);
		    Assert.AreEqual(AirMessageDisassembleComponent.GetServiceProvider("Delta_Test"), ServiceProvider.Delta);
		    Assert.AreEqual(AirMessageDisassembleComponent.GetServiceProvider("Traxon_Test"), ServiceProvider.Traxon);
		    Assert.AreEqual(AirMessageDisassembleComponent.GetServiceProvider("GLSHK_Test"), ServiceProvider.GLSHK);
			Assert.AreEqual(AirMessageDisassembleComponent.GetServiceProvider("CargoStart_Test"), ServiceProvider.CargoStart);
		    Assert.AreEqual(AirMessageDisassembleComponent.GetServiceProvider("CCN_Test"), ServiceProvider.CCN);
		    Assert.AreEqual(AirMessageDisassembleComponent.GetServiceProvider("Descartes_Test"), ServiceProvider.Descartes);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ExtractMessageTypeAndVersion()
		{
			var mockRepository = new MockRepository();
			var envelopContext = mockRepository.PartialMock<BTEnvelopContext>();
			mockRepository.ReplayAll();

			envelopContext.InternalMessage = "FWB/16\r\n081-32652362SYDCHI/T10K100";
			envelopContext.ExtractMessageTypeAndVersion();
			Assert.AreEqual("FWB", envelopContext.MessageType);
			Assert.AreEqual("16", envelopContext.MessageVersion);

			mockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ExtractMessageTypeAndVersionVersionEmpty()
		{
			var mockRepository = new MockRepository();
			var envelopContext = mockRepository.PartialMock<BTEnvelopContext>();
			mockRepository.ReplayAll();

			envelopContext.InternalMessage = "FWB\r\n081-32652362SYDCHI/T10K100";
			envelopContext.ExtractMessageTypeAndVersion();
			Assert.AreEqual("FWB", envelopContext.MessageType);
			Assert.AreEqual(null, envelopContext.MessageVersion);

			mockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ExtractMessageTypeAndVersionVersion_TypeContainsNonLetters_ThrowException()
		{
			var mockRepository = new MockRepository();
			var envelopContext = mockRepository.PartialMock<BTEnvelopContext>();
			mockRepository.ReplayAll();

			envelopContext.InternalMessage = "FSU(ArR-wSD-66192195PVGTPE/?2K\r\n51.0";

			try
			{
				envelopContext.ExtractMessageTypeAndVersion();
				Assert.Fail("Should throw exception when MessageType is invalid");
			}
			catch (Exception e)
			{
				Assert.AreEqual("MessageType is not correct", e.Message);
			}

			mockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ExtractMessageTypeAndVersionVersion_InvalidMessageContent_ThrowException()
		{
			var mockRepository = new MockRepository();
			var envelopContext = mockRepository.PartialMock<BTEnvelopContext>();
			mockRepository.ReplayAll();

			envelopContext.InternalMessage = "FSU/12\r\n403-14313950JFKPUS/T3K37.3\r\n";

			try
			{
				envelopContext.ExtractMessageTypeAndVersion();
				Assert.Fail("Should throw exception when MessageContent is invalid");
			}
			catch (Exception e)
			{
				Assert.AreEqual("Invalid Message content", e.Message);
			}

			mockRepository.VerifyAll();
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void RemoveExtraHeaderFromInternalMessageHasExtraLines()
        {
            var mockRepository = new MockRepository();
            var envelopContext = mockRepository.PartialMock<BTEnvelopContext>();
            envelopContext.MessageType = "FNA";
            mockRepository.ReplayAll();

            var internalMessage = "FNA/1\r\nACK/AWB ALREADY CAPTURED\r\nQK DXBFMEK\r\n.ZCSTXXH 241043 REUAGT89DEGEIS/HAM01-C2B1AE36226B\r\nFWB/16\r\n176-30473951DUSHAN/T62K550.5\r\n";

            try
            {
                envelopContext.InternalMessage = envelopContext.RemoveExtraHeaderFromInternalMessage(internalMessage);
                Assert.AreEqual("FNA/1\r\nACK/AWB ALREADY CAPTURED\r\nFWB/16\r\n176-30473951DUSHAN/T62K550.5\r\n", envelopContext.InternalMessage);
            }
            catch (Exception e)
            {
                Assert.AreEqual("Invalid Message content", e.Message);
            }

            mockRepository.VerifyAll();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void RemoveExtraHeaderFromInternalMessageNoExtraLines()
        {
            var mockRepository = new MockRepository();
            var envelopContext = mockRepository.PartialMock<BTEnvelopContext>();
            envelopContext.MessageType = "FNA"; 
            mockRepository.ReplayAll();

            var internalMessage = "FNA/1\r\nACK/AWB ALREADY CAPTURED\r\nFWB/16\r\n176-30473951DUSHAN/T62K550.5\r\n";

            try
            {
                envelopContext.InternalMessage = envelopContext.RemoveExtraHeaderFromInternalMessage(internalMessage);
                Assert.AreEqual("FNA/1\r\nACK/AWB ALREADY CAPTURED\r\nFWB/16\r\n176-30473951DUSHAN/T62K550.5\r\n", envelopContext.InternalMessage);
            }
            catch (Exception e)
            {
                Assert.AreEqual("Invalid Message content", e.Message);
            }

            mockRepository.VerifyAll();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void RemoveExtraHeaderFromInternalMessageMultipleAckLines()
        {
            var mockRepository = new MockRepository();
            var envelopContext = mockRepository.PartialMock<BTEnvelopContext>();
            envelopContext.MessageType = "FNA";
            mockRepository.ReplayAll();

            var internalMessage = "FMA/0\r\nACK/FWB RECEIVED AND PROCESSED\r\n/Line1\r\n/Line2\r\nFWB/16\r\n176-30473951DUSHAN/T62K550.5\r\n";

            try
            {
                envelopContext.InternalMessage = envelopContext.RemoveExtraHeaderFromInternalMessage(internalMessage);
                Assert.AreEqual("FMA/0\r\nACK/FWB RECEIVED AND PROCESSED\r\n/Line1\r\n/Line2\r\nFWB/16\r\n176-30473951DUSHAN/T62K550.5\r\n", envelopContext.InternalMessage);
            }
            catch (Exception e)
            {
                Assert.AreEqual("Invalid Message content", e.Message);
            }

            mockRepository.VerifyAll();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void RemoveExtraHeaderFromInternalMessageMultipleAckLinesWithExtraLines()
        {
            var mockRepository = new MockRepository();
            var envelopContext = mockRepository.PartialMock<BTEnvelopContext>();
            envelopContext.MessageType = "FNA";
            mockRepository.ReplayAll();

            var internalMessage = "FMA/0\r\nACK/FWB RECEIVED AND PROCESSED\r\n/Line1\r\n/Line2\r\nQK DXBFMEK\r\n.ZCSTXXH 241043 REUAGT89DEGEIS/HAM01-C2B1AE36226B\r\nFWB/16\r\n176-30473951DUSHAN/T62K550.5\r\n";

            try
            {
                envelopContext.InternalMessage = envelopContext.RemoveExtraHeaderFromInternalMessage(internalMessage);
                Assert.AreEqual("FMA/0\r\nACK/FWB RECEIVED AND PROCESSED\r\n/Line1\r\n/Line2\r\nFWB/16\r\n176-30473951DUSHAN/T62K550.5\r\n", envelopContext.InternalMessage);
            }
            catch (Exception e)
            {
                Assert.AreEqual("Invalid Message content", e.Message);
            }

            mockRepository.VerifyAll();
        }
    }
}
