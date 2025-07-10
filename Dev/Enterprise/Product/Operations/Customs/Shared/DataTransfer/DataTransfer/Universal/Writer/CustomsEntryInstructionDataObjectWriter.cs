using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomsEntryInstructionDataObjectWriter : DataObjectWriter<CusEntryInstruction, UniversalCustoms.EntryInstruction>
	{
		public CustomsEntryInstructionDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper) : base(manager)
		{
			this.helper = Argument.NotNull(helper, "UniversalDataObjectWriterHelper helper");
		}
		protected readonly UniversalDataObjectWriterHelper helper;

		protected override UniversalCustoms.EntryInstruction PopulateDataObject(CusEntryInstruction sourceBO)
		{
			var sourcePK = sourceBO.PK;
			var result = new UniversalCustoms.EntryInstruction(writeManager.WriterStrategy)
			{
				Link = helper.AllocateEntryInstructionLink(sourcePK),
				Style = sourceBO.CEI_Style,
				Description = sourceBO.CEI_Description,
				AddInfoCollection = GetEntryInstructionAddInfoCollection(sourceBO),
				AddInfoGroupCollection = GetEntryInstructionAddInfoGroupCollection(sourceBO),
			};
			result.SubStyle = PopulateValue(result.SubStyle, false, () => ListHelper.GetWithDescription<CodeDescriptionPair>(sourceBO.CEI_SubStyle, sourceBO.Lookups.EntrySubStyleList));
			result.MergeBy = PopulateValue(result.MergeBy, false, () => ListHelper.GetWithDescription<CodeDescriptionPair>(sourceBO.CEI_MergeBy, sourceBO.Lookups.MergeByList));
			result.AddOrgAddress(writeManager, sourceBO.Owner, AddressTypes.Owner);
			result.AddOrgAddress(writeManager, sourceBO.BondHolder, AddressTypes.BondHolder);
			result.AddOrgAddress(writeManager, sourceBO.Carrier, DocAddressTypes.Descriptions.Carrier);
			result.AddOrgAddress(writeManager, sourceBO.Warehouse, AddressTypes.Warehouse1);
			result.AddOrgAddress(writeManager, sourceBO.Warehouse2, AddressTypes.Warehouse2);
			PopulateDocAddresses(sourceBO, result);
			result.SetCustomsReferenceCollection(() => CustomsReferenceCollectionCreator.CreateCollection(helper, sourceBO, writeManager));
			result.SetCustomsSupportingInformationCollection(() => sourceBO is Integration.Customs.ICusSupportingInfoTypeSupporter ? GetCustomsSupportingInformationCollection(sourceBO) : null);
			result.DateOfValuation = sourceBO.CEI_DateForDuty;
			result.Procedure = sourceBO.CEI_Procedure;
			return result;
		}

		protected virtual List<UniversalCustoms.CustomsSupportingInformation> GetCustomsSupportingInformationCollection(CusEntryInstruction entryInstructionBO) => CustomsSupportingInformationCollectionCreator.CreateCollection(helper, entryInstructionBO, writeManager, supportedTypes: GetSupportedCusSupportingInfoCSI_Types(entryInstructionBO));
		protected virtual ZString[] GetSupportedCusSupportingInfoCSI_Types(CusEntryInstruction entryInstructionBO) => null;

		protected virtual List<AddInfo> GetEntryInstructionAddInfoCollection(CusEntryInstruction instructionBO)
		{
			return AddInfoCollectionCreator.CreateCollection(instructionBO, CusEntryInstructionSchema.CEI_AddInfo);
		}

		protected virtual List<UniversalCustoms.AddInfoGroup> GetEntryInstructionAddInfoGroupCollection(CusEntryInstruction instructionBO)
		{
			return AddInfoGroupCollectionCreator.CreateCollection(helper, instructionBO, writeManager);
		}

		protected void PopulateDocAddresses(IDocAddresses docAddresses, IOrganizationAddressCollectionParent addressCollectionParent)
		{
			if (docAddresses != null && addressCollectionParent != null)
			{
				addressCollectionParent.AddOrgAddresses(writeManager, docAddresses);
			}
		}
	}
}
