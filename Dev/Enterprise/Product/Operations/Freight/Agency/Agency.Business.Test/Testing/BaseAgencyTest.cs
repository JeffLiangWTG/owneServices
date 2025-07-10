using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	public class BaseAgencyTest : BaseFreightTest
	{
		#region NewShipment
		public AgencyShipment NewShipment(JobSailing sailing, OrgHeader principal, bool isCancelled, bool isConfirmed)
		{
			return NewShipment(Factory, sailing, principal, isCancelled, isConfirmed ? ShipmentStatusList.Codes.Confirmed : ShipmentStatusList.Codes.Booked);
		}

		public static AgencyShipment NewShipment(BusinessObjectFactory factory, JobSailing sailing, OrgHeader principal, bool isCancelled, bool isConfirmed)
		{
			return NewShipment(factory, sailing, principal, isCancelled, isConfirmed ? ShipmentStatusList.Codes.Confirmed : ShipmentStatusList.Codes.Booked);
		}

		public static AgencyShipment NewShipment(BusinessObjectFactory factory, JobSailing sailing, OrgHeader principal, bool isCancelled, ZString shipmentStatus)
		{
			AgencyShipment shipment;
			if (ShipmentStatusHelperMethods.IsBillOfLadingStage(shipmentStatus))
			{
				shipment = factory.New<BillOfLading>();
			}
			else
			{
				shipment = factory.New<AgencyBooking>();
				shipment.JS_ShipmentStatus = shipmentStatus;
			}

			shipment.JS_IsCancelled = isCancelled;
			shipment.JS_OH_DeliveryAgent = principal == null ? ZGuid.Empty : principal.PK;
			if (sailing != null)
			{
				shipment.JS_RL_NKOrigin = sailing.JX_JA_RL_NKPortOfLoading;
				shipment.JS_RL_NKDestination = sailing.JX_JB_RL_NKPortOfDischarge;
				shipment.JS_JX = sailing.PK;
			}

			return shipment;
		}

		#endregion
		#region NewBulkShipment
		public AgencyShipment NewBulkShipment(JobSailing sailing, OrgHeader principal, bool isCancelled, bool isConfirmed, int tonnes, int volume)
		{
			return NewBulkShipment(sailing, principal, isCancelled, isConfirmed ? ShipmentStatusList.Codes.Confirmed : ShipmentStatusList.Codes.Booked, tonnes, volume);
		}

		public AgencyShipment NewBulkShipment(JobSailing sailing, OrgHeader principal, bool isCancelled, ZString shipmentStatus, int tonnes, int volume)
		{
			AgencyShipment shipment = NewShipment(Factory, sailing, principal, isCancelled, shipmentStatus);
			shipment.JS_PackingMode = Constants.ContainerModes.Bulk;
			shipment.JS_UnitOfWeight = Constants.Weight.Tonnes;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_ActualVolume = volume;
			shipment.JS_ActualWeight = tonnes;
			return shipment;
		}

		#endregion
		#region NewFCLShipment
		public AgencyShipment NewFCLShipment(JobSailing sailing, OrgHeader principal, bool isCancelled, bool isConfirmed, int gp20, int re40)
		{
			return NewFCLShipment(sailing, principal, isCancelled, isConfirmed ? ShipmentStatusList.Codes.Confirmed : ShipmentStatusList.Codes.Booked, gp20, re40);
		}

		public AgencyShipment NewFCLShipment(JobSailing sailing, OrgHeader principal, bool isCancelled, ZString shipmentStatus, int gp20, int re40)
		{
			return NewFCLShipment(sailing, principal, isCancelled, shipmentStatus, gp20, re40, ZString.Empty);
		}

		public AgencyShipment NewFCLShipment(JobSailing sailing, OrgHeader principal, bool isCancelled, ZString shipmentStatus, int gp20, int re40, ZString containerNumber)
		{
			AgencyShipment shipment = NewShipment(Factory, sailing, principal, isCancelled, shipmentStatus);
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AgencyShipmentContainerDependentCollection containers = shipment.IsBillOfLadingStage ? shipment.RealContainers : shipment.BookedContainers;
			if (gp20 > 0)
			{
				AgencyShipmentContainer container = containers.AddNew();
				if (!containerNumber.IsEmpty)
				{
					container.JC_ContainerNum = containerNumber;
					gp20 = 1;
				}

				container.JC_GrossWeightUQ = Constants.Weight.Kilograms;
				container.JC_RC = RC_20GP_PK;
				container.JC_ContainerCount = (ZShort)gp20;
				container.JC_GrossWeight = 2400 * gp20;
			}

			if (re40 > 0)
			{
				AgencyShipmentContainer container = containers.AddNew();
				if (!containerNumber.IsEmpty)
				{
					container.JC_ContainerNum = containerNumber;
					re40 = 1;
				}

				container.JC_GrossWeightUQ = Constants.Weight.Tonnes;
				container.JC_RC = RC_40RE_PK;
				container.JC_ContainerCount = (ZShort)re40;
				container.JC_GrossWeight = 5 * re40;
			}

			return shipment;
		}

		#endregion
		#region NewRORShipment
		public AgencyShipment NewRORShipment(JobSailing sailing, OrgHeader principal, bool isCancelled, bool isConfirmed, int count)
		{
			return NewRORShipment(sailing, principal, isCancelled, isConfirmed ? ShipmentStatusList.Codes.Confirmed : ShipmentStatusList.Codes.Booked, count);
		}

		public AgencyShipment NewRORShipment(JobSailing sailing, OrgHeader principal, bool isCancelled, ZString shipmentStatus, int count)
		{
			AgencyShipment shipment = NewShipment(Factory, sailing, principal, isCancelled, shipmentStatus);
			shipment.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_ActualWeight = 650 * count;
			shipment.JS_ActualVolume = 4 * count;
			shipment.Vehicles.RemoveAndDeleteAll();
			var vehicle = shipment.Vehicles.AddNew();
			vehicle.JC_ContainerCount = (ZShort)count;
			vehicle.JC_TotalUnitOfMeasure = Constants.Length.Metres;
			vehicle.JC_TotalLength = 2m;
			vehicle.JC_TotalWidth = 4m;
			vehicle.JC_TotalHeight = 0.5;
			return shipment;
		}

		#endregion
		#region NewCarrier
		public OrgHeader NewCarrier()
		{
			return NewCarrier(Factory);
		}

		public OrgHeader NewCarrier(BusinessObjectFactory factory)
		{
			OrgHeader result = factory.NewWithValidTestData<OrgHeader>();
			result.OH_IsShippingLine = true;
			result.OH_IsShippingProvider = true;
			return result;
		}

		#endregion
		#region NewPrincipal
		public OrgHeader NewPrincipal()
		{
			return NewPrincipal(Factory);
		}

		public static OrgHeader NewPrincipal(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<OrgHeader>();
			SetValuesForNewPrincipal(result);
			return result;
		}

		public static OrgHeader GetCurrentBranchOrgProxy()
		{
			OrgHeader principal = GlbBranch.CurrentBranch.OrgProxy;
			SetValuesForNewPrincipal(principal);

			return principal;
		}

		public static void SetValuesForNewPrincipal(OrgHeader orgHeader, string shortCode = null)
		{
			orgHeader.OH_IsShippingProvider = true;
			orgHeader.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			if (shortCode != null)
			{
				orgHeader.OH_Code = shortCode;
			}
		}

		#endregion
		#region NewStaff
		public GlbStaff NewStaff(IEnumerable<OrgHeader> allowedOrganizationsAndWarehouses)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var security = (IOrgsAndWarehousesAccessProvider)staff;
			security.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedPrincipalsSecurityRightName;
			allowedOrganizationsAndWarehouses.ToList().ForEach(p => security.AddSecurityToAccessOrgOrWarehouse(p.OH_Code));
			return staff;
		}

		#endregion
		#region NewConsignor
		public OrgHeader NewConsignor()
		{
			return NewConsignor(Factory);
		}

		public OrgHeader NewConsignor(BusinessObjectFactory factory)
		{
			OrgHeader result = factory.NewWithValidTestData<OrgHeader>();
			result.OH_IsConsignor = true;
			return result;
		}

		#endregion
		#region NewConignee
		public OrgHeader NewConsignee()
		{
			return NewConsignee(Factory);
		}

		public OrgHeader NewConsignee(BusinessObjectFactory factory)
		{
			OrgHeader result = factory.NewWithValidTestData<OrgHeader>();
			result.OH_IsConsignee = true;
			return result;
		}

		#endregion
		#region New Branch
		protected GlbBranch NewBranch(string countryCode, string branchCode = "auto")
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = countryCode;
			var branch = company.Branches.AddNew();
			branch.GB_Code = branchCode == "auto" ? countryCode : branchCode;
			var homePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, countryCode));
			if (homePort != null)
			{
				branch.GB_RL_NKHomePort = homePort.RL_Code;
			}

			Factory.Save();
			return branch;
		}

		#endregion
		#region FindOrCreateSailing
		protected JobSailing FindOrCreateSailing(JobVoyage voyage, ZString loadPort, ZString dischargePort)
		{
			bool addedPort = false;
			if (voyage.Origins.GetOriginFromLoading(loadPort) == null)
			{
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = loadPort;
				addedPort = true;
			}

			if (voyage.Destinations.GetDestinationFromDischarge(dischargePort) == null)
			{
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = dischargePort;
				addedPort = true;
			}

			if (addedPort)
			{
				voyage.GenerateSailings();
			}

			return voyage.Sailings.GetSailingFromLoadAndDischarge(loadPort, dischargePort);
		}

		#endregion
		#region SetPortAuthoritySettings
		protected void SetPortAuthoritySettings(params ZString[] unlocos)
		{
			PortAuthorityPortCollection ports = new PortAuthorityPortCollection();
			for (int i = 0; i < unlocos.Length; i++)
			{
				PortAuthorityPort setting = ports.AddNew();
				setting.Port = unlocos[i];
				setting.ProductionEmail = string.Format("bob{0}@freadnet.org", i + 1);
				setting.ProductionID = string.Format("RecipientID{0}", i + 1);
			}

			AgencyRegistry.Instance.PortAuthorityPorts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ports);
			PortAuthoritySettings settings = new PortAuthoritySettings();
			foreach (PortAuthoritySetting setting in settings.Settings)
			{
				setting.Status = PortAuthoritySettingStatus.Codes.Production;
				setting.SenderID = string.Format("SenderID{0}", ((IList<ZString>)unlocos).IndexOf(setting.Port) + 1);
			}

			AgencyRegistry.Instance.PortAuthoritySettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);
		}

		#endregion
		#region AssertAllocationUsage
		public void AssertAllocationUsage(string prefix, AllocationUsage usage, ZDecimal tonnes, ZDecimal volume, ZDecimal area, ZDecimal total_teu, ZDecimal gp_teu, ZDecimal reefer_teu, ZInt powerPoints)
		{
			CombineAssertions(delegate
			{
				AssertEquals(prefix + "Tonnes", tonnes, usage.Tonnes);
				AssertEquals(prefix + "Volume", volume, usage.Volume);
				AssertEquals(prefix + "Area", area, usage.Area);
				AssertEquals(prefix + "PowerPoints", powerPoints, usage.PowerPoints);
				AssertEquals(prefix + "TEU", total_teu, usage.TEU);
				AssertEquals(prefix + "GP_TEU", gp_teu, usage.GP_TEU);
				AssertEquals(prefix + "Reefer_TEU", reefer_teu, usage.Reefer_TEU);
			});
		}

		public void AssertAllocationUsage(string prefix, AllocationUsage usage, ZDecimal tonnes, ZDecimal volume, ZDecimal area, ZDecimal total_teu, ZDecimal gp_teu, ZDecimal reefer_teu, ZInt powerPoints, VoyageOrigin origin)
		{
			CombineAssertions(delegate
			{
				AssertAllocationUsageCore(prefix, usage, tonnes, volume, area, total_teu, gp_teu, reefer_teu, powerPoints);
				AssertEquals(prefix + "Parent", origin.PK, usage.ParentPK);
			});
		}

		public void AssertAllocationUsage(string prefix, AllocationUsage usage, ZDecimal tonnes, ZDecimal volume, ZDecimal area, ZDecimal total_teu, ZDecimal gp_teu, ZDecimal reefer_teu, ZInt powerPoints, JobSailing sailing)
		{
			CombineAssertions(delegate
			{
				AssertAllocationUsageCore(prefix, usage, tonnes, volume, area, total_teu, gp_teu, reefer_teu, powerPoints);
				AssertEquals(prefix + "Parent", sailing.PK, usage.ParentPK);
			});
		}

		public void AssertAllocationUsage(string prefix, AllocationUsage expected, AllocationUsage actual)
		{
			CombineAssertions(delegate
			{
				AssertEquals(prefix + ": Tonnes", expected.Tonnes, actual.Tonnes);
				AssertEquals(prefix + ": Volume", expected.Volume, actual.Volume);
				AssertEquals(prefix + ": Area", expected.Area, actual.Area);
				AssertEquals(prefix + ": PowerPoints", expected.PowerPoints, actual.PowerPoints);
				AssertEquals(prefix + ": TEU", expected.TEU, actual.TEU);
				AssertEquals(prefix + ": GP_TEU", expected.GP_TEU, actual.GP_TEU);
				AssertEquals(prefix + ": Reefer_TEU", expected.Reefer_TEU, actual.Reefer_TEU);
			});
		}

		void AssertAllocationUsageCore(string prefix, AllocationUsage usage, ZDecimal tonnes, ZDecimal volume, ZDecimal area, ZDecimal total_teu, ZDecimal gp_teu, ZDecimal reefer_teu, ZInt powerPoints)
		{
			AssertEquals(prefix + "Tonnes", tonnes, usage.Tonnes);
			AssertEquals(prefix + "Volume", volume, usage.Volume);
			AssertEquals(prefix + "Area", area, usage.Area);
			AssertEquals(prefix + "PowerPoints", powerPoints, usage.PowerPoints);
			AssertEquals(prefix + "TEU", total_teu, usage.TEU);
			AssertEquals(prefix + "GP_TEU", gp_teu, usage.GP_TEU);
			AssertEquals(prefix + "Reefer_TEU", reefer_teu, usage.Reefer_TEU);
		}

		#endregion
		#region AssertMessageEquals
		public void AssertMessageEquals(string expected, string actual)
		{
			AssertMessageEquals("", expected, actual);
		}

		public void AssertMessageEquals(string message, string expected, string actual)
		{
			AssertMultilineASCIIEquals(message, expected.Replace("'", System.Environment.NewLine), actual.Replace("'", System.Environment.NewLine));
		}

		#endregion
		#region GetVoyageWithoutNotifications
		public JobVoyage GetVoyageWithoutNotifications()
		{
			ShippingCompany1.OH_IsShippingProvider = true;
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = "1234";
			voyage.JV_OH_Line = ShippingCompany1.PK;
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(2);
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(4);
			voyage.GenerateSailings();
			voyage.RunPreSaveValidation();
			AssertNoNotifications("Precondition: this test needs a voyage without errors, if this fails then adjust the voyage to remove any errors", voyage);
			return voyage;
		}

		#endregion
		#region Customs Codes
		public static void SetAcosCode(OrgHeader header, ZString value)
		{
			SetOrgCusCode(header, OrgCusCode.CodeTypes.OneStopCode, Constants.CountryCodes.Australia, value, false);
		}

		public static void SetAcosCodeIfNotSet(OrgHeader header, ZString value)
		{
			SetOrgCusCode(header, OrgCusCode.CodeTypes.OneStopCode, Constants.CountryCodes.Australia, value, true);
		}

		static void SetOrgCusCode(OrgHeader header, ZString codeType, ZString countryCode, ZString value, bool overwrite)
		{
			OrgCusCode code = header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(codeType, countryCode);
			if (code == null)
			{
				code = header.CustomsCodes.AddNew();
				code.OK_CodeType = codeType;
				code.OK_RN_NKCodeCountry = countryCode;
				code.OK_CustomsRegNo = value;
			}
			else if (code.OK_CustomsRegNo.IsEmpty || overwrite)
			{
				code.OK_CustomsRegNo = value;
			}
		}

		#endregion
		#region ConvertAll
		public static IEnumerable<OutputT> ConvertAllCast<InputT, OutputT>(IEnumerable values, Converter<InputT, OutputT> converter)
		{
			foreach (InputT value in values)
			{
				yield return converter(value);
			}
		}

		public static IEnumerable<OutputT> ConvertAll<InputT, OutputT>(IEnumerable<InputT> values, Converter<InputT, OutputT> converter)
		{
			foreach (InputT value in values)
			{
				yield return converter(value);
			}
		}

		#endregion
		#region FindEmail
		public static EmailDef FindEmail(string emailAddress)
		{
			return FindEmail((e) => e.Recipients.Contains(emailAddress));
		}

		public static EmailDef FindEmail(string emailAddress, string subject)
		{
			return FindEmail((e) => e.Subject == subject && e.Recipients.Contains(emailAddress));
		}

		public static EmailDef FindEmail(Predicate<EmailDef> predicate)
		{
			foreach (EmailDef email in Env.OutgoingMailManager.EmailsCreated)
			{
				if (predicate(email))
				{
					return email;
				}
			}

			return null;
		}

		#endregion
		#region FindAttachment
		public static AttachmentDef FindAttachment(EmailDef email, string displayName)
		{
			return FindAttachment(email, (a) => a.DisplayName == displayName);
		}

		public static AttachmentDef FindAttachment(EmailDef email, Predicate<AttachmentDef> predicate)
		{
			foreach (AttachmentDef attachment in email.Attachments)
			{
				if (predicate(attachment))
				{
					return attachment;
				}
			}

			return null;
		}

		#endregion
		#region Assert Events
		public static void AssertNoEvents(string message, BusinessObject bizObj, Event eventType)
		{
			ZQuery filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, eventType.Code);
			if (bizObj.GetLogs().Find(filter).Length == 0)
			{
				Assert(true);
			}
			else
			{
				StringBuilder builder = new StringBuilder(Html(message));
				builder.AppendFormat("<br/>Not Expecting To Find Events Of Type: {0}<br/>", eventType.Code);
				HtmlEventList(builder, bizObj, eventType, null);
				HtmlFail(builder.ToString());
			}
		}

		public static void AssertNoEvent(string message, BusinessObject bizObj, Event eventType, string reference)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventType.Code);
			filter.AddToFilter(StmALogSchema.SL_Reference, reference);
			if (bizObj.GetLogs().Find(filter).Length == 0)
			{
				Assert(true);
			}
			else
			{
				StringBuilder builder = new StringBuilder(Html(message));
				builder.AppendFormat("<br/>Not Expecting To Find: {0}:{1}<br/>", eventType.Code, reference);
				HtmlEventList(builder, bizObj, eventType, reference);
				HtmlFail(builder.ToString());
			}
		}

		public static void AssertHasEvent(string message, BusinessObject bizObj, Event eventType, string reference)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventType.Code);
			filter.AddToFilter(StmALogSchema.SL_Reference, reference);
			if (bizObj.GetLogs().Find(filter).Length > 0)
			{
				Assert(true);
			}
			else
			{
				StringBuilder builder = new StringBuilder(Html(message));
				builder.AppendFormat("<br/>Expecting To Find: {0}:{1}<br/>", eventType.Code, reference);
				HtmlEventList(builder, bizObj, eventType, reference);
				HtmlFail(builder.ToString());
			}
		}

		static string HtmlEventList(StringBuilder builder, BusinessObject bizObj, Event interestingEventType, string interestingReference)
		{
			string interestingReferenceAsHtml = interestingReference == null ? null : Html(interestingReference.Trim());
			StmALog[] logs = bizObj.GetLogs().GetAllLogs().ToArray<StmALog>();
			if (logs.Length > 0)
			{
				builder.AppendFormat("<b>Found:</b><br/>");
				foreach (StmALog log in logs)
				{
					if (interestingEventType != null && log.SL_SE_NKEvent == interestingEventType.Code)
					{
						builder.AppendFormat("<b>{0}</b>: ", log.SL_SE_NKEvent);
					}
					else
					{
						builder.AppendFormat("{0}: ", Html(log.SL_SE_NKEvent));
					}

					if (!string.IsNullOrEmpty(interestingReferenceAsHtml))
					{
						string logReferenceHtml = Html(log.SL_Reference);
						logReferenceHtml = logReferenceHtml.Replace(interestingReferenceAsHtml, "<b>" + interestingReferenceAsHtml + "</b>");
						builder.Append(logReferenceHtml);
						builder.Append("<br/>");
					}
					else
					{
						builder.AppendFormat(" {0}<br/>", Html(log.SL_Reference));
					}
				}
			}

			return builder.ToString();
		}

		#endregion
		#region FormatEmail
		public static string FormatEmail(EmailDef email)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("FROM:\r\n  {0}\r\n", email.FromAddress);
			builder.AppendFormat("SUBJECT:\r\n  {0}\r\n", email.Subject);
			AppendSorted(builder, "TO:\r\n", "  {0}\r\n", email.Recipients.ToStringCollection());
			AppendSorted(builder, "CC:\r\n", "  {0}\r\n", email.CCRecipients.ToStringCollection());
			AppendSorted(builder, "BCC:\r\n", "  {0}\r\n", email.BCCRecipients.ToStringCollection());
			return builder.ToString();
		}

		static void AppendSorted(StringBuilder builder, string heading, string format, StringCollection values)
		{
			List<string> list = new List<string>(ConvertAllCast(values, (string s) => s));
			if (list.Count > 0)
			{
				list.Sort();
				builder.Append(heading);
				foreach (string value in values)
				{
					builder.AppendFormat(format, value);
				}
			}
		}

		#endregion
		#region Sailings
		public new JobSailing ExportSailing
		{
			get
			{
				if (exportSailing == null)
				{
					SetupSailings();
				}

				return exportSailing;
			}
		}

		public new JobSailing ImportSailing
		{
			get
			{
				if (importSailing == null)
				{
					SetupSailings();
				}

				return importSailing;
			}
		}

		void SetupSailings()
		{
			ZDateTime now = ZDateTime.Now;
			var carrier = NewCarrier();
			carrier.OH_Code = "KARRAMBA";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "x42";
			voyage.JV_OH_Line = carrier.PK;
			VoyageOrigin origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "NZAKL";
			origin1.JA_E_DEP = now.AddDays(1);
			VoyageDestination destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUBNE";
			destination1.JB_E_ARV = now.AddDays(2);
			VoyageOrigin origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUBNE";
			origin2.JA_E_DEP = now.AddDays(3);
			VoyageDestination destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "SGSIN";
			destination2.JB_E_ARV = now.AddDays(4);
			voyage.GenerateSailings();
			importSailing = voyage.Sailings.GetSailingFromLoadAndDischarge("NZAKL", "AUBNE");
			exportSailing = voyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "SGSIN");
		}

		JobSailing exportSailing;
		JobSailing importSailing;
		#endregion
		public void AssertLastUserQuestion(ZString expectedQuestion)
		{
			AssertEquals("Should have asked the user", true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			AssertEquals(expectedQuestion, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void AssertContainUserQuestion(ZString expectedQuestion)
		{
			var actualMessage = UnitTestUserNotification.Instance.PreviousMessages.FirstOrDefault(x => x.Text == expectedQuestion);

			AssertNotNull(actualMessage);
			AssertEquals("Should have asked the user", true, actualMessage.WasQuestion);
		}

		public void AssertLastUserErrorMessage(ZString expectedMessage)
		{
			AssertEquals("Should have displayed an error", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals("The error message was incorrect", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public static string ContainerPackPivots(AgencyShipment shipment)
		{
			const string sql = @"
select JL_DetailedDescription, JC_ContainerNum
from dbo.JobContainerPackPivot
left join dbo.JobPackLines on JL_PK = J6_JL
left join dbo.JobContainer on J6_JC = JC_PK
where JL_JS = @shipmentPK or JC_JS_FCLBookingOnlyLink = @shipmentPK
";
			ZSqlParameterCollection paramCollection = new ZSqlParameterCollection();
			paramCollection.Add("@shipmentPK", shipment.PK, JobShipmentSchema.PK);
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(shipment.Factory);
			collection.Load(sql, paramCollection);
			StringBuilder builder = new StringBuilder();
			foreach (DynamicBusinessObject dbo in collection)
			{
				builder.Append('[');
				builder.Append(dbo["JL_DetailedDescription"]);
				builder.Append("]:[");
				builder.Append(dbo["JC_ContainerNum"]);
				builder.AppendLine("]");
			}

			return builder.ToString();
		}
	}
}
