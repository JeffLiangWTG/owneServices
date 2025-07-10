using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.SailingDataVendor.Business;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MessageProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using Res = Enterprise.Freight.SailingScheduleDataVendor.Res;

[assembly: MessageFilter("MAP", MailDBItemsSchema.Constants.TableName, typeof(OneStopContainerInformationProcessor))]

namespace Enterprise.Freight.SailingDataVendor.Business
{
	internal class OneStopContainerInformationProcessor : IProcessor
	{
		#region Process

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			for (int i = 3; i >= 0; i--)
			{
				token.ThrowIfCancellationRequested();
				try
				{
					TryProcess(notifications);
				}
				catch (ZSaveConcurrencyException)
				{
					if (i == 0)
					{
						throw;
					}
					Thread.Sleep(2000);
				}
			}
		}

#if DEBUG
		protected virtual
#endif
		void TryProcess(INotifications notifications)
		{
			FilteredBusinessObjectReader reader = new FilteredBusinessObjectReader(GetMailItemQuery(), typeof(MailItem));
			reader.BatchSize = 10;
			reader.SaveBeforeLoadNextEnabled = true;

			foreach (MailItem mailItem in reader)
			{
				if (mailItem.MI_Subject.StartsWith(OneStopConstants.ContainerEventsExpiredResponseEmailSubjectPrefix))
				{
					mailItem.MI_Status = MailStatus.Processed;
				}
				else if (mailItem.MI_Subject.StartsWith(OneStopConstants.ContainerEventResponseEmailSubjectPrefix))
				{
					bool result = ProcessResponseMailItem(mailItem, notifications);
					mailItem.MI_Status = result ? MailStatus.Processed : MailStatus.Failed;
				}
			}
			reader.Factory.Save();
		}

