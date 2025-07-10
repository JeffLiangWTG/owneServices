using System;
using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBookingInstructionTmplCollection))]
	class DtbBookingInstructionTmplCollectionTest : ActiveBusinessObjectCollectionTestCase<DtbBookingInstructionTmplCollection>
	{
		public void TestK2_Sequence()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			var instruction1 = template.Instructions.AddNew();
			var instruction2 = template.Instructions.AddNew();
			var instruction3 = template.Instructions.AddNew();
			AssertEquals(Convert.ToSByte(1), instruction1.K2_Sequence);
			AssertEquals(Convert.ToSByte(2), instruction2.K2_Sequence);
			AssertEquals(Convert.ToSByte(3), instruction3.K2_Sequence);

			instruction1.K2_Sequence = 2;
			instruction2.K2_Sequence = 3;
			instruction3.K2_Sequence = 1;
			AssertEquals(instruction3, template.Instructions[0]);
			AssertEquals(instruction1, template.Instructions[1]);
			AssertEquals(instruction2, template.Instructions[2]);
		}

		public void TestAllowNew()
		{
			var template = GetNewTemplate();
			var iBindingList = ((IBindingList)template.Instructions);
			AssertEquals(true, iBindingList.AllowNew);

			template.KT_IsSystem = true;
			AssertEquals(false, iBindingList.AllowNew);
		}

		protected override DtbBookingInstructionTmplCollection GetCollectionToTest()
		{
			return new DtbBookingInstructionTmplCollection(Factory.New<DtbBookingTmpl>());
		}

		DtbBookingTmpl GetNewTemplate()
		{
			return Helper.CreateTransportBookingTemplate("HEY", "YOU", Constants.CartageDirection.Import);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
