using System.Linq;
using NodaTime;
using NodaTime.Text;
using Raven.Client.Documents.Commands;
using Raven.Client.Documents.Indexes;
using Sparrow.Json;
using Xunit;

namespace Raven.Client.NodaTime.Tests
{
    public class NodaYearMonthTests : MyRavenTestDriver
    {
        [Fact]
        public void Can_Use_NodaTime_YearMonth_In_Document_Today()
        {
            Can_Use_NodaTime_YearMonth_In_Document(NodaUtil.YearMonth.Today);
        }

        [Fact]
        public void Can_Use_NodaTime_YearMonth_In_Document_IsoMin()
        {
            Can_Use_NodaTime_YearMonth_In_Document(NodaUtil.YearMonth.MinIsoValue);
        }

        [Fact]
        public void Can_Use_NodaTime_YearMonth_In_Document_IsoMax()
        {
            Can_Use_NodaTime_YearMonth_In_Document(NodaUtil.YearMonth.MaxIsoValue);
        }

        private void Can_Use_NodaTime_YearMonth_In_Document(YearMonth ld)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", YearMonth = ld });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var foo = session.Load<Foo>("foos/1");

                    Assert.Equal(ld, foo.YearMonth);
                }

                using (var session = documentStore.OpenSession())
                {
                    var command = new GetDocumentsCommand("foos/1", null, false);
                    session.Advanced.RequestExecutor.Execute(command, session.Advanced.Context);
                    var json = (BlittableJsonReaderObject)command.Result.Results[0];
                    System.Diagnostics.Debug.WriteLine(json.ToString());
                    var expected = ld.ToString(YearMonthPattern.Iso.PatternText, null);
                    json.TryGet("YearMonth", out string value);
                    Assert.Equal(expected, value);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_YearMonth_In_Dynamic_Index_Today()
        {
            Can_Use_NodaTime_YearMonth_In_Dynamic_Index1(NodaUtil.YearMonth.Today);
        }

        [Fact]
        public void Can_Use_NodaTime_YearMonth_In_Dynamic_Index_IsoMin()
        {
            Can_Use_NodaTime_YearMonth_In_Dynamic_Index1(NodaUtil.YearMonth.MinIsoValue);
        }

        [Fact]
        public void Can_Use_NodaTime_YearMonth_In_Dynamic_Index_IsoMax()
        {
            Can_Use_NodaTime_YearMonth_In_Dynamic_Index2(NodaUtil.YearMonth.MaxIsoValue);
        }

        private void Can_Use_NodaTime_YearMonth_In_Dynamic_Index1(YearMonth ld)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", YearMonth = ld });
                    session.Store(new Foo { Id = "foos/2", YearMonth = ld + Period.FromDays(1) });
                    session.Store(new Foo { Id = "foos/3", YearMonth = ld + Period.FromDays(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.YearMonth == ld);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.YearMonth > ld)
                                    .OrderByDescending(x => x.YearMonth);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].YearMonth > results2[1].YearMonth);

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.YearMonth >= ld)
                                    .OrderByDescending(x => x.YearMonth);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].YearMonth > results3[1].YearMonth);
                    Assert.True(results3[1].YearMonth > results3[2].YearMonth);
                }
            }
        }

        private void Can_Use_NodaTime_YearMonth_In_Dynamic_Index2(YearMonth ld)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", YearMonth = ld });
                    session.Store(new Foo { Id = "foos/2", YearMonth = ld - Period.FromDays(1) });
                    session.Store(new Foo { Id = "foos/3", YearMonth = ld - Period.FromDays(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.YearMonth == ld);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.YearMonth < ld)
                                    .OrderBy(x => x.YearMonth);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].YearMonth < results2[1].YearMonth);

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.YearMonth <= ld)
                                    .OrderBy(x => x.YearMonth);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].YearMonth < results3[1].YearMonth);
                    Assert.True(results3[1].YearMonth < results3[2].YearMonth);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_YearMonth_In_Static_Index_Today()
        {
            Can_Use_NodaTime_YearMonth_In_Static_Index1(NodaUtil.YearMonth.Today);
        }

        [Fact]
        public void Can_Use_NodaTime_YearMonth_In_Static_Index_IsoMin()
        {
            Can_Use_NodaTime_YearMonth_In_Static_Index1(NodaUtil.YearMonth.MinIsoValue);
        }

        [Fact]
        public void Can_Use_NodaTime_YearMonth_In_Static_Index_IsoMax()
        {
            Can_Use_NodaTime_YearMonth_In_Static_Index2(NodaUtil.YearMonth.MaxIsoValue);
        }

        private void Can_Use_NodaTime_YearMonth_In_Static_Index1(YearMonth ld)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", YearMonth = ld });
                    session.Store(new Foo { Id = "foos/2", YearMonth = ld + Period.FromDays(1) });
                    session.Store(new Foo { Id = "foos/3", YearMonth = ld + Period.FromDays(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.YearMonth == ld);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.YearMonth > ld)
                                    .OrderByDescending(x => x.YearMonth);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].YearMonth > results2[1].YearMonth);

                    var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.YearMonth >= ld)
                                    .OrderByDescending(x => x.YearMonth);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].YearMonth > results3[1].YearMonth);
                    Assert.True(results3[1].YearMonth > results3[2].YearMonth);
                }
            }
        }

        private void Can_Use_NodaTime_YearMonth_In_Static_Index2(YearMonth ld)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", YearMonth = ld });
                    session.Store(new Foo { Id = "foos/2", YearMonth = ld - Period.FromDays(1) });
                    session.Store(new Foo { Id = "foos/3", YearMonth = ld - Period.FromDays(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.YearMonth == ld);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.YearMonth < ld)
                                    .OrderBy(x => x.YearMonth);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].YearMonth < results2[1].YearMonth);

                    var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.YearMonth <= ld)
                                    .OrderBy(x => x.YearMonth);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].YearMonth < results3[1].YearMonth);
                    Assert.True(results3[1].YearMonth < results3[2].YearMonth);
                }
            }
        }

        public class Foo
        {
            public string Id { get; set; }
            public YearMonth YearMonth { get; set; }
        }

        public class TestIndex : AbstractIndexCreationTask<Foo>
        {
            public TestIndex()
            {
                Map = foos => from foo in foos
                              select new
                              {
                                  foo.YearMonth
                              };
            }
        }
    }
}
