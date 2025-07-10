using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class TemplateRecordFindboxListProvider : FindBoxListProvider
	{
		public TemplateRecordFindboxListProvider(IBusinessObjectCollection collection) : base(collection) { }

		string ModuleID
		{
			get
			{
				if (moduleID == null)
				{
					var moduleIDAttribute = List?.GetType().GetCustomAttributes(true).OfType<ModuleIDAttribute>().FirstOrDefault();
					if (moduleIDAttribute != null)
					{
						moduleID = moduleIDAttribute.ModuleIdName;
					}
				}
				return moduleID;
			}
		}

		string moduleID;

		public override string CodeFromPrimaryKey(ZGuid pk)
		{
			var code = base.CodeFromPrimaryKey(pk);
			if (string.IsNullOrEmpty(code))
			{
				var templateRecord = List.Factory.Load<StmTemplateRecord>(pk);
				if (templateRecord != null && templateRecord.STR_ModuleID == ModuleID)
				{
					code = templateRecord.STR_ReferenceId;
				}
			}
			return code;
		}

		public override string DescriptionFromPrimaryKey(ZGuid pk)
		{
			var description = base.DescriptionFromPrimaryKey(pk);
			if (string.IsNullOrEmpty(description))
			{
				var templateRecord = List.Factory.Load<StmTemplateRecord>(pk);
				if (templateRecord != null && templateRecord.STR_ModuleID == ModuleID)
				{
					description = templateRecord.Description;
				}
			}
			return description;
		}

		public override ZGuid PrimaryKeyFromCode(string code)
		{
			var pk = base.PrimaryKeyFromCode(code);
			if (pk == ZGuid.Invalid)
			{
				var templateRecordQuery = new ZQuery(StmTemplateRecordSchema.STR_ModuleID, ModuleID);
				templateRecordQuery.AddToFilter(StmTemplateRecordSchema.STR_ReferenceId, code);
				var templateRecord = List.Factory.LoadTop1<StmTemplateRecord>(templateRecordQuery);
				if (templateRecord != null)
				{
					pk = templateRecord.PK;
				}
			}
			return pk;
		}

		protected override IEnumerable<BusinessObject> GetBusinessObjectsFromCodeCore(string code)
		{
			return GetBusinessObjectsOrTemplateRecords(code, base.GetBusinessObjectsFromCodeCore);
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithoutFilter(string code)
		{
			return GetBusinessObjectsOrTemplateRecords(code, base.BizObjsFromCodeWithoutFilter);
		}

		IEnumerable<BusinessObject> GetBusinessObjectsOrTemplateRecords(string code, Func<string, IEnumerable<BusinessObject>> getBizosFunc)
		{
			var bizos = getBizosFunc(code);

			var isEmpty = true;
			if (bizos != null)
			{
				foreach (var bizo in bizos)
				{
					yield return bizo;
					isEmpty = false;
				}
			}

			if (isEmpty)
			{
				var templateRecordQuery = new ZQuery(StmTemplateRecordSchema.STR_ModuleID, ModuleID);
				templateRecordQuery.AddToFilter(StmTemplateRecordSchema.STR_ReferenceId, code);
				var templateRecords = List.Factory.Load<StmTemplateRecord>(templateRecordQuery);

				var localFactory = List.Factory.CreateNewFactory();
				foreach (var templateRecord in templateRecords)
				{
					var bizo = localFactory.New(List.TypeOfElements);
					if (bizo is ITemplateRecordProvider templateRecordProvider)
					{
						templateRecordProvider.LoadFromTemplateRecord(templateRecord);
						yield return bizo;
					}
				}
			}
		}

		public override (string, bool) NearestMatchCore(string code, bool explicitAutoComplete)
		{
			var match = base.NearestMatchCore(code, explicitAutoComplete).Item1;
			var success = false;
			if (match.Equals(code, StringComparison.OrdinalIgnoreCase) && base.PrimaryKeyFromCode(code) == ZGuid.Invalid)
			{
				var templateRecordQuery = new ZQuery(StmTemplateRecordSchema.STR_ModuleID, ModuleID);
				templateRecordQuery.AddToFilter(StmTemplateRecordSchema.STR_ReferenceId, SQLComparisonOperator.StartsWith, code);
				var templateRecord = List.Factory.LoadTop1<StmTemplateRecord>(templateRecordQuery);
				match = templateRecord != null ? templateRecord.STR_ReferenceId.ToString() : code;
				success = templateRecord != null;
			}
			return (match, success);
		}
	}
}
