using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class UNDGCommonDataSubstanceRelationship : CollectionRelationship
	{
		public UNDGCommonDataSubstanceRelationship(UNDGSubstance substance, string commonDataType, string attributeDataType)
			: this(substance, commonDataType, new[] { attributeDataType })
		{
		}

		public UNDGCommonDataSubstanceRelationship(UNDGSubstance substance, string commonDataType, IEnumerable<string> attributeDataTypes, Func<UNDGCommonData, ZString> getAttributeTypeFromCommonData = null)
			: base(typeof(UNDGCommonData))
		{
			Substance = Argument.NotNull(substance, nameof(substance));
			CommonDataType = Argument.NotNull(commonDataType, nameof(commonDataType));
			AttributeDataTypes = Argument.NotNull(attributeDataTypes, nameof(attributeDataTypes)).WhereNotNull().ToArray();
			Argument.GreaterThanZero(AttributeDataTypes.Length, nameof(attributeDataTypes));
			GetAttributeTypeFromCommonData = getAttributeTypeFromCommonData;
		}

		readonly UNDGSubstance Substance;
		readonly string CommonDataType;
		readonly string[] AttributeDataTypes;
		readonly Func<UNDGCommonData, ZString> GetAttributeTypeFromCommonData;

		protected override ZQuery RelationshipFilterCore
		{
			get { return new ZQuery(UNDGCommonDataSchema.DC_Type, CommonDataType); }
		}

		protected override void AddToRelationship(BusinessObject businessObject)
		{
			var commonData = (UNDGCommonData)businessObject;
			var attribute = businessObject.Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = GetAttributeTypeFromCommonData?.Invoke(commonData) ?? AttributeDataTypes[0];
			attribute.DA_Index = commonData.DC_Index;
			attribute.DA_DG = Substance.PK;
		}

		protected override void RemoveFromRelationship(BusinessObject businessObject)
		{
			var commonData = (UNDGCommonData)businessObject;
			if (Substance.DG_IsSystem && commonData.DC_Language == Core.SharedConstants.Languages.English)
			{
				throw new CannotDeleteException("You cannot remove English details from a System Defined Dangerous Goods Substance.");
			}
			else
			{
				var query = new ZQuery(ViewUNDGAttributeSchema.DA_Index, commonData.DC_Index);
				query.AddToFilter(ViewUNDGAttributeSchema.DA_Type, GetAttributeTypeFromCommonData?.Invoke(commonData) ?? AttributeDataTypes[0]);
				query.AddToFilter(ViewUNDGAttributeSchema.DA_DG, Substance.PK);

				var attributes = commonData.Factory.Load<ViewUNDGAttribute>(query);
				foreach (var attribute in attributes)
				{
					attribute.Delete();
				}
			}
		}
	}
}

// Tested in UNDGCommonDataCollection
