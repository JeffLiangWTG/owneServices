using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using ICusEntryNumber = Enterprise.Integration.Customs.ICusEntryNumber;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class PortReferenceDataObjectWriter : DataObjectWriter<BusinessObject, PortReference>
	{
		public PortReferenceDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{ }

		protected override PortReference PopulateDataObject(BusinessObject additionalReferenceBO)
		{
			var cusEntryNumber = (ICusEntryNumber)additionalReferenceBO;
			var cusEntryNumberBO = additionalReferenceBO;
			return new PortReference()
			{
				Type = ListHelper.GetWithDescription<PortReferenceType>(cusEntryNumber.CE_EntryType, cusEntryNumber.Lookups.AdditionalReferenceNumberTypes),
				Reference = cusEntryNumber.CE_EntryNum,
				Status = new PortReferenceStatus { Code = (ZString)cusEntryNumberBO[CusEntryNumSchema.Constants.CE_EntryStatus] },
				Country = new Country { Code = cusEntryNumber.CE_RN_NKCountryCode },
			};
		}
	}
}
