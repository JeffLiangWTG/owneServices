using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.QuotedBookings.Business
{
	[ModuleID(ModuleId.QuotedBookings)]
	public class ViewQuotedBookingForCombineCollection : BusinessObjectCollection<ViewQuotedBooking>
	{
		public ViewQuotedBookingForCombineCollection(QuotedBooking quotedBooking)
			: base(quotedBooking.Factory)
		{
			ParentQuotedBooking = quotedBooking;
		}

		readonly QuotedBooking ParentQuotedBooking;

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			var viewQuotedBooking = bizOAdded as ViewQuotedBooking;

			if (viewQuotedBooking != null && viewQuotedBooking.QuotedBooking != null && ParentQuotedBooking != null)
			{
				var errorMessage = GetErrorWhenCantCombine(viewQuotedBooking.QuotedBooking);
				if (!errorMessage.IsEmpty)
				{
					viewQuotedBooking.AddRowError(errorMessage);
				}

				var warningMessage = GetWarningForUnmatchedFields(viewQuotedBooking.QuotedBooking);
				if (!warningMessage.IsEmpty)
				{
					viewQuotedBooking.AddRowWarning(warningMessage);
				}
			}
		}

		#region Implementation

		ZString GetErrorWhenCantCombine(QuotedBooking quotedBooking)
		{
			var unmatchedProperties = new List<ZString>();

			if (quotedBooking.Booking == null)
			{
				return Res.GetString("5660f822-790e-9b94-434f-4a07e599b17e", "One Off Quotes cannot be chosen. Please choose a booking.");
			}

			if (quotedBooking.PK == ParentQuotedBooking.PK)
			{
				return Res.GetString("f1e37257-7fab-4731-b075-46502555bb5c", "Main booking cannot be chosen here. Please choose another booking.");
			}

			if (quotedBooking.Origin != ParentQuotedBooking.Origin)
			{
				unmatchedProperties.Add(Res.GetString("6cb3a315-a93d-4591-8a30-fa8ec37ef67c", "Origin"));
			}

			if (quotedBooking.Destination != ParentQuotedBooking.Destination)
			{
				unmatchedProperties.Add(Res.GetString("27297d37-4705-454e-95bf-fc3591a481ec", "Destination"));
			}

			if (quotedBooking.Client != null
				&& ParentQuotedBooking.Client != null
				&& quotedBooking.Client.PK != ParentQuotedBooking.Client.PK)
			{
				unmatchedProperties.Add(Res.GetString("d528599e-24f6-4db1-b433-3e8ed7765edf", "Client"));
			}

			if (quotedBooking.Consignee != null
				&& ParentQuotedBooking.Consignee != null
				&& quotedBooking.Consignee.PK != ParentQuotedBooking.Consignee.PK)
			{
				unmatchedProperties.Add(Res.GetString("451563e8-7ea0-43cc-b657-a7796017f9e3", "Consignee"));
			}

			if (quotedBooking.Consignor != null
				&& ParentQuotedBooking.Consignor != null
				&& quotedBooking.Consignor.PK != ParentQuotedBooking.Consignor.PK)
			{
				unmatchedProperties.Add(Res.GetString("79e26448-7c3d-42d3-8749-bec69aa102a6", "Consignor"));
			}

			if (quotedBooking.TransportMode != ParentQuotedBooking.TransportMode)
			{
				unmatchedProperties.Add(Res.GetString("0e4480d9-d2b3-405f-9c51-c7c5e9d46746", "Transport Mode"));
			}

			if (quotedBooking.ContainerMode != ParentQuotedBooking.ContainerMode)
			{
				unmatchedProperties.Add(Res.GetString("ee73896e-95f0-476a-9ad6-999c1f4bbdc6", "Container Mode"));
			}

			if (quotedBooking.ControllingCustomer != null
				&& ParentQuotedBooking.ControllingCustomer != null
				&& quotedBooking.ControllingCustomer.PK != ParentQuotedBooking.ControllingCustomer.PK)
			{
				unmatchedProperties.Add(Res.GetString("5a015947-fbf1-4949-8d7a-40dd922b5379", "Controlling Customer"));
			}

			if (quotedBooking.Booking != null && ParentQuotedBooking.Booking != null
				&& quotedBooking.Booking.ControllingAgent != null
				&& ParentQuotedBooking.Booking.ControllingAgent != null
				&& quotedBooking.Booking.ControllingAgent.PK != ParentQuotedBooking.Booking.ControllingAgent.PK)
			{
				unmatchedProperties.Add(Res.GetString("c46be4ee-b7fc-4e93-86f0-3a3f308cb413", "Controlling Agent"));
			}

			if (unmatchedProperties.Count == 1)
			{
				return Res.GetString("bf28117a-98bc-4178-a1cc-c777bbe38f4f", "This Booking cannot be chosen here. This field is different from the field on the Main booking: {0}. Please choose another booking.", unmatchedProperties[0]);
			}
			else if (unmatchedProperties.Count >= 1)
			{
				return Res.GetString("ddf8e372-5310-456c-8fc7-512cdfc0e183", "This Booking cannot be chosen here. These fields are different from the fields on the Main booking: {0}. Please choose another booking.", string.Join(", ", unmatchedProperties));
			}

			return ZString.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Scope = "GetWarningForUnmatchedFields", Justification = "Null Checks")]
		ZString GetWarningForUnmatchedFields(QuotedBooking quotedBooking)
		{
			var unmatchedProperties = new List<ZString>();

			if (quotedBooking.PaymentTerms != ParentQuotedBooking.PaymentTerms)
			{
				unmatchedProperties.Add(quotedBooking.PaymentTermLabel);
			}

			if (quotedBooking.ServiceLevel != ParentQuotedBooking.ServiceLevel)
			{
				unmatchedProperties.Add(quotedBooking.ServiceLevelInfo.HumanReadableName);
			}

			if (quotedBooking.Booking != null && ParentQuotedBooking.Booking != null)
			{
				if (quotedBooking.Booking.ConsignorPickupAddress.OrganisationPK.IsValid
					&& quotedBooking.Booking.ConsignorPickupAddress.OrganisationPK != ParentQuotedBooking.Booking.ConsignorPickupAddress.OrganisationPK)
				{
					unmatchedProperties.Add(Res.GetString("a2053584-1bf3-4739-80ae-160b088ac67a", "Pickup Org."));
				}

				if (quotedBooking.Booking.ConsigneeDeliveryAddress.OrganisationPK.IsValid
					&& quotedBooking.Booking.ConsigneeDeliveryAddress.OrganisationPK != ParentQuotedBooking.Booking.ConsigneeDeliveryAddress.OrganisationPK)
				{
					unmatchedProperties.Add(Res.GetString("efccb25f-91dd-47b7-8fa6-864f326d5fe1", "Delivery Org."));
				}

				if (quotedBooking.Booking.PickupAgentDocumentaryAddress.OrganisationPK.IsValid
					&& quotedBooking.Booking.PickupAgentDocumentaryAddress.OrganisationPK != ParentQuotedBooking.Booking.PickupAgentDocumentaryAddress.OrganisationPK)
				{
					unmatchedProperties.Add(Res.GetString("d677266f-43e2-47d8-a7a2-d06c3eb6643d", "Pickup Agent"));
				}

				if (quotedBooking.Booking.JS_OH_DeliveryAgent.IsValid
					&& quotedBooking.Booking.JS_OH_DeliveryAgent != ParentQuotedBooking.Booking.JS_OH_DeliveryAgent)
				{
					unmatchedProperties.Add(Res.GetString("52336a80-bd90-4c45-ab63-12281317dbdd", "Delivery Agent"));
				}

				if (quotedBooking.Booking.JS_OH_ExportBroker.IsValid
					&& quotedBooking.Booking.JS_OH_ExportBroker != ParentQuotedBooking.Booking.JS_OH_ExportBroker)
				{
					unmatchedProperties.Add(Res.GetString("3fa90f4a-ad47-48ff-8022-74fa86d420cb", "Export Broker"));
				}

				if (quotedBooking.Booking.JS_OH_ImportBroker.IsValid
					&& quotedBooking.Booking.JS_OH_ImportBroker != ParentQuotedBooking.Booking.JS_OH_ImportBroker)
				{
					unmatchedProperties.Add(Res.GetString("93525aaf-104c-45ba-bc3f-222201e0ef8d", "Import Broker"));
				}

				if (quotedBooking.Booking.DocsAndCartage != null && ParentQuotedBooking.Booking.DocsAndCartage != null
					&& quotedBooking.Booking.DocsAndCartage.PickupCartageCoPK.IsValid
					&& quotedBooking.Booking.DocsAndCartage.PickupCartageCoPK != ParentQuotedBooking.Booking.DocsAndCartage.PickupCartageCoPK)
				{
					unmatchedProperties.Add(Res.GetString("de884d1f-8b65-41d9-915f-a4d159b02cfa", "Port Transport Provider"));
				}

				if (quotedBooking.Booking.BookingPartyDocumentaryAddress.OrganisationPK.IsValid
					&& quotedBooking.Booking.BookingPartyDocumentaryAddress.OrganisationPK != ParentQuotedBooking.Booking.BookingPartyDocumentaryAddress.OrganisationPK)
				{
					unmatchedProperties.Add(Res.GetString("fdbac9a5-1792-4fbb-9f5a-b94a6b3d266e", "Booking Party"));
				}

				if (!quotedBooking.Booking.JS_RL_NKLoadPort.IsEmpty
					&& quotedBooking.Booking.JS_RL_NKLoadPort != ParentQuotedBooking.Booking.JS_RL_NKLoadPort)
				{
					unmatchedProperties.Add(Res.GetString("50e62412-cc7c-8a80-493b-d6d7f8fd2e5e", "Load Port"));
				}

				if (!quotedBooking.Booking.JS_RL_NKDischargePort.IsEmpty
					&& quotedBooking.Booking.JS_RL_NKDischargePort != ParentQuotedBooking.Booking.JS_RL_NKDischargePort)
				{
					unmatchedProperties.Add(Res.GetString("6f71abac-2c05-15bd-4e41-4f6ef8cbb136", "Discharge Port"));
				}
			}

			if (quotedBooking.ExportReceivingDepot.IsValid && quotedBooking.ExportReceivingDepot != ParentQuotedBooking.ExportReceivingDepot)
			{
				unmatchedProperties.Add(Res.GetString("e89624c3-9648-4b1c-a066-dc0f8bf59622", "Pickup CFS/CTO"));
			}

			if (quotedBooking.ImportReleaseDepot.IsValid && quotedBooking.ImportReleaseDepot != ParentQuotedBooking.ImportReleaseDepot)
			{
				unmatchedProperties.Add(Res.GetString("f9fadb59-a511-4af4-8a9a-a61a61b09f4e", "Delivery CFS/CTO"));
			}

			var sailingUnmatchedProperties = GetSailingUnmatchedProperties(quotedBooking);

			if (sailingUnmatchedProperties.Count > 0)
			{
				unmatchedProperties.AddRange(sailingUnmatchedProperties);
			}

			if (unmatchedProperties.Count == 1)
			{
				return Res.GetString("909f5ed8-6286-4c00-bbee-082382bb0cb9", "This field is different from the field on the Main booking and will be deleted when merged with the Main booking: {0}.", unmatchedProperties[0]);
			}
			else if (unmatchedProperties.Count >= 1)
			{
				return Res.GetString("b8c637ce-4d23-49d3-9718-6122d4db15ca", "These fields are different from the fields on the Main booking and will be deleted when merged with the Main booking:{0}.", System.Environment.NewLine + string.Join(", ", unmatchedProperties));
			}

			return ZString.Empty;
		}

		List<ZString> GetSailingUnmatchedProperties(QuotedBooking quotedBooking)
		{
			var unmatchedProperties = new List<ZString>();

			if (quotedBooking.ScheduleChooser.Sailing != null && ParentQuotedBooking.ScheduleChooser.Sailing != null)
			{
				if (!quotedBooking.ScheduleChooser.Sailing.JX_JV_VoyageFlight.IsEmpty
					&& quotedBooking.ScheduleChooser.Sailing.JX_JV_VoyageFlight != ParentQuotedBooking.ScheduleChooser.Sailing.JX_JV_VoyageFlight)
				{
					unmatchedProperties.Add(quotedBooking.ScheduleChooser.GetVoyageNoLabelDependingOnTransportMode());
				}

				if (!quotedBooking.ScheduleChooser.Sailing.JX_JV_NKVessel.IsEmpty
					&& quotedBooking.ScheduleChooser.Sailing.JX_JV_NKVessel != ParentQuotedBooking.ScheduleChooser.Sailing.JX_JV_NKVessel)
				{
					unmatchedProperties.Add(Res.GetString("b632028f-0bdd-4793-8df6-4a1da93dbbe1", "Vessel"));
				}

				if (!quotedBooking.ScheduleChooser.Sailing.JX_JA_RL_NKPortOfLoading.IsEmpty
					&& quotedBooking.ScheduleChooser.Sailing.JX_JA_RL_NKPortOfLoading != ParentQuotedBooking.ScheduleChooser.Sailing.JX_JA_RL_NKPortOfLoading)
				{
					unmatchedProperties.Add(Res.GetString("8aa23542-7194-87a7-4f35-17ec53eb69a9", "Sailing Load Port"));
				}

				if (!quotedBooking.ScheduleChooser.Sailing.JX_JB_RL_NKPortOfDischarge.IsEmpty
					&& quotedBooking.ScheduleChooser.Sailing.JX_JB_RL_NKPortOfDischarge != ParentQuotedBooking.ScheduleChooser.Sailing.JX_JB_RL_NKPortOfDischarge)
				{
					unmatchedProperties.Add(Res.GetString("665403b1-09e8-8c94-44c9-285042f97a98", "Sailing Discharge Port"));
				}
			}

			if (quotedBooking.OH_Carrier.IsValid
				&& quotedBooking.OH_Carrier != ParentQuotedBooking.OH_Carrier)
			{
				unmatchedProperties.Add(Res.GetString("722a99bf-7402-4808-8b9d-10f78a18f97b", "Carrier"));
			}

			if (quotedBooking.Booking != null && ParentQuotedBooking.Booking != null)
			{
				if (quotedBooking.Booking.JS_E_DEP.IsValid
				&& quotedBooking.Booking.JS_E_DEP != ParentQuotedBooking.Booking.JS_E_DEP)
				{
					unmatchedProperties.Add(Res.GetString("9b80edde-e996-4e8e-9724-02bff47f2056", "ETD"));
				}

				if (quotedBooking.Booking.JS_E_ARV.IsValid
					&& quotedBooking.Booking.JS_E_ARV != ParentQuotedBooking.Booking.JS_E_ARV)
				{
					unmatchedProperties.Add(Res.GetString("97213755-d98e-4055-8a1c-ca658ffb1672", "ETA"));
				}
			}

			return unmatchedProperties;
		}

		#endregion
	}
}
