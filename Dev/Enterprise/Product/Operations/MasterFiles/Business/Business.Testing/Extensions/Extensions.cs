using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using FluentAssertions;
using FluentAssertions.Equivalency;
 
namespace Enterprise.MasterFiles.Business.Testing
{
	public static class Extensions
	{
		public static StmALog With(this StmALog log, string reference = "", bool isEstimate = false, string userCode = null, string table = null, ZDateTime? eventTime = null)
		{
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = reference;
				log.SL_IsEstimate = isEstimate;
				log.SL_GS_NKUser = userCode;
				log.SL_Table = table;
				log.SL_EventTime = eventTime ?? ZDateTime.Now;
			}

			return log;
		}

		public static OrgAddress With(this OrgAddress address, string oA_Code = null, string oA_Address1 = null)
		{
			if (oA_Address1 != null)
			{
				address.OA_Address1 = oA_Address1;
			}

			if (oA_Code != null)
			{
				address.OA_Code = oA_Code;
			}

			return address;
		}

		public static OrgHeader With(this OrgHeader header, OrgAddress address = null)
		{
			if (address != null)
			{
				header.Addresses.Add(address);
			}

			return header;
		}

		public static bool IsEquivalentTo<T>(this T obj, T otherObj)
		{
			return obj.IsEquivalentTo(otherObj, options => options.ComparingByMembers<T>());
		}

		static bool IsEquivalentTo<T>(this T obj, T otherObj, Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> options)
		{
			try
			{
				obj.Should().BeEquivalentTo(otherObj, options);
				return true;
			}
			catch (Exception)
			{
				throw;
			}
		}

		public static T GetAttribute<T>(this ZPropertyInfo info)
			where T : Attribute
		{
			return (T)info.PropertyDescriptor.Attributes[typeof(T)];
		}
	}
}
