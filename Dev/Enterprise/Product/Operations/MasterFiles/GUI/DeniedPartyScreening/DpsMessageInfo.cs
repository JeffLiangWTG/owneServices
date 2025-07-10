using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.eTail.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public class DpsMessageInfo
	{
		const string DividingLine = "----------------------------------------------------------------------------------------------------------";
		internal protected List<DpsSourceWithParties> SourceBizOs { get; }
		internal List<DpsSourceWithParties> ExtraSourceBizOs { get; }
		protected bool NeedShowMessage => shouldShowMessage || ResynchronizeStatusList.Count > 0;
		protected int ScreenedItemsCount { get; }

		readonly bool shouldShowMessage;

		public DpsMessageInfo(List<DpsSourceWithParties> sourceBizOs, bool shouldShowMessage, int screenedItemsCount)
		{
			SourceBizOs = sourceBizOs;
			this.shouldShowMessage = shouldShowMessage;
			ScreenedItemsCount = screenedItemsCount;
			ExtraSourceBizOs = new List<DpsSourceWithParties>();
		}

		internal protected void AddFinalStatus(ZGuid pk, string status)
		{
			FinalStatusList.Add(new StatusInfo()
			{
				PK = pk,
				Status = status
			});
		}

		internal void RemoveFinalStatus(ZGuid pk)
		{
			var item = FinalStatusList.SingleOrDefault(x => x.PK == pk);
			if (item != null)
			{
				FinalStatusList.Remove(item);
			}
		}

		internal protected void AddResynchronizeStatus(DpsSourceWithParties dpsSourceWithParties, string status)
		{
			if (SourceBizOs.All(u => u.SourceBizO.PK != dpsSourceWithParties.SourceBizO.PK) && ExtraSourceBizOs.All(u => u.SourceBizO.PK != dpsSourceWithParties.SourceBizO.PK))
			{
				ExtraSourceBizOs.Add(dpsSourceWithParties);
			}

			ResynchronizeStatusList.Add(new StatusInfo
			{
				PK = dpsSourceWithParties.SourceBizO.PK,
				Status = status
			});
		}

		internal protected void ShowMessageIfNeeded()
		{
			if (NeedShowMessage)
			{
				var finalSourceBizOsStatus = GetFinalSourceBizOsStatus();
				if (finalSourceBizOsStatus.Any())
				{
					Globals.Message.Show(GetUserScreenMessage(finalSourceBizOsStatus));
				}
			}
		}

		internal protected List<StatusInfo> FinalStatusList { get; } = new List<StatusInfo>();
		internal List<StatusInfo> ResynchronizeStatusList { get; } = new List<StatusInfo>();

		string GetUserScreenMessage(List<(string JobCode, string FinialStatus, MatchedCountInfo CountInfo, ChangeType ChangeType, bool IsJob)> finalSourceBizOsStatus)
		{
			var statusList = new ScreeningStatusesList();
			var message = new StringBuilder();

			if (ScreenedItemsCount == 0)
			{
				message.AppendLine(Res.GetString("93563C41-66FA-4536-BD11-83CB59615410", "No Records Have Been Screened."));
			}
			else
			{
				message.AppendLine(Res.GetString("307BE851-DE98-40D0-A867-941EF978B025", "{0} Record(s) Screened.", ScreenedItemsCount));
			}

			var shouldAppendForceReScreenHint = ScreenedItemsCount == 0;

			foreach (var status in finalSourceBizOsStatus)
			{
				message.AppendLine(DividingLine);

				if (status.ChangeType == ChangeType.ScreeningChange)
				{
					message.AppendLine(Res.GetString("264F9736-AFAE-40D0-ABA6-2DAEA6A8564B", "{0} will be updated to {1} - {2}.", status.JobCode, status.FinialStatus, statusList.GetDescriptionFromCode(status.FinialStatus ?? string.Empty)));
				}

				else if (status.ChangeType == ChangeType.ResynchronizeChange)
				{
					if (shouldShowMessage)
					{
						message.AppendLine(Res.GetString("AF5D0ED1-2555-4C01-AF83-569012044932", "{0} will be updated to {1} - {2} as we have re-assessed the screening status based on the status of all parties on the job.", status.JobCode, status.FinialStatus, statusList.GetDescriptionFromCode(status.FinialStatus ?? string.Empty)));
					}
					else // show cancelled message
					{
						message.AppendLine(Res.GetString("B8A58A31-2886-414F-BD7A-F66AC1EA934B", "You have canceled the screen results, {0} will be updated to {1} - {2} as we have re-assessed the screening status based on the status of all parties on the job.", status.JobCode, status.FinialStatus, statusList.GetDescriptionFromCode(status.FinialStatus ?? string.Empty)));
					}
				}
				else
				{
					if (status.IsJob)
					{
						message.AppendLine(Res.GetString("12F85ED1-56B5-4FA7-B1E8-322958B43601", "{0} will remain as {1} - {2} based on the screening status of all parties on the job. The screen action will not override a processed record.", status.JobCode, status.FinialStatus, statusList.GetDescriptionFromCode(status.FinialStatus ?? string.Empty)));
					}
					else
					{
						message.AppendLine(Res.GetString("954CFAD0-B8F2-4AFA-AAB8-39681FEC62CD", "{0} will remain as {1} - {2}.", status.JobCode, status.FinialStatus, statusList.GetDescriptionFromCode(status.FinialStatus ?? string.Empty)));
					}
				}

				SetReason(message, status.CountInfo, ref shouldAppendForceReScreenHint);
			}

			if (shouldAppendForceReScreenHint)
			{
				message.AppendLine(System.Environment.NewLine + Res.GetString("6B04A7D4-F119-4562-A18F-ECA4234792CF", "To screen records that are CLR or MAT please use the Force Re-Screen option."));
			}

			return message.ToString();
		}

		void SetReason(StringBuilder message, MatchedCountInfo matchedCountInfo, ref bool shouldAppendForceReScreenHint)
		{
			if (matchedCountInfo.NewMatchCounter > 0)
			{
				message.AppendLine(Res.GetString("969F20C7-B3D9-415C-8F85-015351833CD7", "There are {0} new MAT - Matched record(s) present.", matchedCountInfo.NewMatchCounter));
			}

			if (matchedCountInfo.NotScreenedCounter > 0)
			{
				shouldAppendForceReScreenHint = true;
				message.AppendLine(Res.GetString("47C3CB8E-BE80-4ACF-B91D-794F70801605", "There are {0} record(s) already screened.", matchedCountInfo.NotScreenedCounter));
			}

			if (matchedCountInfo.PreMatchCounter > 0)
			{
				message.AppendLine(Res.GetString("ABD941A8-C3CF-49DC-8550-903268B704AB", "There are {0} pre MAT - Matched record(s) present.", matchedCountInfo.PreMatchCounter));
			}

			if (matchedCountInfo.UnknownRecordCounter > 0)
			{
				message.AppendLine(Res.GetString("3C2227D2-6452-47A9-9D5B-3F7803267F98", "There are {0} record(s) has come back to UNK - Unknown.", matchedCountInfo.UnknownRecordCounter));
			}
		}

		List<(string JobCode, string FinialStatus, MatchedCountInfo CountInfo, ChangeType ChangeType, bool IsShipmentOrConsolOrDeclaration)> GetFinalSourceBizOsStatus()
		{
			var sourceBizOsFinalStatus = new List<(string JobCode, string FinialStatus, MatchedCountInfo CountInfo, ChangeType ChangeType, bool IsShipmentOrConsolOrDeclaration)>();

			if (SourceBizOs != null)
			{
				foreach (var sourceBizO in SourceBizOs.Concat(ExtraSourceBizOs))
				{
					var jobCodeAndType = GetJobCodeInfo(sourceBizO.SourceBizO);
					var updatedStatus = FinalStatusList.FirstOrDefault(u => u.PK == sourceBizO.SourceBizO.PK);
					var resynchronizeStatus = ResynchronizeStatusList.FirstOrDefault(u => u.PK == sourceBizO.SourceBizO.PK);

					if (resynchronizeStatus != null && updatedStatus == null)
					{
						sourceBizOsFinalStatus.Add((jobCodeAndType.JobCode, resynchronizeStatus.Status, GetMatchedCountInfo(sourceBizO.ScreenParties), ChangeType.ResynchronizeChange, jobCodeAndType.IsJob));
					}
					else if (updatedStatus == null)
					{
						if (StatusNeedToRecord((sourceBizO.SourceBizO as IScreeningStatusProvider)?.ScreeningStatus))
						{
							sourceBizOsFinalStatus.Add((jobCodeAndType.JobCode, (sourceBizO.SourceBizO as IScreeningStatusProvider)?.ScreeningStatus, GetMatchedCountInfo(sourceBizO.ScreenParties), ChangeType.NotChange, jobCodeAndType.IsJob));
						}
					}
					else if (StatusNeedToRecord(updatedStatus.Status))
					{
						sourceBizOsFinalStatus.Add((jobCodeAndType.JobCode, updatedStatus.Status, GetMatchedCountInfo(sourceBizO.ScreenParties), ChangeType.ScreeningChange, jobCodeAndType.IsJob));
					}
				}
			}

			return sourceBizOsFinalStatus;
		}

		bool StatusNeedToRecord(string status)
		{
			return !string.IsNullOrEmpty(status) && status != ScreeningStatusesList.Codes.Clear && status != ScreeningStatusesList.Codes.PermanentClear;
		}

		MatchedCountInfo GetMatchedCountInfo(IEnumerable<ScreeningParty> screenParties)
		{
			var countInfo = new MatchedCountInfo();

			if (screenParties != null)
			{
				foreach (var party in screenParties.Where(u => !u.Key.IsEmpty).Select(u => (u.Key, u.CurrentScreeningStatus)).Distinct())
				{
					var finalStatus = FinalStatusList.FirstOrDefault(u => u.PK == party.Key);
					if (finalStatus == null)
					{
						countInfo.NotScreenedCounter++;

						if (party.CurrentScreeningStatus == ScreeningStatusesList.Codes.Matched)
						{
							countInfo.PreMatchCounter++;
						}
					}
					else
					{
						switch (finalStatus.Status)
						{
							case ScreeningStatusesList.Codes.Matched:
								countInfo.NewMatchCounter++;
								break;
							case ScreeningStatusesList.Codes.Unknown:
								countInfo.UnknownRecordCounter++;
								break;
						}
					}
				}
			}

			return countInfo;
		}

		internal protected static (string JobCode, bool IsJob) GetJobCodeInfo(BusinessObject bizO)
		{
			var jobCode = string.Empty;
			var isJob = false;

			switch (bizO)
			{
				case IOrgHeader orgHeader:
					jobCode = orgHeader.OH_Code;
					break;
				case IRefVessel refVessel:
					jobCode = refVessel.RV_Code;
					break;
				case Forwarding.IForwardingShipment shipment:
					jobCode = shipment.JS_UniqueConsignRef;
					isJob = true;
					break;
				case Forwarding.IForwardingConsol consol:
					jobCode = consol.JK_UniqueConsignRef;
					isJob = true;
					break;
				case Customs.IBaseJobDeclaration jobDeclaration:
					jobCode = jobDeclaration.JE_JS.IsEmpty ? jobDeclaration.JE_DeclarationReference.ToString() : Res.GetString("AEE7001B-8BE7-4809-B503-B814E46C0386", "Declaration {0}", jobDeclaration.JE_DeclarationReference);
					isJob = true;
					break;
				case IWhsDocket whsDocket:
					jobCode = whsDocket.WD_DocketID;
					isJob = true;
					break;
				case JobDocAddress _:
					jobCode = Res.GetString("F2C30CDD-0787-4A80-A451-C036A0B7BFDC", "Doc Address");
					break;
				case IHVLVBookingHeader hvlvBookingHeader:
					jobCode = hvlvBookingHeader.HVH_BookingReference;
					break;
			}

			return (jobCode, isJob);
		}

		class MatchedCountInfo
		{
			public int NotScreenedCounter { get; set; }
			public int UnknownRecordCounter { get; set; }
			public int NewMatchCounter { get; set; }
			public int PreMatchCounter { get; set; }
		}

		public class StatusInfo
		{
			public ZGuid PK { get; set; }
			public string Status { get; set; }
		}

		enum ChangeType
		{
			NotChange,
			ScreeningChange,
			ResynchronizeChange
		}
	}
}
