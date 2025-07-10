using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(UpdateDetentionDaysApplicator))]
	internal class UpdateDetentionDaysApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestBadMovementType()
		{
			ContainerMovement movement = NewMovement(Stock, ContainerMovementTypes.Codes.WharfGateOut, ZDateTime.Now.AddDays(-1));
			Factory.Save();
			const string expected = @"
INFO: Updating movements for [HL TEST4100013].
INFO: The [HL Wharf Gate Out] movement does not support detentions.
";
			CombineAssertions(delegate
			{
				ApplyApplicator(new BusinessObject[] { movement }, expected.Trim(), true);
				AssertEquals("Detention Days", (short)0, movement.E9_DetentionDays);
			});
		}

		public void TestDetentionDaysUnchanged()
		{
			ZDateTime now = ZDateTime.Now;
			ContainerMovement ygo = NewMovement(Stock, ContainerMovementTypes.Codes.YardGateOut, now.AddDays(-20));
			ContainerMovement rus = NewMovement(Stock, ContainerMovementTypes.Codes.ReturnedUnshipped, now.AddDays(-1));
			Factory.Save();
			const string expected = @"
INFO: Updating movements for [HL TEST4100013].
INFO: The number of detention days for [HL Returned Un-Shipped] is already set to 10 and does not need to be updated.
";
			CombineAssertions(delegate
			{
				ApplyApplicator(new BusinessObject[] { rus }, expected.Trim(), true);
				AssertEquals("Detention Days", (short)10, rus.E9_DetentionDays);
			});
		}

		public void TestPostedInvoice()
		{
			ZDateTime now = ZDateTime.Now;
			ContainerMovement ygo = NewMovement(Stock, ContainerMovementTypes.Codes.YardGateOut, now.AddDays(-20));
			ContainerMovement rus = NewMovement(Stock, ContainerMovementTypes.Codes.ReturnedUnshipped, now.AddDays(-1));
			rus.E9_DetentionDays = 3;
			ContainerDetention detention = NewExportDetention(rus);
			AddPostedCharge(detention);
			Factory.Save();
			const string expected = @"
INFO: Updating movements for [HL TEST4100013].
WARNING: Cannot update the detention days for the [HL Returned Un-Shipped] movement from 3 to 10 as it already has a posted invoice on [HL DI00000001].
";
			CombineAssertions(delegate
			{
				ApplyApplicator(new BusinessObject[] { rus }, expected.Trim(), true);
				AssertEquals("Detention Days", (short)3, rus.E9_DetentionDays);
			});
		}

		public void TestNonPostedInvoice()
		{
			ZDateTime now = ZDateTime.Now;
			ContainerMovement ygo = NewMovement(Stock, ContainerMovementTypes.Codes.YardGateOut, now.AddDays(-20));
			ContainerMovement rus = NewMovement(Stock, ContainerMovementTypes.Codes.ReturnedUnshipped, now.AddDays(-1));
			rus.E9_DetentionDays = 3;
			ContainerDetention detention = NewExportDetention(rus);
			Factory.Save();
			const string expected = @"
INFO: Updating movements for [HL TEST4100013].
WARNING: Updating the number of detention days for [HL Returned Un-Shipped] from 3 to 10 however it is currently attached to the detention invoice [HL DI00000001].
";
			CombineAssertions(delegate
			{
				ApplyApplicator(new BusinessObject[] { rus }, expected.Trim(), true);
				AssertEquals("Detention Days", (short)10, rus.E9_DetentionDays);
			});
		}

		public void TestNotInvoiced()
		{
			ZDateTime now = ZDateTime.Now;
			ContainerMovement ygo = NewMovement(Stock, ContainerMovementTypes.Codes.YardGateOut, now.AddDays(-20));
			ContainerMovement rus = NewMovement(Stock, ContainerMovementTypes.Codes.ReturnedUnshipped, now.AddDays(-1));
			rus.E9_DetentionDays = 3;
			Factory.Save();
			const string expected = @"
INFO: Updating movements for [HL TEST4100013].
INFO: Updating the number of detention days for [HL Returned Un-Shipped] from 3 to 10.
";
			CombineAssertions(delegate
			{
				ApplyApplicator(new BusinessObject[] { rus }, expected.Trim(), true);
				AssertEquals("Detention Days", (short)10, rus.E9_DetentionDays);
			});
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new UpdateDetentionDaysApplicator();
		}

		RefContainerStock Stock
		{
			get
			{
				if (stock == null)
				{
					stock = Factory.New<RefContainerStock>();
					stock.R6_ContainerNum = "TEST4100013";
					stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				}

				return stock;
			}
		}

		RefContainerStock stock;
		OrgHeader Client
		{
			get
			{
				if (client == null)
				{
					client = Factory.NewWithValidTestData<OrgHeader>();
					client.OH_Code = "Client";
				}

				return client;
			}
		}

		OrgHeader client;
		OrgHeader Principal
		{
			get
			{
				if (principal == null)
				{
					principal = Factory.NewWithValidTestData<OrgHeader>();
					principal.OH_Code = "Principal";
				}

				return principal;
			}
		}

		OrgHeader principal;
		AccChargeCode ChargeCode
		{
			get
			{
				if (chargeCode == null)
				{
					chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
					chargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
					chargeCode.AC_Code = "QXZ";
					chargeCode.SetGLAccountDataForTesting();
				}

				return chargeCode;
			}
		}

		AccChargeCode chargeCode;
		void AddPostedCharge(ContainerDetention detention)
		{
			JobHeader header = new JobHeader.Loader(detention).TryLoadOrCreate();
			AccTransactionHeader invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			AccTransactionLines line = Factory.New<AccTransactionLines>();
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_AH = invoice.PK;
			line.AL_AC = ChargeCode.PK;
			line.AL_GE = header.JH_GE;
			line.AL_GB = header.JH_GB;
			line.AL_JH = header.PK;
			JobCharge charge = (JobCharge)((IBusinessObjectCollection)header["Charges"]).AddNew();
			charge.JR_AC = ChargeCode.PK;
			charge.JR_AL_ARLine = line.PK;
		}

		ContainerMovement NewMovement(RefContainerStock stock, ZString movementType, ZDateTime movementDateTime)
		{
			ContainerMovement movement = stock.Movements.AddNew();
			movement.E9_MovementType = movementType;
			movement.E9_MovementDate = movementDateTime;
			movement.E9_OH_Principal = Principal.PK;
			movement.E9_OH_ResponsibleParty = Client.PK;
			return movement;
		}

		ContainerDetention NewExportDetention(params ContainerMovement[] movements)
		{
			ContainerDetention detention = Factory.New<ContainerDetention>();
			detention.NC_DetentionType = DetentionInvoiceType.Codes.Export;
			detention.NC_OH_Principal = Principal.PK;
			detention.NC_OH_Client = Client.PK;
			for (int i = 0; i < movements.Length; i++)
			{
				detention.Movements.Add(movements[0]);
			}

			return detention;
		}
		#endregion
	}
}
