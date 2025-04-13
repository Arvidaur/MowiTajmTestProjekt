using Xunit;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MowiTajm.Models;
using System.Collections.Generic;

public class UserServiceTests
{
    // Hjälpmetod som skapar en mockad UserManager (krävs eftersom UserManager är svår att instansiera direkt)
    private Mock<UserManager<ApplicationUser>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();

        // UserManager har en lång parameterlista i konstruktorn. Vi mockar bara det allra nödvändigaste.
        // Endast IUserStore<ApplicationUser> krävs för att skapa instansen – resten sätts till null
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,      // Användarens datalager (mockat)
            null,              // IPasswordHasher<ApplicationUser> – används vid lösenordshantering (inte relevant här)
            null,              // IEnumerable<IUserValidator<ApplicationUser>> – används för att validera användare
            null,              // IEnumerable<IPasswordValidator<ApplicationUser>> – för lösenordsvalidering
            null,              // ILookupNormalizer – normaliserar användarnamn/mejl
            null,              // IdentityErrorDescriber – beskriver fel vid Identity-operationer
            null,              // IServiceProvider – DI-container (inte använd här)
            null,              // ILogger<UserManager<ApplicationUser>> – loggning, behövs inte för testerna
            null               // HttpContextAccessor eller options beroende på version (inte använd här)
        );
    }

    [Fact]
    public async Task GetUserContextAsync_UserIsAdmin_ReturnsAdminContext()
    {
        // Arrange
        var mockUserManager = CreateMockUserManager();
        var user = new ApplicationUser { DisplayName = "Arvid" };

        // Skapa en tom ClaimsPrincipal – innehållet spelar ingen roll här eftersom vi mockar resultatet
        var claimsPrincipal = new ClaimsPrincipal();

        // Simulera att en användare hittas och returneras
        mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                       .ReturnsAsync(user);

        // Simulera att användaren tillhör admin-rollen
        mockUserManager.Setup(um => um.IsInRoleAsync(user, "Admin"))
                       .ReturnsAsync(true);

        var service = new UserService(mockUserManager.Object);

        // Act
        var result = await service.GetUserContextAsync(claimsPrincipal);

        // Assert
        Assert.True(result.IsAdmin); // Vi förväntar oss att admin-flaggan är true
        Assert.Equal("Arvid", result.DisplayName); // Namnet ska matcha användarens visningsnamn
    }

    [Fact]
    public async Task GetUserContextAsync_UserIsNotAdmin_ReturnsNonAdminContext()
    {
        // Arrange
        var mockUserManager = CreateMockUserManager();
        var user = new ApplicationUser { DisplayName = "Benjamin" };

        var claimsPrincipal = new ClaimsPrincipal();

        // Simulera att användaren finns men inte är admin
        mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                       .ReturnsAsync(user);

        mockUserManager.Setup(um => um.IsInRoleAsync(user, "Admin"))
                       .ReturnsAsync(false);

        var service = new UserService(mockUserManager.Object);

        // Act
        var result = await service.GetUserContextAsync(claimsPrincipal);

        // Assert
        Assert.False(result.IsAdmin); // Ska vara false för icke-admin
        Assert.Equal("Benjamin", result.DisplayName); // Namnet ska ändå returneras korrekt
    }

    [Fact]
    public async Task GetUserContextAsync_UserIsNull_ReturnsEmptyContext()
    {
        // Arrange
        var mockUserManager = CreateMockUserManager();
        var claimsPrincipal = new ClaimsPrincipal();

        // Simulera att ingen användare hittas (null returneras)
        mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                       .ReturnsAsync((ApplicationUser)null);

        var service = new UserService(mockUserManager.Object);

        // Act
        var result = await service.GetUserContextAsync(claimsPrincipal);

        // Assert
        Assert.False(result.IsAdmin); // Ska inte vara admin om användaren är null
        Assert.Equal(string.Empty, result.DisplayName); // DisplayName ska vara tom sträng
    }

    [Fact]
    public async Task GetUserContextAsync_ExceptionThrown_ReturnsEmptyContext()
    {
        // Arrange
        var mockUserManager = CreateMockUserManager();
        var claimsPrincipal = new ClaimsPrincipal();

        // Simulera att ett exception kastas inuti GetUserAsync (t.ex. databaskrasch)
        mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                       .ThrowsAsync(new Exception("Boom!"));

        var service = new UserService(mockUserManager.Object);

        // Act
        var result = await service.GetUserContextAsync(claimsPrincipal);

        // Assert
        Assert.False(result.IsAdmin); // Ska inte vara admin trots kraschen
        Assert.Equal(string.Empty, result.DisplayName); // En säker fallback – tom DisplayName
    }
}
