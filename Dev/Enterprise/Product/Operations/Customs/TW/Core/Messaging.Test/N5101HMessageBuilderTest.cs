using System.Collections.Generic;
using CargoWise.Customs.TW.MessageDefinitions.N5101H;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	[TestedType(typeof(N5101HMessageBuilder))]
	sealed class N5101HMessageBuilderTest : BaseTWMessageBuilderTest<IN5101HDeclaration, Declaration>
	{
		#region TestData
		class Message : IN5101HDeclaration
		{
			public Message(MessageType messageType = MessageType.FullValue)
			{
				isNodeEmpty = messageType == MessageType.NodeEmpty;
				isValueEmpty = messageType == MessageType.ValueEmpty;
				borderTransportMeansCore = new N5101HTransportMeans();
				consignmentsCore = new IN5101HConsignment[] { new N5101HConsignment(), new N5101HConsignment() };
				goodsShipmentCore = new N5101HGoodsShipment();
			}

			static ZBool isNodeEmpty = ZBool.False;
			static ZBool isValueEmpty = ZBool.False;

			public ZString FunctionalReferenceID => "97162640001012220001";

			public ZString FunctionCode => "9";

			public ZString StatusCode => isValueEmpty ? string.Empty : "Y";

			public ITransportMeans BorderTransportMeans => borderTransportMeansCore;

			readonly ITransportMeans borderTransportMeansCore;

			public ZString CarrierId => isNodeEmpty ? string.Empty : "1300730";

			public IEnumerable<IN5101HConsignment> Consignments => consignmentsCore;

			readonly IEnumerable<IN5101HConsignment> consignmentsCore;

			public ZString DeconsolidatorId => isNodeEmpty ? string.Empty : "12345678";

			public IN5101HGoodsShipment GoodsShipment => goodsShipmentCore;

			readonly IN5101HGoodsShipment goodsShipmentCore;

			public ZDate UnloadingLocationArrivalDateTime => isNodeEmpty ? ZDate.Invalid : new ZDate(2014, 7, 2);

			class N5101HTransportMeans : ITransportMeans
			{
				public ZDate ArrivalDateTime => ZDate.Invalid;

				public ZString TypeCode => "1";

				public IEnumerable<ZString> ItineraryRoutingCountryCodes => null;

				public ZString ID => isValueEmpty ? string.Empty : "8959142";

				public ZString JourneyID => "S100";

				public ZString Registration => isValueEmpty ? string.Empty : "00V001";

				public ZString Name => null;

				public ZString CallSignID => null;
			}

			class N5101HConsignment : IN5101HConsignment
			{
				public N5101HConsignment()
				{
					consigneeCore = new N5101HConsignee();
					consignmentItemCore = new N5101HConsignmentItem();
					consignorCore = new N5101HConsignor();
					loadingLocationCore = new N5101HLoadingLocation();
					notifyPartiesCore = new IPartyDetails[] { new N5101HPartyDetails(), new N5101HPartyDetails() };
					transportContractDocumentCore = new N5101HTransportContractDocument();
					transportEquipmentsCore = new ITransportEquipment[] { new N5101HTransportEquipment(), new N5101HTransportEquipment() };
				}

				public ZDecimal BoardedQuantity => isValueEmpty ? ZDecimal.Zero : 1;

				public ZDecimal TotalPackageQuantity => 100;

				public ZString EscortMark => isValueEmpty ? string.Empty : "Y";

				public ZDecimal TotalGrossMassMeasure => 15000;

				public ZString TypeCode => "1";

				public ZString AssociatedTransportDocumentId => isNodeEmpty ? string.Empty : "ACD 201011301200";

				public IPartyDetails Consignee => consigneeCore;

				readonly IPartyDetails consigneeCore;

				public IN5101HConsignmentItem ConsignmentItem => consignmentItemCore;

				readonly IN5101HConsignmentItem consignmentItemCore;

				public IPartyDetails Consignor => isNodeEmpty ? null : consignorCore;

				readonly IPartyDetails consignorCore;

				public ZString GoodsLocationId => isNodeEmpty ? string.Empty : "017A1110";

				public ILocation LoadingLocation => loadingLocationCore;

				readonly ILocation loadingLocationCore;

				public IEnumerable<IPartyDetails> NotifyParties => isNodeEmpty ? null : notifyPartiesCore;

				readonly IEnumerable<IPartyDetails> notifyPartiesCore;

				public ITransportContractDocument TransportContractDocument => transportContractDocumentCore;

				readonly ITransportContractDocument transportContractDocumentCore;

				public IEnumerable<ITransportEquipment> TransportEquipments => isNodeEmpty ? null : transportEquipmentsCore;

				readonly IEnumerable<ITransportEquipment> transportEquipmentsCore;

				public ZString UnloadingLocationId => isNodeEmpty ? null : "HKHKG";
			}

			class N5101HConsignee : IPartyDetails
			{
				public N5101HConsignee()
				{
					addressCore = new N5101HConsigneeAddress();
				}

				public ZString ID => isValueEmpty ? string.Empty : "23322708";

				public ZString Name => "COMPOSE INFORMATION CO., LTD.";

				public ZString ChineseName => isValueEmpty ? string.Empty : "複合資訊股份有限公司";

				public ZString TypeCode => isValueEmpty ? string.Empty : "58";

				public ZString CustomsControlID => null;

				public ZString PaymentOnAccountBusinessID => null;

				public ZString RoleCode => null;

				public ZString SubBoxID => null;

				public IAddress Address => isNodeEmpty ? null : addressCore;

				readonly IAddress addressCore;

				public ILPCOAuthorizedParty LPCOAuthorizedParty => null;

				public IEnumerable<ICommunication> Communications => null;

				public ZString ContactName => null;

				public ZString MainManufacturer => null;

				public ZString UndertakeCode => null;

				public ZString OwnerName => null;

				public IEnumerable<IAdditionalInformation> AdditionalInformations => null;

				class N5101HConsigneeAddress : IAddress
				{
					public ZString Line => isValueEmpty ? string.Empty : "No. 88, Fareast Rd., Chungli City, Taoyuan, Taiwan, ROC";

					public ZString ChineseLine => isValueEmpty ? string.Empty : "桃園縣中壢市遠東路 88 號";

					public ZString CountryCode => null;

					public ZString CountrySubDivisionID => null;

					public ZString CountrySubDivisionName => null;
				}
			}

			class N5101HConsignmentItem : IN5101HConsignmentItem
			{
				public N5101HConsignmentItem()
				{
					additionalInformationsCore = new IAdditionalInformation[] { new N5101HConsignmentItem_AdditionalInformation(), new N5101HConsignmentItem_AdditionalInformation() };
					commodityCore = new N5101HCommodity();
					goodsMeasureCore = new N5101HGoodsMeasure();
					packagingCore = new N5101Packaging();
				}

				public ZString Split => isValueEmpty ? string.Empty : "P";

				public ZDecimal TotalPackageQuantity => isValueEmpty ? ZDecimal.Zero : 1;

				public IEnumerable<IAdditionalInformation> AdditionalInformations => isNodeEmpty ? null : additionalInformationsCore;

				readonly IEnumerable<IAdditionalInformation> additionalInformationsCore;

				public IN5101HCommodity Commodity => commodityCore;

				readonly IN5101HCommodity commodityCore;

				public IN5101HGoodsMeasure GoodsMeasure => isNodeEmpty ? null : goodsMeasureCore;

				readonly IN5101HGoodsMeasure goodsMeasureCore;

				public IPackaging Packaging => packagingCore;

				readonly IPackaging packagingCore;

				public ZString UCRId => isNodeEmpty ? string.Empty : "OTWA00011111KNN040P10N";

				class N5101HConsignmentItem_AdditionalInformation : IAdditionalInformation
				{
					public ZInt CopyQuantity => ZInt.Zero;

					public ZString StatementCode => "1";

					public ZString StatementDescription => "Y";

					public ZString ProcessNumber => null;

					public ZString Content => null;

					public ZString ApprovalID => null;

					public ZString DelProcessNumber => null;

					ZString IAdditionalInformation.PackingHouse => null;
				}

				class N5101HCommodity : IN5101HCommodity
				{
					public N5101HCommodity()
					{
						classificationsCore = new IClassification[] { new N5101HCommodity_Classification1(), new N5101HCommodity_Classification2() };
					}

					public ZString CargoDescription => "WOVEN FABRICS OF COTTON";

					public IEnumerable<IClassification> Classifications => isNodeEmpty ? null : classificationsCore;

					readonly IEnumerable<IClassification> classificationsCore;

					class N5101HCommodity_Classification1 : IClassification
					{
						public ZString ID => "521031";

						public ZString IdentificationTypeCode => "HS";
					}

					class N5101HCommodity_Classification2 : IClassification
					{
						public ZString ID => "2306";

						public ZString IdentificationTypeCode => "SSO";
					}
				}

				class N5101HGoodsMeasure : IN5101HGoodsMeasure
				{
					public ZDecimal GrossVolumeMeasure => 1;

					public ZString VolumeUnitCode => "MTQ";
				}

				class N5101Packaging : IPackaging
				{
					public ZDecimal QuantityQuantity => ZDecimal.Zero;

					public ZString TypeCode => "PKG";

					public ZString MarksNumbers => isValueEmpty ? string.Empty : "RK1/TWKEL";

					public ZString PackagingMaterialDescription => isValueEmpty ? string.Empty : "Packaging by shipper";

					public ZString Combination => isValueEmpty ? string.Empty : "Y";

					ZDate IPackaging.PackingDateTime => ZDate.Empty;
				}
			}

			class N5101HConsignor : IPartyDetails
			{
				public N5101HConsignor()
				{
					addressCore = new N5101HConsignorAddress();
				}

				public ZString ID => isValueEmpty ? string.Empty : "23322708";

				public ZString Name => "COMPOSE INFORMATION CO., LTD.";

				public ZString ChineseName => isValueEmpty ? string.Empty : "複合資訊股份有限公司";

				public ZString TypeCode => isValueEmpty ? string.Empty : "58";

				public ZString CustomsControlID => null;

				public ZString PaymentOnAccountBusinessID => null;

				public ZString RoleCode => null;

				public ZString SubBoxID => null;

				public IAddress Address => isNodeEmpty ? null : addressCore;

				readonly IAddress addressCore;

				public ILPCOAuthorizedParty LPCOAuthorizedParty => null;

				public IEnumerable<ICommunication> Communications => null;

				public ZString ContactName => null;

				public ZString MainManufacturer => null;

				public ZString UndertakeCode => null;

				public ZString OwnerName => null;

				public IEnumerable<IAdditionalInformation> AdditionalInformations => null;

				class N5101HConsignorAddress : IAddress
				{
					public ZString Line => "No. 88, Fareast Rd., Chungli City, Taoyuan, Taiwan, ROC";

					public ZString ChineseLine => isValueEmpty ? string.Empty : "桃園縣中壢市遠東路 88 號";

					public ZString CountryCode => null;

					public ZString CountrySubDivisionID => null;

					public ZString CountrySubDivisionName => null;
				}
			}

			class N5101HLoadingLocation : ILocation
			{
				public ZString ID => "CNZ99";

				public ZString Name => isValueEmpty ? string.Empty : "陳江";

				public ZDate LoadingDateTime => ZDate.Empty;

				public ZString EstimatedLoadingCode => null;
			}

			class N5101HPartyDetails : IPartyDetails
			{
				public N5101HPartyDetails()
				{
					addressCore = new N5101HPartyDetailsAddress();
				}

				public ZString ID => isValueEmpty ? string.Empty : "23322708";

				public ZString Name => "COMPOSE INFORMATION CO., LTD.";

				public ZString ChineseName => isValueEmpty ? string.Empty : "複合資訊股份有限公司";

				public ZString TypeCode => isValueEmpty ? string.Empty : "58";

				public ZString CustomsControlID => null;

				public ZString PaymentOnAccountBusinessID => null;

				public ZString RoleCode => null;

				public ZString SubBoxID => null;

				public IAddress Address => isNodeEmpty ? null : addressCore;

				readonly IAddress addressCore;

				public ILPCOAuthorizedParty LPCOAuthorizedParty => null;

				public IEnumerable<ICommunication> Communications => null;

				public ZString ContactName => null;

				public ZString MainManufacturer => null;

				public ZString UndertakeCode => null;

				public ZString OwnerName => null;

				public IEnumerable<IAdditionalInformation> AdditionalInformations => null;

				class N5101HPartyDetailsAddress : IAddress
				{
					public ZString Line => "No. 88, Fareast Rd., Chungli City, Taoyuan, Taiwan, ROC";

					public ZString ChineseLine => isValueEmpty ? string.Empty : "桃園縣中壢市遠東路 88 號";

					public ZString CountryCode => null;

					public ZString CountrySubDivisionID => null;

					public ZString CountrySubDivisionName => null;
				}
			}

			class N5101HTransportContractDocument : ITransportContractDocument
			{
				public ZString ID => "36864619940";

				public ZString TypeCode => null;

				public IPartyDetails Deconsolidator => null;
			}

			class N5101HTransportEquipment : ITransportEquipment
			{
				public ZString CharacteristicCode => "22G1";

				public ZString ID => "CTAU0008136";

				public ZString UsedCapacityCode => "1";

				public IEnumerable<ZString> Seals => isNodeEmpty ? null : new ZString[] { "WLS018445", "WLS018446" };
			}

			class N5101HGoodsShipment : IN5101HGoodsShipment
			{
				public N5101HGoodsShipment()
				{
					consignmentCore = new N5101HConsignment();
				}
				public IN5101HConsignment Consignment => consignmentCore;

				readonly IN5101HConsignment consignmentCore;

				public ZString EntryOfficeId => "BA";

				class N5101HConsignment : IN5101HConsignment
				{
					public N5101HConsignment()
					{
						transportContractDocumentCore = new N5101HConsignment_TransportContractDocument();
					}

					public ZDecimal BoardedQuantity => ZDecimal.Zero;

					public ZDecimal TotalPackageQuantity => ZDecimal.Zero;

					public ZString EscortMark => null;

					public ZDecimal TotalGrossMassMeasure => ZDecimal.Zero;

					public ZString TypeCode => null;

					public ZString AssociatedTransportDocumentId => null;

					public IPartyDetails Consignee => null;

					public IN5101HConsignmentItem ConsignmentItem => null;

					public IPartyDetails Consignor => null;

					public ZString GoodsLocationId => "017A1110";

					public ILocation LoadingLocation => null;

					public IEnumerable<IPartyDetails> NotifyParties => null;

					public ITransportContractDocument TransportContractDocument => transportContractDocumentCore;

					readonly ITransportContractDocument transportContractDocumentCore;

					public IEnumerable<ITransportEquipment> TransportEquipments => null;

					public ZString UnloadingLocationId => null;

					class N5101HConsignment_TransportContractDocument : ITransportContractDocument
					{
						public ZString ID => "NYKS4030093130";

						public ZString TypeCode => null;

						public IPartyDetails Deconsolidator => null;
					}
				}
			}
		}

		enum MessageType
		{
			FullValue,
			NodeEmpty,
			ValueEmpty
		}
		#endregion

		[ExpectNoExceptions]
		public void TestPopulateDeclaration()
		{
			var message = new Message();
			var declaration = new N5101HMessageBuilder().PopulateDeclaration(message);

			var xml = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			AssertXMLContains(GetExpectedMessageXML("N5101H_FullValue.xml"), xml);
			CombineAssertions(() =>
			{
				AsserContainsXmlValueByType(xml, message, typeof(Message));
				AsserContainsXmlByType(xml, declaration.GetType());
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclarationNodeEmpty()
		{
			var message = new Message(MessageType.NodeEmpty);
			var declaration = new N5101HMessageBuilder().PopulateDeclaration(message);

			var xml = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			AssertXMLContains(GetExpectedMessageXML("N5101H_NodeEmpty.xml"), xml);
			AsserContainsXmlValueByType(xml, message, typeof(Message));
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclarationValueEmpty()
		{
			var message = new Message(MessageType.ValueEmpty);
			var declaration = new N5101HMessageBuilder().PopulateDeclaration(message);

			var xml = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			AssertXMLContains(GetExpectedMessageXML("N5101H_ValueEmpty.xml"), xml);
			CombineAssertions(() =>
			{
				AsserContainsXmlValueByType(xml, message, typeof(Message));
				AsserContainsXmlByType(xml, declaration.GetType());
			});
		}

		public void TestPopulateXml()
		{
			var message = new Message();
			var xml = new N5101HMessageBuilder().PopulateXml(message);
			AssertXMLContains(GetExpectedMessageXML("N5101H_FullValue.xml"), xml);
		}

		protected override ITWMessageBuilder CreateNewMessageBuilder()
		{
			return new N5101HMessageBuilder();
		}

		protected override IN5101HDeclaration CreateDataSource()
		{
			return new Message();
		}

		protected override ZString FunctionCode => "9";
	}
}
