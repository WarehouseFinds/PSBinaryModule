using System.Security.Cryptography;
using PSBinaryModule.Commands;

namespace PSBinaryModule.Tests.Commands
{
    public class GetFileChecksumCommandTests
    {
        [Fact]
        public void CreateHashAlgorithmReturnsMD5ForMD5Input()
        {
            using var algorithm = GetFileChecksumCommand.CreateHashAlgorithm("MD5");

            _ = Assert.IsType<MD5>(algorithm, exactMatch: false);
        }

        [Fact]
        public void CreateHashAlgorithmReturnsSHA1ForSHA1Input()
        {
            using var algorithm = GetFileChecksumCommand.CreateHashAlgorithm("SHA1");

            _ = Assert.IsType<SHA1>(algorithm, exactMatch: false);
        }

        [Fact]
        public void CreateHashAlgorithmReturnsSHA256ForSHA256Input()
        {
            using var algorithm = GetFileChecksumCommand.CreateHashAlgorithm("SHA256");

            _ = Assert.IsType<SHA256>(algorithm, exactMatch: false);
        }

        [Fact]
        public void CreateHashAlgorithmReturnsSHA512ForSHA512Input()
        {
            using var algorithm = GetFileChecksumCommand.CreateHashAlgorithm("SHA512");

            _ = Assert.IsType<SHA512>(algorithm, exactMatch: false);
        }

        [Fact]
        public void CreateHashAlgorithmThrowsForInvalidAlgorithm()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                GetFileChecksumCommand.CreateHashAlgorithm("INVALID"));

            Assert.Contains("Unsupported algorithm", exception.Message);
        }

        [Fact]
        public void CreateHashAlgorithmIsCaseInsensitive()
        {
            using var algorithm1 = GetFileChecksumCommand.CreateHashAlgorithm("sha256");
            using var algorithm2 = GetFileChecksumCommand.CreateHashAlgorithm("SHA256");
            using var algorithm3 = GetFileChecksumCommand.CreateHashAlgorithm("Sha256");

            _ = Assert.IsType<SHA256>(algorithm1, exactMatch: false);
            _ = Assert.IsType<SHA256>(algorithm2, exactMatch: false);
            _ = Assert.IsType<SHA256>(algorithm3, exactMatch: false);
        }

        [Fact]
        public void ComputeHashCalculatesCorrectSHA256Hash()
        {
            // Arrange
            var testData = "Hello, World!"u8.ToArray();
            var expectedHash = "dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f";

            using var stream = new MemoryStream(testData);

            // Act
            var hashBytes = GetFileChecksumCommand.ComputeHash(stream, "SHA256");
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            // Assert
            Assert.Equal(expectedHash, hashString);
        }

        [Fact]
        public void ComputeHashCalculatesCorrectMD5Hash()
        {
            // Arrange
            var testData = "Hello, World!"u8.ToArray();
            var expectedHash = "65a8e27d8879283831b664bd8b7f0ad4";

            using var stream = new MemoryStream(testData);

            // Act
            var hashBytes = GetFileChecksumCommand.ComputeHash(stream, "MD5");
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            // Assert
            Assert.Equal(expectedHash, hashString);
        }

        [Fact]
        public void ComputeHashCalculatesCorrectSHA1Hash()
        {
            // Arrange
            var testData = "Hello, World!"u8.ToArray();
            var expectedHash = "0a0a9f2a6772942557ab5355d76af442f8f65e01";

            using var stream = new MemoryStream(testData);

            // Act
            var hashBytes = GetFileChecksumCommand.ComputeHash(stream, "SHA1");
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            // Assert
            Assert.Equal(expectedHash, hashString);
        }

        [Fact]
        public void ComputeHashCalculatesCorrectSHA512Hash()
        {
            // Arrange
            var testData = "Hello, World!"u8.ToArray();
            var expectedHash = "374d794a95cdcfd8b35993185fef9ba368f160d8daf432d08ba9f1ed1e5abe6cc69291e0fa2fe0006a52570ef18c19def4e617c33ce52ef0a6e5fbe318cb0387";

            using var stream = new MemoryStream(testData);

            // Act
            var hashBytes = GetFileChecksumCommand.ComputeHash(stream, "SHA512");
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            // Assert
            Assert.Equal(expectedHash, hashString);
        }

        [Fact]
        public void ComputeHashHandlesEmptyStream()
        {
            // Arrange
            var expectedHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"; // SHA256 of empty input

            using var stream = new MemoryStream();

            // Act
            var hashBytes = GetFileChecksumCommand.ComputeHash(stream, "SHA256");
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            // Assert
            Assert.Equal(expectedHash, hashString);
        }

        [Fact]
        public void ComputeHashProducesDifferentHashesForDifferentData()
        {
            // Arrange
            var testData1 = "Hello, World!"u8.ToArray();
            var testData2 = "Hello, Universe!"u8.ToArray();

            using var stream1 = new MemoryStream(testData1);
            using var stream2 = new MemoryStream(testData2);

            // Act
            var hash1 = GetFileChecksumCommand.ComputeHash(stream1, "SHA256");
            var hash2 = GetFileChecksumCommand.ComputeHash(stream2, "SHA256");

            // Assert
            Assert.NotEqual(hash1, hash2);
        }
    }
}
