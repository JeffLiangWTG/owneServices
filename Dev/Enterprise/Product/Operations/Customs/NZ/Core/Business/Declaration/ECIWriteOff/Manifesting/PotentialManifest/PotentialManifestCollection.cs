using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting
{
	public class PotentialManifestCollection : DynamicBusinessObjectCollection<PotentialManifest>
	{
		public PotentialManifestCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
			: base(factory)
		{
			this.companyPkToFilterOn = companyPkToFilterOn;
		}

		readonly ZGuid companyPkToFilterOn;

		#region Load
		public void Load()
		{
			var barrierPortFieldText
				= "case " + JobDeclarationSchema.JE_MessageType.Name
				+ " when @MessageTypeExport then " + JobDeclarationSchema.JE_RL_NKPortOfLoading.Name
				+ " when @MessageTypeImport then " + JobDeclarationSchema.JE_RL_NKPortOfArrival.Name
				+ " else '' end";
			var barrierDateFieldText
				= "convert(DateTime, floor(convert(Float, case " + JobDeclarationSchema.JE_MessageType.Name
				+ " when @MessageTypeExport then " + JobDeclarationSchema.JE_ExportDate.Name
				+ " when @MessageTypeImport then " + JobDeclarationSchema.JE_DateOfArrival.Name
				+ " else null end)))";
			var sQLText = "select "
				+ JobDeclarationSchema.JE_MessageType.Name + ", "
				+ JobDeclarationSchema.JE_MasterBill.Name + ", "
				+ JobDeclarationSchema.JE_VoyageFlightNo.Name + ", "
				+ JobDeclarationSchema.JE_OH_ShippingLine.Name + ", "
				+ barrierPortFieldText + " as " + PotentialManifest.Schema.BarrierPort + ", "
				+ barrierDateFieldText + " as " + PotentialManifest.Schema.BarrierDate + ", "
				+ "count(*) as " + PotentialManifest.Schema.DeclarationCount
				+ " from " + JobDeclarationSchema.Constants.SqlSchemaName + "." + JobDeclarationSchema.Constants.TableName
				+ " where " + JobDeclarationSchema.JE_EntryStatus.Name + " = @EntryStatus"
				+ " and " + JobDeclarationSchema.JE_GB.Name + " IN (select " + GlbBranchSchema.PK.Name + " from " + GlbBranchSchema.Constants.SqlSchemaName + "." + GlbBranchSchema.Constants.TableName + " where " + GlbBranchSchema.GB_GC.Name + " = @CompanyPk )"
				+ " and " + JobDeclarationSchema.JE_MessageSubType.Name + " = @MessageSubType"
				+ " group by "
				+ JobDeclarationSchema.JE_MessageType.Name + ", "
				+ JobDeclarationSchema.JE_MasterBill.Name + ", "
				+ JobDeclarationSchema.JE_VoyageFlightNo.Name + ", "
				+ JobDeclarationSchema.JE_OH_ShippingLine.Name + ", "
				+ barrierPortFieldText + ", "
				+ barrierDateFieldText;

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@EntryStatus", LowValueConsignmentStatusList.Codes.ReadyForManifesting, JobDeclarationSchema.JE_EntryStatus);
			parameters.Add("@MessageSubType", JobMessageSubTypeList.Codes.WriteOff, JobDeclarationSchema.JE_MessageSubType);
			parameters.Add("@MessageTypeImport", JobMessageTypeList.Codes.Import, JobDeclarationSchema.JE_MessageType);
			parameters.Add("@MessageTypeExport", JobMessageTypeList.Codes.Export, JobDeclarationSchema.JE_MessageType);
			parameters.Add("@CompanyPk", companyPkToFilterOn, GlbBranchSchema.GB_GC);
			Load(sQLText, parameters);
		}
		#endregion

		#region Find
		public PotentialManifest Find(ZString masterBill, ZString flightNo, ZGuid carrier, ZString messageType, ZString barrierPort, ZDateTime barrierDate)
		{
			PotentialManifest result = null;
			foreach (PotentialManifest manifest in this)
			{
				if (manifest.JE_MasterBill == masterBill
					&& manifest.JE_VoyageFlightNo == flightNo
					&& manifest.JE_OH_ShippingLine == carrier
					&& manifest.JE_MessageType == messageType
					&& manifest.BarrierPort == barrierPort
					&& manifest.BarrierDate == barrierDate)
				{
					result = manifest;
					break;
				}
			}
			return result;
		}
		#endregion
	}
}
