using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO
{
	public class SpecialProvisionAndRegulatorySectionRule : ISegregationRule
	{
		readonly List<ZString> classesToCheckRegulatorySection = new List<ZString> { "2", "3", "4", "5", "6", "7", "8", "9" };
		readonly string classToCheckSpecialProvision = "7";
		readonly string exemptedRegulatorySection = "2.0.6.6";
		readonly string specialProvisionPrefix = "SP";

		readonly MultilingualString segregationMessage = ResString.GetMultilingualString("DA2FE1AD-99A7-456D-BD39-F3A147239E1F",
			"Check the special provision, or regulatory reference attached to this UN number’s class data to ensure appropriate segregation requirements are met.");

		readonly MultilingualString classNotFoundMessage = ResString.GetMultilingualString("8F67FEA8-BCC3-4938-A9DE-9CCE2C7BA464",
			"The class is empty for the dangerous goods substance.");

		string ISegregationRule.ApplicableStandard => UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

		public bool IsExemption => false;

		public IEnumerable<Message> Check(UNDGSubstance substance1, UNDGSubstance substance2)
		{
			var messages = new List<Message>();

			if(IsWarningMessageRequiredForSubstance(substance1) || IsWarningMessageRequiredForSubstance(substance2))
			{
				messages.Add(new Message(MessageType.Warning, segregationMessage));
			}
			return messages;
		}

		bool IsWarningMessageRequiredForSubstance(UNDGSubstance substance)
		{
			var dgClass = substance.DG_Class;
			var dgSublabel = substance.DG_SubLabel1;

			if (dgClass.IsEmpty)
			{
				throw new DataNotFoundException(classNotFoundMessage);
			}

			if (dgSublabel.IsEmpty)
			{
				return false;
			}

			if (IsSpecialProvisionCase(dgClass, dgSublabel) || IsRegulatorySectionCase(dgClass, dgSublabel))
			{
				return true;
			}

			return false;
		}

		bool IsSpecialProvisionCase(string dgClass, string dgSublabel)
		{
			return dgClass.StartsWith(classToCheckSpecialProvision)
				&& dgSublabel.StartsWith(specialProvisionPrefix, StringComparison.InvariantCultureIgnoreCase);
		}

		bool IsRegulatorySectionCase(string dgClass, string dgSublabel)
		{
			return dgSublabel.Equals(exemptedRegulatorySection)
				&& classesToCheckRegulatorySection.Any(prefix => dgClass.StartsWith(prefix));
		}
	}
}
