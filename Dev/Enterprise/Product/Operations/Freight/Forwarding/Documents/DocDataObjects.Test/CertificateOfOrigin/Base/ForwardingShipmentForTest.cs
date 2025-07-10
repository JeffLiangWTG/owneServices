using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Integration.Customs;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin.Base
{
	public class ForwardingShipmentForTest : ForwardingShipment
	{
		public ForwardingShipmentForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override AutologState AutoLoggingState => AutologState.NotLogged;

		public override IBaseJobDeclaration[] Declarations => declarations;
		IBaseJobDeclaration[] declarations = Array.Empty<IBaseJobDeclaration>();

		public void SetDeclaration(IBaseJobDeclaration[] declaration)
		{
			_ = declaration ?? throw new ArgumentNullException(nameof(declaration));
			this.declarations = declaration;
		}
	}
}
