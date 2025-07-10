using System;
using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class MergeHelperTest : TestCase
	{
		public void TestGetAllCoreActions_IncludesAllCoreMergeActionSubClasses()
		{
			var factory = new BusinessObjectFactory();
			var mergeData = new OrganisationMergeData(Guid.NewGuid(), Guid.NewGuid(), new MergeOrgAddressCollection(factory), new MergeOrgContactCollection(factory));
			var actionTypes = MergeHelper.GetAllCoreActions(mergeData).Select(a => a.GetType()).ToList();
			var coreMergeActionSubClasses = typeof(MergeHelper).Assembly.GetTypes()
				.Where(u => u.IsClass && !u.IsAbstract && u.IsSubclassOf(typeof(CoreMergeAction))).ToList();

			var missingActions = coreMergeActionSubClasses.Except(actionTypes).ToList();
			if (missingActions.Any())
			{
				Fail($"{nameof(MergeHelper.GetAllCoreActions)} should contain all non-abstract subclasses of {nameof(CoreMergeAction)}." + System.Environment.NewLine +
					$"These core merge actions need to be added in: {string.Join(", ", missingActions.Select(u => u.FullName))}");
			}
			AssertEquals(coreMergeActionSubClasses.Count, actionTypes.Count);
		}

		public void TestGetAllParameterisedSqlProviders_IncludesAllParameterisedSqlProviderSubClasses()
		{
			var providerTypes = MergeHelper.GetAllParameterisedSqlProviders().Select(a => a.GetType()).ToList();
			var sqlProviderSubClasses = typeof(MergeHelper).Assembly.GetTypes()
				.Where(u => u.IsClass && !u.IsAbstract && u.IsSubclassOf(typeof(OrgMergeParameterisedSqlProvider))).ToList();

			var missingProviders = sqlProviderSubClasses.Except(providerTypes).ToList();
			if (missingProviders.Any())
			{
				Fail($"{nameof(MergeHelper.GetAllParameterisedSqlProviders)} should contain all non-abstract subclasses of {nameof(OrgMergeParameterisedSqlProvider)}." + System.Environment.NewLine +
					$"These parameterised sql providers need to be added in: {string.Join(", ", missingProviders.Select(u => u.FullName))}");
			}
			AssertEquals(sqlProviderSubClasses.Count, providerTypes.Count);
		}
	}
}
