﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2808
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override void OnTearDown()
		{
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				session.Delete("from System.Object");
				session.Flush();
				transaction.Commit();
			}

			base.OnTearDown();
		}

		[Test]
		public async Task CheckExistanceOfEntityAsync()
		{
			// save an instance of Entity1
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				var a = new Entity { Name = "A" };
				await (session.SaveAsync("Entity1", a, 1));

				await (transaction.CommitAsync());
			}

			// check that it is correctly stored in the Entity1 table and does not exist in the Entity2 table.
			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				var a = await (session.GetAsync("Entity1", 1));
				Assert.IsNotNull(a);

				a = await (session.GetAsync("Entity2", 1));
				Assert.IsNull(a);
			}
		}

		[Test]
		public async Task UpdateAsync()
		{
			// save an instance of Entity1
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				var a = new Entity { Name = "A" };
				await (session.SaveAsync("Entity1", a, 1));

				await (transaction.CommitAsync());
			}

			// load the saved entity, change its name and update.
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				var a = (Entity)await (session.GetAsync("Entity1", 1));
				a.Name = "A'";

				await (session.UpdateAsync("Entity1",a, 1));
				
				await (transaction.CommitAsync());
			}

			// verify
			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				var a = (Entity)await (session.GetAsync("Entity1", 1));

				Assert.AreEqual("A'", a.Name);
			}
		}

		[Test]
		public async Task SaveOrUpdateAsync()
		{
			// save an instance of Entity1.
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				var a = new Entity { Name = "A" };
				await (session.SaveAsync("Entity1", a, 1));

				await (transaction.CommitAsync());
			}

			// load the entity and adjust its name, create a new entity and save or update both.
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				var a = (Entity)await (session.GetAsync("Entity1", 1));
				a.Name = "A'";

				var b = new Entity {Name = "B"};

				await (session.SaveOrUpdateAsync("Entity1", a, 1));
				await (session.SaveOrUpdateAsync("Entity1", b, 2));

				await (transaction.CommitAsync());
			}

			// verify
			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				var a = (Entity)await (session.GetAsync("Entity1", 1));
				var b = (Entity)await (session.GetAsync("Entity1", 2));

				Assert.AreEqual("A'", a.Name);
				Assert.AreEqual("B", b.Name);
			}
		}
	}
}
