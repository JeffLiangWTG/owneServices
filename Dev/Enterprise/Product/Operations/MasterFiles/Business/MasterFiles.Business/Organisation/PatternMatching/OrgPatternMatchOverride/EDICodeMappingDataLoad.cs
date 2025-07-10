using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class EDICodeMappingDataToLoad
	{
		public EDICodeMappingDataToLoad() { }

		public ZString ForeignCode;
		public ZString Relationship;
		public ZString EdiEnterpriseCode;

		public bool IsEmpty
		{
			get { return ForeignCode.IsEmpty || Relationship.IsEmpty || EdiEnterpriseCode.IsEmpty; }
		}
	}

	public class EDICodeMappingDataLoader : DataLoadWithFlexibleColumns
	{
		public EDICodeMappingDataLoader(OrgHeader orgProxy)
		{
			this.orgProxy = orgProxy;
		}

		#region ImportFrom .csv file

		protected override void ProcessDataForThisLine(OCsvLine line)
		{
			try
			{
				CheckElementCount(line);
				EDICodeMappingDataToLoad data = NewCodeMappingToLoad();
				PopulateCodeMappingDataToLoad(line, data);
				ProcessCodeMapping(data);
			}
			catch (ArgumentException ex)
			{
				RunCounters.RecsExcluded++;
				DisplayLogMessage(Res.GetString("628879ee-9977-4a43-880f-da146e2426c9", "Row {0} excluded... data is inconsistent with required format.", RunCounters.CurrentRow.ToString()));
				DisplayLogMessage(ex.Message);
			}
			OnProgressChanged();
		}

		EDICodeMappingDataToLoad NewCodeMappingToLoad()
		{
			return new EDICodeMappingDataToLoad();
		}

		OrgPatternMatchOverride ProcessCodeMapping(EDICodeMappingDataToLoad line)
		{
			OrgPatternMatchOverride result = null;
			try
			{
				ProcessCodeMappingCore(line);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				RunCounters.RecsExcluded++;
				DisplayFormattedLogMessage(line.ForeignCode, line.Relationship, line.EdiEnterpriseCode, ex.Message);
			}

			return result;
		}

		#endregion

		#region LoadValues

		OrgPatternMatchOverride ProcessCodeMappingCore(EDICodeMappingDataToLoad line)
		{
			OrgPatternMatchOverride result = null;
			if (IsValidLine(line))
			{
				ZGuid localGuid = GetLocalGuid(line.Relationship, line.EdiEnterpriseCode);
				ZString localCode = GetLocalCode(line.Relationship, line.EdiEnterpriseCode);

				if (!HasMapping(line, localGuid, localCode))
				{
					result = Factory.New<OrgPatternMatchOverride>();
					using (result.GetValidationSuspender())
					using (result.SuspendSettingHasChanges())
					{
						result.OO_ForeignCode = line.ForeignCode;
						result.OO_Relationship = line.Relationship;
						result.OO_OH = orgProxy.PK;
						result.OO_LocalGuid = localGuid;
						result.OO_LocalCode = localCode;
					}
					result.RunPreSaveValidation();

					if (!result.HasErrors)
					{
						RunCounters.RecsCreated++;
					}
					else
					{
						result.Delete();
						RunCounters.RecsExcluded++;
					}
				}
			}

			if (result == null)
			{
				RunCounters.RecsExcluded++;
			}

			UpdateAndDisplayIfRequired(result != null ? result.PK.ToGuid() : Guid.Empty, OrgPatternMatchOverrideSchema.Constants.TableName);
			return result;
		}

		bool IsValidLine(EDICodeMappingDataToLoad record)
		{
			bool result = false;
			if (record.IsEmpty)
			{
				DisplayFormattedLogMessage(record.ForeignCode, record.Relationship, record.EdiEnterpriseCode,
					Res.GetString("2fd2ed04-322e-4341-8a3e-85c7ade12a14", "Record is either completely or partly empty."));
			}
			else if (!Relationships.Contains(record.Relationship))
			{
				DisplayFormattedLogMessage(record.ForeignCode, record.Relationship, record.EdiEnterpriseCode,
					Res.GetString("217d7bf7-6da9-466f-9116-689ea19e54a0", "Unknown relationship code."));
			}
			else
			{
				int previousMappingLine = 0;
				if (UniqueForeignCodes.TryGetValue(record.ForeignCode, out previousMappingLine))
				{
					DisplayFormattedLogMessage(record.ForeignCode, record.Relationship, record.EdiEnterpriseCode,
						Res.GetString("a85c2f99-768c-46a4-b42e-c68331343889", "The foreign code is a duplicate."));
				}
				else
				{
					UniqueForeignCodes.Add(record.ForeignCode, RunCounters.CurrentRow);
					result = true;
				}
			}
			return result;
		}

		Dictionary<ZString, int> UniqueForeignCodes
		{
			get { return uniqueForeignCodes ?? (uniqueForeignCodes = new Dictionary<ZString, int>()); }
		}
		Dictionary<ZString, int> uniqueForeignCodes;

		bool HasMapping(EDICodeMappingDataToLoad referenceLine, ZGuid localGuid, ZString localCode)
		{
			bool result = false;
			if (localGuid.IsEmpty && localCode.IsEmpty)
			{
				DisplayFormattedLogMessage(referenceLine.ForeignCode, referenceLine.Relationship, referenceLine.EdiEnterpriseCode,
					Res.GetString("7bd4aedc-8669-4c8f-b8c5-135b68ebe47f", "Enterprise code doesn't exist."));
				result = true;
			}
			else if (RecordAlreadyExists(referenceLine, localGuid, localCode))
			{
				DisplayFormattedLogMessage(referenceLine.ForeignCode, referenceLine.Relationship, referenceLine.EdiEnterpriseCode,
					Res.GetString("d47a6490-f28b-47f8-b90b-f15f7adb0370", "Record is duplicated."));
				result = true;
			}
			else if (ForeignCodeAlreadyExists(referenceLine.ForeignCode))
			{
				DisplayFormattedLogMessage(referenceLine.ForeignCode, referenceLine.Relationship, referenceLine.EdiEnterpriseCode,
					Res.GetString("e09d4f31-05e3-4944-bf08-5b29faca59c6", "Mapping for Foreign Code already exists."));
				result = true;
			}
			return result;
		}

		bool RecordAlreadyExists(EDICodeMappingDataToLoad record, ZGuid localGuid, ZString localCode)
		{
			var query = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, orgProxy.PK)
			.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, record.ForeignCode)
			.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, record.Relationship)
			.AddToFilter(OrgPatternMatchOverrideSchema.OO_LocalCode, localCode)
			.AddToFilter(OrgPatternMatchOverrideSchema.OO_LocalGuid, localGuid);
			return orgProxy.Factory.ExistsInDatabase(OrgPatternMatchOverrideSchema.Constants.TableName, query);
		}

		bool ForeignCodeAlreadyExists(ZString foreignCode)
		{
			var query = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, orgProxy.PK)
			.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, foreignCode);
			return orgProxy.Factory.ExistsInDatabase(OrgPatternMatchOverrideSchema.Constants.TableName, query);
		}

		ZGuid GetLocalGuid(ZString relationship, ZString ediEnterpriseCode)
		{
			ZGuid result = ZGuid.Empty;
			switch (relationship)
			{
				case Constants.OrgPatternMatchOverrideRelationships.ContainerType:
					result = GetLocalGuid<RefContainer>(RefContainerSchema.RC_Code, ediEnterpriseCode);
					break;

				case Constants.OrgPatternMatchOverrideRelationships.Country:
					result = GetLocalGuid<RefCountry>(RefCountrySchema.RN_Code, ediEnterpriseCode);
					break;

				case Constants.OrgPatternMatchOverrideRelationships.Commodities:
					result = GetLocalGuid<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, ediEnterpriseCode);
					break;

				case Constants.OrgPatternMatchOverrideRelationships.Currency:
					result = GetLocalGuid<RefCurrency>(RefCurrencySchema.RX_Code, ediEnterpriseCode);
					break;

				case Constants.OrgPatternMatchOverrideRelationships.Equipment:
					result = GetLocalGuid<RefEquipment>(RefEquipmentSchema.RQ_ShortCode, ediEnterpriseCode);
					break;

				case Constants.OrgPatternMatchOverrideRelationships.Organisation:
					result = GetLocalGuid<OrgHeader>(OrgHeaderSchema.OH_Code, ediEnterpriseCode);
					break;

				case Constants.OrgPatternMatchOverrideRelationships.Port:
					result = GetLocalGuid<RefUNLOCO>(RefUNLOCOSchema.RL_Code, ediEnterpriseCode);
					break;

				case Constants.OrgPatternMatchOverrideRelationships.Warehouse:
					result = GetWarehousePK(ediEnterpriseCode);
					break;

				case Constants.OrgPatternMatchOverrideRelationships.ServiceLevel:
					result = GetLocalGuid<RefServiceLevel>(RefServiceLevelSchema.RS_Code, ediEnterpriseCode);
					break;

				case Constants.OrgPatternMatchOverrideRelationships.IntZone:
					result = GetLocalGuid<RefZoneHeader>(RefZoneHeaderSchema.FZ_Code, ediEnterpriseCode);
					break;
			}
			return result;
		}

		ZGuid GetWarehousePK(ZString ediEnterpriseCode)
		{
			var result = ZGuid.Empty;

			var whsWarehouseType = CargoWise.Application.ObjectFactory.GetType<Warehouse.Integration.IWhsWarehouse>();
			var warehouse = Factory.LoadFromUniqueKey(whsWarehouseType, WhsWarehouseSchema.WW_WarehouseCode, ediEnterpriseCode);
			if (warehouse != null)
			{
				result = warehouse.PK;
			}

			return result;
		}

		ZGuid GetLocalGuid<T>(SchemaStringColumn column, ZString code) where T : BusinessObject
		{
			ZGuid result = ZGuid.Empty;
			T bizo = Factory.LoadFromUniqueKey<T>(column, code);
			if (bizo != null)
			{
				result = bizo.PK;
			}
			return result;
		}

		ZString GetLocalCode(ZString relationship, ZString ediEnterpriseCode)
		{
			ZString result = ZString.Empty;
			switch (relationship)
			{
				case Constants.OrgPatternMatchOverrideRelationships.ChargeCodes:
					ZQuery filter = new ZQuery(AccChargeCodeSchema.AC_Code, ediEnterpriseCode);
					filter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
					AccChargeCode chargeCode = Factory.LoadTop1<AccChargeCode>(filter);
					if (chargeCode != null)
					{
						result = ediEnterpriseCode;
					}
					break;

				case Constants.OrgPatternMatchOverrideRelationships.DropMode:
					result = new CombinedEquipmentNeededList().ContainsCode(ediEnterpriseCode) ? ediEnterpriseCode : ZString.Empty;
					break;

				case Constants.OrgPatternMatchOverrideRelationships.PackageType:
					result = GetLocalGuid<RefPackType>(RefPackTypeSchema.F3_Code, ediEnterpriseCode).IsEmpty ? ZString.Empty : ediEnterpriseCode;
					break;

				case Constants.OrgPatternMatchOverrideRelationships.EventCode:
					result = (from Event e in Events.All where e.Code == ediEnterpriseCode select e.Code).First();
					break;

				case Constants.OrgPatternMatchOverrideRelationships.IncoTerm:
					result = IncoTermRegistry.Keys.Single(i => i == ediEnterpriseCode);
					break;

				case Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel:
					result = orgProxy.MiscServ.CarrierServiceLevels.Cast<OrgCarrierServiceLevel>().Any(x => x.PL_Code == ediEnterpriseCode) ? ediEnterpriseCode : ZString.Empty;
					break;
			}
			return result;
		}

		#endregion

		#region ExtractData

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		static class FieldNames
		{
			public const string ForeignCode = "ForeignCode";
			public const string Relationship = "Relationship";
			public const string CargoWiseOneCode = "CargoWiseOneCode";
		}

		/// <summary>
		/// Override to copy data from Line to Record object - do not post to any other business objects, just Record
		/// </summary>
		/// <param name="line"></param>
		/// <param name="record"></param>
		protected virtual void PopulateCodeMappingDataToLoad(OCsvLine line, EDICodeMappingDataToLoad record)
		{
			record.ForeignCode = TryGetStringValue(line, FieldNames.ForeignCode).Left(OrgPatternMatchOverride.Schema.OO_ForeignCodeMaxLength);
			record.Relationship = TryGetStringValue(line, FieldNames.Relationship).Left(OrgPatternMatchOverride.Schema.OO_RelationshipMaxLength);
			record.EdiEnterpriseCode = TryGetStringValue(line, FieldNames.CargoWiseOneCode);
		}

		#endregion

		#region Utilities

		void DisplayFormattedLogMessage(ZString foreignCode, ZString relationship, ZString ediEnterpriseCode, string detailedExceptionMessage)
		{
			string ouputRowNo = Res.GetString("88a4d77d-330e-4b2e-8e32-1524e2540f92", "Line {0}:", RunCounters.CurrentRow.ToString()) + " ";
			string logMessage = Res.GetString("86c205f6-7df8-49d7-b3a8-8c02d0cde31d", "{0} Foreign Code={1}\tRelationship={2}\tEnterprise Code={3}\t{4}", ouputRowNo, foreignCode, relationship, ediEnterpriseCode, detailedExceptionMessage);

			DisplayLogMessage(logMessage);
		}

		#endregion

		public override string CSVTemplateHeading
		{
			get { return string.Join(",", CSVTemplateHeaders); }
		}

		IEnumerable<string> CSVTemplateHeaders
		{
			get
			{
				yield return "ForeignCode";
				yield return (NoResString)"Relationship";
				yield return "CargoWiseOneCode";
			}
		}

		ISet<ZString> Relationships
		{
			get
			{
				return relationships ?? (relationships = new HashSet<ZString>()
				{
					Constants.OrgPatternMatchOverrideRelationships.ChargeCodes,
					Constants.OrgPatternMatchOverrideRelationships.ContainerType,
					Constants.OrgPatternMatchOverrideRelationships.Country,
					Constants.OrgPatternMatchOverrideRelationships.Commodities,
					Constants.OrgPatternMatchOverrideRelationships.Currency,
					Constants.OrgPatternMatchOverrideRelationships.DropMode,
					Constants.OrgPatternMatchOverrideRelationships.Equipment,
					Constants.OrgPatternMatchOverrideRelationships.EventCode,
					Constants.OrgPatternMatchOverrideRelationships.IncoTerm,
					Constants.OrgPatternMatchOverrideRelationships.Organisation,
					Constants.OrgPatternMatchOverrideRelationships.PackageType,
					Constants.OrgPatternMatchOverrideRelationships.Port,
					Constants.OrgPatternMatchOverrideRelationships.Warehouse,
					Constants.OrgPatternMatchOverrideRelationships.ServiceLevel,
					Constants.OrgPatternMatchOverrideRelationships.IntZone,
					Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel
				});
			}
		}
		ISet<ZString> relationships;

		readonly OrgHeader orgProxy;
	}
}
