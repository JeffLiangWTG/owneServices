using System;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.ContainerMovement.Testing
{
	internal class ContainerMovementRelatedMovementHelperTest : TestCaseWithFactory
	{
		public void TestGuessVoyageFromContainerHistory()
		{
			ZDateTime today = ZDateTime.Today;
			ZDateTime epoch = new ZDateTime(today.Year, today.Month, 1);
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_VoyageFlight = "Voyage1";
			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_VoyageFlight = "Voyage2";
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			AddMovement(stock, epoch.AddDays(1), ContainerMovementTypes.Codes.YardGateOut, voyage1.PK);
			AddMovement(stock, epoch.AddDays(3), ContainerMovementTypes.Codes.DepotGateIn, ZGuid.Empty);
			AddMovement(stock, epoch.AddDays(5), ContainerMovementTypes.Codes.DepotGateOut, ZGuid.Empty);
			AddMovement(stock, epoch.AddDays(7), ContainerMovementTypes.Codes.WharfGateIn, voyage1.PK);
			AddMovement(stock, epoch.AddDays(9), ContainerMovementTypes.Codes.WharfGateOut, ZGuid.Empty);
			AddMovement(stock, epoch.AddDays(11), ContainerMovementTypes.Codes.WharfGateIn, voyage2.PK);
			AddMovement(stock, epoch.AddDays(13), ContainerMovementTypes.Codes.Load, voyage2.PK);
			AddMovement(stock, epoch.AddDays(15), ContainerMovementTypes.Codes.Discharge, voyage2.PK);
			AddMovement(stock, epoch.AddDays(17), ContainerMovementTypes.Codes.WharfGateOut, voyage2.PK);
			AddMovement(stock, epoch.AddDays(19), ContainerMovementTypes.Codes.DepotGateIn, ZGuid.Empty);
			AddMovement(stock, epoch.AddDays(21), ContainerMovementTypes.Codes.DepotGateOut, ZGuid.Empty);
			AddMovement(stock, epoch.AddDays(23), ContainerMovementTypes.Codes.YardGateIn, ZGuid.Empty);
			AddMovement(stock, epoch.AddDays(25), ContainerMovementTypes.Codes.YardGateOut, ZGuid.Empty);
			AddMovement(stock, epoch.AddDays(27), ContainerMovementTypes.Codes.DepotGateIn, ZGuid.Empty);
			AddMovement(stock, epoch.AddDays(29), ContainerMovementTypes.Codes.DepotGateOut, ZGuid.Empty);
			AddMovement(stock, epoch.AddDays(31), ContainerMovementTypes.Codes.ReturnedUnshipped, ZGuid.Empty);
			AddMovement(stock, epoch.AddDays(33), ContainerMovementTypes.Codes.RePositionOutOfYard, ZGuid.Empty);
			AddMovement(stock, epoch.AddDays(35), ContainerMovementTypes.Codes.RePositionIntoYard, ZGuid.Empty);
			var voyageChanges = new[] { new
			{
			Date = epoch.AddDays(1), Voyage = voyage1
			}

			, new
			{
			Date = epoch.AddDays(11), Voyage = voyage2
			}

			, new
			{
			Date = epoch.AddDays(13), Voyage = ((JobVoyage)null) }

			, new
			{
			Date = epoch.AddDays(15), Voyage = voyage2
			}

			, new
			{
			Date = epoch.AddDays(23), Voyage = ((JobVoyage)null) }

			, };
			Factory.Save();
			var movements = stock.Movements.ToArray();
			Array.Sort(movements, (m1, m2) => m1.E9_MovementDate.CompareTo(m2.E9_MovementDate));
			bool fail = false;
			StringBuilder builder = new StringBuilder();
			builder.Append("<br /><br /><pre>");
			{
				JobVoyage actualVoyage = CMMRelatedMovementHelper.GuessVoyageFromContainerHistory(stock, movements[0].E9_MovementDate.AddHours(-1));
				if (actualVoyage != null)
				{
					builder.AppendFormat("<span style=\"color: red;\">+-- expected &lt;null&gt but was {0}</span><br />", Html(actualVoyage.JV_VoyageFlight));
					fail = true;
				}
				else
				{
					builder.Append("<span style=\"color: green;\">+-- expected and found &lt;null&gt;</span><br />");
				}
			}

			for (int i = 0; i < movements.Length; i++)
			{
				var movement = movements[i];
				builder.Append(movement.E9_MovementDate.ToShortDateString());
				builder.Append(" ");
				builder.Append(movement.E9_MovementType);
				if (!movement.E9_JV.IsEmpty)
				{
					builder.Append(" (");
					builder.Append(movement.Voyage.JV_VoyageFlight);
					builder.Append(")");
				}

				builder.Append("<br />");
				ZDateTime measureInstant = movement.E9_MovementDate.AddHours(1);
				JobVoyage expectedVoyage = null;
				JobVoyage actualVoyage = CMMRelatedMovementHelper.GuessVoyageFromContainerHistory(stock, measureInstant);
				{
					for (int j = 0; j < voyageChanges.Length; j++)
					{
						if (measureInstant > voyageChanges[j].Date)
						{
							expectedVoyage = voyageChanges[j].Voyage;
						}
						else
						{
							break;
						}
					}
				}

				if (expectedVoyage != actualVoyage)
				{
					builder.AppendFormat("<span style=\"color: red;\">+-- expected {0} but was {1}</span><br />", expectedVoyage == null ? "&lt;null&gt" : Html(expectedVoyage.JV_VoyageFlight), actualVoyage == null ? "&lt;null&gt" : Html(actualVoyage.JV_VoyageFlight));
					fail = true;
				}
				else
				{
					builder.AppendFormat("<span style=\"color: green;\">+-- expected and found {0}</span><br />", expectedVoyage == null ? "&lt;null&gt" : Html(expectedVoyage.JV_VoyageFlight));
				}
			}

			builder.Append("</pre>");
			HtmlAssert(builder.ToString(), !fail);
		}

		#region Implementation
		void AddMovement(RefContainerStock stock, ZDateTime dateTime, string movementType, ZGuid voyagePK)
		{
			var movement = stock.Movements.AddNew();
			movement.E9_MovementDate = dateTime;
			movement.E9_MovementType = movementType;
			movement.E9_JV = voyagePK;
		}
		#endregion
	}
}
