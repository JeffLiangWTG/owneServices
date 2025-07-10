using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.ServerServices
{
	public abstract class WarehouseDocketLineUpdateWSMethod<ParameterType> : TrackingWebServiceMethod<ParameterType>
		where ParameterType : WarehouseDocketLineUpdateParameters
	{
		protected abstract class DocketLineValues
		{
			public ZString Description;
			public ZString Product;
			public ZDecimal Packs;
			public ZString PacksUQ;
			public CodeDescriptionPairList PackTypes;
			public ZDecimal Quantity;
			public ZString ProductUQ;
			public ZString Attribute1;
			public ZString Attribute2;
			public ZString Attribute3;
			public ZString SerialNumber;
			public bool Attribute1ReadOnly;
			public bool Attribute2ReadOnly;
			public bool Attribute3ReadOnly;
			public bool SerialNumberReadOnly;
		}

		protected override void ExecuteCore(ParameterType parameters, WebServiceResponse response)
		{
			var line = GetDocketLine(parameters);

			if (line != null)
			{
				GenerateResponse(parameters, line, response);
			}
		}

		protected abstract DocketLineValues GetNewDocketLineValues();

		protected virtual DocketLineValues GetDocketLineValues(WhsDocketLine line)
		{
			var values = GetNewDocketLineValues();
			values.Description = line.SupplierPart?.OP_Desc ?? string.Empty;
			values.Product = line.SupplierPart?.OP_PartNum ?? string.Empty;
			values.Packs = line.WE_PackQuantity;
			values.PacksUQ = line.WE_F3_NKPackType;
			values.PackTypes = line.Lookups.PackTypes;
			values.Quantity = line.WE_TransactionQuantity;
			values.ProductUQ = line.Lookups.PackTypes.GetDescriptionFromCode(line.ProductUQ);
			values.Attribute1 = line.WE_PartAttrib1;
			values.Attribute2 = line.WE_PartAttrib2;
			values.Attribute3 = line.WE_PartAttrib3;
			values.SerialNumber = line.WE_SerialNumber;
			values.Attribute1ReadOnly = line.WE_PartAttrib1Info.ReadOnly;
			values.Attribute2ReadOnly = line.WE_PartAttrib2Info.ReadOnly;
			values.Attribute3ReadOnly = line.WE_PartAttrib3Info.ReadOnly;
			values.SerialNumberReadOnly = line.WE_SerialNumberInfo.ReadOnly;

			return values;
		}

		void GenerateResponse(ParameterType parameters, WhsDocketLine line, WebServiceResponse response)
		{
			var initialValues = GetDocketLineValues(line);
			UpdateValues(initialValues, parameters, line);

			var updatedValues = GetDocketLineValues(line);
			GenerateResponse(initialValues, updatedValues, parameters, line, response);
		}

		protected virtual void GenerateResponse(DocketLineValues initialValues, DocketLineValues updatedValues, ParameterType parameters, WhsDocketLine line, WebServiceResponse response)
		{
			AddUpdateListToken(response, parameters, parameters.PacksUQControlID, initialValues.PackTypes, updatedValues.PackTypes);

			AddUpdateToken(response, parameters, parameters.DescriptionControlID, initialValues.Description, updatedValues.Description);
			AddUpdateToken(response, parameters, parameters.PacksControlID, initialValues.Packs, updatedValues.Packs, true);
			AddUpdateToken(response, parameters, parameters.PacksUQControlID, initialValues.PacksUQ, updatedValues.PacksUQ);
			AddUpdateToken(response, parameters, parameters.QuantityControlID, initialValues.Quantity, updatedValues.Quantity, true);
			AddUpdateToken(response, parameters, parameters.ProductUQControlID, initialValues.ProductUQ, updatedValues.ProductUQ);
			AddUpdateToken(response, parameters, parameters.Attribute1ControlID, initialValues.Attribute1, updatedValues.Attribute1);
			AddUpdateToken(response, parameters, parameters.Attribute2ControlID, initialValues.Attribute2, updatedValues.Attribute2);
			AddUpdateToken(response, parameters, parameters.Attribute3ControlID, initialValues.Attribute3, updatedValues.Attribute3);
			AddUpdateToken(response, parameters, parameters.SerialNumberControlID, initialValues.SerialNumber, updatedValues.SerialNumber);

			AddSetReadOnlyToken(response, parameters.Attribute1ControlID, initialValues.Attribute1ReadOnly, updatedValues.Attribute1ReadOnly);
			AddSetReadOnlyToken(response, parameters.Attribute2ControlID, initialValues.Attribute2ReadOnly, updatedValues.Attribute2ReadOnly);
			AddSetReadOnlyToken(response, parameters.Attribute3ControlID, initialValues.Attribute3ReadOnly, updatedValues.Attribute3ReadOnly);
			AddSetReadOnlyToken(response, parameters.SerialNumberControlID, initialValues.SerialNumberReadOnly, updatedValues.SerialNumberReadOnly);
		}

		protected virtual void UpdateValues(DocketLineValues initialValues, ParameterType parameters, WhsDocketLine line)
		{
			if (parameters.ModifiedControlID == parameters.ProductControlID && initialValues.Product != parameters.NewValue)
			{
				var supplierPart = GetSupplierPart(parameters.NewValue, line.Factory);
				line.WE_OP = supplierPart?.PK ?? Guid.Empty;
			}

			if (parameters.ModifiedControlID == parameters.PacksControlID && ZDecimal.TryParse(parameters.NewValue, out var packs) && initialValues.Packs != packs)
			{
				line.WE_PackQuantity = packs;
			}

			if (parameters.ModifiedControlID == parameters.PacksUQControlID && initialValues.PacksUQ != parameters.NewValue)
			{
				line.WE_F3_NKPackType = parameters.NewValue;
			}

			if (parameters.ModifiedControlID == parameters.QuantityControlID && ZDecimal.TryParse(parameters.NewValue, out var quantity) && initialValues.Quantity != quantity)
			{
				line.WE_TransactionQuantity = quantity;
			}

			if (!line.Lookups.PackTypes.ContainsCode(line.WE_F3_NKPackType))
			{
				line.WE_F3_NKPackType = line.ProductUQ;
			}
		}

		protected void AddSetReadOnlyToken(WebServiceResponse response, string controlID, bool initialReadOnly, bool updatedReadOnly)
		{
			if (initialReadOnly != updatedReadOnly)
			{
				var setReadOnlyToken = new SetReadOnlyResponseToken(controlID, updatedReadOnly);
				response.Add(setReadOnlyToken);
			}
		}

		protected void AddUpdateToken(WebServiceResponse response, ParameterType parameters, string controlID, object initialValue, object updatedValue, bool emptyStringIfZero = false) => AddUpdateToken(response, parameters.ModifiedControlID, controlID, initialValue, updatedValue, emptyStringIfZero);

		protected void AddUpdateListToken(WebServiceResponse response, ParameterType parameters, string controlID, CodeDescriptionPairList initialValue, CodeDescriptionPairList updatedValue)
		{
			if (controlID != parameters.ModifiedControlID && !string.IsNullOrEmpty(controlID) && CodesHaveChanged(initialValue, updatedValue))
			{
				var updateToken = new UpdateListResponseToken(controlID, updatedValue);
				updateToken.Conditions.Add(new ResponseConditionEqualToken(parameters.ModifiedControlID, parameters.NewValue));
				response.Add(updateToken);
			}
		}

		protected bool CodesHaveChanged(CodeDescriptionPairList initialValue, CodeDescriptionPairList updatedValue)
		{
			if (initialValue != updatedValue)
			{
				return initialValue.Count != updatedValue.Count || initialValue.GetAllCodes().Any(code => !updatedValue.ContainsCode(code));
			}

			return false;
		}

		protected void AddPostBackToken(WebServiceResponse response, string controlID)
		{
			var postBackToken = new PostBackResponseToken(controlID);
			response.Add(postBackToken);
		}

		OrgSupplierPart GetSupplierPart(string product, BusinessObjectFactory factory)
		{
			var query = OrgRestrictionFilterFactory.Instance.GetSubQuery(typeof(OrgSupplierPart), typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			query.AddToFilter(OrgSupplierPartSchema.OP_PartNum, product);
			query.AddToFilter(OrgSupplierPartSchema.OP_IsActive, true);

			return factory.Load<OrgSupplierPart>(query).FirstOrDefault();
		}

		protected abstract WhsDocketLine GetDocketLine(ParameterType parameters);

		protected override bool AddErrorMessageToResponse() => false;
	}
}
