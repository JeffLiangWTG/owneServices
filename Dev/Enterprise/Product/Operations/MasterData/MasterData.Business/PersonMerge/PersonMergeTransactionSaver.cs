using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterData.Business
{
	public class PersonMergeTransactionSaver : IPersonMergeTransactionSaver
	{
		public const string StoredProcedureName = "PersonsReassignFKToOtherPerson";

		List<IFactory> factories;

		public List<IFactory> Factories
		{
			get { return factories ?? (factories = new List<IFactory>()); }
		}

		public IPersonMergeTransactionSaver AddParticipant(IFactory factory)
		{
			Factories.Add(factory);

			return this;
		}

		public bool IsSuccessful { get; private set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public virtual void Save(GlbPerson retainedPerson, GlbPerson dissolvedPerson)
		{
			try
			{
				Db.Connection.RunTransactioned(() =>
				{
					foreach (var factory in Factories)
					{
						factory.Save();
					}

					var dissolvedPersonFactory = dissolvedPerson.Factory;

					using (DbCommand cmd = Db.Connection.Command(StoredProcedureName)) // PersonsMergeFKReassigner uses complex SQL scripts that can't be accomplished by using Business Objects
					{
						cmd.AddParameter("@RetainedParentPk", SqlDbType.UniqueIdentifier, retainedPerson.PK.ToGuid());
						cmd.AddParameter("@DissolvedParentPk", SqlDbType.UniqueIdentifier, dissolvedPerson.PK.ToGuid());
						cmd.AddParameter("@FkSystemLastEditUser", SqlDbType.VarChar, GlbStaff.CurrentUser?.GS_Code.ToString() ?? User.UnKnownUserCode);
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.ExecuteNonQuery();
					}

					dissolvedPerson.Delete();
					dissolvedPersonFactory.Save();
					IsSuccessful = true;
				});
			}
			catch (SqlException ex) when (!ex.IsCriticalException())
			{
				ZString message = (NoResString)"PersonsReassignFKToOtherPerson has failed to resolve dissolved person FK reassigning.";
				ExceptionReporter.Instance.ReportDeveloperException(message, ex);
				throw;
			}
		}
	}
}
