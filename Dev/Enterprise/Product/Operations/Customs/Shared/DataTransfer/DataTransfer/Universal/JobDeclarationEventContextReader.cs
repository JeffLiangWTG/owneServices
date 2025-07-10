using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class JobDeclarationEventContextReader
	{
		public JobDeclarationEventContextReader(BaseJobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, "BaseJobDeclaration declaration");
		}
		protected readonly BaseJobDeclaration declaration;

		internal void AddJobDeclarationContextValues(List<KeyValuePair<TypeWithDescription, IZType>> contextValues, CusEntryHeader entry)
		{
			AddJobDeclarationContextValuesCore(contextValues, entry);
		}

		protected virtual void AddJobDeclarationContextValuesCore(List<KeyValuePair<TypeWithDescription, IZType>> contextValues, CusEntryHeader entry)
		{
			if (declaration.JE_TransportMode == Core.Constants.TransportModes.Sea)
			{
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.CarrierCode, declaration.CarrierCode);
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.CarrierC1CCode, declaration.CarrierC1CCode);
			}

			var helper = new EventContextValuesHelper(declaration.JE_TransportMode == Core.Constants.TransportModes.Air, contextValues);
			helper.AddMasterBillNumberAndPortCodes(declaration.JE_MasterBill, declaration.PortOfLoading, declaration.PortOfArrival);

			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.AgentsReference, declaration.JE_AgentsReference);

			helper.AddHouseBillNumberAndPortCodes(declaration.JE_HouseBill, declaration.Origin, declaration.FinalDestination);

			foreach (var orderNumber in OrderNumbers)
			{
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.OrderNumber, orderNumber);
			}

			declaration.AdditionalReferenceNumbers.AddAdditionalReferences(contextValues);

			if (entry != null)
			{
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.EntryNumber, entry.EntryNumber);
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.EntryReference, entry.CH_BGMReference);
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.EntryType, entry.CH_MessageType);
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.EntryCountry, entry.CountryCode);
			}
		}

		internal void AddAdditionalFieldsToUpdateValues(List<KeyValuePair<IZType, IZType>> contextValues)
		{
			AddAdditionalFieldsToUpdateValuesCore(contextValues);
		}

		protected virtual void AddAdditionalFieldsToUpdateValuesCore(List<KeyValuePair<IZType, IZType>> contextValues)
		{
		}

		IEnumerable<ZString> OrderNumbers
		{
			get
			{
				foreach (var order in declaration.AttachedOrders)
				{
					if (!order.JD_OrderNumber.IsEmpty)
					{
						yield return order.JD_OrderNumber;
					}
				}
			}
		}
	}
}
