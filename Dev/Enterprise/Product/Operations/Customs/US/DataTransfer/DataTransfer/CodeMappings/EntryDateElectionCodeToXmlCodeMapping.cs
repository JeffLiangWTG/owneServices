using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	public class EntryDateElectionCodeToXmlCodeMapping : EnterpriseCodeExternalCodeMappings
	{
		EntryDateElectionCodeToXmlCodeMapping()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(EntryDateElectionCodeList.Codes.ArrivalDate, nameof(Xsd.USDeclarationEntryDateElectionCode.A));
			yield return new Mapping(EntryDateElectionCodeList.Codes.PresentationDate, nameof(Xsd.USDeclarationEntryDateElectionCode.P));
		}

		public static readonly EntryDateElectionCodeToXmlCodeMapping Instance = new EntryDateElectionCodeToXmlCodeMapping();

		protected override string Name
		{
			get { return "Declaration Entry Date Election Code"; }
		}

		public new Xsd.USDeclarationEntryDateElectionCode GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USDeclarationEntryDateElectionCode.A, errorContext, notify);
		}
	}
}
