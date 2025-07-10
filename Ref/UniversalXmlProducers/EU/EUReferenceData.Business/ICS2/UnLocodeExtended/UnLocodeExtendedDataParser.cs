using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;
using Microsoft.IdentityModel.Tokens;

namespace CargoWise.RefDbRepo.EUReferenceData.Business.ICS2;

public class UnLocodeExtendedDataParser : CommonDataParser
{
	public UnLocodeExtendedDataParser() :
		base(Constants.UnLocodeExtended.RDEntityAttributeValue, Constants.UnLocodeExtended.CodeAttributeValue)
	{ }

	protected override void SetRefCusCode(RefCusCodeList refCusCodeList, XElement entity)
	{
		base.SetRefCusCode(refCusCodeList, entity);
		if (!refCusCodeList.ZZD_Code.IsNullOrEmpty())
		{
			refCusCodeList.ZZD_Description = Utils.FindXElementValueByAttribute(entity, Constants.Common.DataItem, Constants.Common.RDEntityAttributeName, Constants.UnLocodeExtended.DescriptionAttributeValue);
		}
	}
}
