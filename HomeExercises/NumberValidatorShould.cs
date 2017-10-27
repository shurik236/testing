using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal.Filters;

namespace HomeExercises
{
    [TestFixture]
    public class NumberValidator_ShouldThrowException
    {
        [TestCase(-1, 0, false, TestName = "on negative precision")]
        [TestCase(0, 0, false, TestName = "on zero precision")]
        [TestCase(3, -2, false, TestName = "on negative scale")]
        [TestCase(2, 3, false, TestName = "on scale > precision")]
        [TestCase(2, 2, false, TestName = "on scale = precision")]
        public void ThrowException(int precision, int scale, bool positiveOnly)
        {
            Assert.That(() => new NumberValidator(precision, scale, positiveOnly),
                Throws.TypeOf<ArgumentException>());
        }
    }

    [TestFixture]
    public class NumberValidator_ShouldFail
    { 
	    [TestCase(null, 3, 2, true,
	        TestName = "null")]
	    [TestCase("", 3, 2, true,
	        TestName = "empty string")]
	    [TestCase("  ", 3, 2, true,
	        TestName = "whitespace")]
	    [TestCase("00.00", 3, 2, true,
            TestName = "00(dot)00 on precision=3, scale=2")]
        [TestCase("0.000", 3, 2, true, 
            TestName = "0(dot)000 on precision=3, scale=2")]
        [TestCase("+0.0", 2, 1, true,
            TestName = "+0(dot)0 on precision=2, scale=1")]
        [TestCase("+", 1, 0, true,
            TestName = "+")]
        [TestCase("++0.12", 5, 2, true,
            TestName = "++0(dot)12")]
        [TestCase(".5", 5, 2, true,
            TestName = "(dot)5")]
        [TestCase("0,,12", 5, 2, true,
            TestName = "0,,12")]
        [TestCase("12,", 5, 2, true,
            TestName = "12,")]
        [TestCase("0.12,34", 7, 2, true, 
            TestName = "0(dot)12,34")]
	    public void FailNumber(string value, int precision, int scale, bool positiveOnly)
	    {
	        Assert.False(new NumberValidator(precision, scale, positiveOnly).IsValidNumber(value));
	    }
    }


	public class NumberValidator
	{
		private readonly Regex numberRegex;
		private readonly bool onlyPositive;
		private readonly int precision;
		private readonly int scale;

		public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
		{
			this.precision = precision;
			this.scale = scale;
			this.onlyPositive = onlyPositive;
			if (precision <= 0)
				throw new ArgumentException("precision must be a positive number");
			if (scale < 0 || scale >= precision)
				throw new ArgumentException("precision must be a non-negative number less or equal than precision");
			numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
		}

		public bool IsValidNumber(string value)
		{
			// Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
			// описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
			// Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
			// целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
			// Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

			if (string.IsNullOrEmpty(value))
				return false;

			var match = numberRegex.Match(value);
			if (!match.Success)
				return false;

			// Знак и целая часть
			var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
			// Дробная часть
			var fracPart = match.Groups[4].Value.Length;

			if (intPart + fracPart > precision || fracPart > scale)
				return false;

			if (onlyPositive && match.Groups[1].Value == "-")
				return false;
			return true;
		}
	}
}