using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
	public class ObjectComparison
	{
		[Test]
		[Description("Проверка текущего царя")]
		[Category("ToRefactor")]
		public void CheckCurrentTsar()
		{
			var actualTsar = TsarRegistry.GetCurrentTsar();

			var expectedTsar = new Person("Ivan IV The Terrible", 54, 170, 70,
				new Person("Vasili III of Russia", 28, 170, 60, 
                new Person("Ivan III of Russia", 65, 172, 71, null)));

            // Перепишите код на использование Fluent Assertions.
            // Переписал.

            actualTsar.Name.ShouldBeEquivalentTo(expectedTsar.Name);
            actualTsar.Age.ShouldBeEquivalentTo(expectedTsar.Age);
            actualTsar.Height.ShouldBeEquivalentTo(expectedTsar.Height);
            actualTsar.Weight.ShouldBeEquivalentTo(expectedTsar.Weight);

            actualTsar.Parent.Name.ShouldBeEquivalentTo(expectedTsar.Parent.Name);
            actualTsar.Parent.Age.ShouldBeEquivalentTo(expectedTsar.Parent.Age);
            actualTsar.Parent.Height.ShouldBeEquivalentTo(expectedTsar.Parent.Height);

            //actualTsar.Parent.Parent.ShouldBeEquivalentTo(expectedTsar.Parent.Parent); Id?!

		    AreEqual(actualTsar.Parent.Parent, expectedTsar.Parent.Parent).Should().BeTrue();
            //Если сравнение нуллов принципиально
		    AreEqual(actualTsar.Parent.Parent.Parent, expectedTsar.Parent.Parent.Parent).Should().BeTrue();
		}

	    [Test]
	    [Description("Еще одно решение с использованием правил исключения полей/свойств")]
	    public void CheckCurrentTsar_WithExclusionRules()
	    {
	        var actualTsar = TsarRegistry.GetCurrentTsar();
	        var expectedTsar = new Person("Ivan IV The Terrible", 54, 170, 70,
	            new Person("Vasili III of Russia", 28, 170, 60,
	                new Person("Ivan III of Russia", 65, 172, 71, null)));

	        actualTsar.ShouldBeEquivalentTo(expectedTsar, options => options
	            .IncludingFields()
	            .Excluding(o => o.SelectedMemberPath.EndsWith("Id")));
        }

		[Test]
		[Description("Альтернативное решение. Какие у него недостатки?")]
		public void CheckCurrentTsar_WithCustomEquality()
		{
			var actualTsar = TsarRegistry.GetCurrentTsar();
			var expectedTsar = new Person("Ivan IV The Terrible", 54, 170, 70,
			    new Person("Vasili III of Russia", 28, 170, 60,
			    new Person("Ivan III of Russia", 65, 172, 71, null)));

            //q: Какие недостатки у такого подхода? 
            //a: При добавлении новых полей их придется также прописывать и в методе AreEqual.
		    //Появляется неудобная зависимость от того, как именно сравниваются объекты.
            //А еще тесты, написанные с помощью FluidAssertions приятнее читать чем стену двойных равенств, 
            //даже если список правил разрастется.

            Assert.True(AreEqual(actualTsar, expectedTsar));

		}

		private bool AreEqual(Person actual, Person expected)
		{
			if (actual == expected) return true;
			if (actual == null || expected == null) return false;
			return
			actual.Name == expected.Name
			&& actual.Age == expected.Age
			&& actual.Height == expected.Height
			&& actual.Weight == expected.Weight
			&& AreEqual(actual.Parent, expected.Parent);
		}
	}

	public class TsarRegistry
	{
		public static Person GetCurrentTsar()
		{
			return new Person(
				"Ivan IV The Terrible", 54, 170, 70,
				new Person("Vasili III of Russia", 28, 170, 60,
				new Person("Ivan III of Russia", 65, 172, 71, null)));
		}
        //Добавил еще и деда. Цари должны лучше знать свою родословную)
	}

	public class Person
	{
		public static int IdCounter = 0;
		public int Age, Height, Weight;
		public string Name;
		public Person Parent;
		public int Id;

		public Person(string name, int age, int height, int weight, Person parent)
		{
			Id = IdCounter++;
			Name = name;
			Age = age;
			Height = height;
			Weight = weight;
			Parent = parent;
		}
	}
}
