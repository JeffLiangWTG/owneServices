using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class WarehouseEntryDataEventContextCreator
	{
		public WarehouseEntryDataEventContextCreator(CusEntryHeader entry)
		{
			this.entry = Argument.NotNull(entry, "entry");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void AddCusEntryHeaderContextValues(List<KeyValuePair<TypeWithDescription, IZType>> values)
		{
			AddCusEntryHeaderContextValuesCore(values);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected void AddJobDeclarationEventContext(List<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			var declaration = entry.Declaration;
			if (declaration != null)
			{
				GetCountrySpecificJobDeclarationEventContextReader(declaration)?.AddJobDeclarationContextValues(contextValues, null);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected JobDeclarationEventContextReader GetCountrySpecificJobDeclarationEventContextReader(BaseJobDeclaration declaration)
		{
			return ObjectFactory.GetCountrySpecificOrDefault<JobDeclarationEventContextReader>(declaration.CountryCode, declaration);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual void AddCusEntryHeaderContextValuesCore(List<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			AddJobDeclarationEventContext(contextValues);
			AddCusEntryNumberContextValues(contextValues);
			contextValues.AddOrUpdateIfNotEmpty(UniversalEvent.ContextTypes.DeclarationReference, entry.CH_BGMReference);
			contextValues.AddOrUpdateIfNotEmpty(UniversalEvent.ContextTypes.MessageType, entry.CH_MessageType);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual void AddCusEntryNumberContextValues(List<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			var cusEntryNumber = entry.CusEntryNumber;
			if (cusEntryNumber != null)
			{
				contextValues.AddOrUpdateIfNotEmpty(UniversalEvent.ContextTypes.EntryNumber, cusEntryNumber.CE_EntryNum);
				contextValues.AddOrUpdateIfNotEmpty(UniversalEvent.ContextTypes.EntryNumberCountryOfIssue, cusEntryNumber.CE_RN_NKCountryCode);
				contextValues.AddOrUpdateIfNotEmpty(UniversalEvent.ContextTypes.EntryNumberType, cusEntryNumber.CE_EntryType);
			}
		}

		internal void AddAdditionalFieldsToUpdateValues(List<KeyValuePair<IZType, IZType>> contextValues)
		{
			AddAdditionalFieldsToUpdateValuesCore(contextValues);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual void AddAdditionalFieldsToUpdateValuesCore(List<KeyValuePair<IZType, IZType>> contextValues)
		{
		}

		protected readonly CusEntryHeader entry;
	}
}
