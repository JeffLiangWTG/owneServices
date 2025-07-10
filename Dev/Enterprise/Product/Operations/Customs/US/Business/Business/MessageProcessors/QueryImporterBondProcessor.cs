using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse)]
	[TopLevel(typeof(AQIBK1), typeof(AQIBK2), typeof(AQIBK3), typeof(AQIBK4), typeof(AQIBK5), typeof(AQIBK6), typeof(AQIBK7), typeof(AQIBK8))]
	public class QueryImporterBondProcessor : ACEABIProcessor
	{
		public class BondData
		{
			public ZString ActivityCode;
			public ZString BondNumber;
			public ZString SuretyCode;
			public ZDecimal BondAmount;
			public ZDate BondEffectiveDate;
			public ZString BondFiledPort;
			public ZDate BondTerminationDate;
			public ZBool BondSufficiencyIndicator;
		}

		BondData bondData;
		HtmlTableCreator bondHTMLTable;
		List<BondData> bondDataCollection;
		ZStringBuilder htmlBody;
		bool bondDataHasBeenSentByCustoms;
		bool someBondsExpired;

		class EntityData
		{
			public ZString Name;
			public ZString AddressLine1;
			public ZString AddressLine2;
			public ZString City;
			public ZString State;
			public ZString Zip;

			public void ClearData()
			{
				Name = ZString.Empty;
				AddressLine1 = ZString.Empty;
				AddressLine2 = ZString.Empty;
				City = ZString.Empty;
				State = ZString.Empty;
				Zip = ZString.Empty;
			}
		}

		public override void Process()
		{
			var mailAddressData = new EntityData();
			EntityData physicalAddressData = null;
			ZString importerNumber = ZString.Empty;
			ZString nameQualifier = ZString.Empty;
			ZString queryResultsCode = ZString.Empty;
			ZString periodicMonthlyStatementStatus = ZString.Empty;
			ZString fullLegalImporterName = ZString.Empty;
			ZString centerIdentifier = ZString.Empty;
			ZString fullCenterIDDescription = ZString.Empty;

			bondData = null;
			bondDataCollection = new List<BondData>();
			htmlBody = new ZStringBuilder();
			bondHTMLTable = new HtmlTableCreator(new string[] { "Result", "Surety Code", "Type", "Amount", "Port", "Effective", "Termination", "Number", "Bond is Sufficient?", "Bond User Status", "Bond User Termination Date" });

			AQIBK1 qibk1 = null;
			AQIBK2 qibk2 = null;
			foreach (MessageBlock block in messageBlocks)
			{
				if (block is AQIBK1)
				{
					if (!mailAddressData.Name.IsEmpty && !importerNumber.IsEmpty && !queryResultsCode.IsEmpty)
					{
						WriteBondToEmailAndAddToCollection(qibk1, qibk2);
						AppendImporterDataToHTML(importerNumber, queryResultsCode, nameQualifier, mailAddressData, periodicMonthlyStatementStatus, physicalAddressData, fullLegalImporterName, centerIdentifier, fullCenterIDDescription);
						bondHTMLTable = new HtmlTableCreator(new string[] { "Result", "Surety Code", "Type", "Amount", "Port", "Effective", "Termination", "Number", "Bond is Sufficient?", "Bond User Status", "Bond User Termination Date" });

						mailAddressData.ClearData();
						physicalAddressData = null;
						importerNumber = ZString.Empty;
						nameQualifier = ZString.Empty;
						queryResultsCode = ZString.Empty;
						periodicMonthlyStatementStatus = ZString.Empty;
						fullLegalImporterName = ZString.Empty;
						centerIdentifier = ZString.Empty;
						fullCenterIDDescription = ZString.Empty;
					}
					qibk1 = block as AQIBK1;
					mailAddressData.Name = qibk1.ImportersName;
					importerNumber = qibk1.ImporterNumber;
					queryResultsCode = QueryResultCodeToText(qibk1.QueryResultsCode.ToString());
					qibk2 = null;
				}
				else
				{
					if (block is AQIBK2)
					{
						qibk2 = block as AQIBK2;
						nameQualifier = qibk2.NameQualifier + " " + qibk2.LineTwoOfImporterName;
						periodicMonthlyStatementStatus = qibk2.PeriodicMonthlyStatementStatus;
					}
					else
					{
						var qibk3 = block as AQIBK3;
						if (qibk3 != null)
						{
							mailAddressData.AddressLine1 = qibk3.AddressLineOne;
							mailAddressData.AddressLine2 = qibk3.AddressLineTwo;
						}
						else
						{
							var qibk4 = block as AQIBK4;
							if (qibk4 != null)
							{
								mailAddressData.City = qibk4.City;
								mailAddressData.State = qibk4.StateCode;
								mailAddressData.Zip = qibk4.PostalCode;
							}
							else
							{
								var qibk5 = block as AQIBK5;
								if (qibk5 != null)
								{
									physicalAddressData = physicalAddressData ?? new EntityData();
									physicalAddressData.AddressLine1 = qibk5.AddressLineOne;
									physicalAddressData.AddressLine2 = qibk5.AddressLineTwo;
								}
								else
								{
									var qibk6 = block as AQIBK6;
									if (qibk6 != null)
									{
										physicalAddressData = physicalAddressData ?? new EntityData();
										physicalAddressData.City = qibk6.City;
										physicalAddressData.State = qibk6.StateCode;
										physicalAddressData.Zip = qibk6.PostalCode;
									}
									else
									{
										var qibk7 = block as AQIBK7;
										if (qibk7 != null)
										{
											fullLegalImporterName = qibk7.FullLegalImporterName.PadRight(GetMessageBlockStringAttributeLength(typeof(AQIBK7), "FullLegalImporterName"));
											centerIdentifier = qibk7.CenterIdentifier;
											fullCenterIDDescription = qibk7.CenterIDDescription.PadRight(GetMessageBlockStringAttributeLength(typeof(AQIBK7), "CenterIDDescription"));
										}
										else
										{
											var qibk8 = block as AQIBK8;
											if (qibk8 != null)
											{
												if (qibk8.AdditionalInformationQualifierCode == "IN1")
												{
													fullLegalImporterName = fullLegalImporterName + qibk8.AdditionalInformation.PadRight(GetMessageBlockStringAttributeLength(typeof(AQIBK8), "AdditionalInformation"));
												}
												else if (qibk8.AdditionalInformationQualifierCode == "IN2")
												{
													fullCenterIDDescription = fullCenterIDDescription + qibk8.AdditionalInformation.PadRight(GetMessageBlockStringAttributeLength(typeof(AQIBK8), "AdditionalInformation"));
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}

			if (qibk1 != null)
			{
				WriteBondToEmailAndAddToCollection(qibk1, qibk2);
			}
			AppendImporterDataToHTML(importerNumber, queryResultsCode, nameQualifier.TrimEnd(), mailAddressData, periodicMonthlyStatementStatus, physicalAddressData, fullLegalImporterName, centerIdentifier, fullCenterIDDescription);
			LinkToOrganisationAndSendEmail(htmlBody);
		}

		int GetMessageBlockStringAttributeLength(Type type, string fieldName)
		{
			var messageBlockStringAttribute = (MessageBlockStringAttribute)type.GetField(fieldName).GetCustomAttributes(typeof(MessageBlockStringAttribute))?.FirstOrDefault();
			if (messageBlockStringAttribute == null)
			{
				throw new DeveloperNotificationException(string.Format(CultureInfo.InvariantCulture, "There should be a {0} field in {1}, and the field {0} should have a MessageBlockStringAttribute.", fieldName, type.Name));
			}
			else
			{
				return messageBlockStringAttribute.Length;
			}
		}

		void AppendImporterDataToHTML(ZString importerNumber, ZString queryResultsCode, ZString nameQualifier, EntityData mailAddressData, ZString periodicMonthlyStatementStatus, EntityData physicalAddressData, ZString fullLegalImporterName, ZString centerIdentifier, ZString fullCenterIDDescription)
		{
			fullLegalImporterName = fullLegalImporterName.Trim();
			fullCenterIDDescription = fullCenterIDDescription.Trim();

			htmlBody.Append("Importer Name: " + mailAddressData.Name + "<br />");
			if (!SocialSecurityNumberValidator.IsValidSSN(importerNumber))
			{
				htmlBody.Append("Importer Number: " + importerNumber + "<br />");
			}
			htmlBody.Append("Query Result Code: " + queryResultsCode + "<br />");
			if (!nameQualifier.IsEmpty)
			{
				htmlBody.Append("Name Qualifier: " + nameQualifier + "<br />");
			}

			if (!mailAddressData.AddressLine1.IsEmpty)
			{
				htmlBody.Append("Address Line 1: " + mailAddressData.AddressLine1 + "<br />");
			}

			if (!mailAddressData.AddressLine2.IsEmpty)
			{
				htmlBody.Append("Address Line 2: " + mailAddressData.AddressLine2 + "<br />");
			}

			if (!mailAddressData.City.IsEmpty)
			{
				htmlBody.Append("City: " + mailAddressData.City + "<br />");
			}

			if (!mailAddressData.State.IsEmpty)
			{
				htmlBody.Append("State: " + mailAddressData.State + "<br />");
			}

			if (!mailAddressData.Zip.IsEmpty)
			{
				htmlBody.Append("Zip: " + mailAddressData.Zip + "<br />");
			}

			if (!periodicMonthlyStatementStatus.IsEmpty)
			{
				htmlBody.Append("Periodic Monthly Statement Status: " + periodicMonthlyStatementStatus + "<br />");
			}

			htmlBody.Append("<br />");

			if (physicalAddressData != null)
			{
				htmlBody.Append("Physical Address Details:<br />");
				if (!physicalAddressData.Name.IsEmpty)
				{
					htmlBody.Append("Name: " + physicalAddressData.Name + "<br />");
				}

				if (!physicalAddressData.AddressLine1.IsEmpty)
				{
					htmlBody.Append("Address Line 1: " + physicalAddressData.AddressLine1 + "<br />");
				}

				if (!physicalAddressData.AddressLine2.IsEmpty)
				{
					htmlBody.Append("Address Line 2: " + physicalAddressData.AddressLine2 + "<br />");
				}

				if (!physicalAddressData.City.IsEmpty)
				{
					htmlBody.Append("City: " + physicalAddressData.City + "<br />");
				}

				if (!physicalAddressData.State.IsEmpty)
				{
					htmlBody.Append("State: " + physicalAddressData.State + "<br />");
				}

				if (!physicalAddressData.Zip.IsEmpty)
				{
					htmlBody.Append("Zip: " + physicalAddressData.Zip + "<br />");
				}
				htmlBody.Append("<br />");
			}

			if (!fullLegalImporterName.IsEmpty)
			{
				htmlBody.Append("Full Legal Importer Name: " + fullLegalImporterName + "<br />");
			}

			if (!centerIdentifier.IsEmpty)
			{
				htmlBody.Append("Center Identifier: " + centerIdentifier + "<br />");
			}

			if (!fullCenterIDDescription.IsEmpty)
			{
				htmlBody.Append("Center ID Description: " + fullCenterIDDescription + "<br />");
			}

			if (!fullLegalImporterName.IsEmpty || !centerIdentifier.IsEmpty || !fullCenterIDDescription.IsEmpty)
			{
				htmlBody.Append("<br />");
			}

			if (bondDataHasBeenSentByCustoms)
			{
				htmlBody.Append(bondHTMLTable.ToHtml());
			}

			htmlBody.Append("<br />");

			bondHTMLTable = null;
		}

		void WriteBondToEmailAndAddToCollection(AQIBK1 qibk1, AQIBK2 qibk2)
		{
			if (qibk1 != null && !qibk1.SuretyCode.IsEmpty && !qibk1.BondTypeActivityCode.IsEmpty && !qibk1.BondNumber.IsEmpty)
			{
				var bondType = qibk1.QueryResultsCode;
				ZDate bondTerminationDate = ZDate.Empty;
				ZBool bondSufficiencyIndicator = ZBool.False;
				ZString bondUserStatusIndicator = ZString.Empty;
				ZDate bondUserTerminationDate = ZDate.Empty;
				if (qibk2 != null)
				{
					bondTerminationDate = qibk2.BondTerminationDate;
					bondSufficiencyIndicator = qibk2.BondSufficiencyIndicator.ToUpper() == "Y";
					switch (qibk2.BondUserStatusIndicator)
					{
						case "A":
							bondUserStatusIndicator = "Active";
							break;
						case "T":
							bondUserStatusIndicator = "Terminated";
							break;
					}
					bondUserTerminationDate = qibk2.BondUserTerminationDate;
				}

				bondHTMLTable.WriteRow(ResultCodeToText(bondType), qibk1.SuretyCode, ActivityCodeToText(qibk1.BondTypeActivityCode), qibk1.BondAmount, qibk1.DistrictPortWhereBondWasFiled,
					qibk1.BondEffectiveDate, bondTerminationDate, qibk1.BondNumber, bondSufficiencyIndicator, bondUserStatusIndicator, bondUserTerminationDate);

				if (bondType == ContinuousBond)
				{
					bondData = new BondData();
					bondData.ActivityCode = qibk1.BondTypeActivityCode;
					bondData.BondAmount = (ZDecimal)qibk1.BondAmount;
					bondData.BondEffectiveDate = qibk1.BondEffectiveDate;
					bondData.BondFiledPort = qibk1.DistrictPortWhereBondWasFiled;
					bondData.BondNumber = qibk1.BondNumber;
					bondData.SuretyCode = qibk1.SuretyCode.PadLeft(3, '0');
					bondData.BondTerminationDate = bondTerminationDate;
					bondData.BondSufficiencyIndicator = bondSufficiencyIndicator;
					bondDataCollection.Add(bondData);
				}

				bondDataHasBeenSentByCustoms = true;
			}
			else
			{
				bondDataHasBeenSentByCustoms = false;
			}
		}

		void LinkToOrganisationAndSendEmail(ZStringBuilder htmlBody)
		{
			ZString uri = ZString.Empty;
			OrgHeader linkedOrganisation = OriginalMessageLinker.Link<OrgHeader>(Message);
			ZString linkedOrganisationCode = ZString.Empty;

			var qIBK1Blocks = messageBlocks.OfType<AQIBK1>();
			if (qIBK1Blocks.IsCountMoreThan(1))
			{
				linkedOrganisationCode = "Multiple Importer Numbers";
			}
			else if (qIBK1Blocks.IsCountEqualTo(1))
			{
				linkedOrganisationCode = qIBK1Blocks.ElementAt(0).ImporterNumber;
			}

			if (linkedOrganisation != null)
			{
				linkedOrganisationCode = linkedOrganisation.OH_Code;
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Organisation, linkedOrganisation.PK.ToGuid());

				OrgHeaderWrapper wrappedOrganisation = OrgHeaderWrapper.New(linkedOrganisation);
				wrappedOrganisation.ZO_IsEINNumberVerifiedIndicator = qIBK1Blocks.Any(k1 => k1.QueryResultsCode == "1" || k1.QueryResultsCode == "2") ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No;

				CreateOrgBondDetails(linkedOrganisation, bondDataCollection);

				if (someBondsExpired)
				{
					htmlBody.Append("<b>" + BondExpiredAdvice + "</ b><br />");
				}
			}
			var branch = Message.OriginalMessage != null ? Message.OriginalMessage.Branch : GlbBranch.CurrentBranch;
			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, linkedOrganisationCode, "Query Importer Bond", htmlBody.ToString(), false, branch, linkedOrganisation);
		}

		string ResultCodeToText(ZString code)
		{
			switch (code)
			{
				case "0":
					return "Not on file";
				case "1":
					return "Continuous";
				case "2":
					return "No bond";
				case "3":
					return "Voided";
				case "4":
					return "Inactive";
				default:
					return "Unknown (" + code + ")";
			}
		}

		string ActivityCodeToText(string code)
		{
			switch (code)
			{
				case "A":
					return ActivityCodeList.Descriptions._1;
				case "B":
					return ActivityCodeList.Descriptions._1a;
				case "C":
					return ActivityCodeList.Descriptions._1a1;
				case "D":
					return ActivityCodeList.Descriptions._2;
				case "E":
					return ActivityCodeList.Descriptions._3;
				case "F":
					return ActivityCodeList.Descriptions._3a;
				case "G":
					return ActivityCodeList.Descriptions._3a3;
				case "H":
					return ActivityCodeList.Descriptions._4;
				case "J":
					return ActivityCodeList.Descriptions._5;
				case "K":
					return ActivityCodeList.Descriptions._11;
				case "L":
					return ActivityCodeList.Descriptions._12;
				case "M":
					return ActivityCodeList.Descriptions._13;
				case "N":
					return ActivityCodeList.Descriptions._14;
				case "O":
					return ActivityCodeList.Descriptions._15;
				case "P":
					return ActivityCodeList.Descriptions._16;
				case "Q":
					return ActivityCodeList.Descriptions._17;
				case "R":
					return ActivityCodeList.Descriptions._18;
				case "S":
					return ActivityCodeList.Descriptions._19;
				case "T":
					return ActivityCodeList.Descriptions._20;
				default:
					return "Unknown (" + code + ")";
			}
		}

		string CustomsActivityCodeToEnterpriseActivityCode(string code)
		{
			switch (code)
			{
				case "A":
					return ActivityCodeList.Codes._1;
				case "B":
					return ActivityCodeList.Codes._1a;
				case "C":
					return ActivityCodeList.Codes._1a1;
				case "D":
					return ActivityCodeList.Codes._2;
				case "E":
					return ActivityCodeList.Codes._3;
				case "F":
					return ActivityCodeList.Codes._3a;
				case "G":
					return ActivityCodeList.Codes._3a3;
				case "H":
					return ActivityCodeList.Codes._4;
				case "J":
					return ActivityCodeList.Codes._5;
				case "K":
					return ActivityCodeList.Codes._11;
				case "L":
					return ActivityCodeList.Codes._12;
				case "M":
					return ActivityCodeList.Codes._13;
				case "N":
					return ActivityCodeList.Codes._14;
				case "O":
					return ActivityCodeList.Codes._15;
				case "P":
					return ActivityCodeList.Codes._16;
				case "Q":
					return ActivityCodeList.Codes._17;
				case "R":
					return ActivityCodeList.Codes._18;
				case "S":
					return ActivityCodeList.Codes._19;
				case "T":
					return ActivityCodeList.Codes._20;
				default:
					return "";
			}
		}

		string QueryResultCodeToText(string code)
		{
			switch (code)
			{
				case "0":
					return "No name and address information is on file";
				case "1":
					return "Name and address information is on file with a continuous bond";
				case "2":
					return "Name and address information is on file with no bond";
				case "3":
					return "Importer number voided: if further assistance is required, contact your CBP Client Representative";
				case "4":
					return "Importer number is in inactive status due to no cargo release, entry summary, or electronic invoice transactions were received within the last 18 months.<BR />To reactivate the importer number, provide CBP with the complete importer number, name, and address information through an Importer/Consignee Create/Update transaction.";
				default:
					return "Unknown (" + code + ")";
			}
		}

		void CreateOrgBondDetails(OrgHeader importer, List<BondData> responseDataCollection)
		{
			var bondDataCollection = new CusBondDetailCollection(importer);
			bondDataCollection.Load();

			var bondQuery = new ZQuery(CusBondDetailSchema.PW_BondType, ImporterBondTypeList.Codes.ContinuousBond);
			var bondDatas = bondDataCollection.Find(bondQuery);
			foreach (CusBondDetail bond in bondDatas)
			{
				if (responseDataCollection.Find(x => (x.BondNumber == bond.PW_BondNumber && CustomsActivityCodeToEnterpriseActivityCode(x.ActivityCode) == bond.PW_ActivityCode)) == null)
				{
					bond.PW_BondExpiryDate = bond.PW_BondEffectiveDate;
					someBondsExpired = true;
				}
			}

			foreach (BondData responseData in responseDataCollection)
			{
				bondQuery = new ZQuery(CusBondDetailSchema.PW_ActivityCode, CustomsActivityCodeToEnterpriseActivityCode(responseData.ActivityCode));
				bondQuery.AddToFilter(CusBondDetailSchema.PW_BondAmount, responseData.BondAmount);
				bondQuery.AddToFilter(CusBondDetailSchema.PW_BondEffectiveDate, responseData.BondEffectiveDate);
				bondQuery.AddToFilter(CusBondDetailSchema.PW_BondNumber, responseData.BondNumber);
				bondQuery.AddToFilter(CusBondDetailSchema.PW_BondType, ImporterBondTypeList.Codes.ContinuousBond);
				bondQuery.AddToFilter(CusBondDetailSchema.PW_SuretyCode, responseData.SuretyCode);
				bondQuery.AddToFilter(CusBondDetailSchema.PW_BondFiledPort, responseData.BondFiledPort);

				bondDatas = bondDataCollection.Find(bondQuery);
				CusBondDetail bondData = null;
				if (bondDatas.Length == 0)
				{
					bondData = bondDataCollection.AddNew();
					bondData.PW_ActivityCode = CustomsActivityCodeToEnterpriseActivityCode(responseData.ActivityCode);
					bondData.PW_BondAmount = responseData.BondAmount;
					bondData.PW_BondEffectiveDate = responseData.BondEffectiveDate;
					bondData.PW_BondNumber = responseData.BondNumber;
					bondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
					bondData.PW_SuretyCode = responseData.SuretyCode;
					bondData.PW_BondFiledPort = responseData.BondFiledPort;
				}
				else
				{
					bondData = (CusBondDetail)bondDatas[0];
				}
				bondData.PW_BondExpiryDate = responseData.BondTerminationDate;
				someBondsExpired |= bondData.PW_BondExpiryDate <= ZDate.Today;
				bondData.HasSufficientFund = responseData.BondSufficiencyIndicator;
			}
		}

		const string ContinuousBond = "1";
		public const string BondExpiredAdvice = "Some Continuous Bonds for this importer have expired and Bond Expiry Date has been set.";
	}
}
