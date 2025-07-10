using System;
using System.Net;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class HtmlCommissionAgreementConflictEmailDocumentParser : DocumentParser<CommissionAgreementConflictEmailCreator>
	{
		public HtmlCommissionAgreementConflictEmailDocumentParser(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override Type TypeOfWrapper
		{
			get { return typeof(DocCommissionAgreementConflictEmailCreator); }
		}

		protected override string GetPropertyValue(BusinessObject docWrapper, string propertyNameWithCorrectCasing)
		{
			var wrapper = (DocCommissionAgreementConflictEmailCreator)docWrapper;
			var value = WebUtility.HtmlEncode(base.GetPropertyValue(docWrapper, propertyNameWithCorrectCasing));
			if (propertyNameWithCorrectCasing == "SpecificAgreementIDHyperlink")
			{
				var agreementHyperlink = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.OrgCommissionAgreement, wrapper.WrappedObject.WinnerCommissionAgreement.PK.ToGuid());
				return string.Format(@"<a href=""{0}"">{1}</a>", agreementHyperlink, value);
			}
			else if (propertyNameWithCorrectCasing == "GenericAgreementIDHyperlink")
			{
				var agreementHyperlink = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.OrgCommissionAgreement, wrapper.WrappedObject.LoserCommissionAgreement.PK.ToGuid());
				return string.Format(@"<a href=""{0}"">{1}</a>", agreementHyperlink, value);
			}
			else if (propertyNameWithCorrectCasing == "SpecificAgreementItems")
			{
				return
					(NoResString)"<ul>" +
						wrapper.WrappedObject.GetConflictsDescription(1, true) +
					(NoResString)"</ul>";
			}

			return value;
		}
	}
}
