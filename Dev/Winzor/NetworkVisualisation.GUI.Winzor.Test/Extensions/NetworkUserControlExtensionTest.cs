using CargoWise.NetworkVisualisation.GUI;
using Enterprise.Winzor.Architecture.Test;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using WinzorFramework;

namespace NetworkVisualisation.GUI.Winzor.Test.Extensions;

[TestFixtureSource(typeof(NetworkUserControlExtensionTest), nameof(FixtureData))]
public class NetworkUserControlExtensionTest
{
	static async Task<INetworkUserControl> CreateControlWithContextAsync()
	{
		NetworkUserControl result = null!;
		using var ctx = new EnterpriseTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() => result = new NetworkUserControlBuilder().Build());
		return result;
	}

	static Task<INetworkUserControl> StubControlAsync()
	{
		var networkControlForTest = new NetworkUserControlForTestBuilder().Build();
		return Task.FromResult((INetworkUserControl)networkControlForTest);
	}

	static IEnumerable<TestFixtureData> FixtureData
	{
		get
		{
			yield return new ("Stub", (Func<Task<INetworkUserControl>>)StubControlAsync);
			yield return new ("Actual", (Func<Task<INetworkUserControl>>)CreateControlWithContextAsync);
		}
	}

	readonly Func<Task<INetworkUserControl>> controlFactory;
	WinzorDispatcher dispatcher;

	[SetUp]
	public void SetUp()
	{
#pragma warning disable CA2000 // Dispose objects before losing scope, to be addressed in a follow-up PR as this will likely require a larger refactor of this test fixture
		dispatcher = new EnterpriseTestContext().WinzorDispatcher;
#pragma warning restore CA2000 // Dispose objects before losing scope, to be addressed in a follow-up PR workflow as this will likely require a larger refactor of this test fixture
	}

	public NetworkUserControlExtensionTest(string name, Func<Task<INetworkUserControl>> controlFactory)
	{
		this.controlFactory = controlFactory;
	}

	[Test]
	public async Task CanAddEntityAsync()
	{
		var entity = Stub.Entity();
		var control = await controlFactory();

		Assert.That(control.NetworkViewModel.Network.Entities, Is.Empty);

		await dispatcher.InvokeAsync(() => control.Add(entity));

		Assert.That(control.NetworkViewModel.Network.Entities, Does.Contain(entity));
	}

	[Test]
	public async Task CanAddMultipleEntitiesAsync()
	{
		var a = Stub.Entity();
		var b = Stub.Entity();
		var control = await controlFactory();

		Assert.That(control.NetworkViewModel.Network.Entities, Is.Empty);

		await dispatcher.InvokeAsync(() => control.AddRange(a, b));

		Assert.That(control.NetworkViewModel.Network.Entities, Is.EquivalentTo(new [] { a, b }));
	}

	[Test]
	public async Task CanGetEntityByGuidAsync()
	{
		var entity = Stub.Entity();
		var control = await controlFactory();

		Assert.That(control.GetEntity(entity.EntityPK), Is.Null);

		await dispatcher.InvokeAsync(() => control.Add(entity));

		Assert.That(control.GetEntity(entity.EntityPK), Is.EqualTo(entity));
	}

	[Test]
	public async Task CanGetViewModelByEntityAsync()
	{
		var entity = Stub.Entity();
		var control = await controlFactory();

		Assert.That(control.ViewModelFor(entity), Is.Null);

		await dispatcher.InvokeAsync(() => control.Add(entity));

		var viewModel = control.ViewModelFor(entity)!;

		Assert.That(viewModel, Is.Not.Null);
		Assert.That(viewModel.Entity, Is.EqualTo(entity));
	}

	[Test]
	public async Task CanRemoveEntityAsync()
	{
		var entity = Stub.Entity();
		var control = await controlFactory();

		await dispatcher.InvokeAsync(() => control.Add(entity));

		Assert.That(control.NetworkViewModel.Network.Entities, Is.EquivalentTo(new [] { entity }));

		await dispatcher.InvokeAsync(() => control.Remove(entity));

		Assert.That(control.NetworkViewModel.Network.Entities, Is.Empty);

		Assert.That(control.GetEntity(entity.EntityPK), Is.Null);
		Assert.That(control.ViewModelFor(entity), Is.Null);
	}

	[Test]
	public async Task CanRemoveMultipleEntitiesAsync()
	{
		var a = Stub.Entity();
		var b = Stub.Entity();
		var control = await controlFactory();

		await dispatcher.InvokeAsync(() => control.AddRange(a, b));

		Assert.That(control.NetworkViewModel.Network.Entities, Is.EquivalentTo(new [] { a, b }));

		await dispatcher.InvokeAsync(() => control.RemoveRange(a, b));

		Assert.That(control.NetworkViewModel.Network.Entities, Is.Empty);
	}

	[Test]
	public async Task CanLinkEntitiesAsync()
	{
		var a = Stub.Entity();
		var b = Stub.Entity();
		var control = await controlFactory();

		await dispatcher.InvokeAsync(() => control.AddRange(a, b));

		Assert.That(control.NetworkViewModel.Connections.Count(), Is.EqualTo(0));

		await dispatcher.InvokeAsync(() => control.Link(a, b));

		Assert.That(control.NetworkViewModel.Connections.Count(), Is.EqualTo(1));

		var relationship = control.NetworkViewModel.Connections.Single()
			.Relationship;

		Assert.That(relationship.From, Is.SameAs(a));
		Assert.That(relationship.To, Is.SameAs(b));

		Assert.That(control.AreLinked(a, b), Is.True);
	}

	[Test]
	public async Task CanUnlinkEntitiesAsync()
	{
		var a = Stub.Entity();
		var b = Stub.Entity();
		var control = await controlFactory();

		await dispatcher.InvokeAsync(() =>
		{
			control.AddRange(a, b);
			control.Link(a, b);
		});

		Assert.That(control.AreLinked(a, b), Is.True);

		Assert.That(control.NetworkViewModel.Connections.Count(), Is.EqualTo(1));

		await dispatcher.InvokeAsync(() => control.Unlink(a, b));

		Assert.That(control.AreLinked(a, b), Is.False);

		Assert.That(control.NetworkViewModel.Connections.Count(), Is.EqualTo(0));
	}
}
