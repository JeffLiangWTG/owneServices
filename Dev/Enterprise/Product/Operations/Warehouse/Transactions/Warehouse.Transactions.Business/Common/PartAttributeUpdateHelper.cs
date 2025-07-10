using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class PartAttributeUpdateHelper
	{
		#region SetPartAttributeValue

		public static void SetPartAttributeValue(WhsAdjustmentLine newLine, PartAttributeNumber number, string attributeValue)
		{
			switch (number)
			{
				case PartAttributeNumber.One:
					newLine.WE_PartAttrib1 = attributeValue;
					break;
				case PartAttributeNumber.Two:
					newLine.WE_PartAttrib2 = attributeValue;
					break;
				case PartAttributeNumber.Three:
					newLine.WE_PartAttrib3 = attributeValue;
					break;
			}
		}

		#endregion

		#region SetPartAttributeUsage

		public static void SetPartAttributeUsage(OrgPartRelation newRelation, PartAttributeNumber number, bool oldClientRelationPartAttributeValue)
		{
			switch (number)
			{
				case PartAttributeNumber.One:
					newRelation.OU_UsePartAttrib1 = oldClientRelationPartAttributeValue;
					break;
				case PartAttributeNumber.Two:
					newRelation.OU_UsePartAttrib2 = oldClientRelationPartAttributeValue;
					break;
				case PartAttributeNumber.Three:
					newRelation.OU_UsePartAttrib3 = oldClientRelationPartAttributeValue;
					break;
			}
		}

		#endregion

		#region GetAttributeWithMatchingTypeAndNameWhenRequired

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Properties are used internally.")]
		public static PartAttributeNumber GetAttributeWithMatchingTypeAndNameWhenRequired(ZString attributeType, ZString attributeName, OrgMiscServ newClientMiscServ, IList<PartAttributeNumber> previouslyMatchedAttrbiuteNumbers)
		{
			var result = PartAttributeNumber.None;

			var typeMatchCount = GetNumberOfMatchingAttributeTypes(newClientMiscServ, attributeType);
			var compareByTypeOnly = typeMatchCount == 1;

			if (typeMatchCount > 0)
			{
				if (!previouslyMatchedAttrbiuteNumbers.Contains(PartAttributeNumber.One) && newClientMiscServ.OM_IMPartAttrib1Type == attributeType && (compareByTypeOnly || newClientMiscServ.OM_IMPartAttrib1Name.EqualsIgnoringCase(attributeName)))
				{
					result = PartAttributeNumber.One;
				}
				else if (!previouslyMatchedAttrbiuteNumbers.Contains(PartAttributeNumber.Two) && newClientMiscServ.OM_IMPartAttrib2Type == attributeType && (compareByTypeOnly || newClientMiscServ.OM_IMPartAttrib2Name.EqualsIgnoringCase(attributeName)))
				{
					result = PartAttributeNumber.Two;
				}
				else if (!previouslyMatchedAttrbiuteNumbers.Contains(PartAttributeNumber.Three) && newClientMiscServ.OM_IMPartAttrib3Type == attributeType && (compareByTypeOnly || newClientMiscServ.OM_IMPartAttrib3Name.EqualsIgnoringCase(attributeName)))
				{
					result = PartAttributeNumber.Three;
				}
			}

			if (result != PartAttributeNumber.None)
			{
				previouslyMatchedAttrbiuteNumbers.Add(result);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Properties are used internally.")]
		public static PartAttributeNumber GetAttributeWithMatchingTypeAndNameWhenRequired(ZString attributeType, ZString attributeName, OrgMiscServ newClientMiscServ, PartAttributeNumber oldClientAttributeNumber,
			Dictionary<PartAttributeNumber, PartAttributeNumber> previouslyMatchedAttrbiuteNumbers)
		{
			var result = PartAttributeNumber.None;

			var typeMatchCount = GetNumberOfMatchingAttributeTypes(newClientMiscServ, attributeType);
			var compareByTypeOnly = typeMatchCount == 1;

			if (!attributeType.IsEmpty && typeMatchCount > 0)
			{
				if (!previouslyMatchedAttrbiuteNumbers.ContainsValue(PartAttributeNumber.One) && newClientMiscServ.OM_IMPartAttrib1Type == attributeType && (compareByTypeOnly || newClientMiscServ.OM_IMPartAttrib1Name.EqualsIgnoringCase(attributeName)))
				{
					result = PartAttributeNumber.One;
				}
				else if (!previouslyMatchedAttrbiuteNumbers.ContainsValue(PartAttributeNumber.Two) && newClientMiscServ.OM_IMPartAttrib2Type == attributeType && (compareByTypeOnly || newClientMiscServ.OM_IMPartAttrib2Name.EqualsIgnoringCase(attributeName)))
				{
					result = PartAttributeNumber.Two;
				}
				else if (!previouslyMatchedAttrbiuteNumbers.ContainsValue(PartAttributeNumber.Three) && newClientMiscServ.OM_IMPartAttrib3Type == attributeType && (compareByTypeOnly || newClientMiscServ.OM_IMPartAttrib3Name.EqualsIgnoringCase(attributeName)))
				{
					result = PartAttributeNumber.Three;
				}
			}

			if (result != PartAttributeNumber.None)
			{
				previouslyMatchedAttrbiuteNumbers.Add(oldClientAttributeNumber, result);
			}

			return result;
		}

		static int GetNumberOfMatchingAttributeTypes(OrgMiscServ clientMiscServ, ZString attributeType)
		{
			var typeMatchCount = 0;
			typeMatchCount += clientMiscServ.OM_IMPartAttrib1Type == attributeType ? 1 : 0;
			typeMatchCount += clientMiscServ.OM_IMPartAttrib2Type == attributeType ? 1 : 0;
			typeMatchCount += clientMiscServ.OM_IMPartAttrib3Type == attributeType ? 1 : 0;

			return typeMatchCount;
		}

		#endregion
	}
}
