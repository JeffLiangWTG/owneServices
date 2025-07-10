using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Internal;
using Enterprise.Customs.US.Business.RefDbEntUS;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	class ACECarrierProcessorErrorProcessor : CarrierProcessorErrorProcessor<ACEABIProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
	}

	[TopLevel(typeof(ERFEF106), typeof(ERFEError))]
	abstract class CarrierProcessorErrorProcessor<T, ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : ABIProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where T : ABIProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		public override bool RequiresSeparateFactory
		{
			get { return true; }
		}

		public override void Process()
		{
			HtmlTableCreator htmlTable = new HtmlTableCreator(new string[] { "Carrier Code", "Carrier Name", "Message" });
			string carrierCode = string.Empty;
			string carrierName = string.Empty;

			foreach (MessageBlock block in messageBlocks)
			{
				ERFEF106 erfef106 = block as ERFEF106;
				if (erfef106 != null)
				{
					carrierCode = erfef106.CarrierCode;
					carrierName = erfef106.CarrierName;
				}

				ERFEError erfeError = block as ERFEError;
				if (erfeError != null)
				{
					if (!erfeError.NarrativeMessage.IsEmpty)
					{
						htmlTable.WriteRow(carrierCode, carrierName, erfeError.NarrativeMessage);
					}
				}
			}

			var branch = Message.OriginalMessage != null ? Message.OriginalMessage.Branch : GlbBranch.CurrentBranch;
			GenerateHtmlEmailAndSendToOriginalOrGroup("", "Carrier Code Request", "Carrier Code Request", htmlTable.ToHtml(), true, branch, null);

			Factory.Save();
		}
	}

	[TopLevel(typeof(ERFF106), typeof(AERFF906))]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	class ACECarrierProcessor : CarrierProcessor<ACEABIProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
	}

	abstract class CarrierProcessor<T, ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : DeletingProcessor<USCarrier, ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where T : ABIProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		protected override bool IsRelatedToSingleRecord
		{
			get { return DistinctCarrierCount == 1; }
		}

		ZInt DistinctCarrierCount
		{
			get
			{
				if (distinctCarrierCount.IsEmpty && messageBlocks.Count > 0)
				{
					distinctCarrierCount = messageBlocks.OfType<ERFF106>().Select(x => x.CarrierCode + x.CarrierName).Distinct().Count();
				}
				return distinctCarrierCount;
			}
		}
		ZInt distinctCarrierCount;

		public override void Process()
		{
			AddFetchIntoFactory();

			var carrierData = new CarrierData();
			var carrierDatas = new List<CarrierData>();
			var carrierKey = ZString.Empty;

			var htmlTable = new HtmlTableCreator(new[] { "Carrier Code", "Carrier Name", "Error Code", "Error Message" });
			var isFailure = false;

			foreach (var block in messageBlocks)
			{
				var previousCarrierKey = carrierKey;
				var needSave = false;

				var f106 = block as ERFF106;
				if (f106 != null)
				{
					carrierKey = f106.CarrierCode + f106.CarrierName;

					if (previousCarrierKey != carrierKey)
					{
						carrierData = new CarrierData
						{
							CarrierName = f106.CarrierName,
							CarrierCode = f106.CarrierCode,
							CarrierModeOfTransportation = f106.CarrierModeOfTransportation,
							AirwayBillPrefix = f106.AirwayBillPrefix,
							CarrierAddress = f106.CarrierAddress
						};

						needSave = true;
					}
					else
					{
						carrierData.CarrierAddress += " ";
						carrierData.CarrierAddress += f106.CarrierAddress;
					}
				}
				else
				{
					var f906 = block as AERFF906;
					if (f906 != null && (!f906.ErrorCode.IsEmpty || !f906.ErrorMessage.IsEmpty))
					{
						isFailure = true;
						needSave = false;

						htmlTable.WriteRow(carrierData.CarrierCode, carrierData.CarrierName, f906.ErrorCode, f906.ErrorMessage);
					}
				}

				if (needSave && carrierDatas.All(c => c != carrierData))
				{
					carrierDatas.Add(carrierData);
				}
			}

			foreach (var data in carrierDatas)
			{
				GetOrCreateCarrier(data);
			}

			var branch = Message.OriginalMessage != null ? Message.OriginalMessage.Branch : GlbBranch.CurrentBranch;
			if (!IsReferenceRequestedByServiceTask)
			{
				if (IsRelatedToSingleRecord)
				{
					var emailBodyBuilder = new ZStringBuilder();
					emailBodyBuilder.Append("Carrier Name: " + carrierData.CarrierName + "<br/>");
					emailBodyBuilder.Append("Carrier Code: " + carrierData.CarrierCode + "<br/>");
					emailBodyBuilder.Append("Carrier Mode of Transportation: " + carrierData.CarrierModeOfTransportation + "<br/>");
					emailBodyBuilder.Append("Airway Bill Prefix: " + carrierData.AirwayBillPrefix + "<br/>");
					emailBodyBuilder.Append("Carrier Address: " + carrierData.CarrierAddress + "<br/>");

					if (isFailure)
					{
						emailBodyBuilder.Append("<br/>" + "Error Information:" + "<br/>");
						emailBodyBuilder.Append(htmlTable.ToHtml() + "<br/>");
					}

					GenerateHtmlEmailAndSendToOriginalOrGroup(string.Empty, carrierData.CarrierName + " (" + carrierData.CarrierCode + ")", "Carrier Code Request", emailBodyBuilder.ToString(), isFailure, branch, null);
				}
				else
				{
					var body = isFailure ? htmlTable.ToHtml() : "Your query for carrier codes was successful. Please check your reference files for updated information.";
					GenerateHtmlEmailAndSendToOriginalOrGroup(string.Empty, "Carrier Code Request", "Carrier Code Request", body, isFailure, branch, null);
				}
			}
		}

		void GetOrCreateCarrier(CarrierData carrierData)
		{
			if (!carrierData.CarrierCode.IsEmpty)
			{
				var name = carrierData.CarrierName;
				var code = carrierData.CarrierCode;
				var modeOfTransportation = carrierData.CarrierModeOfTransportation;
				var airwayBillPrefix = carrierData.AirwayBillPrefix;
				var address = carrierData.CarrierAddress.Left(USCarrier.Schema.USC_AddressMaxLength);
				if (HasMatchingSystemCarrier(code, name, modeOfTransportation, airwayBillPrefix, address))
				{
					Factory.Load<USCarrier>(new ZQuery(USCarrierSchema.USC_Code, carrierData.CarrierCode)).DeleteAll();
				}
				else
				{
					var carriers = new List<USCarrier>(Factory.Load<USCarrier>(new ZQuery(USCarrierSchema.USC_Code, code)));
					var carrier = carriers.FirstOrDefault();
					if (carrier == null)
					{
						carrier = Factory.New<USCarrier>();
					}
					else
					{
						carriers.Remove(carrier);
					}
					carrier.USC_Code = code;
					carrier.USC_Name = name;
					carrier.USC_ModeOfTransportation = modeOfTransportation;
					carrier.USC_AirwayBillPrefix = airwayBillPrefix;
					carrier.USC_Address = address;
					carriers.DeleteAll();
				}
			}
		}

		bool HasMatchingSystemCarrier(ZString code, ZString name, ZString modeOfTransportation, ZString airwayBillPrefix, ZString address)
		{
			var query = new ZQuery(USCCarrierSchema.UI_Code, code);
			query.AddToFilter(USCCarrierSchema.UI_Name, name);
			query.AddToFilter(USCCarrierSchema.UI_ModeOfTransportation, modeOfTransportation);
			query.AddToFilter(USCCarrierSchema.UI_AirwayBillPrefix, airwayBillPrefix);
			query.AddToFilter(USCCarrierSchema.UI_Address, address);
			return Factory.LoadTop1<USCCarrier>(query) != null;
		}

		void AddFetchIntoFactory()
		{
			var carrierCodes = new List<ZString>();

			foreach (var block in messageBlocks)
			{
				var f106 = block as ERFF106;
				if (f106 != null && !f106.CarrierCode.IsEmpty && !carrierCodes.Contains(f106.CarrierCode))
				{
					carrierCodes.Add(f106.CarrierCode);
				}
			}

			if (carrierCodes.Count > 0)
			{
				var carrierCodesArray = carrierCodes.ToArray();
				Factory.AddFetchHint(USCCarrierSchema.Instance, new ZQuery(USCCarrierSchema.UI_Code, carrierCodesArray));
				Factory.AddFetchHint(USCarrierSchema.Instance, new ZQuery(USCarrierSchema.USC_Code, carrierCodesArray));
			}
		}

		#region CarrierData

		class CarrierData
		{
			public ZString CarrierName
			{
				get;
				set;
			}

			public ZString CarrierCode
			{
				get;
				set;
			}

			public ZString CarrierModeOfTransportation
			{
				get;
				set;
			}

			public ZString AirwayBillPrefix
			{
				get;
				set;
			}

			public ZString CarrierAddress
			{
				get;
				set;
			}
		}

		#endregion

		protected override CargoWise.Definitions.Customs.US.ReferenceLockType ReferenceLockType
		{
			get { return CargoWise.Definitions.Customs.US.ReferenceLockType.Carrier; }
		}
	}
}
