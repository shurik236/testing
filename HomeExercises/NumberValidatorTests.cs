using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
	public class NumberValidatorTests
	{
	    private NumberValidator _positiveValidator;
	    private NumberValidator _anyNumberValidator;

        [Test]
		public void Test()
		{
			Assert.Throws<ArgumentException>(() => new NumberValidator(-1, 2, true));
			Assert.DoesNotThrow(() => new NumberValidator(1, 0, true));
			Assert.Throws<ArgumentException>(() => new NumberValidator(-1, 2, false));
			Assert.DoesNotThrow(() => new NumberValidator(1, 0, true));

			Assert.IsTrue(new NumberValidator(17, 2, true).IsValidNumber("0.0"));
			Assert.IsTrue(new NumberValidator(17, 2, true).IsValidNumber("0"));
			Assert.IsTrue(new NumberValidator(17, 2, true).IsValidNumber("0.0")); //intentionally?
			Assert.IsFalse(new NumberValidator(3, 2, true).IsValidNumber("00.00"));
			Assert.IsFalse(new NumberValidator(3, 2, true).IsValidNumber("-0.00"));
			Assert.IsTrue(new NumberValidator(17, 2, true).IsValidNumber("0.0"));
			Assert.IsFalse(new NumberValidator(3, 2, true).IsValidNumber("+0.00"));
			Assert.IsTrue(new NumberValidator(4, 2, true).IsValidNumber("+1.23"));
			Assert.IsFalse(new NumberValidator(3, 2, true).IsValidNumber("+1.23"));
			Assert.IsFalse(new NumberValidator(17, 2, true).IsValidNumber("0.000"));
			Assert.IsFalse(new NumberValidator(3, 2, true).IsValidNumber("-1.23"));
			Assert.IsFalse(new NumberValidator(3, 2, true).IsValidNumber("a.sd"));
		}

	    [Test]
	    public void Validator_ShouldThrowArgumentException_OnNegativePrecision()
	    {
	        Action validatorCreation = () => new NumberValidator(-1, 2);
            validatorCreation.ShouldThrow<ArgumentException>().WithMessage("precision must be a positive number");
	    }

	    [Test]
	    public void Validator_ShouldThrowArgumentException_OnNegativeScale()
	    {
	        Action validatorCreation = () => new NumberValidator(1, -2);
	        validatorCreation.ShouldThrow<ArgumentException>()
	            .WithMessage("precision must be a non-negative number less or equal than precision");
	    }

	    [Test]
	    public void Validator_ShouldThrowArgumentException_OnScaleGreaterThanPrecision()
	    {
	        Action validatorCreation = () => new NumberValidator(1, 2);
	        validatorCreation.ShouldThrow<ArgumentException>()
	            .WithMessage("precision must be a non-negative number less or equal than precision");
	    }

	    [SetUp]
        public void SetUp()
	    {
	        _positiveValidator = new NumberValidator(3, 2, true);
            _anyNumberValidator = new NumberValidator(3, 2);            
	    }

	    [Test]
	    public void Validator_Passes_CorrectPrecisionAndScale()
	    {
	        _positiveValidator.IsValidNumber("0.1").Should().BeTrue();
	        _positiveValidator.IsValidNumber("0").Should().BeTrue();
	        _anyNumberValidator.IsValidNumber("-0.1").Should().BeTrue();
	    }

	    [Test]
	    public void Validator_Fails_ZeroExceedingPrecision()
	    {
	        _positiveValidator.IsValidNumber("00.00").Should().BeFalse();
	    }

	    [Test]
	    public void Validator_Fails_NegativeZeroOnPositiveOnly()
	    {
	        _positiveValidator.IsValidNumber("-0.0").Should().BeFalse();
	    }

	    [Test]
	    public void Validator_Fails_NegativeNumOnPositiveOnly()
	    {
	        _positiveValidator.IsValidNumber("-1.23").Should().BeFalse();
	    }

	    [Test]
	    public void Validator_Fails_ExceedingPrecisionOnPositive()
	    {
	        _positiveValidator.IsValidNumber("12.34").Should().BeFalse();
	    }

	    [Test]
	    public void Validator_Fails_ExceedingPrecisionWithPlus()
	    {
	        _anyNumberValidator.IsValidNumber("+0.12").Should().BeFalse();
	    }

	    [Test]
	    public void Validator_Fails_ExceedingPrecisionWithMinus()
	    {
	        _anyNumberValidator.IsValidNumber("-0.12").Should().BeFalse();
        }

	    [Test]
	    public void Validator_Fails_ExceedingScale()
	    {
	        _positiveValidator.IsValidNumber("0.123").Should().BeFalse();
	    }

	    [Test]
	    public void Validator_Fails_NonNumericChars()
	    {
	        _positiveValidator.IsValidNumber("0.ab").Should().BeFalse();
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