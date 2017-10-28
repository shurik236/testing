using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal.Filters;

namespace HomeExercises
{
    [TestFixture]
    public class NumberValidatorShould_ThrowException
    {
        [TestCase(-1, 0, false, TestName = "on negative precision")]
        [TestCase(0, 0, false, TestName = "on zero precision")]
        [TestCase(3, -2, false, TestName = "on negative scale")]
        [TestCase(2, 3, false, TestName = "on scale > precision")]
        [TestCase(2, 2, false, TestName = "on scale = precision")]
        public void ThrowException(int precision, int scale, bool positiveOnly)
        {
            Assert.Throws<ArgumentException>(() => new NumberValidator(precision, scale, positiveOnly));
        }
    }

    [TestFixture]
    public class NumberValidatorShould_FailUnfittingNumbers
    {

        [TestCase("01,23", 3, 2, true,
            TestName = "01,23 on precision=3, scale=2")]
        [TestCase("0,123", 3, 2, true,
            TestName = "0,123 on precision=3, scale=2")]
        [TestCase("+0,1", 2, 1, true,
            TestName = "+0,1 on precision=2, scale=1")]
        [TestCase("1234", 3, 0, true,
            TestName = "1234 on precision=3, scale=0")]
        [TestCase("-2", 1, 0, false,
            TestName = "-2 on any sign, precision=1, scale=0")]
        [TestCase("-0,12", 4, 3, true,
            TestName = "-0,12 on positive only, precision=4, scale=3")]
        [TestCase("-0,00", 4, 3, true,
            TestName = "-0,00 on positive only, precision=4, scale=3")]
        public void WithParameters(string value, int precision, int scale, bool positiveOnly)
        {
            Assert.False(new NumberValidator(precision, scale, positiveOnly).IsValidNumber(value));
        }
    }

    [TestFixture]
    public class NumberValidatorShould_FailNonNumbers
    {
        [TestCase(null,
            TestName = "null")]
        [TestCase("",
            TestName = "empty string")]
        [TestCase("  ",
            TestName = "whitespace")]
        [TestCase("+",
            TestName = "+ (lone sign)")]
        [TestCase("++0.12",
            TestName = "++0(dot)12 (consequent signs)")]
        [TestCase(".5",
            TestName = "(dot)5 (whole part missing)")]
        [TestCase("0,,12",
            TestName = "0,,12 (consequent separators)")]
        [TestCase("12,",
            TestName = "12, (separator without fraction)")]
        [TestCase("0.12,34",
            TestName = "0(dot)12,34 (multiple separators)")]
        [TestCase("-+0,123",
            TestName = "-+0,123 (consequent different signs)")]
        [TestCase("-0+1,23",
            TestName = "-0+1,23 (multiple signs)")]
        [TestCase("0,abcd",
            TestName = "0,ab_cd (non-numeric chars)")]
        public void RegardlessOfParameter(string value)
        {
            Assert.False(new NumberValidator(17, 5, false).IsValidNumber(value));
        }
    }

    [TestFixture]
    public class NumberValidatorShould_PassWithParameters
    {
        [TestCase("0,12", 3, 2, true,
            TestName = "0,12 on positive only, precision=3, scale=2")]
        [TestCase("+0,12", 4, 2, true,
            TestName = "+0,12 on positive only, precision=4, scale=2")]
        [TestCase("-0,12", 4, 2, false,
            TestName = "-0,12 on any sign, precision=4, scale=2")]
        [TestCase("0.123", 4, 3, true,
            TestName = "0(dot)123 on positive only, precision=4, scale=3")]
        public void PassNumeber(string value, int precision, int scale, bool positiveOnly)
        {
            Assert.True(new NumberValidator(precision, scale, positiveOnly).IsValidNumber(value));
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
