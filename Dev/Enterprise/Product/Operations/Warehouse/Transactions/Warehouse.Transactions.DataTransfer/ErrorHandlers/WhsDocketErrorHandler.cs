using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public abstract class WhsDocketErrorHandler<TBusinessObject> : IErrorHandler
		where TBusinessObject : WhsDocket
	{
		#region Public Methods

		public void SetDocketTypeAndLogDataErrors(TBusinessObject docket, Xsd.WhsDocket value)
		{
			var savedStatus = docket.WD_DocketStatus;

			using (((IBusinessObjectInternals)docket).ResumeValidationForAllDescendantsTemporarily())
			{
				docket.RunPreSaveValidation();
			}

			if (savedStatus != DocketStatus.Codes.Error || docket.WD_DocketStatus != DocketStatus.Codes.Entered)
			{
				docket.WD_DocketStatus = savedStatus;
			}

			if (DocketDataHasErrors(docket, value))
			{
				docket.WD_DocketStatus = DocketStatus.Codes.Error;
			}
		}

		public void SetDocketTypeAndLogDataErrorsFromLine(TBusinessObject docket, WhsDocketLine line, Xsd.WhsDocketLine value)
		{
			using (((IBusinessObjectInternals)line).ResumeValidationForAllDescendantsTemporarily())
			{
				line.RunPreSaveValidation();
			}

			if (DocketLineDataHasErrors(docket, line, value))
			{
				docket.WD_DocketStatus = DocketStatus.Codes.Error;
			}
		}

		#endregion

		#region Implementation

		#region Docket Line

		protected virtual bool DocketLineDataHasErrors(TBusinessObject docket, WhsDocketLine line, Xsd.WhsDocketLine value)
		{
			bool lineHasErrors = BizObjHasErrors(line);

			if (ProductHasErrors(docket, line, value))
			{
				lineHasErrors = true;
			}

			if (WE_PackQuantityHasErrors(docket, line, value))
			{
				lineHasErrors = true;
			}

			if (WE_LineCommentHasErrors(docket, line, value))
			{
				lineHasErrors = true;
			}

			if (WE_PackingDateHasErrors(docket, line, value))
			{
				lineHasErrors = true;
			}

			if (WE_BondedEntryKey(docket, line, value))
			{
				lineHasErrors = true;
			}

			if (WE_ExpiryDateHasErrors(docket, line, value))
			{
				lineHasErrors = true;
			}

			if (WE_PartAttrib1HasErrors(docket, line, value))
			{
				lineHasErrors = true;
			}

			if (WE_PartAttrib2HasErrors(docket, line, value))
			{
				lineHasErrors = true;
			}

			if (WE_PartAttrib3HasErrors(docket, line, value))
			{
				lineHasErrors = true;
			}

			return lineHasErrors;
		}

		protected virtual bool WE_PartAttrib3HasErrors(TBusinessObject docket, WhsDocketLine line, Xsd.WhsDocketLine value)
		{
			bool result = true;
			if (TrimmedEquals(line.WE_PartAttrib3, value.LineAttributes.PartAttribute3))
			{
				result = false;
			}
			if (result)
			{
				AddEventLog(docket, GetLineErrorPrefix(line) + (NoResString)" Error (Part Attr3): " + value.LineAttributes.PartAttribute3); // Developer only string
			}
			return result;
		}

		protected virtual bool WE_PartAttrib2HasErrors(TBusinessObject docket, WhsDocketLine line, Xsd.WhsDocketLine value)
		{
			bool result = true;
			if (TrimmedEquals(line.WE_PartAttrib2, value.LineAttributes.PartAttribute2))
			{
				result = false;
			}
			if (result)
			{
				AddEventLog(docket, GetLineErrorPrefix(line) + (NoResString)" Error (Part Attr2): " + value.LineAttributes.PartAttribute2); // Developer only string
			}
			return result;
		}

		protected virtual bool WE_PartAttrib1HasErrors(TBusinessObject docket, WhsDocketLine line, Xsd.WhsDocketLine value)
		{
			bool result = true;
			if (TrimmedEquals(line.WE_PartAttrib1, value.LineAttributes.PartAttribute1))
			{
				result = false;
			}
			if (result)
			{
				AddEventLog(docket, GetLineErrorPrefix(line) + (NoResString)" Error (Part Attr1): " + value.LineAttributes.PartAttribute1); // Developer only string
			}
			return result;
		}

		protected virtual bool WE_ExpiryDateHasErrors(TBusinessObject docket, WhsDocketLine line, Xsd.WhsDocketLine value)
		{
			bool result = true;
			if (line.WE_ExpiryDate.IsEmpty && value.LineAttributes.ExpiryDate.IsEmpty)
			{
				result = false;
			}
			if (!line.WE_ExpiryDate.IsEmpty && !value.LineAttributes.ExpiryDate.IsEmpty)
			{
				if (line.WE_ExpiryDate == value.LineAttributes.ExpiryDate)
				{
					result = false;
				}
			}
			if (result)
			{
				AddEventLog(docket, GetLineErrorPrefix(line) + (NoResString)" Error (Expiry Date): " + GetZDateValueForError(value.LineAttributes.ExpiryDate)); // Developer only string
			}
			return result;
		}

		protected virtual bool WE_BondedEntryKey(TBusinessObject docket, WhsDocketLine line, Xsd.WhsDocketLine value)
		{
			bool result = true;
			if (TrimmedEquals(line.WE_BondedEntryKey, value.LineAttributes.BondedEntryKey))
			{
				result = false;
			}
			if (result)
			{
				AddEventLog(docket, GetLineErrorPrefix(line) + (NoResString)" Error (Entry Key): " + value.LineAttributes.BondedEntryKey); // Developer only string
			}
			return result;
		}

		protected virtual bool WE_PackingDateHasErrors(TBusinessObject docket, WhsDocketLine line, Xsd.WhsDocketLine value)
		{
			bool result = true;
			if (line.WE_PackingDate.IsEmpty && value.LineAttributes.PackingDate.IsEmpty)
			{
				result = false;
			}
			if (!line.WE_PackingDate.IsEmpty && !value.LineAttributes.PackingDate.IsEmpty)
			{
				if (line.WE_PackingDate == value.LineAttributes.PackingDate)
				{
					result = false;
				}
			}
			if (result)
			{
				AddEventLog(docket, GetLineErrorPrefix(line) + (NoResString)" Error (Packing Date): " + GetZDateValueForError(value.LineAttributes.PackingDate)); // Developer only string
			}
			return result;
		}

		protected virtual bool WE_LineCommentHasErrors(TBusinessObject docket, WhsDocketLine line, Xsd.WhsDocketLine value)
		{
			bool result = true;
			if (TrimmedEquals(line.WE_LineComment, value.LineComments))
			{
				result = false;
			}
			if (result)
			{
				AddEventLog(docket, GetLineErrorPrefix(line) + (NoResString)" Error (Comment): " + value.LineComments); // Developer only string
			}
			return result;
		}

		protected ZString GetLineErrorPrefix(WhsDocketLine line)
		{
			return (NoResString)"Line " + line.WE_LineNo + (NoResString)"." + line.WE_SubLineNo; // Developer only string
		}

		protected ZString GetZDateValueForError(ZDate date)
		{
			return (date.IsEmpty ? (NoResString)"Empty" : date.ToShortDateString()); // Developer only string
		}

		protected ZString GetZDateValueForError(ZDateTime date)
		{
			return (date.IsEmpty ? (NoResString)"Empty" : date.ToShortDateString() + " " + date.ToShortTimeString()); // Developer only string
		}

		protected ZString GetZDateValueForError(ZDateTimeOffset date)
		{
			return (date.IsEmpty ? (NoResString)"Empty" : date.ToShortDateString()); // Developer only string
		}

		protected virtual bool WE_PackQuantityHasErrors(TBusinessObject docket, WhsDocketLine line, Xsd.WhsDocketLine value)
		{
			bool result = true;
			ZDecimal expectedUnits = (value.QuantityFromClientOrder == 0 ? value.QuantityActuallyOrdered : value.QuantityFromClientOrder);
			if (line.WE_PackQuantity == expectedUnits)
			{
				result = false;
			}
			if (result)
			{
				AddEventLog(docket, GetLineErrorPrefix(line) + (NoResString)" Error (Qty): " + expectedUnits); // Developer only string
			}
			return result;
		}

		protected virtual bool ProductHasErrors(TBusinessObject docket, WhsDocketLine line, Xsd.WhsDocketLine value)
		{
			bool result = true;
			if (line.SupplierPart != null)
			{
				if (TrimmedEquals(line.SupplierPart.OP_PartNum, value.Product))
				{
					result = false;
				}
			}
			if (result)
			{
				AddEventLog(docket, GetLineErrorPrefix(line) + (NoResString)" Error (Product): " + value.Product); // Developer only string
			}
			return result;
		}

		#endregion

		#region Docket

		#region DocketDataHasErrors

		protected virtual bool DocketDataHasErrors(TBusinessObject docket, Xsd.WhsDocket value)
		{
			bool docketHasErrors = BizObjHasErrors(docket);

			if (docket.References.HasErrors())
			{
				docketHasErrors = true;
			}

			if (WD_ExternalReferenceHasErrors(docket, value))
			{
				docketHasErrors = true;
			}

			if (WD_CustomerReferenceHasErrors(docket, value))
			{
				docketHasErrors = true;
			}

			if (WD_TransportReferenceHasErrors(docket, value))
			{
				docketHasErrors = true;
			}

			if (TransportServiceLevelHasErrors(docket, value))
			{
				docketHasErrors = true;
			}

			if (ServiceLevelHasErrors(docket, value))
			{
				docketHasErrors = true;
			}

			if (TransportCompanyHasErrors(docket, value))
			{
				docketHasErrors = true;
			}

			if (TransportBilledToHasErrors(docket, value))
			{
				docketHasErrors = true;
			}

			if (ClientHasErrors(docket, value))
			{
				docketHasErrors = true;
			}

			return docketHasErrors;
		}

		#endregion

		#region WD_ExternalReferenceHasErrors

		protected virtual bool WD_ExternalReferenceHasErrors(TBusinessObject docket, Xsd.WhsDocket value)
		{
			bool result = true;
			if (TrimmedEquals(docket.WD_ExternalReference, value.Identifier.Reference))
			{
				result = false;
			}
			if (result)
			{
				AddEventLog(docket, (NoResString)"Error (" + docket.HumanReadableName + (NoResString)" Reference): " + value.Identifier.Reference); // Developer only string
			}
			return result;
		}

		#endregion

		#region WD_TransportReferenceHasErrors

		protected virtual bool WD_TransportReferenceHasErrors(TBusinessObject docket, Xsd.WhsDocket value)
		{
			bool result = true;
			if (TrimmedEquals(docket.WD_TransportReference, value.DocketDetail.TransportReference))
			{
				result = false;
			}
			if (result)
			{
				AddEventLog(docket, (NoResString)"Error (Transport Reference): " + value.DocketDetail.TransportReference); // Developer only string
			}
			return result;
		}

		#endregion

		#region WD_CustomerReferenceHasErrors

		protected virtual bool WD_CustomerReferenceHasErrors(TBusinessObject docket, Xsd.WhsDocket value)
		{
			bool result = true;

			if (TrimmedEquals(docket.WD_CustomerReference, value.DocketDetail.CustomerReference))
			{
				result = false;
			}
			if (result)
			{
				AddEventLog(docket, (NoResString)"Error (Customer Reference): " + value.DocketDetail.CustomerReference); // Developer only string
			}
			return result;
		}

		#endregion

		#region Service Level Has Errors

		#region TransportServiceLevelHasErrors

		protected virtual bool TransportServiceLevelHasErrors(TBusinessObject docket, Xsd.WhsDocket value)
		{
			return ServiceLevelHasErrorsCore(docket, value.DocketDetail.TransportServiceLevel,
				docket.WD_PL_NKCarrierServiceLevel, (NoResString)"Error (Transport Service Level): "); // Developer only string
		}

		#endregion

		#region ServiceLevelHasErrors

		protected virtual bool ServiceLevelHasErrors(TBusinessObject docket, Xsd.WhsDocket value)
		{
			return ServiceLevelHasErrorsCore(docket, value.DocketDetail.ServiceLevel,
				docket.WD_RS_NKServiceLevel, (NoResString)"Error (Service Level): "); // Developer only string
		}

		#endregion

		#region ServiceLevelHasErrorsCore

		bool ServiceLevelHasErrorsCore(TBusinessObject docket, ZString docketDetailServiceLevel, ZString serviceLevelValue, string description)
		{
			var hasError = false;

			if (!docketDetailServiceLevel.IsEmpty &&
				(!TrimmedEquals(serviceLevelValue, docketDetailServiceLevel)))
			{
				hasError = true;
				AddEventLog(docket, description + docketDetailServiceLevel);
			}

			return hasError;
		}

		#endregion

		#endregion

		#region TransportCompanyHasErrors

		protected virtual bool TransportCompanyHasErrors(TBusinessObject docket, Xsd.WhsDocket value)
		{
			return docket is IJobWithTransportCompany jobWithTransportCompany
				&& OrganizationHasErrors(docket, jobWithTransportCompany.TransportCoDocAddress.Organisation, value.DocketDetail.TransportCompany.AddressReference.Organisation, (NoResString)"Transport Company"); // Developer only string
		}

		#endregion

		#region TransportBilledToHasErrors

		protected virtual bool TransportBilledToHasErrors(TBusinessObject docket, Xsd.WhsDocket value)
		{
			return OrganizationHasErrors(docket, docket.TransportBillToDocAddress.Organisation, value.DocketDetail.TransportBilledTo.AddressReference.Organisation, (NoResString)"Transport Billed To"); // Developer only string
		}

		#endregion

		#region ClientHasErrors

		protected virtual bool ClientHasErrors(TBusinessObject docket, Xsd.WhsDocket value)
		{
			return OrganizationHasErrors(docket, docket.Client, value.Identifier.Client, (NoResString)"Client"); // Developer only string
		}

		#endregion

		#endregion

		protected bool BizObjHasErrors(BusinessObject bO)
		{
			return bO.Notifications.HasErrors() || bO.Notifications.HasMessageErrors();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SL_Reference is only English")]
		protected bool OrganizationHasErrors(TBusinessObject docket, OrgHeader org, Xsd.Organisation xsdOrg, ZString fieldLabel)
		{
			var result = true;
			if (org != null)
			{
				result = false;
			}
			else
			{
				if (xsdOrg == null)
				{
					result = false;
				}
				else
				{
					if (xsdOrg.OwnerCode.Trim().IsEmpty)
					{
						result = false;
					}
				}
			}
			if (result)
			{
				if (xsdOrg != null)
				{
					var prefix = "Error (";
					var suffix = "): " + xsdOrg.OwnerCode;
					var field = fieldLabel.SubstringSafe(0, StmALog.Schema.SL_ReferenceMaxLength - prefix.Length - suffix.Length);

					AddEventLog(docket, prefix + field + suffix);
				}
			}
			return result;
		}

		protected virtual StmALog AddEventLog(TBusinessObject docket, ZString eventReference)
		{
			var reference = EventLogReferenceBuilder.New()
				.AddShortenable(eventReference)
				.Build();
			return docket.Logs.AddNew(Events.DataImport, reference);
		}

		bool TrimmedEquals(ZString value1, ZString value2) => value1.Trim() == value2.Trim();

		#endregion

		#region IErrorHandler Members

		void IErrorHandler.SetDocketTypeAndLogDataErrors(BusinessObject docket, Enterprise.DataTransfer.Xml.IValueObject value)
		{
			SetDocketTypeAndLogDataErrors((TBusinessObject)docket, (Xsd.WhsDocket)value);
		}

		void IErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(BusinessObject docket, BusinessObject line, Enterprise.DataTransfer.Xml.IValueObject value)
		{
			this.SetDocketTypeAndLogDataErrorsFromLine((TBusinessObject)docket, (WhsDocketLine)line, (Xsd.WhsDocketLine)value);
		}

		#endregion
	}
}
