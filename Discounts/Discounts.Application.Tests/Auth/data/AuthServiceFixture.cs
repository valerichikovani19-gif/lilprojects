using Discounts.Application.RepoInterfaces;
using Discounts.Application.Services;
using Microsoft.Extensions.Configuration;
using Moq;
namespace Discounts.Application.Tests.Auth.Data
{
    public class AuthServiceFixture : IDisposable
    {
        public AuthService Service { get; }

        // mocks
        public Mock<IUnitOfWork> UnitOfWorkMock { get; }
        public Mock<IAuthRepository> AuthRepoMock { get; }
        //public Mock<IConfiguration> ConfigMock { get; }

        public AuthServiceFixture()
        {
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            AuthRepoMock = new Mock<IAuthRepository>();
           // ConfigMock = new Mock<IConfiguration>();

            //ConfigMock.Setup(c => c["JwtSettings:Secret"]).Returns("SuperSecretKeyForTesting12345!");
            //ConfigMock.Setup(c => c["JwtSettings:Issuer"]).Returns("DiscountsAPI");
            //ConfigMock.Setup(c => c["JwtSettings:Audience"]).Returns("DiscountsClient");
            
            //mockis nacvlad config
            var inMemorySettings = new Dictionary<string, string> {
                {"JwtSettings:Secret", "UdzlieresiParoliRacKiArsebobsDedamiwaze_4567891!"},
                {"JwtSettings:Issuer", "DiscountsAPI"},
                {"JwtSettings:Audience", "DiscountsClient"}
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            UnitOfWorkMock.Setup(u => u.Auth).Returns(AuthRepoMock.Object);

            Service = new AuthService(UnitOfWorkMock.Object, configuration);
        }

        public void Dispose() { }
    }
}
