using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DE;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class ConsolVisualizerDocumentDataIncomingEventProcessorTest : TestCaseWithFactory
	{
		#region TestProcess

		public void TestProcessDE_ReferenceNumberUpdate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "SEA";
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "DEBRE";
				consol.JK_UniqueConsignRef = "C00000001";

				Factory.Save();

				var visualizerDocumentData = Factory.New<VisualizerDocumentData>();
				visualizerDocumentData.JDD_ParentID = consol.PK;
				visualizerDocumentData.JDD_ParentTableCode = consol.TablePrefix;
				visualizerDocumentData.JDD_Name = "XXX";

				var parameters = new Dictionary<string, string>
				{
					[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType] = GermansPortsConstants.DocumentNames.DEAdvancedLogisticsPortOrder,
					[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber] = "CRF0001",
				};

				visualizerDocumentData.Logs.CreateOrRecreateEventLog(Events.MessageAccepted, EstimateActual.Actual, ZDateTimeOffset.Now, string.Empty, parameters.ToArray());

				AssertEquals("CRF0001", consol.Numbers.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Germany && x.CE_EntryType == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber).CE_EntryNum);

				visualizerDocumentData.Logs.CreateOrRecreateEventLog(Events.MessageWithdrawCancelAccepted, EstimateActual.Actual, ZDateTimeOffset.Now, string.Empty, parameters.ToArray());

				AssertNull(consol.Numbers.OfType<CusEntryNumber>().FirstOrDefault(x => x.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Germany && x.CE_EntryType == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber));
			}
		}

		public void TestProcess_MRJ_BookingRequest()
		{
			AssertProcess_MRJ(ConsolDocumentNames.BookingRequest);
		}

		public void TestProcess_MRJ_BookingRequest_WithCSRNumberAllocationLock()
		{
			AssertProcess_MRJ(ConsolDocumentNames.BookingRequest, useCSRNumberAllocationLock: true);
		}

		public void TestProcess_MRJ_ShippingInstruction_WithoutBRAndSOSent()
		{
			AssertProcess_MRJ(ConsolDocumentNames.ShippingInstruction);
		}

		public void TestProcess_MRJ_ShippingInstruction_WithBRSent()
		{
			AssertProcess_MRJ(ConsolDocumentNames.ShippingInstruction, ConsolDocumentNames.BookingRequest, false);
		}

		public void TestProcess_MRJ_ShippingInstruction_WithSOSent()
		{
			AssertProcess_MRJ(ConsolDocumentNames.ShippingInstruction, ConsolDocumentNames.ShippingOrder, false);
		}

		public void TestProcess_MRJ_ShippingOrder()
		{
			AssertProcess_MRJ(ConsolDocumentNames.ShippingOrder);
		}

		#region FR Ports

		public void TestProcess_MAA_AMQ_JC_ExportDepotCustomsReference()
			=> AssertProcess_ContainerFieldUpdate(Events.MessageAccepted, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ, c => c.JC_ExportDepotCustomsReference);

		public void TestProcess_MAA_LDE_JC_ExportDepotCustomsReference()
			=> AssertProcess_ContainerFieldUpdate(Events.MessageAccepted, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE, c => c.JC_ExportDepotCustomsReference, "ECT00000001/1", "ECT00000001");

		public void TestProcess_MAA_LPD_JC_ImportDepotCustomsReference()
			=> AssertProcess_ContainerFieldUpdate(Events.MessageAccepted, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.ProvisionalUnpackingListLPD, c => c.JC_ImportDepotCustomsReference);

		public void TestProcess_STU_LPD_JC_ImportDepotCustomsReference()
			=> AssertProcess_ContainerFieldUpdate(Events.StatusUpdated, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Core.Constants.EventReferenceParameterTypes.LPDNotification, c => c.JC_ImportDepotCustomsReference);

		public void TestProcess_STU_LDE_JC_ExportDepotCustomsReference()
			=> AssertProcess_ContainerFieldUpdate(Events.StatusUpdated, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Core.Constants.EventReferenceParameterTypes.LDENotification, c => c.JC_ExportDepotCustomsReference);

		#endregion

		#region TMining

		public void TestProcess_ATH_TMINING_SecureContainerRelease_JC_ImportDepotCustomsReference()
			=> AssertProcess_ContainerFieldUpdate(Events.Authorised, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.DocumentNames.TMiningSecureContainerRelease, c => c.JC_ImportDepotCustomsReference, referenceNumberCode: CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber);

		public void TestProcess_ATW_TMINING_SecureContainerRelease_JC_ImportDepotCustomsReference()
			=> AssertProcess_ContainerFieldUpdate(Events.AuthorisationWithdrawn, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.DocumentNames.TMiningSecureContainerRelease, c => c.JC_ImportDepotCustomsReference, expectedCusRefnumber: string.Empty, referenceNumberCode: CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber);

		#endregion

		#region InterchangeSent

		public void TestProcess_ISN_BookingRequest()
		{
			AssertProcess_ISN(ConsolDocumentNames.BookingRequest, true);
		}

		public void TestProcess_ISN_ShippingInstruction()
		{
			AssertProcess_ISN(ConsolDocumentNames.ShippingInstruction, true);
		}

		public void TestProcess_ISN_ShippingOrder()
		{
			AssertProcess_ISN(ConsolDocumentNames.ShippingOrder, true);
		}

		public void TestProcess_ISN_VerifiedGrossContainerWeight()
		{
			AssertProcess_ISN(ConsolDocumentNames.VerifiedGrossContainerWeight, false);
		}

		public void TestProcess_ISN_VerifiedGrossContainerWeight_ContainerReferenceNumber()
			=> AssertProcess_ContainerFieldUpdate(Events.InterchangeSent, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, ConsolDocumentNames.VerifiedGrossContainerWeight, c => c.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CarrierMessageReference).CE_EntryNum, referenceNumberCode: CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber);

		#endregion

		#region Implementation

		void AssertProcess_MRJ(string documentName, string extraSentDocument = "",
			bool expectedIsGeneratedCSR = true,
			bool useCSRNumberAllocationLock = false)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_UniqueConsignRef = "C00000069";

			if (!string.IsNullOrEmpty(extraSentDocument))
			{
				var msnParameters = new Dictionary<string, string>
				{
					["MST"] = extraSentDocument
				};

				consol.Logs.CreateOrRecreateEventLog(Events.MessageSent, EstimateActual.Actual, ZDateTimeOffset.Now, string.Empty, msnParameters.ToArray());
			}

			Factory.Save();

			using (var mutex = new ZGlobalMutex(ZArchitecture.Modules.MutexIDs.CSRNumberAllocation, consol.PK.ToString()))
			{
				if (useCSRNumberAllocationLock)
				{
					mutex.Lock();
				}

				var visualizerDocumentData = Factory.New<VisualizerDocumentData>();
				visualizerDocumentData.JDD_ParentID = consol.PK;
				visualizerDocumentData.JDD_ParentTableCode = consol.TablePrefix;
				visualizerDocumentData.JDD_Name = "XXX";

				var mrjParameters = new Dictionary<string, string>
				{
					["MST"] = documentName
				};

				visualizerDocumentData.Logs.CreateOrRecreateEventLog(Events.MessageRejected, EstimateActual.Actual, ZDateTimeOffset.Now, string.Empty, mrjParameters.ToArray());

				if (useCSRNumberAllocationLock)
				{
					var csrNumber = ConsolCarrierShipperReferenceNumberCalculator.GetCarrierShipperReferenceNumber(consol);
					Assert("CSR number would not be recalculated due to mutex lock", csrNumber.IsEmpty);
				}
				else if (expectedIsGeneratedCSR)
				{
					AssertEquals("CSR number had beed recalculated after receiving MRJ", "C00000069-V1", consol.CarrierShipperReferenceWithFallback);
				}
				else
				{
					AssertEquals("CSR number would not be recalculated", "C00000069", consol.CarrierShipperReferenceWithFallback);
				}
			}
		}

		void AssertProcess_ISN(string documentName, bool isSaveRFNNumberToCMR)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";

			Factory.Save();

			var msnParameters = new Dictionary<string, string>
			{
				[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType] = documentName,
				[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber] = "refNumber"
			};

			var visualizerDocumentData = Factory.New<VisualizerDocumentData>();
			visualizerDocumentData.JDD_ParentID = consol.PK;
			visualizerDocumentData.JDD_ParentTableCode = consol.TablePrefix;
			visualizerDocumentData.JDD_Name = "XXX";

			visualizerDocumentData.Logs.CreateOrRecreateEventLog(Events.InterchangeSent, EstimateActual.Actual, ZDateTimeOffset.Now, string.Empty, msnParameters.ToArray());

			var cmrValue = consol.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CarrierMessageReference);

			if (isSaveRFNNumberToCMR)
			{
				AssertNotNull(cmrValue);
				AssertEquals("CRM Number is set", "refNumber", cmrValue.CE_EntryNum);
			}
			else
			{
				AssertNull(cmrValue);
			}
		}

		void AssertProcess_ContainerFieldUpdate(Event eventType, string eventParameterTypeCode, string eventParameterTypeValue, Func<CommonContainer, ZString> containerFieldValueGetter, string cusRefnumber = "ICT00000001", string expectedCusRefnumber = "ICT00000001", string referenceNumberCode = CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_UniqueConsignRef = "C00000069";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "DRY0001";

			Factory.Save();

			var visualizerDocumentData = Factory.New<VisualizerDocumentData>();
			visualizerDocumentData.JDD_ParentID = consol.PK;
			visualizerDocumentData.JDD_ParentTableCode = consol.TablePrefix;
			visualizerDocumentData.JDD_Name = "XXX";

			var parameters = new Dictionary<string, string>
			{
				[eventParameterTypeCode] = eventParameterTypeValue,
				[referenceNumberCode] = cusRefnumber,
				[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber] = container.JC_ContainerNum
			};

			visualizerDocumentData.Logs.CreateOrRecreateEventLog(eventType, EstimateActual.Actual, ZDateTimeOffset.Now, string.Empty, parameters.ToArray());

			AssertEquals("ExportDepotCustomsReference is set", expectedCusRefnumber, containerFieldValueGetter.Invoke(consol.Containers.Cast<CommonContainer>().First()));
		}

		#endregion

		#endregion
	}
}
