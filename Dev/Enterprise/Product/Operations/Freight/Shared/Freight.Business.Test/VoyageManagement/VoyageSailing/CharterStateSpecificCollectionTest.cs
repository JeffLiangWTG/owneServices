using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class CharterStateSpecificCollectionTest : BusinessObjectCollectionTestCase
	{
		#region TestDefaultFilter

		public void TestDefaultFilter()
		{
			BusinessObjectCollection collection = GetCollectionToTest();
			AssertEquals(ModuleFilterCode, collection.FilterBusinessObjectDefaults["Charter:Property"].Value);
		}

		#endregion

		#region TestAdditionalFilter

		public void TestAdditionalFilter()
		{
			JobSailing charteredSailing = CreateSailing(true);
			JobSailing nonCharteredSailing = CreateSailing(false);

			Factory.Save();

			BusinessObjectCollection collection = GetCollectionToTest();
			collection.Load();

			AssertEquals(IsCharter, collection.Contains(charteredSailing));
			AssertEquals(!IsCharter, collection.Contains(nonCharteredSailing));
		}

		#endregion

		#region TestAddErrors

		public void TestAddErrors()
		{
			JobSailing charteredSailing = CreateSailing(true);
			JobSailing nonCharteredSailing = CreateSailing(false);

			BusinessObjectCollection collection = GetCollectionToTest();

			if (IsCharter)
			{
				AssertNotContains(ErrorText, ErrorText, collection.GetAllNotificationsWhenAdditionalFilterNotMet(charteredSailing));
				AssertContains(ErrorText, ErrorText, collection.GetAllNotificationsWhenAdditionalFilterNotMet(nonCharteredSailing));
			}
			else
			{
				AssertContains(ErrorText, ErrorText, collection.GetAllNotificationsWhenAdditionalFilterNotMet(charteredSailing));
				AssertNotContains(ErrorText, ErrorText, collection.GetAllNotificationsWhenAdditionalFilterNotMet(nonCharteredSailing));
			}
		}

		#endregion

		#region Implementation

		JobSailing CreateSailing(bool chartered)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_IsChartered = chartered;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			return voyage.Sailings[0];
		}

		protected abstract ZBool IsCharter { get; }
		protected abstract ZString ErrorText { get; }
		protected abstract ZString ModuleFilterCode { get; }

		#endregion
	}
}
