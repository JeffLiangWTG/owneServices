using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration;

public interface IValidationToolRequestsHandle
{
	void Handle(BusinessObjectFactory factory, IList<ValidationToolRequestHandleParameter> handleParameters);
}