		ZQuery GetMailItemQuery()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(MailDBItemsSchema.MI_Subject, SQLComparisonOperator.StartsWith, "1-STOP ");
			query.AddToFilter(MailDBItemsSchema.MI_From, SQLComparisonOperator.Contains, OneStopConstants.ContainerEventResponseEmailAddress);
			query.AddToFilter(MailDBItemsSchema.MI_Status, MailStatus.Queued);
			query.AddToFilter(MailDBItemsSchema.MI_Direction, MailDirection.Receive);
			return query;
		}

		#endregion

		#region Message Filters

		[MessageFilterCondition(MailDBItemsSchema.Constants.MI_Subject, "^" + OneStopConstants.ContainerEventsExpiredResponseEmailSubjectPrefix)]
		[MessageFilterCondition(MailDBItemsSchema.Constants.MI_From, OneStopConstants.ContainerEventResponseEmailAddress)]
		public bool ProcessContainerEventsExpiredResponseEmail()
		{
			return true;
		}

		[MessageFilterCondition(MailDBItemsSchema.Constants.MI_Subject, "^" + OneStopConstants.ContainerEventResponseEmailSubjectPrefix)]
		[MessageFilterCondition(MailDBItemsSchema.Constants.MI_From, OneStopConstants.ContainerEventResponseEmailAddress)]
		public bool ProcessContainerEventResponseEmail(ZGuid mailItemPK, ILogger logger)
		{
			for (int i = 3; i >= 0; i--)
			{
				try
				{
					BusinessObjectFactory factory = new BusinessObjectFactory();
					MailItem mailItem = factory.Load<MailItem>(mailItemPK);
					if (mailItem != null)
					{
						bool result = ProcessResponseMailItem(mailItem, logger.GetTaskNotificationSubscriber());
						return result;
					}
				}
				catch (ZSaveConcurrencyException)
				{
					if (i == 0)
					{
						throw;
					}
					Thread.Sleep(1000);
				}
			}

			return false;
		}

		#endregion

		#region Process Response

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		bool ProcessResponseMailItem(MailItem mailItem, INotifications notifications)
		{
			Dictionary<ZString, ZString> fields = ParseEmailBody(mailItem);
			if (fields.Count >= 7)
			{
				string oneStopLocationCode = TryGetValue(fields, (NoResString)"Event Location");
				string oneStopContainerNumber = TryGetValue(fields, "Vessel/Container");
				string oneStopEventType = TryGetValue(fields, (NoResString)"Event Type");
				ZGuid oneStopContainerPK = ParseReferenceGuid(TryGetValue(fields, (NoResString)"Information"));
				string oneStopVesselName = TryGetValue(fields, (NoResString)"Vessel Name");
				string oneStopLloyds = TryGetValue(fields, (NoResString)"Lloyds No");
				string oneStopVoyageNumber = TryGetValue(fields, (NoResString)"Voyage Number");
				string oneStopEtdAndLoadPort = TryGetValue(fields, (NoResString)"ETD from Load Port");
				string oneStopEtaAndDischargePort = TryGetValue(fields, (NoResString)"ETA at Discharge Port");
				if (string.IsNullOrEmpty(oneStopEtaAndDischargePort))
				{
					oneStopEtaAndDischargePort = TryGetValue(fields, (NoResString)"ETA at Disharge Port");
				}

				var dateAsString = TryGetValue(fields, "Event Date") + " " + TryGetValue(fields, "Event Time");
				if (!ZDateTime.TryParseExact(dateAsString, out var oneStopEventDate, (NoResString)"yyyy/MM/dd HH:mm tt") || !oneStopEventDate.IsValidSmallDateTime)
				{
					notifications.Notify(new ErrorNotification(ErrorType.DataOutOfRangeError, Res.GetString("B2749835-92F6-410A-8754-9E838515AA00", "Invalid DateTime, 1-stop message: '{0}'", mailItem.MI_Body)));
				}
				else if (oneStopLocationCode.Length > 10)
				{
					CommonContainer container = mailItem.Factory.Load<CommonContainer>(oneStopContainerPK);
					NotifyUserOfLocationFormattedByDescriptionOnce(container);
				}
				else
				{
					var mostInterestingBranch = GetMostInterestingBranch(mailItem.Factory, oneStopLocationCode);
					if (mostInterestingBranch != null)
					{
						using (DisposableEnvironment.ForBranch(mostInterestingBranch.PK.ToGuid()))
						{
							UpdateContainer(mailItem, notifications,
								oneStopEventType,
								oneStopLocationCode,
								oneStopEventDate,
								oneStopContainerPK,
								oneStopContainerNumber,
								oneStopVesselName,
								oneStopLloyds,
								oneStopVoyageNumber,
								oneStopEtdAndLoadPort,
								oneStopEtaAndDischargePort);
							mailItem.Factory.Save();
						}
					}
					else
					{
						notifications.Notify(new ErrorNotification(ErrorType.Warning, Res.GetString("D774983F-92F6-410A-8754-9E838515AA00", "Unable to obtain context branch, 1-stop message: '{0}'", mailItem.MI_Body)));
					}
				}
			}
			else
			{
				notifications.Notify(new ErrorNotification(ErrorType.NotEnoughColumns, Res.GetString("9c4575aa-39db-497c-96cc-9926cd0cb318", "Unexpected field count processing 1-stop message: '{0}'", mailItem.MI_Body)));
				NotifyUserOfWrongFieldCountOnce();
				ErrorReporter.ReportOnce("{DD3B666D-A2E9-491c-AEB6-E1B834F1A2E0}", "Unexpected field count; MailBodyText: " + mailItem.MI_Body);

				return false;
			}

			return true;
		}

		GlbBranch GetMostInterestingBranch(BusinessObjectFactory factory, string oneStopLocationCode)
		{
			var closestPort = GetMatchOneStopOrganisation(factory, oneStopLocationCode)?.ClosestPort;

			if (closestPort != null)
			{
				var closestBranch = GetClosestBranch(factory, closestPort);
				if (closestBranch != null)
				{
					return closestBranch;
				}
			}

			return factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_RN_NKCountryCode, "AU")) ?? factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_RN_NKCountryCode, "NZ"));
		}

		GlbBranch GetClosestBranch(BusinessObjectFactory factory, RefUNLOCO unloco)
		{
			var additionalPorts = factory.Load<GlbBranchExtraPorts>(new ZQuery(GlbBranchExtraPortsSchema.GY_RL_NKAdditionalBranchRelatedPort, unloco.Code));
			var glbBranchQuery = new ZQuery(GlbBranchSchema.GB_IsActive, true);
			var glbBranchCountryQuery = new ZQuery(GlbBranchSchema.GB_RL_NKHomePort, unloco.Code);

			if (additionalPorts.Any())
			{
				glbBranchCountryQuery.AddToFilter(JoinCondition.Or, GlbBranchSchema.PK, Array.ConvertAll(additionalPorts, e => e.GY_GB));
			}
			glbBranchQuery.AddToFilter(glbBranchCountryQuery);

			return factory.LoadTop1<GlbBranch>(glbBranchQuery);
		}

		OrgHeader GetMatchOneStopOrganisation(BusinessObjectFactory factory, string oneStopLocationCode)
		{
			var cusCodeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.OneStopCode);
			cusCodeFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, oneStopLocationCode);

			var customsCodeNumber = factory.LoadTop1<OrgCusCode>(cusCodeFilter);

			if (customsCodeNumber != null)
			{
				return factory.Load<OrgHeader>(customsCodeNumber.OK_OH);
			}
			return null;
		}

		void NotifyUserOfWrongFieldCountOnce()
		{
			if (!userNotifiedOfWrongFieldCount)
			{
				userNotifiedOfWrongFieldCount = true;
				var emailSender = new HtmlNotificationEmailSender();
				try
				{
					var email = emailSender.CreateEmail(Res.GetString("ab1fc752-dc37-4862-9632-9dc2d3aad5ae", @"1-Stop Container Event Alert Service unexpected field count."),
						Res.GetString("e3edb78d-20a5-48df-ad72-d9f29c2f8079", @"One or more incoming ComTrac Container event messages from 1-Stop could not be processed because they were received in an unexpected format. 
Please check your email POP3 configuration, or contact 1-Stop and ask them to investigate their ""Alert Service Notification"" emails."));
					email = AddRecipientsAndSubscriptions(email, null);
					Env.OutgoingMailManager.CreateAndSave(email);
				}
				catch (EmailSendFailedException) { }
			}
		}

		bool userNotifiedOfWrongFieldCount;

		string TryGetValue(IDictionary<ZString, ZString> dictionary, ZString fieldName)
		{
			ZString result;
			dictionary.TryGetValue(fieldName, out result);
			return result;
		}

		void NotifyUserOfLocationFormattedByDescriptionOnce(CommonContainer container)
		{
			if (!userNotifiedOfLocationFormattedByDescription)
			{
				userNotifiedOfLocationFormattedByDescription = true;
				HtmlNotificationEmailSender emailSender = new HtmlNotificationEmailSender();
				try
				{
					EmailDef email = emailSender.CreateEmail(Res.GetString("71d2a06f-47b8-41f4-aba7-cad2eb63c7a4", "1-Stop Container Event Alert Service configuration problem"), WrongOneStopLocationFormatEmailHtml);
					email = AddRecipientsAndSubscriptions(email, container);
					Env.OutgoingMailManager.CreateAndSave(email);
				}
				catch (EmailSendFailedException) { }
			}
		}

		bool userNotifiedOfLocationFormattedByDescription;

		static string WrongOneStopLocationFormatEmailHtml
		{
			get { return "  " + Res.GetString("48e6575b-cc49-46cc-8ffd-145e8d70689b", "<br />  One or more 1-Stop locations were received that have more than 10 characters.<br />  This indicates 1-Stop location codes are incorrectly configured to be sent<br />  by description instead of by code.<br />  <br />  Please contact 1-Stop and ask them to reconfigure your 1-Stop Alert Service<br />  account to send location codes instead of location descriptions in <br />  Alert Service notification emails.<br />  <br />") + "  "; }
		}

		void UpdateContainer(
			MailItem mailItem,
			INotifications notifications,
			ZString oneStopEventType,
			ZString oneStopLocationCode,
			ZDateTime oneStopEventDate,
			ZGuid oneStopContainerPK,
			ZString oneStopContainerNumber,
			ZString oneStopVesselName,
			ZString oneStopLloyds,
			ZString oneStopVoyageNumber,
			string oneStopEtdAndLoadPort,
			string oneStopEtaAndDischargePort)
		{
			CommonContainer container = mailItem.Factory.Load<CommonContainer>(oneStopContainerPK);
			if (container != null && oneStopEventDate.IsValid && container.JC_ContainerNum == oneStopContainerNumber)
			{
				if (VesselVoyageMatchOrEmpty(container, oneStopVesselName, oneStopLloyds, oneStopVoyageNumber))
				{
					notifications.Notify(new InfoNotification(Res.GetString("6ea58b9e-3fbc-453b-b26a-8a5e10779d63", "Processing event '{0}' for container '{1}'", oneStopEventType, container.JC_ContainerNum)));
					UpdateContainer(container, oneStopEventType, oneStopLocationCode, oneStopEtdAndLoadPort, oneStopEtaAndDischargePort, oneStopEventDate, oneStopVesselName, oneStopLloyds, oneStopVoyageNumber);
				}
				else
				{
					NotifyDifferentVessel(mailItem, container);
				}
			}
		}

		void NotifyDifferentVessel(MailItem mailItem, CommonContainer container)
		{
			if (!userNotifiedOfDifferentVessel)
			{
				userNotifiedOfDifferentVessel = true;

				HtmlNotificationEmailSender emailSender = new HtmlNotificationEmailSender();
				try
				{
					string body = DifferentVesselEmailHtml;

					string containerDetails = container.JC_ContainerNum;
					if (container.RefContainer != null)
					{
						containerDetails += " (" + container.RefContainer.RC_Code + ")";
					}

					body = body.Replace("(*ProductName*)", Core.Constants.ProductName);
					body = body.Replace("(*ContainerNumber*)", containerDetails);
					body = body.Replace("(*JobNumber*)", GetParentID(container));

					RefVessel existingVessel = GetMostInterestingVessel(container);
					body = body.Replace("(*ExistingVessel*)", existingVessel != null ? existingVessel.RV_Name : ZString.Empty);
					body = body.Replace("(*ExistingVoyage*)", GetMostInterestingVoyage(container));
					body = body.Replace("(*ExistingLloyds*)", existingVessel != null ? existingVessel.RV_LloydsNumber : ZString.Empty);
					body = body.Replace("(*ExistingLoad*)", GetLoadPort(container));
					body = body.Replace("(*ExistingDischarge*)", GetDischargePort(container));
					body = body.Replace("(*1StopEmailBody*)", mailItem.MI_Body.Replace("\r\n", "\n").Replace("\n", "<br />" + System.Environment.NewLine));

					OrgHeader consignee = null;
					OrgHeader consignor = null;
					if (container.PackLines.Count > 0)
					{
						consignee = container.PackLines[0].CalcConsignee;
						consignor = container.PackLines[0].CalcConsignor;
						foreach (PackLine packLine in container.PackLines)
						{
							if (consignee != packLine.CalcConsignee ||
								consignor != packLine.CalcConsignor)
							{
								consignee = null;
								consignor = null;
								break;
							}
						}
					}

					SetReplaceableTextForOrganization(Res.GetString("49c1a75a-d89e-4c76-b258-9c156f246480", "Consignee"), consignee, ref body);
					SetReplaceableTextForOrganization((NoResString)"Consignor", consignor, ref body);

					EmailDef email = emailSender.CreateEmail(Res.GetString("a3873027-d481-4d6b-961b-ae3469ea0fa2", "1-Stop Vessel Difference"), body);
					email = AddRecipientsAndSubscriptions(email, container);
					Env.OutgoingMailManager.CreateAndSave(email);
				}
				catch (EmailSendFailedException) { }
			}
		}

		void SetReplaceableTextForOrganization(string fieldName, OrgHeader org, ref string text)
		{
			if (org == null)
			{
				text = text.Replace("(*" + fieldName + "*)", "Multiple");
			}
			else
			{
				string description = org.OH_Code + " - " + org.OH_FullNameTruncated;
				if (org.Branch != null)
				{
					description += " - " + org.Branch.GB_Code;
				}
				text = text.Replace("(*" + fieldName + "*)", description);
			}
		}

		bool userNotifiedOfDifferentVessel;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		const string DifferentVesselEmailHtml = @"
<br />
Incoming 1-Stop alert for Container (*ContainerNumber*) is not matching the details found in CargoWise<i>One</i> for this Container. Due to Vessel/Lloyds/Voyage information a discrepancy has been recognised and this event has not been processed.<br /><br />

<b>edi<i>(*ProductName*)</i> Details:</b><br />
<br />
Container Job#: (*JobNumber*)<br />
Consignee: (*Consignee*)<br />
Consignor: (*Consignor*)<br />
Vessel Name: (*ExistingVessel*)<br />
Lloyds No: (*ExistingLloyds*)<br />
Voyage Number: (*ExistingVoyage*)<br />
Load: (*ExistingLoad*)<br />
Discharge: (*ExistingDischarge*)<br />

<br /><br />

<b>1-Stop Message:</b><br />
<br />
(*1StopEmailBody*)<br />";

		void UpdateContainer(CommonContainer container, ZString oneStopEventType, ZString oneStopLocationCode, string oneStopEtdAndLoadPort, string oneStopEtaAndDischargePort, ZDateTime oneStopEventDate, ZString oneStopVesselName, ZString oneStopLloyds, ZString oneStopVoyage)
		{
			ZPropertyInfo containerDateProperty = GetDateProperty(container, oneStopLocationCode, oneStopEventType);
			CreateOneStopCodeNotRegisteredEmailIfRequired(container, oneStopEventType, oneStopLocationCode);

			if (containerDateProperty != null)
			{
				containerDateProperty.Value = oneStopEventDate;
			}
			else if (oneStopEventType == OneStopConstants.ContainerEventTypes.ImportPreAdvised || oneStopEventType == OneStopConstants.ContainerEventTypes.ExportPreAdvised)
			{
				if (!string.IsNullOrEmpty(oneStopEtdAndLoadPort))
				{
					CreateEvent(container, oneStopEventType, oneStopEventDate, oneStopVesselName, oneStopLloyds, oneStopVoyage, oneStopEtdAndLoadPort, (NoResString)"Load");
				}

				if (!string.IsNullOrEmpty(oneStopEtaAndDischargePort))
				{
					CreateEvent(container, oneStopEventType, oneStopEventDate, oneStopVesselName, oneStopLloyds, oneStopVoyage, oneStopEtaAndDischargePort, (NoResString)"Discharge");
				}
			}
		}

		void CreateEvent(CommonContainer container, ZString oneStopEventType, ZDateTime oneStopEventDate, ZString oneStopVesselName, ZString oneStopLloyds, ZString oneStopVoyage, string dateAndLocation, ZString loadOrDischarge)
		{
			var dateAndLocationArray = dateAndLocation.Split(new string[] { (NoResString)" from ", (NoResString)" to " }, StringSplitOptions.RemoveEmptyEntries);
			var date = dateAndLocationArray.Length > 0 ? dateAndLocationArray[0] : string.Empty;
			var location = dateAndLocationArray.Length > 1 ? dateAndLocationArray[1] : string.Empty;
			var type = (oneStopEventType == OneStopConstants.ContainerEventTypes.ImportPreAdvised) ? Constants.EventReferenceParameterTypes.ImportPreAdvice : Constants.EventReferenceParameterTypes.ExportPreAdvice;
			var slReference = string.Format((NoResString)"Lloyds:{0},Vessel:{1},Voyage:{2},{3}:{4}|DEP = 1-stop|TYP={5}|LOC={6}", oneStopLloyds, oneStopVesselName, oneStopVoyage, loadOrDischarge, date, type, location); // No need to translate English for 1stop clients yet

			if (!container.Logs.GetAllLogs().Any(log => ((StmALog)log).SL_Reference == slReference))
			{
				container.Logs.AddNew(Events.StatusUpdated, slReference, oneStopEventDate.ToOffset());
			}
		}

		Dictionary<ZString, ZString> ParseEmailBody(MailItem mailItem)
		{
			Dictionary<ZString, ZString> result = new Dictionary<ZString, ZString>();
			foreach (string row in mailItem.MI_Body.Split('\n'))
			{
				int i = row.IndexOf(":");
				if (i != -1)
				{
					string key = row.Substring(0, i).Trim();
					if (!string.IsNullOrEmpty(key))
					{
						key = key.Replace("=09", "").Trim();
						if (!result.ContainsKey(key))
						{
							result.Add(key, row.Substring(i + 1).Trim());
						}
					}
				}
			}
			return result;
		}

		bool VesselVoyageMatchOrEmpty(CommonContainer container, ZString oneStopVesselName, ZString oneStopLloyds, ZString oneStopVoyage)
		{
			if (oneStopVesselName.IsEmpty && oneStopLloyds.IsEmpty && oneStopVoyage.IsEmpty)
			{
				return true;
			}
			return VesselMatchOrEmpty(container, oneStopVesselName, oneStopLloyds) && VoyageMatchOrEmpty(container, oneStopVoyage);
		}

		bool VesselMatchOrEmpty(CommonContainer container, ZString oneStopVessel, ZString oneStopLloyds)
		{
			return
				oneStopVessel.IsEmpty && oneStopLloyds.IsEmpty ||
				GetVessels(container).Any(existingVessel => existingVessel.RV_Name.Trim().IsEmpty && existingVessel.RV_LloydsNumber.IsEmpty ||
															existingVessel.RV_Name.Trim().EqualsIgnoringCase(oneStopVessel) && !oneStopVessel.IsEmpty ||
															existingVessel.RV_LloydsNumber.Trim().EqualsIgnoringCase(oneStopLloyds) && !oneStopLloyds.IsEmpty);
		}

		bool VoyageMatchOrEmpty(CommonContainer container, ZString oneStopVoyage)
		{
			return
				oneStopVoyage.IsEmpty ||
				GetVoyages(container).Any(existingVoyage => existingVoyage.IsEmpty || existingVoyage.EqualsIgnoringCase(oneStopVoyage));
		}

		IEnumerable<RefVessel> GetVessels(CommonContainer container)
		{
			if (container.Consol == null)
			{
				RefVessel vessel = GetMostInterestingVessel(container);
				return vessel != null ? new[] { vessel } : Array.Empty<RefVessel>();
			}

			return from Transport transport in container.Consol.Transports where transport.Vessel != null select transport.Vessel;
		}

		IEnumerable<ZString> GetVoyages(CommonContainer container)
		{
			return container.Consol == null
					? new[] { GetMostInterestingVoyage(container).Trim() }
					: from Transport transport in container.Consol.Transports select transport.JW_VoyageFlight.Trim();
		}

		ZGuid ParseReferenceGuid(string guidAsString)
		{
			ZGuid result = ZGuid.Invalid;
			if (guidAsString.Length == 32)
			{
				guidAsString = guidAsString.Insert(8, "-");
				guidAsString = guidAsString.Insert(13, "-");
				guidAsString = guidAsString.Insert(18, "-");
				guidAsString = guidAsString.Insert(23, "-");
				result = new ZGuid(guidAsString);
			}
			return result;
		}

		RefVessel GetMostInterestingVessel(CommonContainer container)
		{
			ZString vesselNK = container.Consol != null ? container.Consol.JK_JX_JV_NKVessel : GetDeclarationPropertyValue(container, JobDeclarationSchema.JE_VesselName);
			return RefVessel.LookupVesselByFK(vesselNK, container.Factory);
		}

		ZString GetMostInterestingVoyage(CommonContainer container)
		{
			return container.Consol != null ? container.Consol.JK_JX_JV_VoyageFlight : GetDeclarationPropertyValue(container, JobDeclarationSchema.JE_VoyageFlightNo);
		}

		ZString GetLoadPort(CommonContainer container)
		{
			return container.Consol != null ? container.Consol.JK_RL_NKLoadPort : GetDeclarationPropertyValue(container, JobDeclarationSchema.JE_RL_NKOrigin);
		}

		ZString GetDischargePort(CommonContainer container)
		{
			return container.Consol != null ? container.Consol.JK_RL_NKDischargePort : GetDeclarationPropertyValue(container, JobDeclarationSchema.JE_RL_NKFinalDestination);
		}

		ZString GetParentID(CommonContainer container)
		{
			return container.Consol != null ? container.Consol.JK_UniqueConsignRef : GetDeclarationPropertyValue(container, JobDeclarationSchema.JE_DeclarationReference);
		}

		ZString GetDeclarationPropertyValue(CommonContainer container, SchemaColumn declarationProperty)
		{
			ZQuery customsContainerQuery = new ZQuery(CusContainerSchema.CO_JC, container.PK);
			BusinessObject customsContainer = (BusinessObject)container.Factory.LoadTop1<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(customsContainerQuery);
			BusinessObject declaration = customsContainer != null ? (BusinessObject)customsContainer["Declaration"] : null;
			return declaration == null ? ZString.Empty : (ZString)declaration[declarationProperty];
		}

		ZPropertyInfo GetDateProperty(CommonContainer container, string oneStopLocationCode, string oneStopEventType)
		{
			ZPropertyInfo result = null;
			switch (oneStopEventType)
			{
				case OneStopConstants.ContainerEventTypes.Dehire:
					result = container.JC_ContainerYardEmptyReturnGateInInfo;
					break;

				case OneStopConstants.ContainerEventTypes.GateIn:
					if (oneStopEventType == OneStopConstants.ContainerEventTypes.GateIn && IsContainerTerminal(container.Factory, oneStopLocationCode))
					{
						result = container.JC_FCLWharfGateInInfo;
					}
					else if (IsContainerYard(container.Factory, oneStopLocationCode))
					{
						result = container.JC_ContainerYardEmptyReturnGateInInfo;
					}
					break;

				case OneStopConstants.ContainerEventTypes.GateOut:
					if (IsContainerTerminal(container.Factory, oneStopLocationCode))
					{
						result = container.JC_FCLWharfGateOutInfo;
					}
					else if (IsContainerYard(container.Factory, oneStopLocationCode))
					{
						result = container.JC_ContainerYardEmptyPickupGateOutInfo;
					}
					break;

				case OneStopConstants.ContainerEventTypes.DischargeOffVessel:
					result = container.JC_FCLUnloadFromVesselInfo;
					break;

				case OneStopConstants.ContainerEventTypes.LoadOnVessel:
					result = container.JC_FCLOnBoardVesselInfo;
					break;

				case OneStopConstants.ContainerEventTypes.StorageStart:
					result = container.JC_ArrivalCTOStorageStartDateInfo;
					break;

				case OneStopConstants.ContainerEventTypes.ImportAvailable:
					result = container.JC_FCLAvailableInfo;
					break;

				case OneStopConstants.ContainerEventTypes.ImportPreAdvised:
				case OneStopConstants.ContainerEventTypes.ExportPreAdvised:
					break;

				default:
					ErrorReporter.ReportOnce("{E9371115-EE9A-4e46-9B54-0E55CD8F7DAC}", "Unknown 1-Stop container date type '" + oneStopEventType + "'");
					break;
			}

			return result;
		}

		bool IsContainerTerminal(BusinessObjectFactory factory, ZString locationCode)
		{
			OrgHeader oneStopOrganisation = LoadOrganisationFromOneStopCode(factory, locationCode);
			RefPremisesGateCode premiseCode = LoadPremiseGateCode(factory, locationCode);
			return
				(oneStopOrganisation != null && oneStopOrganisation.OH_IsSeaCTO) ||
				(premiseCode != null && premiseCode.R5_IsWharf);
		}

		bool IsContainerYard(BusinessObjectFactory factory, ZString locationCode)
		{
			OrgHeader oneStopCode = LoadOrganisationFromOneStopCode(factory, locationCode);
			RefPremisesGateCode premiseCode = LoadPremiseGateCode(factory, locationCode);
			return
				(oneStopCode != null && oneStopCode.OH_IsContainerYard) ||
				(premiseCode != null && premiseCode.R5_IsContainerYard);
		}

		OrgHeader LoadOrganisationFromOneStopCode(BusinessObjectFactory factory, ZString oneStopCode)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.OneStopCode);
			query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, oneStopCode);
			OrgCusCode code = factory.LoadTop1<OrgCusCode>(query);
			return (code == null) ? null : code.Header;
		}

		RefPremisesGateCode LoadPremiseGateCode(BusinessObjectFactory factory, ZString oneStopCode)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(RefPremisesGateCodeSchema.R5_DataProvider, PremiseGateCodeDataProviderList.Codes.OneStop);
			query.AddToFilter(RefPremisesGateCodeSchema.R5_PremisesGateCode, oneStopCode);
			return factory.LoadTop1<RefPremisesGateCode>(query);
		}

		#endregion

		#region 1-Stop Code Not Exists Email

		void CreateOneStopCodeNotRegisteredEmailIfRequired(CommonContainer container, ZString eventType, ZString locationCode)
		{
			if ((eventType == OneStopConstants.ContainerEventTypes.GateIn || eventType == OneStopConstants.ContainerEventTypes.GateOut)
				&& !oneStopUnregisteredCodesNotified.Contains(locationCode)) // to prevent excessive spam
			{
				OrgHeader organisation = LoadOrganisationFromOneStopCode(container.Factory, locationCode);

				if (organisation == null)
				{
					EmailDef email = CreateOneStopCodeNotRegisteredEmail(container, eventType, locationCode);
					try
					{
						Env.OutgoingMailManager.CreateAndSave(email);
					}
					catch (EmailSendFailedException) { }

					oneStopUnregisteredCodesNotified.Add(locationCode);
				}
			}
		}

		readonly List<string> oneStopUnregisteredCodesNotified = new List<string>();

		EmailDef CreateOneStopCodeNotRegisteredEmail(CommonContainer container, ZString eventType, ZString locationCode)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(RefPremisesGateCodeSchema.R5_DataProvider, PremiseGateCodeDataProviderList.Codes.OneStop);
			query.AddToFilter(RefPremisesGateCodeSchema.R5_PremisesGateCode, locationCode);
			RefPremisesGateCode premise = container.Factory.LoadTop1<RefPremisesGateCode>(query);

			HtmlNotificationEmailSender emailSender = new HtmlNotificationEmailSender();
			string body = OneStopLocationCodeNotRegisteredHtmlBody;
			body = body.Replace("(*ContainerEventType*)", eventType);
			body = body.Replace("(*OneStopLocationCode*)", locationCode);
			body = body.Replace("(*OneStopLocationDescription*)", (premise == null) ? "" : premise.R5_PremisesGateDescription + " ");
			body = body.Replace("(*ContainerNumber*)", container.JC_ContainerNum);

			EmailDef email = emailSender.CreateEmail(Res.GetString("9f5164d7-e3c3-4f25-bbb7-82ae0c961ccf", "{0} Unregistered 1-Stop Location Notification", eventType), body);
			email = AddRecipientsAndSubscriptions(email, container);
			return email;
		}

		string OneStopLocationCodeNotRegisteredHtmlBody
		{
			get
			{
				if (oneStopLocationCodeNotRegisteredHtmlBody == null)
				{
					Stream stream = typeof(OneStopContainerInformationProcessor).Assembly.GetManifestResourceStream("Enterprise.Freight.SailingScheduleDataVendor.AU_NZ.ContainerEvents.OneStopLocationCodeNotRegistered.htm");
					oneStopLocationCodeNotRegisteredHtmlBody = new StreamReader(stream).ReadToEnd();
				}
				return oneStopLocationCodeNotRegisteredHtmlBody;
			}
		}
		string oneStopLocationCodeNotRegisteredHtmlBody;

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a file name")]
		EmailDef AddRecipientsAndSubscriptions(EmailDef email, CommonContainer container)
		{
			bool foundRecipients = false;

			if (container != null)
			{
				foreach (var interchange in container.ComTracMessages.Cast<ComTracMessage>().Select(x => x.Interchange).Distinct())
				{
					if (interchange != null)
					{
						email.Attachments.Add(new AttachmentDef("Subscription from " + interchange.EI_SystemCreateTimeUtc.ToLongTimeString() + ".csv", Encoding.UTF8.GetBytes(interchange.EI_BodyText)));

						var groupPK = FreightDataRegistry.Instance.OneStopNotificationGroup.GetFallBackValueAtAllLevels(interchange.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty);
						email.AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(groupPK, FreightDataRegistry.Instance.OneStopNotificationGroup);
						foundRecipients = true;
					}
				}
			}

			if (!foundRecipients)
			{
				email.AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(FreightDataRegistry.Instance.OneStopNotificationGroup.Value, FreightDataRegistry.Instance.OneStopNotificationGroup);
			}

			return email;
		}

		#endregion
	}
}
