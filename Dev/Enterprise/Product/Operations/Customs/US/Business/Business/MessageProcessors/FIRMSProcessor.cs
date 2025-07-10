using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	#region Error Processing

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	class ACEFIRMSErrorProcessorA : FIRMSErrorProcessorA<AABIOutputA, AABIOutputB, AABIOutputY>
	{
	}

	[TopLevel(typeof(ERFEError), typeof(ERFF111), typeof(ERFF411))]
	abstract class FIRMSErrorProcessorA<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : FIRMSErrorProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	class ACEFIRMSErrorProcessorB : FIRMSErrorProcessorB<AABIOutputA, AABIOutputB, AABIOutputY>
	{
	}

	[TopLevel(typeof(ERFF111), typeof(ERFF411))]
	abstract class FIRMSErrorProcessorB<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : FIRMSErrorProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
	}

	abstract class FIRMSErrorProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : ABIProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		internal const string FIRMSCodeNotOnFile = "012";

		public override void Process()
		{
			USCFIRMS firm = null;

			HtmlTableCreator htmlTable = new HtmlTableCreator(new string[] { "Code", "Message" });
			string reference = "";

			foreach (MessageBlock block in messageBlocks)
			{
				ERFF111 erff111 = block as ERFF111;
				if (erff111 != null)
				{
					firm = Message.Factory.LoadTop1<USCFIRMS>(new ZQuery(USCFIRMSSchema.US_Code, erff111.FIRMSCode));
					reference = "FIRMS Code: '" + erff111.FIRMSCode + "' Name of Facility: '" + erff111.NameOfFacility + "' District Code: '" + erff111.DistrictCode + "'";
				}

				ERFF411 erff411 = block as ERFF411;
				if (erff411 != null)
				{
					if (!erff411.ErrorCode.IsEmpty || !erff411.ErrorMessage.IsEmpty)
					{
						htmlTable.WriteRow(erff411.ErrorCode, erff411.ErrorMessage);
						var branch = Message.OriginalMessage != null ? Message.OriginalMessage.Branch : GlbBranch.CurrentBranch;
						GenerateHtmlEmailAndSendToOriginalOrGroup("", reference, "FIRMS Request", htmlTable.ToHtml(), false, branch, null);

						if (erff411.ErrorCode == FIRMSCodeNotOnFile)
						{
							if (firm != null)
							{
								firm.US_IsActive = false;
							}
						}
					}
				}
			}
		}
	}

	#endregion

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	class ACEFIRMSProcessor : FIRMSProcessor<AABIOutputA, AABIOutputB, AABIOutputY>
	{
	}

	[TopLevel(typeof(ERFF211), typeof(ERFF311), typeof(ERFF411))]
	abstract class FIRMSProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : ABIWithDatabaseLockProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		#region Processing

		int? MessageCount
		{
			get
			{
				if (messageCount == null)
				{
					messageCount = messageBlocks.Count(x => x is ERFF211);
				}
				return messageCount;
			}
		}
		int? messageCount;

		protected override bool IsRelatedToSingleRecord
		{
			get { return MessageCount == 1; }
		}

		public override void Process()
		{
			USCFIRMS firm = null;
			string reference = "";
			emailBodyBuilder = new ZStringBuilder();

			foreach (MessageBlock block in messageBlocks)
			{
				ERFF211 erff211 = block as ERFF211;
				if (erff211 != null)
				{
					if (!erff211.FIRMSCode.IsEmpty)
					{
						firm = Message.Factory.LoadTop1<USCFIRMS>(new ZQuery(USCFIRMSSchema.US_Code, erff211.FIRMSCode));

						if (firm == null)
						{
							firm = Message.Factory.New<USCFIRMS>();
							firm.US_Code = erff211.FIRMSCode;
						}
					}
					else
					{
						firm = null;
					}

					reference = "FIRMS Code: '" + erff211.FIRMSCode + "' Name of Facility: '" + erff211.NameOfFacility + "' District Code: '" + erff211.DistrictPortCode + "'";
					ProcessF211(firm, erff211);
				}
				else
				{
					ERFF311 erff311 = block as ERFF311;
					if (erff311 != null)
					{
						ProcessF311(firm, erff311);
					}
					else
					{
						ERFF411 erff411 = block as ERFF411;
						if (erff411 != null)
						{
							ProcessF411(firm, erff411);
						}
					}
				}
			}

			var branch = Message.OriginalMessage != null ? Message.OriginalMessage.Branch : GlbBranch.CurrentBranch;
			if (!IsReferenceRequestedByServiceTask)
			{
				if (IsRelatedToSingleRecord)
				{
					GenerateHtmlEmailAndSendToOriginalOrGroup("", reference, "FIRMS Request", emailBodyBuilder.ToString(), false, branch, null);
				}
				else if (Message.EM_ApplicationReference != EmailSent)
				{
					GenerateHtmlEmailAndSendToOriginalOrGroup("", "", "FIRMS Request", "Your query for multiple FIRMS codes was successful. Please check your reference files for updated information", false, branch, null);
				}
				MarkAsEmailSent();
			}
		}

		ZStringBuilder emailBodyBuilder;

		void ProcessF211(USCFIRMS firm, ERFF211 erff211)
		{
			var districtPortCode = erff211.DistrictPortCode.PadLeft(4, '0');
			var facilityType = erff211.FacilityType.ToString().PadLeft(2, '0');

			if (firm != null)
			{
				firm.US_DistrictPortCode = districtPortCode;
				firm.US_FacilityType = facilityType;
				firm.US_LastUpdate = erff211.LastUpdate;
				firm.US_Name = erff211.NameOfFacility;
				firm.US_IsActive = erff211.Status == "A";
			}

			emailBodyBuilder.Append("District Port Code: " + districtPortCode + "<br/>");
			emailBodyBuilder.Append("Facility Type: " + facilityType + "<br/>");
			emailBodyBuilder.Append("Last Update: " + erff211.LastUpdate + "<br/>");
			emailBodyBuilder.Append("Facility Name: " + erff211.NameOfFacility + "<br/>");
			emailBodyBuilder.Append("Status: " + ((erff211.Status == "A") ? "Active" : "Inactive") + "<br/>");
		}

		void ProcessF311(USCFIRMS firm, ERFF311 erff311)
		{
			if (firm != null)
			{
				firm.US_City = erff311.City;
				firm.US_Address = erff311.FacilityAddress;
				firm.US_State = erff311.State;
			}

			emailBodyBuilder.Append("City: " + erff311.City + "<br/>");
			emailBodyBuilder.Append("Address: " + erff311.FacilityAddress + "<br/>");
			emailBodyBuilder.Append("State: " + erff311.State + "<br/>");
		}

		void ProcessF411(USCFIRMS firm, ERFF411 erff411)
		{
			if (firm != null)
			{
				firm.US_Country = erff411.Country;
				firm.US_ZipCode = erff411.ZIPCode;
			}

			emailBodyBuilder.Append("Country: " + erff411.Country + "<br/>");
			emailBodyBuilder.Append("ZIP Code: " + erff411.ZIPCode + "<p/>");
		}

		#endregion

		protected override CargoWise.Definitions.Customs.US.ReferenceLockType ReferenceLockType
		{
			get { return CargoWise.Definitions.Customs.US.ReferenceLockType.FIRMS; }
		}
	}
}
