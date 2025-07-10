namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders
{
	using System.Collections.Generic;
	using CargoWise.Types;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;
	using Enterprise.MasterFiles.Business;

	class MAFAccountDetailsWrapper : IMAFAccountDetails
	{
		MAFAccountDetailsWrapper() { }

		#region GetAccountDetails

		public static IMAFAccountDetails GetAccountDetails(Dictionary<string, OrgHeader> orgs)
		{
			var result = new MAFAccountDetailsWrapper();
			foreach (var pair in orgs)
			{
				var org = pair.Value;
				if (org != null)
				{
					var accountNumber = org.CustomsCodes.GetCustomsRegNo(OrgCusCode.NZCodeTypes.MAFCoverSheetQE);
					if (!accountNumber.IsEmpty)
					{
						result.AccountNumber = accountNumber;
						result.AccountHolderName = org.OH_FullNameTruncated;
						result.AccountHolderDescription = pair.Key;
						return result;
					}
				}
			}

			result.WhereToSetupAccountDetailsDescription =
				string.Format("You can setup a default MPI QE # against {0}.",
							  new ZStringBuilder(orgs.Keys).ToStringWithDelimiterBetweenAppends("/"));

			return result;
		}

		public static IMAFAccountDetails GetAccountDetails(ZString number, ZString name)
		{
			return new MAFAccountDetailsWrapper { AccountNumber = number, AccountHolderName = name };
		}

		#endregion

		#region Implementation of IMAFAccountDetails

		public ZString AccountNumber { get; private set; }
		public ZString AccountHolderName { get; private set; }
		public ZString AccountHolderDescription { get; private set; }
		public ZString WhereToSetupAccountDetailsDescription { get; private set; }

		#endregion
	}
}
