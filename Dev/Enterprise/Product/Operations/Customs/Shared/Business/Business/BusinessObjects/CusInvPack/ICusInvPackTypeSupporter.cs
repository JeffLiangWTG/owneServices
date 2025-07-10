using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public interface ICusInvPackTypeSupporter
	{
		Type PackType { get; }
		TypeDecider PackTypeDecider { get; }
	}
}
