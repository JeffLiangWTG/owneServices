using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCodeGenerator
	{
		readonly CargoWise.Organizations.CodeGeneration.OrgCodeGenerator generator;

		public OrgCodeGenerator()
		{
			generator = new CargoWise.Organizations.CodeGeneration.OrgCodeGenerator(new OrgCodeGeneratorParameters());
		}

		public bool SkippedIllegalCharacter
		{
			get { return generator.SkippedIllegalCharacter; }
		}

		public IEnumerable<char> ValidNonLetterCharsInCode
		{
			get { return WesternLanguageTransliterationHelper.ValidNonLetterCharsInCode; }
		}

		public IOrgCode GenerateCode(IOrgCodeInfo info, BusinessObjectFactory factory)
		{
			return generator.GenerateCode(info, new OrgCodeDbProxy(factory));
		}

		public IOrgCode GenerateCode(OrgHeader organisation)
		{
			return GenerateCode(new OrgHeaderOrgCodeInfo(organisation), organisation.Factory);
		}

		public bool GenerateCodeOnlyIfAlgorithmTypeApplies(OrgCodeAlgorithmType algorithmType, IOrgCodeInfo info, IOrgCodeDbProxy dbProxy, out IOrgCode generatedCode)
		{
			return generator.GenerateCodeOnlyIfAlgorithmTypeApplies(algorithmType, info, dbProxy, out generatedCode);
		}

		public OrgCodeAlgorithm GetAlgorithm(IOrgCodeInfo info)
		{
			return (OrgCodeAlgorithm)generator.GetAlgorithm(info);
		}

		public OrgCodeAlgorithm GetAlgorithm(OrgHeader organisation)
		{
			return GetAlgorithm(new OrgHeaderOrgCodeInfo(organisation));
		}
	}
}
