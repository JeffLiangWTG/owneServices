using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class JobDeclarationEventContextReader : Customs.DataTransfer.Universal.JobDeclarationEventContextReader
	{
		public JobDeclarationEventContextReader(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void AddJobDeclarationContextValuesCore(List<KeyValuePair<TypeWithDescription, IZType>> contextValues, Customs.Business.CusEntryHeader entry)
		{
			base.AddJobDeclarationContextValuesCore(contextValues, entry);
			if (declaration.IsExport)
			{
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.InternalTransactionNumber, declaration.DeclarationNumber);
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.VoyageNumber, declaration.JE_VoyageFlightNo);
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.VesselName, declaration.JE_VesselName);
			}
		}

		protected override void AddAdditionalFieldsToUpdateValuesCore(List<KeyValuePair<IZType, IZType>> contextValues)
		{
			base.AddAdditionalFieldsToUpdateValuesCore(contextValues);

			if (declaration.IsExport && !declaration.DeclarationNumber.IsEmpty)
			{
				contextValues.Add(new KeyValuePair<IZType, IZType>(new ZString("BillOfLading.CustomsEntryNumberType"), new ZString("ITN")));
				contextValues.Add(new KeyValuePair<IZType, IZType>(new ZString("BillOfLading.CustomsEntryNumber"), declaration.DeclarationNumber));
			}
		}
	}
}
