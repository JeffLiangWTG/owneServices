using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	class CustomBodyModelValidatorFixture
	{
		[Test]
		public void ShouldValidateType()
		{
			var modelValidator = new Mock<IObjectModelValidator>();
			var customValidator = new CustomBodyModelValidator(modelValidator.Object);
			object model = 3;
			customValidator.Validate(new ActionContext(), null, "", model);
			modelValidator.Verify(x => x.Validate(It.IsAny<ActionContext>(), It.IsAny<ValidationStateDictionary>(), "", model), Times.Once);

			model = "abc";
			customValidator.Validate(new ActionContext(), null, "", model);
			modelValidator.Verify(x => x.Validate(It.IsAny<ActionContext>(), It.IsAny<ValidationStateDictionary>(), "", model));

			model = true;
			customValidator.Validate(new ActionContext(), null, "", model);
			modelValidator.Verify(x => x.Validate(It.IsAny<ActionContext>(), It.IsAny<ValidationStateDictionary>(), "", model));

			model = new SerializedGeometry();
			modelValidator.Invocations.Clear();
			customValidator.Validate(new ActionContext(), null, "", model);
			modelValidator.Verify(x => x.Validate(It.IsAny<ActionContext>(), It.IsAny<ValidationStateDictionary>(), "", model), Times.Never);
		}
	}
}
