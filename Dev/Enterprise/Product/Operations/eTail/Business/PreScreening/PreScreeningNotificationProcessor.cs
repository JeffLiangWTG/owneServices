using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.Business
{
	public class PreScreeningNotificationProcessor
	{
		public PreScreeningNotificationProcessor()
		{
		}

		public void SendPreScreeningResult(BusinessObject consignmentCollectionParent, IHVLVConsignmentPreScreeningResult[] failedResults)
		{
			var email = new HtmlEmailDef();
			var parentName = consignmentCollectionParent.HumanReadableName;
			email.Subject = Res.GetString("367376FB-A6BC-4BA7-BC4F-4BCA0ACFB1F2", "HVLV Pre-Screening Report – FAL Consignments for {0}", parentName);

			SendNotificationToGroups(consignmentCollectionParent, failedResults, email);
			SendNotificationToStaff(consignmentCollectionParent, failedResults, email);
		}

		void SendNotificationToGroups(BusinessObject consignmentCollectionParent, IHVLVConsignmentPreScreeningResult[] failedResults, HtmlEmailDef email)
		{
			var groupsResultsDictionary = failedResults.SelectMany(r => r.PreScreeningErrorDetails.Concat(r.PreScreeningWarningDetails))
				.Where(x => x.Group != null)
				.GroupBy(d => d.Group)
				.ToDictionary(g => g.Key, g => g.ToArray());
			var emailBody = string.Empty;

			foreach (var groupResultsEntry in groupsResultsDictionary)
			{
				emailBody = GetBody(consignmentCollectionParent, groupResultsEntry.Value);

				email.LoadHtmlUsingTemplate(emailBody);
				if (email.Subject != null && email.Body != null)
				{
					var notificationGroup = groupResultsEntry.Key;
					Env.OutgoingMailManager.CreateAndSave(email, notificationGroup.PK.ToGuid(), GroupSourceLocator.GetFromGroup(notificationGroup));
				}
			}
		}

		void SendNotificationToStaff(BusinessObject consignmentCollectionParent, IHVLVConsignmentPreScreeningResult[] failedResults, HtmlEmailDef email)
		{
			var detailsToSend = failedResults.SelectMany(r => r.PreScreeningErrorDetails.Concat(r.PreScreeningWarningDetails))
				.Where(x => x.NotifyStaffMember).ToArray();
			var emailBody = string.Empty;

			if (detailsToSend != null && detailsToSend.Any())
			{
				emailBody = GetBody(consignmentCollectionParent, detailsToSend);

				email.LoadHtmlUsingTemplate(emailBody);
				if (email.Subject != null && email.Body != null)
				{
					email.AddRecipientForSystemCommunication(GlbStaff.CurrentUser.GS_EmailAddress);
					Env.OutgoingMailManager.CreateAndSave(email);
				}
			}
		}

		string GetBody(BusinessObject consignmentsParent, IPreScreenNotificationDetail[] details)
		{
			var emailBody = new StringBuilder();

			if (consignmentsParent is HVLVBookingHeader)
			{
				emailBody.Append(GetNotificationHeaderFromBookingHeader((HVLVBookingHeader)consignmentsParent));
			}
			else if (consignmentsParent is ForwardingShipment)
			{
				emailBody.Append(GetNotificationHeaderFromShipment((ForwardingShipment)consignmentsParent));
			}

			emailBody.Append(GetPreScreeningResultEmailTable(details));
			emailBody.Append((NoResString)"<br /><hr><br />"); // Hard-coded HTML
			emailBody.Append(Res.GetString("40E2D40F-1066-4627-A391-0186162F4F2F", "Regards,"));
			emailBody.Append("<br /><br />");
			emailBody.Append(Res.GetString("F1E184B4-AD16-49C3-A9CE-CB2FB5FEED17", "CargoWise Administrative Messages Sender"));

			return emailBody.ToString();
		}

		string GetNotificationHeaderFromBookingHeader(HVLVBookingHeader bookingHeader)
		{
			var emailBody = new StringBuilder();
			emailBody.Append(Res.GetString("7A50BD91-269F-4AB2-AF15-46EDE345A89F", "BOOKING HEADER DETAILS:"));
			emailBody.Append("<br />");
			emailBody.Append(Res.GetString("D7B09CB1-E216-4C6A-9E34-4D03FB6A1CBC", "Bill To Party: {0}<br />", bookingHeader.BillToParty?.CompanyName));
			emailBody.Append(Res.GetString("D0739535-9D8E-4D3A-B37D-7F08D0785C58", "Dispatch Address: {0}<br />", bookingHeader.DispatchAddress?.CompanyName));
			emailBody.Append(Res.GetString("0AF2812F-D63E-44A4-ABB9-7AFD8FE08F3E", "Origin Depot: {0}<br />", bookingHeader.OriginDepot?.CompanyName));
			emailBody.Append(Res.GetString("8D6966F2-4093-4E6B-93E5-447277B47570", "HVLV Pre-Screening Report:"));
			emailBody.Append("<br />");

			return emailBody.ToString();
		}

		string GetNotificationHeaderFromShipment(ForwardingShipment shipment)
		{
			var emailBody = new StringBuilder();
			emailBody.Append(Res.GetString("AC79E21B-E595-489C-94E1-E343D80F9BDF", "SHIPMENT DETAILS:"));
			emailBody.Append("<br />");
			emailBody.Append(Res.GetString("D26FA1D9-B81E-459B-A107-C69B687D792F", "Transport Mode: {0}<br />", shipment.TransportMode));
			emailBody.Append(Res.GetString("3AC85703-2B95-4F20-B3EA-E170E6698CD6", "eTailer: {0}<br />", shipment.Consignor?.OH_FullName));
			emailBody.Append(Res.GetString("A86B7765-7A2C-4114-9D9B-889FF23BD00E", "Consignee: {0}<br />", shipment.Consignee?.OH_FullName));
			emailBody.Append(Res.GetString("FFB6F64F-9568-4DE8-B23C-C1D3953B8FEF", "Origin: {0}<br />", shipment.Origin?.Code));
			emailBody.Append(Res.GetString("97443D18-AFC4-4A75-AE47-6EC234C427C5", "Destination: {0}<br />", shipment.Destination?.Code));
			emailBody.Append(Res.GetString("882D5A37-5741-457C-91B8-4553FC824054", "HVLV Pre-Screening Report:"));
			emailBody.Append("<br />");

			return emailBody.ToString();
		}

		string GetPreScreeningResultEmailTable(IPreScreenNotificationDetail[] details)
		{
			var emailTable = new HtmlTableCreator(new string[] {
				Res.GetString("94844CC4-1A2A-4D9D-BF4E-4E0452DA3557", "Consignment ID"),
				Res.GetString("65921008-B463-495E-8DA1-2A512699B5A8", "Consignment Waybill #"),
				Res.GetString("D549C1FA-4113-45B0-A0D5-4F3738644FE8", "Consignment Shipper Ref #"),
				Res.GetString("CB8BF6A7-B753-48BB-B3B3-3372F7DE2B00", "Pre-Screening Status"),
				Res.GetString("ECD4BA8A-E41A-4482-8DB0-915D0ECFEF8F", "Validation Rule"),
				Res.GetString("9DC68CE1-A3A6-463A-ADD0-9719819D9CAA", "Message"),
				Res.GetString("6D62DCBB-CEE4-4CCB-9053-C19C04F5C1E3", "Consignment Hyperlink"),
			});

			emailTable.EnableHTMLEncoding = false;

			foreach (PreScreenNotificationDetail detail in details)
			{
				emailTable.WriteRow(new object[]
				{
					detail.Consignment.HVC_ConsignmentId,
					detail.Consignment.HVC_WaybillNumber,
					detail.Consignment.HVC_ShipperReference,
					HVLVConsignmentPreScreeningStatusCodes.Codes.Failed,
					detail.ValidationRuleCode,
					detail.Message,
					GetHyperlink(detail.Consignment)
				});
			}

			return emailTable.ToHtml();
		}

		ZString GetHyperlink(IHVLVConsignment consignment)
		{
			var url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.HVLVConsignment, consignment.PK.ToGuid());
			var hyperlink = (NoResString)"<a href=\"" + url + (NoResString)"\">" + consignment.HVC_ConsignmentId + (NoResString)"</a>"; // hyperlink
			return hyperlink;
		}
	}
}
