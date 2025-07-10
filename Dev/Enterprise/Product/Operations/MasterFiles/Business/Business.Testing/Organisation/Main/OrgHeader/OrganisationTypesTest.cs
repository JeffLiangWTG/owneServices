using System;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrganisationTypesTest : TestCase
	{
		public void TestAllTypesCombinable()
		{
			foreach (OrganisationTypes type1 in Enum.GetValues(typeof(OrganisationTypes)))
			{
				foreach (OrganisationTypes type2 in Enum.GetValues(typeof(OrganisationTypes)))
				{
					if (type1 != type2 && type1 != OrganisationTypes.None && type2 != OrganisationTypes.None)
					{
						Assert("There must be no bitwise overlap between any OrganisationTypes", (type1 & type2) == 0);
					}
				}
			}
		}
	}
}
