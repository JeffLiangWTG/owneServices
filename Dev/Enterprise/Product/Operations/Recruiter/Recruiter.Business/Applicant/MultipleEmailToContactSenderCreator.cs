using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Recruiter.Business
{
	public class MultipleEmailToContactSenderCreator
	{
		public MultipleEmailToContactSender Create(HRRecruitmentJobCampaign campaign, IEnumerable<HRJobApplication> applicationsToSendTo, NotificationEmailTemplate template)
		{
			EmailToContactBusinessObjectCollection collection = new EmailToContactBusinessObjectCollection(campaign);
			MultipleEmailToContactSender result = new MultipleEmailToContactSender(collection);
			foreach (HRJobApplication application in applicationsToSendTo)
			{
				EmailToApplicantBusinessObject bizO = new EmailToApplicantBusinessObject(application);
				bizO.Subject = template.EmailSubject;
				bizO.Body = template.EmailBody;
				bizO.SubjectInfo.ValueChanged += HandleFromValueChanged(result, bizO, bizO.SubjectInfo.Name);
				bizO.BodyInfo.ValueChanged += HandleFromValueChanged(result, bizO, bizO.BodyInfo.Name);
				bizO.FromDisplayNameInfo.ValueChanged += HandleFromValueChanged(result, bizO, bizO.FromDisplayNameInfo.Name);
				bizO.FromEmailAddressInfo.ValueChanged += HandleFromValueChanged(result, bizO, bizO.FromEmailAddressInfo.Name);
				collection.Add(bizO);
			}
			return result;
		}

		static EventHandler HandleFromValueChanged(MultipleEmailToContactSender sender, EmailToApplicantBusinessObject bizOToCopyFrom, string propertyName)
		{
			return delegate
			{
				foreach (EmailToContactBusinessObject emailToContact in sender.EmailsToContacts)
				{
					if (emailToContact != bizOToCopyFrom)
					{
						emailToContact[propertyName] = bizOToCopyFrom[propertyName];
					}
				}
			};
		}
	}
}
