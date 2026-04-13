using Discounts.Application.DTOs.Auth;
using Discounts.Application.Exceptions;
using Discounts.Application.Tests.Auth.Data;
using Discounts.Domain.Entities;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

namespace Discounts.Application.Tests.Auth
{
    public class AuthServiceTests : IClassFixture<AuthServiceFixture>
    {
        private readonly AuthServiceFixture _fixture;

        public AuthServiceTests(AuthServiceFixture fixture)
        {
            _fixture = fixture;
            _fixture.AuthRepoMock.Reset();
        }
        #region register
        [Fact(DisplayName = "Register -  Should create user and return token")]
        public async Task Register_WhenValid_ShouldReturnToken()
        {
            //arrange
            var registerDto = new RegisterDto
            {
                Email = "HellYeah@user.ge",
                Password = "Password_123!",
                FirstName = "tbc",
                LastName = "academishvili",
                Role = "Customer"
            };
            //GetUserRoleAsync must return role so generateJwt works
             _fixture.AuthRepoMock.Setup(x => x.GetUserRoleAsync(registerDto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync("Customer");

            //GetUserIdAsync must returnID
            _fixture.AuthRepoMock.Setup(x => x.GetUserIdAsync(registerDto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync("user-guid-123");

            //act
            var result = await _fixture.Service.RegisterAsync(registerDto, CancellationToken.None);

            //assert
            using (new AssertionScope())
            {
                result.Token.Should().NotBeNullOrEmpty();
                result.Email.Should().Be(registerDto.Email);

                _fixture.AuthRepoMock.Verify(x => x.RegisterUserAsync(
                    registerDto.Email, registerDto.Password, registerDto.FirstName, registerDto.LastName, registerDto.Role,
                    It.IsAny<CancellationToken>()), Times.Once);
            }
        }
        #endregion
        #region Login tests 
        [Fact(DisplayName = "Login - Should return token when credentials are valid")]
        public async Task Login_WhenValid_ShouldReturnToken()
        {
            // arrange
            var loginDto = new LoginDto { Email = "HellYeah@user.ge", Password = "Password_123!" };

            _fixture.AuthRepoMock.Setup(x => x.GetAllUsersAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ApplicationUser>
                {
                    new ApplicationUser { Email = loginDto.Email, IsBlocked = false }
                });
            //setup validateUser returns true
            _fixture.AuthRepoMock.Setup(x => x.ValidateUserAsync(loginDto.Email, loginDto.Password, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            //mocking role/ID lookup which is needed for token generation
            _fixture.AuthRepoMock.Setup(x => x.GetUserRoleAsync(loginDto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync("Merchant");
            _fixture.AuthRepoMock.Setup(x => x.GetUserIdAsync(loginDto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync("merchant-id-123");

            //act
            var result = await _fixture.Service.LoginAsync(loginDto, CancellationToken.None);

            //assert
            result.Token.Should().NotBeNullOrEmpty();
            result.Role.Should().Be("Merchant");
        }

        [Fact(DisplayName = "Login -Should throw InvalidCredentialsException when password wrong")]
        public async Task Login_WhenInvalid_ShouldThrowException()
        {
            // arrange
            var loginDto = new LoginDto { Email = "hackeribichi@user.ge", Password = "wrongPass123" };

            //ValidateUser returns false
            _fixture.AuthRepoMock.Setup(x => x.ValidateUserAsync(loginDto.Email, loginDto.Password, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            //act
            var action = async () => await _fixture.Service.LoginAsync(loginDto, CancellationToken.None);

            //assert
            await action.Should().ThrowAsync<InvalidCredentialsException>();
        }
        #endregion
    }
}
