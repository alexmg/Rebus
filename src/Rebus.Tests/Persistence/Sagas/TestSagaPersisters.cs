using System;
using NUnit.Framework;
using Ponder;
using Rebus.Tests.Persistence.Sagas.Factories;
using Shouldly;

namespace Rebus.Tests.Persistence.Sagas
{
    [TestFixture(typeof(InMemorySagaPersisterFactory))]
    [TestFixture(typeof(MongoDbSagaPersisterFactory), Category = TestCategories.Mongo)]
    [TestFixture(typeof(SqlServerSagaPersisterFactory), Category = TestCategories.MsSql)]
    [TestFixture(typeof(RavenDbSagaPersisterFactory), Category = TestCategories.Raven)]
    public class TestSagaPersisters<TFactory> : TestSagaPersistersBase<TFactory> where TFactory : ISagaPersisterFactory
    {
        [Test]
        public void PersisterCanFindSagaByPropertiesWithDifferentDataTypes()
        {
            TestFindSagaByPropertyWithType("Hello worlds!!");
            TestFindSagaByPropertyWithType(23);
            TestFindSagaByPropertyWithType(Guid.NewGuid());
        }

        void TestFindSagaByPropertyWithType<TProperty>(TProperty propertyValueToUse)
        {
            var propertyTypeToTest = typeof(TProperty);
            var type = typeof (GenericSagaData<>);
            var sagaDataType = type.MakeGenericType(typeof(TFactory), propertyTypeToTest);
            var savedSagaData = (ISagaData)Activator.CreateInstance(sagaDataType);
            var savedSagaDataId = Guid.NewGuid();
            savedSagaData.Id = savedSagaDataId;
            sagaDataType.GetProperty("Property").SetValue(savedSagaData, propertyValueToUse, new object[0]);
            persister.Insert(savedSagaData, new[] { "Property" });

            var foundSagaData = persister.Find<GenericSagaData<TProperty>>("Property", propertyValueToUse);

            foundSagaData.Id.ShouldBe(savedSagaDataId);
        }

        [Test]
        public void PersisterCanFindSagaById()
        {
            var savedSagaData = new MySagaData();
            var savedSagaDataId = Guid.NewGuid();
            savedSagaData.Id = savedSagaDataId;
            persister.Insert(savedSagaData, new string[0]);

            var foundSagaData = persister.Find<MySagaData>("Id", savedSagaDataId);

            foundSagaData.Id.ShouldBe(savedSagaDataId);
        }

        [Test]
        public void PersistsComplexSagaLikeExpected()
        {
            var sagaDataId = Guid.NewGuid();

            var complexPieceOfSagaData =
                new MySagaData
                {
                    Id = sagaDataId,
                    SomeField = "hello",
                    AnotherField = "world!",
                    Embedded = new SomeEmbeddedThingie
                               {
                                   ThisIsEmbedded = "this is embedded",
                                   Thingies =
                                       {
                                           new SomeCollectedThing { No = 1 },
                                           new SomeCollectedThing { No = 2 },
                                           new SomeCollectedThing { No = 3 },
                                           new SomeCollectedThing { No = 4 }
                                       }
                               }
                };

            persister.Insert(complexPieceOfSagaData, new[] { "SomeField" });

            var sagaData = persister.Find<MySagaData>("Id", sagaDataId);
            sagaData.SomeField.ShouldBe("hello");
            sagaData.AnotherField.ShouldBe("world!");
        }

        [Test]
        public void CanDeleteSaga()
        {
            var mySagaDataId = Guid.NewGuid();
            var mySagaData = new SimpleSagaData
                             {
                                 Id = mySagaDataId,
                                 SomeString = "whoolala"
                             };

            persister.Insert(mySagaData, new[] { "SomeString" });
            persister.Delete(mySagaData);

            var sagaData = persister.Find<SimpleSagaData>("Id", mySagaDataId);
            sagaData.ShouldBe(null);
        }

        [Test]
        public void CanFindSagaByPropertyValues()
        {
            persister.Insert(SagaData(1, "some field 1"), new[] { "AnotherField" });
            persister.Insert(SagaData(2, "some field 2"), new[] { "AnotherField" });
            persister.Insert(SagaData(3, "some field 3"), new[] { "AnotherField" });

            var dataViaNonexistentValue = persister.Find<MySagaData>("AnotherField", "non-existent value");
            var dataViaNonexistentField = persister.Find<MySagaData>("SomeFieldThatDoesNotExist", "doesn't matter");
            var mySagaData = persister.Find<MySagaData>("AnotherField", "some field 2");

            dataViaNonexistentField.ShouldBe(null);
            dataViaNonexistentValue.ShouldBe(null);
            mySagaData.SomeField.ShouldBe("2");
        }

        [Test]
        public void SamePersisterCanSaveMultipleTypesOfSagaDatas()
        {
            var sagaId1 = Guid.NewGuid();
            var sagaId2 = Guid.NewGuid();
            persister.Insert(new SimpleSagaData { Id = sagaId1, SomeString = "Ol�" }, new[] { "Id" });
            persister.Insert(new MySagaData { Id = sagaId2, AnotherField = "Yipiie" }, new[] { "Id" });

            var saga1 = persister.Find<SimpleSagaData>("Id", sagaId1);
            var saga2 = persister.Find<MySagaData>("Id", sagaId2);

            saga1.SomeString.ShouldBe("Ol�");
            saga2.AnotherField.ShouldBe("Yipiie");
        }

       [Test]
       public void PersisterCanFindSagaDataWithNestedElements()
       {
           const string stringValue = "I expect to find something with this string!";
           var path = Reflect.Path<SagaDataWithNestedElement>(d => d.ThisOneIsNested.SomeString);

           persister.Insert(new SagaDataWithNestedElement
                              {
                                  Id = Guid.NewGuid(),
                                  Revision = 12,
                                  ThisOneIsNested = new ThisOneIsNested
                                                        {
                                                            SomeString = stringValue
                                                        }
                              }, new[] {path});

           var loadedSagaData = persister.Find<SagaDataWithNestedElement>(path, stringValue);

           loadedSagaData.ThisOneIsNested.ShouldNotBe(null);
           loadedSagaData.ThisOneIsNested.SomeString.ShouldBe(stringValue);
       }

        MySagaData SagaData(int someNumber, string textInSomeField)
        {
            return new MySagaData
                   {
                       Id = Guid.NewGuid(),
                       SomeField = someNumber.ToString(),
                       AnotherField = textInSomeField,
                   };
        }

        class SagaDataWithNestedElement : ISagaData
        {
            public Guid Id { get; set; }
            public int Revision { get; set; }
            public ThisOneIsNested ThisOneIsNested { get; set; }
        }

        class ThisOneIsNested
        {
            public string SomeString { get; set; }
        }
    }
}