﻿using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH3327
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var t = session.BeginTransaction();
			var parent = new Entity { Name = "Parent" };
			var child = new ChildEntity { Name = "Child", Parent = parent };
			session.Save(parent);
			session.Save(child);
			t.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();
			session.CreateQuery("delete from ChildEntity").ExecuteUpdate();
			session.CreateQuery("delete from Entity").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void NotIsCorrectlyHandled()
		{
			using var session = OpenSession();
			var q = session.CreateQuery(
				@"SELECT COUNT(ROOT.Id)
					FROM Entity AS ROOT
					WHERE (
						EXISTS (FROM ChildEntity AS CHILD WHERE CHILD.Parent = ROOT)
						AND ROOT.Name = 'Parent'
					)");
			Assert.That(q.List()[0], Is.EqualTo(1));

			q = session.CreateQuery(
				@"SELECT COUNT(ROOT.Id)
					FROM Entity AS ROOT
					WHERE NOT (
						EXISTS (FROM ChildEntity AS CHILD WHERE CHILD.Parent = ROOT)
						AND ROOT.Name = 'Parent'
					)");
			Assert.That(q.List()[0], Is.EqualTo(0));
		}
	}
}
