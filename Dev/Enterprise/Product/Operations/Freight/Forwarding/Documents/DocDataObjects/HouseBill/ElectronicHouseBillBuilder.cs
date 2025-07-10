using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ElectronicHouseBillBuilder
	{
		public ElectronicHouseBillBuilder(ForwardingShipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			this.context = new CommonContext(shipment.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingShipment shipment;
		readonly IContext context;

		public HouseBill Build(HouseBill houseBill)
		{
			houseBill = houseBill ?? new HouseBill(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef);

			houseBill.IsElectronicBOL = true;
			houseBill.ElectronicBillOfLadingVersion = shipment.JS_ElectronicBillOfLadingVersion;

			houseBill.BillTerms = new CodeDescription(shipment.Lookups.JS_ElectronicBillOfLadingTerms_List)
			{
				Code = shipment.JS_ElectronicBillOfLadingTerms
			};

			houseBill.BillType = new CodeDescription(shipment.Lookups.JS_ElectronicBillOfLadingType_List)
			{
				Code = shipment.JS_ElectronicBillOfLadingType
			};

			PopulateAddresses(houseBill);
			PopulateAmendmentRequestID(houseBill);

			CheckElectronicBOLMinimumRequirements(houseBill);

			return houseBill;
		}

		#region PopulateAddresses

		void PopulateAddresses(HouseBill houseBill)
		{
			houseBill.ElectronicBillOfLadingShipper = AddressBuilder.Create(context, shipment.ShipperDocAddress);
			houseBill.ElectronicBillOfLadingConsignee = AddressBuilder.Create(context, shipment.JS_ElectronicBillOfLadingConsigneeDocAddress);
			houseBill.ElectronicBillOfLadingToOrder = AddressBuilder.Create(context, shipment.JS_ElectronicBillOfLadingToOrderDocAddress);
			houseBill.CurrentUser = AddressBuilder.CreateForCurrentUser(context);
			houseBill.Holder = AddressBuilder.Create(context, shipment.HolderDocAddress);
			houseBill.SurrenderParty = AddressBuilder.Create(context, shipment.SurrenderPartyDocAddress);
		}

		#endregion

		#region PopulateAmendmentRequestID

		void PopulateAmendmentRequestID(HouseBill houseBill)
		{
			if (shipment.JS_ElectronicBillOfLadingStatus == FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress)
			{
				var latestBillStatusUpdatedLog = shipment.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.BillStatusUpdated)
					.FirstOrDefault(log => log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var type) && type == Core.Constants.BillStatusUpdatedTypes.AmendmentRequested);

				houseBill.AmendmentRequestID = latestBillStatusUpdatedLog != null
						&& latestBillStatusUpdatedLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.RequestNumber, out var requestNumber)
					? requestNumber
					: ZString.Empty;
			}
		}

		#endregion

		#region CheckElectronicBOLMinimumRequirements

		void CheckElectronicBOLMinimumRequirements(HouseBill houseBill)
		{
			if (!shipment.CheckElectronicBOLMinimumRequirements())
			{
				houseBill.ErrorPlaceHolderInfo.AddMessageError(() => true, string.Join(System.Environment.NewLine, GetElectronicBOLMinimumRequirementsValidationErrors()));
				houseBill.Validate(nameof(houseBill.ErrorPlaceHolder));
			}
		}

		IEnumerable<string> GetElectronicBOLMinimumRequirementsValidationErrors()
		{
			var errors = new List<string>()
			{
				(NoResString)"There are validation errors on this form due to missing mandatory information. Please correct these errors before publishing."
			};

			GetValidationErrors(shipment.JS_ElectronicBillOfLadingTypeInfo, (NoResString)"Bill Type", errors);
			GetValidationErrors(shipment.JS_ElectronicBillOfLadingTermsInfo, (NoResString)"Bill Terms", errors);
			GetValidationErrors(shipment.HolderDocAddress?.OrganisationPKInfo, (NoResString)"First Holder", errors);
			GetValidationErrors(shipment.ShipperDocAddress?.OrganisationPKInfo, (NoResString)"Shipper", errors);
			GetValidationErrors(shipment.JS_ElectronicBillOfLadingConsigneeDocAddress?.OrganisationPKInfo, (NoResString)"Consignee", errors);
			GetValidationErrors(shipment.JS_ElectronicBillOfLadingToOrderDocAddress?.OrganisationPKInfo, (NoResString)"To Order", errors);
			GetValidationErrors(shipment.SurrenderPartyDocAddress?.OrganisationPKInfo, (NoResString)"Surrender Party", errors);

			return errors.Distinct();
		}

		void GetValidationErrors(ZPropertyInfo propertyInfo, ZString propertyName, List<string> errors)
		{
			if (propertyInfo == null)
			{
				return;
			}

			if (propertyInfo.HasErrors())
			{
				errors.AddRange(propertyInfo.GetErrors().Where(x => !string.IsNullOrEmpty(x.Message)).Select(x => $"{propertyName}: {x.Message.Replace(System.Environment.NewLine, " ")}"));
			}

			if (propertyInfo.HasMessageErrors())
			{
				errors.AddRange(propertyInfo.GetMessageErrors().Where(x => !string.IsNullOrEmpty(x.Message)).Select(x => $"{propertyName}: {x.Message.Replace(System.Environment.NewLine, " ")}"));
			}
		}

		#endregion
	}
}
