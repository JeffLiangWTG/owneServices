using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomsEntryInstructionDataObjectReader : DataObjectReader<EntryInstruction, CusEntryInstruction>, IOrganisationDataObjectReaderSupporter
	{
		public CustomsEntryInstructionDataObjectReader(EntryInstruction entryInstructionDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, UniversalObjectFactory factory, BaseJobDeclaration declaration)
			: base(entryInstructionDataObject, logger, factory)
		{
			this.helper = Argument.NotNull(helper, "helper");
			this.declaration = Argument.NotNull(declaration, "declaration");
		}
		protected readonly UniversalDataObjectReaderHelper helper;
		protected readonly BaseJobDeclaration declaration;

		protected override CusEntryInstruction GetExistingBusinessObject()
		{
			var code = dataObject.Style ?? ZString.Empty;
			var description = dataObject.Description ?? ZString.Empty;
			var provider = declaration.CustomsEntryInstructionProvider;
			return provider.CustomsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault(
				x => string.Equals(x.CEI_Style, code, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(x.CEI_Description, description, StringComparison.OrdinalIgnoreCase)
				);
		}

		protected override CusEntryInstruction GetNewBusinessObject()
		{
			return declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		}

		protected sealed override void PopulateBusinessObject(CusEntryInstruction targetBO)
		{
			suspendSetterDisposable = SuspendSetters(targetBO);
			var setterSuspender = targetBO.SetterSuspender;
			var instructionRow = GetColumnIndexer(targetBO);
			SetValue(instructionRow, CusEntryInstructionSchema.CEI_MergeBy, dataObject.MergeBy);
			SetValue(instructionRow, CusEntryInstructionSchema.CEI_Description, dataObject.Description);
			using (setterSuspender.ResumeSetting(nameof(targetBO.CEI_Style)))
			{
				SetValue(instructionRow, CusEntryInstructionSchema.CEI_Style, dataObject.Style);
			}
			using (setterSuspender.ResumeSetting(nameof(targetBO.CEI_SubStyle)))
			{
				SetValue(instructionRow, CusEntryInstructionSchema.CEI_SubStyle, dataObject.SubStyle);
			}
			SetValue(instructionRow, CusEntryInstructionSchema.CEI_DateForDuty, dataObject.DateOfValuation.GetValueOrDefault(ZDateTime.Empty));
			SetValue(instructionRow, CusEntryInstructionSchema.CEI_Procedure, dataObject.Procedure);

			var delaySetters = IsDefaultingEnabled ? new Dictionary<string, ValueSetter>() : null;
			PopulateAddInfo(targetBO, instructionRow, delaySetters);

			var collectionParent = dataObject as IOrganizationAddressCollectionParent;
			if (collectionParent.OrganizationAddressCollection != null)
			{
				SetValue(instructionRow, CusEntryInstructionSchema.CEI_OH_BondHolder, helper.GetOrganisationPK(this, collectionParent, targetBO, AddressTypes.BondHolder, OrganisationTypes.None), delaySetters);
				SetValue(instructionRow, CusEntryInstructionSchema.CEI_OH_Carrier, helper.GetOrganisationPK(this, collectionParent, targetBO, DocAddressTypes.Descriptions.Carrier.ToString(), OrganisationTypes.Carrier), delaySetters);
				SetValue(instructionRow, CusEntryInstructionSchema.CEI_OH_Owner, helper.GetOrganisationPK(this, collectionParent, targetBO, AddressTypes.Owner, OrganisationTypes.None), delaySetters);
				SetValue(instructionRow, CusEntryInstructionSchema.CEI_OA_Warehouse, helper.GetAddressPK(this, collectionParent, targetBO, AddressTypes.Warehouse1, OrganisationTypes.WarehouseClient), delaySetters);
				SetValue(instructionRow, CusEntryInstructionSchema.CEI_OA_Warehouse2, helper.GetAddressPK(this, collectionParent, targetBO, AddressTypes.Warehouse2, OrganisationTypes.WarehouseClient), delaySetters);
				helper.FillDocAddresses(targetBO, collectionParent.OrganizationAddressCollection, logger);
			}

			FillAddInfoGroups(targetBO);
			var targetBOPK = targetBO.PK;
			var isTargetInDatabase = targetBO.IsInDatabase;
			FillCustomsReferences(targetBOPK, isTargetInDatabase);
			if (helper.IsSourceAndTargetCountrySame && targetBO is Integration.Customs.ICusSupportingInfoTypeSupporter)
			{
				new CustomsSupportingInformationCollectionDataObjectReader(logger, helper).ReadIntoDataRows(targetBOPK, CusEntryInstructionSchema.Constants.Prefix, isTargetInDatabase, dataObject, GetSupportedCusSupportingInfoCSI_Types(targetBO), GetMatchingCusSupportingInfo());
			}
			FillCountrySpecificDetails(targetBO);

			if (delaySetters != null)
			{
				delaySetters.SetValueOnSetterSupenderParentInSpecificOrder(setterSuspender, helper.GetMatchingKeysInSettingOrder(instructionRow));
			}
			var link = dataObject.Link;
			if (link.HasValue)
			{
				helper.RegisterEntryInstructionPK(link.Value, targetBO.PK);
			}
		}

		protected virtual ZString[] GetSupportedCusSupportingInfoCSI_Types(CusEntryInstruction targetBO) => null;

		protected virtual CustomsSupportingInformationCollectionDataObjectReader.GetMatchingDataPredicate GetMatchingCusSupportingInfo() => null;

		public IEnumerable<IDisposable> GetSuspendSetterDisposables()
		{
			yield return suspendSetterDisposable;
		}
		IDisposable suspendSetterDisposable;

		IDisposable SuspendSetters(CusEntryInstruction entryInstruction)
		{
			return entryInstruction.SetterSuspender.SuspendSetting(GetEntryInstructionPropertiesToSuspendSetting().ToArray());
		}

		protected virtual IEnumerable<ZString> GetEntryInstructionPropertiesToSuspendSetting()
		{
			if (dataObject.Style.HasValue)
			{
				yield return CusEntryInstruction.Schema.CEI_Style;
			}
			if (dataObject.SubStyle != null)
			{
				yield return CusEntryInstruction.Schema.CEI_SubStyle;
			}
			if (dataObject.OrganizationAddressCollection.FirstOrDefault(AddressTypes.Warehouse1) != null)
			{
				yield return CusEntryInstruction.Schema.CEI_OA_Warehouse;
			}
			if (dataObject.OrganizationAddressCollection.FirstOrDefault(AddressTypes.Warehouse2) != null)
			{
				yield return CusEntryInstruction.Schema.CEI_OA_Warehouse2;
			}
		}

		protected IDictionary<ZString, List<CustomsReference>> CustomsReferenceGroupByType => dataObject.CustomsReferenceCollection.GetOrCreateCodeDictionary(ref customsReferenceGroupByType, x => x.Type);
		IDictionary<ZString, List<CustomsReference>> customsReferenceGroupByType;

		protected IDictionary<ZString, List<CustomsSupportingInformation>> CustomsSupportingInformationGroupByCategory => dataObject.CustomsSupportingInformationCollection.GetOrCreateCodeDictionary(ref customsSupportingInformationGroupByCategory, x => x.Category);
		IDictionary<ZString, List<CustomsSupportingInformation>> customsSupportingInformationGroupByCategory;

		void PopulateAddInfo(CusEntryInstruction targetBO, IColumnIndexer instructionRow, Dictionary<string, ValueSetter> delaySetters)
		{
			var addInfoManager = targetBO as IAddInfoManager;
			if (addInfoManager != null)
			{
				var isAddInfoSerialisationEnabled = addInfoManager != null && !IsDefaultingEnabled;

				if (isAddInfoSerialisationEnabled)
				{
					addInfoManager.SetUpdateFromAddInfoSerialisationFlag(false);
				}

				try
				{
					if (helper.IsSourceAndTargetCountrySame)
					{
						var addInfoReader = AddInfoDataObjectReader.New(targetBO, logger, helper, CusEntryInstructionSchema.CEI_AddInfo);
						addInfoReader.ReadIntoRow(addInfoManager, instructionRow, dataObject, delaySetters);
					}
				}
				finally
				{
					if (isAddInfoSerialisationEnabled)
					{
						addInfoManager.UpdateAddInfoFromString(instructionRow.GetValue(CusEntryInstructionSchema.CEI_AddInfo));
						addInfoManager.SetUpdateFromAddInfoSerialisationFlag(true);
					}
				}
			}
		}

		protected virtual void FillCustomsReferences(ZGuid entryInstructionPK, bool entryInstructionIsInDatabase)
		{
			new CustomsReferenceCollectionDataObjectReader(logger, helper).ReadIntoDataRows(entryInstructionPK, CusEntryInstructionSchema.Constants.Prefix, entryInstructionIsInDatabase, dataObject);
		}

		protected virtual void FillCountrySpecificDetails(CusEntryInstruction targetBO)
		{
		}

		void FillAddInfoGroups(CusEntryInstruction targetBO)
		{
			new AddInfoGroupCollectionDataObjectReader(logger, helper).ReadIntoDataRows(targetBO.PK, CusEntryInstructionSchema.Constants.Prefix, targetBO.IsInDatabase, dataObject);
		}

		#region IOrganisationDataObjectReaderSupporter

		OrganisationDataObjectReader IOrganisationDataObjectReaderSupporter.CreateNewReader(OrganizationAddress addressData)
		{
			return new OrganisationDataObjectReader(addressData, logger, factory);
		}

		#endregion

	}
}
