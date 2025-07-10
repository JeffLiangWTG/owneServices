using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Blazor.Common;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test;

public class SessionSecretStoreTests
{
	[Test]
	public void SessionSecretStoreIsAThreadSafeType()
	{
		Assert.That(typeof(SessionSecretStore).BaseType, Is.EqualTo(typeof(ConcurrentDictionary<string, string>)));
	}

	[Test]
	public void SessionSecretStoreStoresAndRetrievesKeysSuccessfully()
	{
		var uniqueId = Guid.NewGuid().ToString();
		var secret = new SecureSecretGenerator().Generate();
		var store = new SessionSecretStore();
		var added = store.TryAdd(uniqueId, secret);

		Assert.That(added, Is.True);
		Assert.That(store.Count, Is.EqualTo(1));
		Assert.That(store.Single().Key, Is.EqualTo(uniqueId));
		Assert.That(store.Single().Value, Is.EqualTo(secret));
	}

	[Test]
	public void SessionSecretStoreStoresAndRemovesKeysSuccessfully()
	{
		var uniqueId = Guid.NewGuid().ToString();
		var secret = new SecureSecretGenerator().Generate();
		var store = new SessionSecretStore();
		var added = store.TryAdd(uniqueId, secret);

		Assert.That(added, Is.True);
		Assert.That(store.Count, Is.EqualTo(1));
		Assert.That(store.Single().Key, Is.EqualTo(uniqueId));
		Assert.That(store.Single().Value, Is.EqualTo(secret));

		var removed = store.Remove(uniqueId, out var value);

		Assert.That(removed, Is.True);
		Assert.That(store.Count, Is.EqualTo(0));
		Assert.That(value, Is.EqualTo(secret));
	}
}