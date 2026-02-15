using System.Globalization;
using PSBinaryModule.Commands;

namespace PSBinaryModule.Tests.Commands
{
    public class GetSystemLocaleCommandTests
    {
        [Fact]
        public void NormalizeLocaleReplacesUnderscoreAndCanonicalizesCase()
        {
            var result = GetSystemLocaleCommand.NormalizeLocale("en_us");

            Assert.Equal("en-US", result);
        }

        [Fact]
        public void NormalizeLocaleReturnsNullForWhitespaceInput()
        {
            var result = GetSystemLocaleCommand.NormalizeLocale("   ");

            Assert.Null(result);
        }

        [Fact]
        public void NormalizeLocaleReturnsNullForNullInput()
        {
            var result = GetSystemLocaleCommand.NormalizeLocale(null);

            Assert.Null(result);
        }

        [Fact]
        public void NormalizeLocaleReturnsNullForInvalidCulture()
        {
            var result = GetSystemLocaleCommand.NormalizeLocale("xx-ZZ");

            Assert.Null(result);
        }

        [Fact]
        public void NormalizeLocaleKeepsAlreadyNormalizedInput()
        {
            var result = GetSystemLocaleCommand.NormalizeLocale("en-US");

            Assert.Equal("en-US", result);
        }

        [Fact]
        public void NormalizeLocaleTrimsAndNormalizesValidInput()
        {
            var result = GetSystemLocaleCommand.NormalizeLocale("  en_us  ");

            Assert.Equal("en-US", result);
        }

        [Fact]
        public void GetNormalizedSystemLocaleReturnsValidCultureName()
        {
            var locale = GetSystemLocaleCommand.GetNormalizedSystemLocale();

            Assert.False(string.IsNullOrWhiteSpace(locale));
            Assert.DoesNotContain("_", locale);

            var culture = CultureInfo.GetCultureInfo(locale);
            Assert.Equal(culture.Name, locale);
        }
    }
}
