#if DEBUG

using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationValidTestDataHelper<TBizo> where TBizo : BusinessObject, IDeduplicatable
	{
		readonly BusinessObjectFactory factory;
		readonly Dictionary<int, TBizo> table = new Dictionary<int, TBizo>();
		TBizo currentBizO;

		public DeduplicationValidTestDataHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public DeduplicationValidTestDataHelper<TBizo> Add(Action<TBizo> bizoPropertiesAction)
		{
			var bizO = factory.NewWithValidTestData<TBizo>();

			bizoPropertiesAction(bizO);
			table.Add(table.Count, bizO);
			currentBizO = bizO;

			return this;
		}

		public DeduplicationValidTestDataHelper<TBizo> AddChildren(Action<TBizo> action)
		{
			action(currentBizO);

			return this;
		}

		public SortedList<int, TBizo> ToSortedData()
		{
			var sortedList = new SortedList<int, TBizo>();

			foreach (var item in table)
			{
				sortedList.Add(item.Key, item.Value);
			}

			return sortedList;
		}
	}

	public static class GlbPersonDeduplicationTestData
	{
		public static SortedList<int, GlbPerson> NewValidTestData(BusinessObjectFactory factory)
		{
			var helper = new DeduplicationValidTestDataHelper<GlbPerson>(factory);

			return helper
				.Add(p =>
				{
					UpdatePerson(p, "Alex Zverev", "0449743938");
				})
				.Add(p =>
				{
					UpdatePerson(p, "Alexander Zverev", "0449743938");
				})
				.Add(p =>
				{
					UpdatePerson(p, "Alexander Zellar", "0449743938");
				})
				.Add(p =>
				{
					UpdatePerson(p, "Alex Zverev", "0449743938");
				}).AddChildren(p =>
				{
					AddContactForPerson(p, "Alex Zverev", "+61 449743938");
				})
				.Add(p =>
				{
					UpdatePerson(p, "Alex Zverev", "0449743938");
				}).AddChildren(p =>
				{
					AddContactForPerson(p, "Alex Zverev", "+61 449743938");
				})
				.Add(p =>
				{
					UpdatePerson(p, "Alex Swarez", "0449743938");
				}).AddChildren(p =>
				{
					AddContactForPerson(p, "Alex Swarez", "0449743938");
				}).AddChildren(p =>
				{
					AddContactForPerson(p, "Alex Swarey", "0449743938");
				}).AddChildren(p =>
				{
					AddContactForPerson(p, "Alex Swarex", "0449743938");
				}).AddChildren(p =>
				{
					AddContactForPerson(p, "Alex Swarez", "0449743938");
				})
				.ToSortedData();
		}

		static void UpdatePerson(GlbPerson person, ZString fullName, ZString mobilePhone)
		{
			person.PER_FullName = fullName;
			person.PER_MobilePhone = mobilePhone;
		}

		static void AddContactForPerson(GlbPerson person, ZString fullName, ZString phone)
		{
			var contact = person.ContactCollection.AddNew();
			contact.OC_ContactName = fullName;
			contact.OC_Phone = phone;
			contact.OC_OH = person.Factory.NewWithValidTestData<OrgHeader>().PK;
		}
	}

	public static class OrgHeaderDeduplicationTestData
	{
		public static SortedList<int, TBizo> NewValidTestData<TBizo>(BusinessObjectFactory factory)
			where TBizo : BusinessObject, IDeduplicatable
		{
			var sortedList = new SortedList<int, TBizo>();
			var items = NewValidTestData(factory);

			foreach (var item in items)
			{
				sortedList.Add(item.Key, (TBizo)(object)item.Value);
			}

			return sortedList;
		}

		public static SortedList<int, OrgHeader> NewValidTestData(BusinessObjectFactory factory)
		{
			var helper = new DeduplicationValidTestDataHelper<OrgHeader>(factory);

			return helper
				.Add(org =>
				{
					UpdateOrganisation(org, "TOLL PTY LTD", "ABX", "AUSYD");
				}).AddChildren(org =>
				{
					AddOrganisationContact(org, "Contact A", "0449743938");
					AddOrganisationContact(org, "Contact B", "0449743938");
					AddOrganisationAddress(org, "Bnt Crescent", "0449743938");
					AddOrganisationAddress(org, "River Drive", "0449743938");
				})
				.Add(org =>
				{
					UpdateOrganisation(org, "TOLL PTY", "ABV", "AUSYD");
				}).AddChildren(org =>
				{
					AddOrganisationContact(org, "Contact A", "0449743938");
					AddOrganisationContact(org, "Contact B", "0449743938");
					AddOrganisationAddress(org, "Bnt Crescent", "0449743938");
					AddOrganisationAddress(org, "River Drive", "0449743938");
				})
				.Add(org =>
				{
					UpdateOrganisation(org, "TOLL LTD", "TEST-O1", "AUSYD");
				})
				.Add(org =>
				{
					UpdateOrganisation(org, "TOLL LTD", "TEST-O2", "AUSYD");
				})
				.Add(org =>
				{
					UpdateOrganisation(org, "TOLL LTD", "TEST-O3", "AUSYD");
				})
				.Add(org =>
				{
					UpdateOrganisation(org, "TOLL LTD", "TEST-O4", "AUSYD");
				})
				.ToSortedData();
		}

		static void UpdateOrganisation(OrgHeader header, ZString fullName, ZString code, ZString closestPort)
		{
			header.OH_FullName = fullName;
			header.OH_Code = code;
			header.OH_RL_NKClosestPort = closestPort;
		}

		static void AddOrganisationContact(OrgHeader header, ZString contactFullName, ZString contactPhone)
		{
			var contact = header.Contacts.AddNew();
			contact.OC_ContactName = contactFullName;
			contact.OC_Phone = contactPhone;
		}

		static void AddOrganisationAddress(OrgHeader header, ZString address1, ZString phone)
		{
			var address = header.Addresses.AddNew();
			address.Address1 = address1;
			address.OA_Phone = phone;
		}
	}
}

#endif
