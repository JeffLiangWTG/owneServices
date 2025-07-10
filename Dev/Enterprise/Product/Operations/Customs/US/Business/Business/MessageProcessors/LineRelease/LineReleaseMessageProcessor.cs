using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.LineRelease)]
	public class LineReleaseMessageProcessor : ACSABIProcessor
	{
		public new LineReleaseMQEDIMessage Message
		{
			get => (LineReleaseMQEDIMessage)base.Message;
		}

		public override void Process()
		{
			string jobNumber = "Unknown";
			string url = "";
			CusEntryHeader entry = null;
			LRLX10 lrlx10 = GetFirstMessageBlock<LRLX10>();
			if (lrlx10 != null)
			{
				CusEntryHeader.Loader loader = new CusEntryHeader.Loader(Factory);
				entry = loader.FindByEntryNumberAndFilerCode(GlbCompany.CurrentCompany.PK, lrlx10.EntryNumber, lrlx10.EntryFilerCode, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
				if (entry != null)
				{
					url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(entry);
					jobNumber = entry.Declaration.DeclarationReferenceAppendedByFormattedEntryNumber;
					Message.SetToComplete();
				}
				else
				{
					url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(Message);
					jobNumber = CusEntryHeader.GetFormmattedFilerCodeAndEntryNumber(lrlx10.EntryFilerCode, lrlx10.EntryNumber);
				}
			}

			StringBuilder html = new StringBuilder();
			HtmlTableCreator lrlx10htmlcreator = null;
			HtmlTableCreator lrlx20htmlcreator = null;
			HtmlTableCreator lrlx25htmlcreator = null;
			HtmlTableCreator lrlx30htmlcreator = null;
			HtmlTableCreator lrlx40htmlcreator = null;
			bool isFailure = false;
			List<ZString> errorCodes = new List<ZString>();

			foreach (MessageBlock block in messageBlocks)
			{
				LRLX30 lrlx30 = block as LRLX30;
				if (lrlx30 != null)
				{
					if (!errorCodes.Contains(lrlx30.ErrorType))
					{
						errorCodes.Add(lrlx30.ErrorType);
					}
					isFailure |= IsRejectedCode(lrlx30.ErrorType);
					AddLRLX30Values(ref lrlx30htmlcreator, lrlx30);
				}
				else
				{
					LRLX20 lrlx20 = block as LRLX20;
					if (lrlx20 != null)
					{
						AddLRLX20Values(ref lrlx20htmlcreator, lrlx20);
						Message.US_SupplierCode = lrlx20.ManufacturerSupplierCode;
					}
					else
					{
						LRLX25 lrlx25 = block as LRLX25;
						if (lrlx25 != null)
						{
							AddLRLX25Values(ref lrlx25htmlcreator, lrlx25);
							Message.US_BillOfLading = lrlx25.MasterBillNumber;
						}
						else
						{
							lrlx10 = block as LRLX10;
							if (lrlx10 != null)
							{
								Message.EM_ApplicationReference = lrlx10.EntryNumber;
								if (lrlx10htmlcreator != null)
								{
									html.Append(GenerateHtmlAndMakeCreatorNullable(ref lrlx10htmlcreator));
									html.Append(GenerateHtmlAndMakeCreatorNullable(ref lrlx20htmlcreator));
									html.Append(GenerateHtmlAndMakeCreatorNullable(ref lrlx25htmlcreator));
									html.Append(GenerateHtmlAndMakeCreatorNullable(ref lrlx30htmlcreator));
								}

								lrlx10htmlcreator = new HtmlTableCreator(GetColumnTitlesForLRLX10());

								var releaseDateTime = lrlx10.ReleaseDate.AddHours(new ZInt(lrlx10.ReleaseTime.Left(2))).AddMinutes(new ZInt(lrlx10.ReleaseTime.SubstringSafe(2, 2)));
								lrlx10htmlcreator.WriteRow(GetTransactionStatusDescription(lrlx10.TransactionStatus), lrlx10.ImporterNumber, lrlx10.DistrictPortOfEntry, lrlx10.EntryFilerCode, lrlx10.EntryNumber,
									releaseDateTime, lrlx10.FIRMSCode);
								Message.US_ImporterNumber = lrlx10.ImporterNumber;
								Message.US_ReleaseDateTime = releaseDateTime;
								Message.US_PortCode = lrlx10.DistrictPortOfEntry;
							}
							else
							{
								LRLX40 lrlx40 = block as LRLX40;
								if (lrlx40 != null)
								{
									AddLRLX40Values(ref lrlx40htmlcreator, lrlx40);
								}
							}
						}
					}
				}
			}

			html.Append(GenerateHtmlAndMakeCreatorNullable(ref lrlx10htmlcreator));
			html.Append(GenerateHtmlAndMakeCreatorNullable(ref lrlx20htmlcreator));
			html.Append(GenerateHtmlAndMakeCreatorNullable(ref lrlx25htmlcreator));
			html.Append(GenerateHtmlAndMakeCreatorNullable(ref lrlx30htmlcreator));
			html.Append(GenerateHtmlAndMakeCreatorNullable(ref lrlx40htmlcreator));

			GlbBranch branch = entry != null ? entry.Branch : null;

			AutoCreateDeclaration();

			GenerateHtmlEmailAndSendToOriginalOrGroup(url, jobNumber, "Border Line Release", html.ToString(), isFailure, branch, entry);
		}

		void AutoCreateDeclaration()
		{
			var company = GlbCompany.CurrentCompany;
			if (DataRegistry.Business.USCustomsDataRegistry.Instance.AutoCreateDeclarationFromLineRelease.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				var declaration = new LineReleaseDeclarationCreator(Message).CreateANewDeclaration();

				var branchPK = GetMatchingBranchForBorderCargoReleasePorts(company);

				if (!branchPK.IsEmpty)
				{
					declaration.JE_GB = branchPK;
				}

				declaration?.Factory.Save();
			}
		}

		ZGuid GetMatchingBranchForBorderCargoReleasePorts(GlbCompany company)
		{
			foreach (var branch in company.ActiveBranches)
			{
				var portCollection = DataRegistry.Business.USCustomsDataRegistry.Instance.BorderCargoReleasePorts.GetFallBackValueAtAllLevels(company.PK.ToGuid(), branch.PK.ToGuid(), Guid.Empty);
				if (portCollection.ContainsPortCode(Message.US_PortCode))
				{
					return branch.PK;
				}
			}
			return ZGuid.Empty;
		}

		string GenerateHtmlAndMakeCreatorNullable(ref HtmlTableCreator htmlcreator)
		{
			if (htmlcreator != null)
			{
				StringBuilder html = new StringBuilder();
				html.Append(htmlcreator.ToHtml());
				html.Append("<BR />");
				htmlcreator = null;
				return html.ToString();
			}

			return "";
		}

		void AddLRLX30Values(ref HtmlTableCreator htmlcreator, LRLX30 lrlx30)
		{
			if (htmlcreator == null)
			{
				htmlcreator = new HtmlTableCreator(GetColumnTitlesForLRLX30());
			}

			string longDescription = lrlx30.ErrorType + " - " + MessageCalculator.GetLongDescription(lrlx30.ErrorType, lrlx30.ErrorMessage);
			htmlcreator.WriteRow(longDescription);
		}

		IEnumerable<string> GetColumnTitlesForLRLX30()
		{
			return new string[] { "Error Message" };
		}

		void AddLRLX40Values(ref HtmlTableCreator htmlcreator, LRLX40 lrlx40)
		{
			if (htmlcreator == null)
			{
				htmlcreator = new HtmlTableCreator(GetColumnTitlesForLRLX30());
			}

			htmlcreator.WriteRow(lrlx40.TotalEntriesAccepted, lrlx40.TotalEntriesRejected, lrlx40.TotalEntries, lrlx40.TotalErrorRecords);
		}

		void AddLRLX25Values(ref HtmlTableCreator htmlcreator, LRLX25 lrlx25)
		{
			if (htmlcreator == null)
			{
				htmlcreator = new HtmlTableCreator(GetColumnTitlesForLRLX25());
			}

			htmlcreator.WriteRow(lrlx25.MasterBillNumber, lrlx25.HouseBillNumber, lrlx25.SubHouseBillNumber, lrlx25.Quantity, lrlx25.ModeOfTransportationMOTCode);
		}

		IEnumerable<string> GetColumnTitlesForLRLX25()
		{
			return new string[] { "Bill of Lading", "House Bill", "Sub-House Bill", "Quantity", "Transport Mode" };
		}

		void AddLRLX20Values(ref HtmlTableCreator htmlcreator, LRLX20 lrlx20)
		{
			if (htmlcreator == null)
			{
				htmlcreator = new HtmlTableCreator(GetColumnTitlesForLRLX20());
			}

			htmlcreator.WriteRow(lrlx20.TariffNumber1, lrlx20.TariffNumber2, lrlx20.CommonCommodityClassificationCodeC4, lrlx20.CountryOfOrigin1, lrlx20.CountryOfOrigin2,
				lrlx20.CountryOfOrigin3, lrlx20.CountryOfOrigin4, lrlx20.CountryOfOrigin5, lrlx20.Quantity, lrlx20.UnitOfMeasure, lrlx20.ManufacturerSupplierCode);
		}

		IEnumerable<string> GetColumnTitlesForLRLX20()
		{
			return new string[] { "1st Tarriff", "2nd Tariff", "Common Commondity Classification Code", "1st Country Of Origin", "2nd Country Of Origin",
				"3rd Country Of Origin", "4th Country Of Origin", "5th Country Of Origin", "Quantity", "Unit Of Measure", "Manufacturer/Supplier Code" };
		}

		string GetTransactionStatusDescription(string code)
		{
			switch (code)
			{
				case "A":
					return "Accepted";
				case "R":
					return "Rejected";
				case "W":
					return "Accepted with warning";
				default:
					return code;
			}
		}

		IEnumerable<string> GetColumnTitlesForLRLX10()
		{
			return new string[] { "Status", "Importer Of Record", "District/Port Of Entry", "Entry Filer Code", "Entry Number",
				"Release Date", "FIRMS Code" };
		}

		protected override Integration.IRegistryItem GetEmailGroupRegistryItem()
		{
			return USCustomsDataRegistry.Instance.BorderLineReleaseMessagesGroup;
		}
	}
}
